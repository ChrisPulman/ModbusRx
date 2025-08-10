// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;

namespace ModbusRx.Reactive;

/// <summary>
/// Array equality comparer for observing distinct changes.
/// </summary>
/// <typeparam name="T">The array element type.</typeparam>
internal class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
{
    /// <summary>
    /// Determines whether the specified arrays are equal.
    /// </summary>
    /// <param name="x">The first array to compare.</param>
    /// <param name="y">The second array to compare.</param>
    /// <returns>True if the arrays are equal; otherwise, false.</returns>
    public bool Equals(T[]? x, T[]? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.Length != y.Length)
        {
            return false;
        }

        return x.SequenceEqual(y);
    }

    /// <summary>
    /// Returns a hash code for the specified array.
    /// </summary>
    /// <param name="obj">The array for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified array.</returns>
    public int GetHashCode(T[] obj)
    {
        if (obj is null)
        {
            return 0;
        }

        var hash = 17;
        foreach (var item in obj)
        {
            hash = (hash * 23) + (item?.GetHashCode() ?? 0);
        }

        return hash;
    }
}
