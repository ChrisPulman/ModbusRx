// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Message;

namespace ModbusRx.Reactive
{
    /// <summary>
    /// ModbusRx.
    /// </summary>
    public static class Create
    {
        /// <summary>
        /// Gets or sets the ping interval.
        /// </summary>
        /// <value>
        /// The ping interval.
        /// </value>
        public static TimeSpan PingInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the check connection interval.
        /// </summary>
        /// <value>
        /// The check connection interval.
        /// </value>
        public static TimeSpan CheckConnectionInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Convert ushort to float.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <returns>
        /// A float.
        /// </returns>
        public static float? ToFloat(this ushort[]? inputs, int start, bool swapWords = true)
        {
            if (inputs == null || inputs.Length < start + 1)
            {
                return null;
            }

            var ba0 = BitConverter.GetBytes(inputs[start]);
            var ba1 = BitConverter.GetBytes(inputs[start + 1]);
            //// byte swap
            var ba = swapWords ? ba1.Concat(ba0).ToArray() : ba0.Concat(ba1).ToArray();
            return BitConverter.ToSingle(ba, 0);
        }

        /// <summary>
        /// Converts to double.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        /// <returns>A double.</returns>
        public static double? ToDouble(this ushort[]? inputs, int start, bool swapWords = true)
        {
            if (inputs == null || inputs.Length < start + 3)
            {
                return null;
            }

            var ba0 = BitConverter.GetBytes(inputs[start]);
            var ba1 = BitConverter.GetBytes(inputs[start + 1]);
            var ba2 = BitConverter.GetBytes(inputs[start + 2]);
            var ba3 = BitConverter.GetBytes(inputs[start + 3]);
            //// byte swap
            var ba = swapWords ? ba1.Concat(ba0).Concat(ba3).Concat(ba2).ToArray() : ba0.Concat(ba1).Concat(ba2).Concat(ba3).ToArray();
            return BitConverter.ToDouble(ba, 0);
        }

        /// <summary>
        /// Froms the float.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        public static void FromFloat(this float input, ushort[] output, int start, bool swapWords = true)
        {
            if (output == null || output.Length < start + 1)
            {
                return;
            }

            var ba = BitConverter.GetBytes(input);
            var ba0 = ba.Take(2).ToArray();
            var ba1 = ba.Skip(2).ToArray();
            output[start] = BitConverter.ToUInt16(swapWords ? ba1 : ba0, 0);
            output[start + 1] = BitConverter.ToUInt16(swapWords ? ba0 : ba1, 0);
        }

        /// <summary>
        /// Froms the double.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapWords">if set to <c>true</c> [swap words].</param>
        public static void FromDouble(this double input, ushort[] output, int start, bool swapWords = true)
        {
            if (output == null || output.Length < start + 3)
            {
                return;
            }

            var ba = BitConverter.GetBytes(input);
            var ba0 = ba.Take(2).ToArray();
            var ba1 = ba.Skip(2).Take(2).ToArray();
            var ba2 = ba.Skip(4).Take(2).ToArray();
            var ba3 = ba.Skip(6).ToArray();
            output[start] = BitConverter.ToUInt16(swapWords ? ba1 : ba0, 0);
            output[start + 1] = BitConverter.ToUInt16(swapWords ? ba0 : ba1, 0);
            output[start + 2] = BitConverter.ToUInt16(swapWords ? ba3 : ba2, 0);
            output[start + 3] = BitConverter.ToUInt16(swapWords ? ba2 : ba3, 0);
        }

        /// <summary>
        /// Observes the data store written to.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of DataStoreEventArgs.</returns>
        public static IObservable<DataStoreEventArgs> ObserveDataStoreReadFrom(this ModbusSlave slave) =>
            Observable.FromEvent<EventHandler<DataStoreEventArgs>, DataStoreEventArgs>(
                handler => (sender, args) => handler(args),
                handler => slave.DataStore.DataStoreReadFrom += handler,
                handler => slave.DataStore.DataStoreReadFrom -= handler);

