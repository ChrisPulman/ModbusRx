// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.DriverTest
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                using var client = new TcpClientRx("127.0.0.1", 502);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                var master = ModbusIpMaster.CreateIp(client);

                // read five input values
                const ushort startAddress = 100;
                const ushort numInputs = 5;
                var inputs = master.ReadInputs(startAddress, numInputs);

                for (var i = 0; i < numInputs; i++)
                {
                    Console.WriteLine($"Input {startAddress + i}={(inputs[i] ? 1 : 0)}");
                }

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
