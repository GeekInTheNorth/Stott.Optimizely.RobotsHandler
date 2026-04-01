using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Stott.Optimizely.RobotsHandler.Environments;

[HtmlTargetElement("meta", Attributes = "name")]
public sealed class MetaRobotsTagHelper(IEnvironmentRobotsService environmentRobotsService) : TagHelper
{
    public override int Order => -9999;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var metaName = GetMetaName(context);
        if (string.Equals(metaName, "robots", System.StringComparison.OrdinalIgnoreCase))
        {
            var environmentContent = environmentRobotsService.GetCurrent();
            var existingContent = GetMetaContent(context);

            if (environmentContent is { IsEnabled: true })
            {
                output.Attributes.SetAttribute("content", environmentContent.ToMetaContent());
            }
            else if (string.IsNullOrWhiteSpace(existingContent))
            {
                output.TagName = null;
            }
        }

        return base.ProcessAsync(context, output);
    }

    private static string? GetMetaName(TagHelperContext context) => GetAttributeValue(context, "name");

    private static string? GetMetaContent(TagHelperContext context) => GetAttributeValue(context, "content");

    private static string? GetAttributeValue(TagHelperContext context, string attributeName)
    {
        if (context.AllAttributes.TryGetAttribute(attributeName, out var attribute))
        {
            return attribute.Value?.ToString();
        }

        return null;
    }
}
