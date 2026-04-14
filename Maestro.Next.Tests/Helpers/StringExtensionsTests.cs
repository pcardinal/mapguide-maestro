// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next;
using Xunit;

namespace Maestro.Next.Tests.Helpers;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("Hello", 10, "Hello")]
    [InlineData("Hello World", 5, "Hello…")]
    [InlineData("", 5, "")]
    [InlineData(null, 5, null)]
    public void Truncate_Works(string? input, int maxLen, string? expected)
    {
        Assert.Equal(expected, input?.Truncate(maxLen));
    }

    [Fact]
    public void Truncate_ExactLength_NoEllipsis()
    {
        Assert.Equal("12345", "12345".Truncate(5));
    }

    [Fact]
    public void Truncate_OneOver_HasEllipsis()
    {
        Assert.Equal("12345…", "123456".Truncate(5));
    }
}
