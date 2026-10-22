// DeferredRegistration.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go2cs;

/// <summary>
/// The module initializer of a generated registration that golib runs on its registry's first miss
/// (golib's GoDeferredRegistrations): the initializer only queues the registration method, so a program whose
/// lookups never miss compiles none of it at startup, while the delegate keeps the method and every instantiation it
/// names compiled under Native AOT. Used by every registry whose only reader is a lookup: the native array openers,
/// the escaping containers' operations, the Go zero factories and the pointer adapters.
/// </summary>
public static class DeferredRegistration
{
    /// <summary>
    /// The module initializer <c>ᴛDefer{name}</c>, indented as a member of the package class, that queues the same
    /// class's parameterless <c>ᴛRegister{name}</c> on golib's <paramref name="registry"/> queue.
    /// </summary>
    public static string Initializer(string registry, string name) =>
        "    [global::System.Runtime.CompilerServices.ModuleInitializer]\r\n" +
        $"    internal static void {TempVarMarker}Defer{name}() =>\r\n" +
        $"        global::go.GoDeferredRegistrations.{registry}.Defer({TempVarMarker}Register{name});\r\n\r\n";

    private const string TempVarMarker = "ᴛ";
}
