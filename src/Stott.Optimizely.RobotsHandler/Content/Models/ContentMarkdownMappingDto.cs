using System.Collections.Generic;

namespace Stott.Optimizely.RobotsHandler.Content.Models;

public sealed class ContentMarkdownMappingDto : ContentMarkdownMappingBase
{
    public IList<PropertyMarkdownMappingDto> Properties { get; set; }
}
