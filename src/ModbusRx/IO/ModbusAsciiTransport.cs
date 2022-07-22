// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text;
using ModbusRx.Message;
using ModbusRx.Utility;

namespace ModbusRx.IO;

/// <summary>
///     Refined Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.
/// </summary>
internal class ModbusAsciiTransport : ModbusSerialTransport
{
    internal ModbusAsciiTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    internal override byte[] BuildMessageFrame(IModbusMessage message)
    {
        var msgFrame = message.MessageFrame;

        var msgFrameAscii = ModbusUtility.GetAsciiBytes(msgFrame);
        var lrcAscii = ModbusUtility.GetAsciiBytes(ModbusUtility.CalculateLrc(msgFrame));
        var nlAscii = Encoding.UTF8.GetBytes(Modbus.NewLine.ToCharArray());

        var frame = new MemoryStream(1 + msgFrameAscii.Length + lrcAscii.Length + nlAscii.Length);
        frame.WriteByte((byte)':');
        frame.Write(msgFrameAscii, 0, msgFrameAscii.Length);
        frame.Write(lrcAscii, 0, lrcAscii.Length);
        frame.Write(nlAscii, 0, nlAscii.Length);

        return frame.ToArray();
    }

    internal override bool ChecksumsMatch(IModbusMessage message, byte[] messageFrame) =>
        ModbusUtility.CalculateLrc(message.MessageFrame) == messageFrame[^1];

    internal override Task<byte[]> ReadRequest() =>
        ReadRequestResponse();

    internal override Task<IModbusMessage> ReadResponse<T>() =>
        CreateResponse<T>(ReadRequestResponse());

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
