using Autofac.Builder;
using Autofac.Core;
using Bonus.SourceGen;

namespace Autofac
{
    public static class ContainerBuilderExtension {
        public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterTypeWithDelegateModule<T>(this ContainerBuilder builder)
            where T : IRegisterADelegateModule {
#if NET7_0_OR_GREATER
            builder.RegisterModule(T.DelegateModule);
#else
            var module = (IModule)typeof(T).GetProperty("DelegateModule").GetValue(null);
            builder.RegisterModule(module);
#endif
            return builder.RegisterType<T>();
        }
    }
}
