namespace Bonus.SourceGen;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class UseActivityAttribute : Attribute {
    public UseActivityAttribute(string nameofActivitySource, string? activityName = default) {
    }
}
