// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if NET8_0_OR_GREATER
using System;
using System.Threading.Tasks;
using ModbusRx.Device;
using ModbusRx.Reactive;

namespace ModbusRx.UnitTests;

/// <summary>Tests for async observable adapters.</summary>
public class ReactiveAsyncObservableTests
{
    /// <summary>Verifies that an IP master read stream can be consumed as an async observable.</summary>
    /// <returns>A task.</returns>
    [TUnit.Core.Test]
    public async Task ReadHoldingRegistersObservable_DisconnectedSource_EmitsErrorTuple()
    {
        var error = new InvalidOperationException("offline");
        var source = Observable.Return((false, (Exception?)error, (ModbusIpMaster?)null));

        var result = await source
            .ReadHoldingRegistersObservable(0, 1, 10)
            .ToObservable()
            .FirstAsync();

        Assert.Null(result.data);
        Assert.Same(error, result.error);
    }

    /// <summary>Verifies async source overloads bridge back through existing polling operators.</summary>
    /// <returns>A task.</returns>
    [TUnit.Core.Test]
    public async Task ReadInputs_WithAsyncSource_EmitsErrorTuple()
    {
        var error = new InvalidOperationException("offline");
        var source = Observable
            .Return((false, (Exception?)error, (ModbusIpMaster?)null))
            .ToModbusObservable();

        var result = await source
            .ReadInputs(0, 1, 10)
            .ToObservable()
            .FirstAsync();

        Assert.Null(result.data);
        Assert.Same(error, result.error);
    }
}
#endif
