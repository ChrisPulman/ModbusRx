// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Unme.Common;
#else
namespace ModbusRx.Unme.Common;
#endif

/// <summary>Provides sequence slicing helpers.</summary>
internal static class SequenceExtensions
{
    /// <summary>Provides slicing operations for enumerable sources.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="source">The source sequence.</param>
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>Returns a bounded slice of the source sequence.</summary>
        /// <param name="startIndex">The zero-based index where the slice starts.</param>
        /// <param name="size">The number of items to include.</param>
        /// <returns>The sliced items.</returns>
        public IEnumerable<T> Slice(int startIndex, int size)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var enumerable = (source as T[]) ?? Materialize(source);
            var num = enumerable.Length;

            if (startIndex < 0 || num < startIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (size < 0 || startIndex + size > num)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            var result = new T[size];
            Array.Copy(enumerable, startIndex, result, 0, size);
            return result;
        }
    }

    /// <summary>Materializes a sequence into an array without using LINQ.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>The materialized items.</returns>
    private static T[] Materialize<T>(IEnumerable<T> source)
    {
        var items = new List<T>();
        items.AddRange(source);
        return items.ToArray();
    }
}
