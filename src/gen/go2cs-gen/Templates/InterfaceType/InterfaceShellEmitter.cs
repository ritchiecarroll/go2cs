// InterfaceShellEmitter.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InterfaceType;

/// <summary>
/// Renders the two runtime duck-typing SHELL classes emitted beside a converted NAMED interface,
/// plus the <c>[GoInterfaceShell]</c> attribute that makes them discoverable.
/// </summary>
/// <remarks>
/// <para>
/// A named Go interface has no runtime duck-typing surface today: golib's type-assert machinery can
/// PROBE a dynamic value's method set structurally, but with nothing to construct it can only answer
/// the probe, never satisfy the assertion. The compile-time recorders cover most pairs, yet they are
/// structurally incomplete by construction — a dynamic type may live in an assembly converted AFTER
/// the interface's own (io/fs is converted before os, so nothing in fs can record os.dirFS).
/// </para>
/// <para>
/// With no dynamic code generation available, a per-interface compile-time artifact is the
/// irreducible minimum, and it belongs in the interface's OWN package class: that assembly is loaded
/// at every asserting site, and one class in one assembly gives one cross-assembly identity. Two are
/// emitted because the tiers have different costs and different Native-AOT properties — see
/// <c>golib/AdapterBinder.cs</c>, which owns the tier choice and all method binding:
/// </para>
/// <list type="number">
/// <item><c>Δ&lt;Iface&gt;&lt;ᴛTTarget&gt;</c> — delegate-bound, for a REFERENCE-typed dynamic value
/// (every <c>ж&lt;X&gt;</c> box). A forwarded call is a delegate invocation, which is what a
/// duck-typed <c>io.Reader</c> inside <c>io.Copy</c> pays per call.</item>
/// <item><c>Δ&lt;Iface&gt;ᴛObj</c> — reflective, for a VALUE-typed dynamic value (the <c>os.dirFS</c>
/// case). Holds the value as <c>object</c> and needs no generic instantiation, so it stays available
/// where a <c>GetType()</c>-driven value-type close is not.</item>
/// </list>
/// <para>
/// Names are deliberately NOT contractual — discovery is by attribute, so this emitter may
/// disambiguate them freely — and no static member is added to the interface itself, which is what
/// kept the CS0108 hiding class and the embedded-static method-set corruption out of the design.
/// </para>
/// </remarks>
internal sealed class InterfaceShellEmitter
{
    // Type T variable that cannot conflict with a method's own generic types.
    private const string TypeTTarget = $"{ShadowVarMarker}TTarget";

    // The receiver LOCAL and the delegates' receiver PARAMETER share one marker-suffixed name so a
    // Go interface method with a parameter called `target` (net/http.Pusher.Push) cannot shadow it.
    private const string ReceiverLocal = $"target{CapturedVarMarker}";

    private readonly string m_interfaceName;
    private readonly string m_scope;
    private readonly string m_genericShellName;
    private readonly string m_objectShellName;
    private readonly MethodInfo[] m_methods;
    private readonly bool m_emitObjectShell;

