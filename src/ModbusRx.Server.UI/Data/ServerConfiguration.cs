// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace ModbusRx.Server.UI.Data;

/// <summary>
/// Entity representing server configuration.
/// </summary>
public sealed class ServerConfiguration
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "ModbusRx Server";

    /// <summary>
    /// Gets or sets the TCP port for the server.
    /// </summary>
    public int TcpPort { get; set; } = 502;

    /// <summary>
    /// Gets or sets the UDP port for the server.
    /// </summary>
    public int UdpPort { get; set; } = 503;

    /// <summary>
    /// Gets or sets the unit ID for the server.
    /// </summary>
    public byte UnitId { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether simulation mode is enabled.
    /// </summary>
    public bool SimulationEnabled { get; set; }

    /// <summary>
    /// Gets or sets the simulation type.
    /// </summary>
    [MaxLength(20)]
    public string SimulationType { get; set; } = "Random";

    /// <summary>
    /// Gets or sets the simulation interval in milliseconds.
    /// </summary>
    public int SimulationIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether the server auto-starts.
    /// </summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Gets or sets when this configuration was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when this configuration was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
