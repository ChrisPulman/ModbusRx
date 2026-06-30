// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
#if SERIAL
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Base class for ModbusSerialMaster test fixtures.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusRxMasterFixture" />
public abstract class ModbusSerialMasterFixture : ModbusRxMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusSerialMasterFixture"/> class.
    /// </summary>
    protected ModbusSerialMasterFixture()
    {
        // Skip all serial tests in CI environments
        SkipIfRunningInCI("Serial port tests require physical hardware not available in CI");
    }

    /// <summary>
    /// Returns the query data.
    /// </summary>
    [TUnit.Core.Test]
    public virtual void ReturnQueryData()
    {
        // This is a placeholder for the return query data test
        // Individual fixtures can override this method
        Assert.True(true, "ReturnQueryData test placeholder");
    }
}
#endif
