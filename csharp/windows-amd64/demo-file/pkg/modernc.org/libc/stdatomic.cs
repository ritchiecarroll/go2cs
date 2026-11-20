// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using go.sync;

partial class libc_package {

internal static readonly ж<sync.Mutex> Ꮡint8Mu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex int8Mu => ref ((StaticBox<sync.Mutex>)Ꮡint8Mu).ValueSlot;
internal static readonly ж<sync.Mutex> Ꮡint16Mu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex int16Mu => ref ((StaticBox<sync.Mutex>)Ꮡint16Mu).ValueSlot;
internal static readonly ж<sync.Mutex> Ꮡint32Mu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex int32Mu => ref ((StaticBox<sync.Mutex>)Ꮡint32Mu).ValueSlot;
internal static readonly ж<sync.Mutex> Ꮡint64Mu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex int64Mu => ref ((StaticBox<sync.Mutex>)Ꮡint64Mu).ValueSlot;

public static int8 /*r*/ X__c11_atomic_fetch_addInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    return X__atomic_fetch_addInt8(t, ptr, val, 0);
}

public static int8 /*r*/ X__atomic_fetch_addInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value += val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint8 /*r*/ X__c11_atomic_fetch_addUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    return X__atomic_fetch_addUint8(t, ptr, val, 0);
}

public static uint8 /*r*/ X__atomic_fetch_addUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value += val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int16 /*r*/ X__c11_atomic_fetch_addInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    return X__atomic_fetch_addInt16(t, ptr, val, 0);
}

public static int16 /*r*/ X__atomic_fetch_addInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value += val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint16 /*r*/ X__c11_atomic_fetch_addUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    return X__atomic_fetch_addUint16(t, ptr, val, 0);
}

public static uint16 /*r*/ X__atomic_fetch_addUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value += val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X__c11_atomic_fetch_addInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    return X__atomic_fetch_addInt32(t, ptr, val, 0);
}

public static int32 /*r*/ X__atomic_fetch_addInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    var p = @unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
    return atomic.AddInt32(p, val) - val;
}

public static uint32 /*r*/ X__c11_atomic_fetch_addUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    return X__atomic_fetch_addUint32(t, ptr, val, 0);
}

public static uint32 /*r*/ X__atomic_fetch_addUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    var p = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr);
    return atomic.AddUint32(p, val) - val;
}

public static int64 /*r*/ X__c11_atomic_fetch_addInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    return X__atomic_fetch_addInt64(t, ptr, val, 0);
}

public static int64 /*r*/ X__atomic_fetch_addInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    var p = @unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
    return atomic.AddInt64(p, val) - val;
}

public static uint64 /*r*/ X__c11_atomic_fetch_addUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    return X__atomic_fetch_addUint64(t, ptr, val, 0);
}

public static uint64 /*r*/ X__atomic_fetch_addUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    var p = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr);
    return atomic.AddUint64(p, val) - val;
}

public static int8 /*r*/ X__c11_atomic_fetch_andInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    return X__atomic_fetch_andInt8(t, ptr, val, 0);
}

public static int8 /*r*/ X__atomic_fetch_andInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value &= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint8 /*r*/ X__c11_atomic_fetch_andUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    return X__atomic_fetch_andUint8(t, ptr, val, 0);
}

public static uint8 /*r*/ X__atomic_fetch_andUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value &= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int16 /*r*/ X__c11_atomic_fetch_andInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    return X__atomic_fetch_andInt16(t, ptr, val, 0);
}

public static int16 /*r*/ X__atomic_fetch_andInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value &= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint16 /*r*/ X__c11_atomic_fetch_andUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    return X__atomic_fetch_andUint16(t, ptr, val, 0);
}

public static uint16 /*r*/ X__atomic_fetch_andUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value &= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X__c11_atomic_fetch_andInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    return X__atomic_fetch_andInt32(t, ptr, val, 0);
}

public static int32 /*r*/ X__atomic_fetch_andInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    int32 r = default;

    var p = @unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadInt32(p);
        if (atomic.CompareAndSwapInt32(p, r, r & val)) {
            return r;
        }
    }
}

public static uint32 /*r*/ X__c11_atomic_fetch_andUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    return X__atomic_fetch_andUint32(t, ptr, val, 0);
}

public static uint32 /*r*/ X__atomic_fetch_andUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    uint32 r = default;

    var p = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadUint32(p);
        if (atomic.CompareAndSwapUint32(p, r, r & val)) {
            return r;
        }
    }
}

