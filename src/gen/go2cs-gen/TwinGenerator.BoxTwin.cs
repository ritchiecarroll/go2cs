// TwinGenerator.BoxTwin.cs - Gbtc
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

// M:N PROTOTYPE, N2 (docs/phase4/DESIGN-mn-box-twin.md, revision 1, section 9; COORD's GO after the re-review): the BOX
// TWIN. A Go method with a pointer receiver converts to `M(this ref T t, …)`, which no async method can take. Its twin
// takes the receiver's box instead, `Mꞏtwin(this ж<T> Ꮡt, …)`, under the prologue `ref var t = ref Ꮡt.DerefOrNull();`
// -- the shape a Go pointer parameter already has, which Rule R derives again after each await (C1). A call names the
// box for its receiver WITHOUT A COPY (the rows of 9.1), or stays plain and named. Aliasing (section 4): the twin holds
// the box, so a write another goroutine makes through the same pointer during a park is read after it, as in Go.
public sealed partial class TwinGenerator
{
    internal sealed partial class Run
    {
        // GoTwinsBoxRecv (default on), GoTwinsBoxForwarders (default on), GoTwinsBoxCopy (default off: the probe's control).
        internal bool BoxRecv { get; init; } = true;
        internal bool BoxForwarders { get; init; } = true;
        internal bool BoxCopy { get; init; }

        internal int BoxCalls, BoxPlain;
        private readonly List<string> m_boxPlainRows = [];
        private readonly Dictionary<ISymbol, bool> m_hasBoxTwin = new(SymbolEqualityComparer.Default);

        private static bool CopyBound(IMethodSymbol method) =>
            method.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "GoCopyBoundAttribute");

        // The receiver's name when this method gets a box twin (9.3): an extension whose FIRST parameter, and only it, is a
        // plain `ref` (never `in`, `out` or `ref readonly`, which copy in Go), not [GoCopyBound] (a value-method-set
        // forwarder, bound through a copy), with a block body, and whose body names no `Ꮡ<receiver>` already.
        private string? BoxReceiverOf(MethodDeclarationSyntax method, IMethodSymbol symbol)
        {
            if (!BoxRecv || !symbol.IsExtensionMethod || symbol.Parameters.Length == 0 || symbol.Parameters[0].RefKind != RefKind.Ref ||
                symbol.Parameters.Skip(1).Any(parameter => parameter.RefKind != RefKind.None) || CopyBound(symbol) || method.Body is null)
            {
                return null;
            }

            string name = symbol.Parameters[0].Name;
            string box = "Ꮡ" + name;

            return method.Body.DescendantTokens().Any(token => token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == box) ? null : name;
        }

        // Whether `definition`, a `this ref T` method of a REFERENCED assembly, has a generated box twin there:
        // `Mꞏtwin(this ж<T>, …)` beside it. Never with the switch off: CanPark and IsBoxTwinTarget then read as the base.
        private bool HasBoxTwinMember(IMethodSymbol definition)
        {
            if (!BoxRecv)
                return false;

            if (m_hasBoxTwin.TryGetValue(definition, out bool has))
                return has;

            if (definition is { IsExtensionMethod: true, Parameters.Length: > 0 } && definition.Parameters[0].RefKind == RefKind.Ref &&
                definition.ContainingType is { } type && !m_candidates.ContainsKey(definition))
            {
                has = type.GetMembers(definition.Name + Suffix).OfType<IMethodSymbol>().Any(twin => IsBoxTwinOf(twin, definition));
            }

            return m_hasBoxTwin[definition] = has;
        }

