// MemberRecordGenerator.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

/// <summary>
/// The comments converted code writes for a Go fact about a MEMBER, in place of an attribute on it
/// (docs/PLAN-marker-comment-parity.md, 5.5 and 5.6), and the one reader every consumer shares: the record
/// writer below, and every compile-time reader. Run-time readers read the record.
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

    /// <summary>
    /// The Go struct tag <paramref name="field"/> carries in the comment the converter writes after its <c>;</c>
    /// (<c>public @string Method; /*`json:"method"`*/</c>), or null when it carries none.
    /// </summary>
    public static string? TagOf(FieldDeclarationSyntax field) => TagAfter(field.SemicolonToken);

    /// <summary>
    /// The Go struct tag of an embedded field the converter declares as a partial property, written after the
    /// property (<c>public partial ref T T { get; } /*`json:"t"`*/</c>), or null when it carries none.
    /// </summary>
    public static string? TagOf(PropertyDeclarationSyntax property) => TagAfter(property.GetLastToken());

    // The tag comment directly after a member's last token, one space between: Go's backquoted spelling of the
    // tag, or, for a tag a backquote cannot hold, Go's quoted spelling with any `*/` written `*\x2f`. A comment
    // anywhere else is not a tag, and a Go comment the converter carries is never spelled as one.
    private static string? TagAfter(SyntaxToken last)
    {
        SyntaxTriviaList trailing = last.TrailingTrivia;

        if (trailing.Count < 2 || !trailing[0].IsKind(SyntaxKind.WhitespaceTrivia) || trailing[0].ToString() != " " || !trailing[1].IsKind(SyntaxKind.MultiLineCommentTrivia))
            return null;

        string comment = trailing[1].ToString();

        if (comment.Length < 6)
            return null;

        if (comment.StartsWith("/*`", StringComparison.Ordinal) && comment.EndsWith("`*/", StringComparison.Ordinal))
        {
            string tag = comment.Substring(3, comment.Length - 6);
            return tag.IndexOf('`') < 0 ? tag : null;
        }

        if (comment.StartsWith("/*\"", StringComparison.Ordinal) && comment.EndsWith("\"*/", StringComparison.Ordinal))
            return GoUnquote(comment.Substring(2, comment.Length - 4));

        return null;
    }

    /// <summary>
    /// The value of a Go double-quoted string literal (<paramref name="literal"/> includes its quotes), decoded as
    /// Go decodes it, or null when it is not one. Go's string is bytes, so escapes build UTF-8, which is then read
    /// back as text.
    /// </summary>
    public static string? GoUnquote(string literal)
    {
        if (literal.Length < 2 || literal[0] != '"' || literal[literal.Length - 1] != '"')
            return null;

        List<byte> bytes = [];
        int end = literal.Length - 1;

        for (int index = 1; index < end; index++)
        {
            char current = literal[index];

            if (current == '"' || current == '\n')
                return null;

            if (current != '\\')
            {
                int length = char.IsHighSurrogate(current) && index + 1 < end && char.IsLowSurrogate(literal[index + 1]) ? 2 : 1;
                bytes.AddRange(Encoding.UTF8.GetBytes(literal.Substring(index, length)));
                index += length - 1;
                continue;
            }

            if (++index >= end)
                return null;

            char escape = literal[index];

            switch (escape)
            {
                case 'a': bytes.Add(0x07); break;
                case 'b': bytes.Add(0x08); break;
                case 'f': bytes.Add(0x0C); break;
                case 'n': bytes.Add(0x0A); break;
                case 'r': bytes.Add(0x0D); break;
                case 't': bytes.Add(0x09); break;
                case 'v': bytes.Add(0x0B); break;
                case '\\': bytes.Add((byte)'\\'); break;
                case '"': bytes.Add((byte)'"'); break;

                case 'x':
                    if (!TryDigits(literal, index + 1, 2, 16, end, out uint hexByte))
                        return null;

                    bytes.Add((byte)hexByte);
                    index += 2;
                    break;

                case 'u':
                case 'U':
                    int digits = escape == 'u' ? 4 : 8;

                    if (!TryDigits(literal, index + 1, digits, 16, end, out uint rune) || rune > 0x10FFFF || rune is >= 0xD800 and <= 0xDFFF)
                        return null;

                    bytes.AddRange(Encoding.UTF8.GetBytes(char.ConvertFromUtf32((int)rune)));
                    index += digits;
                    break;

                case >= '0' and <= '7':
                    if (!TryDigits(literal, index, 3, 8, end, out uint octalByte) || octalByte > 0xFF)
                        return null;

                    bytes.Add((byte)octalByte);
                    index += 2;
                    break;

                default:
                    return null;
            }
        }

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    private static bool TryDigits(string text, int start, int count, int radix, int end, out uint value)
    {
        value = 0;

        if (start + count > end)
            return false;

        for (int index = start; index < start + count; index++)
        {
            int digit = radix == 16 ? HexDigit(text[index]) : text[index] is >= '0' and <= '7' ? text[index] - '0' : -1;

            if (digit < 0)
                return false;

            value = value * (uint)radix + (uint)digit;
        }

        return true;
    }

    private static int HexDigit(char digit) => digit switch
    {
        >= '0' and <= '9' => digit - '0',
        >= 'a' and <= 'f' => digit - 'a' + 10,
        >= 'A' and <= 'F' => digit - 'A' + 10,
        _ => -1
    };

    // golib's GoMemberFact.Embedded and GoMemberFact.Tag.
    public const byte EmbeddedFact = 1;
    public const byte TagFact = 2;
}

