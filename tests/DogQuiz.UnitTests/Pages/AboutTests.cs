namespace DogQuiz.UnitTests.Pages
{
    public sealed class AboutTests : PageTestBase<AboutModel>
    {
        [Fact]
        public Task OnGet()
        {
            double averageScore = 0;
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                averageScore = GenerateStatistics(transaction);
            });

            PageModel.OnGet();

            Assert.Equal(averageScore, PageModel.AverageScore);

            return Verify(PageModel);
        }

        [Fact]
        public Task OnGet_Empty()
        {
            Execute(HydrateBreeds);

            PageModel.OnGet();

            return Verify(PageModel);
        }

        private double GenerateStatistics(IDbTransaction transaction)
        {
            var breedIds = Db
                .Query<int>(Sql["_Test.Breed.GetIds"], transaction)
                .OrderBy(x => Random.Next())
                .Take(10)
                .ToList();

            var scores = new List<int>();
            foreach (var breedId in breedIds)
            {
                var score = Random.Next(0, 101);
                scores.Add(score);
                var questions = Random.Next(20, 800);
                var correctAnswers = (int)Math.Round((double)questions * score / 100);
                Db.Execute(Sql["_Test.Breed.Update"], new { breedId, questions, correctAnswers });
                Db.Execute(Sql["QuizResult.Create"], new { quizId = (string?)null, userSessionId = (string?)null, score, timestamp = Now });
            }

            Db.Execute(Sql["_Test.QuizCount.Update"], new { val = 10 });
            var averageRecognizability = scores.Average();

            return averageRecognizability;
        }

        protected override AboutModel CreatePageModel()
        {
            return new AboutModel(Db, Sql);
        }
    }
}
