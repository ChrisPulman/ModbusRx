// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using ModbusRx.Data;
using ModbusRx.Message;

namespace ModbusRx.IntegrationTests.CustomMessages;

/// <summary>Custom write multiple registers request.</summary>
/// <seealso cref="IModbusMessage" />
public class CustomWriteMultipleRegistersRequest : IModbusMessage
{
    /// <summary>Initializes a new instance of the <see cref="CustomWriteMultipleRegistersRequest"/> class.</summary>
    /// <param name="functionCode">The function code.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="data">The data.</param>
    public CustomWriteMultipleRegistersRequest(byte functionCode, byte slaveAddress, ushort startAddress, RegisterCollection data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        FunctionCode = functionCode;
        SlaveAddress = slaveAddress;
        StartAddress = startAddress;
        NumberOfPoints = (ushort)data.Count;
        ByteCount = data.ByteCount;
        Data = data;
    }

    /// <summary>Gets composition of the slave address and protocol data unit.</summary>
    public byte[] MessageFrame
    {
        get
        {
            var frame = new List<byte>
            {
                SlaveAddress
            };
            frame.AddRange(ProtocolDataUnit);

            return [.. frame];
        }
    }

    /// <summary>Gets composition of the function code and message data.</summary>
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
            pdu.Add(ByteCount);
            pdu.AddRange(Data.NetworkBytes);

            return [.. pdu];
        }
    }

    /// <summary>Gets or sets a unique identifier assigned to a message when using the IP protocol.</summary>
    public ushort TransactionId { get; set; }

    /// <summary>Gets or sets the function code tells the server what kind of action to perform.</summary>
    public byte FunctionCode { get; set; }

    /// <summary>Gets or sets address of the slave (server).</summary>
    public byte SlaveAddress { get; set; }

    /// <summary>Gets or sets the start address.</summary>
    /// <value>
    /// The start address.
    /// </value>
    public ushort StartAddress { get; set; }

    /// <summary>Gets or sets the number of points.</summary>
    /// <value>
    /// The number of points.
    /// </value>
    public ushort NumberOfPoints { get; set; }

    /// <summary>Gets or sets the byte count.</summary>
    /// <value>
    /// The byte count.
    /// </value>
    public byte ByteCount { get; set; }

    /// <summary>Gets or sets the data.</summary>
    /// <value>
    /// The data.
    /// </value>
    public RegisterCollection Data { get; set; }

    /// <summary>Initializes a modbus message from the specified message frame.</summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <exception cref="System.ArgumentNullException">frame.</exception>
    /// <exception cref="System.FormatException">Message frame does not contain enough bytes.</exception>
    public void Initialize(byte[] frame)
    {
        if (frame is null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (frame.Length < 7 || frame.Length < 7 + frame[6])
        {
            throw new FormatException("Message frame does not contain enough bytes.");
        }

        SlaveAddress = frame[0];
        FunctionCode = frame[1];
        StartAddress = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 2));
        NumberOfPoints = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 4));
        ByteCount = frame[6];
        Data = new(CopyFrameData(frame, 7, ByteCount));
    }

    /// <summary>Copies register data bytes from a frame.</summary>
    /// <param name="frame">The source frame.</param>
    /// <param name="startIndex">The first byte to copy.</param>
    /// <param name="count">The number of bytes to copy.</param>
    /// <returns>The copied frame bytes.</returns>
    private static byte[] CopyFrameData(byte[] frame, int startIndex, int count)
    {
        var result = new byte[count];
        Array.Copy(frame, startIndex, result, 0, count);
        return result;
    }
}
