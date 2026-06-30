// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModbusRx.Data;
using ModbusRx.Message;

namespace ModbusRx.IntegrationTests.CustomMessages;

/// <summary>Custom read holding registers response.</summary>
/// <seealso cref="IModbusMessage" />
public class CustomReadHoldingRegistersResponse : IModbusMessage
{
    /// <summary>The data.</summary>
    private RegisterCollection? _data;

    /// <summary>Gets the data.</summary>
    /// <value>
    /// The data.
    /// </value>
    public ushort[] Data => [.. _data!];

    /// <summary>Gets composition of the slave address and protocol data unit.</summary>
    public byte[] MessageFrame
    {
        get
        {
            var frame = new List<byte>()
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
                FunctionCode,
                ByteCount
            };
            pdu.AddRange(_data!.NetworkBytes);

            return [.. pdu];
        }
    }

    /// <summary>Gets or sets a unique identifier assigned to a message when using the IP protocol.</summary>
    public ushort TransactionId { get; set; }

    /// <summary>Gets or sets the function code tells the server what kind of action to perform.</summary>
    public byte FunctionCode { get; set; }

    /// <summary>Gets or sets address of the slave (server).</summary>
    public byte SlaveAddress { get; set; }

    /// <summary>Gets or sets the byte count.</summary>
    /// <value>
    /// The byte count.
    /// </value>
    public byte ByteCount { get; set; }

    /// <summary>Initializes a modbus message from the specified message frame.</summary>
    /// <param name="frame">Bytes of Modbus frame.</param>
    /// <exception cref="System.ArgumentNullException">frame.</exception>
    /// <exception cref="System.ArgumentException">Message frame does not contain enough bytes. - frame.</exception>
    public void Initialize(byte[] frame)
    {
        if (frame is null)
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
        _data = new(CopyFrameData(frame, 3, ByteCount));
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
