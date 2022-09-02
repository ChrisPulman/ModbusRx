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

internal static class TestCases
{
    public static void Serial()
    {
        using var masterPort = new SerialPortRx("COM2");
        using var slavePort = new SerialPortRx("COM1");

        // configure serial ports
        masterPort.BaudRate = slavePort.BaudRate = 9600;
        masterPort.DataBits = slavePort.DataBits = 8;
        masterPort.Parity = slavePort.Parity = Parity.None;
        masterPort.StopBits = slavePort.StopBits = StopBits.One;
        masterPort.Open();
        slavePort.Open();

        using var slave = ModbusSerialSlave.CreateRtu(1, slavePort);
        StartSlave(slave);

        // create modbus master
        using var master = ModbusSerialMaster.CreateRtu(masterPort);
        ReadRegistersAsync(master);
    }

    public static void Tcp()
    {
        var slaveClient = new TcpListener(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
        using var slave = ModbusTcpSlave.CreateTcp((byte)1, slaveClient);
        StartSlave(slave);

        var address = new IPAddress(new byte[] { 127, 0, 0, 1 });
        var masterClient = new TcpClientRx(address.ToString(), 502);

        using var master = ModbusIpMaster.CreateIp(masterClient);
        ReadRegistersAsync(master);
    }

    public static void Udp()
    {
        var slaveClient = new UdpClientRx(502);
        using var slave = ModbusUdpSlave.CreateUdp(slaveClient);
        StartSlave(slave);

        var masterClient = new UdpClientRx();
        var endPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
        masterClient.Connect(endPoint);

        using var master = ModbusIpMaster.CreateIp(masterClient);
        ReadRegistersAsync(master);
    }

    public static void StartSlave(ModbusSlave slave)
    {
        slave.DataStore = DataStoreFactory.CreateTestDataStore();
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.Start();
    }

    public static async Task ReadRegistersAsync(IModbusMaster master)
    {
        var result = await master.ReadHoldingRegistersAsync(1, 0, 5);

        for (var i = 0; i < 5; i++)
        {
            if (result[i] != i + 1)
            {
                throw new Exception();
            }
        }
    }
}
