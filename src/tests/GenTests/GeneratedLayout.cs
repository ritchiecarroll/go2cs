// GeneratedLayout.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace go2cs.Tests;

/// <summary>
/// Reads generated C# for the layout half of the project's style (docs/coding-style.md rules 1 and 2,
/// src/.editorconfig): Allman braces, each pair in one column at four columns per nesting level, every line-leading
/// statement, member, accessor, switch section and attribute list indented to its level, and one statement per line.
/// </summary>
/// <remarks>
/// Ported from the scout instrument genfmt (docs/phase4/data/gen-format-scout/genfmt, docs/PLAN-generated-code-format.md
/// section 1), which read 0 on these arms for every generator but LiteralInterceptorGenerator over 2,578 generated
/// files. Two arms are new here: a statement that shares its line with the statement before it, and an embedded
/// statement that shares its line with its <c>if</c>, <c>else</c> or loop header; <c>dotnet format</c> splits both.
/// Neither applies inside a braced block written on one line (<c>{ T c = v; return f(ref c); }</c>), which the formatter
/// keeps. Attribute lists sharing a line (<c>[A][B]</c>, <c>[A] [B] void M()</c>) are admitted by the owner's ruling
/// (2026-11-05). Line endings, trailing whitespace and blank lines are the whitespace half and are not read here.
/// </remarks>
internal static class GeneratedLayout
{
    /// <summary>
    /// Every layout finding in <paramref name="text"/>, one per line, as <c>kind line: text</c> with a 1-based line.
    /// </summary>
    public static List<string> Findings(string text)
    {
        SourceText source = SourceText.From(text);
        SyntaxNode root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
        List<string> findings = [];

        void Report(string kind, int line) =>
            findings.Add($"{kind} {line + 1}: {source.Lines[line].ToString().Trim()}");

        foreach (SyntaxNode node in root.DescendantNodesAndSelf())
        {
            (SyntaxToken open, SyntaxToken close) = BracePair(node);

            if (open.RawKind == 0 || open.IsMissing || close.IsMissing)
                continue;

            int openLine = Line(open), closeLine = Line(close);

            if (openLine == closeLine)
                continue;

            bool openLeads = LeadsLine(source, open), closeLeads = LeadsLine(source, close);
            int openColumn = Column(open), closeColumn = Column(close);

            if (!openLeads)
                Report("kr", openLine);
            else if (closeLeads && openColumn != closeColumn)
                Report("misalign", closeLine);
            else if (openColumn != 4 * Level(open))
                Report("depth", openLine);
            else if (closeLeads && closeColumn != 4 * Level(close))
                Report("depth", closeLine);
        }

        for (int line = 0; line < source.Lines.Count; line++)
        {
            string content = source.Lines[line].ToString();
            int indent = content.Length - content.TrimStart().Length;

            if (content.Trim().Length == 0)
                continue;

            SyntaxToken first = root.FindToken(source.Lines[line].Start + indent);

            // Inside a token (a multi-line string or comment), or a brace the pair arm already read.
            if (first.SpanStart != source.Lines[line].Start + indent || first.IsKind(SyntaxKind.OpenBraceToken) || first.IsKind(SyntaxKind.CloseBraceToken))
                continue;

            if (StartsStatementOrMember(first) && indent != 4 * Level(first))
                Report("indent", line);
        }

        foreach (StatementSyntax statement in root.DescendantNodes().OfType<StatementSyntax>())
        {
            if (statement is BlockSyntax || InsideOneLineBlock(statement))
                continue;

            int start = Line(statement.GetFirstToken());

            if (statement.Parent is BlockSyntax or SwitchSectionSyntax)
            {
                SyntaxNode? previous = statement.Parent.ChildNodes().TakeWhile(child => child != statement).LastOrDefault();

                if (previous is StatementSyntax && Line(previous.GetLastToken()) == start)
                    Report("two statements", start);
            }
            else if (IsEmbedded(statement) && Line(statement.GetFirstToken().GetPreviousToken()) == start)
            {
                Report("embedded", start);
            }
        }

        return findings;
    }

    private static int Line(SyntaxToken token) => token.GetLocation().GetLineSpan().StartLinePosition.Line;

    private static int Column(SyntaxToken token) => token.GetLocation().GetLineSpan().StartLinePosition.Character;

    private static bool LeadsLine(SourceText source, SyntaxToken token)
    {
        TextLine line = source.Lines.GetLineFromPosition(token.SpanStart);

        return source.ToString(TextSpan.FromBounds(line.Start, token.SpanStart)).Trim().Length == 0;
    }

    // Only statement-shaped pairs: initializers, switch expressions and patterns follow expression layout.
    private static (SyntaxToken open, SyntaxToken close) BracePair(SyntaxNode node) => node switch
    {
        BlockSyntax block => (block.OpenBraceToken, block.CloseBraceToken),
        BaseTypeDeclarationSyntax type => (type.OpenBraceToken, type.CloseBraceToken),
        NamespaceDeclarationSyntax scope => (scope.OpenBraceToken, scope.CloseBraceToken),
        AccessorListSyntax accessors => (accessors.OpenBraceToken, accessors.CloseBraceToken),
        SwitchStatementSyntax switchStatement => (switchStatement.OpenBraceToken, switchStatement.CloseBraceToken),
        _ => (default, default)
    };

    // A non-block statement that is an if, else or loop body, not counting `else if` or a `using` nested directly in a
    // `using` (coding-style rule 1's exception).
    private static bool IsEmbedded(StatementSyntax statement) =>
        statement is not BlockSyntax &&
        statement.Parent is IfStatementSyntax or ElseClauseSyntax or WhileStatementSyntax or ForStatementSyntax or
            CommonForEachStatementSyntax or DoStatementSyntax or UsingStatementSyntax or LockStatementSyntax or FixedStatementSyntax &&
        !(statement.Parent is ElseClauseSyntax && statement is IfStatementSyntax) &&
        !(statement.Parent is UsingStatementSyntax && statement is UsingStatementSyntax);

    private static bool InsideOneLineBlock(SyntaxNode node) =>
        node.Ancestors().OfType<BlockSyntax>().Any(block => Line(block.OpenBraceToken) == Line(block.CloseBraceToken));

    // The level the style indents a token to: one per statement-shaped brace pair strictly around it, one per switch
    // section whose statements hold it (csharp_indent_case_contents), one per embedded statement it sits in.
    private static int Level(SyntaxToken token)
    {
        int level = 0;

        foreach (SyntaxNode node in token.Parent!.AncestorsAndSelf())
        {
            (SyntaxToken open, SyntaxToken close) = BracePair(node);

            if (open.RawKind != 0 && open.SpanStart < token.SpanStart && close.SpanStart > token.SpanStart)
                level++;
            else if (node is SwitchSectionSyntax section && section.Statements.Count > 0 && section.Statements[0].SpanStart <= token.SpanStart)
                level++;

            if (node is StatementSyntax statement && IsEmbedded(statement))
                level++;
        }

        return level;
    }

    private static bool StartsStatementOrMember(SyntaxToken token)
    {
        for (SyntaxNode? node = token.Parent; node is not null && node.SpanStart == token.SpanStart; node = node.Parent)
        {
            if (node is StatementSyntax or MemberDeclarationSyntax or AccessorDeclarationSyntax or SwitchSectionSyntax or AttributeListSyntax)
                return node is not BlockSyntax;
        }

        return false;
    }
}
