// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
using ModbusRx.Data;
using ModbusRx.Device;
using ReactiveUI.Extensions.Async;

#pragma warning disable SA1625

namespace ModbusRx.Reactive;

/// <summary>
/// Async-observable adapters for Modbus reactive operations.
/// </summary>
public static class ModbusAsyncObservableExtensions
{
    /// <summary>
    /// Converts an IP master connection stream to an async observable.
    /// </summary>
    /// <param name="source">The source connection stream.</param>
    /// <returns>The async observable connection stream.</returns>
    public static IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> ToModbusObservableAsync(
        this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source) =>
        source.ToObservableAsync();

    /// <summary>
    /// Converts a serial master connection stream to an async observable.
    /// </summary>
    /// <param name="source">The source connection stream.</param>
    /// <returns>The async observable connection stream.</returns>
    public static IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> ToModbusObservableAsync(
        this IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source) =>
        source.ToObservableAsync();

    /// <summary>
    /// Reads input registers and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegistersAsyncObservable(
        this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadInputRegisters(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads holding registers and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of holding-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegistersAsyncObservable(
        this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadHoldingRegisters(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads coils and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of coil data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadCoilsAsyncObservable(
        this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadCoils(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads discrete inputs and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadInputsAsyncObservable(
        this IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadInputs(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads holding registers from a serial master and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of holding-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegistersAsyncObservable(
        this IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadHoldingRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads input registers from a serial master and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegistersAsyncObservable(
        this IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadInputRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads coils from a serial master and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of coil data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadCoilsAsyncObservable(
        this IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadCoils(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads discrete inputs from a serial master and exposes the polling result as an async observable.
    /// </summary>
    /// <param name="source">The source serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadInputsAsyncObservable(
        this IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ReadInputs(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads input registers from an async IP master stream.
    /// </summary>
    /// <param name="source">The source async connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegisters(
        this IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadInputRegisters(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads holding registers from an async IP master stream.
    /// </summary>
    /// <param name="source">The source async connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of holding-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegisters(
        this IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadHoldingRegisters(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads coils from an async IP master stream.
    /// </summary>
    /// <param name="source">The source async connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of coil data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadCoils(
        this IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadCoils(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads discrete inputs from an async IP master stream.
    /// </summary>
    /// <param name="source">The source async connection stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadInputs(
        this IObservableAsync<(bool connected, Exception? error, ModbusIpMaster? master)> source,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadInputs(startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads holding registers from an async serial master stream.
    /// </summary>
    /// <param name="source">The source async serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of holding-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadHoldingRegisters(
        this IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadHoldingRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads input registers from an async serial master stream.
    /// </summary>
    /// <param name="source">The source async serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input-register data and errors.</returns>
    public static IObservableAsync<(ushort[]? data, Exception? error)> ReadInputRegisters(
        this IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadInputRegisters(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads coils from an async serial master stream.
    /// </summary>
    /// <param name="source">The source async serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of coil data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadCoils(
        this IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadCoils(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Reads discrete inputs from an async serial master stream.
    /// </summary>
    /// <param name="source">The source async serial connection stream.</param>
    /// <param name="slaveAddress">The Modbus slave address.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="numberOfPoints">The number of points.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input data and errors.</returns>
    public static IObservableAsync<(bool[]? data, Exception? error)> ReadInputs(
        this IObservableAsync<(bool connected, Exception? error, IModbusSerialMaster? master)> source,
        byte slaveAddress,
        ushort startAddress,
        ushort numberOfPoints,
        double interval = 1000.0) =>
        source.ToObservable().ReadInputs(slaveAddress, startAddress, numberOfPoints, interval).ToObservableAsync();

    /// <summary>
    /// Observes data-store reads as an async observable.
    /// </summary>
    /// <param name="slave">The slave.</param>
    /// <returns>An async observable of data-store events.</returns>
    public static IObservableAsync<DataStoreEventArgs> ObserveDataStoreReadFromAsync(this ModbusSlave slave) =>
        slave.ObserveDataStoreReadFrom().ToObservableAsync();

    /// <summary>
    /// Observes data-store writes as an async observable.
    /// </summary>
    /// <param name="slave">The slave.</param>
    /// <returns>An async observable of data-store events.</returns>
    public static IObservableAsync<DataStoreEventArgs> ObserveDataStoreWrittenToAsync(this ModbusSlave slave) =>
        slave.ObserveDataStoreWrittenTo().ToObservableAsync();

    /// <summary>
    /// Observes slave requests as an async observable.
    /// </summary>
    /// <param name="slave">The slave.</param>
    /// <returns>An async observable of request events.</returns>
    public static IObservableAsync<ModbusSlaveRequestEventArgs> ObserveRequestAsync(this ModbusSlave slave) =>
        slave.ObserveRequest().ToObservableAsync();

    /// <summary>
    /// Observes write completion as an async observable.
    /// </summary>
    /// <param name="slave">The slave.</param>
    /// <returns>An async observable of request events.</returns>
    public static IObservableAsync<ModbusSlaveRequestEventArgs> ObserveWriteCompleteAsync(this ModbusSlave slave) =>
        slave.ObserveWriteComplete().ToObservableAsync();

    /// <summary>
    /// Observes server data changes as an async observable.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of data snapshots.</returns>
    public static IObservableAsync<(ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs)> ObserveDataChangesAsync(
        this ModbusServer server,
        double interval = 100) =>
        server.ObserveDataChanges(interval).ToObservableAsync();

    /// <summary>
    /// Observes holding-register changes as an async observable.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of registers to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of holding registers.</returns>
    public static IObservableAsync<ushort[]> ObserveHoldingRegistersAsync(this ModbusServer server, ushort startAddress = 0, ushort count = 100, double interval = 100) =>
        server.ObserveHoldingRegisters(startAddress, count, interval).ToObservableAsync();

    /// <summary>
    /// Observes input-register changes as an async observable.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of registers to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of input registers.</returns>
    public static IObservableAsync<ushort[]> ObserveInputRegistersAsync(this ModbusServer server, ushort startAddress = 0, ushort count = 100, double interval = 100) =>
        server.ObserveInputRegisters(startAddress, count, interval).ToObservableAsync();

    /// <summary>
    /// Observes coil changes as an async observable.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of coils to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of coils.</returns>
    public static IObservableAsync<bool[]> ObserveCoilsAsync(this ModbusServer server, ushort startAddress = 0, ushort count = 100, double interval = 100) =>
        server.ObserveCoils(startAddress, count, interval).ToObservableAsync();

    /// <summary>
    /// Observes discrete input changes as an async observable.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of inputs to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An async observable of discrete inputs.</returns>
    public static IObservableAsync<bool[]> ObserveDiscreteInputsAsync(this ModbusServer server, ushort startAddress = 0, ushort count = 100, double interval = 100) =>
        server.ObserveDiscreteInputs(startAddress, count, interval).ToObservableAsync();

    /// <summary>
    /// Writes input registers to TCP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusTcpSlave> WriteInputRegisters(
        this IObservableAsync<ModbusTcpSlave> slave,
        ushort startAddress,
        IObservableAsync<ushort[]> valuesToWrite) =>
        slave.ToObservable().WriteInputRegisters(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes holding registers to TCP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusTcpSlave> WriteHoldingRegisters(
        this IObservableAsync<ModbusTcpSlave> slave,
        ushort startAddress,
        IObservableAsync<ushort[]> valuesToWrite) =>
        slave.ToObservable().WriteHoldingRegisters(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes coils to TCP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusTcpSlave> WriteCoilDiscretes(
        this IObservableAsync<ModbusTcpSlave> slave,
        ushort startAddress,
        IObservableAsync<bool[]> valuesToWrite) =>
        slave.ToObservable().WriteCoilDiscretes(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes discrete inputs to TCP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusTcpSlave> WriteInputDiscretes(
        this IObservableAsync<ModbusTcpSlave> slave,
        ushort startAddress,
        IObservableAsync<bool[]> valuesToWrite) =>
        slave.ToObservable().WriteInputDiscretes(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes input registers to UDP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusUdpSlave> WriteInputRegisters(
        this IObservableAsync<ModbusUdpSlave> slave,
        ushort startAddress,
        IObservableAsync<ushort[]> valuesToWrite) =>
        slave.ToObservable().WriteInputRegisters(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes holding registers to UDP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusUdpSlave> WriteHoldingRegisters(
        this IObservableAsync<ModbusUdpSlave> slave,
        ushort startAddress,
        IObservableAsync<ushort[]> valuesToWrite) =>
        slave.ToObservable().WriteHoldingRegisters(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes coils to UDP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusUdpSlave> WriteCoilDiscretes(
        this IObservableAsync<ModbusUdpSlave> slave,
        ushort startAddress,
        IObservableAsync<bool[]> valuesToWrite) =>
        slave.ToObservable().WriteCoilDiscretes(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes discrete inputs to UDP slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusUdpSlave> WriteInputDiscretes(
        this IObservableAsync<ModbusUdpSlave> slave,
        ushort startAddress,
        IObservableAsync<bool[]> valuesToWrite) =>
        slave.ToObservable().WriteInputDiscretes(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes input registers to serial slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusSerialSlave> WriteInputRegisters(
        this IObservableAsync<ModbusSerialSlave> slave,
        ushort startAddress,
        IObservableAsync<ushort[]> valuesToWrite) =>
        slave.ToObservable().WriteInputRegisters(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes holding registers to serial slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusSerialSlave> WriteHoldingRegisters(
        this IObservableAsync<ModbusSerialSlave> slave,
        ushort startAddress,
        IObservableAsync<ushort[]> valuesToWrite) =>
        slave.ToObservable().WriteHoldingRegisters(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes coils to serial slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusSerialSlave> WriteCoilDiscretes(
        this IObservableAsync<ModbusSerialSlave> slave,
        ushort startAddress,
        IObservableAsync<bool[]> valuesToWrite) =>
        slave.ToObservable().WriteCoilDiscretes(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();

    /// <summary>
    /// Writes discrete inputs to serial slave streams from async observable values.
    /// </summary>
    /// <param name="slave">The async slave stream.</param>
    /// <param name="startAddress">The starting address.</param>
    /// <param name="valuesToWrite">The values to write.</param>
    /// <returns>The async slave stream.</returns>
    public static IObservableAsync<ModbusSerialSlave> WriteInputDiscretes(
        this IObservableAsync<ModbusSerialSlave> slave,
        ushort startAddress,
        IObservableAsync<bool[]> valuesToWrite) =>
        slave.ToObservable().WriteInputDiscretes(startAddress, valuesToWrite.ToObservable()).ToObservableAsync();
}

#pragma warning restore SA1625
#endif
