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
/// (docs/PLAN-marker-comment-parity.md, 5.4 to 5.6), and the one reader every consumer shares: the record
/// writer below, and every compile-time reader (StructTypeTemplate for embeds, TypeGenerator for a type's
/// dims). Run-time readers read the record.
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

    /// <summary>
    /// The Go array dims stated by the comment the converter writes directly before a type, outermost first
    /// (<c>/*[32]*/ array&lt;byte&gt; hash</c>, <c>/*[4][8]*/</c>), or null when there is none. <paramref name="first"/>
    /// is the first token after the comment: a parameter's or field's type, or a type declaration's first
    /// modifier. Roslyn attaches a comment that follows another token on its line (`(`, `,`, a modifier, an
    /// attribute list's `]`) to that token's trailing trivia, and one that opens a line to
    /// <paramref name="first"/>'s leading trivia, so both are read; either way the comment must be the last
    /// trivia before <paramref name="first"/> but for one space.
    /// </summary>
    public static long[]? DimsBefore(SyntaxToken first)
    {
        SyntaxTriviaList leading = first.LeadingTrivia;

        if (leading.Count > 0 && !leading.All(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
            return DimsAtEnd(leading);

        return DimsAtEnd(first.GetPreviousToken().TrailingTrivia);
    }

    /// <summary>The dims comment before <paramref name="parameter"/>'s type.</summary>
    public static long[]? DimsOf(ParameterSyntax parameter) => parameter.Type is { } type ? DimsBefore(type.GetFirstToken()) : null;

    /// <summary>The dims comment before <paramref name="field"/>'s type.</summary>
    public static long[]? DimsOf(FieldDeclarationSyntax field) => DimsBefore(field.Declaration.Type.GetFirstToken());

    /// <summary>The dims comment before a type declaration's first modifier (or its keyword).</summary>
    public static long[]? DimsOf(BaseTypeDeclarationSyntax declaration) =>
        DimsBefore(declaration.Modifiers.Count > 0 ? declaration.Modifiers[0] : declaration.GetFirstToken());

    private static long[]? DimsAtEnd(SyntaxTriviaList trivia)
    {
        if (trivia.Count < 2 || !trivia[trivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) || trivia[trivia.Count - 1].ToString() != " " ||
            !trivia[trivia.Count - 2].IsKind(SyntaxKind.MultiLineCommentTrivia))
        {
            return null;
        }

        return ParseDims(trivia[trivia.Count - 2].ToString());
    }

    /// <summary>The dims of a <c>/*[N]...*/</c> comment, outermost first, or null when it is not one.</summary>
    public static long[]? ParseDims(string comment)
    {
        if (!comment.StartsWith("/*[", StringComparison.Ordinal) || !comment.EndsWith("]*/", StringComparison.Ordinal))
            return null;

        string body = comment.Substring(3, comment.Length - 6);
        string[] parts = body.Split(new[] { "][" }, StringSplitOptions.None);
        long[] dims = new long[parts.Length];

        for (int index = 0; index < parts.Length; index++)
        {
            if (parts[index].Length == 0 || !parts[index].All(char.IsDigit) || !long.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out dims[index]))
                return null;
        }

        return dims;
    }

    /// <summary>The dims list as attribute arguments: <c>32</c>, <c>4, 8</c>.</summary>
    public static string DimsArguments(long[] dims) => string.Join(", ", dims.Select(dim => dim.ToString(CultureInfo.InvariantCulture)));

    // golib's GoMemberFact.Embedded, GoMemberFact.Tag and GoMemberFact.Dims.
    public const byte EmbeddedFact = 1;
    public const byte TagFact = 2;
    public const byte DimsFact = 3;
}

