using ImageMagick;

namespace DogQuiz.Data
{
    public interface IDataLoader
    {
        Task Run();
    }

    public sealed class DataLoader : IDataLoader
    {
        private readonly IDbConnection _Db;
        private readonly ISql _Sql;
        private readonly IDogApi _DogApi;
        private readonly IIdGenerator _IdGenerator;
        private readonly Configuration _Configuration;
        private readonly ILogger<DataLoader> _Logger;
        private readonly MagickGeometry _ImageGeometry;

        public DataLoader(
            IDbConnection db,
            ISql sql,
            IDogApi dogApi,
            IIdGenerator idGenerator,
            IOptions<Configuration> configuration,
            ILogger<DataLoader> logger)
        {
            _Db = db;
            _Sql = sql;
            _DogApi = dogApi;
            _IdGenerator = idGenerator;
            _Logger = logger;
            _Configuration = configuration.Value;
            var imageTargetSize = (uint)_Configuration.ImageTargetSize;
            _ImageGeometry = new MagickGeometry(imageTargetSize, imageTargetSize);
        }

        public async Task Run()
        {
            try
            {
                var isDataLoaded = _Db.ExecuteScalar<bool>(_Sql["Breed.Any"]);
                if (isDataLoaded)
                {
                    return;
                }

                await LoadData();
                _Logger.LogInformation("Data load finished.");
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Could not load data.");
            }
        }

        private async Task LoadData()
        {
            var breeds = await _DogApi.GetBreeds();
            using var transaction = _Db.CreateTransaction();
            foreach (var (superBreed, subBreeds) in breeds)
            {
                var superBreedId = _Db.ExecuteScalar<int>(_Sql["SuperBreed.Create"], new { superBreed }, transaction);
                if (subBreeds.Count == 0)
                {
                    await CreateBreed(transaction, superBreedId, superBreed);
                }

                foreach (var subBreed in subBreeds)
                {
                    var subBreedId = _Db.ExecuteScalar<int>(_Sql["SubBreed.Create"], new { superBreedId, subBreed }, transaction);
                    await CreateBreed(transaction, superBreedId, superBreed, subBreedId, subBreed);
                }
            }

            transaction.Commit();
        }

        private async Task CreateBreed(
            IDbTransaction transaction,
            int superBreedId,
            string superBreed,
            int? subBreedId = null,
            string? subBreed = null)
        {
            var breedId = superBreedId * 1_000;
            if (subBreedId != null)
            {
                breedId += subBreedId.Value;
            }

            _Db.Execute(_Sql["Breed.Create"], new { breedId, superBreedId, subBreedId }, transaction);
            var imageUrls = await _DogApi.GetImageUrls(superBreed, subBreed);
            foreach (var imageUrl in imageUrls)
            {
                var file = await _DogApi.GetImage(imageUrl);
                using var image = new MagickImage(file);
                if (Math.Max(image.Width, image.Height) < _ImageGeometry.Width)
                {
                    continue;
                }

                image.Thumbnail(_ImageGeometry);
                image.Quality = Math.Min(image.Quality, _Configuration.ImageTargetQuality);
                var bytes = image.ToByteArray(MagickFormat.Jpg);
                if (bytes.Length < _Configuration.ImageTargetMinFileSize)
                {
                    continue;
                }

                var filename = _IdGenerator.NewGuid();
                var path = Path.Combine(_Configuration.ImagesPath, $"{filename}.jpg");
                await File.WriteAllBytesAsync(path, bytes);
                _Db.Execute(_Sql["Image.Create"], new { breedId, filename, imageUrl }, transaction);
            }
        }
    }
}
