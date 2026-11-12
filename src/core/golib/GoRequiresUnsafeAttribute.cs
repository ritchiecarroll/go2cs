// GoRequiresUnsafeAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Declare that a hand-owned file needs the C# <c>/unsafe</c> compiler option.
/// </summary>
/// <remarks>
/// <para>
/// The converter regenerates each package's <c>.csproj</c> on every transpile and writes
/// <c>&lt;AllowUnsafeBlocks&gt;</c> from its own <c>usesUnsafeCode</c> predicate — which sees only the
/// code the converter itself emitted. A hand-owned file ([module: <see cref="GoManualConversionAttribute"/>]
/// or an <c>*_impl.cs</c> companion) is invisible to that predicate, so a package whose ONLY need for
/// <c>/unsafe</c> comes from hand-written code could not compile: setting the property by hand is undone
/// by the next reconvert overlay.
/// </para>
/// <para>
/// Applying this attribute states the requirement in the file that HAS it. The converter scans the
/// package's output directory for the marker and unions it into the emitted
/// <c>&lt;AllowUnsafeBlocks&gt;</c>, so the declaration survives every regeneration. The property is
/// permissive — it grants a capability rather than using one — so a union is inert for a platform whose
/// emission contains no unsafe code, and no IL moves.
/// </para>
/// <para>
/// Note that the module-level scope is used as a pseudo file-level marker for this attribute, exactly as
/// <see cref="GoManualConversionAttribute"/> uses it: the converter detects the attribute by scanning the
/// source file directly, so multiple instances across the files of one module are both legal and
/// meaningful, even though assembly metadata will serialize only one.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Module, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)] public class GoRequiresUnsafeAttribute : Attribute
{
}
