// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using ModbusRx.Server.UI.Data;

namespace ModbusRx.Server.UI.Services;

/// <summary>
/// Service for managing Modbus client configurations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConfigurationService"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class ConfigurationService(ModbusServerContext context)
{
    /// <summary>
    /// Gets all client configurations.
    /// </summary>
    /// <returns>A list of client configurations.</returns>
    public async Task<List<ModbusClientConfiguration>> GetClientConfigurationsAsync() => await context.ClientConfigurations
            .OrderBy(c => c.Name)
            .ToListAsync();

    /// <summary>
    /// Gets a client configuration by ID.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <returns>The client configuration or null if not found.</returns>
    public async Task<ModbusClientConfiguration?> GetClientConfigurationAsync(int id) => await context.ClientConfigurations.FindAsync(id);

    /// <summary>
    /// Saves a client configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>The saved configuration.</returns>
    public async Task<ModbusClientConfiguration> SaveClientConfigurationAsync(ModbusClientConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration.ModifiedAt = DateTime.UtcNow;

        if (configuration.Id == 0)
        {
            configuration.CreatedAt = DateTime.UtcNow;
            context.ClientConfigurations.Add(configuration);
        }
        else
        {
            context.ClientConfigurations.Update(configuration);
        }

        await context.SaveChangesAsync();
        return configuration;
    }

    /// <summary>
    /// Deletes a client configuration.
    /// </summary>
    /// <param name="id">The configuration ID to delete.</param>
    /// <returns>True if deleted, false if not found.</returns>
    public async Task<bool> DeleteClientConfigurationAsync(int id)
    {
        var configuration = await context.ClientConfigurations.FindAsync(id);
        if (configuration == null)
        {
            return false;
        }

        context.ClientConfigurations.Remove(configuration);
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets the server configuration.
    /// </summary>
    /// <returns>The server configuration.</returns>
    public async Task<ServerConfiguration> GetServerConfigurationAsync()
    {
        var config = await context.ServerConfigurations.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ServerConfiguration { Id = 1 };
            context.ServerConfigurations.Add(config);
            await context.SaveChangesAsync();
        }

        return config;
    }

    /// <summary>
    /// Saves the server configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <returns>The saved configuration.</returns>
    public async Task<ServerConfiguration> SaveServerConfigurationAsync(ServerConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        configuration.ModifiedAt = DateTime.UtcNow;
        context.ServerConfigurations.Update(configuration);
        await context.SaveChangesAsync();
        return configuration;
    }

    /// <summary>
    /// Gets enabled client configurations.
    /// </summary>
    /// <returns>A list of enabled client configurations.</returns>
    public async Task<List<ModbusClientConfiguration>> GetEnabledClientConfigurationsAsync() => await context.ClientConfigurations
            .Where(c => c.IsEnabled)
            .OrderBy(c => c.Name)
            .ToListAsync();

    /// <summary>
    /// Updates the enabled status of a client configuration.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="enabled">The enabled status.</param>
    /// <returns>True if updated, false if not found.</returns>
    public async Task<bool> UpdateClientEnabledStatusAsync(int id, bool enabled)
    {
        var configuration = await context.ClientConfigurations.FindAsync(id);
        if (configuration == null)
        {
            return false;
        }

        configuration.IsEnabled = enabled;
        configuration.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }
}
