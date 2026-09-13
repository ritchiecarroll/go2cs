// TestRegistry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using go.golib;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host, a structural replacement for Go's
// testing package rather than a conversion of it (the rationale and the measured clobber are in
// testing.cs). No converted source emits at this path, so this marker declares ownership rather than
// resolving a collision; the mechanical guards are the -stdlib skip list (isNonConvertedStdLibPackage)
// and testConversion.go's -tests refusal (requireConvertibleTestTarget).
[module: go.GoManualConversion]

namespace go.testing_runtime;
/// <summary>
/// One converted Go test function, as the generated host registered it.
/// </summary>
/// <param name="Name">Go test-function name (<c>TestFoo</c>), which is also what <c>-run</c> filters on.</param>
/// <param name="Action">The converted body, taking the <c>*testing.T</c> it runs against.</param>
/// <param name="Source">Source file the Go test was declared in.</param>
/// <param name="Line">Line the Go test was declared on.</param>
/// <remarks>
/// <see cref="Source"/> and <see cref="Line"/> are carried purely so a reported failure names the GO
/// declaration site rather than a C# one — the differential oracle compares this host's output
/// against <c>go test -json</c>, and a location naming generated C# has nothing to compare against.
/// </remarks>
public sealed record RegisteredTest(
    string Name,
    Action<ж<testing_package.T>> Action,
    string Source,
    int Line);

/// <summary>
/// The manifest of one converted package's test suite: its tests, the fixture files the run must
/// stage, and its <c>TestMain</c> if it declares one.
/// </summary>
/// <param name="package">Go import path of the package under test, as it appears in reported events.</param>
/// <param name="fixtures">
/// Paths, relative to the package directory, of the <c>testdata</c> files the suite reads.
/// </param>
/// <param name="fixtureDirectories">
/// Immediate subdirectory names of the package's own source directory, created EMPTY in the run
/// directory so its shape matches the one <c>go test</c> shows the package.
/// </param>
/// <remarks>
/// <para>
/// This is filled in by the converter-emitted test host at startup — one <see cref="Add"/> call per
/// discovered <c>_test.go</c> function — and then handed to <see cref="TestHost.Run"/>. Discovery
/// happens at CONVERSION time rather than by reflecting over the assembly, which is what keeps the
/// registered order and the Go declaration sites exact.
/// </para>
/// <para>
/// <see cref="Fixtures"/> exists because a Go test runs with its package directory as the working
/// directory and reads <c>testdata</c> by relative path. The host stages these into an isolated run
/// directory rather than pointing the process at the source tree, so a test that WRITES to testdata
/// cannot corrupt the repository and two packages cannot collide.
/// </para>
/// <para>
/// <see cref="FixtureDirectories"/> completes that picture for a test that asks what its working
/// directory CONTAINS rather than reading a known file: <c>go test</c> runs in the real source
/// directory, where the sibling packages nested under it are present, so os's <c>TestReadDir</c>
/// requires an <c>exec</c> subdirectory beside <c>read_test.go</c>. They are created empty and one
/// level deep — the names are what such a test observes, and a sibling package's files belong to
/// that package's own run. Defaulted so a test host generated before this list existed still
/// compiles; a regenerated one always passes it.
/// </para>
/// </remarks>
public sealed class TestRegistry(string package, IReadOnlyList<string> fixtures, IReadOnlyList<string>? fixtureDirectories = null, IReadOnlyList<string>? fixtureLinks = null)
{
    private readonly List<RegisteredTest> m_tests = [];

    public string Package { get; } = package;

    public IReadOnlyList<string> Fixtures { get; } = fixtures;

    public IReadOnlyList<string> FixtureDirectories { get; } = fixtureDirectories ?? [];

    /// <summary>
    /// Fixture directories staged as a LINK into the real Go source tree rather than as copies —
    /// the runnable-program trees whose sources a test hands to the real toolchain to compile.
    /// </summary>
    /// <remarks>
    /// These paths appear in NEITHER <see cref="Fixtures"/> nor the test project: their files are
    /// deliberately not copied, because the copy is the thing the toolchain refuses. The rule
    /// <c>cmd/go</c> applies is a directory-prefix test — a file compiled outside <c>$GOROOT/src</c>
    /// may not import <c>internal/…</c> — so the tree has to BE the real directory as far as the
    /// toolchain can tell. <see cref="PackageAncestry.StageFixtureLinks"/> creates one link each at
    /// sandbox construction, refuses to let anything write through one, and asks the toolchain
    /// whether it actually resolves the form it created. Defaulted so a test host generated before
    /// this list existed still compiles; a regenerated one always passes it.
    /// </remarks>
    public IReadOnlyList<string> FixtureLinks { get; } = fixtureLinks ?? [];

    public IReadOnlyList<RegisteredTest> Tests => m_tests;

    public Action<ж<testing_package.M>>? TestMain { get; private set; }

    public void Add(string name, Action<ж<testing_package.T>> action, string source, int line) =>
        m_tests.Add(new RegisteredTest(name, action, source, line));

    public void SetTestMain(Action<ж<testing_package.M>> testMain) => TestMain = testMain;
}
