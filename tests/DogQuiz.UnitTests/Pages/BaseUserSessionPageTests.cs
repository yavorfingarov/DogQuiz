using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace DogQuiz.UnitTests.Pages
{
    public sealed class BaseUserSessionPageTests : PageTestBase<TestPageModel>
    {
        private readonly PageHandlerExecutingContext _PageHandlerExecutingContext;

        public BaseUserSessionPageTests()
        {
            _PageHandlerExecutingContext = new PageHandlerExecutingContext(
                PageModel.PageContext,
                new List<IFilterMetadata>(),
                new HandlerMethodDescriptor(),
                new Dictionary<string, object?>(),
                new object());
        }

        [Fact]
        public void OnPageHandlerExecuting()
        {
            PageModel.OnPageHandlerExecuting(_PageHandlerExecutingContext);

            Assert.Null(_PageHandlerExecutingContext.Result);
        }

        [Fact]
        public Task OnPageHandlerExecuting_InvalidModelState()
        {
            PageModel.ModelState.AddModelError("foo", "bar");

            PageModel.OnPageHandlerExecuting(_PageHandlerExecutingContext);

            return Verify(_PageHandlerExecutingContext.Result);
        }

        [Fact]
        public Task BaseTypes()
        {
            var pageTypes = typeof(Program).Assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(PageModel)))
                .Select(x => $"{x.Name} : {x.BaseType!.Name}");

            return Verify(pageTypes);
        }

        protected override TestPageModel CreatePageModel()
        {
            return new TestPageModel(Db, Sql, IdGenerator, TimeProvider, ConfigurationOptions);
        }
    }

    public sealed class TestPageModel : BaseUserSessionPageModel
    {
        public TestPageModel(
            IDbConnection db,
            ISql sql,
            IIdGenerator idGenerator,
            TimeProvider timeProvider,
            IOptions<Configuration> configuration)
            : base(db, sql, idGenerator, timeProvider, configuration)
        {
        }
    }
}
