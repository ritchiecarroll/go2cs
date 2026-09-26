// PackageAncestry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

// go2cs HAND-OWNED (whole file) — part of the Phase-4 test host, a structural replacement for Go's
// testing package rather than a conversion of it (the rationale and the measured clobber are in
// testing.cs). No converted source emits at this path, so this marker declares ownership rather than
// resolving a collision; the mechanical guards are the -stdlib skip list (isNonConvertedStdLibPackage)
// and testConversion.go's -tests refusal (requireConvertibleTestTarget).
[module: go.GoManualConversion]

namespace go.testing_runtime;

/// <summary>
/// Reconstructs, inside the run sandbox, the directory ANCESTRY <c>go test</c> gives a package, so
/// that a test resolving a path relative to its working directory reaches the same content Go's own
/// run does.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TestHost"/> already reproduced the ancestry's SHAPE — the working directory mirrors the
/// package's whole import path, so its own base name and its parents are named as Go names them. What
/// was missing is CONTENT: the parents were empty, so every cwd-relative read that leaves the package
/// directory failed on layout rather than on behavior. Six packages sat behind exactly that, each
/// reaching for something real one or more levels up — <c>io/ioutil</c> lists <c>..</c> for the
/// sibling <c>io</c> package's own files, <c>go/parser</c> reads <c>../printer/nodes.go</c> in its
/// package initializer, <c>internal/godebugs</c> reads <c>../../../doc/godebug.md</c>,
/// <c>internal/testenv</c> stats <c>../../../bin/go</c>, and <c>internal/coverage/cfile</c> needs a
/// <c>src/go.mod</c> above it for the toolchain's module walk to terminate.
/// </para>
/// <para>
/// <b>This view is an ANCESTRY, deliberately not a GOROOT.</b> GOROOT keeps pointing at the real Go
/// installation, and that distinction is the whole design rather than an omission. Reads THROUGH a
/// junction resolve to real content, but a directory WALK does not descend into one: Go reports a
/// junction from <c>Lstat</c> as an irregular file, so <c>filepath.WalkDir</c> steps over it. Measured
/// against Go 1.23.1 on a junction-mirrored root, a walk counting <c>*.gz</c> under GOROOT finds 0
/// where the real tree has 4, and a walk of <c>src/unicode</c> reports 1 entry against the real 19.
/// Two already-validated packages walk GOROOT that way (<c>compress/gzip</c>'s issue14937 test and
/// <c>path/filepath</c>), so repointing GOROOT at this view would REGRESS them. Leaving GOROOT real
/// costs nothing here, because every member of the class resolves against its working directory, and
/// a read through a junction is faithful. The one shape this cannot serve is a test that requires cwd
/// to sit under the GOROOT the process REPORTS — <c>go/build</c>'s <c>ImportDir(cwd)</c> — which is
/// why that package is censused rather than closed.
/// </para>
/// <para>
/// Directories are linked (a junction on Windows, a symlink elsewhere) and files are hard-linked, so
/// staging is a metadata operation rather than a copy: GOROOT's top level alone carries an 81 MB
/// installer archive that a per-run copy would multiply by every package in a sweep. The PACKAGE's own
/// directory is the exception and is populated with real copies, because it is the one directory a
/// test legitimately writes to.
/// </para>
/// <para>
/// Staging is best-effort by construction. A tree with no usable GOROOT — a clone with no Go
/// installation, a platform that refuses the link — leaves the sandbox exactly as it was before this
/// type existed, which is a working run for every package that does not read above itself.
/// </para>
/// </remarks>
internal static class PackageAncestry
{
    /// <summary>
    /// Stages the package's ancestry under <paramref name="runRoot"/>, mirroring GOROOT from its top
    /// level down to (but not including) the package's own directory.
    /// </summary>
    /// <returns>true when the ancestry was staged; false when it was skipped and the sandbox is
    /// unchanged.</returns>
    public static bool TryStage(string? goRoot, string importPath, string runRoot, string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(goRoot))
            return false;

        string goRootSrc = Path.Combine(goRoot, "src");

        // A GOROOT without a source tree is not one this view can mirror. Checked rather than
        // assumed: GOROOT is an environment variable, so it can name anything at all.
        if (!Directory.Exists(goRootSrc))
            return false;

        string[] segments = importPath.TrimEnd('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
            return false;

        // The real package directory has to exist for the ancestry to mean anything — a converted
        // package whose Go sources are not in this GOROOT gets the unchanged sandbox.
        if (!Directory.Exists(Path.Combine(goRootSrc, Path.Combine(segments))))
            return false;

        try
        {
            ReclaimAbandonedSandboxes(runRoot);
            MarkOwner(runRoot);

            // GOROOT's own top level, carving out `src` for the descent.
            MirrorLevel(goRoot, runRoot, "src");

            // `src` is a level of the mirror exactly as it is a level of GOROOT: it is what makes
            // ../../.. from internal/godebugs land on the root rather than one short of it, and it is
            // where the toolchain's module walk finds `module std`. The last level is the package's
            // own directory, which is the working directory and is populated separately.
            string[] levels = ["src", .. segments];
            string realLevel = goRoot;
            string mirrorLevel = runRoot;

            for (int i = 0; i < levels.Length - 1; i++)
            {
                realLevel = Path.Combine(realLevel, levels[i]);
                mirrorLevel = Path.Combine(mirrorLevel, levels[i]);
                MirrorLevel(realLevel, mirrorLevel, levels[i + 1]);
            }

            // The package's own directory: real copies of its own files, and NO links for its
            // subdirectories. Those are the fixture staging's business — it creates the named ones and
            // fills testdata from the digest-tracked build output — and a junction there would put a
            // test's writes inside the real GOROOT.
            CopyOwnFiles(Path.Combine(goRootSrc, Path.Combine(segments)), workingDirectory);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            // A partially staged ancestry is still a superset of the empty one, so the run continues.
            return false;
        }
    }

    /// <summary>
    /// Guarantees every component of <paramref name="directory"/> below <paramref name="runRoot"/> is
    /// a real directory, replacing any link this view staged with an empty one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The fixture staging writes into ancestor-relative paths — compress/{flate,zlib,lzw} all read
    /// <c>../testdata/</c> — and those ancestors now hold links to the real GOROOT. Writing through
    /// one would put staged fixtures INSIDE the Go installation. Converting the component to an empty
    /// real directory first is what makes the sandbox a sandbox; it also restores exactly the
    /// pre-ancestry contract for those paths, since before this view they were empty too.
    /// </para>
    /// <para>
    /// <b>A LINK-STAGED FIXTURE TREE IS EXEMPT, LOUDLY.</b> That unlink-and-recreate is safe for an
    /// ancestry link precisely because an ancestry link's content is owed to nobody — the sandbox
    /// wants those directories empty. It is CONTENT LOSS for a fixture link, whose whole purpose is
    /// to present the real fixture tree: the delete would leave an EMPTY real directory in its place
    /// and every reader of the tree would then fail on a bare file-not-found, with nothing in the
    /// harness attributing it. Silently absorbing that is the same shape as the vanished-fixture
    /// regression the <c>staged == 0</c> check in TestHost.CopyFixtures exists for, so it gets the
    /// same answer: refuse, and name the path.
    /// </para>
    /// </remarks>
    public static void EnsureWritable(string directory, string runRoot)
    {
        string full = Path.GetFullPath(directory);
        string root = Path.GetFullPath(runRoot);

        RefuseWriteIntoFixtureLink(full);

        // Outside the sandbox there is nothing of this view's to unlink, but the directory is still
        // owed to the caller — every caller is about to write into it.
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(full);
            return;
        }

