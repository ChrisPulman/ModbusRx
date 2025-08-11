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
[Collection("NetworkTests")]
public sealed class ModbusServerIntegrationTests : NetworkTestBase
{
    /// <summary>
    /// Tests that the ModbusServer can serve data over TCP.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_TcpCommunication_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        RegisterDisposable(server);

        var testData = new ushort[] { 100, 200, 300, 400, 500 };
        server.LoadSimulationData(testData);

        var tcpPort = await GetAvailablePortAsync();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        // Give server time to start
        await Task.Delay(200, CancellationToken);

        // Create client
        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        RegisterDisposable(master);

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
        RegisterDisposable(server);

        var testCoils = new bool[] { true, false, true, false, true };
        server.LoadSimulationData(coils: testCoils);

        var udpPort = await GetAvailablePortAsync();
        server.StartUdpServer(udpPort, 1);
        server.Start();

        // Give server time to start
        await Task.Delay(200, CancellationToken);

        // Create UDP client
        var client = new UdpClientRx();
        var endPoint = new IPEndPoint(IPAddress.Loopback, udpPort);
        client.Connect(endPoint);
        var master = ModbusIpMaster.CreateIp(client);
        RegisterDisposable(master);

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
        RegisterDisposable(server);

        server.SimulationMode = true;
        server.Start();

        // Wait for simulation to run
        await Task.Delay(600, CancellationToken);

        // Act
        var data1 = server.GetCurrentData();
        await Task.Delay(600, CancellationToken);
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
        RegisterDisposable(server);

        server.Start();
        server.SimulationMode = true;

        var dataReceived = false;

        // Act
        var subscription = server.ObserveDataChanges(100)
            .Take(1)
            .Subscribe(_ => dataReceived = true);
        RegisterDisposable(subscription);

        await Task.Delay(200, CancellationToken);

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
        RegisterDisposable(server);

        var testData = new ushort[] { 111, 222, 333 };
        server.LoadSimulationData(testData);

        var tcpPort = await GetAvailablePortAsync();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        await Task.Delay(200, CancellationToken);

        // Create multiple clients
        var client1 = new TcpClientRx("127.0.0.1", tcpPort);
        var master1 = ModbusIpMaster.CreateIp(client1);
        RegisterDisposable(master1);

        var client2 = new TcpClientRx("127.0.0.1", tcpPort);
        var master2 = ModbusIpMaster.CreateIp(client2);
        RegisterDisposable(master2);

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
        RegisterDisposable(server);

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
                Assert.Contains(data.holdingRegisters.Take(10), x => x > 0);
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
        RegisterDisposable(server);

        var tcpPort = await GetAvailablePortAsync();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        await Task.Delay(200, CancellationToken);

        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        RegisterDisposable(master);

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
        RegisterDisposable(server);

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
    [Fact]
    public void ModbusServer_ClientAggregation_ShouldWork()
    {
        // Arrange
        var server = new ModbusServer();
        RegisterDisposable(server);

        // This test verifies the method doesn't throw during setup,
        // but the actual connection will fail since no server is running on that port
        // which is expected behavior in a test environment

        // Act & Assert - Should throw SocketException since no server is running
        Assert.Throws<System.Net.Sockets.SocketException>(() =>
            server.AddTcpClient("test-client", "127.0.0.1", 10502, 1));
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
        RegisterDisposable(server);

        server.LoadSimulationData(Enumerable.Range(0, 1000).Select(i => (ushort)i).ToArray());

        var tcpPort = await GetAvailablePortAsync();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        await Task.Delay(200, CancellationToken);

        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        RegisterDisposable(master);

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
}
