// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable SA1201, SA1204, SA1402, SA1649

namespace Xunit;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
internal sealed class CollectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class CollectionDefinitionAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public bool DisableParallelization { get; set; }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
internal sealed class CollectionBehaviorAttribute(CollectionBehavior collectionBehavior) : Attribute
{
    public CollectionBehavior CollectionBehavior { get; } = collectionBehavior;

    public bool DisableTestParallelization { get; set; }

    public int MaxParallelThreads { get; set; }
}

internal enum CollectionBehavior
{
    CollectionPerAssembly,
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
internal sealed class TraitAttribute(string name, string value) : Attribute
{
    public string Name { get; } = name;

    public string Value { get; } = value;
}

internal static class Skip
{
    public static void If(bool condition, string reason)
    {
        if (condition)
        {
            TUnit.Core.Skip.Test(reason);
        }
    }

    public static void IfNot(bool condition, string reason)
    {
        if (!condition)
        {
            TUnit.Core.Skip.Test(reason);
        }
    }
}

internal static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            Fail(message ?? "Expected true.");
        }
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            Fail(message ?? "Expected false.");
        }
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (AreEqual(expected, actual))
        {
            return;
        }

        Fail($"Expected: {Format(expected)}{Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void Equal(object? expected, object? actual)
    {
        if (AreEqual(expected, actual))
        {
            return;
        }

        Fail($"Expected: {Format(expected)}{Environment.NewLine}Actual:   {Format(actual)}");
    }

    public static void NotEqual<T>(T notExpected, T actual)
    {
        if (!AreEqual(notExpected, actual))
        {
            return;
        }

        Fail($"Did not expect: {Format(actual)}");
    }

    public static void Same(object? expected, object? actual)
    {
        if (!ReferenceEquals(expected, actual))
        {
            Fail("Expected both references to point to the same object.");
        }
    }

    public static T NotNull<T>(T? value)
        where T : class
    {
        if (value is null)
        {
            Fail("Expected a non-null value.");
        }

        return value!;
    }

    public static void NotNull<T>(T? value)
        where T : struct
    {
        if (!value.HasValue)
        {
            Fail("Expected a non-null value.");
        }
    }

    public static void Null(object? value)
    {
        if (value is not null)
        {
            Fail($"Expected null but found {Format(value)}.");
        }
    }

    public static void Empty(IEnumerable values)
    {
        if (values.Cast<object?>().Any())
        {
            Fail("Expected an empty collection.");
        }
    }

    public static void NotEmpty(IEnumerable values)
    {
        if (!values.Cast<object?>().Any())
        {
            Fail("Expected a non-empty collection.");
        }
    }

    public static T Single<T>(IEnumerable<T> values)
    {
        var list = values.Take(2).ToArray();
        if (list.Length != 1)
        {
            Fail($"Expected a single item but found {list.Length}.");
        }

        return list[0];
    }

    public static void Single(IEnumerable values)
    {
        var count = values.Cast<object?>().Take(2).Count();
        if (count != 1)
        {
            Fail($"Expected a single item but found {count}.");
        }
    }

    public static T IsType<T>(object? value)
    {
        if (value is T typed && value.GetType() == typeof(T))
        {
            return typed;
        }

        Fail($"Expected type {typeof(T).FullName} but found {value?.GetType().FullName ?? "<null>"}.");
        return default!;
    }

    public static void Contains<T>(T expected, IEnumerable<T> values)
    {
        if (!values.Contains(expected))
        {
            Fail($"Expected collection to contain {Format(expected)}.");
        }
    }

    public static void Contains<T>(IEnumerable<T> values, Predicate<T> predicate)
    {
        if (!values.Any(value => predicate(value)))
        {
            Fail("Expected collection to contain a matching item.");
        }
    }

    public static void Contains(string expectedSubstring, string actualString)
    {
        if (!actualString.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            Fail($"Expected string to contain '{expectedSubstring}'.");
        }
    }

    public static void DoesNotContain<T>(T expected, IEnumerable<T> values)
    {
        if (values.Contains(expected))
        {
            Fail($"Expected collection not to contain {Format(expected)}.");
        }
    }

    public static void InRange<T>(T actual, T low, T high)
        where T : IComparable<T>
    {
        if (actual.CompareTo(low) < 0 || actual.CompareTo(high) > 0)
        {
            Fail($"Expected {Format(actual)} to be in range {Format(low)}..{Format(high)}.");
        }
    }

    public static void All<T>(IEnumerable<T> values, Action<T> assertion)
    {
        foreach (var value in values)
        {
            assertion(value);
        }
    }

    public static T Throws<T>(Action action)
        where T : Exception
    {
        try
        {
            action();
        }
        catch (T exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            Fail($"Expected exception {typeof(T).FullName} but found {exception.GetType().FullName}.");
            throw;
        }

        Fail($"Expected exception {typeof(T).FullName}.");
        return null!;
    }

    public static Exception Throws(Type exceptionType, Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            if (exception.GetType() == exceptionType)
            {
                return exception;
            }

            Fail($"Expected exception {exceptionType.FullName} but found {exception.GetType().FullName}.");
            throw;
        }

        Fail($"Expected exception {exceptionType.FullName}.");
        return null!;
    }

    public static T ThrowsAny<T>(Action action)
        where T : Exception
    {
        try
        {
            action();
        }
        catch (T exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            Fail($"Expected exception assignable to {typeof(T).FullName} but found {exception.GetType().FullName}.");
            throw;
        }

        Fail($"Expected exception assignable to {typeof(T).FullName}.");
        return null!;
    }

    public static async Task<T> ThrowsAsync<T>(Func<Task> action)
        where T : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (T exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            Fail($"Expected exception {typeof(T).FullName} but found {exception.GetType().FullName}.");
            throw;
        }

        Fail($"Expected exception {typeof(T).FullName}.");
        return null!;
    }

    private static bool AreEqual<T>(T expected, T actual)
    {
        if (expected is string || actual is string)
        {
            return EqualityComparer<T>.Default.Equals(expected, actual);
        }

        if (expected is IEnumerable expectedValues && actual is IEnumerable actualValues)
        {
            return expectedValues.Cast<object?>().SequenceEqual(actualValues.Cast<object?>());
        }

        return EqualityComparer<T>.Default.Equals(expected, actual);
    }

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

        if (value is IEnumerable values)
        {
            return "[" + string.Join(", ", values.Cast<object?>()) + "]";
        }

        return value.ToString() ?? string.Empty;
    }

    private static void Fail(string message) => throw new InvalidOperationException(message);
}
