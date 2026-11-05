// cscensus: the C# side of the init-statement scope census. For each C# file named on stdin
// ("key<TAB>path") it prints, in source pre-order, every scope-block candidate the converter emits:
//   bare   - a block that is a member of a statement list (the if/switch/type-switch init wrapper, or
//            a Go bare block);
//   elseif - an else clause whose block opens on the `} else {` line with its first statement on
//            that same line and whose last statement is an if (the else-if init form).
// For each it records the names the block itself declares (the scope the braces bound), the
// enclosing declaration space, and every occurrence of those names in that space outside the block.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

var opts = new JsonSerializerOptions { WriteIndented = false };
var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
string? line;
var stdout = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = false };

while ((line = Console.ReadLine()) != null)
{
    var parts = line.Split('\t', 2);
    if (parts.Length != 2) continue;
    var (key, path) = (parts[0], parts[1]);
    var text = File.ReadAllText(path);
    var tree = CSharpSyntaxTree.ParseText(text, parse, path);
    var root = tree.GetRoot();
    int errs = tree.GetDiagnostics().Count(d => d.Severity == DiagnosticSeverity.Error);
    if (errs > 0) Console.Error.WriteLine($"PARSE-ERRORS {errs} {key}");

    foreach (var node in root.DescendantNodes())
    {
        BlockSyntax? b = null;
        string form;
        if (node is BlockSyntax blk && blk.Parent is BlockSyntax or LabeledStatementSyntax or GlobalStatementSyntax)
        {
            b = blk; form = "bare";
        }
        else if (node is ElseClauseSyntax ec && ec.Statement is BlockSyntax eb && eb.Statements.Count >= 2
                 && eb.Statements.Last() is IfStatementSyntax eif
                 && (Line(eb.OpenBraceToken) == Line(eb.Statements[0].GetFirstToken())
                     || Line(eif.IfKeyword) == Line(eb.Statements[eb.Statements.Count - 2].GetLastToken())))
        {
            b = eb; form = "elseif";
        }
        else continue;

        var declared = Declared(b);
        var last = b.Statements.LastOrDefault();
        string lastKind = last switch
        {
            IfStatementSyntax => "if",
            SwitchStatementSyntax => "switch",
            null => "empty",
            _ => last.Kind().ToString()
        };
        // the statement kinds a converter init block may hold, in order
        var kinds = b.Statements.Select(s => s switch
        {
            IfStatementSyntax => "if",
            SwitchStatementSyntax => "switch",
            LocalDeclarationStatementSyntax => "decl",
            ExpressionStatementSyntax => "expr",
            _ => s.Kind().ToString()
        }).ToArray();

        SyntaxNode? space = null;
        bool labeledParent = false;
        if (form == "elseif")
        {
            // the else block is itself the scope today; dropping it would put the names in the scope of the
            // if statement that owns the else, i.e. that if's enclosing declaration space
            SyntaxNode p = b.Parent!.Parent!;            // the IfStatement owning the else
            while (p.Parent is ElseClauseSyntax) p = p.Parent.Parent!;
            space = EnclosingSpace(p, out labeledParent);
        }
        else space = EnclosingSpace(b, out labeledParent);

        var hits = new Dictionary<string, int>();
        var hitLines = new List<int>();
        bool labelInSpace = false;
        if (space != null)
        {
            foreach (var t in space.DescendantTokens())
            {
                if (t.SpanStart >= b.SpanStart && t.Span.End <= b.Span.End) continue;
                if (!t.IsKind(SyntaxKind.IdentifierToken) || !declared.Contains(t.ValueText)) continue;
                if (IsMemberName(t)) continue;
                hits[t.ValueText] = hits.GetValueOrDefault(t.ValueText) + 1;
                hitLines.Add(Line(t));
            }
            labelInSpace = DirectStatements(space).Any(s => s is LabeledStatementSyntax);
        }
        bool lambdaInBlock = b.DescendantNodes().Any(n => n is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);
        string first = b.Statements.Count > 0 ? b.Statements[0].ToString() : "";
        int nl = first.IndexOf('\n');
        if (nl >= 0) first = first[..nl];
        string cond = last is IfStatementSyntax ifs ? ifs.Condition.ToString() : "";

        var rec = new
        {
            key,
            line = Line(b.OpenBraceToken),
            endLine = Line(b.CloseBraceToken),
            hitLines,
            spaceLine = space is null ? 0 : space.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            form,
            lastKind,
            kinds,
            declared = declared.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            hits,
            labelInSpace,
            lambdaInBlock,
            labeledParent,
            spaceKind = space?.Kind().ToString() ?? "",
            first = first.Length > 200 ? first[..200] : first,
            cond = cond.Length > 200 ? cond[..200] : cond,
        };
        stdout.WriteLine(JsonSerializer.Serialize(rec, opts));
    }
}
stdout.Flush();

