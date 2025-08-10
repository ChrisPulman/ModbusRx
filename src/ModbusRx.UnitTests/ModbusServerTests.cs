// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using CP.IO.Ports;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Reactive;
using Xunit;

namespace ModbusRx.UnitTests;

/// <summary>
/// Unit tests for ModbusServer.
/// </summary>
public class ModbusServerTests
{
    /// <summary>
    /// Tests that ModbusServer can be created and disposed properly.
    /// </summary>
    [Fact]
    public void ModbusServer_CreateAndDispose_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        using var server = new ModbusServer();
        Assert.NotNull(server);
        Assert.False(server.IsRunning.FirstAsync().ToTask().Result);
    }

    /// <summary>
    /// Tests that ModbusServer can start and stop properly.
    /// </summary>
    [Fact]
    public void ModbusServer_StartAndStop_ShouldUpdateRunningState()
    {
        // Arrange
        using var server = new ModbusServer();

        // Act
        server.Start();

        // Assert
        Assert.True(server.IsRunning.FirstAsync().ToTask().Result);

        // Act
        server.Stop();

        // Assert
        Assert.False(server.IsRunning.FirstAsync().ToTask().Result);
    }

    /// <summary>
    /// Tests that simulation mode can be enabled and disabled.
    /// </summary>
    [Fact]
    public void ModbusServer_SimulationMode_ShouldUpdateDataStore()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();

        // Act
        server.SimulationMode = true;

        // Wait for simulation to run
        Thread.Sleep(600);

        var data = server.GetCurrentData();

        // Assert
        Assert.True(data.holdingRegisters.Any(x => x > 0));

        // Act
        server.SimulationMode = false;
    }

    /// <summary>
    /// Tests that custom data can be loaded into the server.
    /// </summary>
    [Fact]
    public void ModbusServer_LoadSimulationData_ShouldUpdateDataStore()
    {
        // Arrange
        using var server = new ModbusServer();
        var holdingRegs = new ushort[] { 1, 2, 3, 4, 5 };
        var inputRegs = new ushort[] { 10, 20, 30, 40, 50 };
        var coils = new bool[] { true, false, true, false, true };
        var inputs = new bool[] { false, true, false, true, false };

        // Act
        server.LoadSimulationData(holdingRegs, inputRegs, coils, inputs);
        var data = server.GetCurrentData();

        // Assert
        Assert.Equal(1, data.holdingRegisters[0]);
        Assert.Equal(2, data.holdingRegisters[1]);
        Assert.Equal(10, data.inputRegisters[0]);
        Assert.Equal(20, data.inputRegisters[1]);
        Assert.True(data.coils[0]);
        Assert.False(data.coils[1]);
        Assert.False(data.inputs[0]);
        Assert.True(data.inputs[1]);
    }

    /// <summary>
    /// Tests that TCP server can be started and configured.
    /// </summary>
    [Fact]
    public void ModbusServer_StartTcpServer_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();
        var port = GetAvailablePort();

        // Act
        var subscription = server.StartTcpServer(port, 1);

        // Assert
        Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>
    /// Tests that UDP server can be started and configured.
    /// </summary>
    [Fact]
    public void ModbusServer_StartUdpServer_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();
        var port = GetAvailablePort();

        // Act
        var subscription = server.StartUdpServer(port, 1);

        // Assert
        Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>
    /// Tests reactive server extensions.
    /// </summary>
    [Fact]
    public void ModbusServerExtensions_CreateReactiveServer_ShouldWork()
    {
        // Arrange
        var serverCreated = false;

        // Act
        using var subscription = ModbusServerExtensions.CreateReactiveServer(server =>
        {
            server.SimulationMode = true;
            serverCreated = true;
        }).Subscribe();

        // Assert
        Assert.True(serverCreated);
    }

    /// <summary>
    /// Tests data observation extensions.
    /// </summary>
    [Fact]
    public void ModbusServerExtensions_ObserveDataChanges_ShouldEmitData()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();
        server.SimulationMode = true;

        var dataReceived = false;

        // Act
        using var subscription = server.ObserveDataChanges(50)
            .Take(1)
            .Subscribe(_ => dataReceived = true);

        Thread.Sleep(100);

        // Assert
        Assert.True(dataReceived);
    }

    /// <summary>
    /// Tests holding register observation.
    /// </summary>
    [Fact]
    public void ModbusServerExtensions_ObserveHoldingRegisters_ShouldEmitChanges()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();

        var dataReceived = false;
        var expectedData = new ushort[] { 1, 2, 3, 4, 5 };

        // Act
        using var subscription = server.ObserveHoldingRegisters(0, 5, 50)
            .Take(1)
            .Subscribe(data =>
            {
                dataReceived = true;
            });

        server.LoadSimulationData(expectedData);
        Thread.Sleep(100);

        // Assert
        Assert.True(dataReceived);
    }

    /// <summary>
    /// Tests adding TCP client configuration.
    /// </summary>
    [Fact]
    public void ModbusServer_AddTcpClient_WithValidParameters_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();

        // Act & Assert
        var subscription = server.AddTcpClient("test", "127.0.0.1", 502, 1);
        Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>
    /// Tests adding UDP client configuration.
    /// </summary>
    [Fact]
    public void ModbusServer_AddUdpClient_WithValidParameters_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();

        // Act & Assert
        var subscription = server.AddUdpClient("test", "127.0.0.1", 502, 1);
        Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>
    /// Tests that invalid client names throw exceptions.
    /// </summary>
    /// <param name="name">The name.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ModbusServer_AddTcpClient_WithInvalidName_ShouldThrowException(string? name)
    {
        // Arrange
        using var server = new ModbusServer();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => server.AddTcpClient(name!, "127.0.0.1"));
    }

    /// <summary>
    /// Tests custom data store assignment.
    /// </summary>
    [Fact]
    public void ModbusServer_CustomDataStore_ShouldBeUsed()
    {
        // Arrange
        using var server = new ModbusServer();
        var customDataStore = DataStoreFactory.CreateTestDataStore();

        // Act
        server.DataStore = customDataStore;

        // Assert
        Assert.Equal(customDataStore, server.DataStore);
    }

    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
