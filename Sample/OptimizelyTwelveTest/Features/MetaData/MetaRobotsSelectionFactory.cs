using EPiServer.Shell.ObjectEditing;
using System.Collections.Generic;

namespace OptimizelyTwelveTest.Features.MetaData;

public class MetaRobotsSelectionFactory : ISelectionFactory
{
    public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
    {
        return new List<SelectItem>
        {
            new()
            {
                Text = "No Follow - Instruct the Crawler NOT to follow the links in this page.",
                Value = "nofollow"
            },
            new()
            {
                Text = "No Index - Instruct the Crawler NOT to index on this page.",
                Value = "noindex"
            },
            new()
            {
                Text = "No Image Index - Instruct the Crawler NOT to index images on this page.",
                Value = "noimageindex"
            },
            new()
            {
                Text = "No Archive - Instruct the Search Engine NOT to show a cached link in search results.",
                Value = "noarchive"
            },
            new()
            {
                Text = "No Snippet - Instruct the Search Engine NOT to show a text snippet or video preview in the search results.",
                Value = "nosnippet"
            },
            new()
            {
                Text = "No Translate - Instruct the Search Engine NOT to offer a translation of this page in search results.",
                Value = "notranslate"
            }
        };
    }
}
