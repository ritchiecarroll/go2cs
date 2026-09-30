// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build gc
namespace go.@internal;

using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = go.sync_package;
using fs = io.fs_package;
using go;
using go.os;
using io;
using path;

partial class goroot_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string srcˢ = "src"u8;

// IsStandardPackage reports whether path is a standard package,
// given goroot and compiler.
public static bool IsStandardPackage(@string goroot, @string compiler, @string path) {
    var exprᴛ1 = compiler;
    if (exprᴛ1 == "gc"u8) {
        @string dir = filepath.Join(goroot, srcˢ, path);
        var (dirents, err) = os.ReadDir(dir);
        if (err != default!) {
            return false;
        }
        foreach (var (_, dirent) in dirents) {
            if (strings.HasSuffix(dirent.Name(), ".go"u8)) {
                return true;
            }
        }
        return false;
    }
    if (exprᴛ1 == "gccgo"u8) {
        return ᏑgccgoSearch.isStandard(path);
    }
    { /* default: */
        throw panic("unknown compiler " + compiler);
    }

}

// gccgoSearch holds the gccgo search directories.
[GoType] partial struct gccgoDirs {
    internal sync.Once once;
    internal slice<@string> dirs;
}

// gccgoSearch is used to check whether a gccgo package exists in the
// standard library.
internal static ж<gccgoDirs> ᏑgccgoSearch = new StandardBox<gccgoDirs>(default(gccgoDirs));
internal static ref gccgoDirs gccgoSearch => ref ᏑgccgoSearch.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string gccgoˢ = "GCCGO"u8;
private static readonly @string gccgoˢ2 = "gccgo"u8;
private static readonly @string printSearchDirsˢ = "-print-search-dirs"u8;
private static readonly @string dumpversionˢ = "-dumpversion"u8;
private static readonly @string dumpmachineˢ = "-dumpmachine"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string prefixᶜ = "libraries: ="u8;

// init finds the gccgo search directories. If this fails it leaves dirs == nil.
[GoRecv] internal static void init(this ref gccgoDirs gd) {
    @string gccgo = os.Getenv(gccgoˢ);
    if (gccgo == ""u8) {
        gccgo = gccgoˢ2;
    }
    var (bin, err) = exec.LookPath(gccgo);
    if (err != default!) {
        return;
    }
    (var allDirs, err) = exec.Command(bin, printSearchDirsˢ).Output();
    if (err != default!) {
        return;
    }
    (var versionB, err) = exec.Command(bin, dumpversionˢ).Output();
    if (err != default!) {
        return;
    }
    @string version = strings.TrimSpace(((@string)versionB));
    (var machineB, err) = exec.Command(bin, dumpmachineˢ).Output();
    if (err != default!) {
        return;
    }
    @string machine = strings.TrimSpace(((@string)machineB));
    var dirsEntries = strings.Split(((@string)allDirs), "\n"u8);
    @string prefix = prefixᶜ;
    slice<@string> dirs = default!;
    foreach (var (_, dirEntry) in dirsEntries) {
        if (strings.HasPrefix(dirEntry, prefix)) {
            dirs = filepath.SplitList(strings.TrimPrefix(dirEntry, prefix));
            break;
        }
    }
    if (len(dirs) == 0) {
        return;
    }
    slice<@string> lastDirs = default!;
    foreach (var (_, dir) in dirs) {
        @string goDir = filepath.Join(dir, "go", version);
        {
            var (fi, errΔ1) = os.Stat(goDir); if (errΔ1 == default! && fi.IsDir()) {
                gd.dirs = append(gd.dirs, goDir);
                goDir = filepath.Join(goDir, machine);
                {
                    (fi, errΔ1) = os.Stat(goDir); if (errΔ1 == default! && fi.IsDir()) {
                        gd.dirs = append(gd.dirs, goDir);
                    }
                }
            }
        }
        {
            var (fi, errΔ2) = os.Stat(dir); if (errΔ2 == default! && fi.IsDir()) {
                lastDirs = append(lastDirs, dir);
            }
        }
    }
    gd.dirs = appendꓸꓸꓸ(gd.dirs, lastDirs);
}

// isStandard reports whether path is a standard library for gccgo.
internal static bool isStandard(this ж<gccgoDirs> Ꮡgd, @string path) {
    ref var gd = ref Ꮡgd.DerefOrNull();

    // Quick check: if the first path component has a '.', it's not
    // in the standard library. This skips most GOPATH directories.
    nint i = strings.Index(path, "/"u8);
    if (i < 0) {
        i = len(path);
    }
    if (strings.Contains(path.slice(0, i), "."u8)) {
        return false;
    }
    if (path == "unsafe"u8) {
        // Special case.
        return true;
    }
    Ꮡgd.of(gccgoDirs.Ꮡonce).Do(Ꮡgd.init);
    if (gd.dirs == default!) {
        // We couldn't find the gccgo search directories.
        // Best guess, since the first component did not contain
        // '.', is that this is a standard library package.
        return true;
    }
    foreach (var (_, dir) in gd.dirs) {
        @string full = filepath.Join(dir, path) + ".gox"u8;
        {
            var (fi, err) = os.Stat(full); if (err == default! && !fi.IsDir()) {
                return true;
            }
        }
    }
    return false;
}

} // end goroot_package
