using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ExceptionExtensions;
using VerifyTests.MicrosoftLogging;

namespace DogQuiz.UnitTests.TestHelpers
{
    public abstract class TestBase : IDisposable
    {
        private const int _BreedImageCount = 3;

        protected IDbConnection Db { get; }
        protected ISql Sql { get; }
        protected IIdGenerator IdGenerator { get; }
        protected IOptions<Configuration> ConfigurationOptions { get; }
        protected Configuration Configuration { get; }
        protected DateTime Now { get; }
        protected FakeTimeProvider TimeProvider { get; }
        protected Random Random { get; }
        protected RecordingProvider RecordingProvider { get; }

        protected TestBase()
        {
            var builder = WebApplication.CreateBuilder();
            Db = new SqliteConnection("DataSource=:memory:");
            Db.Open();
            builder.Services.AddScoped(_ => Db);
            builder.Services.AddEmbeddedSql(options =>
            {
                options.Assemblies = new[]
                {
                    typeof(Program).Assembly,
                    typeof(TestBase).Assembly
                };
            });

            var app = builder.Build();
            Sql = app.Services.GetRequiredService<ISql>();
            var scope = app.Services.CreateScope();
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
            migrator.Run();
            IdGenerator = Substitute.For<IIdGenerator>();
            var counter = 0;
            IdGenerator.NewGuid().Returns(_ => $"guid{++counter}");
            IdGenerator.NewUlid().Returns(_ => $"ulid{++counter}");
            Configuration = new Configuration()
            {
                QuizLength = 10
            };

            ConfigurationOptions = Substitute.For<IOptions<Configuration>>();
            ConfigurationOptions.Value.Returns(_ => Configuration);
            Now = new DateTime(2024, 11, 13, 21, 35, 0, DateTimeKind.Utc);
            TimeProvider = new FakeTimeProvider();
            TimeProvider.SetLocalTimeZone(TimeZoneInfo.Utc);
            TimeProvider.SetUtcNow(Now);
            Random = new Random(622_495);
            RecordingProvider = new RecordingProvider();
            Recording.Start();
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            Db?.Dispose();
        }

        protected void Execute(Action<IDbTransaction> action)
        {
            var transaction = Db.BeginTransaction();
            action(transaction);
            transaction.Commit();
        }

        protected void HydrateBreeds(IDbTransaction transaction)
        {
            var breeds = TestDataProvider.Get<Dictionary<string, List<string>>>("Breeds.json");
            foreach (var (superBreed, subBreeds) in breeds)
            {
                var superBreedId = Db.ExecuteScalar<int>(Sql["SuperBreed.Create"], new { superBreed }, transaction);
                if (subBreeds.Count == 0)
                {
                    CreateBreed(transaction, superBreedId, superBreed);
                    continue;
                }

                foreach (var subBreed in subBreeds)
                {
                    var subBreedId = Db.ExecuteScalar<int>(Sql["SubBreed.Create"], new { superBreedId, subBreed }, transaction);
                    CreateBreed(transaction, superBreedId, superBreed, subBreedId, subBreed);
                }
            }
        }

        protected void CreateUserSession(string userSessionId, DateTime expiration, IDbTransaction transaction)
        {
            Db.Execute(Sql["UserSession.Create"], new { userSessionId, expiration }, transaction);
        }

        protected void CreateQuiz(string quizId, DateTime expiration, IDbTransaction transaction)
        {
            Db.Execute(Sql["Quiz.Create"], new { quizId, expiration }, transaction);
            var breedIds = Db
                .Query<int>(Sql["_Test.Breed.GetIds"], null, transaction)
                .OrderBy(x => Random.Next())
                .Take(Configuration.QuizLength)
                .ToList();

            for (var i = 0; i < Configuration.QuizLength; i++)
            {
                var breedId = breedIds[i];
                var imageId = Db
                    .Query<int>(Sql["_Test.Image.GetIds"], new { breedId }, transaction)
                    .OrderBy(x => Random.Next())
                    .First();

                Db.Execute(Sql["QuizQuestion.Create"], new { quizId, questionId = i + 1, breedId, imageId }, transaction);
            }
        }

        protected void AnswerQuiz(
            string quizId,
            string userSessionId,
            double score, DateTime timestamp,
            IDbTransaction transaction)
        {
            var correctAnswerCount = (int)Math.Round(Configuration.QuizLength * score);
            var correctAnswerIndexes = Enumerable.Range(0, correctAnswerCount)
                .OrderBy(x => Random.Next())
                .Take(correctAnswerCount)
                .ToHashSet();

            var breedIds = Db
                .Query<int>(Sql["_Test.Breed.GetIds"], null, transaction)
                .ToList();

            var quizQuestionBreedIds = Db
                .Query<int>(Sql["QuizQuestion.GetBreedIds"], new { quizId })
                .ToList();

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

                Db.Execute(Sql["QuizAnswer.Create"], new { quizId, userSessionId, questionId = i + 1, breedId }, transaction);
            }

            Db.Execute(Sql["QuizResult.Create"], new { quizId, userSessionId, score = score * 100, timestamp });
        }

        private void CreateBreed(
            IDbTransaction transaction,
            int superBreedId,
            string superBreed,
            int? subBreedId = null,
            string? subBreed = null)
        {
            var breedId = superBreedId * 1_000;
            if (subBreedId != null)
            {
                breedId += subBreedId.Value;
            }

            Db.Execute(Sql["Breed.Create"], new { breedId, superBreedId, subBreedId }, transaction);
            for (var i = 0; i < _BreedImageCount; i++)
            {
                var filename = $"{breedId}_{superBreed}";
                if (subBreed != null)
                {
                    filename += $"_{subBreed}";
                }

                filename += $"_{i}";
                var imageUrl = $"https://testing.dog.ceo/api/test/{filename}";
                Db.Execute(Sql["Image.Create"], new { breedId, filename, imageUrl }, transaction);
            }
        }

        protected static IDbConnection CreateBrokenDb()
        {
            var db = Substitute.For<IDbConnection>();
            db.CreateCommand().Throws<NotImplementedException>();

            return db;
        }
    }
}
