// GoTwinForwarderAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks the lambda of an sstring twin's canonical value delegate (<c>Sprintfᶠ</c>, emitted by go2cs-gen's
/// StrGenerator; docs/phase4/DESIGN-sstring-twin-pilot.md) with the Go name of the function the
/// value stands for.
/// </summary>
/// <remarks>
/// <para>
/// The C# compiler emits a lambda as a generated method (<c>&lt;.cctor&gt;b__X_Y</c> on a nested
/// <c>&lt;&gt;c</c> class), and an attribute on the lambda attaches to that method, which is exactly what
/// <c>Delegate.Method</c> returns. <c>GoSyntheticPC.GoNameOf</c> and the runtime's frame naming read
/// <see cref="GoName"/> from it, which is what makes
/// <c>runtime.FuncForPC(reflect.ValueOf(fmt.Sprintf).Pointer()).Name()</c> answer <c>fmt.Sprintf</c>.
/// </para>
/// <para>
/// The lambda is go2cs machinery rather than Go code, so a traceback skips its frame. The twin's
/// <c>@string</c> forwarder needs no such mark: it carries <c>[GeneratedCode("go2cs-gen")]</c>, which the
/// runtime's frame filter already skips.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoTwinForwarderAttribute(string? goName = null) : Attribute
{
    /// <summary>The Go name of the function the delegate stands for, or <c>null</c> when the method's own name is it.</summary>
    public string? GoName { get; } = goName;
}
