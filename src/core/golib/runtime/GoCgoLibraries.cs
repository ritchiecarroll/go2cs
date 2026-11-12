// GoCgoLibraries.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// One C library a converted cgo package binds: the <c>-l</c> flag its <c>#cgo LDFLAGS</c> named, and the load-time name
/// the converter captured for it with the pinned C toolchain (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.1, §11.5): an
/// ELF SONAME (<c>libm.so.6</c>), a DLL as the import table names it, or a darwin install name as an absolute path.
/// </summary>
public sealed class GoCgoLibrary(string flag, string name)
{
    /// <summary>The <c>-l</c> flag the package's <c>#cgo LDFLAGS</c> named.</summary>
    public string Flag { get; } = flag;

    /// <summary>The load-time name the converter captured for it.</summary>
    public string Name { get; } = name;
}

/// <summary>
/// The resolver a converted cgo package registers for its own assembly (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.2).
/// The converter emits only the data: the package's libraries, and a module initializer that registers them.
/// </summary>
public static class GoCgoLibraries
{
    /// <summary>Registers the resolver for <paramref name="assembly"/>'s own <c>[LibraryImport]</c> bindings.</summary>
    public static void Register(Assembly assembly, params GoCgoLibrary[] libraries) => Register(assembly, "", libraries);

    /// <summary>
    /// Registers the resolver for <paramref name="assembly"/>'s own <c>[LibraryImport]</c> bindings, which were converted
    /// for <paramref name="target"/> (<c>GOOS/GOARCH</c>, the target the bindings file's header records).
    /// </summary>
    public static void Register(Assembly assembly, string target, params GoCgoLibrary[] libraries)
    {
        List<GoCgoLibrary> owned = [.. libraries];
        NativeLibrary.SetDllImportResolver(assembly, (name, importing, _) => Resolve(name, importing, owned, target));
    }

    /// <summary>The running process's target, as Go names it: <c>GOOS/GOARCH</c>.</summary>
    internal static string RunningTarget => $"{GoHostOfRecord.HostGOOS()}/{GoHostOfRecord.GoArchOf(RuntimeInformation.ProcessArchitecture)}";

    /// <summary>
    /// Why bindings converted for <paramref name="target"/> cannot run on <paramref name="hostGOOS"/> with
    /// <paramref name="processArchitecture"/>, or <see langword="null"/> when they can (§5.3, as amended by §11.20). The
    /// architecture is the PROCESS's, not the machine's: an x64 process under Rosetta or on Windows on Arm runs amd64
    /// code, with amd64's C layouts. Bindings registered with no target (the original overload) are not checked.
    /// </summary>
    internal static string? TargetMismatch(string target, string hostGOOS, Architecture processArchitecture)
    {
        if (target.Length == 0)
            return null;

        string running = $"{hostGOOS}/{GoHostOfRecord.GoArchOf(processArchitecture)}";

        if (string.Equals(target, running, StringComparison.Ordinal))
            return null;

        return $"cgo: these C bindings were converted for {target}, and this process runs as {running}: their C structs cross with {target}'s layouts and C library, which {running} does not share, so they are refused rather than called (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.3, §11.20)";
    }

    // Where a library the table owns may load from: where a Go binary's own loader would look for it, and never the
    // current directory (the DLL-planting surface, §5.4).
    private static string Places =>
        OperatingSystem.IsWindows() ? "the application directory, the assembly's directory, System32 and the directories added with AddDllDirectory" :
        OperatingSystem.IsMacOS() ? "the dynamic loader's resolution of the captured absolute install name" :
        "the dynamic loader's search (LD_LIBRARY_PATH, the executable's run path, the loader cache and the system directories)";

    /// <summary>
    /// The handle for a library the table owns, or 0 for a name it does not own, which keeps the runtime's own resolution
    /// for any hand-owned import in the same assembly (§11.11). An owned library loads from where a Go binary's loader
    /// would find it: on windows the safe directories and the assembly's own, and elsewhere the system loader, with the
    /// captured SONAME or absolute install name. A miss throws, naming the -l flag, the captured name and the places
    /// tried, and never falls back to the runtime's probing, whose windows search includes the current directory.
    /// On darwin a name that is not absolute is refused before dyld sees it (§11.12, mandatory since increment 4d): dyld's
    /// search for a bare name reached the current directory on both hosted RIDs.
    /// </summary>
    internal static nint Resolve(string name, Assembly assembly, IReadOnlyList<GoCgoLibrary> libraries) => Resolve(name, assembly, libraries, "");

    internal static nint Resolve(string name, Assembly assembly, IReadOnlyList<GoCgoLibrary> libraries, string target)
    {
        // The target is held for EVERY import the assembly makes, a name the table does not own among them: a hand-owned
        // import beside bindings converted for another target crosses with that target's layouts too.
        if (TargetMismatch(target, GoHostOfRecord.HostGOOS(), RuntimeInformation.ProcessArchitecture) is { } mismatch)
            throw new PlatformNotSupportedException(mismatch);

        foreach (GoCgoLibrary library in libraries)
        {
            if (library.Name != name)
                continue;

            if (OperatingSystem.IsMacOS() && !name.StartsWith('/'))
                throw new DllNotFoundException($"cgo: {library.Flag} binds {name}, which is not an absolute install name: darwin loads a captured library only by its absolute path, since dyld's search for a bare name reaches the current directory; the current directory is never searched (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.12)");

            nint handle;

            bool loaded = OperatingSystem.IsWindows() ?
                NativeLibrary.TryLoad(name, assembly, DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory, out handle) :
                NativeLibrary.TryLoad(name, out handle);

            if (loaded)
                return handle;

            throw new DllNotFoundException($"cgo: {library.Flag} binds {name}, which was not found in {Places}; the current directory is never searched (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.2)");
        }

        return 0;
    }
}
