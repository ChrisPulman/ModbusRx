// <copyright file="NModbusTcpSlaveFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using CP.IO.Ports;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusTcpSlaveFixture.
/// </summary>
public class NModbusTcpSlaveFixture
{
    /// <summary>
    /// Tests possible exception when master closes gracefully immediately after transaction
    /// The goal is the test the exception in WriteCompleted when the slave attempts to read another request from an already closed master.
    /// </summary>
    [Fact]
    public void ModbusTcpSlave_ConnectionClosesGracefully()
    {
        var slaveListener = new TcpListener(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using var slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        var masterClient = new TcpClientRx(ModbusMasterFixture.TcpHost.ToString(), ModbusMasterFixture.Port);
        using (var master = ModbusIpMaster.CreateIp(masterClient))
        {
            master.Transport!.Retries = 0;

            var coils = master.ReadCoils(1, 1);

            Assert.Single(coils);
            Assert.Single(slave.Masters);
        }

        // give the slave some time to remove the master
        Thread.Sleep(50);

        Assert.Empty(slave.Masters);
    }

    /// <summary>
    /// Tests possible exception when master closes gracefully and the ReadHeaderCompleted EndRead call returns 0 bytes.
    /// </summary>
    [Fact]
    public void ModbusTcpSlave_ConnectionSlowlyClosesGracefully()
    {
        var slaveListener = new TcpListener(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using var slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        var masterClient = new TcpClientRx(ModbusMasterFixture.TcpHost.ToString(), ModbusMasterFixture.Port);
        using (var master = ModbusIpMaster.CreateIp(masterClient))
        {
            master.Transport!.Retries = 0;

            var coils = master.ReadCoils(1, 1);
            Assert.Single(coils);

            Assert.Single(slave.Masters);

            // wait a bit to let slave move on to read header
            Thread.Sleep(50);
        }

        // give the slave some time to remove the master
        Thread.Sleep(50);
        Assert.Empty(slave.Masters);
    }

    /// <summary>
    /// Modbuses the TCP slave multi threaded.
    /// </summary>
    [Fact]
    public void ModbusTcpSlave_MultiThreaded()
    {
        var slaveListener = new TcpListener(ModbusMasterFixture.TcpHost, ModbusMasterFixture.Port);
        using var slave = ModbusTcpSlave.CreateTcp(ModbusMasterFixture.SlaveAddress, slaveListener);
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.IsBackground = true;
        slaveThread.Start();

        var workerThread1 = new Thread(Read);
        var workerThread2 = new Thread(Read);
        workerThread1.Start();
        workerThread2.Start();

        workerThread1.Join();
        workerThread2.Join();
    }

    /// <summary>
    /// Reads the specified state.
    /// </summary>
    /// <param name="state">The state.</param>
    private static void Read(object? state)
    {
        var masterClient = new TcpClientRx(ModbusMasterFixture.TcpHost.ToString(), ModbusMasterFixture.Port);
        using var master = ModbusIpMaster.CreateIp(masterClient);
        master.Transport!.Retries = 0;

        var random = new Random();
        for (var i = 0; i < 5; i++)
        {
            var coils = master.ReadCoils(1, 1);
            Assert.Single(coils);
            Debug.WriteLine($"{Environment.CurrentManagedThreadId}: Reading coil value");
            Thread.Sleep(random.Next(100));
        }
    }
}
