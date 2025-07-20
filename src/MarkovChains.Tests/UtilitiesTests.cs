using Xunit;
using MarkovChains;
using System.IO;

namespace MarkovChains.Tests;

public class UtilitiesTests
{
    [Theory]
    [InlineData("hello world", new[] { "hello", "world" })]
    [InlineData("hello, world!", new[] { "hello", "world" })]
    [InlineData("foo_bar baz", new[] { "foo_bar", "baz" })]
    [InlineData("  leading and trailing  ", new[] { "leading", "and", "trailing" })]
    [InlineData("multiple   spaces", new[] { "multiple", "spaces" })]
    [InlineData("punctuation: test.", new[] { "punctuation", "test" })]
    [InlineData("123 456", new[] { "123", "456" })]
    [InlineData("a_b_c d_e_f", new[] { "a_b_c", "d_e_f" })]
    [InlineData("", new string[0])]
    [InlineData("!@#$%^&*()", new string[0])]
    public void CleanAndSplit_ReturnsExpectedWords(string input, string[] expected)
    {
        var method = typeof(Utilities)
            .GetMethod("CleanAndSplit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

         if (method == null)
             throw new InvalidOperationException("CleanAndSplit method not found.");

        var result = method.Invoke(null, new object[] { input }) as string[];
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("hello world", new[] { "hello", "world" })]
    [InlineData("hello, world!", new[] { "hello", "world" })]
    [InlineData("foo_bar baz", new[] { "foo_bar", "baz" })]
    [InlineData("  leading and trailing  ", new[] { "leading", "and", "trailing" })]
    [InlineData("multiple   spaces", new[] { "multiple", "spaces" })]
    [InlineData("punctuation: test.", new[] { "punctuation", "test" })]
    [InlineData("123 456", new[] { "123", "456" })]
    [InlineData("a_b_c d_e_f", new[] { "a_b_c", "d_e_f" })]
    [InlineData("", new string[0])]
    [InlineData("!@#$%^&*()", new string[0])]
    public void CleanAndSplitToList_ReturnsExpectedWords(string input, string[] expected)
    {
        var method = typeof(Utilities)
            .GetMethod("CleanAndSplit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException("CleanAndSplit method not found.");

        var result = method.Invoke(null, new object[] { input }) as string[];
        Assert.Equal(expected, result);
    }
}