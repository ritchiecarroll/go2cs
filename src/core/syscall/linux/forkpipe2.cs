// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build dragonfly || freebsd || linux || netbsd || openbsd || solaris
namespace go;

using Δsync = sync_package;

partial class syscall_package {

// forkExecPipe atomically opens a pipe with O_CLOEXEC set on both file
// descriptors.
internal static error forkExecPipe(slice<nint> p) {
    return Pipe2(p, O_CLOEXEC);
}

internal static ж<Δsync.Mutex> ᏑforkingLock = new StandardBox<Δsync.Mutex>(default(Δsync.Mutex));
internal static ref Δsync.Mutex forkingLock => ref ᏑforkingLock.Value;
internal static nint forking;

// hasWaitingReaders reports whether any goroutine is waiting
// to acquire a read lock on rw. It is defined in the sync package.
[global::System.Diagnostics.StackTraceHidden] internal static bool hasWaitingReaders(ж<Δsync.RWMutex> rw) {
    return Δsync.syscall_hasWaitingReaders(rw);
}

// acquireForkLock acquires a write lock on ForkLock.
// ForkLock is exported and we've promised that during a fork
// we will call ForkLock.Lock, so that no other threads create
// new fds that are not yet close-on-exec before we fork.
// But that forces all fork calls to be serialized, which is bad.
// But we haven't promised that serialization, and it is essentially
// undetectable by other users of ForkLock, which is good.
// Avoid the serialization by ensuring that ForkLock is locked
// at the first fork and unlocked when there are no more forks.
internal static void acquireForkLock() {
    GoFrame ᒐ = default;
    try {
        ᏑforkingLock.Lock();
        defer(ᏑforkingLock.Unlock, ref ᒐ);
        if (forking == 0) {
            // There is no current write lock on ForkLock.
            ᏑForkLock.Lock();
            forking++;
            return;
        }
        // ForkLock is currently locked for writing.
        if (hasWaitingReaders(ᏑForkLock)) {
            // ForkLock is locked for writing, and at least one
            // goroutine is waiting to read from it.
            // To avoid lock starvation, allow readers to proceed.
            // The simple way to do this is for us to acquire a
            // read lock. That will block us until all current
            // conceptual write locks are released.
            //
            // Note that this case is unusual on modern systems
            // with O_CLOEXEC and SOCK_CLOEXEC. On those systems
            // the standard library should never take a read
            // lock on ForkLock.
            ᏑforkingLock.Unlock();
            ᏑForkLock.RLock();
            ᏑForkLock.RUnlock();
            ᏑforkingLock.Lock();
            // Readers got a chance, so now take the write lock.
            if (forking == 0) {
                ᏑForkLock.Lock();
            }
        }
        forking++;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// releaseForkLock releases the conceptual write lock on ForkLock
// acquired by acquireForkLock.
internal static void releaseForkLock() {
    GoFrame ᒐ = default;
    try {
        ᏑforkingLock.Lock();
        defer(ᏑforkingLock.Unlock, ref ᒐ);
        if (forking <= 0) {
            throw panic("syscall.releaseForkLock: negative count");
        }
        forking--;
        if (forking == 0) {
            // No more conceptual write locks.
            ᏑForkLock.Unlock();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end syscall_package
