// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using ModbusRx.Data;

namespace ModbusRx.UnitTests.Data;

/// <summary>Tests the BoolModbusDataCollectionFixture behavior.</summary>
[TUnit.Core.InheritsTests]
public class BoolModbusDataCollectionFixture : ModbusDataCollectionFixture<bool>
{
    /// <summary>Removes from read only.</summary>
    [TUnit.Core.Test]
    public void Remove_FromReadOnly()
    {
        bool[] source = { false, false, false, true, false, false };
        var col = new ModbusDataCollection<bool>(new ReadOnlyCollection<bool>(source));
        var expectedCount = source.Length;

        Assert.True(col.Remove(source[3]));

        Assert.Equal(expectedCount, col.Count);
    }

    /// <summary>Gets the array.</summary>
    /// <returns>A bool.</returns>
    protected override bool[] GetArray() =>
        [false, false, true, false, false];

    /// <summary>Gets the non existent element.</summary>
    /// <returns>A bool.</returns>
    protected override bool GetNonExistentElement() => true;
}
