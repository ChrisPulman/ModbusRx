// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Net;
using System.Text;
#if REACTIVE_SHIM
using ModbusRx.Reactive.Utility;
#else
using ModbusRx.Utility;
#endif

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Collection of 16 bit registers.</summary>
public class RegisterCollection : Collection<ushort>, IDataCollection
{
    /// <summary>Initializes a new instance of the <see cref="RegisterCollection" /> class.</summary>
    public RegisterCollection()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RegisterCollection" /> class.</summary>
    /// <param name="bytes">Array for register collection.</param>
    public RegisterCollection(byte[] bytes)
        : this((IList<ushort>)ModbusUtility.NetworkBytesToHostUInt16(bytes))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RegisterCollection" /> class.</summary>
    /// <param name="registers">Array for register collection.</param>
    public RegisterCollection(params ushort[] registers)
        : this((IList<ushort>)registers)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="RegisterCollection" /> class.</summary>
    /// <param name="registers">List for register collection.</param>
    public RegisterCollection(IList<ushort> registers)
        : base(registers?.IsReadOnly == true ? [.. registers] : registers!)
    {
    }

    /// <summary>Gets the network bytes.</summary>
    public byte[] NetworkBytes
    {
        get
        {
            var bytes = new MemoryStream(ByteCount);

            foreach (var register in this)
            {
                var b = BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)register));
                bytes.Write(b, 0, b.Length);
            }

            return bytes.ToArray();
        }
    }

    /// <summary>Gets the byte count.</summary>
    public byte ByteCount => (byte)(Count * 2);

    /// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.</summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string ToString()
    {
        var builder = new StringBuilder("{");
        for (var i = 0; i < Count; i++)
        {
            if (i > 0)
            {
                _ = builder.Append(", ");
            }

            _ = builder.Append(this[i]);
        }

        _ = builder.Append('}');
        return builder.ToString();
    }
}
