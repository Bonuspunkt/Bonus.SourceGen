namespace Bonus.SourceGen;

internal static class ModuleInitializer {
#if !NET48
    [ModuleInitializer]
    public static void Init() {
        VerifySourceGenerators.Initialize();
    }
#endif
}
