using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Razor.Hosting;

namespace DogQuiz.UnitTests
{
    public sealed class AssemblyTests
    {
        private readonly Assembly _Assembly = typeof(Program).Assembly;

        [Fact]
        public Task References()
        {
            var assemblies = _Assembly
                .GetReferencedAssemblies()
                .Where(x => x.Name != "netstandard")
                .Select(x => x.Name)
                .OrderBy(x => x);

            return Verify(assemblies);
        }

        [Fact]
        public Task NonPublicTypes()
        {
            return Verify(GetTypes(x => !x.IsPublic || (!x.IsPublic && x.IsNested)));
        }

        [Fact]
        public Task NestedTypes()
        {
            return Verify(GetTypes(x => x.IsNested));
        }

        [Fact]
        public Task ValueTypes()
        {
            return Verify(GetTypes(x => x.IsValueType));
        }

        [Fact]
        public Task Interfaces()
        {
            return Verify(GetTypes(x => x.IsInterface));
        }

        [Fact]
        public Task Abstract()
        {
            return Verify(GetTypes(x => x.IsClass && x.IsAbstract && !x.IsSealed));
        }

        [Fact]
        public Task Sealed()
        {
            return Verify(GetTypes(x => x.IsClass && !x.IsAbstract && x.IsSealed));
        }

        [Fact]
        public Task Open()
        {
            return Verify(GetTypes(x => x.IsClass && !x.IsAbstract && !x.IsSealed));
        }

        [Fact]
        public Task Static()
        {
            return Verify(GetTypes(x => x.IsClass && x.IsAbstract && x.IsSealed));
        }

        private IEnumerable<string> GetTypes(Func<Type, bool> predicate)
        {
            return _Assembly.GetTypes()
                .Where(x => !x.IsDefined(typeof(CompilerGeneratedAttribute)))
                .Where(x => !x.IsDefined(typeof(RazorCompiledItemMetadataAttribute)))
                .Where(x => !x.IsAssignableTo(typeof(IAsyncStateMachine)))
                .Where(x => !x.FullName!.StartsWith("Coverlet.Core", StringComparison.InvariantCulture))
                .Where(predicate)
                .Select(x => x.FullName!)
                .OrderBy(x => x);
        }
    }
}
