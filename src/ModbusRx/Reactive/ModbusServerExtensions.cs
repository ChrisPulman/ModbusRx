// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using ModbusRx.Data;

namespace ModbusRx.Reactive;

/// <summary>
/// Reactive extensions for ModbusServer.
/// </summary>
public static class ModbusServerExtensions
{
    /// <summary>
    /// Creates an observable stream of data changes from the server.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of server data.</returns>
    public static IObservable<(ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs)>
        ObserveDataChanges(this Device.ModbusServer server, double interval = 100)
    {
        return Observable.Create<(ushort[], ushort[], bool[], bool[])>(observer =>
        {
            var timer = Observable.Interval(TimeSpan.FromMilliseconds(interval))
                .Subscribe(_ =>
                {
                    try
                    {
                        var data = server.GetCurrentData();
                        observer.OnNext(data);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });

            return Disposable.Create(() => timer.Dispose());
        });
    }

    /// <summary>
    /// Observes changes to holding registers in the server data store.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of registers to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of holding register values.</returns>
    public static IObservable<ushort[]> ObserveHoldingRegisters(
        this Device.ModbusServer server,
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => data.holdingRegisters.Skip(startAddress).Take(count).ToArray())
            .DistinctUntilChanged(new ArrayEqualityComparer<ushort>());
    }

    /// <summary>
    /// Observes changes to input registers in the server data store.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of registers to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of input register values.</returns>
    public static IObservable<ushort[]> ObserveInputRegisters(
        this Device.ModbusServer server,
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => data.inputRegisters.Skip(startAddress).Take(count).ToArray())
            .DistinctUntilChanged(new ArrayEqualityComparer<ushort>());
    }

    /// <summary>
    /// Observes changes to coils in the server data store.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of coils to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of coil values.</returns>
    public static IObservable<bool[]> ObserveCoils(
        this Device.ModbusServer server,
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => data.coils.Skip(startAddress).Take(count).ToArray())
            .DistinctUntilChanged(new ArrayEqualityComparer<bool>());
    }

    /// <summary>
    /// Observes changes to discrete inputs in the server data store.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of inputs to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of discrete input values.</returns>
    public static IObservable<bool[]> ObserveDiscreteInputs(
        this Device.ModbusServer server,
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => data.inputs.Skip(startAddress).Take(count).ToArray())
            .DistinctUntilChanged(new ArrayEqualityComparer<bool>());
    }

    /// <summary>
    /// Creates a reactive server that automatically starts and stops based on subscription.
    /// </summary>
    /// <param name="configureServer">Action to configure the server before starting.</param>
    /// <returns>An observable that represents the server lifecycle.</returns>
    public static IObservable<Device.ModbusServer> CreateReactiveServer(
        Action<Device.ModbusServer> configureServer)
    {
        return Observable.Create<Device.ModbusServer>(observer =>
        {
            var server = new Device.ModbusServer();

            try
            {
                configureServer(server);
                server.Start();
                observer.OnNext(server);
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
                server.Dispose();
                return Disposable.Empty;
            }

            return Disposable.Create(() =>
            {
                server.Stop();
                server.Dispose();
            });
        });
    }
}
