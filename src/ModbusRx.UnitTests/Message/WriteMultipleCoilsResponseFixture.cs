// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// WriteMultipleCoilsResponseFixture.
/// </summary>
public class WriteMultipleCoilsResponseFixture
{
    /// <summary>
    /// Creates the write multiple coils response.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleCoilsResponse()
    {
        var response = new WriteMultipleCoilsResponse(17, 19, 45);
        Assert.Equal(Modbus.WriteMultipleCoils, response.FunctionCode);
        Assert.Equal(17, response.SlaveAddress);
        Assert.Equal(19, response.StartAddress);
        Assert.Equal(45, response.NumberOfPoints);
    }

    /// <summary>
    /// Creates the write multiple coils response too much data.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleCoilsResponseTooMuchData() => Assert.Throws<ArgumentOutOfRangeException>(() => new WriteMultipleCoilsResponse(1, 2, Modbus.MaximumDiscreteRequestResponseSize + 1));

    /// <summary>
    /// Creates the maximum size of the write multiple coils response.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleCoilsResponseMaxSize()
    {
        var response = new WriteMultipleCoilsResponse(1, 2, Modbus.MaximumDiscreteRequestResponseSize);
        Assert.Equal(Modbus.MaximumDiscreteRequestResponseSize, response.NumberOfPoints);
    }

    /// <summary>
    /// Converts to string_test.
    /// </summary>
    [Fact]
    public void ToString_Test()
    {
        var response = new WriteMultipleCoilsResponse(1, 2, 3);

        Assert.Equal("Wrote 3 coils starting at address 2.", response.ToString());
    }
}
