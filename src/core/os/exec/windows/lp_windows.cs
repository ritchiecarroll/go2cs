// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using errors = errors_package;
using fs = go.io.fs_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using @internal;
using go.io;
using iter = iter_package;
using path;

partial class exec_package {

// ErrNotFound is the error resulting if a path search failed to find an executable file.
public static error ErrNotFound = errors.New("executable file not found in %PATH%"u8);

internal static error chkStat(@string @file) {
    var (d, err) = os.Stat(@file);
    if (err != default!) {
        return err;
    }
    if (d.IsDir()) {
        return fs.ErrPermission;
    }
    return default!;
}

internal static bool hasExt(@string @file) {
    nint i = strings.LastIndex(@file, "."u8);
    if (i < 0) {
        return false;
    }
    return strings.LastIndexAny(@file, @":\/"u8) < i;
}

internal static (@string, error) findExecutable(@string @file, slice<@string> exts) {
    if (len(exts) == 0) {
        return (@file, chkStat(@file));
    }
    if (hasExt(@file)) {
        if (chkStat(@file) == default!) {
            return (@file, default!);
        }
        // Keep checking exts below, so that programs with weird names
        // like "foo.bat.exe" will resolve instead of failing.
    }
    foreach (var (_, e) in exts) {
        {
            @string f = @file + e; if (chkStat(f) == default!) {
                return (f, default!);
            }
        }
    }
    if (hasExt(@file)) {
        return ("", fs.ErrNotExist);
    }
    return ("", ErrNotFound);
}

internal static (@string, error) lookPath(@string @file) {
    {
        var err = validateLookPath(@file); if (err != default!) {
            return ("", new ΔErrorжerror(Ꮡ(new ΔError(@file, err))));
        }
    }

    return lookPathExts(@file, pathExt());
}

// lookExtensions finds windows executable by its dir and path.
// It uses LookPath to try appropriate extensions.
// lookExtensions does not search PATH, instead it converts `prog` into `.\prog`.
//
// If the path already has an extension found in PATHEXT,
// lookExtensions returns it directly without searching
// for additional extensions. For example,
// "C:\foo\example.com" would be returned as-is even if the
// program is actually "C:\foo\example.com.exe".
internal static (@string, error) lookExtensions(@string path, @string dir) {
    {
        var errΔ1 = validateLookPath(path); if (errΔ1 != default!) {
            return ("", new ΔErrorжerror(Ꮡ(new ΔError(path, errΔ1))));
        }
    }

    if (filepath.Base(path) == path) {
        path = "."u8 + ((@string)(rune)filepath.Separator) + path;
    }
    var exts = pathExt();
    {
        @string extΔ1 = filepath.Ext(path); if (extΔ1 != ""u8) {
            foreach (var (_, e) in exts) {
                if (strings.EqualFold(extΔ1, e)) {
                    // Assume that path has already been resolved.
                    return (path, default!);
                }
            }
        }
    }
    if (dir == ""u8) {
        return lookPathExts(path, exts);
    }
    if (filepath.VolumeName(path) != ""u8) {
        return lookPathExts(path, exts);
    }
    if (len(path) > 1 && os.IsPathSeparator(path[0])) {
        return lookPathExts(path, exts);
    }
    @string dirandpath = filepath.Join(dir, path);
    // We assume that LookPath will only add file extension.
    var (lp, err) = lookPathExts(dirandpath, exts);
    if (err != default!) {
        return ("", err);
    }
    @string ext = strings.TrimPrefix(lp, dirandpath);
    return (path + ext, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathextˢ = @"PATHEXT"u8;

internal static slice<@string> pathExt() {
    slice<@string> exts = default!;
    @string x = os.Getenv(pathextˢ);
    if (x != ""u8){
        foreach (var e in range<@string>(strings.SplitSeq(strings.ToLower(x), @";"u8).Invoke)) {
            if (e == ""u8) {
                continue;
            }
            if (e[0] != (rune)'.') {
                e = "."u8 + e;
            }
            exts = append(exts, e);
        }
    } else {
        exts = new @string[]{".com"u8, ".exe"u8, ".bat"u8, ".cmd"u8}.slice();
    }
    return exts;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pathˢ = "path"u8;

// lookPathExts implements LookPath for the given PATHEXT list.
internal static (@string, error) lookPathExts(@string @file, slice<@string> exts) {
    if (strings.ContainsAny(@file, @":\/"u8)) {
        var (f, err) = findExecutable(@file, exts);
        if (err == default!) {
            return (f, default!);
        }
        return ("", new ΔErrorжerror(Ꮡ(new ΔError(@file, err))));
    }

    // On Windows, creating the NoDefaultCurrentDirectoryInExePath
    // environment variable (with any value or no value!) signals that
    // path lookups should skip the current directory.
    // In theory we are supposed to call NeedCurrentDirectoryForExePathW
    // "as the registry location of this environment variable can change"
    // but that seems exceedingly unlikely: it would break all users who
    // have configured their environment this way!
    // https://docs.microsoft.com/en-us/windows/win32/api/processenv/nf-processenv-needcurrentdirectoryforexepathw
    // See also go.dev/issue/43947.
    @string dotf = default!;
    error dotErr = default!;
    {
        var (_, found) = os.LookupEnv("NoDefaultCurrentDirectoryInExePath"u8); if (!found) {
            {
                var (f, err) = findExecutable(filepath.Join("."u8, @file), exts); if (err == default!) {
                    if (execerrdot.Value() == "0"u8) {
                        execerrdot.IncNonDefault();
                        return (f, default!);
                    }
                    (dotf, dotErr) = (f, new ΔErrorжerror(Ꮡ(new ΔError(@file, ErrDot))));
                }
            }
        }
    }

    @string path = os.Getenv(pathˢ);
    foreach (var (_, dir) in filepath.SplitList(path)) {
        if (dir == ""u8) {
            // Skip empty entries, consistent with what PowerShell does.
            // (See https://go.dev/issue/61493#issuecomment-1649724826.)
            continue;
        }

        {
            var (f, err) = findExecutable(filepath.Join(dir, @file), exts); if (err == default!) {
                if (dotErr != default!) {
                    // https://go.dev/issue/53536: if we resolved a relative path implicitly,
                    // and it is the same executable that would be resolved from the explicit %PATH%,
                    // prefer the explicit name for the executable (and, likely, no error) instead
                    // of the equivalent implicit name with ErrDot.
                    //
                    // Otherwise, return the ErrDot for the implicit path as soon as we find
                    // out that the explicit one doesn't match.
                    var (dotfi, dotfiErr) = os.Lstat(dotf);
                    var (fi, fiErr) = os.Lstat(f);
                    if (dotfiErr != default! || fiErr != default! || !os.SameFile(dotfi, fi)) {
                        return (dotf, dotErr);
                    }
                }

                if (!filepath.IsAbs(f)) {
                    if (execerrdot.Value() != "0"u8) {
                        // If this is the same relative path that we already found,
                        // dotErr is non-nil and we already checked it above.
                        // Otherwise, record this path as the one to which we must resolve,
                        // with or without a dotErr.
                        if (dotErr == default!) {
                            (dotf, dotErr) = (f, new ΔErrorжerror(Ꮡ(new ΔError(@file, ErrDot))));
                        }
                        continue;
                    }
                    execerrdot.IncNonDefault();
                }
                return (f, default!);
            }
        }
    }

    if (dotErr != default!) {
        return (dotf, dotErr);
    }
    return ("", new ΔErrorжerror(Ꮡ(new ΔError(@file, ErrNotFound))));
}

} // end exec_package
