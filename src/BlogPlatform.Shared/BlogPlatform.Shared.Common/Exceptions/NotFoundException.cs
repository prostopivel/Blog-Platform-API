using System.Net;

namespace BlogPlatform.Shared.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound)
        { }
    }
}
