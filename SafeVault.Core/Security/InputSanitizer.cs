using System.Text;

namespace SafeVault.Core.Security;

public static class InputSanitizer
{
    // Sanitizes a fragment to be safely used inside a SQL LIKE pattern.
    // - Whitelists common email characters
    // - Escapes % and _ with backslash so they are treated as literals
    public static string SanitizeForLike(string fragment)
    {
        if (string.IsNullOrEmpty(fragment)) return string.Empty;
        var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@._-+";
        var sb = new StringBuilder(fragment.Length);
        foreach (var c in fragment)
        {
            if (!allowed.Contains(c))
                continue;
            // Remover curingas do usuário; como o padrão final usa %...%, não precisamos manter
            if (c == '%' || c == '_')
                continue;
            sb.Append(c);
        }
        return sb.ToString();
    }
}
