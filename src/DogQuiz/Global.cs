global using System.Data;
global using Dapper;
global using DogQuiz.Common;
global using EmbeddedSql;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1848:Use the LoggerMessage delegates",
    Justification = "Logging performance is not a concern.")]

[assembly: SuppressMessage(
    "Performance", "CA1861:Avoid constant arrays as arguments",
    Justification = "The invocation only runs once.",
    Scope = "member",
    Target = "~M:DogQuiz.Program.Run(Microsoft.AspNetCore.Builder.WebApplicationBuilder)")]

[assembly: SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "Coupling in this context is acceptable.",
    Scope = "type",
    Target = "T:DogQuiz.Program")]
