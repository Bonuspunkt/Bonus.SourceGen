using Autofac.Builder;
using Autofac.Core;
using Bonus.SourceGen;

namespace Autofac;

public static class ContainerBuilderExtension {
    public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        RegisterTypeWithDelegateModule<T>(this ContainerBuilder builder)
        where T : IRegisterADelegateModule {
#if NET7_0_OR_GREATER
        builder.RegisterModule(T.DelegateModule);
#else
#pragma warning disable CS8600,CS8602,CS8604
        // we have generated this code, so there should be no null issues
        var module = (IModule)typeof(T).GetProperty("DelegateModule").GetValue(null);
        builder.RegisterModule(module);
#pragma warning restore CS8600,CS8602,CS8604
#endif
        return builder.RegisterType<T>();
    }
}
