// InternalPackageAnalyzer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace go2cs;

/// <summary>
/// Reports GO2CS0014, a warning, where hand-written C# names a type of a Go internal or vendored package from outside
/// the package's root. Go lets only code at or below that root import such a package; .NET has no such rule, so the
/// converter keeps the package public and stamps its assembly with <c>[assembly: GoInternalRoot(root)]</c>
/// (docs/phase4/DESIGN-internal-package-marking.md).
/// </summary>
/// <remarks>
/// <para>
/// Two copies of this class ship. go2cs-gen's copy acts only in a compilation that declares a <c>[GoPackage]</c> class
/// (a converted project) and checks only its hand-written files: <c>*_impl.cs</c>, <c>*_impl_test.cs</c> and
/// <c>[module: GoManualConversion]</c> files. The consumer copy, built with <c>GO2CS_CONSUMER_ANALYZER</c> and packed in
/// go.lib, acts only in a compilation that declares none (plain C# referencing converted packages) and checks every
/// file. The two are disjoint by that test, so a compilation that loads both gets one check.
/// </para>
/// <para>
/// The consumer's Go path is decoded from its own <c>[GoPackage]</c> classes, the way golib's reflect decodes a package
/// path: the stamped <c>ImportPath</c> when present, else the namespace below <c>go</c> read as directories plus the
/// package name. A trailing <c>_test</c> is dropped from both, so a test compilation's internal and external packages
/// agree. A <c>main</c> package takes its module's path from its <c>GoModInfo</c> stamp and may use every root inside
/// that module. No class, classes that disagree, or a main without the stamp means no path, and every root is
/// forbidden. Standard-library membership is <c>build_property.GoStandardLibrary</c>, which src/core's
/// Directory.Build.props sets.
/// </para>
/// <para>
/// A use is a NAME that binds a type declared in a forbidden assembly, including a <c>using static</c> or alias target
/// and a use of such an alias. A namespace, a member reached through a value and an inferred type (<c>var</c>) are not
/// uses: Go allows each of them without an import. One semantic-model pass per tree reports one diagnostic per (tree,
/// forbidden assembly), at the first such name in source order.
/// </para>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InternalPackageAnalyzer : DiagnosticAnalyzer
{
    private const string RootAttribute = "go.GoInternalRootAttribute";
    private const string PackageAttribute = "go.GoPackageAttribute";
    private const string ModInfoAttribute = "go.GoModInfoAttribute";
    private const string EmissionRootNamespace = "go";
    private const string TestSuffix = "_test";
    private const string StandardLibraryProperty = "build_property.GoStandardLibrary";

#if GO2CS_CONSUMER_ANALYZER
    private const bool DefaultConvertedCopy = false;
#else
    private const bool DefaultConvertedCopy = true;
#endif

    private readonly bool m_convertedCopy;

    /// <summary>
    /// Creates the copy this assembly ships: go2cs-gen's for converted projects, or go.lib's consumer copy.
    /// </summary>
    public InternalPackageAnalyzer() : this(DefaultConvertedCopy)
    {
    }

    /// <summary>
    /// Creates either copy; tests run both from one assembly.
    /// </summary>
    /// <param name="convertedCopy"><c>true</c> for go2cs-gen's copy, <c>false</c> for the consumer copy.</param>
    public InternalPackageAnalyzer(bool convertedCopy)
    {
        m_convertedCopy = convertedCopy;
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GeneratorDiagnostics.InternalPackageUse];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private sealed class MarkedAssembly(string root, bool standard)
    {
        public string Root { get; } = root;

        public bool Standard { get; } = standard;
    }

    private void Start(CompilationStartAnalysisContext context)
    {
        Compilation compilation = context.Compilation;
        Dictionary<IAssemblySymbol, MarkedAssembly> marked = new(SymbolEqualityComparer.Default);

        foreach (IAssemblySymbol reference in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (ReadRoot(reference) is { } mark)
                marked[reference] = mark;
        }

        // Most compilations reference no marked package, and pay nothing more.
        if (marked.Count == 0)
            return;

        List<(string name, string path)> packages = DeclaredPackages(compilation.Assembly.GlobalNamespace, context.CancellationToken);

        // The two copies are disjoint: go2cs-gen's acts in converted projects, the consumer copy in plain C#.
        if (m_convertedCopy != packages.Count > 0)
            return;

        string? consumerPath = ConsumerPath(packages, compilation.Assembly, out bool main);

        context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue(StandardLibraryProperty, out string? standardValue);
        bool consumerStandard = string.Equals(standardValue?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        Dictionary<IAssemblySymbol, (MarkedAssembly mark, string packagePath)> forbidden = new(SymbolEqualityComparer.Default);

        foreach (KeyValuePair<IAssemblySymbol, MarkedAssembly> entry in marked)
        {
            if (Allowed(entry.Value, consumerPath, consumerStandard, main))
                continue;

            List<(string name, string path)> declared = DeclaredPackages(entry.Key.GlobalNamespace, context.CancellationToken);
            forbidden[entry.Key] = (entry.Value, declared.Count > 0 ? declared[0].path : entry.Key.Name);
        }

        if (forbidden.Count == 0)
            return;

        bool convertedCopy = m_convertedCopy;

        context.RegisterSemanticModelAction(modelContext =>
        {
            SemanticModel model = modelContext.SemanticModel;

            if (convertedCopy && !IsHandWritten(model.SyntaxTree))
                return;

            HashSet<IAssemblySymbol> reported = new(SymbolEqualityComparer.Default);

            foreach (SyntaxNode node in model.SyntaxTree.GetRoot(modelContext.CancellationToken).DescendantNodes())
            {
                if (node is not SimpleNameSyntax name || name.IsVar)
                    continue;

                ISymbol? symbol = model.GetSymbolInfo(name, modelContext.CancellationToken).Symbol;

                if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor)
                    symbol = constructor.ContainingType;

                if (symbol is not INamedTypeSymbol type || type.OriginalDefinition.ContainingAssembly is not { } assembly)
                    continue;

                if (!forbidden.TryGetValue(assembly, out (MarkedAssembly mark, string packagePath) found) || !reported.Add(assembly))
                    continue;

                string rootText = found.mark.Root.Length == 0 && found.mark.Standard ? "the standard library" : $"'{found.mark.Root}'";

                modelContext.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.InternalPackageUse, name.GetLocation(),
                    type.ToDisplayString(), found.packagePath, rootText));

                if (reported.Count == forbidden.Count)
                    return;
            }
        });
    }

    /// <summary>
    /// Go's rule, with the standard library's flag: the consumer is at or below the root.
    /// </summary>
    private static bool Allowed(MarkedAssembly mark, string? consumerPath, bool consumerStandard, bool main)
    {
        if (consumerPath is null)
            return false;

        if (mark.Standard)
            return consumerStandard && !main && Under(consumerPath, mark.Root);

        if (consumerStandard)
            return false;

        // A main's directory inside its module is not recorded, so it may use every root inside the module.
        return main ? mark.Root == consumerPath || mark.Root.StartsWith(consumerPath + "/", StringComparison.Ordinal) : Under(consumerPath, mark.Root);
    }

    private static bool Under(string path, string root)
    {
        return root.Length == 0 || path == root || path.StartsWith(root + "/", StringComparison.Ordinal);
    }

    private static MarkedAssembly? ReadRoot(IAssemblySymbol assembly)
    {
        foreach (AttributeData attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != RootAttribute || attribute.ConstructorArguments is not [{ Value: string root }])
                continue;

            bool standard = false;

            foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
            {
                if (argument.Key == "Standard" && argument.Value.Value is bool value)
                    standard = value;
            }

            return new MarkedAssembly(root, standard);
        }

        return null;
    }

    /// <summary>
    /// The consumer's Go path: the one path its package classes agree on, or its module's path for a main package.
    /// </summary>
    internal static string? ConsumerPath(List<(string name, string path)> packages, IAssemblySymbol assembly, out bool main)
    {
        main = false;

        if (packages.Count == 0)
            return null;

        (string name, string path) = packages[0];

        foreach ((string otherName, string otherPath) in packages)
        {
            if (otherName != name || otherPath != path)
                return null;
        }

        if (name != "main")
            return path;

        main = true;

        return ModulePath(assembly);
    }

    /// <summary>
    /// The module path from a <c>GoModInfo</c> stamp's <c>mod</c> line, or null.
    /// </summary>
    private static string? ModulePath(IAssemblySymbol assembly)
    {
        foreach (AttributeData attribute in assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != ModInfoAttribute || attribute.ConstructorArguments is not [{ Value: string value }])
                continue;

            foreach (string line in value.Split('\n'))
            {
                string[] fields = line.Split('\t');

                if (fields.Length >= 2 && fields[0] == "mod" && fields[1].Length > 0)
                    return fields[1];
            }
        }

        return null;
    }

    /// <summary>
    /// The top-level <c>[GoPackage]</c> classes an assembly declares, each as its package name and Go path, with any
    /// trailing <c>_test</c> dropped from both.
    /// </summary>
    private static List<(string name, string path)> DeclaredPackages(INamespaceSymbol global, System.Threading.CancellationToken cancellationToken)
    {
        List<(string name, string path)> packages = [];
        Stack<INamespaceSymbol> namespaces = new();
        namespaces.Push(global);

        while (namespaces.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            INamespaceSymbol current = namespaces.Pop();

            foreach (INamespaceSymbol child in current.GetNamespaceMembers())
                namespaces.Push(child);

            foreach (INamedTypeSymbol type in current.GetTypeMembers())
            {
                foreach (AttributeData attribute in type.GetAttributes())
                {
                    if (attribute.AttributeClass?.ToDisplayString() != PackageAttribute || attribute.ConstructorArguments is not [{ Value: string name }])
                        continue;

                    string? importPath = null;

                    foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
                    {
                        if (argument.Key == "ImportPath" && argument.Value.Value is string { Length: > 0 } stamped)
                            importPath = stamped;
                    }

                    packages.Add(DecodePackage(NamespaceName(current), name, importPath));
                }
            }
        }

        return packages;
    }

    /// <summary>
    /// Decodes a package class's Go path as golib's reflect does (GoReflect.GoPackageClassPath): the stamped path
    /// when present, else the namespace below <c>go</c> as directories plus the package name. A trailing <c>_test</c>
    /// is dropped from the name and the path.
    /// </summary>
    /// <param name="namespaceName">The dotted namespace, in metadata names (no <c>@</c> escapes).</param>
    internal static (string name, string path) DecodePackage(string namespaceName, string name, string? importPath)
    {
        name = TrimTest(name);

        if (importPath is { Length: > 0 })
            return (name, TrimTest(importPath));

        if (namespaceName.Length > EmissionRootNamespace.Length + 1 && namespaceName.StartsWith(EmissionRootNamespace + ".", StringComparison.Ordinal))
            return (name, namespaceName.Substring(EmissionRootNamespace.Length + 1).Replace('.', '/') + "/" + name);

        return (name, name);
    }

    private static string TrimTest(string value)
    {
        return value.EndsWith(TestSuffix, StringComparison.Ordinal) ? value.Substring(0, value.Length - TestSuffix.Length) : value;
    }

    private static string NamespaceName(INamespaceSymbol namespaceSymbol)
    {
        List<string> names = [];

        for (INamespaceSymbol? current = namespaceSymbol; current is { IsGlobalNamespace: false }; current = current.ContainingNamespace)
            names.Add(current.Name);

        names.Reverse();

        return string.Join(".", names);
    }

    /// <summary>
    /// go2cs-gen's copy checks the hand-written files of a converted project only: not converted Go, not the
    /// converter's metadata files, not generated output.
    /// </summary>
    private static bool IsHandWritten(SyntaxTree tree)
    {
        string fileName = Path.GetFileName(tree.FilePath ?? string.Empty);

        if (fileName.Equals("package_init.cs", StringComparison.OrdinalIgnoreCase))
            return false;

#if GO2CS_CONSUMER_ANALYZER
        return true;
#else
        // IsConvertedSourceFile answers false for the metadata files too, which are converter output here.
        if (fileName.Equals("package_info.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("package_test_info.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("package_info_internal_test.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("package_info_external_test.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("go2cs_test_host.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !Common.IsConvertedSourceFile(tree);
#endif
    }
}
