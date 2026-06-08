// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModbusRx.Device;
using ModbusRx.Reactive;
using ReactiveUI.Extensions.Async;
using Xunit;

namespace ModbusRx.UnitTests;

/// <summary>
/// Tests for ReactiveUI.Extensions async observable adapters.
/// </summary>
public class ReactiveAsyncObservableTests
{
    /// <summary>
    /// Verifies that an IP master read stream can be consumed as an async observable.
    /// </summary>
    /// <returns>A task.</returns>
    [TUnit.Core.Test]
    public async Task ReadHoldingRegistersAsyncObservable_DisconnectedSource_EmitsErrorTuple()
    {
        var error = new InvalidOperationException("offline");
        var source = Observable.Return((false, (Exception?)error, (ModbusIpMaster?)null));

        var result = await source
            .ReadHoldingRegistersAsyncObservable(0, 1, 10)
            .FirstAsync(CancellationToken.None);

        Assert.Null(result.data);
        Assert.Same(error, result.error);
    }

    /// <summary>
    /// Verifies async source overloads bridge back through existing polling operators.
    /// </summary>
    /// <returns>A task.</returns>
    [TUnit.Core.Test]
    public async Task ReadInputs_WithAsyncSource_EmitsErrorTuple()
    {
        var error = new InvalidOperationException("offline");
        var source = Observable
            .Return((false, (Exception?)error, (ModbusIpMaster?)null))
            .ToModbusObservableAsync();

        var result = await source
            .ReadInputs(0, 1, 10)
            .FirstAsync(CancellationToken.None);

        Assert.Null(result.data);
        Assert.Same(error, result.error);
    }
}
#endif
