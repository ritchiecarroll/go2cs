// GoTypeDeclarationFinder.cs - Gbtc
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
/// Finds the Go types TypeGenerator emits for: every type declaration carrying a <c>[GoType]</c>
/// attribute (the converter's definition argument, or a hand-written file's opt-in), and every
/// CONVERTED Go type declaration that carries none (see <see cref="Common.IsConvertedGoTypeDeclaration"/>).
/// </summary>
/// <remarks>
/// A converted declaration found by the rule has no attribute list entry, so it is recorded with
/// <c>byRule</c> set and is read as a plain <c>[GoType]</c>. The hand-owned-package opt-out is
/// assembly-wide and is applied by the generator (<see cref="Common.IsHandOwnedAssembly"/>), not
/// here: a syntax receiver sees one node at a time.
/// </remarks>
public sealed class GoTypeDeclarationFinder(string attributeFullName) : ISyntaxContextReceiver
{
    public List<(BaseTypeDeclarationSyntax targetSyntax, List<AttributeSyntax> attributes, bool byRule)> Targets = [];

    public bool HasTargets => Targets.Count > 0;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not BaseTypeDeclarationSyntax targetSyntax)
            return;

        List<AttributeSyntax> attributes = [];

        foreach (AttributeSyntax attributeSyntax in targetSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            if (string.Equals(attributeFullName, attributeSymbol.ContainingType.OriginalDefinition.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
        }

        if (attributes.Count > 0)
            Targets.Add((targetSyntax, attributes, false));
        else if (targetSyntax.IsConvertedGoTypeDeclaration())
            Targets.Add((targetSyntax, attributes, true));
    }
}
