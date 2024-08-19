using System;
using UnityEngine.Networking;

#pragma warning disable IDE1006

namespace SiraLocalizer.Utilities.WebRequests
{
    /// <summary>
    /// Exception thrown when a web request fails.
    /// </summary>
    public class WebRequestException : Exception
    {
        internal WebRequestException(UnityWebRequest.Result result)
            : base($"Failed to send web request: {result}")
        {
        }

        internal WebRequestException(long responseCode, string error, byte[] body)
            : base($"Failed to send web request; got response code: {error}")
        {
            ResponseCode = responseCode;
            Error = error;
            Body = body;
        }

        /// <summary>
        /// Gets the HTTP response code.
        /// </summary>
        public long ResponseCode { get; }

        /// <summary>
        /// Gets the HTTP response error text.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Gets the body of the response.
        /// </summary>
        public byte[] Body { get; }

#if DEBUG
        /// <inheritdoc />
        public override string Message => base.Message + "\n" + System.Text.Encoding.UTF8.GetString(Body ?? Array.Empty<byte>());
#endif
    }
}
