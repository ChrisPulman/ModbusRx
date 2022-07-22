// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using ModbusRx.Utility;

namespace ModbusRx.Data;

/// <summary>
///     Event args for read write actions performed on the DataStore.
/// </summary>
public sealed class DataStoreEventArgs : EventArgs
{
    private DataStoreEventArgs(ushort startAddress, ModbusDataType modbusDataType)
    {
        StartAddress = startAddress;
        ModbusDataType = modbusDataType;
    }

    /// <summary>
    ///     Gets type of Modbus data (e.g. Holding register).
    /// </summary>
    public ModbusDataType ModbusDataType { get; }

    /// <summary>
    ///     Gets start address of data.
    /// </summary>
    public ushort StartAddress { get; }

    /// <summary>
    ///     Gets data that was read or written.
    /// </summary>
    public DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>? Data { get; private set; }

    internal static DataStoreEventArgs CreateDataStoreEventArgs<T>(ushort startAddress, ModbusDataType modbusDataType, IEnumerable<T> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (typeof(T) == typeof(bool))
        {
            var a = new ReadOnlyCollection<bool>(data.Cast<bool>().ToArray());

            return new DataStoreEventArgs(startAddress, modbusDataType)
            {
                Data = DiscriminatedUnion<ReadOnlyCollection<bool>, ReadOnlyCollection<ushort>>.CreateA(a),
            };
        }
        else if (typeof(T) == typeof(ushort))
        {
            var b = new ReadOnlyCollection<ushort>(data.Cast<ushort>().ToArray());

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
