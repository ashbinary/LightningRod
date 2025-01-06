using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class that holds a unique lookup table for <see cref="MsbtValueInfo"/> entries.
/// </summary>
public sealed class MsbtValueMap : IEnumerable<MsbtValueInfo>
{
    #region private members
    private readonly List<MsbtValueInfo> _valueList;
    private readonly Dictionary<string, MsbtValueInfo> _lookupTable;
    #endregion

    #region constructor
    private MsbtValueMap(List<MsbtValueInfo> valueList, Dictionary<string, MsbtValueInfo> lookupTable, DataType dataType)
    {
        _valueList = valueList;
        _lookupTable = lookupTable;
        DataType = dataType;
    }
    #endregion

    #region public properties
    /// <summary>
    /// The datatype of the mapped values.
    /// </summary>
    public DataType DataType { get; }

    /// <summary>
    /// Gets the number of <see cref="MsbtValueInfo"/> elements in this map.
    /// </summary>
    public int Count => _valueList.Count;

    /// <summary>
    /// Gets an empty map instance.
    /// </summary>
    public static MsbtValueMap Empty { get; } = new([], [], DataTypes.UInt16);
    #endregion

    #region public methods
    /// <summary>
    /// Determines whether a <see cref="MsbtValueInfo"/> element with a value or name equal to the given value exists in this map.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <returns><see langword="true"/> if the element was found; otherwise <see langword="false"/>.</returns>
    public bool Contains(string value) => _lookupTable.ContainsKey(value);

    /// <summary>
    /// Finds a <see cref="MsbtValueInfo"/> element with a value or name equal to the given value exists in this map.
    /// </summary>
    /// <param name="value">The value to find.</param>
    /// <param name="valueInfo">The found <see cref="MsbtValueInfo"/> element.</param>
    /// <returns><see langword="true"/> if the element was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(string value, [MaybeNullWhen(false)] out MsbtValueInfo valueInfo) => _lookupTable.TryGetValue(value, out valueInfo);

    /// <summary>
    /// Creates a new <see cref="MsbtValueMap"/> instance from a collection of <see cref="MsbtValueInfo"/> elements.
    /// </summary>
    /// <param name="values">The collection of <see cref="MsbtValueInfo"/> elements.</param>
    /// <param name="dataType">The datatype of the values.</param>
    /// <returns>A new <see cref="MsbtValueMap"/> instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static MsbtValueMap Create(IEnumerable<MsbtValueInfo> values, DataType dataType)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        ArgumentNullException.ThrowIfNull(dataType, nameof(dataType));
        #else
        if (values is null) throw new ArgumentNullException(nameof(values));
        if (dataType is null) throw new ArgumentNullException(nameof(dataType));
        #endif

        var valueList = new List<MsbtValueInfo>();
        var lookupTable = new Dictionary<string, MsbtValueInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in values)
        {
            string val;
            try
            {
                var tmp = dataType.Serialize(value.Value, false, Encoding.UTF8);
                val = dataType.Deserialize(tmp, 0, false, Encoding.UTF8).Item1;
            }
            catch
            {
                throw new ArgumentException($"Failed to parse value as {dataType.Name}.");
            }
            if (!lookupTable.TryAdd(val, value)) throw new ArgumentException($"Entry with the value {value.Value} already exists.");
            if (!string.IsNullOrEmpty(value.Name) && !value.Name.Equals(value.Value, StringComparison.OrdinalIgnoreCase) && !lookupTable.TryAdd(value.Name, value)) throw new ArgumentException($"Entry with the name {value.Name} already exists.");
            valueList.Add(value);
        }

        return new MsbtValueMap(valueList, lookupTable, dataType);
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<MsbtValueInfo> GetEnumerator() => _valueList.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}