// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using runtime = runtime_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
using @unsafe = unsafe_package;
using errno = go.modernc.org.libc.errno_package;
using signal = go.modernc.org.libc.signal_package;
using types = go.modernc.org.libc.sys.types_package;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;
using go.sync;
using path;
using ꓸꓸꓸany = Span<any>;
using static libc_package;

partial class libc_package {

internal static uintptr allocatorPageOverhead => /* 4 * unsafe.Sizeof(int(0)) */ 32;
internal static uintptr stackHeaderSize => /* unsafe.Sizeof(stackHeader{}) */ 48;
internal static uintptr stackSegmentSize => /* 1<<12 - allocatorPageOverhead */ 4064;
internal static uintptr uintptrSize => /* unsafe.Sizeof(uintptr(0)) */ 8;

public static readonly ж<map<uintptr, EmptyStruct>> ᏑCovered = new StaticBox<map<uintptr, EmptyStruct>>(new map<uintptr, EmptyStruct>{});
public static ref map<uintptr, EmptyStruct> Covered => ref ((StaticBox<map<uintptr, EmptyStruct>>)ᏑCovered).ValueSlot;
public static readonly ж<map<@string, EmptyStruct>> ᏑCoveredC = new StaticBox<map<@string, EmptyStruct>>(new map<@string, EmptyStruct>{});
public static ref map<@string, EmptyStruct> CoveredC => ref ((StaticBox<map<@string, EmptyStruct>>)ᏑCoveredC).ValueSlot;
internal static readonly ж<uintptr> ᏑfToken = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr fToken => ref ((StaticBox<uintptr>)ᏑfToken).ValueSlot;
internal static readonly ж<int32> Ꮡtid = new StaticBox<int32>(default(int32));
internal static ref int32 tid => ref ((StaticBox<int32>)Ꮡtid).ValueSlot;
internal static slice<Action> atExit;
internal static readonly ж<sync.Mutex> ᏑatExitMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex atExitMu => ref ((StaticBox<sync.Mutex>)ᏑatExitMu).ValueSlot;
internal static array<uintptr> signals = new(23);
internal static sync.Mutex signalsMu;
internal static readonly ж<sync.Mutex> ᏑobjectMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex objectMu => ref ((StaticBox<sync.Mutex>)ᏑobjectMu).ValueSlot;
internal static map<uintptr, any> objects = new map<uintptr, any>{};
internal static readonly ж<int32> ᏑtlsBalance = new StaticBox<int32>(default(int32));
internal static ref int32 tlsBalance => ref ((StaticBox<int32>)ᏑtlsBalance).ValueSlot;
internal static Func<nint, @string> _ᴛ1ʗ = origin;
internal static Funcꓸꓸꓸ<@string, any, @string> _ᴛ2ʗ = trc;

[GoInit] internal static void init() {
    var n = stackHeaderSize; if (n % 16 != 0) {
        throw panic(fmt.Errorf("internal error: stackHeaderSize %v == %v (mod 16)"u8, n, n % 16));
    }
}

internal static partial @string origin(nint skip) {
    var (pc, fn, fl, _) = runtime.Caller(skip);
    var f = runtime.FuncForPC(pc);
    @string fns = default;
    if (f != nil) {
        fns = f.Name();
        nint x = strings.LastIndex(fns, "."); if (x > 0) {
            fns = fns.slice(x + 1);
        }
    }
    return fmt.Sprintf("%s:%d:%s"u8, filepath.Base(fn), fl, fns);
}

internal static partial @string trc(@string s, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    switch (ᐧ) {
    case {} when s == ""u8: {
        s = fmt.Sprintf(strings.Repeat("%v ", len(args)), args.ꓸꓸꓸ);
        break;
    }
    default: {
        s = fmt.Sprintf(s, args.ꓸꓸꓸ);
        break;
    }}

    @string r = fmt.Sprintf("%s: TRC %s"u8, origin(2), s);
    fmt.Fprintf(new os.FileжWriter(os.Stdout), "%s\n"u8, r);
    os.Stdout.Sync();
    return r;
}

internal static partial @string todo(@string s, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    switch (ᐧ) {
    case {} when s == ""u8: {
        s = fmt.Sprintf(strings.Repeat("%v ", len(args)), args.ꓸꓸꓸ);
        break;
    }
    default: {
        s = fmt.Sprintf(s, args.ꓸꓸꓸ);
        break;
    }}

    @string r = fmt.Sprintf("%s: TODOTODO %s"u8, origin(2), s);
    if (dmesgs) {
        dmesg("%s", r);
    }
    fmt.Fprintf(new os.FileжWriter(os.Stdout), "%s\n"u8, r);
    os.Stdout.Sync();
    os.Exit(1);
    throw panic("unrechable");
}

internal static array<uintptr> coverPCs = new(1);

public static partial void Cover() {
    runtime.Callers(2, coverPCs[..]);
    Covered[coverPCs[0]] = new EmptyStruct();
}

public static error CoverReport(io.Writer w) {
    var aᴷ = new stackAppend<@string>();

    slice<@string> a = default;
    var pcs = new slice<uintptr>(1);
    foreach (var (pc, _) in Covered) {
        pcs[0] = pc;
        var (frame, _) = runtime.CallersFrames(pcs).Next();
        a = append(aᴷ, a, fmt.Sprintf("%s:%07d:%s"u8, filepath.Base(frame.File), frame.Line, frame.Func.Name()));
    }
    sort.Strings(a);
    var (_, err) = fmt.Fprintf(w, "%s\n"u8, strings.Join(a, "\n"));
    return err;
}

public static void CoverC(@string s) {
    CoveredC[s] = new EmptyStruct();
}

public static error CoverCReport(io.Writer w) {
    var aᴷ = new stackAppend<@string>();

    slice<@string> a = default;
    foreach (var (k, _) in CoveredC) {
        a = append(aᴷ, a, k);
    }
    sort.Strings(a);
    var (_, err) = fmt.Fprintf(w, "%s\n"u8, strings.Join(a, "\n"));
    return err;
}

internal static uintptr token() { return atomic.AddUintptr(ᏑfToken, 1); }

internal static uintptr addObject(any o) {
    var t = token();
    ᏑobjectMu.Lock();
    objects[t] = o;
    ᏑobjectMu.Unlock();
    return t;
}

internal static any getObject(uintptr t) {
    GoFrame ᒐ = default;
    try {
        ᏑobjectMu.Lock();
        defer(ᏑobjectMu.Unlock, ref ᒐ);
        var o = objects[t];
        if (o == nil) {
            throw panic(todo("", t));
        }

        return o;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { ᒐ.Run(); }
}

internal static void removeObject(uintptr t) {
    GoFrame ᒐ = default;
    try {
        ᏑobjectMu.Lock();
        defer(ᏑobjectMu.Unlock, ref ᒐ);
        var (_, ok) = objects[t, ꟷ]; if (!ok) {
            throw panic(todo(""));
        }

        delete(objects, t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} partial class libcᴛTLS {

internal static void setErrno(this ж<TLS> Ꮡt, any err) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Ꮡt == nil) {
            throw panic("nil TLS");
        }

        if (memgrind) {
            if (atomic.SwapInt32(Ꮡt.ᏑreentryGuard, 1) != 0) {
                throw panic(todo("concurrent use of TLS instance %p", Ꮡt.OrTypedNil()));
            }

            defer(() => {
                if (atomic.SwapInt32(Ꮡt.ᏑreentryGuard, 0) != 1) {
                    throw panic(todo("concurrent use of TLS instance %p", Ꮡt.OrTypedNil()));
                }
            }, ref ᒐ);
        }
again:
        switch (err.type()) {
        case nint x: {
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)t.errnop)).Value = (int32)x;
            break;
        }
        case int32 x: {
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)t.errnop)).Value = x;
            break;
        }
        case ж<osꓸPathError> x: {
            err = x.Err;
            goto again;
            break;
        }
        case syscallErrno x: {
            (@unsafe.Pointer.To<int32>((@unsafe.Pointer)t.errnop)).Value = (int32)x;
            break;
        }
        case ж<os.SyscallError> x: {
            err = x.Err;
            goto again;
            break;
        }
        default: {
            var x = err;
            throw panic(todo("%T", x));
            break;
        }}
        break_again:;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void Close(this ж<TLS> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, p) in range(t.allocas)) {
        Xfree(Ꮡt, p);
    }
    foreach (var (_, a) in range(t.allocaStack)) {
        foreach (var (_, p) in range(a)) {
            Xfree(Ꮡt, p);
        }
    }
    if (t.stack.page != 0) {
        (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.page)).Value = t.stack;
        var p = t.stack.page;
        for (var q = t.stack.prev; q != 0; q = (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)q)).Value.prev) {
            p = q;
        }
        while (p != 0) {
            var next = (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)p)).Value.next;
            if (memgrind) {
                if (atomic.AddInt32(Ꮡt.ᏑstackHeaderBalance, -1) < 0) {
                    throw panic(todo("negative stack header balance"));
                }
            }
            Xfree(Ꮡt, p);
            p = next;
        }
    }
    if (memgrind) {
        if (t.stackHeaderBalance != 0) {
            throw panic(todo("non zero stack header balance: %d", t.stackHeaderBalance));
        }

        atomic.AddInt32(ᏑtlsBalance, -1);
    }
    t.pthreadData.close(Ꮡt);
    t = new TLS();
}

