// MethodSetKeep.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// Keeps a Go type's method set visible to golib's run-time method-set scan in a fully trimmed publish, for as long as
/// the type itself can reach an interface value.
/// </summary>
/// <remarks>
/// <para>
/// A Go method is emitted as an extension method on its package class, and a Go type satisfies an interface no
/// converted code declares it does (fmt's <c>p.arg.(Stringer)</c> against <c>reflect.Kind</c>, or against <c>main</c>'s
/// own types) only through golib's scan of those extension methods (<c>Assembly.GetTypes</c>, then <c>GetMethods</c>).
/// Neither trimmer can follow that scan, so under Native AOT with a full trim the methods kept their code but lost
/// their metadata, and fmt printed <c>reflect.Array</c> as <c>17</c> (2026-10-12, measured; JIT and the template-default
/// partial trim keep every user assembly's metadata and were unaffected).
/// </para>
/// <para>
/// Each Go method of the type is named (by name, see Dependencies) by a <c>[DynamicDependency]</c> on the type's generated
/// <c>ToString</c> override.
/// A virtual override is kept exactly when its type is CONSTRUCTED (boxed): the moment a value of it can be an
/// interface value, which is the only way a method-set scan ever sees it. A type never boxed costs nothing. Measured on
/// a standalone Native AOT full-trim program: the boxed type's method is visible to the scan, an unboxed type's is not,
/// and without the attribute the package class itself is invisible.
/// </para>
/// </remarks>
internal static class MethodSetKeep
{
    private const string ToStringDeclaration = "public override string ToString()";

    /// <summary>
    /// <paramref name="generatedSource"/> with a <c>[DynamicDependency]</c> for each Go method of <paramref name="type"/>
    /// on its generated <c>ToString</c> override, or unchanged when it has none to name (or the source has no single
    /// generated <c>ToString</c> to carry them).
    /// </summary>
    public static string OnToString(string generatedSource, INamedTypeSymbol? type)
    {
        if (type is null || type.IsGenericType || Dependencies(type, generatedSource) is not { Count: > 0 } dependencies)
            return generatedSource;

        int site = generatedSource.IndexOf(ToStringDeclaration, StringComparison.Ordinal);

        if (site < 0 || generatedSource.IndexOf(ToStringDeclaration, site + ToStringDeclaration.Length, StringComparison.Ordinal) >= 0)
            return generatedSource;

        int lineStart = generatedSource.LastIndexOf('\n', site) + 1;
        string indent = generatedSource.Substring(lineStart, site - lineStart);

        if (indent.Any(character => character is not (' ' or '\t')))
            return generatedSource;

        StringBuilder attributes = new();

        foreach (string dependency in dependencies)
            attributes.Append(indent).Append(dependency).Append("\r\n");

        return generatedSource.Insert(lineStart, attributes.ToString());
    }

    // The Go methods of type: the non-generic extension methods on its package class (the outermost containing type)
    // whose receiver is the type, by value or by ref, or its pointer box ж<T>, each as a [DynamicDependency] naming it by
    // NAME. A name with no parameter list keeps every same-named method of the package class, which also covers the ж<T>
    // twin RecvGenerator writes for a pointer receiver (the form golib binds; a generator never sees another's output).
    // It is also the only form ILLink survives: matching a full signature makes it render every same-named overload's
    // parameter types, and one it cannot render crashed the template-default publish (IL1012, a NullReferenceException
    // in DocumentationSignatureParser.AllParametersMatch, 2026-10-12; Native AOT's own compiler was unaffected).
    //
    // EVERY Go method, unexported ones included (COORD 2026-10-12, under the owner's rule that Go behavior outranks size):
    // a sealed interface's unexported marker method (go/ast's exprNode) is answered by this scan when no nominal adapter
    // exists. Keeping exported methods only measured +1.81 MB on the full-trim consumer against +5.13 MB for all; that is
    // a recorded future optimization in PLAN-golib-full-trim.md, gated on a census.
    private static List<string> Dependencies(INamedTypeSymbol type, string generatedSource)
    {
        List<string> dependencies = [];
        INamedTypeSymbol? package = type.ContainingType;

        while (package?.ContainingType is not null)
            package = package.ContainingType;

        if (package is null || package.IsGenericType)
            return dependencies;

        foreach (IMethodSymbol method in package.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsExtensionMethod || method.IsGenericMethod || method.Parameters.Length == 0 || !IsReceiver(method.Parameters[0].Type, type))
                continue;

            dependencies.Add(Dependency(method.MetadataName, package));
        }

        // The methods this generator writes into the type's OWN part: the forwarders of methods promoted through an
        // embedded field, and their box twins (StructTypeTemplate.PromotedStructReceivers). No generator sees its own
        // output through the compilation, so package.GetMembers() above cannot list them, yet a struct whose Error or
        // String arrives ONLY by promotion is printed through exactly one of them (golib's GoMethodText binds it from
        // the run-time method set). A forwarder of a method promoted from ANOTHER package sits in its own top-level
        // class (`<pkg>ᴛ<Type>ᴛxpkg`), not in the package class, so each is kept on the class that declares it: a
        // dependency on the package class resolves nothing there (ILC IL2037), and the full-trim publish prints the
        // structural text instead.
        foreach ((string name, string declaringType) in GeneratedReceivers(generatedSource))
            dependencies.Add(Dependency(name, declaringType));

        return dependencies.Distinct().ToList();
    }

    private static string Dependency(string metadataName, INamedTypeSymbol package) =>
        Dependency(metadataName, GeneratedPartials.TypeOf(package));

    private static string Dependency(string metadataName, string typeOf) =>
        $"[global::System.Diagnostics.CodeAnalysis.DynamicDependency({SymbolDisplay.FormatLiteral(metadataName, true)}, {typeOf})]";

    // Every extension method declared in generatedSource, as (metadata name, `typeof(...)` of its declaring class).
    private static IEnumerable<(string name, string declaringType)> GeneratedReceivers(string generatedSource)
    {
        if (generatedSource.IndexOf("(this ", StringComparison.Ordinal) < 0)
            yield break;

        CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(generatedSource).GetCompilationUnitRoot();

        foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.ParameterList.Parameters.FirstOrDefault() is not { } receiver || !receiver.Modifiers.Any(SyntaxKind.ThisKeyword) ||
                method.Parent is not ClassDeclarationSyntax declaring)
                continue;

            string namespacePath = declaring.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";
            string classPath = string.Join(".", declaring.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().Reverse().Select(type => type.Identifier.ValueText));

            yield return (method.Identifier.ValueText, $"typeof(global::{(namespacePath.Length > 0 ? namespacePath + "." : "")}{classPath})");
        }
    }

    private static bool IsReceiver(ITypeSymbol receiver, INamedTypeSymbol type) =>
        SymbolEqualityComparer.Default.Equals(receiver, type) ||
        receiver is INamedTypeSymbol { Name: "ж", TypeArguments: [var target] } box && box.ContainingNamespace?.ToDisplayString() == "go" &&
        SymbolEqualityComparer.Default.Equals(target, type);
}
