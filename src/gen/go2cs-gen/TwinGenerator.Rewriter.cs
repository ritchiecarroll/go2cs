// TwinGenerator.Rewriter.cs - Gbtc
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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace go2cs;

public sealed partial class TwinGenerator
{
    // Turns a function's body into its twin's: awaits each call that has a twin, routes each go statement to
    // goǃtwin, and derives a ref local again after an await, since a ref local does not live across one.
    private sealed class Rewriter : CSharpSyntaxRewriter
    {
        private readonly Run m_run;
        private readonly SemanticModel m_model;
        private readonly RefAliases m_aliases;
        private List<List<string>> m_scopes = [];
        private int m_awaits, m_passive, m_temps;
        private StatementSyntax? m_current;
        private List<StatementSyntax> m_spills = [];
        private readonly HashSet<SyntaxNode> m_spilled = [];

        // The spill (12.1 item 5, section 12.4): each call this visit turns into a byref-param box twin call, with the box
        // each argument hands (null for a by-value argument). Such an argument is a VALUE in the rewritten call, so a
        // later awaitable in the same argument list holds no reference across its await: C# evaluates the box once, in
        // argument order, and keeps it in a temp over the await.
        private readonly Dictionary<SyntaxNode, string?[]> m_boxCalls = [];

        // The spill's second rule (12.4): each awaited call this visit moves ahead of its statement (`var ᴛ = await …`).
        // A call inside one of its arguments is judged against that moved statement, not the original one.
        private readonly HashSet<SyntaxNode> m_moving = [];

        public int Awaits, GoTwinned, Unrederivable, Literals, IfaceAwaits, Spills, RefBlocked;

        // The ref-blocked census (row 7, DATA-mn-row7-refblock.md): one row per call left plain because it could not
        // move ahead of its statement, `file|line|caller|callee|reasons|blocker`; and the last RefLikeBefore's verdict.
        public List<string> RefBlockedRows { get; } = [];
        private string LastRefBlockWhy = "";

        // Row 7, inline re-derivation: whether the last RefLikeBefore's only reason was a ref read after the call, the
        // call it stopped at, the reads to rewrite (each to its re-derivation source), and how many calls were served.
        private bool LastOnlyReadAfter;
        private SyntaxNode? LastMoved;
        private readonly Dictionary<IdentifierNameSyntax, string> m_inline = [];
        public int InlineRederived;

        // 7d (DATA-mn-origin-table.md): the line of each await the current state machine writes, in the order written,
        // which is source order and the compiler's state numbering (TwinStampTests holds every stamp to the compiler). The
        // method's own list is the twin's [GoTwinOf] stamp; an async literal swaps in a list of its own while its body is
        // visited and carries that list in its own stamp.
        private List<int> m_sites = [];

        public IReadOnlyList<int> Sites => m_sites;

        // Beside each line: the awaited call's place among its statement's calls of the same name ((ordinal << 16) | total).
        private List<int> m_calls = [];

        public IReadOnlyList<int> Calls => m_calls;

        // The line the compiler's sequence point for this await starts on: its statement's, or the clause of a
        // for statement or the expression body it sits in. A parked frame's line is read from it, as from the PDB before.
        private static int SequenceLine(SyntaxNode node) => SequenceNode(node).GetLocation().GetLineSpan().StartLinePosition.Line + 1;

        private static SyntaxNode SequenceNode(SyntaxNode node)
        {
            SyntaxNode at = node;

            for (SyntaxNode? current = node; current is not null; current = current.Parent)
            {
                if (current.Parent is ForStatementSyntax loop && (current == loop.Condition || loop.Incrementors.Contains(current) ||
                        loop.Declaration == current || loop.Initializers.Contains(current)))
                {
                    at = current;
                    break;
                }

                // Row 7 piece 6: the deconstructing form (two-variable range) alike.
                if (current.Parent is CommonForEachStatementSyntax each && current == each.Expression)
                {
                    at = current;
                    break;
                }

                if (current.Parent is DoStatementSyntax doLoop && current == doLoop.Condition)
                {
                    at = current;
                    break;
                }

                // A case guard (`case X when cond:`, or a switch expression arm's `when`) is a sequence point of its own.
                if (current.Parent is WhenClauseSyntax)
                {
                    at = current.Parent;
                    break;
                }

                if (current.Parent is ArrowExpressionClauseSyntax or LambdaExpressionSyntax && current is ExpressionSyntax)
                {
                    at = current;
                    break;
                }

                if (current is StatementSyntax and not BlockSyntax)
                {
                    at = current;
                    break;
                }
            }

            return at;
        }

        // The awaited call's place among the calls of its name in the sequence point's span, in evaluation order (a call's
        // arguments are evaluated before it): what the runtime counted in the plain method's IL, now counted here.
        private int CallOrdinal(SyntaxNode original)
        {
            if (original is not InvocationExpressionSyntax call || m_model.GetSymbolInfo(call).Symbol is not IMethodSymbol target)
                return 0;

            string name = target.Name.TrimStart('Δ');
            int ordinal = 0, total = 0;

            foreach (InvocationExpressionSyntax other in PostOrder(SequenceNode(original)))
            {
                if (m_model.GetSymbolInfo(other).Symbol is not IMethodSymbol callee || callee.Name.TrimStart('Δ') != name)
                    continue;

                total++;

                if (other == call)
                    ordinal = total;
            }

            return ordinal == 0 ? 0 : (ordinal << 16) | total;
        }

        private static IEnumerable<InvocationExpressionSyntax> PostOrder(SyntaxNode node)
        {
            foreach (SyntaxNode child in node.ChildNodes())
            {
                // A lambda's body runs later, not as part of this statement.
                if (child is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax or LocalFunctionStatementSyntax)
                    continue;

                foreach (InvocationExpressionSyntax inner in PostOrder(child))
                    yield return inner;
            }

            if (node is InvocationExpressionSyntax invocation)
                yield return invocation;
        }

        private void NoteSite(SyntaxNode original)
        {
            m_sites.Add(SequenceLine(original));
            m_calls.Add(CallOrdinal(original));
        }

        // The [GoTwinOf] stamp of the function this twin is generated from, with the given await lines.
        public string Stamp() => m_method is IMethodSymbol origin ? StampFor(origin, m_sites, m_calls) : "";

        // A trimmed build keeps the methods the stamp names while it keeps the twin (G's condition 2 on 7d): the runtime
        // reads the origin by reflection, and a trimmed-away origin would leave the frame without its Go name.
        public string KeepOrigin() => m_method is IMethodSymbol origin
            ? "[global::System.Diagnostics.CodeAnalysis.DynamicDependency(\"" + origin.Name + (origin.Arity > 0 ? "``" + origin.Arity : "") + "\", typeof(" + HostText(origin.ContainingType) + "))]"
            : "";

        private static string HostText(INamedTypeSymbol host) =>
            (host.IsGenericType ? host.ConstructUnboundGenericType() : host).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        internal static string StampFor(IMethodSymbol origin, IReadOnlyList<int> sites, IReadOnlyList<int> calls)
        {
            string hostText = HostText(origin.ContainingType);

            return "[global::go.golib.GoTwinOf(typeof(" + hostText + "), \"" + origin.Name + "\", new global::System.Type[] { " +
                   string.Join(", ", origin.Parameters.Select(parameter => StampType(parameter.Type))) + " }, new int[] { " +
                   string.Join(", ", sites) + " }, new int[] { " + string.Join(", ", calls) + " })]";
        }

        // A parameter's type as the stamp writes it: null where it is or holds a type parameter (an attribute cannot name
        // one), a generic type as its open definition, anything else as itself.
        private static string StampType(ITypeSymbol type) => type switch
        {
            ITypeParameterSymbol => "null",
            INamedTypeSymbol { IsGenericType: true } generic => "typeof(" + generic.ConstructUnboundGenericType().ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")",
            IArrayTypeSymbol or IPointerTypeSymbol when MentionsTypeParameter(type) => "null",
            IFunctionPointerTypeSymbol => "null",
            _ => "typeof(" + type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")"
        };

        private static bool MentionsTypeParameter(ITypeSymbol type) => type switch
        {
            ITypeParameterSymbol => true,
            IArrayTypeSymbol array => MentionsTypeParameter(array.ElementType),
            IPointerTypeSymbol pointer => MentionsTypeParameter(pointer.PointedAtType),
            _ => false
        };

        // An async literal's twin, stamped with its own await lines (and the enclosing function as its origin).
        private ExpressionSyntax StampLiteral(ExpressionSyntax twin, IReadOnlyList<int> sites, IReadOnlyList<int> calls)
        {
            if (m_method is not IMethodSymbol origin)
                return twin;

            AttributeListSyntax stamp = ParseAttributeList(StampFor(origin, sites, calls)).WithTrailingTrivia(Space);

            return twin switch
            {
                // The stamp joins the literal's own attribute lists, never replaces them: the converter's [GoFunc] name and
                // no-inlining mark belong to the twin as much as to the plain half.
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.WithAttributeLists(parenthesized.AttributeLists.Add(stamp)),
                SimpleLambdaExpressionSyntax simple => ParenthesizedLambdaExpression(simple.AsyncKeyword, ParameterList(SingletonSeparatedList(simple.Parameter)),
                        simple.ArrowToken, simple.Block, simple.ExpressionBody)
                    .WithAttributeLists(simple.AttributeLists.Add(stamp)),
                _ => twin
            };
        }

