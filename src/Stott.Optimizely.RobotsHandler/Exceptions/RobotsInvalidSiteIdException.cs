using System;

namespace Stott.Optimizely.RobotsHandler.Exceptions
{
    public class RobotsInvalidSiteIdException : Exception
    {
        public RobotsInvalidSiteIdException(Guid siteId) : base($"'{siteId}' is not a valid siteId.")
        {
        }
    }
}