public static int64 /*r*/ X__c11_atomic_fetch_andInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    return X__atomic_fetch_andInt64(t, ptr, val, 0);
}

public static int64 /*r*/ X__atomic_fetch_andInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    int64 r = default;

    var p = @unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadInt64(p);
        if (atomic.CompareAndSwapInt64(p, r, r & val)) {
            return r;
        }
    }
}

public static uint64 /*r*/ X__c11_atomic_fetch_andUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    return X__atomic_fetch_andUint64(t, ptr, val, 0);
}

public static uint64 /*r*/ X__atomic_fetch_andUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    uint64 r = default;

    var p = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadUint64(p);
        if (atomic.CompareAndSwapUint64(p, r, r & val)) {
            return r;
        }
    }
}

public static int8 /*r*/ X__c11_atomic_fetch_orInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    return X__atomic_fetch_orInt8(t, ptr, val, 0);
}

public static int8 /*r*/ X__atomic_fetch_orInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value |= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint8 /*r*/ X__c11_atomic_fetch_orUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    return X__atomic_fetch_orUint8(t, ptr, val, 0);
}

public static uint8 /*r*/ X__atomic_fetch_orUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value |= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int16 /*r*/ X__c11_atomic_fetch_orInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    return X__atomic_fetch_orInt16(t, ptr, val, 0);
}

public static int16 /*r*/ X__atomic_fetch_orInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value |= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint16 /*r*/ X__c11_atomic_fetch_orUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    return X__atomic_fetch_orUint16(t, ptr, val, 0);
}

public static uint16 /*r*/ X__atomic_fetch_orUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value |= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X__c11_atomic_fetch_orInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    return X__atomic_fetch_orInt32(t, ptr, val, 0);
}

public static int32 /*r*/ X__atomic_fetch_orInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    int32 r = default;

    var p = @unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadInt32(p);
        if (atomic.CompareAndSwapInt32(p, r, r | val)) {
            return r;
        }
    }
}

public static uint32 /*r*/ X__c11_atomic_fetch_orUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    return X__atomic_fetch_orUint32(t, ptr, val, 0);
}

public static uint32 /*r*/ X__atomic_fetch_orUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    uint32 r = default;

    var p = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadUint32(p);
        if (atomic.CompareAndSwapUint32(p, r, r | val)) {
            return r;
        }
    }
}

public static int64 /*r*/ X__c11_atomic_fetch_orInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    return X__atomic_fetch_orInt64(t, ptr, val, 0);
}

public static int64 /*r*/ X__atomic_fetch_orInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    int64 r = default;

    var p = @unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadInt64(p);
        if (atomic.CompareAndSwapInt64(p, r, r | val)) {
            return r;
        }
    }
}

public static uint64 /*r*/ X__c11_atomic_fetch_orUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    return X__atomic_fetch_orUint64(t, ptr, val, 0);
}

public static uint64 /*r*/ X__atomic_fetch_orUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    uint64 r = default;

    var p = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadUint64(p);
        if (atomic.CompareAndSwapUint64(p, r, r | val)) {
            return r;
        }
    }
}

public static int8 /*r*/ X__c11_atomic_fetch_subInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    return X__atomic_fetch_subInt8(t, ptr, val, 0);
}

public static int8 /*r*/ X__atomic_fetch_subInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value -= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint8 /*r*/ X__c11_atomic_fetch_subUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    return X__atomic_fetch_subUint8(t, ptr, val, 0);
}

public static uint8 /*r*/ X__atomic_fetch_subUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value -= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int16 /*r*/ X__c11_atomic_fetch_subInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    return X__atomic_fetch_subInt16(t, ptr, val, 0);
}

public static int16 /*r*/ X__atomic_fetch_subInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value -= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint16 /*r*/ X__c11_atomic_fetch_subUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    return X__atomic_fetch_subUint16(t, ptr, val, 0);
}

public static uint16 /*r*/ X__atomic_fetch_subUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value -= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X__c11_atomic_fetch_subInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    return X__atomic_fetch_subInt32(t, ptr, val, 0);
}

public static int32 /*r*/ X__atomic_fetch_subInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    var p = @unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
    return atomic.AddInt32(p, -val) + val;
}

public static uint32 /*r*/ X__c11_atomic_fetch_subUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    return X__atomic_fetch_subUint32(t, ptr, val, 0);
}

