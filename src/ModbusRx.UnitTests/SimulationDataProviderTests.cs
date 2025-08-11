// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using ModbusRx.Data;
using Xunit;

namespace ModbusRx.UnitTests;

/// <summary>
/// Unit tests for SimulationDataProvider.
/// </summary>
public class SimulationDataProviderTests
{
    /// <summary>
    /// Gets a value indicating whether the tests are running in CI environment.
    /// </summary>
    private static bool IsRunningInCI =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>
    /// Tests that SimulationDataProvider can be created and disposed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SimulationDataProvider_CreateAndDispose_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        using var provider = new SimulationDataProvider();
        Assert.NotNull(provider);
        var isRunning = await provider.IsRunning.FirstAsync().ToTask();
        Assert.False(isRunning);
    }

    /// <summary>
    /// Tests that simulation can be started and stopped.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SimulationDataProvider_StartAndStop_ShouldUpdateRunningState()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        // Act
        provider.Start(dataStore, TimeSpan.FromMilliseconds(100));

        // Assert
        var isRunning = await provider.IsRunning.FirstAsync().ToTask();
        Assert.True(isRunning);

        // Act
        provider.Stop();

        // Assert
        isRunning = await provider.IsRunning.FirstAsync().ToTask();
        Assert.False(isRunning);
    }

    /// <summary>
    /// Tests sine wave generation.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_GenerateSineWave_ShouldCreateValidData()
    {
        // Arrange
        const int length = 100;
        const double amplitude = 32767;

        // Act
        var data = SimulationDataProvider.GenerateSineWave(length, amplitude);

        // Assert
        Assert.Equal(length, data.Length);
        Assert.All(data, value => Assert.InRange(value, 0, 65535));

        // Should have some variation
        Assert.True(data.Distinct().Count() > 1);
    }

    /// <summary>
    /// Tests square wave generation.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_GenerateSquareWave_ShouldCreateValidData()
    {
        // Arrange
        const int length = 100;
        const ushort highValue = 65535;
        const ushort lowValue = 0;

        // Act
        var data = SimulationDataProvider.GenerateSquareWave(length, highValue, lowValue);

        // Assert
        Assert.Equal(length, data.Length);
        Assert.All(data, value => Assert.True(value == highValue || value == lowValue));

        // Should have both high and low values
        Assert.Contains(highValue, data);
        Assert.Contains(lowValue, data);
    }

    /// <summary>
    /// Tests sawtooth wave generation.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_GenerateSawtoothWave_ShouldCreateValidData()
    {
        // Arrange
        const int length = 100;
        const ushort maxValue = 1000;
        const ushort minValue = 0;

        // Act
        var data = SimulationDataProvider.GenerateSawtoothWave(length, maxValue, minValue);

        // Assert
        Assert.Equal(length, data.Length);
        Assert.Equal(minValue, data[0]);
        Assert.Equal(maxValue, data[^1]);

        // Should be monotonically increasing
        for (var i = 1; i < length; i++)
        {
            Assert.True(data[i] >= data[i - 1]);
        }
    }

    /// <summary>
    /// Tests random data generation.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_GenerateRandomData_ShouldCreateValidData()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        const int length = 100;
        const ushort minValue = 100;
        const ushort maxValue = 1000;

        // Act
        var data = provider.GenerateRandomData(length, minValue, maxValue);

        // Assert
        Assert.Equal(length, data.Length);
        Assert.All(data, value => Assert.InRange(value, minValue, maxValue));

        // Should have variation
        Assert.True(data.Distinct().Count() > 1);
    }

    /// <summary>
    /// Tests test pattern loading.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_LoadTestPattern_ShouldUpdateDataStore()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        // Act
        provider.LoadTestPattern(dataStore, TestPattern.CountingUp);

        // Assert - Check the actual data values, starting from index 1 in Modbus collections
        Assert.Equal(0, dataStore.HoldingRegisters[1]); // First real value
        Assert.Equal(1, dataStore.HoldingRegisters[2]); // Second real value
        Assert.Equal(2, dataStore.HoldingRegisters[3]); // Third real value
    }

    /// <summary>
    /// Tests continuous simulation updates data store.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_ContinuousSimulation_ShouldUpdateDataStore()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        // Capture initial state of first real register (index 1)
        var initialValue = dataStore.HoldingRegisters[1];

        // Act
        provider.Start(dataStore, TimeSpan.FromMilliseconds(50), SimulationType.Random);

        // Wait long enough for simulation to execute at least once
        // Simulation uses Observable.Interval so first execution happens after the interval
        var waitTime = GetEnvironmentTimeout(TimeSpan.FromMilliseconds(300)); // Wait much longer than 50ms interval
        Thread.Sleep(waitTime);
        provider.Stop();

        var finalValue = dataStore.HoldingRegisters[1];

        // Assert
        Assert.NotEqual(initialValue, finalValue);
    }

    /// <summary>
    /// Tests that different simulation types produce different patterns.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_DifferentSimulationTypes_ShouldProduceDifferentPatterns()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore1 = DataStoreFactory.CreateDefaultDataStore();
        var dataStore2 = DataStoreFactory.CreateDefaultDataStore();

        // Act
        provider.Start(dataStore1, TimeSpan.FromMilliseconds(50), SimulationType.CountingUp);

        // Wait long enough for simulation to execute at least once
        var waitTime = GetEnvironmentTimeout(TimeSpan.FromMilliseconds(200));
        Thread.Sleep(waitTime);
        provider.Stop();

        provider.Start(dataStore2, TimeSpan.FromMilliseconds(50), SimulationType.Random);
        Thread.Sleep(waitTime);
        provider.Stop();

        // Assert
        var values1 = dataStore1.HoldingRegisters.Take(10).ToArray();
        var values2 = dataStore2.HoldingRegisters.Take(10).ToArray();

        Assert.False(values1.SequenceEqual(values2));
    }

    /// <summary>
    /// Gets an appropriate timeout based on the environment.
    /// </summary>
    /// <param name="normalTimeout">Normal timeout for local testing.</param>
    /// <returns>Appropriate timeout for the environment.</returns>
    private static TimeSpan GetEnvironmentTimeout(TimeSpan normalTimeout) => IsRunningInCI ?
            TimeSpan.FromMilliseconds(normalTimeout.TotalMilliseconds * 0.6) : // Less aggressive reduction for simulation tests
            normalTimeout;
}
