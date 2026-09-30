// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using os = os_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using @internal;
using fs = io.fs_package;
using io;
using unicode;

partial class filepath_package {

// ErrBadPattern indicates a pattern was malformed.
public static error ErrBadPattern = errors.New("syntax error in pattern"u8);

// Match reports whether name matches the shell file name pattern.
// The pattern syntax is:
//
//	pattern:
//		{ term }
//	term:
//		'*'         matches any sequence of non-Separator characters
//		'?'         matches any single non-Separator character
//		'[' [ '^' ] { character-range } ']'
//		            character class (must be non-empty)
//		c           matches character c (c != '*', '?', '\\', '[')
//		'\\' c      matches character c
//
//	character-range:
//		c           matches character c (c != '\\', '-', ']')
//		'\\' c      matches character c
//		lo '-' hi   matches character c for lo <= c <= hi
//
// Match requires pattern to match all of name, not just a substring.
// The only possible returned error is [ErrBadPattern], when pattern
// is malformed.
//
// On Windows, escaping is disabled. Instead, '\\' is treated as
// path separator.
public static (bool matched, error err) Match(@string pattern, @string name) {
Pattern:
    while (len(pattern) > 0) {
        bool star = default!;
        @string chunk = default!;
        (star, chunk, pattern) = scanChunk(pattern);
        if (star && chunk == ""u8) {
            // Trailing * matches rest of string unless it has a /.
            return (!strings.Contains(name, ((@string)(rune)Separator)), default!);
        }
        // Look for match at current position.
        var (t, ok, errΔ1) = matchChunk(chunk, name);
        // if we're the last chunk, make sure we've exhausted the name
        // otherwise we'll give a false result even if we could still match
        // using the star
        if (ok && (len(t) == 0 || len(pattern) > 0)) {
            name = t;
            continue;
        }
        if (errΔ1 != default!) {
            return (false, errΔ1);
        }
        if (star) {
            // Look for match skipping i+1 bytes.
            // Cannot skip /.
            for (nint i = 0; i < len(name) && name[i] != Separator; i++) {
                var (tΔ1, okΔ1, errΔ2) = matchChunk(chunk, name.slice(i + 1));
                if (okΔ1) {
                    // if we're the last chunk, make sure we exhausted the name
                    if (len(pattern) == 0 && len(tΔ1) > 0) {
                        continue;
                    }
                    name = tΔ1;
                    goto continue_Pattern;
                }
                if (errΔ2 != default!) {
                    return (false, errΔ2);
                }
            }
        }
        return (false, default!);
continue_Pattern:;
    }
break_Pattern:;
    return (len(name) == 0, default!);
}

// scanChunk gets the next segment of pattern, which is a non-star string
// possibly preceded by a star.
internal static (bool star, @string chunk, @string rest) scanChunk(@string pattern) {
    bool star = default!;

    while (len(pattern) > 0 && pattern[0] == (rune)'*') {
        pattern = pattern[1..];
        star = true;
    }
    var inrange = false;
    nint i = default!;
Scan:
    for (i = 0; i < len(pattern); i++) {
        switch (pattern[i]) {
        case (rune)'\\': {
            if (runtime.GOOS != "windows"u8) {
                // error check handled in matchChunk: bad pattern.
                if (i + 1 < len(pattern)) {
                    i++;
                }
            }
            break;
        }
        case (rune)'[': {
            inrange = true;
            break;
        }
        case (rune)']': {
            inrange = false;
            break;
        }
        case (rune)'*': {
            if (!inrange) {
                goto break_Scan;
            }
            break;
        }}

continue_Scan:;
    }
break_Scan:;
    return (star, pattern.slice(0, i), pattern.slice(i));
}

// matchChunk checks whether chunk matches the beginning of s.
// If so, it returns the remainder of s (after the match).
// Chunk is all single-character operators: literals, char classes, and ?.
internal static (@string rest, bool ok, error err) matchChunk(@string chunk, @string s) {
    error err = default!;

    // failed records whether the match has failed.
    // After the match fails, the loop continues on processing chunk,
    // checking that the pattern is well-formed but no longer reading s.
    var failed = false;
    while (len(chunk) > 0) {
        if (!failed && len(s) == 0) {
            failed = true;
        }
        var exprᴛ1 = chunk[0];
        var matchᴛ1 = false;
        if (exprᴛ1 is (rune)'[') { matchᴛ1 = true;
            // character class
            rune r = default!;
            if (!failed) {
                nint n = default!;
                (r, n) = utf8.DecodeRuneInString(s);
                s = s.slice(n);
            }
            chunk = chunk[1..];
            var negated = false;
            if (len(chunk) > 0 && chunk[0] == (rune)'^') {
                // possibly negated
                negated = true;
                chunk = chunk[1..];
            }
            var match = false;
            nint nrange = 0;
            while (ᐧ) {
                // parse all ranges
                if (len(chunk) > 0 && chunk[0] == (rune)']' && nrange > 0) {
                    chunk = chunk[1..];
                    break;
                }
                rune loΔ1 = default!;
                rune hiΔ1 = default!;
                {
                    (loΔ1, chunk, err) = getEsc(chunk); if (err != default!) {
                        return ("", false, err);
                    }
                }
                hiΔ1 = loΔ1;
                if (chunk[0] == (rune)'-') {
                    {
                        (hiΔ1, chunk, err) = getEsc(chunk[1..]); if (err != default!) {
                            return ("", false, err);
                        }
                    }
                }
                if (loΔ1 <= r && r <= hiΔ1) {
                    match = true;
                }
                nrange++;
            }
            if (match == negated) {
                failed = true;
            }
        }
        else if (exprᴛ1 is (rune)'?') { matchᴛ1 = true;
            if (!failed) {
                if (s[0] == Separator) {
                    failed = true;
                }
                var (_, n) = utf8.DecodeRuneInString(s);
                s = s.slice(n);
            }
            chunk = chunk[1..];
        }
        else if (exprᴛ1 is (rune)'\\') { matchᴛ1 = true;
            if (runtime.GOOS != "windows"u8) {
                chunk = chunk[1..];
                if (len(chunk) == 0) {
                    return ("", false, ErrBadPattern);
                }
            }
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            if (!failed) {
                if (chunk[0] != s[0]) {
                    failed = true;
                }
                s = s[1..];
            }
            chunk = chunk[1..];
        }

    }
    if (failed) {
        return ("", false, default!);
    }
    return (s, true, default!);
}

// getEsc gets a possibly-escaped character from chunk, for a character class.
internal static (rune r, @string nchunk, error err) getEsc(@string chunk) {
    rune r = default!;
    @string nchunk = default!;
    error err = default!;

    if (len(chunk) == 0 || chunk[0] == (rune)'-' || chunk[0] == (rune)']') {
        err = ErrBadPattern;
        return (r, nchunk, err);
    }
    if (chunk[0] == (rune)'\\' && runtime.GOOS != "windows"u8) {
        chunk = chunk[1..];
        if (len(chunk) == 0) {
            err = ErrBadPattern;
            return (r, nchunk, err);
        }
    }
    (r, var n) = utf8.DecodeRuneInString(chunk);
    if (r == utf8.RuneError && n == 1) {
        err = ErrBadPattern;
    }
    nchunk = chunk.slice(n);
    if (len(nchunk) == 0) {
        err = ErrBadPattern;
    }
    return (r, nchunk, err);
}

// Glob returns the names of all files matching pattern or nil
// if there is no matching file. The syntax of patterns is the same
// as in [Match]. The pattern may describe hierarchical names such as
// /usr/*/bin/ed (assuming the [Separator] is '/').
//
// Glob ignores file system errors such as I/O errors reading directories.
// The only possible returned error is [ErrBadPattern], when pattern
// is malformed.
public static (slice<@string> matches, error err) Glob(@string pattern) {
    return globWithLimit(pattern, 0);
}

internal static (slice<@string> matches, error err) globWithLimit(@string pattern, nint depth) {
    slice<@string> matches = default!;
    error err = default!;

    // This limit is used prevent stack exhaustion issues. See CVE-2022-30632.
    const nint pathSeparatorsLimit = 10000;
    if (depth == pathSeparatorsLimit) {
        return (default!, ErrBadPattern);
    }
    // Check pattern is well-formed.
    {
        var (_, errΔ1) = Match(pattern, ""u8); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (!hasMeta(pattern)) {
        {
            (_, err) = os.Lstat(pattern); if (err != default!) {
                return (default!, default!);
            }
        }
        return (new @string[]{pattern}.slice(), default!);
    }
    var (dir, @file) = Split(pattern);
    nint volumeLen = 0;
    if (runtime.GOOS == "windows"u8){
        (volumeLen, dir) = cleanGlobPathWindows(dir);
    } else {
        dir = cleanGlobPath(dir);
    }
    if (!hasMeta(dir.slice(volumeLen))) {
        return glob(dir, @file, default!);
    }
    // Prevent infinite recursion. See issue 15879.
    if (dir == pattern) {
        return (default!, ErrBadPattern);
    }
    slice<@string> m = default!;
    (m, err) = globWithLimit(dir, depth + 1);
    if (err != default!) {
        return (matches, err);
    }
    foreach (var (_, d) in m) {
        (matches, err) = glob(d, @file, matches);
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
    if (exprᴛ1 == ((@string)(rune)Separator)) {
        return path;
    }
    { /* default: */
        return path.slice(0, len(path) - 1); // chop off trailing separator
    }

}

// do nothing to the path

// cleanGlobPathWindows is windows version of cleanGlobPath.
internal static (nint prefixLen, @string cleaned) cleanGlobPathWindows(@string path) {
    nint vollen = filepathlite.VolumeNameLen(path);
    switch (ᐧ) {
    case {} when path == ""u8: {
        return (0, ".");
    }
    case {} when vollen + 1 == len(path) && os.IsPathSeparator(path[len(path) - 1]): {
        return (vollen + 1, path);
    }
    case {} when vollen == len(path) && len(path) == 2: {
        return (vollen, path + "."); // convert C: into C:.
    }
    default: {
        if (vollen >= len(path)) {
            // /, \, C:\ and C:/
            // do nothing to the path
            // C:
            vollen = len(path) - 1;
        }
        return (vollen, path.slice(0, len(path) - 1)); // chop off trailing separator
    }}

}

// glob searches for files matching pattern in the directory dir
// and appends them to matches. If the directory cannot be
// opened, it returns the existing matches. New matches are
// added in lexicographical order.
internal static (slice<@string> m, error e) glob(@string dir, @string pattern, slice<@string> matches) {
    slice<@string> m = default!;
    error e = default!;
    GoFrame ᒐ = default;
    try {
        m = matches;
        var (fi, err) = os.Stat(dir);
        if (err != default!) {
            goto ᒐdone; // ignore I/O error
        }
        if (!fi.IsDir()) {
            goto ᒐdone; // ignore I/O error
        }
        (var d, err) = os.Open(dir);
        if (err != default!) {
            goto ᒐdone; // ignore I/O error
        }
        var dʗ1 = d;
        defer(() => dʗ1.Close(), ref ᒐ);
        var (names, _) = d.Readdirnames(-1);
        slices.Sort<slice<@string>, @string>(names);
        foreach (var (_, n) in names) {
            var (matched, errΔ1) = Match(pattern, n);
            if (errΔ1 != default!) {
                (m, e) = (m, errΔ1); goto ᒐdone;
            }
            if (matched) {
                m = append(m, Join(dir, n));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (m, e);
}

// hasMeta reports whether path contains any of the magic characters
// recognized by Match.
internal static bool hasMeta(@string path) {
    @string magicChars = @"*?["u8;
    if (runtime.GOOS != "windows"u8) {
        magicChars = @"*?[\"u8;
    }
    return strings.ContainsAny(path, magicChars);
}

} // end filepath_package
