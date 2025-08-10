// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Reactive;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Integration tests for the new ModbusServer functionality.
/// </summary>
public sealed class ModbusServerIntegrationTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    /// <summary>
    /// Disposes test resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tests that the ModbusServer can serve data over TCP.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_TcpCommunication_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        var testData = new ushort[] { 100, 200, 300, 400, 500 };
        server.LoadSimulationData(testData);

        var tcpPort = GetAvailablePort();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        // Give server time to start
        await Task.Delay(100);

        // Create client
        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        _disposables.Add(master);

        // Act
        var result = await master.ReadHoldingRegistersAsync(1, 0, 5);

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Equal(100, result[0]);
        Assert.Equal(500, result[4]);
    }

    /// <summary>
    /// Tests that the ModbusServer can serve data over UDP.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_UdpCommunication_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        var testCoils = new bool[] { true, false, true, false, true };
        server.LoadSimulationData(coils: testCoils);

        var udpPort = GetAvailablePort();
        server.StartUdpServer(udpPort, 1);
        server.Start();

        // Give server time to start
        await Task.Delay(100);

        // Create UDP client
        var client = new UdpClientRx();
        var endPoint = new IPEndPoint(IPAddress.Loopback, udpPort);
        client.Connect(endPoint);
        var master = ModbusIpMaster.CreateIp(client);
        _disposables.Add(master);

        // Act
        var result = await master.ReadCoilsAsync(1, 0, 5);

        // Assert
        Assert.Equal(5, result.Length);
        Assert.True(result[0]);
        Assert.False(result[1]);
        Assert.True(result[4]);
    }

    /// <summary>
    /// Tests that simulation mode generates changing data.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_SimulationMode_ShouldGenerateData()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        server.SimulationMode = true;
        server.Start();

        // Wait for simulation to run
        await Task.Delay(600);

        // Act
        var data1 = server.GetCurrentData();
        await Task.Delay(600);
        var data2 = server.GetCurrentData();

        // Assert - data should have changed
        Assert.True(data1.holdingRegisters.Zip(data2.holdingRegisters, (a, b) => a != b).Any(changed => changed));
    }

    /// <summary>
    /// Tests reactive data observation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_ReactiveObservation_ShouldEmitData()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        server.Start();
        server.SimulationMode = true;

        var dataReceived = false;

        // Act
        var subscription = server.ObserveDataChanges(100)
            .Take(1)
            .Subscribe(_ => dataReceived = true);
        _disposables.Add(subscription);

        await Task.Delay(200);

        // Assert
        Assert.True(dataReceived);
    }

    /// <summary>
    /// Tests that multiple clients can connect to the same server.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_MultipleClients_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        var testData = new ushort[] { 111, 222, 333 };
        server.LoadSimulationData(testData);

        var tcpPort = GetAvailablePort();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        await Task.Delay(100);

        // Create multiple clients
        var client1 = new TcpClientRx("127.0.0.1", tcpPort);
        var master1 = ModbusIpMaster.CreateIp(client1);
        _disposables.Add(master1);

        var client2 = new TcpClientRx("127.0.0.1", tcpPort);
        var master2 = ModbusIpMaster.CreateIp(client2);
        _disposables.Add(master2);

        // Act
        var result1 = await master1.ReadHoldingRegistersAsync(1, 0, 3);
        var result2 = await master2.ReadHoldingRegistersAsync(1, 0, 3);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(111, result1[0]);
        Assert.Equal(333, result1[2]);
    }

    /// <summary>
    /// Tests different simulation patterns.
    /// </summary>
    /// <param name="pattern">The test pattern to verify.</param>
    [Theory]
    [InlineData(TestPattern.CountingUp)]
    [InlineData(TestPattern.SineWave)]
    [InlineData(TestPattern.SquareWave)]
    [InlineData(TestPattern.Random)]
    public void ModbusServer_SimulationPatterns_ShouldLoadCorrectly(TestPattern pattern)
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        using var provider = new SimulationDataProvider();

        // Act
        provider.LoadTestPattern(server.DataStore!, pattern);
        var data = server.GetCurrentData();

        // Assert
        switch (pattern)
        {
            case TestPattern.CountingUp:
                Assert.Equal(0, data.holdingRegisters[0]);
                Assert.Equal(1, data.holdingRegisters[1]);
                Assert.Equal(2, data.holdingRegisters[2]);
                break;

            case TestPattern.SineWave:
            case TestPattern.SquareWave:
            case TestPattern.Random:
                // For these patterns, just verify data was loaded
                Assert.True(data.holdingRegisters.Take(10).Any(x => x > 0));
                break;
        }
    }

    /// <summary>
    /// Tests writing data to the server.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_WriteOperations_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        var tcpPort = GetAvailablePort();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        await Task.Delay(100);

        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        _disposables.Add(master);

        // Act - Write single register
        await master.WriteSingleRegisterAsync(1, 0, 12345);
        var readResult = await master.ReadHoldingRegistersAsync(1, 0, 1);

        // Assert
        Assert.Equal(12345, readResult[0]);

        // Act - Write multiple registers
        var writeData = new ushort[] { 1000, 2000, 3000 };
        await master.WriteMultipleRegistersAsync(1, 10, writeData);
        var multiReadResult = await master.ReadHoldingRegistersAsync(1, 10, 3);

        // Assert
        Assert.Equal(writeData, multiReadResult);
    }

    /// <summary>
    /// Tests server start/stop functionality.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_StartStop_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        // Act & Assert - Initially not running
        Assert.False(await server.IsRunning.FirstAsync());

        // Start server
        server.Start();
        Assert.True(await server.IsRunning.FirstAsync());

        // Stop server
        server.Stop();
        Assert.False(await server.IsRunning.FirstAsync());
    }

    /// <summary>
    /// Tests that server properly handles client aggregation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_ClientAggregation_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        // This test would require a mock Modbus device to connect to
        // For now, just verify the method doesn't throw

        // Act & Assert
        var subscription = server.AddTcpClient("test-client", "127.0.0.1", 10502, 1);
        _disposables.Add(subscription);

        Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>
    /// Tests performance under load.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_PerformanceTest_ShouldHandleMultipleRequests()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        server.LoadSimulationData(Enumerable.Range(0, 1000).Select(i => (ushort)i).ToArray());

        var tcpPort = GetAvailablePort();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        await Task.Delay(100);

        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        _disposables.Add(master);

        // Act - Perform multiple concurrent reads
        var tasks = new List<Task<ushort[]>>();
        for (var i = 0; i < 10; i++)
        {
            var startAddr = (ushort)(i * 10);
            tasks.Add(master.ReadHoldingRegistersAsync(1, startAddr, 10));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        foreach (var result in results)
        {
            Assert.Equal(10, result.Length);
        }
    }

    private static int GetAvailablePort()
    {
        using var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }

            _disposables.Clear();
        }
    }
}
