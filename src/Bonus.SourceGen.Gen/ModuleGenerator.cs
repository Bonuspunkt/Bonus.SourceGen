namespace Bonus.SourceGen;

[Generator]
public class ModuleGenerator : IIncrementalGenerator {
    private static readonly ImmutableHashSet<string> _attributeNames = new[] {
        RegisterDelegateGenerator.Attribute,
        UseHistogramGenerator.Attribute,
        UseActivityGenerator.Attribute
    }.ToImmutableHashSet();

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(type => type != null);

        var compilation = context.CompilationProvider.Combine(methodDeclarations.Collect());

        context.RegisterSourceOutput(
            compilation,
            static (context, source) => Execute(context, source.Left, source.Right!)
        );
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

            if (_attributeNames.Contains(fullName)) {
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
        var mappings = new Dictionary<string, GenerateSourceCodeFactory> {
                { RegisterDelegateGenerator.Attribute, RegisterDelegateGenerator.Create },
                { UseHistogramGenerator.Attribute, UseHistogramGenerator.Create },
                { UseActivityGenerator.Attribute, UseActivityGenerator.Create },
            }
            .ToImmutableDictionary(
                keyValue => compilation.GetTypeByMetadataName(keyValue.Key),
                keyValue => keyValue.Value,
                SymbolEqualityComparer.Default
            );

        var result = new List<ModuleInfo>();

        foreach (var group in groups) {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (group.Key is not TypeDeclarationSyntax typeDeclaration) {
                // usually broken code
                continue;
            }

            if (!typeDeclaration.Modifiers.Has(SyntaxKind.PartialKeyword)) {
                context.ReportDiagnostic(Issues.TypeNotPartial(typeDeclaration));
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var methods = new List<MethodInfo>();

            foreach (var methodDeclarationSyntax in group) {
                if (!methodDeclarationSyntax.Modifiers.Has(SyntaxKind.StaticKeyword)) {
                    context.ReportDiagnostic(Issues.NotStatic(methodDeclarationSyntax));
                    continue;
                }

                var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax, context.CancellationToken);
                if (methodSymbol == null) continue;

                if (semanticModel.GetSymbolInfo(methodDeclarationSyntax.ReturnType).Symbol is not INamedTypeSymbol
                        delegateSymbol ||
                    delegateSymbol?.DelegateInvokeMethod?.ReturnType == null) {
                    context.ReportDiagnostic(Issues.DoesNotReturnDelegate(methodDeclarationSyntax.ReturnType));
                    continue;
                }

                var attributes = methodDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes);
                var generators = new List<IGenerateSourceCode>();
                foreach (var attribute in attributes) {
                    if (semanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol
                        is not IMethodSymbol attributeSymbol) continue;

                    var receiverTypeSymbol = attributeSymbol.ReceiverType;
                    if (!mappings.TryGetValue(receiverTypeSymbol, out var factory)) continue;

                    var generateSourceContext = new GenerateSourceContext {
                        Compilation = compilation,
                        SemanticModel = semanticModel,
                        ReportDiagnostic = context.ReportDiagnostic,

                        AttributeSyntax = attribute,
                        MethodDeclarationSyntax = methodDeclarationSyntax,
                        MethodSymbol = methodSymbol,
                        DelegateSymbol = delegateSymbol
                    };

                    var generator = factory(generateSourceContext);
                    if (generator != null)
                        generators.Add(generator);
                }

                if (generators.Count == 0) {
                    continue;
                }

                if (generators.All(generator => generator is not RegisterDelegateGenerator)) {
                    context.ReportDiagnostic(Issues.MissingRegisterDelegate(methodDeclarationSyntax));
                    continue;
                }

                methods.Add(new MethodInfo {
                    Method = methodDeclarationSyntax,
                    Delegate = delegateSymbol,
                    Generators = generators
                        .OrderBy(generator => generator.SortNo)
                        .ToImmutableArray(),
                });
            }

            var types = typeDeclaration.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().Reverse().ToImmutableArray();
            if (types.Length > 1) {
                //context.ReportDiagnostic(Issues.NestedClasses(types[^1]));
            }

            result.Add(new ModuleInfo {
                Classes = types,
                Namespace = GetNamespace(typeDeclaration),
                Methods = methods
            });
        }

        return result;
    }


    private static string? GetNamespace(TypeDeclarationSyntax? typeDeclaration) {
        return typeDeclaration?.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
    }
}

internal readonly record struct ModuleInfo {
    public required ImmutableArray<TypeDeclarationSyntax> Classes { get; init; }
    public string? Namespace { get; init; }
    public required IReadOnlyCollection<MethodInfo> Methods { get; init; }
}

internal readonly record struct MethodInfo {
    public required MethodDeclarationSyntax Method { get; init; }
    public INamedTypeSymbol Delegate { get; init; }
    public ReturnType DelegateReturnType { get; init; }
    public ImmutableArray<IGenerateSourceCode> Generators { get; init; }
}

internal static class SyntaxExtensions {
    public static bool Has(this SyntaxTokenList list, SyntaxKind kind) => list.Any(token => token.IsKind(kind));
}

[Flags]
internal enum ReturnType {
    Void = 0,
    Value = 1 << 0,
    Task = 1 << 1,
}
