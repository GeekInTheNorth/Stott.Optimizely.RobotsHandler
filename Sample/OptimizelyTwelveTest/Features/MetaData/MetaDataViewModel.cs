using System.Collections.Generic;

namespace OptimizelyTwelveTest.Features.MetaData;

public class MetaDataViewModel
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string Image { get; set; }

    public string Robots { get; set; }

    public Dictionary<string, string> MetaTags { get; set; }
}