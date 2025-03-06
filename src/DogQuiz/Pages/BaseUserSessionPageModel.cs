using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DogQuiz.Pages
{
    public abstract class BaseUserSessionPageModel : PageModel
    {
        protected IDbConnection Db { get; }
        protected ISql Sql { get; }
        protected IIdGenerator IdGenerator { get; }
        protected TimeProvider TimeProvider { get; }
        protected Configuration Configuration { get; }

        protected BaseUserSessionPageModel(
            IDbConnection db,
            ISql sql,
            IIdGenerator idGenerator,
            TimeProvider timeProvider,
            IOptions<Configuration> configuration)
        {
            Db = db;
            Sql = sql;
            IdGenerator = idGenerator;
            TimeProvider = timeProvider;
            Configuration = configuration.Value;
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = BadRequest();
            }
        }

        protected void SetCookie(string? userSessionId = null)
        {
            if (userSessionId == null)
            {
                userSessionId = IdGenerator.NewUlid();
                var expiration = TimeProvider.GetUtcNow().AddDays(Configuration.QuizExpirationInDays);
                Db.Execute(Sql["UserSession.Create"], new { userSessionId, expiration });
            }

            var cookieOptions = new CookieOptions()
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                MaxAge = TimeSpan.FromDays(Configuration.QuizExpirationInDays)
            };

            Response.Cookies.Append(Configuration.UserSessionIdCookieName, userSessionId, cookieOptions);
        }

        protected bool TryGetUserSessionId([NotNullWhen(true)] out string? userSessionId)
        {
            if (Request.Cookies.TryGetValue(Configuration.UserSessionIdCookieName, out userSessionId))
            {
                var expiration = TimeProvider.GetUtcNow().AddDays(Configuration.QuizExpirationInDays);
                var rows = Db.Execute(Sql["UserSession.Update"], new { userSessionId, expiration });
                if (rows == 1)
                {
                    SetCookie(userSessionId);

                    return true;
                }
            }

            return false;
        }
    }
}
