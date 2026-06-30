// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using CP.IO.Ports;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
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

/// <summary>
/// A reactive Modbus server that can serve multiple clients via TCP/UDP.
/// Supports unified client aggregation and simulation modes.
/// </summary>
public sealed class ModbusServer : IDisposable
{
    /// <summary>Stores the clients value.</summary>
    private readonly ConcurrentDictionary<string, IModbusMaster> _clients = new();

    /// <summary>Stores the tcp Slaves value.</summary>
    private readonly ConcurrentDictionary<string, ModbusTcpSlave> _tcpSlaves = new();

    /// <summary>Stores the udp Slaves value.</summary>
    private readonly ConcurrentDictionary<string, ModbusUdpSlave> _udpSlaves = new();

    /// <summary>Stores the is Running value.</summary>
    private readonly BehaviorSignal<bool> _isRunning = new(false);

    /// <summary>Stores the disposables value.</summary>
    private readonly CompositeDisposable _disposables = [];

    /// <summary>Stores the random Number Generator value.</summary>
    private readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

    /// <summary>Stores the lock value.</summary>
    private readonly Lock _lock = new();

    /// <summary>Stores the simulation Mode value.</summary>
    private bool _simulationMode;

    /// <summary>Stores the simulation Timer value.</summary>
    private IDisposable? _simulationTimer;

    /// <summary>Initializes a new instance of the <see cref="ModbusServer"/> class.</summary>
    public ModbusServer()
    {
        DataStore = DataStoreFactory.CreateDefaultDataStore();
        _disposables.Add(_isRunning);
    }

    /// <summary>Gets an observable that indicates if the server is running.</summary>
    public IObservable<bool> IsRunning => _isRunning.AsObservable();

    /// <summary>Gets or sets the data store for the server.</summary>
    public DataStore? DataStore { get; set; }

    /// <summary>Gets or sets a value indicating whether simulation mode is enabled.</summary>
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

