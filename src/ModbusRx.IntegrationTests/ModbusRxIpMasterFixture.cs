// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Sockets;
using System.Threading;
using CP.IO.Ports;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusIpMasterFixture.
/// </summary>
public class ModbusRxIpMasterFixture
{
    /// <summary>
    /// Overrides the timeout on TCP client.
    /// </summary>
    [Fact]
    public void OverrideTimeoutOnTcpClient()
    {
        var listener = new TcpListener(ModbusRxMasterFixture.TcpHost, ModbusRxMasterFixture.Port);
        using var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, listener);
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.Start();

        var client = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), ModbusRxMasterFixture.Port)
        {
            ReadTimeout = 1500,
            WriteTimeout = 3000
        };
        using var master = ModbusIpMaster.CreateIp(client);
        Assert.Equal(1500, client.ReadTimeout);
        Assert.Equal(3000, client.WriteTimeout);
    }

    /// <summary>
    /// Overrides the timeout on network stream.
    /// </summary>
    [Fact]
    public void OverrideTimeoutOnNetworkStream()
    {
        var listener = new TcpListener(ModbusRxMasterFixture.TcpHost, ModbusRxMasterFixture.Port);
        using var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, listener);
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.Start();

        var client = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), ModbusRxMasterFixture.Port);
        client.Stream.ReadTimeout = 1500;
        client.Stream.WriteTimeout = 3000;
        using var master = ModbusIpMaster.CreateIp(client);
        Assert.Equal(1500, client.ReadTimeout);
        Assert.Equal(3000, client.WriteTimeout);
    }
}
