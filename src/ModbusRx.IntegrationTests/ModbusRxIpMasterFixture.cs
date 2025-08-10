// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusIpMasterFixture.
/// </summary>
[Collection("NetworkTests")]
public class ModbusRxIpMasterFixture : NetworkTestBase
{
    /// <summary>
    /// Overrides the timeout on TCP client.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OverrideTimeoutOnTcpClient()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var listener = new TcpListener(ModbusRxMasterFixture.TcpHost, port);
        var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, listener);
        RegisterDisposable(slave);

        var startedEvent = new ManualResetEventSlim(false);
        var slaveTask = Task.Run(async () =>
        {
            try
            {
                startedEvent.Set();
                await slave.ListenAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (ObjectDisposedException)
            {
                // Expected when listener is disposed
            }
        });

        // Wait for slave to start with timeout
        var started = await WaitForConditionAsync(() => startedEvent.IsSet, TimeSpan.FromSeconds(5));
        Assert.True(started, "Slave failed to start within timeout");

        await Task.Delay(100, CancellationToken); // Give a bit more time for socket binding

        // Act & Assert
        using var client = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), port)
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
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OverrideTimeoutOnNetworkStream()
    {
        // Arrange
        var port = await GetAvailablePortAsync();
        var listener = new TcpListener(ModbusRxMasterFixture.TcpHost, port);
        var slave = ModbusTcpSlave.CreateTcp(ModbusRxMasterFixture.SlaveAddress, listener);
        RegisterDisposable(slave);

        var startedEvent = new ManualResetEventSlim(false);
        var slaveTask = Task.Run(async () =>
        {
            try
            {
                startedEvent.Set();
                await slave.ListenAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
            catch (ObjectDisposedException)
            {
                // Expected when listener is disposed
            }
        });

        // Wait for slave to start with timeout
        var started = await WaitForConditionAsync(() => startedEvent.IsSet, TimeSpan.FromSeconds(5));
        Assert.True(started, "Slave failed to start within timeout");

        await Task.Delay(100, CancellationToken); // Give a bit more time for socket binding

        // Act & Assert
        using var client = new TcpClientRx(ModbusRxMasterFixture.TcpHost.ToString(), port);
        client.Stream.ReadTimeout = 1500;
        client.Stream.WriteTimeout = 3000;

        using var master = ModbusIpMaster.CreateIp(client);
        Assert.Equal(1500, client.ReadTimeout);
        Assert.Equal(3000, client.WriteTimeout);
    }
}
