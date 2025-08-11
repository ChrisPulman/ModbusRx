// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Test collection for simulation tests that can run in parallel.
/// These tests don't involve network resources and are safe to run concurrently.
/// </summary>
[CollectionDefinition("SimulationTests")]
public class SimulationTestCollection;
