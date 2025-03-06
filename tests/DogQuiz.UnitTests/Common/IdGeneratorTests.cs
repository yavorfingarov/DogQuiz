using System.Globalization;

namespace DogQuiz.UnitTests.Common
{
    public class IdGeneratorTests
    {
        private readonly IdGenerator _IdGenerator = new();

        [Fact]
        public void NewGuid()
        {
            var guidString = _IdGenerator.NewGuid();

            var guid = Guid.ParseExact(guidString, "N");
            Assert.NotEqual(Guid.Empty, guid);
            Assert.All(guidString, x =>
            {
                Assert.True(char.IsAsciiHexDigitLower(x));
            });
        }

        [Fact]
        public void NewUlid()
        {
            var ulidString = _IdGenerator.NewUlid();

            var ulid = Ulid.Parse(ulidString, CultureInfo.InvariantCulture);
            Assert.Equal(ulid.Time, DateTime.UtcNow, TimeSpan.FromSeconds(1));
            Assert.All(ulidString, x =>
            {
                Assert.True(char.IsDigit(x) || char.IsLower(x));
            });
        }
    }
}
