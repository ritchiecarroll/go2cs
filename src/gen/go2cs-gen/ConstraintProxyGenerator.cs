// ConstraintProxyGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// Publishes the GENERIC constraint proxy (<c>жLesser&lt;E&gt;</c>) of every self-referential constraint
/// interface a package declares: an arity-1 interface closed over the very parameter it constrains,
/// <c>where T : Lesser&lt;T&gt;</c>, named or lifted from an inline constraint.
/// </summary>
/// <remarks>
/// Driven by the constraint clauses themselves, so the converter writes no record for it. A package
/// that instantiates the constraint over a pointer to an element neither it nor the interface's package
/// declares names this proxy, and binds it (ImplementGenerator.EmitConstraintProxyBind).
/// </remarks>
[Generator]
public class ConstraintProxyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SelfReferentialConstraintFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SelfReferentialConstraintFinder { Interfaces.Count: > 0 } finder)
            return;

        HashSet<string> emittedHintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach (INamedTypeSymbol interfaceDef in finder.Interfaces.OrderBy(symbol => symbol.ToDisplayString(), StringComparer.Ordinal))
            ImplementGenerator.EmitGenericConstraintProxy(context, interfaceDef, emittedHintNames);
    }

    private sealed class SelfReferentialConstraintFinder : ISyntaxContextReceiver
    {
        public readonly HashSet<INamedTypeSymbol> Interfaces = new(SymbolEqualityComparer.Default);

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not TypeParameterConstraintClauseSyntax clause)
                return;

            foreach (TypeConstraintSyntax constraint in clause.Constraints.OfType<TypeConstraintSyntax>())
            {
                // Declared by THIS package, inside its package class: the proxy is published beside the interface.
                if (context.SemanticModel.GetTypeInfo(constraint.Type).Type is INamedTypeSymbol { TypeKind: TypeKind.Interface, TypeArguments.Length: 1 } closed &&
                    closed.TypeArguments[0] is ITypeParameterSymbol parameter &&
                    parameter.Name == clause.Name.Identifier.ValueText &&
                    closed.OriginalDefinition is { ContainingType.IsStatic: true } interfaceDef &&
                    SymbolEqualityComparer.Default.Equals(interfaceDef.ContainingAssembly, context.SemanticModel.Compilation.Assembly))
                {
                    Interfaces.Add(interfaceDef);
                }
            }
        }
    }
}
