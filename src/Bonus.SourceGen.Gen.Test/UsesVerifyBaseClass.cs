namespace Bonus.SourceGen
{
    [UsesVerify]
    public abstract class UsesVerifyBaseClass {
#if NET48
        static UsesVerifyBaseClass() {
            VerifySourceGenerators.Initialize();
        }
#endif
    }
}
