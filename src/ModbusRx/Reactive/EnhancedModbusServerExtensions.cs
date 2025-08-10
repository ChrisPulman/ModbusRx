// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Utility;

namespace ModbusRx.Reactive;

/// <summary>
/// Enhanced reactive extensions for ModbusServer with performance optimizations.
/// </summary>
public static class EnhancedModbusServerExtensions
{
    /// <summary>
    /// Observes data changes in the server with high-performance optimizations.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="interval">The observation interval in milliseconds.</param>
    /// <returns>An observable of data changes.</returns>
    public static IObservable<ModbusServerDataSnapshot> ObserveDataChangesOptimized(this ModbusServer server, int interval = 100)
    {
        if (server == null)
        {
            throw new ArgumentNullException(nameof(server));
        }

        return Observable.Create<ModbusServerDataSnapshot>(observer =>
        {
            var disposables = new CompositeDisposable();
            var lastSnapshot = new ModbusServerDataSnapshot();
            var hasChanged = false;

            // Subscribe to data store events for immediate change detection
            if (server.DataStore != null)
            {
                var writeSubscription = Observable.FromEvent<EventHandler<DataStoreEventArgs>, DataStoreEventArgs>(
                    handler => (sender, args) => handler(args),
                    handler => server.DataStore.DataStoreWrittenTo += handler,
                    handler => server.DataStore.DataStoreWrittenTo -= handler)
                    .Subscribe(_ => hasChanged = true);

                disposables.Add(writeSubscription);
            }

            // Periodic snapshot with change detection
            var timerSubscription = Observable.Interval(TimeSpan.FromMilliseconds(interval))
                .Where(_ => hasChanged || lastSnapshot.IsEmpty)
                .Select(_ => CreateSnapshot(server))
                .Where(snapshot => !snapshot.Equals(lastSnapshot))
                .Subscribe(snapshot =>
                {
                    lastSnapshot = snapshot;
                    hasChanged = false;
                    observer.OnNext(snapshot);
                });

            disposables.Add(timerSubscription);

            return disposables;
        });
    }

