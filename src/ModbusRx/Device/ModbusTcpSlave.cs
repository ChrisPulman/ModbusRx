// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Sockets;
using CP.IO.Ports;
#if TIMER
    using System.Timers;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Modbus TCP slave device.</summary>
public sealed class ModbusTcpSlave : ModbusSlave
{
    /// <summary>Stores the server Lock value.</summary>
    private readonly Lock _serverLock = new();

    /// <summary>Stores the masters value.</summary>
    private readonly ConcurrentDictionary<string, ModbusMasterTcpConnection> _masters =
        new();

    /// <summary>Stores the server value.</summary>
    private TcpListener? _server;

#if TIMER
        private Timer _timer;
#endif
    /// <summary>Initializes a new instance of the Modbus Tcp Slave class.</summary>
    /// <param name="unitId">The unit Id value.</param>
    /// <param name="tcpListener">The tcp Listener value.</param>
    private ModbusTcpSlave(byte unitId, TcpListener tcpListener)
        : base(unitId, new EmptyTransport())
    {
        if (tcpListener is null)
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

    /// <summary>Gets the Modbus TCP Masters connected to this Modbus TCP Slave.</summary>
    public ReadOnlyCollection<TcpClientRx> Masters
    {
        get
        {
            var masters = new List<TcpClientRx>(_masters.Count);
            foreach (var masterConnection in _masters.Values)
            {
                masters.Add(masterConnection.TcpClient);
            }

            return new(masters);
        }
    }

    /// <summary>Gets the server.</summary>
    /// <value>The server.</value>
    /// <remarks>This property is not thread safe, it should only be consumed within a lock.</remarks>
    private TcpListener Server =>
        _server ?? throw new ObjectDisposedException(nameof(ModbusTcpSlave));

    /// <summary>Modbus TCP slave factory method.</summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="tcpListener">The TCP listener.</param>
    /// <returns>A ModbusTcpSlave.</returns>
    public static ModbusTcpSlave CreateTcp(byte unitId, TcpListener tcpListener) =>
        new(unitId, tcpListener);

#if TIMER
/// <summary>
/// Creates ModbusTcpSlave with timer which polls connected clients every
/// <paramref name="pollInterval"/> milliseconds on that they are connected.
/// </summary>
        public static ModbusTcpSlave CreateTcp(byte unitId, TcpListener tcpListener, double pollInterval)
        {
            return new ModbusTcpSlave(unitId, tcpListener, pollInterval);
        }
#endif

    /// <summary>Start slave listening for requests.</summary>
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
            _ = _masters.TryAdd(client.Client.RemoteEndPoint!.ToString()!, masterConnection);
        }
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    /// <remarks>Dispose is thread-safe.</remarks>
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///     unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                lock (_serverLock)
                {
                    var server = _server;
                    if (server is not null)
                    {
                        server.Stop();
                        _server = null;

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
        }
        finally
        {
            base.Dispose(disposing);
        }
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
    /// <summary>Executes the On Master Connection Closed Handler operation.</summary>
    /// <param name="sender">The sender value.</param>
    /// <param name="e">The e value.</param>
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
