namespace Bonus.SourceGen; 

internal class RegisterDelegateGenerator: IGenerateSourceCode {
    public static string Attribute => "Bonus.SourceGen.RegisterDelegateAttribute";
    public static RegisterDelegateGenerator? Create(GenerateSourceContext context) {

        if (!Validate(context)) return null;

        return new RegisterDelegateGenerator(context.MethodSymbol);
    }

    private static bool Validate(GenerateSourceContext context) {
        if (context.DelegateSymbol.DeclaringSyntaxReferences.Length <= 0) {
            return true;
        }

        var typeDeclarationSyntax = context.MethodDeclarationSyntax.Parent;
        var syntaxReference = context.DelegateSymbol.DeclaringSyntaxReferences.First();
        var delegateDeclarationSyntax = (DelegateDeclarationSyntax)syntaxReference.GetSyntax();

        if (delegateDeclarationSyntax.Parent == typeDeclarationSyntax &&
            !delegateDeclarationSyntax.Modifiers.Has(SyntaxKind.InternalKeyword)) {
            context.ReportDiagnostic(Issues.DelegateShouldBeInternal(delegateDeclarationSyntax));
        }

        var delegateParameterSymbols = delegateDeclarationSyntax.ParameterList.Parameters
            .Select(parameter => context.SemanticModel.GetSymbolInfo(parameter.Type!).Symbol)
            .OfType<ITypeSymbol>()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);

        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax);
        if (delegateParameterSymbols.Contains(typeSymbol)) {
            context.ReportDiagnostic(Issues.SelfAsDelegateParameter(delegateDeclarationSyntax));
        }

        return true;
    }

    private readonly IMethodSymbol _methodSymbol;

    private RegisterDelegateGenerator(IMethodSymbol methodSymbol) {
        _methodSymbol = methodSymbol;
    }

    public int SortNo => 0;
    private const string FunctionName = "actual_delegate";

    public void Emit(IndentedStringBuilder builder, ref string? returnMethod) {

        foreach (var parameter in _methodSymbol.Parameters) {
            var type = parameter.Type;
            var name = parameter.Name;
            builder.AppendLine($"var {name} = ctx.Resolve<{type}>();");
        }

        if (!_methodSymbol.Parameters.IsEmpty) {
            builder.AppendLine();
        }

        var parameterNames = _methodSymbol.Parameters.Select(parameter => parameter.Name);

        builder
            .Append($"var {FunctionName} = {_methodSymbol.ContainingType}.{_methodSymbol.Name}")
            .Append($"({string.Join(".", parameterNames)});")
            .AppendLine();

        returnMethod = FunctionName;
    }
}
