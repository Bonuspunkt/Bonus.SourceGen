namespace Bonus.SourceGen;

internal class ModuleSourceGeneration {
    public static string Create(ModuleInfo module) {
        var builder = new IndentedStringBuilder();
        builder.AppendLine("// <auto-generated />");
        // required for extension methods
        builder.AppendLine("using Autofac;");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();

        builder.AppendNamespace(module);

        builder.AppendPartialClass(module);

        builder.AppendLine();

        builder.AppendModule(module);

        var source = builder.ToString();
        return source;
    }
}


internal static class IndentedStringBuilderExtensions {
    public static IndentedStringBuilder AppendNamespace(this IndentedStringBuilder builder, ModuleInfo module) {
        if (module.Namespace == default) {
            return builder;
        }

        builder.AppendLine($"namespace {module.Namespace};");
        builder.AppendLine();
        return builder;
    }

    public static IndentedStringBuilder AppendPartialClass(this IndentedStringBuilder builder, ModuleInfo module) {
        var lastClass = module.Classes[^1];
        foreach (var @class in module.Classes) {
            builder.Append($"partial class {@class.Identifier} ");
            if (@class == lastClass && !@class.Modifiers.Has(SyntaxKind.StaticKeyword)) {
                builder.Append(": Bonus.SourceGen.IRegisterADelegateModule ");
            }

            builder.AppendLine("{");
            builder.IncrementIndent();
        }

        builder.AppendLine("public static Autofac.Core.IModule DelegateModule { get; } = new Module();");

        foreach (var _ in module.Classes) {
            builder.DecrementIndent();
            builder.AppendLine("}");
        }

        return builder;
    }

    public static IndentedStringBuilder AppendModule(this IndentedStringBuilder builder, ModuleInfo module) {
        builder.AppendLine("file class Module : Autofac.Module {");
        builder.IncrementIndent();

        builder.AppendLine("protected override void Load(ContainerBuilder builder) {");
        builder.IncrementIndent();

        foreach (var methodInfo in module.Methods) {
            builder.AppendLine($"builder.Register<{methodInfo.Delegate}>(ctx => {{");
            builder.IncrementIndent();
            string returnMethod = null;
            foreach (var generator in methodInfo.Generators) {
                generator.Emit(builder, ref returnMethod);
                builder.AppendLine();
            }

            builder.AppendLine($"return {returnMethod};");
            builder.DecrementIndent();
            builder.AppendLine("});");
        }

        builder.DecrementIndent();
        builder.AppendLine("}"); // /Load
        builder.DecrementIndent();
        builder.AppendLine("}"); // /Module
        return builder;
    }
}
