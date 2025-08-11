// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Data;
using Xunit;

namespace ModbusRx.UnitTests.Data;

/// <summary>
/// RegisterCollectionFixture.
/// </summary>
public class RegisterCollectionFixture
{
    /// <summary>
    /// Bytes the count.
    /// </summary>
    [Fact]
    public void ByteCount()
    {
        var col = new RegisterCollection(1, 2, 3);
        Assert.Equal(6, col.ByteCount);
    }

    /// <summary>
    /// Creates new registercollection.
    /// </summary>
    [Fact]
    public void NewRegisterCollection()
    {
        var col = new RegisterCollection(5, 3, 4, 6);
        Assert.NotNull(col);
        Assert.Equal(4, col.Count);
        Assert.Equal(5, col[0]);
    }

    /// <summary>
    /// Creates new registercollectionfrombytes.
    /// </summary>
    [Fact]
    public void NewRegisterCollectionFromBytes()
    {
        var col = new RegisterCollection([0, 1, 0, 2, 0, 3]);
        Assert.NotNull(col);
        Assert.Equal(3, col.Count);
        Assert.Equal(1, col[0]);
        Assert.Equal(2, col[1]);
        Assert.Equal(3, col[2]);
    }

    /// <summary>
    /// Registers the collection network bytes.
    /// </summary>
    [Fact]
    public void RegisterCollectionNetworkBytes()
    {
        var col = new RegisterCollection(5, 3, 4, 6);
        var bytes = col.NetworkBytes;
        Assert.NotNull(bytes);
        Assert.Equal(8, bytes.Length);
        Assert.Equal([0, 5, 0, 3, 0, 4, 0, 6], bytes);
    }

    /// <summary>
    /// Registers the collection empty.
    /// </summary>
    [Fact]
    public void RegisterCollectionEmpty()
    {
        var col = new RegisterCollection();
        Assert.NotNull(col);
        Assert.Empty(col.NetworkBytes);
    }

    /// <summary>
    /// Modifies the register.
    /// </summary>
    [Fact]
    public void ModifyRegister()
    {
        var col = new RegisterCollection(1, 2, 3, 4)
        {
            [0] = 5
        };
    }

    /// <summary>
    /// Adds the register.
    /// </summary>
    [Fact]
    public void AddRegister()
    {
        var col = new RegisterCollection();
        Assert.Empty(col);

        col.Add(45);
        Assert.Single(col);
    }

    /// <summary>
    /// Removes the register.
    /// </summary>
    [Fact]
    public void RemoveRegister()
    {
        var col = new RegisterCollection(3, 4, 5);
        Assert.Equal(3, col.Count);
        col.RemoveAt(2);
        Assert.Equal(2, col.Count);
    }
}
