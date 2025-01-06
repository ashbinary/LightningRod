﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// Default implementation of a <see cref="IMsbtFormatProvider"/>.<br/>
/// Message format: <c>message.Label</c> : <c>formattedText</c><br/>
/// Tag format:<br/>
/// - Empty tag name: <c>string.Empty</c><br/>
/// - Without arguments: {{<c>tagName</c>}}<br/>
/// - With arguments: {{<c>tagName</c> <c>arg.Name</c>="<c>arg.Value</c>"}}
/// </summary>
public class MsbtDefaultFormatProvider : IMsbtFormatProvider
{
    #region IMsbtFormatProvider interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public string FormatMessage(MsbtMessage message, string rawText)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        ArgumentNullException.ThrowIfNull(rawText, nameof(rawText));
        #else
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (rawText is null) throw new ArgumentNullException(nameof(rawText));
        #endif

        return rawText;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public string FormatTag(MsbtMessage message, string tagName, IEnumerable<MsbtTagArgument> arguments)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(message, nameof(message));
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));
        #else
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (arguments is null) throw new ArgumentNullException(nameof(arguments));
        #endif
        if (string.IsNullOrEmpty(tagName)) return string.Empty;

        var sb = new StringBuilder();
        sb.Append("{{").Append(tagName);

        foreach (var arg in arguments)
        {
            sb.Append(' ').Append(arg.Name).Append("=\"").Append(arg.Value).Append('\"');
        }

        sb.Append("}}");
        return sb.ToString();
    }
    #endregion
}