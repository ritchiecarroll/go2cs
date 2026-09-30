// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package os provides a platform-independent interface to operating system
// functionality. The design is Unix-like, although the error handling is
// Go-like; failing calls return values of type error rather than error numbers.
// Often, more information is available within the error. For example,
// if a call that takes a file name fails, such as [Open] or [Stat], the error
// will include the failing file name when printed and will be of type
// [*PathError], which may be unpacked for more information.
//
// The os interface is intended to be uniform across all operating systems.
// Features not generally available appear in the system-specific package syscall.
//
// Here is a simple example, opening a file and reading some of it.
//
//	file, err := os.Open("file.go") // For read access.
//	if err != nil {
//		log.Fatal(err)
//	}
//
// If the open fails, the error string will be self-explanatory, like
//
//	open file.go: no such file or directory
//
// The file's data can then be read into a slice of bytes. Read and
// Write take their byte counts from the length of the argument slice.
//
//	data := make([]byte, 100)
//	count, err := file.Read(data)
//	if err != nil {
//		log.Fatal(err)
//	}
//	fmt.Printf("read %d bytes: %q\n", count, data[:count])
//
// # Concurrency
//
// The methods of [File] correspond to file system operations. All are
// safe for concurrent use. The maximum number of concurrent
// operations on a File may be limited by the OS or the system. The
// number should be high, but exceeding it may degrade performance or
// cause other issues.
namespace go;

using errors = errors_package;
using filepathlite = @internal.filepathlite_package;
using poll = @internal.poll_package;
using testlog = @internal.testlog_package;
using Δio = io_package;
using fs = go.io.fs_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using go.io;
using go.sync;

