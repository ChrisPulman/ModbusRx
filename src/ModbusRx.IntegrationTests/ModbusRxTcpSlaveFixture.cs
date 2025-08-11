// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusTcpSlaveFixture.
/// </summary>
[Collection("NetworkTests")]
public class ModbusRxTcpSlaveFixture : NetworkTestBase
{
    /// <summary>
    /// Tests possible exception when master closes gracefully immediately after transaction
    /// The goal is the test the exception in WriteCompleted when the slave attempts to read another request from an already closed master.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusTcpSlave_ConnectionClosesGracefully()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var slaveListener = new TcpListener(ModbusRxMasterFixture.TcpHost, port);
        var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, slaveListener);
        RegisterDisposable(slave);

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

        // Wait for slave to start
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        // Act
        var masterClient = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), port);
        using (var master = ModbusIpMaster.CreateIp(masterClient))
        {
            master.Transport!.Retries = 0;

            var coils = await master.ReadCoilsAsync(1, 1);

            Assert.Single(coils);
            Assert.Single(slave.Masters);
        }

        // Give the slave some time to remove the master
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        Assert.Empty(slave.Masters);
    }

    /// <summary>
    /// Tests possible exception when master closes gracefully and the ReadHeaderCompleted EndRead call returns 0 bytes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusTcpSlave_ConnectionSlowlyClosesGracefully()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var slaveListener = new TcpListener(ModbusRxMasterFixture.TcpHost, port);
        var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, slaveListener);
        RegisterDisposable(slave);

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

        // Wait for slave to start
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        // Act
        var masterClient = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), port);
        using (var master = ModbusIpMaster.CreateIp(masterClient))
        {
            master.Transport!.Retries = 0;

            var coils = await master.ReadCoilsAsync(1, 1);
            Assert.Single(coils);

            Assert.Single(slave.Masters);

            // Wait a bit to let slave move on to read header
            await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);
        }

        // Give the slave some time to remove the master
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);
        Assert.Empty(slave.Masters);
    }

    /// <summary>
    /// Modbuses the TCP slave multi threaded.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusTcpSlave_MultiThreaded()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var slaveListener = new TcpListener(ModbusRxMasterFixture.TcpHost, port);
        var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, slaveListener);
        RegisterDisposable(slave);

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

        // Wait for slave to start
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(100)), CancellationToken);

        // Act
        var workerTask1 = ReadAsync(port);
        var workerTask2 = ReadAsync(port);

        await Task.WhenAll(workerTask1, workerTask2);
    }

    /// <summary>
    /// Reads from the specified port asynchronously.
    /// </summary>
    /// <param name="port">The port to connect to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ReadAsync(int port)
    {
        var masterClient = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), port);
        using var master = ModbusIpMaster.CreateIp(masterClient);
        master.Transport!.Retries = 0;

        var random = new Random();
        for (var i = 0; i < 5; i++)
        {
            var coils = await master.ReadCoilsAsync(1, 1);
            Assert.Single(coils);
            Debug.WriteLine($"{Environment.CurrentManagedThreadId}: Reading coil value");

            var delay = GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(random.Next(100)));
            await Task.Delay(delay, CancellationToken);
        }
    }
}