        // Walk root -> leaf so an outer link is replaced before an inner component is examined
        // through it.
        foreach (string component in ComponentsBetween(root, full))
        {
            if (IsLink(component))
            {
                new DirectoryInfo(component).Delete();
                Directory.CreateDirectory(component);
            }
        }

        Directory.CreateDirectory(full);
    }

    /// <summary>
    /// Stages the RUNNABLE-PROGRAM fixture trees as links into the real GOROOT directory, so the Go
    /// toolchain accepts the <c>internal/…</c> imports of the sources a test hands it to compile.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>cmd/go/internal/load.disallowInternal</c> permits an <c>internal/…</c> import for a
    /// standard-library package on ONE test: the directory holding the file being compiled must sit
    /// under <c>$GOROOT/src</c>, compared plainly and then again through <c>expandPath</c>
    /// (<c>filepath.EvalSymlinks</c>). Under <c>go test</c> that directory IS the real fixture
    /// directory; under this host it is the sandbox, so internal/trace's
    /// <c>go run ./testdata/testprog/cpu-profile.go</c> is refused its <c>internal/profile</c>
    /// import. Presenting the directory as a LINK to the real one closes that, and does it without
    /// touching GOROOT, which is what keeps the 2026-08-13 walk-equivalence finding — and the banked
    /// rows that depend on it — undisturbed.
    /// </para>
    /// <para>
    /// <b>Which directories</b> is decided at CONVERSION time, not here: the converter's predicate is
    /// a <c>testdata</c> subdirectory holding at least one <c>.go</c> file in which every <c>.go</c>
    /// file declares <c>package main</c>, and it emits the selected paths into the test host. Their
    /// files are in neither the csproj nor the fixture list, so this is the only staging they get and
    /// a half-done job is not survivable — every failure below throws.
    /// </para>
    /// <para>
    /// <b>Symlink first, junction as the fallback.</b> Measured on Go 1.23.12 / Windows, both forms
    /// are accepted where a plain copy is refused, but only the symlink is ATTRIBUTABLE:
    /// <c>filepath.EvalSymlinks</c> resolves it to the GOROOT path, which is exactly what
    /// <c>expandPath</c> does inside disallowInternal, while a junction reads back as
    /// <c>ModeIrregular</c> and <c>EvalSymlinks</c> returns it unchanged — so the junction's
    /// acceptance is a measured fact whose mechanism inside cmd/go is not pinned. Building on the
    /// explainable form and keeping the unexplained one for the machine that refuses
    /// <c>SeCreateSymbolicLinkPrivilege</c> is what that asymmetry buys.
    /// </para>
    /// <para>
    /// <b>And neither form is TRUSTED — the toolchain is asked.</b> A link that the filesystem
    /// creates happily but cmd/go does not resolve would produce exactly the failure this staging
    /// exists to remove, one layer deeper and with no attribution. So the first link is probed with a
    /// real <c>go list</c> through it, and the answer that counts is whether cmd/go reports the
    /// directory as a path under <c>$GOROOT/src</c> — the property disallowInternal actually tests,
    /// rather than a proxy for it. A rejected symlink is rebuilt as a junction and re-probed; if
    /// neither form survives, the run fails here rather than in a test.
    /// </para>
    /// </remarks>
    public static void StageFixtureLinks(IReadOnlyList<string> links, string? goRoot, string importPath, string workingDirectory, string runRoot)
    {
        if (links.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(goRoot))
        {
            throw new InvalidOperationException(
                $"this suite stages {links.Count} fixture tree(s) as links into the real Go source tree " +
                "(they hold programs the test compiles with the real toolchain, which refuses their " +
                "internal/… imports from anywhere outside $GOROOT/src), and GOROOT is not set. The " +
                "trees are in neither the project's copied fixtures nor the build output, so there is " +
                "nothing to fall back to: set GOROOT for this run.");
        }

        string realPackageDirectory = Path.Combine(goRoot, "src", importPath.Replace('/', Path.DirectorySeparatorChar));

        if (!Directory.Exists(realPackageDirectory))
        {
            throw new InvalidOperationException(
                $"fixture link staging needs the package's real source directory and '{realPackageDirectory}' " +
                $"does not exist — GOROOT ('{goRoot}') does not hold the sources for '{importPath}'.");
        }

        string root = Path.GetFullPath(runRoot);
        List<(string Target, string Real)> staged = [];
        bool symbolicLinks = true;

        foreach (string relativePath in links)
        {
            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
            string target = Path.GetFullPath(Path.Combine(workingDirectory, normalized));
            string real = Path.GetFullPath(Path.Combine(realPackageDirectory, normalized));

            // The converter enumerates these from the package's own tree, so an escaping path is a
            // defect rather than a configuration; both ends are checked, because the two roots fail
            // for different reasons.
            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"fixture link escapes run root: {relativePath}");

            if (!real.StartsWith(Path.GetFullPath(realPackageDirectory), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"fixture link escapes the package's source directory: {relativePath}");

            if (!Directory.Exists(real))
                throw new InvalidOperationException($"fixture link target does not exist: {real}");

            // The PARENT has to be a real directory the link can be planted in — for
            // `testdata/testprog` that is the `testdata` the run-directory shape pass just created,
            // and for a link at `testdata` itself it is the working directory. Protection is
            // registered only afterwards, so this call cannot trip over a link of its own making.
            string parent = Path.GetDirectoryName(target)!;
            EnsureWritable(parent, runRoot);

            RemoveExisting(target);
            CreateFixtureLink(target, real, out bool isSymbolicLink);
            ProtectFixtureLink(target);

            staged.Add((target, real));
            symbolicLinks &= isSymbolicLink;
        }

        // The JUNCTION FALLBACK, and only it: at Go 1.24 a junction-staged tree is refused without
        // this and the symbolic-link form needs nothing. Set BEFORE the probe below, because the
        // probe is the first thing that runs the toolchain through these links. The ANSWER is carried
        // forward rather than recomputed: a run whose own environment names winsymlink gets no setting
        // from this host, and the refusal below must not claim otherwise.
        bool junctionGodebug = !symbolicLinks && ApplyJunctionGodebug(goRoot);

        AssertToolchainAcceptsLinks(goRoot, staged, symbolicLinks, junctionGodebug);

        static void RemoveExisting(string target)
        {
            if (!Directory.Exists(target))
                return;

            // A link is unlinked, never traversed. A real directory here is the empty one the
            // run-directory shape pass created; anything else means two stagings disagree about the
            // same path, and taking the recursive delete on trust is how content quietly disappears.
            if (IsLink(target))
            {
                new DirectoryInfo(target).Delete();
                return;
            }

            if (new DirectoryInfo(target).EnumerateFileSystemInfos().GetEnumerator().MoveNext())
                throw new InvalidOperationException($"fixture link target '{target}' already holds staged content; refusing to replace it with a link");

            Directory.Delete(target);
        }
    }

    /// <summary>
    /// Forgets this run's fixture links, so the write refusal cannot outlive the sandbox it guards.
    /// </summary>
    public static void ReleaseFixtureLinks()
    {
        lock (s_fixtureLinks)
            s_fixtureLinks.Clear();
    }

    // The fixture links staged for THIS run. Absolute, full paths; a write at or below one is
    // refused. Process-wide because one host run executes per test process, which is Go's model.
    private static readonly HashSet<string> s_fixtureLinks = new(StringComparer.OrdinalIgnoreCase);

    private static void ProtectFixtureLink(string target)
    {
        lock (s_fixtureLinks)
            s_fixtureLinks.Add(Path.GetFullPath(target));
    }

    private static void RefuseWriteIntoFixtureLink(string full)
    {
        lock (s_fixtureLinks)
        {
            if (s_fixtureLinks.Count == 0)
                return;

            foreach (string link in s_fixtureLinks)
            {
                if (!full.Equals(link, StringComparison.OrdinalIgnoreCase) &&
                    !full.StartsWith(link + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"refusing to make '{full}' writable: it is inside the fixture tree link-staged at " +
                    $"'{link}', which points at the real Go source tree. Making it writable would unlink " +
                    "the tree and leave an EMPTY directory in its place — every reader of those fixtures " +
                    "would then fail on a bare file-not-found. A link-staged fixture tree is read-only by " +
                    "construction; if a test needs to write here, that tree must not be link-staged.");
            }
        }
    }

    // Creates the fixture link, preferring the ATTRIBUTABLE form. A Windows directory symlink needs
    // SeCreateSymbolicLinkPrivilege (administrator, or Developer Mode) which no test run may assume,
    // so a refusal falls back to the junction — unprivileged, and measured to be accepted by the
    // toolchain just the same. Elsewhere a symlink is unprivileged and there is no second form.
    private static void CreateFixtureLink(string link, string target, out bool isSymbolicLink)
    {
        try
        {
            Directory.CreateSymbolicLink(link, target);
            isSymbolicLink = true;
            return;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or PlatformNotSupportedException)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException(
                    $"could not create the fixture link '{link}' -> '{target}': {ex.Message}", ex);
            }
        }

        // Directory.CreateSymbolicLink may have left the failed link behind.
        if (Directory.Exists(link) && IsLink(link))
            new DirectoryInfo(link).Delete();

        CreateJunction(link, target);
        isSymbolicLink = false;
    }

    /// <summary>
    /// Asks the TOOLCHAIN whether the links it just created actually buy what they were created for,
    /// and rebuilds them as junctions if the symlink form does not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The probe asks the question disallowInternal answers, and it took a wrong one to find it.</b>
    /// The obvious probe — <c>go list -e -f {{.Dir}}</c> on the linked DIRECTORY, and see whether
    /// cmd/go reports a path under <c>$GOROOT/src</c> — is right in a bare scratch directory and
    /// WRONG in the sandbox, because the sandbox has a module: the ancestry view stages GOROOT's
    /// <c>src/go.mod</c> (<c>module std</c>) above the package, so cmd/go resolves the directory
    /// against THAT module and reports the sandbox path for a copy, a symlink and a junction alike.
    /// Measured 2026-08-29: the directory form cannot tell the three apart inside a module, and the
    /// first implementation of this assertion consequently failed a run whose links were working.
    /// </para>
    /// <para>
    /// The FILE form does tell them apart, because it is the form the tests themselves use
    /// (<c>go run ./testdata/testprog/x.go</c>) and it is what reaches <c>disallowInternal</c>. On
    /// one of Go's own fixtures, from a module-rooted sandbox:
    /// </para>
    /// <code>
    ///   staged shape   go list -e -f '{{.Error}}' &lt;file&gt;                 go build
    ///   copy           use of internal package internal/profile not allowed   REFUSED
    ///   symlink        (none)                                                 built
    ///   junction       (none)                                                 built
    /// </code>
    /// <para>
    /// So the probe names one <c>.go</c> file per link-staged tree THROUGH the link and looks for the
    /// refusal. It is a matched control, not a proxy — the copy row is the failure this staging
    /// exists to remove, and it is the row the probe reports. A tree whose sources import nothing
    /// internal reports nothing either way, which is the right answer: there was no refusal to close.
    /// </para>
    /// </remarks>
    private static void AssertToolchainAcceptsLinks(string goRoot, List<(string Target, string Real)> staged, bool symbolicLinks, bool junctionGodebug)
    {
        string goTool = Path.Combine(goRoot, "bin", OperatingSystem.IsWindows() ? "go.exe" : "go");

        if (!File.Exists(goTool))
        {
            // No toolchain to ask. Every test that would compile through these links skips itself for
            // the same reason (testenv.MustHaveGoBuild/MustHaveGoRun), so failing here would convert
            // Go's own skip into a hard error. Said out loud, because an unprobed link is a weaker
            // claim than a probed one and the difference should not be invisible.
            Console.Error.WriteLine($"testing: fixture links staged unprobed — no Go toolchain at '{goTool}'");
            return;
        }

        string? refusal = FirstRefusal(goTool, staged);

        if (refusal is null)
            return;

        if (symbolicLinks && OperatingSystem.IsWindows())
        {
            // The symlink is the ATTRIBUTABLE form, not the only accepted one. A filesystem or policy
            // that creates one the toolchain will not follow still has the junction, so try it before
            // giving up — all of them, since one form per run is the only coherent state to be in.
            foreach ((string target, string real) in staged)
            {
                if (Directory.Exists(target) && IsLink(target))
                    new DirectoryInfo(target).Delete();

                CreateJunction(target, real);
            }

            // Junctions now, so this run is on the fallback path after all and owes it the same
            // GODEBUG the fallback sets — before the re-probe, and before any fixture program runs.
            junctionGodebug = ApplyJunctionGodebug(goRoot);

            refusal = FirstRefusal(goTool, staged);

            if (refusal is null)
                return;
        }

        // Naming the PRIVILEGE, because that is the one thing the operator can act on: every machine
        // that reaches the junction path at all reached it by being refused a symbolic link.
        // The symbolic-link arm is worded for EVERY platform, not just Windows: the rebuild block
        // above is gated on OperatingSystem.IsWindows(), and off Windows CreateFixtureLink re-throws
        // instead of falling back, so no junction is ever built there and "rebuilt as junctions just
        // above" was simply untrue on the platforms that reach this arm without one.
        string privilege = symbolicLinks
            ? "This machine DID create the symbolic links — on Windows they were rebuilt as junctions " +
              "just above only because the toolchain refused the symbolic form too; on other platforms " +
              "no junction is built at all, so the symbolic-link refusal itself is the finding."
            : "This machine has no SeCreateSymbolicLinkPrivilege, so the staging never got the " +
              "attributable form and fell back to a JUNCTION; granting it (Developer Mode, or an " +
              "elevated shell) is what gets the symbolic link, which the toolchain accepts unaided.";

        // And naming WHOSE winsymlink the probe ran under, because the two answers send the reader to
        // different places. Saying "probed with winsymlink=0" unconditionally is FALSE on exactly the
        // run that most needs the truth: a caller who pre-set the variable gets an early return from
        // ApplyJunctionGodebug, the junction is probed with THEIR value, and a refusal that credits
        // this host with a setting it did not make points the investigation at the host instead of at
        // the environment that produced it.
        // Every Windows route to this throw has been through a junction — either the staging fell back
        // to one, or the block above rebuilt the symlinks as one — and no other platform has a
        // junction to probe at all.
        bool junctionProbed = !symbolicLinks || OperatingSystem.IsWindows();

        string godebug = !junctionProbed
            ? $"No junction was probed on this platform, so {JunctionGodebugName} does not enter into it."
            : junctionGodebug
                ? $"The junction was probed with {GodebugVariable}={JunctionGodebugSetting}, as this " +
                  "host set it — which is what Go 1.24 needs to resolve a mount point at all, so that " +
                  "is not the missing piece here."
                : $"This host set NO {JunctionGodebugName}: the run's own environment already names one " +
                  $"({GodebugVariable}={Environment.GetEnvironmentVariable(GodebugVariable)}) and an " +
                  "explicit setting from the caller is left alone, so the junction was probed with the " +
                  $"{JunctionGodebugName} value YOUR environment supplies — which is what refused it. " +
                  $"Dropping {JunctionGodebugName} from {GodebugVariable} lets this host apply the one " +
                  "Go 1.24 needs.";

        throw new InvalidOperationException(
            "the Go toolchain still refuses an internal/… import through a link-staged fixture tree, " +
            "so the programs staged there would fail exactly as a plain copy does. Neither a directory " +
            $"symlink nor a junction was accepted on this machine. {privilege} {godebug} " +
            "The toolchain said: " + refusal);

        // Returns the toolchain's refusal text for the first link-staged tree that is still refused,
        // or null when none is.
        static string? FirstRefusal(string goTool, List<(string Target, string Real)> staged)
        {
            foreach ((string target, string _) in staged)
            {
                string[] sources;

                try
                {
                    sources = Directory.GetFiles(target, "*.go");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    return $"could not read '{target}': {ex.Message}";
                }

                if (sources.Length == 0)
                    continue;

                Array.Sort(sources, StringComparer.OrdinalIgnoreCase);

                string? refused = Refusal(goTool, target, sources[0]);

                if (refused is not null)
                    return refused;
            }

            return null;
        }

        static string? Refusal(string goTool, string link, string source)
        {
            try
            {
                ProcessStartInfo start = new(goTool)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    // The directory the tests name their fixtures from, so the probe sees the module
                    // context the real invocation sees.
                    WorkingDirectory = Path.GetDirectoryName(link)!
                };

                // -e keeps a load error a DATUM rather than a non-zero exit, which is what lets the
                // refusal be read rather than merely detected. No -deps: the refusal is reported on
                // the command-line-arguments package itself, and loading the whole graph to learn the
                // same fact would cost seconds per link.
                foreach (string argument in new[] { "list", "-e", "-f", "{{if .Error}}{{.Error}}{{end}}", source })
                    start.ArgumentList.Add(argument);

                using Process? probe = Process.Start(start);

                if (probe is null)
                    return $"could not start '{goTool}'";

                string output = probe.StandardOutput.ReadToEnd();
                string errors = probe.StandardError.ReadToEnd();

                if (!probe.WaitForExit(ProbeTimeoutMilliseconds))
                {
                    try { probe.Kill(entireProcessTree: true); } catch (Exception) { }
                    return $"'go list' did not answer within {ProbeTimeoutMilliseconds / 1000}s for '{source}'";
                }

                string reported = output + errors;

                // ONLY the internal-import refusal. Anything else `go list -e` has to say about a
                // fixture — a syntax error in a deliberately malformed program, a missing dependency —
                // is the fixture's own business and says nothing about the link.
                return reported.Contains("not allowed", StringComparison.Ordinal) ? reported.Trim() : null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                return $"could not probe '{source}': {ex.Message}";
            }
        }
    }

    // JUNCTION FALLBACK ONLY. Go 1.24's `go` binary dropped winsymlink=0 from its DefaultGODEBUG
    // (1.23's carried it), so os.Lstat stopped reporting a mount point as a symbolic link and
    // filepath.EvalSymlinks stopped evaluating one — which means cmd/go's expandPath hands
    // disallowInternal a junction path UNRESOLVED, it no longer sees a directory under $GOROOT/src,
    // and an internal/… import from a junction-staged fixture tree is refused. Putting the setting
    // back for the toolchain restores the 1.23 resolution for exactly this staging and nothing else;
    // a machine that got the symbolic link never comes here.
    //
    // Measured 2026-09-20 on internal/coverage/cfile's testdata/harness.go, one axis at a time:
    //   junction  go1.23.12  GODEBUG unset -> accepted | junction  go1.24.13  GODEBUG unset -> REFUSED
    //   symlink   go1.24.13  GODEBUG unset -> accepted | junction  go1.24.13  winsymlink=0 -> accepted
    //
    // Published to BOTH environments, because both sides run the toolchain: the probe above starts
    // `go list` as a CLR child, while a fixture program is started by the TEST through converted
    // os/exec, whose Cmd.Environ() reads the converted syscall package's own copy. Same two halves,
    // and the same measured reason, as TestHost's sandbox marker.
    //
    // ONE OF TWO, AND THE OTHER IS NOT PUT BACK — worth naming, because the junction path is thereby
    // a configuration that existed at NEITHER release. `winreadlinkvolume=0` sat in the same dropped
    // list: `go version -m` reads
    //   1.23.12  build DefaultGODEBUG=…,winreadlinkvolume=0,winsymlink=0,…
    //   1.24.13  (no DefaultGODEBUG line at all)
    // and the corpus reads that neighbour too (os/windows/file_windows.cs:384,414,417 — both are in
    // internal/godebugs/table.cs:68-69 as os settings changed at 23). Restoring only winsymlink is
    // what the measurement supports and all this staging needs; the neighbour is NAMED here, not
    // measured and not proposed, so that a later surprise involving reparse-point volume paths starts
    // from a known asymmetry rather than from scratch.
    //
    // Returns whether THIS host applied the setting — false when the run's own environment already
    // names it, which the refusal text downstream has to be able to say.
    private static bool ApplyJunctionGodebug(string goRoot)
    {
        // TAKEN WHOLE, AND OVER THE FILE'S EXISTING LOCK OBJECT. s_fixtureLinks already guards this
        // file's other cross-host state — the staged links themselves, at :326, :336 and :342 — and
        // the junction GODEBUG is reached on the same thread, at the same two moments (staging and
        // teardown), through the same public surface: there is no reachability difference between the
        // two to justify a second object, a second answer, or a lock ordering to get wrong. NEITHER
        // set needs a lock for cross-host safety as the tier runs today, because those many hosts in
        // one process are SERIALISED — the measurement is at the statics' declaration below. Both
        // take it as uniform care, so the file says ONE thing about its cross-host state and so that
        // re-enabling [assembly: Parallelize] cannot silently open a window here.
        //
        // The whole body, not just the capture: the pre-value is worth keeping only if the write it
        // was captured for cannot be interleaved, and the early return below reads the same store the
        // write changes. Held across TestHost.PublishEnvironmentVariable deliberately — that call is
        // a plain CLR write plus a reflective set into the converted syscall package, so it re-enters
        // nothing here and waits on nothing — and across the stderr line. That IS a longer hold than
        // the three regions above, which are a Clear, an Add and a set scan; it is not borrowing a
        // precedent from them. It is affordable because nothing contends for this monitor: the tier
        // is single-threaded (below), both calls are bounded, and neither can block on a thread that
        // wants this lock.
        lock (s_fixtureLinks)
        {
            string? existing = Environment.GetEnvironmentVariable(GodebugVariable);

            // An explicit winsymlink from the run's own environment WINS, whichever way it points: the
            // caller has said something specific about the one setting this would change, and a host
            // that silently appends its own is no longer running the configuration it was handed. It
            // also makes this idempotent, which the in-process guard tier needs — it runs many hosts
            // in one process, and appending once per host would build a list instead of setting a
            // value. Idempotent is what makes a SECOND call in one host harmless; the lock above is
            // what would make a second host harmless, and neither substitutes for the other.
            if (NamesJunctionSetting(existing))
                return false;

            // CAPTURED BEFORE THE FIRST WRITE, in both stores, because the write is process-wide and
            // the setting is owed back at the staged tree's teardown (TestHost.Run's finally calls
            // RestoreJunctionGodebug below). The two are read separately rather than assumed equal —
            // the converted store is a snapshot syscall took at its own static init, so it can
            // legitimately differ from the CLR's — and this is the same idiom, for the same two
            // stores, that T.Setenv uses in TestExecution. Guarded so that a second apply within one
            // host could never overwrite the pre-value with an already-modified one; the two call
            // sites are mutually exclusive today.
            if (!s_junctionGodebugApplied)
            {
                s_junctionGodebugPreviousManaged = existing;
                s_junctionGodebugConvertedPresent =
                    TestHost.TryGetConvertedEnvironmentVariable(GodebugVariable, out s_junctionGodebugPreviousConverted);
                s_junctionGodebugApplied = true;
            }

            // APPEND, never replace. GODEBUG is a list and the pipeline puts real settings in it;
            // taking the variable over would silently drop them.
            string value = string.IsNullOrEmpty(existing)
                ? JunctionGodebugSetting
                : $"{existing},{JunctionGodebugSetting}";

            // THE GUARD ABOVE RESTS ON AN ORDERING INSIDE THIS CALL: PublishEnvironmentVariable writes
            // the CLR store first and the converted store second, and the second is the half that can
            // throw. Reversed, a throwing converted write would leave the CLR store unchanged, the
            // guard would not fire for the next host in this process, and the value would grow a
            // SECOND winsymlink=0. One ordering away — said again at that method's own site, since it
            // is the one that could move.
            TestHost.PublishEnvironmentVariable(GodebugVariable, value);

            // Said out loud, on the channel the unprobed-link note above already uses and for the same
            // reason: this run hands the toolchain a NON-DEFAULT setting, and a configuration the host
            // imposed on itself should not be something a reader has to infer from the link type. It
            // is also what makes "the symbolic-link path sets nothing" readable rather than merely
            // argued — the line is absent on every run that got the attributable form.
            //
            // READ IT AS "this host imposed the setting", NOT as "the setting is in force". A junction
            // run whose caller pre-set winsymlink returns above and prints nothing while running with
            // the caller's value — so the line's ABSENCE says only that this host did not impose one,
            // and is not evidence about the link type or about what the toolchain saw.
            Console.Error.WriteLine(
                $"testing: fixture trees staged as junctions (no symbolic-link privilege) — {GodebugVariable}={value} " +
                "set for the Go toolchain, without which Go 1.24 refuses their internal/… imports");

            // AND KEPT for a toolchain child that sets its OWN GODEBUG, which the publish above cannot
            // reach: os/exec keeps the last duplicate key, so a test doing
            // `append(os.Environ(), "GODEBUG=...")` hands the toolchain a GODEBUG without it (see
            // SetToolchainGodebugComposer).
            SetToolchainGodebugComposer(goRoot);

            return true;
        }
    }

    /// <summary>
    /// Puts back the <c>GODEBUG</c> the junction fallback replaced, in both stores — a no-op on every
    /// run that did not apply one.
    /// </summary>
    /// <remarks>
    /// The setting lives exactly as long as the junctions do. It is process-wide, and the corpus's own
    /// GODEBUG reader observes the CLR store, so a host that left it set would hand the NEXT host in
    /// the process a <c>winsymlink</c> the environment never named — and the in-process guard tier
    /// runs many hosts in one process. Absence is restored AS ABSENCE, each store to the value that
    /// store held.
    /// <para>
    /// THE HALF-APPLICATION MIRRORS HERE, POINTING THE OTHER WAY. The converted store is put back
    /// through the same reflective writer the apply wrote it with
    /// (<see cref="TestHost.SetConvertedEnvironmentVariable"/>, the half of
    /// <c>PublishEnvironmentVariable</c> that can fail), and that writer WARNS rather than throws —
    /// inherited from the publisher <c>T.Setenv</c> shares, and deliberately unchanged. So a restore
    /// whose converted half fails leaves the CLR store PUT BACK and the converted store still
    /// carrying <c>winsymlink=0</c>: the two stores disagree in the OPPOSITE direction from a
    /// half-failed apply, where it is the CLR store that carries the setting the other lacks. The
    /// tell is the same one either way — that writer's single stderr line — and there is no other.
    /// </para>
    /// </remarks>
    public static void RestoreJunctionGodebug()
    {
        // WHOLE, over the same object and for the same reason as the apply: this is the other end of
        // the pairing those statics record, and a put-back that could interleave with an apply is the
        // one window the capture exists to close. Sequential today, so it is uniform care rather than
        // a fix. No nesting is introduced: TestHost.Run's finally calls ReleaseFixtureLinks (:326,
        // which takes this lock over the link set) and then calls this from a SEPARATE try, so the
        // two are consecutive acquisitions — and Monitor is reentrant in any case.
        lock (s_fixtureLinks)
        {
            if (!s_junctionGodebugApplied)
                return;

            // Cleared FIRST, so a throwing restore cannot leave this run's pre-values armed for the
            // next host to put back on top of its own.
            s_junctionGodebugApplied = false;

            // The composer lives exactly as long as the setting it carries.
            SetToolchainGodebugComposer(null);

            string? managed = s_junctionGodebugPreviousManaged;
            string? converted = s_junctionGodebugPreviousConverted;
            bool convertedPresent = s_junctionGodebugConvertedPresent;

            s_junctionGodebugPreviousManaged = null;
            s_junctionGodebugPreviousConverted = null;
            s_junctionGodebugConvertedPresent = false;

            // Aimed store by store rather than through PublishEnvironmentVariable, which writes ONE
            // value to both: the apply computed its value from the CLR store and wrote it to both, so
            // putting the CLR's old value into the converted store would be a second clobber rather
            // than a restore.
            Environment.SetEnvironmentVariable(GodebugVariable, managed);
            TestHost.SetConvertedEnvironmentVariable(GodebugVariable, convertedPresent ? converted : null);
        }
    }

    // THE JUNCTION SETTING MUST REACH A TOOLCHAIN CHILD THAT SETS ITS OWN GODEBUG. The publish above
    // puts winsymlink=0 into both environments, and a child that inherits them gets it. But a test
    // that builds its child's environment as `append(os.Environ(), "GODEBUG=...")` replaces it, because
    // os/exec keeps the LAST duplicate key. internal/trace's testTraceProg does exactly that: its
    // `go run` of the junction-staged testprog is then refused ("use of internal package
    // internal/profile not allowed"), all four TestTraceCPUProfile verdicts fail on a junction host,
    // and appending winsymlink=0 to that one line made all four pass (i9, 2026-09-26, two runs).
    //
    // So while the fallback is in force, the host installs a composer at the one place every converted
    // child starts: syscall's hand-owned StartProcess (syscall/windows/exec_windows.cs,
    // childEnvironmentComposer), which offers it the RESOLVED executable path and the child's
    // environment. The composer acts only when that path IS this GOROOT's toolchain -- the binary
    // testenv.GoToolPath names, and the one AssertToolchainAcceptsLinks probed -- and then only if the
    // child's GODEBUG does not already name winsymlink, whichever way (the child's own setting wins, as
    // the run's own environment wins in ApplyJunctionGodebug). It appends to that GODEBUG, never
    // reordering or altering another key, or adds one if the child has none. The program under test,
    // and every other child, keeps its environment exactly. A host holding the symbolic-link
    // privilege never gets here.
    //
    // Installed by reflection, since testing does not reference syscall (TestHost.SyscallPackageTypeName
    // says why). No syscall assembly means no converted child can start, so there is nothing to carry.
    // A syscall WITHOUT the seam is a stale pairing that would silently bring the refusal back, so it
    // throws.
    private static void SetToolchainGodebugComposer(string? goRoot)
    {
        Type? syscallPackage = Type.GetType(TestHost.SyscallPackageTypeName, throwOnError: false);

        if (syscallPackage is null)
            return;

        FieldInfo? seam = syscallPackage.GetField(ChildEnvironmentComposerField, BindingFlags.NonPublic | BindingFlags.Static);

        if (seam is null)
        {
            if (goRoot is null)
                return;

            throw new InvalidOperationException(
                $"the junction fallback cannot carry {JunctionGodebugSetting} to a toolchain child that sets its own " +
                $"{GodebugVariable}: syscall has no '{ChildEnvironmentComposerField}' seam (syscall/windows/exec_windows.cs)");
        }

        string? goTool = goRoot is null
            ? null
            : Path.GetFullPath(Path.Combine(goRoot, "bin", OperatingSystem.IsWindows() ? "go.exe" : "go"));

        seam.SetValue(null, goTool is null ? null : (Func<string, string[], string[]?>)(
            (executable, environment) => ComposeToolchainGodebug(goTool, executable, environment)));
    }

    // Returns null for "unchanged": not the toolchain, or its GODEBUG already names winsymlink.
    private static string[]? ComposeToolchainGodebug(string goTool, string executable, string[] environment)
    {
        if (!string.Equals(Path.GetFullPath(executable), goTool, StringComparison.OrdinalIgnoreCase))
            return null;

        // The LAST GODEBUG entry, the one that takes effect, matched as Windows matches names.
        int at = -1;

        for (int i = 0; i < environment.Length; i++)
        {
            if (environment[i].StartsWith(GodebugVariable + "=", StringComparison.OrdinalIgnoreCase))
                at = i;
        }

        if (at < 0)
            return [.. environment, $"{GodebugVariable}={JunctionGodebugSetting}"];

        string value = environment[at][(GodebugVariable.Length + 1)..];

        if (NamesJunctionSetting(value))
            return null;

        string[] composed = (string[])environment.Clone();
        composed[at] = string.IsNullOrEmpty(value)
            ? $"{GodebugVariable}={JunctionGodebugSetting}"
            : $"{GodebugVariable}={value},{JunctionGodebugSetting}";

        return composed;
    }

    // TOKEN-WISE, never a substring. GODEBUG is a comma-separated list of name=value settings, and the
    // question is whether the run's own environment names THIS setting — which `Contains("winsymlink")`
    // does not answer: a future `winsymlinkfoo=1` satisfies it and would suppress the fix for a run
    // that said nothing about winsymlink at all. The neighbour `winreadlinkvolume` matches under
    // neither form, and must not. No trimming, deliberately: internal/godebug's own parser splits on
    // ',' and '=' without it, so a token the toolchain will not honour must not count here either.
    private static bool NamesJunctionSetting(string? godebug)
    {
        if (string.IsNullOrEmpty(godebug))
            return false;

        foreach (string setting in godebug.Split(','))
        {
            int separator = setting.IndexOf('=');
            ReadOnlySpan<char> name = separator < 0 ? setting.AsSpan() : setting.AsSpan(0, separator);

            if (name.Equals(JunctionGodebugName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    // The GODEBUG this run displaced, captured once on the first apply and owed back at teardown.
    // Static because the setting it guards is process-wide, and one host run executes per test
    // process — which is Go's model, and the same reason s_fixtureLinks above is static.
    //
    // WRITTEN AND READ UNDER lock (s_fixtureLinks), the same object and the same discipline as the
    // link set itself — ApplyJunctionGodebug and RestoreJunctionGodebug each take it over their whole
    // body. NEITHER set needs that lock for cross-host safety as the tier runs today, because the
    // "many hosts in one process" the notes above keep referring to are SERIALISED. MEASURED
    // 2026-09-20 at this ref: the in-process tier is MSTest, and the property that matters is NOT that
    // the test files are free of concurrency — they are not — but that nothing concurrent WRAPS A HOST
    // RUN. Every one of the 27 TestHost.Run calls in TestingRuntimeTests.cs is a synchronous call in
    // the test method's own body, neither awaited nor Task-wrapped, which is stronger than awaited, and
    // NO thread or task in either tree encloses one. The thread a reader grepping for Thread WILL find
    // is real — TestingRuntimeTests.cs:191, under `using System.Threading` at :10 — and it does not
    // matter: it is constructed INSIDE a registered test body (the registry.Add lambda opening at
    // :189), started at :192 and joined at :193 before that body returns, so it lives strictly within
    // the ONE host run at :196 and cannot make two hosts concurrent. Thread.Sleep at :279 is the same
    // shape, and the six Parallel() hits (:32, :38, :247, :271, :298, :302) are Go's T.Parallel() under
    // test on a converted testing.T, not C# parallelism; the file contains no async, await or Task at
    // all. GolibTests adds 8 more call sites across five files (2, 2, 2, 1, 1), 35 in the tree, on the
    // same terms — counted by a strict predicate, no `//` before the call on its line, because a raw
    // grep reads 36 and the extra is HostUnknownFlagPassThroughTests.cs:55 MENTIONING the call in a
    // comment, which is not a call site. One of those five does construct a thread
    // (MainGoroutineIdentityTests.cs:167, started :194, joined :195), but in a different test method
    // from its host run at :108, and that body calls no host at all.
    // The enforcement is that MSTest runs one test method
    // at a time absent an [assembly: Parallelize], that the tree's only such attribute is
    // DELIBERATELY disabled at BehavioralTestBase.cs:23 under its reason on :22 ("Don't enable for
    // timing tests:"), and that no .runsettings exists anywhere to raise parallelism instead.
    // So the lock is UNIFORM CARE, not a fix for a live race: it costs an uncontended Monitor twice
    // per host, it keeps the file saying one thing about its cross-host state, and it means that
    // re-enabling that attribute cannot silently open a window here. What re-enabling WOULD still
    // owe is a reading of whether one process-wide capture is the right shape at all when several
    // hosts stage at once, or whether it has to become per-host — the lock makes that a design
    // question rather than a corruption. Said at the attribute too, so the dependency is findable
    // from the line somebody would change.
    private static bool s_junctionGodebugApplied;
    private static string? s_junctionGodebugPreviousManaged;
    private static string? s_junctionGodebugPreviousConverted;
    private static bool s_junctionGodebugConvertedPresent;

    private const string GodebugVariable = "GODEBUG";
    private const string JunctionGodebugName = "winsymlink";
    private const string JunctionGodebugSetting = $"{JunctionGodebugName}=0";
    private const string ChildEnvironmentComposerField = "childEnvironmentComposer";

    // Generous, because it is a safety net against a wedged child rather than a performance
    // assumption: a cold `go list` on a slow host pays for the module load before it answers.
    private const int ProbeTimeoutMilliseconds = 120_000;

    /// <summary>
    /// Removes the run sandbox without following the links this view staged.
    /// </summary>
    /// <remarks>
    /// <see cref="Directory.Delete(string, bool)"/> does not traverse a reparse point — verified, and
    /// the guarantee this whole design rests on — but it does not remove one either: it throws
    /// UnauthorizedAccessException and leaves the tree behind. Unlinking each one first is what makes
    /// the sandbox actually go away, and doing it depth-first means a link is gone before anything
    /// recursive reaches its parent.
    /// </remarks>
    public static void Delete(string runRoot)
    {
        if (!Directory.Exists(runRoot))
            return;

        // Unlinking comes FIRST and is exhaustive, because the two halves fail independently and
        // only one of them is dangerous. Removing the files can legitimately fail — a test that
        // shelled out to the Go toolchain leaves handles that outlive the child briefly, and
        // go/build's suite does it on every run — which strands the sandbox. A stranded sandbox full
        // of copies is inert; a stranded sandbox full of links INTO GOROOT is a trap for any tool
        // that later deletes the temp tree and follows reparse points (PowerShell 5.1's
        // Remove-Item -Recurse does). So every link is removed even when its siblings refuse, and a
        // failure to remove the emptied tree afterwards is not allowed to prevent that.
        Unlink(runRoot);

        try
        {
            Directory.Delete(runRoot, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Left behind as ordinary files; the links are already gone.
        }

        static void Unlink(string directory)
        {
            string[] children;

            try
            {
                children = Directory.GetDirectories(directory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return;
            }

            foreach (string child in children)
            {
                try
                {
                    if (IsLink(child))
                        new DirectoryInfo(child).Delete();
                    else
                        Unlink(child);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // One link that will not go is not allowed to strand the rest.
                }
            }
        }
    }

    // Names the file that records which process owns a sandbox. Its presence is what makes an
    // abandoned sandbox distinguishable from a running one.
    private const string OwnerFileName = ".go2cs-owner";

    private static void MarkOwner(string runRoot)
    {
        try
        {
            using Process self = Process.GetCurrentProcess();
            Directory.CreateDirectory(runRoot);
            File.WriteAllText(Path.Combine(runRoot, OwnerFileName), $"{self.Id} {self.ProcessName}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Without the marker this run's sandbox is simply never reclaimed by a later one.
        }
    }

    /// <summary>
    /// Removes sandboxes for THIS package that were left behind by a host that died without running
    /// its teardown.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A normally-finishing run cleans up after itself, but two ways of dying skip the finally
    /// entirely: an uncatchable stack overflow (go/parser's depth suite produced one before the
    /// thread reservation was raised) and an external kill — which this repository documents as a
    /// routine hazard, since a cleanup preamble matching processes by NAME reaps sibling worktrees'
    /// runs. What is stranded then is not inert: it holds links INTO GOROOT, and the whole point of
    /// the teardown ordering is that such a tree must never outlive its run.
    /// </para>
    /// <para>
    /// Reclaiming is scoped so it can never touch a LIVE run, including one belonging to another
    /// worktree. Only sandboxes of the same package are considered — nothing else creates them — and
    /// only those whose recorded owner process is gone. An age threshold would not do: a legitimate
    /// suite can run for hours (hash/maphash takes ~40 minutes, index/suffixarray longer), so
    /// "old" and "abandoned" are different questions and only the second one is safe to act on.
    /// </para>
    /// </remarks>
    private static void ReclaimAbandonedSandboxes(string runRoot)
    {
        string? packageRoot = Path.GetDirectoryName(runRoot);

        if (packageRoot is null || !Directory.Exists(packageRoot))
            return;

        foreach (string sandbox in SafeDirectories(packageRoot))
        {
            if (string.Equals(sandbox, runRoot, StringComparison.OrdinalIgnoreCase) || IsOwnerAlive(sandbox))
                continue;

            try
            {
                Delete(sandbox);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }

        static string[] SafeDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return [];
            }
        }
    }

    // A sandbox counts as live unless its marker names a process that is demonstrably gone. Every
    // uncertainty resolves to "alive": an unreadable or absent marker, a malformed one, or a PID
    // whose lookup throws all leave the tree alone, because deleting a running sibling's sandbox is
    // far worse than leaving a dead one behind. The process NAME is compared alongside the id so a
    // recycled PID cannot make an unrelated process vouch for a sandbox.
    private static bool IsOwnerAlive(string sandbox)
    {
        string marker = Path.Combine(sandbox, OwnerFileName);

        if (!File.Exists(marker))
            return true;

        try
        {
            string[] parts = File.ReadAllText(marker).Split(' ', 2, StringSplitOptions.TrimEntries);

            if (parts.Length != 2 || !int.TryParse(parts[0], out int id))
                return true;

            using Process owner = Process.GetProcessById(id);
            return string.Equals(owner.ProcessName, parts[1], StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            // GetProcessById: no such process. The only answer that means "abandoned".
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    // Mirrors ONE directory level: every subdirectory becomes a link to the real one, every file a
    // hard link (a copy where the filesystem refuses one — a different volume, most often). The
    // carve-out is the next level down, which the caller materializes instead.
    private static void MirrorLevel(string realDirectory, string mirrorDirectory, string carveOut)
    {
        Directory.CreateDirectory(mirrorDirectory);

        DirectoryInfo real = new(realDirectory);

        foreach (FileSystemInfo entry in real.EnumerateFileSystemInfos())
        {
            string target = Path.Combine(mirrorDirectory, entry.Name);

            if (File.Exists(target) || Directory.Exists(target))
                continue;

            try
            {
                if (entry is DirectoryInfo)
                {
                    if (string.Equals(entry.Name, carveOut, StringComparison.OrdinalIgnoreCase))
                        continue;

                    CreateDirectoryLink(target, entry.FullName);
                }
                else
                {
                    CreateFileLink(target, entry.FullName);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                // One unmirrored entry is a gap in the view, not a failed run.
            }
        }
    }

    // The package's own files, as real copies. Subdirectories are deliberately not touched.
    private static void CopyOwnFiles(string realDirectory, string workingDirectory)
    {
        Directory.CreateDirectory(workingDirectory);

        foreach (FileInfo file in new DirectoryInfo(realDirectory).EnumerateFiles())
        {
            string target = Path.Combine(workingDirectory, file.Name);

            if (File.Exists(target))
                continue;

            try
            {
                file.CopyTo(target);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }
    }

    private static bool IsLink(string path)
    {
        try
        {
            DirectoryInfo directory = new(path);

            // Attributes on a path that does not exist is (FileAttributes)(-1) — every bit set,
            // ReparsePoint among them — so existence has to be established first or a directory this
            // view never staged reads back as a link and gets "unlinked".
            return directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static IEnumerable<string> ComponentsBetween(string root, string full)
    {
        string relative = Path.GetRelativePath(root, full);

        if (relative is "." or "")
            yield break;

        string current = root;

        foreach (string segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (segment.Length == 0)
                continue;

            current = Path.Combine(current, segment);
            yield return current;
        }
    }

    private static void CreateDirectoryLink(string link, string target)
    {
        if (!OperatingSystem.IsWindows())
        {
            // Unprivileged everywhere but Windows, and equivalent for this view's purpose: reads
            // resolve, walks do not descend.
            Directory.CreateSymbolicLink(link, target);
            return;
        }

        // A Windows SYMLINK needs SeCreateSymbolicLinkPrivilege — administrator, or Developer Mode —
        // which a test run cannot assume. A JUNCTION is the unprivileged equivalent for directories
        // and has no managed API, so it is set here by hand.
        CreateJunction(link, target);
    }

    private static void CreateFileLink(string link, string target)
    {
        if (OperatingSystem.IsWindows())
        {
            if (CreateHardLinkW(link, target, IntPtr.Zero))
                return;
        }
        else
        {
            try
            {
                File.CreateSymbolicLink(link, target);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }

        // Different volume, or a filesystem with no link support: the file is small enough to copy or
        // it is not one a test reads.
        File.Copy(target, link);
    }

    private const uint IoReparseTagMountPoint = 0xA0000003;
    private const uint FsctlSetReparsePoint = 0x000900A4;
    private const uint GenericWrite = 0x40000000;
    private const uint OpenExisting = 3;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;

    private static void CreateJunction(string link, string target)
    {
        Directory.CreateDirectory(link);

        // The reparse point stores an NT-namespace path; the print name is the plain one Explorer
        // and `dir` show.
        string substituteName = @"\??\" + Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar);
        string printName = Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar);

        byte[] substitute = System.Text.Encoding.Unicode.GetBytes(substituteName);
        byte[] print = System.Text.Encoding.Unicode.GetBytes(printName);

        // REPARSE_DATA_BUFFER: an 8-byte header, then the 8-byte mount-point sub-header, then the two
        // NUL-terminated names back to back.
        int pathBufferLength = substitute.Length + 2 + print.Length + 2;
        int dataLength = 8 + pathBufferLength;
        int totalLength = 8 + dataLength;

        byte[] buffer = new byte[totalLength];
        int offset = 0;

        void WriteUInt32(uint value)
        {
            BitConverter.GetBytes(value).CopyTo(buffer, offset);
            offset += 4;
        }

        void WriteUInt16(ushort value)
        {
            BitConverter.GetBytes(value).CopyTo(buffer, offset);
            offset += 2;
        }

        WriteUInt32(IoReparseTagMountPoint);
        WriteUInt16((ushort)dataLength);
        WriteUInt16(0);
        WriteUInt16(0);                                    // SubstituteNameOffset
        WriteUInt16((ushort)substitute.Length);            // SubstituteNameLength
        WriteUInt16((ushort)(substitute.Length + 2));      // PrintNameOffset
        WriteUInt16((ushort)print.Length);                 // PrintNameLength

        substitute.CopyTo(buffer, offset);
        print.CopyTo(buffer, offset + substitute.Length + 2);

        using SafeFileHandle handle = CreateFileW(link, GenericWrite, 0, IntPtr.Zero, OpenExisting,
            FileFlagBackupSemantics | FileFlagOpenReparsePoint, IntPtr.Zero);

        if (handle.IsInvalid)
            throw new IOException($"could not open '{link}' to set a junction", Marshal.GetLastWin32Error());

        IntPtr native = Marshal.AllocHGlobal(totalLength);

        try
        {
            Marshal.Copy(buffer, 0, native, totalLength);

            if (!DeviceIoControl(handle, FsctlSetReparsePoint, native, totalLength, IntPtr.Zero, 0, out _, IntPtr.Zero))
                throw new IOException($"could not set a junction at '{link}'", Marshal.GetLastWin32Error());
        }
        finally
        {
            Marshal.FreeHGlobal(native);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW")]
    private static extern SafeFileHandle CreateFileW(string fileName, uint desiredAccess, uint shareMode,
        IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(SafeFileHandle device, uint ioControlCode, IntPtr inBuffer,
        int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned, IntPtr overlapped);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateHardLinkW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLinkW(string fileName, string existingFileName, IntPtr securityAttributes);
}
