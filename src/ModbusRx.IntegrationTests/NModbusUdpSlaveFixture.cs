// <copyright file="NModbusUdpSlaveFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusUdpSlaveFixture.
/// </summary>
public class NModbusUdpSlaveFixture
{
    /// <summary>
    /// Modbuses the UDP slave ensure the slave shuts down cleanly.
    /// </summary>
    [Fact]
    public void ModbusUdpSlave_EnsureTheSlaveShutsDownCleanly()
    {
        var client = new UdpClientRx(ModbusMasterFixture.Port);
        using var slave = ModbusUdpSlave.CreateUdp(1, client);
        var handle = new AutoResetEvent(false);

        var backgroundThread = new Thread(async (_) =>
        {
            handle.Set();
            await slave.ListenAsync();
        })
        {
            IsBackground = true
        };
        backgroundThread.Start();

        handle.WaitOne();
        Thread.Sleep(100);
    }

    /// <summary>
    /// Modbuses the UDP slave not bound.
    /// </summary>
    [Fact]
    public void ModbusUdpSlave_NotBound()
    {
        var client = new UdpClientRx();
        ModbusSlave slave = ModbusUdpSlave.CreateUdp(1, client);
        Assert.ThrowsAsync<InvalidOperationException>(async () => await slave.ListenAsync());
    }

    /// <summary>
    /// Modbuses the UDP slave multiple masters.
    /// </summary>
    [Fact]
    public void ModbusUdpSlave_MultipleMasters()
    {
        var randomNumberGenerator = new Random();
        var master1Complete = false;
        var master2Complete = false;
        var masterClient1 = new UdpClientRx();
        masterClient1.Connect(ModbusMasterFixture.DefaultModbusIPEndPoint);
        var master1 = ModbusIpMaster.CreateIp(masterClient1);

        var masterClient2 = new UdpClientRx();
        masterClient2.Connect(ModbusMasterFixture.DefaultModbusIPEndPoint);
        var master2 = ModbusIpMaster.CreateIp(masterClient2);

        var slaveClient = NModbusUdpSlaveFixture.CreateAndStartUdpSlave(ModbusMasterFixture.Port, DataStoreFactory.CreateTestDataStore());

        var master1Thread = new Thread(() =>
        {
            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(randomNumberGenerator.Next(1000));
                Debug.WriteLine("Read from master 1");
                Assert.Equal(new ushort[] { 2, 3, 4, 5, 6 }, master1.ReadHoldingRegisters(1, 5));
            }

            master1Complete = true;
        });

        var master2Thread = new Thread(() =>
        {
            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(randomNumberGenerator.Next(1000));
                Debug.WriteLine("Read from master 2");
                Assert.Equal(new ushort[] { 3, 4, 5, 6, 7 }, master2.ReadHoldingRegisters(2, 5));
            }

            master2Complete = true;
        });

        master1Thread.Start();
        master2Thread.Start();

        while (!master1Complete || !master2Complete)
        {
            Thread.Sleep(200);
        }

        slaveClient.Close();
        masterClient1.Close();
        masterClient2.Close();
    }

    /// <summary>
    /// Modbuses the UDP slave multi threaded.
    /// </summary>
    [Fact(Skip ="Fault in new code")]
    public void ModbusUdpSlave_MultiThreaded()
    {
        var dataStore = DataStoreFactory.CreateDefaultDataStore();
        dataStore.CoilDiscretes.Add(false);

        using var slave = NModbusUdpSlaveFixture.CreateAndStartUdpSlave(ModbusMasterFixture.Port, dataStore);
        var workerThread1 = new Thread(ReadThread);
        var workerThread2 = new Thread(ReadThread);
        workerThread1.Start();
        workerThread2.Start();

        workerThread1.Join();
        workerThread2.Join();
    }

    /// <summary>
    /// Reads the thread.
    /// </summary>
    /// <param name="state">The state.</param>
    private static void ReadThread(object? state)
    {
        var masterClient = new UdpClientRx();
        masterClient.Connect(ModbusMasterFixture.DefaultModbusIPEndPoint);
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

    /// <summary>
    /// Creates the and start UDP slave.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <param name="dataStore">The data store.</param>
    /// <returns>A UdpClientRx.</returns>
    private static UdpClientRx CreateAndStartUdpSlave(int port, DataStore dataStore)
    {
        var slaveClient = new UdpClientRx(port);
        ModbusSlave slave = ModbusUdpSlave.CreateUdp(slaveClient);
        slave.DataStore = dataStore;
        var slaveThread = new Thread(async () => await slave.ListenAsync());
        slaveThread.Start();

        return slaveClient;
    }
}
