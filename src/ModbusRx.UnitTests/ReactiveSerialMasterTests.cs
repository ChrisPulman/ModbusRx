// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using ModbusRx.Reactive;

namespace ModbusRx.UnitTests;

/// <summary>Tests for reactive serial masters (RTU/ASCII) in Create.</summary>
public class ReactiveSerialMasterTests
{
    /// <summary>The original connection interval restored after each fixture instance.</summary>
    private readonly TimeSpan _origInterval;

    /// <summary>Initializes a new instance of the <see cref="ReactiveSerialMasterTests"/> class.</summary>
    public ReactiveSerialMasterTests()
    {
        _origInterval = Create.CheckConnectionInterval;
        Create.CheckConnectionInterval = TimeSpan.FromMilliseconds(50); // speed up tests
    }

    /// <summary>Finalizes an instance of the <see cref="ReactiveSerialMasterTests"/> class. Restores original check-connection interval.</summary>
    ~ReactiveSerialMasterTests()
    {
        Create.CheckConnectionInterval = _origInterval;
    }

    /// <summary>Verifies that the reactive RTU master stream emits a status tuple upon subscription.</summary>
    /// <returns>A task.</returns>
    [TUnit.Core.Test]
    public async Task SerialRtuMaster_Subscribe_ShouldEmitStatus()
    {
        // Arrange
        var emitted = false;

        // Act
        using var sub = Create.SerialRtuMaster("COM_DOES_NOT_EXIST")
            .Take(1)
            .Timeout(TimeSpan.FromSeconds(2))
            .Subscribe(_ => emitted = true);

        await Task.Delay(200);

        // Assert
        Assert.True(emitted);
    }

    /// <summary>Verifies that the reactive ASCII master stream emits a status tuple upon subscription.</summary>
    /// <returns>A task.</returns>
    [TUnit.Core.Test]
    public async Task SerialAsciiMaster_Subscribe_ShouldEmitStatus()
    {
        // Arrange
        var emitted = false;

        // Act
        using var sub = Create.SerialAsciiMaster("COM_DOES_NOT_EXIST")
            .Take(1)
            .Timeout(TimeSpan.FromSeconds(2))
            .Subscribe(_ => emitted = true);

        await Task.Delay(200);

        // Assert
        Assert.True(emitted);
    }
}
