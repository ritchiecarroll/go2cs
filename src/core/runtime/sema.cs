// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Semaphore implementation exposed to Go.
// Intended use is provide a sleep and wakeup
// primitive that can be used in the contended case
// of other synchronization primitives.
// Thus it targets the same goal as Linux's futex,
// but it has much simpler semantics.
//
// That is, don't think of these as semaphores.
// Think of them as a way to implement sleep and wakeup
// such that every sleep is paired with a single wakeup,
// even if, due to races, the wakeup happens before the sleep.
//
// See Mullender and Cox, ``Semaphores in Plan 9,''
// https://swtch.com/semaphore.pdf
namespace go;

using cpu = @internal.cpu_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// Asynchronous semaphore for sync.Mutex.

// A semaRoot holds a balanced tree of sudog with distinct addresses (s.elem).
// Each of those sudog may in turn point (through s.waitlink) to a list
// of other sudogs waiting on the same address.
// The operations on the inner lists of sudogs with the same address
// are all O(1). The scanning of the top-level semaRoot list is O(log n),
// where n is the number of distinct addresses with goroutines blocked
// on them that hash to the given semaRoot.
// See golang.org/issue/17953 for a program that worked badly
// before we introduced the second level of list, and
// BenchmarkSemTable/OneAddrCollision/* for a benchmark that exercises this.
[GoType] partial struct semaRoot {
    internal mutex @lock;
    internal ж<sudog> treap;     // root of balanced tree of unique waiters.
    internal atomic.Uint32 nwait; // Number of waiters. Read w/o the lock.
}

internal static ж<semTable> Ꮡsemtable = new StandardBox<semTable>(default(semTable));
internal static ref semTable semtable => ref Ꮡsemtable.Value;

// Prime to not correlate with any user patterns.
internal static UntypedInt semTabSize => 251;

[GoType("dyn")] partial struct semTableᴛ1 {
    internal semaRoot root;
    internal array<byte> pad = new(40);
}

[GoType("[251]semTableᴛ1")] /* [semTabSize]semTableᴛ1 */
partial struct semTable;

[GoRecv] internal static ж<semaRoot> rootFor(this ref semTable t, ж<uint32> Ꮡaddr) {
    return Ꮡ(t.Value, (int)((((uintptr)Ꮡaddr >> (int)(3))) % (uintptr)semTabSize)).of(semTableᴛ1.Ꮡroot);
}

// sync_runtime_Semacquire should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_Semacquire sync.runtime_Semacquire
internal static void sync_runtime_Semacquire(ж<uint32> Ꮡaddr) {
    semacquire1(Ꮡaddr, false, semaBlockProfile, 0, waitReasonSemacquire);
}

//go:linkname poll_runtime_Semacquire internal/poll.runtime_Semacquire
internal static void poll_runtime_Semacquire(ж<uint32> Ꮡaddr) {
    semacquire1(Ꮡaddr, false, semaBlockProfile, 0, waitReasonSemacquire);
}

// sync_runtime_Semrelease should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname sync_runtime_Semrelease sync.runtime_Semrelease
internal static void sync_runtime_Semrelease(ж<uint32> Ꮡaddr, bool handoff, nint skipframes) {
    semrelease1(Ꮡaddr, handoff, skipframes);
}

//go:linkname internal_sync_runtime_SemacquireMutex internal/sync.runtime_SemacquireMutex
internal static void internal_sync_runtime_SemacquireMutex(ж<uint32> Ꮡaddr, bool lifo, nint skipframes) {
    semacquire1(Ꮡaddr, lifo, (semaProfileFlags)(semaBlockProfile | semaMutexProfile), skipframes, waitReasonSyncMutexLock);
}

