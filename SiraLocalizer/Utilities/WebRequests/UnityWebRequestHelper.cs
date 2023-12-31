using System.Threading.Tasks;
using SiraUtil.Logging;
using UnityEngine.Networking;

namespace SiraLocalizer.Utilities.WebRequests
{
    internal class UnityWebRequestHelper
    {
        private readonly SiraLog _logger;

        internal UnityWebRequestHelper(SiraLog logger)
        {
            _logger = logger;
        }

        internal async Task SendRequest(UnityWebRequest webRequest)
        {
            _logger.Debug($"{webRequest.method} {webRequest.url}");

            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                throw new WebRequestException(webRequest.responseCode, webRequest.error, webRequest.downloadHandler?.data);
            }
            else if (webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new WebRequestException(webRequest.result);
            }
        }
    }
}
