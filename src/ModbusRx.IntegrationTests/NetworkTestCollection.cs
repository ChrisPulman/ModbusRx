// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Test collection for network-related tests that cannot run in parallel.
/// These tests involve TCP/UDP sockets and network resources that must be isolated.
/// </summary>
public static class NetworkTestCollection
{
    /// <summary>Gets the collection name.</summary>
    public static string Name => nameof(NetworkTestCollection);
}
