using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using DogQuiz.Data;
using DogQuiz.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using NLog;
using NLog.Web;

namespace DogQuiz
{
    public static class Program
    {
        private readonly static string _AppInfo = GetAppInfo();

        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();
            var logger = LogManager.Setup()
                .LoadConfigurationFromAppSettings()
                .GetCurrentClassLogger();

            logger.Info(
                "Application starting. Environment: {Environment} / AppInfo: {AppInfo}",
                builder.Environment.EnvironmentName,
                _AppInfo);

            try
            {
                Run(builder);
                logger.Info("Application stopping gracefully.");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Application could not start.");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private static void Run(WebApplicationBuilder builder)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            var connectionString = builder.Configuration["ConnectionString"];
            var imagesPath = builder.Configuration["ImagesPath"]!;
            Directory.CreateDirectory(imagesPath);
            builder.Environment.WebRootFileProvider = new CompositeFileProvider(
                new PhysicalFileProvider(builder.Environment.WebRootPath),
                new PhysicalFileProvider(imagesPath));

            builder.Logging.ClearProviders();

            builder.Host.UseNLog();

            builder.Services
                .AddOptions<Configuration>()
                .Bind(builder.Configuration)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddScoped<IDbConnection>(_ => new SqliteConnection(connectionString));

            builder.Services.AddEmbeddedSql();

            builder.Services.AddSingleton(TimeProvider.System);

            builder.Services.AddSingleton<IIdGenerator, IdGenerator>();

            builder.Services.AddHttpClient<IDogApi, DogApi>();

            builder.Services.AddScoped<IDataLoader, DataLoader>();

            builder.Services.AddScoped<IDataCleaner, DataCleaner>();

            builder.Services.AddHostedService<DataManager>();

            builder.Services.AddAntiforgery(options =>
            {
                options.FormFieldName = Configuration.AntiforgeryTokenFormFieldName;
                options.Cookie = new CookieBuilder()
                {
                    Name = Configuration.AntiforgeryTokenCookieName,
                    Path = "/",
                    SecurePolicy = CookieSecurePolicy.Always,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true
                };
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("Post", httpContext =>
                {
                    if (httpContext.Request.Method != "POST" ||
                        httpContext.Connection.RemoteIpAddress == null)
                    {
                        return RateLimitPartition.GetNoLimiter((IPAddress.None, PathString.Empty));
                    }

                    return RateLimitPartition.GetTokenBucketLimiter(
                        (httpContext.Connection.RemoteIpAddress, httpContext.Request.Path),
                        _ => new TokenBucketRateLimiterOptions()
                        {
                            TokenLimit = 7,
                            TokensPerPeriod = 7,
                            AutoReplenishment = true,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(10)
                        });
                });
            });

            builder.Services
                .AddRazorPages()
                .AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = false;
                });

            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });
            }

            var app = builder.Build();

            app.UseExceptionHandler("/error");

            app.UseSecurityHeaders(policies =>
            {
                policies.AddDefaultSecurityHeaders();
                policies.AddCustomHeader(Configuration.AppInfoHeaderName, _AppInfo);
            });

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapMethods("/health", new[] { "HEAD" }, HealthEndpoint.Handle);

            app.UseStatusCodePagesWithReExecute("/error");

            app.UseRateLimiter();

            app.MapRazorPages();

            MigrateDb(app);

            app.Run();
        }

        private static string GetAppInfo()
        {
            var assemblyName = typeof(Program).Assembly.GetName();
            var appName = assemblyName.Name!;
            var version = assemblyName.Version!;
            var versionString = $"{version.Major:00}.{version.Minor:00}.{version.Build:00}.{version.Revision:0000}";
            var productHeaderValue = new ProductHeaderValue(appName, versionString);
            var appInfo = productHeaderValue.ToString();

            return appInfo;
        }

        private static void MigrateDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
            migrator.Run();
        }
    }
}
