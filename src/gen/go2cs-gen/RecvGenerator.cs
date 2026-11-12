// RecvGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

//#define DEBUG_GENERATOR

using System;
using System.Collections.Generic;
using go2cs.Templates.ReceiverMethod;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;
using static go2cs.Symbols;

#if DEBUG_GENERATOR
using System.Diagnostics;
#endif

namespace go2cs;

[Generator]
public class RecvGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find the Go pointer-receiver methods emitted `this ref T`, and the value-receiver ones
        context.RegisterForSyntaxNotifications(() => new PointerReceiverFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not PointerReceiverFinder finder || finder.Methods.Count + finder.ValueMethods.Count == 0)
            return;

        // Roslyn hintNames are compared case-INSENSITIVELY, and Go routinely pairs an exported
        // method with an unexported case-twin on the same receiver (math/rand's Int31n/int31n on
        // *Rand) — a raw name-based hintName then throws, suppressing ALL ж-overloads for the
        // package (every box.Method() call fails CS1929).
        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        // In a hand-owned package a method is a pointer receiver by [GoRecv] alone.
        bool handOwnedPackage = context.Compilation.Assembly.IsHandOwnedPackage();
        bool callerFrame = NoInliningPartials.CallerFrameMarkAvailable(context.Compilation);

        foreach (MethodDeclarationSyntax methodSyntax in finder.Methods)
        {
            if (handOwnedPackage && !methodSyntax.IsPointerSetMethod(handOwnedPackage: true))
                continue;

            SyntaxTree syntaxTree = methodSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            string packageNamespace = methodSyntax.GetNamespaceName();
            string packageClassName = methodSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;
            string identifier = methodSyntax.Identifier.Text;

            // GetExplicitAccessModifier ?? GetScope precedence, the same TypeGenerator uses (Common.cs):
            // the converter's OWN modifier on this method declaration is ground truth (it already knows,
            // via real go/types facts, whether the method's signature touches an unexported production
            // type — W3a); a bare name-casing read here cannot see that and would re-widen a method the
            // converter deliberately narrowed to `internal`, producing this generated overload `public`
            // over a signature the converter's own copy is not (CS0050/CS0051 in the OPPOSITE direction).
            string scope = GetExplicitAccessModifier(methodSyntax) ?? GetScope(identifier);

            // The RECEIVER TYPE's accessibility is read from its SYMBOL, never from the Go export case
            // of its name — the same shared rule (Common.EffectiveScopeIsPublic) every sibling generator
            // already routes through (ImplicitConvGenerator, ImplementGenerator.AdapterSidePublic,
            // StructDeclarationSyntaxExtensions). RecvGenerator was the last name-only reader, and a name
            // is the wrong oracle for a type the converter PUBLICIZED: an unexported Go type reached
            // through an exported signature is emitted `public partial struct` (ecdsa's hmacDRBG, handed
            // out by the exported TestingOnlyNewDRBG), yet the name still reads unexported — so this
            // ж-overload was narrowed to `internal`, a CONSUMING assembly saw only the `ref T` primary,
            // and every `box.Method(…)` there was CS1929 (crypto/internal/fips140test's acvp_test, 2
            // sites; the twin partials TypeGenerator and ImplicitConvGenerator emit for that same type
            // were already public, so RecvGenerator alone disagreed).
            // The narrowing itself STAYS: a genuinely internal receiver still forces `internal`, because
            // a public extension method over `ж<internalT>` is CS0051. Only the ORACLE changes.
            ITypeSymbol? receiverTypeSymbol = semanticModel.GetDeclaredSymbol(methodSyntax) is IMethodSymbol { Parameters.Length: > 0 } methodSymbol
                ? methodSymbol.Parameters[0].Type
                : null;

            string[] usingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel);

            MethodInfo method = methodSyntax.GetMethodInfo(context.Compilation);

            // Only process methods with a reference receiver to create
            // a generated overload the handles a ptr<T> receiver
            if (method.Parameters.Length == 0 || !method.IsRefRecv)
                continue;

            // A HAND-WRITTEN twin already stands where this overload would go — the hand-owned
            // testing.T/B/F logging methods, whose `params` tail needs a twin written by hand —
            // so emitting one here is a duplicate member (CS0111). [GoRecv] on such a method
            // states only what it is, a pointer-receiver method, for the run-time method set,
            // which an unmarked `this ref` receiver states as well.
            if (HasDeclaredPointerTwin(semanticModel, methodSyntax))
                continue;

            // A null symbol (no semantic info for this declaration) falls back to the name rule,
            // which is exactly the behaviour this read replaces — never a widening by default.
            string receiverSimpleName = GetSimpleName(method.Parameters[0].type);

            bool receiverTypeIsPublic = receiverTypeSymbol is null
                ? GetScope(receiverSimpleName) == "public"
                : EffectiveScopeIsPublic(receiverTypeSymbol, receiverSimpleName);

            string generatedSource = new ReceiverMethodTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                HostClassName = HostClassOf(methodSyntax),
                HostStaticUsings = HostStaticUsingsOf(methodSyntax, semanticModel),
                Scope = scope,
                Method = method,
                ReceiverTypeIsPublic = receiverTypeIsPublic,
                NoInlining = HasNoInliningMark(methodSyntax, semanticModel),
                CallerFrame = callerFrame,
                OverloadResolutionPriority = GetOverloadResolutionPriority(methodSyntax),
                UsingStatements = usingStatements
            }
            .Generate();

            // Add the source code to the compilation
            context.AddGeneratedSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.{method.Parameters[0].type}.g.cs")), generatedSource);
        }

        // THE VALUE-RECEIVER TWINS. Go's *T method set holds T's value methods, so `p.M()` on a pointer is legal
        // Go and calls M on a copy of *p. Each Go value method of a type this compilation declares gets a
        // `M(this ж<T>, ...)` twin that dereferences and copies (ReceiverMethodTemplate.ValueReceiver), so the
        // converter writes `p.M()` as Go does. A hand-owned package has none: its methods are Go methods by
        // attribute alone, and the converter never relies on a twin there.
        if (handOwnedPackage)
            return;

        foreach (MethodDeclarationSyntax methodSyntax in finder.ValueMethods)
        {
            SyntaxTree syntaxTree = methodSyntax.SyntaxTree;
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol { IsExtensionMethod: true, Parameters.Length: > 0, ReturnsByRef: false, ReturnsByRefReadonly: false } methodSymbol)
                continue;

            // Only a receiver TYPE this compilation declares: a Go method's receiver is a type of its own package. A
            // hand-written helper extension over a golib or BCL type is not a Go method and gets no twin.
            if (methodSymbol.Parameters[0].Type is not INamedTypeSymbol receiverType ||
                !SymbolEqualityComparer.Default.Equals(receiverType.ContainingAssembly, context.Compilation.Assembly) ||
                receiverType.TypeKind is TypeKind.Interface || receiverType.Name == PointerPrefix)
                continue;

            if (HasDeclaredPointerTwin(semanticModel, methodSyntax))
                continue;

            // Only a Go method: one the package class or a type's method holder declares.
            if (!methodSymbol.ContainingType.Name.EndsWith(PackageSuffix, StringComparison.Ordinal) && !GoMethodHosts.IsMethodHost(methodSymbol.ContainingType))
                continue;

            string packageNamespace = methodSyntax.GetNamespaceName();
            string packageClassName = methodSyntax.GetParentClassName();
            string packageName = packageClassName.EndsWith(PackageSuffix) ? packageClassName[..^PackageSuffix.Length] : packageClassName;
            string identifier = methodSyntax.Identifier.Text;
            string scope = GetExplicitAccessModifier(methodSyntax) ?? GetScope(identifier);
            MethodInfo method = methodSyntax.GetMethodInfo(context.Compilation);

            string generatedSource = new ReceiverMethodTemplate
            {
                PackageNamespace = packageNamespace,
                PackageName = packageName,
                HostClassName = HostClassOf(methodSyntax),
                HostStaticUsings = HostStaticUsingsOf(methodSyntax, semanticModel),
                Scope = scope,
                Method = method,
                ReceiverTypeIsPublic = EffectiveScopeIsPublic(receiverType, GetSimpleName(method.Parameters[0].type)),
                NoInlining = HasNoInliningMark(methodSyntax, semanticModel),
                CallerFrame = callerFrame,
                OverloadResolutionPriority = GetOverloadResolutionPriority(methodSyntax),
                ValueReceiver = true,
                UsingStatements = GetFullyQualifiedUsingStatements(syntaxTree, semanticModel)
            }
            .Generate();

            context.AddGeneratedSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.{method.Parameters[0].type}.value.g.cs")), generatedSource);
        }
    }

    // The class a forwarder is written into: the method's own, which every part of it then shares, so the twin
    // overloads the method where it is declared and a private method stays reachable. A class that is not declared
    // partial cannot be reopened (CS0260), so its forwarder keeps the separate `<class>_package` class it had before
    // methods moved to their holders (null, TemplateBase's default).
    internal static string? HostClassOf(MethodDeclarationSyntax methodSyntax) =>
        methodSyntax.Parent is ClassDeclarationSyntax { Parent: not TypeDeclarationSyntax } host && host.Modifiers.Any(SyntaxKind.PartialKeyword)
            ? host.Identifier.Text
            : null;

    // The classes of this assembly that the file of a method in a method holder reads through `using static`
    // (TemplateBase.HostStaticUsings); null for a method of any other class.
    internal static string[]? HostStaticUsingsOf(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel)
    {
        if (methodSyntax.Parent is not ClassDeclarationSyntax host ||
            semanticModel.GetDeclaredSymbol(host) is not INamedTypeSymbol hostSymbol || !GoMethodHosts.IsMethodHost(hostSymbol))
        {
            return null;
        }

        List<string> targets = [];

        foreach (SyntaxNode node in methodSyntax.SyntaxTree.GetRoot().DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax))
        {
            if (node is not UsingDirectiveSyntax { Alias: null, Name: not null } directive || !directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                continue;

            if (semanticModel.GetSymbolInfo(directive.Name).Symbol is not INamedTypeSymbol { IsStatic: true } target ||
                !SymbolEqualityComparer.Default.Equals(target.ContainingAssembly, hostSymbol.ContainingAssembly))
            {
                continue;
            }

            string name = target.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (!targets.Contains(name))
                targets.Add(name);
        }

        return targets.Count == 0 ? null : [.. targets];
    }

    // Whether the method's own class DECLARES its pointer twin: a same-named method whose receiver is
    // `ж<T>` over this method's `ref T` receiver and whose remaining parameters match. Only a twin
    // written in source can be seen here — a generator never sees generator output — so converted
    // code, which declares none, is unaffected.
    private static bool HasDeclaredPointerTwin(SemanticModel semanticModel, MethodDeclarationSyntax methodSyntax)
    {
        if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol { Parameters.Length: > 0, ContainingType: not null } method)
            return false;

        ITypeSymbol receiver = method.Parameters[0].Type;

        foreach (ISymbol member in method.ContainingType.GetMembers(method.Name))
        {
            if (member is not IMethodSymbol sibling || sibling.Parameters.Length != method.Parameters.Length)
                continue;

            if (sibling.Parameters[0].Type is not INamedTypeSymbol { Name: PointerPrefix, TypeArguments.Length: 1 } box ||
                !SymbolEqualityComparer.Default.Equals(box.TypeArguments[0], receiver))
                continue;

            bool sameTail = true;

            for (int i = 1; i < method.Parameters.Length && sameTail; i++)
                sameTail = SymbolEqualityComparer.Default.Equals(sibling.Parameters[i].Type, method.Parameters[i].Type);

            if (sameTail)
                return true;
        }

        return false;
    }

    // Selects every method whose receiver is `this ref T` and that MethodInfo.IsGoRecv reads as a
    // POINTER-set method: converted code emits a Go pointer receiver `this ref T` with no mark, and a
    // hand-written file may still carry [GoRecv], which reads the same. A by-ref receiver marked
    // [GoCopyBound] is a VALUE-set method and has no pointer twin. Read from syntax alone, so the
    // selection costs no semantic model per node; a hand-owned package's narrower rule ([GoRecv]
    // alone) is applied in Execute, where the compilation is known.
    private sealed class PointerReceiverFinder : ISyntaxContextReceiver
    {
        public List<MethodDeclarationSyntax> Methods { get; } = [];

        // The Go VALUE-receiver methods: an extension in a top-level static class whose receiver is not in the
        // pointer set (no ref/in, or a by-ref receiver marked [GoCopyBound]). Execute keeps those the package class
        // or a method holder declares, which only the class's attributes can tell.
        public List<MethodDeclarationSyntax> ValueMethods { get; } = [];

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not MethodDeclarationSyntax { ParameterList.Parameters.Count: > 0 } method)
                return;

            SyntaxTokenList modifiers = method.ParameterList.Parameters[0].Modifiers;

            if (!modifiers.Any(SyntaxKind.ThisKeyword))
                return;

            if (modifiers.Any(SyntaxKind.RefKeyword) && method.IsPointerSetMethod(handOwnedPackage: false))
                Methods.Add(method);
            else if (!method.IsPointerSetMethod(handOwnedPackage: false) && method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                     method.Parent is ClassDeclarationSyntax { Parent: not TypeDeclarationSyntax })
                ValueMethods.Add(method);
        }
    }

    private static string? GetOverloadResolutionPriority(MethodDeclarationSyntax methodSyntax)
    {
        // Read as SPELLED in the emission, like HasNoInliningMark: the argument is the converter's
        // integer literal, so it is carried across verbatim.
        foreach (AttributeListSyntax list in methodSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attribute in list.Attributes)
            {
                string name = attribute.Name.ToString();

                if (!name.EndsWith("OverloadResolutionPriority", StringComparison.Ordinal) && !name.EndsWith("OverloadResolutionPriorityAttribute", StringComparison.Ordinal))
                    continue;

                if (attribute.ArgumentList is { Arguments.Count: 1 } arguments)
                    return arguments.Arguments[0].Expression.ToString();
            }
        }

        return null;
    }

    // The converter's frame-preserving mark, read as it is SPELLED in the emission --
    // `[MethodImpl(MethodImplOptions.NoInlining)]` (computeNoInliningClosure), or the no-inline
    // carrier shape that replaced it on methods, a partial method's implementing part with no
    // declaring part (NoInliningPartials; NoInliningPartialGenerator writes that declaring part, and
    // a generator cannot see another's output) -- so the forwarder inherits exactly the functions the
    // converter protected and nothing else. StrGenerator reads the same mark for the same reason.
    internal static bool HasNoInliningMark(MethodDeclarationSyntax methodSyntax, SemanticModel? semanticModel)
    {
        if (NoInliningPartials.IsCarrier(methodSyntax, semanticModel))
            return true;

        foreach (AttributeListSyntax list in methodSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attribute in list.Attributes)
            {
                string name = attribute.Name.ToString();

                if (!name.EndsWith("MethodImpl", StringComparison.Ordinal) && !name.EndsWith("MethodImplAttribute", StringComparison.Ordinal))
                    continue;

                if (attribute.ArgumentList?.ToString().Contains("NoInlining") == true)
                    return true;
            }
        }

        return false;
    }
}
