using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

namespace Stott.Optimizely.RobotsHandler.Presentation;

public abstract class BaseApiController : Controller
{
    protected static IActionResult CreateSafeJsonResult<T>(T objectToSerialize)
    {
        var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var content = JsonSerializer.Serialize(objectToSerialize, serializationOptions);

        return new ContentResult
        {
            StatusCode = (int)HttpStatusCode.OK,
            ContentType = "application/json",
            Content = content
        };
    }
}
