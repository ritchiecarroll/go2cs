// GoCgoLibraries.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace go.golib;

/// <summary>
/// One C library a converted cgo package binds: the <c>-l</c> flag its <c>#cgo LDFLAGS</c> named, and the load-time name
/// the converter captured for it with the pinned C toolchain (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.1, §11.5): an
/// ELF SONAME (<c>libm.so.6</c>), a DLL as the import table names it, or a darwin install name as an absolute path.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoCgoLibrary(string flag, string name)
{
    /// <summary>The <c>-l</c> flag the package's <c>#cgo LDFLAGS</c> named.</summary>
    public string Flag { get; } = flag;

    /// <summary>The load-time name the converter captured for it.</summary>
    public string Name { get; } = name;
}

/// <summary>
/// The refusal of C bindings converted for another target than the running process's (cgo against go.*, A6): its own type,
/// so the type-assertion binder can tell it from an ordinary binding miss and rethrow it (<c>AdapterBinder.IsBindingFailure</c>),
/// and a <see cref="PlatformNotSupportedException"/>, so every existing catch of that still reads it.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public sealed class GoCgoTargetMismatchException(string message) : PlatformNotSupportedException(message);

/// <summary>
/// The resolver a converted cgo package registers for its own assembly (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.2).
/// The converter emits only the data: the package's libraries, and a module initializer that registers them.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)] public static partial class GoCgoLibraries
{
    /// <summary>Registers the resolver for <paramref name="assembly"/>'s own <c>[LibraryImport]</c> bindings.</summary>
    public static void Register(Assembly assembly, params GoCgoLibrary[] libraries) => Register(assembly, "", libraries);

    /// <summary>
    /// Registers the resolver for <paramref name="assembly"/>'s own <c>[LibraryImport]</c> bindings, which were converted
    /// for <paramref name="target"/> (<c>GOOS/GOARCH</c>, the target the bindings file's header records).
    /// </summary>
    public static void Register(Assembly assembly, string target, params GoCgoLibrary[] libraries)
    {
        // A6: refused HERE, in the package's module initializer, before any of its code runs (cgo against go.*, INC 3).
        // Its managed code carries C layouts folded for the target (unsafe.Sizeof of a C type is a constant), so code that
        // never calls C is as wrong on another target as a call is. Resolve keeps its own check as the second layer.
        // A process on the target the bindings were converted for sees no change: one compare, and on linux the libc
        // readings below, once per cgo assembly at load.
        if (Refusal(target) is { } refusal)
            throw new GoCgoTargetMismatchException(refusal);

        // A process that links a cgo package keeps the pin-extent index, which the void pointer's door asks
        // (docs/phase4/DESIGN-cgo-pin-extents.md, D4). Every other process never creates it.
        PinExtents.Activate();

        List<GoCgoLibrary> owned = [.. libraries];
        s_registered.AddOrUpdate(assembly, new Registration(owned, target));
        NativeLibrary.SetDllImportResolver(assembly, (name, importing, _) => Resolve(name, importing, owned, target));
    }

    // What each cgo assembly registered, for a C variable's lookup (Variable), which loads its library by the same rules.
    private sealed record Registration(List<GoCgoLibrary> Libraries, string Target);

    private static readonly ConditionalWeakTable<Assembly, Registration> s_registered = new();

    /// <summary>
    /// A box over the C global variable <paramref name="symbol"/> that <paramref name="library"/> exports, for
    /// <paramref name="assembly"/>'s bindings (cgo item 3, Go's <c>C.x</c>): Go reads and writes a C variable in place,
    /// so the box aliases the symbol's address and owns no storage. The library loads as the assembly's own
    /// <c>[LibraryImport]</c> bindings load it (<see cref="Resolve(string, Assembly, IReadOnlyList{GoCgoLibrary}, string)"/>),
    /// never from the current directory; a symbol it does not export is refused by name.
    /// </summary>
    public static ж<T> Variable<T>(Assembly assembly, string library, string symbol)
    {
        if (!s_registered.TryGetValue(assembly, out Registration? registration))
            throw new InvalidOperationException($"cgo: C.{symbol}: {assembly.GetName().Name} registered no C libraries, so no library is known to export the variable");

        nint handle = Resolve(library, assembly, registration.Libraries, registration.Target);

        if (handle == 0)
            throw new DllNotFoundException($"cgo: C.{symbol}: {library} is not one of the libraries {assembly.GetName().Name} registered");

        if (!NativeLibrary.TryGetExport(handle, symbol, out nint address) || address == 0)
            throw new EntryPointNotFoundException($"cgo: C.{symbol}: {library} exports no variable of that name (docs/Limitations.md#cgo)");

        return new NativeBox<T>((nuint)address);
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

        return $"cgo: these C bindings were converted for {target}, and this process runs as {running}: their C structs cross with {target}'s layouts and C library, which {running} does not share, so they are refused rather than called (docs/Limitations.md#cgo)";
    }

    /// <summary>
    /// Why bindings converted for <paramref name="target"/> cannot run in this process, or <see langword="null"/>: the
    /// target's GOOS/GOARCH against the process's (<see cref="TargetMismatch"/>), then, for a linux target, the process's C
    /// library (<see cref="LinuxLibcMismatch"/>). No target, no check (the two-argument Register).
    /// </summary>
    internal static string? Refusal(string target) =>
        TargetMismatch(target, GoHostOfRecord.HostGOOS(), RuntimeInformation.ProcessArchitecture) ??
        (target.StartsWith("linux/", StringComparison.Ordinal) && OperatingSystem.IsLinux() ? RunningLibcMismatch(target) : null);

    private static readonly Lazy<(bool glibcExport, string runtimeIdentifier, bool ldMuslMapped)> s_libc = new(() =>
    {
        bool export = NativeLibrary.TryGetExport(NativeLibrary.GetMainProgramHandle(), "gnu_get_libc_version", out _);
        bool musl = false;

        try
        {
            foreach (string line in File.ReadLines("/proc/self/maps"))
            {
                if (line.Contains("/ld-musl-", StringComparison.Ordinal))
                {
                    musl = true;
                    break;
                }
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        return (export, RuntimeInformation.RuntimeIdentifier, musl);
    });

    private static string? RunningLibcMismatch(string target)
    {
        (bool export, string rid, bool musl) = s_libc.Value;
        return LinuxLibcMismatch(target, export, rid, musl);
    }

    /// <summary>
    /// Why linux bindings, which narrow v1 converts against glibc only (the converter's R3 refuses a musl compiler), cannot
    /// run in a process with these libc readings, or <see langword="null"/> when every reading says glibc:
    /// <paramref name="glibcExport"/>, a POSITIVE signal, that gnu_get_libc_version resolves in the process;
    /// <paramref name="runtimeIdentifier"/>, a second reading, refusing only a musl form, a "musl" RID or an Alpine
    /// distribution RID (alpine.3.22-x64; Alpine is musl only), where a distribution-built glibc runtime reports a
    /// distribution RID too (ubuntu.24.04-x64) and is glibc; and <paramref name="ldMuslMapped"/>, musl's loader in the
    /// process's maps, which gcompat (Alpine's glibc shim, which exports gnu_get_libc_version) cannot hide.
    /// </summary>
    internal static string? LinuxLibcMismatch(string target, bool glibcExport, string runtimeIdentifier, bool ldMuslMapped)
    {
        string? why =
            ldMuslMapped ? "musl's loader (ld-musl) is mapped in this process" :
            runtimeIdentifier.Contains("musl", StringComparison.OrdinalIgnoreCase) || runtimeIdentifier.StartsWith("alpine", StringComparison.OrdinalIgnoreCase) ?
                $"this runtime reports the musl RID {runtimeIdentifier}" :
            !glibcExport ? $"glibc's gnu_get_libc_version does not resolve in this process (RID {runtimeIdentifier})" :
            null;

        return why is null ? null :
            $"cgo: these C bindings were converted for {target} against glibc, and {why}: their C calls and layouts are glibc's, so they are refused rather than called (docs/Limitations.md#cgo)";
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
        if (Refusal(target) is { } mismatch)
            throw new GoCgoTargetMismatchException(mismatch);

        foreach (GoCgoLibrary library in libraries)
        {
            if (library.Name != name)
                continue;

            if (OperatingSystem.IsMacOS() && !name.StartsWith('/'))
                throw new DllNotFoundException($"cgo: {library.Flag} binds {name}, which is not an absolute install name: darwin loads a captured library only by its absolute path, since dyld's search for a bare name reaches the current directory; the current directory is never searched (docs/Limitations.md#cgo)");

            nint handle = OperatingSystem.IsWindows() ? LoadOnWindows(name, assembly) : NativeLibrary.TryLoad(name, out nint loaded) ? loaded : 0;

            if (handle != 0)
                return handle;

            throw new DllNotFoundException($"cgo: {library.Flag} binds {name}, which was not found in {Places}; the current directory is never searched (docs/Limitations.md#cgo)");
        }

        return 0;
    }

    // On windows a bound library loads from the assembly's directory, then the application directory, System32 and the
    // AddDllDirectory directories, and its DEPENDENTS are held to the same places: LoadLibraryExW's LOAD_LIBRARY_SEARCH
    // flags apply to the library and every dependency it loads. NativeLibrary.TryLoad with DllImportSearchPath flags did
    // not hold the dependents: a dependent only in the current directory loaded
    // (TestCgoResolveHoldsABoundLibrarysDependentsOutOfTheCurrentDirectory, b927d8e4fb, cgo-e2e 46064580544 and
    // 46064584096), the shape Phase 4's side library measured first (TestCgoPhase4H, 46054089826).
    private static nint LoadOnWindows(string name, Assembly assembly)
    {
        string directory = assembly.Location.Length > 0 ? Path.GetDirectoryName(assembly.Location)! : AppContext.BaseDirectory;
        string beside = Path.GetFullPath(Path.Combine(directory, name));

        nint handle = File.Exists(beside) ? LoadLibraryExW(beside, 0, LoadLibrarySearchDllLoadDir | LoadLibrarySearchDefaultDirs) : 0;

        return handle != 0 ? handle : LoadLibraryExW(name, 0, LoadLibrarySearchDefaultDirs);
    }

    // LoadLibraryExW's search flags (libloaderapi.h): the loaded library's own directory, and the application directory,
    // System32 and the AddDllDirectory directories, for the library and every dependency it loads.
    private const uint LoadLibrarySearchDllLoadDir = 0x00000100;
    private const uint LoadLibrarySearchDefaultDirs = 0x00001000;

    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint LoadLibraryExW(string fileName, nint file, uint flags);
}
