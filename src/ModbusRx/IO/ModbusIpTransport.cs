// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Unme.Common;
#else
using ModbusRx.Unme.Common;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.IO;
#else
namespace ModbusRx.IO;
#endif

/// <summary>Transport for Internet protocols. Refined Abstraction - http://en.wikipedia.org/wiki/Bridge_Pattern.</summary>
internal sealed class ModbusIpTransport : ModbusTransport
{
    /// <summary>Stores the transaction Id Lock value.</summary>
    private static readonly Lock _transactionIdLock = new();

    /// <summary>Stores the transaction Id value.</summary>
    private ushort _transactionId;

    /// <summary>Initializes a new instance of the Modbus Ip Transport class.</summary>
    /// <param name="streamResource">The stream Resource value.</param>
    internal ModbusIpTransport(IStreamResource streamResource)
        : base(streamResource) => Debug.Assert(streamResource is not null, "Argument streamResource cannot be null.");

    /// <summary>Executes the Read Request Response operation.</summary>
    /// <param name="streamResource">The stream Resource value.</param>
    /// <returns>The result.</returns>
    internal static async Task<byte[]> ReadRequestResponse(IStreamResource streamResource)
    {
        // read header
        var mbapHeader = new byte[6];
        var numBytesRead = 0;

        while (numBytesRead != 6)
        {
            var bytesRead = await streamResource.ReadAsync(mbapHeader, numBytesRead, 6 - numBytesRead);

            if (bytesRead == 0)
            {
                throw new IOException("Read resulted in 0 bytes returned.");
            }

            numBytesRead += bytesRead;
        }

        Debug.WriteLine($"MBAP header: {string.Join(", ", mbapHeader)}");
        var frameLength = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(mbapHeader, 4));
        Debug.WriteLine($"{frameLength} bytes in PDU.");

        // read message
        var messageFrame = new byte[frameLength];
        numBytesRead = 0;

        while (numBytesRead != frameLength)
        {
            var bytesRead = await streamResource.ReadAsync(messageFrame, numBytesRead, frameLength - numBytesRead);

            if (bytesRead == 0)
            {
                throw new IOException("Read resulted in 0 bytes returned.");
            }

            numBytesRead += bytesRead;
        }

        Debug.WriteLine($"PDU: {frameLength}");
        var frame = new byte[mbapHeader.Length + messageFrame.Length];
        Array.Copy(mbapHeader, 0, frame, 0, mbapHeader.Length);
        Array.Copy(messageFrame, 0, frame, mbapHeader.Length, messageFrame.Length);
        Debug.WriteLine($"RX: {string.Join(", ", frame)}");

        return frame;
    }

    /// <summary>Executes the Get Mbap Header operation.</summary>
    /// <param name="message">The message value.</param>
    /// <returns>The result.</returns>
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

    /// <summary>Create a new transaction ID.</summary>
    /// <returns>The result.</returns>
    internal ushort GetNewTransactionId()
    {
        lock (_transactionIdLock)
        {
            _transactionId = _transactionId == ushort.MaxValue ? (ushort)1 : ++_transactionId;
        }

        return _transactionId;
    }

    /// <summary>Executes the Create Message And Initialize Transaction Id Async operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="fullFrame">The full Frame value.</param>
    /// <returns>The result.</returns>
    internal async Task<IModbusMessage> CreateMessageAndInitializeTransactionIdAsync<T>(Task<byte[]> fullFrame)
        where T : IModbusMessage, new()
    {
        var lfullframe = await fullFrame;
        var mbapHeader = new byte[6];
        Array.Copy(lfullframe, 0, mbapHeader, 0, mbapHeader.Length);

        var message = new byte[lfullframe.Length - mbapHeader.Length];
        Array.Copy(lfullframe, mbapHeader.Length, message, 0, message.Length);
        var messageFrame = Task.FromResult(message);

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

    internal override Task<IModbusMessage> ReadResponse<T>() =>
       CreateMessageAndInitializeTransactionIdAsync<T>(ReadRequestResponse(StreamResource));

    internal override void OnValidateResponse(IModbusMessage request, IModbusMessage response)
    {
        if (request.TransactionId == response.TransactionId)
        {
            return;
        }

        var msg = $"Response was not of expected transaction ID. Expected {request.TransactionId}, received {response.TransactionId}.";
        throw new IOException(msg);
    }

    internal override bool OnShouldRetryResponse(IModbusMessage request, IModbusMessage response)
    {
        return request.TransactionId > response.TransactionId && request.TransactionId - response.TransactionId < RetryOnOldResponseThreshold ? true : base.OnShouldRetryResponse(request, response);
    }
}
