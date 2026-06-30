// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text;
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
internal sealed class ModbusAsciiTransport : ModbusSerialTransport
{
    /// <summary>Initializes a new instance of the Modbus Ascii Transport class.</summary>
    /// <param name="streamResource">The stream Resource value.</param>
    internal ModbusAsciiTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    internal override byte[] BuildMessageFrame(IModbusMessage message)
    {
        var msgFrame = message.MessageFrame;

        var msgFrameAscii = ModbusUtility.GetAsciiBytes(msgFrame);
        var lrcAscii = ModbusUtility.GetAsciiBytes(ModbusUtility.CalculateLrc(msgFrame));
        var newLineAsciiBytes = Encoding.UTF8.GetBytes(Modbus.NewLine.ToCharArray());

        var frame = new MemoryStream(1 + msgFrameAscii.Length + lrcAscii.Length + newLineAsciiBytes.Length);
        frame.WriteByte((byte)':');
        frame.Write(msgFrameAscii, 0, msgFrameAscii.Length);
        frame.Write(lrcAscii, 0, lrcAscii.Length);
        frame.Write(newLineAsciiBytes, 0, newLineAsciiBytes.Length);

        return frame.ToArray();
    }

    internal override bool ChecksumsMatch(IModbusMessage message, byte[] messageFrame) =>
        ModbusUtility.CalculateLrc(message.MessageFrame) == messageFrame[^1];

    internal override Task<byte[]> ReadRequest() =>
        ReadRequestResponse();

    internal override Task<IModbusMessage> ReadResponse<T>() =>
        CreateResponse<T>(ReadRequestResponse());

    /// <summary>Executes the Read Request Response operation.</summary>
    /// <returns>The result.</returns>
    internal async Task<byte[]> ReadRequestResponse()
    {
        // read message frame, removing frame start ':'
        var frameHex = (await StreamResourceUtility.ReadLineAsync(StreamResource))[1..];

        // convert hex to bytes
        var frame = ModbusUtility.HexToBytes(frameHex);
        Debug.WriteLine($"RX: {string.Join(", ", frame)}");

        if (frame.Length < 3)
        {
            throw new IOException("Premature end of stream, message truncated.");
        }

        return frame;
    }
}
