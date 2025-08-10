// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ModbusRx.Data;

/// <summary>
/// Provides simulation data for Modbus testing and development.
/// </summary>
public sealed class SimulationDataProvider : IDisposable
{
    private readonly Random _random = new();
    private readonly BehaviorSubject<bool> _isRunning = new(false);
    private readonly CompositeDisposable _disposables = new();

    private IObservable<long>? _timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationDataProvider"/> class.
    /// </summary>
    public SimulationDataProvider()
    {
        _disposables.Add(_isRunning);
    }

    /// <summary>
    /// Gets an observable indicating if simulation is running.
    /// </summary>
    public IObservable<bool> IsRunning => _isRunning.AsObservable();

    /// <summary>
    /// Generates sine wave pattern data.
    /// </summary>
    /// <param name="length">The number of data points.</param>
    /// <param name="amplitude">The amplitude of the sine wave.</param>
    /// <param name="frequency">The frequency of the sine wave.</param>
    /// <param name="phase">The phase offset.</param>
    /// <returns>An array of sine wave values.</returns>
    public static ushort[] GenerateSineWave(int length, double amplitude = 32767, double frequency = 1.0, double phase = 0.0)
    {
        var result = new ushort[length];
        for (var i = 0; i < length; i++)
        {
            var value = (amplitude * Math.Sin((2 * Math.PI * frequency * i / length) + phase)) + amplitude;
            result[i] = (ushort)Math.Max(0, Math.Min(65535, value));
        }

        return result;
    }

    /// <summary>
    /// Generates square wave pattern data.
    /// </summary>
    /// <param name="length">The number of data points.</param>
    /// <param name="highValue">The high value of the square wave.</param>
    /// <param name="lowValue">The low value of the square wave.</param>
    /// <param name="dutyCycle">The duty cycle (0.0 to 1.0).</param>
    /// <returns>An array of square wave values.</returns>
    public static ushort[] GenerateSquareWave(int length, ushort highValue = 65535, ushort lowValue = 0, double dutyCycle = 0.5)
    {
        var result = new ushort[length];
        var switchPoint = (int)(length * dutyCycle);

        for (var i = 0; i < length; i++)
        {
            result[i] = (i % length) < switchPoint ? highValue : lowValue;
        }

        return result;
    }

    /// <summary>
    /// Generates sawtooth wave pattern data.
    /// </summary>
    /// <param name="length">The number of data points.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <returns>An array of sawtooth wave values.</returns>
    public static ushort[] GenerateSawtoothWave(int length, ushort maxValue = 65535, ushort minValue = 0)
    {
        var result = new ushort[length];
        var range = maxValue - minValue;

        for (var i = 0; i < length; i++)
        {
            result[i] = (ushort)(minValue + (range * i / length));
        }

        return result;
    }

    /// <summary>
    /// Starts the simulation with the specified interval.
    /// </summary>
    /// <param name="dataStore">The data store to update.</param>
    /// <param name="interval">The update interval.</param>
    /// <param name="simulationType">The type of simulation to run.</param>
    public void Start(DataStore dataStore, TimeSpan interval, SimulationType simulationType = SimulationType.Random)
    {
        if (_isRunning.Value)
        {
            return;
        }

        _timer = Observable.Interval(interval);

        _disposables.Add(_timer.Subscribe(_ => UpdateData(dataStore, simulationType)));
        _isRunning.OnNext(true);
    }

    /// <summary>
    /// Stops the simulation.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning.Value)
        {
            return;
        }

