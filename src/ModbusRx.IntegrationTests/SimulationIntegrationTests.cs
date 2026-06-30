// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>Integration tests for simulation and data generation.</summary>
public sealed class SimulationIntegrationTests : IDisposable
{
    /// <summary>The resources registered for cleanup.</summary>
    private readonly List<IDisposable> _disposables = [];

    /// <summary>Disposes test resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Tests that simulation generates realistic sine wave data.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_SineWaveGeneration_ShouldBeRealistic()
    {
        // Arrange
        const int length = 360; // One full cycle in degrees
        const double amplitude = 10_000;

        // Act
        var sineData = SimulationDataProvider.GenerateSineWave(length, amplitude);

        // Assert
        Assert.Equal(length, sineData.Length);

        // Check that we have a proper sine wave
        Assert.Equal((ushort)amplitude, sineData[0]); // sin(0) = 0, shifted by amplitude
        Assert.True(sineData[90] > amplitude); // sin(90 degrees) = 1, should be > amplitude
        Assert.Equal((ushort)amplitude, sineData[180]); // sin(180 degrees) = 0, shifted by amplitude
        Assert.True(sineData[270] < amplitude); // sin(270 degrees) = -1, should be < amplitude
    }

    /// <summary>Tests square wave generation with different duty cycles.</summary>
    /// <param name="dutyCycle">The duty cycle to test.</param>
    [TUnit.Core.Test]
    [TUnit.Core.Arguments(0.25)]
    [TUnit.Core.Arguments(0.5)]
    [TUnit.Core.Arguments(0.75)]
    public void SimulationDataProvider_SquareWaveGeneration_ShouldRespectDutyCycle(double dutyCycle)
    {
        // Arrange
        const int length = 100;
        const ushort highValue = 1000;
        const ushort lowValue = 0;

        // Act
        var squareData = SimulationDataProvider.GenerateSquareWave(length, highValue, lowValue, dutyCycle);

        // Assert
        var expectedHighCount = (int)(length * dutyCycle);
        var actualHighCount = CountEqual(squareData, highValue);

        // Allow for rounding differences
        Assert.True(Math.Abs(actualHighCount - expectedHighCount) <= 1);
    }

    /// <summary>Tests that simulation provider can run continuously.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task SimulationDataProvider_ContinuousSimulation_ShouldUpdateData()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        // Capture initial values from valid Modbus addresses (starting at index 1)
        var initialValues = new[]
        {
            dataStore.HoldingRegisters[1],
            dataStore.HoldingRegisters[2],
            dataStore.HoldingRegisters[3]
        };

        // Act
        provider.Start(dataStore, TimeSpan.FromMilliseconds(50), SimulationType.Random);
        await Task.Delay(200); // Let it run for several update cycles
        provider.Stop();

        var finalValues = new[]
        {
            dataStore.HoldingRegisters[1],
            dataStore.HoldingRegisters[2],
            dataStore.HoldingRegisters[3]
        };

