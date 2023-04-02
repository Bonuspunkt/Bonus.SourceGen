namespace Bonus.SourceGen;

internal class UseHistogramGenerator : IGenerateSourceCode {
    public static string Attribute => "Bonus.SourceGen.UseHistogramAttribute";

    private const string Histogram = "System.Diagnostics.Metrics.Histogram`1";
    private const string Double = "System.Double";

    public static UseHistogramGenerator? Create(GenerateSourceContext context) {

        var histogramSymbol = FindHistogram(context.AttributeSyntax, context);
        if (histogramSymbol == default) return null;

        var returnInfo = MethodReturnInfo.Create(context.DelegateSymbol.DelegateInvokeMethod.ReturnType, context.Compilation);
        return new UseHistogramGenerator(context.DelegateSymbol.DelegateInvokeMethod, histogramSymbol, returnInfo);
    }

    private static ISymbol? FindHistogram(AttributeSyntax attributeSyntax, GenerateSourceContext context) {
        if (attributeSyntax.ArgumentList?.Arguments.Count != 1) return null;

        var histogramSymbol = context.Compilation.GetTypeByMetadataName(Histogram);
        var doubleSymbol = context.Compilation.GetTypeByMetadataName(Double);

        var firstArgument = attributeSyntax.ArgumentList.Arguments[0];
        if (firstArgument.Expression is InvocationExpressionSyntax {
            Expression: IdentifierNameSyntax { Identifier.Text: "nameof" }
        } invocationExpression) {
            var argument = invocationExpression.ArgumentList.Arguments[0].Expression;
            var target = context.SemanticModel.GetSymbolInfo(argument).Symbol;

            var typeSymbol = target switch {
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                _ => null
            } as INamedTypeSymbol;

            if (typeSymbol == null) return null;

            if (SymbolEqualityComparer.Default.Equals(typeSymbol.ConstructedFrom, histogramSymbol) &&
                SymbolEqualityComparer.Default.Equals(typeSymbol.TypeArguments[0], doubleSymbol)) {
                return target;
            }
            context.ReportDiagnostic(Issues.MustBeHistogramDouble(firstArgument));
            return null;
        }

        context.ReportDiagnostic(Issues.UseNameOfExpression(firstArgument));
        return null;
    }

    private readonly IMethodSymbol _invokeMethodSymbol;
    private readonly ISymbol _histogramSymbol;
    private readonly MethodReturnInfo _methodReturnInfo;

    private UseHistogramGenerator(IMethodSymbol invokeMethodSymbol, ISymbol histogramSymbol,
        MethodReturnInfo methodReturnInfo) {
        _invokeMethodSymbol = invokeMethodSymbol;
        _histogramSymbol = histogramSymbol;
        _methodReturnInfo = methodReturnInfo;
    }

    public int SortNo => 1_000;
    private const string FunctionName = "wrapped_with_histogram";

    public void Emit(IndentedStringBuilder builder, ref string? returnMethod) {

        var parameterList = _invokeMethodSymbol.Parameters.Select(parameter => $"{parameter.Type} {parameter.Name}");
        var argumentList = _invokeMethodSymbol.Parameters.Select(parameter => parameter.Name);


        builder.AppendLine("[System.Diagnostics.DebuggerStepThrough]");
        builder.Append($"{_methodReturnInfo.Async}{_invokeMethodSymbol.ReturnType} ");
        builder.Append(FunctionName);
        builder.AppendLine($"({string.Join(", ", parameterList)}) {{");
        builder.IncrementIndent();
        builder.AppendLine($"using var _ = Bonus.SourceGen.HistogramEntry.Record({_histogramSymbol});");
        builder.AppendLine($"{_methodReturnInfo.Return}{returnMethod}({string.Join(", ", argumentList)});");
        builder.DecrementIndent();
        builder.AppendLine("}");

        returnMethod = FunctionName;
    }
}
