// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using ModbusRx.Message;

namespace ModbusRx.IntegrationTests.CustomMessages;

/// <summary>
/// CustomReadHoldingRegistersRequest.
/// </summary>
/// <seealso cref="ModbusRx.Message.IModbusMessage" />
/// <remarks>
/// Initializes a new instance of the <see cref="CustomReadHoldingRegistersRequest"/> class.
/// </remarks>
/// <param name="functionCode">The function code.</param>
/// <param name="slaveAddress">The slave address.</param>
/// <param name="startAddress">The start address.</param>
/// <param name="numberOfPoints">The number of points.</param>
public class CustomReadHoldingRegistersRequest(byte functionCode, byte slaveAddress, ushort startAddress, ushort numberOfPoints) : IModbusMessage
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
    public byte FunctionCode { get; set; } = functionCode;

    /// <summary>
    /// Gets or sets address of the slave (server).
    /// </summary>
    public byte SlaveAddress { get; set; } = slaveAddress;

    /// <summary>
    /// Gets or sets the start address.
    /// </summary>
    /// <value>
    /// The start address.
    /// </value>
    public ushort StartAddress { get; set; } = startAddress;

    /// <summary>
    /// Gets or sets the number of points.
    /// </summary>
    /// <value>
    /// The number of points.
    /// </value>
    public ushort NumberOfPoints { get; set; } = numberOfPoints;

    /// <summary>
    /// Initializes a modbus message from the specified message frame.
    /// </summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <exception cref="System.ArgumentNullException">frame.</exception>
    /// <exception cref="System.ArgumentException">Invalid frame. - frame.</exception>
    public void Initialize(byte[] frame)
    {
        if (frame == null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (frame.Length != 6)
        {
            throw new ArgumentException("Invalid frame.", nameof(frame));
        }

        SlaveAddress = frame[0];
        FunctionCode = frame[1];
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
    }
}
