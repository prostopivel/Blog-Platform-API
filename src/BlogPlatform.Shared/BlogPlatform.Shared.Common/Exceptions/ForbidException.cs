using System.Net;

namespace BlogPlatform.Shared.Common.Exceptions
{
    public class ForbidException : AppException
    {
        public ForbidException(string message)
            : base(message, HttpStatusCode.Forbidden)
        { }
    }
}
