// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.IO;

namespace ModbusRx.Device;

/// <summary>
/// A reactive Modbus server that can serve multiple clients via TCP/UDP.
/// Supports unified client aggregation and simulation modes.
/// </summary>
public sealed class ModbusServer : IDisposable
{
    private readonly ConcurrentDictionary<string, IModbusMaster> _clients = new();
    private readonly ConcurrentDictionary<string, ModbusTcpSlave> _tcpSlaves = new();
    private readonly ConcurrentDictionary<string, ModbusUdpSlave> _udpSlaves = new();
    private readonly BehaviorSubject<bool> _isRunning = new(false);
    private readonly CompositeDisposable _disposables = [];
    private readonly Random _random = new();
    private readonly object _lock = new();

    private DataStore? _dataStore;
    private bool _simulationMode;
    private IDisposable? _simulationTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusServer"/> class.
    /// </summary>
    public ModbusServer()
    {
        DataStore = DataStoreFactory.CreateDefaultDataStore();
        _disposables.Add(_isRunning);
    }

    /// <summary>
    /// Gets an observable that indicates if the server is running.
    /// </summary>
    public IObservable<bool> IsRunning => _isRunning.AsObservable();

    /// <summary>
    /// Gets or sets the data store for the server.
    /// </summary>
    public DataStore? DataStore { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether simulation mode is enabled.
    /// </summary>
    public bool SimulationMode
    {
        get => _simulationMode;
        set
        {
            _simulationMode = value;
            if (value)
            {
                StartSimulation();
            }
            else
            {
                StopSimulation();
            }
        }
    }

    /// <summary>
    /// Adds a Modbus TCP/IP client to serve data from.
    /// </summary>
    /// <param name="name">The name identifier for the client.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <param name="port">The port number.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable AddTcpClient(string name, string ipAddress, int port = 502, byte slaveAddress = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
        }

        var client = new TcpClientRx(ipAddress, port);
        var master = ModbusIpMaster.CreateIp(client);
        _clients[name] = master;

        // Start periodic reading from this client
        var subscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
            .Where(_ => _isRunning.Value)
            .Subscribe(async _ =>
            {
                try
                {
                    await UpdateDataFromClient(master, slaveAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading from client {name}: {ex.Message}");
                }
            });

        _disposables.Add(subscription);
        _disposables.Add(master);

        return Disposable.Create(() =>
        {
            _clients.TryRemove(name, out _);
            master.Dispose();
        });
    }

