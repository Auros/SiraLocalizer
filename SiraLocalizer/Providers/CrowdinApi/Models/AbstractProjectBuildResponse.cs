using System;

namespace SiraLocalizer.Providers.CrowdinApi.Models
{
    internal class AbstractProjectBuildResponse
    {
        public long id { get; set; }

        public long projectId { get; set; }

        public ProjectBuildStatus status { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? finishedAt { get; set; }
    }
}
