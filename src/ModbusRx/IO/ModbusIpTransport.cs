// <copyright file="ModbusIpTransport.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Diagnostics;
using System.Net;
using ModbusRx.Message;
using ModbusRx.Unme.Common;

namespace ModbusRx.IO;

/// <summary>
///     Transport for Internet protocols.
///     Refined Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.
/// </summary>
internal class ModbusIpTransport : ModbusTransport
{
    private static readonly object _transactionIdLock = new();
    private ushort _transactionId;

    internal ModbusIpTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    internal static async Task<byte[]> ReadRequestResponse(IStreamResource streamResource)
    {
        // read header
        var mbapHeader = new byte[6];
        var numBytesRead = 0;

        while (numBytesRead != 6)
        {
            var bRead = await streamResource.ReadAsync(mbapHeader, numBytesRead, 6 - numBytesRead);

            if (bRead == 0)
            {
                throw new IOException("Read resulted in 0 bytes returned.");
            }

            numBytesRead += bRead;
        }

        Debug.WriteLine($"MBAP header: {string.Join(", ", mbapHeader)}");
        var frameLength = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(mbapHeader, 4));
        Debug.WriteLine($"{frameLength} bytes in PDU.");

        // read message
        var messageFrame = new byte[frameLength];
        numBytesRead = 0;

        while (numBytesRead != frameLength)
        {
            var bRead = await streamResource.ReadAsync(messageFrame, numBytesRead, frameLength - numBytesRead);

            if (bRead == 0)
            {
                throw new IOException("Read resulted in 0 bytes returned.");
            }

            numBytesRead += bRead;
        }

        Debug.WriteLine($"PDU: {frameLength}");
        var frame = mbapHeader.Concat(messageFrame).ToArray();
        Debug.WriteLine($"RX: {string.Join(", ", frame)}");

        return frame;
    }

    internal static byte[] GetMbapHeader(IModbusMessage message)
    {
        var transactionId = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)message.TransactionId));
        var length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(message.ProtocolDataUnit.Length + 1)));

        var stream = new MemoryStream(7);
        stream.Write(transactionId, 0, transactionId.Length);
        stream.WriteByte(0);
        stream.WriteByte(0);
        stream.Write(length, 0, length.Length);
        stream.WriteByte(message.SlaveAddress);

        return stream.ToArray();
    }

    /// <summary>
    ///     Create a new transaction ID.
    /// </summary>
    internal virtual ushort GetNewTransactionId()
    {
        lock (_transactionIdLock)
        {
            _transactionId = _transactionId == ushort.MaxValue ? (ushort)1 : ++_transactionId;
        }

        return _transactionId;
    }

    internal async Task<IModbusMessage> CreateMessageAndInitializeTransactionIdAsync<T>(Task<byte[]> fullFrame)
        where T : IModbusMessage, new()
    {
        var lfullframe = await fullFrame;
        var mbapHeader = lfullframe.Slice(0, 6).ToArray();
        var messageFrame = Task.FromResult(lfullframe.Slice(6, lfullframe.Length - 6).ToArray());

        var response = await CreateResponse<T>(messageFrame);
        response.TransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(mbapHeader, 0));

        return response;
    }

    internal override byte[] BuildMessageFrame(IModbusMessage message)
    {
        var header = GetMbapHeader(message);
        var pdu = message.ProtocolDataUnit;
        var messageBody = new MemoryStream(header.Length + pdu.Length);

        messageBody.Write(header, 0, header.Length);
        messageBody.Write(pdu, 0, pdu.Length);

        return messageBody.ToArray();
    }

    internal override void Write(IModbusMessage message)
    {
        message.TransactionId = GetNewTransactionId();
        var frame = BuildMessageFrame(message);
        Debug.WriteLine($"TX: {string.Join(", ", frame)}");
        StreamResource.Write(frame, 0, frame.Length);
    }

    internal override Task<byte[]> ReadRequest() =>
        ReadRequestResponse(StreamResource);

    internal override async Task<IModbusMessage> ReadResponse<T>() =>
       await CreateMessageAndInitializeTransactionIdAsync<T>(ReadRequestResponse(StreamResource));

    internal override void OnValidateResponse(IModbusMessage request, IModbusMessage response)
    {
        if (request.TransactionId != response.TransactionId)
        {
            var msg = $"Response was not of expected transaction ID. Expected {request.TransactionId}, received {response.TransactionId}.";
            throw new IOException(msg);
        }
    }

    internal override bool OnShouldRetryResponse(IModbusMessage request, IModbusMessage response)
    {
        if (request.TransactionId > response.TransactionId && request.TransactionId - response.TransactionId < RetryOnOldResponseThreshold)
        {
            // This response was from a previous request
            return true;
        }

        return base.OnShouldRetryResponse(request, response);
    }
}