partial class os_package {

// Name returns the name of the file as presented to Open.
//
// It is safe to call Name after [Close].
[GoRecv] public static @string Name(this ref File f) {
    return f.name;
}

// Stdin, Stdout, and Stderr are open Files pointing to the standard input,
// standard output, and standard error file descriptors.
//
// Note that the Go runtime writes to standard error for panics and crashes;
// closing Stderr may cause those messages to go elsewhere, perhaps
// to a file opened later.
public static ж<File> Stdin;
internal static void initᴛStdin() { Stdin = NewFile((uintptr)syscall.Stdin, "/dev/stdin"u8); }

public static ж<File> Stdout;
internal static void initᴛStdout() { Stdout = NewFile((uintptr)syscall.Stdout, "/dev/stdout"u8); }

public static ж<File> Stderr;
internal static void initᴛStderr() { Stderr = NewFile((uintptr)syscall.Stderr, "/dev/stderr"u8); }

// Flags to OpenFile wrapping those of the underlying system. Not all
// flags may be implemented on a given system.
public const nint O_RDONLY = /* syscall.O_RDONLY */ 0;             // open the file read-only.

public const nint O_WRONLY = /* syscall.O_WRONLY */ 1;             // open the file write-only.

public const nint O_RDWR = /* syscall.O_RDWR */ 2;               // open the file read-write.

public const nint O_APPEND = /* syscall.O_APPEND */ 1024;             // append data to the file when writing.

public const nint O_CREATE = /* syscall.O_CREAT */ 64;             // create a new file if none exists.

public const nint O_EXCL = /* syscall.O_EXCL */ 128;               // used with O_CREATE, file must not exist.

public const nint O_SYNC = /* syscall.O_SYNC */ 4096;               // open for synchronous I/O.

public const nint O_TRUNC = /* syscall.O_TRUNC */ 512;              // truncate regular writable file when opened.

// Seek whence values.
//
// Deprecated: Use io.SeekStart, io.SeekCurrent, and io.SeekEnd.
public const nint SEEK_SET = 0; // seek relative to the origin of the file

public const nint SEEK_CUR = 1; // seek relative to the current offset

public const nint SEEK_END = 2; // seek relative to the end

// LinkError records an error during a link or symlink or rename
// system call and the paths that caused it.
[GoType] partial struct LinkError {
    public @string Op;
    public @string Old;
    public @string New;
    public error Err;
}

[GoRecv] public static @string Error(this ref LinkError e) {
    return e.Op + " "u8 + e.Old + " "u8 + e.New + ": "u8 + e.Err.Error();
}

[GoRecv] public static error Unwrap(this ref LinkError e) {
    return e.Err;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readˢ = "read"u8;

// Read reads up to len(b) bytes from the File and stores them in b.
// It returns the number of bytes read and any error encountered.
// At end of file, Read returns 0, io.EOF.
public static (nint n, error err) Read(this ж<File> Ꮡf, slice<byte> b) {
    nint n = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    {
        var errΔ1 = Ꮡf.checkValid(readˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    (n, var e) = Ꮡf.read(b);
    return (n, f.wrapErr(readˢ, e));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string negativeOffsetˢ = "negative offset"u8;

// ReadAt reads len(b) bytes from the File starting at byte offset off.
// It returns the number of bytes read and the error, if any.
// ReadAt always returns a non-nil error when n < len(b).
// At end of file, that error is io.EOF.
public static (nint n, error err) ReadAt(this ж<File> Ꮡf, slice<byte> b, int64 off) {
    nint n = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    {
        var errΔ1 = Ꮡf.checkValid(readˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    if (off < 0) {
        return (0, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readat"u8, Path: f.name, Err: errors.New(negativeOffsetˢ)))));
    }
    while (len(b) > 0) {
        var (m, e) = Ꮡf.pread(b, off);
        if (e != default!) {
            err = f.wrapErr(readˢ, e);
            break;
        }
        n += m;
        b = b.slice(m);
        off += (int64)m;
    }
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeˢ = "write"u8;

// ReadFrom implements io.ReaderFrom.
public static (int64 n, error err) ReadFrom(this ж<File> Ꮡf, Δio.Reader r) {
    int64 n = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    {
        var errΔ1 = Ꮡf.checkValid(writeˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    (n, var handled, var e) = f.readFrom(r);
    if (!handled) {
        return genericReadFrom(Ꮡf, r); // without wrapping
    }
    return (n, f.wrapErr(writeˢ, e));
}

// noReadFrom can be embedded alongside another type to
// hide the ReadFrom method of that other type.
[GoType] partial struct noReadFrom {
}

// ReadFrom hides another ReadFrom method.
// It should never be called.
internal static (int64, error) ReadFrom(this noReadFrom _Δp0, Δio.Reader _Δp1) {
    throw panic("can't happen");
}

// fileWithoutReadFrom implements all the methods of *File other
// than ReadFrom. This is used to permit ReadFrom to call io.Copy
// without leading to a recursive call to ReadFrom.
[GoType] partial struct fileWithoutReadFrom {
    internal partial ref noReadFrom noReadFrom { get; }
    public partial ref ж<File> File { get; }
}

internal static (int64, error) genericReadFrom(ж<File> Ꮡf, Δio.Reader r) {
    return Δio.Copy(new fileWithoutReadFrom(File: Ꮡf), r);
}

// Write writes len(b) bytes from b to the File.
// It returns the number of bytes written and an error, if any.
// Write returns a non-nil error when n != len(b).
public static (nint n, error err) Write(this ж<File> Ꮡf, slice<byte> b) {
    nint n = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    {
        var errΔ1 = Ꮡf.checkValid(writeˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    (n, var e) = Ꮡf.write(b);
    if (n < 0) {
        n = 0;
    }
    if (n != len(b)) {
        err = Δio.ErrShortWrite;
    }
    epipecheck(ref (Ꮡf).DerefOrNull(), e);
    if (e != default!) {
        err = f.wrapErr(writeˢ, e);
    }
    return (n, err);
}

internal static error errWriteAtInAppendMode = errors.New("os: invalid use of WriteAt on file opened with O_APPEND"u8);

// WriteAt writes len(b) bytes to the File starting at byte offset off.
// It returns the number of bytes written and an error, if any.
// WriteAt returns a non-nil error when n != len(b).
//
// If file was opened with the O_APPEND flag, WriteAt returns an error.
public static (nint n, error err) WriteAt(this ж<File> Ꮡf, slice<byte> b, int64 off) {
    nint n = default!;
    error err = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    {
        var errΔ1 = Ꮡf.checkValid(writeˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    if (f.appendMode) {
        return (0, errWriteAtInAppendMode);
    }
    if (off < 0) {
        return (0, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "writeat"u8, Path: f.name, Err: errors.New(negativeOffsetˢ)))));
    }
    while (len(b) > 0) {
        var (m, e) = Ꮡf.pwrite(b, off);
        if (e != default!) {
            err = f.wrapErr(writeˢ, e);
            break;
        }
        n += m;
        b = b.slice(m);
        off += (int64)m;
    }
    return (n, err);
}

// WriteTo implements io.WriterTo.
public static (int64 n, error err) WriteTo(this ж<File> Ꮡf, Δio.Writer w) {
    int64 n = default!;

    ref var f = ref Ꮡf.DerefOrNull();
    {
        var errΔ1 = Ꮡf.checkValid(readˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    (n, var handled, var e) = f.writeTo(w);
    if (handled) {
        return (n, f.wrapErr(readˢ, e));
    }
    return genericWriteTo(Ꮡf, w); // without wrapping
}

// noWriteTo can be embedded alongside another type to
// hide the WriteTo method of that other type.
[GoType] partial struct noWriteTo {
}

// WriteTo hides another WriteTo method.
// It should never be called.
internal static (int64, error) WriteTo(this noWriteTo _Δp0, Δio.Writer _Δp1) {
    throw panic("can't happen");
}

// fileWithoutWriteTo implements all the methods of *File other
// than WriteTo. This is used to permit WriteTo to call io.Copy
// without leading to a recursive call to WriteTo.
[GoType] partial struct fileWithoutWriteTo {
    internal partial ref noWriteTo noWriteTo { get; }
    public partial ref ж<File> File { get; }
}

internal static (int64, error) genericWriteTo(ж<File> Ꮡf, Δio.Writer w) {
    return Δio.Copy(w, new fileWithoutWriteTo(File: Ꮡf));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string seekˢ = "seek"u8;

// Seek sets the offset for the next Read or Write on file to offset, interpreted
// according to whence: 0 means relative to the origin of the file, 1 means
// relative to the current offset, and 2 means relative to the end.
// It returns the new offset and an error, if any.
// The behavior of Seek on a file opened with O_APPEND is not specified.
public static (int64 ret, error err) Seek(this ж<File> Ꮡf, int64 offset, nint whence) {
    ref var f = ref Ꮡf.DerefOrNull();

    {
        var errΔ1 = Ꮡf.checkValid(seekˢ); if (errΔ1 != default!) {
            return (0, errΔ1);
        }
    }
    var (r, e) = Ꮡf.seek(offset, whence);
    if (e == default! && Ꮡf.of(File.Ꮡdirinfo).Load() != nil && r != 0) {
        e = syscall.EISDIR;
    }
    if (e != default!) {
        return (0, f.wrapErr(seekˢ, e));
    }
    return (r, default!);
}

// WriteString is like Write, but writes the contents of string s rather than
// a slice of bytes.
public static (nint n, error err) WriteString(this ж<File> Ꮡf, @string s) {
    var b = @unsafe.Slice(@unsafe.StringData(s), len(s));
    return Ꮡf.Write(b);
}

// Mkdir creates a new directory with the specified name and permission
// bits (before umask).
// If there is an error, it will be of type *PathError.
public static error Mkdir(@string name, FileMode perm) {
    @string longName = fixLongPath(name);
    var e = ignoringEINTR(() => syscall.Mkdir(longName, syscallMode(perm)));
    if (e != default!) {
        return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "mkdir"u8, Path: name, Err: e)));
    }
    // mkdir(2) itself won't handle the sticky bit on *BSD and Solaris
    if (!supportsCreateWithStickyBit && (FileMode)(perm & ModeSticky) != 0) {
        e = setStickyBit(name);
        if (e != default!) {
            Remove(name);
            return e;
        }
    }
    return default!;
}

// setStickyBit adds ModeSticky to the permission bits of path, non atomic.
internal static error setStickyBit(@string name) {
    var (fi, err) = Stat(name);
    if (err != default!) {
        return err;
    }
    return Chmod(name, (fs.FileMode)(fi.Mode() | ModeSticky));
}

// Chdir changes the current working directory to the named directory.
// If there is an error, it will be of type *PathError.
public static error Chdir(@string dir) {
    {
        var e = syscall.Chdir(dir); if (e != default!) {
            testlog.Open(dir); // observe likely non-existent directory
            return new fs.PathErrorжerror(Ꮡ(new PathError(Op: "chdir"u8, Path: dir, Err: e)));
        }
    }
    if (runtime.GOOS == "windows"u8) {
        var abs = filepathlite.IsAbs(dir);
        ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Lock();
        if (abs){
            getwdCache.dir = dir;
        } else {
            getwdCache.dir = ""u8;
        }
        ᏑgetwdCache.of(getwdCacheᴛ1.ᏑMutex).Unlock();
    }
    {
        var log = testlog.Logger(); if (log != default!) {
            var (wd, err) = Getwd();
            if (err == default!) {
                log.Chdir(wd);
            }
        }
    }
    return default!;
}

// Open opens the named file for reading. If successful, methods on
// the returned file can be used for reading; the associated file
// descriptor has mode O_RDONLY.
// If there is an error, it will be of type *PathError.
public static (ж<File>, error) Open(@string name) {
    return OpenFile(name, O_RDONLY, 0);
}

// Create creates or truncates the named file. If the file already exists,
// it is truncated. If the file does not exist, it is created with mode 0o666
// (before umask). If successful, methods on the returned File can
// be used for I/O; the associated file descriptor has mode O_RDWR.
// The directory containing the file must already exist.
// If there is an error, it will be of type *PathError.
public static (ж<File>, error) Create(@string name) {
    return OpenFile(name, (nint)((nint)(nint)(O_RDWR | O_CREATE) | O_TRUNC), 438);
}

// OpenFile is the generalized open call; most users will use Open
// or Create instead. It opens the named file with specified flag
// (O_RDONLY etc.). If the file does not exist, and the O_CREATE flag
// is passed, it is created with mode perm (before umask);
// the containing directory must exist. If successful,
// methods on the returned File can be used for I/O.
// If there is an error, it will be of type *PathError.
public static (ж<File>, error) OpenFile(@string name, nint flag, FileMode perm) {
    testlog.Open(name);
    var (f, err) = openFileNolog(name, flag, perm);
    if (err != default!) {
        return (default!, err);
    }
    f.Value.appendMode = (nint)(flag & O_APPEND) != 0;
    return (f, default!);
}

internal static error errPathEscapes = errors.New("path escapes from parent"u8);

// openDir opens a file which is assumed to be a directory. As such, it skips
// the syscalls that make the file descriptor non-blocking as these take time
// and will fail on file descriptors for directories.
internal static (ж<File>, error) openDir(@string name) {
    testlog.Open(name);
    return openDirNolog(name);
}

// lstat is overridden in tests.
internal static ж<Func<@string, (FileInfo, error)>> Ꮡlstat = new StandardBox<Func<@string, (FileInfo, error)>>(default(Func<@string, (FileInfo, error)>));
internal static ref Func<@string, (FileInfo, error)> lstat => ref Ꮡlstat.ValueSlot;
internal static void initᴛlstat() { lstat = Lstat; }

// Rename renames (moves) oldpath to newpath.
// If newpath already exists and is not a directory, Rename replaces it.
// If newpath already exists and is a directory, Rename returns an error.
// OS-specific restrictions may apply when oldpath and newpath are in different directories.
// Even within the same directory, on non-Unix platforms Rename is not an atomic operation.
// If there is an error, it will be of type *LinkError.
public static error Rename(@string oldpath, @string newpath) {
    return rename(oldpath, newpath);
}

// Readlink returns the destination of the named symbolic link.
// If there is an error, it will be of type *PathError.
//
// If the link destination is relative, Readlink returns the relative path
// without resolving it to an absolute one.
public static (@string, error) Readlink(@string name) {
    return readlink(name);
}

// Many functions in package syscall return a count of -1 instead of 0.
// Using fixCount(call()) instead of call() corrects the count.
internal static (nint, error) fixCount(nint n, error err) {
    if (n < 0) {
        n = 0;
    }
    return (n, err);
}

// checkWrapErr is the test hook to enable checking unexpected wrapped errors of poll.ErrFileClosing.
// It is set to true in the export_test.go for tests (including fuzz tests).
internal static bool checkWrapErr = false;

// wrapErr wraps an error that occurred during an operation on an open file.
// It passes io.EOF through unchanged, otherwise converts
// poll.ErrFileClosing to ErrClosed and wraps the error in a PathError.
[GoRecv] internal static error wrapErr(this ref File f, @string op, error err) {
    if (err == default! || AreEqual(err, Δio.EOF)) {
        return err;
    }
    if (AreEqual(err, poll.ErrFileClosing)){
        err = ErrClosed;
    } else 
    if (checkWrapErr && errors.Is(err, poll.ErrFileClosing)) {
        throw panic("unexpected error wrapping poll.ErrFileClosing: " + err.Error());
    }
    return new fs.PathErrorжerror(Ꮡ(new PathError(Op: op, Path: f.name, Err: err)));
}

// TempDir returns the default directory to use for temporary files.
//
// On Unix systems, it returns $TMPDIR if non-empty, else /tmp.
// On Windows, it uses GetTempPath, returning the first non-empty
// value from %TMP%, %TEMP%, %USERPROFILE%, or the Windows directory.
// On Plan 9, it returns /tmp.
//
// The directory is neither guaranteed to exist nor have accessible
// permissions.
public static @string TempDir() {
    return tempDir();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localAppDataˢ = "LocalAppData"u8;
internal static readonly @string localAppDataIsNotDefinedˢ = "%LocalAppData% is not defined"u8;
internal static readonly @string homeˢ = "HOME"u8;
internal static readonly @string homeIsNotDefinedˢ = "$HOME is not defined"u8;
internal static readonly @string homeˢ2 = "home"u8;
internal static readonly @string homeIsNotDefinedˢ2 = "$home is not defined"u8;
internal static readonly @string xdgCacheHomeˢ = "XDG_CACHE_HOME"u8;
internal static readonly @string neitherXdgCacheHomeNorˢ = "neither $XDG_CACHE_HOME nor $HOME are defined"u8;
internal static readonly @string pathInXdgCacheHomeIsˢ = "path in $XDG_CACHE_HOME is relative"u8;

// UserCacheDir returns the default root directory to use for user-specific
// cached data. Users should create their own application-specific subdirectory
// within this one and use that.
//
// On Unix systems, it returns $XDG_CACHE_HOME as specified by
// https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html if
// non-empty, else $HOME/.cache.
// On Darwin, it returns $HOME/Library/Caches.
// On Windows, it returns %LocalAppData%.
// On Plan 9, it returns $home/lib/cache.
//
// If the location cannot be determined (for example, $HOME is not defined) or
// the path in $XDG_CACHE_HOME is relative, then it will return an error.
public static (@string, error) UserCacheDir() {
    @string dir = default!;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        dir = Getenv(localAppDataˢ);
        if (dir == ""u8) {
            return ("", errors.New(localAppDataIsNotDefinedˢ));
        }
    }
    else if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
        dir = Getenv(homeˢ);
        if (dir == ""u8) {
            return ("", errors.New(homeIsNotDefinedˢ));
        }
        dir += "/Library/Caches"u8;
    }
    else if (exprᴛ1 == "plan9"u8) {
        dir = Getenv(homeˢ2);
        if (dir == ""u8) {
            return ("", errors.New(homeIsNotDefinedˢ2));
        }
        dir += "/lib/cache"u8;
    }
    else { /* default: */
        dir = Getenv(xdgCacheHomeˢ);
        if (dir == ""u8){
            // Unix
            dir = Getenv(homeˢ);
            if (dir == ""u8) {
                return ("", errors.New(neitherXdgCacheHomeNorˢ));
            }
            dir += "/.cache"u8;
        } else 
        if (!filepathlite.IsAbs(dir)) {
            return ("", errors.New(pathInXdgCacheHomeIsˢ));
        }
    }

    return (dir, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string appDataˢ = "AppData"u8;
internal static readonly @string appDataIsNotDefinedˢ = "%AppData% is not defined"u8;
internal static readonly @string xdgConfigHomeˢ = "XDG_CONFIG_HOME"u8;
internal static readonly @string neitherXdgConfigHomeNorˢ = "neither $XDG_CONFIG_HOME nor $HOME are defined"u8;
internal static readonly @string pathInXdgConfigHomeIsˢ = "path in $XDG_CONFIG_HOME is relative"u8;

// UserConfigDir returns the default root directory to use for user-specific
// configuration data. Users should create their own application-specific
// subdirectory within this one and use that.
//
// On Unix systems, it returns $XDG_CONFIG_HOME as specified by
// https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html if
// non-empty, else $HOME/.config.
// On Darwin, it returns $HOME/Library/Application Support.
// On Windows, it returns %AppData%.
// On Plan 9, it returns $home/lib.
//
// If the location cannot be determined (for example, $HOME is not defined) or
// the path in $XDG_CONFIG_HOME is relative, then it will return an error.
public static (@string, error) UserConfigDir() {
    @string dir = default!;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        dir = Getenv(appDataˢ);
        if (dir == ""u8) {
            return ("", errors.New(appDataIsNotDefinedˢ));
        }
    }
    else if (exprᴛ1 == "darwin"u8 || exprᴛ1 == "ios"u8) {
        dir = Getenv(homeˢ);
        if (dir == ""u8) {
            return ("", errors.New(homeIsNotDefinedˢ));
        }
        dir += "/Library/Application Support"u8;
    }
    else if (exprᴛ1 == "plan9"u8) {
        dir = Getenv(homeˢ2);
        if (dir == ""u8) {
            return ("", errors.New(homeIsNotDefinedˢ2));
        }
        dir += "/lib"u8;
    }
    else { /* default: */
        dir = Getenv(xdgConfigHomeˢ);
        if (dir == ""u8){
            // Unix
            dir = Getenv(homeˢ);
            if (dir == ""u8) {
                return ("", errors.New(neitherXdgConfigHomeNorˢ));
            }
            dir += "/.config"u8;
        } else 
        if (!filepathlite.IsAbs(dir)) {
            return ("", errors.New(pathInXdgConfigHomeIsˢ));
        }
    }

    return (dir, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string homeˢ3 = "$HOME"u8;
internal static readonly @string userprofileˢ = "USERPROFILE"u8;
internal static readonly @string userprofileˢ2 = "%userprofile%"u8;
internal static readonly @string homeˢ4 = "$home"u8;
internal static readonly @string sdcardˢ = "/sdcard"u8;

// UserHomeDir returns the current user's home directory.
//
// On Unix, including macOS, it returns the $HOME environment variable.
// On Windows, it returns %USERPROFILE%.
// On Plan 9, it returns the $home environment variable.
//
// If the expected variable is not set in the environment, UserHomeDir
// returns either a platform-specific default value or a non-nil error.
public static (@string, error) UserHomeDir() {
    @string env = homeˢ;
    @string enverr = homeˢ3;
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "windows"u8) {
        (env, enverr) = (userprofileˢ, userprofileˢ2);
    }
    else if (exprᴛ1 == "plan9"u8) {
        (env, enverr) = (homeˢ2, homeˢ4);
    }

    {
        @string v = Getenv(env); if (v != ""u8) {
            return (v, default!);
        }
    }
    // On some geese the home directory is not always defined.
    var exprᴛ2 = runtime.GOOS;
    if (exprᴛ2 == "android"u8) {
        return (sdcardˢ, default!);
    }
    if (exprᴛ2 == "ios"u8) {
        return ("/", default!);
    }

    return ("", errors.New(enverr + " is not defined"u8));
}

// Chmod changes the mode of the named file to mode.
// If the file is a symbolic link, it changes the mode of the link's target.
// If there is an error, it will be of type *PathError.
//
// A different subset of the mode bits are used, depending on the
// operating system.
//
// On Unix, the mode's permission bits, ModeSetuid, ModeSetgid, and
// ModeSticky are used.
//
// On Windows, only the 0o200 bit (owner writable) of mode is used; it
// controls whether the file's read-only attribute is set or cleared.
// The other bits are currently unused. For compatibility with Go 1.12
// and earlier, use a non-zero mode. Use mode 0o400 for a read-only
// file and 0o600 for a readable+writable file.
//
// On Plan 9, the mode's permission bits, ModeAppend, ModeExclusive,
// and ModeTemporary are used.
public static error Chmod(@string name, FileMode mode) {
    return chmod(name, mode);
}

// Chmod changes the mode of the file to mode.
// If there is an error, it will be of type *PathError.
public static error Chmod(this ж<File> Ꮡf, FileMode mode) {
    return Ꮡf.chmod(mode);
}

// SetDeadline sets the read and write deadlines for a File.
// It is equivalent to calling both SetReadDeadline and SetWriteDeadline.
//
// Only some kinds of files support setting a deadline. Calls to SetDeadline
// for files that do not support deadlines will return ErrNoDeadline.
// On most systems ordinary files do not support deadlines, but pipes do.
//
// A deadline is an absolute time after which I/O operations fail with an
// error instead of blocking. The deadline applies to all future and pending
// I/O, not just the immediately following call to Read or Write.
// After a deadline has been exceeded, the connection can be refreshed
// by setting a deadline in the future.
//
// If the deadline is exceeded a call to Read or Write or to other I/O
// methods will return an error that wraps ErrDeadlineExceeded.
// This can be tested using errors.Is(err, os.ErrDeadlineExceeded).
// That error implements the Timeout method, and calling the Timeout
// method will return true, but there are other possible errors for which
// the Timeout will return true even if the deadline has not been exceeded.
//
// An idle timeout can be implemented by repeatedly extending
// the deadline after successful Read or Write calls.
//
// A zero value for t means I/O operations will not time out.
public static error SetDeadline(this ж<File> Ꮡf, time.Time t) {
    return Ꮡf.setDeadline(t);
}

// SetReadDeadline sets the deadline for future Read calls and any
// currently-blocked Read call.
// A zero value for t means Read will not time out.
// Not all files support setting deadlines; see SetDeadline.
public static error SetReadDeadline(this ж<File> Ꮡf, time.Time t) {
    return Ꮡf.setReadDeadline(t);
}

// SetWriteDeadline sets the deadline for any future Write calls and any
// currently-blocked Write call.
// Even if Write times out, it may return n > 0, indicating that
// some of the data was successfully written.
// A zero value for t means Write will not time out.
// Not all files support setting deadlines; see SetDeadline.
public static error SetWriteDeadline(this ж<File> Ꮡf, time.Time t) {
    return Ꮡf.setWriteDeadline(t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syscallConnˢ = "SyscallConn"u8;

// SyscallConn returns a raw file.
// This implements the syscall.Conn interface.
public static (syscall.RawConn, error) SyscallConn(this ж<File> Ꮡf) {
    {
        var err = Ꮡf.checkValid(syscallConnˢ); if (err != default!) {
            return (default!, err);
        }
    }
    var (ᴛ1, ᴛ2) = newRawConn(Ꮡf);
    return (new rawConnжRawConn(ᴛ1), ᴛ2);
}

// DirFS returns a file system (an fs.FS) for the tree of files rooted at the directory dir.
//
// Note that DirFS("/prefix") only guarantees that the Open calls it makes to the
// operating system will begin with "/prefix": DirFS("/prefix").Open("file") is the
// same as os.Open("/prefix/file"). So if /prefix/file is a symbolic link pointing outside
// the /prefix tree, then using DirFS does not stop the access any more than using
// os.Open does. Additionally, the root of the fs.FS returned for a relative path,
// DirFS("prefix"), will be affected by later calls to Chdir. DirFS is therefore not
// a general substitute for a chroot-style security mechanism when the directory tree
// contains arbitrary content.
//
// Use [Root.FS] to obtain a fs.FS that prevents escapes from the tree via symbolic links.
//
// The directory dir must not be "".
//
// The result implements [io/fs.StatFS], [io/fs.ReadFileFS] and
// [io/fs.ReadDirFS].
public static fs.FS DirFS(@string dir) {
    return ((dirFS)dir);
}

[GoType("@string")] partial struct dirFS;

internal static (fs.File, error) Open(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "open"u8, Path: name, Err: err))));
    }
    (var f, err) = Open(fullname);
    if (err != default!) {
        // DirFS takes a string appropriate for GOOS,
        // while the name argument here is always slash separated.
        // dir.join will have mixed the two; undo that for
        // error reporting.
        err._<ж<PathError>>().Value.Path = name;
        return (default!, err);
    }
    return (new FileжFile(f), default!);
}

// The ReadFile method calls the [ReadFile] function for the file
// with the given name in the directory. The function provides
// robust handling for small files and special file systems.
// Through this method, dirFS implements [io/fs.ReadFileFS].
internal static (slice<byte>, error) ReadFile(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readfile"u8, Path: name, Err: err))));
    }
    (var b, err) = ReadFile(fullname);
    if (err != default!) {
        {
            var (e, ok) = err._<ж<PathError>>(ᐧ); if (ok) {
                // See comment in dirFS.Open.
                e.Value.Path = name;
            }
        }
        return (default!, err);
    }
    return (b, default!);
}

// ReadDir reads the named directory, returning all its directory entries sorted
// by filename. Through this method, dirFS implements [io/fs.ReadDirFS].
internal static (slice<DirEntry>, error) ReadDir(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "readdir"u8, Path: name, Err: err))));
    }
    (var entries, err) = ReadDir(fullname);
    if (err != default!) {
        {
            var (e, ok) = err._<ж<PathError>>(ᐧ); if (ok) {
                // See comment in dirFS.Open.
                e.Value.Path = name;
            }
        }
        return (default!, err);
    }
    return (entries, default!);
}

internal static (fs.FileInfo, error) Stat(this dirFS dir, @string name) {
    var (fullname, err) = dir.join(name);
    if (err != default!) {
        return (default!, new fs.PathErrorжerror(Ꮡ(new PathError(Op: "stat"u8, Path: name, Err: err))));
    }
    (var f, err) = Stat(fullname);
    if (err != default!) {
        // See comment in dirFS.Open.
        err._<ж<PathError>>().Value.Path = name;
        return (default!, err);
    }
    return (f, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string osDirFSWithEmptyRootˢ = "os: DirFS with empty root"u8;

// join returns the path for name in dir.
internal static (@string, error) join(this dirFS dir, @string name) {
    if (dir == ""u8) {
        return ("", errors.New(osDirFSWithEmptyRootˢ));
    }
    (name, var err) = filepathlite.Localize(name);
    if (err != default!) {
        return ("", ErrInvalid);
    }
    if (IsPathSeparator(dir[len(dir) - 1])) {
        return (((@string)dir) + name, default!);
    }
    return (((@string)dir) + ((@string)(rune)PathSeparator) + name, default!);
}

// ReadFile reads the named file and returns the contents.
// A successful call returns err == nil, not err == EOF.
// Because ReadFile reads the whole file, it does not treat an EOF from Read
// as an error to be reported.
public static (slice<byte>, error) ReadFile(@string name) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = Open(name);
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

internal static (slice<byte>, error) readFileContents(ж<File> Ꮡf) {
    nint size = default!;
    {
        var (info, err) = Ꮡf.Stat(); if (err == default!) {
            var size64 = info.Size();
            if ((int64)(nint)size64 == size64) {
                size = (nint)size64;
            }
        }
    }
    size++; // one byte for final read at EOF
    // If a file claims a small size, read at least 512 bytes.
    // In particular, files in Linux's /proc claim size 0 but
    // then do not work right if read in small pieces,
    // so an initial read of 1 byte would not work correctly.
    if (size < 512) {
        size = 512;
    }
    var data = new slice<byte>(0, size);
    while (ᐧ) {
        var (n, err) = Ꮡf.Read(data.slice(len(data), cap(data)));
        data = data.slice(0, len(data) + n);
        if (err != default!) {
            if (AreEqual(err, Δio.EOF)) {
                err = default!;
            }
            return (data, err);
        }
        if (len(data) >= cap(data)) {
            var d = append(data.slice(0, cap(data)), (byte)(0));
            data = d.slice(0, len(data));
        }
    }
}

// WriteFile writes data to the named file, creating it if necessary.
// If the file does not exist, WriteFile creates it with permissions perm (before umask);
// otherwise WriteFile truncates it before writing, without changing permissions.
// Since WriteFile requires multiple system calls to complete, a failure mid-operation
// can leave the file in a partially written state.
public static error WriteFile(@string name, slice<byte> data, FileMode perm) {
    var (f, err) = OpenFile(name, (nint)((nint)(nint)(O_WRONLY | O_CREATE) | O_TRUNC), perm);
    if (err != default!) {
        return err;
    }
    (_, err) = f.Write(data);
    {
        var err1 = f.Close(); if (err1 != default! && err == default!) {
            err = err1;
        }
    }
    return err;
}

} // end os_package
