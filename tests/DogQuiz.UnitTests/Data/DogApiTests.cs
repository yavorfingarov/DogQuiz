using System.Net.Http.Headers;
using VerifyTests.Http;

namespace DogQuiz.UnitTests.Data
{
    public sealed class DogApiTests : TestBase
    {
        private readonly string _ImageUrl = "https://testing.dog.ceo/api/image1.jpg";

        public DogApiTests()
        {
            Configuration.DogApiBaseUrl = "https://testing.dog.ceo/api";
        }

        [Fact]
        public async Task GetBreeds()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("BreedsRaw.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            var breeds = await dogApi.GetBreeds();

            await Verify(new { httpClient.Calls, breeds });
        }

        [Fact]
        public Task GetBreeds_EmptyResponse()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("Null.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            return ThrowsTask(dogApi.GetBreeds);
        }

        [Fact]
        public Task GetBreeds_EmptyMessage()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("Empty.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            return ThrowsTask(dogApi.GetBreeds);
        }

        [Fact]
        public async Task GetImageUrls()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("ImageUrls.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            var imageUrls = await dogApi.GetImageUrls("Appenzeller", null);

            await Verify(new { httpClient.Calls, imageUrls });
        }

        [Fact]
        public async Task GetImageUrls_WithSubBreed()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("ImageUrls.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            var imageUrls = await dogApi.GetImageUrls("Australian", "Kelpie");

            await Verify(new { httpClient.Calls, imageUrls });
        }

        [Fact]
        public Task GetImageUrls_EmptyResponse()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("Null.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            return ThrowsTask(() => dogApi.GetImageUrls("Appenzeller", null));
        }

        [Fact]
        public Task GetImageUrls_EmptyMessage()
        {
            var httpClient = CreateMockHttpClient(x => x.Content = CreateJsonContent("Empty.json"));
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            return ThrowsTask(() => dogApi.GetImageUrls("Appenzeller", null));
        }

        [Fact]
        public async Task GetImage()
        {
            var imageBytes = TestDataProvider.GetBytes("300x200.jpg");
            var httpClient = CreateMockHttpClient(x =>
            {
                x.Content = new ByteArrayContent(imageBytes);
                x.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            });

            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            var result = await dogApi.GetImage(_ImageUrl);

            Assert.Equal(imageBytes, result);
            await Verify(httpClient.Calls);
        }

        [Fact]
        public async Task GetImage_NotJpeg()
        {
            var imageBytes = TestDataProvider.GetBytes("300x200.jpg");
            var httpClient = CreateMockHttpClient(x =>
            {
                x.Content = new ByteArrayContent(imageBytes);
                x.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            });

            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            await ThrowsTask(() => dogApi.GetImage(_ImageUrl));
        }

        [Fact]
        public async Task GetImage_NotFound()
        {
            var httpClient = CreateMockHttpClient(x => x.StatusCode = HttpStatusCode.NotFound);
            var dogApi = new DogApi(httpClient, ConfigurationOptions);

            await ThrowsTask(() => dogApi.GetImage(_ImageUrl));
        }

        private static MockHttpClient CreateMockHttpClient(Action<HttpResponseMessage>? configure = null)
        {
            var httpClient = new MockHttpClient(request =>
            {
                var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                configure?.Invoke(httpResponseMessage);
                httpResponseMessage.RequestMessage = request;

                return httpResponseMessage;
            });

            return httpClient;
        }

        private static ByteArrayContent CreateJsonContent(string filename)
        {
            var bytes = TestDataProvider.GetBytes(filename);
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return content;
        }
    }
}
