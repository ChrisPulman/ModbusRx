// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using ModbusRx.Data;
using Xunit;

namespace ModbusRx.UnitTests.Data;

/// <summary>
/// DataStoreEventArgsFixture.
/// </summary>
public class DataStoreEventArgsFixture
{
    /// <summary>
    /// Creates the data store event arguments.
    /// </summary>
    [Fact]
    public void CreateDataStoreEventArgs()
    {
        var eventArgs = DataStoreEventArgs.CreateDataStoreEventArgs(5, ModbusDataType.HoldingRegister, new ushort[] { 1, 2, 3 });
        Assert.Equal(ModbusDataType.HoldingRegister, eventArgs.ModbusDataType);
        Assert.Equal(5, eventArgs.StartAddress);
        Assert.Equal(new ushort[] { 1, 2, 3 }, eventArgs.Data!.B?.ToArray());
    }

    /// <summary>
    /// Creates the type of the data store event arguments invalid.
    /// </summary>
    [Fact]
    public void CreateDataStoreEventArgs_InvalidType() =>
        Assert.Throws<ArgumentException>(() => DataStoreEventArgs.CreateDataStoreEventArgs(5, ModbusDataType.HoldingRegister, new int[] { 1, 2, 3 }));

    /// <summary>
    /// Creates the data store event arguments data null.
    /// </summary>
    [Fact]
    public void CreateDataStoreEventArgs_DataNull() =>
        Assert.Throws<ArgumentNullException>(() =>
            DataStoreEventArgs.CreateDataStoreEventArgs(5, ModbusDataType.HoldingRegister, default(ushort[])!));
}
