using Cake.Frosting;
using Yf.Cake.Layers;
using Yf.Cake.Layers.Steps;

namespace Build
{
    public static class Program
    {
        public static int Main(string[] args) => Runner.Run(args);

        public sealed class Initialize : BaseInitialize
        {
            public override string[] AdditionalDirectoriesToClean => new[] { "**/wwwroot" };
        }

        [IsDependentOn(typeof(Initialize))]
        public sealed class RestoreNuGetPackages : BaseRestoreNuGetPackages
        {
        }

        [IsDependentOn(typeof(RestoreNuGetPackages))]
        public sealed class ScanCode : BaseScanCode
        {
        }

        [IsDependentOn(typeof(ScanCode))]
        public sealed class Build : BaseBuild
        {
        }

        [IsDependentOn(typeof(Build))]
        public sealed class RunUnitTests : BaseRunTests
        {
            public override string[] CoverageReportClassFilter => new[] { "-*.Program", "-*.Pages_*" };
        }

        [IsDependentOn(typeof(RunUnitTests))]
        public sealed class RunMutationTests : BaseRunMutationTests
        {
        }

        [IsDependentOn(typeof(RunMutationTests))]
        public sealed class CreatePackage : BasePublish
        {
            public override string Runtime => "win-x86";
        }
    }
}
