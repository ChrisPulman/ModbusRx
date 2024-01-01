// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using ModbusRx.IO;
using ModbusRx.Message;
using Xunit;

namespace ModbusRx.UnitTests.IO;

/// <summary>
/// EmptyTransportFixture.
/// </summary>
public static class EmptyTransportFixture
{
    /// <summary>
    /// Negatives this instance.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public static async Task NegativeAsync()
    {
        var transport = new EmptyTransport();
        await Assert.ThrowsAsync<NotImplementedException>(() => transport.ReadRequest());
        await Assert.ThrowsAsync<NotImplementedException>(() => transport.ReadResponse<ReadCoilsInputsResponse>());
        Assert.Throws<NotImplementedException>(() => transport.BuildMessageFrame(null!));
        Assert.Throws<NotImplementedException>(() => transport.Write(null!));
        Assert.Throws<NotImplementedException>(() => transport.OnValidateResponse(null!, null!));
    }
}
