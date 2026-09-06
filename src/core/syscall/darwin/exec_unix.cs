// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
// Fork, exec, wait, etc.
namespace go;

using errorspkg = errors_package;
using bytealg = @internal.bytealg_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using @unsafe = unsafe_package;
using @internal;

partial class syscall_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸerrors() {
    builtin.initPackage(typeof(errors_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸbytealg() {
    builtin.initPackage(typeof(@internal.bytealg_package));
}

// ForkLock is used to synchronize creation of new file descriptors
// with fork.
//
// We want the child in a fork/exec sequence to inherit only the
// file descriptors we intend. To do that, we mark all file
// descriptors close-on-exec and then, in the child, explicitly
// unmark the ones we want the exec'ed program to keep.
// Unix doesn't make this easy: there is, in general, no way to
// allocate a new file descriptor close-on-exec. Instead you
// have to allocate the descriptor and then mark it close-on-exec.
// If a fork happens between those two events, the child's exec
// will inherit an unwanted file descriptor.
//
// This lock solves that race: the create new fd/mark close-on-exec
// operation is done holding ForkLock for reading, and the fork itself
// is done holding ForkLock for writing. At least, that's the idea.
// There are some complications.
//
// Some system calls that create new file descriptors can block
// for arbitrarily long times: open on a hung NFS server or named
// pipe, accept on a socket, and so on. We can't reasonably grab
// the lock across those operations.
//
// It is worse to inherit some file descriptors than others.
// If a non-malicious child accidentally inherits an open ordinary file,
// that's not a big deal. On the other hand, if a long-lived child
// accidentally inherits the write end of a pipe, then the reader
// of that pipe will not see EOF until that child exits, potentially
// causing the parent program to hang. This is a common problem
// in threaded C programs that use popen.
//
// Luckily, the file descriptors that are most important not to
// inherit are not the ones that can take an arbitrarily long time
// to create: pipe returns instantly, and the net package uses
// non-blocking I/O to accept on a listening socket.
// The rules for which file descriptor-creating operations use the
// ForkLock are as follows:
//
//   - [Pipe]. Use pipe2 if available. Otherwise, does not block,
//     so use ForkLock.
//   - [Socket]. Use SOCK_CLOEXEC if available. Otherwise, does not
//     block, so use ForkLock.
//   - [Open]. Use [O_CLOEXEC] if available. Otherwise, may block,
//     so live with the race.
//   - [Dup]. Use [F_DUPFD_CLOEXEC] or dup3 if available. Otherwise,
//     does not block, so use ForkLock.
public static ж<Δsync.RWMutex> ᏑForkLock = new StandardBox<Δsync.RWMutex>(default(Δsync.RWMutex));
public static ref Δsync.RWMutex ForkLock => ref ᏑForkLock.Value;

// StringSlicePtr converts a slice of strings to a slice of pointers
// to NUL-terminated byte arrays. If any string contains a NUL byte
// this function panics instead of returning an error.
//
// Deprecated: Use [SlicePtrFromStrings] instead.
public static slice<ж<byte>> StringSlicePtr(slice<@string> ss) {
    var bb = new slice<ж<byte>>(len(ss) + 1);
    for (nint i = 0; i < len(ss); i++) {
        bb[i] = StringBytePtr(ss[i]);
    }
    bb[len(ss)] = default!;
    return bb;
}

// SlicePtrFromStrings converts a slice of strings to a slice of
// pointers to NUL-terminated byte arrays. If any string contains
// a NUL byte, it returns (nil, [EINVAL]).
public static (slice<ж<byte>>, error) SlicePtrFromStrings(slice<@string> ss) {
    nint n = 0;
    foreach (var (_, s) in ss) {
        if (bytealg.IndexByteString(s, 0) != -1) {
            return (default!, EINVAL);
        }
        n += len(s) + 1; // +1 for NUL
    }
    var bb = new slice<ж<byte>>(len(ss) + 1);
    var b = new slice<byte>(n);
    n = 0;
    foreach (var (i, s) in ss) {
        bb[i] = Ꮡ(b, n);
        copy(b[(int)(n)..], s);
        n += len(s) + 1;
    }
    return (bb, default!);
}

public static void CloseOnExec(nint fd) {
    fcntl(fd, F_SETFD, FD_CLOEXEC);
}

public static error /*err*/ SetNonblock(nint fd, bool nonblocking) {
    error err = default!;

    (var flag, err) = fcntl(fd, F_GETFL, 0);
    if (err != default!) {
        return err;
    }
    if (((nint)(flag & (nint)O_NONBLOCK) != 0) == nonblocking) {
        return default!;
    }
    if (nonblocking){
        flag |= (nint)(O_NONBLOCK);
    } else {
        flag &= ~(nint)(O_NONBLOCK);
    }
    (_, err) = fcntl(fd, F_SETFL, flag);
    return err;
}

// Credential holds user and group identities to be assumed
// by a child process started by [StartProcess].
[GoType] partial struct Credential {
    public uint32 Uid;   // User ID.
    public uint32 Gid;   // Group ID.
    public slice<uint32> Groups; // Supplementary group IDs.
    public bool NoSetGroups;     // If true, don't set supplementary groups
}

// ProcAttr holds attributes that will be applied to a new process started
// by [StartProcess].
[GoType] partial struct ProcAttr {
    public @string Dir;   // Current working directory.
    public slice<@string> Env; // Environment.
    public slice<uintptr> Files; // File descriptors.
    public ж<SysProcAttr> Sys;
}

internal static ж<ProcAttr> ᏑzeroProcAttr = new StandardBox<ProcAttr>(default(ProcAttr));
internal static ref ProcAttr zeroProcAttr => ref ᏑzeroProcAttr.Value;

internal static ж<SysProcAttr> ᏑzeroSysProcAttr = new StandardBox<SysProcAttr>(default(SysProcAttr));
internal static ref SysProcAttr zeroSysProcAttr => ref ᏑzeroSysProcAttr.Value;

// go2cs generated this placeholder — func forkExec is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Combination of fork and exec, careful to be thread safe.
public static (nint pid, error err) ForkExec(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    return forkExec(argv0, argv, Ꮡattr);
}

// StartProcess wraps [ForkExec] for package os.
public static (nint pid, uintptr handle, error err) StartProcess(@string argv0, slice<@string> argv, ж<ProcAttr> Ꮡattr) {
    nint pid = default!;
    error err = default!;

    (pid, err) = forkExec(argv0, argv, Ꮡattr);
    return (pid, 0, err);
}

// Implemented in runtime package.
internal static partial void runtime_BeforeExec();

internal static partial void runtime_AfterExec();

// execveLibc is non-nil on OS using libc syscall, set to execve in exec_libc.go; this
// avoids a build dependency for other platforms.
internal static Func<uintptr, uintptr, uintptr, Errno> execveLibc;

internal static Func<ж<byte>, ж<ж<byte>>, ж<ж<byte>>, error> execveDarwin;

internal static Func<ж<byte>, ж<ж<byte>>, ж<ж<byte>>, error> execveOpenBSD;

// go2cs generated this placeholder — func Exec is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end syscall_package
