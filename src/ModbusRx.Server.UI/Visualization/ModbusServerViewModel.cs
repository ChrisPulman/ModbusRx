// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Reactive;
using ModbusRx.Server.UI.Data;
using ModbusRx.Server.UI.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ModbusRx.Server.UI.Visualization;

/// <summary>ViewModel for Modbus server visualization using ReactiveUI.</summary>
public partial class ModbusServerViewModel : ReactiveObject, IDisposable
{
    /// <summary>The disposable resources owned by the view model.</summary>
    private readonly CompositeDisposable _disposables = [];

    /// <summary>The configuration persistence service.</summary>
    private readonly ConfigurationService _configurationService;

    /// <summary>The hosted Modbus server.</summary>
    private ModbusServer? _server;

    /// <summary>A value indicating whether the server is running.</summary>
    [Reactive]
    private bool _isServerRunning;

    /// <summary>A value indicating whether simulation mode is active.</summary>
    [Reactive]
    private bool _simulationMode;

    /// <summary>The selected test pattern.</summary>
    [Reactive]
    private TestPattern _selectedTestPattern = TestPattern.Random;

    /// <summary>The selected simulation data generator.</summary>
    [Reactive]
    private SimulationType _selectedSimulationType = SimulationType.Random;

    /// <summary>The active server configuration.</summary>
    [Reactive]
    private ServerConfiguration? _serverConfiguration;

    /// <summary>The selected client configuration.</summary>
    [Reactive]
    private ModbusClientConfiguration? _selectedClientConfiguration;

    /// <summary>The name for a new client configuration.</summary>
    [Reactive]
    private string _newClientName = string.Empty;

    /// <summary>The address for a new client configuration.</summary>
    [Reactive]
    private string _newClientAddress = "127.0.0.1";

    /// <summary>The port for a new client configuration.</summary>
    [Reactive]
    private int _newClientPort = 502;

    /// <summary>The connection type for a new client configuration.</summary>
    [Reactive]
    private string _newClientConnectionType = "TCP";

    /// <summary>The current UI status message.</summary>
    [Reactive]
    private string _statusMessage = "Ready";

    /// <summary>A value indicating whether this instance has been disposed.</summary>
    private bool _disposedValue;

    /// <summary>Initializes a new instance of the <see cref="ModbusServerViewModel"/> class.</summary>
    /// <param name="configurationService">The configuration service.</param>
    public ModbusServerViewModel(ConfigurationService configurationService)
    {
        _configurationService = configurationService;

        HoldingRegisters = [];
        InputRegisters = [];
        Coils = [];
        Inputs = [];
        ClientConfigurations = [];

        SetupCommands();
        InitializeAsync();
    }

    /// <summary>Gets the available test patterns.</summary>
    public static Array TestPatterns => Enum.GetValues<TestPattern>();

    /// <summary>Gets the available simulation types.</summary>
    public static Array SimulationTypes => Enum.GetValues<SimulationType>();

    /// <summary>Gets the available connection types.</summary>
    public static string[] ConnectionTypes => ["TCP", "UDP", "RTU", "ASCII"];

    /// <summary>Gets the holding registers collection.</summary>
    public ObservableCollection<RegisterData> HoldingRegisters { get; }

    /// <summary>Gets the input registers collection.</summary>
    public ObservableCollection<RegisterData> InputRegisters { get; }

    /// <summary>Gets the coils collection.</summary>
    public ObservableCollection<CoilData> Coils { get; }

    /// <summary>Gets the inputs collection.</summary>
    public ObservableCollection<CoilData> Inputs { get; }

    /// <summary>Gets the client configurations collection.</summary>
    public ObservableCollection<ModbusClientConfiguration> ClientConfigurations { get; }

    /// <summary>Gets the start server command.</summary>
    public ReactiveCommand<object?, object?> StartServerCommand { get; private set; } = null!;

    /// <summary>Gets the stop server command.</summary>
    public ReactiveCommand<object?, object?> StopServerCommand { get; private set; } = null!;

    /// <summary>Gets the load test pattern command.</summary>
    public ReactiveCommand<object?, object?> LoadTestPatternCommand { get; private set; } = null!;

