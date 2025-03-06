global using System.Data;
global using System.Net;
global using System.Reflection;
global using System.Text.Json;
global using Dapper;
global using DogQuiz.Common;
global using DogQuiz.Data;
global using DogQuiz.Infrastructure;
global using DogQuiz.Pages;
global using DogQuiz.UnitTests.TestData;
global using DogQuiz.UnitTests.TestHelpers;
global using EmbeddedSql;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.Data.Sqlite;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using NSubstitute;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Test names can contain underscores.")]

namespace DogQuiz.UnitTests
{
    public static class ModuleInit
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            VerifyHttp.Initialize();
            VerifyNSubstitute.Initialize();
            VerifyMicrosoftLogging.Initialize();

            VerifierSettings.DontIgnoreEmptyCollections();
            VerifierSettings.DontScrubDateTimes();
            VerifierSettings.IgnoreMembersWithType<HttpContext>();
            VerifierSettings.AddExtraSettings(x => x.DefaultValueHandling = Argon.DefaultValueHandling.Include);
            var exceptionProperties = typeof(Exception).GetProperties().Where(x => x.Name != "Message");
            foreach (var exceptionProperty in exceptionProperties)
            {
                VerifierSettings.IgnoreMember<Exception>(exceptionProperty.Name);
            }

            var pageModelProperties = typeof(PageModel).GetProperties();
            foreach (var pageModelProperty in pageModelProperties)
            {
                VerifierSettings.IgnoreMember<PageModel>(pageModelProperty.Name);
            }

            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }
    }
}
