// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using stringslite = @internal.stringslite_package;
using testlog = @internal.testlog_package;
using fs = go.io.fs_package;
using runtime = runtime_package;
using slices = slices_package;
using @internal;
using go.io;

partial class os_package {

// OpenInRoot opens the file name in the directory dir.
// It is equivalent to OpenRoot(dir) followed by opening the file in the root.
//
// OpenInRoot returns an error if any component of the name
// references a location outside of dir.
//
// See [Root] for details and limitations.
public static (ж<File>, error) OpenInRoot(@string dir, @string name) {
    GoFrame ᒐ = default;
    try {
        var (r, err) = OpenRoot(dir);
        if (err != default!) {
            return (default!, err);
        }
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        return r.Open(name);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Root may be used to only access files within a single directory tree.
//
// Methods on Root can only access files and directories beneath a root directory.
// If any component of a file name passed to a method of Root references a location
// outside the root, the method returns an error.
// File names may reference the directory itself (.).
//
// Methods on Root will follow symbolic links, but symbolic links may not
// reference a location outside the root.
// Symbolic links must not be absolute.
//
// Methods on Root do not prohibit traversal of filesystem boundaries,
// Linux bind mounts, /proc special files, or access to Unix device files.
//
// Methods on Root are safe to be used from multiple goroutines simultaneously.
//
// On most platforms, creating a Root opens a file descriptor or handle referencing
// the directory. If the directory is moved, methods on Root reference the original
// directory in its new location.
//
// Root's behavior differs on some platforms:
//
//   - When GOOS=windows, file names may not reference Windows reserved device names
//     such as NUL and COM1.
//   - When GOOS=js, Root is vulnerable to TOCTOU (time-of-check-time-of-use)
//     attacks in symlink validation, and cannot ensure that operations will not
//     escape the root.
//   - When GOOS=plan9 or GOOS=js, Root does not track directories across renames.
//     On these platforms, a Root references a directory name, not a file descriptor.
[GoType] partial struct Root {
    internal ж<root> root;
}

internal static UntypedInt rootMaxSymlinks => 8;

// OpenRoot opens the named directory.
// If there is an error, it will be of type *PathError.
public static (ж<Root>, error) OpenRoot(@string name) {
    testlog.Open(name);
    return openRootNolog(name);
}

// Name returns the name of the directory presented to OpenRoot.
//
// It is safe to call Name after [Close].
[GoRecv] public static @string Name(this ref Root r) {
    return r.root.Name();
}

// Close closes the Root.
// After Close is called, methods on Root return errors.
[GoRecv] public static error Close(this ref Root r) {
    return r.root.Close();
}

// Open opens the named file in the root for reading.
// See [Open] for more details.
public static (ж<File>, error) Open(this ж<Root> Ꮡr, @string name) {
    return Ꮡr.OpenFile(name, O_RDONLY, 0);
}

// Create creates or truncates the named file in the root.
// See [Create] for more details.
public static (ж<File>, error) Create(this ж<Root> Ꮡr, @string name) {
    return Ꮡr.OpenFile(name, (nint)((nint)(nint)(O_RDWR | O_CREATE) | O_TRUNC), 438);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsupportedFileModeˢ = "unsupported file mode"u8;

// OpenFile opens the named file in the root.
// See [OpenFile] for more details.
//
// If perm contains bits other than the nine least-significant bits (0o777),
// OpenFile returns an error.
public static (ж<File>, error) OpenFile(this ж<Root> Ꮡr, @string name, nint flag, FileMode perm) {
    ref var r = ref Ꮡr.DerefOrNull();

    if ((FileMode)(perm & 511) != perm) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "openat"u8, Path: name, Err: errors.New(unsupportedFileModeˢ)))));
    }
    r.logOpen(name);
    var (rf, err) = rootOpenFileNolog(Ꮡr, name, flag, perm);
    if (err != default!) {
        return (default!, err);
    }
    rf.Value.appendMode = (nint)(flag & O_APPEND) != 0;
    return (rf, default!);
}

// OpenRoot opens the named directory in the root.
// If there is an error, it will be of type *PathError.
public static (ж<Root>, error) OpenRoot(this ж<Root> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.logOpen(name);
    return openRootInRoot(Ꮡr, name);
}

// Mkdir creates a new directory in the root
// with the specified name and permission bits (before umask).
// See [Mkdir] for more details.
//
// If perm contains bits other than the nine least-significant bits (0o777),
// OpenFile returns an error.
public static error Mkdir(this ж<Root> Ꮡr, @string name, FileMode perm) {
    if ((FileMode)(perm & 511) != perm) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdirat"u8, Path: name, Err: errors.New(unsupportedFileModeˢ))));
    }
    return rootMkdir(ref (Ꮡr).DerefOrNull(), name, perm);
}

// Remove removes the named file or (empty) directory in the root.
// See [Remove] for more details.
public static error Remove(this ж<Root> Ꮡr, @string name) {
    return rootRemove(ref (Ꮡr).DerefOrNull(), name);
}

// Stat returns a [FileInfo] describing the named file in the root.
// See [Stat] for more details.
public static (FileInfo, error) Stat(this ж<Root> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.logStat(name);
    return rootStat(ref (Ꮡr).DerefOrNull(), name, false);
}

// Lstat returns a [FileInfo] describing the named file in the root.
// If the file is a symbolic link, the returned FileInfo
// describes the symbolic link.
// See [Lstat] for more details.
public static (FileInfo, error) Lstat(this ж<Root> Ꮡr, @string name) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.logStat(name);
    return rootStat(ref (Ꮡr).DerefOrNull(), name, true);
}