        private static AttributeListSyntax ParseAttributeList(string text) =>
            ((CompilationUnitSyntax)ParseCompilationUnit(text + " class ᴛstamp { }")).Members[0].AttributeLists[0];

        // N3's D3 core: the method's own func-value parameters whose pair test is HOISTED to the twin's entry (never
        // reassigned), the awaits that pair tests added (they never make a literal can-park), and the method itself.
        public readonly List<string> HoistedPairs = [];

        // The static-pair cache fields this twin is the first to need, declared in its class after it.
        public readonly List<string> CacheFields = [];
        private readonly ISymbol? m_method;
        private readonly bool m_blockBody;
        private int m_pairAwaits;

        // N2: the box twin's receiver (`this ref T t` becomes `this ж<T> Ꮡt`), whose ref the prologue declares; Rule R
        // derives it again after each await from the box (or, under GoTwinsBoxCopy, the probe's control, from a copy).
        public string? BoxReceiver { get; init; }

        // byref-param (12.1): the boxed `ref` parameters (`ref T p` becomes `ж<T> Ꮡp`), each a ref local of the twin as the
        // receiver is, declared by the prologue and derived again by Rule R.
        public List<string>? BoxParams { get; init; }

        // PB-5 (12.5): the stack locals this twin declares as heap locals (`ref var x = ref heap<T>(init, out var Ꮡx)`),
        // each then a ref local over its box that Rule R derives again after each await.
        public Dictionary<string, ITypeSymbol>? Promoted { get; init; }
        private bool m_boxSeeded, m_paramsSeeded;

        public Rewriter(Run run, SemanticModel model, MethodDeclarationSyntax method)
        {
            m_run = run;
            m_model = model;
            m_method = model.GetDeclaredSymbol(method);
            m_blockBody = method.Body is not null;

            // A box or path that is assigned anywhere in the function may name another variable after an await, unless
            // every assignment of it is the converter's own re-alias form (C1, TwinGenerator.RefAliases.cs).
            m_aliases = new RefAliases(method);
        }

        private static SyntaxToken Twin(SyntaxToken identifier) =>
            ParseToken(identifier.Text + Suffix).WithLeadingTrivia(identifier.LeadingTrivia).WithTrailingTrivia(identifier.TrailingTrivia);

        private static ExpressionSyntax Rename(ExpressionSyntax expression) => expression switch
        {
            IdentifierNameSyntax name => name.WithIdentifier(Twin(name.Identifier)),
            GenericNameSyntax generic => generic.WithIdentifier(Twin(generic.Identifier)),
            MemberAccessExpressionSyntax access => access.WithName((SimpleNameSyntax)Rename(access.Name)),
            _ => expression
        };

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (m_passive > 0 || !m_run.Sites.TryGetValue(node, out Site? site))
                return base.VisitInvocationExpression(node);

            if (site.Go == GoKind.Literal)
                return m_run.LiteralLaunches(site) ? GoLiteral(node) : base.VisitInvocationExpression(node);

            if (site.Go == GoKind.Group && m_run.CanPark(site.GoTarget))
                return GoGroup(node);

            // A call through a func value in this (existing) twin: test its target for a pair and await the twin. A
            // void call is rewritten at its statement (VisitExpressionStatement); a value call here.
            // A value call inside a larger statement moves ahead of it into a temp, as an awaited call spills, so nothing
            // evaluated before it (a ref returned by a slice's indexer, say) lives across the await; where the call
            // cannot move, it stays plain.
            if (site is { Active: true, Delegate: true, Go: GoKind.None } && node.Parent is not ExpressionStatementSyntax &&
                m_current is not null && PairShape(node) is { Void: false } shape)
            {
                RefLikeBefore(node, out bool pairMovable);

                if (pairMovable)
                {
                    (string test, string twin, string plain, string? read) = BuildPairCall(node, shape);
                    string temp = "ᴛtwin" + ++m_temps;

                    m_spilled.Add(node);
                    NoteSite(node);

                    // A field or element callee is read once, ahead of the test (the func-value seat).
                    if (read is not null)
                        m_spills.Add(ParseStatement(read));

                    m_spills.Add(ParseStatement("var " + temp + " = " + test + " ? await " + twin + " : " + plain + ";"));
                    Spills++;

                    return IdentifierName(temp).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                }
            }

            if (!m_run.ShouldAwait(site))
            {
                // N2: a call to a box twin with no box to name stays plain, and is named in the report (SN-7).
                if (site.Active && !site.PairBody && m_run.IsBoxTwinTarget(site.Target) && !m_run.BoxCallable(site))
                    m_run.NoteBoxPlain(node, site.Target!, text: site.ArgPlain);

                return base.VisitInvocationExpression(node);
            }

            // A ref struct value evaluated before the await (a u8 literal passed as a format, say) cannot live
            // across it. Where everything evaluated earlier is inert, the awaited call moves ahead of its
            // statement; anywhere else the call stays a blocking one, which hands its P off as a thread park does.
            bool spill = RefLikeBefore(node, out bool movable);

            // Row 7, inline re-derivation (DATA-mn-row7-refblock.md): a call that cannot move only because a ref local is read
            // after it in the same evaluation (an if, else-if or while header, a case guard) is awaited in place, and each
            // such read becomes its re-derivation source, read through the box after the await. Every ref must have one.
            if (spill && !movable && LastOnlyReadAfter && InlineSources(node, LastMoved) is { } sources)
            {
                foreach ((IdentifierNameSyntax read, string source) in sources)
                    m_inline[read] = source;

                InlineRederived++;
                spill = false;
            }

            if (spill && !movable)
            {
                RefBlocked++;
                RefBlockedRows.Add(System.IO.Path.GetFileName(node.SyntaxTree.FilePath) + "|" + (node.GetLocation().GetLineSpan().StartLinePosition.Line + 1) + "|" +
                                   (m_method?.Name ?? "?") + "|" + (site.Target?.Name ?? "?") + "|" + LastRefBlockWhy);
                return base.VisitInvocationExpression(node);
            }

            if (spill)
                m_moving.Add(node);

            if (HandsArgBoxes(site, node.ArgumentList.Arguments.Count) is { } handsBoxes)
                m_boxCalls[node] = handsBoxes;

            InvocationExpressionSyntax visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;

            // An interface call reaches its twin through the hidden interface's call helper, or by name where the
            // twin is a default-bodied member of the interface itself.
            InvocationExpressionSyntax call = site.Interface && m_run.IfaceHidden
                ? m_run.IfaceCall(site.IfaceTarget!, visited)
                : visited.WithExpression(Rename(visited.Expression)).WithoutTrivia();

            // The Gosched-park seat: runtime.Gosched's twin is golib's builtin.Goschedꞏtwin, not a member of runtime.
            if (m_run.IsGoschedIntrinsic(site.Target))
                call = call.WithExpression(ParseExpression("global::go.builtin.Goschedꞏtwin"));

            // N2: a call to a box twin through a `this ref` receiver hands the twin the receiver's box (9.1) instead.
            if (site.BoxOf is { } box && !site.ViaBoxOverload && m_run.IsBoxTwinTarget(site.Target) && call.Expression is MemberAccessExpressionSyntax member)
            {
                call = call.WithExpression(member.WithExpression(ParseExpression(box)));
                m_run.BoxCalls++;
            }
            else if (site.ViaBoxOverload && m_run.IsBoxTwinTarget(site.Target))
            {
                m_run.BoxCalls++;
            }
            // byref-param (12.1): each by-ref argument becomes the box it names, read once, at the call; under
            // GoTwinsBoxArgCopy (the call-site control) a fresh COPY of the argument's value instead.
            else if (HandsArgBoxes(site, call.ArgumentList.Arguments.Count) is { } boxes)
            {
                SeparatedSyntaxList<ArgumentSyntax> arguments = call.ArgumentList.Arguments;

                for (int i = 0; i < boxes.Length; i++)
                {
                    if (boxes[i] is not { } argumentBox)
                        continue;

                    ExpressionSyntax handed = m_run.BoxArgCopy
                        ? ParseExpression("global::go.builtin.Ꮡ(" + arguments[i].Expression.WithoutTrivia().ToFullString() + ")")
                        : ParseExpression(argumentBox);

                    arguments = arguments.Replace(arguments[i], arguments[i].WithRefKindKeyword(default).WithExpression(handed.WithTriviaFrom(arguments[i].Expression)));
                }

                call = call.WithArgumentList(call.ArgumentList.WithArguments(arguments));
                m_run.BoxCalls++;
            }

            ExpressionSyntax awaited = AwaitExpression(Token(SyntaxKind.AwaitKeyword).WithTrailingTrivia(Space), call);
            NoteSite(node);

