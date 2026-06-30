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
namespace ModbusRx.Reactive.IO;
#else
namespace ModbusRx.IO;
#endif

/// <summary>Transport for Serial protocols. Refined Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.</summary>
public abstract class ModbusSerialTransport : ModbusTransport
{
    /// <summary>Initializes a new instance of the Modbus Serial Transport class.</summary>
    /// <param name="streamResource">The stream Resource value.</param>
    internal ModbusSerialTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    /// <summary>Gets or sets a value indicating whether LRC/CRC frame checking is performed on messages.</summary>
    public bool CheckFrame { get; set; } = true;

    /// <summary>Executes the Discard In Buffer operation.</summary>
    internal void DiscardInBuffer() =>
        StreamResource.DiscardInBuffer();

    internal override void Write(IModbusMessage message)
    {
        DiscardInBuffer();

        var frame = BuildMessageFrame(message);
        Debug.WriteLine($"TX: {string.Join(", ", frame)}");
        StreamResource.Write(frame, 0, frame.Length);
    }

    internal override async Task<IModbusMessage> CreateResponse<T>(Task<byte[]> frame)
    {
        var response = await CreateResponseMessage<T>(frame);

        // compare checksum
        if (CheckFrame && !ChecksumsMatch(response, await frame))
        {
            var msg = $"Checksums failed to match {string.Join(", ", response.MessageFrame)} != {string.Join(", ", frame)}";
            Debug.WriteLine(msg);
            throw new IOException(msg);
        }

        return response;
    }

    /// <summary>Executes the Checksums Match operation.</summary>
    /// <param name="message">The message value.</param>
    /// <param name="messageFrame">The message Frame value.</param>
    /// <returns>The result.</returns>
    internal abstract bool ChecksumsMatch(IModbusMessage message, byte[] messageFrame);

    internal override void OnValidateResponse(IModbusMessage request, IModbusMessage response)
    {
        // no-op
    }
}
