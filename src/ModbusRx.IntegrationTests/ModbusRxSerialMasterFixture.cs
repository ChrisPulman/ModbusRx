// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>Tests the ModbusSerialMasterFixture behavior.</summary>
/// <seealso cref="ModbusRxMasterFixture" />
[TUnit.Core.InheritsTests]
public abstract class ModbusRxSerialMasterFixture : ModbusRxMasterFixture
{
    /// <summary>Returns the query data.</summary>
    [TUnit.Core.Test]
    public virtual void ReturnQueryData()
    {
        Assert.True(((ModbusSerialMaster)Master!).ReturnQueryData(SlaveAddress, 18));
        Assert.True(((ModbusSerialMaster)Master).ReturnQueryData(SlaveAddress, 5));
    }
}
