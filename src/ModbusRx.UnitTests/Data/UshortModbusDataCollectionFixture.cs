// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using ModbusRx.Data;

namespace ModbusRx.UnitTests.Data;

/// <summary>Tests the UshortModbusDataCollectionFixture behavior.</summary>
[TUnit.Core.InheritsTests]
public class UshortModbusDataCollectionFixture : ModbusDataCollectionFixture<ushort>
{
    /// <summary>Removes from read only.</summary>
    [TUnit.Core.Test]
    public void Remove_FromReadOnly()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<ushort>(new ReadOnlyCollection<ushort>(source));
        var expectedCount = source.Length;

        Assert.False(col.Remove(GetNonExistentElement()));
        Assert.True(col.Remove(source[3]));
        Assert.Equal(expectedCount, col.Count);
    }

    /// <summary>Gets the array.</summary>
    /// <returns>A ushort.</returns>
    protected override ushort[] GetArray() =>
        [5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];

    /// <summary>Gets the non existent element.</summary>
    /// <returns>A ushort.</returns>
    protected override ushort GetNonExistentElement() => 42;
}
