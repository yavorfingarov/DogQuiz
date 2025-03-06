namespace DogQuiz.UnitTests.Data
{
    public sealed class DataManagerTests : TestBase
    {
        private readonly IDataLoader _DataLoader;
        private readonly IDataCleaner _DataCleaner;
        private readonly DataManager _DataManager;

        public DataManagerTests()
        {
            _DataLoader = Substitute.For<IDataLoader>();
            _DataCleaner = Substitute.For<IDataCleaner>();
            var services = new ServiceCollection();
            services.AddScoped(_ => _DataLoader);
            services.AddScoped(_ => _DataCleaner);
            var serviceProvider = services.BuildServiceProvider();
            var scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(serviceProvider);
            var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);
            _DataManager = new DataManager(serviceScopeFactory, ConfigurationOptions);
        }

        [Fact]
        public async Task StartAsync()
        {
            await _DataManager.StartAsync(CancellationToken.None);

            await Task.Delay(250);
            await Verify(new
            {
                dataLoader = _DataLoader.ReceivedCalls(),
                dataCleaner = _DataCleaner.ReceivedCalls()
            });
        }

        [Fact]
        public async Task StartAsync_DataCleanerDelayed()
        {
            Configuration.CleanerWaitHours = 1;

            await _DataManager.StartAsync(CancellationToken.None);

            await Task.Delay(250);
            await Verify(new
            {
                dataLoader = _DataLoader.ReceivedCalls(),
                dataCleaner = _DataCleaner.ReceivedCalls()
            });
        }

        [Fact]
        public async Task StopAsync()
        {
            await _DataManager.StopAsync(CancellationToken.None);
        }

        public override void Dispose()
        {
            _DataManager.Dispose();
            base.Dispose();
        }
    }
}
