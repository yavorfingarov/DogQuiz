namespace DogQuiz.UnitTests.Pages
{
    public sealed class QuizTests : PageTestBase<QuizModel>
    {
        [Fact]
        public void EnableRateLimiting()
        {
            var enableRateLimitingAttribute = PageModel.GetType()
                .GetCustomAttribute<EnableRateLimitingAttribute>();

            Assert.NotNull(enableRateLimitingAttribute);
            Assert.Equal("Post", enableRateLimitingAttribute.PolicyName);
        }

        [Fact]
        public Task OnGet()
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
        public Task OnGet_AlreadyAnswered()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
                AnswerQuiz(QuizId, UserSessionId, 0.5, Now.AddMonths(-1), transaction);
            });

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
        public Task OnGetCookieCheck()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
            });

            var actionResult = PageModel.OnGetCookieCheck(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnGetCookieCheck_NoUserSession()
        {
            var actionResult = PageModel.OnGetCookieCheck(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnGetCookieCheck_InvalidUserSession()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);

            var actionResult = PageModel.OnGetCookieCheck(QuizId);

            return Verify(actionResult);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0.8)]
        public Task OnPost(double score)
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
            });

            AnswerQuiz(score);

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult)
                .UseParameters(score);
        }

        [Fact]
        public Task OnPost_NoUserSession()
        {
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
            });

            AnswerQuiz(0.7);

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnPost_InvalidUserSession()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
            });

            AnswerQuiz(0.7);

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnPost_InvalidAnswersCount()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
            });

            PageModel.Answers = Db
                .Query<int>(Sql["QuizQuestion.GetBreedIds"], new { quizId = QuizId })
                .OrderBy(x => Random.Next())
                .Take(Configuration.QuizLength - 1)
                .ToArray();

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnPost_AlreadyAnswered()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
                AnswerQuiz(QuizId, UserSessionId, 0.5, Now.AddMonths(-1), transaction);
            });

            AnswerQuiz(0.7);

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnPost_InvalidQuiz()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
            });

            PageModel.Answers = Enumerable
                .Range(1, Configuration.QuizLength)
                .ToArray();

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult);
        }

        [Fact]
        public Task OnPost_InvalidAnswer()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                CreateQuiz(QuizId, Now.AddMonths(1), transaction);
            });

            AnswerQuiz(0.7);
            PageModel.Answers[5] = 10;

            var actionResult = PageModel.OnPost(QuizId);

            return Verify(actionResult);
        }

        private void AnswerQuiz(double score)
        {
            var correctAnswerCount = (int)Math.Round(Configuration.QuizLength * score);
            var correctAnswerIndexes = Enumerable.Range(0, correctAnswerCount)
                .OrderBy(x => Random.Next())
                .Take(correctAnswerCount)
                .ToHashSet();

            var quizQuestionBreedIds = Db
                .Query<int>(Sql["QuizQuestion.GetBreedIds"], new { quizId = QuizId })
                .ToList();

            var breedIds = Db
                .Query<int>(Sql["_Test.Breed.GetIds"])
                .ToList();

            PageModel.Answers = new int[Configuration.QuizLength];
            for (var i = 0; i < Configuration.QuizLength; i++)
            {
                var breedId = quizQuestionBreedIds[i];
                if (!correctAnswerIndexes.Contains(i))
                {
                    while (breedId == quizQuestionBreedIds[i])
                    {
                        breedId = breedIds
                            .OrderBy(x => Random.Next())
                            .First();
                    }
                }

                PageModel.Answers[i] = breedId;
            }
        }

        private SettingsTask Verify(IActionResult actionResult)
        {
            var userSessions = Db.Query(Sql["_Test.UserSession.GetAll"]);
            var quizzes = Db.Query(Sql["_Test.Quiz.GetAll"]);
            var quizQuestionBreedIds = Db.Query(Sql["QuizQuestion.GetBreedIds"], new { quizId = QuizId });
            var quizAnswers = Db.Query(Sql["_Test.QuizAnswer.GetAll"]);
            var quizResults = Db.Query(Sql["_Test.QuizResult.GetAll"]);
            var updatedBreeds = Db.Query(Sql["_Test.Breed.GetAll"])
                .Where(x => x.questions != 0);

            return Verifier.Verify(new
            {
                actionResult,
                PageModel,
                PageModel.Response.Headers,
                userSessions,
                quizzes,
                quizQuestionBreedIds,
                quizAnswers,
                quizResults,
                updatedBreeds
            });
        }

        protected override QuizModel CreatePageModel()
        {
            return new QuizModel(Db, Sql, IdGenerator, TimeProvider, ConfigurationOptions);
        }
    }
}
