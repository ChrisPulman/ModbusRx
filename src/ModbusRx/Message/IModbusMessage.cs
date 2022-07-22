// <copyright file="IModbusMessage.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace ModbusRx.Message;

/// <summary>
///     A message built by the master (client) that initiates a Modbus transaction.
/// </summary>
public interface IModbusMessage
{
    /// <summary>
    ///     Gets or sets the function code tells the server what kind of action to perform.
    /// </summary>
    byte FunctionCode { get; set; }

    /// <summary>
    ///     Gets or sets address of the slave (server).
    /// </summary>
    byte SlaveAddress { get; set; }

    /// <summary>
    ///     Gets composition of the slave address and protocol data unit.
    /// </summary>
    byte[] MessageFrame { get; }

    /// <summary>
    ///     Gets composition of the function code and message data.
    /// </summary>
    byte[] ProtocolDataUnit { get; }

    /// <summary>
    ///     Gets or sets a unique identifier assigned to a message when using the IP protocol.
    /// </summary>
    ushort TransactionId { get; set; }

    /// <summary>
    ///     Initializes a modbus message from the specified message frame.
    /// </summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    void Initialize(byte[] frame);
}
