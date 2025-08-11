// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using CP.IO.Ports;
using ModbusRx.Device;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// NModbusTcpMasterNModbusTcpSlaveFixture.
/// </summary>
/// <seealso cref="ModbusRx.IntegrationTests.ModbusRxMasterFixture" />
public class ModbusRxTcpMasterNModbusTcpSlaveFixture : ModbusRxMasterFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusRxTcpMasterNModbusTcpSlaveFixture"/> class.
    /// </summary>
    public ModbusRxTcpMasterNModbusTcpSlaveFixture()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes the TCP connections asynchronously with CI-safe port allocation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task InitializeAsync()
    {
        // Use dynamic port allocation to avoid conflicts in CI
        var port = await GetAvailablePortAsync();
        
        SlaveTcp = new TcpListener(TcpHost, port);
        SlaveTcp.Start();
        RegisterDisposable(new TcpListenerDisposable(SlaveTcp));
        
        Slave = ModbusTcpSlave.CreateTcp(SlaveAddress, SlaveTcp);
        StartSlave();

        // Give slave time to start listening
        await Task.Delay(GetEnvironmentAppropriateTimeout(TimeSpan.FromMilliseconds(200)), CancellationToken);

        MasterTcp = new TcpClientRx(TcpHost.ToString(), port);
        Master = ModbusIpMaster.CreateIp(MasterTcp);
    }

    /// <summary>
    /// Helper class to properly dispose TcpListener.
    /// </summary>
    private class TcpListenerDisposable : IDisposable
    {
        private readonly TcpListener _listener;
        private bool _disposed;

        public TcpListenerDisposable(TcpListener listener)
        {
            _listener = listener;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _listener?.Stop();
                }
                catch
                {
                    // Ignore cleanup exceptions
                }

                _disposed = true;
            }
        }
    }
}
