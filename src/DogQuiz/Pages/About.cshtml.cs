namespace DogQuiz.Pages
{
    public sealed class AboutModel : PageModel
    {
        public int QuizCount { get; set; }
        public int SolvedCount { get; set; }
        public double? AverageScore { get; set; }
        public int BreedCount { get; set; }
        public int ImageCount { get; set; }
        public Dictionary<string, List<BreedSummary>> Breeds { get; set; } = null!;

        private readonly IDbConnection _Db;
        private readonly ISql _Sql;

        public AboutModel(
            IDbConnection db,
            ISql sql)
        {
            _Db = db;
            _Sql = sql;
        }

        public void OnGet()
        {
            Breeds = _Db
                .Query<BreedSummary>(_Sql["Breed.GetSummary"])
                .GroupBy(x => x.SuperBreed)
                .ToDictionary(x => x.Key, x => x.ToList());

            BreedCount = Breeds
                .SelectMany(x => x.Value)
                .Count();

            ImageCount = Breeds
                .SelectMany(x => x.Value)
                .Sum(x => x.ImageCount);

            QuizCount = _Db.ExecuteScalar<int>(_Sql["QuizCount.Get"]);
            SolvedCount = _Db.ExecuteScalar<int>(_Sql["QuizResult.GetCount"]);
            AverageScore = _Db.ExecuteScalar<double?>(_Sql["QuizResult.GetAverageScore"]);
        }

        public sealed class BreedSummary
        {
            public string SuperBreed { get; set; } = null!;
            public string SubBreed { get; set; } = null!;
            public int ImageCount { get; set; }
            public double? Recognizability { get; set; }
        }
    }
}
