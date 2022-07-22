// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ModbusRx.Data;
using ModbusRx.UnitTests.Message;
using ModbusRx.Unme.Common;
using Xunit;

namespace ModbusRx.UnitTests.Utility;

/// <summary>
/// CollectionUtilityFixture.
/// </summary>
public class CollectionUtilityFixture
{
    /// <summary>
    /// Slices the middle.
    /// </summary>
    [Fact]
    public void SliceMiddle()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.Equal(new byte[] { 3, 4, 5, 6, 7 }, test.Slice(2, 5).ToArray());
    }

    /// <summary>
    /// Slices the beginning.
    /// </summary>
    [Fact]
    public void SliceBeginning()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.Equal(new byte[] { 1, 2 }, test.Slice(0, 2).ToArray());
    }

    /// <summary>
    /// Slices the end.
    /// </summary>
    [Fact]
    public void SliceEnd()
    {
        byte[] test = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        Assert.Equal(new byte[] { 9, 10 }, test.Slice(8, 2).ToArray());
    }

    /// <summary>
    /// Slices the collection.
    /// </summary>
    [Fact]
    public void SliceCollection()
    {
        var col = new Collection<bool>(new bool[] { true, false, false, false, true, true });
        Assert.Equal(new bool[] { false, false, true }, col.Slice(2, 3).ToArray());
    }

    /// <summary>
    /// Slices the read only collection.
    /// </summary>
    [Fact]
    public void SliceReadOnlyCollection()
    {
        var col = new ReadOnlyCollection<bool>(new bool[] { true, false, false, false, true, true });
        Assert.Equal(new bool[] { false, false, true }, col.Slice(2, 3).ToArray());
    }

    /// <summary>
    /// Slices the null i collection.
    /// </summary>
    [Fact]
    public void SliceNullICollection()
    {
        ICollection<bool> col = null!;
        Assert.Throws<ArgumentNullException>(() => col.Slice(1, 1).ToArray());
    }

    /// <summary>
    /// Slices the null array.
    /// </summary>
    [Fact]
    public void SliceNullArray()
    {
        bool[] array = null!;
        Assert.Throws<ArgumentNullException>(() => array.Slice(1, 1).ToArray());
    }

    /// <summary>
    /// Creates the default size of the collection negative.
    /// </summary>
    [Fact]
    public void CreateDefaultCollectionNegativeSize() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(0, -1));

    /// <summary>
    /// Creates the default collection.
    /// </summary>
    [Fact]
    public void CreateDefaultCollection()
    {
        var col = MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(3, 5);
        Assert.Equal(5, col.Count);
        Assert.Equal(new ushort[] { 3, 3, 3, 3, 3 }, col.ToArray());
    }
}
