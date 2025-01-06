using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A dynamic MSBT tag map.
/// </summary>
public class MsbtDynamicTagMap : IMsbtTagMap, IEnumerable<MsbtTagInfo>
{
    #region private members
    private readonly List<MsbtTagInfo> _tags = [];
    private readonly Dictionary<ushort, TagGroup> _numericLookupMap = [];
    private readonly Dictionary<string, MsbtTagInfo> _namedLookupMap = new(
        StringComparer.OrdinalIgnoreCase
    );
    #endregion

    #region IMsbtTagMap interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public void GetTag(
        MsbtTag tag,
        bool bigEndian,
        Encoding encoding,
        out string tagName,
        out IEnumerable<MsbtTagArgument> tagArgs
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
#else
        if (tag is null)
            throw new ArgumentNullException(nameof(tag));
        if (encoding is null)
            throw new ArgumentNullException(nameof(encoding));
#endif

        var argList = new List<MsbtTagArgument>();
        var argOffset = 0;

        if (TryGetInfo(tag.Group, tag.Type, out var tagInfo))
        {
            tagName = string.IsNullOrEmpty(tagInfo.Name) ? tagInfo.Group.ToString() : tagInfo.Name;
            if (!tagInfo.Type.HasValue || tagInfo.Type.Value != tag.Type)
            {
                if (tagInfo.TypeMap.TryGetValue(tag.Type.ToString(), out var typeName))
                    tagName += $":{typeName.NameOrValue}";
                else
                    tagName += $":{tag.Type}";
            }

            foreach (var argInfo in tagInfo.Arguments)
            {
                //discard padding
                if (argInfo.IsDiscard)
                {
                    argOffset = tag.Args.Length;
                    break;
                }

                //argument padding
                if (argInfo.IsPadding)
                {
                    argOffset +=
                        argInfo.DataType.Length
                        * (argInfo.ArrayLength > 0 ? argInfo.ArrayLength : 1);
                    continue;
                }

                //handle arrays
                if (argInfo.ArrayLength > 0)
                {
                    var arr = new object[argInfo.ArrayLength];
                    for (var i = 0; i < argInfo.ArrayLength; ++i)
                    {
                        arr[i] = ParseArgument(argInfo, tagInfo);
                    }
                    argList.Add(new MsbtTagArgument(argInfo.Name, arr));
                }
                else
                {
                    argList.Add(new MsbtTagArgument(argInfo.Name, ParseArgument(argInfo, tagInfo)));
                }
            }

            if (argOffset < tag.Args.Length)
            {
                argList.Add(
                    new MsbtTagArgument("otherArg", tag.Args[argOffset..].ToHexString(true))
                );
            }
        }
        else
        {
            tagName = $"{tag.Group}:{tag.Type}";
            if (tag.Args.Length > 0)
                argList.Add(new MsbtTagArgument("arg", tag.Args.ToHexString(true)));
        }

        tagArgs = argList;
        return;

        string ParseArgument(MsbtArgumentInfo arg, MsbtTagInfo info)
        {
            try
            {
                var (value, count) = arg.DataType.Deserialize(
                    tag.Args,
                    argOffset,
                    bigEndian,
                    encoding
                );
                argOffset += count;
                return arg.ValueMap.TryGetValue(value, out var valueInfo)
                    ? valueInfo.NameOrValue
                    : value;
            }
            catch
            {
                throw new FormatException(
                    $"Failed to parse tag argument value of \"{arg.Name}\" on \"{info.Name}\" as {arg.DataType.Name}."
                );
            }
        }
    }
    #endregion

    #region public properties
    /// <summary>
    /// Gets the number of tag definitions in the map.
    /// </summary>
    public int Count => _tags.Count;
    #endregion

    #region public methods
    /// <summary>
    /// Adds a new tag definition entry to the map.
    /// </summary>
    /// <param name="tag">The tag definition to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void AddInfo(MsbtTagInfo tag)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
#else
        if (tag is null)
            throw new ArgumentNullException(nameof(tag));
