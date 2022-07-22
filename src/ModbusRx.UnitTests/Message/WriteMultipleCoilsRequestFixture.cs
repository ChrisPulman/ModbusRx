// <copyright file="WriteMultipleCoilsRequestFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using ModbusRx.Data;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// WriteMultipleCoilsRequestFixture.
/// </summary>
public class WriteMultipleCoilsRequestFixture
{
    /// <summary>
    /// Creates the write multiple coils request.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleCoilsRequest()
    {
        var col = new DiscreteCollection(true, false, true, false, true, true, true, false, false);
        var request = new WriteMultipleCoilsRequest(34, 45, col);
        Assert.Equal(Modbus.WriteMultipleCoils, request.FunctionCode);
        Assert.Equal(34, request.SlaveAddress);
        Assert.Equal(45, request.StartAddress);
        Assert.Equal(9, request.NumberOfPoints);
        Assert.Equal(2, request.ByteCount);
        Assert.Equal(col.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>
    /// Creates the write multiple coils request too much data.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleCoilsRequestTooMuchData() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WriteMultipleCoilsRequest(1, 2, MessageUtility.CreateDefaultCollection<DiscreteCollection, bool>(true, Modbus.MaximumDiscreteRequestResponseSize + 1)));

    /// <summary>
    /// Creates the maximum size of the write multiple coils request.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleCoilsRequestMaxSize()
    {
        var request = new WriteMultipleCoilsRequest(1, 2, MessageUtility.CreateDefaultCollection<DiscreteCollection, bool>(true, Modbus.MaximumDiscreteRequestResponseSize));

        Assert.Equal(Modbus.MaximumDiscreteRequestResponseSize, request.Data.Count);
    }

    /// <summary>
    /// Converts to string_writemultiplecoilsrequest.
    /// </summary>
    [Fact]
    public void ToString_WriteMultipleCoilsRequest()
    {
        var col = new DiscreteCollection(true, false, true, false, true, true, true, false, false);
        var request = new WriteMultipleCoilsRequest(34, 45, col);

        Assert.Equal("Write 9 coils starting at address 45.", request.ToString());
    }
}
