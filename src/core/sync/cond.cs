// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using go.sync;

partial class sync_package {

// Cond implements a condition variable, a rendezvous point
// for goroutines waiting for or announcing the occurrence
// of an event.
//
// Each Cond has an associated Locker L (often a [*Mutex] or [*RWMutex]),
// which must be held when changing the condition and
// when calling the [Cond.Wait] method.
//
// A Cond must not be copied after first use.
//
// In the terminology of [the Go memory model], Cond arranges that
// a call to [Cond.Broadcast] or [Cond.Signal] “synchronizes before” any Wait call
// that it unblocks.
//
// For many simple use cases, users will be better off using channels than a
// Cond (Broadcast corresponds to closing a channel, and Signal corresponds to
// sending on a channel).
//
// For more on replacements for [sync.Cond], see [Roberto Clapis's series on
// advanced concurrency patterns], as well as [Bryan Mills's talk on concurrency
// patterns].
//
// [the Go memory model]: https://go.dev/ref/mem
// [Roberto Clapis's series on advanced concurrency patterns]: https://blogtitle.github.io/categories/concurrency/
// [Bryan Mills's talk on concurrency patterns]: https://drive.google.com/file/d/1nPdvhB0PutEJzdCq5ms6UI58dp50fcAN/view
partial struct Cond {
    internal noCopy noCopy;
    // L is held while observing or changing the condition
    public Locker L;
    internal notifyList notify;
    internal copyChecker checker;
}

// NewCond returns a new Cond with Locker l.
public static ж<Cond> NewCond(Locker l) {
    return Ꮡ(new Cond(L: l));
}

// Wait atomically unlocks c.L and suspends execution
// of the calling goroutine. After later resuming execution,
// Wait locks c.L before returning. Unlike in other systems,
// Wait cannot return unless awoken by [Cond.Broadcast] or [Cond.Signal].
//
// Because c.L is not locked while Wait is waiting, the caller
// typically cannot assume that the condition is true when
// Wait returns. Instead, the caller should Wait in a loop:
//
//	c.L.Lock()
//	for !condition() {
//	    c.Wait()
//	}
//	... make use of condition ...
//	c.L.Unlock()
public static void Wait(this ж<Cond> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.of(Cond.Ꮡchecker).check();
    var t = runtime_notifyListAdd(Ꮡc.of(Cond.Ꮡnotify));
    c.L.Unlock();
    runtime_notifyListWait(Ꮡc.of(Cond.Ꮡnotify), t);
    c.L.Lock();
}

// Signal wakes one goroutine waiting on c, if there is any.
//
// It is allowed but not required for the caller to hold c.L
// during the call.
//
// Signal() does not affect goroutine scheduling priority; if other goroutines
// are attempting to lock c.L, they may be awoken before a "waiting" goroutine.
public static void Signal(this ж<Cond> Ꮡc) {
    Ꮡc.of(Cond.Ꮡchecker).check();
    runtime_notifyListNotifyOne(Ꮡc.of(Cond.Ꮡnotify));
}

// Broadcast wakes all goroutines waiting on c.
//
// It is allowed but not required for the caller to hold c.L
// during the call.
public static void Broadcast(this ж<Cond> Ꮡc) {
    Ꮡc.of(Cond.Ꮡchecker).check();
    runtime_notifyListNotifyAll(Ꮡc.of(Cond.Ꮡnotify));
}

partial struct copyChecker /*num:uintptr*/;

// go2cs generated this placeholder — func check is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// noCopy may be added to structs which must not be copied
// after the first use.
//
// See https://golang.org/issues/8005#issuecomment-190753527
// for details.
//
// Note that it must not be embedded, due to the Lock and Unlock methods.
partial struct noCopy {
}

// Lock is a no-op used by -copylocks checker from `go vet`.
internal static void Lock(this ref noCopy _) {
}

internal static void Unlock(this ref noCopy _) {
}

} // end sync_package
