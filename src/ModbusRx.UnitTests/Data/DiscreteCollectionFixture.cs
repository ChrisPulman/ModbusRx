// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using ModbusRx.Data;
using Xunit;

namespace ModbusRx.UnitTests.Data;

/// <summary>
/// DiscreteCollectionFixture.
/// </summary>
public class DiscreteCollectionFixture
{
    /// <summary>
    /// Bytes the count.
    /// </summary>
    [Fact]
    public void ByteCount()
    {
        var col = new DiscreteCollection(true, true, false, false, false, false, false, false, false);
        Assert.Equal(2, col.ByteCount);
    }

    /// <summary>
    /// Bytes the count even.
    /// </summary>
    [Fact]
    public void ByteCountEven()
    {
        var col = new DiscreteCollection(true, true, false, false, false, false, false, false);
        Assert.Equal(1, col.ByteCount);
    }

    /// <summary>
    /// Networks the bytes.
    /// </summary>
    [Fact]
    public void NetworkBytes()
    {
        var col = new DiscreteCollection(true, true);
        Assert.Equal(new byte[] { 3 }, col.NetworkBytes);
    }

    /// <summary>
    /// Creates the new discrete collection initialize.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionInitialize()
    {
        var col = new DiscreteCollection(true, true, true);
        Assert.Equal(3, col.Count);
        Assert.DoesNotContain(false, col);
    }

    /// <summary>
    /// Creates the new discrete collection from bool parameters.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionFromBoolParams()
    {
        var col = new DiscreteCollection(true, false, true);
        Assert.Equal(3, col.Count);
    }

    /// <summary>
    /// Creates the new discrete collection from bytes parameters.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionFromBytesParams()
    {
        var col = new DiscreteCollection(1, 2, 3);
        Assert.Equal(24, col.Count);
        var expected = new bool[]
        {
            true, false, false, false, false, false, false, false,
            false, true, false, false, false, false, false, false,
            true, true, false, false, false, false, false, false,
        };

        Assert.Equal(expected, col);
    }

    /// <summary>
    /// Creates the new discrete collection from bytes parameters zero length array.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionFromBytesParams_ZeroLengthArray()
    {
        var col = new DiscreteCollection(Array.Empty<byte>());
        Assert.Empty(col);
    }

    /// <summary>
    /// Creates the new discrete collection from bytes parameters null array.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionFromBytesParams_NullArray() =>
        Assert.Throws<ArgumentNullException>(() => new DiscreteCollection((byte[])null!));

    /// <summary>
    /// Creates the new discrete collection from bytes parameters order.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionFromBytesParamsOrder()
    {
        var col = new DiscreteCollection(194);
        Assert.Equal(new bool[] { false, true, false, false, false, false, true, true }, col.ToArray());
    }

    /// <summary>
    /// Creates the new discrete collection from bytes parameters order2.
    /// </summary>
    [Fact]
    public void CreateNewDiscreteCollectionFromBytesParamsOrder2()
    {
        var col = new DiscreteCollection(157, 7);
        Assert.Equal(
            new bool[]
            {
                true, false, true, true, true, false, false, true, true, true, true, false, false, false, false, false,
            },
            col.ToArray());
    }

    /// <summary>
    /// Resizes this instance.
    /// </summary>
    [Fact]
    public void Resize()
    {
        var col = new DiscreteCollection(byte.MaxValue, byte.MaxValue);
        Assert.Equal(16, col.Count);
        col.RemoveAt(3);
        Assert.Equal(15, col.Count);
    }

    /// <summary>
    /// Byteses the persistence.
    /// </summary>
    [Fact]
    public void BytesPersistence()
    {
        var col = new DiscreteCollection(byte.MaxValue, byte.MaxValue);
        Assert.Equal(16, col.Count);
        var originalBytes = col.NetworkBytes;
        col.RemoveAt(3);
        Assert.Equal(15, col.Count);
        Assert.NotEqual(originalBytes, col.NetworkBytes);
    }

    /// <summary>
    /// Adds the coil.
    /// </summary>
    [Fact]
    public void AddCoil()
    {
        var col = new DiscreteCollection();
        Assert.Empty(col);

        col.Add(true);
        Assert.Single(col);
    }
}