public static uint32 /*r*/ X__atomic_fetch_subUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    var p = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr);
    return atomic.AddUint32(p, ~(val - 1)) + val;
}

public static int64 /*r*/ X__c11_atomic_fetch_subInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    return X__atomic_fetch_subInt64(t, ptr, val, 0);
}

public static int64 /*r*/ X__atomic_fetch_subInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    var p = @unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
    return atomic.AddInt64(p, -val) + val;
}

public static uint64 /*r*/ X__c11_atomic_fetch_subUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    return X__atomic_fetch_subUint64(t, ptr, val, 0);
}

public static uint64 /*r*/ X__atomic_fetch_subUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    var p = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr);
    return atomic.AddUint64(p, ~(val - 1)) + val;
}

public static int8 /*r*/ X__c11_atomic_fetch_xorInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    return X__atomic_fetch_xorInt8(t, ptr, val, 0);
}

public static int8 /*r*/ X__atomic_fetch_xorInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value ^= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint8 /*r*/ X__c11_atomic_fetch_xorUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    return X__atomic_fetch_xorUint8(t, ptr, val, 0);
}

public static uint8 /*r*/ X__atomic_fetch_xorUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value ^= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int16 /*r*/ X__c11_atomic_fetch_xorInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    return X__atomic_fetch_xorInt16(t, ptr, val, 0);
}

public static int16 /*r*/ X__atomic_fetch_xorInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value ^= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint16 /*r*/ X__c11_atomic_fetch_xorUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    return X__atomic_fetch_xorUint16(t, ptr, val, 0);
}

public static uint16 /*r*/ X__atomic_fetch_xorUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value ^= val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X__c11_atomic_fetch_xorInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    return X__atomic_fetch_xorInt32(t, ptr, val, 0);
}

public static int32 /*r*/ X__atomic_fetch_xorInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    int32 r = default;

    var p = @unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadInt32(p);
        if (atomic.CompareAndSwapInt32(p, r, r ^ val)) {
            return r;
        }
    }
}

public static uint32 /*r*/ X__c11_atomic_fetch_xorUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    return X__atomic_fetch_xorUint32(t, ptr, val, 0);
}

public static uint32 /*r*/ X__atomic_fetch_xorUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    uint32 r = default;

    var p = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadUint32(p);
        if (atomic.CompareAndSwapUint32(p, r, r ^ val)) {
            return r;
        }
    }
}

public static int64 /*r*/ X__c11_atomic_fetch_xorInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    return X__atomic_fetch_xorInt64(t, ptr, val, 0);
}

public static int64 /*r*/ X__atomic_fetch_xorInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    int64 r = default;

    var p = @unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadInt64(p);
        if (atomic.CompareAndSwapInt64(p, r, r ^ val)) {
            return r;
        }
    }
}

public static uint64 /*r*/ X__c11_atomic_fetch_xorUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    return X__atomic_fetch_xorUint64(t, ptr, val, 0);
}

public static uint64 /*r*/ X__atomic_fetch_xorUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    uint64 r = default;

    var p = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr);
    for (;;) {
        r = atomic.LoadUint64(p);
        if (atomic.CompareAndSwapUint64(p, r, r ^ val)) {
            return r;
        }
    }
}

public static int8 /*r*/ X__c11_atomic_exchangeInt8(ж<TLS> t, uintptr ptr, int8 val, int32 _) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static void X__atomic_exchangeInt8(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ret)).Value = (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value;
        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<int8>((@unsafe.Pointer)val)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static uint8 /*r*/ X__c11_atomic_exchangeUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 _) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value = val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static void X__atomic_exchangeUint8(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ret)).Value = (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value;
        (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)val)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static int16 /*r*/ X__c11_atomic_exchangeInt16(ж<TLS> t, uintptr ptr, int16 val, int32 _) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static void X__atomic_exchangeInt16(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ret)).Value = (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value;
        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<int16>((@unsafe.Pointer)val)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static uint16 /*r*/ X__c11_atomic_exchangeUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 _) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr);
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value = val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static void X__atomic_exchangeUint16(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ret)).Value = (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value;
        (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)val)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static int32 /*r*/ X__c11_atomic_exchangeInt32(ж<TLS> t, uintptr ptr, int32 val, int32 _) {
    return atomic.SwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr), val);
}

public static void X__atomic_exchangeInt32(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)ret)).Value = atomic.SwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr), ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)val));
}

