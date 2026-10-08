// ValueAdapterImplTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the VALUE-sourced interface implementation adapter for a
/// <c>GoImplement&lt;TStruct, TInterface&gt;</c> attribute whose struct is FOREIGN — declared in
/// another assembly, so the value-boxing partial-struct implementation is impossible there and
/// unknowable here (os's <c>Signal</c> interface is downstream of <c>syscall.Signal</c>; neither
/// assembly can partial the other).
/// </summary>
/// <remarks>
/// A Go interface value created from a VALUE holds a copy, so the adapter wraps a COPY of the
/// struct and forwards interface members to its methods. Forwarding calls the extension methods
/// in their CONTAINER-QUALIFIED static form (<c>global::go.encoding.binary_package.Uint32(m_value, …)</c>):
/// instance-form extension calls require a <c>using</c> for the foreign package's namespace, which
/// the generated file only has for root-namespace packages (<c>using go;</c>) — a sub-namespace
/// package like encoding/binary never resolved (debug/plan9obj CS1061 ×6). Equality is VALUE
/// equality, matching Go's value-interface comparison semantics.
/// </remarks>
internal class ValueAdapterImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;
    public required string AdapterName;
    public required string AdapterScope;
    public bool ImplementsFormattable;
    public required List<MethodInfo> Methods;

    /// <summary>
    /// Members the wrapped struct satisfies by PROMOTION through an embedded interface FIELD rather
    /// than by a method of its own, mapped to that field's name.
    /// </summary>
    /// <remarks>
    /// net/http's <c>type nothingWrittenError struct { error }</c> is the shape: its only declared
    /// method is <c>Unwrap</c>, and <c>Error()</c> lives on the embedded <c>error</c> VALUE. Composed
    /// as an extension call on the copy (<c>http_package.Error(m_value)</c>) it bound whatever
    /// one-argument <c>Error</c> the package happened to declare — <c>http2ConnectionError</c>'s —
    /// and reported CS1503 against a type the source never mentions. Forward through the field.
    /// </remarks>
    public Dictionary<string, string>? PromotedFieldForwards;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Value-sourced '{{GetSimpleName(InterfaceName)}}' implementation adapter for the foreign
             /// '{{StructName}}' — wraps a COPY, exactly as Go's interface holds a value.
             /// </summary>
             [{{NonUserCodeAttribute}}]
             {{AdapterScope}} sealed class {{AdapterName}} : {{InterfaceName}}, IValueAdapter
             {
                 private readonly {{StructName}} m_value;

                 // The dependency registers this adapter for trimming (golib's GoTypeRegistry) whenever it is constructed.
                 {{string.Format(AdapterRegistration, AdapterName)}}
                 public {{AdapterName}}({{StructName}} value) => m_value = value;

                 // The Go DYNAMIC TYPE of this interface value is the wrapped struct, never this
                 // class: the golib runtime unwraps here for `==`, type asserts, type switches and
                 // %T. Implemented EXPLICITLY so it can never collide with a forwarded Go method
                 // named Value (a promoted adapter binds its members by bare name).
                 object? IValueAdapter.Value => m_value;

                 {{MethodsImplementation}}

                 // Go value-interface equality compares the held values.
                 public override bool Equals(object? obj) => obj switch
                 {
                     {{AdapterName}} adapter => m_value.Equals(adapter.m_value),
                     {{StructName}} value => m_value.Equals(value),
                     _ => false
                 };

                 public override int GetHashCode() => m_value.GetHashCode();

                 public override string? ToString() => m_value.ToString();{{FormattableImplementation}}
             }
         """;

    // The interface may inherit System.IFormattable (the hand-finished io stub's Reader);
    // its member cannot forward through the copy uncast — implement directly.
    private string FormattableImplementation =>
        ImplementsFormattable
            ? "\r\n\r\n        string System.IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => m_value.ToString() ?? \"\";"
            : string.Empty;

    // The static class containing the wrapped struct's extension methods — the struct's own
    // container (converted Go methods are extensions on the package class the struct nests in).
    // Empty when StructName has no qualifier; forwarding then falls back to instance form.
    private string ExtensionContainer
    {
        get
        {
            // Derive from the OPEN name — a dot inside generic type arguments is not a
            // container separator.
            string baseName = StructName;
            int genericStart = baseName.IndexOf('<');

            if (genericStart > 0)
                baseName = baseName.Substring(0, genericStart);

            int lastDot = baseName.LastIndexOf('.');
            return lastDot > 0 ? baseName.Substring(0, lastDot) : string.Empty;
        }
    }

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();
            string container = ExtensionContainer;

            foreach (MethodInfo method in Methods)
            {
                string simpleMethodName = GetSimpleName(method.Name);

                // Implemented under the interface's name, forwarded under the EMITTED one — see
                // MethodInfo.ForwardName. Inert unless the collision pass renamed the implementation.
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

                // Promoted through an embedded interface FIELD — the member is on the field's
                // interface value, not on the struct, so no extension form can reach it.
                if (PromotedFieldForwards is not null && PromotedFieldForwards.TryGetValue(simpleMethodName, out string? promotedField))
                {
                    result.Append($"{method.ReturnType} {method.GetSignature()} => m_value.{promotedField}.{forwardName}{method.GetGenericSignature()}({method.CallParameters});");
                    continue;
                }

                if (container.Length > 0)
                {
                    string callParameters = method.CallParameters;
                    string forwardedParameters = callParameters.Length > 0 ? $"m_value, {callParameters}" : "m_value";

                    result.Append($"{method.ReturnType} {method.GetSignature()} => {container}.{forwardName}{method.GetGenericSignature()}({forwardedParameters});");
                }
                else
                {
                    result.Append($"{method.ReturnType} {method.GetSignature()} => m_value.{forwardName}{method.GetGenericSignature()}({method.CallParameters});");
                }
            }

            return result.ToString();
        }
    }
}
