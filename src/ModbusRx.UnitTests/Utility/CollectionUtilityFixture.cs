// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModbusRx.Data;
using ModbusRx.UnitTests.Message;
using ModbusRx.Unme.Common;

namespace ModbusRx.UnitTests.Utility;

/// <summary>Tests the CollectionUtilityFixture behavior.</summary>
public class CollectionUtilityFixture
{
    /// <summary>Slices the middle.</summary>
    [TUnit.Core.Test]
    public void SliceMiddle()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.Equal<IEnumerable<byte>>([3, 4, 5, 6, 7], test.Slice(2, 5));
    }

    /// <summary>Slices the beginning.</summary>
    [TUnit.Core.Test]
    public void SliceBeginning()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.Equal<IEnumerable<byte>>([1, 2], test.Slice(0, 2));
    }

    /// <summary>Slices the end.</summary>
    [TUnit.Core.Test]
    public void SliceEnd()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.Equal<IEnumerable<byte>>([9, 10], test.Slice(8, 2));
    }

    /// <summary>Slices the collection.</summary>
    [TUnit.Core.Test]
    public void SliceCollection()
    {
        var col = new Collection<bool>([ true, false, false, false, true, true]);
        Assert.Equal<IEnumerable<bool>>([false, false, true], col.Slice(2, 3));
    }

    /// <summary>Slices the read only collection.</summary>
    [TUnit.Core.Test]
    public void SliceReadOnlyCollection()
    {
        var col = new ReadOnlyCollection<bool>([ true, false, false, false, true, true]);
        Assert.Equal<IEnumerable<bool>>([false, false, true], col.Slice(2, 3));
    }

    /// <summary>Slices the null i collection.</summary>
    [TUnit.Core.Test]
    public void SliceNullICollection()
    {
        ICollection<bool> col = null!;
        _ = Assert.Throws<ArgumentNullException>(() => _ = col.Slice(1, 1));
    }

    /// <summary>Slices the null array.</summary>
    [TUnit.Core.Test]
    public void SliceNullArray()
    {
        bool[] array = null!;
        _ = Assert.Throws<ArgumentNullException>(() => _ = array.Slice(1, 1));
    }

    /// <summary>Creates the default size of the collection negative.</summary>
    [TUnit.Core.Test]
    public void CreateDefaultCollectionNegativeSize() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(0, -1));

    /// <summary>Creates the default collection.</summary>
    [TUnit.Core.Test]
    public void CreateDefaultCollection()
    {
        var col = MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(3, 5);
        Assert.Equal(5, col.Count);
        Assert.Equal<IEnumerable<ushort>>([3, 3, 3, 3, 3], col);
    }
}
