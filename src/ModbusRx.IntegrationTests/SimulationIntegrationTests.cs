// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Integration tests for simulation and data generation.
/// </summary>
[Collection("SimulationTests")]
public sealed class SimulationIntegrationTests : IDisposable
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
    /// Tests that simulation generates realistic sine wave data.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_SineWaveGeneration_ShouldBeRealistic()
    {
        // Arrange
        const int length = 360; // One full cycle in degrees
        const double amplitude = 10000;

        // Act
        var sineData = SimulationDataProvider.GenerateSineWave(length, amplitude);

        // Assert
        Assert.Equal(length, sineData.Length);

        // Check that we have a proper sine wave
        Assert.Equal((ushort)amplitude, sineData[0]); // sin(0) = 0, shifted by amplitude
        Assert.True(sineData[90] > amplitude); // sin(90°) = 1, should be > amplitude
        Assert.Equal((ushort)amplitude, sineData[180]); // sin(180°) = 0, shifted by amplitude
        Assert.True(sineData[270] < amplitude); // sin(270°) = -1, should be < amplitude
    }

    /// <summary>
    /// Tests square wave generation with different duty cycles.
    /// </summary>
    /// <param name="dutyCycle">The duty cycle to test.</param>
    [Theory]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
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
        var actualHighCount = squareData.Count(x => x == highValue);

        // Allow for rounding differences
        Assert.True(Math.Abs(actualHighCount - expectedHighCount) <= 1);
    }

    /// <summary>
    /// Tests that simulation provider can run continuously.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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
        Assert.True(initialValues.Zip(finalValues, (initial, final) => initial != final).Any(changed => changed));
    }

    /// <summary>
    /// Tests different simulation types produce different patterns.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
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
        var values1 = dataStore1.HoldingRegisters.Take(10).ToArray();
        var values2 = dataStore2.HoldingRegisters.Take(10).ToArray();

        Assert.False(values1.SequenceEqual(values2));
    }

    /// <summary>
    /// Tests that server with simulation can handle real client connections.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ModbusServer_WithSimulation_ShouldServeRealtimeData()
    {
        // Arrange
        var server = new ModbusServer();
        _disposables.Add(server);

        var tcpPort = GetAvailablePort();
        server.StartTcpServer(tcpPort, 1);
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
        Assert.False(reading1.SequenceEqual(reading2));
    }

    /// <summary>
    /// Tests boolean pattern generation for coils and inputs.
    /// </summary>
    [Fact]
    public void SimulationDataProvider_BooleanPatterns_ShouldBeCorrect()
    {
        // Arrange
        using var provider = new SimulationDataProvider();
        const int length = 8;

        // Act & Assert - AllTrue pattern
        var allTrue = provider.GenerateBooleanPattern(length, BooleanPattern.AllTrue);
        Assert.All(allTrue, Assert.True);

        // Act & Assert - AllFalse pattern
        var allFalse = provider.GenerateBooleanPattern(length, BooleanPattern.AllFalse);
        Assert.All(allFalse, Assert.False);

        // Act & Assert - Alternating pattern
        var alternating = provider.GenerateBooleanPattern(length, BooleanPattern.Alternating);
        for (var i = 0; i < length; i++)
        {
            Assert.Equal(i % 2 == 0, alternating[i]);
        }

        // Act & Assert - Random pattern (should have variation)
        var random = provider.GenerateBooleanPattern(100, BooleanPattern.Random);
        Assert.True(random.Distinct().Count() > 1); // Should have both true and false
    }

    /// <summary>
    /// Tests that sawtooth wave increases linearly.
    /// </summary>
    [Fact]
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

    /// <summary>
    /// Tests comprehensive simulation with mixed data types.
    /// </summary>
    [Fact]
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
        Assert.True(dataStore.CoilDiscretes[2]);  // 1 % 2 == 1 -> true
        Assert.False(dataStore.CoilDiscretes[3]); // 2 % 2 == 0 -> false

        // Check inputs (inverted coils) - use 1-based indexing
        Assert.True(dataStore.InputDiscretes[1]);  // !false
        Assert.False(dataStore.InputDiscretes[2]); // !true
        Assert.True(dataStore.InputDiscretes[3]);  // !false
    }

    private static int GetAvailablePort()
    {
        using var socket = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        socket.Start();
        var port = ((System.Net.IPEndPoint)socket.LocalEndpoint).Port;
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
                try
                {
                    disposable?.Dispose();
                }
                catch
                {
                    // Ignore disposal exceptions in tests
                }
            }

            _disposables.Clear();
        }
    }
}
