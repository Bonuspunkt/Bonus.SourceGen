namespace Bonus.SourceGen
{
    public class CombinedTests : UsesVerifyBaseClass {
        [Fact]
        public Task UseAllAttributes() {
            var source = """
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Bonus.SourceGen;

partial class X {
    internal static ActivitySource _activitySource = null;
    internal static Histogram<double> _histogram = null;

    internal delegate void Void();

    [RegisterDelegate]
    [UseActivity(nameof(_activitySource))]
    [UseHistogram(nameof(_histogram))]
    internal static Void _()
    {
        return () => {};
    }
}
""";
            return TestHelper.Compile(source).Validate(Check.Validators);
        }

    }
}