    /// <summary>Adds a Modbus TCP/IP client to serve data from.</summary>
    /// <param name="name">The name identifier for the client.</param>
    /// <param name="hostAddress">The host address of the client.</param>
    /// <param name="port">The port number.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable AddTcpClient(string name, string hostAddress, int port = 502, byte slaveAddress = 1)
    {
        ThrowIfNullOrWhiteSpace(name, nameof(name));

        var client = new TcpClientRx(hostAddress, port);
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
            _ = _clients.TryRemove(name, out _);
            master.Dispose();
        });
    }

    /// <summary>Adds a Modbus UDP client to serve data from.</summary>
    /// <param name="name">The name identifier for the client.</param>
    /// <param name="hostAddress">The host address of the client.</param>
    /// <param name="port">The port number.</param>
    /// <param name="slaveAddress">The slave address.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable AddUdpClient(string name, string hostAddress, int port = 502, byte slaveAddress = 1)
    {
        ThrowIfNullOrWhiteSpace(name, nameof(name));

        var client = new UdpClientRx();
        var endPoint = new IPEndPoint(IPAddress.Parse(hostAddress), port);
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
            _ = _clients.TryRemove(name, out _);
            master.Dispose();
        });
    }

    /// <summary>Starts a TCP server on the specified port.</summary>
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
            _ = _tcpSlaves.TryRemove(serverKey, out _);
            slave.Dispose();
        });
    }

    /// <summary>Starts a UDP server on the specified port.</summary>
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
            _ = _udpSlaves.TryRemove(serverKey, out _);
            slave.Dispose();
        });
    }

    /// <summary>Starts the server with all configured endpoints.</summary>
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

    /// <summary>Stops the server and all endpoints.</summary>
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

    /// <summary>Loads simulation data from specified values for testing.</summary>
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
        var dataStore = DataStore;
        if (dataStore is null)
        {
            return;
        }

        dataStore.Lock.EnterWriteLock();
        try
        {
            if (holdingRegisters is not null)
            {
                for (var i = 0; i < Math.Min(holdingRegisters.Length, dataStore.HoldingRegisters.Count - 1); i++)
                {
                    dataStore.HoldingRegisters[i + 1] = holdingRegisters[i]; // Modbus collections are 1-based
                }
            }

            if (inputRegisters is not null)
            {
                for (var i = 0; i < Math.Min(inputRegisters.Length, dataStore.InputRegisters.Count - 1); i++)
                {
                    dataStore.InputRegisters[i + 1] = inputRegisters[i]; // Modbus collections are 1-based
                }
            }

            if (coils is not null)
            {
                for (var i = 0; i < Math.Min(coils.Length, dataStore.CoilDiscretes.Count - 1); i++)
                {
                    dataStore.CoilDiscretes[i + 1] = coils[i]; // Modbus collections are 1-based
                }
            }

            if (inputs is not null)
            {
                for (var i = 0; i < Math.Min(inputs.Length, dataStore.InputDiscretes.Count - 1); i++)
                {
                    dataStore.InputDiscretes[i + 1] = inputs[i]; // Modbus collections are 1-based
                }
            }
        }
        finally
        {
            dataStore.Lock.ExitWriteLock();
        }
    }

    /// <summary>Gets the current data from the server's data store.</summary>
    /// <returns>A snapshot of the current data.</returns>
    public (ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs) GetCurrentData()
    {
        var dataStore = DataStore;
        if (dataStore is null)
        {
            return ([], [], [], []);
        }

        dataStore.Lock.EnterReadLock();
        try
        {
            // Skip index 0 since Modbus collections are 1-based
            return (
                CopyFromOneBased(dataStore.HoldingRegisters),
                CopyFromOneBased(dataStore.InputRegisters),
                CopyFromOneBased(dataStore.CoilDiscretes),
                CopyFromOneBased(dataStore.InputDiscretes));
        }
        finally
        {
            dataStore.Lock.ExitReadLock();
        }

        static T[] CopyFromOneBased<T>(IList<T> values)
        {
            var result = new T[Math.Max(0, values.Count - 1)];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = values[i + 1];
            }

            return result;
        }
    }

    /// <summary>Disposes the server and all resources.</summary>
    public void Dispose()
    {
        Stop();
        _disposables.Dispose();
        _isRunning.Dispose();
        _randomNumberGenerator.Dispose();
    }

    /// <summary>Executes the Throw If Null Or White Space operation.</summary>
    /// <param name="value">The value.</param>
    /// <param name="parameterName">The parameter name.</param>
    private static void ThrowIfNullOrWhiteSpace(string value, string parameterName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        throw new ArgumentException("The value cannot be null, empty, or whitespace.", parameterName);
    }

    /// <summary>Executes the Update Data From Client operation.</summary>
    /// <param name="master">The master value.</param>
    /// <param name="slaveAddress">The slave Address value.</param>
    /// <returns>The result.</returns>
    private async Task UpdateDataFromClient(ModbusIpMaster master, byte slaveAddress)
    {
        var dataStore = DataStore;
        if (dataStore is null)
        {
            return;
        }

        try
        {
            // Read holding registers
            var holdingRegs = await master.ReadHoldingRegistersAsync(slaveAddress, 0, 100);
            var inputRegs = await master.ReadInputRegistersAsync(slaveAddress, 0, 100);
            var coils = await master.ReadCoilsAsync(slaveAddress, 0, 100);
            var inputs = await master.ReadInputsAsync(slaveAddress, 0, 100);

            dataStore.Lock.EnterWriteLock();
            try
            {
                for (var i = 0; i < Math.Min(holdingRegs.Length, dataStore.HoldingRegisters.Count - 1); i++)
                {
                    dataStore.HoldingRegisters[i + 1] = holdingRegs[i]; // Modbus collections are 1-based
                }

                for (var i = 0; i < Math.Min(inputRegs.Length, dataStore.InputRegisters.Count - 1); i++)
                {
                    dataStore.InputRegisters[i + 1] = inputRegs[i]; // Modbus collections are 1-based
                }

                for (var i = 0; i < Math.Min(coils.Length, dataStore.CoilDiscretes.Count - 1); i++)
                {
                    dataStore.CoilDiscretes[i + 1] = coils[i]; // Modbus collections are 1-based
                }

                for (var i = 0; i < Math.Min(inputs.Length, dataStore.InputDiscretes.Count - 1); i++)
                {
                    dataStore.InputDiscretes[i + 1] = inputs[i]; // Modbus collections are 1-based
                }
            }
            finally
            {
                dataStore.Lock.ExitWriteLock();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating data from client: {ex.Message}");
        }
    }

    /// <summary>Executes the Start Simulation operation.</summary>
    private void StartSimulation()
    {
        if (_simulationTimer is not null || DataStore is null)
        {
            return;
        }

        _simulationTimer = Observable.Interval(TimeSpan.FromMilliseconds(500))
            .Where(_ => _isRunning.Value && _simulationMode)
            .Subscribe(_ => UpdateSimulationData());

        _disposables.Add(_simulationTimer);
    }

    /// <summary>Executes the Stop Simulation operation.</summary>
    private void StopSimulation()
    {
        _simulationTimer?.Dispose();
        _simulationTimer = null;
    }

    /// <summary>Executes the Update Simulation Data operation.</summary>
    private void UpdateSimulationData()
    {
        var dataStore = DataStore;
        if (dataStore is null)
        {
            return;
        }

        dataStore.Lock.EnterWriteLock();
        try
        {
            // Simulate changing values - use 1-based indexing for Modbus collections
            for (var i = 1; i < Math.Min(101, dataStore.HoldingRegisters.Count); i++)
            {
                dataStore.HoldingRegisters[i] = (ushort)GetRandomInt32(65_536);
            }

            for (var i = 1; i < Math.Min(101, dataStore.InputRegisters.Count); i++)
            {
                dataStore.InputRegisters[i] = (ushort)GetRandomInt32(65_536);
            }

            for (var i = 1; i < Math.Min(101, dataStore.CoilDiscretes.Count); i++)
            {
                dataStore.CoilDiscretes[i] = GetRandomBoolean();
            }

            for (var i = 1; i < Math.Min(101, dataStore.InputDiscretes.Count); i++)
            {
                dataStore.InputDiscretes[i] = GetRandomBoolean();
            }
        }
        finally
        {
            dataStore.Lock.ExitWriteLock();
        }
    }

    /// <summary>Executes the Get Random Boolean operation.</summary>
    /// <returns>The result.</returns>
    private bool GetRandomBoolean() => GetRandomInt32(2) == 1;

    /// <summary>Executes the Get Random Int32 operation.</summary>
    /// <param name="maxExclusive">The max Exclusive value.</param>
    /// <returns>The result.</returns>
    private int GetRandomInt32(int maxExclusive) => GetRandomInt32(0, maxExclusive);

    /// <summary>Executes the Get Random Int32 operation.</summary>
    /// <param name="minInclusive">The min Inclusive value.</param>
    /// <param name="maxExclusive">The max Exclusive value.</param>
    /// <returns>The result.</returns>
    private int GetRandomInt32(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            return minInclusive;
        }

        var range = (uint)(maxExclusive - minInclusive);
        var limit = uint.MaxValue - (uint.MaxValue % range);
        var bytes = new byte[sizeof(uint)];
        uint value;

        do
        {
            _randomNumberGenerator.GetBytes(bytes);
            value = BitConverter.ToUInt32(bytes, 0);
        }
        while (value >= limit);

        return minInclusive + (int)(value % range);
    }
}
