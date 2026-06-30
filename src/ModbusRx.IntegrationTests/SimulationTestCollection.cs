// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Test collection for simulation tests that can run in parallel.
/// These tests don't involve network resources and are safe to run concurrently.
/// </summary>
public static class SimulationTestCollection
{
    /// <summary>Gets the collection name.</summary>
    public static string Name => nameof(SimulationTestCollection);
}
