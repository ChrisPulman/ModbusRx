// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;

namespace ModbusRx.Data;

/// <summary>
/// High-performance extensions for DataStore operations using optimized techniques.
/// </summary>
public static class DataStoreExtensions
{
    /// <summary>
    /// Reads holding registers with optimized performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of register values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static ushort[] ReadHoldingRegistersOptimized(this DataStore dataStore, ushort startAddress, ushort count)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var result = new ushort[count];
        var registers = dataStore.HoldingRegisters;
        var maxIndex = Math.Min(startAddress + count, registers.Count);

        for (var i = 0; i < count; i++)
        {
            var index = startAddress + i;
            if (index < maxIndex && index > 0)
            {
                result[i] = registers[index];
            }
        }

        return result;
    }

    /// <summary>
    /// Reads input registers with optimized performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of register values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static ushort[] ReadInputRegistersOptimized(this DataStore dataStore, ushort startAddress, ushort count)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var result = new ushort[count];
        var registers = dataStore.InputRegisters;
        var maxIndex = Math.Min(startAddress + count, registers.Count);

        for (var i = 0; i < count; i++)
        {
            var index = startAddress + i;
            if (index < maxIndex && index > 0)
            {
                result[i] = registers[index];
            }
        }

        return result;
    }

    /// <summary>
    /// Reads coils with optimized performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of coil values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static bool[] ReadCoilsOptimized(this DataStore dataStore, ushort startAddress, ushort count)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var result = new bool[count];
        var coils = dataStore.CoilDiscretes;
        var maxIndex = Math.Min(startAddress + count, coils.Count);

        for (var i = 0; i < count; i++)
        {
            var index = startAddress + i;
            if (index < maxIndex && index > 0)
            {
                result[i] = coils[index];
            }
        }

        return result;
    }

    /// <summary>
    /// Reads discrete inputs with optimized performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of input values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static bool[] ReadInputsOptimized(this DataStore dataStore, ushort startAddress, ushort count)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var result = new bool[count];
        var inputs = dataStore.InputDiscretes;
        var maxIndex = Math.Min(startAddress + count, inputs.Count);

        for (var i = 0; i < count; i++)
        {
            var index = startAddress + i;
            if (index < maxIndex && index > 0)
            {
                result[i] = inputs[index];
            }
        }

        return result;
    }

    /// <summary>
    /// Writes holding registers with optimized performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="values">The values to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static void WriteHoldingRegistersOptimized(this DataStore dataStore, ushort startAddress, ushort[] values)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (values == null)
        {
            return;
        }

        var registers = dataStore.HoldingRegisters;

        for (var i = 0; i < values.Length; i++)
        {
            var index = startAddress + i;

            // Ensure collection is large enough
            while (registers.Count <= index)
            {
                registers.Add(0);
            }

            if (index > 0)
            {
                registers[index] = values[i];
            }
        }
    }

    /// <summary>
    /// Writes coils with optimized performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="values">The values to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static void WriteCoilsOptimized(this DataStore dataStore, ushort startAddress, bool[] values)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (values == null)
        {
            return;
        }

        var coils = dataStore.CoilDiscretes;

        for (var i = 0; i < values.Length; i++)
        {
            var index = startAddress + i;

            // Ensure collection is large enough
            while (coils.Count <= index)
            {
                coils.Add(false);
            }

            if (index > 0)
            {
                coils[index] = values[i];
            }
        }
    }

    /// <summary>
    /// Performs a bulk copy operation between data stores with high performance.
    /// </summary>
    /// <param name="sourceStore">The source data store.</param>
    /// <param name="destinationStore">The destination data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when sourceStore or destinationStore is null.</exception>
    public static void BulkCopyHoldingRegisters(
        this DataStore sourceStore,
        DataStore destinationStore,
        ushort startAddress,
        ushort count)
    {
        if (sourceStore == null)
        {
            throw new ArgumentNullException(nameof(sourceStore));
        }

        if (destinationStore == null)
        {
            throw new ArgumentNullException(nameof(destinationStore));
        }

        var data = sourceStore.ReadHoldingRegistersOptimized(startAddress, count);
        destinationStore.WriteHoldingRegistersOptimized(startAddress, data);
    }

    /// <summary>
    /// Performs a bulk copy operation for coils between data stores with high performance.
    /// </summary>
    /// <param name="sourceStore">The source data store.</param>
    /// <param name="destinationStore">The destination data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when sourceStore or destinationStore is null.</exception>
    public static void BulkCopyCoils(
        this DataStore sourceStore,
        DataStore destinationStore,
        ushort startAddress,
        ushort count)
    {
        if (sourceStore == null)
        {
            throw new ArgumentNullException(nameof(sourceStore));
        }

        if (destinationStore == null)
        {
            throw new ArgumentNullException(nameof(destinationStore));
        }

        var data = sourceStore.ReadCoilsOptimized(startAddress, count);
        destinationStore.WriteCoilsOptimized(startAddress, data);
    }

    /// <summary>
    /// Clears a range of holding registers with high performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of registers to clear.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static void ClearHoldingRegisters(this DataStore dataStore, ushort startAddress, ushort count)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var zeros = new ushort[count];
        dataStore.WriteHoldingRegistersOptimized(startAddress, zeros);
    }

    /// <summary>
    /// Clears a range of coils with high performance.
    /// </summary>
    /// <param name="dataStore">The data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of coils to clear.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public static void ClearCoils(this DataStore dataStore, ushort startAddress, ushort count)
    {
        if (dataStore == null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var falses = new bool[count];
        dataStore.WriteCoilsOptimized(startAddress, falses);
    }

    /// <summary>
    /// Performs a memory-efficient comparison between two data stores.
    /// </summary>
    /// <param name="store1">The first data store.</param>
    /// <param name="store2">The second data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of elements to compare.</param>
    /// <returns>True if the data ranges are identical.</returns>
    /// <exception cref="ArgumentNullException">Thrown when store1 or store2 is null.</exception>
    public static bool CompareHoldingRegisters(
        this DataStore store1,
        DataStore store2,
        ushort startAddress,
        ushort count)
    {
        if (store1 == null)
        {
            throw new ArgumentNullException(nameof(store1));
        }

        if (store2 == null)
        {
            throw new ArgumentNullException(nameof(store2));
        }

        var data1 = store1.ReadHoldingRegistersOptimized(startAddress, count);
        var data2 = store2.ReadHoldingRegistersOptimized(startAddress, count);

        if (data1.Length != data2.Length)
        {
            return false;
        }

        for (var i = 0; i < data1.Length; i++)
        {
            if (data1[i] != data2[i])
            {
                return false;
            }
        }

        return true;
    }
}
