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
    public static void Register(Assembly assembly, params GoCgoLibrary[] libraries)
    {
        List<GoCgoLibrary> owned = [.. libraries];
        NativeLibrary.SetDllImportResolver(assembly, (name, importing, _) => Resolve(name, importing, owned));
    }

    /// <summary>
    /// The handle for a library the table owns, loaded as §6.2 first wrote it: the runtime's default search, and 0 on a
    /// miss, which leaves the runtime to probe on.
    /// </summary>
    internal static nint Resolve(string name, Assembly assembly, IReadOnlyList<GoCgoLibrary> libraries)
    {
        foreach (GoCgoLibrary library in libraries)
        {
            if (library.Name != name)
                continue;

            return NativeLibrary.TryLoad(name, assembly, null, out nint handle) ? handle : 0;
        }

        return 0;
    }
}
