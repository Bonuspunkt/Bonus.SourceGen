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
    public Task BSG003_MissingRegisterDelegate() {
        var source = """
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

public partial class Class {
    internal Histogram<double> _histogram = null;
    internal delegate void Void();

    [UseHistogram(nameof(_histogram)]
    internal static Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG003"); });

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
    public Task BSG100_UseNameOfExpression() {
        var source = """
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

public partial class Class {
    internal Histogram<double> _histogram = null;
    internal delegate void Void();

    [RegisterDelegate]
    [UseHistogram("_histogram"]
    internal static Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG100"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }

    [Fact]
    public Task BSG101_MustBeHistogramDouble() {
        var source = """
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

public partial class Class {
    internal Histogram<long> _histogram = null;
    internal delegate void Void();

    [RegisterDelegate]
    [UseHistogram(nameof(_histogram)]
    internal static Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG101"); });

            return Task.CompletedTask;
        }, Check.Snapshots);
    }

    [Fact]
    public Task BSG200_MustBeActivitySource() {
        var source = """
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

public partial class Class {
    internal string _activitySource = null;
    internal delegate void Void();

    [RegisterDelegate]
    [UseActivity(nameof(_activitySource)]
    internal static Void _() => () => {}
}
""";
        return TestHelper.Compile(source).Validate(data => {
            data.Diagnostics.Should().SatisfyRespectively(diagnostic => { diagnostic.Id.Should().Be("BSG200"); });

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
