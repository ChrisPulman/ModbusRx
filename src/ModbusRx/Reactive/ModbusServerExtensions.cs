// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive;
#else
namespace ModbusRx;
#endif

/// <summary>Reactive extensions for ModbusServer.</summary>
public static class ModbusServerExtensions
{
    /// <summary>Provides observation adapters for Modbus servers.</summary>
    /// <param name="server">The Modbus server.</param>
    extension(Device.ModbusServer server)
    {
    /// <summary>Creates an observable stream of data changes from the server.</summary>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of server data.</returns>
    public IObservable<(ushort[] holdingRegisters, ushort[] inputRegisters, bool[] coils, bool[] inputs)>
        ObserveDataChanges(double interval = 100)
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

    /// <summary>Observes changes to holding registers in the server data store.</summary>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of registers to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of holding register values.</returns>
    public IObservable<ushort[]> ObserveHoldingRegisters(
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => CopyRange(data.holdingRegisters, startAddress, count))
            .DistinctUntilChanged(new ArrayEqualityComparer<ushort>());
    }

    /// <summary>Observes changes to input registers in the server data store.</summary>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of registers to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of input register values.</returns>
    public IObservable<ushort[]> ObserveInputRegisters(
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => CopyRange(data.inputRegisters, startAddress, count))
            .DistinctUntilChanged(new ArrayEqualityComparer<ushort>());
    }

    /// <summary>Observes changes to coils in the server data store.</summary>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of coils to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of coil values.</returns>
    public IObservable<bool[]> ObserveCoils(
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => CopyRange(data.coils, startAddress, count))
            .DistinctUntilChanged(new ArrayEqualityComparer<bool>());
    }

    /// <summary>Observes changes to discrete inputs in the server data store.</summary>
    /// <param name="startAddress">The starting address to monitor.</param>
    /// <param name="count">The number of inputs to monitor.</param>
    /// <param name="interval">The polling interval in milliseconds.</param>
    /// <returns>An observable stream of discrete input values.</returns>
    public IObservable<bool[]> ObserveDiscreteInputs(
        ushort startAddress = 0,
        ushort count = 100,
        double interval = 100)
    {
        return server.ObserveDataChanges(interval)
            .Select(data => CopyRange(data.inputs, startAddress, count))
            .DistinctUntilChanged(new ArrayEqualityComparer<bool>());
    }
    }

    /// <summary>Creates a reactive server that automatically starts and stops based on subscription.</summary>
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
                return EmptyDisposable.Instance;
            }

            return Disposable.Create(() =>
            {
                server.Stop();
                server.Dispose();
            });
        });
    }

    /// <summary>Copies a range from an array.</summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source values.</param>
    /// <param name="startAddress">The zero-based start address.</param>
    /// <param name="count">The requested count.</param>
    /// <returns>The copied range.</returns>
    private static T[] CopyRange<T>(T[] source, ushort startAddress, ushort count)
    {
        var start = (int)startAddress;
        if (start >= source.Length)
        {
            return [];
        }

        var length = Math.Min((int)count, source.Length - start);
        var result = new T[length];
        Array.Copy(source, start, result, 0, length);
        return result;
    }
}