        // Assert
        Assert.True(HasAnyChange(initialValues, finalValues));
    }

    /// <summary>Tests different simulation types produce different patterns.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task SimulationDataProvider_DifferentTypes_ShouldProduceDifferentPatterns()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore1 = DataStoreFactory.CreateDefaultDataStore();
        var dataStore2 = DataStoreFactory.CreateDefaultDataStore();

        // Act - Run different simulation types
        provider.Start(dataStore1, TimeSpan.FromMilliseconds(50), SimulationType.CountingUp);
        await Task.Delay(100);
        provider.Stop();

        provider.Start(dataStore2, TimeSpan.FromMilliseconds(50), SimulationType.SineWave);
        await Task.Delay(100);
        provider.Stop();

        // Assert - Should produce different patterns
        var values1 = CopyFirst(dataStore1.HoldingRegisters, 10);
        var values2 = CopyFirst(dataStore2.HoldingRegisters, 10);

        Assert.False(SequenceEqual(values1, values2));
    }

    /// <summary>Tests that server with simulation can handle real client connections.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task ModbusServer_WithSimulation_ShouldServeRealtimeData()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        var tcpPort = GetAvailablePort();
        _ = server.StartTcpServer(tcpPort, 1);
        server.SimulationMode = true;
        server.Start();

        await Task.Delay(200);

        var client = new CP.IO.Ports.TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        _disposables.Add(master);

        // Act - Read data multiple times to see changes
        var reading1 = await master.ReadHoldingRegistersAsync(1, 0, 10);
        await Task.Delay(600); // Wait for simulation to update
        var reading2 = await master.ReadHoldingRegistersAsync(1, 0, 10);

        // Assert - Data should have changed due to simulation
        Assert.False(SequenceEqual(reading1, reading2));
    }

    /// <summary>Tests boolean pattern generation for coils and inputs.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_BooleanPatterns_ShouldBeCorrect()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        const int length = 8;

        // Act & Assert - AllTrue pattern
        var allTrue = provider.GenerateBooleanPattern(length, BooleanPattern.AllTrue);
        Assert.All(allTrue, value => Assert.True(value));

        // Act & Assert - AllFalse pattern
        var allFalse = provider.GenerateBooleanPattern(length, BooleanPattern.AllFalse);
        Assert.All(allFalse, value => Assert.False(value));

        // Act & Assert - Alternating pattern
        var alternating = provider.GenerateBooleanPattern(length, BooleanPattern.Alternating);
        for (var i = 0; i < length; i++)
        {
            Assert.Equal(i % 2 == 0, alternating[i]);
        }

        // Act & Assert - Random pattern (should have variation)
        var random = provider.GenerateBooleanPattern(100, BooleanPattern.Random);
        Assert.True(HasVariation(random)); // Should have both true and false
    }

    /// <summary>Tests that sawtooth wave increases linearly.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_SawtoothWave_ShouldIncreaseLinearly()
    {
        // Arrange
        const int length = 10;
        const ushort minValue = 0;
        const ushort maxValue = 900; // Use 900 to avoid overflow in calculations

        // Act
        var sawtoothData = SimulationDataProvider.GenerateSawtoothWave(length, maxValue, minValue);

        // Assert
        Assert.Equal(length, sawtoothData.Length);
        Assert.Equal(minValue, sawtoothData[0]);
        Assert.Equal(maxValue, sawtoothData[^1]);

        // Check that values increase
        for (var i = 1; i < length; i++)
        {
            Assert.True(sawtoothData[i] >= sawtoothData[i - 1]);
        }
    }

    /// <summary>Tests comprehensive simulation with mixed data types.</summary>
    [TUnit.Core.Test]
    public void SimulationDataProvider_MixedDataTypes_ShouldLoadCorrectly()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        var dataStore = DataStoreFactory.CreateDefaultDataStore();

        // Act - Load different patterns
        provider.LoadTestPattern(dataStore, TestPattern.CountingUp);

        // Check holding registers (counting up) - use 1-based indexing
        Assert.Equal(0, dataStore.HoldingRegisters[1]); // First real value
        Assert.Equal(1, dataStore.HoldingRegisters[2]); // Second real value
        Assert.Equal(2, dataStore.HoldingRegisters[3]); // Third real value

        // Check that input registers match
        Assert.Equal(dataStore.HoldingRegisters[1], dataStore.InputRegisters[1]);
        Assert.Equal(dataStore.HoldingRegisters[2], dataStore.InputRegisters[2]);

        // Check coils (should reflect register values) - use 1-based indexing
        Assert.False(dataStore.CoilDiscretes[1]); // 0 % 2 == 0 -> false
        Assert.True(dataStore.CoilDiscretes[2]); // 1 % 2 == 1 -> true
        Assert.False(dataStore.CoilDiscretes[3]); // 2 % 2 == 0 -> false

        // Check inputs (inverted coils) - use 1-based indexing
        Assert.True(dataStore.InputDiscretes[1]); // !false
        Assert.False(dataStore.InputDiscretes[2]); // !true
        Assert.True(dataStore.InputDiscretes[3]); // !false
    }

    /// <summary>Copies the first values from the specified collection.</summary>
    /// <param name="values">The values to copy.</param>
    /// <param name="count">The maximum number of values to copy.</param>
    /// <returns>The copied values.</returns>
    private static ushort[] CopyFirst(IEnumerable<ushort> values, int count)
    {
        var result = new List<ushort>(count);

        foreach (var value in values)
        {
            if (result.Count >= count)
            {
                break;
            }

            result.Add(value);
        }

        return [.. result];
    }

    /// <summary>Counts values that match the expected value.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <param name="expected">The value to count.</param>
    /// <returns>The number of matching values.</returns>
    private static int CountEqual(ushort[] values, ushort expected)
    {
        var count = 0;

        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] == expected)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Gets an available TCP port on the loopback adapter.</summary>
    /// <returns>The available port number.</returns>
    private static int GetAvailablePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }

    /// <summary>Determines whether any values differ between two arrays.</summary>
    /// <param name="initialValues">The initial values.</param>
    /// <param name="finalValues">The final values.</param>
    /// <returns><c>true</c> when any value has changed; otherwise, <c>false</c>.</returns>
    private static bool HasAnyChange(ushort[] initialValues, ushort[] finalValues)
    {
        var length = Math.Min(initialValues.Length, finalValues.Length);

        for (var i = 0; i < length; i++)
        {
            if (initialValues[i] != finalValues[i])
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether the boolean values contain more than one value.</summary>
    /// <param name="values">The values to inspect.</param>
    /// <returns><c>true</c> when the sequence contains both <c>true</c> and <c>false</c>; otherwise, <c>false</c>.</returns>
    private static bool HasVariation(bool[] values)
    {
        if (values.Length == 0)
        {
            return false;
        }

        var first = values[0];

        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] != first)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Determines whether two arrays contain the same values in order.</summary>
    /// <param name="left">The left values.</param>
    /// <param name="right">The right values.</param>
    /// <returns><c>true</c> when both arrays contain the same values; otherwise, <c>false</c>.</returns>
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

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        foreach (var disposable in _disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Expected during cleanup.
            }
            catch (InvalidOperationException)
            {
                // Expected during cleanup.
            }
            catch (SocketException)
            {
                // Expected during cleanup.
            }
        }

        _disposables.Clear();
    }
}
