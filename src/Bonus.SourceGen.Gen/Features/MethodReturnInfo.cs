namespace Bonus.SourceGen;

internal record MethodReturnInfo {
    public static MethodReturnInfo Create(ITypeSymbol typeSymbol, Compilation compilation) {
        var voidSymbol = compilation.GetTypeByMetadataName(typeof(void).FullName);
        var taskSymbol = compilation.GetTypeByMetadataName(typeof(Task).FullName);

        Func<ISymbol?, ISymbol?, bool> equals = SymbolEqualityComparer.Default.Equals;
        if (equals(voidSymbol, typeSymbol))
            return new MethodReturnInfo { Async = null, Return = null };
        if (equals(taskSymbol, typeSymbol))
            return new MethodReturnInfo { Async = "async ", Return = "await " };
        if (equals(taskSymbol, typeSymbol.BaseType))
            return new MethodReturnInfo { Async = "async ", Return = "return await " };

        return new MethodReturnInfo { Async = null, Return = "return " };
    }

    public required string? Async { get; init; }
    public required string? Return { get; init; }
}
