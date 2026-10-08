// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package path implements utility routines for manipulating slash-separated
// paths.
//
// The path package should only be used for paths separated by forward
// slashes, such as the paths in URLs. This package does not deal with
// Windows paths with drive letters or backslashes; to manipulate
// operating system paths, use the [path/filepath] package.
namespace go;

using bytealg = @internal.bytealg_package;
using @internal;
using ꓸꓸꓸstring = Span<@string>;

partial class path_package {

// A lazybuf is a lazily constructed path buffer.
// It supports append, reading previously appended bytes,
// and retrieving the final string. It does not allocate a buffer
// to hold the output until that output diverges from s.
partial struct lazybuf {
    internal @string s;
    internal slice<byte> buf;
    internal nint w;
}

internal static byte index(this ref lazybuf b, nint i) {
    if (b.buf != default!) {
        return b.buf[i];
    }
    return b.s[i];
}

internal static void append(this ref lazybuf b, byte c) {
    if (b.buf == default!) {
        if (b.w < len(b.s) && b.s[b.w] == c) {
            b.w++;
            return;
        }
        b.buf = new slice<byte>(len(b.s));
        copy(b.buf, b.s.slice(0, b.w));
    }
    b.buf[b.w] = c;
    b.w++;
}

internal static @string @string(this ref lazybuf b) {
    if (b.buf == default!) {
        return b.s.slice(0, b.w);
    }
    return ((@string)(b.buf.slice(0, b.w)));
}

// Clean returns the shortest path name equivalent to path
// by purely lexical processing. It applies the following rules
// iteratively until no further processing can be done:
//
//  1. Replace multiple slashes with a single slash.
//  2. Eliminate each . path name element (the current directory).
//  3. Eliminate each inner .. path name element (the parent directory)
//     along with the non-.. element that precedes it.
//  4. Eliminate .. elements that begin a rooted path:
//     that is, replace "/.." by "/" at the beginning of a path.
//
// The returned path ends in a slash only if it is the root "/".
//
// If the result of this process is an empty string, Clean
// returns the string ".".
//
// See also Rob Pike, “Lexical File Names in Plan 9 or
// Getting Dot-Dot Right,”
// https://9p.io/sys/doc/lexnames.html
public static @string Clean(@string path) {
    if (path == ""u8) {
        return "."u8;
    }
    var rooted = path[0] == (rune)'/';
    nint n = len(path);
    // Invariants:
    //	reading from path; r is index of next byte to process.
    //	writing to buf; w is index of next byte to write.
    //	dotdot is index in buf where .. must stop, either because
    //		it is the leading slash or it is a leading ../../.. prefix.
    var @out = new lazybuf(s: path);
    nint r = 0;
    nint dotdot = 0;
    if (rooted) {
        @out.append((rune)'/');
        (r, dotdot) = (1, 1);
    }
    while (r < n) {
        switch (ᐧ) {
        case {} when path[r] is (rune)'/': {
            r++;
            break;
        }
        case {} when path[r] == (rune)'.' && (r + 1 == n || path[r + 1] == (rune)'/'): {
            r++;
            break;
        }
        case {} when path[r] == (rune)'.' && path[r + 1] == (rune)'.' && (r + 2 == n || path[r + 2] == (rune)'/'): {
            r += 2;
            switch (ᐧ) {
            case {} when @out.w > dotdot: {
                @out.w--;
                while (@out.w > dotdot && @out.index(@out.w) != (rune)'/') {
                    // empty path element
                    // . element
                    // .. element: remove to last /
                    // can backtrack
                    @out.w--;
                }
                break;
            }
            case {} when !rooted: {
                if (@out.w > 0) {
                    // cannot backtrack, but not rooted, so append .. element.
                    @out.append((rune)'/');
                }
                @out.append((rune)'.');
                @out.append((rune)'.');
                dotdot = @out.w;
                break;
            }}

            break;
        }
        default: {
            if (rooted && @out.w != 1 || !rooted && @out.w != 0) {
                // real path element.
                // add slash if needed
                @out.append((rune)'/');
            }
            for (; r < n && path[r] != (rune)'/'; r++) {
                // copy element
                @out.append(path[r]);
            }
            break;
        }}

    }
    // Turn empty string into "."
    if (@out.w == 0) {
        return "."u8;
    }
    return @out.@string();
}

// Split splits path immediately following the final slash,
// separating it into a directory and file name component.
// If there is no slash in path, Split returns an empty dir and
// file set to path.
// The returned values have the property that path = dir+file.
public static (@string dir, @string @file) Split(@string path) {
    nint i = bytealg.LastIndexByteString(path, (rune)'/');
    return (path.slice(0, i + 1), path.slice(i + 1));
}

// Join joins any number of path elements into a single path,
// separating them with slashes. Empty elements are ignored.
// The result is Cleaned. However, if the argument list is
// empty or all its elements are empty, Join returns
// an empty string.
public static @string Join(params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.sslice();

    nint size = 0;
    foreach (var (_, e) in elem) {
        size += len(e);
    }
    if (size == 0) {
        return ""u8;
    }
    var buf = new slice<byte>(0, size + len(elem) - 1);
    foreach (var (_, e) in elem) {
        if (len(buf) > 0 || e != ""u8) {
            if (len(buf) > 0) {
                buf = builtin.append(buf, (byte)((rune)'/'));
            }
            buf = builtin.append(buf, e.ꓸꓸꓸ);
        }
    }
    return Clean(((@string)buf));
}

// Ext returns the file name extension used by path.
// The extension is the suffix beginning at the final dot
// in the final slash-separated element of path;
// it is empty if there is no dot.
public static @string Ext(@string path) {
    for (nint i = len(path) - 1; i >= 0 && path[i] != (rune)'/'; i--) {
        if (path[i] == (rune)'.') {
            return path.slice(i);
        }
    }
    return ""u8;
}

// Base returns the last element of path.
// Trailing slashes are removed before extracting the last element.
// If the path is empty, Base returns ".".
// If the path consists entirely of slashes, Base returns "/".
public static @string Base(@string path) {
    if (path == ""u8) {
        return "."u8;
    }
    // Strip trailing slashes.
    while (len(path) > 0 && path[len(path) - 1] == (rune)'/') {
        path = path.slice(0, len(path) - 1);
    }
    // Find the last element
    {
        nint i = bytealg.LastIndexByteString(path, (rune)'/'); if (i >= 0) {
            path = path.slice(i + 1);
        }
    }
    // If empty now, it had only slashes.
    if (path == ""u8) {
        return "/"u8;
    }
    return path;
}

// IsAbs reports whether the path is absolute.
public static bool IsAbs(@string path) {
    return len(path) > 0 && path[0] == (rune)'/';
}

// Dir returns all but the last element of path, typically the path's directory.
// After dropping the final element using [Split], the path is Cleaned and trailing
// slashes are removed.
// If the path is empty, Dir returns ".".
// If the path consists entirely of slashes followed by non-slash bytes, Dir
// returns a single slash. In any other case, the returned path does not end in a
// slash.
public static @string Dir(@string path) {
    var (dir, _) = Split(path);
    return Clean(dir);
}

} // end path_package
