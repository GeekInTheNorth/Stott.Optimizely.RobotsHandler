namespace Stott.Optimizely.RobotsHandler.Extensions;

using System;

internal static class StringExtensions
{
    internal static string? GetSanitizedHostDomain(this string? hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return null;
        }

        var sanitized = hostName.TrimEnd('/');
        var normalized = sanitized.Contains("://") ? sanitized : $"https://{sanitized}";
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            return uri.Host + (uri.IsDefaultPort ? string.Empty : ":" + uri.Port);
        }

        return sanitized;
    }
}
