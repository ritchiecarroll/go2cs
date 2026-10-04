// ReceiverMethodTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.ReceiverMethod;

internal class ReceiverMethodTemplate : TemplateBase
{
    // Template Parameters
    public required MethodInfo Method;

    // Whether the SOURCE method carries [MethodImpl(MethodImplOptions.NoInlining)] -- the converter's
    // mark (computeNoInliningClosure) on a function whose frame runtime.Caller/Callers' skip count
    // depends on. The ж-forwarder emitted here must then carry it too: it is a two-line deref-and-call
    // the optimizing JIT inlines into the CALLER, and although isGoSourceFrame never counts a go2cs-gen
    // frame (so the skip count survives), the caller's return address then sits inside the inlinee
    // with no IL offset -- the caller's frame resolves to file "" / line 0. Measured on log/slog's
    // TestCallDepth: FAIL under TieredCompilation=0, PASS with JitNoInline=1. Required for the same
    // reason ReceiverTypeIsPublic is: the one construction site cannot silently take a default.
    public required bool NoInlining;

    // The SOURCE method's [OverloadResolutionPriority(n)] argument, or null. The converter marks the
    // sstring member of an sstring twin with priority 1 (docs/phase4/DESIGN-sstring-twin-pilot.md), and
    // the ж-forwarder emitted here must carry the same priority. Otherwise the two forwarders a twin pair
    // generates tie, and a u8 argument reaching the method through a pointer is CS0121, because both
    // parameter types are implicitly convertible from ReadOnlySpan<byte> (i9 twin probe, arm n1).
    // Required for the same reason NoInlining is.
    public required string? OverloadResolutionPriority;

    private string ForwarderAttributes
    {
        get
        {
            string attributes = NoInlining
                ? $"{GeneratedCodeAttribute}, global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)"
                : GeneratedCodeAttribute;

            if (OverloadResolutionPriority is not null)
                attributes += $", global::System.Runtime.CompilerServices.OverloadResolutionPriority({OverloadResolutionPriority})";

            // The forwarder calls the user's [GoRecv] method: Just My Code steps through it.
            attributes += $", {NonUserCodeAttribute}";

            return $"[{attributes}]";
        }
    }

    // Whether the RECEIVER type is public IN THE EMISSION, read from its symbol by the generator
    // (Common.EffectiveScopeIsPublic) rather than from the Go export case of its name. Required, so
    // the one construction site (RecvGenerator) cannot forget it and silently take a default. See
    // TargetScope at the bottom of this file for what it decides and why the name was the wrong oracle.
    public required bool ReceiverTypeIsPublic;

    private string? m_receiverParamName;
    private string ReceiverParamName => m_receiverParamName ??= Method.Parameters.First().name;

    // The heap-box parameter/local name (`Ꮡx`). Built from the UNescaped receiver name: a C#-keyword
    // receiver is escaped as `@enum`, but `Ꮡ@enum` is invalid ('@' is only valid as a leading prefix).
    // The `Ꮡ` prefix already yields a distinct, valid identifier, so strip the '@' → `Ꮡenum`.
    private string? m_receiverBoxName;
    private string ReceiverBoxName => m_receiverBoxName ??= $"{AddressPrefix}{GetUnsanitizedIdentifier(ReceiverParamName)}";

    private string? m_receiverParamType;
    private string ReceiverParamType => m_receiverParamType ??= $"{PointerPrefix}<{Method.Parameters.First().type}>";

