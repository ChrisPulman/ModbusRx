// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Splat;

namespace ModbusRx.Server.UI;

/// <summary>
/// Debug logger for ReactiveUI.
/// </summary>
internal class DebugLogger : ILogger
{
    /// <summary>
    /// Gets or sets the current log level.
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Writes a log message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="logLevel">The log level.</param>
    public void Write(string message, LogLevel logLevel)
    {
        if (logLevel >= Level)
        {
            Debug.WriteLine($"[{logLevel}] {message}");
        }
    }

    /// <summary>
    /// Writes a log message with type information.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="type">The type that generated the message.</param>
    /// <param name="logLevel">The log level.</param>
    public void Write(string message, Type type, LogLevel logLevel)
    {
        Write($"{type.Name}: {message}", logLevel);
    }

    /// <summary>
    /// Writes an exception with message.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="logLevel">The log level.</param>
    public void Write(Exception exception, string message, LogLevel logLevel)
    {
        Write($"{message} - Exception: {exception}", logLevel);
    }

    /// <summary>
    /// Writes an exception with message and type information.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="type">The type that generated the message.</param>
    /// <param name="logLevel">The log level.</param>
    public void Write(Exception exception, string message, Type type, LogLevel logLevel)
    {
        Write($"{type.Name}: {message} - Exception: {exception}", logLevel);
    }
}
