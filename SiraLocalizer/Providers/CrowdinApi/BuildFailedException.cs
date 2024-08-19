using SiraLocalizer.Providers.CrowdinApi.Models;

namespace SiraLocalizer.Providers.CrowdinApi
{
    /// <summary>
    /// Error thrown when a Crowdin project build has failed.
    /// </summary>
    public class BuildFailedException : CrowdinApiException
    {
        internal BuildFailedException(ProjectBuildStatus status)
            : base($"Build status: {status}")
        {
        }
    }
}
