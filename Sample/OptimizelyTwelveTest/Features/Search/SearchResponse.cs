namespace OptimizelyTwelveTest.Features.Search;

using System.Collections.Generic;

public sealed class SearchResponse
{
    public int TotalRecords { get; set; }

    public IList<SearchResultItem> Results { get; set; }
}