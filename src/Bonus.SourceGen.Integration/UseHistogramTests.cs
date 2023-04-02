using System.Diagnostics.Metrics;
using Autofac;
using FluentAssertions;

namespace Bonus.SourceGen.Integration; 

public class Collector<T> : IDisposable where T : struct {
    public static Collector<T> Create(Instrument<T> instrument) {
        var collector = new Collector<T>(instrument);
        collector.Start();
        return collector;
    }

    private readonly MeterListener _listener;
    private readonly List<T> _values = new();

    private Collector(Instrument instrument) {
        _listener = new MeterListener();
        _listener.SetMeasurementEventCallback<T>((_, measurement, _, _) => {
            _values.Add(measurement);
        });
        _listener.EnableMeasurementEvents(instrument);
    }

    private void Start() {
        _listener.Start();
    }

    public IEnumerable<T> Values => _values;

    public void Dispose()
    {
        _listener.Dispose();
    }
}

public static class Metrics {
    private static readonly Meter _meter = new("MyMeter");

    public static Histogram<double> MyHistogram { get; } = _meter.CreateHistogram<double>("MyHistogram", unit:"ns");
}

public partial class UseHistogramTests {

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

    [Fact]
    public async Task VerifyItWorks() {
        using var collector = Collector<double>.Create(Metrics.MyHistogram);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterTypeWithDelegateModule<Target>();

        var container = containerBuilder.Build();
        var target = container.Resolve<Target>();

        await target.Work();

        collector.Values.Should().HaveCount(1);
    }
}
