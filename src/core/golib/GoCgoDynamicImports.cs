// GoCgoDynamicImports.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace go;

/// <summary>
/// Resolves a cgo-imported trampoline to the REAL, callable address of its dynamic symbol —
/// the class-B half of <c>abi.FuncPCABI0</c>.
/// </summary>
/// <remarks>
/// <para>
/// The value this returns is dereferenced by design: <c>rawSyscall</c> jumps to it. That is the whole
/// difference from the synthetic-PC half, which mints a token that symbolizes and is never called, and
/// it is why a token here would be the same defect as the <c>return default</c> this replaces —
/// plausible, unique, stable, and fatal at the first call.
/// </para>
/// <para>
/// Lookup is scoped to the assembly that DECLARES the trampoline, reached from the argument itself
/// (<c>Delegate.Method.DeclaringType.Assembly</c>), so no package needs to publish a registry and no
/// initialization order matters. A stub carrying the external-stub marker WITHOUT a record here is
/// class C — Go's own assembly, which has nothing to resolve from — and the caller throws loudly
/// rather than inventing a value.
/// </para>
/// <para>
/// Failure is loud in both directions on purpose. A record naming a library that will not load, or a
/// symbol the library does not export, throws with the symbol and library named; it never falls back
/// to zero, because a zero here is exactly the silent wrong answer the arc exists to remove.
/// </para>
/// </remarks>
public static class GoCgoDynamicImports
{
    // Keyed on the method itself: the same trampoline resolves to the same export for the process's
    // life, and the dictionary also keeps the resolution off the hot path after first call.
    private static readonly ConcurrentDictionary<MethodInfo, nint> s_entryPoints = new();

    // Handles are cached separately because many trampolines share one library — libSystem exports
    // every symbol the darwin syscall package imports — and NativeLibrary.Load is not free.
    private static readonly ConcurrentDictionary<string, nint> s_libraries = new();

    // The reverse map, so the dispatch bottom can NAME the symbol it was handed when it must refuse
    // a call: libcCall receives an address and nothing else, and "cannot dispatch 0x7fff..." is not
    // a message anyone can act on. Filled at resolution, read only on the failure path.
    private static readonly ConcurrentDictionary<nint, string> s_symbols = new();

    /// <summary>
    /// The libSystem imports Go's linker binds as <c>&lt;name&gt;$INODE64</c> on macOS/amd64
    /// (cmd/link/internal/ld/macho.go, "Some 64-bit functions have a $INODE64 ... suffix").
    /// </summary>
    /// <remarks>
    /// The bare x86_64 symbols are the LEGACY 32-bit-inode ABI, whose <c>struct dirent</c> and
    /// <c>struct statfs</c> are not the layouts Go's syscall types describe. Binding the bare readdir_r
    /// read four empty, directory-typed names per directory on osx-x64 and sent filepath.WalkDir into an
    /// unbounded self-recursion (run 36976772120, 2026-10-02). arm64 has only the one layout. repoguard's
    /// TestGolibInode64RenamesMatchGoLinker keeps this list equal to the one in GOROOT's macho.go.
    /// </remarks>
    public static readonly string[] Inode64RenamedSymbols = ["fdopendir", "readdir_r", "getfsstat"];

    private const string LibSystem = "/usr/lib/libSystem.B.dylib";

    /// <summary>
    /// The name Go's linker would bind <paramref name="symbol"/> to: <c>&lt;symbol&gt;$INODE64</c> for a
    /// listed libSystem import on macOS x86_64, the symbol itself everywhere else.
    /// </summary>
    public static string LinkerSymbolName(string symbol, string library, bool isMacOS, Architecture architecture) =>
        isMacOS && architecture == Architecture.X64 && library == LibSystem && Array.IndexOf(Inode64RenamedSymbols, symbol) >= 0 ?
            symbol + "$INODE64" :
            symbol;

    /// <summary><see cref="LinkerSymbolName(string, string, bool, Architecture)"/> for this process.</summary>
    public static string LinkerSymbolName(string symbol, string library) =>
        LinkerSymbolName(symbol, library, OperatingSystem.IsMacOS(), RuntimeInformation.ProcessArchitecture);

