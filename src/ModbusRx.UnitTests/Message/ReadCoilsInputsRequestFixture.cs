// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the ReadCoilsInputsRequestFixture behavior.</summary>
public class ReadCoilsInputsRequestFixture
{
    /// <summary>Creates the read coils request.</summary>
    [TUnit.Core.Test]
    public void CreateReadCoilsRequest()
    {
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 5, 1, 10);
        Assert.Equal(Modbus.ReadCoils, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(1, request.StartAddress);
        Assert.Equal(10, request.NumberOfPoints);
    }

    /// <summary>Creates the read inputs request.</summary>
    [TUnit.Core.Test]
    public void CreateReadInputsRequest()
    {
        var request = new ReadCoilsInputsRequest(Modbus.ReadInputs, 5, 1, 10);
        Assert.Equal(Modbus.ReadInputs, request.FunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(1, request.StartAddress);
        Assert.Equal(10, request.NumberOfPoints);
    }

    /// <summary>Creates the read coils inputs request too much data.</summary>
    [TUnit.Core.Test]
    public void CreateReadCoilsInputsRequestTooMuchData() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 2, Modbus.MaximumDiscreteRequestResponseSize + 1));

    /// <summary>Creates the maximum size of the read coils inputs request.</summary>
    [TUnit.Core.Test]
    public void CreateReadCoilsInputsRequestMaxSize()
    {
        var response = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 2, Modbus.MaximumDiscreteRequestResponseSize);
        Assert.Equal(Modbus.MaximumDiscreteRequestResponseSize, response.NumberOfPoints);
    }

    /// <summary>Converts to string_readcoilsrequest.</summary>
    [TUnit.Core.Test]
    public void ToString_ReadCoilsRequest()
    {
        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 5, 1, 10);

        Assert.Equal("Read 10 coils starting at address 1.", request.ToString());
    }

    /// <summary>Converts to string_readinputsrequest.</summary>
    [TUnit.Core.Test]
    public void ToString_ReadInputsRequest()
    {
        var request = new ReadCoilsInputsRequest(Modbus.ReadInputs, 5, 1, 10);

        Assert.Equal("Read 10 inputs starting at address 1.", request.ToString());
    }
}
