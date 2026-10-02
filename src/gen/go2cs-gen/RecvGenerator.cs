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
    private const string Namespace = "go";
    private const string AttributeName = "GoRecv";
    private const string FullAttributeName = $"{Namespace}.{AttributeName}Attribute";

    public void Initialize(GeneratorInitializationContext context)
    {
    #if DEBUG_GENERATOR
        if (!Debugger.IsAttached)
            Debugger.Launch();
    #endif

        // Register to find "GoRecvAttribute" on method declarations
        context.RegisterForSyntaxNotifications(() => new AttributeFinder<MethodDeclarationSyntax>(FullAttributeName));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not AttributeFinder<MethodDeclarationSyntax> { HasAttributes: true } attributeFinder)
            return;

        // Roslyn hintNames are compared case-INSENSITIVELY, and Go routinely pairs an exported
        // method with an unexported case-twin on the same receiver (math/rand's Int31n/int31n on
        // *Rand) — a raw name-based hintName then throws, suppressing ALL ж-overloads for the
        // package (every box.Method() call fails CS1929).
        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach ((MethodDeclarationSyntax methodSyntax, List<AttributeSyntax> attributes) in attributeFinder.TargetAttributes)
        {
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

            foreach (AttributeSyntax attribute in attributes)
            {
                MethodInfo method = methodSyntax.GetMethodInfo(context.Compilation);

                // Only process methods with a reference receiver to create
                // a generated overload the handles a ptr<T> receiver
                if (method.Parameters.Length == 0 || !method.IsRefRecv)
                    continue;

                // A HAND-WRITTEN twin already stands where this overload would go — the hand-owned
                // testing.T/B/F logging methods, whose `params` tail needs a twin written by hand —
                // so emitting one here is a duplicate member (CS0111). [GoRecv] on such a method
                // states only what it is, a pointer-receiver method, for the run-time method set.
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
                    Scope = scope,
                    Method = method,
                    ReceiverTypeIsPublic = receiverTypeIsPublic,
                    NoInlining = HasNoInliningMark(methodSyntax),
                    OverloadResolutionPriority = GetOverloadResolutionPriority(methodSyntax),
                    UsingStatements = usingStatements
                }
                .Generate();

                // Add the source code to the compilation
                context.AddSource(GetUniqueHintName(emittedHintNames, GetValidFileName($"{packageNamespace}.{packageClassName}.{identifier}.{method.Parameters[0].type}.g.cs")), generatedSource);
            }
        }
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
    // `[MethodImpl(MethodImplOptions.NoInlining)]` (computeNoInliningClosure) -- so the forwarder
    // inherits exactly the functions the converter protected and nothing else. StrGenerator
    // reads the same mark for the same reason.
    internal static bool HasNoInliningMark(MethodDeclarationSyntax methodSyntax)
    {
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
