﻿//HintName: Simple.Module.g.cs
// <auto-generated />
using Autofac;
#nullable enable

partial class Simple : Bonus.SourceGen.IRegisterADelegateModule {
    public static Autofac.Core.IModule DelegateModule { get; } = new Module();
}

file class Module : Autofac.Module {
    protected override void Load(ContainerBuilder builder) {
        builder.Register<Simple.Void>(ctx => {
            var actual_delegate = Simple._();

            return actual_delegate;
        });
    }
}
