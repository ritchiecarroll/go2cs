// ConstraintProxyBindTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the module initializer that binds a FOREIGN interface's generic constraint proxy to one
/// element's Go methods, for a <c>GoImplement&lt;TElement, TInterface&gt;(ConstraintProxy = true)</c>
/// record whose interface another package declares.
/// </summary>
/// <remarks>
/// Every package that instantiates the constraint over the element records the pair, so the slots are
/// bound by whichever of them loads first, and again, to the same methods, by each that loads after.
/// A module initializer runs before any code of its assembly, so the binding precedes every value this
/// package can construct.
/// </remarks>
internal class ConstraintProxyBindTemplate : TemplateBase
{
    // Template Parameters
    public required string MethodName;      // e.g. "ᴛBindNodeжLesser"
    public required string ProxyRef;        // e.g. "global::go.example.lib_package.жLesser<global::go.example.elem_package.Node>"
    public required string Bindings;

    public override string TemplateBody =>
        $$"""
             // '{{ProxyRef}}' forwards through slots: bind them to the element's Go methods.
             [global::System.Runtime.CompilerServices.ModuleInitializer]
             internal static void {{MethodName}}()
             {
                 {{Bindings}}
             }
         """;
}
