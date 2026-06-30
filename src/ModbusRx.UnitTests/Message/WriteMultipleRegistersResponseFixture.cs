// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the WriteMultipleRegistersResponseFixture behavior.</summary>
public class WriteMultipleRegistersResponseFixture
{
    /// <summary>Creates the write multiple registers response.</summary>
    [TUnit.Core.Test]
    public void CreateWriteMultipleRegistersResponse()
    {
        var response = new WriteMultipleRegistersResponse(12, 39, 2);
        Assert.Equal(Modbus.WriteMultipleRegisters, response.FunctionCode);
        Assert.Equal(12, response.SlaveAddress);
        Assert.Equal(39, response.StartAddress);
        Assert.Equal(2, response.NumberOfPoints);
    }

    /// <summary>Creates the write multiple registers response too much data.</summary>
    [TUnit.Core.Test]
    public void CreateWriteMultipleRegistersResponseTooMuchData() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new WriteMultipleRegistersResponse(1, 2, Modbus.MaximumRegisterRequestResponseSize + 1));

    /// <summary>Creates the maximum size of the write multiple registers response.</summary>
    [TUnit.Core.Test]
    public void CreateWriteMultipleRegistersResponseMaxSize()
    {
        var response = new WriteMultipleRegistersResponse(1, 2, Modbus.MaximumRegisterRequestResponseSize);
        Assert.Equal(Modbus.MaximumRegisterRequestResponseSize, response.NumberOfPoints);
    }

    /// <summary>Converts to string_test.</summary>
    [TUnit.Core.Test]
    public void ToString_Test()
    {
        var response = new WriteMultipleRegistersResponse(1, 2, 3);

        Assert.Equal("Wrote 3 holding registers starting at address 2.", response.ToString());
    }
}
