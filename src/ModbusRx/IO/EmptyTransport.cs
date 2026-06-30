// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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

/// <summary>Provides EmptyTransport functionality.</summary>
/// <seealso cref="ModbusTransport" />
public class EmptyTransport : ModbusTransport
{
    internal override Task<byte[]> ReadRequest() =>
        Task.FromException<byte[]>(CreateUnsupportedReadException());

    internal override Task<IModbusMessage> ReadResponse<T>() =>
        Task.FromException<IModbusMessage>(CreateUnsupportedReadException());

    internal override byte[] BuildMessageFrame(IModbusMessage message)
    {
        return (message ?? throw new ArgumentNullException(nameof(message))).MessageFrame;
    }

    internal override void Write(IModbusMessage message)
    {
        _ = message ?? throw new ArgumentNullException(nameof(message));
    }

    internal override void OnValidateResponse(IModbusMessage request, IModbusMessage response)
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));
        _ = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>Executes the Create Unsupported Read Exception operation.</summary>
    /// <returns>The result.</returns>
    private static InvalidOperationException CreateUnsupportedReadException() =>
        new("Empty transport does not provide a readable stream.");
}
