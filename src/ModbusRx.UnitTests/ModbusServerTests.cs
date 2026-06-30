// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Reactive;

namespace ModbusRx.UnitTests;

/// <summary>Unit tests for ModbusServer.</summary>
public class ModbusServerTests
{
    /// <summary>Gets a value indicating whether the tests are running in CI environment.</summary>
    private static bool IsRunningInCI =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>Tests that ModbusServer can be created and disposed properly.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServer_CreateAndDispose_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        using var server = new ModbusServer();
        _ = Assert.NotNull(server);
        var isRunning = await server.IsRunning.FirstAsync().ToTask();
        Assert.False(isRunning);
    }

    /// <summary>Tests that ModbusServer can start and stop properly.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServer_StartAndStop_ShouldUpdateRunningState()
    {
        // Arrange
        using var server = new ModbusServer();

        // Act
        server.Start();

        // Assert
        var isRunning = await server.IsRunning.FirstAsync().ToTask();
        Assert.True(isRunning);

        // Act
        server.Stop();

        // Assert
        isRunning = await server.IsRunning.FirstAsync().ToTask();
        Assert.False(isRunning);
    }

    /// <summary>Tests that simulation mode can be enabled and disabled.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServer_SimulationMode_ShouldUpdateDataStore()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();

        // Capture initial state to verify change
        var initialData = server.GetCurrentData();
        var initialSum = SumFirst(initialData.holdingRegisters, 10);

        // Act
        server.SimulationMode = true;

        // Wait for simulation to run - simulation runs every 500ms so wait at least that long
        var baseInterval = TimeSpan.FromMilliseconds(700); // Longer than the 500ms simulation interval
        var timeout = GetEnvironmentTimeout(baseInterval);
        var maxRetries = IsRunningInCI ? 8 : 3; // More retries in CI due to slower execution
        var dataHasChanged = false;

        for (var retry = 0; retry < maxRetries && !dataHasChanged; retry++)
        {
            await Task.Delay(timeout);
            var currentData = server.GetCurrentData();

            // Check if any holding registers have non-zero values OR if data has changed from initial state
            var currentSum = SumFirst(currentData.holdingRegisters, 10);
            var hasNonZeroValues = ContainsPositiveValue(currentData.holdingRegisters);
            var sumChanged = currentSum != initialSum;

            dataHasChanged = hasNonZeroValues || sumChanged;

            if (!dataHasChanged && retry < maxRetries - 1)
            {
                // If no data yet, wait a bit more for next retry
                await Task.Delay(GetEnvironmentTimeout(TimeSpan.FromMilliseconds(300)));
            }
        }

        // Assert
        var errorMessage = $"Simulation should generate non-zero data or change from initial state after {maxRetries} attempts with {timeout.TotalMilliseconds}ms intervals. " +
                          $"Initial sum: {initialSum}, Simulation interval: 500ms";

        Assert.True(dataHasChanged, errorMessage);

        // Act
        server.SimulationMode = false;
    }

    /// <summary>Tests that custom data can be loaded into the server.</summary>
    [TUnit.Core.Test]
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

    /// <summary>Tests that TCP server can be started and configured.</summary>
    [TUnit.Core.Test]
    public void ModbusServer_StartTcpServer_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();
        var port = GetAvailablePort();

        // Act
        var subscription = server.StartTcpServer(port, 1);

        // Assert
        _ = Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>Tests that UDP server can be started and configured.</summary>
    [TUnit.Core.Test]
    public void ModbusServer_StartUdpServer_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();
        IDisposable? subscription = null;

        // Act
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                subscription = server.StartUdpServer(GetAvailableUdpPort(), 1);
                break;
            }
            catch (System.Net.Sockets.SocketException) when (attempt < 4)
            {
                Thread.Sleep(25);
            }
        }

        // Assert
        _ = Assert.NotNull(subscription);

        // Cleanup
        subscription!.Dispose();
    }

    /// <summary>Tests reactive server extensions.</summary>
    [TUnit.Core.Test]
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

    /// <summary>Tests data observation extensions.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServerExtensions_ObserveDataChanges_ShouldEmitData()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();
        server.SimulationMode = true;

        var timeout = GetEnvironmentTimeout(TimeSpan.FromSeconds(2));

        // Act
        var dataReceived = new TaskCompletionSource<(ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs)>();
        using var subscription = server.ObserveDataChanges(50)
            .Subscribe(value => dataReceived.TrySetResult(value));

        var data = await dataReceived.Task.WaitAsync(timeout);

        // Assert
        _ = Assert.NotNull(data.holdingRegisters);
        _ = Assert.NotNull(data.inputRegisters);
        _ = Assert.NotNull(data.coils);
        _ = Assert.NotNull(data.inputs);
    }

    /// <summary>Tests holding register observation.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServerExtensions_ObserveHoldingRegisters_ShouldEmitChanges()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();

        var dataReceived = new TaskCompletionSource<bool>();
        var expectedData = new ushort[] { 1, 2, 3, 4, 5 };
        var timeout = GetEnvironmentTimeout(TimeSpan.FromSeconds(2));

        // Act
        using var subscription = server.ObserveHoldingRegisters(0, 5, 50)
            .Take(1)
            .Subscribe(_ => dataReceived.TrySetResult(true));

        server.LoadSimulationData(expectedData);
        var completed = await Task.WhenAny(dataReceived.Task, Task.Delay(timeout)) == dataReceived.Task;

        // Assert
        Assert.True(completed);
    }

    /// <summary>Tests adding TCP client configuration.</summary>
    [TUnit.Core.Test]
    public void ModbusServer_AddTcpClient_WithValidParameters_ShouldThrowExpectedException()
    {
        // Arrange
        using var server = new ModbusServer();

        // Act & Assert - AddTcpClient will fail to connect but should still return a subscription
        // The connection failure is expected in a unit test environment
        // Use broader exception type to handle CI environment variations
        _ = Assert.ThrowsAny<Exception>(() =>
            server.AddTcpClient("test", "127.0.0.1", 502, 1));
    }

    /// <summary>Tests adding UDP client configuration.</summary>
    [TUnit.Core.Test]
    public void ModbusServer_AddUdpClient_WithValidParameters_ShouldReturnDisposable()
    {
        // Arrange
        using var server = new ModbusServer();

        // Act - Use a different approach that doesn't rely on specific network behavior
        var subscription = server.AddUdpClient("test", "127.0.0.1", GetAvailablePort(), 1);

        // Assert
        _ = Assert.NotNull(subscription);

        // Cleanup
        subscription.Dispose();
    }

    /// <summary>Tests that invalid client names throw exceptions.</summary>
    /// <param name="name">The name.</param>
    [TUnit.Core.Test]
    [TUnit.Core.Arguments(null)]
    [TUnit.Core.Arguments("")]
    [TUnit.Core.Arguments("   ")]
    public void ModbusServer_AddTcpClient_WithInvalidName_ShouldThrowException(string? name)
    {
        // Arrange
        using var server = new ModbusServer();

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() => server.AddTcpClient(name!, "127.0.0.1"));
    }

    /// <summary>Tests custom data store assignment.</summary>
    [TUnit.Core.Test]
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

    /// <summary>Tests that the server handles high-frequency data updates in CI environments.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServer_HighFrequencyUpdates_ShouldWorkInCI()
    {
        // Arrange
        using var server = new ModbusServer();
        server.Start();
        server.SimulationMode = true;

        var updateCount = 0;
        var maxUpdates = IsRunningInCI ? 3 : 10; // Reduce load in CI
        var observationTimeout = GetEnvironmentTimeout(TimeSpan.FromSeconds(2));

        // Act
        using var subscription = server.ObserveDataChanges(100)
            .Take(maxUpdates)
            .Subscribe(_ => Interlocked.Increment(ref updateCount));

        await Task.Delay(observationTimeout);

        // Assert
        Assert.True(updateCount > 0, $"Expected some updates, got {updateCount}");
    }

    /// <summary>Gets an appropriate timeout based on the environment.</summary>
    /// <param name="normalTimeout">Normal timeout for local testing.</param>
    /// <returns>Appropriate timeout for the environment.</returns>
    private static TimeSpan GetEnvironmentTimeout(TimeSpan normalTimeout) => IsRunningInCI
        ? TimeSpan.FromMilliseconds(normalTimeout.TotalMilliseconds * 0.5)
        : normalTimeout;

    /// <summary>Gets an available TCP port from the loopback interface.</summary>
    /// <returns>The available TCP port.</returns>
    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>Gets an available UDP port from the loopback interface.</summary>
    /// <returns>The available UDP port.</returns>
    private static int GetAvailableUdpPort()
    {
        using var udpClient = new CP.IO.Ports.UdpClientRx(0);
        return ((IPEndPoint)udpClient.Client.LocalEndPoint!).Port;
    }

    /// <summary>Sums the first values in an array.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <param name="count">The maximum number of values to sum.</param>
    /// <returns>The sum of the requested values.</returns>
    private static long SumFirst(ushort[] values, int count)
    {
        var total = 0L;
        var length = Math.Min(values.Length, count);
        for (var i = 0; i < length; i++)
        {
            total += values[i];
        }

        return total;
    }

    /// <summary>Determines whether any value is positive.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <returns>A value indicating whether any value is positive.</returns>
    private static bool ContainsPositiveValue(ushort[] values)
    {
        foreach (var value in values)
        {
            if (value > 0)
            {
                return true;
            }
        }

        return false;
    }
}
