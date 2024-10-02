using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Stott.Optimizely.RobotsHandler.Environments;

[HtmlTargetElement("meta", Attributes = "name")]
public sealed class MetaRobotsTagHelper : TagHelper
{
    private readonly IEnvironmentRobotsService _environmentRobotsService;

    public override int Order => -9999;

    public MetaRobotsTagHelper(IEnvironmentRobotsService environmentRobotsService)
    {
        _environmentRobotsService = environmentRobotsService;
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var metaName = GetMetaName(context);
        if (string.Equals(metaName, "robots", System.StringComparison.OrdinalIgnoreCase))
        {
            var environmentContent = _environmentRobotsService.GetCurrent();
            output.Attributes.SetAttribute("content", environmentContent?.ToMetaContent() ?? string.Empty);
        }

        return base.ProcessAsync(context, output);
    }

    private static string GetMetaName(TagHelperContext context)
    {
        if (context.AllAttributes.TryGetAttribute("name", out var nameAttribute))
        {
            return nameAttribute.Value?.ToString();
        }

        return null;
    }
}