    /// <summary>
    /// Adds a Modbus UDP client to serve data from.
    /// </summary>
    /// <param name="name">The name identifier for the client.</param>
    /// <param name="ipAddress">The IP address of the client.</param>
    /// <param name="port">The port number.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable AddUdpClient(string name, string ipAddress, int port = 502, byte slaveAddress = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
        }

        var client = new UdpClientRx();
        var endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        client.Connect(endPoint);
        var master = ModbusIpMaster.CreateIp(client);
        _clients[name] = master;

        // Start periodic reading from this client
        var subscription = Observable.Interval(TimeSpan.FromMilliseconds(100))
            .Where(_ => _isRunning.Value)
            .Subscribe(async _ =>
            {
                try
                {
                    await UpdateDataFromClient(master, slaveAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading from UDP client {name}: {ex.Message}");
                }
            });

        _disposables.Add(subscription);
        _disposables.Add(master);

        return Disposable.Create(() =>
        {
            _clients.TryRemove(name, out _);
            master.Dispose();
        });
    }

    /// <summary>
    /// Starts a TCP server on the specified port.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    /// <param name="unitId">The unit ID for the slave.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable StartTcpServer(int port = 502, byte unitId = 1)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        var slave = ModbusTcpSlave.CreateTcp(unitId, listener);
        slave.DataStore = DataStore ?? DataStoreFactory.CreateDefaultDataStore();

        var serverKey = $"tcp_{port}_{unitId}";
        _tcpSlaves[serverKey] = slave;

        var task = Task.Run(async () =>
        {
            try
            {
                await slave.ListenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP Server error on port {port}: {ex.Message}");
            }
        });

        _disposables.Add(slave);

        return Disposable.Create(() =>
        {
            _tcpSlaves.TryRemove(serverKey, out _);
            slave.Dispose();
        });
    }

    /// <summary>
    /// Starts a UDP server on the specified port.
    /// </summary>
    /// <param name="port">The port to listen on.</param>
    /// <param name="unitId">The unit ID for the slave.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable StartUdpServer(int port = 502, byte unitId = 1)
    {
        var client = new UdpClientRx(port);
        var slave = ModbusUdpSlave.CreateUdp(unitId, client);
        slave.DataStore = DataStore ?? DataStoreFactory.CreateDefaultDataStore();

        var serverKey = $"udp_{port}_{unitId}";
        _udpSlaves[serverKey] = slave;

        var task = Task.Run(async () =>
        {
            try
            {
                await slave.ListenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Server error on port {port}: {ex.Message}");
            }
        });

        _disposables.Add(slave);

        return Disposable.Create(() =>
        {
            _udpSlaves.TryRemove(serverKey, out _);
            slave.Dispose();
        });
    }

    /// <summary>
    /// Starts the server with all configured endpoints.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning.Value)
            {
                return;
            }

            _isRunning.OnNext(true);

            if (_simulationMode)
            {
                StartSimulation();
            }
        }
    }

    /// <summary>
    /// Stops the server and all endpoints.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning.Value)
            {
                return;
            }

            _isRunning.OnNext(false);
            StopSimulation();
        }
    }

    /// <summary>
    /// Loads simulation data from specified values for testing.
    /// </summary>
    /// <param name="holdingRegisters">Holding register values.</param>
    /// <param name="inputRegisters">Input register values.</param>
    /// <param name="coils">Coil values.</param>
    /// <param name="inputs">Input values.</param>
    public void LoadSimulationData(
        ushort[]? holdingRegisters = null,
        ushort[]? inputRegisters = null,
        bool[]? coils = null,
        bool[]? inputs = null)
    {
        if (DataStore == null)
        {
            return;
        }

        lock (DataStore.SyncRoot)
        {
            if (holdingRegisters != null)
            {
                for (var i = 0; i < Math.Min(holdingRegisters.Length, DataStore.HoldingRegisters.Count - 1); i++)
                {
                    DataStore.HoldingRegisters[i + 1] = holdingRegisters[i]; // Modbus collections are 1-based
                }
            }

            if (inputRegisters != null)
            {
                for (var i = 0; i < Math.Min(inputRegisters.Length, DataStore.InputRegisters.Count - 1); i++)
                {
                    DataStore.InputRegisters[i + 1] = inputRegisters[i]; // Modbus collections are 1-based
                }
            }

            if (coils != null)
            {
                for (var i = 0; i < Math.Min(coils.Length, DataStore.CoilDiscretes.Count - 1); i++)
                {
                    DataStore.CoilDiscretes[i + 1] = coils[i]; // Modbus collections are 1-based
                }
            }

            if (inputs != null)
            {
                for (var i = 0; i < Math.Min(inputs.Length, DataStore.InputDiscretes.Count - 1); i++)
                {
                    DataStore.InputDiscretes[i + 1] = inputs[i]; // Modbus collections are 1-based
                }
            }
        }
    }

    /// <summary>
    /// Gets the current data from the server's data store.
    /// </summary>
    /// <returns>A snapshot of the current data.</returns>
    public (ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs) GetCurrentData()
    {
        if (DataStore == null)
        {
            return ([], [], [], []);
        }

        lock (DataStore.SyncRoot)
        {
            // Skip index 0 since Modbus collections are 1-based
            return (
                DataStore.HoldingRegisters.Skip(1).ToArray(),
                DataStore.InputRegisters.Skip(1).ToArray(),
                DataStore.CoilDiscretes.Skip(1).ToArray(),
                DataStore.InputDiscretes.Skip(1).ToArray());
        }
    }

    /// <summary>
    /// Disposes the server and all resources.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _disposables.Dispose();
        _isRunning.Dispose();
    }

    private async Task UpdateDataFromClient(IModbusMaster master, byte slaveAddress)
    {
        if (DataStore == null)
        {
            return;
        }

        try
        {
            // Read holding registers
            var holdingRegs = await master.ReadHoldingRegistersAsync(slaveAddress, 0, 100);
            lock (DataStore.SyncRoot)
            {
                for (var i = 0; i < Math.Min(holdingRegs.Length, DataStore.HoldingRegisters.Count - 1); i++)
                {
                    DataStore.HoldingRegisters[i + 1] = holdingRegs[i]; // Modbus collections are 1-based
                }
            }

            // Read input registers
            var inputRegs = await master.ReadInputRegistersAsync(slaveAddress, 0, 100);
            lock (DataStore.SyncRoot)
            {
                for (var i = 0; i < Math.Min(inputRegs.Length, DataStore.InputRegisters.Count - 1); i++)
                {
                    DataStore.InputRegisters[i + 1] = inputRegs[i]; // Modbus collections are 1-based
                }
            }

            // Read coils
            var coils = await master.ReadCoilsAsync(slaveAddress, 0, 100);
            lock (DataStore.SyncRoot)
            {
                for (var i = 0; i < Math.Min(coils.Length, DataStore.CoilDiscretes.Count - 1); i++)
                {
                    DataStore.CoilDiscretes[i + 1] = coils[i]; // Modbus collections are 1-based
                }
            }

            // Read inputs
            var inputs = await master.ReadInputsAsync(slaveAddress, 0, 100);
            lock (DataStore.SyncRoot)
            {
                for (var i = 0; i < Math.Min(inputs.Length, DataStore.InputDiscretes.Count - 1); i++)
                {
                    DataStore.InputDiscretes[i + 1] = inputs[i]; // Modbus collections are 1-based
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating data from client: {ex.Message}");
        }
    }

    private void StartSimulation()
    {
        if (_simulationTimer != null || DataStore == null)
        {
            return;
        }

        _simulationTimer = Observable.Interval(TimeSpan.FromMilliseconds(500))
            .Where(_ => _isRunning.Value && _simulationMode)
            .Subscribe(_ => UpdateSimulationData());

        _disposables.Add(_simulationTimer);
    }

    private void StopSimulation()
    {
        _simulationTimer?.Dispose();
        _simulationTimer = null;
    }

    private void UpdateSimulationData()
    {
        if (DataStore == null)
        {
            return;
        }

        lock (DataStore.SyncRoot)
        {
            // Simulate changing values - use 1-based indexing for Modbus collections
            for (var i = 1; i < Math.Min(101, DataStore.HoldingRegisters.Count); i++)
            {
                DataStore.HoldingRegisters[i] = (ushort)_random.Next(0, 65536);
            }

            for (var i = 1; i < Math.Min(101, DataStore.InputRegisters.Count); i++)
            {
                DataStore.InputRegisters[i] = (ushort)_random.Next(0, 65536);
            }

            for (var i = 1; i < Math.Min(101, DataStore.CoilDiscretes.Count); i++)
            {
                DataStore.CoilDiscretes[i] = _random.Next(0, 2) == 1;
            }

            for (var i = 1; i < Math.Min(101, DataStore.InputDiscretes.Count); i++)
            {
                DataStore.InputDiscretes[i] = _random.Next(0, 2) == 1;
            }
        }
    }
}
