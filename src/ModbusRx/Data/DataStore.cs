// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Unme.Common;
#else
using ModbusRx.Unme.Common;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>
/// Object simulation of device memory map.
/// The underlying collections are thread safe when using the ModbusMaster API to read/write values.
/// You can use the SyncRoot property to synchronize direct access to the DataStore collections.
/// </summary>
public class DataStore : IDisposable
{
    /// <summary>Stores the lock value.</summary>
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>Initializes a new instance of the <see cref="DataStore" /> class.</summary>
    public DataStore()
    {
        CoilDiscretes = new() { ModbusDataType = ModbusDataType.Coil };
        InputDiscretes = new() { ModbusDataType = ModbusDataType.Input };
        HoldingRegisters = new() { ModbusDataType = ModbusDataType.HoldingRegister };
        InputRegisters = new() { ModbusDataType = ModbusDataType.InputRegister };
    }

    /// <summary>Initializes a new instance of the <see cref="DataStore"/> class.</summary>
    /// <param name="coilDiscretes">List of discrete coil values.</param>
    /// <param name="inputDiscretes">List of discrete input values.</param>
    /// <param name="holdingRegisters">List of holding register values.</param>
    /// <param name="inputRegisters">List of input register values.</param>
    internal DataStore(
        IList<bool> coilDiscretes,
        IList<bool> inputDiscretes,
        IList<ushort> holdingRegisters,
        IList<ushort> inputRegisters)
    {
        CoilDiscretes = new(coilDiscretes) { ModbusDataType = ModbusDataType.Coil };
        InputDiscretes = new(inputDiscretes) { ModbusDataType = ModbusDataType.Input };
        HoldingRegisters = new(holdingRegisters) { ModbusDataType = ModbusDataType.HoldingRegister };
        InputRegisters = new(inputRegisters) { ModbusDataType = ModbusDataType.InputRegister };
    }

    /// <summary>Occurs when the DataStore is written to via a Modbus command.</summary>
    public event EventHandler<DataStoreEventArgs>? DataStoreWrittenTo;

    /// <summary>Occurs when the DataStore is read from via a Modbus command.</summary>
    public event EventHandler<DataStoreEventArgs>? DataStoreReadFrom;

    /// <summary>Gets the discrete coils.</summary>
    public ModbusDataCollection<bool> CoilDiscretes { get; }

    /// <summary>Gets the discrete inputs.</summary>
    public ModbusDataCollection<bool> InputDiscretes { get; }

    /// <summary>Gets the holding registers.</summary>
    public ModbusDataCollection<ushort> HoldingRegisters { get; }

    /// <summary>Gets the input registers.</summary>
    public ModbusDataCollection<ushort> InputRegisters { get; }

    /// <summary>Gets an object that can be used to synchronize direct access to the DataStore collections.</summary>
    public object SyncRoot { get; } = new();

    /// <summary>Gets the reader-writer lock for more granular access control.</summary>
    public ReaderWriterLockSlim Lock { get; } = new();

