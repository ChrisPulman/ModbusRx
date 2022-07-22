// <copyright file="CustomWriteMultipleRegistersResponse.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using ModbusRx.Message;

namespace ModbusRx.IntegrationTests.CustomMessages;

/// <summary>
/// CustomWriteMultipleRegistersResponse.
/// </summary>
/// <seealso cref="ModbusRx.Message.IModbusMessage" />
public class CustomWriteMultipleRegistersResponse : IModbusMessage
{
    /// <summary>
    /// Gets composition of the slave address and protocol data unit.
    /// </summary>
    public byte[] MessageFrame
    {
        get
        {
            var frame = new List<byte>
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
                FunctionCode
            };
            pdu.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)StartAddress)));
            pdu.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)NumberOfPoints)));

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
    /// Gets or sets the start address.
    /// </summary>
    /// <value>
    /// The start address.
    /// </value>
    public ushort StartAddress { get; set; }

    /// <summary>
    /// Gets or sets the number of points.
    /// </summary>
    /// <value>
    /// The number of points.
    /// </value>
    public ushort NumberOfPoints { get; set; }

    /// <summary>
    /// Initializes a modbus message from the specified message frame.
    /// </summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <exception cref="System.ArgumentNullException">frame.</exception>
    /// <exception cref="System.FormatException">Message frame does not contain enough bytes.</exception>
    public void Initialize(byte[] frame)
    {
        if (frame == null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (frame.Length < 6)
        {
            throw new FormatException("Message frame does not contain enough bytes.");
        }

        SlaveAddress = frame[0];
        FunctionCode = frame[1];
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
    }
}
