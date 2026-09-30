// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using path = path_package;

partial class fs_package {

// A GlobFS is a file system with a Glob method.
[GoType] partial interface GlobFS :
    FS
{
    // Glob returns the names of all files matching pattern,
    // providing an implementation of the top-level
    // Glob function.
    (slice<@string>, error) Glob(@string pattern);
}

// Glob returns the names of all files matching pattern or nil
// if there is no matching file. The syntax of patterns is the same
// as in [path.Match]. The pattern may describe hierarchical names such as
// usr/*/bin/ed.
//
// Glob ignores file system errors such as I/O errors reading directories.
// The only possible returned error is [path.ErrBadPattern], reporting that
// the pattern is malformed.
//
// If fs implements [GlobFS], Glob calls fs.Glob.
// Otherwise, Glob uses [ReadDir] to traverse the directory tree
// and look for matches for the pattern.
public static (slice<@string> matches, error err) Glob(FS fsys, @string pattern) {
    return globWithLimit(fsys, pattern, 0);
}

internal static (slice<@string> matches, error err) globWithLimit(FS fsys, @string pattern, nint depth) {
    slice<@string> matches = default!;
    error err = default!;

    // This limit is added to prevent stack exhaustion issues. See
    // CVE-2022-30630.
    const nint pathSeparatorsLimit = 10000;
    if (depth > pathSeparatorsLimit) {
        return (default!, path.ErrBadPattern);
    }
    {
        var (fsysΔ1, ok) = fsys._<GlobFS>(ᐧ); if (ok) {
            return fsysΔ1.Glob(pattern);
        }
    }
    // Check pattern is well-formed.
    {
        var (_, errΔ1) = path.Match(pattern, ""u8); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (!hasMeta(pattern)) {
        {
            (_, err) = Stat(fsys, pattern); if (err != default!) {
                return (default!, default!);
            }
        }
        return (new @string[]{pattern}.slice(), default!);
    }
    var (dir, @file) = path.Split(pattern);
    dir = cleanGlobPath(dir);
    if (!hasMeta(dir)) {
        return glob(fsys, dir, @file, default!);
    }
    // Prevent infinite recursion. See issue 15879.
    if (dir == pattern) {
        return (default!, path.ErrBadPattern);
    }
    slice<@string> m = default!;
    (m, err) = globWithLimit(fsys, dir, depth + 1);
    if (err != default!) {
        return (default!, err);
    }
    foreach (var (_, d) in m) {
        (matches, err) = glob(fsys, d, @file, matches);
        if (err != default!) {
            return (matches, err);
        }
    }
    return (matches, err);
}

// cleanGlobPath prepares path for glob matching.
internal static @string cleanGlobPath(@string path) {
    var exprᴛ1 = path;
    if (exprᴛ1 == ""u8) {
        return "."u8;
    }
    { /* default: */
        return path.slice(0, len(path) - 1); // chop off trailing separator
    }

}

// glob searches for files matching pattern in the directory dir
// and appends them to matches, returning the updated slice.
// If the directory cannot be opened, glob returns the existing matches.
// New matches are added in lexicographical order.
internal static (slice<@string> m, error e) glob(FS fs, @string dir, @string pattern, slice<@string> matches) {
    slice<@string> m = default!;
    error e = default!;

    m = matches;
    var (infos, err) = ReadDir(fs, dir);
    if (err != default!) {
        return (m, e); // ignore I/O error
    }
    foreach (var (_, info) in infos) {
        @string n = info.Name();
        var (matched, errΔ1) = path.Match(pattern, n);
        if (errΔ1 != default!) {
            return (m, errΔ1);
        }
        if (matched) {
            m = append(m, path.Join(dir, n));
        }
    }
    return (m, e);
}

// hasMeta reports whether path contains any of the magic characters
// recognized by path.Match.
internal static bool hasMeta(@string path) {
    for (nint i = 0; i < len(path); i++) {
        switch (path[i]) {
        case (rune)'*' or (rune)'?' or (rune)'[' or (rune)'\\': {
            return true;
        }}

    }
    return false;
}

} // end fs_package
