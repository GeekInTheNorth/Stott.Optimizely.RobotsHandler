using System;
using System.Collections.Generic;

using Stott.Optimizely.RobotsHandler.Applications;
using Stott.Optimizely.RobotsHandler.Common;

namespace Stott.Optimizely.RobotsHandler.Llms
{
    public sealed class ApplicationLlmsViewModel : IApplicationContentViewModel
    {
        public Guid Id { get; set; }

        public string? AppId { get; set; }

        public string? AppName { get; set; }

        public List<HostViewModel> AvailableHosts { get; set; } = [];

        public bool IsForWholeSite { get; set; }

        public string? SpecificHost { get; set; }

        public string? LlmsContent { get; set; }
    }
}
