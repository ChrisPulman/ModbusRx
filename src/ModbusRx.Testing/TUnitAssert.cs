// Copyright (c) 2022-2026 Chris Pulman. All rights reserved.
// Chris Pulman licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TUnitAssertions = TUnit.Assertions.Assert;

namespace ModbusRx.Testing;

/// <summary>Provides xUnit-style assertion helpers backed by TUnit assertions.</summary>
internal static class TUnitAssert
{
    /// <summary>Asserts that a condition is true.</summary>
    /// <param name="condition">The condition to verify.</param>
    /// <param name="message">The optional failure message.</param>
    public static void True(bool condition, string? message = null)
    {
        if (condition)
        {
            return;
        }

        Fail(message ?? "Expected the condition to be true.");
    }

    /// <summary>Asserts that a condition is false.</summary>
    /// <param name="condition">The condition to verify.</param>
    /// <param name="message">The optional failure message.</param>
    public static void False(bool condition, string? message = null)
    {
        if (!condition)
        {
            return;
        }

        Fail(message ?? "Expected the condition to be false.");
    }

    /// <summary>Asserts that two strongly typed values are equal.</summary>
    /// <typeparam name="T">The value type to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    public static void Equal<T>(T expected, T actual)
    {
        if (AreEqual(expected, actual))
        {
            return;
        }

        Fail($"Expected: {Format(expected)}{Environment.NewLine}Actual:   {Format(actual)}");
    }

    /// <summary>Asserts that two object values are equal.</summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    public static void Equal(object? expected, object? actual)
    {
        if (AreEqual(expected, actual))
        {
            return;
        }

        Fail($"Expected: {Format(expected)}{Environment.NewLine}Actual:   {Format(actual)}");
    }

    /// <summary>Asserts that two values are not equal.</summary>
    /// <typeparam name="T">The value type to compare.</typeparam>
    /// <param name="notExpected">The value that should not be observed.</param>
    /// <param name="actual">The actual value.</param>
    public static void NotEqual<T>(T notExpected, T actual)
    {
        if (!AreEqual(notExpected, actual))
        {
            return;
        }

        Fail($"Did not expect: {Format(actual)}");
    }

    /// <summary>Asserts that two references point to the same object.</summary>
    /// <param name="expected">The expected reference.</param>
    /// <param name="actual">The actual reference.</param>
    public static void Same(object? expected, object? actual)
    {
        if (ReferenceEquals(expected, actual))
        {
            return;
        }

        Fail("Expected both references to point to the same object.");
    }

    /// <summary>Asserts that a reference value is not null.</summary>
    /// <typeparam name="T">The reference type to verify.</typeparam>
    /// <param name="value">The value to verify.</param>
    /// <returns>The non-null reference value.</returns>
    public static T NotNull<T>(T? value)
        where T : class
    {
        if (value is not null)
        {
            return value;
        }

        Fail("Expected a non-null reference value.");
        return value!;
    }

    /// <summary>Asserts that a nullable value type contains a value.</summary>
    /// <typeparam name="T">The value type to verify.</typeparam>
    /// <param name="value">The value to verify.</param>
    public static void NotNull<T>(T? value)
        where T : struct
    {
        if (value.HasValue)
        {
            return;
        }

        Fail("Expected a non-null value.");
    }

    /// <summary>Asserts that a value is null.</summary>
    /// <param name="value">The value to verify.</param>
    public static void Null(object? value)
    {
        if (value is null)
        {
            return;
        }

        Fail($"Expected a null value but found {Format(value)}.");
    }

