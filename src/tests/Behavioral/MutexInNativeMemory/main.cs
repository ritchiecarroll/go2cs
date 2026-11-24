namespace go;

using fmt = fmt_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.sync;

partial class main_package {

partial struct mutex {
    /*embed*/ public sync_package.Mutex Mutex;
    internal int32 cnt;
    internal int32 id;
    internal bool recursive;
}

internal static uintptr alloc() {
    var (p, _, errno) = syscall.Syscall6(syscall.SYS_MMAP, 0, 4096, (uintptr)((uintptr)syscall.PROT_READ | (uintptr)syscall.PROT_WRITE), (uintptr)((uintptr)syscall.MAP_ANON | (uintptr)syscall.MAP_PRIVATE), ~(uintptr)0, 0);
    if (errno != 0) {
        throw panic(errno);
    }
    return p;
}

internal static void enter(uintptr m, int32 id) {
    if (!(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.recursive) {
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Lock();
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, id);
        return;
    }
    if (atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid) == id) {
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt++;
        return;
    }
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Lock();
    atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, id);
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt = 1;
}

internal static bool tryEnter(uintptr m, int32 id) {
    if ((@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.recursive && atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid) == id) {
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt++;
        return true;
    }
    if (!(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).TryLock()) {
        return false;
    }
    atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, id);
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt = 1;
    return true;
}

internal static void leave(uintptr m) {
    if (!(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.recursive) {
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, 0);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Unlock();
        return;
    }
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt--;
    if ((@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Value.cnt == 0) {
        atomic.StoreInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m).Ꮡid, 0);
        (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)m)).Unlock();
    }
}

internal static partial int64 count(uintptr m, nint n) {
    var total = @unsafe.Pointer.To<int64>((@unsafe.Pointer)(m + 64));
    total.Value = 0;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint g = 1; g <= n; g++) {
        Ꮡwg.Add(1);
        var totalʗ1 = total;
        goǃ([GoFunc("count.func1")] (int32 id) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint i = 0; i < 20000; i++) {
                    enter(m, id);
                    totalʗ1.Value++;
                    leave(m);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, (int32)g);
    }
    Ꮡwg.Wait();
    return total.Value;
}

internal static void main() {
    var fast = alloc();
    var rec = alloc();
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)rec)).Value.recursive = true;
    fmt.Println("sizeof mutex:", /* unsafe.Sizeof(mutex{}) */ (uintptr)20, "sizeof sync.Mutex:", /* unsafe.Sizeof(sync.Mutex{}) */ (uintptr)8);

    enter(fast, 1);
    fmt.Println("fast: trylock while held:", (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)fast)).TryLock());
    leave(fast);
    fmt.Println("fast: trylock when free:", (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)fast)).TryLock());
    (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)fast)).Unlock();

    enter(rec, 2);
    enter(rec, 2);
    fmt.Println("recursive: cnt after two enters:", (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)rec)).Value.cnt);
    fmt.Println("recursive: tryEnter by the owner:", tryEnter(rec, 2), (@unsafe.Pointer.To<mutex>((@unsafe.Pointer)rec)).Value.cnt);
    fmt.Println("recursive: tryEnter by another id:", tryEnter(rec, 3));
    leave(rec);
    leave(rec);
    leave(rec);
    fmt.Println("recursive: free again:", atomic.LoadInt32(@unsafe.Pointer.To<mutex>((@unsafe.Pointer)rec).Ꮡid), tryEnter(rec, 3));
    leave(rec);

    foreach (var (_, n) in range(slice<nint>([2, 4, 8]))) {
        fmt.Println("fast, goroutines", n, "total:", count(fast, n));
    }
    foreach (var (_, n) in range(slice<nint>([2, 4, 8]))) {
        fmt.Println("recursive, goroutines", n, "total:", count(rec, n));
    }
}

} // end main_package
