namespace DogQuiz.Data
{
    public interface IDataCleaner
    {
        void Run();
    }

    public sealed class DataCleaner : IDataCleaner
    {
        private readonly IDbConnection _Db;
        private readonly ISql _Sql;
        private readonly TimeProvider _TimeProvider;
        private readonly ILogger<DataCleaner> _Logger;

        public DataCleaner(
            IDbConnection db,
            ISql sql,
            TimeProvider timeProvider,
            ILogger<DataCleaner> logger)
        {
            _Db = db;
            _Sql = sql;
            _TimeProvider = timeProvider;
            _Logger = logger;
        }

        public void Run()
        {
            try
            {
                var rows = Clean();
                _Logger.LogInformation("Data clean finished. Rows affected: {RowsAffected}", rows);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Could not clean data.");
            }
        }

        private int Clean()
        {
            var now = _TimeProvider.GetUtcNow();
            var rows = _Db.Execute(_Sql["UserSession.DeleteStale"], new { now });
            rows += _Db.Execute(_Sql["Quiz.DeleteStale"], new { now });
            _Db.Execute(_Sql["Db.IncrementalVacuum"]);
            _Db.Execute(_Sql["Db.Optimize"]);

            return rows;
        }
    }
}
