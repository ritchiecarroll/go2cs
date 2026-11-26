// PlatformExclusive.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The ONE definition of "is this behavioral package native to this host", shared by the two
// harnesses that enumerate behavioral packages: BehavioralRunner and the MSTest BehavioralTestBase.
// LINKED into both by path, for the same reason ConverterBuildInputs.cs is -- the runners take no
// assembly dependency on each other, so a shared assembly is not available, and three copies of a
// predicate is three chances for them to disagree about which packages a host may measure.
//
// The class this gates bites in BOTH directions and was invisible in each until it was named. A
// behavioral package whose Go source calls unix-only syscall API (Socketpair, UnixRights, SendmsgN)
// cannot be TYPE-CHECKED on Windows; one calling FindFirstFile or the WSA surface cannot be
// type-checked on Linux. The converter then emits a best-effort conversion, and every harness
// reported the result as a failure or -- worse -- as a byte-identical pass over a file it never
// regenerated. Marking the package makes the skip LOUD and BY NAME, which is the point: a silent
// drop from the enumeration would trade one invisible problem for another.
//
// NOT ALWAYS A TYPE-CHECK FAILURE, which is why the skip lines say "cannot measure" rather than
// "cannot type-check". Most marked packages fail at TRANSPILE (the converter cannot type-check
// unix-only or Win32-only syscall API), but LocalTimeZone type-checks anywhere and fails at OUTPUT:
// its converted form reaches syscall.GetTimeZoneInformation and faults on kernel32 off Windows.
// One marker covers both by construction -- a non-native host skips EVERY phase -- so the wording
// is the thing that has to stay honest about which phase actually bites.
//
// TWO AXES since 2026-09-04, GOOS and GOARCH, because the class turned out to have a second half
// nobody had a marker for. Go's own filename rule makes name_GOARCH.go an implicit build constraint,
// so StdLibInternalAbi -- which copies internal/abi and internal/goarch into a package main carrying
// abi_amd64.go, goarch_amd64.go and zgoarch_amd64.go -- does not build on arm64 in GO, measured:
// GOARCH=arm64 go build reports `undefined: IntArgRegs`, `undefined: _ArchFamily` and the Is*
// endianness constants, where GOARCH=amd64 builds clean. Both darwin censuses read the downstream
// half of that on osx-arm64 as `goarch.cs(23,22): error CS0145` while osx-x64 passed every phase.
// The axes are orthogonal and a package may carry both markers; EITHER excluding the host skips it.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

internal static class PlatformExclusive
{
    // Line-anchored on purpose. The attribute NAME appears in prose -- this file's own header is an
    // example -- and an unanchored match would gate a package on a comment. Same reasoning as the
    // corpus's GoManualConversion census, which reported 63 marked files against a real 40 until it
    // was anchored.
    private static readonly Regex s_marker =
        new(@"^\s*\[(?:go\.)?GoPlatformExclusive\s*\(([^)]*)\)\]", RegexOptions.Multiline | RegexOptions.Compiled);

