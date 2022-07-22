// <copyright file="DiscriminatedUnionFixture.cs" company="Chris Pulman">
// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using ModbusRx.Utility;
using Xunit;

namespace ModbusRx.UnitTests.Utility;

/// <summary>
/// DiscriminatedUnionFixture.
/// </summary>
public class DiscriminatedUnionFixture
{
    /// <summary>
    /// Discriminateds the union create a.
    /// </summary>
    [Fact]
    public void DiscriminatedUnion_CreateA()
    {
        var du = DiscriminatedUnion<string, string>.CreateA("foo");
        Assert.Equal(DiscriminatedUnionOption.A, du.Option);
        Assert.Equal("foo", du.A);
    }

    /// <summary>
    /// Discriminateds the union create b.
    /// </summary>
    [Fact]
    public void DiscriminatedUnion_CreateB()
    {
        var du = DiscriminatedUnion<string, string>.CreateB("foo");
        Assert.Equal(DiscriminatedUnionOption.B, du.Option);
        Assert.Equal("foo", du.B);
    }

    /// <summary>
    /// Discriminateds the union allow nulls.
    /// </summary>
    [Fact]
    public void DiscriminatedUnion_AllowNulls()
    {
        var du = DiscriminatedUnion<object, object>.CreateB(null!);
        Assert.Equal(DiscriminatedUnionOption.B, du.Option);
        Assert.Null(du.B);
    }

    /// <summary>
    /// Accesses the invalid option a.
    /// </summary>
    [Fact]
    public void AccessInvalidOption_A()
    {
        var du = DiscriminatedUnion<string, string>.CreateB("foo");
        Assert.Throws<InvalidOperationException>(() => du.A?.ToString());
    }

    /// <summary>
    /// Accesses the invalid option b.
    /// </summary>
    [Fact]
    public void AccessInvalidOption_B()
    {
        var du = DiscriminatedUnion<string, string>.CreateA("foo");
        Assert.Throws<InvalidOperationException>(() => du.B?.ToString());
    }

    /// <summary>
    /// Discriminateds the union to string.
    /// </summary>
    [Fact]
    public void DiscriminatedUnion_ToString()
    {
        var du = DiscriminatedUnion<string, string>.CreateA("foo");
        Assert.Equal("foo", du.ToString());
    }
}
