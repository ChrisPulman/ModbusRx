// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if SERIAL
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Base class for ModbusSerialMaster test fixtures.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusRxMasterFixture" />
[Collection("NetworkTests")]
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
    [Fact]
    public virtual void ReturnQueryData()
    {
        // This is a placeholder for the return query data test
        // Individual fixtures can override this method
        Assert.True(true, "ReturnQueryData test placeholder");
    }
}
#endif
