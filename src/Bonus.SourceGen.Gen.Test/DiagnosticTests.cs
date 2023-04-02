namespace Bonus.SourceGen;

public class DiagnosticTests : UsesVerifyBaseClass {

    [Fact]
    public Task BSG001_TypeNotPartial() {
        var source = """
namespace Bonus.SourceGen;

public class ClassNotPartial {
    internal delegate void Void();

    [RegisterDelegate]
    internal static Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG001"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }

    [Fact]
    public Task BSG002_NotStatic() {
        var source = """
namespace Bonus.SourceGen;

public partial class NotStatic {
    internal delegate void Void();

    [RegisterDelegate]
    internal Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG002"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }

    [Fact]
    public Task BSG004_DoesNotReturnDelegate() {
        var source = """
using Bonus.SourceGen;

public partial class Class {
    [RegisterDelegate]
    internal static int _() => 5;
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG004"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }

    [Fact]
    public Task BSG666_SelfAsDelegateParameter() {
        var source = """
namespace Bonus.SourceGen;

internal partial class SelfAsParameter
{
    internal delegate void DoSomething(SelfAsParameter self);

    [RegisterDelegate]
    internal static DoSomething _()
    {
        return self => {};
    }
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG666"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }

    [Fact]
    public Task BSG667_DelegateShouldBeInternal() {
        var source = """
namespace Bonus.SourceGen;

partial class DelegateShouldBeInternal {
    public delegate void Void();

    [RegisterDelegate]
    internal static Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG667"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }
}
