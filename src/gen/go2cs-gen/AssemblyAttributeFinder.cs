// AssemblyAttributeFinder.cs - Gbtc
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

// nonGenericAttributeFullName optionally names a NON-generic attribute of the same family that the generator also
// reads: GoImplement's open-generic form, `[assembly: GoImplement(typeof(sliceIter<>), typeof(Iter))]`, whose type
// definition only a typeof argument can carry.
public sealed class AssemblyAttributeFinder(string attributeFullName, string? nonGenericAttributeFullName = null) : ISyntaxContextReceiver
{
    public List<(AttributeSyntax attribute, GeneratorSyntaxContext context, CompilationUnitSyntax compilationUnit, FileScopedNamespaceDeclarationSyntax? namespaceSyntax)> TargetAttributes = [];

    public bool HasAttributes => TargetAttributes.Count > 0;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        // Assembly attributes are contained in AttributeListSyntax nodes that have the "assembly" target specifier
        if (context.Node is not AttributeListSyntax attributeList || attributeList.Target?.Identifier.Text != "assembly")
            return;

        List<AttributeSyntax> attributes = GetSemanticTargetForGeneration(context, attributeList);

        if (attributes.Count == 0)
            return;

        // Get the compilation unit (file-level syntax node) - allows access to other code elements in same file
        CompilationUnitSyntax? compilationUnit = attributeList.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();

        if (compilationUnit is null)
            return;

        // Find the first namespace in the file (if any)
        FileScopedNamespaceDeclarationSyntax? namespaceSyntax = compilationUnit.Members
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        // Add each attribute with its containing context
        foreach (AttributeSyntax attribute in attributes)
            TargetAttributes.Add((attribute, context, compilationUnit, namespaceSyntax));
    }

    private List<AttributeSyntax> GetSemanticTargetForGeneration(GeneratorSyntaxContext context, AttributeListSyntax attributeList)
    {
        List<AttributeSyntax> attributes = [];

        foreach (AttributeSyntax attributeSyntax in attributeList.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue;

            INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

            if (attributeContainingTypeSymbol.IsGenericType && string.Equals(attributeFullName, attributeContainingTypeSymbol.OriginalDefinition.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
            else if (!attributeContainingTypeSymbol.IsGenericType && nonGenericAttributeFullName is not null && string.Equals(nonGenericAttributeFullName, attributeContainingTypeSymbol.ToDisplayString(), StringComparison.Ordinal))
                attributes.Add(attributeSyntax);
        }

        return attributes;
    }
}