//go:linkname sync_runtime_SemacquireRWMutexR sync.runtime_SemacquireRWMutexR
internal static void sync_runtime_SemacquireRWMutexR(ж<uint32> Ꮡaddr, bool lifo, nint skipframes) {
    semacquire1(Ꮡaddr, lifo, (semaProfileFlags)(semaBlockProfile | semaMutexProfile), skipframes, waitReasonSyncRWMutexRLock);
}

//go:linkname sync_runtime_SemacquireRWMutex sync.runtime_SemacquireRWMutex
internal static void sync_runtime_SemacquireRWMutex(ж<uint32> Ꮡaddr, bool lifo, nint skipframes) {
    semacquire1(Ꮡaddr, lifo, (semaProfileFlags)(semaBlockProfile | semaMutexProfile), skipframes, waitReasonSyncRWMutexLock);
}

//go:linkname sync_runtime_SemacquireWaitGroup sync.runtime_SemacquireWaitGroup
internal static void sync_runtime_SemacquireWaitGroup(ж<uint32> Ꮡaddr) {
    semacquire1(Ꮡaddr, false, semaBlockProfile, 0, waitReasonSyncWaitGroupWait);
}

//go:linkname poll_runtime_Semrelease internal/poll.runtime_Semrelease
internal static void poll_runtime_Semrelease(ж<uint32> Ꮡaddr) {
    semrelease(Ꮡaddr);
}

//go:linkname internal_sync_runtime_Semrelease internal/sync.runtime_Semrelease
internal static void internal_sync_runtime_Semrelease(ж<uint32> Ꮡaddr, bool handoff, nint skipframes) {
    semrelease1(Ꮡaddr, handoff, skipframes);
}

internal static void readyWithTime(ref sudog s, nint traceskip) {
    if (s.releasetime != 0) {
        s.releasetime = cputicks();
    }
    goready(s.g, traceskip);
}

[GoType("num:nint")] partial struct semaProfileFlags;

internal static semaProfileFlags semaBlockProfile => /* 1 << iota */ 1;
internal static semaProfileFlags semaMutexProfile => 2;

// Called from runtime.
internal static void semacquire(ж<uint32> Ꮡaddr) {
    semacquire1(Ꮡaddr, false, 0, 0, waitReasonSemacquire);
}

// go2cs generated this placeholder — func semacquire1 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void semrelease(ж<uint32> Ꮡaddr) {
    semrelease1(Ꮡaddr, false, 0);
}

// go2cs generated this placeholder — func semrelease1 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static bool cansemacquire(ж<uint32> Ꮡaddr) {
    while (ᐧ) {
        var v = atomic.Load(Ꮡaddr);
        if (v == 0) {
            return false;
        }
        if (atomic.Cas(Ꮡaddr, v, v - 1)) {
            return true;
        }
    }
}

