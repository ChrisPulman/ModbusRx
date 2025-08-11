// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;
using Xunit;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Example integration tests demonstrating CI-safe network testing patterns.
/// </summary>
[Collection("NetworkTests")]
public class CISafeNetworkTests : NetworkTestBase
{
    /// <summary>
    /// Test that requires live network connectivity - skipped in CI environments.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [SkippableFact]
    public async Task LiveNetworkTest_ShouldConnectToRealDevice()
    {
        // Skip this test if running in CI to avoid failures
        Skip.IfNot(!IsRunningInCI, "This test requires a real Modbus device on the network");

        // This test would only run in local development environments
        var canConnect = await TryConnectAsync("192.168.1.100", 502);
        
        Skip.IfNot(canConnect, "No Modbus device found at 192.168.1.100:502");

        // Proceed with actual device testing
        var client = new TcpClientRx("192.168.1.100", 502);
        using var master = ModbusIpMaster.CreateIp(client);
        RegisterDisposable(master);

        var registers = await master.ReadHoldingRegistersAsync(1, 0, 10);
        Assert.NotNull(registers);
        Assert.True(registers.Length > 0);
    }

    /// <summary>
    /// Test that works in both CI and local environments using localhost.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LocalhostTest_ShouldWorkInAllEnvironments()
    {
        // This test uses localhost/loopback only - safe for CI
        var server = new ModbusServer();
        RegisterDisposable(server);

        var testData = new ushort[] { 100, 200, 300, 400, 500 };
        server.LoadSimulationData(testData);

        var tcpPort = await GetAvailablePortAsync();
        server.StartTcpServer(tcpPort, 1);
        server.Start();

        // Use CI-appropriate timeout
        var timeout = GetEnvironmentAppropriateTimeout(TimeSpan.FromSeconds(5));
        await Task.Delay(200, CancellationToken);

        var client = new TcpClientRx("127.0.0.1", tcpPort);
        var master = ModbusIpMaster.CreateIp(client);
        RegisterDisposable(master);

        var result = await master.ReadHoldingRegistersAsync(1, 0, 5);

        Assert.Equal(5, result.Length);
        Assert.Equal(100, result[0]);
        Assert.Equal(500, result[4]);
    }

    /// <summary>
    /// Test that demonstrates conditional behavior based on environment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ConditionalNetworkTest_ShouldAdaptToEnvironment()
    {
        if (IsRunningInGitHubActions)
        {
            // In GitHub Actions, test with mock/simulation only
            var server = new ModbusServer();
            RegisterDisposable(server);
            
            server.SimulationMode = true;
            server.Start();

            await Task.Delay(100, CancellationToken);
            var data = server.GetCurrentData();
            
            Assert.NotNull(data.holdingRegisters);
        }
        else
        {
            // In local environment, can test with more comprehensive scenarios
            var server = new ModbusServer();
            RegisterDisposable(server);

            // Start multiple endpoints
            var tcpPort = await GetAvailablePortAsync();
            var udpPort = await GetAvailablePortAsync();
            
            server.StartTcpServer(tcpPort, 1);
            server.StartUdpServer(udpPort, 1);
            server.Start();

            await Task.Delay(200, CancellationToken);

            // Test TCP connection
            var tcpClient = new TcpClientRx("127.0.0.1", tcpPort);
            var tcpMaster = ModbusIpMaster.CreateIp(tcpClient);
            RegisterDisposable(tcpMaster);

            var tcpResult = await tcpMaster.ReadHoldingRegistersAsync(1, 0, 5);
            Assert.NotNull(tcpResult);

            // Test UDP connection
            var udpClient = new UdpClientRx();
            var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, udpPort);
            udpClient.Connect(endPoint);
            var udpMaster = ModbusIpMaster.CreateIp(udpClient);
            RegisterDisposable(udpMaster);

            var udpResult = await udpMaster.ReadHoldingRegistersAsync(1, 0, 5);
            Assert.NotNull(udpResult);
        }
    }

    /// <summary>
    /// Test that requires external internet connectivity - always skipped in CI.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact(Skip = "Requires external internet connectivity - run manually for development testing")]
    public async Task ExternalConnectivityTest_ManualTestOnly()
    {
        // This test is explicitly skipped but can be enabled manually
        // for development/debugging purposes        
        var canPing = await TryConnectAsync("google.com", 80, TimeSpan.FromSeconds(10));
        Assert.True(canPing, "No internet connectivity available");
        
        // Additional external connectivity tests would go here
    }
}
