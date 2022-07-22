// <copyright file="ModbusDataType.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ModbusRx.Data;

/// <summary>
///     Types of data supported by the Modbus protocol.
/// </summary>
public enum ModbusDataType
{
    /// <summary>
    ///     Read/write register.
    /// </summary>
    HoldingRegister,

    /// <summary>
    ///     Readonly register.
    /// </summary>
    InputRegister,

    /// <summary>
    ///     Read/write discrete.
    /// </summary>
    Coil,

    /// <summary>
    ///     Readonly discrete.
    /// </summary>
    Input,
}