public static uint32 /*r*/ X__c11_atomic_exchangeUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 _) {
    return (uint32)atomic.SwapInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr), (int32)val);
}

public static void X__atomic_exchangeUint32(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)ret)).Value = atomic.SwapUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr), ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)val));
}

public static int64 /*r*/ X__c11_atomic_exchangeInt64(ж<TLS> t, uintptr ptr, int64 val, int32 _) {
    return atomic.SwapInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr), val);
}

public static void X__atomic_exchangeInt64(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)ret)).Value = atomic.SwapInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr), ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)val));
}

public static uint64 /*r*/ X__c11_atomic_exchangeUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 _) {
    return (uint64)atomic.SwapInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr), (int64)val);
}

public static void X__atomic_exchangeUint64(ж<TLS> t, uintptr ptr, uintptr val, uintptr ret, int32 _) {
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ret)).Value = atomic.SwapUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr), ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)val));
}

public static int32 X__atomic_compare_exchangeInt8(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        var have = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        if (have == ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)expected)) {
            (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<int8>((@unsafe.Pointer)desired)).Value;
            return 1;
        }

        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)expected)).Value = have;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 X__atomic_compare_exchangeUint8(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    return X__atomic_compare_exchangeInt8(t, ptr, expected, desired, weak, success, failure);
}

public static int32 X__atomic_compare_exchangeInt16(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        var have = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        if (have == ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)expected)) {
            (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<int16>((@unsafe.Pointer)desired)).Value;
            return 1;
        }

        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)expected)).Value = have;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 X__atomic_compare_exchangeUint16(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    return X__atomic_compare_exchangeInt16(t, ptr, expected, desired, weak, success, failure);
}

public static int32 X__atomic_compare_exchangeInt32(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    var p = @unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
    var exp = @unsafe.Pointer.To<int32>((@unsafe.Pointer)expected);
    var des = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)desired);
    var old = exp.Value;
    for (;;) {
        if (atomic.CompareAndSwapInt32(p, old, des)) {
            return 1;
        }
        var cur = atomic.LoadInt32(p);
        if (cur != old) {
            exp.Value = cur;
            return 0;
        }
    }
}

public static int32 X__atomic_compare_exchangeUint32(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    var p = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr);
    var exp = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)expected);
    var des = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)desired);
    var old = exp.Value;
    for (;;) {
        if (atomic.CompareAndSwapUint32(p, old, des)) {
            return 1;
        }
        var cur = atomic.LoadUint32(p);
        if (cur != old) {
            exp.Value = cur;
            return 0;
        }
    }
}

public static int32 X__atomic_compare_exchangeInt64(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    var p = @unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
    var exp = @unsafe.Pointer.To<int64>((@unsafe.Pointer)expected);
    var des = ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)desired);
    var old = exp.Value;
    for (;;) {
        if (atomic.CompareAndSwapInt64(p, old, des)) {
            return 1;
        }
        var cur = atomic.LoadInt64(p);
        if (cur != old) {
            exp.Value = cur;
            return 0;
        }
    }
}

public static int32 X__atomic_compare_exchangeUint64(ж<TLS> t, uintptr ptr, uintptr expected, uintptr desired, int32 weak, int32 success, int32 failure) {
    var p = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr);
    var exp = @unsafe.Pointer.To<uint64>((@unsafe.Pointer)expected);
    var des = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)desired);
    var old = exp.Value;
    for (;;) {
        if (atomic.CompareAndSwapUint64(p, old, des)) {
            return 1;
        }
        var cur = atomic.LoadUint64(p);
        if (cur != old) {
            exp.Value = cur;
            return 0;
        }
    }
}

public static int32 X__c11_atomic_compare_exchange_strongInt8(ж<TLS> t, uintptr ptr, uintptr expected, int8 desired, int32 success, int32 failure) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        var have = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
        if (have == ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)expected)) {
            (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = desired;
            return 1;
        }

        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)expected)).Value = have;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 X__c11_atomic_compare_exchange_strongUint8(ж<TLS> t, uintptr ptr, uintptr expected, uint8 desired, int32 success, int32 failure) {
    return X__c11_atomic_compare_exchange_strongInt8(t, ptr, expected, (int8)desired, success, failure);
}

