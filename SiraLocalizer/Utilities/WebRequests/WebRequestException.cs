using System;
using System.Text;

#pragma warning disable IDE1006

namespace SiraLocalizer.Utilities.WebRequests
{
    public class WebRequestException : Exception
    {
        internal WebRequestException()
            : base($"Failed to send web request")
        {
        }

        internal WebRequestException(long responseCode, string error, byte[] body)
            : base($"Failed to send web request; got response code: {error}")
        {
            ResponseCode = responseCode;
            Error = error;
            Body = body;
        }

        public long ResponseCode { get; }

        public string Error { get; }

        public byte[] Body { get; }

#if DEBUG
        public override string Message => base.Message + "\n" + Encoding.UTF8.GetString(Body);
#endif
    }
}
