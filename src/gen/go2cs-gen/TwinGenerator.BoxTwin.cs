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

// M:N (experimental), N2 (docs/phase4/DESIGN-mn-box-twin.md, revision 1, section 9; approved after the re-review): the BOX
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

        // byref-param (DESIGN-mn-box-twin.md 12.1): GoTwinsBoxParam (default on) gives a ref-lowered pointer parameter
        // `ref T p` the box, as a receiver has it; GoTwinsBoxArgCopy (default off) is the CALL-SITE control: each box a
        // call hands is replaced by a fresh copy `builtin.Ꮡ(arg)` -- wrong on purpose, never a build anyone ships.
        internal bool BoxParam { get; init; } = true;
        internal bool BoxArgCopy { get; init; }

        internal int BoxCalls, BoxPlain;
        private readonly List<string> m_boxPlainRows = [];
        private readonly List<string> m_viaOverloadRows = [];
        private readonly Dictionary<ISymbol, bool> m_hasBoxTwin = new(SymbolEqualityComparer.Default);

        private static bool CopyBound(IMethodSymbol method) =>
            method.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "GoCopyBoundAttribute");

        // The receiver's name when this method gets a box twin (9.3): an extension whose FIRST parameter, and only it, is a
        // plain `ref` (never `in`, `out` or `ref readonly`, which copy in Go), not [GoCopyBound] (a value-method-set
        // forwarder, bound through a copy), with a body, and whose body names no `Ꮡ<receiver>` already. The body may be an
        // EXPRESSION: since the glyph seat (C1, train 4) a method is a `M(this ref T t, …) => M⁔(ref t, …)` forwarder whose
        // body is `M⁔`. Refused for want of a block, those forwarders had no box twin, so a twinned caller holding the box
        // called the plain forwarder and everything under it ran plain (DATA-mn-closure-cure.md: parks 36 -> 0).
        private string? BoxReceiverOf(MethodDeclarationSyntax method, IMethodSymbol symbol)
        {
            SyntaxNode? body = (SyntaxNode?)method.Body ?? method.ExpressionBody;

            if (!BoxRecv || !symbol.IsExtensionMethod || symbol.Parameters.Length == 0 || symbol.Parameters[0].RefKind != RefKind.Ref ||
                symbol.Parameters.Skip(1).Any(parameter => parameter.RefKind != RefKind.None) || CopyBound(symbol) || body is null)
            {
                return null;
            }

            string name = symbol.Parameters[0].Name;
            string box = "Ꮡ" + name;

            return body.DescendantTokens().Any(token => token.IsKind(SyntaxKind.IdentifierToken) && token.ValueText == box) ? null : name;
        }

        // byref-param (12.1): the names of every plain `ref` parameter of a NON-extension function when its twin takes
        // their boxes, each `Ꮡp`, under `ref var p = ref Ꮡp.DerefOrNull();`. Null when the switch is off, when any by-ref
        // parameter is `out`, `in` or `ref readonly` (Go copies none of those; refused `byref-out`), when there is no
        // block body, or when the body already names one of the boxes. Ref-lowering never applies to methods, so a
        // `this ref` method with another `ref` parameter is refused by name (`byref-mixed`), never boxed here.
        private List<string>? BoxParamsOf(MethodDeclarationSyntax method, IMethodSymbol symbol)
        {
            if (!BoxParam || symbol.IsExtensionMethod || method.Body is null)
                return null;

            List<string> names = [];

            foreach (IParameterSymbol parameter in symbol.Parameters)
            {
                if (parameter.RefKind == RefKind.None)
                    continue;

                if (parameter.RefKind != RefKind.Ref)
                    return null;

                names.Add(parameter.Name);
            }

            if (names.Count == 0)
                return null;

            HashSet<string> boxes = new(names.Select(name => "Ꮡ" + name), StringComparer.Ordinal);

            return method.Body.DescendantTokens().Any(token => token.IsKind(SyntaxKind.IdentifierToken) && boxes.Contains(token.ValueText)) ? null : names;
        }

        // The refusal of a function for its by-ref parameters, or null when each one gets a box. With GoTwinsBoxParam off
        // it is the N2 rule, unchanged (the switch-off arm equals arm N by construction).
        private string? ByRefRefusal(MethodDeclarationSyntax method, IMethodSymbol symbol)
        {
            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                RefKind kind = symbol.Parameters[i].RefKind;

                if (kind == RefKind.None)
                    continue;

                bool receiver = i == 0 && symbol.IsExtensionMethod;

                // N2: a plain `ref` receiver without [GoCopyBound] gets a box twin instead (BoxReceiverOf).
                if (receiver && BoxReceiverOf(method, symbol) is not null)
                    continue;

                if (!BoxParam)
                    return receiver ? "byref-recv" : "byref-param";

                if (receiver)
                    return BoxRecv && kind == RefKind.Ref && symbol.Parameters.Skip(1).Any(parameter => parameter.RefKind == RefKind.Ref) ? "byref-mixed" : "byref-recv";

                if (kind != RefKind.Ref)
                    return "byref-out";

                if (symbol.IsExtensionMethod)
                    return symbol.Parameters[0].RefKind == RefKind.Ref ? "byref-mixed" : "byref-param";

                if (BoxParamsOf(method, symbol) is null)
                    return "byref-param";
            }

            return null;
        }

        // Whether a call to `target` reaches a byref-param box twin this run emits (no such callee is in another assembly:
        // ref-lowering applies only to unexported functions, 12.1's scope facts).
        internal bool IsParamBoxTwin(IMethodSymbol? target) =>
            target is not null && m_candidates.TryGetValue(target, out Candidate? candidate) && candidate.BoxParams is not null && candidate.Twinned;

        // The boxes a call hands a byref-param box twin, one per by-ref argument (12.1, all or nothing): `boxes[i]` is the
        // box for argument i (null for a by-value argument), or `boxes` is null and `plain` names the first by-ref
        // argument that names no box, the call's `// P` row. Each box is bound by SN-1 to ж<the argument's type>.
        internal string?[]? ArgumentBoxes(InvocationExpressionSyntax node, IMethodSymbol target, SemanticModel model, Candidate caller, RefAliases aliases, out string? plain)
        {
            SeparatedSyntaxList<ArgumentSyntax> arguments = node.ArgumentList.Arguments;
            string?[] boxes = new string?[arguments.Count];
            plain = null;

            for (int i = 0; i < arguments.Count; i++)
            {
                ArgumentSyntax argument = arguments[i];

                if (argument.RefKindKeyword.IsKind(SyntaxKind.None))
                    continue;

                string? box = null;

                if (argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword) && argument.NameColon is null && i < target.Parameters.Length &&
                    target.Parameters[i].RefKind == RefKind.Ref && model.GetTypeInfo(argument.Expression).Type is { } type)
                {
                    box = ArgBoxOf(argument.Expression, model, caller, aliases, node.SpanStart);

                    if (box is not null && !Binds(box, type, model, caller, node.SpanStart))
                        box = null;
                }

                if (box is null)
                {
                    plain = argument.Expression.ToString().Replace("\r", " ").Replace("\n", " ");

                    // PB-5 (12.5): a stack local refused promotion names its reason.
                    if (argument.Expression is IdentifierNameSyntax refusedName && caller.PromoteRefused.TryGetValue(refusedName.Identifier.ValueText, out string? reason))
                        plain += " (" + reason + ")";

                    return null;
                }

                boxes[i] = box;
            }

            return boxes;
        }

        // 12.1's enumerated aliasing forms for an ARGUMENT, beside the receiver rows BoxOf already knows: (a) `(E).DerefOrNull()`
        // or `E.DerefOrNull()` with E a side-effect-free ж expression or an element pointer `Ꮡ(x, i)`: E itself, read once at
        // the call as Go reads it (review finding 5: never `ref E.Value`, which faults at the call when E is nil where a handed
        // box would fault at the callee's first dereference, possibly after a park; the converter emits `(p).DerefOrNull()`
        // for a pointer argument, and `.Value` comes from hand-owned code); (c) ruling 10.4's nil-checked field `nonnil(ref e).f`: the field pointer
        // from e's box, which faults as Go does when e is nil. Never `builtin.Ꮡ(v)` (it COPIES v into a new box).
        private string? ArgBoxOf(ExpressionSyntax operand, SemanticModel model, Candidate caller, RefAliases aliases, int position)
        {
            while (operand is ParenthesizedExpressionSyntax parenthesized)
                operand = parenthesized.Expression;

            ExpressionSyntax? boxed = operand switch
            {
                InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name.Identifier.ValueText: "DerefOrNull" } deref } => deref.Expression,
                _ => null
            };

            if (boxed is not null)
            {
                while (boxed is ParenthesizedExpressionSyntax inner)
                    boxed = inner.Expression;

                return BoxSideEffectFree(boxed, model) ? boxed.WithoutTrivia().ToFullString() : null;
            }

            // (c), and next step 1 (5bd877cfb6): a field CHAIN `nonnil(ref e).f.g…` (Go's `&e.f.g`), each level an instance field of the
            // one before and every level but the last of struct type, so each is a field of e's own object: FieldBox once per
            // level, outermost first. The nil check stays on the first level only (the first token faults at a nil e, as Go
            // panics at the outer nil); a deeper level cannot be nil, since it is not a pointer.
            if (operand is MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name: IdentifierNameSyntax })
            {
                List<IFieldSymbol> fields = [];
                ExpressionSyntax level = operand;

                while (level is MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression, Name: IdentifierNameSyntax } chain &&
                       model.GetSymbolInfo(chain).Symbol is IFieldSymbol { IsStatic: false } chainField &&
                       (fields.Count == 0 || chainField.Type is { IsValueType: true, TypeKind: TypeKind.Struct }))
                {
                    fields.Insert(0, chainField);
                    level = chain.Expression;

                    while (level is ParenthesizedExpressionSyntax parenthesizedLevel)
                        level = parenthesizedLevel.Expression;
                }

                if (fields.Count > 0 &&
                    level is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nonnil" }, ArgumentList.Arguments: [{ RefKindKeyword.RawKind: (int)SyntaxKind.RefKeyword } checkedArgument] } nonnil &&
                    Builtin(model.GetSymbolInfo(nonnil).Symbol, "nonnil"))
                {
                    string? box = ArgBoxOf(checkedArgument.Expression, model, caller, aliases, position);

                    foreach (IFieldSymbol field in fields)
                        box = box is null ? null : FieldBox(box, field, model, caller, position);

                    return box;
                }
            }

            return BoxOf(operand, model, caller, aliases, position);
        }

        // The field-access generator's members over a box (`e.f`, a ref to the field; `e.Ꮡf`, the field's pointer): a field
        // read through the box, nothing else, so a field chain may cross them (the probe's `(h.Ꮡc).DerefOrNull()` and
        // `(e.p).DerefOrNull()`). They are extension properties of `go.GoFieldAccess`.
        private static bool FieldAccessMember(INamedTypeSymbol type)
        {
            for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
            {
                if (current is { Name: "GoFieldAccess", ContainingType: null, ContainingNamespace.Name: "go" })
                    return true;
            }

            return false;
        }

        // A ж expression a call can read once and hand on: a name (a local, a parameter or a static), a field chain over
        // one, a `~` dereference of one, or an element pointer `Ꮡ(x, i)` over such operands. A package-level root is read
        // at the call, once, as Go reads it; the box is spilled nowhere, since nothing is evaluated between.
        private static bool BoxSideEffectFree(ExpressionSyntax expression, SemanticModel model) => expression switch
        {
            IdentifierNameSyntax => true,
            ParenthesizedExpressionSyntax parenthesized => BoxSideEffectFree(parenthesized.Expression, model),
            // 12.1's nil (should-fix 4): the converter's nil pointer `(ж<T>)default`, handed as is; the callee faults at its
            // first dereference, as Go's does.
            CastExpressionSyntax { Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.DefaultLiteralExpression } } => true,
            PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.BitwiseNotExpression } deref => BoxSideEffectFree(deref.Operand, model),
            MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression } member =>
                model.GetSymbolInfo(member).Symbol is var symbol && (symbol is IFieldSymbol || symbol is IPropertySymbol { ContainingType: { } accessor } && FieldAccessMember(accessor)) &&
                BoxSideEffectFree(member.Expression, model),
            InvocationExpressionSyntax { ArgumentList.Arguments.Count: 2 } element when
                element.Expression is IdentifierNameSyntax { Identifier.ValueText: "Ꮡ" } or MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Ꮡ" } &&
                element.ArgumentList.Arguments.All(argument => argument.RefKindKeyword.IsKind(SyntaxKind.None) &&
                    (argument.Expression is LiteralExpressionSyntax || BoxSideEffectFree(argument.Expression, model))) =>
                model.GetSymbolInfo(element).Symbol is IMethodSymbol { Parameters.Length: 2 } method && method.Parameters[0].Type is INamedTypeSymbol { Name: "slice" or "array" or "IArray" },
            _ => false
        };

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
                has = type.GetMembers(definition.Name + Suffix).OfType<IMethodSymbol>().Any(twin => TwinBinds(twin) && IsBoxTwinOf(twin, definition));
            }

            return m_hasBoxTwin[definition] = has;
        }

        // Code review 6: compared as SYMBOLS, ref kinds and the return included (display strings matched a box-shaped
        // member returning no task). A generic twin is read over the definition's own type parameters.
        private static bool IsBoxTwinOf(IMethodSymbol twin, IMethodSymbol definition)
        {
            if (!twin.IsStatic || twin.Parameters.Length != definition.Parameters.Length || twin.TypeParameters.Length != definition.TypeParameters.Length)
                return false;

            IMethodSymbol shaped = twin.TypeParameters.Length > 0 ? twin.Construct(definition.TypeParameters.Cast<ITypeSymbol>().ToArray()) : twin;

            if (shaped.Parameters[0].RefKind != RefKind.None || shaped.Parameters[0].Type is not INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } box ||
                !SymbolEqualityComparer.Default.Equals(box.TypeArguments[0], definition.Parameters[0].Type))
            {
                return false;
            }

            for (int i = 1; i < shaped.Parameters.Length; i++)
            {
                if (shaped.Parameters[i].RefKind != definition.Parameters[i].RefKind ||
                    !SymbolEqualityComparer.Default.Equals(shaped.Parameters[i].Type, definition.Parameters[i].Type))
                {
                    return false;
                }
            }

            // The twin's task over the definition's result: GoTask for a void method, GoTask<R> for R.
            return shaped.ReturnType is INamedTypeSymbol { Name: "GoTask" } task &&
                   (definition.ReturnsVoid
                       ? task.TypeArguments.Length == 0
                       : task.TypeArguments.Length == 1 && SymbolEqualityComparer.Default.Equals(task.TypeArguments[0], definition.ReturnType));
        }

        // Whether a call to `target` (normalized) reaches a box twin: one this run emits, or one a referenced assembly holds.
        // Never with the switch off, so that arm is the base by construction. And never for a method that has a twin of its
        // OWN shape (`Mꞏtwin(this ref T)`, sync's hand-written `Lockꞏtwin`), which the base already awaits in place: a
        // box-shaped `Mꞏtwin(this ж<T>)` beside it is stage (c)'s interface-forwarder twin (`*Mutex` implements Locker),
        // and routing through it would leave plain each call that names no box (closure arm O's 5 `Lock` rows).
        internal bool IsBoxTwinTarget(IMethodSymbol? target) =>
            target is not null &&
            (m_candidates.TryGetValue(target, out Candidate? candidate) ? (candidate.BoxReceiver is not null || candidate.BoxParams is not null) && candidate.Twinned : HasBoxTwinMember(target) && !HasTwinMember(target));

        // Must-fix 2 of the re-review: a call bound to RecvGenerator's box overload `M(this ж<T> Ꮡt, …)`, which forwards to
        // the `this ref T` method, is followed to that method -- resolved by FULL SIGNATURE (an sstring pair holds two
        // forwarders of one name), and only to a plain-`ref`, non-[GoCopyBound] target. A VALUE receiver's box overload
        // copies (`var t = ~Ꮡt;`) and never matches, since its target takes `T`, not `ref T`.
        // Code review 4: a GENERIC method compares its own type parameters with the overload's, so it never matches as is.
        // It is not followed (substituted matching is a later step, if a census asks for it); where it would match once
        // constructed over the overload's type parameters it is handed back in `genericUnfollowed`, so its plain call is
        // named (`generic-unfollowed`) and SN-7 counts it.
        internal IMethodSymbol? BoxOverloadTarget(IMethodSymbol method, out IMethodSymbol? genericUnfollowed)
        {
            genericUnfollowed = null;

            if (!BoxRecv || !method.IsExtensionMethod || method.Parameters.Length == 0 || m_candidates.ContainsKey(method) ||
                method.Parameters[0].Type is not INamedTypeSymbol { Name: "ж", TypeArguments.Length: 1 } box || method.ContainingType is not { } type)
            {
                return null;
            }

            foreach (IMethodSymbol other in type.GetMembers(method.Name).OfType<IMethodSymbol>())
            {
                IMethodSymbol definition = Normalize(other);

                if (SymbolEqualityComparer.Default.Equals(definition, method) || !definition.IsExtensionMethod || definition.Parameters.Length != method.Parameters.Length ||
                    definition.Parameters[0].RefKind != RefKind.Ref || CopyBound(definition) || definition.TypeParameters.Length != method.TypeParameters.Length)
                {
                    continue;
                }

                if (Matches(definition, method, box))
                    return definition;

                if (definition.TypeParameters.Length > 0 && Matches(definition.Construct(method.TypeParameters.Cast<ITypeSymbol>().ToArray()), method, box))
                    genericUnfollowed = definition;
            }

            return null;
        }

        // The full signature: the receiver's type is the box's element, every other parameter by ref kind and type, the
        // same return type.
        private static bool Matches(IMethodSymbol definition, IMethodSymbol overload, INamedTypeSymbol box)
        {
            if (!SymbolEqualityComparer.Default.Equals(definition.Parameters[0].Type, box.TypeArguments[0]))
                return false;

            for (int i = 1; i < definition.Parameters.Length; i++)
            {
                if (definition.Parameters[i].RefKind != overload.Parameters[i].RefKind ||
                    !SymbolEqualityComparer.Default.Equals(definition.Parameters[i].Type, overload.Parameters[i].Type))
                {
                    return false;
                }
            }

            return SymbolEqualityComparer.Default.Equals(definition.ReturnType, overload.ReturnType);
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

            foreach (IParameterSymbol parameter in caller.Symbol.Parameters)
            {
                bool boxed = parameter.Ordinal == 0 && caller.BoxReceiver == parameter.Name || caller.BoxParams?.Contains(parameter.Name) == true;
                string own = "Ꮡ" + parameter.Name;

                if (boxed && (box == own || box.StartsWith(own + ".", StringComparison.Ordinal)))
                {
                    check = "((global::go.ж<" + parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">)default!)" + box.Substring(own.Length);
                    break;
                }
            }

            // PB-5 (12.5): a promoted stack local's box, which exists only in the twin.
            foreach (KeyValuePair<string, ITypeSymbol> promoted in caller.Promotable)
            {
                string own = "Ꮡ" + promoted.Key;

                if (check == box && (box == own || box.StartsWith(own + ".", StringComparison.Ordinal)))
                {
                    check = "((global::go.ж<" + promoted.Value.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">)default!)" + box.Substring(own.Length);
                    break;
                }
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

                // byref-param (12.1 (b)): the twin's own boxed parameter, by its symbol.
                case IdentifierNameSyntax name when symbol is IParameterSymbol { RefKind: RefKind.Ref, ContainingSymbol: IMethodSymbol owner } && caller.BoxParams is { } boxed &&
                                                    SymbolEqualityComparer.Default.Equals(Normalize(owner), caller.Symbol) && boxed.Contains(name.Identifier.ValueText):
                    return "Ꮡ" + name.Identifier.ValueText;

                // A ref local over a box, CURRENT (9.4): declared exactly `ref Ꮡx.DerefOrNull()` / `ref Ꮡx.Value` (or a heap
                // local's `ref Ꮡv.ValueSlot`) over a plain root, and Rederivable -- a root reassigned other than in the
                // converter's re-alias form would hand M another object than the one the ref local names.
                case IdentifierNameSyntax name when symbol is ILocalSymbol { RefKind: RefKind.Ref } local:
                {
                    if (local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: RefExpressionSyntax { Expression: var source } })
                        return null;

                    // byref-param (12.1 (b)): a HEAP local, `ref var v = ref heap(…, out var Ꮡv)`, hands its own box, never
                    // re-pointed. Gated on GoTwinsBoxParam, so the switch-off arm equals arm N.
                    // Review finding 7: refused only where v is RE-POINTED (`v = ref …`) or its box reassigned (`Ꮡv = …`); a
                    // whole-value store `v = …` writes through the box, which still names v.
                    if (BoxParam && HeapBox(source) is { } heapBox && Builtin(model.GetSymbolInfo(source).Symbol, "heap"))
                    {
                        return caller.Syntax.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any(assignment =>
                            assignment.Left is IdentifierNameSyntax left &&
                            (left.Identifier.ValueText == heapBox || left.Identifier.ValueText == name.Identifier.ValueText && assignment.Right is RefExpressionSyntax)) ? null : heapBox;
                    }

                    IdentifierNameSyntax? rootName = BoxRoot(source);

                    // Code review 1: the root is a plain LOCAL or PARAMETER. A package-level pointer would be read again
                    // after a park, though another goroutine may have replaced it, where Go read it once.
                    if (rootName is null || model.GetSymbolInfo(rootName).Symbol is not (ILocalSymbol { RefKind: RefKind.None } or IParameterSymbol { RefKind: RefKind.None }))
                        return null;

                    // Code review 7: a ref local RE-POINTED anywhere in the function (`x = ref Ꮡy.DerefOrNull()`) no longer
                    // names its declared root's object; only the converter's re-alias form (the declaration's own source)
                    // keeps it.
                    string declared = source.WithoutTrivia().ToString();

                    if (caller.Syntax.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any(assignment =>
                            assignment.Left is IdentifierNameSyntax left && left.Identifier.ValueText == name.Identifier.ValueText &&
                            assignment.Right is RefExpressionSyntax { Expression: var repointed } && repointed.WithoutTrivia().ToString() != declared))
                    {
                        return null;
                    }

                    string root = rootName.Identifier.ValueText;

                    return aliases.Rederivable(root, name.Identifier.ValueText) ? root : null;
                }

                // PB-5 (12.5): a STACK local whose reference reaches a box twin. Its twin declares it a heap local
                // (`ref var x = ref heap<T>(init, out var Ꮡx)`), as Go heap-allocates an address that escapes, and hands
                // Ꮡx. Gated on GoTwinsBoxParam, so the switch-off arm equals arm N.
                case IdentifierNameSyntax name when BoxParam && symbol is ILocalSymbol { RefKind: RefKind.None, IsConst: false } stackLocal &&
                                                    stackLocal.Type is { IsRefLikeType: false }:
                {
                    string local = stackLocal.Name;

                    if (caller.Promotable.ContainsKey(local))
                        return "Ꮡ" + local;

                    if (caller.PromoteRefused.ContainsKey(local))
                        return null;

                    string? refusal = PromoteRefusal(stackLocal, caller);

                    if (refusal is not null)
                    {
                        caller.PromoteRefused[local] = refusal;
                        return null;
                    }

                    caller.Promotable[local] = stackLocal.Type;
                    return "Ꮡ" + local;
                }

                // `Ꮡx.DerefOrNull()` as the receiver.
                case InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "DerefOrNull", Expression: IdentifierNameSyntax r } }:
                    return r.Identifier.ValueText;

                // A package variable: a ref property over its static box `Ꮡname`, which must bind at the call to that static
                // of the property's own type (review finding 6: by symbol, never a local of that name).
                case IdentifierNameSyntax name when symbol is IPropertySymbol { IsStatic: true, ReturnsByRef: true } property &&
                                                    model.GetSpeculativeSymbolInfo(position, SyntaxFactory.IdentifierName("Ꮡ" + name.Identifier.ValueText), SpeculativeBindingOption.BindAsExpression).Symbol
                                                        is { IsStatic: true, Kind: SymbolKind.Field or SymbolKind.Property } staticBox &&
                                                    SymbolEqualityComparer.Default.Equals(staticBox.ContainingType, property.ContainingType):
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

        // PB-5 (12.5): why a stack local cannot become a heap local in the twin, or null. `captured`: a lambda or local
        // function names it (a ref local cannot be captured); `declaration`: not the one variable of a local declaration
        // statement directly in a block of the function itself (a deconstruction, a for or using header, a pattern, a
        // switch section); `loop`: declared inside a loop body, a deconstructing foreach included (fresh-per-iteration
        // aliasing, refused in the first cut);
        // `name`: the name Ꮡx is already taken in the function; `shadow`: another local of the same name.
        private static string? PromoteRefusal(ILocalSymbol local, Candidate caller)
        {
            SyntaxNode body = caller.Syntax;

            if (local.DeclaringSyntaxReferences is not [var reference] ||
                reference.GetSyntax() is not VariableDeclaratorSyntax declarator ||
                declarator.Parent is not VariableDeclarationSyntax { Variables.Count: 1, Type: not RefTypeSyntax } ||
                declarator.Parent.Parent is not LocalDeclarationStatementSyntax { UsingKeyword.RawKind: 0, AwaitKeyword.RawKind: 0 } statement ||
                statement.Parent is not BlockSyntax ||
                !body.Span.Contains(statement.Span) ||
                statement.Ancestors().TakeWhile(ancestor => ancestor != body).Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
            {
                return "declaration";
            }

            if (statement.Ancestors().TakeWhile(ancestor => ancestor != body).Any(ancestor => ancestor is ForStatementSyntax or CommonForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax))
                return "loop";

            string name = local.Name;
            List<IdentifierNameSyntax> uses = [.. body.DescendantNodes().OfType<IdentifierNameSyntax>().Where(use => use.Identifier.ValueText == name)];

            if (uses.Any(use => use.Ancestors().TakeWhile(ancestor => ancestor != body).Any(ancestor => ancestor is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)))
                return "captured";

            if (body.DescendantTokens().Any(token => token.ValueText == "Ꮡ" + name))
                return "name";

            if (body.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count(other => other.Identifier.ValueText == name) +
                body.DescendantNodes().OfType<SingleVariableDesignationSyntax>().Count(other => other.Identifier.ValueText == name) > 1)
            {
                return "shadow";
            }

            return null;
        }

        // The box a ref local's source derives from: `Ꮡx.DerefOrNull()`, `Ꮡx.Value` or a heap local's `Ꮡv.ValueSlot`.
        private static IdentifierNameSyntax? BoxRoot(ExpressionSyntax source) => source switch
        {
            InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "DerefOrNull", Expression: IdentifierNameSyntax r } } => r,
            MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Value" or "ValueSlot", Expression: IdentifierNameSyntax r } => r,
            _ => null
        };

        // A heap local's box: `heap<T>(out var Ꮡv)` or `heap(init, out var Ꮡv)`.
        private static string? HeapBox(ExpressionSyntax source) =>
            source is InvocationExpressionSyntax { Expression: GenericNameSyntax { Identifier.ValueText: "heap" } or IdentifierNameSyntax { Identifier.ValueText: "heap" } } heap &&
            heap.ArgumentList.Arguments.LastOrDefault() is { RefKindKeyword.RawKind: (int)SyntaxKind.OutKeyword, Expression: DeclarationExpressionSyntax { Designation: SingleVariableDesignationSyntax designation } }
                ? designation.Identifier.ValueText
                : null;

        // A golib builtin, by symbol (review finding 6): `go.builtin.<name>`.
        private static bool Builtin(ISymbol? symbol, string name) =>
            symbol is IMethodSymbol { Name: var method, ContainingType: { Name: "builtin", ContainingNamespace.Name: "go" } } && method == name;

        // A keyword-named local keeps its escape (code review 3: `func (in *T) M()` converts to `this ref T @in`).
        internal static string Local(string name) =>
            SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

        private static bool SideEffectFree(ExpressionSyntax expression, SemanticModel model) => expression switch
        {
            IdentifierNameSyntax => true,
            MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression } member => model.GetSymbolInfo(member).Symbol is IFieldSymbol && SideEffectFree(member.Expression, model),
            ParenthesizedExpressionSyntax parenthesized => SideEffectFree(parenthesized.Expression, model),
            _ => false
        };

        // A plain call to a box twin, for the report (SN-7 reads their count); `reason` tags a row that is plain for a
        // named reason (code review 4: `generic-unfollowed`).
        internal void NoteBoxPlain(InvocationExpressionSyntax node, IMethodSymbol target, string? reason = null, string? text = null)
        {
            BoxPlain++;
            m_boxPlainRows.Add("// P " + Where(node) + "|" + target.Name + "|" + (text ?? ReceiverText(node)) + (reason is null ? "" : "|" + reason));
        }

        // Code review 2: each call followed through a box overload, with the method it was followed to, so that a unit
        // pins BoxOverloadTarget's guards directly (an overload pair, a [GoCopyBound] method, a value method).
        internal void NoteViaOverload(InvocationExpressionSyntax node, IMethodSymbol overload, IMethodSymbol target) =>
            m_viaOverloadRows.Add("// V " + Where(node) + "|" + overload.Name + "|" + target.ToDisplayString());

        private static string Where(SyntaxNode node) =>
            System.IO.Path.GetFileName(node.SyntaxTree.FilePath) + "|" + (node.SyntaxTree.GetLineSpan(node.Span).StartLinePosition.Line + 1);

        private static string ReceiverText(InvocationExpressionSyntax node) =>
            node.Expression is MemberAccessExpressionSyntax access ? access.Expression.ToString().Replace("\r", " ").Replace("\n", " ") : "-";

        internal IEnumerable<string> BoxPlainRows => m_boxPlainRows;

        internal IEnumerable<string> ViaOverloadRows => m_viaOverloadRows;

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

                // Code review 1: the same hazard through a PACKAGE-LEVEL pointer (`ref var x = ref gp.DerefOrNull()`, gp a
                // static field or property): derived again from gp after a park, where Go read it once.
                if (BoxRoot(source) is { } root && model.GetSymbolInfo(root).Symbol is IFieldSymbol { IsStatic: true } or IPropertySymbol { IsStatic: true })
                    return true;
            }

            return false;
        }
    }
}