    /// <summary>
    /// Observes holding register changes with range filtering.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="startAddress">The start address to observe.</param>
    /// <param name="count">The number of registers to observe.</param>
    /// <param name="interval">The observation interval in milliseconds.</param>
    /// <returns>An observable of register values.</returns>
    public static IObservable<ushort[]> ObserveHoldingRegistersOptimized(
        this ModbusServer server,
        ushort startAddress,
        ushort count,
        int interval = 100)
    {
        if (server == null)
        {
            throw new ArgumentNullException(nameof(server));
        }

        return Observable.Create<ushort[]>(observer =>
        {
            var lastValues = new ushort[count];
            var hasChanged = false;

            var disposables = new CompositeDisposable();

            // Subscribe to relevant data store changes
            if (server.DataStore != null)
            {
                var writeSubscription = Observable.FromEvent<EventHandler<DataStoreEventArgs>, DataStoreEventArgs>(
                    handler => (sender, args) => handler(args),
                    handler => server.DataStore.DataStoreWrittenTo += handler,
                    handler => server.DataStore.DataStoreWrittenTo -= handler)
                    .Where(args => IsAddressInRange(args.StartAddress, GetDataLength(args), startAddress, count))
                    .Subscribe(_ => hasChanged = true);

                disposables.Add(writeSubscription);
            }

            // Periodic observation with change detection
            var timerSubscription = Observable.Interval(TimeSpan.FromMilliseconds(interval))
                .Where(_ => hasChanged || IsArrayEmpty(lastValues))
                .Subscribe(_ =>
                {
                    try
                    {
                        var currentValues = server.DataStore?.ReadHoldingRegistersOptimized(startAddress, count) ?? new ushort[count];

                        if (!ArraysEqual(currentValues, lastValues))
                        {
                            Array.Copy(currentValues, lastValues, count);
                            hasChanged = false;
                            observer.OnNext(currentValues);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });

            disposables.Add(timerSubscription);

            return disposables;
        });
    }

    /// <summary>
    /// Observes coil changes with range filtering.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="startAddress">The start address to observe.</param>
    /// <param name="count">The number of coils to observe.</param>
    /// <param name="interval">The observation interval in milliseconds.</param>
    /// <returns>An observable of coil values.</returns>
    public static IObservable<bool[]> ObserveCoilsOptimized(
        this ModbusServer server,
        ushort startAddress,
        ushort count,
        int interval = 100)
    {
        if (server == null)
        {
            throw new ArgumentNullException(nameof(server));
        }

        return Observable.Create<bool[]>(observer =>
        {
            var lastValues = new bool[count];
            var hasChanged = false;

            var disposables = new CompositeDisposable();

            // Subscribe to relevant data store changes
            if (server.DataStore != null)
            {
                var writeSubscription = Observable.FromEvent<EventHandler<DataStoreEventArgs>, DataStoreEventArgs>(
                    handler => (sender, args) => handler(args),
                    handler => server.DataStore.DataStoreWrittenTo += handler,
                    handler => server.DataStore.DataStoreWrittenTo -= handler)
                    .Where(args => IsAddressInRange(args.StartAddress, GetDataLength(args), startAddress, count))
                    .Subscribe(_ => hasChanged = true);

                disposables.Add(writeSubscription);
            }

            // Periodic observation with change detection
            var timerSubscription = Observable.Interval(TimeSpan.FromMilliseconds(interval))
                .Where(_ => hasChanged || IsArrayEmpty(lastValues))
                .Subscribe(_ =>
                {
                    try
                    {
                        var currentValues = server.DataStore?.ReadCoilsOptimized(startAddress, count) ?? new bool[count];

                        if (!ArraysEqual(currentValues, lastValues))
                        {
                            Array.Copy(currentValues, lastValues, count);
                            hasChanged = false;
                            observer.OnNext(currentValues);
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });

            disposables.Add(timerSubscription);

            return disposables;
        });
    }

    /// <summary>
    /// Creates a buffered observable with change detection and batching.
    /// </summary>
    /// <param name="server">The Modbus server.</param>
    /// <param name="bufferSize">The buffer size for batching changes.</param>
    /// <param name="bufferTimeMilliseconds">The buffer time window in milliseconds.</param>
    /// <returns>An observable of batched data changes.</returns>
    public static IObservable<ModbusServerDataSnapshot[]> ObserveDataChangesBuffered(
        this ModbusServer server,
        int bufferSize = 10,
        int bufferTimeMilliseconds = 1000)
    {
        if (server == null)
        {
            throw new ArgumentNullException(nameof(server));
        }

        var timeSpan = TimeSpan.FromMilliseconds(bufferTimeMilliseconds);

        return server.ObserveDataChangesOptimized()
            .Buffer(timeSpan, bufferSize)
            .Select(list => list.ToArray())
            .Where(buffer => buffer.Length > 0);
    }

    private static ModbusServerDataSnapshot CreateSnapshot(ModbusServer server)
    {
        try
        {
            var dataStore = server.DataStore;
            if (dataStore == null)
            {
                return new ModbusServerDataSnapshot();
            }

            // Read a reasonable range of data for snapshot
            const ushort maxRegisters = 100;
            const ushort maxCoils = 100;

            var holdingRegisters = dataStore.ReadHoldingRegistersOptimized(1, Math.Min(maxRegisters, (ushort)Math.Max(1, dataStore.HoldingRegisters.Count - 1)));
            var inputRegisters = dataStore.ReadInputRegistersOptimized(1, Math.Min(maxRegisters, (ushort)Math.Max(1, dataStore.InputRegisters.Count - 1)));
            var coils = dataStore.ReadCoilsOptimized(1, Math.Min(maxCoils, (ushort)Math.Max(1, dataStore.CoilDiscretes.Count - 1)));
            var inputs = dataStore.ReadInputsOptimized(1, Math.Min(maxCoils, (ushort)Math.Max(1, dataStore.InputDiscretes.Count - 1)));

            return new ModbusServerDataSnapshot
            {
                HoldingRegisters = holdingRegisters,
                InputRegisters = inputRegisters,
                Coils = coils,
                Inputs = inputs,
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return new ModbusServerDataSnapshot();
        }
    }

    private static int GetDataLength(DataStoreEventArgs args)
    {
        if (args.Data == null)
        {
            return 0;
        }

        return args.Data.Option switch
        {
            DiscriminatedUnionOption.A => args.Data.A?.Count ?? 0,
            DiscriminatedUnionOption.B => args.Data.B?.Count ?? 0,
            _ => 0
        };
    }

    private static bool IsAddressInRange(ushort startAddress, int length, ushort observeStart, ushort observeCount)
    {
        var endAddress = startAddress + length - 1;
        var observeEnd = observeStart + observeCount - 1;

        return !(endAddress < observeStart || startAddress > observeEnd);
    }

    private static bool ArraysEqual<T>(T[] array1, T[] array2)
        where T : IEquatable<T>
    {
        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (var i = 0; i < array1.Length; i++)
        {
            if (!array1[i].Equals(array2[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsArrayEmpty<T>(T[] array)
        where T : struct => array.All(item => EqualityComparer<T>.Default.Equals(item, default));
}
