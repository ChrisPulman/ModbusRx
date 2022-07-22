// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Message;

namespace ModbusRx.IO;

/// <summary>
/// EmptyTransport.
/// </summary>
/// <seealso cref="ModbusRx.IO.ModbusTransport" />
public class EmptyTransport : ModbusTransport
{
    internal override Task<byte[]> ReadRequest() =>
        throw new NotImplementedException();

    internal override Task<IModbusMessage> ReadResponse<T>() =>
        throw new NotImplementedException();

    internal override byte[] BuildMessageFrame(Message.IModbusMessage message) =>
        throw new NotImplementedException();

    internal override void Write(IModbusMessage message) =>
        throw new NotImplementedException();

    internal override void OnValidateResponse(IModbusMessage request, IModbusMessage response) =>
        throw new NotImplementedException();
}