    /// <summary>Gets the clear data command.</summary>
    public ReactiveCommand<object?, object?> ClearDataCommand { get; private set; } = null!;

    /// <summary>Gets the add client command.</summary>
    public ReactiveCommand<object?, object?> AddClientCommand { get; private set; } = null!;

    /// <summary>Gets the remove client command.</summary>
    public ReactiveCommand<object?, object?> RemoveClientCommand { get; private set; } = null!;

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _disposables.Dispose();
            _server?.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>Updates a register collection from a value buffer.</summary>
    /// <param name="collection">The UI collection to update.</param>
    /// <param name="values">The register values.</param>
    private static void UpdateRegisterCollection(ObservableCollection<RegisterData> collection, ushort[] values)
    {
        // Ensure collection has enough items
        while (collection.Count < values.Length)
        {
            collection.Add(new RegisterData { Address = (ushort)collection.Count });
        }

        // Update values
        for (var i = 0; i < values.Length; i++)
        {
            collection[i].Value = values[i];
        }
    }

    /// <summary>Updates a coil collection from a value buffer.</summary>
    /// <param name="collection">The UI collection to update.</param>
    /// <param name="values">The coil values.</param>
    private static void UpdateCoilCollection(ObservableCollection<CoilData> collection, bool[] values)
    {
        // Ensure collection has enough items
        while (collection.Count < values.Length)
        {
            collection.Add(new CoilData { Address = (ushort)collection.Count });
        }

        // Update values
        for (var i = 0; i < values.Length; i++)
        {
            collection[i].Value = values[i];
        }
    }

    /// <summary>Copies the first register values from a source buffer.</summary>
    /// <param name="values">The source values.</param>
    /// <param name="count">The maximum number of values to copy.</param>
    /// <returns>The copied values.</returns>
    private static ushort[] CopyFirst(ushort[] values, int count)
    {
        var length = Math.Min(values.Length, count);
        var result = new ushort[length];
        Array.Copy(values, result, length);
        return result;
    }

    /// <summary>Copies the first coil values from a source buffer.</summary>
    /// <param name="values">The source values.</param>
    /// <param name="count">The maximum number of values to copy.</param>
    /// <returns>The copied values.</returns>
    private static bool[] CopyFirst(bool[] values, int count)
    {
        var length = Math.Min(values.Length, count);
        var result = new bool[length];
        Array.Copy(values, result, length);
        return result;
    }

    /// <summary>Creates a command for a synchronous action without exposing a unit result type in source.</summary>
    /// <param name="execute">The action to execute.</param>
    /// <returns>The configured command.</returns>
    private static ReactiveCommand<object?, object?> CreateCommand(Action execute) =>
        ReactiveCommand.Create<object?, object?>(_ =>
        {
            execute();
            return null;
        });

    /// <summary>Creates a command for an asynchronous action without exposing a unit result type in source.</summary>
    /// <param name="execute">The action to execute.</param>
    /// <param name="canExecute">The command availability stream.</param>
    /// <returns>The configured command.</returns>
    private static ReactiveCommand<object?, object?> CreateCommand(Func<Task> execute, IObservable<bool> canExecute) =>
        ReactiveCommand.CreateFromTask<object?, object?>(
            async _ =>
            {
                await execute();
                return null;
            },
            canExecute);

