namespace DogQuiz.UnitTests
{
    public sealed class DbTests : TestBase
    {
        [Fact]
        public Task Schema()
        {
            var schema = Db.Query<string>(Sql["_Test.Db.Schema"]);

            return Verify(schema);
        }
    }
}