public sealed class MemberMarkerFinder : ISyntaxReceiver
{
    public List<MemberDeclarationSyntax> Members { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        switch (syntaxNode)
        {
            case FieldDeclarationSyntax field when MemberMarkers.HasEmbed(field) || MemberMarkers.TagOf(field) is not null:
            case PropertyDeclarationSyntax property when MemberMarkers.TagOf(property) is not null:
                Members.Add((MemberDeclarationSyntax)syntaxNode);
                break;
        }
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
        if (context.SyntaxReceiver is not MemberMarkerFinder { Members.Count: > 0 } finder)
            return;

        Dictionary<INamedTypeSymbol, (string ns, List<string> records)> byType = new(SymbolEqualityComparer.Default);

        void record(SyntaxNode declaration, ISymbol? symbol, string fact, string? value = null)
        {
            if (symbol is not { ContainingType: { } declaringType } || !GeneratedPartials.CanReopen(declaringType))
                return;

            string ns = declaration.GetNamespaceName();

            if (ns.Length == 0)
                return;

            if (!byType.TryGetValue(declaringType, out (string ns, List<string> records) slot))
            {
                slot = (ns, []);
                byType[declaringType] = slot;
            }

            string payload = value is null ? "" : ", " + SymbolDisplay.FormatLiteral(value, true);
            slot.records.Add($"[global::go.GoMemberRecord({SymbolDisplay.FormatLiteral(symbol.Name, true)}, global::go.GoMemberFact.{fact}{payload})]");
        }

        foreach (MemberDeclarationSyntax member in finder.Members)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(member.SyntaxTree);

            switch (member)
            {
                case FieldDeclarationSyntax field:
                    bool embedded = MemberMarkers.HasEmbed(field);
                    string? tag = MemberMarkers.TagOf(field);

                    // One record per name: a Go field group (`a, b int `json:"x"``) shares its tag.
                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        ISymbol? symbol = semanticModel.GetDeclaredSymbol(variable);

                        if (embedded)
                            record(field, symbol, "Embedded");

                        // An empty tag is Go's untagged field: nothing to record.
                        if (tag is { Length: > 0 })
                            record(field, symbol, "Tag", tag);
                    }

                    break;

                case PropertyDeclarationSyntax property when MemberMarkers.TagOf(property) is { Length: > 0 } propertyTag:
                    record(property, semanticModel.GetDeclaredSymbol(property), "Tag", propertyTag);
                    break;
            }
        }

        HashSet<string> hintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<INamedTypeSymbol, (string ns, List<string> records)> pair in byType)
            context.AddSource(GeneratedPartials.HintName(hintNames, pair.Value.ns, pair.Key, "members"), GeneratedPartials.Source(pair.Value.ns, pair.Key, pair.Value.records));
    }
}
