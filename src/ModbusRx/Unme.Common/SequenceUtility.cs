// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Unme.Common;

internal static class SequenceUtility
{
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, int startIndex, int size)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var enumerable = source as T[] ?? source.ToArray();
        var num = enumerable.Length;

        if (startIndex < 0 || num < startIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (size < 0 || startIndex + size > num)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        return enumerable.Skip(startIndex).Take(size);
    }
}
