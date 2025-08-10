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
