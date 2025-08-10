// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ModbusRx.Server.UI.Visualization;

/// <summary>
/// Data model for register display using ReactiveUI.
/// </summary>
public partial class RegisterData : ReactiveObject
{
    [Reactive]
    private ushort _value;

    /// <summary>
    /// Gets or sets the register address.
    /// </summary>
    public ushort Address { get; set; }

    /// <summary>
    /// Gets the hexadecimal representation of the value.
    /// </summary>
    public string HexValue => $"0x{Value:X4}";

    /// <summary>
    /// Gets or sets a value indicating whether this register can be edited.
    /// </summary>
    public bool IsEditable { get; set; } = true;
}
