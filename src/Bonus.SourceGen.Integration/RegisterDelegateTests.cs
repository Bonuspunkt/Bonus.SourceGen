using Autofac;
using FluentAssertions;

namespace Bonus.SourceGen.Integration;

public partial class RegisterDelegateTests {

    internal partial class Target {

        private static int _perDependencyCreated;
        public static int PerDependencyCreated => _perDependencyCreated;

        internal delegate void PerDependency();

        [RegisterDelegate]
        internal static PerDependency _PerDependency() {
            Interlocked.Increment(ref _perDependencyCreated);
            return () => { };
        }


        public Target(PerDependency perDependency) { }
    }

    [Fact]
    public void VerifyItWorks() {
        var builder = new ContainerBuilder();
        builder.RegisterTypeWithDelegateModule<Target>();

        var container = builder.Build();
        container.Resolve<Target>();

        Target.PerDependencyCreated.Should().Be(1);

        container.Resolve<Target>();

        Target.PerDependencyCreated.Should().Be(2);
    }
}
