// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusUdpSlaveFixture.
/// </summary>
public class ModbusRxUdpSlaveFixture
{
    /// <summary>
    /// Modbuses the UDP slave ensure the slave shuts down cleanly.
    /// </summary>
    [Fact]
    public void ModbusUdpSlave_EnsureTheSlaveShutsDownCleanly()
    {
        var client = new UdpClientRx(ModbusRxMasterFixture.Port);
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
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusUdpSlave_NotBound()
    {
        var client = new UdpClientRx();
        ModbusSlave slave = ModbusUdpSlave.CreateUdp(1, client);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await slave.ListenAsync());
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
        masterClient1.Connect(ModbusRxMasterFixture.DefaultModbusIPEndPoint);
        var master1 = ModbusIpMaster.CreateIp(masterClient1);

        var masterClient2 = new UdpClientRx();
        masterClient2.Connect(ModbusRxMasterFixture.DefaultModbusIPEndPoint);
        var master2 = ModbusIpMaster.CreateIp(masterClient2);

        var slaveClient = CreateAndStartUdpSlave(ModbusRxMasterFixture.Port, DataStoreFactory.CreateTestDataStore());

        var master1Thread = new Thread(async () =>
        {
            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(randomNumberGenerator.Next(1000));
                Debug.WriteLine("Read from master 1");
                Assert.Equal(new ushort[] { 2, 3, 4, 5, 6 }, await master1.ReadHoldingRegistersAsync(1, 5));
            }

            master1Complete = true;
        });

        var master2Thread = new Thread(async () =>
        {
            for (var i = 0; i < 5; i++)
            {
                Thread.Sleep(randomNumberGenerator.Next(1000));
                Debug.WriteLine("Read from master 2");
                Assert.Equal(new ushort[] { 3, 4, 5, 6, 7 }, await master2.ReadHoldingRegistersAsync(2, 5));
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
    [Fact]
    public void ModbusUdpSlave_MultiThreaded()
    {
        var dataStore = DataStoreFactory.CreateDefaultDataStore();
        dataStore.CoilDiscretes.Add(false);

        using var slave = CreateAndStartUdpSlave(ModbusRxMasterFixture.Port, dataStore);
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
    private static async void ReadThread(object? state)
    {
        var masterClient = new UdpClientRx();
        masterClient.Connect(ModbusRxMasterFixture.DefaultModbusIPEndPoint);
        using var master = ModbusIpMaster.CreateIp(masterClient);
        master.Transport!.Retries = 0;

        var random = new Random();
        for (var i = 0; i < 5; i++)
        {
            var coils = await master.ReadCoilsAsync(1, 1);
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
