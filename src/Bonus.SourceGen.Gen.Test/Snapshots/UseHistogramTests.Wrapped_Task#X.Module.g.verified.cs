﻿//HintName: X.Module.g.cs
// <auto-generated />
using Autofac;
#nullable enable

partial class X : Bonus.SourceGen.IRegisterADelegateModule {
    public static Autofac.Core.IModule DelegateModule { get; } = new Module();
}

file class Module : Autofac.Module {
    protected override void Load(ContainerBuilder builder) {
        builder.Register<X.TaskVoid>(ctx => {
            var actual_delegate = X._();

            [System.Diagnostics.DebuggerStepThrough]
            async System.Threading.Tasks.Task wrapped_with_histogram() {
                using var _ = Bonus.SourceGen.HistogramEntry.Record(X._histogram);
                await actual_delegate();
            }

            return wrapped_with_histogram;
        });
    }
}