    public InterfaceShellEmitter(string nonGenericInterfaceName, string? interfaceScope, MethodInfo[] methods)
    {
        m_interfaceName = nonGenericInterfaceName;

        // A shell is never referenced by name — the binder finds it through the attribute — so it is
        // emitted as narrowly as the interface allows. `internal` everywhere else keeps a public
        // interface's shell out of the assembly's public surface and, because its members are then
        // never more accessible than a parameter type can be, sidesteps the inconsistent-accessibility
        // class entirely (CS0051/CS0060).
        m_scope = interfaceScope == "private" ? "private" : "internal";

        // Compose on the BARE name: a Go interface whose name is a C# keyword is declared escaped
        // (database/sql's `decimal` → `@decimal`) and `@` is legal only at the START of a token, so
        // `Δ@decimal` would not parse. The Δ prefix also guarantees the composed name is never itself
        // a keyword, so it needs no escape of its own.
        string bareName = GetUnsanitizedIdentifier(nonGenericInterfaceName);

        m_genericShellName = $"{ShadowVarMarker}{bareName}";
        m_objectShellName = $"{ShadowVarMarker}{bareName}{TempVarMarker}Obj";

        // One forwarder per Go method NAME. A Go interface cannot declare two methods with the same
        // name, so the name is the method set's key — but a non-Go base interface could still
        // contribute a colliding member, and the binder resolves purely by name. Direct methods are
        // collected before inherited ones, so first-wins keeps the syntax-declared member.
        m_methods = methods
            .GroupBy(method => GetUnsanitizedIdentifier(method.Name))
            .Select(group => group.First())
            .ToArray();

        // The reflective shell forwards through `object`, so it can only exist when every member
        // survives that round-trip. Omitting it is not a loss of capability: the binder belts a
        // value-typed source to the delegate shell (see AdapterBinder.BuildFactory).
        m_emitObjectShell = m_methods.All(method => method.IsInvokerForwardable);
    }

    /// <summary>
    /// Gets the attribute line stamped on the interface's partial declaration.
    /// </summary>
    public string Attribute
    {
        get
        {
            string names = string.Join(", ", m_methods.Select(method => $"\"{GetUnsanitizedIdentifier(method.Name)}\""));
            string objectShell = m_emitObjectShell ? $"typeof({m_objectShellName})" : "null";

            return $"[global::go.GoInterfaceShell(typeof({m_genericShellName}<>), {objectShell}, {names})]\r\n    ";
        }
    }

    /// <summary>
    /// Gets the shell class declarations, appended after the interface declaration.
    /// </summary>
    public string Shells => m_emitObjectShell ? $"{GenericShell}{ObjectShell}" : GenericShell;

    private string GenericShell =>
        $$"""


              // Runtime duck-typing shell for '{{m_interfaceName}}' over a REFERENCE-typed Go dynamic value —
              // every receiver box 'ж<X>', i.e. every pointer-sourced Go interface value. Used when the
              // compile-time recorders could not have seen the (type, interface) pair, e.g. a dynamic type
              // in an assembly converted after this one. Each interface method is a PRE-BOUND delegate, so
              // a forwarded call costs a delegate invocation rather than a reflective one. Discovered
              // through the interface's [GoInterfaceShell] attribute; built only by go.AdapterBinder, which
              // owns the Go value-vs-pointer receiver rule.
              [{{GeneratedCodeAttribute}}]
              {{m_scope}} sealed class {{m_genericShellName}}<{{TypeTTarget}}> : {{m_interfaceName}}, IInterfaceAdapter
              {
                  private readonly {{TypeTTarget}} m_target{{TempVarMarker}} = default!;
                  private readonly {{PointerPrefix}}<{{TypeTTarget}}>? m_target{{TempVarMarker}}_ptr;
                  private readonly bool m_target{{TempVarMarker}}_is_ptr;

                  // Set when every interface method bound a receiver method for that source form. The
                  // binder reads these BEFORE handing the shell out, so an unbindable pair reproduces a
                  // plain assertion MISS instead of null-dispatching on the first forwarded call.
                  internal static readonly bool {{TempVarMarker}}BoundByPtr;
                  internal static readonly bool {{TempVarMarker}}BoundByVal;

                  public {{m_genericShellName}}({{TypeTTarget}} {{ReceiverLocal}}) => m_target{{TempVarMarker}} = {{ReceiverLocal}};

                  public {{m_genericShellName}}({{PointerPrefix}}<{{TypeTTarget}}> {{ReceiverLocal}}_ptr)
                  {
                      m_target{{TempVarMarker}}_ptr = {{ReceiverLocal}}_ptr;
                      m_target{{TempVarMarker}}_is_ptr = true;
                  }

                  // The Go dynamic value this shell stands in for — the receiver box (the *T) when
                  // pointer-backed, the wrapped value otherwise. Joining the IInterfaceAdapter unwrap
                  // protocol keeps the shell invisible to interface equality, type() switches, %T and
                  // re-asserts, and preserves Go's value-vs-pointer method-set rule on re-assert (a
                  // value-backed shell unwraps to the VALUE, exposing only its value method set).
                  // Explicit implementation: the wrapped interface may itself declare a member named Value.
                  object? IInterfaceAdapter.Value => m_target{{TempVarMarker}}_is_ptr ? m_target{{TempVarMarker}}_ptr : (object?)m_target{{TempVarMarker}};

                  // The dynamic value's Go text (its Error(), else its String()), which the value and the ж box
                  // each answer through golib's GoMethodText; a shell adds nothing of its own to print.
                  public override string ToString() => (m_target{{TempVarMarker}}_is_ptr ? m_target{{TempVarMarker}}_ptr?.ToString() : m_target{{TempVarMarker}}?.ToString()) ?? "<nil>";
          {{GenericForwarders}}{{GenericBinder}}
              }
          """;

