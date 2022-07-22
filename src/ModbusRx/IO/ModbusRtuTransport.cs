// <copyright file="ModbusRtuTransport.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using ModbusRx.Message;
using ModbusRx.Utility;

namespace ModbusRx.IO;

/// <summary>
///     Refined Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.
/// </summary>
internal class ModbusRtuTransport : ModbusSerialTransport
{
    public const int RequestFrameStartLength = 7;

    public const int ResponseFrameStartLength = 4;

    internal ModbusRtuTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    public static int RequestBytesToRead(byte[] frameStart)
    {
        var functionCode = frameStart[1];
        try
        {
            return functionCode switch
            {
                Modbus.ReadCoils or
                Modbus.ReadInputs or
                Modbus.ReadHoldingRegisters or
                Modbus.ReadInputRegisters or
                Modbus.WriteSingleCoil or
                Modbus.WriteSingleRegister or
                Modbus.Diagnostics
                    => 1,

                Modbus.WriteMultipleCoils or
                Modbus.WriteMultipleRegisters
                    => frameStart[6] + 2,

                _ => throw new NotImplementedException($"Function code {functionCode} not supported."),
            };
        }
        catch (NotImplementedException ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    public static int ResponseBytesToRead(byte[] frameStart)
    {
        var functionCode = frameStart[1];

        // exception response
        if (functionCode > Modbus.ExceptionOffset)
        {
            return 1;
        }

        try
        {
            return functionCode switch
            {
                Modbus.ReadCoils or
                Modbus.ReadInputs or
                Modbus.ReadHoldingRegisters or
                Modbus.ReadInputRegisters
                    => frameStart[2] + 1,

                Modbus.WriteSingleCoil or
                Modbus.WriteSingleRegister or
                Modbus.WriteMultipleCoils or
                Modbus.WriteMultipleRegisters or
                Modbus.Diagnostics
                    => 4,

                _ => throw new NotImplementedException($"Function code {functionCode} not supported."),
            };
        }
        catch (NotImplementedException ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    public virtual byte[] Read(int count)
    {
        var frameBytes = new byte[count];
        var numBytesRead = 0;

        while (numBytesRead != count)
        {
            numBytesRead += StreamResource.ReadAsync(frameBytes, numBytesRead, count - numBytesRead).Result;
        }

        return frameBytes;
    }

    internal override byte[] BuildMessageFrame(IModbusMessage message)
    {
        var messageFrame = message.MessageFrame;
        var crc = ModbusUtility.CalculateCrc(messageFrame);
        var messageBody = new MemoryStream(messageFrame.Length + crc.Length);

        messageBody.Write(messageFrame, 0, messageFrame.Length);
        messageBody.Write(crc, 0, crc.Length);

        return messageBody.ToArray();
    }

    internal override bool ChecksumsMatch(IModbusMessage message, byte[] messageFrame) =>
        BitConverter.ToUInt16(messageFrame, messageFrame.Length - 2) ==
            BitConverter.ToUInt16(ModbusUtility.CalculateCrc(message.MessageFrame), 0);

    internal override Task<IModbusMessage> ReadResponse<T>()
    {
        var frameStart = Read(ResponseFrameStartLength);
        var frameEnd = Read(ResponseBytesToRead(frameStart));
        var frame = Enumerable.Concat(frameStart, frameEnd).ToArray();
        Debug.WriteLine($"RX: {string.Join(", ", frame)}");

        return CreateResponse<T>(Task.FromResult(frame));
    }

    internal override Task<byte[]> ReadRequest()
    {
        var frameStart = Read(RequestFrameStartLength);
        var frameEnd = Read(RequestBytesToRead(frameStart));
        var frame = Enumerable.Concat(frameStart, frameEnd).ToArray();
        Debug.WriteLine($"RX: {string.Join(", ", frame)}");

        return Task.FromResult(frame);
    }
}
