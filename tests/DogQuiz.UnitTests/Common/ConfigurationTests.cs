namespace DogQuiz.UnitTests.Common
{
    public sealed class ConfigurationTests
    {
        [Fact]
        public Task DogApiBaseUrl()
        {
            var attributes = GetAttributes(nameof(Configuration.DogApiBaseUrl));

            return Verify(attributes);
        }

        [Fact]
        public Task ImagesPath()
        {
            var attributes = GetAttributes(nameof(Configuration.ImagesPath));

            return Verify(attributes);
        }

        [Fact]
        public Task ImageTargetSize()
        {
            var attributes = GetAttributes(nameof(Configuration.ImageTargetSize));

            return Verify(attributes);
        }

        [Fact]
        public Task ImageTargetQuality()
        {
            var attributes = GetAttributes(nameof(Configuration.ImageTargetQuality));

            return Verify(attributes);
        }

        [Fact]
        public Task ImageTargetMinFileSize()
        {
            var attributes = GetAttributes(nameof(Configuration.ImageTargetMinFileSize));

            return Verify(attributes);
        }

        [Fact]
        public Task QuizLength()
        {
            var attributes = GetAttributes(nameof(Configuration.QuizLength));

            return Verify(attributes);
        }

        [Fact]
        public Task QuizExpirationInDays()
        {
            var attributes = GetAttributes(nameof(Configuration.QuizExpirationInDays));

            return Verify(attributes);
        }

        [Fact]
        public Task CleanerWaitHours()
        {
            var attributes = GetAttributes(nameof(Configuration.CleanerWaitHours));

            return Verify(attributes);
        }

        private static IEnumerable<Attribute> GetAttributes(string propertyName)
        {
            var attributes = typeof(Configuration)
                .GetProperty(propertyName)!
                .GetCustomAttributes();

            return attributes;
        }
    }
}
