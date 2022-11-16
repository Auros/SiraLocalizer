using UnityEngine.Networking;

namespace SiraLocalizer.Utilities
{
    internal static class UnityWebRequestExtensions
    {
        public static bool IsSuccessResponseCode(this UnityWebRequest webRequest)
        {
            return webRequest.responseCode is >= 200 and < 300;
        }
    }
}
