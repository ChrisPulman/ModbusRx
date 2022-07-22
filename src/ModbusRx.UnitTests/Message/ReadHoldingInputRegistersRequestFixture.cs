// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// ReadHoldingInputRegistersRequestFixture.
/// </summary>
public class ReadHoldingInputRegistersRequestFixture
{
    /// <summary>
    /// Creates the read holding registers request.
    /// </summary>
    [Fact]
    public void CreateReadHoldingRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 5, 1, 10);

        Assert.Equal(Modbus.ReadHoldingRegisters, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(1, request.StartAddress);
        Assert.Equal(10, request.NumberOfPoints);
    }

    /// <summary>
    /// Creates the read input registers request.
    /// </summary>
    [Fact]
    public void CreateReadInputRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadInputRegisters, 5, 1, 10);

        Assert.Equal(Modbus.ReadInputRegisters, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(1, request.StartAddress);
        Assert.Equal(10, request.NumberOfPoints);
    }

    /// <summary>
    /// Creates the read holding input registers request too much data.
    /// </summary>
    [Fact]
    public void CreateReadHoldingInputRegistersRequestTooMuchData() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 2, Modbus.MaximumRegisterRequestResponseSize + 1));

    /// <summary>
    /// Creates the maximum size of the read holding input registers request.
    /// </summary>
    [Fact]
    public void CreateReadHoldingInputRegistersRequestMaxSize()
    {
        var response = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 2, Modbus.MaximumRegisterRequestResponseSize);

        Assert.Equal(Modbus.MaximumRegisterRequestResponseSize, response.NumberOfPoints);
    }

    /// <summary>
    /// Converts to string_readholdingregistersrequest.
    /// </summary>
    [Fact]
    public void ToString_ReadHoldingRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 5, 1, 10);

        Assert.Equal("Read 10 holding registers starting at address 1.", request.ToString());
    }

    /// <summary>
    /// Converts to string_readinputregistersrequest.
    /// </summary>
    [Fact]
    public void ToString_ReadInputRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadInputRegisters, 5, 1, 10);

        Assert.Equal("Read 10 input registers starting at address 1.", request.ToString());
    }
}
