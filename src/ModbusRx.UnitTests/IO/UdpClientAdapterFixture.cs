// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.IO;

namespace ModbusRx.UnitTests.IO;

/// <summary>Tests the UdpClientAdapterFixture behavior.</summary>
public class UdpClientAdapterFixture
{
    /// <summary>Reads the argument validation.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task Read_ArgumentValidationAsync()
    {
        var adapter = new UdpClientAdapter(new UdpClientRx());

        // buffer
        await Assert.ThrowsAsync<ArgumentNullException>(() => adapter.ReadAsync(null!, 1, 1));

        // offset
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], -1, 2));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], 3, 3));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], 0, -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => adapter.ReadAsync(new byte[2], 1, 2));
    }

    /// <summary>Writes the argument validation.</summary>
    [TUnit.Core.Test]
    public void Write_ArgumentValidation()
    {
        var adapter = new UdpClientAdapter(new UdpClientRx());

        // buffer
        _ = Assert.Throws<ArgumentNullException>(() => adapter.Write(null!, 1, 1));

        // offset
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => adapter.Write(new byte[2], -1, 2));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => adapter.Write(new byte[2], 3, 3));
    }
}
