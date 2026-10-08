// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using bufio = bufio_package;
using types = global::go.go.types_package;
using os = os_package;
using exec = global::go.os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using fs = global::go.io.fs_package;
using global::go.go;
using global::go.io;
using global::go.os;
using io = io_package;
using path;
using ꓸꓸꓸstring = Span<@string>;

partial class gccgoimporter_package {

// Information about a specific installation of gccgo.
partial struct GccgoInstallation {
    // Version of gcc (e.g. 4.8.0).
    public @string GccVersion;
    // Target triple (e.g. x86_64-unknown-linux-gnu).
    public @string TargetTriple;
    // Built-in library paths used by this installation.
    public slice<@string> LibPaths;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string targetˢ = "Target: "u8;

// Ask the driver at the given path for information for this GccgoInstallation.
// The given arguments are passed directly to the call of the driver.
public static error /*err*/ InitFromDriver(this ref GccgoInstallation inst, @string gccgoPath, params ꓸꓸꓸstring argsʗp) {
    error err = default!;
    var args = argsʗp.sslice();

    var argv = appendꓸꓸꓸ(new @string[]{"-###"u8, "-S"u8, "-x"u8, "go"u8, "-"u8}.slice(), args);
    var cmd = exec.Command(gccgoPath, argv.ꓸꓸꓸ);
    (var stderr, err) = cmd.StderrPipe();
    if (err != default!) {
        return err;
    }
    err = cmd.Start();
    if (err != default!) {
        return err;
    }
    var scanner = bufio.NewScanner(stderr);
    while (scanner.Scan()) {
        @string line = scanner.Text();
        switch (ᐧ) {
        case {} when strings.HasPrefix(line, targetˢ): {
            inst.TargetTriple = line[8..];
            break;
        }
        case {} when line[0] is (rune)' ': {
            var argsΔ2 = strings.Fields(line);
            foreach (var (_, arg) in argsΔ2[1..]) {
                if (strings.HasPrefix(arg, "-L"u8)) {
                    inst.LibPaths = append(inst.LibPaths, arg[2..]);
                }
            }
            break;
        }}

    }
    argv = appendꓸꓸꓸ(new @string[]{"-dumpversion"u8}.slice(), args);
    (var stdout, err) = exec.Command(gccgoPath, argv.ꓸꓸꓸ).Output();
    if (err != default!) {
        return err;
    }
    inst.GccVersion = strings.TrimSpace(((@string)stdout));
    return err;
}

// Return the list of export search paths for this GccgoInstallation.
public static slice<@string> /*paths*/ SearchPaths(this ref GccgoInstallation inst) {
    slice<@string> paths = default!;

    foreach (var (_, lpath) in inst.LibPaths) {
        @string spath = filepath.Join(lpath, "go", inst.GccVersion);
        var (fi, err) = os.Stat(spath);
        if (err != default! || !fi.IsDir()) {
            continue;
        }
        paths = append(paths, spath);
        spath = filepath.Join(spath, inst.TargetTriple);
        (fi, err) = os.Stat(spath);
        if (err != default! || !fi.IsDir()) {
            continue;
        }
        paths = append(paths, spath);
    }
    paths = appendꓸꓸꓸ(paths, inst.LibPaths);
    return paths;
}

// Return an importer that searches incpaths followed by the gcc installation's
// built-in search paths and the current directory.
public static Func<map<@string, ж<types.Package>>, @string, @string, Func<@string, (io.ReadCloser, error)>, (ж<types.Package>, error)> GetImporter(this ref GccgoInstallation inst, slice<@string> incpaths, map<ж<types.Package>, InitData> initmap) {
    return GetImporter(append(appendꓸꓸꓸ(incpaths, inst.SearchPaths()), "."u8), initmap);
}

} // end gccgoimporter_package
