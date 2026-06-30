// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>Tests the ModbusIpMasterFixture behavior.</summary>
public class ModbusRxIpMasterFixture : NetworkTestBase
{
    /// <summary>Overrides the timeout on TCP client.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
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

    /// <summary>Overrides the timeout on network stream.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
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
            catch (SocketException ex) when (ex.ErrorCode == 995)
            {
                // Expected when I/O operation is aborted due to thread exit or application request
                // This is normal during test cleanup in CI environments
            }
            catch (SocketException)
            {
                // Other socket exceptions during cleanup are also expected
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
