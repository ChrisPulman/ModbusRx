// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>High-performance extensions for DataStore operations using optimized techniques.</summary>
public static class DataStoreExtensions
{
    /// <summary>Provides optimized operations for data stores.</summary>
    /// <param name="dataStore">The data store.</param>
    extension(DataStore dataStore)
    {
    /// <summary>Reads holding registers with optimized performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of register values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public ushort[] ReadHoldingRegistersOptimized(ushort startAddress, ushort count)
    {
        if (dataStore is null)
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

    /// <summary>Reads input registers with optimized performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of register values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public ushort[] ReadInputRegistersOptimized(ushort startAddress, ushort count)
    {
        if (dataStore is null)
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

    /// <summary>Reads coils with optimized performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of coil values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public bool[] ReadCoilsOptimized(ushort startAddress, ushort count)
    {
        if (dataStore is null)
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

    /// <summary>Reads discrete inputs with optimized performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The count.</param>
    /// <returns>Array of input values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public bool[] ReadInputsOptimized(ushort startAddress, ushort count)
    {
        if (dataStore is null)
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

    /// <summary>Writes holding registers with optimized performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="values">The values to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public void WriteHoldingRegistersOptimized(ushort startAddress, ushort[] values)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (values is null)
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

    /// <summary>Writes coils with optimized performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="values">The values to write.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public void WriteCoilsOptimized(ushort startAddress, bool[] values)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (values is null)
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

    /// <summary>Performs a bulk copy operation between data stores with high performance.</summary>
    /// <param name="destinationStore">The destination data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when sourceStore or destinationStore is null.</exception>
    public void BulkCopyHoldingRegisters(
        DataStore destinationStore,
        ushort startAddress,
        ushort count)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (destinationStore is null)
        {
            throw new ArgumentNullException(nameof(destinationStore));
        }

        var data = dataStore.ReadHoldingRegistersOptimized(startAddress, count);
        destinationStore.WriteHoldingRegistersOptimized(startAddress, data);
    }

    /// <summary>Performs a bulk copy operation for coils between data stores with high performance.</summary>
    /// <param name="destinationStore">The destination data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when sourceStore or destinationStore is null.</exception>
    public void BulkCopyCoils(
        DataStore destinationStore,
        ushort startAddress,
        ushort count)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (destinationStore is null)
        {
            throw new ArgumentNullException(nameof(destinationStore));
        }

        var data = dataStore.ReadCoilsOptimized(startAddress, count);
        destinationStore.WriteCoilsOptimized(startAddress, data);
    }

    /// <summary>Clears a range of holding registers with high performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of registers to clear.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public void ClearHoldingRegisters(ushort startAddress, ushort count)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var zeros = new ushort[count];
        dataStore.WriteHoldingRegistersOptimized(startAddress, zeros);
    }

    /// <summary>Clears a range of coils with high performance.</summary>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of coils to clear.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataStore is null.</exception>
    public void ClearCoils(ushort startAddress, ushort count)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        var falses = new bool[count];
        dataStore.WriteCoilsOptimized(startAddress, falses);
    }

    /// <summary>Performs a memory-efficient comparison between two data stores.</summary>
    /// <param name="store2">The second data store.</param>
    /// <param name="startAddress">The start address.</param>
    /// <param name="count">The number of elements to compare.</param>
    /// <returns>True if the data ranges are identical.</returns>
    /// <exception cref="ArgumentNullException">Thrown when store1 or store2 is null.</exception>
    public bool CompareHoldingRegisters(
        DataStore store2,
        ushort startAddress,
        ushort count)
    {
        if (dataStore is null)
        {
            throw new ArgumentNullException(nameof(dataStore));
        }

        if (store2 is null)
        {
            throw new ArgumentNullException(nameof(store2));
        }

        var data1 = dataStore.ReadHoldingRegistersOptimized(startAddress, count);
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
}
