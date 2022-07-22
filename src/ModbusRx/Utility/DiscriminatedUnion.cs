// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ModbusRx.Utility;

/// <summary>
///     Possible options for DiscriminatedUnion type.
/// </summary>
public enum DiscriminatedUnionOption
{
    /// <summary>
    ///     Option A.
    /// </summary>
    A,

    /// <summary>
    ///     Option B.
    /// </summary>
    B,
}

/// <summary>
///     A data type that can store one of two possible strongly typed options.
/// </summary>
/// <typeparam name="TA">The type of option A.</typeparam>
/// <typeparam name="TB">The type of option B.</typeparam>
#pragma warning disable SA1402 // File may only contain a single type
public class DiscriminatedUnion<TA, TB>
#pragma warning restore SA1402 // File may only contain a single type
{
    private TA? _optionA;
    private TB? _optionB;

    /// <summary>
    ///     Gets the value of option A.
    /// </summary>
    public TA? A
    {
        get
        {
            if (Option != DiscriminatedUnionOption.A)
            {
                var msg = $"{DiscriminatedUnionOption.A} is not a valid option for this discriminated union instance.";
                throw new InvalidOperationException(msg);
            }

            return _optionA;
        }
    }

    /// <summary>
    ///     Gets the value of option B.
    /// </summary>
    public TB? B
    {
        get
        {
            if (Option != DiscriminatedUnionOption.B)
            {
                var msg = $"{DiscriminatedUnionOption.B} is not a valid option for this discriminated union instance.";
                throw new InvalidOperationException(msg);
            }

            return _optionB;
        }
    }

    /// <summary>
    ///     Gets the discriminated value option set for this instance.
    /// </summary>
    public DiscriminatedUnionOption Option { get; private set; }

    /// <summary>
    /// Factory method for creating DiscriminatedUnion with option A set.
    /// </summary>
    /// <param name="a">a.</param>
    /// <returns>A DiscriminatedUnion.</returns>
    public static DiscriminatedUnion<TA, TB> CreateA(TA a) =>
        new() { Option = DiscriminatedUnionOption.A, _optionA = a };

    /// <summary>
    /// Factory method for creating DiscriminatedUnion with option B set.
    /// </summary>
    /// <param name="b">The b.</param>
    /// <returns>A DiscriminatedUnion.</returns>
    public static DiscriminatedUnion<TA, TB> CreateB(TB b) =>
        new() { Option = DiscriminatedUnionOption.B, _optionB = b };

    /// <summary>
    ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string? ToString() =>
        Option switch
        {
            DiscriminatedUnionOption.A => A?.ToString(),
            DiscriminatedUnionOption.B => B?.ToString(),
            _ => null,
        };
}