        /// <summary>
        /// Observes the data store written to.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of DataStoreEventArgs.</returns>
        public static IObservable<DataStoreEventArgs> ObserveDataStoreWrittenTo(this ModbusSlave slave) =>
            Observable.FromEvent<EventHandler<DataStoreEventArgs>, DataStoreEventArgs>(
                handler => (sender, args) => handler(args),
                handler => slave.DataStore.DataStoreWrittenTo += handler,
                handler => slave.DataStore.DataStoreWrittenTo -= handler);

        /// <summary>
        /// Observes the request.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of ModbusSlaveRequestEventArgs.</returns>
        public static IObservable<ModbusSlaveRequestEventArgs> ObserveRequest(this ModbusSlave slave) =>
            Observable.FromEvent<EventHandler<ModbusSlaveRequestEventArgs>, ModbusSlaveRequestEventArgs>(
                handler => (sender, args) => handler(args),
                handler => slave.ModbusSlaveRequestReceived += handler,
                handler => slave.ModbusSlaveRequestReceived -= handler);

        /// <summary>
        /// Observes the write complete.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <returns>An Observable of ModbusSlaveRequestEventArgs.</returns>
        public static IObservable<ModbusSlaveRequestEventArgs> ObserveWriteComplete(this ModbusSlave slave) =>
            Observable.FromEvent<EventHandler<ModbusSlaveRequestEventArgs>, ModbusSlaveRequestEventArgs>(
                handler => (sender, args) => handler(args),
                handler => slave.WriteComplete += handler,
                handler => slave.WriteComplete -= handler);

