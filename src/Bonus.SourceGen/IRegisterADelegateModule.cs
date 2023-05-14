namespace Bonus.SourceGen;

public interface IRegisterADelegateModule {
#if NET7_0_OR_GREATER
    static abstract Autofac.Core.IModule DelegateModule { get; }
#endif
}
