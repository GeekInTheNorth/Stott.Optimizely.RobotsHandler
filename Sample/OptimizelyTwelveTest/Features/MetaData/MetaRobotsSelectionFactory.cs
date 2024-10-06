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
                Text = "No Follow - Instruct search engines not to follow links on a page.",
                Value = "nofollow"
            },
            new()
            {
                Text = "No Index - Instruct search engines not to index a page.",
                Value = "noindex"
            },
            new()
            {
                Text = "No Image Index - Instruct search engines not to index images on a page.",
                Value = "noimageindex"
            },
            new()
            {
                Text = "No Archive - Instruct search engines not to show a cached link in search results.",
                Value = "noarchive"
            },
            new()
            {
                Text = "No Snippet - Instruct search engines not to show a text snippet or video preview in the search results.",
                Value = "nosnippet"
            },
            new()
            {
                Text = "No Translate - Instruct search engines not to offer a translation of a page in search results.",
                Value = "notranslate"
            }
        };
    }
}
