// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Test collection for network-related tests that cannot run in parallel.
/// These tests involve TCP/UDP sockets and network resources that must be isolated.
/// </summary>
[CollectionDefinition("NetworkTests", DisableParallelization = true)]
public class NetworkTestCollection;
