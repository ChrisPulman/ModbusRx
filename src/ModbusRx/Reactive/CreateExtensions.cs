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

#if REACTIVE_SHIM
namespace ModbusRx.Reactive;
#else
namespace ModbusRx;
#endif

/// <summary>Extension methods for Modbus reactive creation helpers.</summary>
public static class CreateExtensions
{
    /// <summary>Provides polling read operations for serial master streams.</summary>
    /// <param name="source">The source serial connection stream.</param>
    extension(IObservable<(bool connected, Exception? error, IModbusSerialMaster? master)> source)
    {
        /// <summary>Reads coils from the serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(bool[]? data, Exception? error)> ReadCoils(byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadCoilsCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads coils from slave address 1.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(bool[]? data, Exception? error)> ReadCoils(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadCoilsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads holding registers from the serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadHoldingRegistersCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads holding registers from slave address 1.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadHoldingRegistersCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads discrete inputs from the serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(bool[]? data, Exception? error)> ReadInputs(byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadInputsCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads discrete inputs from slave address 1.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(bool[]? data, Exception? error)> ReadInputs(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadInputsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads input registers from the serial master stream.</summary>
        /// <param name="slaveAddress">The Modbus slave address.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(byte slaveAddress, ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadInputRegistersCore(source, slaveAddress, startAddress, numberOfPoints, interval);

        /// <summary>Reads input registers from slave address 1.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadInputRegistersCore(source, startAddress, numberOfPoints, interval);
    }

    /// <summary>Provides polling read operations for IP master streams.</summary>
    /// <param name="source">The source connection stream.</param>
    extension(IObservable<(bool connected, Exception? error, ModbusIpMaster? master)> source)
    {
        /// <summary>Reads coils from the IP master stream.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(bool[]? data, Exception? error)> ReadCoils(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadCoilsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads holding registers from the IP master stream.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(ushort[]? data, Exception? error)> ReadHoldingRegisters(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadHoldingRegistersCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads discrete inputs from the IP master stream.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(bool[]? data, Exception? error)> ReadInputs(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadInputsCore(source, startAddress, numberOfPoints, interval);

        /// <summary>Reads input registers from the IP master stream.</summary>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points.</param>
        /// <param name="interval">The polling interval.</param>
        /// <returns>An observable of data and error tuples.</returns>
        public IObservable<(ushort[]? data, Exception? error)> ReadInputRegisters(ushort startAddress, ushort numberOfPoints, double interval = 1000.0) =>
            Create.ReadInputRegistersCore(source, startAddress, numberOfPoints, interval);
    }

    /// <summary>Provides observable event adapters for Modbus slaves.</summary>
    /// <param name="slave">The slave.</param>
    extension(ModbusSlave slave)
    {
        /// <summary>Observes reads from the data store.</summary>
        /// <returns>An observable of data-store events.</returns>
        public IObservable<DataStoreEventArgs> ObserveDataStoreReadFrom() =>
            Create.ObserveDataStoreReadFromCore(slave);

        /// <summary>Observes received slave requests.</summary>
        /// <returns>An observable of request events.</returns>
        public IObservable<ModbusSlaveRequestEventArgs> ObserveRequest() =>
            Create.ObserveRequestCore(slave);

        /// <summary>Observes completed writes.</summary>
        /// <returns>An observable of request events.</returns>
        public IObservable<ModbusSlaveRequestEventArgs> ObserveWriteComplete() =>
            Create.ObserveWriteCompleteCore(slave);

        /// <summary>Observes writes to the data store.</summary>
        /// <returns>An observable of data-store events.</returns>
        public IObservable<DataStoreEventArgs> ObserveDataStoreWrittenTo() =>
            Create.ObserveDataStoreWrittenToCore(slave);
    }

    /// <summary>Provides conversion operations for register spans.</summary>
    /// <param name="inputs">The input register span.</param>
    extension(ReadOnlySpan<ushort> inputs)
    {
        /// <summary>Converts register data to a double.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        /// <returns>A double value or null if insufficient data is available.</returns>
        public double? ToDouble(int start, bool swapWords = true) =>
            Create.ToDoubleCore(inputs, start, swapWords);

        /// <summary>Converts register data to a float.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        /// <returns>A float value or null if insufficient data is available.</returns>
        public float? ToFloat(int start, bool swapWords = true) =>
            Create.ToFloatCore(inputs, start, swapWords);
    }

    /// <summary>Provides register-write conversions for double values.</summary>
    /// <param name="input">The input value.</param>
    extension(double input)
    {
        /// <summary>Writes the double value to a register span.</summary>
        /// <param name="output">The output span.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        public void FromDouble(Span<ushort> output, int start, bool swapWords = true) =>
            Create.FromDoubleCore(input, output, start, swapWords);

        /// <summary>Writes the double value to a register array.</summary>
        /// <param name="output">The output array.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        public void FromDouble(ushort[] output, int start, bool swapWords = true) =>
            Create.FromDoubleCore(input, output, start, swapWords);
    }

    /// <summary>Provides register-write conversions for float values.</summary>
    /// <param name="input">The input value.</param>
    extension(float input)
    {
        /// <summary>Writes the float value to a register span.</summary>
        /// <param name="output">The output span.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        public void FromFloat(Span<ushort> output, int start, bool swapWords = true) =>
            Create.FromFloatCore(input, output, start, swapWords);

        /// <summary>Writes the float value to a register array.</summary>
        /// <param name="output">The output array.</param>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        public void FromFloat(ushort[] output, int start, bool swapWords = true) =>
            Create.FromFloatCore(input, output, start, swapWords);
    }

    /// <summary>Provides conversion operations for nullable register arrays.</summary>
    /// <param name="inputs">The input register array.</param>
    extension(ushort[]? inputs)
    {
        /// <summary>Converts register data to a double.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        /// <returns>A double value or null if insufficient data is available.</returns>
        public double? ToDouble(int start, bool swapWords = true) =>
            Create.ToDoubleCore(inputs, start, swapWords);

        /// <summary>Converts register data to a float.</summary>
        /// <param name="start">The start index.</param>
        /// <param name="swapWords">Whether to swap words.</param>
        /// <returns>A float value or null if insufficient data is available.</returns>
        public float? ToFloat(int start, bool swapWords = true) =>
            Create.ToFloatCore(inputs, start, swapWords);
    }
}