static int Line(SyntaxToken t) => t.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

// The declaration space a block's statements would join if its braces were dropped.
static SyntaxNode? EnclosingSpace(SyntaxNode n, out bool labeled)
{
    labeled = false;
    var p = n.Parent;
    if (p is LabeledStatementSyntax) { labeled = true; p = p.Parent; }
    return p switch
    {
        BlockSyntax => p,
        SwitchSectionSyntax => p.Parent,   // every section of a switch shares the switch block's space
        GlobalStatementSyntax => p.Parent,
        _ => p
    };
}

static IEnumerable<StatementSyntax> DirectStatements(SyntaxNode space) => space switch
{
    BlockSyntax b => b.Statements,
    SwitchStatementSyntax s => s.Sections.SelectMany(x => x.Statements),
    _ => Enumerable.Empty<StatementSyntax>()
};

// A token that names a member, an argument or an initializer target binds to no local.
static bool IsMemberName(SyntaxToken t)
{
    var p = t.Parent;
    if (p is IdentifierNameSyntax or GenericNameSyntax)
    {
        var gp = p.Parent;
        if (gp is MemberAccessExpressionSyntax ma && ma.Name == p) return true;
        if (gp is MemberBindingExpressionSyntax) return true;
        if (gp is NameColonSyntax or NameEqualsSyntax) return true;
        if (gp is QualifiedNameSyntax qn && qn.Right == p) return true;
        if (gp is AssignmentExpressionSyntax ae && ae.Left == p && ae.Parent is InitializerExpressionSyntax ie
            && ie.IsKind(SyntaxKind.ObjectInitializerExpression)) return true;
    }
    return false;
}

// The names whose scope is the block itself: locals and local functions its statements declare,
// and the expression variables of its statements' own scope (an if's condition, a switch's
// governing expression, an expression statement), never those inside a nested scope or a lambda.
static HashSet<string> Declared(BlockSyntax b)
{
    var names = new HashSet<string>(StringComparer.Ordinal);
    foreach (var s in b.Statements)
    {
        IEnumerable<SyntaxNode> region = s switch
        {
            LocalDeclarationStatementSyntax or ExpressionStatementSyntax or ReturnStatementSyntax
                or ThrowStatementSyntax or YieldStatementSyntax => new[] { s },
            IfStatementSyntax i => new SyntaxNode[] { i.Condition },
            SwitchStatementSyntax w => new SyntaxNode[] { w.Expression },
            LocalFunctionStatementSyntax lf => Array.Empty<SyntaxNode>(),
            _ => Array.Empty<SyntaxNode>()
        };
        if (s is LocalFunctionStatementSyntax lfs) names.Add(lfs.Identifier.ValueText);
        foreach (var r in region)
        {
            foreach (var d in r.DescendantNodesAndSelf(x => x is not AnonymousFunctionExpressionSyntax))
            {
                if (d is VariableDeclaratorSyntax v) names.Add(v.Identifier.ValueText);
                else if (d is SingleVariableDesignationSyntax sv) names.Add(sv.Identifier.ValueText);
            }
        }
    }
    return names;
}