public static uintptr /*r*/ Alloc(this ж<TLS> Ꮡt, nint n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.sp++;
        if (memgrind) {
            if (atomic.SwapInt32(Ꮡt.ᏑreentryGuard, 1) != 0) {
                throw panic(todo("concurrent use of TLS instance %p", Ꮡt.OrTypedNil()));
            }

            defer(() => {
                if (atomic.SwapInt32(Ꮡt.ᏑreentryGuard, 0) != 1) {
                    throw panic(todo("concurrent use of TLS instance %p", Ꮡt.OrTypedNil()));
                }
            }, ref ᒐ);
        }
        n += 15;
        n &= ~(nint)(15);
        if (t.stack.free >= n) {
            r = t.stack.sp;
            t.stack.free -= n;
            t.stack.sp += (uintptr)n;
            goto ᒐdone;
        }
        var nstack = t.stack.next; if (nstack != 0) {
            if ((@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)nstack)).Value.free >= n) {
                (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.page)).Value = t.stack;
                t.stack = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)nstack);
                r = t.stack.sp;
                t.stack.free -= n;
                t.stack.sp += (uintptr)n;
                goto ᒐdone;
            }
            var nstackΔ1 = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.next);
            for (; ; nstackΔ1 = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)nstackΔ1.next)) {
                if (memgrind) {
                    if (atomic.AddInt32(Ꮡt.ᏑstackHeaderBalance, -1) < 0) {
                        throw panic(todo("negative stack header balance"));
                    }
                }
                Xfree(Ꮡt, nstackΔ1.page);
                if (nstackΔ1.next == 0) {
                    break;
                }
            }
            t.stack.next = 0;
        }

        if (t.stack.page != 0) {
            (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.page)).Value = t.stack;
        }

        nint rq = n + (nint)stackHeaderSize;
        if (rq % (nint)stackSegmentSize != 0) {
            rq -= rq % (nint)stackSegmentSize;
            rq += (nint)stackSegmentSize;
        }
        t.stack.free = rq - (nint)stackHeaderSize;
        t.stack.prev = t.stack.page;

        rq += 15;
        rq &= ~(nint)(15);
        t.stack.page = Xmalloc(Ꮡt, (typesꓸSize_t)rq);
        if (t.stack.page == 0) {
            throw panic("OOM");
        }

        if (memgrind) {
            atomic.AddInt32(Ꮡt.ᏑstackHeaderBalance, 1);
        }
        t.stack.sp = t.stack.page + stackHeaderSize;

        r = t.stack.sp;
        t.stack.free -= n;
        t.stack.sp += (uintptr)n;
        if (t.stack.prev != 0) {
            (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.prev)).Value.next = t.stack.page;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

} partial class libc_package {

internal static UntypedInt stackFrameKeepalive => 2;

} partial class libcᴛTLS {

public static void Free(this ж<TLS> Ꮡt, nint n) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        t.sp--;
        if (memgrind) {
            if (atomic.SwapInt32(Ꮡt.ᏑreentryGuard, 1) != 0) {
                throw panic(todo("concurrent use of TLS instance %p", Ꮡt.OrTypedNil()));
            }

            defer(() => {
                if (atomic.SwapInt32(Ꮡt.ᏑreentryGuard, 0) != 1) {
                    throw panic(todo("concurrent use of TLS instance %p", Ꮡt.OrTypedNil()));
                }
            }, ref ᒐ);
        }
        n += 15;
        n &= ~(nint)(15);
        t.stack.free += n;
        t.stack.sp -= (uintptr)n;
        if (t.stack.sp != t.stack.page + stackHeaderSize) {
            return;
        }

        var nstack = t.stack;

        if (t.stack.prev == 0) {
            for (; ; nstack = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)nstack.next)) {
                if (memgrind) {
                    if (atomic.AddInt32(Ꮡt.ᏑstackHeaderBalance, -1) < 0) {
                        throw panic(todo("negative stack header balance"));
                    }
                }
                Xfree(Ꮡt, nstack.page);
                if (nstack.next == 0) {
                    break;
                }
            }
            t.stack = new stackHeader();
            return;
        }

        for (nint i = 0; i < stackFrameKeepalive; i++) {
            if (nstack.next == 0) {
                (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.page)).Value = t.stack;
                t.stack = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.prev);
                return;
            }
            nstack = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)nstack.next);
        }

        if (memgrind) {
            if (atomic.AddInt32(Ꮡt.ᏑstackHeaderBalance, -1) < 0) {
                throw panic(todo("negative stack header balance"));
            }
        }
        Xfree(Ꮡt, nstack.page);
        (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)nstack.prev)).Value.next = 0;
        (@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.page)).Value = t.stack;
        t.stack = ~@unsafe.Pointer.To<stackHeader>((@unsafe.Pointer)t.stack.prev);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} partial class libc_package {

partial struct stackHeader {
    internal nint free;
    internal uintptr page;
    internal uintptr prev;
    internal uintptr next;
    internal uintptr sp;
    internal stackHeaderPadding _;
}

internal static uintptr cString(ж<TLS> t, @string s) {
    nint n = len(s);
    var p = Xmalloc(t, ((typesꓸSize_t)n) + 1);
    if (p == 0) {
        throw panic("OOM");
    }

    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623), s);
    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + (uintptr)n))).Value = 0;
    return p;
}

