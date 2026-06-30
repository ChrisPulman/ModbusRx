// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using ModbusRx.Data;
using ModbusRx.IO;
using ModbusRx.Message;

namespace ModbusRx.UnitTests.IO;

/// <summary>Tests empty transport behavior.</summary>
public class EmptyTransportFixture
{
    /// <summary>The single-coil successful response data.</summary>
    private static readonly bool[] SingleTrueCoil = [true];

    /// <summary>Negatives this instance.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [TUnit.Core.Test]
    public async Task NegativeAsync()
    {
        var transport = new EmptyTransport();
        await Assert.ThrowsAsync<InvalidOperationException>(() => transport.ReadRequest());
        await Assert.ThrowsAsync<InvalidOperationException>(() => transport.ReadResponse<ReadCoilsInputsResponse>());

        var request = new ReadCoilsInputsRequest(Modbus.ReadCoils, 1, 0, 1);
        var response = new ReadCoilsInputsResponse(Modbus.ReadCoils, 1, 1, new DiscreteCollection(SingleTrueCoil));

        Assert.Equal(request.MessageFrame, transport.BuildMessageFrame(request));
        transport.Write(request);
        transport.OnValidateResponse(request, response);
    }
}