    private string GenericForwarders
    {
        get
        {
            StringBuilder results = new();

            foreach (MethodInfo method in m_methods)
            {
                string bareName = GetUnsanitizedIdentifier(method.Name);
                string typedParameters = Prefixed(method.GetTypedParameters(false));
                string callParameters = Prefixed(method.GetCallParameters(false));
                string returnStatement = method.ReturnType == "void" ? "" : "return ";

                results.Append(
                    $$"""


                              // '{{m_interfaceName}}.{{method.Name}}' forwarder
                              private delegate {{method.ReturnType}} {{bareName}}ByPtr{{method.GetGenericSignature()}}({{PointerPrefix}}<{{TypeTTarget}}> {{ReceiverLocal}}{{typedParameters}}){{method.GetWhereConstraints()}};
                              private delegate {{method.ReturnType}} {{bareName}}ByVal{{method.GetGenericSignature()}}({{TypeTTarget}} {{ReceiverLocal}}{{typedParameters}}){{method.GetWhereConstraints()}};

                              private static readonly {{bareName}}ByPtr? s_{{bareName}}ByPtr;
                              private static readonly {{bareName}}ByVal? s_{{bareName}}ByVal;

                              [global::System.Diagnostics.DebuggerNonUserCode]
                              public {{method.ReturnType}} {{method.GetSignature(false)}}
                              {
                                  // Go's *T method set prefers the pointer-receiver overload and otherwise
                                  // calls the value-receiver method on the CURRENT pointee (Go copies the
                                  // value at the call, so the deref happens here, not at construction).
                                  if (m_target{{TempVarMarker}}_is_ptr && s_{{bareName}}ByPtr is not null)
                                      {{returnStatement}}s_{{bareName}}ByPtr(m_target{{TempVarMarker}}_ptr!{{callParameters}});
                                  else
                                      {{returnStatement}}s_{{bareName}}ByVal!(m_target{{TempVarMarker}}_is_ptr ? m_target{{TempVarMarker}}_ptr!.Value : m_target{{TempVarMarker}}{{callParameters}});
                              }
                      """);
            }

            return results.ToString();
        }
    }

