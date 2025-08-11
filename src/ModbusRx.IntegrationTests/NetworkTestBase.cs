// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusRx.IntegrationTests;

/// <summary>
/// Base class for network-related integration tests with proper resource management.
/// </summary>
public abstract class NetworkTestBase : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly SemaphoreSlim PortSemaphore = new(1, 1);
    private static int _currentPortBase = 15000; // Start high to avoid conflicts
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether the tests are running in GitHub Actions CI environment.
    /// </summary>
    protected static bool IsRunningInGitHubActions => 
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>
    /// Gets a value indicating whether the tests are running in any CI environment.
    /// </summary>
    protected static bool IsRunningInCI => 
        IsRunningInGitHubActions ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")) || // Azure DevOps
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")) || // Jenkins
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")); // TeamCity

    /// <summary>
    /// Gets a cancellation token for test operations.
    /// </summary>
    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Registers a disposable resource for cleanup.
    /// </summary>
    /// <param name="disposable">The disposable resource.</param>
    protected void RegisterDisposable(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    /// <summary>
    /// Gets an available port with proper isolation.
    /// </summary>
    /// <returns>An available port number.</returns>
    protected static async Task<int> GetAvailablePortAsync()
    {
        await PortSemaphore.WaitAsync();
        try
        {
            var port = Interlocked.Increment(ref _currentPortBase);

            // Ensure port is available
            using var tempListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port);
            try
            {
                tempListener.Start();
                tempListener.Stop();
                return port;
            }
            catch
            {
                // If port is busy, try the next one
                return await GetAvailablePortAsync();
            }
        }
        finally
        {
            PortSemaphore.Release();
        }
    }

    /// <summary>
    /// Waits for a condition with timeout.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="timeout">The timeout period.</param>
    /// <param name="pollInterval">The polling interval.</param>
    /// <returns>True if condition was met, false if timeout occurred.</returns>
    protected static async Task<bool> WaitForConditionAsync(
        Func<bool> condition,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        if (condition == null)
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

    /// <summary>
    /// Skips the test if running in CI environment to avoid network connectivity issues.
    /// </summary>
    /// <param name="reason">The reason for skipping (optional).</param>
    protected static void SkipIfRunningInCI(string reason = "Live network tests are not supported in CI environments")
    {
        if (IsRunningInCI)
        {
            throw new Xunit.SkipException(reason);
        }
    }

    /// <summary>
    /// Skips the test if running in GitHub Actions specifically.
    /// </summary>
    /// <param name="reason">The reason for skipping (optional).</param>
    protected static void SkipIfRunningInGitHubActions(string reason = "Live network tests are not supported in GitHub Actions")
    {
        if (IsRunningInGitHubActions)
        {
            throw new Xunit.SkipException(reason);
        }
    }

    /// <summary>
    /// Gets a shorter timeout for CI environments to prevent test timeouts.
    /// </summary>
    /// <param name="normalTimeout">The normal timeout for local testing.</param>
    /// <param name="ciTimeout">The reduced timeout for CI environments.</param>
    /// <returns>The appropriate timeout based on the environment.</returns>
    protected static TimeSpan GetEnvironmentAppropriateTimeout(TimeSpan normalTimeout, TimeSpan? ciTimeout = null)
    {
        if (IsRunningInCI)
        {
            return ciTimeout ?? TimeSpan.FromMilliseconds(normalTimeout.TotalMilliseconds * 0.5);
        }

        return normalTimeout;
    }

    /// <summary>
    /// Attempts to connect to a network resource with appropriate timeout for the environment.
    /// </summary>
    /// <param name="host">The host to connect to.</param>
    /// <param name="port">The port to connect to.</param>
    /// <param name="timeout">The connection timeout.</param>
    /// <returns>True if connection succeeded, false otherwise.</returns>
    protected static async Task<bool> TryConnectAsync(string host, int port, TimeSpan? timeout = null)
    {
        timeout ??= GetEnvironmentAppropriateTimeout(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(2));

        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeout.Value));
            
            return completedTask == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Disposes the test base.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the test base.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _cancellationTokenSource.Cancel();

            // Dispose all registered resources
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch
                {
                    // Ignore disposal exceptions in tests
                }
            }

            _disposables.Clear();

            try
            {
                _cancellationTokenSource.Dispose();
            }
            catch
            {
                // Ignore disposal exceptions
            }

            _disposed = true;
        }
    }
}
