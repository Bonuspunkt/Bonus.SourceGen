﻿//HintName: Simple.Module.g.cs
// <auto-generated />
#pragma warning disable 8019
using Autofac;
using Bonus.SourceGen;
using System.Diagnostics;
using System.Threading.Tasks;
#pragma warning restore 8019
#nullable enable

namespace Style.New;
partial class Simple : IRegisterADelegateModule {
    public static Autofac.Core.IModule DelegateModule { get; } = new Module();
}

file class Module : Autofac.Module {
    protected override void Load(ContainerBuilder builder) {
        builder.Register<Style.New.Simple.Void>(ctx => {
            return Simple._();
        });
    }
}
