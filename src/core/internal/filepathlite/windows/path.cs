// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package filepathlite implements a subset of path/filepath,
// only using packages which may be imported by "os".
//
// Tests for these functions are in path/filepath.
namespace go.@internal;

using errors = errors_package;
using stringslite = go.@internal.stringslite_package;
using fs = io.fs_package;
using slices = slices_package;
using go.@internal;
using io;
using ꓸꓸꓸbyte = Span<byte>;

partial class filepathlite_package {

internal static error errInvalidPath = errors.New("invalid path"u8);

// A lazybuf is a lazily constructed path buffer.
// It supports append, reading previously appended bytes,
// and retrieving the final string. It does not allocate a buffer
// to hold the output until that output diverges from s.
[GoType] partial struct lazybuf {
    internal @string path;
    internal slice<byte> buf;
    internal nint w;
    internal @string volAndPath;
    internal nint volLen;
}

[GoRecv] internal static byte index(this ref lazybuf b, nint i) {
    if (b.buf != default!) {
        return b.buf[i];
    }
    return b.path[i];
}

[GoRecv] internal static void append(this ref lazybuf b, byte c) {
    if (b.buf == default!) {
        if (b.w < len(b.path) && b.path[b.w] == c) {
            b.w++;
            return;
        }
        b.buf = new slice<byte>(len(b.path));
        copy(b.buf, b.path.slice(0, b.w));
    }
    b.buf[b.w] = c;
    b.w++;
}

[GoRecv] internal static void prepend(this ref lazybuf b, params ꓸꓸꓸbyte prefixʗp) {
    var prefix = prefixʗp.sslice();

    b.buf = slices.Insert(b.buf, 0, prefix.ꓸꓸꓸ);
    b.w += len(prefix);
}

[GoRecv] internal static @string @string(this ref lazybuf b) {
    if (b.buf == default!) {
        return b.volAndPath.slice(0, b.volLen + b.w);
    }
    return b.volAndPath.slice(0, b.volLen) + ((sstring)(b.buf.slice(0, b.w)));
}

// Clean is filepath.Clean.
public static @string Clean(@string path) {
    @string originalPath = path;
    nint volLen = volumeNameLen(path);
    path = path.slice(volLen);
    if (path == ""u8) {
        if (volLen > 1 && IsPathSeparator(originalPath[0]) && IsPathSeparator(originalPath[1])) {
            // should be UNC
            return FromSlash(originalPath);
        }
        return originalPath + "."u8;
    }
    var rooted = IsPathSeparator(path[0]);
    // Invariants:
    //	reading from path; r is index of next byte to process.
    //	writing to buf; w is index of next byte to write.
    //	dotdot is index in buf where .. must stop, either because
    //		it is the leading slash or it is a leading ../../.. prefix.
    nint n = len(path);
    ref var @out = ref heap<lazybuf>(out var Ꮡout);
    @out = new lazybuf(path: path, volAndPath: originalPath, volLen: volLen);
    nint r = 0;
    nint dotdot = 0;
    if (rooted) {
        @out.append(Separator);
        (r, dotdot) = (1, 1);
    }
    while (r < n) {
        switch (ᐧ) {
        case {} when IsPathSeparator(path[r]): {
            r++;
            break;
        }
        case {} when path[r] == (rune)'.' && (r + 1 == n || IsPathSeparator(path[r + 1])): {
            r++;
            break;
        }
        case {} when path[r] == (rune)'.' && path[r + 1] == (rune)'.' && (r + 2 == n || IsPathSeparator(path[r + 2])): {
            r += 2;
            switch (ᐧ) {
            case {} when @out.w > dotdot: {
                @out.w--;
                while (@out.w > dotdot && !IsPathSeparator(@out.index(@out.w))) {
                    // empty path element
                    // . element
                    // .. element: remove to last separator
                    // can backtrack
                    @out.w--;
                }
                break;
            }
            case {} when !rooted: {
                if (@out.w > 0) {
                    // cannot backtrack, but not rooted, so append .. element.
                    @out.append(Separator);
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
                @out.append(Separator);
            }
            for (; r < n && !IsPathSeparator(path[r]); r++) {
                // copy element
                @out.append(path[r]);
            }
            break;
        }}

    }
    // Turn empty string into "."
    if (@out.w == 0) {
        @out.append((rune)'.');
    }
    postClean(Ꮡout); // avoid creating absolute paths on Windows
    return FromSlash(@out.@string());
}

// IsLocal is filepath.IsLocal.
public static bool IsLocal(@string path) {
    return isLocal(path);
}

internal static bool unixIsLocal(@string path) {
    if (IsAbs(path) || path == ""u8) {
        return false;
    }
    var hasDots = false;
    for (@string p = path; p != ""u8; ) {
        @string part = default!;
        (part, p, _) = stringslite.Cut(p, "/"u8);
        if (part == "."u8 || part == ".."u8) {
            hasDots = true;
            break;
        }
    }
    if (hasDots) {
        path = Clean(path);
    }
    if (path == ".."u8 || stringslite.HasPrefix(path, "../"u8)) {
        return false;
    }
    return true;
}

// Localize is filepath.Localize.
public static (@string, error) Localize(@string path) {
    if (!fs.ValidPath(path)) {
        return ("", errInvalidPath);
    }
    return localize(path);
}

// ToSlash is filepath.ToSlash.
public static @string ToSlash(@string path) {
    if (Separator == (rune)'/') {
        return path;
    }
    return replaceStringByte(path, Separator, (rune)'/');
}

// FromSlash is filepath.ToSlash.
public static @string FromSlash(@string path) {
    if (Separator == (rune)'/') {
        return path;
    }
    return replaceStringByte(path, (rune)'/', Separator);
}

internal static @string replaceStringByte(@string s, byte old, byte @new) {
    if (stringslite.IndexByte(s, old) == -1) {
        return s;
    }
    var n = slice<byte>(s);
    foreach (var (i, _) in n) {
        if (n[i] == old) {
            n[i] = @new;
        }
    }
    return ((@string)n);
}

// Split is filepath.Split.
public static (@string dir, @string @file) Split(@string path) {
    @string vol = VolumeName(path);
    nint i = len(path) - 1;
    while (i >= len(vol) && !IsPathSeparator(path[i])) {
        i--;
    }
    return (path.slice(0, i + 1), path.slice(i + 1));
}

// Ext is filepath.Ext.
public static @string Ext(@string path) {
    for (nint i = len(path) - 1; i >= 0 && !IsPathSeparator(path[i]); i--) {
        if (path[i] == (rune)'.') {
            return path.slice(i);
        }
    }
    return ""u8;
}

// Base is filepath.Base.
public static @string Base(@string path) {
    if (path == ""u8) {
        return "."u8;
    }
    // Strip trailing slashes.
    while (len(path) > 0 && IsPathSeparator(path[len(path) - 1])) {
        path = path.slice(0, len(path) - 1);
    }
    // Throw away volume name
    path = path.slice(len(VolumeName(path)));
    // Find the last element
    nint i = len(path) - 1;
    while (i >= 0 && !IsPathSeparator(path[i])) {
        i--;
    }
    if (i >= 0) {
        path = path.slice(i + 1);
    }
    // If empty now, it had only slashes.
    if (path == ""u8) {
        return ((@string)(rune)Separator);
    }
    return path;
}

// Dir is filepath.Dir.
public static @string Dir(@string path) {
    @string vol = VolumeName(path);
    nint i = len(path) - 1;
    while (i >= len(vol) && !IsPathSeparator(path[i])) {
        i--;
    }
    @string dir = Clean(path.slice(len(vol), i + 1));
    if (dir == "."u8 && len(vol) > 2) {
        // must be UNC
        return vol;
    }
    return vol + dir;
}

// VolumeName is filepath.VolumeName.
public static @string VolumeName(@string path) {
    return FromSlash(path.slice(0, volumeNameLen(path)));
}

// VolumeNameLen returns the length of the leading volume name on Windows.
// It returns 0 elsewhere.
public static nint VolumeNameLen(@string path) {
    return volumeNameLen(path);
}

} // end filepathlite_package
