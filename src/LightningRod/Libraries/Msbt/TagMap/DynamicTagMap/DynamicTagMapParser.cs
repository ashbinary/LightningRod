using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LightningRod.Libraries.Bmg;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A utility class for parsing serialized tag maps.
/// </summary>
public static class DynamicTagMapParser
{
    #region private members
    private static readonly IReadOnlyDictionary<string, DataType> DatatypeMap =
        DataTypes.GetTypeList();
    private static readonly Regex TagRegex = new(
        @"^\[\s*(\d+)\s*,\s*(\d+|\d+\s*-\s*\d+|_|\(\s*\d+(?:\s*-\s*\d+)?(?:\s*,\s*\d+(?:\s*-\s*\d+)?)*\s*\)|\{[A-Za-z0-9_]+\})\s*\]\s*(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.*))?$"
    );
    private static readonly Regex ArgumentRegex = new(
        @"^\s{2,}([A-Za-z0-9_]+|\{[A-Za-z0-9_]+\})(?:\[(\d+)\])?\s*(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.*))?$"
    );
    private static readonly Regex ArgumentPaddingRegex = new("^0x(?:[A-Fa-f0-9]{2})+$");
    private static readonly Regex ArgumentDiscardPaddingRegex = new("^_(0x[A-Fa-f0-9]{2})?$");
    private static readonly Regex MapRegex = new(
        @"^map\s+([A-Za-z0-9_]+)(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.*))?$"
    );
    private static readonly Regex MapValueRegex = new(
        @"^\s{2,}(-?[A-Za-z0-9_]+)(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.*))?$"
    );
    #endregion

    #region public methods
    /// <summary>
    /// Parses a serialized tag map into a <see cref="MsbtDynamicTagMap"/> instance.
    /// </summary>
    /// <param name="content">The serialized tag map.</param>
    /// <returns>A new <see cref="MsbtDynamicTagMap"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public static MsbtDynamicTagMap ParseMsbtTagMap(string content)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(content, nameof(content));
#else
        if (content is null)
            throw new ArgumentNullException(nameof(content));
#endif

        var map = new MsbtDynamicTagMap();
        foreach (var (line, tag) in ParseMap(content, false))
        {
            try
            {
                map.AddInfo(tag.ToMsbtInfo());
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    $"Failed to add tag definition to map: {ex.Message} (line {line + 1})"
                );
            }
        }
        return map;
    }

    /// <summary>
    /// Parses a serialized tag map into a <see cref="BmgDynamicTagMap"/> instance.
    /// </summary>
    /// <param name="content">The serialized tag map.</param>
    /// <returns>A new <see cref="BmgDynamicTagMap"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public static BmgDynamicTagMap ParseBmgTagMap(string content)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(content, nameof(content));
#else
        if (content is null)
            throw new ArgumentNullException(nameof(content));
