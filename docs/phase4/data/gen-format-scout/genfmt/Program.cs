// genfmt: reads every .cs under a root and reports, per file, layout defects against the project's C# style
// (docs/coding-style.md rules 1, 2, 7; src/.editorconfig). Two derivations, both from Roslyn's parse:
//   (b) NORMALIZE: the file differs from SyntaxNode.NormalizeWhitespace("    ", eol) of itself -> lines changed.
//   (c) BRACES: for every brace pair a syntax node owns (OpenBraceToken/CloseBraceToken):
//       kr      the open brace is not the first token on its line, and the pair spans lines (K&R, rule 1);
//       misalign  both braces lead their lines and sit in different columns;
//       depth   a brace that leads its line sits at a column other than 4 x its brace nesting depth;
//       body    a token leading a line directly inside the pair is indented other than brace column + 4;
//   plus line-level facts: tabs, trailing whitespace, mixed CRLF/LF, more than one blank line in a row.
// Output: one TSV row per file: path, lines, each count, normalize_changed_lines; then first examples to stderr.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

internal static class Program
{
    private static int Main(string[] args)
    {
        string root = args[0];
        string examplesPath = args.Length > 1 ? args[1] : null;
        using StreamWriter examples = examplesPath is null ? null : new StreamWriter(examplesPath);
        Console.WriteLine("path\tlines\tkr\tmisalign\tdepth\tbody\ttabs\ttrailing\tmixed_eol\tblank_runs\tnormalize_changed");
        foreach (string path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            string text = File.ReadAllText(path);
            SourceText source = SourceText.From(text);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview));
            SyntaxNode rootNode = tree.GetRoot();
            int kr = 0, misalign = 0, depth = 0, body = 0;
            void Example(string kind, int line)
            {
                examples?.WriteLine($"{kind}\t{Path.GetRelativePath(root, path)}\t{line + 1}\t{source.Lines[line].ToString().TrimEnd()}");
            }
            foreach ((SyntaxToken open, SyntaxToken close) in BracePairs(rootNode))
            {
                if (open.IsMissing || close.IsMissing)
                    continue;
                LinePosition o = open.GetLocation().GetLineSpan().StartLinePosition;
                LinePosition c = close.GetLocation().GetLineSpan().StartLinePosition;
                if (o.Line == c.Line)
                    continue;
                bool openLeads = LeadsLine(source, open);
                bool closeLeads = LeadsLine(source, close);
                if (!openLeads) { kr++; Example("kr", o.Line); }
                if (openLeads && closeLeads && o.Character != c.Character) { misalign++; Example("misalign", c.Line); }
                if (openLeads && o.Character != 4 * Level(open)) { depth++; Example("depth", o.Line); }
                else if (closeLeads && c.Character != 4 * Level(close)) { depth++; Example("depth", c.Line); }
            }
            for (int line = 0; line < source.Lines.Count; line++)
            {
                TextLine tl = source.Lines[line];
                string s = tl.ToString();
                int indent = s.Length - s.TrimStart().Length;
                if (s.Trim().Length == 0)
                    continue;
                SyntaxToken first = rootNode.FindToken(tl.Start + indent);
                if (first.SpanStart != tl.Start + indent)
                    continue; // inside a token (a multi-line string or comment)
                if (first.IsKind(SyntaxKind.CloseBraceToken) || first.IsKind(SyntaxKind.OpenBraceToken))
                    continue;
                if (IsStatementOrMemberStart(first) && indent != 4 * Level(first))
                {
                    body++; Example("body", line);
                }
            }
            int tabs = 0, trailing = 0, crlf = 0, lf = 0, blankRuns = 0, blank = 0;
            for (int i = 0; i < source.Lines.Count; i++)
            {
                TextLine tl = source.Lines[i];
                string s = tl.ToString();
                if (s.Contains('\t')) { tabs++; Example("tab", i); }
                if (s.Length > 0 && char.IsWhiteSpace(s[^1])) { trailing++; if (trailing <= 3) Example("trailing", i); }
                int brk = tl.EndIncludingLineBreak - tl.End;
                if (brk == 2) crlf++; else if (brk == 1) lf++;
                if (s.Trim().Length == 0) { blank++; if (blank == 2) { blankRuns++; Example("blankrun", i); } } else blank = 0;
            }
            int mixed = crlf > 0 && lf > 0 ? Math.Min(crlf, lf) : 0;
            string eol = crlf >= lf ? "\r\n" : "\n";
            string normalized = rootNode.NormalizeWhitespace("    ", eol).ToFullString();
            int changed = ChangedLines(text, normalized);
            Console.WriteLine($"{Path.GetRelativePath(root, path)}\t{source.Lines.Count}\t{kr}\t{misalign}\t{depth}\t{body}\t{tabs}\t{trailing}\t{mixed}\t{blankRuns}\t{changed}");
        }
        return 0;
    }

    private static (SyntaxToken open, SyntaxToken close) BracePairOf(SyntaxNode n) => n switch
    {
        BlockSyntax b => (b.OpenBraceToken, b.CloseBraceToken),
        BaseTypeDeclarationSyntax t => (t.OpenBraceToken, t.CloseBraceToken),
        NamespaceDeclarationSyntax ns => (ns.OpenBraceToken, ns.CloseBraceToken),
        AccessorListSyntax a => (a.OpenBraceToken, a.CloseBraceToken),
        SwitchStatementSyntax sw => (sw.OpenBraceToken, sw.CloseBraceToken),
        InitializerExpressionSyntax i => (i.OpenBraceToken, i.CloseBraceToken),
        SwitchExpressionSyntax se => (se.OpenBraceToken, se.CloseBraceToken),
        AnonymousObjectCreationExpressionSyntax ao => (ao.OpenBraceToken, ao.CloseBraceToken),
        PropertyPatternClauseSyntax pp => (pp.OpenBraceToken, pp.CloseBraceToken),
        _ => (default, default),
    };

    private static IEnumerable<(SyntaxToken, SyntaxToken)> BracePairs(SyntaxNode root)
    {
        foreach (SyntaxNode n in root.DescendantNodesAndSelf())
        {
            (SyntaxToken o, SyntaxToken c) = BracePairOf(n);
            // Only statement-shaped pairs: blocks, types, namespaces, accessor lists, switch statements. Initializers and
            // patterns follow expression layout, which rule 1 does not govern the same way.
            if (o.RawKind != 0 && n is BlockSyntax or BaseTypeDeclarationSyntax or NamespaceDeclarationSyntax or AccessorListSyntax or SwitchStatementSyntax)
                yield return (o, c);
        }
    }

    // The nesting level the project's style indents a token to: one per statement-shaped brace pair strictly around it,
    // plus one per switch section whose statements hold it (csharp_indent_case_contents = true).
    private static int Level(SyntaxToken t)
    {
        int level = 0;
        foreach (SyntaxNode n in t.Parent.AncestorsAndSelf())
        {
            if (n is BlockSyntax or BaseTypeDeclarationSyntax or NamespaceDeclarationSyntax or AccessorListSyntax or SwitchStatementSyntax)
            {
                (SyntaxToken o, SyntaxToken c) = BracePairOf(n);
                if (o.SpanStart < t.SpanStart && c.SpanStart > t.SpanStart)
                    level++;
            }
            else if (n is SwitchSectionSyntax section && section.Statements.Count > 0 && section.Statements[0].SpanStart <= t.SpanStart)
            {
                level++;
            }
            // an embedded statement without braces (if (c) return;, else, loops, using, lock, fixed) is indented one level
            if (n is StatementSyntax and not BlockSyntax && n.Parent is IfStatementSyntax or ElseClauseSyntax or WhileStatementSyntax
                    or ForStatementSyntax or CommonForEachStatementSyntax or DoStatementSyntax or UsingStatementSyntax
                    or LockStatementSyntax or FixedStatementSyntax
                && !(n.Parent is ElseClauseSyntax && n is IfStatementSyntax)
                && !(n.Parent is UsingStatementSyntax && n is UsingStatementSyntax))
            {
                level++;
            }
        }
        // a block that is a statement of a switch section, or the body of a member, adds no level of its own beyond its braces
        return level;
    }

    private static bool LeadsLine(SourceText s, SyntaxToken t)
    {
        TextLine line = s.Lines.GetLineFromPosition(t.SpanStart);
        return s.ToString(TextSpan.FromBounds(line.Start, t.SpanStart)).Trim().Length == 0;
    }

    private static int LineIndent(SourceText s, int line)
    {
        string text = s.Lines[line].ToString();
        return text.Length - text.TrimStart().Length;
    }

    private static bool DirectlyInside(SyntaxToken t, SyntaxToken open, SyntaxToken close)
    {
        // the innermost statement-shaped brace pair around t is (open, close)
        foreach (SyntaxNode n in t.Parent.AncestorsAndSelf())
        {
            if (n is not (BlockSyntax or BaseTypeDeclarationSyntax or NamespaceDeclarationSyntax or AccessorListSyntax or SwitchStatementSyntax))
                continue;
            (SyntaxToken o, SyntaxToken c) = BracePairOf(n);
            if (o.SpanStart < t.SpanStart && c.SpanStart > t.SpanStart)
                return o == open && c == close;
        }
        return false;
    }

    private static bool IsStatementOrMemberStart(SyntaxToken t)
    {
        SyntaxNode p = t.Parent;
        while (p != null && p.SpanStart == t.SpanStart)
        {
            if (p is StatementSyntax or MemberDeclarationSyntax or AccessorDeclarationSyntax or SwitchSectionSyntax or AttributeListSyntax)
                return p is not BlockSyntax;
            p = p.Parent;
        }
        return false;
    }

    private static int ChangedLines(string a, string b)
    {
        // blank lines are not compared: NormalizeWhitespace drops every blank line and the final newline, both of which
        // the project's style keeps between members
        string[] x = a.Replace("\r\n", "\n").Split('\n').Where(l => l.Trim().Length > 0).ToArray();
        string[] y = b.Replace("\r\n", "\n").Split('\n').Where(l => l.Trim().Length > 0).ToArray();
        // line multiset difference: lines of a not matched in b (order-insensitive, cheap and stable)
        Dictionary<string, int> counts = new();
        foreach (string l in y) counts[l] = counts.TryGetValue(l, out int n) ? n + 1 : 1;
        int changed = 0;
        foreach (string l in x)
        {
            if (counts.TryGetValue(l, out int n) && n > 0) counts[l] = n - 1;
            else changed++;
        }
        return changed;
    }
}
