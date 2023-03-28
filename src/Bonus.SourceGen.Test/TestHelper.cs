namespace Bonus.SourceGen;

internal static class TestHelper {

    public static CompilationResult Compile(string source) {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(Autofac.Module).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(RegisterDelegateAttribute).Assembly.Location));

        var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create("foo")
            .WithOptions(compilationOptions)
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        var generator = new ModuleGenerator();

        var driver = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        return new CompilationResult { Compilation = outputCompilation, Diagnostics = diagnostics, Driver = driver };
    }
}

internal delegate Task Validator(CompilationResult data);

internal record CompilationResult {
    public required GeneratorDriver Driver { get; init; }
    public required Compilation Compilation { get; init; }
    public required IReadOnlyCollection<Diagnostic> Diagnostics { get; init; }


    public async Task Validate(params Validator[] checks) {
        await Task.WhenAll(checks.Select(check => check(this)));
    }
}

internal static class Check {

    public static Validator[] Validators { get; } = {
        Snapshots,
        Compilation,
    };

    internal static Task Compilation(CompilationResult data) {
        data.Diagnostics.Should().BeEmpty();

        using var stream = new MemoryStream();
        var result = data.Compilation.Emit(stream);

        var filteredDiagnostics = result.Diagnostics
            .Where(diagnostic => diagnostic.Severity != DiagnosticSeverity.Hidden);

        using (new AssertionScope()) {
            result.Success.Should().Be(true);
            filteredDiagnostics.Should().BeEmpty();
        }
        return Task.CompletedTask;
    }
    internal static Task Snapshots(CompilationResult data) {
        return Verify(data.Driver)
            .UseDirectory("Snapshots");
    }
}
