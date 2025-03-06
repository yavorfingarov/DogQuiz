using Microsoft.AspNetCore.Routing;

namespace DogQuiz.UnitTests.Pages
{
    public sealed class ResultTests : PageTestBase<ResultModel>
    {
        [Fact]
        public Task OnGet()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
                AnswerQuiz(QuizId, UserSessionId, 0.8, Now, transaction);
            });

            var actionResult = PageModel.OnGet(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnGet_NoUserSession()
        {
            var actionResult = PageModel.OnGet(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnGet_InvalidUserSession()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);

            var actionResult = PageModel.OnGet(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnGet_InvalidQuiz()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
            });

            var actionResult = PageModel.OnGet(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnGet_NoAnswers()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
            });

            var actionResult = PageModel.OnGet(QuizId);

            return Verify(actionResult);
        }

        private Task Verify(IActionResult actionResult)
        {
            var userSessions = Db.Query(Sql["_Test.UserSession.GetAll"]);

            return Verifier.Verify(new { actionResult, PageModel, PageModel.Response.Headers, userSessions });
        }

        protected override ResultModel CreatePageModel()
        {
            var linkGenerator = Substitute.For<LinkGenerator>();
            var pageModel = new ResultModel(Db, Sql, IdGenerator, TimeProvider, ConfigurationOptions, linkGenerator);
            linkGenerator
                .GetPathByAddress(
                    Arg.Is<RouteValuesAddress>(x =>
                        (string?)x.ExplicitValues["page"] == "/Quiz" &&
                        (string?)x.ExplicitValues["quizId"] == QuizId),
                    Arg.Is<RouteValueDictionary>(x =>
                        (string?)x["page"] == "/Quiz" &&
                        (string?)x["quizId"] == QuizId))
                .Returns("https://dogquiz.com/test");

            return pageModel;
        }
    }
}
