using Microsoft.AspNetCore.Diagnostics;

namespace DogQuiz.Pages
{
    [IgnoreAntiforgeryToken]
    public sealed class ErrorModel : PageModel
    {
        public string Message { get; private set; } = null!;
        public bool ShowCookieHint { get; private set; }

        private readonly ILogger<ErrorModel> _Logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _Logger = logger;
        }

        public void OnGet()
        {
            Handle();
        }

        public void OnPost()
        {
            Handle();
        }

        private void Handle()
        {
            if (Response.StatusCode == 200)
            {
                Response.StatusCode = 404;
            }

            var path = GetPath();
            var hasAntiforgeryToken = Request.Cookies.Keys
                .Any(x => x == Configuration.AntiforgeryTokenCookieName);

            Request.Cookies.TryGetValue(Configuration.UserSessionIdCookieName, out var userSessionId);
            ShowCookieHint =
                Response.StatusCode == StatusCodes.Status400BadRequest &&
                !hasAntiforgeryToken &&
                userSessionId == null;

            Message = Response.StatusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad request",
                StatusCodes.Status404NotFound => "Not found",
                StatusCodes.Status429TooManyRequests => "Too many requests",
                StatusCodes.Status500InternalServerError => "Internal server error",
                _ => "Something went wrong."
            };

            _Logger.LogInformation(
                "Error handled. Request: {Method} {Path} | Status code: {StatusCode} | " +
                "HasAntiforgeryToken: {HasAntiforgeryToken} | UserSessionId: {UserSessionId}",
                Sanitize(Request.Method),
                Sanitize(path),
                Response.StatusCode,
                hasAntiforgeryToken,
                userSessionId);
        }

        private string GetPath()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature != null)
            {
                return exceptionHandlerPathFeature.Path;
            }

            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature != null)
            {
                return statusCodeReExecuteFeature.OriginalPath;
            }

            return Request.Path;
        }

        private static string Sanitize(string input)
        {
            return input
                .Replace(Environment.NewLine, "")
                .Replace("\n", "")
                .Replace("\r", "");
        }
    }
}