// queue adds s to the blocked goroutines in semaRoot.
internal static void queue(this ж<semaRoot> Ꮡroot, ж<uint32> Ꮡaddr, ж<sudog> Ꮡs, bool lifo) {
    ref var root = ref Ꮡroot.DerefOrNull();
    ref var addr = ref Ꮡaddr.DerefOrNull();
    ref var s = ref Ꮡs.DerefOrNull();

    s.g = getg();
    s.elem = @unsafe.Pointer.FromPinnedBox(Ꮡaddr);
    s.next = default!;
    s.prev = default!;
    s.waiters = 0;
    ж<sudog> last = default!;
    var pt = Ꮡroot.of(semaRoot.Ꮡtreap);
    for (var t = pt.ValueSlot; t != nil; t = pt.ValueSlot) {
        if ((~t).elem == @unsafe.Pointer.FromPinnedBox(Ꮡaddr)) {
            // Already have addr in list.
            if (lifo){
                // Substitute s in t's place in treap.
                pt.ValueSlot = Ꮡs;
                s.ticket = t.Value.ticket;
                s.acquiretime = t.Value.acquiretime; // preserve head acquiretime as oldest time
                s.parent = t.Value.parent;
                s.prev = t.Value.prev;
                s.next = t.Value.next;
                if (s.prev != nil) {
                    s.prev.Value.parent = Ꮡs;
                }
                if (s.next != nil) {
                    s.next.Value.parent = Ꮡs;
                }
                // Add t first in s's wait list.
                s.waitlink = t;
                s.waittail = t.Value.waittail;
                if (s.waittail == nil) {
                    s.waittail = t;
                }
                s.waiters = t.Value.waiters;
                if ((uint16)(s.waiters + 1) != 0) {
                    s.waiters++;
                }
                t.Value.parent = default!;
                t.Value.prev = default!;
                t.Value.next = default!;
                t.Value.waittail = default!;
            } else {
                // Add s to end of t's wait list.
                if ((~t).waittail == nil){
                    t.Value.waitlink = Ꮡs;
                } else {
                    t.Value.waittail.Value.waitlink = Ꮡs;
                }
                t.Value.waittail = Ꮡs;
                s.waitlink = default!;
                if ((uint16)((~t).waiters + 1) != 0) {
                    t.Value.waiters++;
                }
            }
            return;
        }
        last = t;
        if ((uintptr)Ꮡaddr < (uintptr)(~t).elem){
            pt = t.of(sudog.Ꮡprev);
        } else {
            pt = t.of(sudog.Ꮡnext);
        }
    }
    // Add s as new leaf in tree of unique addrs.
    // The balanced tree is a treap using ticket as the random heap priority.
    // That is, it is a binary tree ordered according to the elem addresses,
    // but then among the space of possible binary trees respecting those
    // addresses, it is kept balanced on average by maintaining a heap ordering
    // on the ticket: s.ticket <= both s.prev.ticket and s.next.ticket.
    // https://en.wikipedia.org/wiki/Treap
    // https://faculty.washington.edu/aragon/pubs/rst89.pdf
    //
    // s.ticket compared with zero in couple of places, therefore set lowest bit.
    // It will not affect treap's quality noticeably.
    s.ticket = (uint32)(cheaprand() | 1);
    s.parent = last;
    pt.ValueSlot = Ꮡs;
    // Rotate up into tree according to ticket (priority).
    while (s.parent != nil && (~s.parent).ticket > s.ticket) {
        if ((~s.parent).prev == Ꮡs){
            root.rotateRight(s.parent);
        } else {
            if ((~s.parent).next != Ꮡs) {
                throw panic("semaRoot queue");
            }
            root.rotateLeft(s.parent);
        }
    }
}

