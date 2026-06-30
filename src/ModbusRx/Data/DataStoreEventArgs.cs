// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Utility;
#else
using ModbusRx.Utility;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Event args for read write actions performed on the DataStore.</summary>
public sealed class DataStoreEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the Data Store Event Args class.</summary>
    /// <param name="startAddress">The start Address value.</param>
    /// <param name="modbusDataType">The modbus Data Type value.</param>
    private DataStoreEventArgs(ushort startAddress, ModbusDataType modbusDataType)
    {
        StartAddress = startAddress;
        ModbusDataType = modbusDataType;
    }

    /// <summary>Gets type of Modbus data (e.g. Holding register).</summary>
    public ModbusDataType ModbusDataType { get; }

    /// <summary>Gets start address of data.</summary>
    public ushort StartAddress { get; }

    /// <summary>Gets data that was read or written.</summary>
    public DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>? Data { get; private set; }

    /// <summary>Executes the Create Data Store Event Args operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="startAddress">The start Address value.</param>
    /// <param name="modbusDataType">The modbus Data Type value.</param>
    /// <param name="data">The data value.</param>
    /// <returns>The result.</returns>
    internal static DataStoreEventArgs CreateDataStoreEventArgs<T>(ushort startAddress, ModbusDataType modbusDataType, IEnumerable<T> data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (typeof(T) == typeof(bool))
        {
            var values = new List<bool>();
            foreach (var item in data)
            {
                if (item is bool value)
                {
                    values.Add(value);
                }
            }

            var a = new ReadOnlyCollection<bool>(values);

            return new DataStoreEventArgs(startAddress, modbusDataType)
            {
                Data = DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>.CreateA(a),
            };
        }
        else if (typeof(T) == typeof(ushort))
        {
            var values = new List<ushort>();
            foreach (var item in data)
            {
                if (item is ushort value)
                {
                    values.Add(value);
                }
            }

            var b = new ReadOnlyCollection<ushort>(values);

            return new DataStoreEventArgs(startAddress, modbusDataType)
            {
                Data = DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>.CreateB(b),
            };
        }
        else
        {
            throw new ArgumentException("Generic type T should be of type bool or ushort");
        }
    }
}