    // The receiver alias is nil-DEFERRING, matching the converter's own entry preamble for a
    // direct-ж receiver. This bridge is how a `ref T`-receiver method is reached through a box, so
    // `.Value` deref'd at the BRIDGE — a nil receiver panicked before the method it forwards to
    // could run at all, which is one call frame earlier than Go, where the method RUNS and only the
    // body's own dereference panics. DerefOrNull binds a null ref instead: legal to hold and to pass
    // on as `ref T`, and the callee's first field read/write raises the nil-pointer panic with Go's
    // message, at Go's point — after any side effect the callee performed first. A non-nil box is
    // unaffected (the same real slot).
    // A B′-S0 arm-(a) primary returns `ref T` — the receiver itself (the R3 ruling, 2026-09-02:
    // Go's fluent `return v` returns the receiver POINTER, which the primary cannot mint). The
    // twin restores the ж surface for every existing consumer: it delegates (discarding the ref —
    // the mutation already landed in the box's own storage through DerefOrNull) and returns ITS
    // OWN box, which IS Go's receiver pointer — `p := Ꮡv.M(…); p == Ꮡv` holds by construction
    // (the identity guard row in ZhBoxSelectionProbeTests' fluent class).
    private bool IsRefReturnPrimary =>
        Method.ReturnType.StartsWith("ref ", StringComparison.Ordinal);

    public override string TemplateBody => IsRefReturnPrimary
        ? $$"""
            {{ForwarderAttributes}}
            {{TargetScope}} static {{ReceiverParamType}} {{Method.Name}}{{Method.GetGenericSignature()}}({{DeclParams}}){{Method.GetWhereConstraints()}}
            {
                ref var {{ReceiverParamName}} = ref {{ReceiverBoxName}}.{{NilDeferringDerefAccessor}};
                {{ReceiverParamName}}.{{Method.Name}}({{CallParams}});
                return {{ReceiverBoxName}};
            }
        """
        : $$"""
            {{ForwarderAttributes}}
            {{TargetScope}} static {{Method.ReturnType}} {{Method.Name}}{{Method.GetGenericSignature()}}({{DeclParams}}){{Method.GetWhereConstraints()}}
            {
                ref var {{ReceiverParamName}} = ref {{ReceiverBoxName}}.{{NilDeferringDerefAccessor}};
                {{ReturnStatement}}{{ReceiverParamName}}.{{Method.Name}}({{CallParams}});
            }
        """;

    private string DeclParams
    {
        get
        {
            List<string> result = [];
            bool first = true;

            foreach ((string type, string name) in Method.Parameters)
            {
                if (first)
                {
                    result.Add($"this {PointerPrefix}<{type}> {ReceiverBoxName}");
                    first = false;
                }
                else
                {
                    result.Add($"{type} {name}");
                }
            }

            return string.Join(", ", result);
        }
    }

    private string ReturnStatement =>
        Method.ReturnType == "void" ? "" : "return ";

    private string CallParams => 
        string.Join(", ", Method.Parameters.Skip(1).Select(item => item.name));

    // The narrowest of {this method's own scope, the receiver type's scope}. The narrowing is
    // load-bearing in BOTH directions: a public overload over a `ж<internalT>` receiver is CS0051,
    // and re-widening a method the converter deliberately narrowed is CS0050/CS0051 the other way.
    //
    // What changed (2026-09-20): the receiver side used to be `GetScope(GetSimpleName(type))` — the
    // Go export case of the NAME. That answers "internal" for a type the converter PUBLICIZED, i.e.
    // an unexported Go type emitted `public partial struct` because an exported signature reaches it
    // (ecdsa's hmacDRBG, returned by the exported TestingOnlyNewDRBG). The overload was then minted
    // `internal` over a genuinely public type, so a CONSUMING assembly saw only the `ref T` primary
    // and `box.Method(…)` there was CS1929 — while the twin partials TypeGenerator and
    // ImplicitConvGenerator emit for that same type were already public. RecvGenerator now reads the
    // receiver's DECLARED accessibility through the same shared rule as its siblings, and the answer
    // arrives here as ReceiverTypeIsPublic.
    private string TargetScope
    {
        get
        {
            string receiverScope = ReceiverTypeIsPublic ? "public" : "internal";
            return Scope == receiverScope ? Scope : "internal";
        }
    }
}
