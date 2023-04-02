using System.Diagnostics.Metrics;

namespace Bonus.SourceGen;

internal class Recording : IDisposable {
    private readonly Histogram<double> _histogram;
    private readonly Func<DateTimeOffset> _getNow;
    private readonly DateTimeOffset _start;

#if !NET7_0_OR_GREATER
    // NOTE: see https://learn.microsoft.com/en-us/dotnet/api/system.timespan.ticks?view=net-7.0#remarks
    private const double NanosecondsPerTick = 100;
    private const double TicksPerMicrosecond = 10;
#endif

    public Recording(Histogram<double> histogram, Func<DateTimeOffset>? getNow = default) {
        _histogram = histogram;
        _getNow = getNow ?? (() => DateTimeOffset.Now);
        _start = _getNow();
    }
    public void Dispose() {
        var duration = _getNow() - _start;

        switch (_histogram.Unit) {
            case "s":
                _histogram.Record(duration.TotalSeconds);
                break;
#if NET7_0_OR_GREATER
            case "µs":
                _histogram.Record(duration.TotalMicroseconds);
                break;
            case "ns":
                _histogram.Record(duration.TotalNanoseconds);
                break;
#else
            case "µs":
                _histogram.Record(duration.Ticks / TicksPerMicrosecond);
                break;
            case "ns":
                _histogram.Record(duration.Ticks * NanosecondsPerTick);
                break;
#endif
            case "ms":
            default:
                _histogram.Record(duration.TotalMilliseconds);
                break;
        }
    }
}

public static class HistogramEntry {
    public static IDisposable Record(Histogram<double> histogram) => new Recording(histogram);
}
