// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModbusRx.Data;
using Xunit;

namespace ModbusRx.UnitTests.Data;

/// <summary>
/// ModbusDataCollectionFixture.
/// </summary>
/// <typeparam name="TData">The type of the data.</typeparam>
public abstract class ModbusDataCollectionFixture<TData>
    where TData : struct
{
    /// <summary>
    /// Defaults the contstructor.
    /// </summary>
    [Fact]
    public void DefaultContstructor()
    {
        var col = new ModbusDataCollection<TData>();
        Assert.NotEmpty(col);
        Assert.Single(col);

        col.Add(default!);
        Assert.Equal(2, col.Count);
    }

    /// <summary>
    /// Contstructors the with parameters.
    /// </summary>
    [Fact]
    public void ContstructorWithParams()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<TData>(source);
        Assert.Equal(source.Length + 1, col.Count);
        Assert.NotEmpty(col);

        col.Add(default!);
        Assert.Equal(source.Length + 2, col.Count);
    }

    /// <summary>
    /// Contstructors the with i list.
    /// </summary>
    [Fact]
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

    /// <summary>
    /// Contstructors the with i list from read only list.
    /// </summary>
    [Fact]
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

    /// <summary>
    /// Sets the zero element using item.
    /// </summary>
    [Fact]
    public void SetZeroElementUsingItem()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<TData>(source);
        Assert.Throws<ArgumentOutOfRangeException>(() => col[0] = source[3]);
    }

    /// <summary>
    /// Zeroes the element using item negative.
    /// </summary>
    [Fact]
    public void ZeroElementUsingItem_Negative()
    {
        var source = GetArray();
        var col = new ModbusDataCollection<TData>(source);

        Assert.Throws<ArgumentOutOfRangeException>(() => col[0] = source[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => col.Insert(0, source[3]));
        Assert.Throws<ArgumentOutOfRangeException>(() => col.RemoveAt(0));

        // Remove forst zero/false
        Assert.Throws<ArgumentOutOfRangeException>(() => col.Remove(default!));
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    [Fact]
    public void Clear()
    {
        var col = new ModbusDataCollection<TData>(GetArray());
        col.Clear();

        Assert.Single(col);
    }

    /// <summary>
    /// Removes this instance.
    /// </summary>
    [Fact]
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

    /// <summary>
    /// Gets the array.
    /// </summary>
    /// <returns>A value of T.</returns>
    protected abstract TData[] GetArray();

    /// <summary>
    /// Gets the non existent element.
    /// </summary>
    /// <returns>A value of T.</returns>
    protected abstract TData GetNonExistentElement();

    /// <summary>
    /// Gets the list.
    /// </summary>
    /// <returns>A value of T.</returns>
    protected List<TData> GetList() =>
        new(GetArray());
}