        /// <summary>
        /// Reads the input registers.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of ushort.
        /// </returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 100.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry()
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error == null)
                                {
                                    var result = await modbus.master!.ReadInputRegistersAsync(startAddress, numberOfPoints);
                                    if (result != null)
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
                }).Retry();

        /// <summary>
        /// Reads the holding registers.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of ushort.
        /// </returns>
        public static IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 100.0) =>
            Observable.Create<(ushort[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry()
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error == null)
                                {
                                    var result = await modbus.master!.ReadHoldingRegistersAsync(startAddress, numberOfPoints);
                                    if (result != null)
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
                                observer.OnError(new ModbusCommunicationException("Read Holding Registers Error", ex));
                            }
                        },
                        (exception) => observer.OnError(exception));
                    return Disposable.Create(() => subscription.Dispose());
                }).Retry();

        /// <summary>
        /// Reads the coils.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of bool.
        /// </returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadCoils(this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 100.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry()
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error == null)
                                {
                                    var result = await modbus.master!.ReadCoilsAsync(startAddress, numberOfPoints);
                                    if (result != null)
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
                }).Retry();

        /// <summary>
        /// Reads the coils.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// A Observable of bool.
        /// </returns>
        public static IObservable<(bool[]? data, Exception? error)> ReadInputs(this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source, ushort startAddress, ushort numberOfPoints, double interval = 100.0) =>
            Observable.Create<(bool[]? data, Exception? error)>(observer =>
                {
                    var isConnected = false;
                    var subscription = source
                    .CombineLatest(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Where(_ => isConnected).StartWith(long.MinValue), (modbus, _) => modbus)
                    .Retry()
                    .Subscribe(
                        async modbus =>
                        {
                            try
                            {
                                isConnected = modbus.connected;
                                if (modbus.connected && modbus.error == null)
                                {
                                    var result = await modbus.master!.ReadInputsAsync(startAddress, numberOfPoints);
                                    if (result != null)
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
                }).Retry();

        /// <summary>
        /// Create a TcpIpMaster with the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> TcpIpMaster(string ipAddress, int port = 502) =>
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
                    if (connected && master == null)
                    {
                        observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                        connected = false;
                    }

                    if (!connected && !connectionMessageSent)
                    {
                        connectionMessageSent = true;
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }
                }));

                dis.Add(Observable.Timer(CheckConnectionInterval, PingInterval)
                    .Where(_ => !connected)
                    .Select(async _ => await pingSender.SendPingAsync(ipAddress, 1000).ConfigureAwait(false))
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
                                if (master == null && res?.Status == IPStatus.Success)
                                {
                                    observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                                    master = ModbusIpMaster.CreateIp(new TcpClientRx(ipAddress, port));
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
                    }).Retry().Subscribe());
                return dis;
            }).Publish().RefCount();

        /// <summary>
        /// TCPs the ip slave.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>An Observable of.</returns>
        /// <exception cref="ArgumentNullException">nameof(ipAddress).</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// nameof(port)
        /// or
        /// nameof(unitId).
        /// </exception>
        public static IObservable<ModbusTcpSlave> TcpIpSlave(string ipAddress!!, int port = 502, byte unitId = 1)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentOutOfRangeException(nameof(ipAddress));
            }

            if (port < 0 || port > 65535)
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
                 var address = IPAddress.Parse(ipAddress);
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
             }).Retry().Publish().RefCount();
        }

        /// <summary>
        /// Create a UdpIpMaster with the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> UdpIpMaster(string ipAddress, int port = 502) =>
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
                    if (connected && master == null)
                    {
                        observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                        connected = false;
                    }

                    if (!connected && !connectionMessageSent)
                    {
                        connectionMessageSent = true;
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }
                }));

                dis.Add(Observable.Timer(CheckConnectionInterval, PingInterval)
                    .Where(_ => !connected)
                    .Select(async _ => await pingSender.SendPingAsync(ipAddress, 1000).ConfigureAwait(false))
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
                                if (master == null && res?.Status == IPStatus.Success)
                                {
                                    observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                                    master = ModbusIpMaster.CreateIp(new UdpClientRx(ipAddress, port));
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
                    }).Retry().Subscribe());

                return dis;
            }).Publish().RefCount();

        /// <summary>
        /// Creates an UdpIp slave.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>An Observable of.</returns>
        /// <exception cref="ArgumentNullException">nameof(ipAddress).</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// nameof(port)
        /// or
        /// nameof(unitId).
        /// </exception>
        public static IObservable<ModbusUdpSlave> UdpIpSlave(string ipAddress!!, int port = 502, byte unitId = 1)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentOutOfRangeException(nameof(ipAddress));
            }

            if (port < 0 || port > 65535)
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
                 using var slave = ModbusUdpSlave.CreateUdp(unitId, new UdpClientRx(ipAddress, port));
                 await slave.ListenAsync();
                 dis.Add(slave);
                 observer.OnNext(slave);
                 return Disposable.Create(() =>
                   {
                       dis.Dispose();
                   });
             }).Retry().Publish().RefCount();
        }

        /// <summary>
        /// Create a SerialIpMaster with the specified ip address.
        /// </summary>
        /// <param name="port">The COM Port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> SerialIpMaster(string port, int baudRate = 502) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                var pingSender = new Ping();
                dis.Add(pingSender);
                ModbusIpMaster? master = null;
                var connected = false;
                var connectionMessageSent = false;

                dis.Add(Observable.Interval(CheckConnectionInterval).Subscribe(_ =>
                {
                    if (connected && master == null)
                    {
                        observer.OnNext((false, new ModbusCommunicationException("Reset connected Master is null"), null));
                        connected = false;
                    }

                    if (!connected && !connectionMessageSent)
                    {
                        connectionMessageSent = true;
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }
                }));

                var comdis = new CompositeDisposable();

                // Subscribe to com ports available
                SerialPortRx.PortNames().Do(x =>
                {
                    try
                    {
                        if (comdis?.Count == 0 && x.Contains(port))
                        {
                            observer.OnNext((false, new ModbusCommunicationException("Create Master"), null));
                            master = ModbusIpMaster.CreateIp(new SerialPortRx(port, baudRate));
                            comdis.Add(master);
                            connected = true;
                            connectionMessageSent = false;
                            observer.OnNext((connected, null, master));
                        }
                        else
                        {
                            dis.Remove(comdis!);
                            comdis?.Dispose();
                            connected = false;
                            master = null;
                            observer.OnNext((connected, null, master));
                            comdis = new CompositeDisposable();
                            comdis.AddTo(dis);
                        }
                    }
                    catch (Exception ex)
                    {
                        dis.Remove(comdis!);
                        comdis?.Dispose();
                        connected = false;
                        master = null;
                        observer.OnNext((connected, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                        comdis = new CompositeDisposable();
                        comdis.AddTo(dis);
                    }
                }).Retry().Subscribe().AddTo(dis);

                return dis;
            }).Publish().RefCount();

        /// <summary>
        /// Creates an Serial Rtu Slave.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>
        /// An Observable of.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">nameof(port)
        /// or
        /// nameof(unitId).</exception>
        /// <exception cref="ArgumentNullException">nameof(ipAddress).</exception>
        public static IObservable<ModbusSerialSlave> SerialRtuSlave(string port!!, byte unitId = 1, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (unitId < 1 || unitId > 247)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId));
            }

            return Observable.Create<ModbusSerialSlave>(observer =>
             {
                 var dis = new CompositeDisposable();
                 var comdis = new CompositeDisposable();
                 Thread? slaveThread = null;
                 SerialPortRx.PortNames().Do(x =>
                {
                    try
                    {
                        if (comdis?.Count == 0 && x.Contains(port))
                        {
                            using var slave = ModbusSerialSlave.CreateRtu(unitId, new SerialPortRx(port, baudRate, dataBits, parity, stopBits, handshake));
                            slaveThread = new Thread(async () => await slave.ListenAsync())
                            {
                                IsBackground = true
                            };
                            slaveThread.Start();
                            dis.Add(slave);
                            observer.OnNext(slave);
                        }
                        else
                        {
                            dis.Remove(comdis!);
                            comdis?.Dispose();
                            slaveThread?.Abort();
                            comdis = new CompositeDisposable();
                            comdis.AddTo(dis);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(new ModbusCommunicationException("ModbusRx Slave Fault", ex));
                        dis.Remove(comdis!);
                        comdis?.Dispose();
                        slaveThread?.Abort();
                        comdis = new CompositeDisposable();
                        comdis.AddTo(dis);
                    }
                }).Retry().Subscribe();

                 return Disposable.Create(() =>
                   {
                       slaveThread?.Abort();
                       dis.Dispose();
                   });
             }).Retry().Publish().RefCount();
        }

        /// <summary>
        /// Creates an Serial Ascii Slave.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="dataBits">The data bits.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="stopBits">The stop bits.</param>
        /// <param name="handshake">The handshake.</param>
        /// <returns>
        /// An Observable of.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">nameof(port)
        /// or
        /// nameof(unitId).</exception>
        /// <exception cref="ArgumentNullException">nameof(ipAddress).</exception>
        public static IObservable<ModbusSerialSlave> SerialAsciiSlave(string port!!, byte unitId = 1, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None)
        {
            if (string.IsNullOrWhiteSpace(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (unitId < 1 || unitId > 247)
            {
                throw new ArgumentOutOfRangeException(nameof(unitId));
            }

            return Observable.Create<ModbusSerialSlave>(observer =>
             {
                 var dis = new CompositeDisposable();
                 var comdis = new CompositeDisposable();
                 Thread? slaveThread = null;
                 SerialPortRx.PortNames().Do(x =>
                {
                    try
                    {
                        if (comdis?.Count == 0 && x.Contains(port))
                        {
                            using var slave = ModbusSerialSlave.CreateAscii(unitId, new SerialPortRx(port, baudRate, dataBits, parity, stopBits, handshake));
                            slaveThread = new Thread(async () => await slave.ListenAsync())
                            {
                                IsBackground = true
                            };
                            slaveThread.Start();
                            dis.Add(slave);
                            observer.OnNext(slave);
                        }
                        else
                        {
                            dis.Remove(comdis!);
                            comdis?.Dispose();
                            slaveThread?.Abort();
                            comdis = new CompositeDisposable();
                            comdis.AddTo(dis);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(new ModbusCommunicationException("ModbusRx Slave Fault", ex));
                        dis.Remove(comdis!);
                        comdis?.Dispose();
                        slaveThread?.Abort();
                        comdis = new CompositeDisposable();
                        comdis.AddTo(dis);
                    }
                }).Retry().Subscribe();

                 return Disposable.Create(() =>
                   {
                       slaveThread?.Abort();
                       dis.Dispose();
                   });
             }).Retry().Publish().RefCount();
        }
    }
}
