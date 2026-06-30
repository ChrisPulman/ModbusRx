// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using ModbusRx.Server.UI.Data;
using ModbusRx.Server.UI.Services;
using ModbusRx.Server.UI.Visualization;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ModbusRx.Server.UI;

/// <summary>Interaction logic for MainWindow.xaml.</summary>
[IViewFor<ModbusServerViewModel>]
public partial class MainWindow
{
    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();
        SetupDependencies();
    }

    /// <summary>Configures the data context and application services.</summary>
    private void SetupDependencies()
    {
        // Setup Entity Framework
        var optionsBuilder = new DbContextOptionsBuilder<ModbusServerContext>();
        _ = optionsBuilder.UseSqlite("Data Source=modbusrx.db");

        var context = new ModbusServerContext(optionsBuilder.Options);
        _ = context.Database.EnsureCreated();

        // Setup services
        var configurationService = new ConfigurationService(context);

        // Create and set view model
        ViewModel = new(configurationService);
        DataContext = ViewModel;

        // Setup window events
        Closed += (_, _) => ViewModel?.Dispose();

        // Handle window closing to ensure proper cleanup
        Closing += (sender, e) =>
        {
            if (ViewModel?.IsServerRunning != true)
            {
                return;
            }

            // Let the ViewModel handle the exit confirmation
            _ = ViewModel.ExitApplicationCommand.Execute().Subscribe();
            e.Cancel = true; // Cancel the close to let the command handle it
        };
    }
}
