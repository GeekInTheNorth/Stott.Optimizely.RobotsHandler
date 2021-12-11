using System;

namespace Stott.Optimizely.RobotsHandler.Exceptions
{
    public class RobotsInvalidSiteException : Exception
    {
        public RobotsInvalidSiteException(string message) : base(message)
        {
        }
    }
}
