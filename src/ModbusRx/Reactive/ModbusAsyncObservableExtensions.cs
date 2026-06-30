// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Device;
#else
using ModbusRx.Device;
#endif
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Async;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive;
#else
namespace ModbusRx;
#endif

/// <summary>Async-observable adapters for Modbus reactive operations.</summary>
public static class ModbusAsyncObservableExtensions
{
    /// <summary>Provides async-observable adapters for serial master connection streams.</summary>
    /// <param name="source">The source serial connection stream.</param>
    extension(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source)
    {
        /// <summary>Converts a serial master connection stream to an async observable.</summary>
        /// <returns>The async observable connection stream.</returns>
        public IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> ToModbusObservable() =>
            source.ToAsyncObservable();

        /// <summary>Reads holding registers from a serial master and exposes the polling result as an async observable.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of holding-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegistersObservable(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadHoldingRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads input registers from a serial master and exposes the polling result as an async observable.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegistersObservable(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadInputRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads coils from a serial master and exposes the polling result as an async observable.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of coil data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadCoilsObservable(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadCoils(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads discrete inputs from a serial master and exposes the polling result as an async observable.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadInputsObservable(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadInputs(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();
    }

    /// <summary>Provides async-observable adapters for IP master connection streams.</summary>
    /// <param name="source">The source connection stream.</param>
    extension(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source)
    {
        /// <summary>Converts an IP master connection stream to an async observable.</summary>
        /// <returns>The async observable connection stream.</returns>
        public IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> ToModbusObservable() =>
            source.ToAsyncObservable();

        /// <summary>Reads input registers and exposes the polling result as an async observable.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegistersObservable(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadInputRegisters(startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads holding registers and exposes the polling result as an async observable.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of holding-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegistersObservable(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadHoldingRegisters(startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads coils and exposes the polling result as an async observable.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of coil data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadCoilsObservable(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadCoils(startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads discrete inputs and exposes the polling result as an async observable.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadInputsObservable(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ReadInputs(startAddress, numberOfPoints, interval).ToAsyncObservable();
    }

    /// <summary>Provides bridge adapters from synchronous observables to async observables.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="source">The source observable.</param>
    extension<T>(IObservable<T> source)
    {
        /// <summary>Converts a synchronous observable to an async observable.</summary>
        /// <returns>The async observable adapter.</returns>
        public IObservableAsync<T> ToAsyncObservable() =>
            new ObservableAsyncAdapter<T>(source);
    }

    /// <summary>Provides Modbus read adapters for async serial master streams.</summary>
    /// <param name="source">The source async serial connection stream.</param>
    extension(IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> source)
    {
        /// <summary>Reads holding registers from an async serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of holding-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegisters(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadHoldingRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads input registers from an async serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegisters(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadInputRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads coils from an async serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of coil data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadCoils(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadCoils(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads discrete inputs from an async serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadInputs(
            byte slaveAddress,
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadInputs(slaveAddress, startAddress, numberOfPoints, interval).ToAsyncObservable();
    }

