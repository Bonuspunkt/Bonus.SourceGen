namespace Bonus.SourceGen;

public class RegisterDelegateTests : UsesVerifyBaseClass {

    [Fact]
    public Task NoNamespace() {
        var source = """
using Bonus.SourceGen;

public partial class Simple
{
    internal delegate void Void();

    [RegisterDelegate]
    internal static Void _()
    {
        return () => {};
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Nested_Using() {
        const string source = """
namespace Style.Classic
{
    using System.Threading.Tasks;
    using Bonus.SourceGen;

    public partial class Simple
    {
        internal delegate Task ReturnTask();

        [RegisterDelegate]
        internal static ReturnTask _()
        {
            return () => Task.CompletedTask;
        }
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }


    [Fact]
    public Task Namespace_Classic() {
        var source = """
using Bonus.SourceGen;

namespace Style.Classic
{
    public partial class Simple
    {
        internal delegate void Void();

        [RegisterDelegate]
        internal static Void _()
        {
            return () => {};
        }
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

    [Fact]
    public Task Namespace_New() {
        var source = """
using Bonus.SourceGen;

namespace Style.New;

public partial class Simple
{
    internal delegate void Void();

    [RegisterDelegate]
    internal static Void _()
    {
        return () => {};
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }



    [Fact]
    public Task Exposing_Delegates() {
        var source = """
using Bonus.SourceGen;

public delegate int GetFive();

public static partial class Registrations
{
    [RegisterDelegate]
    internal static GetFive _()
    {
        return () => 5;
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

}
