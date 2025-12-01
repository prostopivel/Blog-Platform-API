using BlogPlatform.Shared.API;
using BlogPlatform.Shared.Common.Models;

namespace BlogPlatform.Blog.API.Middleware
{
    public class ExceptionHandlingMiddleware : ExceptionHandlingMiddlewareBase
    {
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
            : base(next, logger, env) { }

        protected override async Task HandleExceptionAsync(HttpContext context,
            Exception exception)
        {
            context.Response.ContentType = "application/json";

            ErrorResponse response;

            switch (exception)
            {
                default:
                    await HandleCommonExceptionAsync(context, exception);
                    return;
            }

            await WriteErrorAsync(context, exception, response);
        }
    }
}