// dequeue searches for and finds the first goroutine
// in semaRoot blocked on addr.
// If the sudog was being profiled, dequeue returns the time
// at which it was woken up as now. Otherwise now is 0.
// If there are additional entries in the wait list, dequeue
// returns tailtime set to the last entry's acquiretime.
// Otherwise tailtime is found.acquiretime.
internal static (ж<sudog> found, int64 now, int64 tailtime) dequeue(this ж<semaRoot> Ꮡroot, ж<uint32> Ꮡaddr) {
    int64 now = default!;
    int64 tailtime = default!;

    ref var root = ref Ꮡroot.DerefOrNull();
    ref var addr = ref Ꮡaddr.DerefOrNull();
    var ps = Ꮡroot.of(semaRoot.Ꮡtreap);
    var s = ps.ValueSlot;
    for (; s != nil; s = ps.ValueSlot) {
        if ((~s).elem == @unsafe.Pointer.FromPinnedBox(Ꮡaddr)) {
            goto Found;
        }
        if ((uintptr)Ꮡaddr < (uintptr)(~s).elem){
            ps = s.of(sudog.Ꮡprev);
        } else {
            ps = s.of(sudog.Ꮡnext);
        }
    }
    return (default!, 0, 0);
Found:
    now = (int64)0;
    if ((~s).acquiretime != 0) {
        now = cputicks();
    }
    {
        var t = s.Value.waitlink; if (t != nil){
            // Substitute t, also waiting on addr, for s in root tree of unique addrs.
            ps.ValueSlot = t;
            t.Value.ticket = s.Value.ticket;
            t.Value.parent = s.Value.parent;
            t.Value.prev = s.Value.prev;
            if ((~t).prev != nil) {
                t.Value.prev.Value.parent = t;
            }
            t.Value.next = s.Value.next;
            if ((~t).next != nil) {
                t.Value.next.Value.parent = t;
            }
            if ((~t).waitlink != nil){
                t.Value.waittail = s.Value.waittail;
            } else {
                t.Value.waittail = default!;
            }
            t.Value.waiters = s.Value.waiters;
            if ((~t).waiters > 1) {
                t.Value.waiters--;
            }
            // Set head and tail acquire time to 'now',
            // because the caller will take care of charging
            // the delays before now for all entries in the list.
            t.Value.acquiretime = now;
            tailtime = s.Value.waittail.Value.acquiretime;
            s.Value.waittail.Value.acquiretime = now;
            s.Value.waitlink = default!;
            s.Value.waittail = default!;
        } else {
            // Rotate s down to be leaf of tree for removal, respecting priorities.
            while ((~s).next != nil || (~s).prev != nil) {
                if ((~s).next == nil || (~s).prev != nil && (~(~s).prev).ticket < (~(~s).next).ticket){
                    root.rotateRight(s);
                } else {
                    root.rotateLeft(s);
                }
            }
            // Remove s, now a leaf.
            if ((~s).parent != nil){
                if ((~(~s).parent).prev == s){
                    s.Value.parent.Value.prev = default!;
                } else {
                    s.Value.parent.Value.next = default!;
                }
            } else {
                root.treap = default!;
            }
            tailtime = s.Value.acquiretime;
        }
    }
    s.Value.parent = default!;
    s.Value.elem = default!;
    s.Value.next = default!;
    s.Value.prev = default!;
    s.Value.ticket = 0;
    return (s, now, tailtime);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string semaRootRotateLeftˢ = "semaRoot rotateLeft"u8;

// rotateLeft rotates the tree rooted at node x.
// turning (x a (y b c)) into (y (x a b) c).
[GoRecv] internal static void rotateLeft(this ref semaRoot root, ж<sudog> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    // p -> (x a (y b c))
    var Δp = x.parent;
    var y = x.next;
    var b = y.Value.prev;
    y.Value.prev = Ꮡx;
    x.parent = y;
    x.next = b;
    if (b != nil) {
        b.Value.parent = Ꮡx;
    }
    y.Value.parent = Δp;
    if (Δp == nil){
        root.treap = y;
    } else 
    if ((~Δp).prev == Ꮡx){
        Δp.Value.prev = y;
    } else {
        if ((~Δp).next != Ꮡx) {
            @throw(semaRootRotateLeftˢ);
        }
        Δp.Value.next = y;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string semaRootRotateRightˢ = "semaRoot rotateRight"u8;

// rotateRight rotates the tree rooted at node y.
// turning (y (x a b) c) into (x a (y b c)).
[GoRecv] internal static void rotateRight(this ref semaRoot root, ж<sudog> Ꮡy) {
    ref var y = ref Ꮡy.DerefOrNull();

    // p -> (y (x a b) c)
    var Δp = y.parent;
    var x = y.prev;
    var b = x.Value.next;
    x.Value.next = Ꮡy;
    y.parent = x;
    y.prev = b;
    if (b != nil) {
        b.Value.parent = Ꮡy;
    }
    x.Value.parent = Δp;
    if (Δp == nil){
        root.treap = x;
    } else 
    if ((~Δp).prev == Ꮡy){
        Δp.Value.prev = x;
    } else {
        if ((~Δp).next != Ꮡy) {
            @throw(semaRootRotateRightˢ);
        }
        Δp.Value.next = x;
    }
}

// notifyList is a ticket-based notification list used to implement sync.Cond.
//
// It must be kept in sync with the sync package.
[GoType] partial struct notifyList {
    // wait is the ticket number of the next waiter. It is atomically
    // incremented outside the lock.
    internal atomic.Uint32 wait;
    // notify is the ticket number of the next waiter to be notified. It can
    // be read outside the lock, but is only written to with lock held.
    //
    // Both wait & notify can wrap around, and such cases will be correctly
    // handled as long as their "unwrapped" difference is bounded by 2^31.
    // For this not to be the case, we'd need to have 2^31+ goroutines
    // blocked on the same condvar, which is currently not possible.
    internal uint32 notify;
    // List of parked waiters.
    internal mutex @lock;
    internal ж<sudog> head;
    internal ж<sudog> tail;
}

// less checks if a < b, considering a & b running counts that may overflow the
// 32-bit range, and that their "unwrapped" difference is always less than 2^31.
internal static bool less(uint32 a, uint32 b) {
    return (int32)(a - b) < 0;
}

// notifyListAdd adds the caller to a notify list such that it can receive
// notifications. The caller must eventually call notifyListWait to wait for
// such a notification, passing the returned ticket number.
//
//go:linkname notifyListAdd sync.runtime_notifyListAdd
internal static uint32 notifyListAdd(ж<notifyList> Ꮡl) {
    // This may be called concurrently, for example, when called from
    // sync.Cond.Wait while holding a RWMutex in read mode.
    return Ꮡl.of(notifyList.Ꮡwait).Add(1) - 1;
}

// notifyListWait waits for a notification. If one has been sent since
// notifyListAdd was called, it returns immediately. Otherwise, it blocks.
//
//go:linkname notifyListWait sync.runtime_notifyListWait
internal static void notifyListWait(ж<notifyList> Ꮡl, uint32 t) {
    ref var l = ref Ꮡl.DerefOrNull();

    lockWithRank(Ꮡl.of(notifyList.Ꮡlock), lockRankNotifyList);
    // Return right away if this ticket has already been notified.
    if (less(t, l.notify)) {
        unlock(Ꮡl.of(notifyList.Ꮡlock));
        return;
    }
    // Enqueue itself.
    var s = acquireSudog();
    s.Value.g = getg();
    s.Value.ticket = t;
    s.Value.releasetime = 0;
    var t0 = (int64)0;
    if (blockprofilerate > 0) {
        t0 = cputicks();
        s.Value.releasetime = -1;
    }
    if (l.tail == nil){
        l.head = s;
    } else {
        l.tail.Value.next = s;
    }
    l.tail = s;
    goparkunlock(Ꮡl.of(notifyList.Ꮡlock), waitReasonSyncCondWait, traceBlockCondWait, 3);
    if (t0 != 0) {
        blockevent((~s).releasetime - t0, 2);
    }
    releaseSudog(s);
}

// notifyListNotifyAll notifies all entries in the list.
//
//go:linkname notifyListNotifyAll sync.runtime_notifyListNotifyAll
internal static void notifyListNotifyAll(ж<notifyList> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    // Fast-path: if there are no new waiters since the last notification
    // we don't need to acquire the lock.
    if (Ꮡl.of(notifyList.Ꮡwait).Load() == atomic.Load(Ꮡl.of(notifyList.Ꮡnotify))) {
        return;
    }
    // Pull the list out into a local variable, waiters will be readied
    // outside the lock.
    lockWithRank(Ꮡl.of(notifyList.Ꮡlock), lockRankNotifyList);
    var s = l.head;
    l.head = default!;
    l.tail = default!;
    // Update the next ticket to be notified. We can set it to the current
    // value of wait because any previous waiters are already in the list
    // or will notice that they have already been notified when trying to
    // add themselves to the list.
    atomic.Store(Ꮡl.of(notifyList.Ꮡnotify), Ꮡl.of(notifyList.Ꮡwait).Load());
    unlock(Ꮡl.of(notifyList.Ꮡlock));
    // Go through the local list and ready all waiters.
    while (s != nil) {
        var next = s.Value.next;
        s.Value.next = default!;
        if ((~(~s).g).syncGroup != nil && (~getg()).syncGroup != (~(~s).g).syncGroup) {
            println((@string)"semaphore wake of synctest goroutine"u8, (~(~s).g).goid, (@string)"from outside bubble"u8);
            throw panic("semaphore wake of synctest goroutine from outside bubble");
        }
        readyWithTime(ref (s).DerefOrNull(), 4);
        s = next;
    }
}

// notifyListNotifyOne notifies one entry in the list.
//
//go:linkname notifyListNotifyOne sync.runtime_notifyListNotifyOne
internal static void notifyListNotifyOne(ж<notifyList> Ꮡl) {
    ref var l = ref Ꮡl.DerefOrNull();

    // Fast-path: if there are no new waiters since the last notification
    // we don't need to acquire the lock at all.
    if (Ꮡl.of(notifyList.Ꮡwait).Load() == atomic.Load(Ꮡl.of(notifyList.Ꮡnotify))) {
        return;
    }
    lockWithRank(Ꮡl.of(notifyList.Ꮡlock), lockRankNotifyList);
    // Re-check under the lock if we need to do anything.
    var t = l.notify;
    if (t == Ꮡl.of(notifyList.Ꮡwait).Load()) {
        unlock(Ꮡl.of(notifyList.Ꮡlock));
        return;
    }
    // Update the next notify ticket number.
    atomic.Store(Ꮡl.of(notifyList.Ꮡnotify), t + 1);
    // Try to find the g that needs to be notified.
    // If it hasn't made it to the list yet we won't find it,
    // but it won't park itself once it sees the new notify number.
    //
    // This scan looks linear but essentially always stops quickly.
    // Because g's queue separately from taking numbers,
    // there may be minor reorderings in the list, but we
    // expect the g we're looking for to be near the front.
    // The g has others in front of it on the list only to the
    // extent that it lost the race, so the iteration will not
    // be too long. This applies even when the g is missing:
    // it hasn't yet gotten to sleep and has lost the race to
    // the (few) other g's that we find on the list.
    for (var (Δp, s) = (((ж<sudog>)nil), l.head); s != nil; (Δp, s) = (s, s.Value.next)) {
        if ((~s).ticket == t) {
            var n = s.Value.next;
            if (Δp != nil){
                Δp.Value.next = n;
            } else {
                l.head = n;
            }
            if (n == nil) {
                l.tail = Δp;
            }
            unlock(Ꮡl.of(notifyList.Ꮡlock));
            s.Value.next = default!;
            if ((~(~s).g).syncGroup != nil && (~getg()).syncGroup != (~(~s).g).syncGroup) {
                println((@string)"semaphore wake of synctest goroutine"u8, (~(~s).g).goid, (@string)"from outside bubble"u8);
                throw panic("semaphore wake of synctest goroutine from outside bubble");
            }
            readyWithTime(ref (s).DerefOrNull(), 4);
            return;
        }
    }
    unlock(Ꮡl.of(notifyList.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badNotifyListSizeˢ = "bad notifyList size"u8;

//go:linkname notifyListCheck sync.runtime_notifyListCheck
internal static void notifyListCheck(uintptr sz) {
    if (sz != /* unsafe.Sizeof(notifyList{}) */ (uintptr)32) {
        print((@string)"runtime: bad notifyList size - sync="u8, sz, (@string)" runtime="u8, /* unsafe.Sizeof(notifyList{}) */ (uintptr)32, (@string)"\n"u8);
        @throw(badNotifyListSizeˢ);
    }
}

//go:linkname internal_sync_nanotime internal/sync.runtime_nanotime
internal static int64 internal_sync_nanotime() {
    return nanotime();
}

} // end runtime_package
