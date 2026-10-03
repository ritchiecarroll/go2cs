// GeneratorDiagnostics.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.CodeAnalysis;

namespace go2cs;

/// <summary>
/// The diagnostics a go2cs-gen generator reports for one record or declaration it cannot use, in
/// place of THROWING: a source generator that throws contributes nothing, so every other record in
/// the compilation went with the bad one. One record costs its own output only.
/// </summary>
/// <remarks>
/// The severity is decided by what a skipped record can leave behind. A WARNING where the omission can
/// only fail later and loudly (a missing conversion operator does not compile at its use site), or
/// where nothing was owed (a record for an empty interface). An ERROR where the build could otherwise
/// go green with members missing at run time (a type's generated members, an adapter reached only
/// dynamically): the error keeps the build red, at the record, without erasing everything else.
/// RS2008 asks for analyzer release-tracking files; go2cs-gen ships with the converter and keeps none.
/// </remarks>
internal static class GeneratorDiagnostics
{
#pragma warning disable RS2008
    public static readonly DiagnosticDescriptor MalformedRecord = new(
        id: "GO2CS0002",
        title: "Unusable go2cs-gen record",
        messageFormat: "Skipped {0}: {1}; nothing is generated for it",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UngeneratableRecord = new(
        id: "GO2CS0003",
        title: "go2cs-gen record cannot be generated",
        messageFormat: "{0} cannot be generated: {1}",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
#pragma warning restore RS2008
}
