// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using ModbusRx.Data;

namespace ModbusRx.UnitTests.Data;

/// <summary>Tests the DataStoreEventArgsFixture behavior.</summary>
public class DataStoreEventArgsFixture
{
    /// <summary>Creates the data store event arguments.</summary>
    [TUnit.Core.Test]
    public void CreateDataStoreEventArgs()
    {
        var eventArgs = DataStoreEventArgs.CreateDataStoreEventArgs(5, ModbusDataType.HoldingRegister, [(ushort)1, (ushort)2, (ushort)3]);
        Assert.Equal(ModbusDataType.HoldingRegister, eventArgs.ModbusDataType);
        Assert.Equal(5, eventArgs.StartAddress);
        Assert.Equal<IEnumerable<ushort>>([(ushort)1, (ushort)2, (ushort)3], Assert.NotNull(eventArgs.Data!.B));
    }

    /// <summary>Creates the type of the data store event arguments invalid.</summary>
    [TUnit.Core.Test]
    public void CreateDataStoreEventArgs_InvalidType() =>
        Assert.Throws<ArgumentException>(() => DataStoreEventArgs.CreateDataStoreEventArgs(5, ModbusDataType.HoldingRegister, [ 1, 2, 3]));

    /// <summary>Creates the data store event arguments data null.</summary>
    [TUnit.Core.Test]
    public void CreateDataStoreEventArgs_DataNull() =>
        Assert.Throws<ArgumentNullException>(() =>
            DataStoreEventArgs.CreateDataStoreEventArgs(5, ModbusDataType.HoldingRegister, default(ushort[])!));
}
