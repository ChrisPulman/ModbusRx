// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusRx.IntegrationTests;

/// <summary>Base class for network-related integration tests with proper resource management.</summary>
public abstract class NetworkTestBase : IDisposable
{
    /// <summary>Coordinates port probing across network tests.</summary>
    private static readonly SemaphoreSlim PortSemaphore = new(1, 1);

    /// <summary>The resources registered for cleanup.</summary>
    private readonly List<IDisposable> _disposables = [];

    /// <summary>The cancellation source for test operations.</summary>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>A value indicating whether the base has been disposed.</summary>
    private bool _disposed;

    /// <summary>Gets a value indicating whether the tests are running in GitHub Actions CI environment.</summary>
    protected static bool IsRunningInGitHubActions =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>Gets a value indicating whether the tests are running in any CI environment.</summary>
    protected static bool IsRunningInCI =>
        IsRunningInGitHubActions ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")) || // Azure DevOps
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")) || // Jenkins
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")); // TeamCity

    /// <summary>Gets a cancellation token for test operations.</summary>
    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>Disposes the test base.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Gets an available port with proper isolation.</summary>
    /// <returns>An available port number.</returns>
    protected static async Task<int> GetAvailablePortAsync()
    {
        await PortSemaphore.WaitAsync();
        try
        {
            while (true)
            {
                using var tempListener = new TcpListener(IPAddress.Any, 0);
                tempListener.Start();
                var port = ((IPEndPoint)tempListener.LocalEndpoint).Port;

                if (!IsUdpPortAvailable(port))
                {
                    continue;
                }

                tempListener.Stop();
                return port;
            }
        }
        finally
        {
            _ = PortSemaphore.Release();
        }
    }

    /// <summary>Waits for a condition with timeout.</summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="timeout">The timeout period.</param>
    /// <param name="pollInterval">The polling interval.</param>
    /// <returns>True if condition was met, false if timeout occurred.</returns>
    protected static async Task<bool> WaitForConditionAsync(
        Func<bool> condition,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        if (condition is null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        pollInterval ??= TimeSpan.FromMilliseconds(50);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(pollInterval.Value);
        }

        return false;
    }

    /// <summary>Gets a shorter timeout for CI environments to prevent test timeouts.</summary>
    /// <param name="normalTimeout">The normal timeout for local testing.</param>
    /// <param name="reducedTimeout">The reduced timeout for CI environments.</param>
    /// <returns>The appropriate timeout based on the environment.</returns>
    protected static TimeSpan GetEnvironmentAppropriateTimeout(TimeSpan normalTimeout, TimeSpan? reducedTimeout = null)
    {
        return IsRunningInCI ? reducedTimeout ?? TimeSpan.FromMilliseconds(normalTimeout.TotalMilliseconds * 0.5) : normalTimeout;
    }

    /// <summary>Attempts to connect to a network resource with appropriate timeout for the environment.</summary>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="timeout">The connection timeout.</param>
    /// <returns>True if connection succeeded, false otherwise.</returns>
    protected static async Task<bool> TryConnectAsync(string host, int port, TimeSpan? timeout = null)
    {
        timeout ??= GetEnvironmentAppropriateTimeout(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2));

        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeout.Value));

            return completedTask == connectTask && client.Connected;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Registers a disposable resource for cleanup.</summary>
    /// <param name="disposable">The disposable resource.</param>
    protected void RegisterDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    /// <summary>Disposes the test base.</summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _cancellationTokenSource.Cancel();

        // Dispose all registered resources
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // Expected during test cleanup.
            }
            catch (IOException)
            {
                // Expected during test cleanup.
            }
            catch (SocketException)
            {
                // Expected during test cleanup.
            }
            catch (InvalidOperationException)
            {
                // Expected during test cleanup.
            }
        }

        _disposables.Clear();

        try
        {
            _cancellationTokenSource.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Expected during test cleanup.
        }

        _disposed = true;
    }

    /// <summary>Determines whether a UDP port is available.</summary>
    /// <param name="port">The UDP port to probe.</param>
    /// <returns>A value indicating whether the UDP port is available.</returns>
    private static bool IsUdpPortAvailable(int port)
    {
        try
        {
            using var udpClient = new UdpClient(port);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