    /// <summary>Provides Modbus read adapters for async IP master streams.</summary>
    /// <param name="source">The source async connection stream.</param>
    extension(IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> source)
    {
        /// <summary>Reads input registers from an async IP master stream.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegisters(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadInputRegisters(startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads holding registers from an async IP master stream.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of holding-register data and errors.</returns>
        public IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegisters(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadHoldingRegisters(startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads coils from an async IP master stream.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of coil data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadCoils(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadCoils(startAddress, numberOfPoints, interval).ToAsyncObservable();

        /// <summary>Reads discrete inputs from an async IP master stream.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input data and errors.</returns>
        public IObservableAsync<(bool[]? data, Exception? error)> ReadInputs(
            ushort startAddress,
            ushort numberOfPoints,
            double interval = 1000.0) =>
            source.ToObservable().ReadInputs(startAddress, numberOfPoints, interval).ToAsyncObservable();
    }

    /// <summary>Provides async observable write adapters for serial slaves.</summary>
    /// <param name="slave">The async slave stream.</param>
    extension(IObservableAsync<ModbusSerialSlave> slave)
    {
        /// <summary>Writes input registers to serial slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusSerialSlave> WriteInputRegisters(ushort startAddress, IObservableAsync<ushort[]> valuesToWrite) =>
            slave.ToObservable().WriteInputRegisters(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes holding registers to serial slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusSerialSlave> WriteHoldingRegisters(ushort startAddress, IObservableAsync<ushort[]> valuesToWrite) =>
            slave.ToObservable().WriteHoldingRegisters(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes coils to serial slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusSerialSlave> WriteCoilDiscretes(ushort startAddress, IObservableAsync<bool[]> valuesToWrite) =>
            slave.ToObservable().WriteCoilDiscretes(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes discrete inputs to serial slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusSerialSlave> WriteInputDiscretes(ushort startAddress, IObservableAsync<bool[]> valuesToWrite) =>
            slave.ToObservable().WriteInputDiscretes(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();
    }

    /// <summary>Provides async observable write adapters for TCP slaves.</summary>
    /// <param name="slave">The async slave stream.</param>
    extension(IObservableAsync<ModbusTcpSlave> slave)
    {
        /// <summary>Writes input registers to TCP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusTcpSlave> WriteInputRegisters(ushort startAddress, IObservableAsync<ushort[]> valuesToWrite) =>
            slave.ToObservable().WriteInputRegisters(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes holding registers to TCP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusTcpSlave> WriteHoldingRegisters(ushort startAddress, IObservableAsync<ushort[]> valuesToWrite) =>
            slave.ToObservable().WriteHoldingRegisters(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes coils to TCP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusTcpSlave> WriteCoilDiscretes(ushort startAddress, IObservableAsync<bool[]> valuesToWrite) =>
            slave.ToObservable().WriteCoilDiscretes(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes discrete inputs to TCP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusTcpSlave> WriteInputDiscretes(ushort startAddress, IObservableAsync<bool[]> valuesToWrite) =>
            slave.ToObservable().WriteInputDiscretes(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();
    }

    /// <summary>Provides async observable write adapters for UDP slaves.</summary>
    /// <param name="slave">The async slave stream.</param>
    extension(IObservableAsync<ModbusUdpSlave> slave)
    {
        /// <summary>Writes input registers to UDP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusUdpSlave> WriteInputRegisters(ushort startAddress, IObservableAsync<ushort[]> valuesToWrite) =>
            slave.ToObservable().WriteInputRegisters(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes holding registers to UDP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusUdpSlave> WriteHoldingRegisters(ushort startAddress, IObservableAsync<ushort[]> valuesToWrite) =>
            slave.ToObservable().WriteHoldingRegisters(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes coils to UDP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusUdpSlave> WriteCoilDiscretes(ushort startAddress, IObservableAsync<bool[]> valuesToWrite) =>
            slave.ToObservable().WriteCoilDiscretes(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();

        /// <summary>Writes discrete inputs to UDP slave streams from async observable values.</summary>
        /// <param name="startAddress">The starting address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>The async slave stream.</returns>
        public IObservableAsync<ModbusUdpSlave> WriteInputDiscretes(ushort startAddress, IObservableAsync<bool[]> valuesToWrite) =>
            slave.ToObservable().WriteInputDiscretes(startAddress, valuesToWrite.ToObservable()).ToAsyncObservable();
    }

    /// <summary>Provides bridge adapters from async observables to synchronous observables.</summary>
    /// <typeparam name="T">The observed value type.</typeparam>
    /// <param name="source">The source async observable.</param>
    extension<T>(IObservableAsync<T> source)
    {
        /// <summary>Converts an async observable to a synchronous observable.</summary>
        /// <returns>The synchronous observable adapter.</returns>
        public IObservable<T> ToObservable() =>
            new ObservableAdapter<T>(source);
    }

    /// <summary>Provides async observable adapters for Modbus servers.</summary>
    /// <param name="server">The Modbus server.</param>
    extension(ModbusServer server)
    {
        /// <summary>Observes server data changes as an async observable.</summary>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of data snapshots.</returns>
        public IObservableAsync<(ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs)> ObserveDataChangesObservable(double interval = 100) =>
            server.ObserveDataChanges(interval).ToAsyncObservable();

        /// <summary>Observes holding-register changes as an async observable.</summary>
        /// <param name="startAddress">The starting address to monitor.</param>
        /// <param name="count">The number of registers to monitor.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of holding registers.</returns>
        public IObservableAsync<ushort[]> ObserveHoldingRegistersObservable(ushort startAddress = 0, ushort count = 100, double interval = 100) =>
            server.ObserveHoldingRegisters(startAddress, count, interval).ToAsyncObservable();

        /// <summary>Observes input-register changes as an async observable.</summary>
        /// <param name="startAddress">The starting address to monitor.</param>
        /// <param name="count">The number of registers to monitor.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of input registers.</returns>
        public IObservableAsync<ushort[]> ObserveInputRegistersObservable(ushort startAddress = 0, ushort count = 100, double interval = 100) =>
            server.ObserveInputRegisters(startAddress, count, interval).ToAsyncObservable();

        /// <summary>Observes coil changes as an async observable.</summary>
        /// <param name="startAddress">The starting address to monitor.</param>
        /// <param name="count">The number of coils to monitor.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of coils.</returns>
        public IObservableAsync<bool[]> ObserveCoilsObservable(ushort startAddress = 0, ushort count = 100, double interval = 100) =>
            server.ObserveCoils(startAddress, count, interval).ToAsyncObservable();

        /// <summary>Observes discrete input changes as an async observable.</summary>
        /// <param name="startAddress">The starting address to monitor.</param>
        /// <param name="count">The number of inputs to monitor.</param>
        /// <param name="interval">The polling interval in milliseconds.</param>
        /// <returns>An async observable of discrete inputs.</returns>
        public IObservableAsync<bool[]> ObserveDiscreteInputsObservable(ushort startAddress = 0, ushort count = 100, double interval = 100) =>
            server.ObserveDiscreteInputs(startAddress, count, interval).ToAsyncObservable();
    }

    /// <summary>Provides async observable adapters for Modbus slaves.</summary>
    /// <param name="slave">The Modbus slave.</param>
    extension(ModbusSlave slave)
    {
        /// <summary>Observes data-store reads as an async observable.</summary>
        /// <returns>An async observable of data-store events.</returns>
        public IObservableAsync<DataStoreEventArgs> ObserveDataStoreReadFromObservable() =>
            slave.ObserveDataStoreReadFrom().ToAsyncObservable();

        /// <summary>Observes data-store writes as an async observable.</summary>
        /// <returns>An async observable of data-store events.</returns>
        public IObservableAsync<DataStoreEventArgs> ObserveDataStoreWrittenToObservable() =>
            slave.ObserveDataStoreWrittenTo().ToAsyncObservable();

        /// <summary>Observes slave requests as an async observable.</summary>
        /// <returns>An async observable of request events.</returns>
        public IObservableAsync<ModbusSlaveRequestEventArgs> ObserveRequestObservable() =>
            slave.ObserveRequest().ToAsyncObservable();

        /// <summary>Observes write completion as an async observable.</summary>
        /// <returns>An async observable of request events.</returns>
        public IObservableAsync<ModbusSlaveRequestEventArgs> ObserveWriteCompleteObservable() =>
            slave.ObserveWriteComplete().ToAsyncObservable();
    }

    /// <summary>Executes the Complete Value Task operation.</summary>
    /// <param name="valueTask">The value Task value.</param>
    private static void CompleteValueTask(ValueTask valueTask)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            valueTask.GetAwaiter().GetResult();
            return;
        }

        _ = CompleteValueTaskAsync(valueTask);
    }

    /// <summary>Executes the Complete Value Task Async operation.</summary>
    /// <param name="valueTask">The value Task value.</param>
    /// <returns>The result.</returns>
    private static async Task CompleteValueTaskAsync(ValueTask valueTask) =>
        await valueTask.ConfigureAwait(false);

    /// <summary>Provides Observable Adapter functionality.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="source">The source value.</param>
    private sealed class ObservableAdapter<T>(IObservableAsync<T> source) : IObservable<T>
    {
        /// <summary>Executes the Subscribe operation.</summary>
        /// <param name="observer">The observer value.</param>
        /// <returns>The result.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            var subscription = new AsyncToSyncSubscription<T>(source, observer);
            subscription.Connect();
            return subscription;
        }
    }

    /// <summary>Provides Async To Sync Subscription functionality.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="source">The source value.</param>
    /// <param name="observer">The observer value.</param>
    private sealed class AsyncToSyncSubscription<T>(IObservableAsync<T> source, IObserver<T> observer) : IDisposable, IObserverAsync<T>
    {
        /// <summary>Stores the cancellation Token Source value.</summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>Stores the subscription value.</summary>
        private IAsyncDisposable? _subscription;

        /// <summary>Stores the disposed value.</summary>
        private bool _disposed;

        /// <summary>Executes the Connect operation.</summary>
        public void Connect()
        {
            _ = ConnectAsync();
        }

        /// <summary>Executes the On Next Async operation.</summary>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation Token value.</param>
        /// <returns>The result.</returns>
        public ValueTask OnNextAsync(T value, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || _disposed)
            {
                return default;
            }

            observer.OnNext(value);

            return default;
        }

        /// <summary>Executes the On Error Resume Async operation.</summary>
        /// <param name="error">The error value.</param>
        /// <param name="cancellationToken">The cancellation Token value.</param>
        /// <returns>The result.</returns>
        public ValueTask OnErrorResumeAsync(Exception error, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || _disposed)
            {
                return default;
            }

            observer.OnError(error);

            return default;
        }

        /// <summary>Executes the On Completed Async operation.</summary>
        /// <param name="result">The result value.</param>
        /// <returns>The result.</returns>
        public ValueTask OnCompletedAsync(Result result)
        {
            if (_disposed)
            {
                return default;
            }

            observer.OnCompleted();

            return default;
        }

        /// <summary>Executes the Dispose operation.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cancellationTokenSource.Cancel();
            var subscription = Interlocked.Exchange(ref _subscription, null);
            if (subscription is not null)
            {
                subscription.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }

            _cancellationTokenSource.Dispose();
        }

        /// <summary>Executes the Dispose Async operation.</summary>
        /// <returns>The result.</returns>
        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        /// <summary>Executes the Connect Async operation.</summary>
        /// <returns>The result.</returns>
        private async Task ConnectAsync()
        {
            try
            {
                var subscription = await source.SubscribeAsync(this, _cancellationTokenSource.Token);
                if (Interlocked.CompareExchange(ref _subscription, subscription, null) is not null || _disposed)
                {
                    await subscription.DisposeAsync();
                }
            }
            catch (Exception ex) when (!_disposed)
            {
                observer.OnError(ex);
            }
        }
    }

    /// <summary>Provides Observable Async Adapter functionality.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="source">The source value.</param>
    private sealed class ObservableAsyncAdapter<T>(IObservable<T> source) : IObservableAsync<T>
    {
        /// <summary>Executes the Subscribe Async operation.</summary>
        /// <param name="observer">The observer value.</param>
        /// <param name="cancellationToken">The cancellation Token value.</param>
        /// <returns>The result.</returns>
        public ValueTask<IAsyncDisposable> SubscribeAsync(IObserverAsync<T> observer, CancellationToken cancellationToken)
        {
            var subscription = source.Subscribe(new AsyncObserverAdapter<T>(observer, cancellationToken));

            return new ValueTask<IAsyncDisposable>(new AsyncSubscription(subscription));
        }
    }

    /// <summary>Provides Async Observer Adapter functionality.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="observer">The observer value.</param>
    /// <param name="cancellationToken">The cancellation Token value.</param>
    private sealed class AsyncObserverAdapter<T>(IObserverAsync<T> observer, CancellationToken cancellationToken) : IObserver<T>
    {
        /// <summary>Executes the On Completed operation.</summary>
        public void OnCompleted()
        {
            CompleteValueTask(observer.OnCompletedAsync(Result.Success));
        }

        /// <summary>Executes the On Error operation.</summary>
        /// <param name="error">The error value.</param>
        public void OnError(Exception error)
        {
            CompleteValueTask(observer.OnErrorResumeAsync(error, cancellationToken));
        }

        /// <summary>Executes the On Next operation.</summary>
        /// <param name="value">The value.</param>
        public void OnNext(T value)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            CompleteValueTask(observer.OnNextAsync(value, cancellationToken));
        }
    }

    /// <summary>Provides Async Subscription functionality.</summary>
    /// <param name="subscription">The subscription value.</param>
    private sealed class AsyncSubscription(IDisposable subscription) : IAsyncDisposable
    {
        /// <summary>Executes the Dispose Async operation.</summary>
        /// <returns>The result.</returns>
        public ValueTask DisposeAsync()
        {
            subscription.Dispose();
            return default;
        }
    }
}
