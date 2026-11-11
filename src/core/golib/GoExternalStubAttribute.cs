// GoExternalStubAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a method body that <c>go2cs-gen</c>'s <c>PartialStubGenerator</c> supplied for a bodyless
/// <c>partial</c> declaration — a Go function that NOTHING IN THAT COMPILATION implements. It may be
/// assembly or cgo, a darwin dylib trampoline, or a linkname whose push did not arrive; this
/// marker does not distinguish them, and the generator cannot -- it sees one compilation. What it
/// records is the equivalence below, not a cause.
/// </summary>
/// <remarks>
/// <para>
/// The marker exists because "has no managed implementation" is not otherwise decidable at runtime,
/// and one caller must decide it: <c>internal/abi</c>'s <c>FuncPCABI0</c>/<c>FuncPCABIInternal</c>
/// are handed a method group and must answer either a synthetic PC (the function exists and may be
/// symbolized) or a loud failure (it does not exist and no number is honest). See
/// <c>docs/phase4/DESIGN-synthetic-pc-registry.md</c>.
/// </para>
/// <para>
/// It is stamped by the stub generator rather than inferred, because the two available proxies are
/// both measurably wrong. A bodyless <c>partial</c> DECLARATION covers assembly routines and
/// darwin's dylib trampolines alike (design §5, classes B and C), so it does not identify either.
/// And <c>[GeneratedCode]</c> over-matches: <c>Common.cs</c> mints one tool string for every
/// generator in this analyzer, so <c>RecvGenerator</c>'s ж-overloads carry it too — and
/// <c>runtime/time.cs</c> passes exactly one of those (<c>(*timers).run</c>, a real function with a
/// real body) to <c>FuncPCABIInternal</c>. A discriminator with a known false positive is not a
/// discriminator.
/// </para>
/// <para>
/// The stub generator is the precise oracle by construction: it already declines to stub a partial
/// another generator is obliged to implement (<c>[LibraryImport]</c>) and one a hand-written
/// <c>*_impl.cs</c> companion supplies, so "it stubbed this method" holds exactly when nothing in
/// the compilation implements it. That equivalence is what this attribute records, and it stays
/// true for every bodyless partial added after today without anyone maintaining a list.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoExternalStubAttribute : Attribute
{
}
