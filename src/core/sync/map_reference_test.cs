// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using isync = @internal.sync_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using go.sync;
using static go.sync_internal_test_package;

partial class sync_test_package {

// This file contains reference map implementations for unit-tests.

// mapInterface is the interface Map implements.
partial interface mapInterface {
    (any value, bool ok) Load(any key);
    void Store(any key, any value);
    (any actual, bool loaded) LoadOrStore(any key, any value);
    (any value, bool loaded) LoadAndDelete(any key);
    void Delete(any _);
    (any previous, bool loaded) Swap(any key, any value);
    bool /*swapped*/ CompareAndSwap(any key, any old, any @new);
    bool /*deleted*/ CompareAndDelete(any key, any old);
    void Range(Func<any, any, bool> _);
    void Clear();
}

internal static mapInterface _ᴛ1ʗ = new sync_test_package.RWMutexMapжmapInterface(Ꮡ(new RWMutexMap(nil)));
internal static mapInterface _ᴛ2ʗ = new sync_test_package.DeepCopyMapжmapInterface(Ꮡ(new DeepCopyMap(nil)));
internal static mapInterface _ᴛ3ʗ = new sync_test_package.sync_HashTrieMapжmapInterface(Ꮡ(new isync.HashTrieMap<any, any>(nil)));

// RWMutexMap is an implementation of mapInterface using a sync.RWMutex.
partial struct RWMutexMap {
    internal Δsync.RWMutex mu;
    internal map<any, any> dirty;
}

public static (any value, bool ok) Load(this ж<RWMutexMap> Ꮡm, any key) {
    any value = default!;
    bool ok = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    Ꮡm.of(RWMutexMap.Ꮡmu).RLock();
    (value, ok) = m.dirty[key, ꟷ];
    Ꮡm.of(RWMutexMap.Ꮡmu).RUnlock();
    return (value, ok);
}

public static void Store(this ж<RWMutexMap> Ꮡm, any key, any value) {
    ref var m = ref Ꮡm.DerefOrNull();

    Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
    if (m.dirty == default!) {
        m.dirty = new map<any, any>();
    }
    m.dirty[key] = value;
    Ꮡm.of(RWMutexMap.Ꮡmu).Unlock();
}

public static (any actual, bool loaded) LoadOrStore(this ж<RWMutexMap> Ꮡm, any key, any value) {
    any actual = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
    (actual, loaded) = m.dirty[key, ꟷ];
    if (!loaded) {
        actual = value;
        if (m.dirty == default!) {
            m.dirty = new map<any, any>();
        }
        m.dirty[key] = value;
    }
    Ꮡm.of(RWMutexMap.Ꮡmu).Unlock();
    return (actual, loaded);
}

public static (any previous, bool loaded) Swap(this ж<RWMutexMap> Ꮡm, any key, any value) {
    any previous = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
    if (m.dirty == default!) {
        m.dirty = new map<any, any>();
    }
    (previous, loaded) = m.dirty[key, ꟷ];
    m.dirty[key] = value;
    Ꮡm.of(RWMutexMap.Ꮡmu).Unlock();
    return (previous, loaded);
}

public static (any value, bool loaded) LoadAndDelete(this ж<RWMutexMap> Ꮡm, any key) {
    any value = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
    (value, loaded) = m.dirty[key, ꟷ];
    if (!loaded) {
        Ꮡm.of(RWMutexMap.Ꮡmu).Unlock();
        return (default!, false);
    }
    delete(m.dirty, key);
    Ꮡm.of(RWMutexMap.Ꮡmu).Unlock();
    return (value, loaded);
}

public static void Delete(this ж<RWMutexMap> Ꮡm, any key) {
    ref var m = ref Ꮡm.DerefOrNull();

    Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
    delete(m.dirty, key);
    Ꮡm.of(RWMutexMap.Ꮡmu).Unlock();
}

public static bool /*swapped*/ CompareAndSwap(this ж<RWMutexMap> Ꮡm, any key, any old, any @new) {
    bool swapped = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
        defer(Ꮡm.of(RWMutexMap.Ꮡmu).Unlock, ref ᒐ);
        if (m.dirty == default!) {
            swapped = false; goto ᒐdone;
        }
        var (value, loaded) = m.dirty[key, ꟷ];
        if (loaded && AreEqual(value, old)) {
            m.dirty[key] = @new;
            swapped = true; goto ᒐdone;
        }
        swapped = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return swapped;
}

public static bool /*deleted*/ CompareAndDelete(this ж<RWMutexMap> Ꮡm, any key, any old) {
    bool deleted = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
        defer(Ꮡm.of(RWMutexMap.Ꮡmu).Unlock, ref ᒐ);
        if (m.dirty == default!) {
            deleted = false; goto ᒐdone;
        }
        var (value, loaded) = m.dirty[key, ꟷ];
        if (loaded && AreEqual(value, old)) {
            delete(m.dirty, key);
            deleted = true; goto ᒐdone;
        }
        deleted = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return deleted;
}

public static void Range(this ж<RWMutexMap> Ꮡm, Func<any, any, bool> f) {
    ref var m = ref Ꮡm.DerefOrNull();

    Ꮡm.of(RWMutexMap.Ꮡmu).RLock();
    var keys = new slice<any>(0, len(m.dirty));
    foreach (var (k, _) in m.dirty) {
        keys = append(keys, k);
    }
    Ꮡm.of(RWMutexMap.Ꮡmu).RUnlock();
    foreach (var (_, k) in keys) {
        var (v, ok) = Ꮡm.Load(k);
        if (!ok) {
            continue;
        }
        if (!f(k, v)) {
            break;
        }
    }
}

public static void Clear(this ж<RWMutexMap> Ꮡm) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        Ꮡm.of(RWMutexMap.Ꮡmu).Lock();
        ᒐd1 = true;
        clear(m.dirty);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡm.of(RWMutexMap.Ꮡmu).Unlock(); ᒐ.Run(); }
}

