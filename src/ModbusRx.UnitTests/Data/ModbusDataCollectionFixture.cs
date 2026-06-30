// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModbusRx.Data;

namespace ModbusRx.UnitTests.Data;

/// <summary>Tests the ModbusDataCollectionFixture behavior.</summary>
/// <typeparam name="TData">The type of the data.</typeparam>
public abstract class ModbusDataCollectionFixture<TData>
    where TData : struct
{
    /// <summary>Defaults the contstructor.</summary>
    [TUnit.Core.Test]
    public void DefaultContstructor()
    {
        var col = new ModbusDataCollection<TData>();
        Assert.NotEmpty(col);
        _ = Assert.Single(col);

        col.Add(default!);
        Assert.Equal(2, col.Count);
    }

    /// <summary>Contstructors the with parameters.</summary>
    [TUnit.Core.Test]
    public void ContstructorWithParams()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<TData>(source);
        Assert.Equal(source.Length + 1, col.Count);
        Assert.NotEmpty(col);

        col.Add(default!);
        Assert.Equal(source.Length + 2, col.Count);
    }

    /// <summary>Contstructors the with i list.</summary>
    [TUnit.Core.Test]
    public void ContstructorWithIList()
    {
        var source = GetList();
        var expectedCount = source.Count;

        var col = new ModbusDataCollection<TData>(source);

        Assert.Equal(expectedCount + 1, source.Count);
        Assert.Equal(expectedCount + 1, col.Count);

        source.Insert(0, default!);
        Assert.Equal(source, col);
    }

    /// <summary>Contstructors the with i list from read only list.</summary>
    [TUnit.Core.Test]
    public void ContstructorWithIList_FromReadOnlyList()
    {
        var source = GetList();
        var readOnly = new ReadOnlyCollection<TData>(source);
        var expectedCount = source.Count;

        var col = new ModbusDataCollection<TData>(readOnly);

        Assert.Equal(expectedCount, source.Count);
        Assert.Equal(expectedCount + 1, col.Count);

        source.Insert(0, default!);
        Assert.Equal(source, col);
    }

    /// <summary>Sets the zero element using item.</summary>
    [TUnit.Core.Test]
    public void SetZeroElementUsingItem()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<TData>(source);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => col[0] = source[3]);
    }

    /// <summary>Zeroes the element using item negative.</summary>
    [TUnit.Core.Test]
    public void ZeroElementUsingItem_Negative()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<TData>(source);

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => col[0] = source[3]);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => col.Insert(0, source[3]));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveAt(0));

        // Remove forst zero/false
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => col.Remove(default!));
    }

    /// <summary>Clears this instance.</summary>
    [TUnit.Core.Test]
    public void Clear()
    {
        var col = new ModbusDataCollection<TData>(GetArray());
        col.Clear();

        _ = Assert.Single(col);
    }

    /// <summary>Removes this instance.</summary>
    [TUnit.Core.Test]
    public void Remove()
    {
        var source = GetList();
        var col = new ModbusDataCollection<TData>(source);
        var expectedCount = source.Count - 1;

        Assert.True(col.Remove(source[3]));

        Assert.Equal(expectedCount, col.Count);
        Assert.Equal(expectedCount, source.Count);
        Assert.Equal(source, col);
    }

    /// <summary>Gets the array.</summary>
    /// <returns>A value of T.</returns>
    protected abstract TData[] GetArray();

    /// <summary>Gets the non existent element.</summary>
    /// <returns>A value of T.</returns>
    protected abstract TData GetNonExistentElement();

    /// <summary>Gets the list.</summary>
    /// <returns>A value of T.</returns>
    protected List<TData> GetList() =>
        [.. GetArray()];
}
