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

        AssertToolchainAcceptsLinks(goRoot, staged, symbolicLinks);

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
    private static void AssertToolchainAcceptsLinks(string goRoot, List<(string Target, string Real)> staged, bool symbolicLinks)
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

            refusal = FirstRefusal(goTool, staged);

            if (refusal is null)
                return;
        }

        throw new InvalidOperationException(
            "the Go toolchain still refuses an internal/… import through a link-staged fixture tree, " +
            "so the programs staged there would fail exactly as a plain copy does. Neither a directory " +
            "symlink nor a junction was accepted on this machine. The toolchain said: " + refusal);

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
