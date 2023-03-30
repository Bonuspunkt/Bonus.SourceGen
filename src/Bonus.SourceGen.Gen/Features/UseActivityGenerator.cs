using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bonus.SourceGen;

internal class UseActivityGenerator : IGenerateSourceCode {
    public static string Attribute => "Bonus.SourceGen.UseActivityAttribute";
    private const string ActivitySource = "System.Diagnostics.ActivitySource";

    public static UseActivityGenerator? Create(GenerateSourceContext context) {

        var activitySourceSymbol = FindActivitySource(context.AttributeSyntax, context);
        if (activitySourceSymbol == default) return null;

        var activityName = context.AttributeSyntax.ArgumentList.Arguments.Count == 2
            ? context.AttributeSyntax.ArgumentList.Arguments[1].Expression.ToString()
            : $"\"{context.DelegateSymbol}\"";

        var returnInfo = MethodReturnInfo.Create(context.DelegateSymbol.DelegateInvokeMethod.ReturnType, context.Compilation);
        return new UseActivityGenerator(
            context.DelegateSymbol.DelegateInvokeMethod,
            activitySourceSymbol,
            activityName,
            returnInfo
        );
    }

    private static ISymbol? FindActivitySource(AttributeSyntax attributeSyntax, GenerateSourceContext context) {
        if (attributeSyntax.ArgumentList?.Arguments.Count == 0) return null;

        var activitySymbol = context.Compilation.GetTypeByMetadataName(ActivitySource);

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

            if (SymbolEqualityComparer.Default.Equals(typeSymbol, activitySymbol)) {
                return target;
            }
            context.ReportDiagnostic(Issues.MustBeActivitySource(firstArgument));
            return null;
        }

        context.ReportDiagnostic(Issues.UseNameOfExpression(firstArgument));
        return null;
    }

    private readonly IMethodSymbol _invokeMethodSymbol;
    private readonly ISymbol _activitySourceSymbol;
    private readonly string _activityName;
    private readonly MethodReturnInfo _methodReturnInfo;

    private UseActivityGenerator(IMethodSymbol invokeMethodSymbol, ISymbol activitySourceSymbol,
        string activityName, MethodReturnInfo methodReturnInfo) {
        _invokeMethodSymbol = invokeMethodSymbol;
        _activitySourceSymbol = activitySourceSymbol;
        _activityName = activityName;
        _methodReturnInfo = methodReturnInfo;
    }

    public int SortNo => 2_000;
    private const string FunctionName = "wrapped_with_activity";

    public void Emit(IndentedStringBuilder builder, ref string? returnMethod) {

        var parameterList = _invokeMethodSymbol.Parameters.Select(parameter => $"{parameter.Type} {parameter.Name}");
        var argumentList = _invokeMethodSymbol.Parameters.Select(parameter => parameter.Name);

        builder.AppendLine("[System.Diagnostics.DebuggerStepThrough]");
        builder.Append($"{_methodReturnInfo.Async}{_invokeMethodSymbol.ReturnType} ");
        builder.Append(FunctionName);
        builder.AppendLine($"({string.Join(", ", parameterList)}) {{");
        builder.IncrementIndent();
        builder.AppendLine($"using var _ = {_activitySourceSymbol}.StartActivity({_activityName});");
        builder.AppendLine($"{_methodReturnInfo.Return}{returnMethod}({string.Join(", ", argumentList)});");
        builder.DecrementIndent();
        builder.AppendLine("}");

        returnMethod = FunctionName;
    }
}
