// <copyright file="WriteMultipleRegistersRequestFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using ModbusRx.Data;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// WriteMultipleRegistersRequestFixture.
/// </summary>
public class WriteMultipleRegistersRequestFixture
{
    /// <summary>
    /// Creates the write multiple registers request fixture.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleRegistersRequestFixture()
    {
        var col = new RegisterCollection(10, 20, 30, 40, 50);
        var request = new WriteMultipleRegistersRequest(11, 34, col);

        Assert.Equal(Modbus.WriteMultipleRegisters, request.FunctionCode);
        Assert.Equal(11, request.SlaveAddress);
        Assert.Equal(34, request.StartAddress);
        Assert.Equal(10, request.ByteCount);
        Assert.Equal(col.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>
    /// Creates the write multiple registers request too much data.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleRegistersRequestTooMuchData() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new WriteMultipleRegistersRequest(1, 2, MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(3, Modbus.MaximumRegisterRequestResponseSize + 1)));

    /// <summary>
    /// Creates the maximum size of the write multiple registers request.
    /// </summary>
    [Fact]
    public void CreateWriteMultipleRegistersRequestMaxSize()
    {
        var request = new WriteMultipleRegistersRequest(1, 2, MessageUtility.CreateDefaultCollection<RegisterCollection, ushort>(3, Modbus.MaximumRegisterRequestResponseSize));

        Assert.Equal(Modbus.MaximumRegisterRequestResponseSize, request.NumberOfPoints);
    }

    /// <summary>
    /// Converts to string_writemultipleregistersrequest.
    /// </summary>
    [Fact]
    public void ToString_WriteMultipleRegistersRequest()
    {
        var col = new RegisterCollection(10, 20, 30, 40, 50);
        var request = new WriteMultipleRegistersRequest(11, 34, col);

        Assert.Equal("Write 5 holding registers starting at address 34.", request.ToString());
    }
}
