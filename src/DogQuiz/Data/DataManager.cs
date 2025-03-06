namespace DogQuiz.Data
{
    public sealed class DataManager : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _ServiceScopeFactory;
        private readonly Configuration _Configuration;

        private Timer? _Timer;

        public DataManager(IServiceScopeFactory serviceScopeFactory, IOptions<Configuration> configuration)
        {
            _ServiceScopeFactory = serviceScopeFactory;
            _Configuration = configuration.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(RunDataLoader, cancellationToken);
            var dueTime = TimeSpan.FromHours(_Configuration.CleanerWaitHours);
            var period = TimeSpan.FromHours(_Configuration.CleanerPeriodHours);
            _Timer = new Timer(RunDataCleaner, null, dueTime, period);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _Timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _Timer?.Dispose();
        }

        private async Task RunDataLoader()
        {
            using var scope = _ServiceScopeFactory.CreateScope();
            var dataLoader = scope.ServiceProvider.GetRequiredService<IDataLoader>();
            await dataLoader.Run();
        }

        private void RunDataCleaner(object? state)
        {
            using var scope = _ServiceScopeFactory.CreateScope();
            var dataCleaner = scope.ServiceProvider.GetRequiredService<IDataCleaner>();
            dataCleaner.Run();
        }
    }
}
