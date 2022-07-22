// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ModbusRx.Data;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// DiagnosticsRequestResponseFixture.
/// </summary>
public class DiagnosticsRequestResponseFixture
{
    /// <summary>
    /// Converts to string_test.
    /// </summary>
    [Fact]
    public void ToString_Test()
    {
        DiagnosticsRequestResponse response;

        response = new DiagnosticsRequestResponse(Modbus.DiagnosticsReturnQueryData, 3, new RegisterCollection(5));
        Assert.Equal("Diagnostics message, sub-function return query data - {5}.", response.ToString());
    }
}
