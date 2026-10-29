// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using path = path_package;

partial class fs_package {

// SkipDir is used as a return value from [WalkDirFunc] to indicate that
// the directory named in the call is to be skipped. It is not returned
// as an error by any function.
public static error SkipDir = errors.New("skip this directory"u8);

// SkipAll is used as a return value from [WalkDirFunc] to indicate that
// all remaining files and directories are to be skipped. It is not returned
// as an error by any function.
public static error SkipAll = errors.New("skip everything and stop the walk"u8);

// WalkDirFunc is the type of the function called by [WalkDir] to visit
// each file or directory.
//
// The path argument contains the argument to [WalkDir] as a prefix.
// That is, if WalkDir is called with root argument "dir" and finds a file
// named "a" in that directory, the walk function will be called with
// argument "dir/a".
//
// The d argument is the [DirEntry] for the named path.
//
// The error result returned by the function controls how [WalkDir]
// continues. If the function returns the special value [SkipDir], WalkDir
// skips the current directory (path if d.IsDir() is true, otherwise
// path's parent directory). If the function returns the special value
// [SkipAll], WalkDir skips all remaining files and directories. Otherwise,
// if the function returns a non-nil error, WalkDir stops entirely and
// returns that error.
//
// The err argument reports an error related to path, signaling that
// [WalkDir] will not walk into that directory. The function can decide how
// to handle that error; as described earlier, returning the error will
// cause WalkDir to stop walking the entire tree.
//
// [WalkDir] calls the function with a non-nil err argument in two cases.
//
// First, if the initial [Stat] on the root directory fails, WalkDir
// calls the function with path set to root, d set to nil, and err set to
// the error from [fs.Stat].
//
// Second, if a directory's ReadDir method (see [ReadDirFile]) fails, WalkDir calls the
// function with path set to the directory's path, d set to an
// [DirEntry] describing the directory, and err set to the error from
// ReadDir. In this second case, the function is called twice with the
// path of the directory: the first call is before the directory read is
// attempted and has err set to nil, giving the function a chance to
// return [SkipDir] or [SkipAll] and avoid the ReadDir entirely. The second call
// is after a failed ReadDir and reports the error from ReadDir.
// (If ReadDir succeeds, there is no second call.)
//
// The differences between WalkDirFunc compared to [path/filepath.WalkFunc] are:
//
//   - The second argument has type [DirEntry] instead of [FileInfo].
//   - The function is called before reading a directory, to allow [SkipDir]
//     or [SkipAll] to bypass the directory read entirely or skip all remaining
//     files and directories respectively.
//   - If a directory read fails, the function is called a second time
//     for that directory to report the error.
// type WalkDirFunc is a methodless func type — rendered inline as its base delegate

// walkDir recursively descends path, calling walkDirFn.
internal static error walkDir(FS fsys, @string name, DirEntry d, Func<@string, DirEntry, error, error> walkDirFn) {
    {
        var errΔ1 = walkDirFn(name, d, default); if (errΔ1 != nil || !d.IsDir()) {
            if (AreEqual(errΔ1, SkipDir) && d.IsDir()) {
                // Successfully skipped directory.
                errΔ1 = default;
            }
            return errΔ1;
        }
    }

    var (dirs, err) = ReadDir(fsys, name);
    if (err != nil) {
        // Second call, to report ReadDir error.
        err = walkDirFn(name, d, err);
        if (err != nil) {
            if (AreEqual(err, SkipDir) && d.IsDir()) {
                err = default;
            }
            return err;
        }
    }

    foreach (var (_, d1) in dirs) {
        @string name1 = path.Join(name, d1.Name());
        {
            var errΔ2 = walkDir(fsys, name1, d1, walkDirFn); if (errΔ2 != nil) {
                if (AreEqual(errΔ2, SkipDir)) {
                    break;
                }
                return errΔ2;
            }
        }
    }
    return default;
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
// WalkDir does not follow symbolic links found in directories,
// but if root itself is a symbolic link, its target will be walked.
public static error WalkDir(FS fsys, @string root, Func<@string, DirEntry, error, error> fn) {
    var (info, err) = Stat(fsys, root);
    if (err != nil) {
        err = fn(root, default, err);
    } else {
        err = walkDir(fsys, root, FileInfoToDirEntry(info), fn);
    }
    if (AreEqual(err, SkipDir) || AreEqual(err, SkipAll)) {
        return default;
    }
    return err;
}

} // end fs_package
