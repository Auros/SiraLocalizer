using SiraLocalizer.Providers.CrowdinApi.Models;

namespace SiraLocalizer.Providers.CrowdinApi
{
    public class BuildFailedException : CrowdinApiException
    {
        internal BuildFailedException(ProjectBuildStatus status)
            : base($"Build status: {status}")
        {
        }
    }
}
