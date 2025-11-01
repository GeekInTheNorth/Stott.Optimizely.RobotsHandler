using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using Stott.Optimizely.RobotsHandler.Content.Models;

namespace Stott.Optimizely.RobotsHandler.Content;

public sealed class ContentMarkdownMappingService : IContentMarkdownMappingService
{
    private readonly IContentTypeRepository _repository;

    public ContentMarkdownMappingService(IContentTypeRepository repository)
    {
        _repository = repository;
    }

    public ContentMarkdownMappingDto Get(Guid id)
    {
        var contentTypes = _repository.List() ?? Enumerable.Empty<ContentType>();
        var first = contentTypes.Where(x => x.GUID == id).FirstOrDefault();

        return new ContentMarkdownMappingDto
        {
            Id = first.GUID,
            DisplayName = string.IsNullOrWhiteSpace(first.DisplayName) ? first.Name : first.DisplayName,
            Description = first.Description,
            ContentName = first.Name,
            ContentType = first.Base.ToString(),
            IsEnabled = false,
            IsConfigured = false,
            Properties = first.PropertyDefinitions.Select(p => new PropertyMarkdownMappingDto
            {
                PropertyName = p.Name,
                PropertyType = p.Type.ToString(),
                IsMapped = false
            }).ToList()
        };
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
                DisplayName = string.IsNullOrWhiteSpace(x.DisplayName) ? x.Name : x.DisplayName,
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