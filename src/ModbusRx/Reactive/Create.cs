// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.NetworkInformation;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.Reactive
{
    /// <summary>
    /// ModbusRx.
    /// </summary>
    public static class Create
    {
        /// <summary>
        /// Convert ushort to float.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="start">The start.</param>
        /// <param name="swapBytes">if set to <c>true</c> [swap bytes].</param>
        /// <returns>
        /// A float.
        /// </returns>
        public static float? ToFloat(this ushort[]? inputs, int start, bool swapBytes = true)
        {
            if (inputs == null || inputs.Length < start + 1)
            {
                return null;
            }

            var ba0 = BitConverter.GetBytes(inputs[start]);
            var ba1 = BitConverter.GetBytes(inputs[start + 1]);
            //// byte swap
            var ba = swapBytes ? new byte[] { ba1[0], ba1[1], ba0[0], ba0[1] } : new byte[] { ba0[0], ba0[1], ba1[0], ba1[1] };
            return BitConverter.ToSingle(ba, 0);
        }

        /// <summary>
        /// Create a TcpIpMaster with the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> TcpIpMaster(string ipAddress, int port = 502, double interval = 100.0) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                var pingSender = new Ping();
                dis.Add(pingSender);
                ModbusIpMaster? master = null;
                var connected = false;

                dis.Add(Observable.Interval(TimeSpan.FromMilliseconds(5000)).Subscribe(_ =>
                {
                    if (!connected)
                    {
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }
                }));

                dis.Add(Observable.Interval(TimeSpan.FromSeconds(10))
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
                            if (master == null && res?.Status == IPStatus.Success)
                            {
                                master = ModbusIpMaster.CreateIp(new TcpClientRx(ipAddress, port));
                                dis.Add(master);
                                connected = true;
                                observer.OnNext((connected, null, master));
                            }
                        }

                        return res;
                    }).Retry().Subscribe());

                dis.Add(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Subscribe(_ =>
                {
                    try
                    {
                        if (connected && master != null)
                        {
                            observer.OnNext((connected, null, master));
                        }
                    }
                    catch (Exception ex)
                    {
                        master?.Dispose();
                        master = null;
                        observer.OnNext((false, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                    }
                    finally
                    {
                        connected = master == null;
                    }
                }));

                return dis;
            });

        /// <summary>
        /// Create a UdpIpMaster with the specified ip address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        public static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> UdpIpMaster(string ipAddress, int port = 502, double interval = 100.0) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                var pingSender = new Ping();
                dis.Add(pingSender);
                ModbusIpMaster? master = null;
                var connected = false;

                dis.Add(Observable.Interval(TimeSpan.FromMilliseconds(5000)).Subscribe(_ =>
                {
                    if (!connected)
                    {
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }
                }));

                dis.Add(Observable.Interval(TimeSpan.FromSeconds(10))
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
                            if (master == null && res?.Status == IPStatus.Success)
                            {
                                master = ModbusIpMaster.CreateIp(new UdpClientRx(ipAddress, port));
                                dis.Add(master);
                                connected = true;
                                observer.OnNext((connected, null, master));
                            }
                        }

                        return res;
                    }).Retry().Subscribe());

                dis.Add(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Subscribe(_ =>
                {
                    try
                    {
                        if (connected && master != null)
                        {
                            observer.OnNext((connected, null, master));
                        }
                    }
                    catch (Exception ex)
                    {
                        master?.Dispose();
                        master = null;
                        observer.OnNext((false, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                    }
                    finally
                    {
                        connected = master == null;
                    }
                }));

                return dis;
            });

        /// <summary>
        /// Create a SerialIpMaster with the specified ip address.
        /// </summary>
        /// <param name="port">The COM Port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>
        /// The master and connection status.
        /// </returns>
        private static IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> SerialIpMaster(string port, int baudRate = 502, double interval = 100.0) =>
            Observable.Create<(bool connected, Exception? error, ModbusIpMaster? master)>(observer =>
            {
                var dis = new CompositeDisposable();
                var pingSender = new Ping();
                dis.Add(pingSender);
                ModbusIpMaster? master = null;
                var connected = false;

                dis.Add(Observable.Interval(TimeSpan.FromMilliseconds(5000)).Subscribe(_ =>
                {
                    if (!connected)
                    {
                        observer.OnNext((connected, new ModbusCommunicationException("Lost Communication"), master));
                    }
                }));

                // TODO : Add a retry mechanism for the serial port.

                ////dis.Add(Observable.Interval(TimeSpan.FromSeconds(10))
                ////    .Where(_ => !connected)
                ////    .Select(async _ => await pingSender.SendPingAsync(ipAddress, 1000).ConfigureAwait(false))
                ////    .Select(x =>
                ////    {
                ////        var res = default(PingReply);

                ////        try
                ////        {
                ////            res = x?.Result;
                ////        }
                ////        finally
                ////        {
                ////            if (master == null && res?.Status == IPStatus.Success)
                ////            {
                ////                master = ModbusIpMaster.CreateIp(new SerialPortRx(port, baudRate));
                ////                dis.Add(master);
                ////                connected = true;
                ////                observer.OnNext((connected, null, master));
                ////            }
                ////        }

                ////        return res;
                ////    }).Retry().Subscribe());

                dis.Add(Observable.Interval(TimeSpan.FromMilliseconds(interval)).Subscribe(_ =>
                {
                    try
                    {
                        if (connected && master != null)
                        {
                            observer.OnNext((connected, null, master));
                        }
                    }
                    catch (Exception ex)
                    {
                        master?.Dispose();
                        master = null;
                        observer.OnNext((false, new ModbusCommunicationException("ModbusRx Master Fault", ex), master));
                    }
                    finally
                    {
                        connected = master == null;
                    }
                }));

                return dis;
            });
    }
}
