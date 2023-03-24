# Bonus.SourceGen

We all have seen this code

```csharp
public class MyClass {

    private MyService _myService;

    internal MyClass(MyService myService) {
        _myService = myService;
    }

    public void DoWork() {
        // stuff
        _myService.Method();
        // more stuff
    }
}
```
to test it, we usually go ahead and introduce `IMyService` and derive `MyService` from it.

How about the following approach
```csharp
public class MyClass {

    internal delegate void MyServiceMethod();

    private MyServiceMethod _myServiceMethod;

    internal MyClass(MyServiceMethod myServiceMethod) {
        _myServiceMethod = myServiceMethod;
    }

    public void DoWork() {
        // stuff
        _myServiceMethod();
        // more stuff
    }
}
```
the test would look like
```csharp
public class MyClassTest {
    [Fact]
    public void Test() {
        var wasCalled = false;
        var target = new MyClass(() => { wasCalled = true; });
        target.DoWork();
        wasCalled.Should().BeTrue();
    }
}
```
and we need to register the delegate
```csharp
public class MyClass {
    internal class Module : Autofac.Module {
        protected override void Load(ContainerBuilder builder) {
            builder.Register<MyServiceMethod>(ctx => {
                var myService = ctx.Resolve<MyService>();

                void MyServiceMethod() => myService.Method();
                return MyServiceMethod;
            });
        }
    }
}
```

neat, but the delegate is not really testable.

well fair point, we can fix this

```csharp
public class MyClass {
    internal class Module : Autofac.Module {
        protected override void Load(ContainerBuilder builder) {
            builder.Register<MyServiceMethod>(ctx => {
                var myService = ctx.Resolve<MyService>();

                return RegisterMyServiceMethod(service);
            });
        }
    }

    internal static MyServiceMethod RegisterMyServiceMethod(MyService) {
        void MyServiceMethod() => myService.Method();
        return MyServiceMethod;
    }
}
```
ok, but i don't want to write the code for the autofac module, can't we solve this with Source Generators?

yes we can!

```csharp
public partial class MyClass {

    [RegisterDelegate]
    internal static ServiceMethod RegisterMyServiceMethod(MyService) {
        void MyServiceMethod() => myService.Method();
        return MyServiceMethod;
    }

    //...
}
```
and we generate
```csharp
partial class MyClass {

    internal class Module : Autofac.Module {
        protected override void Load(ContainerBuilder builder) {
            builder.Register<MyClass.ServiceMethod>(ctx => {
                var service = ctx.Resolve<IService>();

                return RegisterMyServiceMethod(service);
            });
        }
    }
}
```

but wait, we can go further.
We want metrics collected via `Histogram` and we don't want to touch `MyService`.

How about
```csharp
internal static class Telemetry {
    private static readonly _meter = new Meter("myApp");
    public static Histogram<long> MyServiceMethod { get; } = _meter.CreateHistogram<long>("MyService.Method");
}

public partial class MyClass {

    private static readonly Histogram<long> _histogram = Telemetry.MyServiceMethod;

    [RegisterDelegate]
    [UseHistogram(nameof(_histogram)]
    internal static ServiceMethod RegisterMyServiceMethod(MyService myService) {
        //...
}
```
and we generate
```csharp
partial class MyClass {

    internal class Module : Autofac.Module {
        protected override void Load(ContainerBuilder builder) {
            builder.Register<MyClass.ServiceMethod>(ctx => {
                var service = ctx.Resolve<IService>();

                var method = RegisterMyServiceMethod(service);

                [DebuggerStepThrough]
                void WrappedMethod() {
                    using var _ = HistogramEntry.Record(_histogram);
                    method();
                }
                return WrappedMethod;
            });
        }
    }
}
```
\* see `src/Bonus.SourceGen.Attributes/HistogramEntry.cs` for `HistogramEntry` details

ok, but what about tracing?

```csharp
public partial class MyClass {
    private static readonly ActivitySource _activitySouce = //...

    [RegisterDelegate]
    [UseActivity(nameof(_activitySource)]
    [UseHistogram(nameof(_histogram)]
    internal static ServiceMethod RegisterMyServiceMethod(MyService service) {
        //...
}
```
and we generate
```csharp
partial class MyClass {

    internal class Module : Autofac.Module {
        protected override void Load(ContainerBuilder builder) {
            builder.Register<MyClass.ServiceMethod>(ctx => {
                var myService = ctx.Resolve<myService>();

                var method = RegisterMyServiceMethod(myService);

                [DebuggerStepThrough]
                void WrappedMethod() {
                    using var _ = _activitySource.StartActivity(nameof(MyClass) + "." + nameof(MyServiceMethod));
                    using var _ = HistogramEntry.Record(_histogram);
                    method();
                }
                return WrappedMethod;
            });
        }
    }
}
```