// DeepCopyMap is an implementation of mapInterface using a Mutex and
// atomic.Value.  It makes deep copies of the map on every write to avoid
// acquiring the Mutex in Load.
partial struct DeepCopyMap {
    internal Δsync.Mutex mu;
    internal atomic.Value clean;
}

public static (any value, bool ok) Load(this ж<DeepCopyMap> Ꮡm, any key) {
    any value = default!;
    bool ok = default!;

    var (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
    (value, ok) = clean[key, ꟷ];
    return (value, ok);
}

public static void Store(this ж<DeepCopyMap> Ꮡm, any key, any value) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Lock();
    var dirty = Ꮡm.dirty();
    dirty[key] = value;
    Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
    m.mu.Unlock();
}

public static (any actual, bool loaded) LoadOrStore(this ж<DeepCopyMap> Ꮡm, any key, any value) {
    any actual = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    var (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
    (actual, loaded) = clean[key, ꟷ];
    if (loaded) {
        return (actual, loaded);
    }
    m.mu.Lock();
    // Reload clean in case it changed while we were waiting on m.mu.
    (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
    (actual, loaded) = clean[key, ꟷ];
    if (!loaded) {
        var dirty = Ꮡm.dirty();
        dirty[key] = value;
        actual = value;
        Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
    }
    m.mu.Unlock();
    return (actual, loaded);
}

public static (any previous, bool loaded) Swap(this ж<DeepCopyMap> Ꮡm, any key, any value) {
    any previous = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    m.mu.Lock();
    var dirty = Ꮡm.dirty();
    (previous, loaded) = dirty[key, ꟷ];
    dirty[key] = value;
    Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
    m.mu.Unlock();
    return (previous, loaded);
}

public static (any value, bool loaded) LoadAndDelete(this ж<DeepCopyMap> Ꮡm, any key) {
    any value = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.DerefOrNull();
    m.mu.Lock();
    var dirty = Ꮡm.dirty();
    (value, loaded) = dirty[key, ꟷ];
    delete(dirty, key);
    Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
    m.mu.Unlock();
    return (value, loaded);
}

public static void Delete(this ж<DeepCopyMap> Ꮡm, any key) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Lock();
    var dirty = Ꮡm.dirty();
    delete(dirty, key);
    Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
    m.mu.Unlock();
}

public static bool /*swapped*/ CompareAndSwap(this ж<DeepCopyMap> Ꮡm, any key, any old, any @new) {
    bool swapped = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        var (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
        {
            var (previous, ok) = clean[key, ꟷ]; if (!ok || !AreEqual(previous, old)) {
                swapped = false; goto ᒐdone;
            }
        }
        m.mu.Lock();
        defer(Ꮡm.of(DeepCopyMap.Ꮡmu).Unlock, ref ᒐ);
        var dirty = Ꮡm.dirty();
        var (value, loaded) = dirty[key, ꟷ];
        if (loaded && AreEqual(value, old)) {
            dirty[key] = @new;
            Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
            swapped = true; goto ᒐdone;
        }
        swapped = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return swapped;
}

public static bool /*deleted*/ CompareAndDelete(this ж<DeepCopyMap> Ꮡm, any key, any old) {
    bool deleted = default!;
    GoFrame ᒐ = default;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        var (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
        {
            var (previous, ok) = clean[key, ꟷ]; if (!ok || !AreEqual(previous, old)) {
                deleted = false; goto ᒐdone;
            }
        }
        m.mu.Lock();
        defer(Ꮡm.of(DeepCopyMap.Ꮡmu).Unlock, ref ᒐ);
        var dirty = Ꮡm.dirty();
        var (value, loaded) = dirty[key, ꟷ];
        if (loaded && AreEqual(value, old)) {
            delete(dirty, key);
            Ꮡm.of(DeepCopyMap.Ꮡclean).Store(dirty);
            deleted = true; goto ᒐdone;
        }
        deleted = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return deleted;
}

public static void Range(this ж<DeepCopyMap> Ꮡm, Func<any, any, bool> f) {
    var (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
    foreach (var (k, v) in clean) {
        if (!f(k, v)) {
            break;
        }
    }
}

internal static map<any, any> dirty(this ж<DeepCopyMap> Ꮡm) {
    var (clean, _) = Ꮡm.of(DeepCopyMap.Ꮡclean).Load()._<map<any, any>>(ᐧ);
    var dirty = new map<any, any>(len(clean) + 1);
    foreach (var (k, v) in clean) {
        dirty[k] = v;
    }
    return dirty;
}

public static void Clear(this ж<DeepCopyMap> Ꮡm) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var m = ref Ꮡm.DerefOrNull();

        m.mu.Lock();
        ᒐd1 = true;
        Ꮡm.of(DeepCopyMap.Ꮡclean).Store(((map<any, any>)default!));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡm.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

} // end sync_test_package
