// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ModbusRx.Message;

namespace ModbusRx.UnitTests.Message;

/// <summary>Tests the WriteSingleRegisterRequestResponseFixture behavior.</summary>
public class WriteSingleRegisterRequestResponseFixture
{
    /// <summary>Creates new writesingleregisterrequestresponse.</summary>
    [TUnit.Core.Test]
    public void NewWriteSingleRegisterRequestResponse()
    {
        var message = new WriteSingleRegisterRequestResponse(12, 5, 1200);
        Assert.Equal(12, message.SlaveAddress);
        Assert.Equal(5, message.StartAddress);
        _ = Assert.Single(message.Data);
        Assert.Equal(1200, message.Data[0]);
    }

    /// <summary>Converts to stringoverride.</summary>
    [TUnit.Core.Test]
    public void ToStringOverride()
    {
        var message = new WriteSingleRegisterRequestResponse(12, 5, 1200);
        Assert.Equal("Write single holding register 1200 at address 5.", message.ToString());
    }
}
