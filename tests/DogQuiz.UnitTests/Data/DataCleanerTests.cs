namespace DogQuiz.UnitTests.Data
{
    public sealed class DataCleanerTests : TestBase
    {
        [Fact]
        public Task Run()
        {
            Execute(transaction =>
            {
                HydrateBreeds(transaction);
                CreateUserSession("not_expired_user_session", Now.AddMonths(1), transaction);
                CreateUserSession("expired_user_session", Now.AddMonths(-1), transaction);
                CreateQuiz("expired_quiz_no_answers", Now.AddMonths(-1), transaction);
                CreateQuizAndAnswer("expired_quiz_with_answers", Now.AddMonths(-1), "not_expired_user_session", 0.5, Now, transaction);
                CreateQuizAndAnswer("expired_quiz_with_expired_answers", Now.AddMonths(-1), "expired_user_session", 0.5, Now, transaction);
                CreateQuiz("not_expired_quiz_no_answers", Now.AddMonths(1), transaction);
                CreateQuizAndAnswer("not_expired_quiz_with_answers", Now.AddMonths(-1), "not_expired_user_session", 0.5, Now, transaction);
                CreateQuizAndAnswer("not_expired_quiz_with_expired_answers", Now.AddMonths(-1), "expired_user_session", 0.5, Now, transaction);
            });

            var logger = RecordingProvider.CreateLogger<DataCleaner>();
            var dataCleaner = new DataCleaner(Db, Sql, TimeProvider, logger);

            dataCleaner.Run();

            var userSessions = Db.Query(Sql["_Test.UserSession.GetAll"]);
            var quizzes = Db.Query(Sql["_Test.Quiz.GetAll"]);
            var quizQuestions = Db.Query(Sql["_Test.QuizQuestion.GetAll"]);
            var quizAnswers = Db.Query(Sql["_Test.QuizAnswer.GetAll"]);
            var quizResults = Db.Query(Sql["_Test.QuizResult.GetAll"]);

            return Verify(new { userSessions, quizzes, quizQuestions, quizAnswers, quizResults });
        }

        [Fact]
        public Task Run_Error()
        {
            var db = CreateBrokenDb();
            var logger = RecordingProvider.CreateLogger<DataCleaner>();
            var dataCleaner = new DataCleaner(db, Sql, TimeProvider, logger);

            dataCleaner.Run();

            return Verify();
        }

        private void CreateQuizAndAnswer(
            string quizId,
            DateTime quizExpiration,
            string userSessionId,
            double score,
            DateTime answerTimestamp,
            IDbTransaction transaction)
        {
            CreateQuiz(quizId, quizExpiration, transaction);
            AnswerQuiz(quizId, userSessionId, score, answerTimestamp, transaction);
        }
    }
}
