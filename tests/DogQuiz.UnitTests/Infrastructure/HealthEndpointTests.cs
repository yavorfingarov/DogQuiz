namespace DogQuiz.UnitTests.Infrastructure
{
    public sealed class HealthEndpointTests : TestBase
    {
        private readonly ILogger<HealthEndpoint> _Logger;

        public HealthEndpointTests()
        {
            _Logger = RecordingProvider.CreateLogger<HealthEndpoint>();
        }

        [Fact]
        public Task Ok()
        {
            var result = HealthEndpoint.Handle(Db, Sql, _Logger);

            return Verify(result);
        }

        [Fact]
        public Task NotOk()
        {
            var db = CreateBrokenDb();
            var result = HealthEndpoint.Handle(db, Sql, _Logger);

            return Verify(result);
        }
    }
}
