namespace Bonus.SourceGen;

internal interface IGenerateSourceCode {
    int SortNo { get; }
    void Emit(IndentedStringBuilder builder, ref string? returnMethod);
}

internal delegate IGenerateSourceCode? GenerateSourceCodeFactory(GenerateSourceContext context);

internal record GenerateSourceContext {
    public required Compilation Compilation { get; init; }
    public required SemanticModel SemanticModel { get; init; }
    public required Action<Diagnostic> ReportDiagnostic { get; init; }
    public required AttributeSyntax AttributeSyntax { get; init; }
    public required MethodDeclarationSyntax MethodDeclarationSyntax { get; init; }
    public required IMethodSymbol MethodSymbol { get; set; }
    public required INamedTypeSymbol DelegateSymbol { get; init; }
}
