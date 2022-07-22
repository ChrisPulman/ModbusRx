// <copyright file="TcpConnectionEventArgsFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.UnitTests.Device;

/// <summary>
/// TcpConnectionEventArgsFixture.
/// </summary>
public class TcpConnectionEventArgsFixture
{
    /// <summary>
    /// TCPs the connection event arguments null end point.
    /// </summary>
    [Fact]
    public void TcpConnectionEventArgs_NullEndPoint() =>
        Assert.Throws<ArgumentNullException>(() => new TcpConnectionEventArgs(null!));

    /// <summary>
    /// TCPs the connection event arguments empty end point.
    /// </summary>
    [Fact]
    public void TcpConnectionEventArgs_EmptyEndPoint() =>
        Assert.Throws<ArgumentException>(() => new TcpConnectionEventArgs(string.Empty));

    /// <summary>
    /// TCPs the connection event arguments.
    /// </summary>
    [Fact]
    public void TcpConnectionEventArgs()
    {
        var args = new TcpConnectionEventArgs("foo");

        Assert.Equal("foo", args.EndPoint);
    }
}
