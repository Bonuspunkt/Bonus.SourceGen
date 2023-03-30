using Autofac;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FluentAssertions;

namespace Bonus.SourceGen.Integration;

public static class Telemetry {
    public static readonly ActivitySource ActivitySource = new("Telemetry");
}

public partial class UseActivityTests {
    internal partial class Target {
        internal delegate void Void();

        [RegisterDelegate]
        [UseActivity(nameof(Telemetry.ActivitySource))]
        internal static Void _() => () => { };

        internal delegate int ReturnOne();

        [RegisterDelegate]
        [UseActivity(nameof(Telemetry.ActivitySource), "one")]
        internal static ReturnOne __() => () => 1;

        private readonly Void _void;
        private readonly ReturnOne _returnOne;

        public Target(Void @void, ReturnOne returnOne) {
            _void = @void;
            _returnOne = returnOne;
        }

        public void Run() => _void();
        public int Calculate() => _returnOne();
    }

    private enum Action {
        Start,
        Stop
    }
    private class Log {
        public Log(Action action, Activity activity) {
            Action = action;
            Activity = activity;
        }
        public Activity Activity { get; }
        public Action Action { get; }
    }

    [Fact]
    public void VerifyItWorks() {
        var logs = new List<Log>();

        var listener = new ActivityListener();
        listener.ShouldListenTo = _ => true;
        listener.ActivityStarted = activity => logs.Add(new Log(Action.Start,activity));
        listener.ActivityStopped = activity => logs.Add(new Log(Action.Stop,activity));
        listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
            ActivitySamplingResult.AllDataAndRecorded;
        ActivitySource.AddActivityListener(listener);

        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterTypeWithDelegateModule<Target>();

        var container = containerBuilder.Build();
        var target = container.Resolve<Target>();

        Telemetry.ActivitySource.HasListeners().Should().BeTrue();

        target.Run();

        logs.Should().SatisfyRespectively(
            log => {
                log.Action.Should().Be(Action.Start);
                log.Activity.DisplayName.Should().Be("Bonus.SourceGen.Integration.UseActivityTests.Target.Void");
            },
            log => {
                log.Action.Should().Be(Action.Stop);
                log.Activity.DisplayName.Should().Be("Bonus.SourceGen.Integration.UseActivityTests.Target.Void");
            }
        );

        logs.Clear();

        target.Calculate();

        logs.Should().SatisfyRespectively(
            log => {
                log.Action.Should().Be(Action.Start);
                log.Activity.DisplayName.Should().Be("one");
            },
            log => {
                log.Action.Should().Be(Action.Stop);
                log.Activity.DisplayName.Should().Be("one");
            }
        );
    }
}