    // The GOARCH axis, added 2026-09-04 and anchored for the same reason. A SEPARATE marker rather
    // than another token form of the one above: the two axes are orthogonal, and the package that
    // motivated this is native to every GOOS while exclusive to one GOARCH, so an os/arch token form
    // would mean spelling all three GOOS names to say one thing about the arch.
    private static readonly Regex s_archMarker =
        new(@"^\s*\[(?:go\.)?GoArchExclusive\s*\(([^)]*)\)\]", RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex s_platform = new("\"([^\"]+)\"", RegexOptions.Compiled);

    /// <summary>
    /// The GOOS names a behavioral package is native to, or an empty array when it is unmarked
    /// (i.e. native everywhere, which is the overwhelming majority).
    /// </summary>
    public static string[] PlatformsFor(string packageDir) => NamesFor(packageDir, s_marker);

    /// <summary>
    /// The GOARCH names a behavioral package is native to, or an empty array when it is unmarked.
    /// </summary>
    public static string[] ArchesFor(string packageDir) => NamesFor(packageDir, s_archMarker);

    // One extraction for both axes, so the two markers cannot drift apart in how they are read --
    // the same reason this whole file exists rather than a predicate per harness.
    private static string[] NamesFor(string packageDir, Regex marker)
    {
        string infoFile = Path.Combine(packageDir, "package_info.cs");

        if (!File.Exists(infoFile))
            return Array.Empty<string>();

        Match match = marker.Match(File.ReadAllText(infoFile));

        if (!match.Success)
            return Array.Empty<string>();

        return s_platform.Matches(match.Groups[1].Value)
            .Select(m => m.Groups[1].Value)
            .ToArray();
    }

    /// <summary>
    /// The GOOS this host measures as. Honors GoTargetOS so a cross-target run gates on the target
    /// it is actually building for rather than on the machine it happens to run on -- the corpus's
    /// per-GOOS layout makes that a real distinction, not a hypothetical one.
    /// </summary>
    public static string HostGoos
    {
        get
        {
            string target = Environment.GetEnvironmentVariable("GoTargetOS") ?? "";

            if (!string.IsNullOrWhiteSpace(target))
                return target.Trim().ToLowerInvariant();

            if (OperatingSystem.IsWindows())
                return "windows";

            return OperatingSystem.IsMacOS() ? "darwin" : "linux";
        }
    }

    /// <summary>
    /// The GOARCH this host measures as, in Go's spelling.
    /// </summary>
    /// <remarks>
    /// There is deliberately NO env override here, where <see cref="HostGoos"/> honors GoTargetOS.
    /// No such knob exists: `GoTargetArch` appears nowhere in the tree, the corpus layout has no arch
    /// dimension, and no harness passes the converter's `-platforms` -- so the arch a run measures is
    /// simply the arch its own converter defaulted to, which is this process's. Inventing an override
    /// no instrument sets would be a branch nothing can exercise. If a GoTargetArch is ever
    /// introduced, this property is the one place it joins.
    ///
    /// RESIDUAL, stated rather than guarded: this reads the HARNESS process's architecture, and a
    /// mixed toolchain (an emulated x64 .NET beside a native arm64 `go`) would disagree with the
    /// converter's own `runtime.GOARCH`. No fleet host is one; the exact-but-costlier source would be
    /// `go env GOARCH` per run.
    /// </remarks>
    public static string HostGoarch => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "amd64",
        Architecture.Arm64 => "arm64",
        Architecture.X86 => "386",
        Architecture.Arm => "arm",
        _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
    };

    /// <summary>
    /// How this host describes itself in a skip line: the GOOS and GOARCH it measures as.
    /// </summary>
    public static string HostTarget => $"{HostGoos}/{HostGoarch}";

    /// <summary>
    /// True when the package is marked on EITHER axis and this host is excluded by it. An unmarked
    /// package is never skipped.
    /// </summary>
    /// <param name="packageDir">The behavioral package directory to test.</param>
    /// <param name="nativeTo">
    /// What the package is native to, for the skip line -- GOOS names, GOARCH names, or both when it
    /// carries both markers. The two name spaces are disjoint (no GOOS is spelled "amd64"), so the
    /// list is self-describing without an axis prefix.
    /// </param>
    public static bool ShouldSkip(string packageDir, out string nativeTo)
    {
        string[] platforms = PlatformsFor(packageDir);
        string[] arches = ArchesFor(packageDir);

        nativeTo = string.Join(", ", platforms.Concat(arches));

        bool wrongPlatform = platforms.Length > 0 && !platforms.Contains(HostGoos, StringComparer.OrdinalIgnoreCase);
        bool wrongArch = arches.Length > 0 && !arches.Contains(HostGoarch, StringComparer.OrdinalIgnoreCase);

        return wrongPlatform || wrongArch;
    }
}
