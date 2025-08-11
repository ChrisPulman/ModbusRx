// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using ModbusRx.Server.UI.Data;
using ModbusRx.Server.UI.Services;
using ModbusRx.Server.UI.Visualization;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ModbusRx.Server.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
[IViewFor<ModbusServerViewModel>]
public partial class MainWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        SetupDependencies();
    }

    private void SetupDependencies()
    {
        // Setup Entity Framework
        var optionsBuilder = new DbContextOptionsBuilder<ModbusServerContext>();
        optionsBuilder.UseSqlite("Data Source=modbusrx.db");

        var context = new ModbusServerContext(optionsBuilder.Options);
        context.Database.EnsureCreated();

        // Setup services
        var configurationService = new ConfigurationService(context);

        // Create and set view model
        ViewModel = new ModbusServerViewModel(configurationService);
        DataContext = ViewModel;

        // Setup window events
        Closed += (_, _) => ViewModel?.Dispose();

        // Handle window closing to ensure proper cleanup
        Closing += (sender, e) =>
        {
            if (ViewModel?.IsServerRunning == true)
            {
                // Let the ViewModel handle the exit confirmation
                ViewModel.ExitApplicationCommand.Execute().Subscribe();
                e.Cancel = true; // Cancel the close to let the command handle it
            }
        };
    }
}
