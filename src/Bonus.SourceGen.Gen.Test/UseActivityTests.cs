namespace Bonus.SourceGen; 

public class UseActivityTests : UsesVerifyBaseClass {
    [Fact]
    public Task Basic() {
        var source = """
using System.Diagnostics;
using Bonus.SourceGen;

public static class Telemetry {
    public static ActivitySource ActivitySource { get; } = new("Telemetry");
}
partial class X {
    internal delegate void Void();

    [RegisterDelegate]
    [UseActivity(nameof(Telemetry.ActivitySource))]
    internal static Void _() {
        return () => {};
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Named_Activity() {
        var source = """
using System.Diagnostics;
using Bonus.SourceGen;

public static class Telemetry {
    public static readonly ActivitySource ActivitySource = new("Telemetry");
}
partial class X {
    internal delegate void Void();

    [RegisterDelegate]
    [UseActivity(nameof(Telemetry.ActivitySource), "thisIsWorking")]
    internal static Void _() {
        return () => {};
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }
}
