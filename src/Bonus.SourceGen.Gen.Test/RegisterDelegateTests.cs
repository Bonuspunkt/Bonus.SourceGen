using System.Diagnostics;

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
    public Task Static_Class_Exposing_Delegates() {
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

    [Fact]
    public Task ResolveParameter() {
        var source = """
using Bonus.SourceGen;

namespace Somewhere.Else {
    internal delegate int ReturnOne();
}

namespace This.Place {
    using Somewhere.Else;

    public static partial class Registrations
    {
        internal delegate int GetTwo();

        [RegisterDelegate]
        internal static GetTwo _(ReturnOne returnOne)
        {
            return () => returnOne() + returnOne();
        }
    }
}
""";
        return TestHelper.Compile(source).Validate(Check.Validators);
    }

}