    /// <summary>Asserts that a sequence contains no items.</summary>
    /// <param name="values">The sequence to inspect.</param>
    public static void Empty(IEnumerable values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var enumerator = values.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
            {
                return;
            }

            Fail("Expected an empty collection.");
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    /// <summary>Asserts that a sequence contains at least one item.</summary>
    /// <param name="values">The sequence to inspect.</param>
    public static void NotEmpty(IEnumerable values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var enumerator = values.GetEnumerator();
        try
        {
            if (enumerator.MoveNext())
            {
                return;
            }

            Fail("Expected a non-empty collection.");
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    /// <summary>Asserts that a typed sequence contains exactly one item.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="values">The sequence to inspect.</param>
    /// <returns>The single item from the sequence.</returns>
    public static T Single<T>(IEnumerable<T> values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        using var enumerator = values.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            Fail("Expected exactly one item but found none.");
        }

        var single = enumerator.Current;
        if (enumerator.MoveNext())
        {
            Fail("Expected exactly one item but found multiple items.");
        }

        return single;
    }

    /// <summary>Asserts that an untyped sequence contains exactly one item.</summary>
    /// <param name="values">The sequence to inspect.</param>
    public static void Single(IEnumerable values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var enumerator = values.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
            {
                Fail("Expected exactly one item but found none.");
            }

            if (enumerator.MoveNext())
            {
                Fail("Expected exactly one item but found multiple items.");
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    /// <summary>Asserts that a value has exactly the specified runtime type.</summary>
    /// <typeparam name="T">The expected runtime type.</typeparam>
    /// <param name="value">The value to verify.</param>
    /// <returns>The value cast to the expected type.</returns>
    public static T IsType<T>(object? value)
    {
        if (value is T typedValue && value.GetType() == typeof(T))
        {
            return typedValue;
        }

        Fail($"Expected type {typeof(T).FullName} but found {value?.GetType().FullName ?? "<null>"}.");
        return (T)value!;
    }

    /// <summary>Asserts that a typed sequence contains the expected value.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="values">The sequence to inspect.</param>
    public static void Contains<T>(T expected, IEnumerable<T> values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var comparer = EqualityComparer<T>.Default;
        foreach (var value in values)
        {
            if (comparer.Equals(expected, value))
            {
                return;
            }
        }

        Fail($"Expected collection to contain {Format(expected)}.");
    }

    /// <summary>Asserts that a typed sequence contains a matching value.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="values">The sequence to inspect.</param>
    /// <param name="predicate">The match predicate.</param>
    public static void Contains<T>(IEnumerable<T> values, Predicate<T> predicate)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        foreach (var value in values)
        {
            if (predicate(value))
            {
                return;
            }
        }

        Fail("Expected collection to contain a matching item.");
    }

    /// <summary>Asserts that a string contains the expected substring.</summary>
    /// <param name="expectedSubstring">The expected substring.</param>
    /// <param name="actualString">The string to inspect.</param>
    public static void Contains(string expectedSubstring, string actualString)
    {
        if (expectedSubstring is null)
        {
            throw new ArgumentNullException(nameof(expectedSubstring));
        }

        if (actualString?.Contains(expectedSubstring, StringComparison.Ordinal) == true)
        {
            return;
        }

        Fail($"Expected string to contain {Format(expectedSubstring)}.");
    }

    /// <summary>Asserts that a typed sequence does not contain the specified value.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="expected">The value that should not be present.</param>
    /// <param name="values">The sequence to inspect.</param>
    public static void DoesNotContain<T>(T expected, IEnumerable<T> values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var comparer = EqualityComparer<T>.Default;
        foreach (var value in values)
        {
            if (comparer.Equals(expected, value))
            {
                Fail($"Expected collection not to contain {Format(expected)}.");
            }
        }
    }

    /// <summary>Asserts that a value falls within an inclusive range.</summary>
    /// <typeparam name="T">The comparable value type.</typeparam>
    /// <param name="actual">The value to verify.</param>
    /// <param name="low">The inclusive lower bound.</param>
    /// <param name="high">The inclusive upper bound.</param>
    public static void InRange<T>(T actual, T low, T high)
        where T : IComparable<T>
    {
        if (actual.CompareTo(low) >= 0 && actual.CompareTo(high) <= 0)
        {
            return;
        }

        Fail($"Expected {Format(actual)} to be in range {Format(low)}..{Format(high)}.");
    }

    /// <summary>Applies an assertion to every item in a sequence.</summary>
    /// <typeparam name="T">The sequence item type.</typeparam>
    /// <param name="values">The sequence to inspect.</param>
    /// <param name="assertion">The assertion to apply.</param>
    public static void All<T>(IEnumerable<T> values, Action<T> assertion)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (assertion is null)
        {
            throw new ArgumentNullException(nameof(assertion));
        }

        foreach (var value in values)
        {
            assertion(value);
        }
    }

    /// <summary>Asserts that an action throws exactly the specified exception type.</summary>
    /// <typeparam name="T">The expected exception type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <returns>The thrown exception.</returns>
    public static T Throws<T>(Action action)
        where T : Exception =>
        TUnitAssertions.ThrowsExactly<T>(action);

    /// <summary>Asserts that an action throws the specified exception type or a derived type.</summary>
    /// <param name="exceptionType">The expected exception type.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>The thrown exception.</returns>
    public static Exception Throws(Type exceptionType, Action action) => TUnitAssertions.Throws(exceptionType, action);

    /// <summary>Asserts that an action throws the specified exception type or a derived type.</summary>
    /// <typeparam name="T">The expected exception type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <returns>The thrown exception.</returns>
    public static T ThrowsAny<T>(Action action)
        where T : Exception =>
        TUnitAssertions.Throws<T>(action);

    /// <summary>Asserts that an async action throws exactly the specified exception type.</summary>
    /// <typeparam name="T">The expected exception type.</typeparam>
    /// <param name="action">The async action to execute.</param>
    /// <returns>The thrown exception.</returns>
    public static async Task<T> ThrowsAsync<T>(Func<Task> action)
        where T : Exception
    {
        var exception = await TUnitAssertions.ThrowsExactlyAsync<T>(action);
        return exception!;
    }

    /// <summary>Compares two values with sequence-aware equality.</summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <returns>A value indicating whether the two values are equal.</returns>
    private static bool AreEqual(object? expected, object? actual)
    {
        if (expected is string || actual is string)
        {
            return Equals(expected, actual);
        }

        return expected is IEnumerable expectedValues && actual is IEnumerable actualValues
            ? AreSequencesEqual(expectedValues, actualValues)
            : Equals(expected, actual);
    }

    /// <summary>Compares two untyped sequences item by item.</summary>
    /// <param name="expectedValues">The expected sequence.</param>
    /// <param name="actualValues">The actual sequence.</param>
    /// <returns>A value indicating whether the sequences are equal.</returns>
    private static bool AreSequencesEqual(IEnumerable expectedValues, IEnumerable actualValues)
    {
        var expectedEnumerator = expectedValues.GetEnumerator();
        var actualEnumerator = actualValues.GetEnumerator();

        try
        {
            while (true)
            {
                var expectedHasValue = expectedEnumerator.MoveNext();
                var actualHasValue = actualEnumerator.MoveNext();

                if (!expectedHasValue || !actualHasValue)
                {
                    return expectedHasValue == actualHasValue;
                }

                if (!Equals(expectedEnumerator.Current, actualEnumerator.Current))
                {
                    return false;
                }
            }
        }
        finally
        {
            (expectedEnumerator as IDisposable)?.Dispose();
            (actualEnumerator as IDisposable)?.Dispose();
        }
    }

    /// <summary>Formats a value for assertion failure output.</summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted value.</returns>
    private static string Format(object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        if (value is string text)
        {
            return text;
        }

        return value is IEnumerable values ? FormatSequence(values) : value.ToString() ?? string.Empty;
    }

    /// <summary>Formats an untyped sequence for assertion failure output.</summary>
    /// <param name="values">The sequence to format.</param>
    /// <returns>The formatted sequence.</returns>
    private static string FormatSequence(IEnumerable values)
    {
        var builder = new StringBuilder("[");
        var needsSeparator = false;

        foreach (var value in values)
        {
            if (needsSeparator)
            {
                _ = builder.Append(", ");
            }

            _ = builder.Append(value);
            needsSeparator = true;
        }

        _ = builder.Append(']');
        return builder.ToString();
    }

    /// <summary>Fails the current TUnit test.</summary>
    /// <param name="message">The failure message.</param>
    private static void Fail(string message) => TUnitAssertions.Fail(message);
}
