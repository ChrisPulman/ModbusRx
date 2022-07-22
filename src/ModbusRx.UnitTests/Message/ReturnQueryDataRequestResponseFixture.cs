// <copyright file="ReturnQueryDataRequestResponseFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Data;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// ReturnQueryDataRequestResponseFixture.
/// </summary>
public class ReturnQueryDataRequestResponseFixture
{
    /// <summary>
    /// Returns the query data request response.
    /// </summary>
    [Fact]
    public void ReturnQueryDataRequestResponse()
    {
        var data = new RegisterCollection(1, 2, 3, 4);
        var request = new DiagnosticsRequestResponse(Modbus.DiagnosticsReturnQueryData, 5, data);
        Assert.Equal(Modbus.Diagnostics, request.FunctionCode);
        Assert.Equal(Modbus.DiagnosticsReturnQueryData, request.SubFunctionCode);
        Assert.Equal(5, request.SlaveAddress);
        Assert.Equal(data.NetworkBytes, request.Data.NetworkBytes);
    }

    /// <summary>
    /// Protocols the data unit.
    /// </summary>
    [Fact]
    public void ProtocolDataUnit()
    {
        var data = new RegisterCollection(1, 2, 3, 4);
        var request = new DiagnosticsRequestResponse(Modbus.DiagnosticsReturnQueryData, 5, data);
        Assert.Equal(new byte[] { 8, 0, 0, 0, 1, 0, 2, 0, 3, 0, 4 }, request.ProtocolDataUnit);
    }
}
