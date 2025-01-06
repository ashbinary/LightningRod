using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NintendoTools.Utils;

internal sealed class YamlTextWriter(TextWriter writer) : IDisposable
{
    #region private members
    private static readonly char[] UnsafeChars =
    [
        ':',
        '{',
        '}',
        '[',
        ']',
        ',',
        '&',
        '*',
        '#',
        '?',
        '|',
        '-',
        '<',
        '>',
        '=',
        '!',
        '%',
        '@',
        '`',
    ];

    private readonly Stack<WriterState> _stateStack = new();
    private int _level;
    private bool _firstDictInArray;
    private bool _firstArrayInArray;
    private bool _disposed;
    #endregion

    #region public methods
    public void WriteStartDocument()
    {
        writer.WriteLine("---");
        _stateStack.Clear();
    }

    public void WriteStartArray()
    {
        if (_stateStack.TryPeek(out var state))
        {
            switch (state)
            {
                case WriterState.Dictionary:
                    throw new YamlTextWriterException(
                        "Invalid writer state. Start of array cannot be written after dictionary."
                    );
                case WriterState.Array:
                    writer.Write(GetIndent() + "-");
                    _firstArrayInArray = true;
                    break;
                case WriterState.Property:
                    writer.WriteLine();
                    break;
            }
        }

        _stateStack.Push(WriterState.Array);
        ++_level;
    }

    public void WriteEndArray()
    {
        switch (_stateStack.Pop())
        {
            case WriterState.Property:
                throw new YamlTextWriterException(
                    "Invalid writer state. End of array cannot be written after property."
                );
        }

        --_level;

        if (_stateStack.TryPeek(out var state) && state == WriterState.Property)
            _stateStack.Pop();
    }

    public void WriteStartDictionary()
    {
        if (_stateStack.TryPeek(out var state))
        {
            switch (state)
            {
                case WriterState.Dictionary:
                    throw new YamlTextWriterException(
                        "Invalid writer state. Start of dictionary cannot be written after dictionary."
                    );
                case WriterState.Array:
                    writer.Write(GetIndent() + "- ");
                    _firstDictInArray = true;
                    break;
                case WriterState.Property:
                    writer.WriteLine();
                    break;
            }
        }

        _stateStack.Push(WriterState.Dictionary);
        ++_level;
    }

    public void WriteEndDictionary()
    {
        switch (_stateStack.Pop())
        {
            case WriterState.Property:
                throw new YamlTextWriterException(
                    "Invalid writer state. End of dictionary cannot be written after property."
                );
        }

        --_level;

        if (_stateStack.TryPeek(out var state) && state == WriterState.Property)
            _stateStack.Pop();
    }

    public void WritePropertyName(string name)
    {
        if (_stateStack.TryPeek(out var state))
        {
            switch (state)
            {
                case WriterState.Array:
                    throw new YamlTextWriterException(
                        "Invalid writer state. Property cannot be written after array."
                    );
                case WriterState.Property:
                    throw new YamlTextWriterException(
                        "Invalid writer state. Property cannot be written after property."
                    );
            }
        }

        if (_firstDictInArray)
            writer.Write(SafePropertyName(name) + ":");
        else
            writer.Write(GetIndent() + SafePropertyName(name) + ":");

        _stateStack.Push(WriterState.Property);
        _firstDictInArray = false;
    }

    public void WriteValue(bool value) => InternalWriteValue(value ? "true" : "false");

    public void WriteValue(byte value) => InternalWriteValue(value.ToString());

    public void WriteValue(sbyte value) => InternalWriteValue(value.ToString());

    public void WriteValue(short value) => InternalWriteValue(value.ToString());

    public void WriteValue(ushort value) => InternalWriteValue(value.ToString());

    public void WriteValue(int value) => InternalWriteValue(value.ToString());

    public void WriteValue(uint value) => InternalWriteValue(value.ToString());

    public void WriteValue(long value) => InternalWriteValue(value.ToString());

    public void WriteValue(ulong value) => InternalWriteValue(value.ToString());

    public void WriteValue(decimal value) =>
        InternalWriteValue(value.ToString(NumberFormatInfo.InvariantInfo));

    public void WriteValue(float value) =>
        InternalWriteValue(value.ToString(NumberFormatInfo.InvariantInfo));

    public void WriteValue(double value) =>
        InternalWriteValue(value.ToString(NumberFormatInfo.InvariantInfo));

    public void WriteValue(char value) => InternalWriteValue("\"" + value + "\"");

    public void WriteValue(string value) => InternalWriteValue("\"" + value + "\"");

    public void WriteValue(DateTime value) =>
        InternalWriteValue(value.ToString("s", CultureInfo.InvariantCulture));

    public void WriteNull() => InternalWriteValue("null");

    public void WriteValue(object? value)
    {
        if (value is null)
            WriteNull();
        else
        {
            switch (value)
            {
                case bool b:
                    WriteValue(b);
                    break;
                case byte bt:
                    WriteValue(bt);
                    break;
                case sbyte sbt:
                    WriteValue(sbt);
                    break;
                case short s:
                    WriteValue(s);
                    break;
                case ushort us:
                    WriteValue(us);
                    break;
                case int i:
                    WriteValue(i);
                    break;
                case uint ui:
                    WriteValue(ui);
                    break;
                case long l:
                    WriteValue(l);
                    break;
                case ulong ul:
                    WriteValue(ul);
                    break;
                case decimal dec:
                    WriteValue(dec);
                    break;
                case float f:
                    WriteValue(f);
                    break;
                case double d:
                    WriteValue(d);
                    break;
                case char chr:
                    WriteValue(chr);
                    break;
                case string str:
                    WriteValue(str);
                    break;
                case DateTime dt:
                    WriteValue(dt);
                    break;
                default:
                    throw new YamlTextWriterException("Invalid value type.");
            }
        }
    }

    public void Flush() => writer.Flush();

    public void Close() => Dispose();
    #endregion

    #region private methods
    private string GetIndent() => new(' ', _level * 2);

    private void InternalWriteValue(string value)
    {
        switch (_stateStack.Peek())
        {
            case WriterState.Dictionary:
                throw new YamlTextWriterException(
                    "Invalid writer state. Value cannot be written after dictionary."
                );
            case WriterState.Array:
                if (_firstArrayInArray)
                    writer.Write(" -");
                else
                    writer.Write(GetIndent() + "-");
                break;
            case WriterState.Property:
                _stateStack.Pop();
                break;
        }

        writer.WriteLine(" " + value);
        _firstArrayInArray = false;
    }

    private static string SafePropertyName(string name)
    {
        foreach (var c in UnsafeChars)
        {
            if (name.IndexOf(c) > -1)
                return "'" + name + "'";
        }

        return name;
    }
    #endregion

    #region IDisposable interface
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;
        writer.Flush();
        writer.Close();
        _disposed = true;
    }
    #endregion

    #region helper classes
    private enum WriterState
    {
        Dictionary,
        Array,
        Property,
    }
    #endregion
}
