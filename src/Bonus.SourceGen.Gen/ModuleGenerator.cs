namespace Bonus.SourceGen;

[Generator]
public class ModuleGenerator : IIncrementalGenerator {

    private static readonly string _registerDelegate = typeof(RegisterDelegateAttribute).FullName;

    public void Initialize(IncrementalGeneratorInitializationContext context) {

        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(type => type != null);

        var compilation = context.CompilationProvider.Combine(methodDeclarations.Collect());

        context.RegisterSourceOutput(compilation,
            static (context, source) => Execute(context, source.Left, source.Right!));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) {
        return node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static MethodDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context,
        CancellationToken token) {
        var method = (MethodDeclarationSyntax)context.Node;

        foreach (var attribute in method.AttributeLists.SelectMany(list => list.Attributes)) {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol) {
                // NOTE: when Assembly was not referenced, which contained the Attribute
                continue;
            }

            var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            var fullName = attributeContainingTypeSymbol.ToDisplayString();

            if (_registerDelegate == fullName) {
                return method;
            }
        }

        return null;
    }


    private static void Execute(SourceProductionContext context, Compilation compilation,
        ImmutableArray<MethodDeclarationSyntax> methods) {
        var classes = methods.GroupBy(method => method.Parent);
        var modules = GetModulesToGenerate(context, compilation, classes);

        foreach (var module in modules) {
            if (module.Methods.Count == 0) continue;

            var result = ModuleSourceGeneration.Create(module);
            var file = string.Join(".", module.Classes.Select(@class => @class.Identifier));
            context.AddSource($"{file}.Module.g.cs", result);
        }
    }


    private static IReadOnlyCollection<ModuleInfo> GetModulesToGenerate(
        SourceProductionContext context, Compilation compilation,
        IEnumerable<IGrouping<SyntaxNode?, MethodDeclarationSyntax>> groups) {

        var registerDelegate = compilation.GetTypeByMetadataName(_registerDelegate);
        if (registerDelegate == null) {
            context.ReportDiagnostic(Issues.RegisterDelegateNotFound());
            return Array.Empty<ModuleInfo>();
        }

        var result = new List<ModuleInfo>();

        foreach (var group in groups) {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (group.Key is not ClassDeclarationSyntax classDeclaration) {
                // NOTE: broken code
                continue;
            }

            if (!classDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword))) {
                context.ReportDiagnostic(Issues.ClassNotPartial(classDeclaration));
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var methods = new List<MethodInfo>();

            foreach (var method in group) {
                var attributeSymbols = method.AttributeLists
                    .SelectMany(a => a.Attributes)
                    .Select(attribute => semanticModel.GetSymbolInfo(attribute).Symbol)
                    .OfType<IMethodSymbol>()
                    .Select(symbol => symbol.ReceiverType)
                    .ToImmutableHashSet(SymbolEqualityComparer.Default);

                if (!method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword))) {
                    context.ReportDiagnostic(Issues.NotStatic(method));
                    continue;
                }

                if (!attributeSymbols.Contains(registerDelegate)) {
                    context.ReportDiagnostic(Issues.MissingRegisterDelegate(method));
                    continue;
                }

                if (semanticModel.GetSymbolInfo(method.ReturnType).Symbol is not INamedTypeSymbol delegateSymbol) {
                    // !!!!!
                    continue;
                }

                if (delegateSymbol.TypeKind != TypeKind.Delegate ||
                    delegateSymbol.DelegateInvokeMethod == null) {
                    context.ReportDiagnostic(Issues.DoesNotReturnDelegate(method));
                    continue;
                }

                if (delegateSymbol.DeclaringSyntaxReferences.Length > 0) {
                    var syntaxReference = delegateSymbol.DeclaringSyntaxReferences.First();

                    var delegateDeclaration = (DelegateDeclarationSyntax)syntaxReference.GetSyntax();

                    if (delegateDeclaration.Parent == classDeclaration &&
                        delegateDeclaration.Modifiers.All(modifier => !modifier.IsKind(SyntaxKind.InternalKeyword))) {
                        context.ReportDiagnostic(Issues.DelegateShouldBeInternal(delegateDeclaration));
                    }

                    var delegateParameterSymbols = delegateDeclaration.ParameterList.Parameters
                        .Select(parameter =>
                            semanticModel.GetSymbolInfo(parameter.Type!, context.CancellationToken).Symbol
                        )
                        .OfType<ITypeSymbol>()
                        .ToImmutableHashSet(SymbolEqualityComparer.Default);

                    var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                    if (delegateParameterSymbols.Contains(classSymbol)) {
                        context.ReportDiagnostic(Issues.SelfAsDelegateParameter(delegateDeclaration));
                    }
                }

                methods.Add(new MethodInfo {
                    Method = method,
                    Delegate = delegateSymbol,
                });
            }

            var classes = classDeclaration.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().Reverse().ToImmutableArray();
            if (classes.Length > 1) {
                context.ReportDiagnostic(Issues.NestedClasses(classes[^1]));
            }

            result.Add(new ModuleInfo {
                Classes = classes,
                Namespace = GetNamespace(classDeclaration),
                Methods = methods
            });
        }

        return result;
    }

    private static string? GetNamespace(ClassDeclarationSyntax? classDeclaration) {
        return classDeclaration?.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
    }
}

internal readonly record struct ModuleInfo {
    public required IReadOnlyList<ClassDeclarationSyntax> Classes { get; init; }
    public string? Namespace { get; init; }
    public required IReadOnlyCollection<MethodInfo> Methods { get; init; }
}

internal readonly record struct MethodInfo {
    public required MethodDeclarationSyntax Method { get; init; }
    public INamedTypeSymbol Delegate { get; init; }
}
