// AttributeSyntaxExtensions.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class AttributeSyntaxExtensions
{
    public static (string name, string value)[] GetArgumentValues(this AttributeSyntax attribute)
    {
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList?.Arguments ?? default;

        return arguments.Select(argument => (
            name: argument.NameEquals?.Name.Identifier.Text ?? string.Empty,
            value: argument.Expression.NormalizeWhitespace().ToFullString()
        )).ToArray();
    }

    public static (ITypeSymbol? typeArg1, ITypeSymbol? typeArg2) Get2GenericTypeArguments(this AttributeSyntax attributeSyntax, GeneratorSyntaxContext context)
    {
        // Check if the attribute type is generic. A namespace-qualified spelling (`go.GoImplement<…>`,
        // `global::go.GoImplement<…>`) is the same attribute — the finder matches its SYMBOL — so its
        // generic name is read from the qualified name's right-hand side.
        GenericNameSyntax? genericName = attributeSyntax.Name switch
        {
            GenericNameSyntax name => name,
            QualifiedNameSyntax { Right: GenericNameSyntax right } => right,
            AliasQualifiedNameSyntax { Name: GenericNameSyntax aliased } => aliased,
            _ => null
        };

        if (genericName is null)
            return (null, null);

        // Get the type arguments
        SeparatedSyntaxList<TypeSyntax> typeArguments = genericName.TypeArgumentList.Arguments;

        if (typeArguments.Count != 2)
            return (null, null);

        // Get semantic information for each type argument
        ITypeSymbol? typeArg1 = context.SemanticModel.GetTypeInfo(typeArguments[0]).Type;
        ITypeSymbol? typeArg2 = context.SemanticModel.GetTypeInfo(typeArguments[1]).Type;

        return (typeArg1, typeArg2);
    }
}
