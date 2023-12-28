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

        internal async Task<UnityWebRequest> SendRequest(UnityWebRequest webRequest)
        {
            _logger.Debug($"{webRequest.method} {webRequest.url}");

            UnityWebRequestAsyncOperation result = await webRequest.SendWebRequest();

            if (!result.isDone || webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new WebRequestException();
            }

            if (!webRequest.IsSuccessResponseCode())
            {
                throw new WebRequestException(webRequest.responseCode, webRequest.error, webRequest.downloadHandler.data);
            }

            return result.webRequest;
        }
    }
}