    /// <summary>Creates and wires the reactive commands.</summary>
    private void SetupCommands()
    {
        var canStart = this.WhenAnyValue(x => x.IsServerRunning).Select(running => !running);
        var canStop = this.WhenAnyValue(x => x.IsServerRunning);
        var hasSelectedClient = this.WhenAnyValue(x => x.SelectedClientConfiguration).Select(c => c is not null);
        var canAddClient = this.WhenAnyValue(x => x.NewClientName, x => x.NewClientAddress)
            .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2));

        StartServerCommand = CreateCommand(StartServerAsync, canStart);
        StopServerCommand = CreateCommand(StopServerAsync, canStop);
        LoadTestPatternCommand = CreateCommand(LoadTestPattern);
        ClearDataCommand = CreateCommand(ClearData);
        AddClientCommand = CreateCommand(AddClientAsync, canAddClient);
        RemoveClientCommand = CreateCommand(RemoveClientAsync, hasSelectedClient);

        // Subscribe to command results
        _ = StartServerCommand.Subscribe(_ => StatusMessage = "Server started successfully");
        _ = StopServerCommand.Subscribe(_ => StatusMessage = "Server stopped");
        _ = LoadTestPatternCommand.Subscribe(_ => StatusMessage = $"Loaded {SelectedTestPattern} pattern");
        _ = ClearDataCommand.Subscribe(_ => StatusMessage = "Data cleared");

        // Handle errors
        _ = StartServerCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error starting server: {ex.Message}");
        _ = StopServerCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error stopping server: {ex.Message}");
        _ = AddClientCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error adding client: {ex.Message}");
        _ = RemoveClientCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error removing client: {ex.Message}");

        _disposables.Add(StartServerCommand);
        _disposables.Add(StopServerCommand);
        _disposables.Add(LoadTestPatternCommand);
        _disposables.Add(ClearDataCommand);
        _disposables.Add(AddClientCommand);
        _disposables.Add(RemoveClientCommand);
    }

    /// <summary>Initializes configuration and server state asynchronously.</summary>
    private async void InitializeAsync()
    {
        try
        {
            await LoadConfigurationAsync();
            InitializeServer();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
        }
    }

    /// <summary>Creates the server and starts observing data changes.</summary>
    private void InitializeServer()
    {
        _server = new();

        // Start data observation
        var subscription = _server.ObserveDataChanges(100)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateData);
        _disposables.Add(subscription);

        // Monitor simulation mode changes
        _ = this.WhenAnyValue(x => x.SimulationMode)
            .Skip(1) // Skip initial value
            .Subscribe(enabled =>
            {
                if (_server is null)
                {
                    return;
                }

                _server.SimulationMode = enabled;
                StatusMessage = enabled ? "Simulation enabled" : "Simulation disabled";
            });
    }

    /// <summary>Starts the configured Modbus server endpoints.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task StartServerAsync()
    {
        if (_server is null || ServerConfiguration is null)
        {
            return;
        }

        try
        {
            // Start TCP server
            var tcpSubscription = _server.StartTcpServer(ServerConfiguration.TcpPort, ServerConfiguration.UnitId);
            _disposables.Add(tcpSubscription);

            // Start UDP server
            var udpSubscription = _server.StartUdpServer(ServerConfiguration.UdpPort, ServerConfiguration.UnitId);
            _disposables.Add(udpSubscription);

            // Add configured clients
            var enabledClients = await _configurationService.GetEnabledClientConfigurationsAsync();
            foreach (var clientConfig in enabledClients)
            {
                try
                {
                    var clientSubscription = clientConfig.ConnectionType.ToUpper() switch
                    {
                        "TCP" => _server.AddTcpClient(clientConfig.Name, clientConfig.Address, clientConfig.Port, clientConfig.SlaveId),
                        "UDP" => _server.AddUdpClient(clientConfig.Name, clientConfig.Address, clientConfig.Port, clientConfig.SlaveId),
                        _ => throw new NotSupportedException($"Connection type {clientConfig.ConnectionType} not supported")
                    };
                    _disposables.Add(clientSubscription);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error adding client {clientConfig.Name}: {ex.Message}";
                }
            }

            _server.Start();
            IsServerRunning = true;

            if (ServerConfiguration.SimulationEnabled)
            {
                SimulationMode = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error starting server: {ex.Message}";
            throw;
        }
    }

    /// <summary>Stops the Modbus server.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task StopServerAsync()
    {
        if (_server is null)
        {
            return;
        }

        try
        {
            _server.Stop();
            SimulationMode = false;
            IsServerRunning = false;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error stopping server: {ex.Message}";
            throw;
        }
    }

    /// <summary>Loads the selected test pattern into the server data store.</summary>
    private void LoadTestPattern()
    {
        if (_server?.DataStore is null)
        {
            return;
        }

        using var provider = new SimulationDataProvider();
        provider.LoadTestPattern(_server.DataStore, SelectedTestPattern);
    }

    /// <summary>Clears all displayed server data.</summary>
    private void ClearData()
    {
        if (_server?.DataStore is null)
        {
            return;
        }

        _server.LoadSimulationData(
            new ushort[100],
            new ushort[100],
            new bool[100],
            new bool[100]);
    }

    /// <summary>Adds a client configuration.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task AddClientAsync()
    {
        try
        {
            var clientConfig = new ModbusClientConfiguration
            {
                Name = NewClientName,
                Address = NewClientAddress,
                Port = NewClientPort,
                ConnectionType = NewClientConnectionType,
                SlaveId = 1,
                IsEnabled = true
            };

            await _configurationService.SaveClientConfigurationAsync(clientConfig);
            ClientConfigurations.Add(clientConfig);

            // Clear input fields
            NewClientName = string.Empty;
            NewClientAddress = "127.0.0.1";
            NewClientPort = 502;
            NewClientConnectionType = "TCP";

            StatusMessage = $"Client '{clientConfig.Name}' added successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding client: {ex.Message}";
            throw;
        }
    }

    /// <summary>Removes the selected client configuration.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RemoveClientAsync()
    {
        if (SelectedClientConfiguration is null)
        {
            return;
        }

        try
        {
            await _configurationService.DeleteClientConfigurationAsync(SelectedClientConfiguration.Id);
            _ = ClientConfigurations.Remove(SelectedClientConfiguration);
            StatusMessage = $"Client '{SelectedClientConfiguration.Name}' removed successfully";
            SelectedClientConfiguration = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing client: {ex.Message}";
            throw;
        }
    }

    /// <summary>Saves the current server configuration.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [ReactiveCommand]
    private async Task SaveConfigurationAsync()
    {
        if (ServerConfiguration is null)
        {
            return;
        }

        try
        {
            ServerConfiguration.SimulationEnabled = SimulationMode;
            await _configurationService.SaveServerConfigurationAsync(ServerConfiguration);
            StatusMessage = "Configuration saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving configuration: {ex.Message}";
            throw;
        }
    }

    /// <summary>Loads server and client configuration.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [ReactiveCommand]
    private async Task LoadConfigurationAsync()
    {
        try
        {
            ServerConfiguration = await _configurationService.GetServerConfigurationAsync();
            SimulationMode = ServerConfiguration.SimulationEnabled;

            var clients = await _configurationService.GetClientConfigurationsAsync();
            ClientConfigurations.Clear();
            foreach (var client in clients)
            {
                ClientConfigurations.Add(client);
            }

            StatusMessage = "Configuration loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading configuration: {ex.Message}";
            throw;
        }
    }

    /// <summary>Exits the application after stopping the server when needed.</summary>
    [ReactiveCommand]
    private void ExitApplication()
    {
        try
        {
            // Show confirmation dialog if server is running
            if (IsServerRunning)
            {
                var result = System.Windows.MessageBox.Show(
                    "The Modbus server is currently running. Do you want to stop the server and exit the application?",
                    "Confirm Exit",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    return;
                }

                // Stop the server
                _server?.Stop();
                StatusMessage = "Server stopped. Exiting application...";
            }

            // Save configuration before exiting
            if (ServerConfiguration is not null)
            {
                ServerConfiguration.SimulationEnabled = SimulationMode;
                _ = Task.Run(() => _configurationService.SaveServerConfigurationAsync(ServerConfiguration));
            }

            // Request application shutdown
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during application exit: {ex.Message}";

            // Show error message to user
            _ = System.Windows.MessageBox.Show(
                $"An error occurred while closing the application: {ex.Message}\n\nThe application will still close.",
                "Exit Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            // Still try to exit even if there's an error
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>Updates the displayed data collections.</summary>
    /// <param name="data">The latest server data snapshot.</param>
    private void UpdateData((ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs) data)
    {
        // Update holding registers
        UpdateRegisterCollection(HoldingRegisters, CopyFirst(data.holdingRegisters, 50));

        // Update input registers
        UpdateRegisterCollection(InputRegisters, CopyFirst(data.inputRegisters, 50));

        // Update coils
        UpdateCoilCollection(Coils, CopyFirst(data.coils, 50));

        // Update inputs
        UpdateCoilCollection(Inputs, CopyFirst(data.inputs, 50));
    }
}
