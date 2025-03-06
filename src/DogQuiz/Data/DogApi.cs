using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DogQuiz.Data
{
    public interface IDogApi
    {
        Task<Dictionary<string, List<string>>> GetBreeds();
        Task<List<string>> GetImageUrls(string breed, string? subBreed);
        Task<byte[]> GetImage(string url);
    }

    public sealed class DogApi : IDogApi
    {
        private readonly Configuration _Configuration;
        private readonly HttpClient _HttpClient;

        public DogApi(HttpClient httpClient, IOptions<Configuration> configuration)
        {
            _HttpClient = httpClient;
            _Configuration = configuration.Value;
        }

        public async Task<Dictionary<string, List<string>>> GetBreeds()
        {
            var response = await _HttpClient
                .GetFromJsonAsync<Response<Dictionary<string, List<string>>>>($"{_Configuration.DogApiBaseUrl}/breeds/list/all");

            EnsureNotNull(response);
            var breeds = new Dictionary<string, List<string>>();
            foreach (var (key, value) in response.Message)
            {
                breeds[ToTitleCase(key)] = value
                    .Select(ToTitleCase)
                    .ToList();
            }

            return breeds;
        }

        public async Task<List<string>> GetImageUrls(string breed, string? subBreed)
        {
            var url = subBreed == null ?
                $"{_Configuration.DogApiBaseUrl}/breed/{breed.ToLowerInvariant()}/images" :
                $"{_Configuration.DogApiBaseUrl}/breed/{breed.ToLowerInvariant()}/{subBreed.ToLowerInvariant()}/images";

            var response = await _HttpClient.GetFromJsonAsync<Response<List<string>>>(url);
            EnsureNotNull(response);

            return response.Message;
        }

        public async Task<byte[]> GetImage(string url)
        {
            using var response = await _HttpClient.GetAsync(url);
            EnsureImageResponse(response);
            await using var file = await response.Content.ReadAsStreamAsync();
            await using var memory = new MemoryStream();
            file.CopyTo(memory);

            return memory.ToArray();
        }

        private static void EnsureNotNull<T>(
            [NotNull] Response<T>? response,
            [CallerArgumentExpression(nameof(response))] string? parameterName = null)
            where T : class
        {
            if (response is null)
            {
                throw new InvalidOperationException($"Fetched an unexpected null value for '{parameterName}'.");
            }

            if (response.Message is null)
            {
                throw new InvalidOperationException($"Fetched an unexpected null value for message in '{parameterName}'.");
            }
        }

        private static void EnsureImageResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Received status code '{response.StatusCode}' for '{response.RequestMessage?.RequestUri}'.");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType != "image/jpeg")
            {
                throw new InvalidOperationException(
                    $"Fetched an unexpected media type '{mediaType}' for '{response.RequestMessage?.RequestUri}'.");
            }
        }

        private static string ToTitleCase(string str)
        {
            var result = string.Concat(str[0].ToString().ToUpperInvariant(), str.AsSpan(1));

            return result;
        }

        private sealed record Response<T>(T Message) where T : class;
    }
}