        _isRunning.OnNext(false);
        _timer = null;
    }

    /// <summary>
    /// Generates random data within specified bounds.
    /// </summary>
    /// <param name="length">The number of data points.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>An array of random values.</returns>
    public ushort[] GenerateRandomData(int length, ushort minValue = 0, ushort maxValue = 65535)
    {
        var result = new ushort[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = (ushort)_random.Next(minValue, maxValue + 1);
        }

        return result;
    }

    /// <summary>
    /// Generates boolean pattern for discrete values.
    /// </summary>
    /// <param name="length">The number of data points.</param>
    /// <param name="pattern">The pattern type.</param>
    /// <returns>An array of boolean values.</returns>
    public bool[] GenerateBooleanPattern(int length, BooleanPattern pattern = BooleanPattern.Random)
    {
        var result = new bool[length];

        return pattern switch
        {
            BooleanPattern.AllTrue => Enumerable.Repeat(true, length).ToArray(),
            BooleanPattern.AllFalse => Enumerable.Repeat(false, length).ToArray(),
            BooleanPattern.Alternating => Enumerable.Range(0, length).Select(i => i % 2 == 0).ToArray(),
            BooleanPattern.Random => Enumerable.Range(0, length).Select(_ => _random.Next(2) == 1).ToArray(),
            _ => result
        };
    }

    /// <summary>
    /// Loads predefined test patterns into a data store.
    /// </summary>
    /// <param name="dataStore">The data store to populate.</param>
    /// <param name="pattern">The pattern type to load.</param>
    public void LoadTestPattern(DataStore dataStore, TestPattern pattern)
    {
        const int dataLength = 1000;
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore), "Data store cannot be null.");
        }

        lock (dataStore.SyncRoot)
        {
            switch (pattern)
            {
                case TestPattern.CountingUp:
                    LoadCountingPattern(dataStore, dataLength, true);
                    break;

                case TestPattern.CountingDown:
                    LoadCountingPattern(dataStore, dataLength, false);
                    break;

                case TestPattern.SineWave:
                    LoadSineWavePattern(dataStore, dataLength);
                    break;

                case TestPattern.SquareWave:
                    LoadSquareWavePattern(dataStore, dataLength);
                    break;

                case TestPattern.Random:
                    LoadRandomPattern(dataStore, dataLength);
                    break;

                case TestPattern.AllZeros:
                    LoadConstantPattern(dataStore, dataLength, 0, false);
                    break;

                case TestPattern.AllOnes:
                    LoadConstantPattern(dataStore, dataLength, 65535, true);
                    break;
            }
        }
    }

    /// <summary>
    /// Disposes the simulation data provider.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _disposables.Dispose();
        _isRunning.Dispose();
    }

    private static void UpdateCountingData(DataStore dataStore, int count, bool countUp)
    {
        for (var i = 0; i < Math.Min(count, dataStore.HoldingRegisters.Count); i++)
        {
            var currentValue = dataStore.HoldingRegisters[i];
            dataStore.HoldingRegisters[i] = countUp ?
                (ushort)((currentValue + 1) % 65536) :
                (ushort)((currentValue == 0) ? 65535 : currentValue - 1);

            dataStore.InputRegisters[i] = dataStore.HoldingRegisters[i];
            dataStore.CoilDiscretes[i] = (dataStore.HoldingRegisters[i] % 2) == 1;
            dataStore.InputDiscretes[i] = !dataStore.CoilDiscretes[i];
        }
    }

    private static void UpdateSineWaveData(DataStore dataStore, int count)
    {
        var time = DateTime.Now.Millisecond / 1000.0;

        for (var i = 0; i < Math.Min(count, dataStore.HoldingRegisters.Count); i++)
        {
            var value = (32767 * Math.Sin((2 * Math.PI * 0.1 * time) + (i * 0.1))) + 32767;
            dataStore.HoldingRegisters[i] = (ushort)Math.Max(0, Math.Min(65535, value));
            dataStore.InputRegisters[i] = dataStore.HoldingRegisters[i];
            dataStore.CoilDiscretes[i] = dataStore.HoldingRegisters[i] > 32767;
            dataStore.InputDiscretes[i] = !dataStore.CoilDiscretes[i];
        }
    }

    private static void UpdateSquareWaveData(DataStore dataStore, int count)
    {
        var time = DateTime.Now.Second;
        var isHigh = (time % 4) < 2;

        for (var i = 0; i < Math.Min(count, dataStore.HoldingRegisters.Count); i++)
        {
            dataStore.HoldingRegisters[i] = isHigh ? (ushort)65535 : (ushort)0;
            dataStore.InputRegisters[i] = dataStore.HoldingRegisters[i];
            dataStore.CoilDiscretes[i] = isHigh;
            dataStore.InputDiscretes[i] = !isHigh;
        }
    }

    private static void LoadCountingPattern(DataStore dataStore, int length, bool countUp)
    {
        for (var i = 0; i < Math.Min(length, dataStore.HoldingRegisters.Count); i++)
        {
            var value = countUp ? i : (length - i - 1);
            dataStore.HoldingRegisters[i] = (ushort)(value % 65536);
            dataStore.InputRegisters[i] = dataStore.HoldingRegisters[i];
            dataStore.CoilDiscretes[i] = (value % 2) == 1;
            dataStore.InputDiscretes[i] = !dataStore.CoilDiscretes[i];
        }
    }

    private static void LoadSineWavePattern(DataStore dataStore, int length)
    {
        var sineData = GenerateSineWave(length);
        for (var i = 0; i < Math.Min(length, dataStore.HoldingRegisters.Count); i++)
        {
            dataStore.HoldingRegisters[i] = sineData[i];
            dataStore.InputRegisters[i] = sineData[i];
            dataStore.CoilDiscretes[i] = sineData[i] > 32767;
            dataStore.InputDiscretes[i] = !dataStore.CoilDiscretes[i];
        }
    }

    private static void LoadSquareWavePattern(DataStore dataStore, int length)
    {
        var squareData = GenerateSquareWave(length);
        for (var i = 0; i < Math.Min(length, dataStore.HoldingRegisters.Count); i++)
        {
            dataStore.HoldingRegisters[i] = squareData[i];
            dataStore.InputRegisters[i] = squareData[i];
            dataStore.CoilDiscretes[i] = squareData[i] > 0;
            dataStore.InputDiscretes[i] = !dataStore.CoilDiscretes[i];
        }
    }

    private static void LoadConstantPattern(DataStore dataStore, int length, ushort value, bool boolValue)
    {
        for (var i = 0; i < Math.Min(length, dataStore.HoldingRegisters.Count); i++)
        {
            dataStore.HoldingRegisters[i] = value;
            dataStore.InputRegisters[i] = value;
            dataStore.CoilDiscretes[i] = boolValue;
            dataStore.InputDiscretes[i] = !boolValue;
        }
    }

    private void UpdateData(DataStore dataStore, SimulationType simulationType)
    {
        const int updateSize = 100;

        lock (dataStore.SyncRoot)
        {
            switch (simulationType)
            {
                case SimulationType.Random:
                    UpdateRandomData(dataStore, updateSize);
                    break;

                case SimulationType.CountingUp:
                    UpdateCountingData(dataStore, updateSize, true);
                    break;

                case SimulationType.CountingDown:
                    UpdateCountingData(dataStore, updateSize, false);
                    break;

                case SimulationType.SineWave:
                    UpdateSineWaveData(dataStore, updateSize);
                    break;

                case SimulationType.SquareWave:
                    UpdateSquareWaveData(dataStore, updateSize);
                    break;
            }
        }
    }

    private void UpdateRandomData(DataStore dataStore, int count)
    {
        for (var i = 0; i < Math.Min(count, dataStore.HoldingRegisters.Count); i++)
        {
            dataStore.HoldingRegisters[i] = (ushort)_random.Next(65536);
            dataStore.InputRegisters[i] = (ushort)_random.Next(65536);
            dataStore.CoilDiscretes[i] = _random.Next(2) == 1;
            dataStore.InputDiscretes[i] = _random.Next(2) == 1;
        }
    }

    private void LoadRandomPattern(DataStore dataStore, int length)
    {
        var randomData = GenerateRandomData(length);
        var boolData = GenerateBooleanPattern(length);

        for (var i = 0; i < Math.Min(length, dataStore.HoldingRegisters.Count); i++)
        {
            dataStore.HoldingRegisters[i] = randomData[i];
            dataStore.InputRegisters[i] = randomData[i];
            dataStore.CoilDiscretes[i] = boolData[i];
            dataStore.InputDiscretes[i] = !boolData[i];
        }
    }
}