        private static bool IsBoxTwinOf(IMethodSymbol twin, IMethodSymbol definition) =>
            twin.IsStatic && twin.Parameters.Length == definition.Parameters.Length && twin.TypeParameters.Length == definition.TypeParameters.Length &&
            twin.Parameters[0].RefKind == RefKind.None &&
            twin.Parameters[0].Type is INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } box &&
            box.TypeArguments[0].ToDisplayString() == definition.Parameters[0].Type.ToDisplayString() &&
            twin.Parameters.Skip(1).Zip(definition.Parameters.Skip(1), (a, b) => a.Type.ToDisplayString() == b.Type.ToDisplayString()).All(same => same);

        // Whether a call to `target` (normalized) reaches a box twin: one this run emits, or one a referenced assembly holds.
        // Never with the switch off, so that arm is the base by construction. And never for a method that has a twin of its
        // OWN shape (`Mꞏtwin(this ref T)`, sync's hand-written `Lockꞏtwin`), which the base already awaits in place: a
        // box-shaped `Mꞏtwin(this ж<T>)` beside it is stage (c)'s interface-forwarder twin (`*Mutex` implements Locker),
        // and routing through it would leave plain each call that names no box (closure arm O's 5 `Lock` rows).
        internal bool IsBoxTwinTarget(IMethodSymbol? target) =>
            target is not null &&
            (m_candidates.TryGetValue(target, out Candidate? candidate) ? candidate.BoxReceiver is not null && candidate.Twinned : HasBoxTwinMember(target) && !HasTwinMember(target));

        // Must-fix 2 of the re-review: a call bound to RecvGenerator's box overload `M(this ж<T> Ꮡt, …)`, which forwards to
        // the `this ref T` method, is followed to that method -- resolved by FULL SIGNATURE (an sstring pair holds two
        // forwarders of one name), and only to a plain-`ref`, non-[GoCopyBound] target. A VALUE receiver's box overload
        // copies (`var t = ~Ꮡt;`) and never matches, since its target takes `T`, not `ref T`.
        internal IMethodSymbol? BoxOverloadTarget(IMethodSymbol method)
        {
            if (!BoxRecv || !method.IsExtensionMethod || method.Parameters.Length == 0 || m_candidates.ContainsKey(method) ||
                method.Parameters[0].Type is not INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } box || method.ContainingType is not { } type)
            {
                return null;
            }

            foreach (IMethodSymbol other in type.GetMembers(method.Name).OfType<IMethodSymbol>())
            {
                IMethodSymbol definition = Normalize(other);

                if (SymbolEqualityComparer.Default.Equals(definition, method) || !definition.IsExtensionMethod || definition.Parameters.Length != method.Parameters.Length ||
                    definition.Parameters[0].RefKind != RefKind.Ref || CopyBound(definition) || definition.TypeParameters.Length != method.TypeParameters.Length ||
                    !SymbolEqualityComparer.Default.Equals(definition.Parameters[0].Type, box.TypeArguments[0]))
                {
                    continue;
                }

                bool same = true;

                for (int i = 1; same && i < definition.Parameters.Length; i++)
                {
                    same = definition.Parameters[i].RefKind == method.Parameters[i].RefKind &&
                           SymbolEqualityComparer.Default.Equals(definition.Parameters[i].Type, method.Parameters[i].Type);
                }

                if (same && SymbolEqualityComparer.Default.Equals(definition.ReturnType, method.ReturnType))
                    return definition;
            }

            return null;
        }

        // The box a call site names for a `this ref T` receiver WITHOUT A COPY (9.1's rows), or null: the call stays plain.
        // Every form is bound speculatively and must be a ж<T> of the receiver's type (SN-1, mechanical): anything else is
        // named-plain, never guessed.
        internal string? BoxExpression(InvocationExpressionSyntax node, SemanticModel model, Candidate caller, RefAliases aliases)
        {
            if (node.Expression is not MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression } access ||
                model.GetTypeInfo(access.Expression).Type is not { } receiverType)
            {
                return null;
            }

            string? box = BoxOf(access.Expression, model, caller, aliases, node.SpanStart);

            return box is not null && Binds(box, receiverType, model, caller, node.SpanStart) ? box : null;
        }

        // SN-1: a box form must bind, at the call, to ж<the receiver's type>. The box twin's own `Ꮡt` exists only in the
        // twin, so the check binds a typed stand-in of it in the plain method.
        private static bool Binds(string box, ITypeSymbol expected, SemanticModel model, Candidate caller, int position)
        {
            string check = box;

            if (caller is { BoxReceiver: { } receiver, Symbol.Parameters.Length: > 0 } && (box == "Ꮡ" + receiver || box.StartsWith("Ꮡ" + receiver + ".", StringComparison.Ordinal)))
            {
                check = "((global::go.ж<" + caller.Symbol.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">)default!)" +
                        box.Substring(("Ꮡ" + receiver).Length);
            }

            TypeInfo bound = model.GetSpeculativeTypeInfo(position, SyntaxFactory.ParseExpression(check), SpeculativeBindingOption.BindAsExpression);

            return bound.Type is INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } boxType &&
                   SymbolEqualityComparer.Default.Equals(boxType.TypeArguments[0], expected);
        }

        // A field's pointer from its parent's box: the field-access generator's `parent.Ꮡf` where the converter recorded
        // the type ([GoFieldAccess]), else the field token every converted struct has, `parent.of(T.Field‿Tokens.Ꮡf‿)` --
        // the same FieldRefBox either way. Neither binding: none.
        private static string? FieldBox(string parent, IFieldSymbol field, SemanticModel model, Candidate caller, int position)
        {
            string pointer = parent + ".Ꮡ" + field.Name;

            if (Binds(pointer, field.Type, model, caller, position))
                return pointer;

            string token = parent + ".of(" + field.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ".Field‿Tokens.Ꮡ" + field.Name + "‿)";

            return Binds(token, field.Type, model, caller, position) ? token : null;
        }

        // The review's nit: an element under a package variable (`name.f[i]`, 3 sites on the closure) is named-plain in the
        // first cut.
        private static bool RootedInPackageVar(ExpressionSyntax expression, SemanticModel model)
        {
            while (expression is MemberAccessExpressionSyntax or ParenthesizedExpressionSyntax)
                expression = expression is MemberAccessExpressionSyntax member ? member.Expression : ((ParenthesizedExpressionSyntax)expression).Expression;

            return model.GetSymbolInfo(expression).Symbol is IPropertySymbol { IsStatic: true, ReturnsByRef: true };
        }

        private string? BoxOf(ExpressionSyntax operand, SemanticModel model, Candidate caller, RefAliases aliases, int position)
        {
            while (operand is ParenthesizedExpressionSyntax parenthesized)
                operand = parenthesized.Expression;

            ISymbol? symbol = model.GetSymbolInfo(operand).Symbol;

            switch (operand)
            {
                // The box twin's own receiver.
                case IdentifierNameSyntax name when symbol is IParameterSymbol { RefKind: RefKind.Ref, Ordinal: 0 } && caller.BoxReceiver == name.Identifier.ValueText:
                    return "Ꮡ" + name.Identifier.ValueText;

                // A ref local over a box, CURRENT (9.4): declared exactly `ref Ꮡx.DerefOrNull()` / `ref Ꮡx.Value` (or a heap
                // local's `ref Ꮡv.ValueSlot`) over a plain root, and Rederivable -- a root reassigned other than in the
                // converter's re-alias form would hand M another object than the one the ref local names.
                case IdentifierNameSyntax name when symbol is ILocalSymbol { RefKind: RefKind.Ref } local:
                {
                    if (local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: RefExpressionSyntax { Expression: var source } })
                        return null;

                    string? root = source switch
                    {
                        InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "DerefOrNull", Expression: IdentifierNameSyntax r } } => r.Identifier.ValueText,
                        MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Value" or "ValueSlot", Expression: IdentifierNameSyntax r } => r.Identifier.ValueText,
                        _ => null
                    };

                    return root is not null && aliases.Rederivable(root, name.Identifier.ValueText) ? root : null;
                }

                // `Ꮡx.DerefOrNull()` as the receiver.
                case InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "DerefOrNull", Expression: IdentifierNameSyntax r } }:
                    return r.Identifier.ValueText;

                // A package variable: a ref property over its static box `Ꮡname`.
                case IdentifierNameSyntax name when symbol is IPropertySymbol { IsStatic: true, ReturnsByRef: true }:
                    return "Ꮡ" + name.Identifier.ValueText;

                // A field of a receiver that has a box: the field's pointer from that box.
                case MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name: IdentifierNameSyntax } member
                    when symbol is IFieldSymbol { IsStatic: false } field && BoxOf(member.Expression, model, caller, aliases, position) is { } parent:
                    return FieldBox(parent, field, model, caller, position);

                // A slice or array element with a side-effect-free index: the element pointer, index evaluated once.
                case ElementAccessExpressionSyntax { ArgumentList.Arguments.Count: 1 } element
                    when element.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax or LiteralExpressionSyntax &&
                         model.GetTypeInfo(element.Expression).Type is INamedTypeSymbol { Name: "slice" or "array" } &&
                         SideEffectFree(element.Expression, model) && !RootedInPackageVar(element.Expression, model):
                    return "global::go.builtin.Ꮡ(" + element.Expression.WithoutTrivia().ToFullString() + ", " + element.ArgumentList.Arguments[0].Expression.WithoutTrivia().ToFullString() + ")";

                default:
                    return null;
            }
        }

        private static bool SideEffectFree(ExpressionSyntax expression, SemanticModel model) => expression switch
        {
            IdentifierNameSyntax => true,
            MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression } member => model.GetSymbolInfo(member).Symbol is IFieldSymbol && SideEffectFree(member.Expression, model),
            ParenthesizedExpressionSyntax parenthesized => SideEffectFree(parenthesized.Expression, model),
            _ => false
        };

        // A plain call to a box twin, for the report (SN-7 reads their count).
        internal void NoteBoxPlain(InvocationExpressionSyntax node, IMethodSymbol target)
        {
            BoxPlain++;
            FileLinePositionSpan span = node.SyntaxTree.GetLineSpan(node.Span);
            m_boxPlainRows.Add("// P " + System.IO.Path.GetFileName(node.SyntaxTree.FilePath) + "|" + (span.StartLinePosition.Line + 1) + "|" + target.Name + "|" +
                               (node.Expression is MemberAccessExpressionSyntax access ? access.Expression.ToString().Replace("\r", " ").Replace("\n", " ") : "-"));
        }

        internal IEnumerable<string> BoxPlainRows => m_boxPlainRows;

        // Must-fix 1 of the re-review: a ref local whose source CROSSES a ж-typed member (`ref var m = ref n.next.DerefOrNull()`)
        // would be derived again after a park through a member another goroutine may have replaced, where Go read it once.
        // Such a twin is refused by name, "ref-shared-member" (0 on the stage (c) closure; the detector holds it there).
        private bool SharedMemberRef(Candidate candidate)
        {
            SemanticModel model = Model(candidate.Syntax.SyntaxTree);

            foreach (VariableDeclaratorSyntax declarator in candidate.Syntax.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                if (declarator.Initializer?.Value is not RefExpressionSyntax { Expression: var source })
                    continue;

                // A HEAP local, `ref var v = ref heap<T>(init, out var Ꮡv)`, is derived again from its own box (`Ꮡv.ValueSlot`):
                // a member in its initializer is read once, as Go reads it (closure arm O's runtime traceRegionAlloc.alloc).
                if (source is InvocationExpressionSyntax { Expression: GenericNameSyntax { Identifier.ValueText: "heap" } or IdentifierNameSyntax { Identifier.ValueText: "heap" } } heap &&
                    heap.ArgumentList.Arguments.LastOrDefault() is { RefKindKeyword.RawKind: (int)SyntaxKind.OutKeyword })
                {
                    continue;
                }

                if (source.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>().Any(member =>
                        member.Parent is not InvocationExpressionSyntax && model.GetTypeInfo(member).Type is INamedTypeSymbol { Name: "ж" }))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
