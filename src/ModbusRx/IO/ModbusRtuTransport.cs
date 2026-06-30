// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Utility;
#else
using ModbusRx.Utility;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.IO;
#else
namespace ModbusRx.IO;
#endif

/// <summary>Refined Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.</summary>
internal sealed class ModbusRtuTransport : ModbusSerialTransport
{
    /// <summary>Defines the Request Frame Start Length value.</summary>
    public const int RequestFrameStartLength = 7;

    /// <summary>Defines the Response Frame Start Length value.</summary>
    public const int ResponseFrameStartLength = 4;

    /// <summary>Initializes a new instance of the Modbus Rtu Transport class.</summary>
    /// <param name="streamResource">The stream Resource value.</param>
    internal ModbusRtuTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    /// <summary>Executes the Request Bytes To Read operation.</summary>
    /// <param name="frameStart">The frame Start value.</param>
    /// <returns>The result.</returns>
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

                _ => throw new NotSupportedException($"Function code {functionCode} is not supported."),
            };
        }
        catch (NotSupportedException ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>Executes the Response Bytes To Read operation.</summary>
    /// <param name="frameStart">The frame Start value.</param>
    /// <returns>The result.</returns>
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

                _ => throw new NotSupportedException($"Function code {functionCode} is not supported."),
            };
        }
        catch (NotSupportedException ex)
        {
            Debug.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>Executes the Read operation.</summary>
    /// <param name="count">The count value.</param>
    /// <returns>The result.</returns>
    public byte[] Read(int count)
    {
        var frameBytes = new byte[count];
        for (var i = 0; i < count; i++)
        {
            var br = StreamResource.ReadAsync(frameBytes, i, 1).Result;
            if (br != 1)
            {
                throw new IOException($"Unable to read byte at position {i}");
            }
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
        var frame = CombineFrames(frameStart, frameEnd);
        Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} Master RX: {string.Join(", ", frame)}");

        return CreateResponse<T>(Task.FromResult(frame));
    }

    internal override Task<byte[]> ReadRequest()
    {
        var frameStart = Read(RequestFrameStartLength);
        var frameEnd = Read(RequestBytesToRead(frameStart));
        var frame = CombineFrames(frameStart, frameEnd);
        Console.WriteLine($"Slave RX: {string.Join(", ", frame)}");

        return Task.FromResult(frame);
    }

    /// <summary>Combines frame segments into a single message frame.</summary>
    /// <param name="frameStart">The first frame segment.</param>
    /// <param name="frameEnd">The final frame segment.</param>
    /// <returns>The combined frame.</returns>
    private static byte[] CombineFrames(byte[] frameStart, byte[] frameEnd)
    {
        var frame = new byte[frameStart.Length + frameEnd.Length];
        Array.Copy(frameStart, 0, frame, 0, frameStart.Length);
        Array.Copy(frameEnd, 0, frame, frameStart.Length, frameEnd.Length);
        return frame;
    }
}
