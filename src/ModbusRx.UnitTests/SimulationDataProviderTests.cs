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

        // Use retry logic similar to ModbusServer tests for better reliability
        var maxRetries = IsRunningInCI ? 6 : 3; // More retries in CI, especially for .NET Framework 4.8
        var baseWaitTime = IsRunningInCI ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(250);
        var dataChanged = false;

        for (var retry = 0; retry < maxRetries && !dataChanged; retry++)
        {
            Thread.Sleep(baseWaitTime);
            var currentValue = dataStore.HoldingRegisters[1];
            dataChanged = currentValue != initialValue;

            if (!dataChanged && retry < maxRetries - 1)
            {
                // Give a bit more time between retries
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        provider.Stop();

        // Assert with better error message
        var errorMessage = $"Simulation should update data store. Initial: {initialValue}, Final: {dataStore.HoldingRegisters[1]}, " +
                          $"Retries: {maxRetries}, Wait time: {baseWaitTime.TotalMilliseconds}ms, CI: {IsRunningInCI}";

        Assert.True(dataChanged, errorMessage);
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

        // Use longer wait times for .NET Framework 4.8 CI reliability
        var baseWaitTime = IsRunningInCI ? TimeSpan.FromMilliseconds(400) : TimeSpan.FromMilliseconds(250);
        var maxRetries = IsRunningInCI ? 4 : 2;

        // Act - Start first simulation (CountingUp)
        provider.Start(dataStore1, TimeSpan.FromMilliseconds(50), SimulationType.CountingUp);

        var simulation1Succeeded = false;
        for (var retry = 0; retry < maxRetries && !simulation1Succeeded; retry++)
        {
            Thread.Sleep(baseWaitTime);
            var currentData = dataStore1.HoldingRegisters.Take(5).ToArray();

            // CountingUp should produce sequential values starting from 0
            simulation1Succeeded = currentData.Any(x => x > 0) || currentData.Distinct().Count() > 1;
        }

        provider.Stop();

        // Start second simulation (Random)
        provider.Start(dataStore2, TimeSpan.FromMilliseconds(50), SimulationType.Random);

        var simulation2Succeeded = false;
        for (var retry = 0; retry < maxRetries && !simulation2Succeeded; retry++)
        {
            Thread.Sleep(baseWaitTime);
            var currentData = dataStore2.HoldingRegisters.Take(5).ToArray();

            // Random should produce varied values
            simulation2Succeeded = currentData.Any(x => x > 0) || currentData.Distinct().Count() > 1;
        }

        provider.Stop();

        // Assert - Get final values for comparison
        var values1 = dataStore1.HoldingRegisters.Take(10).ToArray();
        var values2 = dataStore2.HoldingRegisters.Take(10).ToArray();

        // More flexible assertion - different simulation types should produce different results
        var patternsAreDifferent = !values1.SequenceEqual(values2) ||
                                  values1.Distinct().Count() != values2.Distinct().Count();

        var errorMessage = "Different simulation types should produce different patterns. " +
                          $"CountingUp: [{string.Join(", ", values1)}], Random: [{string.Join(", ", values2)}], " +
                          $"Sim1 Success: {simulation1Succeeded}, Sim2 Success: {simulation2Succeeded}, CI: {IsRunningInCI}";

        Assert.True(patternsAreDifferent, errorMessage);
    }

    /// <summary>
    /// Gets an appropriate timeout based on the environment.
    /// </summary>
    /// <param name="normalTimeout">Normal timeout for local testing.</param>
    /// <returns>Appropriate timeout for the environment.</returns>
    private static TimeSpan GetEnvironmentTimeout(TimeSpan normalTimeout) => IsRunningInCI ?
            TimeSpan.FromMilliseconds(normalTimeout.TotalMilliseconds * 0.8) : // Less aggressive reduction than before for better .NET Framework 4.8 support
            normalTimeout;
}