public static int32 X__c11_atomic_compare_exchange_strongInt16(ж<TLS> t, uintptr ptr, uintptr expected, int16 desired, int32 success, int32 failure) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        var have = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
        if (have == ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)expected)) {
            (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = desired;
            return 1;
        }

        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)expected)).Value = have;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 X__c11_atomic_compare_exchange_strongUint16(ж<TLS> t, uintptr ptr, uintptr expected, uint16 desired, int32 success, int32 failure) {
    return X__c11_atomic_compare_exchange_strongInt16(t, ptr, expected, (int16)desired, success, failure);
}

public static int32 X__c11_atomic_compare_exchange_strongInt32(ж<TLS> t, uintptr ptr, uintptr expected, int32 desired, int32 success, int32 failure) {
    GoFrame ᒐ = default;
    try {
        Ꮡint32Mu.Lock();

        defer(Ꮡint32Mu.Unlock, ref ᒐ);

        var have = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr);
        if (have == ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)expected)) {
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr)).Value = desired;
            return 1;
        }

        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)expected)).Value = have;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 X__c11_atomic_compare_exchange_strongUint32(ж<TLS> t, uintptr ptr, uintptr expected, uint32 desired, int32 success, int32 failure) {
    return X__c11_atomic_compare_exchange_strongInt32(t, ptr, expected, (int32)desired, success, failure);
}

public static int32 X__c11_atomic_compare_exchange_strongInt64(ж<TLS> t, uintptr ptr, uintptr expected, int64 desired, int32 success, int32 failure) {
    GoFrame ᒐ = default;
    try {
        Ꮡint64Mu.Lock();

        defer(Ꮡint64Mu.Unlock, ref ᒐ);

        var have = ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr);
        if (have == ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)expected)) {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr)).Value = desired;
            return 1;
        }

        (@unsafe.Pointer.To<int64>((@unsafe.Pointer)expected)).Value = have;
        return 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

public static int32 X__c11_atomic_compare_exchange_strongUint64(ж<TLS> t, uintptr ptr, uintptr expected, uint64 desired, int32 success, int32 failure) {
    return X__c11_atomic_compare_exchange_strongInt64(t, ptr, expected, (int64)desired, success, failure);
}

public static int8 /*r*/ X__c11_atomic_loadInt8(ж<TLS> t, uintptr ptr, int32 memorder) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static void X__atomic_loadInt8(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ret)).Value = (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static uint8 /*r*/ X__c11_atomic_loadUint8(ж<TLS> t, uintptr ptr, int32 memorder) {
    return (uint8)X__c11_atomic_loadInt8(t, ptr, memorder);
}

public static void X__atomic_loadUint8(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    X__atomic_loadInt8(t, ptr, ret, memorder);
}

public static int16 /*r*/ X__c11_atomic_loadInt16(ж<TLS> t, uintptr ptr, int32 memorder) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static void X__atomic_loadInt16(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ret)).Value = (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static uint16 /*r*/ X__c11_atomic_loadUint16(ж<TLS> t, uintptr ptr, int32 memorder) {
    return (uint16)X__c11_atomic_loadInt16(t, ptr, memorder);
}

public static void X__atomic_loadUint16(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    X__atomic_loadInt16(t, ptr, ret, memorder);
}

public static int32 /*r*/ X__c11_atomic_loadInt32(ж<TLS> t, uintptr ptr, int32 memorder) {
    return atomic.LoadInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr));
}

public static void X__atomic_loadInt32(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)ret)).Value = atomic.LoadInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr));
}

public static uint32 /*r*/ X__c11_atomic_loadUint32(ж<TLS> t, uintptr ptr, int32 memorder) {
    return (uint32)X__c11_atomic_loadInt32(t, ptr, memorder);
}

public static void X__atomic_loadUint32(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    X__atomic_loadInt32(t, ptr, ret, memorder);
}

public static int64 /*r*/ X__c11_atomic_loadInt64(ж<TLS> t, uintptr ptr, int32 memorder) {
    return atomic.LoadInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr));
}

public static void X__atomic_loadInt64(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)ret)).Value = atomic.LoadInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr));
}

public static uint64 /*r*/ X__c11_atomic_loadUint64(ж<TLS> t, uintptr ptr, int32 memorder) {
    return (uint64)X__c11_atomic_loadInt64(t, ptr, memorder);
}

public static void X__atomic_loadUint64(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    X__atomic_loadInt64(t, ptr, ret, memorder);
}

public static void X__atomic_loadFloat32(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    X__atomic_loadInt32(t, ptr, ret, memorder);
}

public static void X__atomic_loadFloat64(ж<TLS> t, uintptr ptr, uintptr ret, int32 memorder) {
    X__atomic_loadInt64(t, ptr, ret, memorder);
}

