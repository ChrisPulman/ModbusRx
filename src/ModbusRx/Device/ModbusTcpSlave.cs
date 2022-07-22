// <copyright file="ModbusTcpSlave.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using CP.IO.Ports;
#if TIMER
    using System.Timers;
#endif
using ModbusRx.IO;

namespace ModbusRx.Device;

/// <summary>
///     Modbus TCP slave device.
/// </summary>
public class ModbusTcpSlave : ModbusSlave
{
    private const int TimeWaitResponse = 1000;
    private readonly object _serverLock = new();

    private readonly ConcurrentDictionary<string, ModbusMasterTcpConnection> _masters =
        new();

    private TcpListener _server;

#if TIMER
        private Timer _timer;
#endif
    private ModbusTcpSlave(byte unitId, TcpListener tcpListener)
        : base(unitId, new EmptyTransport())
    {
        if (tcpListener == null)
        {
            throw new ArgumentNullException(nameof(tcpListener));
        }

        _server = tcpListener;
    }

#if TIMER
        private ModbusTcpSlave(byte unitId, TcpListener tcpListener, double timeInterval)
            : base(unitId, new EmptyTransport())
        {
            ArgumentNullException.ThrowIfNull(tcpListener);

            _server = tcpListener;
            _timer = new Timer(timeInterval);
            _timer.Elapsed += OnTimer;
            _timer.Enabled = true;
        }
#endif

    /// <summary>
    ///     Gets the Modbus TCP Masters connected to this Modbus TCP Slave.
    /// </summary>
    public ReadOnlyCollection<TcpClientRx> Masters =>
        new(_masters.Values.Select(mc => mc.TcpClient).ToList());

    /// <summary>
    ///     Gets the server.
    /// </summary>
    /// <value>The server.</value>
    /// <remarks>
    ///     This property is not thread safe, it should only be consumed within a lock.
    /// </remarks>
    private TcpListener Server
    {
        get
        {
            if (_server is null)
            {
                throw new ObjectDisposedException("Server");
            }

            return _server;
        }
    }

    /// <summary>
    /// Modbus TCP slave factory method.
    /// </summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="tcpListener">The TCP listener.</param>
    /// <returns>A ModbusTcpSlave.</returns>
    public static ModbusTcpSlave CreateTcp(byte unitId, TcpListener tcpListener) =>
        new(unitId, tcpListener);

#if TIMER
        /// <summary>
        ///     Creates ModbusTcpSlave with timer which polls connected clients every
        ///     <paramref name="pollInterval"/> milliseconds on that they are connected.
        /// </summary>
        public static ModbusTcpSlave CreateTcp(byte unitId, TcpListener tcpListener, double pollInterval)
        {
            return new ModbusTcpSlave(unitId, tcpListener, pollInterval);
        }
#endif

    /// <summary>
    ///     Start slave listening for requests.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ListenAsync()
    {
        Debug.WriteLine("Start Modbus Tcp Server.");

        // TODO: add state {stoped, listening} and check it before starting
        Server.Start();

        while (true)
        {
            var client = await Server.AcceptTcpClientAsync().ConfigureAwait(false);
            var masterConnection = new ModbusMasterTcpConnection(new(client), this);
            masterConnection.ModbusMasterTcpConnectionClosed += OnMasterConnectionClosedHandler;
            _masters.TryAdd(client.Client.RemoteEndPoint!.ToString()!, masterConnection);
        }
    }

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///     unmanaged resources.
    /// </param>
    /// <remarks>Dispose is thread-safe.</remarks>
    protected override void Dispose(bool disposing)
    {
        if (!disposing || _server is null)
        {
            return;
        }

        lock (_serverLock)
        {
            if (_server is not null)
            {
                _server.Stop();
                _server = null!;

#if TIMER
                if (_timer is not null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
#endif

                foreach (var key in _masters.Keys)
                {
                    if (_masters.TryRemove(key, out var connection))
                    {
                        connection.ModbusMasterTcpConnectionClosed -= OnMasterConnectionClosedHandler;
                        connection.Dispose();
                    }
                }
            }
        }
    }

    private static bool IsSocketConnected(Socket socket)
    {
        var poll = socket.Poll(TimeWaitResponse, SelectMode.SelectRead);
        var available = socket.Available == 0;
        return poll && available;
    }

#if TIMER
        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            foreach (var master in _masters.ToList())
            {
                if (IsSocketConnected(master.Value.TcpClient.Client) == false)
                {
                    master.Value.Dispose();
                }
            }
        }
#endif
    private void OnMasterConnectionClosedHandler(object? sender, TcpConnectionEventArgs e)
    {
        if (!_masters.TryRemove(e.EndPoint, out var _))
        {
            var msg = $"EndPoint {e.EndPoint} cannot be removed, it does not exist.";
            throw new ArgumentException(msg);
        }

        Debug.WriteLine($"Removed Master {e.EndPoint}");
    }
}
