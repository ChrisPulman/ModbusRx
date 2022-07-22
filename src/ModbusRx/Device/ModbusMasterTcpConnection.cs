// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CP.IO.Ports;
using ModbusRx.IO;
using ModbusRx.Message;

namespace ModbusRx.Device;

/// <summary>
/// Represents an incoming connection from a Modbus master. Contains the slave's logic to process the connection.
/// </summary>
internal class ModbusMasterTcpConnection : ModbusDevice
{
    private readonly ModbusTcpSlave _slave;

    private readonly byte[] _mbapHeader = new byte[6];
    private byte[]? _messageFrame;

    public ModbusMasterTcpConnection(TcpClientRx client!!, ModbusTcpSlave slave!!)
        : base(new ModbusIpTransport(new TcpClientAdapter(client)))
    {
        TcpClient = client;
        _slave = slave;

        EndPoint = client.Client.RemoteEndPoint!.ToString()!;
        Stream = client.Stream;
        var requestHandlerTask = Task.Run(HandleRequestAsync);
    }

    /// <summary>
    ///     Occurs when a Modbus master TCP connection is closed.
    /// </summary>
    public event EventHandler<TcpConnectionEventArgs>? ModbusMasterTcpConnectionClosed;

    public string EndPoint { get; }

    public Stream Stream { get; }

    public TcpClientRx TcpClient { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Stream.Dispose();
        }

        base.Dispose(disposing);
    }

    private async Task HandleRequestAsync()
    {
        while (true)
        {
            Debug.WriteLine($"Begin reading header from Master at IP: {EndPoint}");
#pragma warning disable CA1835 // Change the 'ReadAsync' method call to use the 'Stream.ReadAsync(Memory<byte>, CancellationToken)' overload.
            var readBytes = await Stream.ReadAsync(_mbapHeader, 0, 6, CancellationToken.None).ConfigureAwait(false);
            if (readBytes == 0)
            {
                Debug.WriteLine($"0 bytes read, Master at {EndPoint} has closed Socket connection.");
                ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
                return;
            }

            var frameLength = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(_mbapHeader, 4));
            Debug.WriteLine($"Master at {EndPoint} sent header: \"{string.Join(", ", _mbapHeader)}\" with {frameLength} bytes in PDU");

            _messageFrame = new byte[frameLength];
            readBytes = await Stream.ReadAsync(_messageFrame, 0, frameLength, CancellationToken.None).ConfigureAwait(false);
            if (readBytes == 0)
            {
                Debug.WriteLine($"0 bytes read, Master at {EndPoint} has closed Socket connection.");
                ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
                return;
            }

            Debug.WriteLine($"Read frame from Master at {EndPoint} completed {readBytes} bytes");
            var frame = _mbapHeader.Concat(_messageFrame).ToArray();
            Debug.WriteLine($"RX from Master at {EndPoint}: {string.Join(", ", frame)}");

            var request = ModbusMessageFactory.CreateModbusRequest(_messageFrame);
            request.TransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 0));

            // perform action and build response
            var response = _slave.ApplyRequest(request);
            response.TransactionId = request.TransactionId;

            // write response
            var responseFrame = Transport?.BuildMessageFrame(response);
            Debug.WriteLine($"TX to Master at {EndPoint}: {string.Join(", ", responseFrame!)}");
            await Stream.WriteAsync(responseFrame!, 0, responseFrame!.Length, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CA1835 // Change the 'ReadAsync' method call to use the 'Stream.ReadAsync(Memory<byte>, CancellationToken)' overload.
        }
    }
}
