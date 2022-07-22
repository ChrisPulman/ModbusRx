// <copyright file="CustomReadHoldingRegistersResponse.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using ModbusRx.Data;
using ModbusRx.Message;

namespace ModbusRx.IntegrationTests.CustomMessages;

/// <summary>
/// CustomReadHoldingRegistersResponse.
/// </summary>
/// <seealso cref="ModbusRx.Message.IModbusMessage" />
public class CustomReadHoldingRegistersResponse : IModbusMessage
{
    /// <summary>
    /// The data.
    /// </summary>
    private RegisterCollection? _data;

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public ushort[] Data => _data!.ToArray();

    /// <summary>
    /// Gets composition of the slave address and protocol data unit.
    /// </summary>
    public byte[] MessageFrame
    {
        get
        {
            var frame = new List<byte>()
            {
                SlaveAddress
            };
            frame.AddRange(ProtocolDataUnit);

            return frame.ToArray();
        }
    }

    /// <summary>
    /// Gets composition of the function code and message data.
    /// </summary>
    public byte[] ProtocolDataUnit
    {
        get
        {
            var pdu = new List<byte>
            {
                FunctionCode,
                ByteCount
            };
            pdu.AddRange(_data!.NetworkBytes);

            return pdu.ToArray();
        }
    }

    /// <summary>
    /// Gets or sets a unique identifier assigned to a message when using the IP protocol.
    /// </summary>
    public ushort TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the function code tells the server what kind of action to perform.
    /// </summary>
    public byte FunctionCode { get; set; }

    /// <summary>
    /// Gets or sets address of the slave (server).
    /// </summary>
    public byte SlaveAddress { get; set; }

    /// <summary>
    /// Gets or sets the byte count.
    /// </summary>
    /// <value>
    /// The byte count.
    /// </value>
    public byte ByteCount { get; set; }

    /// <summary>
    /// Initializes a modbus message from the specified message frame.
    /// </summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <exception cref="System.ArgumentNullException">frame.</exception>
    /// <exception cref="System.ArgumentException">Message frame does not contain enough bytes. - frame.</exception>
    public void Initialize(byte[] frame)
    {
        if (frame == null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (frame.Length < 3 || frame.Length < 3 + frame[2])
        {
            throw new ArgumentException("Message frame does not contain enough bytes.", nameof(frame));
        }

        SlaveAddress = frame[0];
        FunctionCode = frame[1];
        ByteCount = frame[2];
        _data = new RegisterCollection(frame.Skip(3).Take(ByteCount).ToArray());
    }
}
