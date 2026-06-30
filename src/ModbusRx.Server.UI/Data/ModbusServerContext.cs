// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;

namespace ModbusRx.Server.UI.Data;

/// <summary>Entity Framework database context for ModbusRx Server configuration.</summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ModbusServerContext"/> class.
/// </remarks>
/// <param name="options">The database context options.</param>
public class ModbusServerContext(DbContextOptions<ModbusServerContext> options) : DbContext(options)
{
    /// <summary>Gets or sets the Modbus client configurations.</summary>
    public DbSet<ModbusClientConfiguration> ClientConfigurations { get; set; } = null!;

    /// <summary>Gets or sets the server configurations.</summary>
    public DbSet<ServerConfiguration> ServerConfigurations { get; set; } = null!;

    /// <summary>Configures the model that was discovered by convention from the entity types.</summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        base.OnModelCreating(modelBuilder);

        // Configure ModbusClientConfiguration
        _ = modelBuilder.Entity<ModbusClientConfiguration>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            _ = entity.Property(e => e.ConnectionType).IsRequired().HasMaxLength(10);
            _ = entity.Property(e => e.Address).IsRequired().HasMaxLength(50);
            _ = entity.HasIndex(e => e.Name);
        });

        // Configure ServerConfiguration
        _ = modelBuilder.Entity<ServerConfiguration>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            _ = entity.Property(e => e.SimulationType).HasMaxLength(20);
        });

        // Seed default server configuration
        _ = modelBuilder.Entity<ServerConfiguration>().HasData(
            new ServerConfiguration
            {
                Id = 1,
                Name = "Default Server",
                TcpPort = 502,
                UdpPort = 503,
                UnitId = 1,
                SimulationEnabled = false,
                SimulationType = "Random",
                SimulationIntervalMs = 1000,
                AutoStart = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            });
    }
}
