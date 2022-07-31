// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using ModbusRx.Data;
using ModbusRx.Device;
using ModbusRx.Message;

namespace ModbusRx.Reactive
{
    /// <summary>
    /// ModbusUdpSlaveExtensions.
    /// </summary>
    public static class ModbusUdpSlaveExtensions
    {
        /// <summary>
        /// Writes the Input Registers.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>
        /// Observable ModbusTcpSlave.
        /// </returns>
        public static IObservable<ModbusUdpSlave> WriteInputRegisters(this IObservable<ModbusUdpSlave> slave, ushort startAddress, IObservable<ushort[]> valuesToWrite)
        {
            slave.CombineLatest(
            valuesToWrite, (slave, data) => (slave, data))
                .Subscribe(source => ModbusSlave.WriteMultipleRegisters(new WriteMultipleRegistersRequest(1, startAddress, new RegisterCollection(source.data)), source.slave.DataStore, source.slave.DataStore.InputRegisters));
            return slave;
        }

        /// <summary>
        /// Writes the holding registers.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>
        /// Observable ModbusTcpSlave.
        /// </returns>
        public static IObservable<ModbusUdpSlave> WriteHoldingRegisters(this IObservable<ModbusUdpSlave> slave, ushort startAddress, IObservable<ushort[]> valuesToWrite)
        {
            slave.CombineLatest(
            valuesToWrite, (slave, data) => (slave, data))
                .Subscribe(source => ModbusSlave.WriteMultipleRegisters(new WriteMultipleRegistersRequest(1, startAddress, new RegisterCollection(source.data)), source.slave.DataStore, source.slave.DataStore.HoldingRegisters));
            return slave;
        }

        /// <summary>
        /// Writes the coil discretes.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>Observable ModbusTcpSlave.</returns>
        public static IObservable<ModbusUdpSlave> WriteCoilDiscretes(this IObservable<ModbusUdpSlave> slave, ushort startAddress, IObservable<bool[]> valuesToWrite)
        {
            slave.CombineLatest(
            valuesToWrite, (slave, data) => (slave, data))
                .Subscribe(source => ModbusSlave.WriteMultipleCoils(new WriteMultipleCoilsRequest(1, startAddress, new DiscreteCollection(source.data)), source.slave.DataStore, source.slave.DataStore.CoilDiscretes));
            return slave;
        }

        /// <summary>
        /// Writes the input discretes.
        /// </summary>
        /// <param name="slave">The slave.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="valuesToWrite">The values to write.</param>
        /// <returns>Observable ModbusTcpSlave.</returns>
        public static IObservable<ModbusUdpSlave> WriteInputDiscretes(this IObservable<ModbusUdpSlave> slave, ushort startAddress, IObservable<bool[]> valuesToWrite)
        {
            slave.CombineLatest(
            valuesToWrite, (slave, data) => (slave, data))
                .Subscribe(source => ModbusSlave.WriteMultipleCoils(new WriteMultipleCoilsRequest(1, startAddress, new DiscreteCollection(source.data)), source.slave.DataStore, source.slave.DataStore.InputDiscretes));
            return slave;
        }
    }
}
