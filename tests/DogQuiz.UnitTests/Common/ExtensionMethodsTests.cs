namespace DogQuiz.UnitTests.Common
{
    public sealed class ExtensionMethodsTests
    {
        private readonly IDbConnection _Db = Substitute.For<IDbConnection>();

        [Fact]
        public Task CreateTransaction()
        {
            _Db.CreateTransaction();

            return Verify(_Db.ReceivedCalls());
        }

        [Fact]
        public Task CreateTransaction_DbOpen()
        {
            _Db.State.Returns(ConnectionState.Open);

            _Db.CreateTransaction();

            return Verify(_Db.ReceivedCalls());
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData((double)7, "7%")]
        [InlineData(58.69, "58.69%")]
        public void ToPercent(double? value, string? expected)
        {
            var result = value.ToPercent();

            Assert.Equal(expected, result);
        }
    }
}