    /// <summary>
    /// Attempts to resolve <paramref name="method"/> as a cgo-imported trampoline.
    /// </summary>
    /// <param name="method">The delegate target's method, i.e. the trampoline stub.</param>
    /// <param name="entryPoint">On success, the exported function's address.</param>
    /// <returns>
    /// <c>true</c> when the declaring assembly carries a <see cref="GoCgoImportDynamicAttribute"/>
    /// for this method; <c>false</c> when it does not, which is the caller's signal that the stub is
    /// class C and must throw.
    /// </returns>
    /// <exception cref="EntryPointNotFoundException">
    /// A record exists but its library or symbol could not be resolved. This is deliberately NOT a
    /// <c>false</c> return: "there is no record" and "the record is wrong" are different answers, and
    /// collapsing them would let a typo read as class C.
    /// </exception>
    public static bool TryResolve(MethodInfo method, out nint entryPoint)
    {
        if (method is null)
        {
            entryPoint = 0;
            return false;
        }

        if (s_entryPoints.TryGetValue(method, out entryPoint))
        {
            return true;
        }

        GoCgoImportDynamicAttribute? record = FindRecord(method);

        if (record is null)
        {
            entryPoint = 0;
            return false;
        }

        entryPoint = Resolve(record.Symbol, record.Library);
        s_entryPoints[method] = entryPoint;
        s_symbols[entryPoint] = record.Symbol;
        return true;
    }

    /// <summary>
    /// Resolves one symbol in one library to its exported address.
    /// </summary>
    /// <remarks>
    /// Exposed separately from <see cref="TryResolve(MethodInfo, out nint)"/> so the seam can be
    /// exercised on a host that is not the target: there is no mac in the fleet, and a guard that can
    /// only run on darwin is a guard that never runs.
    /// </remarks>
    /// <param name="symbol">The dynamic symbol, e.g. <c>fork</c>.</param>
    /// <param name="library">The library the pragma names, e.g. <c>/usr/lib/libSystem.B.dylib</c>.</param>
    /// <returns>The exported function's address, never zero.</returns>
    /// <exception cref="EntryPointNotFoundException">The library or the symbol could not be resolved.</exception>
    public static nint Resolve(string symbol, string library)
    {
        nint handle = s_libraries.GetOrAdd(library, static path =>
        {
            if (!NativeLibrary.TryLoad(path, out nint loaded))
            {
                throw new EntryPointNotFoundException(
                    $"go2cs: could not load '{path}' named by a //go:cgo_import_dynamic pragma");
            }

            return loaded;
        });

        // Bind the name Go's linker would: on macOS x86_64 a few libSystem imports are $INODE64.
        symbol = LinkerSymbolName(symbol, library);

        if (!NativeLibrary.TryGetExport(handle, symbol, out nint address) || address == 0)
        {
            throw new EntryPointNotFoundException(
                $"go2cs: '{library}' does not export '{symbol}', named by a //go:cgo_import_dynamic pragma");
        }

        s_symbols[address] = symbol;
        return address;
    }

    /// <summary>
    /// Names the symbol a previously resolved address belongs to, or <c>null</c> when this process
    /// never resolved it — for the dispatch bottom's refusal messages, never for control flow.
    /// </summary>
    public static string? SymbolOf(nint entryPoint)
    {
        return s_symbols.TryGetValue(entryPoint, out string? symbol) ? symbol : null;
    }

    private static GoCgoImportDynamicAttribute? FindRecord(MethodInfo method)
    {
        Assembly? assembly = method.DeclaringType?.Assembly;

        if (assembly is null)
        {
            return null;
        }

        foreach (GoCgoImportDynamicAttribute record in
                 assembly.GetCustomAttributes<GoCgoImportDynamicAttribute>())
        {
            if (string.Equals(record.TrampolineName, method.Name, StringComparison.Ordinal))
            {
                return record;
            }
        }

        return null;
    }
}