public static void X__c11_atomic_storeInt8(ж<TLS> t, uintptr ptr, int8 val, int32 memorder) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void X__atomic_storeInt8(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<int8>((@unsafe.Pointer)val)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void X__c11_atomic_storeUint8(ж<TLS> t, uintptr ptr, uint8 val, int32 memorder) {
    X__c11_atomic_storeInt8(t, ptr, (int8)val, memorder);
}

public static void X__atomic_storeUint8(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    X__atomic_storeInt8(t, ptr, val, memorder);
}

public static void X__c11_atomic_storeInt16(ж<TLS> t, uintptr ptr, int16 val, int32 memorder) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = val;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void X__atomic_storeInt16(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = (@unsafe.Pointer.To<int16>((@unsafe.Pointer)val)).Value;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void X__c11_atomic_storeUint16(ж<TLS> t, uintptr ptr, uint16 val, int32 memorder) {
    X__c11_atomic_storeInt16(t, ptr, (int16)val, memorder);
}

public static void X__atomic_storeUint16(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    X__atomic_storeInt16(t, ptr, val, memorder);
}

public static void X__c11_atomic_storeInt32(ж<TLS> t, uintptr ptr, int32 val, int32 memorder) {
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr), val);
}

public static void X__atomic_storeInt32(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr), ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)val));
}

public static void X__c11_atomic_storeUint32(ж<TLS> t, uintptr ptr, uint32 val, int32 memorder) {
    X__c11_atomic_storeInt32(t, ptr, (int32)val, memorder);
}

public static void X__atomic_storeUint32(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    X__atomic_storeInt32(t, ptr, val, memorder);
}

public static void X__c11_atomic_storeInt64(ж<TLS> t, uintptr ptr, int64 val, int32 memorder) {
    atomic.StoreInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr), val);
}

public static void X__atomic_storeInt64(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    atomic.StoreInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr), ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)val));
}

public static void X__c11_atomic_storeUint64(ж<TLS> t, uintptr ptr, uint64 val, int32 memorder) {
    X__c11_atomic_storeInt64(t, ptr, (int64)val, memorder);
}

public static void X__atomic_storeUint64(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    X__atomic_storeInt64(t, ptr, val, memorder);
}

public static void X__atomic_storeFloat32(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    X__atomic_storeInt32(t, ptr, val, memorder);
}

public static void X__atomic_storeFloat64(ж<TLS> t, uintptr ptr, uintptr val, int32 memorder) {
    X__atomic_storeInt64(t, ptr, val, memorder);
}

public static int8 /*r*/ X__sync_val_compare_and_swapInt8(ж<TLS> t, uintptr ptr, int8 oldval, int8 newval) {
    int8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<int8>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint8 /*r*/ X__sync_val_compare_and_swapUint8(ж<TLS> t, uintptr ptr, uint8 oldval, uint8 newval) {
    uint8 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint8Mu.Lock();

        defer(Ꮡint8Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int16 /*r*/ X__sync_val_compare_and_swapInt16(ж<TLS> t, uintptr ptr, int16 oldval, int16 newval) {
    int16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<int16>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint16 /*r*/ X__sync_val_compare_and_swapUint16(ж<TLS> t, uintptr ptr, uint16 oldval, uint16 newval) {
    uint16 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint16Mu.Lock();

        defer(Ꮡint16Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int32 /*r*/ X__sync_val_compare_and_swapInt32(ж<TLS> t, uintptr ptr, int32 oldval, int32 newval) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint32Mu.Lock();

        defer(Ꮡint32Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint32 /*r*/ X__sync_val_compare_and_swapUint32(ж<TLS> t, uintptr ptr, uint32 oldval, uint32 newval) {
    uint32 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint32Mu.Lock();

        defer(Ꮡint32Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static int64 /*r*/ X__sync_val_compare_and_swapInt64(ж<TLS> t, uintptr ptr, int64 oldval, int64 newval) {
    int64 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint64Mu.Lock();

        defer(Ꮡint64Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static uint64 /*r*/ X__sync_val_compare_and_swapUint64(ж<TLS> t, uintptr ptr, uint64 oldval, uint64 newval) {
    uint64 r = default;
    GoFrame ᒐ = default;
    try {
        Ꮡint64Mu.Lock();

        defer(Ꮡint64Mu.Unlock, ref ᒐ);

        r = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr); if (r == oldval) {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ptr)).Value = newval;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

} // end libc_package
