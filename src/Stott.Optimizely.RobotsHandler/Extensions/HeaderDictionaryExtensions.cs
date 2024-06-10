using System;
using System.Linq;

using Microsoft.AspNetCore.Http;

namespace Stott.Optimizely.RobotsHandler.Extensions;

internal static class HeaderDictionaryExtensions
{
    public static void AddOrUpdateHeader(this IHeaderDictionary headers, string headerName, string headerValue)
    {
        if (headers is null || string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue))
        {
            return;
        }

        try
        {
            if (headers.ContainsKey(headerName))
            {
                headers[headerName] = headerValue;
            }
            else
            {
                headers.Append(headerName, headerValue);
            }
        }
        catch (Exception)
        {
            // Ignored on purpose.
        }
    }
}