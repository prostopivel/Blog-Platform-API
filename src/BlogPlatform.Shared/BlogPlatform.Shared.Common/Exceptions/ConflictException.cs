using System.Net;

namespace BlogPlatform.Shared.Common.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, HttpStatusCode.Conflict)
        { }
    }
}
