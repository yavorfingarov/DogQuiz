namespace DogQuiz.UnitTests.Pages
{
    public sealed class IndexTests : PageTestBase<IndexModel>
    {
        public IndexTests()
        {
            Execute(HydrateBreeds);
        }

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
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
                for (var i = 0; i < 5; i++)
                {
                    var quizId = $"{QuizId}_{i}";
                    var expiration = Now.AddDays(i * 7);
                    Db.Execute(Sql["Quiz.Create"], new { quizId, expiration }, transaction);
                    var timestamp = Now.AddDays(-i);
                    var score = (double)Random.Next(0, 1001) / 10;
                    Db.Execute(Sql["QuizResult.Create"], new { quizId, userSessionId = UserSessionId, score, timestamp }, transaction);
                }
            });

            PageModel.OnGet();

            return VerifyGet();
        }

        [Fact]
        public Task OnGet_NoScores()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);
            Execute(transaction =>
            {
                CreateUserSession(UserSessionId, Now.AddMonths(1), transaction);
            });

            PageModel.OnGet();

            return VerifyGet();
        }

        [Fact]
        public Task OnGet_NoUserSession()
        {
            PageModel.OnGet();

            return VerifyGet();
        }

        [Fact]
        public Task OnGet_InvalidUserSession()
        {
            AddCookie(Configuration.UserSessionIdCookieName, UserSessionId);

            PageModel.OnGet();

            return VerifyGet();
        }

        [Fact]
        public Task OnPost()
        {
            var result = PageModel.OnPost();

            var quiz = Db.Query(Sql["_Test.Quiz.GetAll"]);
            var quizCount = Db.Query(Sql["_Test.QuizCount.GetAll"]);
            var quizQuestionImages = Db.Query<QuizQuestion>(Sql["_Test.QuizQuestionImage.GetAll"])
                .OrderBy(x => x.QuestionId);

            var quizId = quizQuestionImages
                .Select(x => x.QuizId)
                .Distinct()
                .Single();

            var questionIds = quizQuestionImages
                .Select(x => x.QuestionId);

            var superBreeds = quizQuestionImages
                .Select(x => x.BreedId / 1_000)
                .Distinct()
                .Count();

            Assert.Equal(Configuration.QuizLength, quizQuestionImages.Count());
            Assert.Equal("ulid1", quizId);
            Assert.Equal(Enumerable.Range(1, Configuration.QuizLength), questionIds);
            Assert.Equal(Configuration.QuizLength, superBreeds);

            foreach (var quizQuestion in quizQuestionImages)
            {
                CheckImage(quizQuestion.Filename);
            }

            return Verify(new { result, quiz, quizCount });
        }

        private SettingsTask VerifyGet()
        {
            CheckImage(PageModel.RandomImage);
            var userSessions = Db.Query(Sql["_Test.UserSession.GetAll"]);

            return Verify(new { PageModel, PageModel.Response.Headers, userSessions })
                .IgnoreMember(nameof(IndexModel.RandomImage));
        }

        protected override IndexModel CreatePageModel()
        {
            return new IndexModel(Db, Sql, IdGenerator, TimeProvider, ConfigurationOptions);
        }

        private void CheckImage(string? filename)
        {
            var imageExists = Db.ExecuteScalar<bool>(Sql["_Test.Image.Exists"], new { filename });
            Assert.True(imageExists);
        }

        private sealed class QuizQuestion
        {
            public string QuizId { get; set; } = null!;
            public int QuestionId { get; set; }
            public int BreedId { get; set; }
            public string Filename { get; set; } = null!;
        }
    }
}
