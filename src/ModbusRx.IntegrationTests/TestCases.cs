// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Test case examples for different Modbus communication modes.
/// These are not unit tests but examples for manual testing and development.
/// </summary>
internal static class TestCases
{
    /// <summary>
    /// Example for serial communication testing.
    /// Note: Requires physical hardware and won't work in CI environments.
    /// </summary>
    public static async Task SerialAsync()
    {
        using var masterPort = new SerialPortRx("COM2");
        using var slavePort = new SerialPortRx("COM1");

        // Configure serial ports
        masterPort.BaudRate = slavePort.BaudRate = 9600;
        masterPort.DataBits = slavePort.DataBits = 8;
        masterPort.Parity = slavePort.Parity = Parity.None;
        masterPort.StopBits = slavePort.StopBits = StopBits.One;
        await masterPort.Open();
        await slavePort.Open();

        using var slave = ModbusSerialSlave.CreateRtu(1, slavePort);
        StartSlave(slave);

        // Create modbus master
        using var master = ModbusSerialMaster.CreateRtu(masterPort);
        await ReadRegistersAsync(master);
    }

    /// <summary>
    /// Example for TCP communication testing.
    /// CI-safe as it uses localhost only.
    /// </summary>
    public static async Task TcpAsync()
    {
        var slaveClient = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
        using var slave = ModbusTcpSlave.CreateTcp((byte)1, slaveClient);
        StartSlave(slave);

        var address = new IPAddress(new byte[] { 127, 0, 0, 1 });
        var masterClient = new TcpClientRx(address.ToString(), 502);

        using var master = ModbusIpMaster.CreateIp(masterClient);
        await ReadRegistersAsync(master);
    }

    /// <summary>
    /// Example for UDP communication testing.
    /// CI-safe as it uses localhost only.
    /// </summary>
    public static async Task UdpAsync()
    {
        var slaveClient = new UdpClientRx(502);
        using var slave = ModbusUdpSlave.CreateUdp(slaveClient);
        StartSlave(slave);

        var masterClient = new UdpClientRx();
        var endPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
        masterClient.Connect(endPoint);

        using var master = ModbusIpMaster.CreateIp(masterClient);
        await ReadRegistersAsync(master);
    }

    /// <summary>
    /// Starts a slave with background task instead of Thread.
    /// </summary>
    /// <param name="slave">The slave to start.</param>
    public static void StartSlave(ModbusSlave slave)
    {
        slave.DataStore = DataStoreFactory.CreateTestDataStore();
        
        // Use Task.Run instead of Thread for better async patterns
        var slaveTask = Task.Run(async () =>
        {
            try
            {
                await slave.ListenAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (ObjectDisposedException)
            {
                // Expected when disposed
            }
        });
    }

    /// <summary>
    /// Reads registers asynchronously.
    /// </summary>
    /// <param name="master">The master to read from.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task ReadRegistersAsync(IModbusMaster master)
    {
        var result = await master.ReadHoldingRegistersAsync(1, 0, 5);
        
        // Process results...
        Console.WriteLine($"Read {result.Length} registers: [{string.Join(", ", result)}]");
    }
}
