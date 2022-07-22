// <copyright file="WriteSingleRegisterRequestResponseFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// WriteSingleRegisterRequestResponseFixture.
/// </summary>
public class WriteSingleRegisterRequestResponseFixture
{
    /// <summary>
    /// Creates new writesingleregisterrequestresponse.
    /// </summary>
    [Fact]
    public void NewWriteSingleRegisterRequestResponse()
    {
        var message = new WriteSingleRegisterRequestResponse(12, 5, 1200);
        Assert.Equal(12, message.SlaveAddress);
        Assert.Equal(5, message.StartAddress);
        Assert.Single(message.Data);
        Assert.Equal(1200, message.Data[0]);
    }

    /// <summary>
    /// Converts to stringoverride.
    /// </summary>
    [Fact]
    public void ToStringOverride()
    {
        var message = new WriteSingleRegisterRequestResponse(12, 5, 1200);
        Assert.Equal("Write single holding register 1200 at address 5.", message.ToString());
    }
}
