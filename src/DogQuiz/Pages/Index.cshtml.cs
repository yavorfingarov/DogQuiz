namespace DogQuiz.Pages
{
    [EnableRateLimiting("Post")]
    public sealed class IndexModel : BaseUserSessionPageModel
    {
        public string? RandomImage { get; set; }
        public IEnumerable<Result>? Results { get; set; }

        public IndexModel(
            IDbConnection db,
            ISql sql,
            IIdGenerator idGenerator,
            TimeProvider timeProvider,
            IOptions<Configuration> configuration)
            : base(db, sql, idGenerator, timeProvider, configuration)
        {
        }

        public void OnGet()
        {
            RandomImage = Db.ExecuteScalar<string>(Sql["Image.GetRandom"]);
            if (TryGetUserSessionId(out var userSessionId))
            {
                Results = Db.Query<Result>(Sql["QuizResult.GetAll"], new { userSessionId });
            }
        }

        public IActionResult OnPost()
        {
            using var transaction = Db.CreateTransaction();
            var quizId = IdGenerator.NewUlid();
            var expiration = TimeProvider.GetUtcNow().AddDays(Configuration.QuizExpirationInDays);
            Db.Execute(Sql["Quiz.Create"], new { quizId, expiration }, transaction);
            var count = Configuration.QuizLength;
            var breedImages = Db
                .Query<int>(Sql["ImageBreed.GetRandom"], new { count }, transaction)
                .ToList();

            for (var i = 0; i < breedImages.Count; i++)
            {
                Db.Execute(Sql["QuizQuestion.Create"], new { quizId, questionId = i + 1, imageId = breedImages[i] }, transaction);
            }

            Db.Execute(Sql["QuizCount.Bump"]);
            transaction.Commit();

            return RedirectToPage("/Quiz", new { quizId });
        }

        public sealed class Result
        {
            public string QuizId { get; set; } = null!;
            public double Score { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
