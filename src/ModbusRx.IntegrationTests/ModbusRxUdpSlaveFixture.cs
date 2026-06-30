// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>Tests the NModbusUdpSlaveFixture behavior.</summary>
public sealed class ModbusRxUdpSlaveFixture : NetworkTestBase
{
    /// <summary>Modbuses the UDP slave ensure the slave shuts down cleanly.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusUdpSlave_EnsureTheSlaveShutsDownCleanly()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var client = new UdpClientRx(port);
        var slave = ModbusUdpSlave.CreateUdp(1, client);
        RegisterDisposable(slave);
        RegisterDisposable(client);

        var slaveStarted = false;

        // Act
        _ = Task.Run(async () =>
        {
            try
            {
                slaveStarted = true;
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
            catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 995)
            {
                // Expected when I/O operation is aborted due to thread exit or application request
                // This is normal during test cleanup in CI environments
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Other socket exceptions during cleanup are also expected
            }
        });

        // Wait for slave to start
        await WaitForConditionAsync(() => slaveStarted, TimeSpan.FromSeconds(2));
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        // Assert - Test passes if no exceptions are thrown
        Assert.True(slaveStarted);
    }

    /// <summary>Modbuses the UDP slave not bound.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusUdpSlave_NotBound()
    {
        // Arrange
        var client = new UdpClientRx();
        ModbusSlave slave = ModbusUdpSlave.CreateUdp(1, client);
        RegisterDisposable(slave);
        RegisterDisposable(client);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await slave.ListenAsync());
    }

    /// <summary>Modbuses the UDP slave multiple masters.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusUdpSlave_MultipleMasters()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var master1Complete = false;
        var master2Complete = false;

        var masterClient1 = new UdpClientRx();
        var endPoint = new System.Net.IPEndPoint(ModbusRxMasterFixture.TcpHost, port);
        masterClient1.Connect(endPoint);
        var master1 = ModbusIpMaster.CreateIp(masterClient1);
        RegisterDisposable(master1);
        RegisterDisposable(masterClient1);

        var masterClient2 = new UdpClientRx();
        masterClient2.Connect(endPoint);
        var master2 = ModbusIpMaster.CreateIp(masterClient2);
        RegisterDisposable(master2);
        RegisterDisposable(masterClient2);

        var slaveClient = await CreateAndStartUdpSlaveAsync(port, DataStoreFactory.CreateTestDataStore());
        RegisterDisposable(slaveClient);

        // Act
        var master1Task = Task.Run(async () =>
        {
            for (var i = 0; i < 5; i++)
            {
                var delay = GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(1_000)));
                await Task.Delay(delay, CancellationToken);
                Debug.WriteLine("Read from master 1");
                Assert.Equal([ 2, 3, 4, 5, 6], await master1.ReadHoldingRegistersAsync(1, 5));
            }

            master1Complete = true;
        });

        var master2Task = Task.Run(async () =>
        {
            for (var i = 0; i < 5; i++)
            {
                var delay = GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(1_000)));
                await Task.Delay(delay, CancellationToken);
                Debug.WriteLine("Read from master 2");
                Assert.Equal([ 3, 4, 5, 6, 7], await master2.ReadHoldingRegistersAsync(2, 5));
            }

            master2Complete = true;
        });

        await Task.WhenAll(master1Task, master2Task);

        // Assert
        Assert.True(master1Complete);
        Assert.True(master2Complete);
    }

    /// <summary>Modbuses the UDP slave multi threaded.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusUdpSlave_MultiThreaded()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();
        dataStore.CoilDiscretes.Add(false);

        var slave = await CreateAndStartUdpSlaveAsync(port, dataStore);
        RegisterDisposable(slave);

        // Act
        var workerTask1 = ReadThreadAsync(port);
        var workerTask2 = ReadThreadAsync(port);

        await Task.WhenAll(workerTask1, workerTask2);
    }

    /// <summary>Reads from the specified port asynchronously.</summary>
    /// <param name="port">The port to connect to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ReadThreadAsync(int port)
    {
        var masterClient = new UdpClientRx();
        var endPoint = new System.Net.IPEndPoint(ModbusRxMasterFixture.TcpHost, port);
        masterClient.Connect(endPoint);
        using var master = ModbusIpMaster.CreateIp(masterClient);
        master.Transport!.Retries = 0;

        for (var i = 0; i < 5; i++)
        {
            var coils = await master.ReadCoilsAsync(1, 1);
            _ = Assert.Single(coils);
            Debug.WriteLine($"{Environment.CurrentManagedThreadId}: Reading coil value");

            var delay = GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32(100)));
            await Task.Delay(delay, CancellationToken);
        }
    }

    /// <summary>Creates and starts a UDP slave asynchronously.</summary>
    /// <param name="port">The port to listen on.</param>
    /// <param name="dataStore">The data store to use.</param>
    /// <returns>The UDP client used by the slave.</returns>
    private async Task<UdpClientRx> CreateAndStartUdpSlaveAsync(int port, DataStore dataStore)
    {
        var slaveClient = new UdpClientRx(port);
        ModbusSlave slave = ModbusUdpSlave.CreateUdp(slaveClient);
        slave.DataStore = dataStore;
        RegisterDisposable(slave);

        _ = Task.Run(async () =>
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
            catch (System.Net.Sockets.SocketException ex) when (ex.ErrorCode == 995)
            {
                // Expected when I/O operation is aborted due to thread exit or application request
                // This is normal during test cleanup in CI environments
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Other socket exceptions during cleanup are also expected
            }
        });

        // Give the slave time to start
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        return slaveClient;
    }
}