    private string GenericBinder
    {
        get
        {
            StringBuilder results = new();

            results.Append(
                $$"""


                          static {{m_genericShellName}}()
                          {
                              global::System.Reflection.MethodInfo? byPtr{{TempVarMarker}};
                              global::System.Reflection.MethodInfo? byVal{{TempVarMarker}};
                              bool boundByPtr{{TempVarMarker}} = true;
                              bool boundByVal{{TempVarMarker}} = true;
                  """);

            foreach (MethodInfo method in m_methods)
            {
                // `nameof` is deliberately NOT used: Roslyn hands back the UNescaped name for an
                // inherited (AllInterfaces) member, and the binder matches reflection's Name, which is
                // the bare form. The compound delegate/field names compose on the bare name too —
                // keyword + suffix is never itself a keyword and `@` cannot appear mid-token.
                string bareName = GetUnsanitizedIdentifier(method.Name);

                results.Append(
                    $$"""


                                  global::go.AdapterBinder.ResolveReceiverMethods(typeof({{TypeTTarget}}), "{{bareName}}", out byPtr{{TempVarMarker}}, out byVal{{TempVarMarker}});
                                  s_{{bareName}}ByPtr = byPtr{{TempVarMarker}} is null ? null : byPtr{{TempVarMarker}}.CreateStaticDelegate(typeof({{bareName}}ByPtr)) as {{bareName}}ByPtr;
                                  s_{{bareName}}ByVal = byVal{{TempVarMarker}} is null ? null : byVal{{TempVarMarker}}.CreateStaticDelegate(typeof({{bareName}}ByVal)) as {{bareName}}ByVal;
                                  boundByPtr{{TempVarMarker}} &= s_{{bareName}}ByPtr is not null || s_{{bareName}}ByVal is not null;
                                  boundByVal{{TempVarMarker}} &= s_{{bareName}}ByVal is not null;
                      """);
            }

            results.Append(
                $$"""


                              {{TempVarMarker}}BoundByPtr = boundByPtr{{TempVarMarker}};
                              {{TempVarMarker}}BoundByVal = boundByVal{{TempVarMarker}};
                          }
                  """);

            return results.ToString();
        }
    }

    private string ObjectShell =>
        $$"""


              // Runtime duck-typing shell for '{{m_interfaceName}}' over a VALUE-typed Go dynamic value — the
              // os.dirFS case, a '[GoType("@string")]' struct held in an fs.FS. Holds the value as 'object'
              // and forwards through the invokers go.AdapterBinder resolved once for the pair, so it needs
              // no generic instantiation at all: a MakeGenericType driven by a run-time GetType() over a
              // value type is exactly the shape Native AOT cannot materialize. Slower per call than the
              // delegate shell, which is why it is the VALUE tier and not the only tier.
              [{{GeneratedCodeAttribute}}]
              {{m_scope}} sealed class {{m_objectShellName}} : {{m_interfaceName}}, IInterfaceAdapter
              {
                  private readonly object m_target{{TempVarMarker}};
                  private readonly global::go.GoShellBinding m_binding{{TempVarMarker}};

                  public {{m_objectShellName}}(object {{ReceiverLocal}}, global::go.GoShellBinding binding{{TempVarMarker}})
                  {
                      m_target{{TempVarMarker}} = {{ReceiverLocal}};
                      m_binding{{TempVarMarker}} = binding{{TempVarMarker}};
                  }

                  // The Go dynamic value, exactly as stored — the box when pointer-backed, the boxed value
                  // otherwise. See the delegate shell's remarks: the unwrap protocol is what keeps the
                  // shell invisible to equality, type switches, %T and re-asserts.
                  object? IInterfaceAdapter.Value => m_target{{TempVarMarker}};

                  // The dynamic value's Go text, as the delegate shell's ToString answers it.
                  public override string ToString() => m_target{{TempVarMarker}}?.ToString() ?? "<nil>";
          {{ObjectForwarders}}
              }
          """;

    private string ObjectForwarders
    {
        get
        {
            StringBuilder results = new();

            for (int index = 0; index < m_methods.Length; index++)
            {
                MethodInfo method = m_methods[index];
                string callParameters = Prefixed(method.GetCallParameters(false));
                string invocation = $"m_binding{TempVarMarker}.Invoke({index}, m_target{TempVarMarker}{callParameters})";
                string body = method.ReturnType == "void" ? $"{invocation};" : $"({method.ReturnType}){invocation}!;";

                results.Append(
                    $$"""


                              // '{{m_interfaceName}}.{{method.Name}}' forwarder
                              [global::System.Diagnostics.DebuggerNonUserCode]
                              public {{method.ReturnType}} {{method.GetSignature(false)}} => {{body}}
                      """);
            }

            return results.ToString();
        }
    }

    private static string Prefixed(string parameters)
    {
        return parameters.Length > 0 ? $", {parameters}" : parameters;
    }
}
