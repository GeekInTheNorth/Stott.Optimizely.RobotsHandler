using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Core;
using EPiServer.DataAbstraction;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Content;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = RobotsConstants.AuthorizationPolicy)]
public sealed class ContentMarkdownMappingController : BaseApiController
{
    private readonly IContentMarkdownMappingService _service;

    public ContentMarkdownMappingController(IContentMarkdownMappingService service)
    {
        _service = service;
    }

    [HttpGet]
    [Route("/stott.robotshandler/api/markdown/[action]")]
    public IActionResult List()
    {
        var model = _service.List();

        return CreateSafeJsonResult(model);
    }
}

public interface IContentMarkdownMappingService
{
    IList<ContentMarkdownMappingSummaryDto> List();

    IList<ContentMarkdownMappingDto> Get(Guid id);
}

public sealed class ContentMarkdownMappingService : IContentMarkdownMappingService
{
    private readonly IContentTypeRepository _repository;

    public ContentMarkdownMappingService(IContentTypeRepository repository)
    {
        _repository = repository;
    }

    public IList<ContentMarkdownMappingDto> Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public IList<ContentMarkdownMappingSummaryDto> List()
    {
        var contentTypes = _repository.List() ?? Enumerable.Empty<ContentType>();
        var first = contentTypes.FirstOrDefault();

        return contentTypes
            .Where(x => x is { PropertyDefinitions.Count: > 0 } && (x.Base == ContentTypeBase.Page || x.Base == ContentTypeBase.Block))
            .Select(x => new ContentMarkdownMappingSummaryDto
            {
                Id = x.GUID,
                DisplayName = x.DisplayName,
                Description = x.Description,
                ContentName = x.Name,
                ContentType = x.Base.ToString(),
                IsEnabled = false,
                IsConfigured = false
            })
            .OrderByDescending(x => x.ContentType)
            .ThenBy(x => x.DisplayName)
            .ToList();
    }
}

public sealed class ContentMarkdownMappingDto
{

}

public sealed class ContentMarkdownMappingSummaryDto
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public string ContentName { get; set; }

    public string ContentType { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsConfigured { get; set; }
}