            if (site.Interface)
                IfaceAwaits++;

            if (spill)
            {
                string temp = "ᴛtwin" + ++m_temps;

                m_spilled.Add(node);
                m_spills.Add(ParseStatement("var " + temp + " = " + awaited.ToFullString() + ";"));
                m_awaits++;
                Awaits++;
                Spills++;

                return IdentifierName(temp).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
            }

            // Row 7 piece 6: an await that IS a for statement's iterator stays a statement expression (CS0201 when
            // parenthesized: go/parser's `for (; p.tok != token.EOF; Ꮡp.next())`); VisitForStatement adds the re-derivations.
            // The closure cure: an awaited for INITIALIZER is a statement expression too (runtime's unwinder, `for (Ꮡu.initAt(…);`).
            if (node.Parent is not (ExpressionStatementSyntax or ArrowExpressionClauseSyntax or LambdaExpressionSyntax) &&
                !(node.Parent is ForStatementSyntax loop && (loop.Incrementors.Contains(node) || loop.Initializers.Contains(node))))
                awaited = ParenthesizedExpression(awaited);

            m_awaits++;
            Awaits++;

            return awaited.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
        }

        // The boxes an awaited call hands its byref-param box twin, when the call becomes one: the branch order above (a
        // receiver box and a box overload come first).
        private string?[]? HandsArgBoxes(Site site, int arguments) =>
            site.BoxOf is not null && !site.ViaBoxOverload && m_run.IsBoxTwinTarget(site.Target) ? null :
            site.ViaBoxOverload && m_run.IsBoxTwinTarget(site.Target) ? null :
            site.ArgBoxes is { } boxes && m_run.IsParamBoxTwin(site.Target) && boxes.Length == arguments ? boxes : null;

        // Whether a ref struct value is evaluated before this call inside its statement, and whether the call can
        // move ahead of that statement: it can when the enclosing statement list is visiting the statement, the
        // statement reaches the call unconditionally, and nothing is evaluated before the call but constants,
        // static method names and calls that already moved.
        private bool RefLikeBefore(InvocationExpressionSyntax node, out bool movable)
        {
            bool refLike = false, refTarget = false;
            bool inert = true;
            SyntaxNode child = node;

            // The ref-blocked census (row 7, DATA-mn-row7-refblock.md): the FIRST thing that made the call unmovable.
            string? blocker = null;

            void Block(string why)
            {
                if (inert)
                    blocker = why;

                inert = false;
            }

            void Earlier(ExpressionSyntax expression)
            {
                TypeInfo info = m_model.GetTypeInfo(expression);
                refLike |= info.Type is { IsRefLikeType: true } || info.ConvertedType is { IsRefLikeType: true };

                ExpressionSyntax bare = expression;

                while (bare is ParenthesizedExpressionSyntax parenthesized)
                    bare = parenthesized.Expression;

                // Row 7, the Go-order move (DATA-mn-row7-refblock.md, COORD's ruling): a PURE READ evaluated before the call
                // no longer pins it. Go orders only calls, method calls, receives and logical operators; a variable's read
                // is unordered against a call, and gc reads after the calls (go1.26.11, MnGoOrder: a local, a parameter,
                // a field of a value or through a pointer, a unary operand, a binary left operand and a numeric cast all
                // read the call's write; a nil pointer panics after the call's side effect). So the call moves ahead, as
                // gc runs it, and Rule S derives the refs again before the read.
                if (!m_spilled.Contains(bare) && !bare.IsKind(SyntaxKind.Utf8StringLiteralExpression) && !m_model.GetConstantValue(bare).HasValue &&
                    !IsPureRead(bare))
                    Block("earlier:" + bare.Kind());
            }

            SyntaxNode? moved = null;

            for (SyntaxNode? parent = node.Parent; parent is not null && moved is null; child = parent, parent = parent.Parent)
            {
                if (parent is StatementSyntax statement)
                {
                    if (!ReferenceEquals(statement, m_current))
                        Block("nested:" + statement.Kind());
                    else if (statement is not (ExpressionStatementSyntax or LocalDeclarationStatementSyntax or ReturnStatementSyntax))
                        Block("stmt:" + statement.Kind());

                    break;
                }

                if (parent is ArrowExpressionClauseSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax)
                {
                    Block("in:" + parent.Kind());
                    break;
                }

                switch (parent)
                {
                    case ArgumentSyntax or ParenthesizedExpressionSyntax or CastExpressionSyntax or EqualsValueClauseSyntax or VariableDeclaratorSyntax or MemberAccessExpressionSyntax:
                        break;
                    case BaseObjectCreationExpressionSyntax when child is ArgumentListSyntax:
                        break;
                    // Stage (c): nothing is evaluated before a call's receiver, nor before a binary operator's left
                    // operand (`parksT().Next(…)`, `parksB() && …`), so a call there moves ahead of its statement.
                    case InvocationExpressionSyntax invocation when child == invocation.Expression:
                        break;
                    case BinaryExpressionSyntax binary when child == binary.Left:
                        break;
                    // Row 7, the Go-order move: a call in a binary operator's RIGHT operand moves when the left operand is a
                    // pure read (`b + f()` reads b after f in gc). A logical operator stays ordered (Go orders `&&` and `||`,
                    // and the right side runs only on the left's value), as does `??`.
                    case BinaryExpressionSyntax binary when child == binary.Right &&
                                                           !binary.IsKind(SyntaxKind.LogicalAndExpression) && !binary.IsKind(SyntaxKind.LogicalOrExpression) &&
                                                           !binary.IsKind(SyntaxKind.CoalesceExpression):
                        Earlier(binary.Left);
                        break;
                    // A unary operator applies after its operand: nothing is evaluated before a call under `-`, `!`, `~`, `+`.
                    case PrefixUnaryExpressionSyntax unary when unary.IsKind(SyntaxKind.UnaryMinusExpression) || unary.IsKind(SyntaxKind.UnaryPlusExpression) ||
                                                                unary.IsKind(SyntaxKind.LogicalNotExpression) || unary.IsKind(SyntaxKind.BitwiseNotExpression):
                        break;
                    case VariableDeclarationSyntax declaration:
                        if (declaration.Variables.Count != 1)
                            Block("multi-declarator");

                        break;
                    case BaseArgumentListSyntax list:
                        m_boxCalls.TryGetValue(list.Parent!, out string?[]? handed);

                        for (int index = 0; index < list.Arguments.Count; index++)
                        {
                            ArgumentSyntax argument = list.Arguments[index];

                            if (argument == child)
                                break;

                            // Stage (c): net/http's `refererForURL(…, ref (req.URL).DerefOrNull(), req.Header.Get(…))`. A
                            // by-ref argument evaluated before the await would hold its reference across it (CS8178).
                            // The spill (12.4): not one the box call hands as a box, which is a value.
                            refLike |= !argument.RefKindKeyword.IsKind(SyntaxKind.None) && handed?[index] is null;
                            Earlier(argument.Expression);
                        }

                        break;
                    case InvocationExpressionSyntax invocation when child is ArgumentListSyntax:
                        if (m_model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { IsStatic: true, MethodKind: MethodKind.Ordinary or MethodKind.LocalFunction })
                        {
                            ExpressionSyntax receiver = invocation.Expression is MemberAccessExpressionSyntax access ? access.Expression : invocation.Expression;
                            refLike |= ByRefReceiver(receiver);
                            Earlier(receiver);
                        }

                        // The spill's second rule (12.4): the enclosing call moves ahead of its statement, so this call
                        // runs inside `var ᴛ = await <that call>`: nothing outside it (a store through a ref, say) is
                        // evaluated before this await. The walk stops; this call awaits in place there, never moving on.
                        if (m_moving.Contains(invocation))
                        {
                            moved = invocation;
                            Block("inside-moving-call");
                        }

                        break;
                    // Stage (c): crypto/x509's `s.haveSum[sha256.Sum224(…)]`. A struct indexer takes `this` by reference,
                    // so a struct receiver that reaches a ref local, evaluated before the await, would hold the ref
                    // across it (CS9217): the awaited index moves ahead where it can, and stays plain where it cannot.
                    case ElementAccessExpressionSyntax element when child == element.ArgumentList:
                        refLike |= ByRefReceiver(element.Expression);
                        Earlier(element.Expression);
                        Block("element-index");
                        break;
                    // A COMPOUND store through a ref (`c.scanAlloc += f()`, stage (c)) takes the same spill: `x op= y` reads x
                    // once, and the spec leaves that read and the call on the right unordered, as for a plain store.
                    case AssignmentExpressionSyntax assignment when child == assignment.Right &&
                                                                    (assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) || WritesThroughRef(assignment.Left)):
                        // C2 (COORD's approval, 2026-11-15): a store into a ref local, or through one or a ref-returning
                        // member (`err = …`, `(t.Trailer, err) = …`), would hold the target's reference over the await
                        // (CS9217 / CS8178). The awaited value moves ahead instead, the live ref locals are derived
                        // again (Rule S), and the store runs after: every store still lands in the box the Go variable
                        // names. Go's two-phase assignment leaves the order between reading a plain variable on the
                        // left and the call on the right unspecified, so reading the left after the call is Go-legal.
                        if (WritesThroughRef(assignment.Left))
                            refTarget = true;
                        else if (m_model.GetSymbolInfo(assignment.Left).Symbol is not (ILocalSymbol { RefKind: RefKind.None } or IParameterSymbol { RefKind: RefKind.None }))
                            Block("store-left:" + assignment.Left.Kind());

                        break;
                    default:
                        foreach (SyntaxNode sibling in parent.ChildNodes())
                        {
                            if (sibling == child)
                                break;

                            if (sibling is ExpressionSyntax expression)
                                Earlier(expression);
                        }

                        Block("in:" + parent.Kind());
                        break;
                }
            }

