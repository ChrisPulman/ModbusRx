// <copyright file="UdpClientAdapterFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using CP.IO.Ports;
using ModbusRx.IO;
using Xunit;

namespace ModbusRx.UnitTests.IO;

/// <summary>
/// UdpClientAdapterFixture.
/// </summary>
public class UdpClientAdapterFixture
{
    /// <summary>
    /// Reads the argument validation.
    /// </summary>
    [Fact]
    public void Read_ArgumentValidation()
    {
        var adapter = new UdpClientAdapter(new UdpClientRx());

        // buffer
        Assert.ThrowsAsync<ArgumentNullException>(() => adapter.ReadAsync(null!, 1, 1));

        // offset
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], -1, 2));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], 3, 3));

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], 0, -1));
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], 1, 2));
    }

    /// <summary>
    /// Writes the argument validation.
    /// </summary>
    [Fact]
    public void Write_ArgumentValidation()
    {
        var adapter = new UdpClientAdapter(new UdpClientRx());

        // buffer
        Assert.Throws<ArgumentNullException>(() => adapter.Write(null!, 1, 1));

        // offset
        Assert.Throws<ArgumentOutOfRangeException>(() => adapter.Write(new byte[2], -1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => adapter.Write(new byte[2], 3, 3));
    }
}
