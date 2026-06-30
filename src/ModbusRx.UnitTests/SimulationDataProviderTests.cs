// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ModbusRx.Data;

namespace ModbusRx.UnitTests;

/// <summary>Unit tests for SimulationDataProvider.</summary>
public class SimulationDataProviderTests
{
    /// <summary>Gets a value indicating whether the tests are running in CI environment.</summary>
    private static bool IsRunningInCI =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>Tests that SimulationDataProvider can be created and disposed.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task SimulationDataProvider_CreateAndDispose_ShouldNotThrow()
    {
        // Arrange & Act & Assert
        using var provider = new SimulationDataProvider();
        _ = Assert.NotNull(provider);
        var isRunning = await provider.IsRunning.FirstAsync().ToTask();
        Assert.False(isRunning);
    }

    /// <summary>Tests that simulation can be started and stopped.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
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

    /// <summary>Tests sine wave generation.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_GenerateSineWave_ShouldCreateValidData()
    {
        // Arrange
        const int length = 100;
        const double amplitude = 32_767;

        // Act
        var data = SimulationDataProvider.GenerateSineWave(length, amplitude);

        // Assert
        Assert.Equal(length, data.Length);
        Assert.All(data, value => Assert.InRange(value, 0, 65_535));

        // Should have some variation
        Assert.True(HasVariation(data));
    }

    /// <summary>Tests square wave generation.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_GenerateSquareWave_ShouldCreateValidData()
    {
        // Arrange
        const int length = 100;
        const ushort highValue = 65_535;
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

    /// <summary>Tests sawtooth wave generation.</summary>
    [TUnit.Core.Test]
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

    /// <summary>Tests random data generation.</summary>
    [TUnit.Core.Test]
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
        Assert.True(HasVariation(data));
    }

    /// <summary>Tests test pattern loading.</summary>
    [TUnit.Core.Test]
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

    /// <summary>Tests continuous simulation updates data store.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_ContinuousSimulation_ShouldUpdateDataStore()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        // Capture initial state of first real register (index 1)
        var initialValue = dataStore.HoldingRegisters[1];

        // Act
        provider.Start(dataStore, TimeSpan.FromMilliseconds(50), SimulationType.CountingUp);

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

    /// <summary>Tests that different simulation types produce different patterns.</summary>
    [TUnit.Core.Test]
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
            var currentData = CopyFirst(dataStore1.HoldingRegisters, 5);

            // CountingUp should produce sequential values starting from 0
            simulation1Succeeded = ContainsPositiveValue(currentData) || HasVariation(currentData);
        }

        provider.Stop();

        // Start second simulation (Random)
        provider.Start(dataStore2, TimeSpan.FromMilliseconds(50), SimulationType.Random);

        var simulation2Succeeded = false;
        for (var retry = 0; retry < maxRetries && !simulation2Succeeded; retry++)
        {
            Thread.Sleep(baseWaitTime);
            var currentData = CopyFirst(dataStore2.HoldingRegisters, 5);

            // Random should produce varied values
            simulation2Succeeded = ContainsPositiveValue(currentData) || HasVariation(currentData);
        }

        provider.Stop();

        // Assert - Get final values for comparison
        var values1 = CopyFirst(dataStore1.HoldingRegisters, 10);
        var values2 = CopyFirst(dataStore2.HoldingRegisters, 10);

        // More flexible assertion - different simulation types should produce different results
        var patternsAreDifferent = !SequenceEqual(values1, values2) ||
                                  CountDistinct(values1) != CountDistinct(values2);

        var errorMessage = "Different simulation types should produce different patterns. " +
                          $"CountingUp: [{FormatValues(values1)}], Random: [{FormatValues(values2)}], " +
                          $"Sim1 Success: {simulation1Succeeded}, Sim2 Success: {simulation2Succeeded}, CI: {IsRunningInCI}";

        Assert.True(patternsAreDifferent, errorMessage);
    }

    /// <summary>Copies the first values from a sequence.</summary>
    /// <param name="values">The source values.</param>
    /// <param name="count">The maximum number of values to copy.</param>
    /// <returns>The copied values.</returns>
    private static ushort[] CopyFirst(IEnumerable<ushort> values, int count)
    {
        var result = new List<ushort>(count);
        foreach (var value in values)
        {
            if (result.Count == count)
            {
                break;
            }

            result.Add(value);
        }

        return result.ToArray();
    }

    /// <summary>Determines whether values contain more than one distinct value.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <returns>A value indicating whether variation exists.</returns>
    private static bool HasVariation(IReadOnlyList<ushort> values) => CountDistinct(values) > 1;

    /// <summary>Determines whether values contain a positive value.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <returns>A value indicating whether any value is positive.</returns>
    private static bool ContainsPositiveValue(IEnumerable<ushort> values)
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

    /// <summary>Counts distinct values with explicit iteration.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <returns>The number of distinct values.</returns>
    private static int CountDistinct(IEnumerable<ushort> values)
    {
        var distinct = new HashSet<ushort>();
        foreach (var value in values)
        {
            _ = distinct.Add(value);
        }

        return distinct.Count;
    }

    /// <summary>Compares two value arrays.</summary>
    /// <param name="left">The left values.</param>
    /// <param name="right">The right values.</param>
    /// <returns>A value indicating whether both arrays contain the same values.</returns>
    private static bool SequenceEqual(ushort[] left, ushort[] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Formats values for assertion output.</summary>
    /// <param name="values">The values to format.</param>
    /// <returns>The formatted values.</returns>
    private static string FormatValues(ushort[] values)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                _ = builder.Append(", ");
            }

            _ = builder.Append(values[i]);
        }

        return builder.ToString();
    }
}
