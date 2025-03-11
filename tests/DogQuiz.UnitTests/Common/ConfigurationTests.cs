namespace DogQuiz.UnitTests.Common
{
    public sealed class ConfigurationTests
    {
        [Fact]
        public Task Snapshot()
        {
            var configuration = new Dictionary<string, object?>();
            var properties = typeof(Configuration).GetProperties();
            foreach (var property in properties)
            {
                var getMethod = property.GetGetMethod();
                if (getMethod != null && getMethod.IsStatic)
                {
                    configuration[property.Name] = getMethod.Invoke(null, Array.Empty<object?>());
                }
                else
                {
                    configuration[property.Name] = property.GetCustomAttributes();
                }
            }

            return Verify(configuration);
        }
    }
}
