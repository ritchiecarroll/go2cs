// TwinGenerator.RefAliases.cs - Gbtc
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

// M:N (experimental), N3's CS9217 cut (cure C1 of 79437adb79, approved 2026-11-15): which ref locals a twin can derive again after an
// await. A ref local does not live across an await, so the twin re-points each one after it (Rule R) at the source it
// was declared from. That is sound only while the source still names the same storage the Go variable does. The
// converter spells a Go pointer variable `p` as its box `Ꮡp` plus a ref alias `ref var p = ref Ꮡp.DerefOrNull()`, and
// every time it assigns the box it re-aliases on the next statement (`Ꮡp = …; p = ref Ꮡp.DerefOrNull();`). So a
// root that is assigned ONLY in that form is still re-derivable: deriving `p` again after an await reads the CURRENT
// box, exactly the alias the converter keeps. A root written in any other way (no re-alias, ++, a ref argument) is
// not, and a ref local rooted there that is live across an await refuses the twin by name (RefusesTwin).
public sealed partial class TwinGenerator
{
    internal sealed partial class Run
    {
        // C1's REFUSAL: a function to be twinned that declares a ref local it cannot derive again (RefAliases) and uses
        // it after one of the twin's awaits -- later in the text, or earlier in a loop that holds the await but not the
        // declaration (the next iteration) -- would hold a stale reference across the await (CS9217) or name the wrong
        // storage. No sound re-derivation exists, so the function is refused by name, never compiled wrong.
        private bool UnrederivableLive(Candidate candidate)
        {
            SyntaxNode? body = (SyntaxNode?)candidate.Syntax.Body ?? candidate.Syntax.ExpressionBody;

            if (body is null)
                return false;

            List<InvocationExpressionSyntax> awaits = [.. body.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(node => Sites.TryGetValue(node, out Site? site) && site.Direct && (ShouldAwait(site) || site.Delegate && CallerRuleTwins(site)))];

            if (awaits.Count == 0)
                return false;

            RefAliases aliases = new(candidate.Syntax);

            foreach (LocalDeclarationStatementSyntax declaration in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                if (aliases.Rederive(declaration, out string? name) is not null || name is null || declaration.Parent is not { } scope)
                    continue;

                // A use reads the ref; the left side of a ref re-assignment (`name = ref …`) re-defines it instead.
                List<IdentifierNameSyntax> uses = [.. scope.DescendantNodes().OfType<IdentifierNameSyntax>()
                    .Where(use => use.Identifier.Text == name && use.SpanStart > declaration.Span.End &&
                                  !(use.Parent is AssignmentExpressionSyntax { Right: RefExpressionSyntax } reassignment && reassignment.Left == use))];

                foreach (InvocationExpressionSyntax awaited in awaits.Where(node => node.SpanStart > declaration.Span.End && scope.Span.Contains(node.Span)))
                {
                    if (uses.Any(use => use.SpanStart > awaited.Span.End))
                        return true;

                    foreach (StatementSyntax loop in awaited.Ancestors().TakeWhile(ancestor => ancestor != scope).OfType<StatementSyntax>()
                                 .Where(statement => statement is ForStatementSyntax or CommonForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax))
                    {
                        if (uses.Any(use => loop.Span.Contains(use.Span)))
                            return true;
                    }
                }
            }

            return false;
        }
    }

    internal sealed class RefAliases
    {
        private readonly HashSet<string> m_assigned = [];

        // root -> the ref locals re-aliased on the statement right after EVERY write of the root; null when some
        // write of it is not followed by a re-alias.
        private readonly Dictionary<string, HashSet<string>?> m_reAliased = new(System.StringComparer.Ordinal);