public static uintptr /*r*/ VaList(uintptr p, params ꓸꓸꓸany argsʗp) {
    uintptr r = default;
    var args = argsʗp.sslice();

    if ((uintptr)(p & 7) != 0) {
        throw panic("internal error");
    }

    r = p;
    foreach (var (_, vᴛ1) in range(args)) {
        ref var v = ref heap<any>(out var Ꮡv);
        v = vᴛ1;

        switch (v.type()) {
        case nint x: {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = (int64)x;
            break;
        }
        case int32 x: {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = (int64)x;
            break;
        }
        case int64 x: {
            (@unsafe.Pointer.To<int64>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case nuint x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = (uint64)x;
            break;
        }
        case uint16 x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = (uint64)x;
            break;
        }
        case uint32 x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = (uint64)x;
            break;
        }
        case uint64 x: {
            (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case float64 x: {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case uintptr x: {
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value = x;
            break;
        }
        case @string x: {
            throw panic("libc.VaList: a Go string is not a valid C variadic argument, use CString");
            break;
        }
        default: {
            var x = v;
            var sz = reflect.TypeOf(v).Size();
            copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)p), sz), @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)@unsafe.Pointer.ElementRef<uintptr>(@unsafe.Pointer.FromPinnedBox(Ꮡv), 1, 2)), sz));
            p += roundup(sz, 8);
            continue;
            break;
        }}
        p += 8;
    }
    return r;
}

