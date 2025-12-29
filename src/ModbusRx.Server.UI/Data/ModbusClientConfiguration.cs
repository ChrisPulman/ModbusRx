// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace ModbusRx.Server.UI.Data;

/// <summary>
/// Entity representing a Modbus client configuration.
/// </summary>
public sealed class ModbusClientConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the friendly name for this client.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection type (TCP, UDP, RTU, ASCII).
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string ConnectionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address or COM port.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port number or baud rate.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the slave/unit ID.
    /// </summary>
    public byte SlaveId { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether this client is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout in milliseconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the polling interval in milliseconds.
    /// </summary>
    public int PollingIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets additional configuration as JSON.
    /// </summary>
    public string? AdditionalConfig { get; set; }

    /// <summary>
    /// Gets or sets when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this configuration was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
