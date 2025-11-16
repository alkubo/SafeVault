using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SafeVault.Tests;

public class AuthIntegrationTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        _client.BaseAddress = new Uri("https://localhost");
    }

    private static string ExtractRequestVerificationToken(string html)
    {
        var match = Regex.Match(html, @"name=""__RequestVerificationToken""\s+type=""hidden""\s+value=""([^""]+)""");
        Assert.True(match.Success, "Antiforgery token not found in HTML.");
        return match.Groups[1].Value;
    }

    private static string GetPath(Uri? location)
    {
        Assert.NotNull(location);
        return location!.IsAbsoluteUri ? location.AbsolutePath : location.OriginalString;
    }

    [Fact]
    public async Task ProtectedPageRedirectsToLogin_WhenAnonymous()
    {
        var resp = await _client.GetAsync("/Dashboard/User");
        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        Assert.Equal("/Account/Login", GetPath(resp.Headers.Location));
    }

    [Fact]
    public async Task AdminLogin_LandsOnAdminDashboard()
    {
        var getLogin = await _client.GetAsync("/Account/Login");
        var html = await getLogin.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(html);

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Username"] = "admin",
            ["Password"] = "ChangeMe!123"
        };
        var post = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(formData));
        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);
        Assert.Equal("/Admin/Dashboard", GetPath(post.Headers.Location));

        var follow = await _client.GetAsync("/Admin/Dashboard");
        Assert.Equal(HttpStatusCode.OK, follow.StatusCode);
    }

    [Fact]
    public async Task WrongCredentialsStayOnLogin()
    {
        var getLogin = await _client.GetAsync("/Account/Login");
        var htmlGet = await getLogin.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(htmlGet);

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Username"] = "admin",
            ["Password"] = "wrongpass"
        };
        var post = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(formData));
        // No redirect when invalid, stays on page
        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var html = await post.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials.", html);
    }

    [Fact]
    public async Task UserRole_CannotAccess_AdminDashboard()
    {
        // Register a normal user
        var getRegister = await _client.GetAsync("/Account/Register");
        var regHtml = await getRegister.Content.ReadAsStringAsync();
        var regToken = ExtractRequestVerificationToken(regHtml);
        var uname = $"user{Guid.NewGuid():N}";
        var regForm = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = regToken,
            ["Form.Username"] = uname,
            ["Form.Email"] = $"{uname}@test.com",
            ["Password"] = "P4sswordA",
            ["ConfirmPassword"] = "P4sswordA"
        };
        var regPost = await _client.PostAsync("/Account/Register", new FormUrlEncodedContent(regForm));
        Assert.Equal(HttpStatusCode.OK, regPost.StatusCode);

        // Login as the new user
        var loginGet = await _client.GetAsync("/Account/Login");
        var loginHtml = await loginGet.Content.ReadAsStringAsync();
        var loginToken = ExtractRequestVerificationToken(loginHtml);
        var loginForm = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = loginToken,
            ["Username"] = uname,
            ["Password"] = "P4sswordA"
        };
        var loginPost = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginForm));
        Assert.Equal(HttpStatusCode.Redirect, loginPost.StatusCode);
        Assert.Equal("/Dashboard/User", GetPath(loginPost.Headers.Location));

        // Try accessing admin dashboard
        var adminResp = await _client.GetAsync("/Admin/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect, adminResp.StatusCode);
        Assert.Equal("/Account/AccessDenied", GetPath(adminResp.Headers.Location));
    }

    [Fact]
    public async Task Logout_ClearsSession_AndRedirectsHome()
    {
        // Login admin first
        var getLogin = await _client.GetAsync("/Account/Login");
        var html = await getLogin.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(html);
        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Username"] = "admin",
            ["Password"] = "ChangeMe!123"
        };
        var post = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(formData));
        Assert.Equal(HttpStatusCode.Redirect, post.StatusCode);

        // Get logout page and post
        var logoutGet = await _client.GetAsync("/Account/Logout");
        var logoutHtml = await logoutGet.Content.ReadAsStringAsync();
        var logoutToken = ExtractRequestVerificationToken(logoutHtml);
        var logoutPost = await _client.PostAsync("/Account/Logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = logoutToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, logoutPost.StatusCode);
        Assert.Equal("/", GetPath(logoutPost.Headers.Location));

        // Now accessing a protected page should redirect to login
        var resp = await _client.GetAsync("/Admin/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        Assert.Equal("/Account/Login", GetPath(resp.Headers.Location));
    }
}