public static partial uintptr /*va_list*/ NewVaListN(nint n) {
    return Xmalloc(nil, (typesꓸSize_t)(8 * n));
}

public static partial uintptr /*va_list*/ NewVaList(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return VaList(NewVaListN(len(args)), args.ꓸꓸꓸ);
}

public static uintptr /*r*/ VaOther(ж<uintptr> app, uint64 sz) {
    uintptr r = default;

    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    r = ap;
    ap = roundup(ap + (uintptr)sz, 8);
    (app).Value = ap;
    return r;
}

public static int32 VaInt32(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = (int32)(~@unsafe.Pointer.To<int64>((@unsafe.Pointer)ap));
    ap += 8;
    (app).Value = ap;
    return v;
}

public static uint32 VaUint32(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = (uint32)(~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ap));
    ap += 8;
    (app).Value = ap;
    return v;
}

public static int64 VaInt64(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<int64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static uint64 VaUint64(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static float32 VaFloat32(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0F;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return (float32)v;
}

public static float64 VaFloat64(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0D;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

public static uintptr VaUintptr(ж<uintptr> app) {
    var ap = ~app;
    if (ap == 0) {
        return 0;
    }

    ap = roundup(ap, 8);
    var v = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)ap);
    ap += 8;
    (app).Value = ap;
    return v;
}

internal static slice<@string> getVaList(uintptr va) {
    var r = slice<@string>([]);

    for (var p = va; ; p += 8) {
        var st = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
        if (st == 0) {
            return r;
        }
        r = append(r, GoString(st));
    }
    return r;
}

internal static uintptr roundup(uintptr n, uintptr to) {
    var r = n % to; if (r != 0) {
        return n + to - r;
    }

    return n;
}

public static bool Bool(bool v) { return v; }

public static int32 Bool32(bool b) {
    if (b) {
        return 1;
    }

    return 0;
}

public static int64 Bool64(bool b) {
    if (b) {
        return 1;
    }

    return 0;
}

public static (uintptr, error) CString(@string s) {
    nint n = len(s);
    var p = Xmalloc(nil, ((typesꓸSize_t)n) + 1);
    if (p == 0) {
        return (0, fmt.Errorf("CString: cannot allocate %d bytes"u8, n + 1));
    }

    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)p, 0, n, n, 1125899906842623), s);
    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + (uintptr)n))).Value = 0;
    return (p, default);
}

