using SafeVault.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;

namespace SafeVault.Tests;

public class XssValidationTests
{
    [Fact]
    public void UsernameWithScriptTags_IsInvalidByDataAnnotations()
    {
        var model = new UserInput
        {
            Username = "<script>alert(1)</script>",
            Email = "user@example.com"
        };

        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UserInput.Username)));
    }

    [Fact]
    public void RazorLikeHtmlEncoding_EscapesScriptTags()
    {
        var payload = "<script>alert(1)</script>";
        var encoded = HtmlEncoder.Default.Encode(payload);

        Assert.DoesNotContain("<script>", encoded);
        Assert.Contains("&lt;script&gt;", encoded);
    }
}
