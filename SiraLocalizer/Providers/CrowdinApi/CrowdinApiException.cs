using System;

namespace SiraLocalizer.Providers.CrowdinApi
{
    /// <summary>
    /// Base class for Crowdin API errors.
    /// </summary>
    public class CrowdinApiException : Exception
    {
        internal CrowdinApiException(string message)
            : base(message)
        {
        }
    }
}
