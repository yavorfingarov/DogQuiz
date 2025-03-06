namespace DogQuiz.Pages
{
    [EnableRateLimiting("Post")]
    public sealed class QuizModel : BaseUserSessionPageModel
    {
        public List<string> Images { get; set; } = null!;
        public Dictionary<string, List<Breed>> Breeds { get; set; } = null!;

        [BindProperty]
        public int[] Answers { get; set; } = null!;

        public QuizModel(
            IDbConnection db,
            ISql sql,
            IIdGenerator idGenerator,
            TimeProvider timeProvider,
            IOptions<Configuration> configuration)
            : base(db, sql, idGenerator, timeProvider, configuration)
        {
        }

        public IActionResult OnGet(string quizId)
        {
            if (!TryGetUserSessionId(out var userSessionId))
            {
                SetCookie();

                return RedirectToPage("/Quiz", "CookieCheck", new { quizId });
            }

            if (IsAlreadyAnswered(quizId, userSessionId))
            {
                return RedirectToPage("/Result", new { quizId });
            }

            Images = Db
                .Query<string>(Sql["QuizQuestion.GetImages"], new { quizId })
                .ToList();

            if (Images.Count == 0)
            {
                return NotFound();
            }

            Answers = new int[Configuration.QuizLength];
            Breeds = Db
                .Query<Breed>(Sql["Breed.GetAll"])
                .GroupBy(x => x.SuperBreed)
                .ToDictionary(x => x.Key, x => x.ToList());

            return Page();
        }

        public IActionResult OnGetCookieCheck(string quizId)
        {
            if (TryGetUserSessionId(out _))
            {
                return RedirectToPage(new { quizId });
            }

            return BadRequest();
        }

        public IActionResult OnPost(string quizId)
        {
            if (!TryGetUserSessionId(out var userSessionId) ||
                Answers.Length != Configuration.QuizLength ||
                IsAlreadyAnswered(quizId, userSessionId))
            {
                return BadRequest();
            }

            using var transaction = Db.CreateTransaction();
            var timestamp = TimeProvider.GetUtcNow();
            var expiration = timestamp.AddDays(Configuration.QuizExpirationInDays);
            var rows = Db.Execute(Sql["Quiz.Update"], new { quizId, expiration }, transaction);
            if (rows == 0)
            {
                return NotFound();
            }

            var correctBreedIds = Db
                .Query<int>(Sql["QuizQuestion.GetBreedIds"], new { quizId }, transaction)
                .ToList();

            var correctAnswers = 0;
            for (var i = 0; i < Answers.Length; i++)
            {
                var breedId = Answers[i];
                var questionId = i + 1;
                var correctAnswersIncrement = breedId == correctBreedIds[i] ? 1 : 0;
                correctAnswers += correctAnswersIncrement;
                rows = Db.Execute(Sql["Breed.Update"], new { breedId, correctAnswersIncrement }, transaction);
                if (rows == 0)
                {
                    return BadRequest();
                }

                Db.Execute(Sql["QuizAnswer.Create"], new { quizId, userSessionId, questionId, breedId }, transaction);
            }

            var score = Math.Round((double)correctAnswers / Answers.Length * 100, 2);
            Db.Execute(Sql["QuizResult.Create"], new { quizId, userSessionId, score, timestamp }, transaction);
            transaction.Commit();

            return RedirectToPage("/Result", new { quizId });
        }

        private bool IsAlreadyAnswered(string quizId, string userSessionId)
        {
            var isAlreadyAnswered = Db.ExecuteScalar<bool>(Sql["QuizAnswer.Exists"], new { quizId, userSessionId });

            return isAlreadyAnswered;
        }

        public sealed class Breed
        {
            public int Id { get; set; }
            public string SuperBreed { get; set; } = null!;
            public string SubBreed { get; set; } = null!;
        }
    }
}