    /// <summary>Disposes the DataStore and releases resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Performs a bulk read operation with optimized memory allocation.</summary>
    /// <typeparam name="T">The collection type.</typeparam>
    /// <typeparam name="TU">The type of elements in the collection.</typeparam>
    /// <param name="dataSource">The data source to read from.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="count">The number of items to read.</param>
    /// <returns>The read data collection.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadDataOptimized<T, TU>(
        ModbusDataCollection<TU> dataSource,
        ushort startAddress,
        ushort count)
        where T : Collection<TU>, new()
        where TU : struct
    {
        if (dataSource is null)
        {
            throw new ArgumentNullException(nameof(dataSource));
        }

        var startIndex = startAddress + 1;

        if (startIndex < 0 || dataSource.Count < startIndex + count)
        {
            throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);
        }

        TU[] dataToRetrieve;

        _lock.EnterReadLock();
        try
        {
            dataToRetrieve = new TU[count];
            for (var i = 0; i < dataToRetrieve.Length; i++)
            {
                dataToRetrieve[i] = dataSource[startIndex + i];
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        var result = new T();
        for (var i = 0; i < count; i++)
        {
            result.Add(dataToRetrieve[i]);
        }

        var dataStoreEventArgs = DataStoreEventArgs.CreateDataStoreEventArgs(startAddress, dataSource.ModbusDataType, result);
        DataStoreReadFrom?.Invoke(this, dataStoreEventArgs);
        return result;
    }

    /// <summary>Performs a bulk write operation with optimized memory allocation.</summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="items">The items to write.</param>
    /// <param name="destination">The destination collection.</param>
    /// <param name="startAddress">The starting address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDataOptimized<TData>(
        IEnumerable<TData> items,
        ModbusDataCollection<TData> destination,
        ushort startAddress)
        where TData : struct
    {
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        var materializedItems = MaterializeItems(items);
        var startIndex = startAddress + 1;

        if (startIndex < 0 || destination.Count < startIndex + materializedItems.Count)
        {
            throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);
        }

        _lock.EnterWriteLock();
        try
        {
            Update(materializedItems, destination, startIndex);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        var dataStoreEventArgs = DataStoreEventArgs.CreateDataStoreEventArgs(
            startAddress,
            destination.ModbusDataType,
            materializedItems);

        DataStoreWrittenTo?.Invoke(this, dataStoreEventArgs);
    }

    /// <summary>Retrieves subset of data from collection.</summary>
    /// <typeparam name="T">The collection type.</typeparam>
    /// <typeparam name="TU">The type of elements in the collection.</typeparam>
    /// <param name="dataStore">The data store raising the read event.</param>
    /// <param name="dataSource">The source collection.</param>
    /// <param name="startAddress">The starting Modbus address.</param>
    /// <param name="count">The number of values to read.</param>
    /// <param name="syncRoot">The synchronization root for the source collection.</param>
    /// <returns>The result.</returns>
    internal static T ReadData<T, TU>(
        DataStore dataStore,
        ModbusDataCollection<TU> dataSource,
        ushort startAddress,
        ushort count,
        object syncRoot)
        where T : Collection<TU>, new()
        where TU : struct
    {
        DataStoreEventArgs dataStoreEventArgs;
        var startIndex = startAddress + 1;

        if (startIndex < 0 || dataSource.Count < startIndex + count)
        {
            throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);
        }

        TU[] dataToRetrieve;
        lock (syncRoot)
        {
            dataToRetrieve = new TU[count];
            for (var i = 0; i < dataToRetrieve.Length; i++)
            {
                dataToRetrieve[i] = dataSource[startIndex + i];
            }
        }

        var result = new T();
        for (var i = 0; i < count; i++)
        {
            result.Add(dataToRetrieve[i]);
        }

        dataStoreEventArgs = DataStoreEventArgs.CreateDataStoreEventArgs(startAddress, dataSource.ModbusDataType, result);
        dataStore.DataStoreReadFrom?.Invoke(dataStore, dataStoreEventArgs);
        return result;
    }

    /// <summary>Write data to data store.</summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="dataStore">The data store raising the write event.</param>
    /// <param name="items">The values to write.</param>
    /// <param name="destination">The destination collection.</param>
    /// <param name="startAddress">The starting Modbus address.</param>
    /// <param name="syncRoot">The synchronization root for the destination collection.</param>
    internal static void WriteData<TData>(
        DataStore dataStore,
        IEnumerable<TData> items,
        ModbusDataCollection<TData> destination,
        ushort startAddress,
        object syncRoot)
        where TData : struct
    {
        DataStoreEventArgs dataStoreEventArgs;
        var startIndex = startAddress + 1;
        var materializedItems = MaterializeItems(items);

        if (startIndex < 0 || destination.Count < startIndex + materializedItems.Count)
        {
            throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);
        }

        lock (syncRoot)
        {
            Update(materializedItems, destination, startIndex);
        }

        dataStoreEventArgs = DataStoreEventArgs.CreateDataStoreEventArgs(
            startAddress,
            destination.ModbusDataType,
            materializedItems);

        dataStore.DataStoreWrittenTo?.Invoke(dataStore, dataStoreEventArgs);
    }

    /// <summary>Updates a subset of values in a collection.</summary>
    /// <typeparam name="T">The collection item type.</typeparam>
    /// <param name="items">The items to write.</param>
    /// <param name="destination">The destination collection.</param>
    /// <param name="startIndex">The zero-based destination index.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Update<T>(IEnumerable<T> items, IList<T> destination, int startIndex)
    {
        var materializedItems = MaterializeItems(items);

        if (startIndex < 0 || destination.Count < startIndex + materializedItems.Count)
        {
            throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);
        }

        var index = startIndex;

        foreach (var item in materializedItems)
        {
            destination[index] = item;
            ++index;
        }
    }

    /// <summary>Protected virtual dispose method.</summary>
    /// <param name="disposing">Indicates if disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _lock.Dispose();
    }

    /// <summary>Executes the Materialize Items operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="items">The items value.</param>
    /// <returns>The result.</returns>
    private static IReadOnlyList<T> MaterializeItems<T>(IEnumerable<T> items) =>
        (items as IReadOnlyList<T>) ?? new List<T>(items);
}
