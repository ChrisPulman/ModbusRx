// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
using ModbusRx.Reactive.Data;
#else
using ModbusRx.Data;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.IO;
#else
using ModbusRx.IO;
#endif
#if REACTIVE_SHIM
using ModbusRx.Reactive.Message;
#else
using ModbusRx.Message;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Device;
#else
namespace ModbusRx.Device;
#endif

/// <summary>Modbus master device.</summary>
public abstract class ModbusMaster : ModbusDevice, IModbusMaster
{
    /// <summary>Initializes a new instance of the Modbus Master class.</summary>
    /// <param name="transport">The transport value.</param>
    internal ModbusMaster(ModbusTransport transport)
        : base(transport)
    {
    }

    /// <summary>Asynchronously reads from 1 to 2000 contiguous coils status.</summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of coils to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public Task<bool[]> ReadCoilsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 2000);

        var request = new ReadCoilsInputsRequest(
            Modbus.ReadCoils,
            slaveAddress,
            startAddress,
            numberOfPoints);

        return PerformReadDiscretesAsync(request);
    }

    /// <summary>Asynchronously reads from 1 to 2000 contiguous discrete input status.</summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of discrete inputs to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public Task<bool[]> ReadInputsAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 2000);

        var request = new ReadCoilsInputsRequest(
            Modbus.ReadInputs,
            slaveAddress,
            startAddress,
            numberOfPoints);

        return PerformReadDiscretesAsync(request);
    }

    /// <summary>Asynchronously reads contiguous block of holding registers.</summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public Task<ushort[]> ReadHoldingRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 125);

        var request = new ReadHoldingInputRegistersRequest(
            Modbus.ReadHoldingRegisters,
            slaveAddress,
            startAddress,
            numberOfPoints);

        return PerformReadRegistersAsync(request);
    }

    /// <summary>Asynchronously reads contiguous block of input registers.</summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startAddress">Address to begin reading.</param>
    /// <param name="numberOfPoints">Number of holding registers to read.</param>
    /// <returns>A task that represents the asynchronous read operation.</returns>
    public Task<ushort[]> ReadInputRegistersAsync(byte slaveAddress, ushort startAddress, ushort numberOfPoints)
    {
        ValidateNumberOfPoints(nameof(numberOfPoints), numberOfPoints, 125);

        var request = new ReadHoldingInputRegistersRequest(
            Modbus.ReadInputRegisters,
            slaveAddress,
            startAddress,
            numberOfPoints);

        return PerformReadRegistersAsync(request);
    }

    /// <summary>Asynchronously writes a single coil value.</summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="coilAddress">Address to write value to.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task WriteSingleCoilAsync(byte slaveAddress, ushort coilAddress, bool value)
    {
        var request = new WriteSingleCoilRequestResponse(slaveAddress, coilAddress, value);
        return PerformWriteRequestAsync<WriteSingleCoilRequestResponse>(request);
    }

    /// <summary>Asynchronously writes a single holding register.</summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="registerAddress">Address to write.</param>
    /// <param name="value">Value to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task WriteSingleRegisterAsync(byte slaveAddress, ushort registerAddress, ushort value)
    {
        var request = new WriteSingleRegisterRequestResponse(
            slaveAddress,
            registerAddress,
            value);

        return PerformWriteRequestAsync<WriteSingleRegisterRequestResponse>(request);
    }

    /// <summary>Asynchronously writes a block of 1 to 123 contiguous registers.</summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task WriteMultipleRegistersAsync(byte slaveAddress, ushort startAddress, ushort[] data)
    {
        ValidateData(nameof(data), data, 123);

        var request = new WriteMultipleRegistersRequest(
            slaveAddress,
            startAddress,
            new RegisterCollection(data));

        return PerformWriteRequestAsync<WriteMultipleRegistersResponse>(request);
    }

    /// <summary>Asynchronously writes a sequence of coils.</summary>
    /// <param name="slaveAddress">Address of the device to write to.</param>
    /// <param name="startAddress">Address to begin writing values.</param>
    /// <param name="data">Values to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task WriteMultipleCoilsAsync(byte slaveAddress, ushort startAddress, bool[] data)
    {
        ValidateData(nameof(data), data, 1968);

        var request = new WriteMultipleCoilsRequest(
            slaveAddress,
            startAddress,
            new DiscreteCollection(data));

        return PerformWriteRequestAsync<WriteMultipleCoilsResponse>(request);
    }

    /// <summary>
    /// Asynchronously performs a combined write and read holding-register transaction.
    /// The write operation is performed before the read.
    /// </summary>
    /// <param name="slaveAddress">Address of device to read values from.</param>
    /// <param name="startReadAddress">Address to begin reading (Holding registers are addressed starting at 0).</param>
    /// <param name="numberOfPointsToRead">Number of registers to read.</param>
    /// <param name="startWriteAddress">Address to begin writing (Holding registers are addressed starting at 0).</param>
    /// <param name="writeData">Register values to write.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task<ushort[]> ReadWriteMultipleRegistersAsync(
        byte slaveAddress,
        ushort startReadAddress,
        ushort numberOfPointsToRead,
        ushort startWriteAddress,
        ushort[] writeData)
    {
        ValidateNumberOfPoints("numberOfPointsToRead", numberOfPointsToRead, 125);
        ValidateData(nameof(writeData), writeData, 121);

        var request = new ReadWriteMultipleRegistersRequest(slaveAddress, startReadAddress, numberOfPointsToRead, startWriteAddress, new RegisterCollection(writeData));

        return PerformReadRegistersAsync(request);
    }

    /// <summary>Executes the custom message.</summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="request">The request.</param>
    /// <returns>A Responce of type T.</returns>
    public TResponse ExecuteCustomMessage<TResponse>(IModbusMessage request)
        where TResponse : IModbusMessage, new() =>
        Transport!.UnicastMessage<TResponse>(request);

    /// <summary>Executes the Validate Data operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="argumentName">The argument Name value.</param>
    /// <param name="data">The data value.</param>
    /// <param name="maxDataLength">The max Data Length value.</param>
    private static void ValidateData<T>(string argumentName, T[] data, int maxDataLength)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data.Length != 0 && data.Length <= maxDataLength)
        {
            return;
        }

        var msg = $"The length of argument {argumentName} must be between 1 and {maxDataLength} inclusive.";
        throw new ArgumentException(msg);
    }

    /// <summary>Executes the Validate Number Of Points operation.</summary>
    /// <param name="argumentName">The argument Name value.</param>
    /// <param name="numberOfPoints">The number Of Points value.</param>
    /// <param name="maxNumberOfPoints">The max Number Of Points value.</param>
    private static void ValidateNumberOfPoints(string argumentName, ushort numberOfPoints, ushort maxNumberOfPoints)
    {
        if (numberOfPoints >= 1 && numberOfPoints <= maxNumberOfPoints)
        {
            return;
        }

        var msg = $"Argument {argumentName} must be between 1 and {maxNumberOfPoints} inclusive.";
        throw new ArgumentException(msg);
    }

    /// <summary>Copies response data into an array of the requested length.</summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="data">The response data.</param>
    /// <param name="count">The number of values to copy.</param>
    /// <returns>The copied values.</returns>
    private static T[] CopyResponseData<T>(IList<T> data, ushort count)
    {
        var result = new T[count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = data[i];
        }

        return result;
    }

    /// <summary>Executes the Perform Read Discretes operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private bool[] PerformReadDiscretes(ReadCoilsInputsRequest request)
    {
        var response = Transport?.UnicastMessage<ReadCoilsInputsResponse>(request);
        return CopyResponseData(response!.Data, request.NumberOfPoints);
    }

    /// <summary>Executes the Perform Read Discretes Async operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private Task<bool[]> PerformReadDiscretesAsync(ReadCoilsInputsRequest request) =>
        Task.Factory.StartNew(
            () => PerformReadDiscretes(request),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);

    /// <summary>Executes the Perform Read Registers operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private ushort[] PerformReadRegisters(ReadHoldingInputRegistersRequest request)
    {
        var response = Transport?.UnicastMessage<ReadHoldingInputRegistersResponse>(request);

        return CopyResponseData(response!.Data, request.NumberOfPoints);
    }

    /// <summary>Executes the Perform Read Registers operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private ushort[] PerformReadRegisters(ReadWriteMultipleRegistersRequest request)
    {
        var response = Transport?.UnicastMessage<ReadHoldingInputRegistersResponse>(request);

        return CopyResponseData(response!.Data, request.ReadRequest!.NumberOfPoints);
    }

    /// <summary>Executes the Perform Read Registers Async operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private Task<ushort[]> PerformReadRegistersAsync(ReadHoldingInputRegistersRequest request) =>
        Task.Factory.StartNew(
            () => PerformReadRegisters(request),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);

    /// <summary>Executes the Perform Read Registers Async operation.</summary>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private Task<ushort[]> PerformReadRegistersAsync(ReadWriteMultipleRegistersRequest request) =>
        Task.Factory.StartNew(
            () => PerformReadRegisters(request),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);

    /// <summary>Executes the Perform Write Request Async operation.</summary>
    /// <typeparam name="T">The T type.</typeparam>
    /// <param name="request">The request value.</param>
    /// <returns>The result.</returns>
    private Task PerformWriteRequestAsync<T>(IModbusMessage request)
        where T : IModbusMessage, new() =>
        Task.Factory.StartNew(
            () => Transport?.UnicastMessage<T>(request),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);
}
