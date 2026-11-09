// TypeArgumentSubstitution.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// Reads the type arguments of a C# type spelling and puts them in place of a generic declaration's type
/// parameters, in another spelling written in those parameters.
/// </summary>
/// <remarks>
/// A Go defined type over an INSTANTIATED generic struct, `type Q pair[int]`, has the struct's fields at the
/// instantiation's types: Q's A is an int. The converter spells the wrapper's definition as the instantiation,
/// `[GoType("pair&lt;nint&gt;")]`, while the struct's declaration, in source or in metadata, spells each field in
/// its own type parameters (`public T A;`). The wrapper forwards each field with that parameter replaced by the
/// argument the definition gives it, so it declares `ref nint A`, not an unbound `ref T A` (CS0246).
/// </remarks>
internal static class TypeArgumentSubstitution
{
    /// <summary>
    /// The type arguments of <paramref name="typeName"/>, such as <c>nint</c> for <c>pair&lt;nint&gt;</c> or
    /// <c>@string, nint</c> for <c>global::go.lib_package.duo&lt;@string, nint&gt;</c>; empty for a spelling with none.
    /// </summary>
    public static string[] TypeArguments(string typeName)
    {
        SimpleNameSyntax? name = SyntaxFactory.ParseTypeName(typeName) switch
        {
            QualifiedNameSyntax qualified => qualified.Right,
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name,
            SimpleNameSyntax simple => simple,
            _ => null
        };

        return name is GenericNameSyntax generic ? generic.TypeArgumentList.Arguments.Select(argument => argument.ToString()).ToArray() : [];
    }

    /// <summary>
    /// <paramref name="typeName"/> with each of <paramref name="parameters"/> replaced by the argument at the same
    /// position in <paramref name="arguments"/>. Unchanged when the two lists differ in length or are empty.
    /// </summary>
    public static string Substitute(string typeName, IReadOnlyList<string> parameters, IReadOnlyList<string> arguments)
    {
        if (parameters.Count == 0 || parameters.Count != arguments.Count)
            return typeName;

        Dictionary<string, string> substitutions = [];

        for (int i = 0; i < parameters.Count; i++)
            substitutions[parameters[i]] = arguments[i];

        return new ParameterRewriter(substitutions).Visit(SyntaxFactory.ParseTypeName(typeName)).ToFullString();
    }

    /// <summary>
    /// The name of an UNQUALIFIED type spelling without its type arguments, <c>pair</c> for <c>pair&lt;nint&gt;</c>;
    /// null for a qualified spelling, which names a type of another package.
    /// </summary>
    public static string? SimpleName(string typeName) =>
        SyntaxFactory.ParseTypeName(typeName) is SimpleNameSyntax name ? name.Identifier.ValueText : null;

    /// <summary>The type parameter names a struct declaration declares, in order.</summary>
    public static string[] TypeParameters(StructDeclarationSyntax declaration) =>
        declaration.TypeParameterList?.Parameters.Select(parameter => parameter.Identifier.ValueText).ToArray() ?? [];

    /// <summary>The type parameter names a struct symbol's definition declares, in order.</summary>
    public static string[] TypeParameters(INamedTypeSymbol symbol) =>
        symbol.OriginalDefinition.TypeParameters.Select(parameter => parameter.Name).ToArray();

    // Replaces a bare identifier naming a type parameter. A qualified name's parts never name one: a type
    // parameter cannot be qualified, and `go.T` names a type called T.
    private sealed class ParameterRewriter(Dictionary<string, string> substitutions) : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Parent is QualifiedNameSyntax or AliasQualifiedNameSyntax || !substitutions.TryGetValue(node.Identifier.ValueText, out string? argument))
                return node;

            return SyntaxFactory.ParseTypeName(argument).WithTriviaFrom(node);
        }
    }
}
