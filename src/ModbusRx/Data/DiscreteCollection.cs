// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

#if REACTIVE_SHIM
namespace ModbusRx.Reactive.Data;
#else
namespace ModbusRx.Data;
#endif

/// <summary>Collection of discrete values.</summary>
public class DiscreteCollection : Collection<bool>, IDataCollection
{
    /// <summary>Defines the Bits Per Byte value.</summary>
    private const int BitsPerByte = 8;

    /// <summary>Stores the discretes value.</summary>
    private readonly List<bool> _discretes;

    /// <summary>Initializes a new instance of the <see cref="DiscreteCollection" /> class.</summary>
    public DiscreteCollection()
        : this((bool[])[])
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DiscreteCollection" /> class.</summary>
    /// <param name="bits">Array for discrete collection.</param>
    public DiscreteCollection(params bool[] bits)
        : this((IList<bool>)bits)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DiscreteCollection" /> class.</summary>
    /// <param name="bytes">Array for discrete collection.</param>
    public DiscreteCollection(params byte[] bytes)
        : this()
    {
        if (bytes is null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        _discretes.Capacity = bytes.Length * BitsPerByte;

        foreach (var b in bytes)
        {
            _discretes.Add((b & 1) == 1);
            _discretes.Add((b & 2) == 2);
            _discretes.Add((b & 4) == 4);
            _discretes.Add((b & 8) == 8);
            _discretes.Add((b & 16) == 16);
            _discretes.Add((b & 32) == 32);
            _discretes.Add((b & 64) == 64);
            _discretes.Add((b & 128) == 128);
        }
    }

    /// <summary>Initializes a new instance of the <see cref="DiscreteCollection" /> class.</summary>
    /// <param name="bits">List for discrete collection.</param>
    public DiscreteCollection(IList<bool> bits)
        : this(new List<bool>(bits))
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DiscreteCollection" /> class.</summary>
    /// <param name="bits">List for discrete collection.</param>
    internal DiscreteCollection(List<bool> bits)
        : base(bits)
    {
        Debug.Assert(bits is not null, "Discrete bits is null.");
        _discretes = bits!;
    }

    /// <summary>Gets the network bytes.</summary>
    public byte[] NetworkBytes
    {
        get
        {
            var bytes = new byte[ByteCount];

            for (var index = 0; index < _discretes.Count; index++)
            {
                if (_discretes[index])
                {
                    bytes[index / BitsPerByte] |= (byte)(1 << (index % BitsPerByte));
                }
            }

            return bytes;
        }
    }

    /// <summary>Gets the byte count.</summary>
    public byte ByteCount => (byte)((Count + 7) / 8);

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

            _ = builder.Append(this[i] ? "1" : "0");
        }

        _ = builder.Append('}');
        return builder.ToString();
    }
}
