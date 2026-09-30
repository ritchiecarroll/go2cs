// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.path;

using strings = strings_package;
using syscall = syscall_package;

partial class filepath_package {

// normVolumeName is like VolumeName, but makes drive letter upper case.
// result of EvalSymlinks must be unique, so we have
// EvalSymlinks(`c:\a`) == EvalSymlinks(`C:\a`).
internal static @string normVolumeName(@string path) {
    @string volume = VolumeName(path);
    if (len(volume) > 2) {
        // isUNC
        return volume;
    }
    return strings.ToUpper(volume);
}

// normBase returns the last element of path with correct case.
internal static (@string, error) normBase(@string path) {
    var (p, err) = syscall.UTF16PtrFromString(path);
    if (err != default!) {
        return ("", err);
    }
    ref var data = ref heap(new syscall.Win32finddata(), out var Ꮡdata);
    (var h, err) = syscall.FindFirstFile(p, Ꮡdata);
    if (err != default!) {
        return ("", err);
    }
    syscall.FindClose(h);
    return (syscall.UTF16ToString(data.FileName[..]), default!);
}

// baseIsDotDot reports whether the last element of path is "..".
// The given path should be 'Clean'-ed in advance.
internal static bool baseIsDotDot(@string path) {
    nint i = strings.LastIndexByte(path, Separator);
    return path.slice(i + 1) == "..";
}

// toNorm returns the normalized path that is guaranteed to be unique.
// It should accept the following formats:
//   - UNC paths                              (e.g \\server\share\foo\bar)
//   - absolute paths                         (e.g C:\foo\bar)
//   - relative paths begin with drive letter (e.g C:foo\bar, C:..\foo\bar, C:.., C:.)
//   - relative paths begin with '\'          (e.g \foo\bar)
//   - relative paths begin without '\'       (e.g foo\bar, ..\foo\bar, .., .)
//
// The returned normalized path will be in the same form (of 5 listed above) as the input path.
// If two paths A and B are indicating the same file with the same format, toNorm(A) should be equal to toNorm(B).
// The normBase parameter should be equal to the normBase func, except for in tests.  See docs on the normBase func.
internal static (@string, error) toNorm(@string path, Func<@string, (@string, error)> normBase) {
    if (path == ""u8) {
        return (path, default!);
    }
    @string volume = normVolumeName(path);
    path = path.slice(len(volume));
    // skip special cases
    if (path == ""u8 || path == "."u8 || path == @"\"u8) {
        return (volume + path, default!);
    }
    @string normPath = default!;
    while (ᐧ) {
        if (baseIsDotDot(path)) {
            normPath = path + @"\"u8 + normPath;
            break;
        }
        var (name, err) = normBase(volume + path);
        if (err != default!) {
            return ("", err);
        }
        normPath = name + @"\"u8 + normPath;
        nint i = strings.LastIndexByte(path, Separator);
        if (i == -1) {
            break;
        }
        if (i == 0) {
            // `\Go` or `C:\Go`
            normPath = @"\"u8 + normPath;
            break;
        }
        path = path.slice(0, i);
    }
    normPath = normPath.slice(0, len(normPath) - 1); // remove trailing '\'
    return (volume + normPath, default!);
}

internal static (@string, error) evalSymlinks(@string path) {
    var (newpath, err) = walkSymlinks(path);
    if (err != default!) {
        return ("", err);
    }
    (newpath, err) = toNorm(newpath, normBase);
    if (err != default!) {
        return ("", err);
    }
    return (newpath, default!);
}

} // end filepath_package