public static slice<@string> /*r*/ GetEnviron() {
    slice<@string> r = default;

    for (var p = Environ(); ; p += /* unsafe.Sizeof(p) */ (uintptr)8) {
        var q = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
        if (q == 0) {
            return r;
        }

        r = append(r, GoString(q));
    }
}

internal static (bool seenDigits, bool neg, uintptr next, uint64 n, int32 err) strToUint64(ref TLS t, uintptr s, int32 @base) {
    bool seenDigits = default;
    bool neg = default;
    uint64 n = default;

    byte c = default;
@out:
    for (;;) {
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        switch (c) {
        case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
            s++;
            break;
        }
        case (byte)'+': {
            s++;
            goto break_out;
            break;
        }
        case (byte)'-': {
            s++;
            neg = true;
            goto break_out;
            break;
        }
        default: {
            goto break_out;
            break;
        }}

continue_out:;
    }
break_out:;
    for (;;) {
        c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        uint64 digit = default;
        switch (@base) {
        case 10: {
            switch (ᐧ) {
            case {} when c >= (byte)'0' && c <= (byte)'9': {
                seenDigits = true;
                digit = (uint64)c - '0';
                break;
            }
            default: {
                return (seenDigits, neg, s, n, 0);
            }}

            break;
        }
        case 16: {
            if (c >= 'A' && c <= 'F') {
                c = (byte)(c + ((rune)'a' - (rune)'A'));
            }
            switch (ᐧ) {
            case {} when c >= (byte)'0' && c <= (byte)'9': {
                seenDigits = true;
                digit = (uint64)c - '0';
                break;
            }
            case {} when c >= (byte)'a' && c <= (byte)'f': {
                seenDigits = true;
                digit = (uint64)c - 'a' + 10;
                break;
            }
            default: {
                return (seenDigits, neg, s, n, 0);
            }}

            break;
        }
        default: {
            throw panic(todo("", @base));
            break;
        }}

        var n0 = n;
        n = (uint64)@base * n + digit;
        if (n < n0) {
            return (seenDigits, neg, s, n0, errno.ERANGE);
        }

        s++;
    }
}