public sealed class MemberMarkerFinder : ISyntaxReceiver
{
    public List<MemberDeclarationSyntax> Members { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        switch (syntaxNode)
        {
            case FieldDeclarationSyntax field when MemberMarkers.HasEmbed(field) || MemberMarkers.TagOf(field) is not null || MemberMarkers.DimsOf(field) is not null:
            case PropertyDeclarationSyntax property when MemberMarkers.TagOf(property) is not null:
            case MethodDeclarationSyntax method when method.ParameterList.Parameters.Any(parameter => MemberMarkers.DimsOf(parameter) is not null):
            case StructDeclarationSyntax or ClassDeclarationSyntax when MemberMarkers.DimsOf((BaseTypeDeclarationSyntax)syntaxNode) is not null:
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

        void record(SyntaxNode declaration, ISymbol? symbol, string fact, string? value = null) =>
            add(declaration, symbol?.ContainingType, $"[global::go.GoMemberRecord({SymbolDisplay.FormatLiteral(symbol?.Name ?? "", true)}, global::go.GoMemberFact.{fact}{(value is null ? "" : ", " + value)})]");

        // An attribute on a generated partial of declaringType. A comment the converter writes only where a record
        // can carry it (a converted type is partial everywhere) is an error anywhere else, never a silent loss.
        void add(SyntaxNode declaration, INamedTypeSymbol? declaringType, string attribute)
        {
            if (declaringType is null || !GeneratedPartials.CanReopen(declaringType))
            {
                context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.UngeneratableRecord, declaration.GetLocation(), attribute, "its declaring type is not partial everywhere"));
                return;
            }

            string ns = declaration.GetNamespaceName();

            if (ns.Length == 0)
                return;

            if (!byType.TryGetValue(declaringType, out (string ns, List<string> records) slot))
            {
                slot = (ns, []);
                byType[declaringType] = slot;
            }

            slot.records.Add(attribute);
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
                            record(field, symbol, "Tag", SymbolDisplay.FormatLiteral(tag, true));

                        if (MemberMarkers.DimsOf(field) is { } fieldDims)
                            record(field, symbol, "Dims", MemberMarkers.DimsArguments(fieldDims));
                    }

                    break;

                case PropertyDeclarationSyntax property when MemberMarkers.TagOf(property) is { Length: > 0 } propertyTag:
                    record(property, semanticModel.GetDeclaredSymbol(property), "Tag", SymbolDisplay.FormatLiteral(propertyTag, true));
                    break;

                // A parameter is keyed as GoSigChanDir keys a method: name, typeof each parameter type (the one
                // spelling, GeneratedPartials.TypeOf), and position. A method an attribute cannot name (generic,
                // or a signature with a type parameter or pointer) keeps [GoArrayDims] in converted code, so a dims
                // comment on one is an error.
                case MethodDeclarationSyntax method:
                    if (semanticModel.GetDeclaredSymbol(method) is not IMethodSymbol methodSymbol)
                        break;

                    if (methodSymbol.IsGenericMethod || methodSymbol.Parameters.Any(parameter => !GeneratedPartials.IsNameable(parameter.Type)))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(GeneratorDiagnostics.UngeneratableRecord, method.GetLocation(), $"The array dims comment on {methodSymbol.Name}'s parameters", "a record cannot name a generic method or a type parameter; the parameter keeps [GoArrayDims]"));
                        break;
                    }

                    string parameterTypes = string.Join(", ", methodSymbol.Parameters.Select(parameter => GeneratedPartials.TypeOf(parameter.Type)));

                    for (int position = 0; position < method.ParameterList.Parameters.Count; position++)
                    {
                        if (MemberMarkers.DimsOf(method.ParameterList.Parameters[position]) is { } parameterDims)
                        {
                            add(method, methodSymbol.ContainingType, $"[global::go.GoParamDims({SymbolDisplay.FormatLiteral(methodSymbol.Name, true)}, " +
                                $"new global::System.Type[] {{ {parameterTypes} }}, {position}, {MemberMarkers.DimsArguments(parameterDims)})]");
                        }
                    }

                    break;

                // A type's dims ride a generated partial as the attribute itself, where golib's TypeStampedDims and
                // TypeGenerator read them.
                case BaseTypeDeclarationSyntax typeDeclaration when MemberMarkers.DimsOf(typeDeclaration) is { } typeDims:
                    add(typeDeclaration, semanticModel.GetDeclaredSymbol(typeDeclaration), $"[global::go.GoArrayDims({MemberMarkers.DimsArguments(typeDims)})]");
                    break;
            }
        }

        HashSet<string> hintNames = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<INamedTypeSymbol, (string ns, List<string> records)> pair in byType)
            context.AddSource(GeneratedPartials.HintName(hintNames, pair.Value.ns, pair.Key, "members"), GeneratedPartials.Source(pair.Value.ns, pair.Key, pair.Value.records));
    }
}
