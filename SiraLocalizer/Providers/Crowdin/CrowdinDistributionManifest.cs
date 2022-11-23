namespace SiraLocalizer.Providers.Crowdin
{
    internal record CrowdinDistributionManifest
    {
        public string[] files { get; set; }

        public string[] languages { get; set; }

        public long timestamp { get; set; }
    }
}