[GoRecv] internal static void logOpen(this ref Root r, @string name) {
    {
        var log = testlog.Logger(); if (log != default!) {
            // This won't be right if r's name has changed since it was opened,
            // but it's the best we can do.
            log.Open(joinPath(r.Name(), name));
        }
    }
}

[GoRecv] internal static void logStat(this ref Root r, @string name) {
    {
        var log = testlog.Logger(); if (log != default!) {
            // This won't be right if r's name has changed since it was opened,
            // but it's the best we can do.
            log.Stat(joinPath(r.Name(), name));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyPathˢ = "empty path"u8;

// splitPathInRoot splits a path into components
// and joins it with the given prefix and suffix.
//
// The path is relative to a Root, and must not be
// absolute, volume-relative, or "".
//
// "." components are removed, except in the last component.
//
// Path separators following the last component are returned in suffixSep.
internal static (slice<@string>, @string suffixSep, error err) splitPathInRoot(@string s, slice<@string> prefix, slice<@string> suffix) {
    @string suffixSep = default!;
    error err = default!;

    if (len(s) == 0) {
        return (default!, "", errors.New(emptyPathˢ));
    }
    if (IsPathSeparator(s[0])) {
        return (default!, "", errPathEscapes);
    }
    if (runtime.GOOS == "windows"u8) {
        // Windows cleans paths before opening them.
        (s, err) = rootCleanPath(s, prefix, suffix);
        if (err != default!) {
            return (default!, "", err);
        }
        prefix = default!;
        suffix = default!;
    }
    var parts = appendꓸꓸꓸ(new @string[]{}.slice(), prefix);
    nint i = 0;
    nint j = 1;
    while (ᐧ) {
        if (j < len(s) && !IsPathSeparator(s[j])) {
            // Keep looking for the end of this component.
            j++;
            continue;
        }
        parts = append(parts, s.slice(i, j));
        // Advance to the next component, or end of the path.
        nint partEnd = j;
        while (j < len(s) && IsPathSeparator(s[j])) {
            j++;
        }
        if (j == len(s)) {
            // If this is the last path component,
            // preserve any trailing path separators.
            suffixSep = s.slice(partEnd);
            break;
        }
        if (parts[len(parts) - 1] == ".") {
            // Remove "." components, except at the end.
            parts = parts.slice(0, len(parts) - 1);
        }
        i = j;
    }
    if (len(suffix) > 0 && len(parts) > 0 && parts[len(parts) - 1] == ".") {
        // Remove a trailing "." component if we're joining to a suffix.
        parts = parts.slice(0, len(parts) - 1);
    }
    parts = appendꓸꓸꓸ(parts, suffix);
    return (parts, suffixSep, default!);
}

// FS returns a file system (an fs.FS) for the tree of files in the root.
//
// The result implements [io/fs.StatFS], [io/fs.ReadFileFS] and
// [io/fs.ReadDirFS].
public static fs.FS FS(this ж<Root> Ꮡr) {
    return new rootFSжFS(Ꮡr.Reinterpret<Root, rootFS>());
}

[GoType("Root")] partial struct rootFS;

internal static (fs.File, error) Open(this ж<rootFS> Ꮡrfs, @string name) {
    var r = Ꮡrfs.Reinterpret<rootFS, Root>();
    if (!isValidRootFSPath(name)) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: ErrInvalid))));
    }
    var (f, err) = r.Open(name);
    if (err != default!) {
        return (default!, err);
    }
    return (new FileжFile(f), default!);
}

internal static (slice<DirEntry>, error) ReadDir(this ж<rootFS> Ꮡrfs, @string name) {
    GoFrame ᒐ = default;
    try {
        var r = Ꮡrfs.Reinterpret<rootFS, Root>();
        if (!isValidRootFSPath(name)) {
            return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdir"u8, Path: name, Err: ErrInvalid))));
        }
        // This isn't efficient: We just open a regular file and ReadDir it.
        // Ideally, we would skip creating a *File entirely and operate directly
        // on the file descriptor, but that will require some extensive reworking
        // of directory reading in general.
        //
        // This suffices for the moment.
        var (f, err) = r.Open(name);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var dirs, err) = f.ReadDir(-1);
        slices.SortFunc(dirs, (DirEntry a, DirEntry b) => bytealg.CompareString(a.Name(), b.Name()));
        return (dirs, err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (slice<byte>, error) ReadFile(this ж<rootFS> Ꮡrfs, @string name) {
    GoFrame ᒐ = default;
    try {
        var r = Ꮡrfs.Reinterpret<rootFS, Root>();
        if (!isValidRootFSPath(name)) {
            return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readfile"u8, Path: name, Err: ErrInvalid))));
        }
        var (f, err) = r.Open(name);
        if (err != default!) {
            return (default!, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        return readFileContents(f);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static (FileInfo, error) Stat(this ж<rootFS> Ꮡrfs, @string name) {
    var r = Ꮡrfs.Reinterpret<rootFS, Root>();
    if (!isValidRootFSPath(name)) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "stat"u8, Path: name, Err: ErrInvalid))));
    }
    return r.Stat(name);
}

// isValidRootFSPath reprots whether name is a valid filename to pass a Root.FS method.
internal static bool isValidRootFSPath(@string name) {
    if (!fs.ValidPath(name)) {
        return false;
    }
    if (runtime.GOOS == "windows"u8) {
        // fs.FS paths are /-separated.
        // On Windows, reject the path if it contains any \ separators.
        // Other forms of invalid path (for example, "NUL") are handled by
        // Root's usual file lookup mechanisms.
        if (stringslite.IndexByte(name, (rune)'\\') >= 0) {
            return false;
        }
    }
    return true;
}

} // end os_package