#endif

        var map = new BmgDynamicTagMap();
        foreach (var (line, tag) in ParseMap(content, false))
        {
            try
            {
                map.AddInfo(tag.ToBmgInfo());
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    $"Failed to add tag definition to map: {ex.Message} (line {line + 1})"
                );
            }
        }
        return map;
    }
    #endregion

    #region private methods
    private static Dictionary<int, TagEntry> ParseMap(string content, bool forBmg)
    {
        var tags = new Dictionary<int, TagEntry>();
        var valueMaps = new Dictionary<string, (List<MsbtValueInfo>, DataType)>();
        var mapReferences = new Dictionary<int, (object, string)>();

        var tagSection = false;
        var mapSection = false;
        TagEntry? currentTag = null;
        var discardDefined = false;
        (List<MsbtValueInfo>, DataType)? currentValueMap = null;

        //parse file
        var lines = content.Split('\n');
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                continue;

            //parse tag header
            if (TagRegex.IsMatch(line, out var tagMatch))
            {
                tagSection = true;
                mapSection = false;
                discardDefined = false;

                ushort group;
                ushort type = 0;
                ushort[] typeList = [];
                var isDiscardType = tagMatch.Groups[2].Value == "_";
                string? mapName = null;

                if (forBmg)
                {
                    if (!byte.TryParse(tagMatch.Groups[1].Value, out var groupVal))
                        throw new FormatException(
                            $"Failed to parse group ID as 8 bit integer (line {line + 1})."
                        );
                    group = groupVal;
                }
                else if (!ushort.TryParse(tagMatch.Groups[1].Value, out group))
                    throw new FormatException(
                        $"Failed to parse group ID as 16 bit integer (line {line + 1})."
                    );

                if (
                    !isDiscardType
                    && !TryGetTypeList(tagMatch.Groups[2].Value, out typeList)
                    && !TryGetMapName(tagMatch.Groups[2].Value, out mapName)
                    && !ushort.TryParse(tagMatch.Groups[2].Value, out type)
                )
                {
                    throw new FormatException(
                        $"Failed to parse type ID as 16 bit integer (line {line + 1})."
                    );
                }

                currentTag = new TagEntry
                {
                    Group = group,
                    Type =
                        isDiscardType || typeList.Length > 0 || mapName is not null ? null : type,
                    TypeList = typeList,
                    HasDiscard = isDiscardType,
                    Name = tagMatch.Groups[3].Success ? tagMatch.Groups[3].Value : string.Empty,
                    Description = tagMatch.Groups[4].Success
                        ? tagMatch.Groups[4].Value.Trim()
                        : string.Empty,
                };
                tags.Add(i, currentTag);

                if (mapName is not null)
                    mapReferences.Add(i, (currentTag, mapName));

                continue;
            }

            //parse tag content
            if (tagSection && ArgumentRegex.IsMatch(line, out var argumentMatch))
            {
                if (discardDefined)
                    throw new FormatException(
                        $"Argument discard padding must be the last argument in a tag declaration (line {i + 1})."
                    );

                var dataType = DataTypes.Bool;
                var isPadding = false;
                if (
                    !TryGetMapName(argumentMatch.Groups[1].Value, out var mapName)
                    && !DatatypeMap.TryGetValue(argumentMatch.Groups[1].Value, out dataType)
                )
                {
                    if (ArgumentPaddingRegex.IsMatch(argumentMatch.Groups[1].Value))
                    {
                        dataType = DataTypes.GetPadding(argumentMatch.Groups[1].Value);
                        isPadding = true;
                    }
                    else if (
                        ArgumentDiscardPaddingRegex.IsMatch(
                            argumentMatch.Groups[1].Value,
                            out var discardMatch
                        )
                    )
                    {
                        if (argumentMatch.Groups[2].Success)
                            throw new FormatException(
                                $"Argument discard padding cannot be an array (line {i + 1})."
                            );

                        dataType = DataTypes.GetPadding(
                            discardMatch.Groups[1].Success ? discardMatch.Groups[1].Value : "0x00"
                        );
                        isPadding = true;
                        discardDefined = true;
                    }
                    else
                        throw new FormatException(
                            $"Unknown argument data type \"{argumentMatch.Groups[1].Value}\" on line {i + 1}."
                        );
                }

                var arrayLength = 0;
                if (
                    argumentMatch.Groups[2].Success
                    && !int.TryParse(argumentMatch.Groups[2].Value, out arrayLength)
                )
                    throw new FormatException(
                        $"Failed to parse array length as 32 bit integer (line {i + 1})."
                    );

                var name = argumentMatch.Groups[3].Success
                    ? argumentMatch.Groups[3].Value
                    : $"arg{currentTag!.Arguments.Count + 1}";

                var arg = new ArgumentEntry
                {
                    DataType = dataType,
                    ArrayLength = arrayLength,
                    IsDiscard = discardDefined,
                    Name = name,
                    Description = argumentMatch.Groups[4].Success
                        ? argumentMatch.Groups[4].Value.Trim()
                        : string.Empty,
                };

                if (
                    !currentTag!.Arguments.TryAdd(
                        arg,
                        a => a.DataType is PaddingDataType && isPadding || a.Name != name
                    )
                )
                {
                    throw new FormatException(
                        $"An argument declaration with the same name already exists ({i + 1})."
                    );
                }

                if (mapName is not null)
                    mapReferences.Add(i, (arg, mapName));

                continue;
            }

            //parse map header
            if (MapRegex.IsMatch(line, out var mapMatch))
            {
                tagSection = false;
                mapSection = true;

                var dataType = DataTypes.UInt16;
                if (
                    mapMatch.Groups[2].Success
                    && !DatatypeMap.TryGetValue(mapMatch.Groups[2].Value, out dataType)
                )
                {
                    throw new FormatException(
                        $"Unknown map data type \"{mapMatch.Groups[2].Value}\" on line {i + 1}."
                    );
                }

                currentValueMap = ([], dataType);

                if (
                    !valueMaps.TryAdd(
                        mapMatch.Groups[1].Value,
                        ((List<MsbtValueInfo>, DataType))currentValueMap
                    )
                )
                {
                    throw new FormatException(
                        $"A map declaration with the same name already exists (line {i + 1})."
                    );
                }

                continue;
            }

            //parse map values
            if (mapSection && MapValueRegex.IsMatch(line, out var mapValueMatch))
            {
                var value = mapValueMatch.Groups[1].Value;
                var name = mapValueMatch.Groups[2].Success
                    ? mapValueMatch.Groups[2].Value
                    : string.Empty;

                var datatype = currentValueMap!.Value.Item2;
                string valueStr;
                try
                {
                    var valueBytes = datatype.Serialize(
                        value,
                        !BitConverter.IsLittleEndian,
                        Encoding.UTF8
                    );
                    valueStr = datatype
                        .Deserialize(valueBytes, 0, !BitConverter.IsLittleEndian, Encoding.UTF8)
                        .Item1;
                }
                catch
                {
                    throw new FormatException(
                        $"Failed to convert map value \"{value}\" to \"{datatype.Name}\" (line {i + 1})."
                    );
                }

                var item = new MsbtValueInfo
                {
                    Value = valueStr,
                    Name = name,
                    Description = mapValueMatch.Groups[3].Success
                        ? mapValueMatch.Groups[3].Value.Trim()
                        : string.Empty,
                };

                if (
                    !currentValueMap.Value.Item1.TryAdd(
                        item,
                        v => v.Value != value && (string.IsNullOrEmpty(name) || v.Name != name)
                    )
                )
                {
                    throw new FormatException(
                        $"A map item with the same name or value already exists ({i + 1})."
                    );
                }

                continue;
            }

            if (tagSection)
                throw new FormatException($"Invalid argument format on line {i + 1}.");
            if (mapSection)
                throw new FormatException($"Invalid map item format on line {i + 1}.");
            throw new FormatException($"Unrecognized value found on line {i + 1}");
        }

        //resolve map references
        foreach (var (line, (reference, mapName)) in mapReferences)
        {
            if (!valueMaps.TryGetValue(mapName, out var valueMapData))
            {
                throw new FormatException(
                    $"Value map \"{mapName}\" is not defined (line {line + 1})."
                );
            }

            MsbtValueMap valueMap;
            try
            {
                valueMap = MsbtValueMap.Create(valueMapData.Item1, valueMapData.Item2);
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    $"Failed to parse value map \"{mapName}\" (line {line + 1}): {ex.Message}"
                );
            }

            switch (reference)
            {
                case TagEntry when valueMap.DataType != DataTypes.UInt16:
                    throw new FormatException(
                        $"Value maps used for tag types must be \"u16\" (line {line + 1})."
                    );
                case TagEntry tag:
                    tag.TypeMap = valueMap;
                    break;
                case ArgumentEntry arg:
                    arg.DataType = valueMap.DataType;
                    arg.ValueMap = valueMap;
                    break;
            }
        }

        return tags;
    }

    private static bool TryGetTypeList(string input, out ushort[] typeList)
    {
        typeList = [];
        if (!input.Contains('-') && (!input.StartsWith('(') || !input.EndsWith(')')))
            return false;

        var types = new HashSet<ushort>();

        foreach (var item in input.TrimStart('(').TrimEnd(')').Split(','))
        {
            var bounds = item.Split('-');

            var lowerBound = ushort.Parse(bounds[0]);
            types.Add(lowerBound);
            if (bounds.Length == 1)
                continue;

            var upperBound = ushort.Parse(bounds[1]);
            for (var i = lowerBound; i <= upperBound; ++i)
                types.Add(i);
        }

        typeList = [.. types];
        Array.Sort(typeList);
        return true;
    }

    private static bool TryGetMapName(string input, [MaybeNullWhen(false)] out string name)
    {
        name = null;
        if (!input.StartsWith('{') || !input.EndsWith('}'))
            return false;

        name = input[1..^1];
        return true;
    }
    #endregion

    #region helper classes
    private class TagEntry
    {
        public required ushort Group { get; init; }

        public ushort? Type { get; init; }

        public ushort[] TypeList { get; init; } = [];

        public MsbtValueMap? TypeMap { get; set; }

        public bool HasDiscard { get; init; }

        public required string Name { get; init; }

        public string Description { get; init; } = string.Empty;

        public List<ArgumentEntry> Arguments { get; } = [];

        public BmgTagInfo ToBmgInfo() =>
            new()
            {
                Group = (byte)Group,
                Type = Type,
                TypeList = TypeList,
                TypeMap = TypeMap ?? MsbtValueMap.Empty,
                HasDiscard = HasDiscard,
                Name = Name,
                Description = Description,
                Arguments = Arguments.Select(arg => arg.ToMsbtInfo()).ToArray(),
            };

        public MsbtTagInfo ToMsbtInfo() =>
            new()
            {
                Group = Group,
                Type = Type,
                TypeList = TypeList,
                TypeMap = TypeMap ?? MsbtValueMap.Empty,
                HasDiscard = HasDiscard,
                Name = Name,
                Description = Description,
                Arguments = Arguments.Select(arg => arg.ToMsbtInfo()).ToArray(),
            };
    }

    private class ArgumentEntry
    {
        public required DataType DataType { get; set; }

        public MsbtValueMap? ValueMap { get; set; }

        public int ArrayLength { get; init; }

        public bool IsDiscard { get; init; }

        public required string Name { get; init; }

        public string Description { get; init; } = string.Empty;

        public MsbtArgumentInfo ToMsbtInfo() =>
            new()
            {
                DataType = DataType,
                ValueMap = ValueMap ?? MsbtValueMap.Empty,
                ArrayLength = ArrayLength,
                IsDiscard = IsDiscard,
                Name = Name,
                Description = Description,
            };
    }
    #endregion
}