        public RefAliases(SyntaxNode method)
        {
            foreach (SyntaxNode node in method.DescendantNodes())
            {
                switch (node)
                {
                    case AssignmentExpressionSyntax assignment:
                        foreach (string root in WrittenNames(assignment.Left))
                            Write(root, assignment.Right is RefExpressionSyntax ? null : ReAliasAfter(assignment, root));

                        break;
                    case PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PreIncrementExpression or (int)SyntaxKind.PreDecrementExpression } prefix
                        when prefix.Operand is IdentifierNameSyntax operand:
                        Write(operand.Identifier.Text, null);
                        break;
                    case PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PostIncrementExpression or (int)SyntaxKind.PostDecrementExpression } postfix
                        when postfix.Operand is IdentifierNameSyntax operand:
                        Write(operand.Identifier.Text, null);
                        break;
                    case ArgumentSyntax argument when !argument.RefKindKeyword.IsKind(SyntaxKind.None) && argument.Expression is IdentifierNameSyntax written:
                        Write(written.Identifier.Text, null);
                        break;
                }
            }
        }

        // Whether a name is written anywhere in the method, in any form.
        public bool Assigned(string name) => m_assigned.Contains(name);

        // Whether a ref local `name` declared from a source rooted at `root` can be derived again after an await.
        public bool Rederivable(string root, string name) =>
            !m_assigned.Contains(root) || m_reAliased.TryGetValue(root, out HashSet<string>? names) && names is not null && names.Contains(name);

        // The statement that derives a ref local declaration again after an await, or null when there is none; name is
        // the declared ref local (null when the statement declares none).
        public string? Rederive(StatementSyntax statement, out string? name)
        {
            name = null;

            if (statement is not LocalDeclarationStatementSyntax { Declaration: { Type: RefTypeSyntax, Variables: { Count: 1 } variables } } ||
                variables[0].Initializer?.Value is not RefExpressionSyntax { Expression: { } source })
                return null;

            name = variables[0].Identifier.Text;

            if (Idempotent(source, out string root))
                return Rederivable(root, name) ? name + " = ref " + source.WithoutTrivia().ToFullString() + ";" : null;

            if (source is InvocationExpressionSyntax { Expression: SimpleNameSyntax { Identifier.Text: "heap" } } heap &&
                heap.ArgumentList.Arguments.LastOrDefault() is { RefKindKeyword.RawKind: (int)SyntaxKind.OutKeyword } last)
            {
                string? box = last.Expression switch
                {
                    DeclarationExpressionSyntax { Designation: SingleVariableDesignationSyntax designation } => designation.Identifier.Text,
                    IdentifierNameSyntax identifier => identifier.Identifier.Text,
                    _ => null
                };

                if (box is not null && Rederivable(box, name))
                    return name + " = ref " + box + ".ValueSlot;";
            }

            return null;
        }

        internal static bool Idempotent(ExpressionSyntax expression, out string root)
        {
            switch (expression)
            {
                case IdentifierNameSyntax identifier:
                    root = identifier.Identifier.Text;
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

        private void Write(string root, string? reAliased)
        {
            bool first = m_assigned.Add(root);

            if (first)
            {
                m_reAliased[root] = reAliased is null ? null : [reAliased];
                return;
            }

            if (m_reAliased.TryGetValue(root, out HashSet<string>? names) && names is not null)
            {
                if (reAliased is null)
                    m_reAliased[root] = null;
                else
                    names.IntersectWith([reAliased]);

                if (m_reAliased[root] is { Count: 0 })
                    m_reAliased[root] = null;
            }
        }

        // The identifiers an assignment's left side writes: a name, or the names of a tuple's elements.
        private static IEnumerable<string> WrittenNames(ExpressionSyntax left)
        {
            if (left is IdentifierNameSyntax identifier)
                yield return identifier.Identifier.Text;
            else if (left is TupleExpressionSyntax tuple)
            {
                foreach (ArgumentSyntax argument in tuple.Arguments)
                {
                    if (argument.Expression is IdentifierNameSyntax element)
                        yield return element.Identifier.Text;
                }
            }
        }

        // The ref local the converter re-aliases on the statement right after this write of `root`
        // (`root = …; name = ref <source rooted at root>;`), or null when the next statement is not that.
        private static string? ReAliasAfter(AssignmentExpressionSyntax assignment, string root)
        {
            if (assignment.Parent is not ExpressionStatementSyntax statement)
                return null;

            SyntaxList<StatementSyntax> list = statement.Parent switch
            {
                BlockSyntax block => block.Statements,
                SwitchSectionSyntax section => section.Statements,
                _ => default
            };

            int index = list.IndexOf(statement);

            if (index < 0 || index + 1 >= list.Count ||
                list[index + 1] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { Left: IdentifierNameSyntax name, Right: RefExpressionSyntax { Expression: { } source } } } ||
                !Idempotent(source, out string sourceRoot) || sourceRoot != root)
            {
                return null;
            }

            return name.Identifier.Text;
        }
    }
}
