// <copyright file="WriteSingleCoilRequestResponseFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.Message;

/// <summary>
/// WriteSingleCoilRequestResponseFixture.
/// </summary>
public class WriteSingleCoilRequestResponseFixture
{
    /// <summary>
    /// Creates new writesinglecoilrequestresponse.
    /// </summary>
    [Fact]
    public void NewWriteSingleCoilRequestResponse()
    {
        var request = new WriteSingleCoilRequestResponse(11, 5, true);
        Assert.Equal(11, request.SlaveAddress);
        Assert.Equal(5, request.StartAddress);
        Assert.Single(request.Data);
        Assert.Equal(Modbus.CoilOn, request.Data[0]);
    }

    /// <summary>
    /// Converts to string_true.
    /// </summary>
    [Fact]
    public void ToString_True()
    {
        var request = new WriteSingleCoilRequestResponse(11, 5, true);

        Assert.Equal("Write single coil 1 at address 5.", request.ToString());
    }

    /// <summary>
    /// Converts to string_false.
    /// </summary>
    [Fact]
    public void ToString_False()
    {
        var request = new WriteSingleCoilRequestResponse(11, 5, false);

        Assert.Equal("Write single coil 0 at address 5.", request.ToString());
    }
}
