// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>Tests the NModbusTcpMasterNModbusTcpSlaveFixture behavior.</summary>
/// <seealso cref="ModbusRxMasterFixture" />
[TUnit.Core.InheritsTests]
public class ModbusRxTcpMasterNModbusTcpSlaveFixture : ModbusRxMasterFixture
{
    /// <summary>Initializes a new instance of the <see cref="ModbusRxTcpMasterNModbusTcpSlaveFixture"/> class.</summary>
    public ModbusRxTcpMasterNModbusTcpSlaveFixture() => InitializeAsync().GetAwaiter().GetResult();

    /// <summary>Initializes the TCP connections asynchronously with CI-safe port allocation.</summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task InitializeAsync()
    {
        // Use dynamic port allocation to avoid conflicts in CI
        var port = await GetAvailablePortAsync();

        SlaveTcp = new(TcpHost, port);
        SlaveTcp.Start();
        RegisterDisposable(new TcpListenerDisposable(SlaveTcp));

        Slave = ModbusTcpSlave.CreateTcp(SlaveAddress, SlaveTcp);
        StartSlave();

        // Give slave time to start listening
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(200)), CancellationToken);

        MasterTcp = new(TcpHost.ToString(), port);
        Master = ModbusIpMaster.CreateIp(MasterTcp);
    }

    /// <summary>Helper class to properly dispose TcpListener.</summary>
    /// <param name="listener">The TCP listener to dispose.</param>
    private sealed class TcpListenerDisposable(TcpListener listener) : IDisposable
    {
        /// <summary>A value indicating whether the listener has been disposed.</summary>
        private bool _disposed;

        /// <summary>Disposes the TCP listener.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                listener?.Stop();
            }
            catch (SocketException)
            {
                // Expected during listener cleanup.
            }
            catch (ObjectDisposedException)
            {
                // Expected during listener cleanup.
            }

            _disposed = true;
        }
    }
}
