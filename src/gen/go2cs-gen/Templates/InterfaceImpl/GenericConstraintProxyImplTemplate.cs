// GenericConstraintProxyImplTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the GENERIC self-referential constraint proxy a package publishes for each constraint
/// interface it declares: <c>жLesser&lt;E&gt; : Lesser&lt;жLesser&lt;E&gt;&gt;</c>.
/// </summary>
/// <remarks>
/// <para>
/// The proxy is the TYPE ARGUMENT of the constrained instantiation, so it is part of the type's
/// identity: <c>lib.Ord2[*elem.Node]</c> is one C# type only if every package that names it names one
/// proxy. A package instantiating a FOREIGN interface over a FOREIGN element can never declare that
/// proxy itself (a second importer would declare another), and the interface's package cannot
/// declare a concrete one for an element it does not import. This proxy, published by the interface's
/// package and closed over the element, is the one name every importer can reach.
/// </para>
/// <para>
/// It cannot forward by itself: the element's Go methods are extension methods over
/// <c>ж&lt;Node&gt;</c>, and an extension method does not bind through a type parameter. So each
/// forwarder calls a static slot, bound to the element's methods by a module initializer in every
/// package that instantiates the constraint (<c>ConstraintProxyBindTemplate</c>). A module initializer
/// runs before any code of its assembly, so no value of the closed type exists unbound; a slot no
/// package bound panics naming the element, the interface and the method.
/// </para>
/// </remarks>
internal class GenericConstraintProxyImplTemplate : TemplateBase
{
    // Template Parameters
    public required string ProxyName;       // e.g. "жLesser"
    public required string InterfaceRef;    // e.g. "global::go.example.lib_package.Lesser<жLesser<E>>"
    public required string InterfaceName;   // e.g. "Lesser", as the unbound panic spells it
    public required string AdapterScope;
    public required string SlotDeclarations;
    public required string MethodsImplementation;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Generic self-referential constraint proxy: the pointer to any element 'E' satisfies
             /// '{{InterfaceName}}' by wrapping the box 'ж&lt;E&gt;' and implementing the interface over ITSELF,
             /// forwarding each method through a slot that instantiating packages bind.
             /// </summary>
             [global::System.Diagnostics.DebuggerNonUserCode]
             {{AdapterScope}} sealed class {{ProxyName}}<E> : {{InterfaceRef}}, IжAdapter
             {
                 private readonly ж<E> m_box;

                 // The dependency registers this proxy for trimming (golib's GoTypeRegistry) whenever it is constructed.
                 {{string.Format(AdapterRegistration, $"{ProxyName}<>")}}
                 public {{ProxyName}}(ж<E> box) => m_box = box;

                 public object? Box => m_box;

                 // Implicit conversions marshal every self-typed (T) boundary automatically, exactly as
                 // a concrete proxy's do.
                 public static implicit operator {{ProxyName}}<E>(ж<E> box) => new {{ProxyName}}<E>(box);

                 public static implicit operator ж<E>({{ProxyName}}<E> proxy) => proxy.m_box;

                 // E's Go methods, one slot per interface method, bound by the module initializer of each
                 // package that instantiates the constraint over E. Every binder binds the same methods.
                 {{SlotDeclarations}}

                 {{MethodsImplementation}}

                 private static global::System.Exception {{TempVarMarker}}Unbound(string method) =>
                     global::go.builtin.panic($"go2cs: constraint proxy {{ProxyName}}<{typeof(E).FullName}>: no package instantiating {{InterfaceName}}[*{typeof(E).Name}] bound method {method}");

                 // Go pointer-interface equality is pointer identity: proxies over the same box compare equal.
                 public override bool Equals(object? obj) => obj switch
                 {
                     IжAdapter adapter => ReferenceEquals(adapter.Box, m_box),
                     ж<E> box => ReferenceEquals(box, m_box),
                     _ => ReferenceEquals(obj, this)
                 };

                 public override int GetHashCode() => m_box?.GetHashCode() ?? 0;

                 public override string? ToString() => m_box?.ToString();
             }
         """;
}
