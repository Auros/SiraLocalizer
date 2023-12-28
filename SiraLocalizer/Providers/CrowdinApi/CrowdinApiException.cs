using System;

namespace SiraLocalizer.Providers.CrowdinApi
{
    public class CrowdinApiException : Exception
    {
        internal CrowdinApiException(string message)
            : base(message)
        {
        }
    }
}
