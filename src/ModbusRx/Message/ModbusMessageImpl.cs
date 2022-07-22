// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using ModbusRx.Data;

namespace ModbusRx.Message;

/// <summary>
///     Class holding all implementation shared between two or more message types.
///     Interfaces expose subsets of type specific implementations.
/// </summary>
internal class ModbusMessageImpl
{
    public ModbusMessageImpl()
    {
    }

    public ModbusMessageImpl(byte slaveAddress, byte functionCode)
    {
        SlaveAddress = slaveAddress;
        FunctionCode = functionCode;
    }

    public byte? ByteCount { get; set; }

    public byte? ExceptionCode { get; set; }

    public ushort TransactionId { get; set; }

    public byte FunctionCode { get; set; }

    public ushort? NumberOfPoints { get; set; }

    public byte SlaveAddress { get; set; }

    public ushort? StartAddress { get; set; }

    public ushort? SubFunctionCode { get; set; }

    public IDataCollection? Data { get; set; }

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

    public byte[] ProtocolDataUnit
    {
        get
        {
            var pdu = new List<byte>
            {
                FunctionCode
            };

            if (ExceptionCode.HasValue)
            {
                pdu.Add(ExceptionCode.Value);
            }

            if (SubFunctionCode.HasValue)
            {
                pdu.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)SubFunctionCode.Value)));
            }

            if (StartAddress.HasValue)
            {
                pdu.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)StartAddress.Value)));
            }

            if (NumberOfPoints.HasValue)
            {
                pdu.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)NumberOfPoints.Value)));
            }

            if (ByteCount.HasValue)
            {
                pdu.Add(ByteCount.Value);
            }

            if (Data is not null)
            {
                pdu.AddRange(Data.NetworkBytes);
            }

            return pdu.ToArray();
        }
    }

    public void Initialize(byte[] frame)
    {
        if (frame == null)
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
