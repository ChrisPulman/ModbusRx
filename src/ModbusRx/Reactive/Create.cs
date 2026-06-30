// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CP.IO.Ports;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Device;
#else
using ModbusRx.Device;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Utility;
#else
using ModbusRx.Utility;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive
#else
namespace ModbusRx
#endif
{
    /// <summary>Provides ModbusRx functionality.</summary>
    public static class Create
    {
        /// <summary>Gets or sets the ping interval.</summary>
        /// <value>The ping interval.</value>
        public static TimeSpan PingInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>Gets or sets the check connection interval.</summary>
        /// <value>The check connection interval.</value>
        public static TimeSpan CheckConnectionInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>Create a TcpIpMaster with the specified host address.</summary>
        /// <param name="hostAddress">The host address.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> TcpIpMaster(string hostAddress, int port = 502) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                var pingSender = new Ping();
                dis.Add(pingSender);
                ModbusIpMaster? master = null;
                var connected = false;
                var connectionMessageSent = false;

                dis.Add(Observable.Timer(PingInterval, CheckConnectionInterval).Subscribe(_ =>
                {
                    if (connected && master is null)
                    {
                        observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                        connected = false;
                    }

                    if (connected || connectionMessageSent)
                    {
                        return;
                    }

                    connectionMessageSent = true;
                    observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                }));

                dis.Add(Observable.Timer(CheckConnectionInterval, PingInterval)
                    .Where(_ => !connected)
                    .Select(_ => pingSender.SendPingAsync(hostAddress, 1000))
                    .Select(x =>
                    {
                        var res = default(PingReply);

                        try
                        {
                            res = x?.Result;
                        }
                        finally
                        {
                            try
                            {
                                if (master is null && res?.Status == IPStatus.Success)
                                {
                                    observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                                    master = ModbusIpMaster.CreateIp(new TcpClientRx(hostAddress, port));
                                    dis.Add(master);
                                    connected = true;
                                    connectionMessageSent = false;
                                    observer.OnNext((connected, null, master));
                                }
                            }
                            catch (Exception ex)
                            {
                                master?.Dispose();
                                master = null;
                                connected = false;
                                observer.OnNext((connected, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                            }
                        }

                        return res;
                    }).Retry(int.MaxValue).Subscribe());
                return dis;
            }).Publish().RefCount();

        /// <summary>TCPs the ip slave.</summary>
        /// <param name="hostAddress">The host address.</param>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>An Observable of.</returns>
        /// <exception cref="ArgumentNullException">nameof(hostAddress).</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// nameof(port)
        /// or
        /// nameof(unitId).
        /// </exception>
        public static IObservable<ModbusTcpSlave> TcpIpSlave(string hostAddress, int port = 502, byte unitId = 1)
        {
            if (string.IsNullOrWhiteSpace(hostAddress))
            {
                throw new ArgumentOutOfRangeException(nameof(hostAddress));
            }

            if (port < 0 || port > 65_535)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (unitId < 1 || unitId > 247)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId));
            }

            return Observable.Create<ModbusTcpSlave>(async observer =>
             {
                 var dis = new CompositeDisposable();
                 var address = IPAddress.Parse(hostAddress);
                 var slaveListener = new TcpListener(address, 502);
                 using var slave = ModbusTcpSlave.CreateTcp(1, slaveListener);
                 dis.Add(slave);
                 observer.OnNext(slave);
                 await slave.ListenAsync();

                 return Disposable.Create(() =>
                   {
                       slaveListener.Stop();
                       dis.Dispose();
                   });
             }).Retry(int.MaxValue).Publish().RefCount();
        }

        /// <summary>Create a UdpIpMaster with the specified host address.</summary>
        /// <param name="hostAddress">The host address.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> UdpIpMaster(string hostAddress, int port = 502) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                var pingSender = new Ping();
                dis.Add(pingSender);
                ModbusIpMaster? master = null;
                var connected = false;
                var connectionMessageSent = false;

                dis.Add(Observable.Timer(PingInterval, CheckConnectionInterval).Subscribe(_ =>
                {
                    if (connected && master is null)
                    {
                        observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                        connected = false;
                    }

                    if (connected || connectionMessageSent)
                    {
                        return;
                    }

                    connectionMessageSent = true;
                    observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                }));

                dis.Add(Observable.Timer(CheckConnectionInterval, PingInterval)
                    .Where(_ => !connected)
                    .Select(_ => pingSender.SendPingAsync(hostAddress, 1000))
                    .Select(x =>
                    {
                        var res = default(PingReply);

                        try
                        {
                            res = x?.Result;
                        }
                        finally
                        {
                            try
                            {
                                if (master is null && res?.Status == IPStatus.Success)
                                {
                                    observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                                    master = ModbusIpMaster.CreateIp(new UdpClientRx(hostAddress, port));
                                    dis.Add(master);
                                    connected = true;
                                    connectionMessageSent = false;
                                    observer.OnNext((connected, null, master));
                                }
                            }
                            catch (Exception ex)
                            {
                                master?.Dispose();
                                master = null;
                                connected = false;
                                observer.OnNext((connected, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                            }
                        }

                        return res;
                    }).Retry(int.MaxValue).Subscribe());

                return dis;
            }).Publish().RefCount();

        /// <summary>Creates an UdpIp slave.</summary>
        /// <param name="hostAddress">The host address.</param>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>An Observable of.</returns>
        /// <exception cref="ArgumentNullException">nameof(hostAddress).</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// nameof(port)
        /// or
        /// nameof(unitId).
        /// </exception>
        public static IObservable<ModbusUdpSlave> UdpIpSlave(string hostAddress, int port = 502, byte unitId = 1)
        {
            if (string.IsNullOrWhiteSpace(hostAddress))
            {
                throw new ArgumentOutOfRangeException(nameof(hostAddress));
            }

            if (port < 0 || port > 65_535)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (unitId < 1 || unitId > 247)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId));
            }

            return Observable.Create<ModbusUdpSlave>(async observer =>
             {
                 var dis = new CompositeDisposable();
                 using var slave = ModbusUdpSlave.CreateUdp(unitId, new UdpClientRx(hostAddress, port));
                 await slave.ListenAsync();
                 dis.Add(slave);
                 observer.OnNext(slave);
                 return Disposable.Create(() => dis.Dispose());
             }).Retry(int.MaxValue).Publish().RefCount();
        }

        /// <summary>Create a SerialIpMaster with the specified ip address.</summary>
        /// <param name="port">The COM Port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> SerialIpMaster(string port, int baudRate = 502) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                ModbusIpMaster? master = null;
                var connected = false;
                var connectionMessageSent = false;

                dis.Add(Observable.Interval(CheckConnectionInterval).Subscribe(_ =>
                {
                    if (connected && master is null)
                    {
                        observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                        connected = false;
                    }

                    if (connected || connectionMessageSent)
                    {
                        return;
                    }

                    connectionMessageSent = true;
                    observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                }));

                var comdis = new CompositeDisposable();

                // Subscribe to com ports available
                dis.Add(SerialPortRx.PortNames().Do(async x =>
                {
                    try
                    {
                        if (comdis?.Count == 0 && ContainsPortName(x, port))
                        {
                            observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                            var serialport = new SerialPortRx(port, baudRate);
                            master = ModbusIpMaster.CreateIp(serialport);
                            comdis.Add(master);
                            await serialport.Open();
                            connected = true;
                            connectionMessageSent = false;
                            observer.OnNext((connected, null, master));
                        }
                        else
                        {
                            _ = dis.Remove(comdis!);
                            comdis?.Dispose();
                            connected = false;
                            master = null;
                            observer.OnNext((connected, null, master));
                            comdis = [];
                            dis.Add(comdis);
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = dis.Remove(comdis!);
                        comdis?.Dispose();
                        connected = false;
                        master = null;
                        observer.OnNext((connected, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                        comdis = [];
                        dis.Add(comdis);
                    }
                }).Retry(int.MaxValue).Subscribe());

                return dis;
            }).Publish().RefCount();

        /// <summary>Creates an Serial Rtu Slave.</summary>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>An observable of serial RTU slave instances.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="port"/> or <paramref name="unitId"/> is invalid.</exception>
        public static IObservable<ModbusSerialSlave> SerialRtuSlave(string port, byte unitId = 1, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (unitId < 1 || unitId > 247)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId));
            }

            return Observable.Create<ModbusSerialSlave>(async observer =>
             {
                 var dis = new CompositeDisposable();
                 var comdis = new CompositeDisposable();
                 Task? slaveThread = null;
                 _ = SerialPortRx.PortNames().Do(async x =>
                {
                    try
                    {
                        if (comdis?.Count == 0 && ContainsPortName(x, port))
                        {
                            var serialport = CreateSerialPort(port, baudRate, dataBits, parity, stopBits, handshake);
                            var slave = ModbusSerialSlave.CreateRtu(unitId, serialport);
                            await serialport.Open();
                            slaveThread = new(async () => await slave.ListenAsync(), TaskCreationOptions.LongRunning);
                            slaveThread.Start();
                            dis.Add(slave);
                            observer.OnNext(slave);
                        }
                        else
                        {
                            _ = dis.Remove(comdis!);
                            comdis?.Dispose();
                            slaveThread?.Dispose();
                            comdis = [];
                            dis.Add(comdis);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(new ModbusCommunicationException("ModbusRx Slave Fault", ex));
                        _ = dis.Remove(comdis!);
                        comdis?.Dispose();
                        slaveThread?.Dispose();
                        comdis = [];
                        dis.Add(comdis);
                    }
                }).Retry(int.MaxValue).Subscribe();

                 return Disposable.Create(() =>
                   {
                       slaveThread?.Dispose();
                       dis.Dispose();
                   });
             }).Retry(int.MaxValue).Publish().RefCount();
        }

        /// <summary>Creates an Serial Ascii Slave.</summary>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>An observable of serial ASCII slave instances.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="port"/> or <paramref name="unitId"/> is invalid.</exception>
        public static IObservable<ModbusSerialSlave> SerialAsciiSlave(string port, byte unitId = 1, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (unitId < 1 || unitId > 247)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId));
            }

            return Observable.Create<ModbusSerialSlave>(async observer =>
             {
                 var dis = new CompositeDisposable();
                 var comdis = new CompositeDisposable();
                 Task? slaveThread = null;
                 _ = SerialPortRx.PortNames().Do(async x =>
                {
                    try
                    {
                        if (comdis?.Count == 0 && ContainsPortName(x, port))
                        {
                            var serialport = CreateSerialPort(port, baudRate, dataBits, parity, stopBits, handshake);
                            var slave = ModbusSerialSlave.CreateAscii(unitId, serialport);
                            await serialport.Open();
                            slaveThread = new(async () => await slave.ListenAsync(), TaskCreationOptions.LongRunning);
                            slaveThread.Start();
                            dis.Add(slave);
                            observer.OnNext(slave);
                        }
                        else
                        {
                            _ = dis.Remove(comdis!);
                            comdis?.Dispose();
                            slaveThread?.Dispose();
                            comdis = [];
                            dis.Add(comdis);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(new ModbusCommunicationException("ModbusRx Slave Fault", ex));
                        _ = dis.Remove(comdis!);
                        comdis?.Dispose();
                        slaveThread?.Dispose();
                        comdis = [];
                        dis.Add(comdis);
                    }
                }).Retry(int.MaxValue).Subscribe();

                 return Disposable.Create(() =>
                   {
                       slaveThread?.Dispose();
                       dis.Dispose();
                   });
             }).Retry(int.MaxValue).Publish().RefCount();
        }

        /// <summary>Create a reactive Modbus Serial RTU master that automatically manages connection state.</summary>
        /// <param name="port">The COM port (e.g., "COM1").</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>An observable stream of connection status and the RTU master.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when port is null or whitespace.</exception>
        public static IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> SerialRtuMaster(
            string port,
            int baudRate = 9600,
            int dataBits = 8,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None) =>
            Observable.Create<(bool connected, Exception? error, IModbusSerialMaster? master)>(async observer =>
            {
                if (string.IsNullOrWhiteSpace(port))
                {
                    throw new ArgumentOutOfRangeException(nameof(port));
                }

                var dis = new CompositeDisposable();
                IModbusSerialMaster? master = null;
                var connected = false;
                var connectionMessageSent = false;

                // Connection watchdog
                dis.Add(Observable.Interval(CheckConnectionInterval)
                    .Subscribe(_ =>
                    {
                        if (connected && master is null)
                        {
                            observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                            connected = false;
                        }

                        if (connected || connectionMessageSent)
                        {
                            return;
                        }

                        connectionMessageSent = true;
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }));

                // Directly create master
                try
                {
                    observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                    var serial = CreateSerialPort(port, baudRate, dataBits, parity, stopBits, handshake);
                    await serial.Open();
                    serial.ReadTimeout = 10_000; // Set timeout to 10 seconds
                    master = ModbusSerialMaster.CreateRtu(serial);
                    connected = true;
                    connectionMessageSent = false;
                    observer.OnNext((connected, null, master));
                }
                catch (Exception ex)
                {
                    connected = false;
                    master = null;
                    Console.WriteLine($"SerialRtuMaster error: {ex.Message}");
                    observer.OnNext((connected, new ModbusCommunicationException("ModbusRx Serial RTU Master Fault", ex), master));
                }

                return dis;
            }).Publish().RefCount();

        /// <summary>Create a reactive Modbus Serial ASCII master that automatically manages connection state.</summary>
        /// <param name="port">The COM port (e.g., "COM1").</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>An observable stream of connection status and the ASCII master.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when port is null or whitespace.</exception>
        public static IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> SerialAsciiMaster(
            string port,
            int baudRate = 9600,
            int dataBits = 7,
            Parity parity = Parity.Even,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None) =>
            Observable.Create<(bool connected, Exception? error, IModbusSerialMaster? master)>(async observer =>
            {
                if (string.IsNullOrWhiteSpace(port))
                {
                    throw new ArgumentOutOfRangeException(nameof(port));
                }

                var dis = new CompositeDisposable();
                IModbusSerialMaster? master = null;
                var connected = false;
                var connectionMessageSent = false;

                // Connection watchdog
                dis.Add(Observable.Interval(CheckConnectionInterval)
                    .Subscribe(_ =>
                    {
                        if (connected && master is null)
                        {
                            observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                            connected = false;
                        }

                        if (connected || connectionMessageSent)
                        {
                            return;
                        }

                        connectionMessageSent = true;
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }));

                // Directly create master
                try
                {
                    observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                    var serialport = CreateSerialPort(port, baudRate, dataBits, parity, stopBits, handshake);
                    master = ModbusSerialMaster.CreateAscii(serialport);
                    await serialport.Open();
                    connected = true;
                    connectionMessageSent = false;
                    observer.OnNext((connected, null, master));
                }
                catch (Exception ex)
                {
                    connected = false;
                    master = null;
                    observer.OnNext((connected, new ModbusCommunicationException("ModbusRx Serial ASCII Master Fault", ex), master));
                }

                return dis;
            }).Publish().RefCount();

        /// <summary>Reads holding registers from a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadHoldingRegistersCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads input registers from a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadInputRegistersCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads coils from a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadCoils(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadCoilsCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads discrete inputs from a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadInputs(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadInputsCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads holding registers from slave address 1 on a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadHoldingRegistersCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads input registers from slave address 1 on a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadInputRegistersCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads coils from slave address 1 on a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadCoils(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadCoilsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads discrete inputs from slave address 1 on a serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadInputs(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadInputsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads holding registers from an IP master stream.</summary>
        /// <param name="source">The source IP master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadHoldingRegistersCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads input registers from an IP master stream.</summary>
        /// <param name="source">The source IP master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadInputRegistersCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads coils from an IP master stream.</summary>
        /// <param name="source">The source IP master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadCoils(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadCoilsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads discrete inputs from an IP master stream.</summary>
        /// <param name="source">The source IP master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadInputs(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            ReadInputsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads the holding registers using a reactive serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegistersCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            if (modbus.master is null)
                            {
                                return;
                            }

                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                modbus.master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Holding Registers Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Reads the input registers using a reactive serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(ushort[]? data, Exception? error)> ReadInputRegistersCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            if (modbus.master is null)
                            {
                                return;
                            }

                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master.ReadInputRegistersAsync(slaveAddress, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                modbus.master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Input Registers Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Reads the coils using a reactive serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(bool[]? data, Exception? error)> ReadCoilsCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            if (modbus.master is null)
                            {
                                return;
                            }

                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                modbus.master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Coils Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Reads the discrete inputs using a reactive serial master stream.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(bool[]? data, Exception? error)> ReadInputsCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            if (modbus.master is null)
                            {
                                return;
                            }

                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master.ReadInputsAsync(slaveAddress, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                modbus.master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Inputs Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Convenience overload that defaults the slave address to 1 for ReadHoldingRegisters.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegistersCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            IModbusSerialMaster? master = null;
                            Console.WriteLine($"ReadHoldingRegisters polling: connected={modbus.connected}, error={modbus.error?.Message}");
                            try
                            {
                                isConnected = modbus.connected;
                                master = modbus.master;
                                if (modbus.connected && modbus.error is null && master is not null)
                                {
                                    var result = await master.ReadHoldingRegistersAsync(1, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                master?.Dispose();
                                isConnected = false;
                                Console.WriteLine($"ReadHoldingRegisters error: {ex.Message}");
                                observer.OnError(new ModbusCommunicationException("Read Holding Registers Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Convenience overload that defaults the slave address to 1 for ReadInputRegisters.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(ushort[]? data, Exception? error)> ReadInputRegistersCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            IModbusSerialMaster? master = null;
                            try
                            {
                                isConnected = modbus.connected;
                                master = modbus.master;
                                if (modbus.connected && modbus.error is null && master is not null)
                                {
                                    var result = await master.ReadInputRegistersAsync(1, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Input Registers Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Convenience overload that defaults the slave address to 1 for ReadCoils.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(bool[]? data, Exception? error)> ReadCoilsCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            IModbusSerialMaster? master = null;
                            try
                            {
                                isConnected = modbus.connected;
                                master = modbus.master;
                                if (modbus.connected && modbus.error is null && master is not null)
                                {
                                    var result = await master.ReadCoilsAsync(1, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Coils Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Convenience overload that defaults the slave address to 1 for ReadInputs.</summary>
        /// <param name="source">The source serial master stream.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points to read.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An observable sequence producing the result data or error.</returns>
        internal static IObservable<(bool[]? data, Exception? error)> ReadInputsCore(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
            {
                var isConnected = false;
                var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            IModbusSerialMaster? master = null;
                            try
                            {
                                isConnected = modbus.connected;
                                master = modbus.master;
                                if (modbus.connected && modbus.error is null && master is not null)
                                {
                                    var result = await master.ReadInputsAsync(1, startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                master?.Dispose();
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Inputs Error", ex));
                            }
                        },
                        exception => observer.OnError(exception));
                return Disposable.Create(() => subscription.Dispose());
            }).Retry(int.MaxValue);

        /// <summary>Convert ushort span to float with high-performance operations.</summary>
        /// <param name="inputs">The inputs span.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <returns>A float value or null if insufficient data.</returns>
        internal static float? ToFloatCore(ReadOnlySpan<ushort> inputs, int start, bool swapWords = true)
        {
            return inputs.Length < start + 2 ? null : ModbusUtility.ReadSingle(inputs[start..], swapWords);
        }

        /// <summary>Convert ushort array to float.</summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <returns>
        /// A float.
        /// </returns>
        internal static float? ToFloatCore(ushort[]? inputs, int start, bool swapWords = true)
        {
            return inputs is null || inputs.Length < start + 2 ? null : inputs.AsSpan().ToFloat(start, swapWords);
        }

        /// <summary>Convert ushort span to double with high-performance operations.</summary>
        /// <param name="inputs">The inputs span.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <returns>A double value or null if insufficient data.</returns>
        internal static double? ToDoubleCore(ReadOnlySpan<ushort> inputs, int start, bool swapWords = true)
        {
            return inputs.Length < start + 4 ? null : ModbusUtility.ReadDouble(inputs[start..], swapWords);
        }

        /// <summary>Converts to double.</summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <returns>A double.</returns>
        internal static double? ToDoubleCore(ushort[]? inputs, int start, bool swapWords = true)
        {
            return inputs is null || inputs.Length < start + 4 ? null : inputs.AsSpan().ToDouble(start, swapWords);
        }

        /// <summary>Write float to ushort span with high-performance operations.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="output">The output span.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <exception cref="ArgumentException">Thrown when output span is too small.</exception>
        internal static void FromFloatCore(float input, Span<ushort> output, int start, bool swapWords = true)
        {
            if (output.Length < start + 2)
            {
                throw new ArgumentException("Output span is too small.", nameof(output));
            }

            ModbusUtility.WriteSingle(input, output[start..], swapWords);
        }

        /// <summary>Froms the float.</summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        internal static void FromFloatCore(float input, ushort[] output, int start, bool swapWords = true)
        {
            if (output is null || output.Length < start + 2)
            {
                return;
            }

            input.FromFloat(output.AsSpan(), start, swapWords);
        }

        /// <summary>Write double to ushort span with high-performance operations.</summary>
        /// <param name="input">The input value.</param>
        /// <param name="output">The output span.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <exception cref="ArgumentException">Thrown when output span is too small.</exception>
        internal static void FromDoubleCore(double input, Span<ushort> output, int start, bool swapWords = true)
        {
            if (output.Length < start + 4)
            {
                throw new ArgumentException("Output span is too small.", nameof(output));
            }

            ModbusUtility.WriteDouble(input, output[start..], swapWords);
        }

        /// <summary>Froms the double.</summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        internal static void FromDoubleCore(double input, ushort[] output, int start, bool swapWords = true)
        {
            if (output is null || output.Length < start + 4)
            {
                return;
            }

            input.FromDouble(output.AsSpan(), start, swapWords);
        }

        /// <summary>Observes the data store written to.</summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of DataStoreEventArgs.</returns>
        internal static IObservable<DataStoreEventArgs> ObserveDataStoreReadFromCore(ModbusSlave slave) =>
            Observable.FromEventPattern<DataStoreEventArgs>(
                handler => slave.DataStore.DataStoreReadFrom += handler,
                handler => slave.DataStore.DataStoreReadFrom -= handler)
                .Select(pattern => pattern.EventArgs);

        /// <summary>Observes the data store written to.</summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of DataStoreEventArgs.</returns>
        internal static IObservable<DataStoreEventArgs> ObserveDataStoreWrittenToCore(ModbusSlave slave) =>
            Observable.FromEventPattern<DataStoreEventArgs>(
                handler => slave.DataStore.DataStoreWrittenTo += handler,
                handler => slave.DataStore.DataStoreWrittenTo -= handler)
                .Select(pattern => pattern.EventArgs);

        /// <summary>Observes the request.</summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of ModbusSlaveRequestEventArgs.</returns>
        internal static IObservable<ModbusSlaveRequestEventArgs> ObserveRequestCore(ModbusSlave slave) =>
            Observable.FromEventPattern<ModbusSlaveRequestEventArgs>(
                handler => slave.ModbusSlaveRequestReceived += handler,
                handler => slave.ModbusSlaveRequestReceived -= handler)
                .Select(pattern => pattern.EventArgs);

        /// <summary>Observes the write complete.</summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of ModbusSlaveRequestEventArgs.</returns>
        internal static IObservable<ModbusSlaveRequestEventArgs> ObserveWriteCompleteCore(ModbusSlave slave) =>
            Observable.FromEventPattern<ModbusSlaveRequestEventArgs>(
                handler => slave.WriteComplete += handler,
                handler => slave.WriteComplete -= handler)
                .Select(pattern => pattern.EventArgs);

        /// <summary>Reads the input registers.</summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of ushort.
        /// </returns>
        internal static IObservable<(ushort[]? data, Exception? error)> ReadInputRegistersCore(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master!.ReadInputRegistersAsync(startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                modbus.master?.Dispose();
                                modbus.master = null;
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Input Registers Error", ex));
                            }
                        },
                        (exception) => observer.OnError(exception));
                    return Disposable.Create(() => subscription.Dispose());
                }).Retry(int.MaxValue);

        /// <summary>Reads the holding registers.</summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of ushort.
        /// </returns>
        internal static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegistersCore(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            Console.WriteLine($"ReadHoldingRegisters polling: connected={modbus.connected}, error={modbus.error?.Message}");
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master!.ReadHoldingRegistersAsync(startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                modbus.master?.Dispose();
                                modbus.master = null;
                                isConnected = false;
                                Console.WriteLine($"ReadHoldingRegisters error: {ex.Message}");
                                observer.OnError(new ModbusCommunicationException("Read Holding Registers Error", ex));
                            }
                        },
                        (exception) => observer.OnError(exception));
                    return Disposable.Create(() => subscription.Dispose());
                }).Retry(int.MaxValue);

        /// <summary>Reads the coils.</summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of bool.
        /// </returns>
        internal static IObservable<(bool[]? data, Exception? error)> ReadCoilsCore(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master!.ReadCoilsAsync(startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                modbus.master?.Dispose();
                                modbus.master = null;
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Coils Error", ex));
                            }
                        },
                        (exception) => observer.OnError(exception));
                    return Disposable.Create(() => subscription.Dispose());
                }).Retry(int.MaxValue);

        /// <summary>Reads the coils.</summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of bool.
        /// </returns>
        internal static IObservable<(bool[]? data, Exception? error)> ReadInputsCore(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry(int.MaxValue)
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error is null)
                                {
                                    var result = await modbus.master!.ReadInputsAsync(startAddress, numberOfPoints);
                                    if (result is not null)
                                    {
                                        observer.OnNext((result, modbus.error));
                                    }
                                }
                                else
                                {
                                    observer.OnNext((null, modbus.error));
                                }
                            }
                            catch (Exception ex)
                            {
                                // Asume the connection is broken.
                                modbus.master?.Dispose();
                                modbus.master = null;
                                isConnected = false;
                                observer.OnError(new ModbusCommunicationException("Read Inputs Error", ex));
                            }
                        },
                        (exception) => observer.OnError(exception));
                    return Disposable.Create(() => subscription.Dispose());
                }).Retry(int.MaxValue);

        /// <summary>Creates a serial port resource configured with the requested serial settings.</summary>
        /// <param name="port">The COM port name.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>The configured serial port resource.</returns>
        private static SerialPortRx CreateSerialPort(
            string port,
            int baudRate,
            int dataBits,
            Parity parity,
            StopBits stopBits,
            Handshake handshake) =>
            new(port, baudRate)
            {
                DataBits = dataBits,
                Parity = parity,
                StopBits = stopBits,
                Handshake = handshake
            };

        /// <summary>Determines whether any available port name contains the requested port token.</summary>
        /// <param name="portNames">The available port names.</param>
        /// <param name="port">The requested port token.</param>
        /// <returns>True when a matching port name is present.</returns>
        private static bool ContainsPortName(IEnumerable<string> portNames, string port)
        {
            foreach (var portName in portNames)
            {
                if (portName.Contains(port))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
