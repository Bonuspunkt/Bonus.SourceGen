﻿//HintName: Simple.Module.g.cs
// <auto-generated />
using Autofac;
#nullable enable

namespace Style.New;

partial class Simple : Bonus.SourceGen.IRegisterADelegateModule {
    public static Autofac.Core.IModule DelegateModule { get; } = new Module();
}

file class Module : Autofac.Module {
    protected override void Load(ContainerBuilder builder) {
        builder.Register<Style.New.Simple.Void>(ctx => {
            var actual_delegate = Style.New.Simple._();

            return actual_delegate;
        });
    }
}
