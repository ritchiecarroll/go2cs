// GeneratorDiagnostics.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using Microsoft.CodeAnalysis;
using static go2cs.Symbols;

namespace go2cs;

/// <summary>
/// The go2cs-gen diagnostic REGISTRY: every <see cref="DiagnosticDescriptor"/> the generators and the analyzer
/// report is declared here and nowhere else, so one file lists every GO2CS0xxx code the C# build can raise.
/// </summary>
/// <remarks>
/// <para>
/// Codes are ranged by side: GO2CS0xxx is go2cs-gen's (a diagnostic in the C# build); GO2CS1xxx and GO2CS2xxx are
/// the converter's, declared in src/go2cs/diagnosticOutput.go; GO2CS3xxx is the build's, raised by go2cs's MSBuild
/// targets and declared in src/core/golib/buildTransitive/go.lib.targets. Every code has one row in docs/DiagnosticCodes.md,
/// and the repoguard test TestGo2csDiagnosticCodeRegistry fails by name on a code used but not registered here,
/// a code registered twice, a code outside 0001-0999, or a code without its documentation row.
/// </para>
/// <para>
/// A generator reports a record or declaration it cannot use in place of THROWING: a source generator that
/// throws contributes nothing, so every other record in the compilation went with the bad one. One record
/// costs its own output only. The severity is decided by what a skipped record can leave behind. A WARNING
/// where the omission can only fail later and loudly (a missing conversion operator does not compile at its
/// use site), or where nothing was owed (a record for an empty interface). An ERROR where the build could
/// otherwise go green with members missing at run time (a type's generated members, an adapter reached only
/// dynamically): the error keeps the build red, at the record, without erasing everything else.
/// </para>
/// <para>
/// RS2008 asks for analyzer release-tracking files; go2cs-gen ships with the converter and keeps none.
/// </para>
/// </remarks>
internal static class GeneratorDiagnostics
{
#pragma warning disable RS2008
    // The premise StructTypeTemplate.XpkgClassName's uniqueness rests on, checked where it could fail: a package
    // stem holding `ᴛ` makes the stem boundary ambiguous (stem `aᴛb` + struct `c` and stem `a` + struct `bᴛc`
    // name one class), and the collision would otherwise surface as a bare CS0101 on a generated class with
    // nothing pointing here. No Go package in the corpus has such a stem.
    public static readonly DiagnosticDescriptor XpkgStemMarker = new(
        id: "GO2CS0001",
        title: "Package class stem contains the lifted-name marker",
        messageFormat: "Package class stem '{0}' contains '" + TempVarMarker + "': the cross-package forwarder class '{1}' may collide with another struct's (CS0101)",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

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

    // StringLiteralBindingAnalyzer's one rule: a C# string literal in converted code that binds where Go's
    // string would not (the A1-A3 shapes in that analyzer's remarks).
    public static readonly DiagnosticDescriptor StringLiteralBindsWrongType = new(
        id: "GO2CS0004",
        title: "A C# string literal binds a type Go's string is not",
        messageFormat: "The C# string literal {0} binds {1}; converted code must pass a Go string (go.@string) here",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // LiteralInterceptorGenerator's refusal: a bare C# string literal reaches an object slot of a call the generator
    // cannot intercept, so the callee would see System.String. The converter emits a bare literal in an any slot only
    // where the call is interceptable; this names any other, with the reason.
    public static readonly DiagnosticDescriptor LiteralNotIntercepted = new(
        id: "GO2CS0005",
        title: "A C# string literal in an any slot cannot be intercepted",
        messageFormat: "The C# string literal {0} reaches the any slot of '{1}', and the call cannot be intercepted: {2}",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // LiteralInterceptorGenerator's binding guard: Go has one callee per call, but a C# method group can hold another
    // member whose parameter captures a bare literal (it prefers go.@string to object), so the call would no longer
    // reach the Go function's any slot.
    public static readonly DiagnosticDescriptor LiteralOverloadDrift = new(
        id: "GO2CS0006",
        title: "An overload captures a C# string literal meant for an any slot",
        messageFormat: "The C# string literal {0} binds '{1}', while '{2}' takes it in an any slot; an overload beside the converted callee has captured the call",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ImplicitConvGenerator's refusal of a record pairing a type with its own box, <T, ж<T>> in either order. Go has
    // no implicit conversion between a value and a pointer, so such a record means the converter put a value where
    // Go passes a pointer. The operator it used to generate boxed a COPY: the site compiled, and a write through
    // the box never reached the original (runtime's runqput stored a copy of the caller's g in runnext).
    public static readonly DiagnosticDescriptor SelfBoxConversion = new(
        id: "GO2CS0008",
        title: "An implicit conversion record pairs a type with its own box",
        messageFormat: "{0} pairs '{1}' with its own box '{2}': Go never converts between a value and a pointer implicitly, so the converter placed a value where Go passes a pointer; no operator is generated, because it would box a copy",
        category: "go2cs-gen",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
#pragma warning restore RS2008
}
