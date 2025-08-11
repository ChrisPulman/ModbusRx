// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Reactive;
using ModbusRx.Server.UI.Data;
using ModbusRx.Server.UI.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ModbusRx.Server.UI.Visualization;

/// <summary>
/// ViewModel for Modbus server visualization using ReactiveUI.
/// </summary>
public partial class ModbusServerViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = [];
    private readonly ConfigurationService _configurationService;
    private ModbusServer? _server;

    [Reactive]
    private bool _isServerRunning;

    [Reactive]
    private bool _simulationMode;

    [Reactive]
    private TestPattern _selectedTestPattern = TestPattern.Random;

    [Reactive]
    private SimulationType _selectedSimulationType = SimulationType.Random;

    [Reactive]
    private ServerConfiguration? _serverConfiguration;

    [Reactive]
    private ModbusClientConfiguration? _selectedClientConfiguration;

    [Reactive]
    private string _newClientName = string.Empty;

    [Reactive]
    private string _newClientAddress = "127.0.0.1";

    [Reactive]
    private int _newClientPort = 502;

    [Reactive]
    private string _newClientConnectionType = "TCP";

    [Reactive]
    private string _statusMessage = "Ready";
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModbusServerViewModel"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets the available test patterns.
    /// </summary>
    public static Array TestPatterns => Enum.GetValues<TestPattern>();

    /// <summary>
    /// Gets the available simulation types.
    /// </summary>
    public static Array SimulationTypes => Enum.GetValues<SimulationType>();

    /// <summary>
    /// Gets the available connection types.
    /// </summary>
    public static string[] ConnectionTypes => ["TCP", "UDP", "RTU", "ASCII"];

    /// <summary>
    /// Gets the holding registers collection.
    /// </summary>
    public ObservableCollection<RegisterData> HoldingRegisters { get; }

    /// <summary>
    /// Gets the input registers collection.
    /// </summary>
    public ObservableCollection<RegisterData> InputRegisters { get; }

    /// <summary>
    /// Gets the coils collection.
    /// </summary>
    public ObservableCollection<CoilData> Coils { get; }

    /// <summary>
    /// Gets the inputs collection.
    /// </summary>
    public ObservableCollection<CoilData> Inputs { get; }

    /// <summary>
    /// Gets the client configurations collection.
    /// </summary>
    public ObservableCollection<ModbusClientConfiguration> ClientConfigurations { get; }

    /// <summary>
    /// Gets the start server command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> StartServerCommand { get; private set; } = null!;

    /// <summary>
    /// Gets the stop server command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> StopServerCommand { get; private set; } = null!;

    /// <summary>
    /// Gets the load test pattern command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoadTestPatternCommand { get; private set; } = null!;

    /// <summary>
    /// Gets the clear data command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ClearDataCommand { get; private set; } = null!;

    /// <summary>
    /// Gets the add client command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddClientCommand { get; private set; } = null!;

    /// <summary>
    /// Gets the remove client command.
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemoveClientCommand { get; private set; } = null!;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _disposables.Dispose();
                _server?.Dispose();
            }

            _disposedValue = true;
        }
    }

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

    private void SetupCommands()
    {
        var canStart = this.WhenAnyValue(x => x.IsServerRunning).Select(running => !running);
        var canStop = this.WhenAnyValue(x => x.IsServerRunning);
        var hasSelectedClient = this.WhenAnyValue(x => x.SelectedClientConfiguration).Select(c => c != null);
        var canAddClient = this.WhenAnyValue(x => x.NewClientName, x => x.NewClientAddress)
            .Select(x => !string.IsNullOrWhiteSpace(x.Item1) && !string.IsNullOrWhiteSpace(x.Item2));

        StartServerCommand = ReactiveCommand.CreateFromTask(StartServerAsync, canStart);
        StopServerCommand = ReactiveCommand.CreateFromTask(StopServerAsync, canStop);
        LoadTestPatternCommand = ReactiveCommand.Create(LoadTestPattern);
        ClearDataCommand = ReactiveCommand.Create(ClearData);
        AddClientCommand = ReactiveCommand.CreateFromTask(AddClientAsync, canAddClient);
        RemoveClientCommand = ReactiveCommand.CreateFromTask(RemoveClientAsync, hasSelectedClient);

        // Subscribe to command results
        StartServerCommand.Subscribe(_ => StatusMessage = "Server started successfully");
        StopServerCommand.Subscribe(_ => StatusMessage = "Server stopped");
        LoadTestPatternCommand.Subscribe(_ => StatusMessage = $"Loaded {SelectedTestPattern} pattern");
        ClearDataCommand.Subscribe(_ => StatusMessage = "Data cleared");

        // Handle errors
        StartServerCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error starting server: {ex.Message}");
        StopServerCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error stopping server: {ex.Message}");
        AddClientCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error adding client: {ex.Message}");
        RemoveClientCommand.ThrownExceptions.Subscribe(ex => StatusMessage = $"Error removing client: {ex.Message}");

        _disposables.Add(StartServerCommand);
        _disposables.Add(StopServerCommand);
        _disposables.Add(LoadTestPatternCommand);
        _disposables.Add(ClearDataCommand);
        _disposables.Add(AddClientCommand);
        _disposables.Add(RemoveClientCommand);
    }

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

    private void InitializeServer()
    {
        _server = new ModbusServer();

        // Start data observation
        var subscription = _server.ObserveDataChanges(100)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateData);
        _disposables.Add(subscription);

        // Monitor simulation mode changes
        this.WhenAnyValue(x => x.SimulationMode)
            .Skip(1) // Skip initial value
            .Subscribe(enabled =>
            {
                if (_server != null)
                {
                    _server.SimulationMode = enabled;
                    StatusMessage = enabled ? "Simulation enabled" : "Simulation disabled";
                }
            })
            .DisposeWith(_disposables);
    }

    private async Task StartServerAsync()
    {
        if (_server == null || ServerConfiguration == null)
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

    private async Task StopServerAsync()
    {
        if (_server == null)
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

    private void LoadTestPattern()
    {
        if (_server?.DataStore == null)
        {
            return;
        }

        using var provider = new SimulationDataProvider();
        provider.LoadTestPattern(_server.DataStore, SelectedTestPattern);
    }

    private void ClearData()
    {
        if (_server?.DataStore == null)
        {
            return;
        }

        _server.LoadSimulationData(
            new ushort[100],
            new ushort[100],
            new bool[100],
            new bool[100]);
    }

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

    private async Task RemoveClientAsync()
    {
        if (SelectedClientConfiguration == null)
        {
            return;
        }

        try
        {
            await _configurationService.DeleteClientConfigurationAsync(SelectedClientConfiguration.Id);
            ClientConfigurations.Remove(SelectedClientConfiguration);
            StatusMessage = $"Client '{SelectedClientConfiguration.Name}' removed successfully";
            SelectedClientConfiguration = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing client: {ex.Message}";
            throw;
        }
    }

    [ReactiveCommand]
    private async Task SaveConfigurationAsync()
    {
        if (ServerConfiguration == null)
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
            if (ServerConfiguration != null)
            {
                ServerConfiguration.SimulationEnabled = SimulationMode;
                Task.Run(async () => await _configurationService.SaveServerConfigurationAsync(ServerConfiguration));
            }

            // Request application shutdown
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during application exit: {ex.Message}";

            // Show error message to user
            System.Windows.MessageBox.Show(
                $"An error occurred while closing the application: {ex.Message}\n\nThe application will still close.",
                "Exit Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            // Still try to exit even if there's an error
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void UpdateData((ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs) data)
    {
        // Update holding registers
        UpdateRegisterCollection(HoldingRegisters, [.. data.holdingRegisters.Take(50)]);

        // Update input registers
        UpdateRegisterCollection(InputRegisters, [.. data.inputRegisters.Take(50)]);

        // Update coils
        UpdateCoilCollection(Coils, [.. data.coils.Take(50)]);

        // Update inputs
        UpdateCoilCollection(Inputs, [.. data.inputs.Take(50)]);
    }
}
