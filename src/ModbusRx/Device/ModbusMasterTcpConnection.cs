// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using CP.IO.Ports;
#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Represents an incoming connection from a Modbus master. Contains the slave's logic to process the connection.</summary>
internal sealed class ModbusMasterTcpConnection : ModbusDevice
{
    /// <summary>Stores the slave value.</summary>
    private readonly ModbusTcpSlave _slave;

    /// <summary>Stores the mbap Header value.</summary>
    private readonly byte[] _mbapHeader = new byte[6];

    /// <summary>Stores the message Frame value.</summary>
    private byte[]? _messageFrame;

    /// <summary>Initializes a new instance of the Modbus Master Tcp Connection class.</summary>
    /// <param name="client">The client value.</param>
    /// <param name="slave">The slave value.</param>
    public ModbusMasterTcpConnection(TcpClientRx client, ModbusTcpSlave slave)
        : base(new ModbusIpTransport(new TcpClientAdapter(client)))
    {
        TcpClient = client;
        _slave = slave;

        EndPoint = client.Client.RemoteEndPoint!.ToString()!;
        Stream = client.Stream;
        var requestHandlerTask = Task.Run(HandleRequestAsync);
    }

    /// <summary>Occurs when a Modbus master TCP connection is closed.</summary>
    public event EventHandler<TcpConnectionEventArgs>? ModbusMasterTcpConnectionClosed;

    /// <summary>Gets or sets the End Point value.</summary>
    public string EndPoint { get; }

    /// <summary>Gets or sets the Stream value.</summary>
    public Stream Stream { get; }

    /// <summary>Gets or sets the Tcp Client value.</summary>
    public TcpClientRx TcpClient { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stream.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Executes the Handle Request Async operation.</summary>
    /// <returns>The result.</returns>
    private async Task HandleRequestAsync()
    {
        while (true)
        {
            Debug.WriteLine($"Begin reading header from Master at IP: {EndPoint}");
#if NET8_0_OR_GREATER
            var readBytes = await Stream.ReadAsync(_mbapHeader.AsMemory(0, 6), CancellationToken.None).ConfigureAwait(false);
#else
            var readBytes = await Stream.ReadAsync(_mbapHeader, 0, 6, CancellationToken.None).ConfigureAwait(false);
#endif
            if (readBytes == 0)
            {
                Debug.WriteLine($"0 bytes read, Master at {EndPoint} has closed Socket connection.");
                ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
                return;
            }

            var frameLength = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(_mbapHeader, 4));
            Debug.WriteLine($"Master at {EndPoint} sent header: \"{string.Join(", ", _mbapHeader)}\" with {frameLength} bytes in PDU");

            _messageFrame = new byte[frameLength];
#if NET8_0_OR_GREATER
            readBytes = await Stream.ReadAsync(_messageFrame.AsMemory(0, frameLength), CancellationToken.None).ConfigureAwait(false);
#else
            readBytes = await Stream.ReadAsync(_messageFrame, 0, frameLength, CancellationToken.None).ConfigureAwait(false);
#endif
            if (readBytes == 0)
            {
                Debug.WriteLine($"0 bytes read, Master at {EndPoint} has closed Socket connection.");
                ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
                return;
            }

            Debug.WriteLine($"Read frame from Master at {EndPoint} completed {readBytes} bytes");
            var frame = new byte[_mbapHeader.Length + _messageFrame.Length];
            Array.Copy(_mbapHeader, 0, frame, 0, _mbapHeader.Length);
            Array.Copy(_messageFrame, 0, frame, _mbapHeader.Length, _messageFrame.Length);
            Debug.WriteLine($"RX from Master at {EndPoint}: {string.Join(", ", frame)}");

            var request = ModbusMessageFactory.CreateModbusRequest(_messageFrame);
            request.TransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 0));

            // perform action and build response
            var response = _slave.ApplyRequest(request);
            response.TransactionId = request.TransactionId;

            // write response
            var responseFrame = Transport?.BuildMessageFrame(response);
            Debug.WriteLine($"TX to Master at {EndPoint}: {string.Join(", ", responseFrame!)}");
#if NET8_0_OR_GREATER
            await Stream.WriteAsync(responseFrame.AsMemory(0, responseFrame!.Length), CancellationToken.None).ConfigureAwait(false);
#else
            await Stream.WriteAsync(responseFrame!, 0, responseFrame!.Length, CancellationToken.None).ConfigureAwait(false);
#endif
        }
    }
}
