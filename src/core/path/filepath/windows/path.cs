// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package filepath implements utility routines for manipulating filename paths
// in a way compatible with the target operating system-defined file paths.
//
// The filepath package uses either forward slashes or backslashes,
// depending on the operating system. To process paths such as URLs
// that always use forward slashes regardless of the operating
// system, see the [path] package.
namespace go.path;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using filepathlite = @internal.filepathlite_package;
using fs = io.fs_package;
using os = os_package;
using slices = slices_package;
using @internal;
using io;
using ꓸꓸꓸstring = Span<@string>;

partial class filepath_package {

public static UntypedInt Separator => /* os.PathSeparator */ 92;
public static UntypedInt ListSeparator => /* os.PathListSeparator */ 59;

// Clean returns the shortest path name equivalent to path
// by purely lexical processing. It applies the following rules
// iteratively until no further processing can be done:
//
//  1. Replace multiple [Separator] elements with a single one.
//  2. Eliminate each . path name element (the current directory).
//  3. Eliminate each inner .. path name element (the parent directory)
//     along with the non-.. element that precedes it.
//  4. Eliminate .. elements that begin a rooted path:
//     that is, replace "/.." by "/" at the beginning of a path,
//     assuming Separator is '/'.
//
// The returned path ends in a slash only if it represents a root directory,
// such as "/" on Unix or `C:\` on Windows.
//
// Finally, any occurrences of slash are replaced by Separator.
//
// If the result of this process is an empty string, Clean
// returns the string ".".
//
// On Windows, Clean does not modify the volume name other than to replace
// occurrences of "/" with `\`.
// For example, Clean("//host/share/../x") returns `\\host\share\x`.
//
// See also Rob Pike, “Lexical File Names in Plan 9 or
// Getting Dot-Dot Right,”
// https://9p.io/sys/doc/lexnames.html
public static @string Clean(@string path) {
    return filepathlite.Clean(path);
}

// IsLocal reports whether path, using lexical analysis only, has all of these properties:
//
//   - is within the subtree rooted at the directory in which path is evaluated
//   - is not an absolute path
//   - is not empty
//   - on Windows, is not a reserved name such as "NUL"
//
// If IsLocal(path) returns true, then
// Join(base, path) will always produce a path contained within base and
// Clean(path) will always produce an unrooted path with no ".." path elements.
//
// IsLocal is a purely lexical operation.
// In particular, it does not account for the effect of any symbolic links
// that may exist in the filesystem.
public static bool IsLocal(@string path) {
    return filepathlite.IsLocal(path);
}

// Localize converts a slash-separated path into an operating system path.
// The input path must be a valid path as reported by [io/fs.ValidPath].
//
// Localize returns an error if the path cannot be represented by the operating system.
// For example, the path a\b is rejected on Windows, on which \ is a separator
// character and cannot be part of a filename.
//
// The path returned by Localize will always be local, as reported by IsLocal.
public static (@string, error) Localize(@string path) {
    return filepathlite.Localize(path);
}

// ToSlash returns the result of replacing each separator character
// in path with a slash ('/') character. Multiple separators are
// replaced by multiple slashes.
public static @string ToSlash(@string path) {
    return filepathlite.ToSlash(path);
}

// FromSlash returns the result of replacing each slash ('/') character
// in path with a separator character. Multiple slashes are replaced
// by multiple separators.
//
// See also the Localize function, which converts a slash-separated path
// as used by the io/fs package to an operating system path.
public static @string FromSlash(@string path) {
    return filepathlite.FromSlash(path);
}

// SplitList splits a list of paths joined by the OS-specific [ListSeparator],
// usually found in PATH or GOPATH environment variables.
// Unlike strings.Split, SplitList returns an empty slice when passed an empty
// string.
public static slice<@string> SplitList(@string path) {
    return splitList(path);
}

// Split splits path immediately following the final [Separator],
// separating it into a directory and file name component.
// If there is no Separator in path, Split returns an empty dir
// and file set to path.
// The returned values have the property that path = dir+file.
public static (@string dir, @string @file) Split(@string path) {
    return filepathlite.Split(path);
}

// Join joins any number of path elements into a single path,
// separating them with an OS specific [Separator]. Empty elements
// are ignored. The result is Cleaned. However, if the argument
// list is empty or all its elements are empty, Join returns
// an empty string.
// On Windows, the result will only be a UNC path if the first
// non-empty element is a UNC path.
public static @string Join(params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.slice();

    return join(elem);
}

// Ext returns the file name extension used by path.
// The extension is the suffix beginning at the final dot
// in the final element of path; it is empty if there is
// no dot.
public static @string Ext(@string path) {
    return filepathlite.Ext(path);
}

// EvalSymlinks returns the path name after the evaluation of any symbolic
// links.
// If path is relative the result will be relative to the current directory,
// unless one of the components is an absolute symbolic link.
// EvalSymlinks calls [Clean] on the result.
public static (@string, error) EvalSymlinks(@string path) {
    return evalSymlinks(path);
}

// IsAbs reports whether the path is absolute.
public static bool IsAbs(@string path) {
    return filepathlite.IsAbs(path);
}

// Abs returns an absolute representation of path.
// If the path is not absolute it will be joined with the current
// working directory to turn it into an absolute path. The absolute
// path name for a given file is not guaranteed to be unique.
// Abs calls [Clean] on the result.
public static (@string, error) Abs(@string path) {
    return abs(path);
}

internal static (@string, error) unixAbs(@string path) {
    if (IsAbs(path)) {
        return (Clean(path), default!);
    }
    var (wd, err) = os.Getwd();
    if (err != default!) {
        return ("", err);
    }
    return (Join(wd, path), default!);
}

// Rel returns a relative path that is lexically equivalent to targpath when
// joined to basepath with an intervening separator. That is,
// [Join](basepath, Rel(basepath, targpath)) is equivalent to targpath itself.
// On success, the returned path will always be relative to basepath,
// even if basepath and targpath share no elements.
// An error is returned if targpath can't be made relative to basepath or if
// knowing the current working directory would be necessary to compute it.
// Rel calls [Clean] on the result.
public static (@string, error) Rel(@string basepath, @string targpath) {
    @string baseVol = VolumeName(basepath);
    @string targVol = VolumeName(targpath);
    @string @base = Clean(basepath);
    @string targ = Clean(targpath);
    if (sameWord(targ, @base)) {
        return (".", default!);
    }
    @base = @base.slice(len(baseVol));
    targ = targ.slice(len(targVol));
    if (@base == "."u8){
        @base = ""u8;
    } else 
    if (@base == ""u8 && filepathlite.VolumeNameLen(baseVol) > 2) {
        /* isUNC */
        // Treat any targetpath matching `\\host\share` basepath as absolute path.
        @base = ((@string)(rune)Separator);
    }
    // Can't use IsAbs - `\a` and `a` are both relative in Windows.
    var baseSlashed = len(@base) > 0 && @base[0] == Separator;
    var targSlashed = len(targ) > 0 && targ[0] == Separator;
    if (baseSlashed != targSlashed || !sameWord(baseVol, targVol)) {
        return ("", errors.New("Rel: can't make "u8 + targpath + " relative to "u8 + basepath));
    }
    // Position base[b0:bi] and targ[t0:ti] at the first differing elements.
    nint bl = len(@base);
    nint tl = len(targ);
    nint b0 = default!;
    nint bi = default!;
    nint t0 = default!;
    nint ti = default!;
    while (ᐧ) {
        while (bi < bl && @base[bi] != Separator) {
            bi++;
        }
        while (ti < tl && targ[ti] != Separator) {
            ti++;
        }
        if (!sameWord(targ.slice(t0, ti), @base.slice(b0, bi))) {
            break;
        }
        if (bi < bl) {
            bi++;
        }
        if (ti < tl) {
            ti++;
        }
        b0 = bi;
        t0 = ti;
    }
    if (@base.slice(b0, bi) == "..") {
        return ("", errors.New("Rel: can't make "u8 + targpath + " relative to "u8 + basepath));
    }
    if (b0 != bl) {
        // Base elements left. Must go up before going down.
        nint seps = bytealg.CountString(@base.slice(b0, bl), Separator);
        nint size = 2 + seps * 3;
        if (tl != t0) {
            size += 1 + tl - t0;
        }
        var buf = new slice<byte>(size);
        nint n = copy(buf, ".."u8);
        for (nint i = 0; i < seps; i++) {
            buf[n] = Separator;
            copy(buf.slice(n + 1), ".."u8);
            n += 3;
        }
        if (t0 != tl) {
            buf[n] = Separator;
            copy(buf.slice(n + 1), targ.slice(t0));
        }
        return (((@string)buf), default!);
    }
    return (targ.slice(t0), default!);
}

// SkipDir is used as a return value from [WalkFunc] to indicate that
// the directory named in the call is to be skipped. It is not returned
// as an error by any function.
public static error SkipDir = fs.SkipDir;

// SkipAll is used as a return value from [WalkFunc] to indicate that
// all remaining files and directories are to be skipped. It is not returned
// as an error by any function.
public static error SkipAll = fs.SkipAll;

// type WalkFunc is a methodless func type — rendered inline as its base delegate

internal static ж<Func<@string, (fs.FileInfo, error)>> Ꮡlstat = new StandardBox<Func<@string, (fs.FileInfo, error)>>(os.Lstat);
internal static ref Func<@string, (fs.FileInfo, error)> lstat => ref Ꮡlstat.ValueSlot;                   // for testing

// walkDir recursively descends path, calling walkDirFn.
internal static error walkDir(@string path, fs.DirEntry d, Func<@string, fs.DirEntry, error, error> walkDirFn) {
    {
        var errΔ1 = walkDirFn(path, d, default!); if (errΔ1 != default! || !d.IsDir()) {
            if (AreEqual(errΔ1, SkipDir) && d.IsDir()) {
                // Successfully skipped directory.
                errΔ1 = default!;
            }
            return errΔ1;
        }
    }
    var (dirs, err) = os.ReadDir(path);
    if (err != default!) {
        // Second call, to report ReadDir error.
        err = walkDirFn(path, d, err);
        if (err != default!) {
            if (AreEqual(err, SkipDir) && d.IsDir()) {
                err = default!;
            }
            return err;
        }
    }
    foreach (var (_, d1) in dirs) {
        @string path1 = Join(path, d1.Name());
        {
            var errΔ2 = walkDir(path1, d1, walkDirFn); if (errΔ2 != default!) {
                if (AreEqual(errΔ2, SkipDir)) {
                    break;
                }
                return errΔ2;
            }
        }
    }
    return default!;
}

// walk recursively descends path, calling walkFn.
internal static error walk(@string path, fs.FileInfo info, Func<@string, fs.FileInfo, error, error> walkFn) {
    if (!info.IsDir()) {
        return walkFn(path, info, default!);
    }
    var (names, err) = readDirNames(path);
    var err1 = walkFn(path, info, err);
    // If err != nil, walk can't walk into this directory.
    // err1 != nil means walkFn want walk to skip this directory or stop walking.
    // Therefore, if one of err and err1 isn't nil, walk will return.
    if (err != default! || err1 != default!) {
        // The caller's behavior is controlled by the return value, which is decided
        // by walkFn. walkFn may ignore err and return nil.
        // If walkFn returns SkipDir or SkipAll, it will be handled by the caller.
        // So walk should return whatever walkFn returns.
        return err1;
    }
    foreach (var (_, name) in names) {
        @string filename = Join(path, name);
        var (fileInfo, errΔ1) = lstat(filename);
        if (errΔ1 != default!){
            {
                var errΔ2 = walkFn(filename, fileInfo, errΔ1); if (errΔ2 != default! && !AreEqual(errΔ2, SkipDir)) {
                    return errΔ2;
                }
            }
        } else {
            errΔ1 = walk(filename, fileInfo, walkFn);
            if (errΔ1 != default!) {
                if (!fileInfo.IsDir() || !AreEqual(errΔ1, SkipDir)) {
                    return errΔ1;
                }
            }
        }
    }
    return default!;
}

// WalkDir walks the file tree rooted at root, calling fn for each file or
// directory in the tree, including root.
//
// All errors that arise visiting files and directories are filtered by fn:
// see the [fs.WalkDirFunc] documentation for details.
//
// The files are walked in lexical order, which makes the output deterministic
// but requires WalkDir to read an entire directory into memory before proceeding
// to walk that directory.
//
// WalkDir does not follow symbolic links.
//
// WalkDir calls fn with paths that use the separator character appropriate
// for the operating system. This is unlike [io/fs.WalkDir], which always
// uses slash separated paths.
public static error WalkDir(@string root, Func<@string, fs.DirEntry, error, error> fn) {
    var (info, err) = os.Lstat(root);
    if (err != default!){
        err = fn(root, default!, err);
    } else {
        err = walkDir(root, fs.FileInfoToDirEntry(info), fn);
    }
    if (AreEqual(err, SkipDir) || AreEqual(err, SkipAll)) {
        return default!;
    }
    return err;
}

// Walk walks the file tree rooted at root, calling fn for each file or
// directory in the tree, including root.
//
// All errors that arise visiting files and directories are filtered by fn:
// see the [WalkFunc] documentation for details.
//
// The files are walked in lexical order, which makes the output deterministic
// but requires Walk to read an entire directory into memory before proceeding
// to walk that directory.
//
// Walk does not follow symbolic links.
//
// Walk is less efficient than [WalkDir], introduced in Go 1.16,
// which avoids calling os.Lstat on every visited file or directory.
public static error Walk(@string root, Func<@string, fs.FileInfo, error, error> fn) {
    var (info, err) = os.Lstat(root);
    if (err != default!){
        err = fn(root, default!, err);
    } else {
        err = walk(root, info, fn);
    }
    if (AreEqual(err, SkipDir) || AreEqual(err, SkipAll)) {
        return default!;
    }
    return err;
}

// readDirNames reads the directory named by dirname and returns
// a sorted list of directory entry names.
internal static (slice<@string>, error) readDirNames(@string dirname) {
    var (f, err) = os.Open(dirname);
    if (err != default!) {
        return (default!, err);
    }
    (var names, err) = f.Readdirnames(-1);
    f.Close();
    if (err != default!) {
        return (default!, err);
    }
    slices.Sort<slice<@string>, @string>(names);
    return (names, default!);
}

// Base returns the last element of path.
// Trailing path separators are removed before extracting the last element.
// If the path is empty, Base returns ".".
// If the path consists entirely of separators, Base returns a single separator.
public static @string Base(@string path) {
    return filepathlite.Base(path);
}

// Dir returns all but the last element of path, typically the path's directory.
// After dropping the final element, Dir calls [Clean] on the path and trailing
// slashes are removed.
// If the path is empty, Dir returns ".".
// If the path consists entirely of separators, Dir returns a single separator.
// The returned path does not end in a separator unless it is the root directory.
public static @string Dir(@string path) {
    return filepathlite.Dir(path);
}

// VolumeName returns leading volume name.
// Given "C:\foo\bar" it returns "C:" on Windows.
// Given "\\host\share\foo" it returns "\\host\share".
// On other platforms it returns "".
public static @string VolumeName(@string path) {
    return filepathlite.VolumeName(path);
}

} // end filepath_package
