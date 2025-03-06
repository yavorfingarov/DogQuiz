namespace DogQuiz.Pages
{
    public sealed class ResultModel : BaseUserSessionPageModel
    {
        public double? Score { get; set; }
        public List<Answer> Answers { get; set; } = null!;
        public string? ShareUrl { get; set; } = null!;

        private readonly LinkGenerator _LinkGenerator;

        public ResultModel(
            IDbConnection db,
            ISql sql,
            IIdGenerator idGenerator,
            TimeProvider timeProvider,
            IOptions<Configuration> configuration,
            LinkGenerator linkGenerator)
            : base(db, sql, idGenerator, timeProvider, configuration)
        {
            _LinkGenerator = linkGenerator;
        }

        public IActionResult OnGet(string quizId)
        {
            if (!TryGetUserSessionId(out var userSessionId))
            {
                return BadRequest();
            }

            Score = Db.ExecuteScalar<double?>(Sql["QuizResult.GetScore"], new { quizId, userSessionId });
            if (Score == null)
            {
                return NotFound();
            }

            ShareUrl = _LinkGenerator.GetPathByPage("/Quiz", values: new { quizId });
            Answers = Db
                .Query<Answer>(Sql["QuizAnswer.GetDiff"], new { quizId, userSessionId })
                .ToList();

            return Page();
        }

        public sealed class Answer
        {
            public string Given { get; set; } = null!;
            public string Correct { get; set; } = null!;
            public string ImageFilename { get; set; } = null!;
        }
    }
}
