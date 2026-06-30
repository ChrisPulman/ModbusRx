// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Net;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Message;
#else
namespace ModbusRx.Message;
#endif

/// <summary>
/// Class holding all implementation shared between two or more message types.
/// Interfaces expose subsets of type specific implementations.
/// </summary>
internal sealed class ModbusMessageImpl
{
    /// <summary>Initializes a new instance of the Modbus Message Impl class.</summary>
    public ModbusMessageImpl()
    {
    }

    /// <summary>Initializes a new instance of the Modbus Message Impl class.</summary>
    /// <param name="slaveAddress">The slave Address value.</param>
    /// <param name="functionCode">The function Code value.</param>
    public ModbusMessageImpl(byte slaveAddress, byte functionCode)
    {
        SlaveAddress = slaveAddress;
        FunctionCode = functionCode;
    }

    /// <summary>Gets or sets the Byte Count value.</summary>
    public byte? ByteCount { get; set; }

    /// <summary>Gets or sets the Exception Code value.</summary>
    public byte? ExceptionCode { get; set; }

    /// <summary>Gets or sets the Transaction Id value.</summary>
    public ushort TransactionId { get; set; }

    /// <summary>Gets or sets the Function Code value.</summary>
    public byte FunctionCode { get; set; }

    /// <summary>Gets or sets the Number Of Points value.</summary>
    public ushort? NumberOfPoints { get; set; }

    /// <summary>Gets or sets the Slave Address value.</summary>
    public byte SlaveAddress { get; set; }

    /// <summary>Gets or sets the Start Address value.</summary>
    public ushort? StartAddress { get; set; }

    /// <summary>Gets or sets the Sub Function Code value.</summary>
    public ushort? SubFunctionCode { get; set; }

    /// <summary>Gets or sets the Data value.</summary>
    public IDataCollection? Data { get; set; }

    /// <summary>Gets the Message Frame value.</summary>
    public byte[] MessageFrame
    {
        get
        {
            var pdu = ProtocolDataUnit;
            var frame = new MemoryStream(1 + pdu.Length);

            frame.WriteByte(SlaveAddress);
            frame.Write(pdu, 0, pdu.Length);

            return frame.ToArray();
        }
    }

    /// <summary>Gets the Protocol Data Unit value.</summary>
    public byte[] ProtocolDataUnit
    {
        get
        {
            var pdu = new List<byte>
            {
                FunctionCode
            };

            AddOptionalByte(pdu, ExceptionCode);
            AddOptionalNetworkOrder(pdu, SubFunctionCode);
            AddOptionalNetworkOrder(pdu, StartAddress);
            AddOptionalNetworkOrder(pdu, NumberOfPoints);
            AddOptionalByte(pdu, ByteCount);
            AddData(pdu, Data);

            return pdu.ToArray();

            static void AddOptionalByte(List<byte> target, byte? value)
            {
                if (!value.HasValue)
                {
                    return;
                }

                target.Add(value.Value);
            }

            static void AddOptionalNetworkOrder(List<byte> target, ushort? value)
            {
                if (!value.HasValue)
                {
                    return;
                }

                target.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)value.Value)));
            }

            static void AddData(List<byte> target, IDataCollection? data)
            {
                if (data is null)
                {
                    return;
                }

                target.AddRange(data.NetworkBytes);
            }
        }
    }

    /// <summary>Executes the Initialize operation.</summary>
    /// <param name="frame">The frame value.</param>
    public void Initialize(byte[] frame)
    {
        if (frame is null)
        {
            throw new ArgumentNullException(nameof(frame));
        }

        if (frame.Length < Modbus.MinimumFrameSize)
        {
            var msg = $"Message frame must contain at least {Modbus.MinimumFrameSize} bytes of data.";
            throw new FormatException(msg);
        }

        SlaveAddress = frame[0];
        FunctionCode = frame[1];
    }
}
