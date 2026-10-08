// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fs defines basic interfaces to a file system.
// A file system can be provided by the host operating system
// but also by other packages.
//
// See the [testing/fstest] package for support with testing
// implementations of file systems.
namespace go.io;

using oserror = @internal.oserror_package;
using time = time_package;
using utf8 = unicode.utf8_package;
using @internal;
using io = io_package;
using unicode;

partial class fs_package {

// An FS provides access to a hierarchical file system.
//
// The FS interface is the minimum implementation required of the file system.
// A file system may implement additional interfaces,
// such as [ReadFileFS], to provide additional or optimized functionality.
//
// [testing/fstest.TestFS] may be used to test implementations of an FS for
// correctness.
partial interface FS {
    // Open opens the named file.
    // [File.Close] must be called to release any associated resources.
    //
    // When Open returns an error, it should be of type *PathError
    // with the Op field set to "open", the Path field set to name,
    // and the Err field describing the problem.
    //
    // Open should reject attempts to open names that do not satisfy
    // ValidPath(name), returning a *PathError with Err set to
    // ErrInvalid or ErrNotExist.
    (File, error) Open(@string name);
}

// ValidPath reports whether the given path name
// is valid for use in a call to Open.
//
// Path names passed to open are UTF-8-encoded,
// unrooted, slash-separated sequences of path elements, like “x/y/z”.
// Path names must not contain an element that is “.” or “..” or the empty string,
// except for the special case that the name "." may be used for the root directory.
// Paths must not start or end with a slash: “/x” and “x/” are invalid.
//
// Note that paths are slash-separated on all systems, even Windows.
// Paths containing other characters such as backslash and colon
// are accepted as valid, but those characters must never be
// interpreted by an [FS] implementation as path element separators.
public static bool ValidPath(@string name) {
    if (!utf8.ValidString(name)) {
        return false;
    }
    if (name == "."u8) {
        // special case
        return true;
    }
    // Iterate over elements in name, checking each.
    while (ᐧ) {
        nint i = 0;
        while (i < len(name) && name[i] != (rune)'/') {
            i++;
        }
        @string elem = name.slice(0, i);
        if (elem == ""u8 || elem == "."u8 || elem == ".."u8) {
            return false;
        }
        if (i == len(name)) {
            return true; // reached clean ending
        }
        name = name.slice(i + 1);
    }
}

// A File provides access to a single file.
// The File interface is the minimum implementation required of the file.
// Directory files should also implement [ReadDirFile].
// A file may implement [io.ReaderAt] or [io.Seeker] as optimizations.
partial interface File :
    io.ReadCloser
{
    (FileInfo, error) Stat();
}

// A DirEntry is an entry read from a directory
// (using the [ReadDir] function or a [ReadDirFile]'s ReadDir method).
partial interface DirEntry {
    // Name returns the name of the file (or subdirectory) described by the entry.
    // This name is only the final element of the path (the base name), not the entire path.
    // For example, Name would return "hello.go" not "home/gopher/hello.go".
    @string Name();
    // IsDir reports whether the entry describes a directory.
    bool IsDir();
    // Type returns the type bits for the entry.
    // The type bits are a subset of the usual FileMode bits, those returned by the FileMode.Type method.
    FileMode Type();
    // Info returns the FileInfo for the file or subdirectory described by the entry.
    // The returned FileInfo may be from the time of the original directory read
    // or from the time of the call to Info. If the file has been removed or renamed
    // since the directory read, Info may return an error satisfying errors.Is(err, ErrNotExist).
    // If the entry denotes a symbolic link, Info reports the information about the link itself,
    // not the link's target.
    (FileInfo, error) Info();
}

// A ReadDirFile is a directory file whose entries can be read with the ReadDir method.
// Every directory file should implement this interface.
// (It is permissible for any file to implement this interface,
// but if so ReadDir should return an error for non-directories.)
partial interface ReadDirFile :
    File
{
    // ReadDir reads the contents of the directory and returns
    // a slice of up to n DirEntry values in directory order.
    // Subsequent calls on the same file will yield further DirEntry values.
    //
    // If n > 0, ReadDir returns at most n DirEntry structures.
    // In this case, if ReadDir returns an empty slice, it will return
    // a non-nil error explaining why.
    // At the end of a directory, the error is io.EOF.
    // (ReadDir must return io.EOF itself, not an error wrapping io.EOF.)
    //
    // If n <= 0, ReadDir returns all the DirEntry values from the directory
    // in a single slice. In this case, if ReadDir succeeds (reads all the way
    // to the end of the directory), it returns the slice and a nil error.
    // If it encounters an error before the end of the directory,
    // ReadDir returns the DirEntry list read until that point and a non-nil error.
    (slice<DirEntry>, error) ReadDir(nint n);
}

// Generic file system errors.
// Errors returned by file systems can be tested against these errors
// using [errors.Is].
public static error ErrInvalid = errInvalid(); // "invalid argument"

public static error ErrPermission = errPermission(); // "permission denied"

public static error ErrExist = errExist(); // "file already exists"

public static error ErrNotExist = errNotExist(); // "file does not exist"

public static error ErrClosed = errClosed(); // "file already closed"

internal static error errInvalid() {
    return oserror.ErrInvalid;
}

internal static error errPermission() {
    return oserror.ErrPermission;
}

internal static error errExist() {
    return oserror.ErrExist;
}

internal static error errNotExist() {
    return oserror.ErrNotExist;
}

internal static error errClosed() {
    return oserror.ErrClosed;
}

// A FileInfo describes a file and is returned by [Stat].
partial interface FileInfo {
    @string Name();      // base name of the file
    int64 Size();        // length in bytes for regular files; system-dependent for others
    FileMode Mode();     // file mode bits
    time.Time ModTime(); // modification time
    bool IsDir();        // abbreviation for Mode().IsDir()
    any Sys();           // underlying data source (can return nil)
}

partial struct FileMode /*num:uint32*/;

// The defined file mode bits are the most significant bits of the [FileMode].
// The nine least-significant bits are the standard Unix rwxrwxrwx permissions.
// The values of these bits should be considered part of the public API and
// may be used in wire protocols or disk representations: they must not be
// changed, although new bits might be added.
public static FileMode ModeDir => /* 1 << (32 - 1 - iota) */ 2147483648;                         // d: is a directory

public static FileMode ModeAppend => 1073741824;                      // a: append-only

public static FileMode ModeExclusive => 536870912;                   // l: exclusive use

public static FileMode ModeTemporary => 268435456;                   // T: temporary file; Plan 9 only

public static FileMode ModeSymlink => 134217728;                     // L: symbolic link

public static FileMode ModeDevice => 67108864;                      // D: device file

public static FileMode ModeNamedPipe => 33554432;                   // p: named pipe (FIFO)

public static FileMode ModeSocket => 16777216;                      // S: Unix domain socket

public static FileMode ModeSetuid => 8388608;                      // u: setuid

public static FileMode ModeSetgid => 4194304;                      // g: setgid

public static FileMode ModeCharDevice => 2097152;                  // c: Unix character device, when ModeDevice is set

public static FileMode ModeSticky => 1048576;                      // t: sticky

public static FileMode ModeIrregular => 524288;                   // ?: non-regular file; nothing else is known about this file

public static FileMode ModeType => /* ModeDir | ModeSymlink | ModeNamedPipe | ModeSocket | ModeDevice | ModeCharDevice | ModeIrregular */ 2401763328;

public static FileMode ModePerm => /* 0777 */ 511;  // Unix permission bits

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string strᶜ = "dalTLDpSugct?"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string rwxᶜ = "rwxrwxrwx"u8;

public static @string String(this FileMode m) {
    @string str = strᶜ;
    array<byte> buf = new(32);               // Mode is uint32.
    nint w = 0;
    foreach (var (i, c) in (@string)str) {
        if ((FileMode)(m & ((FileMode)((uint32)1 << (int)((nuint)(32 - 1 - i))))) != 0) {
            buf[w] = (byte)c;
            w++;
        }
    }
    if (w == 0) {
        buf[w] = (rune)'-';
        w++;
    }
    @string rwx = rwxᶜ;
    foreach (var (i, c) in (@string)rwx) {
        if ((FileMode)(m & ((FileMode)((uint32)1 << (int)((nuint)(9 - 1 - i))))) != 0){
            buf[w] = (byte)c;
        } else {
            buf[w] = (rune)'-';
        }
        w++;
    }
    return ((@string)(buf.slice(0, w)));
}

// IsDir reports whether m describes a directory.
// That is, it tests for the [ModeDir] bit being set in m.
public static bool IsDir(this FileMode m) {
    return (FileMode)(m & ModeDir) != 0;
}

// IsRegular reports whether m describes a regular file.
// That is, it tests that no mode type bits are set.
public static bool IsRegular(this FileMode m) {
    return (FileMode)(m & ModeType) == 0;
}

// Perm returns the Unix permission bits in m (m & [ModePerm]).
public static FileMode Perm(this FileMode m) {
    return (FileMode)(m & ModePerm);
}

// Type returns type bits in m (m & [ModeType]).
public static FileMode Type(this FileMode m) {
    return (FileMode)(m & ModeType);
}

// PathError records an error and the operation and file path that caused it.
partial struct PathError {
    public @string Op;
    public @string Path;
    public error Err;
}

public static @string Error(this ref PathError e) {
    return e.Op + " "u8 + e.Path + ": "u8 + e.Err.Error();
}

public static error Unwrap(this ref PathError e) {
    return e.Err;
}

internal partial interface Timeout_type /*dyn*/ {
    bool Timeout();
}

// Timeout reports whether this error represents a timeout.
public static bool Timeout(this ref PathError e) {
    var (t, ok) = e.Err._<Timeout_type>(ᐧ);
    return ok && t.Timeout();
}

} // end fs_package
