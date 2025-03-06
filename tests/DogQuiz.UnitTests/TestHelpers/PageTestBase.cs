using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace DogQuiz.UnitTests.TestHelpers
{
    public abstract class PageTestBase<T> : TestBase
        where T : PageModel
    {
        protected virtual bool HasFeatureCollection { get; }

        protected T PageModel { get; }
        protected string UserSessionId { get; }
        protected string QuizId { get; }

        private readonly List<string> _CookieKeys;

        protected PageTestBase()
        {
            var httpContext = HasFeatureCollection ? new DefaultHttpContext(Substitute.For<IFeatureCollection>()) : new DefaultHttpContext();
            var modelState = new ModelStateDictionary();
            var routeData = new RouteData();
            var pageActionDescriptor = new PageActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, pageActionDescriptor, modelState);
            PageModel = CreatePageModel();
            PageModel.PageContext = new PageContext(actionContext);
            PageModel.Request.Cookies = Substitute.For<IRequestCookieCollection>();
            _CookieKeys = new List<string>();
            PageModel.Request.Cookies.Keys.Returns(_CookieKeys);
            UserSessionId = "testUserSessionId";
            QuizId = "testQuizId";
            Configuration.QuizExpirationInDays = 90;
        }

        protected void AddCookie(string key, string value)
        {
            _CookieKeys.Add(key);
            PageModel.Request.Cookies.TryGetValue(key, out Arg.Any<string?>())
                .Returns(x =>
                {
                    x[1] = value;

                    return true;
                });
        }

        protected abstract T CreatePageModel();
    }
}
