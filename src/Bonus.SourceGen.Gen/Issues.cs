namespace Bonus.SourceGen;

static class Issues {
    // v-- error
    public static Diagnostic RegisterDelegateNotFound() {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG000",
            title: "RegisterDelegate not present",
            messageFormat: "Could not resolve Bonus.SourceGen.RegisterDelegate",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, null);
    }

    public static Diagnostic ClassNotPartial(ClassDeclarationSyntax classDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG001",
            title: "class not partial",
            messageFormat: "class {0} is not marked as partial",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, classDeclaration.GetLocation());
    }

    public static Diagnostic NotStatic(MethodDeclarationSyntax methodDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG002",
            title: "method not static",
            messageFormat: "method {0} is not static",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, methodDeclaration.GetLocation());
    }

    public static Diagnostic MissingRegisterDelegate(MethodDeclarationSyntax methodDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG003",
            title: "method missing RegisterDelegate",
            messageFormat: "method {0} is missing RegisterDelegate attribute",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, methodDeclaration.GetLocation());
    }

    public static Diagnostic DoesNotReturnDelegate(MethodDeclarationSyntax methodDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG004",
            title: "method does not return a delegate",
            messageFormat: ":(",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, methodDeclaration.GetLocation());
    }

    // v-- warning
    public static Diagnostic SelfAsDelegateParameter(DelegateDeclarationSyntax delegateDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG666",
            title: "self reference in delegate",
            messageFormat: "delegate parameter takes itself",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, delegateDeclaration.GetLocation());
    }

    public static Diagnostic DelegateShouldBeInternal(DelegateDeclarationSyntax delegateDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG667",
            title: "delegate should be internal",
            messageFormat: "when nested delegate should be internal",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, delegateDeclaration.GetLocation());
    }

    // v-- info
    public static Diagnostic NestedClasses(ClassDeclarationSyntax classDeclaration) {
        var descriptor = new DiagnosticDescriptor(
            id: "BSG999",
            title: "nested classes are supported but is this a good idea?",
            messageFormat: "",
            category: "SourceGen",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true
        );
        return Diagnostic.Create(descriptor, classDeclaration.GetLocation());
    }
}
