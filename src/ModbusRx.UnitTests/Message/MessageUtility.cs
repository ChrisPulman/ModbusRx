// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// MessageUtility.
/// </summary>
public static class MessageUtility
{
    /// <summary>
    /// Creates a collection initialized to a default value.
    /// </summary>
    /// <typeparam name="T">The Key.</typeparam>
    /// <typeparam name="TV">The type of the v.</typeparam>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="size">The size.</param>
    /// <returns>A value of T.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">size - Collection size cannot be less than 0.</exception>
    public static T CreateDefaultCollection<T, TV>(TV defaultValue, int size)
        where T : ICollection<TV>, new()
    {
        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Collection size cannot be less than 0.");
        }

        var col = new T();

        for (var i = 0; i < size; i++)
        {
            col.Add(defaultValue);
        }

        return col;
    }
}
