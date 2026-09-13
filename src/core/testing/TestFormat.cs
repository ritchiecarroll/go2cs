// TestFormat.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host, a structural replacement for Go's
// testing package rather than a conversion of it (the rationale and the measured clobber are in
// testing.cs). No converted source emits at this path, so this marker declares ownership rather than
// resolving a collision; the mechanical guards are the -stdlib skip list (isNonConvertedStdLibPackage)
// and testConversion.go's -tests refusal (requireConvertibleTestTarget).
[module: go.GoManualConversion]

namespace go.testing_runtime;

/// <summary>
/// Go-style formatting for the testing host, delegated to the converted <c>fmt</c> package.
/// </summary>
/// <remarks>
/// <para>
/// The host formats through the SAME package Go's own <c>testing</c> formats through:
/// <c>t.Log(args...)</c> is <c>fmt.Sprintln(args...)</c> and <c>t.Logf</c> is <c>fmt.Sprintf</c>,
/// so this file is a two-method seam and no longer a second implementation of Go's verbs.
/// </para>
/// <para>
/// HISTORY, kept because the reasons were good and it is the measurement that retired them, not a
/// change of mind. Until 2026-09-03 this file carried a self-contained formatter and the remark
/// said the testing runtime "must stay fmt-free", for two stated reasons: (1) testing is a FIXED
/// reference of every converted test project, so fmt would sit underneath every suite — including
/// fmt's own, where the host would be reporting on a package it is itself running on; and (2) any
/// suite that hand-owns or stubs part of the fmt closure would drag a second copy into one build.
/// Both were measured rather than argued before this changed:
/// </para>
/// <list type="bullet">
///   <item>Reason (1) on its own sharpest case — fmt's OWN test host under this reference converts,
///   builds with zero errors and RUNS 63 pass / 1 skip / 0 fail. The self-reporting case works.</item>
///   <item>The project graph stays acyclic on all three GOOS targets (asserted by
///   <c>check-solution-integrity.ps1</c>, whose injection mechanism was positive-controlled on the
///   known W1 edge at exactly six cycles), because fmt's closure does not contain testing.</item>
///   <item>Reason (2) is UNTESTED rather than disproven: no suite that hand-owns or stubs part of
///   the fmt closure was identified. If one appears, it meets this reference first.</item>
///   <item>The cost is real and was accepted deliberately: testing's closure grows 38 → 58 projects
///   and a cold test-host build grows 9s → 19s (one axis, same row).</item>
/// </list>
/// <para>
/// What the shim could not do is why this changed. It parsed the <c>#</c> flag and then DROPPED it
/// for <c>%v</c>, so <c>%#v</c> rendered a per-object hash instead of Go-syntax fields — which is
/// how net's <c>TestUnixConnLocalAndRemoteNames</c> reported a genuine address divergence as two
/// hex words and sent that arc to the reflect bridge for weeks. Through delegation the same
/// <c>Fatalf</c> prints
/// <c>got &amp;net.UnixAddr{Name:"", Net:"unix"}, expected &amp;net.UnixAddr{Name:"@", Net:"unix"}</c>,
/// which names its own cause. Delegating WHOLESALE rather than per verb is deliberate: a shim that
/// covers "the common verbs" is a list someone must keep, and every gap in it is a diagnostic that
/// misleads at exactly the moment a suite is failing.
/// </para>
/// </remarks>
internal static class TestFormat
{
    /// <summary>
    /// Formats arguments as Go's <c>t.Log</c> does — <c>fmt.Sprintln</c> semantics (spaces between
    /// all operands) — without the trailing newline this class's callers have never wanted.
    /// </summary>
    public static string Sprint(ReadOnlySpan<object> args)
    {
        string text = fmt_package.Sprintln(args.ToArray()).ToString();

        // Sprintln appends exactly one newline; remove exactly that one, never a newline the
        // caller's own last operand ended with.
        return text.Length > 0 && text[^1] == '\n' ? text[..^1] : text;
    }

    /// <summary>
    /// Formats a Go format string as Go's <c>t.Logf</c> does — <c>fmt.Sprintf</c>, every verb and
    /// flag the converted fmt implements, including the <c>%#v</c> Go-syntax form and fmt's own
    /// disclosure styles for bad verbs and missing or extra arguments.
    /// </summary>
    public static string Sprintf(string format, ReadOnlySpan<object> args) =>
        fmt_package.Sprintf(format, args.ToArray()).ToString();
}
