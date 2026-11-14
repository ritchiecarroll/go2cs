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
        private readonly HashSet<string> m_assigned = [];
        private List<List<string>> m_scopes = [];
        private int m_awaits, m_passive, m_temps;
        private StatementSyntax? m_current;
        private List<StatementSyntax> m_spills = [];
        private readonly HashSet<SyntaxNode> m_spilled = [];

        public int Awaits, GoTwinned, Unrederivable, Literals, IfaceAwaits, Spills, RefBlocked;

        // N3's D3 core: the method's own func-value parameters whose pair test is HOISTED to the twin's entry (never
        // reassigned), the awaits that pair tests added (they never make a literal can-park), and the method itself.
        public readonly List<string> HoistedPairs = [];
        private readonly ISymbol? m_method;
        private readonly bool m_blockBody;
        private int m_pairAwaits;

        public Rewriter(Run run, SemanticModel model, MethodDeclarationSyntax method)
        {
            m_run = run;
            m_model = model;
            m_method = model.GetDeclaredSymbol(method);
            m_blockBody = method.Body is not null;

            // A box or path that is assigned anywhere in the function may name another variable after an await.
            foreach (SyntaxNode node in method.DescendantNodes())
            {
                ExpressionSyntax? written = node switch
                {
                    AssignmentExpressionSyntax assignment => assignment.Left,
                    PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PreIncrementExpression or (int)SyntaxKind.PreDecrementExpression } prefix => prefix.Operand,
                    PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PostIncrementExpression or (int)SyntaxKind.PostDecrementExpression } postfix => postfix.Operand,
                    ArgumentSyntax argument when !argument.RefKindKeyword.IsKind(SyntaxKind.None) => argument.Expression,
                    _ => null
                };

                if (written is IdentifierNameSyntax name)
                    m_assigned.Add(name.Identifier.Text);
            }
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
                return GoLiteral(node);

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
                    (string test, string twin, string plain) = BuildPairCall(node, shape);
                    string temp = "ᴛtwin" + ++m_temps;

                    m_spilled.Add(node);
                    m_spills.Add(ParseStatement("var " + temp + " = " + test + " ? await " + twin + " : " + plain + ";"));
                    Spills++;

                    return IdentifierName(temp).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
                }
            }

            if (!m_run.ShouldAwait(site))
                return base.VisitInvocationExpression(node);

            // A ref struct value evaluated before the await (a u8 literal passed as a format, say) cannot live
            // across it. Where everything evaluated earlier is inert, the awaited call moves ahead of its
            // statement; anywhere else the call stays a blocking one, which hands its P off as a thread park does.
            bool spill = RefLikeBefore(node, out bool movable);

            if (spill && !movable)
            {
                RefBlocked++;
                return base.VisitInvocationExpression(node);
            }

            InvocationExpressionSyntax visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;

            // An interface call reaches its twin through the hidden interface's call helper, or by name where the
            // twin is a default-bodied member of the interface itself.
            InvocationExpressionSyntax call = site.Interface && m_run.IfaceHidden
                ? m_run.IfaceCall(site.IfaceTarget!, visited)
                : visited.WithExpression(Rename(visited.Expression)).WithoutTrivia();

            ExpressionSyntax awaited = AwaitExpression(Token(SyntaxKind.AwaitKeyword).WithTrailingTrivia(Space), call);

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

            if (node.Parent is not (ExpressionStatementSyntax or ArrowExpressionClauseSyntax or LambdaExpressionSyntax))
                awaited = ParenthesizedExpression(awaited);

            m_awaits++;
            Awaits++;

            return awaited.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
        }

        // Whether a ref struct value is evaluated before this call inside its statement, and whether the call can
        // move ahead of that statement: it can when the enclosing statement list is visiting the statement, the
        // statement reaches the call unconditionally, and nothing is evaluated before the call but constants,
        // static method names and calls that already moved.
        private bool RefLikeBefore(InvocationExpressionSyntax node, out bool movable)
        {
            bool refLike = false;
            bool inert = true;
            SyntaxNode child = node;

            void Earlier(ExpressionSyntax expression)
            {
                TypeInfo info = m_model.GetTypeInfo(expression);
                refLike |= info.Type is { IsRefLikeType: true } || info.ConvertedType is { IsRefLikeType: true };

                ExpressionSyntax bare = expression;

                while (bare is ParenthesizedExpressionSyntax parenthesized)
                    bare = parenthesized.Expression;

                if (!m_spilled.Contains(bare) && !bare.IsKind(SyntaxKind.Utf8StringLiteralExpression) && !m_model.GetConstantValue(bare).HasValue)
                    inert = false;
            }

            for (SyntaxNode? parent = node.Parent; parent is not null; child = parent, parent = parent.Parent)
            {
                if (parent is StatementSyntax statement)
                {
                    inert &= ReferenceEquals(statement, m_current) &&
                             statement is ExpressionStatementSyntax or LocalDeclarationStatementSyntax or ReturnStatementSyntax;
                    break;
                }

                if (parent is ArrowExpressionClauseSyntax or MemberDeclarationSyntax or AnonymousFunctionExpressionSyntax)
                {
                    inert = false;
                    break;
                }

                switch (parent)
                {
                    case ArgumentSyntax or ParenthesizedExpressionSyntax or CastExpressionSyntax or EqualsValueClauseSyntax or VariableDeclaratorSyntax or MemberAccessExpressionSyntax:
                        break;
                    case BaseObjectCreationExpressionSyntax when child is ArgumentListSyntax:
                        break;
                    case VariableDeclarationSyntax declaration:
                        inert &= declaration.Variables.Count == 1;
                        break;
                    case BaseArgumentListSyntax list:
                        foreach (ArgumentSyntax argument in list.Arguments)
                        {
                            if (argument == child)
                                break;

                            Earlier(argument.Expression);
                        }

                        break;
                    case InvocationExpressionSyntax invocation when child is ArgumentListSyntax:
                        if (m_model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { IsStatic: true, MethodKind: MethodKind.Ordinary or MethodKind.LocalFunction })
                            Earlier(invocation.Expression is MemberAccessExpressionSyntax access ? access.Expression : invocation.Expression);

                        break;
                    case AssignmentExpressionSyntax assignment when child == assignment.Right && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression):
                        inert &= m_model.GetSymbolInfo(assignment.Left).Symbol is ILocalSymbol { RefKind: RefKind.None } or IParameterSymbol { RefKind: RefKind.None };
                        break;
                    default:
                        foreach (SyntaxNode sibling in parent.ChildNodes())
                        {
                            if (sibling == child)
                                break;

                            if (sibling is ExpressionSyntax expression)
                                Earlier(expression);
                        }

                        inert = false;
                        break;
                }
            }

            movable = inert;

            return refLike;
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
            m_scopes = [];

            CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(lambda.Body)!;

            m_scopes = scopes;
            m_awaits = awaits;

            LinePosition start = lambda.SyntaxTree.GetLineSpan(lambda.Span).StartLinePosition;
            string directive = "\r\n#line " + (start.Line + 1) + " \"" + lambda.SyntaxTree.FilePath + "\"\r\n" + new string(' ', start.Character);

            ExpressionSyntax twin = lambda.WithBody(body).WithoutTrivia()
                .WithModifiers(lambda.Modifiers.Add(Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(Space)))
                .WithLeadingTrivia(ParseLeadingTrivia(directive));

            GoTwinned++;
            Literals++;

            SyntaxNodeOrToken[] list = [Argument(original.WithoutTrailingTrivia()), Token(SyntaxKind.CommaToken), Argument(twin)];

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

        public override SyntaxNode? VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (m_passive == 0 && node.Expression is InvocationExpressionSyntax call && m_run.Sites.TryGetValue(call, out Site? site) &&
                site is { Active: true, Delegate: true, Go: GoKind.None } && PairShape(call) is { } shape)
            {
                (string test, string twin, string plain) = BuildPairCall(call, shape);
                return ParseStatement("if (" + test + ") await " + twin + "; else " + plain + ";").WithTriviaFrom(node);
            }

            return base.VisitExpressionStatement(node);
        }

        // Whether a call through a func value takes the pair test, and its twin delegate type; no side effects.
        private (string TwinType, bool Void)? PairShape(InvocationExpressionSyntax node)
        {
            if (!m_run.Pairs || node.Expression is not IdentifierNameSyntax ||
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
        private (string Test, string Twin, string Plain) BuildPairCall(InvocationExpressionSyntax node, (string TwinType, bool Void) shape)
        {
            IdentifierNameSyntax callee = (IdentifierNameSyntax)node.Expression;
            string twinType = shape.TwinType;
            string name = callee.Identifier.Text;
            string arguments = ((ArgumentListSyntax)Visit(node.ArgumentList)!).WithoutTrivia().ToFullString();
            bool hoisted = m_blockBody && m_model.GetSymbolInfo(callee).Symbol is IParameterSymbol parameter &&
                           SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, m_method) && !m_assigned.Contains(name);

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

            return (test, "((" + twinType + ")" + pairName + ".Twin)" + arguments, name + arguments);
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
            m_scopes = [];
            m_awaits = 0;
            m_pairAwaits = 0;

            CSharpSyntaxNode body = (CSharpSyntaxNode)Visit(node.Body)!;
            bool parks = m_awaits - m_pairAwaits > 0;

            m_scopes = scopes;
            m_awaits = awaits;
            m_pairAwaits = pairAwaits;

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

            ExpressionSyntax twin = node.WithBody(body).WithoutTrivia()
                .WithModifiers(node.Modifiers.Add(Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(Space)))
                .WithLeadingTrivia(ParseLeadingTrivia(directive));

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

            m_run.PairsMade++;

            string plainType = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string group = argument.WithoutTrivia().ToFullString();

            return ParseExpression("new " + plainType + "(new " + m_run.PairClass(type, twinType) + "(" + group + ", " + Rename(argument.WithoutTrivia()).ToFullString() + ").Invoke)")
                .WithTriviaFrom(node);
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

        private static bool Idempotent(ExpressionSyntax expression, out string root)
        {
            switch (expression)
            {
                case IdentifierNameSyntax name:
                    root = name.Identifier.Text;
                    return true;
                case MemberAccessExpressionSyntax access when access.IsKind(SyntaxKind.SimpleMemberAccessExpression):
                    return Idempotent(access.Expression, out root);
                case InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "DerefOrNull" or "Deref" } deref }:
                    return Idempotent(deref.Expression, out root);
                default:
                    root = "";
                    return false;
            }
        }

        private void Register(StatementSyntax statement)
        {
            if (statement is not LocalDeclarationStatementSyntax { Declaration: { Type: RefTypeSyntax, Variables: { Count: 1 } variables } } ||
                variables[0].Initializer?.Value is not RefExpressionSyntax { Expression: { } source })
                return;

            string name = variables[0].Identifier.Text;
            string? rederive = null;

            if (Idempotent(source, out string root))
            {
                if (!m_assigned.Contains(root))
                    rederive = name + " = ref " + source.WithoutTrivia().ToFullString() + ";";
            }
            else if (source is InvocationExpressionSyntax { Expression: SimpleNameSyntax { Identifier.Text: "heap" } } heap &&
                     heap.ArgumentList.Arguments.LastOrDefault() is { RefKindKeyword.RawKind: (int)SyntaxKind.OutKeyword } last)
            {
                string? box = last.Expression switch
                {
                    DeclarationExpressionSyntax { Designation: SingleVariableDesignationSyntax designation } => designation.Identifier.Text,
                    IdentifierNameSyntax identifier => identifier.Identifier.Text,
                    _ => null
                };

                if (box is not null && !m_assigned.Contains(box))
                    rederive = name + " = ref " + box + ".ValueSlot;";
            }

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
                // and the live ref locals are derived again between the two.
                if (spills.Count > 0)
                {
                    spills[0] = spills[0].WithLeadingTrivia(visited.GetLeadingTrivia());

                    for (int i = 1; i < spills.Count; i++)
                        spills[i] = spills[i].WithLeadingTrivia(Space);

                    result.AddRange(spills);
                    result.AddRange(Live(TriviaList(Space)));
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
            SeparatedSyntaxList<ExpressionSyntax> initializers = SeparatedList(node.Initializers.Select(item => Head(item)!), node.Initializers.GetSeparators());
            ExpressionSyntax? condition = Head(node.Condition);
            SeparatedSyntaxList<ExpressionSyntax> incrementors = SeparatedList(node.Incrementors.Select(item => Head(item)!), node.Incrementors.GetSeparators());
            bool stale = HeaderAwaits > 0;
            StatementSyntax statement = (StatementSyntax)Visit(node.Statement)!;

            return node.WithDeclaration(declaration).WithInitializers(initializers).WithCondition(condition).WithIncrementors(incrementors)
                .WithStatement(stale ? Enter(statement) : statement);
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

            foreach (SwitchSectionSyntax section in node.Sections)
            {
                SyntaxList<SwitchLabelSyntax> labels = List(section.Labels.Select(label => (SwitchLabelSyntax)Visit(label)!));
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
