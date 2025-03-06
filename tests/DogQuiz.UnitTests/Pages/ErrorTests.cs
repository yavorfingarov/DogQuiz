using System.Diagnostics;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;

namespace DogQuiz.UnitTests.Pages
{
    public sealed class ErrorTests : PageTestBase<ErrorModel>
    {
        protected override bool HasFeatureCollection => true;

        private readonly IExceptionHandlerPathFeature _ExceptionHandlerPathFeature;
        private readonly IStatusCodeReExecuteFeature _StatusCodeReExecuteFeature;
        private readonly string _AntiforgeryToken;

        public ErrorTests()
        {
            PageModel.Request.Path = "/request/path";
            PageModel.HttpContext.Features.Get<IExceptionHandlerPathFeature>().Returns(_ => null);
            PageModel.HttpContext.Features.Get<IStatusCodeReExecuteFeature>().Returns(_ => null);
            _ExceptionHandlerPathFeature = Substitute.For<IExceptionHandlerPathFeature>();
            _ExceptionHandlerPathFeature.Path.Returns("/exception/path");
            _StatusCodeReExecuteFeature = Substitute.For<IStatusCodeReExecuteFeature>();
            _StatusCodeReExecuteFeature.OriginalPath.Returns("/status-code-re-execute/path");
            _AntiforgeryToken = "antiforgeryToken";
        }

        [Fact]
        public void IgnoreAntiforgeryToken()
        {
            var ignoresAntiforgeryToken = PageModel.GetType()
                .IsDefined(typeof(IgnoreAntiforgeryTokenAttribute), true);

            Assert.True(ignoresAntiforgeryToken);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task Ok(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status200OK;

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task BadRequest(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status400BadRequest;
            PageModel.HttpContext.Features.Get<IStatusCodeReExecuteFeature>().Returns(_StatusCodeReExecuteFeature);
            AddCookie(Configuration.AntiforgeryTokenCookieName, _AntiforgeryToken);
            AddCookie(Configuration.UserSessionIdCookieName, "testUserSessionId");

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task BadRequest_NoCookies(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status400BadRequest;
            PageModel.HttpContext.Features.Get<IStatusCodeReExecuteFeature>().Returns(_StatusCodeReExecuteFeature);

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task NotFound(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status404NotFound;
            PageModel.HttpContext.Features.Get<IStatusCodeReExecuteFeature>().Returns(_StatusCodeReExecuteFeature);
            AddCookie(Configuration.AntiforgeryTokenCookieName, _AntiforgeryToken);
            AddCookie(Configuration.UserSessionIdCookieName, "testUserSessionId");

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task TooManyRequests(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            PageModel.HttpContext.Features.Get<IStatusCodeReExecuteFeature>().Returns(_StatusCodeReExecuteFeature);

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task ImATeapot(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status418ImATeapot;
            PageModel.HttpContext.Features.Get<IStatusCodeReExecuteFeature>().Returns(_StatusCodeReExecuteFeature);

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public Task InternalServerError(string httpMethod)
        {
            PageModel.Request.Method = httpMethod;
            PageModel.Response.StatusCode = StatusCodes.Status500InternalServerError;
            PageModel.HttpContext.Features.Get<IExceptionHandlerPathFeature>().Returns(_ExceptionHandlerPathFeature);
            AddCookie(Configuration.AntiforgeryTokenCookieName, _AntiforgeryToken);

            Invoke(httpMethod);

            return Verify(PageModel)
                .UseParameters(httpMethod);
        }

        private void Invoke(string httpMethod)
        {
            if (httpMethod == "GET")
            {
                PageModel.OnGet();
            }
            else if (httpMethod == "POST")
            {
                PageModel.OnPost();
            }
            else
            {
                throw new UnreachableException();
            }
        }

        protected override ErrorModel CreatePageModel()
        {
            var logger = RecordingProvider.CreateLogger<ErrorModel>();

            return new ErrorModel(logger);
        }
    }
}
