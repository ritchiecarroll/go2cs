// Copyright 2021 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using math = math_package;
using os = os_package;
using sync = sync_package;
using @unsafe = unsafe_package;
using io = io_package;
using static libc_package;

partial class libc_package {

internal static map<uintptr, watch> watches = new map<uintptr, watch>{};
internal static readonly ж<sync.Mutex> ᏑwatchesMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex watchesMu => ref ((StaticBox<sync.Mutex>)ᏑwatchesMu).ValueSlot;

partial interface watch {
    @string msg();
}

partial struct watcher /*@string*/;

} partial class libcᴛwatcher {

internal static @string msg(this watcher w) {
    if (w == ""u8) {
        return "";
    }

    return fmt.Sprintf(": %s"u8, w);
}

} partial class libc_package {

partial struct watchInt8 {
    internal int8 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchInt8(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchInt8жwatch(Ꮡ(new watchInt8(~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchUint8 {
    internal uint8 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchUint8(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchUint8жwatch(Ꮡ(new watchUint8(~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchInt16 {
    internal int16 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchInt16(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchInt16жwatch(Ꮡ(new watchInt16(~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchUint16 {
    internal uint16 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchUint16(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchUint16жwatch(Ꮡ(new watchUint16(~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchInt32 {
    internal int32 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchInt32(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchInt32жwatch(Ꮡ(new watchInt32(~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchUint32 {
    internal uint32 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchUint32(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchUint32жwatch(Ꮡ(new watchUint32(~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchInt64 {
    internal int64 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchInt64(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchInt64жwatch(Ꮡ(new watchInt64(~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchUint64 {
    internal uint64 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchUint64(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchUint64жwatch(Ꮡ(new watchUint64(~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchFloat32 {
    internal float32 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchFloat32(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchFloat32жwatch(Ꮡ(new watchFloat32(~@unsafe.Pointer.To<float32>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchFloat64 {
    internal float64 val;
    /*embed*/ internal watcher watcher;
}

public static void WatchFloat64(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchFloat64жwatch(Ꮡ(new watchFloat64(~@unsafe.Pointer.To<float64>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

partial struct watchPtr {
    internal uintptr val;
    /*embed*/ internal watcher watcher;
}

public static void WatchPtr(uintptr p, @string msg) {
    ᏑwatchesMu.Lock();
    watches[p] = new watchPtrжwatch(Ꮡ(new watchPtr(~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p), (watcher)msg)));
    ᏑwatchesMu.Unlock();
}

public static partial void Watch() {
    ᏑwatchesMu.Lock();
    var flush = false;
    foreach (var (p, v) in watches) {
        switch (v.type()) {
        case ж<watchInt8> x: {
            {
                var val = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: int8@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchUint8> x: {
            {
                var val = ~@unsafe.Pointer.To<uint8>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: uint8@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchInt16> x: {
            {
                var val = ~@unsafe.Pointer.To<int16>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: int16@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchUint16> x: {
            {
                var val = ~@unsafe.Pointer.To<uint16>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: uint16@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchInt32> x: {
            {
                var val = ~@unsafe.Pointer.To<int32>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: int32@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchUint32> x: {
            {
                var val = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: uint32@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchInt64> x: {
            {
                var val = ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: int64@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchUint64> x: {
            {
                var val = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: uint64@%#x was %d, new %d%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchFloat32> x: {
            {
                var val = ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)p); if (math.Float32bits(val) != math.Float32bits(x.val)) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: float32@%#x was %v(%#x), new %v(%#x)%s\n"u8, origin(2), p, x.val, math.Float32bits(x.val), val, math.Float32bits(val), x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchFloat64> x: {
            {
                var val = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)p); if (math.Float64bits(val) != math.Float64bits(x.val)) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: float64@%#x was %v(%#x), new %v(%#x)%s\n"u8, origin(2), p, x.val, math.Float64bits(x.val), val, math.Float64bits(val), x.msg());
                    x.val = val;
                }
            }
            break;
        }
        case ж<watchPtr> x: {
            {
                var val = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p); if (val != x.val) {
                    flush = true;
                    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%v: ptr@%#x was %#x, new %#x%s\n"u8, origin(2), p, x.val, val, x.msg());
                    x.val = val;
                }
            }
            break;
        }
        default: {
            var x = v;
            throw panic(todo("%T", x));
            break;
        }}
    }
    if (flush) {
        os.Stderr.Sync();
    }
    ᏑwatchesMu.Unlock();
}

public static void WatchDelete(uintptr p) {
    ᏑwatchesMu.Lock();
    delete(watches, p);
    ᏑwatchesMu.Unlock();
}

} // end libc_package