#endif

        //nothing to add
        if (!tag.Type.HasValue && tag is { HasDiscard: false, TypeList.Count: 0, TypeMap.Count: 0 })
            return;

        //check names and types
        if (!string.IsNullOrEmpty(tag.Name))
        {
            if (tag.Name.ContainsAny(':', ' '))
                throw new ArgumentException("Tag name contains invalid characters.");
            if (ushort.TryParse(tag.Name, out _))
                throw new ArgumentException("Tag name cannot be a number.");
        }
        if (tag.TypeMap.Count > 0 && tag.TypeMap.DataType != DataTypes.UInt16)
            throw new ArgumentException("Tag type map datatype must be u16.");
        foreach (var type in tag.TypeMap)
        {
            if (string.IsNullOrEmpty(type.Name))
                continue;
            if (type.Name.ContainsAny(':', ' '))
                throw new ArgumentException("Tag type name contains invalid characters.");
            if (ushort.TryParse(type.Name, out _))
                throw new ArgumentException("Tag type name cannot be a number.");
        }
        foreach (var argument in tag.Arguments)
        {
            if (argument.ValueMap.Count == 0)
                continue;
            if (argument.IsDiscard)
                throw new ArgumentException(
                    "Tag argument cannot be discard and value map at the same time."
                );
            if (argument.DataType != argument.ValueMap.DataType)
                throw new ArgumentException(
                    $"Tag argument value map datatype must be {argument.DataType.Name}."
                );
        }

        //check for value collisions
        if (_numericLookupMap.TryGetValue(tag.Group, out var tagGroup))
        {
            if (tag.HasDiscard && tagGroup.Discard is not null)
                throw new ArgumentException("A discard tag already exists in the same group.");
            if (tag.Type.HasValue)
            {
                if (tagGroup.Tags.ContainsKey(tag.Type.Value))
                    throw new ArgumentException(
                        $"A tag with the type {tag.Type.Value} already exists in the same group."
                    );
                if (!string.IsNullOrEmpty(tag.Name) && _namedLookupMap.ContainsKey(tag.Name))
                    throw new ArgumentException(
                        $"A tag with the name {tag.Type.Value} already exists in the same group."
                    );
            }
            foreach (var type in tag.TypeList)
            {
                if (tagGroup.Tags.ContainsKey(type))
                    throw new ArgumentException(
                        $"A tag with the type {type} already exists in the same group."
                    );
            }
            foreach (var typeInfo in tag.TypeMap)
            {
                var typeId = ushort.Parse(typeInfo.Value);
                if (tagGroup.Tags.ContainsKey(typeId))
                    throw new ArgumentException(
                        $"A tag with the type {typeId} already exists in the same group."
                    );
                if (
                    !string.IsNullOrEmpty(typeInfo.Name)
                    && _namedLookupMap.ContainsKey($"{tag.Group}:{typeInfo.Name}")
                )
                    throw new ArgumentException(
                        $"A tag with the name {typeInfo.Name} already exists in the same group."
                    );
            }
        }

        //add group if missing
        if (tagGroup is null)
        {
            tagGroup = new TagGroup();
            _numericLookupMap.Add(tag.Group, tagGroup);
        }

        //add tag to map
        _tags.Add(tag);
        if (tag.Type.HasValue)
        {
            tagGroup.Tags.Add(tag.Type.Value, tag);
            _namedLookupMap.Add($"{tag.Group}:{tag.Type.Value}", tag);
            if (!string.IsNullOrEmpty(tag.Name))
            {
                _namedLookupMap.Add(tag.Name, tag);
                _namedLookupMap.Add($"{tag.Name}:{tag.Type.Value}", tag);
            }
        }
        if (tag.HasDiscard)
        {
            tagGroup.Discard = tag;
            _namedLookupMap.Add($"{tag.Group}::", tag);
            if (!string.IsNullOrEmpty(tag.Name))
                _namedLookupMap.Add($"{tag.Name}::", tag);
        }
        foreach (var type in tag.TypeList)
        {
            tagGroup.Tags.Add(type, tag);
            _namedLookupMap.Add($"{tag.Group}:{type}", tag);
            if (!string.IsNullOrEmpty(tag.Name))
                _namedLookupMap.Add($"{tag.Name}:{type}", tag);
        }
        foreach (var typeInfo in tag.TypeMap)
        {
            tagGroup.Tags.Add(ushort.Parse(typeInfo.Value), tag);
            _namedLookupMap.Add($"{tag.Group}:{typeInfo.Value}", tag);
            if (!string.IsNullOrEmpty(typeInfo.Name))
                _namedLookupMap.Add($"{tag.Group}:{typeInfo.Name}", tag);
            if (!string.IsNullOrEmpty(tag.Name))
            {
                _namedLookupMap.Add($"{tag.Name}:{typeInfo.Value}", tag);
                if (!string.IsNullOrEmpty(typeInfo.Name))
                    _namedLookupMap.Add($"{tag.Name}:{typeInfo.Name}", tag);
            }
        }
    }

    /// <summary>
    /// Attempts to get tag definition with the given group and type values.
    /// </summary>
    /// <param name="group">The tag group value.</param>
    /// <param name="type">The tag type value.</param>
    /// <param name="tag">The retrieved tag definition.</param>
    /// <returns><see langword="true"/> if tag was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetInfo(ushort group, ushort type, [MaybeNullWhen(false)] out MsbtTagInfo tag)
    {
        tag = null;

        if (!_numericLookupMap.TryGetValue(group, out var tagGroup))
            return false;
        if (tagGroup.Tags.TryGetValue(type, out tag))
            return true;
        tag = tagGroup.Discard;
        return tag is not null;
    }

    /// <summary>
    /// Attempts to get tag definition with the given name value.
    /// </summary>
    /// <param name="tagName">The tag name value.</param>
    /// <param name="tag">The retrieved tag definition.</param>
    /// <returns><see langword="true"/> if tag was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetInfo(string tagName, [MaybeNullWhen(false)] out MsbtTagInfo tag)
    {
        if (_namedLookupMap.TryGetValue(tagName, out tag))
            return true;
        var tagParts = tagName.Split(':');
        if (tagParts.Length != 2 || !ushort.TryParse(tagParts[1], out _))
            return false;
        return _namedLookupMap.TryGetValue(tagParts[0] + "::", out tag);
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<MsbtTagInfo> GetEnumerator() => _tags.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region helper classes
    private class TagGroup
    {
        public Dictionary<ushort, MsbtTagInfo> Tags { get; } = [];

        public MsbtTagInfo? Discard { get; set; }
    }
    #endregion
}
