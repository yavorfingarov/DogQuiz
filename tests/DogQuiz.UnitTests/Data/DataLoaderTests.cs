using ImageMagick;

namespace DogQuiz.UnitTests.Data
{
    public sealed class DataLoaderTests : TestBase
    {
        private readonly DataLoader _DataLoader;

        public DataLoaderTests()
        {
            var dogApi = Substitute.For<IDogApi>();
            var breeds = TestDataProvider.Get<Dictionary<string, List<string>>>("Breeds.json");
            dogApi.GetBreeds().Returns(breeds);
            var images = new List<string>()
            {
                "https://testing.dog.ceo/api/image1",
                "https://testing.dog.ceo/api/image2",
                "https://testing.dog.ceo/api/image3",
                "https://testing.dog.ceo/api/image4"
            };

            dogApi.GetImageUrls(Arg.Any<string>(), Arg.Any<string>()).Returns(images);
            dogApi.GetImage("https://testing.dog.ceo/api/image1").Returns(TestDataProvider.GetBytes("300x200.jpg"));
            dogApi.GetImage("https://testing.dog.ceo/api/image2").Returns(TestDataProvider.GetBytes("600x400.jpg"));
            dogApi.GetImage("https://testing.dog.ceo/api/image3").Returns(TestDataProvider.GetBytes("900x600.jpg"));
            dogApi.GetImage("https://testing.dog.ceo/api/image4").Returns(TestDataProvider.GetBytes("600x400-low-quality.jpg"));
            Configuration.ImagesPath = "Images/";
            Configuration.ImageTargetSize = 500;
            Configuration.ImageTargetQuality = 75;
            Configuration.ImageTargetMinFileSize = 2_560;
            Directory.CreateDirectory(Configuration.ImagesPath);
            var logger = RecordingProvider.CreateLogger<DataLoader>();
            _DataLoader = new DataLoader(Db, Sql, dogApi, IdGenerator, ConfigurationOptions, logger);
        }

        [Fact]
        public async Task Run()
        {
            await _DataLoader.Run();

            var superBreeds = Db.Query(Sql["_Test.SuperBreed.GetAll"]);
            var subBreeds = Db.Query(Sql["_Test.SubBreed.GetAll"]);
            var breeds = Db.Query(Sql["_Test.Breed.GetAll"]);
            var images = Db.Query(Sql["_Test.Image.GetAll"]);

            foreach (var image in images)
            {
                var imagePath = Path.Combine(ConfigurationOptions.Value.ImagesPath, $"{image.filename}.jpg");
                var imageBytes = await File.ReadAllBytesAsync(imagePath);
                using var imageFile = new MagickImage(imageBytes);
                Assert.Equal(ConfigurationOptions.Value.ImageTargetSize, (int)Math.Max(imageFile.Width, imageFile.Height));
                Assert.Equal(ConfigurationOptions.Value.ImageTargetQuality, imageFile.Quality);
            }

            await Verify(new { superBreeds, subBreeds, breeds, images });
        }

        [Fact]
        public async Task Run_Error()
        {
            Db.Execute(Sql["SuperBreed.Create"], new { superBreed = "Appenzeller" });

            await _DataLoader.Run();

            var breedImages = Db.Query(Sql["_Test.Breed.GetAll"])
                .ToList();

            await Verify(breedImages);
        }

        [Fact]
        public async Task Run_AlreadyLoaded()
        {
            var breedId = 1000;
            var superBreedId = Db.ExecuteScalar<int>(Sql["SuperBreed.Create"], new { superBreed = "Appenzeller" });
            Db.Execute(Sql["Breed.Create"], new { breedId, superBreedId, subBreedId = (int?)null });
            Db.Execute(Sql["Image.Create"], new { breedId, filename = "guid1", imageUrl = "https://testing.dog.ceo/api/test/image1" });

            await _DataLoader.Run();

            var breedImages = Db.Query(Sql["_Test.Breed.GetAll"]);
            await Verify(breedImages);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (Directory.Exists(ConfigurationOptions.Value.ImagesPath))
            {
                var images = Directory.EnumerateFiles(ConfigurationOptions.Value.ImagesPath);
                foreach (var image in images)
                {
                    File.Delete(image);
                }
            }
        }
    }
}
