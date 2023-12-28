using System;

namespace SiraLocalizer.Providers.CrowdinApi.Models
{
    internal class DownloadLinkResponse
    {
        public string url { get; set; }

        public DateTime expireIn { get; set; }
    }
}
