// MemberRecordGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// The comments converted code writes for a Go fact about a MEMBER, in place of an attribute on it
/// (docs/PLAN-marker-comment-parity.md, 5.6), and the one reader both their consumers share.
/// </summary>
public static class MemberMarkers
{
    /// <summary>Marks a Go EMBEDDED field: <c>/*embed*/ public Reader Reader;</c>.</summary>
    public const string Embed = "/*embed*/";

    /// <summary>
    /// Whether <paramref name="field"/> carries the <see cref="Embed"/> comment where the converter writes it:
    /// directly before the declaration's first token after any attribute lists, followed by one space
    /// (<c>/*embed*/ public Reader Reader;</c>). A comment anywhere else is not a marker, and a Go comment the
    /// converter carries is never spelled as one (memberMarkers.go, carriedComment), so neither can be read as
    /// a fact about this field.
    /// </summary>
    public static bool HasEmbed(FieldDeclarationSyntax field)
    {
        SyntaxToken first = field.Modifiers.Count > 0 ? field.Modifiers[0] : field.Declaration.Type.GetFirstToken();
        SyntaxTriviaList leading = first.LeadingTrivia;

        return leading.Count >= 2 &&
            leading[leading.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) && leading[leading.Count - 1].ToString() == " " &&
            leading[leading.Count - 2].IsKind(SyntaxKind.MultiLineCommentTrivia) && leading[leading.Count - 2].ToString() == Embed;
    }

    /// <summary>
    /// Whether <paramref name="field"/> is a Go embedded field, however it says so: <c>[GoEmbedded]</c> (hand-written
    /// files), the <see cref="Embed"/> comment on a declaration in this compilation, or, for a field read from
    /// METADATA, the <c>[GoMemberRecord]</c> this generator wrote on its type when that assembly was built.
    /// </summary>
    public static bool IsGoEmbedded(IFieldSymbol field)
    {
        if (field.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "GoEmbeddedAttribute"))
            return true;

        foreach (SyntaxReference reference in field.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax declaration } && HasEmbed(declaration))
                return true;
        }

        return field.ContainingType is { } type && type.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.Name == "GoMemberRecordAttribute" &&
            attribute.ConstructorArguments is [{ Value: string member }, { Value: { } fact }, ..] &&
            member == field.Name && Convert.ToByte(fact) == EmbeddedFact);
    }

    // golib's GoMemberFact.Embedded.
    public const byte EmbeddedFact = 1;
}

public sealed class MemberMarkerFinder : ISyntaxReceiver
{
    public List<FieldDeclarationSyntax> Fields { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is FieldDeclarationSyntax field && MemberMarkers.HasEmbed(field))
            Fields.Add(field);
    }
}

/// <summary>
/// Emits, for every member converted code marks with a fact comment, a <c>[GoMemberRecord]</c> on a generated
/// partial of the member's declaring type, which golib's <c>GoReflect.MemberRecords</c> reads back and checks
/// against the member it names. Converted code keeps only the comment.
/// </summary>
/// <remarks>
/// A member of a type that is not partial everywhere (with its enclosing types) cannot be recorded, since no
/// generated part can be added to it; converted types are all partial.
/// </remarks>
[Generator]
public class MemberRecordGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new MemberMarkerFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not MemberMarkerFinder { Fields.Count: > 0 } finder)
            return;

        Dictionary<INamedTypeSymbol, (string ns, List<string> records)> byType = new(SymbolEqualityComparer.Default);

        foreach (FieldDeclarationSyntax field in finder.Fields)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(field.SyntaxTree);

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                if (semanticModel.GetDeclaredSymbol(variable) is not IFieldSymbol { ContainingType: { } declaringType } symbol ||
                    !GeneratedPartials.CanReopen(declaringType))
                {
                    continue;
                }

                string ns = field.GetNamespaceName();

                if (ns.Length == 0)
                    continue;

                if (!byType.TryGetValue(declaringType, out (string ns, List<string> records) slot))
                {
                    slot = (ns, []);
                    byType[declaringType] = slot;
                }

                slot.records.Add($"[global::go.GoMemberRecord(\"{symbol.Name}\", global::go.GoMemberFact.Embedded)]");
            }
        }

        HashSet<string> hintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<INamedTypeSymbol, (string ns, List<string> records)> pair in byType)
            context.AddSource(GeneratedPartials.HintName(hintNames, pair.Value.ns, pair.Key, "members"), GeneratedPartials.Source(pair.Value.ns, pair.Key, pair.Value.records));
    }
}
