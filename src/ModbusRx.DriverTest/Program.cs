// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Message;
using ModbusRx.Reactive;

namespace ModbusRx.DriverTest
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Create.TcpIpSlave("127.0.0.1")
                    .WriteInputRegisters(0, Observable.Return(new ushort[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }))
                    .Subscribe(slave =>
                    {
                        Console.WriteLine(slave.Masters.Count);
                    });

                Create.TcpIpMaster("127.0.0.1")
                    .ReadInputRegisters(0, 12)
                    .Subscribe(modbus =>
                    {
                        for (var i = 0; i < modbus.data?.Length; i++)
                        {
                            Console.WriteLine($"Input {i}={modbus.data[i]}");
                        }
                    });

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
