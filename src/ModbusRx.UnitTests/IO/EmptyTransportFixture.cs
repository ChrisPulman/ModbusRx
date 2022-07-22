// <copyright file="EmptyTransportFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
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
    [Fact]
    public static void Negative()
    {
        var transport = new EmptyTransport();
        Assert.ThrowsAsync<NotImplementedException>(() => transport.ReadRequest());
        Assert.ThrowsAsync<NotImplementedException>(() => transport.ReadResponse<ReadCoilsInputsResponse>());
        Assert.Throws<NotImplementedException>(() => transport.BuildMessageFrame(null!));
        Assert.Throws<NotImplementedException>(() => transport.Write(null!));
        Assert.Throws<NotImplementedException>(() => transport.OnValidateResponse(null!, null!));
    }
}
