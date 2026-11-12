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
        private int m_awaits, m_passive;

        public int Awaits, GoTwinned, Unrederivable, Literals, IfaceAwaits;

        public Rewriter(Run run, SemanticModel model, MethodDeclarationSyntax method)
        {
            m_run = run;
            m_model = model;

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

            if (!m_run.ShouldAwait(site))
                return base.VisitInvocationExpression(node);

            InvocationExpressionSyntax visited = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;

            // An interface call reaches its twin through the hidden interface's call helper, or by name where the
            // twin is a default-bodied member of the interface itself.
            InvocationExpressionSyntax call = site.Interface && m_run.IfaceHidden
                ? m_run.IfaceCall(site.IfaceTarget!, visited)
                : visited.WithExpression(Rename(visited.Expression)).WithoutTrivia();

            ExpressionSyntax awaited = AwaitExpression(Token(SyntaxKind.AwaitKeyword).WithTrailingTrivia(Space), call);

            if (site.Interface)
                IfaceAwaits++;

            if (node.Parent is not (ExpressionStatementSyntax or ArrowExpressionClauseSyntax or LambdaExpressionSyntax))
                awaited = ParenthesizedExpression(awaited);

            m_awaits++;
            Awaits++;

            return awaited.WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
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
            m_passive++;
            SyntaxNode? visited = base.VisitParenthesizedLambdaExpression(node);
            m_passive--;
            return visited;
        }

        public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            m_passive++;
            SyntaxNode? visited = base.VisitSimpleLambdaExpression(node);
            m_passive--;
            return visited;
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
                StatementSyntax visited = (StatementSyntax)Visit(statement)!;
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
