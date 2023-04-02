namespace Bonus.SourceGen;

public class UseHistogramTests : UsesVerifyBaseClass {

    [Fact]
    public Task Wrapped_Void() {
        var source = """
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

public static class Metrics {
    private static readonly Meter _meter = new Meter("X");
    public static Histogram<double> Histogram = _meter.CreateHistogram<double>("H");
}
partial class X {
    internal delegate void Void();

    [RegisterDelegate]
    [UseHistogram(nameof(Metrics.Histogram))]
    internal static Void _()
    {
        return () => {};
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Wrapped_Return() {
        var source = """
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

partial class X {
    private static Meter _meter = new Meter("X");
    internal static Histogram<double> _histogram = _meter.CreateHistogram<double>("H");

    internal delegate int Number();

    [RegisterDelegate]
    [UseHistogram(nameof(_histogram))]
    internal static Number _()
    {
        return () => 5;
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Wrapped_Task() {
        var source = """
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Bonus.SourceGen;

partial class X {
    private static Meter _meter = new Meter("X");
    internal static Histogram<double> _histogram = _meter.CreateHistogram<double>("H");

    internal delegate Task TaskVoid();

    [RegisterDelegate]
    [UseHistogram(nameof(_histogram))]
    internal static TaskVoid _()
    {
        return () => Task.CompletedTask;
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Wrapped_TaskResult() {
        var source = """
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Bonus.SourceGen;

partial class X {
    private static Meter _meter = new Meter("X");
    internal static Histogram<double> _histogram = _meter.CreateHistogram<double>("H");

    internal delegate Task<int> TaskNumber();

    [RegisterDelegate]
    [UseHistogram(nameof(_histogram))]
    internal static TaskNumber _()
    {
        return () => Task.FromResult(5);
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Histogram_As_Property() {
        var source = """
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Bonus.SourceGen;

public static class Metrics {
    private static readonly Meter _meter = new("MyMeter");

    public static Histogram<double> MyHistogram { get; } = _meter.CreateHistogram<double>("MyHistogram");
}

internal partial class Target {

    internal delegate Task TaskYield();

    [RegisterDelegate]
    [UseHistogram(nameof(Metrics.MyHistogram))]
    internal static TaskYield _() => async () => await Task.Yield();

    private readonly TaskYield _taskYield;
    public Target(TaskYield taskYield) {
        _taskYield = taskYield;
    }

    public Task Work() => _taskYield();
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Parameters() {
        var source = """
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Bonus.SourceGen;

public static class Metrics {
    private static readonly Meter _meter = new("MyMeter");

    public static Histogram<double> MyHistogram { get; } = _meter.CreateHistogram<double>("MyHistogram");
}

namespace This.Place {
    using System;

    public static partial class Registrations {
        internal delegate int GetFullNameLength(Type type);

        [RegisterDelegate]
        [UseHistogram(nameof(Metrics.MyHistogram))]
        internal static GetFullNameLength _() {
            return type => type.FullName.Length;
        }
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

}
