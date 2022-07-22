// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CP.IO.Ports;
using ModbusRx.IO;
using ModbusRx.Message;
using ModbusRx.Unme.Common;

namespace ModbusRx.Device;

/// <summary>
///     Modbus UDP slave device.
/// </summary>
public sealed class ModbusUdpSlave : ModbusSlave
{
    private readonly UdpClientRx _udpClient;

    private ModbusUdpSlave(byte unitId, UdpClientRx udpClient)
        : base(unitId, new ModbusIpTransport(new UdpClientAdapter(udpClient))) => _udpClient = udpClient;

    /// <summary>
    /// Modbus UDP slave factory method.
    /// Creates NModbus UDP slave with default.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>A ModbusUdpSlave.</returns>
    public static ModbusUdpSlave CreateUdp(UdpClientRx client) =>
        new(Modbus.DefaultIpSlaveUnitId, client);

    /// <summary>
    /// Modbus UDP slave factory method.
    /// </summary>
    /// <param name="unitId">The unit identifier.</param>
    /// <param name="client">The client.</param>
    /// <returns>A ModbusUdpSlave.</returns>
    public static ModbusUdpSlave CreateUdp(byte unitId, UdpClientRx client) =>
        new(unitId, client);

    /// <summary>
    ///     Start slave listening for requests.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public override async Task ListenAsync()
    {
        Debug.WriteLine("Start Modbus Udp Server.");

        try
        {
            while (true)
            {
                var receiveResult = await _udpClient.ReceiveAsync().ConfigureAwait(false);
                var masterEndPoint = receiveResult.RemoteEndPoint;
                var frame = receiveResult.Buffer;

                Debug.WriteLine($"Read Frame completed {frame.Length} bytes");
                Debug.WriteLine($"RX: {string.Join(", ", frame)}");

                var request = ModbusMessageFactory.CreateModbusRequest(frame.Slice(6, frame.Length - 6).ToArray());
                request.TransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 0));

                // perform action and build response
                var response = ApplyRequest(request);
                response.TransactionId = request.TransactionId;

                // write response
                var responseFrame = Transport?.BuildMessageFrame(response);
                Debug.WriteLine($"TX: {string.Join(", ", responseFrame!)}");
                await _udpClient.SendAsync(responseFrame!, responseFrame!.Length, masterEndPoint).ConfigureAwait(false);
            }
        }
        catch (SocketException se) when (se.SocketErrorCode == SocketError.Interrupted)
        {
            throw;
        }
    }
}
