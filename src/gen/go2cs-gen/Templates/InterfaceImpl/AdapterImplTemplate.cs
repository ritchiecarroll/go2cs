// AdapterImplTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the POINTER-sourced interface implementation adapter for a
/// <c>GoImplement&lt;TStruct, TInterface&gt;(Pointer = true)</c> attribute.
/// </summary>
/// <remarks>
/// A Go interface value created from a pointer (<c>var s Iface = &amp;t</c>) holds the
/// <em>pointer</em> — every call through the interface mutates the original object, and a type
/// assert back to <c>*T</c> recovers that same pointer. The value-boxing partial-struct
/// implementation cannot model this (the C# interface box is a COPY, and direct-ж receiver
/// methods cannot bind from a struct's <c>this</c> at all), so the adapter wraps the receiver
/// box <c>ж&lt;T&gt;</c> itself: interface members forward to the box (binding the direct-ж or
/// RecvGenerator ж-overload forms), equality is box identity (Go pointer equality), and the
/// golib type-assert machinery unwraps adapters through <c>IжAdapter.Box</c>.
/// </remarks>
internal class AdapterImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;
    public required string AdapterName;
    public bool ImplementsFormattable;
    public required string AdapterScope;
    public required List<MethodInfo> Methods;

    // A GENERIC struct's adapter is ONE generic class over the struct's open type parameters
    // (crypto/elliptic's `nistCurveжCurve<Point>` wrapping `ж<nistCurve<Point>>`), NOT a class
    // per instantiation. TypeParameters is the `<Point>` list appended to the class NAME (the
    // ctor keeps the bare AdapterName); ConstraintClause repeats the struct's `where Point :
    // nistPoint<Point>` so the wrapped `ж<nistCurve<Point>>` field satisfies nistCurve's own
    // constraint (CS0314). Both are empty for a non-generic adapter.
    public string TypeParameters = "";
    public string ConstraintClause = "";

    // Maps an interface member's simple name to its forwarding receiver expression:
    // "m_box" when the struct method binds on the box (direct-ж primary form, or a
    // [GoRecv] ref extension whose RecvGenerator ж-twin exists), or "m_box.Value" for
    // a plain value-receiver method (Go copies the value at the call, matching ref-return
    // property access semantics here).
    public required Dictionary<string, string> ForwardReceivers;

    // Per-method package-class STATIC call target for a FOREIGN struct in another namespace
    // segment — its extensions are invisible to lookup here (CS1929); the receiver string in
    // ForwardReceivers becomes the first ARGUMENT (m_box / m_box.Value / ref m_box.Value).
    public Dictionary<string, string>? ForwardStaticCalls;

    // ⚠ Per-method RESULT ADAPTER for a member whose DECLARED result is an interface the forwarded
    // Go method does not return. The adapter implements the interface AS DECLARED, so the forwarded
    // receiver box has to be wrapped in the RESULT interface's own adapter before it is handed back:
    // Go has no return covariance, and handing the box back is CS0266 inside this generated file.
    // crypto/mlkem's projected `decapsulationKey[encapsulationKey]` is the corpus shape — its
    // `EncapsulationKey() E` binds E to the INTERFACE while the concrete method returns the pointer.
    // Empty for every member that needs no wrap, which is all of them outside that shape.
    public Dictionary<string, string>? ForwardResultWraps;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Pointer-sourced '{{GetSimpleName(InterfaceName)}}' implementation adapter for 'ж&lt;{{StructName}}&gt;' —
             /// the interface value aliases the wrapped receiver box exactly as Go's interface holds the '*T'.
             /// </summary>
             [{{NonUserCodeAttribute}}]
             {{AdapterScope}} sealed class {{AdapterName}}{{TypeParameters}} : {{InterfaceName}}, IжAdapter{{ConstraintClause}}
             {
                 private readonly ж<{{StructName}}> m_box;

                 // A nil *T entering the interface wraps the CANONICAL typed nil box (never a null
                 // Box): the interface value keeps its Go dynamic type, typed-nil equality holds
                 // across the any/interface boundary, and `case *T:` matches with a nil pointee —
                 // Go's interface-holding-nil-pointer semantics. See ж<T>.NilBox.
                 public {{AdapterName}}(ж<{{StructName}}> box) => m_box = box ?? ж<{{StructName}}>.NilBox;

                 public object? Box => m_box;{{RegistryInitialization}}

                 {{MethodsImplementation}}

                 // Go pointer-interface equality is pointer identity: two interface values created
                 // from the same '*T' compare equal, as does the pointer box itself.
                 public override bool Equals(object? obj) => obj switch
                 {
                     IжAdapter adapter => ReferenceEquals(adapter.Box, m_box),
                     ж<{{StructName}}> box => ReferenceEquals(box, m_box),
                     _ => ReferenceEquals(obj, this)
                 };

                 public override int GetHashCode() => m_box?.GetHashCode() ?? 0;

                 public override string? ToString() => m_box?.ToString();{{FormattableImplementation}}
             }
         """;

    // Registers the box→adapter factory at module load so golib's type-assert machinery
    // (builtin._<T>) can re-wrap a raw receiver box matched against a NAMED interface case
    // label by Go method-set semantics. A GENERIC adapter cannot host a module initializer
    // and its open registration key is unrepresentable — generic instantiations stay on the
    // nominal-match path.
    private string RegistryInitialization =>
        TypeParameters.Length > 0
            ? string.Empty
            : "\r\n\r\n        [global::System.Runtime.CompilerServices.ModuleInitializer]\r\n" +
              $"        internal static void {TempVarMarker}RegisterAdapter() =>\r\n" +
              $"            global::go.AdapterRegistry.Register(typeof({PointerPrefix}<{StructName}>), typeof({InterfaceName}), static box => new {AdapterName}(({PointerPrefix}<{StructName}>)box));";

    // The interface may inherit System.IFormattable (the hand-finished io stub's Reader);
    // its member cannot forward through the box (no such overload) — implement directly.
    private string FormattableImplementation =>
        ImplementsFormattable
            ? "\r\n\r\n        string System.IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => m_box?.ToString() ?? \"\";"
            : string.Empty;

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleMethodName = GetSimpleName(method.Name);

                // The member is IMPLEMENTED under the interface's name and FORWARDS under the emitted
                // one, which differ when the collision pass Δ-renamed the implementation. Both lookups
                // below are keyed by the DECLARED name — ForwardReceivers is built from the struct's
                // own method list — so the resolved name drives the receiver choice as well as the call
                // target: a value-receiver `ΔString(this URLValue)` needs `m_box.Value`, and the Go-name
                // miss used to fall through to the `m_box` default and strand it (CS1929) even once the
                // call target was right. Inert for every member the collision pass left alone.
                string forwardName = method.ForwardMemberName(simpleMethodName);

                if (result.Length > 0)
                    result.Append("\r\n\r\n        ");

                // A cross-assembly unexported interface marker (Go's package-sealing exprNode()/tree()
                // etc.) has no accessible implementation to forward to (CS1061) and is never callable
                // from outside its package — satisfy the required member with a no-op / default stub.
                if (method.IsInaccessibleMarker)
                {
                    result.Append($"{method.ReturnType} {method.GetSignature()}{(method.ReturnType == "void" ? " { }" : " => default!;")}");
                    continue;
                }

                if (!ForwardReceivers.TryGetValue(forwardName, out string? receiver))
                    receiver = "m_box";

                // The projected-result wrap, applied to whichever forward form the member takes so
                // the two branches cannot diverge on it. Empty parens for the wrap's own argument
                // are impossible here: the forward is always an expression.
                string wrapOpen = "";
                string wrapClose = "";

                if (ForwardResultWraps is not null && ForwardResultWraps.TryGetValue(forwardName, out string? resultAdapter))
                {
                    wrapOpen = $"new {resultAdapter}(";
                    wrapClose = ")";
                }

                if (ForwardStaticCalls is not null && ForwardStaticCalls.TryGetValue(forwardName, out string? staticCall))
                {
                    string staticArgs = string.IsNullOrEmpty(method.CallParameters) ? receiver : $"{receiver}, {method.CallParameters}";
                    result.Append($"{method.ReturnType} {method.GetSignature()} => {wrapOpen}{staticCall}{method.GetGenericSignature()}({staticArgs}){wrapClose};");
                }
                else
                {
                    result.Append($"{method.ReturnType} {method.GetSignature()} => {wrapOpen}{receiver}.{forwardName}{method.GetGenericSignature()}({method.CallParameters}){wrapClose};");
                }
            }

            return result.ToString();
        }
    }
}
