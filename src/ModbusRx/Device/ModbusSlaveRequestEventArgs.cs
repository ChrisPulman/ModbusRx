// <copyright file="ModbusSlaveRequestEventArgs.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Message;

namespace ModbusRx.Device;

/// <summary>
///     Modbus Slave request event args containing information on the message.
/// </summary>
public class ModbusSlaveRequestEventArgs : EventArgs
{
    internal ModbusSlaveRequestEventArgs(IModbusMessage message) => Message = message;

    /// <summary>
    ///     Gets the message.
    /// </summary>
    public IModbusMessage Message { get; }
}