            movable = inert;

            bool readAfter = !refLike && !refTarget && RefReadAfter(node, moved);
            LastOnlyReadAfter = readAfter;
            LastMoved = moved;
            LastRefBlockWhy = string.Join("+", new[] { refLike ? "ref-like-before" : null, refTarget ? "store-through-ref" : null,
                    readAfter ? "ref-read-after:" + RederivableReads(node, moved) : null }
                .Where(reason => reason is not null)) + "|" + (blocker ?? "movable");

            return refLike || refTarget || readAfter;
        }

        // Stage (c): a receiver passed by reference (a struct value: its instance members take `this` by ref) that reaches
        // a reference -- a ref local, a ref-returning property such as ж<T>.Value (h2_bundle's `Ꮡsc.Value.logf(…)`) or a
        // ref-returning call -- so evaluating it before an await holds that reference across it.
        private bool ByRefReceiver(ExpressionSyntax receiver) =>
            m_model.GetTypeInfo(receiver).Type is { IsValueType: true } && WritesThroughRef(receiver);

        // Row 7, the Go-order move's operand classes (COORD's ruling; each probed in src/tools/mnspike2/MnGoOrder against
        // gc): a read of a local, a parameter (a box parameter or receiver included), a ref local or a field; a member
        // chain of such reads, a field of a value or through a pointer or box (a nil one panics after the call, as in gc);
        // a numeric cast of a pure read; a unary operator on one. A call, an indexer, an element access, an object
        // creation (the slice-to-array conversion among them: gc converts BEFORE the call) or anything else is not.
        private bool IsPureRead(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    return IsPureRead(parenthesized.Expression);
                case IdentifierNameSyntax identifier:
                    return m_model.GetSymbolInfo(identifier).Symbol is ILocalSymbol or IParameterSymbol or IFieldSymbol or IPropertySymbol { ReturnsByRef: true };
                case MemberAccessExpressionSyntax member when member.IsKind(SyntaxKind.SimpleMemberAccessExpression):
                    return m_model.GetSymbolInfo(member).Symbol is IFieldSymbol or IPropertySymbol { ReturnsByRef: true } &&
                           (IsPureRead(member.Expression) || member.Expression is ThisExpressionSyntax ||
                            m_model.GetSymbolInfo(member.Expression).Symbol is INamedTypeSymbol or INamespaceSymbol);
                case CastExpressionSyntax cast:
                    return IsNumeric(m_model.GetTypeInfo(cast.Type).Type) && IsNumeric(m_model.GetTypeInfo(cast.Expression).Type) && IsPureRead(cast.Expression);
                case PrefixUnaryExpressionSyntax unary when unary.IsKind(SyntaxKind.UnaryMinusExpression) || unary.IsKind(SyntaxKind.UnaryPlusExpression) ||
                                                            unary.IsKind(SyntaxKind.LogicalNotExpression) || unary.IsKind(SyntaxKind.BitwiseNotExpression):
                    return IsPureRead(unary.Operand);
                default:
                    return false;
            }

            static bool IsNumeric(ITypeSymbol? type) =>
                type is { SpecialType: >= SpecialType.System_Char and <= SpecialType.System_Double or SpecialType.System_IntPtr or SpecialType.System_UIntPtr } ||
                type is { TypeKind: TypeKind.Enum };
        }

        // Stage (c): whether a ref local is read LATER in the call's statement than the call (math/big's
        // `@new<Float>().SetPrec(n).SetMode(x.mode)`, crypto/rsa's `encryptOAEP(opts.Hash.New(), opts.Hash.New(), …)`).
        // Awaited in place, the ref would be read after the await, before Rule R derives it again at the next statement
        // (CS9217); spilled, the call runs first as it did, and the rest of the statement reads the ref derived again.
        // Inside a call that moves ahead of its statement (the spill's second rule, 12.4), only that call's own text runs
        // before the ref is derived again, so only it is scanned.
        private bool RefReadAfter(InvocationExpressionSyntax node, SyntaxNode? moved = null) => RefReadsAfter(node, moved).Any();

        // The reads RefReadAfter counts, in text order (the ref-blocked census names them).
        private IEnumerable<IdentifierNameSyntax> RefReadsAfter(InvocationExpressionSyntax node, SyntaxNode? moved = null)
        {
            if (m_current is null || !m_current.Span.Contains(node.Span))
                yield break;

            foreach (IdentifierNameSyntax name in m_current.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (moved is not null && !moved.Span.Contains(name.Span))
                    continue;

                // Only a read evaluated with the statement's own expression: one in a NESTED statement (a switch case,
                // an if body: MnMixedStorm's worker) comes after Rule R has derived the ref again.
                if (name.SpanStart >= node.Span.End && EvaluatedWith(name.Ancestors().OfType<StatementSyntax>().FirstOrDefault(), name, node) &&
                    IsRefLocal(m_model.GetSymbolInfo(name).Symbol))
                {
                    yield return name;
                }
            }
        }

        // Row 7, inline re-derivation: each read RefReadAfter counts, with its re-derivation source (the text after
        // ` = ref ` in its live re-derivation), or null when any of them has none.
        private List<(IdentifierNameSyntax read, string source)>? InlineSources(InvocationExpressionSyntax node, SyntaxNode? moved)
        {
            string[] live = [.. m_scopes.SelectMany(scope => scope)];
            List<(IdentifierNameSyntax, string)> sources = [];

            foreach (IdentifierNameSyntax read in RefReadsAfter(node, moved))
            {
                string prefix = read.Identifier.Text + " = ref ";
                string? rederive = live.LastOrDefault(entry => entry.StartsWith(prefix, System.StringComparison.Ordinal));

                if (rederive is null)
                    return null;

                sources.Add((read, rederive.Substring(prefix.Length).TrimEnd().TrimEnd(';')));
            }

            return sources.Count > 0 ? sources : null;
        }

        // The ref-blocked census: whether every ref local read after the call is one Rule S can derive again (it has a
        // live re-derivation), the precondition of deriving it again inline instead.
        private string RederivableReads(InvocationExpressionSyntax node, SyntaxNode? moved)
        {
            string[] names = RefReadsAfter(node, moved).Select(name => name.Identifier.Text).Distinct().ToArray();
            string[] live = [.. m_scopes.SelectMany(scope => scope)];
            string[] missing = names.Where(name => !live.Any(rederive => rederive.StartsWith(name + " = ref ", System.StringComparison.Ordinal))).ToArray();

            return missing.Length == 0 ? "rederivable" : "unrederivable(" + string.Join(",", missing) + ")";
        }

        // Whether a read owned by `owner` is evaluated with the awaited call, before any statement derives the refs again:
        // the statement in hand itself, or (row 7 piece 6, DATA-mn-row7-r5.md) an `else if` of its chain whose CONDITION
        // holds both the call and the read (archive/zip's checksumReader.Read, go/types' conversion). An else-if is a nested
        // IfStatement, so the read was not counted and the call was awaited in place: CS9217. Its body and any later
        // else-if are entered with the refs derived again (Rule H), so only its own condition counts.
        private bool EvaluatedWith(StatementSyntax? owner, SyntaxNode read, SyntaxNode call)
        {
            // The closure cure: a label is not a statement of its own (crypto/x509's `candidateLoop: foreach (…)`, whose
            // header reads `opts` after an await): the statement being visited is the one the labels wrap.
            StatementSyntax? visited = m_current;

            while (visited is LabeledStatementSyntax labeled)
                visited = labeled.Statement;

            if (ReferenceEquals(owner, m_current) || ReferenceEquals(owner, visited))
                return true;

            if (owner is not IfStatementSyntax nested || !nested.Condition.Span.Contains(read.Span) || !nested.Condition.Span.Contains(call.Span))
                return false;

            for (SyntaxNode current = nested; current.Parent is ElseClauseSyntax { Parent: IfStatementSyntax outer }; current = outer)
            {
                if (ReferenceEquals(outer, m_current))
                    return true;
            }

            return false;
        }

        // A ref local of the twin. N2: in a box twin the receiver is one too (the prologue's `ref var t = ref
        // Ꮡt.DerefOrNull()`), though the plain method's model binds it as the `this ref` parameter (closure arm N's
        // CS9217 in runtime's gcWork.putObjBatch).
        private bool IsRefLocal(ISymbol? symbol) =>
            symbol is ILocalSymbol { RefKind: not RefKind.None } ||
            Promoted is { } promotedLocals && symbol is ILocalSymbol { RefKind: RefKind.None } promotedLocal && promotedLocals.ContainsKey(promotedLocal.Name) &&
            SymbolEqualityComparer.Default.Equals(promotedLocal.ContainingSymbol, m_method) ||
            BoxReceiver is { } receiver && symbol is IParameterSymbol { Ordinal: 0, RefKind: RefKind.Ref, ContainingSymbol: IMethodSymbol { IsExtensionMethod: true } } parameter &&
            parameter.Name == receiver ||
            BoxParams is { } boxed && symbol is IParameterSymbol { RefKind: RefKind.Ref } boxedParameter &&
            SymbolEqualityComparer.Default.Equals(boxedParameter.ContainingSymbol, m_method) && boxed.Contains(boxedParameter.Name);

        // Whether an assignment's left side writes through a reference: a ref local, a member or element reached from
        // one, or a ref-returning property or call (the field-access generator's `GoFieldAccess.get_F(ж<T>)`); a
        // tuple's elements each count.
        private bool WritesThroughRef(ExpressionSyntax left)
        {
            if (left is TupleExpressionSyntax tuple)
                return tuple.Arguments.Any(argument => WritesThroughRef(argument.Expression));

            foreach (SyntaxNode node in left.DescendantNodesAndSelf())
            {
                if (node is not ExpressionSyntax expression)
                    continue;

                ISymbol? symbol = m_model.GetSymbolInfo(expression).Symbol;

                if (IsRefLocal(symbol))
                    return true;

                switch (symbol)
                {
                    case IPropertySymbol { ReturnsByRef: true }:
                    case IMethodSymbol { ReturnsByRef: true }:
                        return true;
                }
            }

            return false;
        }

        private static IdentifierNameSyntax Launcher(InvocationExpressionSyntax node) =>
            IdentifierName(Identifier("goǃtwin")).WithTriviaFrom(node.Expression);

        // goǃ(X, a, b) becomes goǃtwin(X, Xꞏtwin, a, b).
        private SyntaxNode GoGroup(InvocationExpressionSyntax node)
        {
            InvocationExpressionSyntax visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;
            SeparatedSyntaxList<ArgumentSyntax> arguments = visited.ArgumentList.Arguments;
            List<SyntaxNodeOrToken> list = [arguments[0].WithoutTrailingTrivia(), Token(SyntaxKind.CommaToken).WithTrailingTrivia(Space), Argument(Rename(arguments[0].Expression.WithoutTrivia()))];

            for (int i = 1; i < arguments.Count; i++)
            {
                list.Add(arguments.GetSeparator(i - 1));
                list.Add(arguments[i]);
            }

            GoTwinned++;

            return visited.WithExpression(Launcher(node)).WithArgumentList(visited.ArgumentList.WithArguments(SeparatedList<ArgumentSyntax>(list)));
        }

        // goǃ(() => {...}) becomes goǃtwin(() => {...}, async () => {...}): the literal as it stands, then its
        // twin under a #line directive that puts it on the literal's own lines.
        private SyntaxNode GoLiteral(InvocationExpressionSyntax node)
        {
            ParenthesizedLambdaExpressionSyntax lambda = (ParenthesizedLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression;
            ExpressionSyntax original = (ExpressionSyntax)Visit(lambda)!;

            List<List<string>> scopes = m_scopes;
            int awaits = m_awaits;
            List<int> sites = m_sites, calls = m_calls;
            m_scopes = [];
            m_sites = [];
            m_calls = [];

            CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(lambda.Body)!;
            List<int> literalSites = m_sites, literalCalls = m_calls;

            m_scopes = scopes;
            m_awaits = awaits;
            m_sites = sites;
            m_calls = calls;

            LinePosition start = lambda.SyntaxTree.GetLineSpan(lambda.Span).StartLinePosition;
            string directive = "\r\n#line " + (start.Line + 1) + " \"" + lambda.SyntaxTree.FilePath + "\"\r\n" + new string(' ', start.Character);

            ExpressionSyntax twin = StampLiteral(lambda.WithBody(body).WithoutTrivia()
                    .WithModifiers(lambda.Modifiers.Add(Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(Space))), literalSites, literalCalls)
                .WithLeadingTrivia(ParseLeadingTrivia(directive));

            GoTwinned++;
            Literals++;

            // A literal with arguments (`go func(i int) { ... }(v)`): they follow the twin, as goǃtwin's ladder takes them.
            SeparatedSyntaxList<ArgumentSyntax> arguments = node.ArgumentList.Arguments;
            List<SyntaxNodeOrToken> list = [Argument(original.WithoutTrailingTrivia()), Token(SyntaxKind.CommaToken), Argument(twin)];

            for (int i = 1; i < arguments.Count; i++)
            {
                list.Add(arguments.GetSeparator(i - 1));
                list.Add((ArgumentSyntax)Visit(arguments[i])!);
            }

            return node.WithExpression(Launcher(node)).WithArgumentList(node.ArgumentList.WithArguments(SeparatedList<ArgumentSyntax>(list)));
        }

        // An interceptor is keyed on the call it was written for and does not reach a copy of that call, so a
        // string literal passed as an object is given its Go type here.
        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (!node.IsKind(SyntaxKind.StringLiteralExpression))
                return node;

            TypeInfo info = m_model.GetTypeInfo(node);

            if (info.Type?.SpecialType != SpecialType.System_String || info.ConvertedType?.SpecialType != SpecialType.System_Object)
                return node;

            return ParenthesizedExpression(CastExpression(ParseTypeName("global::go.@string"), node.WithoutTrivia())).WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            if (PairLiteral(node) is { } pair)
                return pair;

            m_passive++;
            SyntaxNode? visited = base.VisitParenthesizedLambdaExpression(node);
            m_passive--;
            return visited;
        }

        public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            if (PairLiteral(node) is { } pair)
                return pair;

            m_passive++;
            SyntaxNode? visited = base.VisitSimpleLambdaExpression(node);
            m_passive--;
            return visited;
        }

        // PB-5 (12.5): the one variable of a promoted stack local's declaration, or null.
        private string? PromotedName(StatementSyntax statement) =>
            m_passive == 0 && Promoted is { } promoted &&
            statement is LocalDeclarationStatementSyntax { Declaration: { Type: not RefTypeSyntax, Variables: [var variable] } } &&
            promoted.ContainsKey(variable.Identifier.ValueText) && m_model.GetDeclaredSymbol(variable) is ILocalSymbol
                ? variable.Identifier.ValueText
                : null;

        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (PromotedName(node) is not { } name)
                return base.VisitLocalDeclarationStatement(node);

            LocalDeclarationStatementSyntax visited = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node)!;
            ExpressionSyntax? init = visited.Declaration.Variables[0].Initializer?.Value;
            string type = Promoted![name].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string heap = "global::go.builtin.heap<" + type + ">(" + (init is null ? "" : init.WithoutTrivia().ToFullString() + ", ") + "out var Ꮡ" + name + ")";

            return ParseStatement("ref var " + Run.Local(name) + " = ref " + heap + ";").WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (m_passive == 0 && node.Expression is InvocationExpressionSyntax launch && m_run.Sites.TryGetValue(launch, out Site? goSite) &&
                goSite is { Active: true, Go: GoKind.Other } && GoPairLaunch(launch) is { } launched)
            {
                return ParseStatement(launched).WithTriviaFrom(node);
            }

            if (m_passive == 0 && node.Expression is InvocationExpressionSyntax call && m_run.Sites.TryGetValue(call, out Site? site) &&
                site is { Active: true, Delegate: true, Go: GoKind.None } && PairShape(call) is { } shape)
            {
                (string test, string twin, string plain, string? read) = BuildPairCall(call, shape);
                NoteSite(call);
                string pairIf = "if (" + test + ") await " + twin + "; else " + plain + ";";

                // A field or element callee is read once, into a temp the test and the call share (the func-value seat).
                return ParseStatement(read is null ? pairIf : "{ " + read + " " + pairIf + " }").WithTriviaFrom(node);
            }

            return base.VisitExpressionStatement(node);
        }

        // Fix 6 of 9.3: `go f(args)` on a func value held by a local or parameter, in a twin: when f is pair-bound the
        // goroutine launches the pair's twin through goǃtwin (golib's unwrap names it by f's plain body), anything else
        // launches as before. A value-returning f and a ref argument keep today's launch. `defer f()` is untouched: a
        // deferred call runs in the frame's synchronous unwind.
        private string? GoPairLaunch(InvocationExpressionSyntax node)
        {
            SeparatedSyntaxList<ArgumentSyntax> arguments = node.ArgumentList.Arguments;

            if (!m_run.Pairs || node.Expression is not IdentifierNameSyntax { Identifier.Text: "goǃ" } ||
                arguments.Count is < 1 or > MaxGoArguments + 1 || arguments.Any(argument => !argument.RefKindKeyword.IsKind(SyntaxKind.None) || argument.NameColon is not null) ||
                arguments[0].Expression is not IdentifierNameSyntax callee ||
                m_model.GetTypeInfo(callee).Type is not INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { ReturnsVoid: true } invoke } type ||
                invoke.Parameters.Length != arguments.Count - 1 || Run.TwinDelegateType(type) is not { } twinType)
            {
                return null;
            }

            string name = callee.Identifier.Text;
            string rest = string.Concat(arguments.Skip(1).Select(argument => ", " + ((ExpressionSyntax)Visit(argument.Expression)!).WithoutTrivia().ToFullString()));
            string pairName = "ᴘpair" + ++m_temps;

            GoTwinned++;

            return "if (" + name + "?.Target is global::go.golib.GoFuncPair " + pairName + ") goǃtwin(" + name + ".Invoke, ((" + twinType + ")" +
                   pairName + ".Twin)" + rest + "); else goǃ(" + name + rest + ");";
        }

        // Whether a call through a func value takes the pair test, and its twin delegate type; no side effects.
        private (string TwinType, bool Void)? PairShape(InvocationExpressionSyntax node)
        {
            if (!m_run.Pairs || !Run.PairableCallee(node.Expression, m_model) ||
                m_model.GetTypeInfo(node.Expression).Type is not INamedTypeSymbol { TypeKind: TypeKind.Delegate } type ||
                Run.TwinDelegateType(type) is not { } twinType || RefLikeBefore(node, out _))
            {
                return null;
            }

            return (twinType, type.DelegateInvokeMethod!.ReturnsVoid);
        }

        // N3's D3 core, the CALL: a call through a func value held by a local or parameter (named once, so reading its
        // target and calling it see one value). Its target is tested for a pair -- once at the twin's entry for a
        // parameter never reassigned (HOISTED), else at the call -- and the twin, typed by the Go signature's canonical
        // twin delegate, is awaited; anything else is called as before. Null where the shape is not covered: the call
        // stays plain, as today.
        // The func-value seat: a FIELD or ELEMENT callee (t.run, fs[i], m[k]) is read once into a temp first (Read, a
        // statement the caller places ahead of the test), so the test and the call see one value and an index expression
        // runs once, in its own order before the arguments, as C# evaluates a delegate call's target first.
        private (string Test, string Twin, string Plain, string? Read) BuildPairCall(InvocationExpressionSyntax node, (string TwinType, bool Void) shape)
        {
            string twinType = shape.TwinType;
            string? read = null;
            string name;

            if (node.Expression is IdentifierNameSyntax identifier)
            {
                name = identifier.Identifier.Text;
            }
            else
            {
                name = "ᴛfv" + ++m_temps;
                read = "var " + name + " = " + ((ExpressionSyntax)Visit(node.Expression)!).WithoutTrivia().ToFullString() + ";";
            }

            string arguments = ((ArgumentListSyntax)Visit(node.ArgumentList)!).WithoutTrivia().ToFullString();
            bool hoisted = read is null && m_blockBody && m_model.GetSymbolInfo(node.Expression).Symbol is IParameterSymbol parameter &&
                           SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, m_method) && !m_aliases.Assigned(name);

            string pairName;
            string test;

            if (hoisted)
            {
                pairName = name + "ꞏpair";
                test = pairName + " is not null";

                if (!HoistedPairs.Contains(name))
                    HoistedPairs.Add(name);
            }
            else
            {
                pairName = "ᴘpair" + ++m_temps;
                test = name + "?.Target is global::go.golib.GoFuncPair " + pairName;
            }

            m_awaits++;
            m_pairAwaits++;
            Awaits++;
            m_run.PairTests++;

            return (test, "((" + twinType + ")" + pairName + ".Twin)" + arguments, name + arguments, read);
        }

        // N3's D3 core, the PAIR: a literal in this twin body whose body can park -- awaits from pair tests do not count,
        // since a literal is never paired for calling a func value (the caller rule's question) -- becomes its plain and
        // twin lambdas in one expression (so C# gives them one closure), handed out as its own delegate type bound to the
        // pair's Invoke. An expression body pairs too: it is how the converter spells a bound method value (fix 2 of 9.3),
        // the receiver snapshotted once into a local both halves capture. A goǃ literal is the go statement's (GoLiteral);
        // a signature that cannot carry a twin stays plain.
        private ExpressionSyntax? PairLiteral(LambdaExpressionSyntax node)
        {
            if (!m_run.Pairs || m_passive > 0 || node.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
                node.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "goǃ" } } } } ||
                m_model.GetTypeInfo(node).ConvertedType is not INamedTypeSymbol { TypeKind: TypeKind.Delegate } type ||
                Run.TwinDelegateType(type) is not { } twinType)
            {
                return null;
            }

            List<List<string>> scopes = m_scopes;
            int awaits = m_awaits, pairAwaits = m_pairAwaits, totalAwaits = Awaits, temps = m_temps, tests = m_run.PairTests;
            List<string> hoisted = [.. HoistedPairs];
            List<int> sites = m_sites, calls = m_calls;
            m_scopes = [];
            m_awaits = 0;
            m_pairAwaits = 0;
            m_sites = [];
            m_calls = [];

            CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(node.Body)!;
            bool parks = m_awaits - m_pairAwaits > 0;
            List<int> literalSites = m_sites, literalCalls = m_calls;

            m_scopes = scopes;
            m_awaits = awaits;
            m_pairAwaits = pairAwaits;
            m_sites = sites;
            m_calls = calls;

            if (!parks)
            {
                // Not can-park: undo what the twin visit counted, and leave the literal plain.
                Awaits = totalAwaits;
                m_temps = temps;
                m_run.PairTests = tests;
                HoistedPairs.Clear();
                HoistedPairs.AddRange(hoisted);
                return null;
            }

            m_passive++;
            ExpressionSyntax plain = (ExpressionSyntax)(node is ParenthesizedLambdaExpressionSyntax parenthesized
                ? base.VisitParenthesizedLambdaExpression(parenthesized)!
                : base.VisitSimpleLambdaExpression((SimpleLambdaExpressionSyntax)node)!);
            m_passive--;

            LinePosition start = node.SyntaxTree.GetLineSpan(node.Span).StartLinePosition;
            string directive = "\r\n#line " + (start.Line + 1) + " \"" + node.SyntaxTree.FilePath + "\"\r\n" + new string(' ', start.Character);

            ExpressionSyntax twin = StampLiteral(node.WithBody(body).WithoutTrivia()
                    .WithModifiers(node.Modifiers.Add(Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(Space))), literalSites, literalCalls)
                .WithLeadingTrivia(ParseLeadingTrivia(directive));

            // C3 (COORD's approval, 2026-11-15): a literal with an EXPLICIT return type (`(uint32, error) () => {…}`)
            // keeps it on the plain half; the twin half returns the task of it, as its twin delegate does (CS8934).
            if (twin is ParenthesizedLambdaExpressionSyntax { ReturnType: { } declared } typed)
            {
                IMethodSymbol invoke = type.DelegateInvokeMethod!;
                string task = invoke.ReturnsVoid
                    ? "global::go.golib.GoTask"
                    : "global::go.golib.GoTask<" + invoke.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">";

                twin = typed.WithReturnType(ParseTypeName(task).WithTriviaFrom(declared));
            }

            string plainType = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string pairClass = m_run.PairClass(type, twinType);

            m_run.PairsMade++;
            Literals++;

            string pairInvoke = "new " + pairClass + "(" + plain.WithoutTrivia().ToFullString() + ", " + twin.ToFullString() + ").Invoke";

            // Fix 5: the sole argument of the converter's `new D(literal)` binds that creation to the pair itself, never a
            // delegate over a delegate (whose target would hide the pair from the call test).
            if (node.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Arguments.Count: 1, Parent: ObjectCreationExpressionSyntax creation } } &&
                SymbolEqualityComparer.Default.Equals(m_model.GetTypeInfo(creation).Type, type))
            {
                return ParseExpression(pairInvoke).WithTriviaFrom(node);
            }

            return ParseExpression("new " + plainType + "(" + pairInvoke + ")").WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) =>
            PairGroup(node) ?? base.VisitObjectCreationExpression(node);

        // Fix 5: the converter's `new D(F)` over a STATIC function that can park becomes a pair of F and its twin, in this
        // twin body. A bound or generic method group, or a signature that cannot carry a twin, stays as it is.
        private ExpressionSyntax? PairGroup(ObjectCreationExpressionSyntax node)
        {
            if (!m_run.Pairs || m_passive > 0 || node.Initializer is not null || node.ArgumentList is not { Arguments.Count: 1 } list ||
                m_model.GetTypeInfo(node).Type is not INamedTypeSymbol { TypeKind: TypeKind.Delegate } type ||
                Run.TwinDelegateType(type) is not { } twinType)
            {
                return null;
            }

            ExpressionSyntax argument = list.Arguments[0].Expression;

            if (argument is not (IdentifierNameSyntax or MemberAccessExpressionSyntax) ||
                m_model.GetSymbolInfo(argument).Symbol is not IMethodSymbol { IsStatic: true, MethodKind: MethodKind.Ordinary, ReducedFrom: null, IsGenericMethod: false } method ||
                !m_run.CanPark(Run.Normalize(method)))
            {
                return null;
            }

            return StaticPair(argument, node, method, type, twinType);
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node) =>
            m_inline.TryGetValue(node, out string? source)
                ? ParenthesizedExpression(ParseExpression(source)).WithTriviaFrom(node)
                : PairImplicitGroup(node) ?? base.VisitIdentifierName(node);

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) =>
            PairImplicitGroup(node) ?? base.VisitMemberAccessExpression(node);

        // The func-value seat: a method group the converter hands to a delegate-typed slot WITHOUT `new D(F)` (a struct
        // literal's field, `new("receive", rowReceive)`; a slice literal's element, `[rowReceive, rowWait]`; an argument)
        // is the same func value as fix 5's `new D(F)`, so it becomes the same pair. Without it, P5's table-driven main
        // was twinned by the caller rule and still held its carrier: the stored delegates were plain, so the pair test
        // found none and called them plain (MnTableMain: steps=1 parks=0 thread-blocks=2).
        private ExpressionSyntax? PairImplicitGroup(ExpressionSyntax node)
        {
            // A call's own target, the name half of a member access (the whole access is visited instead), and nameof are
            // not implicit conversions to pair here.
            if (!m_run.Pairs || m_passive > 0 || node.SyntaxTree != m_model.SyntaxTree ||
                node.Parent is InvocationExpressionSyntax invocation && invocation.Expression == node ||
                node.Parent is MemberAccessExpressionSyntax access && access.Name == node ||
                node.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } } })
            {
                return null;
            }

            if (m_model.GetSymbolInfo(node).Symbol is not IMethodSymbol { IsStatic: true, MethodKind: MethodKind.Ordinary, ReducedFrom: null, IsGenericMethod: false } method ||
                m_model.GetTypeInfo(node).ConvertedType is not INamedTypeSymbol { TypeKind: TypeKind.Delegate } type ||
                Run.TwinDelegateType(type) is not { } twinType ||
                !m_run.CanPark(Run.Normalize(method)))
            {
                return null;
            }

            // Fix 5's own `new D(F)` (the creation IS the delegate: PairGroup's) is not paired twice. Any other one-argument
            // creation is a struct literal (Go `job{run: work}` -> `new job(work)`), whose argument is an implicit group like
            // any other: decided by the creation's type, as the literal path decides it, never by the argument count.
            if (node.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Arguments.Count: 1, Parent: ObjectCreationExpressionSyntax creation } } &&
                SymbolEqualityComparer.Default.Equals(m_model.GetTypeInfo(creation).Type, type))
            {
                return null;
            }

            // A func value handed to a golib builtin (`goǃ(F, …)`, a defer) is that builtin's own twin machinery's to
            // place; the pair is for a value the program stores and calls itself.
            if (node.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax call } } &&
                m_model.GetSymbolInfo(call).Symbol is IMethodSymbol { ContainingType: { Name: "builtin", ContainingNamespace.Name: "go" } })
            {
                return null;
            }

            return StaticPair(node, node, method, type, twinType);
        }

        // The pair of static function `method` (named by `argument`) and its twin, as a `type` delegate bound to the pair's
        // Invoke, in place of `replaced`: fix 5's `new D(F)` and the seat's implicit group alike.
        private ExpressionSyntax StaticPair(ExpressionSyntax argument, SyntaxNode replaced, IMethodSymbol method, INamedTypeSymbol type, string twinType)
        {
            ExpressionSyntax node = (ExpressionSyntax)replaced;
            string plainType = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string group = argument.WithoutTrivia().ToFullString();
            string pair = "new " + plainType + "(new " + m_run.PairClass(type, twinType) + "(" + group + ", " + Rename(argument.WithoutTrivia()).ToFullString() + ").Invoke)";

            // Made ONCE per function and delegate type: a field of this twin's own class, filled on first use (a race only
            // makes a second, equal pair). Where the class cannot hold a field, the pair is made per conversion.
            if (m_method?.ContainingType is { TypeKind: TypeKind.Class or TypeKind.Struct } owner)
            {
                (string cache, bool isNew) = m_run.StaticPairCache(owner, method, type);

                if (isNew)
                {
                    m_run.PairsMade++;
                    CacheFields.Add("private static " + plainType + "? " + cache + ";");
                }

                return ParseExpression("(" + cache + " ??= " + pair + ")").WithTriviaFrom(node);
            }

            m_run.PairsMade++;
            return ParseExpression(pair).WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            m_passive++;
            SyntaxNode? visited = base.VisitAnonymousMethodExpression(node);
            m_passive--;
            return visited;
        }

        public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            m_passive++;
            SyntaxNode? visited = base.VisitLocalFunctionStatement(node);
            m_passive--;
            return visited;
        }

        // ---- ref locals ----

        private void Register(StatementSyntax statement)
        {
            if (PromotedName(statement) is { } promoted)
            {
                m_scopes[m_scopes.Count - 1].Add(Run.Local(promoted) + " = ref Ꮡ" + promoted + ".ValueSlot;");
                return;
            }

            string? rederive = m_aliases.Rederive(statement, out string? name);

            if (name is null)
                return;

            if (rederive is null)
                Unrederivable++;
            else
                m_scopes[m_scopes.Count - 1].Add(rederive);
        }

        private List<StatementSyntax> Live(SyntaxTriviaList leading)
        {
            List<StatementSyntax> live = [];

            foreach (string rederive in m_scopes.SelectMany(scope => scope))
                live.Add(ParseStatement(rederive).WithLeadingTrivia(live.Count == 0 ? leading : TriviaList(Space)));

            return live;
        }

        private static bool IsJump(StatementSyntax statement) =>
            statement is ReturnStatementSyntax or BreakStatementSyntax or ContinueStatementSyntax or GotoStatementSyntax or ThrowStatementSyntax or YieldStatementSyntax;

        // Rule R: a statement that awaited is followed, on its own line, by every live ref local derived again.
        private SyntaxList<StatementSyntax> Statements(SyntaxList<StatementSyntax> statements)
        {
            m_scopes.Add([]);

            // N2: the box twin's receiver is a ref local over the box, declared by the prologue before the body.
            if (BoxReceiver is { } receiver && !m_boxSeeded)
            {
                m_boxSeeded = true;
                m_scopes[^1].Add(Run.Local(receiver) + " = ref " + (m_run.BoxCopy ? "ᴛcopy" + receiver : "Ꮡ" + receiver + ".DerefOrNull()") + ";");
            }

            // byref-param: each boxed parameter likewise.
            if (BoxParams is { } boxedParams && !m_paramsSeeded)
            {
                m_paramsSeeded = true;

                foreach (string name in boxedParams)
                    m_scopes[^1].Add(Run.Local(name) + " = ref " + (m_run.BoxCopy ? "ᴛcopy" + name : "Ꮡ" + name + ".DerefOrNull()") + ";");
            }
            List<StatementSyntax> result = [];

            foreach (StatementSyntax statement in statements)
            {
                int before = m_awaits;
                StatementSyntax? outer = m_current;
                List<StatementSyntax> outerSpills = m_spills;

                m_current = statement;
                m_spills = [];

                StatementSyntax visited = (StatementSyntax)Visit(statement)!;
                List<StatementSyntax> spills = m_spills;

                m_current = outer;
                m_spills = outerSpills;

                // Rule S: a call that moved ahead of its statement stands before it on the statement's own line,
                // and the live ref locals are derived again after EACH moved call (stage (c): crypto/rsa's
                // `encryptOAEP(opts.Hash.New(), opts.Hash.New(), …)` moves two, and the second reads `opts`).
                if (spills.Count > 0)
                {
                    spills[0] = spills[0].WithLeadingTrivia(visited.GetLeadingTrivia());

                    for (int i = 1; i < spills.Count; i++)
                        spills[i] = spills[i].WithLeadingTrivia(Space);

                    foreach (StatementSyntax spill in spills)
                    {
                        result.Add(spill);
                        result.AddRange(Live(TriviaList(Space)));
                    }

                    visited = visited.WithLeadingTrivia(Space);
                }

                Register(statement);

                List<StatementSyntax> live = m_awaits != before && !IsJump(statement) ? Live(TriviaList(Space)) : [];

                if (live.Count == 0)
                {
                    result.Add(visited);
                    continue;
                }

                result.Add(visited.WithoutTrailingTrivia());
                live[live.Count - 1] = live[live.Count - 1].WithTrailingTrivia(visited.GetTrailingTrivia());
                result.AddRange(live);
            }

            m_scopes.RemoveAt(m_scopes.Count - 1);

            return List(result);
        }

        public override SyntaxNode? VisitBlock(BlockSyntax node)
        {
            if (m_passive > 0)
                return base.VisitBlock(node);

            return node.WithStatements(Statements(node.Statements));
        }

        // Rule H: a body entered after its header awaited starts with the live ref locals derived again.
        private StatementSyntax Enter(StatementSyntax body)
        {
            if (body is BlockSyntax block)
            {
                if (block.Statements.Count == 0)
                    return body;

                List<StatementSyntax> entry = Live(block.Statements[0].GetLeadingTrivia());

                if (entry.Count == 0)
                    return body;

                entry.Add(block.Statements[0].WithLeadingTrivia(Space));
                entry.AddRange(block.Statements.Skip(1));

                return block.WithStatements(List(entry));
            }

            List<StatementSyntax> wrapped = Live(TriviaList(Space));

            if (wrapped.Count == 0)
                return body;

            wrapped.Add(body.WithoutTrivia().WithLeadingTrivia(Space));

            return Block(Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(body.GetLeadingTrivia()), List(wrapped),
                Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(Space).WithTrailingTrivia(body.GetTrailingTrivia()));
        }

        // How many awaits the header expressions of the statement in hand raised.
        private int HeaderAwaits;

        private TNode? Head<TNode>(TNode? node) where TNode : SyntaxNode
        {
            int before = m_awaits;
            TNode? visited = (TNode?)Visit(node);
            HeaderAwaits += m_awaits - before;
            return visited;
        }

        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitIfStatement(node);

            HeaderAwaits = 0;
            ExpressionSyntax condition = Head(node.Condition)!;
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;
            ElseClauseSyntax? otherwise = (ElseClauseSyntax?)Visit(node.Else);

            if (stale)
            {
                statement = Enter(statement);
                otherwise = otherwise?.WithStatement(Enter(otherwise.Statement));
            }

            return node.WithCondition(condition).WithStatement(statement).WithElse(otherwise);
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitWhileStatement(node);

            HeaderAwaits = 0;
            ExpressionSyntax condition = Head(node.Condition)!;
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;

            return node.WithCondition(condition).WithStatement(stale ? Enter(statement) : statement);
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitDoStatement(node);

            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;
            HeaderAwaits = 0;
            ExpressionSyntax condition = Head(node.Condition)!;

            return node.WithCondition(condition).WithStatement(HeaderAwaits > 0 ? Enter(statement) : statement);
        }

        public override SyntaxNode? VisitForStatement(ForStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitForStatement(node);

            HeaderAwaits = 0;
            VariableDeclarationSyntax? declaration = Head(node.Declaration);
            // The closure cure: an awaited initializer cannot stay in the header (C# parses `for (await f(), …` as a declaration
            // of a type named `await`), so when one awaits, EVERY initializer moves, in order, into the statement prelude
            // (Rule S's spills): they run once, before the loop, as the header ran them, and the live ref locals are
            // derived again after each. runtime's unwinder loops, `for (Ꮡu.initAt(…); u.valid(); Ꮡu.next())`. A DECLARING
            // initializer (go/internal/gcimporter's `for ((nint i, nint n) = (0, r.Len()); …)`) stays in the header: its
            // await is nested, so it parses, and moved before the loop its variables would leave the loop's scope (CS0136).
            int initialAwaits = m_awaits;
            List<ExpressionSyntax> started = [.. node.Initializers.Select(item => Head(item)!)];
            SeparatedSyntaxList<ExpressionSyntax> initializers = SeparatedList(started, node.Initializers.GetSeparators());
            bool declaring = node.Initializers.Any(item => item.DescendantNodesAndSelf().OfType<DeclarationExpressionSyntax>().Any());

            if (m_awaits > initialAwaits && !declaring)
            {
                m_spills.AddRange(started.Select(item => (StatementSyntax)ExpressionStatement(item.WithoutTrivia())));
                initializers = default;
            }

            ExpressionSyntax? condition = Head(node.Condition);

            // Row 7 piece 6 (DATA-mn-row7-r5.md): an iterator that awaited is followed, IN the iterator list, by the live
            // ref locals derived again (`await Ꮡp.nextꞏtwin(), p = ref Ꮡp.DerefOrNull()`), so the condition reads them
            // fresh. The iterator stays the loop's own, so a `continue` and a labeled `goto continue_x` still run it.
            List<ExpressionSyntax> stepped = [];

            foreach (ExpressionSyntax item in node.Incrementors)
            {
                int before = m_awaits;
                stepped.Add(Head(item)!);

                if (m_awaits > before)
                {
                    foreach (string rederive in m_scopes.SelectMany(scope => scope))
                        stepped.Add(ParseExpression(rederive.TrimEnd().TrimEnd(';')).WithLeadingTrivia(Space));
                }
            }

            SeparatedSyntaxList<ExpressionSyntax> incrementors = stepped.Count == node.Incrementors.Count
                ? SeparatedList(stepped, node.Incrementors.GetSeparators())
                : SeparatedList(stepped);
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;

            return node.WithDeclaration(declaration).WithInitializers(initializers).WithCondition(condition).WithIncrementors(incrementors)
                .WithStatement(stale ? Enter(statement) : statement);
        }

        // Row 7 piece 6: Go's two-variable range is a DECONSTRUCTING foreach (`foreach (var (k, v) in range(…))`), which
        // Roslyn parses as ForEachVariableStatementSyntax, not ForEachStatementSyntax. Rule H applies to it alike: a body
        // entered after its header awaited starts with the live ref locals derived again (net/http's send, internal/diff,
        // archive/zip and text/template read a stale ref there, CS9217, when the whole standard library was twinned).
        public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitForEachVariableStatement(node);

            HeaderAwaits = 0;
            ExpressionSyntax expression = Head(node.Expression)!;
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;

            return node.WithExpression(expression).WithStatement(stale ? Enter(statement) : statement);
        }

        public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitForEachStatement(node);

            HeaderAwaits = 0;
            ExpressionSyntax expression = Head(node.Expression)!;
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;

            return node.WithExpression(expression).WithStatement(stale ? Enter(statement) : statement);
        }

        public override SyntaxNode? VisitUsingStatement(UsingStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitUsingStatement(node);

            HeaderAwaits = 0;
            VariableDeclarationSyntax? declaration = Head(node.Declaration);
            ExpressionSyntax? expression = Head(node.Expression);
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;

            return node.WithDeclaration(declaration).WithExpression(expression).WithStatement(stale ? Enter(statement) : statement);
        }

        public override SyntaxNode? VisitSwitchStatement(SwitchStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitSwitchStatement(node);

            HeaderAwaits = 0;
            ExpressionSyntax expression = Head(node.Expression)!;
            bool stale = HeaderAwaits > 0;
            List<SwitchSectionSyntax> sections = [];

            // Stage (c): an await in a CASE GUARD (net/http's `case {} when isCommonNetReadError(err)`) runs before the
            // sections' bodies, as the switch expression's does, so it too makes every section derive the refs again
            // on entry. The labels are visited first, all of them, to know.
            int labelAwaits = m_awaits;
            List<SyntaxList<SwitchLabelSyntax>> visitedLabels = [.. node.Sections.Select(section => List(section.Labels.Select(label => (SwitchLabelSyntax)Visit(label)!)))];
            stale |= m_awaits != labelAwaits;

            for (int index = 0; index < node.Sections.Count; index++)
            {
                SwitchSectionSyntax section = node.Sections[index];
                SyntaxList<SwitchLabelSyntax> labels = visitedLabels[index];
                SyntaxList<StatementSyntax> statements = Statements(section.Statements);

                if (stale && statements.Count > 0)
                {
                    if (statements[0] is BlockSyntax { Statements.Count: > 0 } block)
                    {
                        statements = statements.Replace(statements[0], Enter(block));
                    }
                    else
                    {
                        List<StatementSyntax> entry = Live(statements[0].GetLeadingTrivia());

                        if (entry.Count > 0)
                        {
                            entry.Add(statements[0].WithLeadingTrivia(Space));
                            entry.AddRange(statements.Skip(1));
                            statements = List(entry);
                        }
                    }
                }

                sections.Add(section.WithLabels(labels).WithStatements(statements));
            }

            return node.WithExpression(expression).WithSections(List(sections));
        }

        public override SyntaxNode? VisitTryStatement(TryStatementSyntax node)
        {
            if (m_passive > 0)
                return base.VisitTryStatement(node);

            int before = m_awaits;
            BlockSyntax block = (BlockSyntax)Visit(node.Block)!;
            bool stale = m_awaits != before;
            SyntaxList<CatchClauseSyntax> catches = List(node.Catches.Select(clause =>
            {
                CatchClauseSyntax visited = (CatchClauseSyntax)Visit(clause)!;
                return stale ? visited.WithBlock((BlockSyntax)Enter(visited.Block)) : visited;
            }));
            FinallyClauseSyntax? last = (FinallyClauseSyntax?)Visit(node.Finally);

            if (stale && last is not null)
                last = last.WithBlock((BlockSyntax)Enter(last.Block));

            return node.WithBlock(block).WithCatches(catches).WithFinally(last);
        }
    }
}
