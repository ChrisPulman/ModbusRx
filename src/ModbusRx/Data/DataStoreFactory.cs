// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Data story factory.</summary>
public static class DataStoreFactory
{
    /// <summary>Factory method for default data store - register values set to 0 and discrete values set to false.</summary>
    /// <returns>A DataStore.</returns>
    public static DataStore CreateDefaultDataStore() =>
        CreateDefaultDataStore(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);

    /// <summary>Factory method for default data store - register values set to 0 and discrete values set to false.</summary>
    /// <param name="coilsCount">Number of discrete coils.</param>
    /// <param name="inputsCount">Number of discrete inputs.</param>
    /// <param name="holdingRegistersCount">Number of holding registers.</param>
    /// <param name="inputRegistersCount">Number of input registers.</param>
    /// <returns>New instance of Data store with defined inputs/outputs.</returns>
    public static DataStore CreateDefaultDataStore(ushort coilsCount, ushort inputsCount, ushort holdingRegistersCount, ushort inputRegistersCount)
    {
        var coils = new bool[coilsCount];
        var inputs = new bool[inputsCount];
        var holdingRegs = new ushort[holdingRegistersCount];
        var inputRegs = new ushort[inputRegistersCount];

        return new DataStore(coils, inputs, holdingRegs, inputRegs);
    }

    /// <summary>Factory method for test data store.</summary>
    /// <returns>The result.</returns>
    internal static DataStore CreateTestDataStore()
    {
        var dataStore = new DataStore();

        for (var i = 1; i < 3000; i++)
        {
            var value = i % 2 > 0;
            dataStore.CoilDiscretes.Add(value);
            dataStore.InputDiscretes.Add(!value);
            dataStore.HoldingRegisters.Add((ushort)i);
            dataStore.InputRegisters.Add((ushort)(i * 10));
        }

        return dataStore;
    }
}