internal static (float64 n, int32 errno) strToFloatt64(ref TLS t, uintptr s, nint bits) {
    float64 n = default;
    int32 errno = default;
    GoFrame ᒐ = default;
    try {
        ref var b = ref heap<slice<byte>>(out var Ꮡb);
        bool neg = default;

        defer(() => {
            error err = default;
            (n, err) = strconv.ParseFloat((@string)Ꮡb.ValueSlot, bits); if (err != nil) {
                throw panic(todo(""));
            }

            if (neg) {
                n = -n;
            }
        }, ref ᒐ);

        byte c = default;
@out:
        for (;;) {
            c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
            switch (c) {
            case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
                s++;
                break;
            }
            case (byte)'+': {
                s++;
                goto break_out;
                break;
            }
            case (byte)'-': {
                s++;
                neg = true;
                goto break_out;
                break;
            }
            default: {
                goto break_out;
                break;
            }}

continue_out:;
        }
break_out:;
        for (;;) {
            c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
            switch (ᐧ) {
            case {} when c >= (byte)'0' && c <= (byte)'9': {
                b = append(b, c);
                break;
            }
            case {} when c is (byte)'.': {
                b = append(b, c);
                s++;
                for (;;) {
                    c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
                    switch (ᐧ) {
                    case {} when c >= (byte)'0' && c <= (byte)'9': {
                        b = append(b, c);
                        break;
                    }
                    case {} when c == (byte)'e' || c == (byte)'E': {
                        b = append(b, c);
                        s++;
                        for (;;) {
                            c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
                            switch (ᐧ) {
                            case {} when c == (byte)'+' || c == (byte)'-': {
                                b = append(b, c);
                                s++;
                                for (;;) {
                                    c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
                                    switch (ᐧ) {
                                    case {} when c >= (byte)'0' && c <= (byte)'9': {
                                        b = append(b, c);
                                        break;
                                    }
                                    default: {
                                        goto ᒐdone;
                                    }}

                                    s++;
                                }
                                break;
                            }
                            default: {
                                throw panic(todo("%q %q", b, (@string)c));
                                break;
                            }}

                        }
                        break;
                    }
                    default: {
                        goto ᒐdone;
                    }}

                    s++;
                }
                break;
            }
            default: {
                throw panic(todo("%q %q", b, (@string)c));
                break;
            }}

            s++;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, errno);
}

internal static (@string name, nint off, bool ok) parseZone(@string s) {
    nint off = default;
    bool ok = default;

    nint i = 0;
    for (; i < len(s); i++) {
        var c = s[i]; if (!(c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_' || c == '/')) {
            break;
        }
    }
    if (i == len(s)) {
        return (s, 0, true);
    }

    if (i < 3) {
        return ("", 0, false);
    }

    (_, off, ok) = parseOffset(s.slice(i)); if (!ok) {
        return ("", 0, false);
    }

    return (s.slice(0, i), off, true);
}

internal static (@string, nint, bool) parseOffset(@string s) {
    nint k = 1;
    if (len(s) != 0) {
        switch (s[0]) {
        case (byte)'+': {
            s = s[1..];
            break;
        }
        case (byte)'-': {
            k = -1;
            s = s[1..];
            break;
        }}

    }
    (s, var hh, var ok) = parseUint(s);
    if (!ok) {
        return (s, 0, false);
    }

    nint n = hh * 3600;
    if (len(s) == 0 || s[0] != ':') {
        return (s, k * n, true);
    }

    (s, var mm, ok) = parseUint(s[1..]);
    if (!ok) {
        return (s, 0, false);
    }

    n += mm * 60;
    if (len(s) == 0 || s[0] != ':') {
        return (s, k * n, true);
    }

    s = s[1..];
    if (len(s) == 0) {
        return (s, 0, false);
    }

    (s, var ss, _) = parseUint(s);
    return (s, k * (n + ss), true);
}

internal static (@string, nint, bool) parseUint(@string s) {
    bool ok = default;
    nint r = default;
    while (len(s) != 0 && s[0] >= '0' && s[0] <= '9') {
        r = 10 * r + (nint)((byte)(s[0] - '0')); if (r > 0xffff) {
            return (s, 0, false);
        }

        ok = true;
        s = s[1..];
    }
    return (s, r, ok);
}

internal static bool isTimeDST(time.Time t) {
    return t.IsDST();
}

} // end libc_package
