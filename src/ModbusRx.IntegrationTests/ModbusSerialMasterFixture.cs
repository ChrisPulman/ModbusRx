// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// ModbusSerialMasterFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusMasterFixture" />
public abstract class ModbusSerialMasterFixture : ModbusMasterFixture
{
    /// <summary>
    /// Returns the query data.
    /// </summary>
    [Fact]
    public virtual void ReturnQueryData()
    {
        Assert.True(((ModbusSerialMaster)Master!).ReturnQueryData(SlaveAddress, 18));
        Assert.True(((ModbusSerialMaster)Master).ReturnQueryData(SlaveAddress, 5));
    }
}
