# SafeVault

SafeVault is a minimal secure web application built with ASP.NET Core (Razor Pages) and SQLite. It was developed in three incremental security activities.

## Activity 1 – Secure Coding Foundations
- Input validation using DataAnnotations (`Username` regex whitelist, `EmailAddress` attribute).
- SQL injection protection via parameterized Dapper queries (no string concatenation).
- Basic XSS mitigation relying on Razor automatic HTML encoding (no `Html.Raw` usage).
- Initial security tests covering SQL injection style inputs and XSS encoding.

## Activity 2 – Authentication & Authorization (RBAC)
- Cookie-based authentication with secure cookie settings (HttpOnly, Secure, SameSite=Lax).
- Password hashing using bcrypt (`BCrypt.Net-Next`).
- Role-based access control: `user` and `admin` with protected Admin Dashboard (`[Authorize(Roles="admin")]`).
- Pages: Login, Logout, Register, User Dashboard (profile + password change), Admin Dashboard (list all users).
- Integration tests for login, invalid credentials, RBAC denial, logout.

## Activity 3 – Vulnerability Debugging & Hardening
- Audited for remaining risks: LIKE-based wildcard abuse and potential XSS vectors.
- Implemented centralized input sanitization (`InputSanitizer.SanitizeForLike`) removing SQL LIKE wildcards (`%`, `_`) from user fragments.
- Added `ESCAPE '\\'` in LIKE query for robust pattern handling.
- Hardened admin seeding: only in Development or when `SeedAdmin=true`; optional password override via `SAFEVAULT_ADMIN_PASSWORD` env var.
- Added security headers: `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`.
- Added header tests (CSP presence) and sanitizer unit tests.

## Security Features Summary
| Threat | Mitigation |
|--------|-----------|
| SQL Injection | Strict parameterized queries, sanitized LIKE fragments |
| XSS (Reflected) | Razor encoding, no raw HTML, CSP header |
| Credential Theft | Bcrypt hashing, HttpOnly + Secure cookies |
| CSRF | Antiforgery tokens on form POSTs (Login, Logout, Register, password change) |
| Privilege Escalation | Role claim enforcement (`Authorize(Roles="admin")`) |
| Weak Defaults | Conditional admin seed + env var override |

## Project Structure
- `SafeVault.App` – Razor Pages UI & DI setup
- `SafeVault.Core` – Models, Services, Security utilities
- `SafeVault.Tests` – Unit & integration tests (`WebApplicationFactory` for integration)

## Requirements
- .NET 10 (preview). You can retarget to a stable version like `net8.0` if needed.

## Getting Started
```bash
# Restore & build
dotnet restore
dotnet build

# Run (Development)
dotnet run --project SafeVault.App
```
Navigate to `https://localhost:5001` (or the URL shown) for the Home page.

### Default Admin (Development Only)
- Username: `admin`
- Password: `ChangeMe!123` (override with `SAFEVAULT_ADMIN_PASSWORD`)
- Disable seeding by setting environment variable `SeedAdmin=false`.

## Running Tests
```bash
dotnet test
```
Included tests:
- Authentication & RBAC integration tests
- Credential validation unit tests
- Sanitizer (LIKE) tests
- CSP header integration tests
- XSS / SQL injection resistance scenarios

## Password Policy
Enforced (length >= 8, contains digit & uppercase). Logic resides in Register and User Dashboard PageModels.

## Future Improvements
- Account lockout on repeated failed logins.
- Session expiration / sliding expiration.
- Audit logging for security events (role change, password update).
- Secret management (e.g., Azure Key Vault for admin seed password).
- Stronger CSP (nonces, SRI).
- 2FA / MFA integration.

## Contributing
Open issues or pull requests for enhancements and additional security test scenarios.

## License
MIT (add a LICENSE file if desired).

---
Demonstrates progressive hardening using secure coding practices, authentication & authorization, and targeted vulnerability debugging supported by AI-assisted development.