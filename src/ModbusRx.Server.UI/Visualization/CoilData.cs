// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ModbusRx.Server.UI.Visualization;

/// <summary>
/// Data model for coil display using ReactiveUI.
/// </summary>
public partial class CoilData : ReactiveObject
{
    [Reactive]
    private bool _value;

    /// <summary>
    /// Gets or sets the coil address.
    /// </summary>
    public ushort Address { get; set; }

    /// <summary>
    /// Gets the display representation of the value.
    /// </summary>
    public string DisplayValue => Value ? "TRUE" : "FALSE";

    /// <summary>
    /// Gets or sets a value indicating whether this coil can be edited.
    /// </summary>
    public bool IsEditable { get; set; } = true;
}
