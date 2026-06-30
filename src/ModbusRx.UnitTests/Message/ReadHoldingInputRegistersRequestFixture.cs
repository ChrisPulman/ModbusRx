// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the ReadHoldingInputRegistersRequestFixture behavior.</summary>
public class ReadHoldingInputRegistersRequestFixture
{
    /// <summary>Creates the read holding registers request.</summary>
    [TUnit.Core.Test]
    public void CreateReadHoldingRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 5, 1, 10);

        Assert.Equal(Modbus.ReadHoldingRegisters, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(1, request.StartAddress);
        Assert.Equal(10, request.NumberOfPoints);
    }

    /// <summary>Creates the read input registers request.</summary>
    [TUnit.Core.Test]
    public void CreateReadInputRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadInputRegisters, 5, 1, 10);

        Assert.Equal(Modbus.ReadInputRegisters, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(1, request.StartAddress);
        Assert.Equal(10, request.NumberOfPoints);
    }

    /// <summary>Creates the read holding input registers request too much data.</summary>
    [TUnit.Core.Test]
    public void CreateReadHoldingInputRegistersRequestTooMuchData() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _ = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 2, Modbus.MaximumRegisterRequestResponseSize + 1));

    /// <summary>Creates the maximum size of the read holding input registers request.</summary>
    [TUnit.Core.Test]
    public void CreateReadHoldingInputRegistersRequestMaxSize()
    {
        var response = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 1, 2, Modbus.MaximumRegisterRequestResponseSize);

        Assert.Equal(Modbus.MaximumRegisterRequestResponseSize, response.NumberOfPoints);
    }

    /// <summary>Converts to string_readholdingregistersrequest.</summary>
    [TUnit.Core.Test]
    public void ToString_ReadHoldingRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadHoldingRegisters, 5, 1, 10);

        Assert.Equal("Read 10 holding registers starting at address 1.", request.ToString());
    }

    /// <summary>Converts to string_readinputregistersrequest.</summary>
    [TUnit.Core.Test]
    public void ToString_ReadInputRegistersRequest()
    {
        var request = new ReadHoldingInputRegistersRequest(Modbus.ReadInputRegisters, 5, 1, 10);

        Assert.Equal("Read 10 input registers starting at address 1.", request.ToString());
    }
}
