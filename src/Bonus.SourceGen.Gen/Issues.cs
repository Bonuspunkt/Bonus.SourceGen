namespace Bonus.SourceGen;

static class Issues {
    // v-- error
    private static readonly DiagnosticDescriptor _typeNotPartial = new(
        id: "BSG001",
        title: "type not partial",
        messageFormat: "type {0} is not marked as partial",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic TypeNotPartial(TypeDeclarationSyntax typeDeclaration) {
        return Diagnostic.Create(_typeNotPartial, typeDeclaration.GetLocation(), typeDeclaration.Identifier);
    }


    private static readonly DiagnosticDescriptor _notStatic = new(
        id: "BSG002",
        title: "method not static",
        messageFormat: "method {0} is not static",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic NotStatic(MethodDeclarationSyntax methodDeclaration) {
        return Diagnostic.Create(_notStatic, methodDeclaration.GetLocation(), methodDeclaration.Identifier);
    }

    private static readonly DiagnosticDescriptor _missingRegisterDelegate = new(
        id: "BSG003",
        title: "method missing RegisterDelegate",
        messageFormat: "method {0} is missing RegisterDelegate attribute",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic MissingRegisterDelegate(MethodDeclarationSyntax methodDeclaration) {
        return Diagnostic.Create(_missingRegisterDelegate, methodDeclaration.GetLocation(), methodDeclaration.Identifier);
    }

    private static readonly DiagnosticDescriptor _doesNotReturnDelegate = new(
        id: "BSG004",
        title: "method does not return a delegate",
        messageFormat: "that is not a delegate",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic DoesNotReturnDelegate(TypeSyntax typeSyntax) {
        return Diagnostic.Create(_doesNotReturnDelegate, typeSyntax.GetLocation());
    }

    private static readonly DiagnosticDescriptor _useNameOfExpression = new(
        id: "BSG100",
        title: "use nameof()",
        messageFormat: "use nameof()",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic UseNameOfExpression(AttributeArgumentSyntax argument) {
        return Diagnostic.Create(_useNameOfExpression, argument.GetLocation());
    }

    private static readonly DiagnosticDescriptor _mustBeHistogramDouble = new(
        id: "BSG101",
        title: "requires Histogram<double>",
        messageFormat: "does not resolve to Histogram<double>",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic MustBeHistogramDouble(AttributeArgumentSyntax argument) {
        return Diagnostic.Create(_mustBeHistogramDouble, argument.GetLocation());
    }

    private static readonly DiagnosticDescriptor _mustBeActivitySource = new(
        id: "BSG200",
        title: "requires ActivitySource",
        messageFormat: "does not resolve to ActivitySource",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static Diagnostic MustBeActivitySource(AttributeArgumentSyntax argument) {
        return Diagnostic.Create(_mustBeActivitySource, argument.GetLocation());
    }

    // v-- warning
    private static readonly DiagnosticDescriptor _selfAsDelegateParameter = new(
        id: "BSG666",
        title: "self reference in delegate",
        messageFormat: "delegate parameter takes itself",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static Diagnostic SelfAsDelegateParameter(DelegateDeclarationSyntax delegateDeclaration) {
        return Diagnostic.Create(_selfAsDelegateParameter, delegateDeclaration.GetLocation());
    }

    private static readonly DiagnosticDescriptor _delegateShouldBeInternal = new(
        id: "BSG667",
        title: "delegate should be internal",
        messageFormat: "when nested delegate should be internal",
        category: "SourceGen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static Diagnostic DelegateShouldBeInternal(DelegateDeclarationSyntax delegateDeclaration) {
        return Diagnostic.Create(_delegateShouldBeInternal, delegateDeclaration.GetLocation());
    }
}
