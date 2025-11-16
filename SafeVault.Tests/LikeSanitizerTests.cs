using SafeVault.Core.Security;
using Xunit;

namespace SafeVault.Tests;

public class LikeSanitizerTests
{
    [Theory]
    [InlineData("abc%def", "abcdef")] // % removido
    [InlineData("abc_def", "abcdef")] // _ removido
    [InlineData("<script>", "script")] // stripped
    [InlineData("a+b", "a+b")] // plus allowed
    public void SanitizeForLike_RemovesWildcards_AndWhitelists(string input, string expected)
    {
        var sanitized = InputSanitizer.SanitizeForLike(input);
        Assert.Equal(expected, sanitized);
    }
}
