namespace DogQuiz.UnitTests.TestData
{
    public static class TestDataProvider
    {
        public static T Get<T>(string filename)
        {
            using var stream = GetManifestResourceStream(filename);
            var result = JsonSerializer.Deserialize<T>(stream)
                ?? throw new InvalidOperationException($"Unexpected null value for '{typeof(T)}'.");

            return result;
        }

        public static byte[] GetBytes(string filename)
        {
            using var stream = GetManifestResourceStream(filename);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            var bytes = memory.ToArray();

            return bytes;
        }

        private static Stream GetManifestResourceStream(string filename)
        {
            var stream = typeof(TestDataProvider).Assembly
                .GetManifestResourceStream($"DogQuiz.UnitTests.TestData.{filename}")
                ?? throw new InvalidOperationException($"Could not load '{filename}'.");

            return stream;
        }
    }
}
