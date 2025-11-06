using System;
using System.Collections.Generic;
using Stott.Optimizely.RobotsHandler.Content.Models;

namespace Stott.Optimizely.RobotsHandler.Content;

public interface IContentMarkdownMappingService
{
    ContentMarkdownSettingsDto GetSettings();

    IList<ContentMarkdownMappingSummaryDto> List();

    ContentMarkdownMappingDto Get(Guid id);
}