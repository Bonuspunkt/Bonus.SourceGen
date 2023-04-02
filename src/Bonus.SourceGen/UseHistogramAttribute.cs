namespace Bonus.SourceGen;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class UseHistogramAttribute : Attribute {
    public UseHistogramAttribute(string nameofHistogram) {
    }
}
