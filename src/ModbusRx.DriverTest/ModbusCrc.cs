// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using ModbusRx.Reactive;

namespace ModbusRx.DriverTest;

/// <summary>
/// ModbusCrc.
/// </summary>
public static class ModbusCrc
{
    /// <summary>
    /// Computes the specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="length">The length.</param>
    /// <returns>A ushort.</returns>
    /// <exception cref="ArgumentNullException">data.</exception>
    public static ushort Compute(byte[] data, int length)
    {
        ushort crc = 0xFFFF;
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        for (var i = 0; i < length; i++)
        {
            crc ^= data[i];

            for (var b = 0; b < 8; b++)
            {
                var lsb = (crc & 0x0001) != 0;
                crc >>= 1;
                if (lsb)
                {
                    crc ^= 0xA001;
                }
            }
        }

        return crc;
    }
}
