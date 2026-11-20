// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

global using Tdouble_t = double;
global using Tuint32_t = uint;
global using Tuint64_t = ulong;
global using Tfloat_t = float;

namespace go.modernc.org;

using bufio = bufio_package;
using bytes = bytes_package;
using crand = crypto.rand_package;
using fmt = fmt_package;
using math = math_package;
using mbits = go.math.bits_package;
using rand = go.math.rand_package;
using os = os_package;
using runtime = runtime_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using gotime = time_package;
using @unsafe = unsafe_package;
using isatty = github.com.mattn.go_isatty.isatty_package;
using errno = go.modernc.org.libc.errno_package;
using stdio = go.modernc.org.libc.stdio_package;
using types = go.modernc.org.libc.sys.types_package;
using time = go.modernc.org.libc.time_package;
using unistd = go.modernc.org.libc.unistd_package;
using mathutil = go.modernc.org.mathutil_package;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using github.com.mattn.go_isatty;
using go.golib;
using go.math;
using go.modernc.org;
using go.modernc.org.libc;
using go.modernc.org.libc.sys;
using go.runtime;
using go.sync;
using io = io_package;
using ꓸꓸꓸany = Span<any>;

partial class libc_package {

public static UntypedInt ENOENT => /* errno.ENOENT */ 2;

/* [ unsafe.Sizeof(RawMem{})  (uintptr)1125899906842623 /  unsafe.Sizeof(uint64(0))  (uintptr)8]uint64 */
partial struct RawMem64 /*[140737488355327]uint64*/;

internal static readonly ж<sync.Mutex> ᏑallocMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex allocMu => ref ((StaticBox<sync.Mutex>)ᏑallocMu).ValueSlot;
internal static bool environInitialized;
internal static bool isWindows;
internal static sync.Mutex ungetcMu;
internal static map<uintptr, byte> ungetc = new map<uintptr, byte>{};

public static readonly ж<uintptr> ᏑXenviron = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr Xenviron => ref ((StaticBox<uintptr>)ᏑXenviron).ValueSlot;
public static readonly ж<uintptr> ᏑXstdin = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr Xstdin => ref ((StaticBox<uintptr>)ᏑXstdin).ValueSlot;
internal static void initᴛXstdin() { Xstdin = newFile(ref ((ж<TLS>)default).DerefOrNull(), unistd.STDIN_FILENO); }
public static readonly ж<uintptr> ᏑXstdout = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr Xstdout => ref ((StaticBox<uintptr>)ᏑXstdout).ValueSlot;
internal static void initᴛXstdout() { Xstdout = newFile(ref ((ж<TLS>)default).DerefOrNull(), unistd.STDOUT_FILENO); }
public static readonly ж<uintptr> ᏑXstderr = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr Xstderr => ref ((StaticBox<uintptr>)ᏑXstderr).ValueSlot;
internal static void initᴛXstderr() { Xstderr = newFile(ref ((ж<TLS>)default).DerefOrNull(), unistd.STDERR_FILENO); }

internal static partial void setEnviron() {
    SetEnviron(nil, os.Environ());
}

public static partial uintptr Environ() {
    if (!environInitialized) {
        SetEnviron(nil, os.Environ());
    }
    return Xenviron;
}

public static partial uintptr EnvironP() {
    if (!environInitialized) {
        SetEnviron(nil, os.Environ());
    }
    return (uintptr)@unsafe.Pointer.FromBox(ᏑXenviron);
}

public static partial uintptr X___errno_location(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return X__errno_location(t);
}

public static partial uintptr X__errno_location(ж<TLS> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (__ccgo_strace) {
        trc("t=%v, (%v:)", Ꮡt.OrTypedNil(), origin(2));
    }
    return t.errnop;
}

public static void Start(Func<ж<TLS>, int32, uintptr, int32> main) {
    GoFrame ᒐ = default;
    try {
        if (dmesgs) {
            var (wd, err) = os.Getwd();
            dmesg("%v: %v, wd %v, %v", origin(1), os.Args, wd, err);

            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                var errΔ1 = recover(); if (errΔ1 != nil) {
                    dmesg("%v: CRASH: %v\n%s", origin(1), errΔ1, debug.Stack());
                }
            }, ref ᒐ);
        }
        runtime.LockOSThread();
        var t = Ꮡ(new TLS(errnop: (uintptr)Ꮡerrno0));
        var argv = Xcalloc(t, 1, (typesꓸSize_t)((len(os.Args) + 1) * (nint)uintptrSize));
        if (argv == 0) {
            throw panic("OOM");
        }

        var p = argv;
        foreach (var (_, v) in range(os.Args)) {
            var s = Xcalloc(t, 1, (typesꓸSize_t)(len(v) + 1));
            if (s == 0) {
                throw panic("OOM");
            }

            copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)s, 0, len(v), len(v), 1125899906842623), v);
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value = s;
            p += uintptrSize;
        }
        SetEnviron(t, os.Environ());
        var audit = false;
        if (memgrind) {
            @string s = os.Getenv("LIBC_MEMGRIND_START"); if (s != "0"u8) {
                MemAuditStart();
                audit = true;
            }
        }
        t = NewTLS();
        var rc = main(t, (int32)len(os.Args), argv);
        exit(t, rc, audit);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void Xexit(ж<TLS> t, int32 status) {
    if (__ccgo_strace) {
        trc("t=%v status=%v, (%v:)", t.OrTypedNil(), status, origin(2));
    }
    exit(t, status, false);
}

internal static void exit(ж<TLS> t, int32 status, bool audit) {
    if (len(Covered) != 0) {
        var buf = bufio.NewWriter(new os.FileжWriter(os.Stdout));
        CoverReport(new bufio_WriterжWriter(buf));
        buf.Flush();
    }
    if (len(CoveredC) != 0) {
        var buf = bufio.NewWriter(new os.FileжWriter(os.Stdout));
        CoverCReport(new bufio_WriterжWriter(buf));
        buf.Flush();
    }
    for (nint i = len(atExit) - 1; i >= 0; i--) {
        atExit[i]();
    }
    if (audit) {
        t.Close();
        if (tlsBalance != 0) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "non zero TLS balance: %d\n"u8, tlsBalance);
            status = 1;
        }
    }
    X_exit(nil, status);
}

public static partial void X_exit(ж<TLS> _, int32 status) {
    if (dmesgs) {
        dmesg("%v: EXIT %v", origin(1), status);
    }
    os.Exit((nint)status);
}

public static void SetEnviron(ж<TLS> t, slice<@string> env) {
    if (environInitialized) {
        return;
    }

    environInitialized = true;
    var p = Xcalloc(t, 1, (typesꓸSize_t)((len(env) + 1) * ((nint)uintptrSize)));
    if (p == 0) {
        throw panic("OOM");
    }

    Xenviron = p;
    foreach (var (_, v) in range(env)) {
        var s = Xcalloc(t, 1, (typesꓸSize_t)(len(v) + 1));
        if (s == 0) {
            throw panic("OOM");
        }

        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)s, 0, len(v), len(v), 1125899906842623), v);
        (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p)).Value = s;
        p += uintptrSize;
    }
}

public static partial void Xsetbuf(ж<TLS> t, uintptr stream, uintptr buf) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v, (%v:)", t.OrTypedNil(), buf, origin(2));
    }
}

public static partial int32 Xputs(ж<TLS> t, uintptr s) {
    if (__ccgo_strace) {
        trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
    }
    var (n, err) = fmt.Printf("%s\n"u8, GoString(s));
    if (err != nil) {
        return stdio.EOF;
    }

    return (int32)n;
}

internal static readonly ж<sync.Mutex> ᏑrandomMu = new StaticBox<sync.Mutex>(default(sync.Mutex));
internal static ref sync.Mutex randomMu => ref ((StaticBox<sync.Mutex>)ᏑrandomMu).ValueSlot;
internal static ж<rand.Rand> randomGen = rand.New(rand.NewSource(42));

public static partial @long Xrandom(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    ᏑrandomMu.Lock();
    var r = randomGen.Int63n(2147483648L);
    ᏑrandomMu.Unlock();
    return (@long)r;
}

internal static (nint, error) write(slice<byte> b) {
    var (_, err) = os.Stdout.Write(b); if (err != nil) {
        return (-1, err);
    }

    return (len(b), default);
}

public static partial void X__builtin_bzero(ж<TLS> t, uintptr s, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v n=%v, (%v:)", t.OrTypedNil(), s, n, origin(2));
    }
    Xbzero(t, s, n);
}

public static partial void X__builtin_abort(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    Xabort(t);
}

public static partial int32 X__builtin_abs(ж<TLS> t, int32 j) {
    if (__ccgo_strace) {
        trc("t=%v j=%v, (%v:)", t.OrTypedNil(), j, origin(2));
    }
    return Xabs(t, j);
}

public static partial int32 X__builtin_clz(ж<TLS> t, uint32 n) {
    if (__ccgo_strace) {
        trc("t=%v n=%v, (%v:)", t.OrTypedNil(), n, origin(2));
    }
    return (int32)mbits.LeadingZeros32(n);
}

public static partial int32 X__builtin_clzl(ж<TLS> t, @ulong n) {
    if (__ccgo_strace) {
        trc("t=%v n=%v, (%v:)", t.OrTypedNil(), n, origin(2));
    }
    if (/* unsafe.Sizeof(n) */ (uintptr)4 == 4) {
        return (int32)mbits.LeadingZeros32((uint32)n);
    }

    return (int32)mbits.LeadingZeros64((uint64)n);
}

public static partial int32 X__builtin_clzll(ж<TLS> t, uint64 n) {
    if (__ccgo_strace) {
        trc("t=%v n=%v, (%v:)", t.OrTypedNil(), n, origin(2));
    }
    return (int32)mbits.LeadingZeros64(n);
}
public static partial void X__builtin_constant_p_impl() { throw panic(todo("internal error: should never be called")); }

public static partial float64 X__builtin_copysign(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return Xcopysign(t, x, y);
}

public static partial float32 X__builtin_copysignf(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return Xcopysignf(t, x, y);
}

public static partial float64 X__builtin_copysignl(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return Xcopysign(t, x, y);
}

public static partial void X__builtin_exit(ж<TLS> t, int32 status) {
    if (__ccgo_strace) {
        trc("t=%v status=%v, (%v:)", t.OrTypedNil(), status, origin(2));
    }
    Xexit(t, status);
}

public static partial @long X__builtin_expect(ж<TLS> t, @long exp, @long c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    return exp;
}

public static partial float64 X__builtin_fabs(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return Xfabs(t, x);
}

public static partial float32 X__builtin_fabsf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return Xfabsf(t, x);
}

public static partial float64 X__builtin_fabsl(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return Xfabsl(t, x);
}

public static partial void X__builtin_free(ж<TLS> t, uintptr ptr) {
    if (__ccgo_strace) {
        trc("t=%v ptr=%v, (%v:)", t.OrTypedNil(), ptr, origin(2));
    }
    Xfree(t, ptr);
}

public static partial int32 X__builtin_getentropy(ж<TLS> t, uintptr buf, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v buf=%v n=%v, (%v:)", t.OrTypedNil(), buf, n, origin(2));
    }
    return Xgetentropy(t, buf, n);
}

public static partial float64 X__builtin_huge_val(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return math.Inf(1);
}

public static partial float32 X__builtin_huge_valf(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return (float32)math.Inf(1);
}

public static partial float64 X__builtin_inf(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return math.Inf(1);
}

public static partial float32 X__builtin_inff(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return (float32)math.Inf(1);
}

public static partial float64 X__builtin_infl(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    return math.Inf(1);
}

public static partial uintptr X__builtin_malloc(ж<TLS> t, typesꓸSize_t size) {
    if (__ccgo_strace) {
        trc("t=%v size=%v, (%v:)", t.OrTypedNil(), size, origin(2));
    }
    return Xmalloc(t, size);
}

public static partial int32 X__builtin_memcmp(ж<TLS> t, uintptr s1, uintptr s2, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s2=%v n=%v, (%v:)", t.OrTypedNil(), s2, n, origin(2));
    }
    return Xmemcmp(t, s1, s2, n);
}

public static partial float64 X__builtin_nan(ж<TLS> t, uintptr s) {
    if (__ccgo_strace) {
        trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
    }
    return math.NaN();
}

public static partial float32 X__builtin_nanf(ж<TLS> t, uintptr s) {
    if (__ccgo_strace) {
        trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
    }
    return (float32)math.NaN();
}

public static partial float64 X__builtin_nanl(ж<TLS> t, uintptr s) {
    if (__ccgo_strace) {
        trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
    }
    return math.NaN();
}

public static partial void X__builtin_prefetch(ж<TLS> t, uintptr addr, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
    }
}

public static partial int32 X__builtin_printf(ж<TLS> t, uintptr s, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
    }
    return Xprintf(t, s, args);
}

public static partial uintptr X__builtin_strchr(ж<TLS> t, uintptr s, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v, (%v:)", t.OrTypedNil(), s, c, origin(2));
    }
    return Xstrchr(t, s, c);
}

public static partial int32 X__builtin_strcmp(ж<TLS> t, uintptr s1, uintptr s2) {
    if (__ccgo_strace) {
        trc("t=%v s2=%v, (%v:)", t.OrTypedNil(), s2, origin(2));
    }
    return Xstrcmp(t, s1, s2);
}

public static partial uintptr X__builtin_strcpy(ж<TLS> t, uintptr dest, uintptr src) {
    if (__ccgo_strace) {
        trc("t=%v src=%v, (%v:)", t.OrTypedNil(), src, origin(2));
    }
    return Xstrcpy(t, dest, src);
}

public static partial typesꓸSize_t X__builtin_strlen(ж<TLS> t, uintptr s) {
    if (__ccgo_strace) {
        trc("t=%v s=%v, (%v:)", t.OrTypedNil(), s, origin(2));
    }
    return Xstrlen(t, s);
}

public static partial void X__builtin_trap(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    Xabort(t);
}

public static partial int32 X__isnan(ж<TLS> t, float64 arg) {
    if (__ccgo_strace) {
        trc("t=%v arg=%v, (%v:)", t.OrTypedNil(), arg, origin(2));
    }
    return X__builtin_isnan(t, arg);
}

public static partial int32 X__isnanf(ж<TLS> t, float32 arg) {
    if (__ccgo_strace) {
        trc("t=%v arg=%v, (%v:)", t.OrTypedNil(), arg, origin(2));
    }
    return Xisnanf(t, arg);
}

public static partial int32 X__isnanl(ж<TLS> t, float64 arg) {
    if (__ccgo_strace) {
        trc("t=%v arg=%v, (%v:)", t.OrTypedNil(), arg, origin(2));
    }
    return Xisnanl(t, arg);
}

public static partial int32 Xvfprintf(ж<TLS> t, uintptr stream, uintptr format, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    return Xfprintf(t, stream, format, ap);
}

public static partial int32 X__builtin_popcount(ж<TLS> t, uint32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (int32)mbits.OnesCount32(x);
}

public static partial int32 X__builtin_popcountl(ж<TLS> t, @ulong x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (int32)mbits.OnesCount64((uint64)x);
}

public static partial int32 X__builtin_popcountll(ж<TLS> t, uint64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (int32)mbits.OnesCount64((uint64)x);
}

public static partial uintptr X__builtin___strcpy_chk(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t os) {
    if (__ccgo_strace) {
        trc("t=%v src=%v os=%v, (%v:)", t.OrTypedNil(), src, os, origin(2));
    }
    return Xstrcpy(t, dest, src);
}

public static partial uintptr X__builtin_mmap(ж<TLS> t, uintptr addr, typesꓸSize_t length, int32 prot, int32 flags, int32 fd, typesꓸOff_t offset) {
    if (__ccgo_strace) {
        trc("t=%v addr=%v length=%v fd=%v offset=%v, (%v:)", t.OrTypedNil(), addr, length, fd, offset, origin(2));
    }
    return Xmmap(t, addr, length, prot, flags, fd, offset);
}

public static partial uint16 X__builtin_bswap16(ж<TLS> t, uint16 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (uint16)((uint16)(x << 8) | x >> 8);
}

public static partial uint32 X__builtin_bswap32(ж<TLS> t, uint32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return x << 24 | (x & 0xff00) << 8 | (x & 0xff0000) >> 8 | x >> 24;
}

public static partial uint64 X__builtin_bswap64(ж<TLS> t, uint64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return x << 56 | (x & 0xff00) << 40 | (x & 0xff0000) << 24 | (x & 0xff000000U) << 8 | (x & 0xff00000000UL) >> 8 | (x & 0xff0000000000UL) >> 24 | (x & 0xff000000000000UL) >> 40 | x >> 56;
}

public static partial int32 X__builtin_add_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var (r, ovf) = mathutil.AddOverflowInt64(a, b);
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static partial int32 X__builtin_add_overflowUint32(ж<TLS> t, uint32 a, uint32 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var r = a + b;
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)res)).Value = r;
    return Bool32(r < a);
}

public static partial int32 X__builtin_add_overflowUint64(ж<TLS> t, uint64 a, uint64 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var r = a + b;
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(r < a);
}

public static partial int32 X__builtin_sub_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var (r, ovf) = mathutil.SubOverflowInt64(a, b);
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static partial int32 X__builtin_mul_overflowInt64(ж<TLS> t, int64 a, int64 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var (r, ovf) = mathutil.MulOverflowInt64(a, b);
    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static partial int32 X__builtin_mul_overflowUint64(ж<TLS> t, uint64 a, uint64 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var (hi, lo) = mbits.Mul64(a, b);
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)res)).Value = lo;
    return Bool32(hi != 0);
}

public static partial int32 X__builtin_mul_overflowUint128(ж<TLS> t, Uint128 a, Uint128 b, uintptr res) {
    if (__ccgo_strace) {
        trc("t=%v b=%v res=%v, (%v:)", t.OrTypedNil(), b, res, origin(2));
    }
    var (r, ovf) = a.mulOvf(b);
    (@unsafe.Pointer.To<Uint128>((@unsafe.Pointer)res)).Value = r;
    return Bool32(ovf);
}

public static partial void X__builtin_unreachable(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "unrechable\n"u8);
    os.Stderr.Sync();
    Xexit(t, 1);
}

public static partial int32 X__builtin_snprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v args=%v, (%v:)", t.OrTypedNil(), str, size, args, origin(2));
    }
    return Xsnprintf(t, str, size, format, args);
}

public static partial int32 /*r*/ X__builtin_sprintf(ж<TLS> t, uintptr str, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xsprintf(t, str, format, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial uintptr /*r*/ X__builtin_memcpy(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v n=%v, (%v:)", t.OrTypedNil(), src, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xmemcpy(t, dest, src, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial uintptr /*r*/ X__builtin___memcpy_chk(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n, typesꓸSize_t os) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v os=%v, (%v:)", t.OrTypedNil(), src, os, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (os != ~((typesꓸSize_t)0) && n < os) {
            Xabort(t);
        }

        r = Xmemcpy(t, dest, src, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial uintptr X__builtin_memset(ж<TLS> t, uintptr s, int32 c, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v n=%v, (%v:)", t.OrTypedNil(), s, c, n, origin(2));
    }
    return Xmemset(t, s, c, n);
}

public static partial uintptr X__builtin___memset_chk(ж<TLS> t, uintptr s, int32 c, typesꓸSize_t n, typesꓸSize_t os) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v os=%v, (%v:)", t.OrTypedNil(), s, c, os, origin(2));
    }
    if (os < n) {
        Xabort(t);
    }

    return Xmemset(t, s, c, n);
}

public static partial typesꓸSize_t X__builtin_object_size(ж<TLS> t, uintptr p, int32 typ) {
    if (__ccgo_strace) {
        trc("t=%v p=%v typ=%v, (%v:)", t.OrTypedNil(), p, typ, origin(2));
    }
    switch (typ) {
    case 0 or 1: {
        return ~((typesꓸSize_t)0);
    }
    default: {
        return 0;
    }}

}

public static void AtomicStoreNUint8(uintptr ptr, uint8 val, int32 memorder) {
    a_store_8(ptr, val);
}

public static void AtomicStoreNUint16(uintptr ptr, uint16 val, int32 memorder) {
    a_store_16(ptr, val);
}

public static partial int32 /*r*/ Xsprintf(ж<TLS> t, uintptr str, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var b = printf(format, args);
        r = (int32)len(b);
        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)str, 0, r, r, 1125899906842623), b);
        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(str + (uintptr)r))).Value = 0;
        r = (int32)len(b);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 /*r*/ X__builtin___sprintf_chk(ж<TLS> t, uintptr s, int32 flag, typesꓸSize_t os, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v s=%v flag=%v os=%v args=%v, (%v:)", t.OrTypedNil(), s, flag, os, args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xsprintf(t, s, format, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial void Xqsort(ж<TLS> t, uintptr @base, typesꓸSize_t nmemb, typesꓸSize_t size, uintptr compar) {
    if (__ccgo_strace) {
        trc("t=%v base=%v size=%v compar=%v, (%v:)", t.OrTypedNil(), @base, size, compar, origin(2));
    }
    qsort(t, @base, (uintptr)nmemb, (uintptr)size, compar);
}

public static partial void X__assert_fail(ж<TLS> t, uintptr assertion, uintptr @file, uint32 line, uintptr function) {
    if (__ccgo_strace) {
        trc("t=%v file=%v line=%v function=%v, (%v:)", t.OrTypedNil(), @file, line, function, origin(2));
    }
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "assertion failure: %s:%d.%s: %s\n"u8, GoString(@file), line, GoString(function), GoString(assertion));
    if (memgrind) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%s\n"u8, debug.Stack());
    }
    os.Stderr.Sync();
    Xexit(t, 1);
}

public static partial int32 Xvprintf(ж<TLS> t, uintptr s, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    return Xprintf(t, s, ap);
}

public static partial int32 Xvsprintf(ж<TLS> t, uintptr str, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v va=%v, (%v:)", t.OrTypedNil(), va, origin(2));
    }
    return Xsprintf(t, str, format, va);
}

public static partial int32 Xvsnprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v va=%v, (%v:)", t.OrTypedNil(), str, size, va, origin(2));
    }
    return Xsnprintf(t, str, size, format, va);
}

public static partial int32 X__builtin_vsnprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v str=%v size=%v va=%v, (%v:)", t.OrTypedNil(), str, size, va, origin(2));
    }
    return Xvsnprintf(t, str, size, format, va);
}

public static partial int32 Xobstack_vprintf(ж<TLS> t, uintptr obstack, uintptr template, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v va=%v, (%v:)", t.OrTypedNil(), va, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_obstack_newchunk(ж<TLS> t, uintptr obstack, int32 length) {
    if (__ccgo_strace) {
        trc("t=%v obstack=%v length=%v, (%v:)", t.OrTypedNil(), obstack, length, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_obstack_begin(ж<TLS> t, uintptr obstack, int32 size, int32 alignment, uintptr chunkfun, uintptr freefun) {
    if (__ccgo_strace) {
        trc("t=%v obstack=%v alignment=%v freefun=%v, (%v:)", t.OrTypedNil(), obstack, alignment, freefun, origin(2));
    }
    throw panic(todo(""));
}

public static partial void Xobstack_free(ж<TLS> t, uintptr obstack, uintptr obj) {
    if (__ccgo_strace) {
        trc("t=%v obj=%v, (%v:)", t.OrTypedNil(), obj, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 Xsleep(ж<TLS> t, uint32 seconds) {
    if (__ccgo_strace) {
        trc("t=%v seconds=%v, (%v:)", t.OrTypedNil(), seconds, origin(2));
    }
    gotime.Sleep(gotime.Second * ((gotime.Duration)(int64)seconds));
    return 0;
}

public static partial typesꓸSize_t /*r*/ Xstrcspn(ж<TLS> t, uintptr s, uintptr reject) {
    typesꓸSize_t r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v reject=%v, (%v:)", t.OrTypedNil(), reject, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bits = newBits(256);
        for (;;) {
            var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)reject);
            if (c == 0) {
                break;
            }

            reject++;
            bits.set((nint)c);
        }
        for (;;) {
            var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
            if (c == 0 || bits.has((nint)c)) {
                goto ᒐdone;
            }

            s++;
            r++;
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial int32 Xprintf(ж<TLS> t, uintptr format, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v args=%v, (%v:)", t.OrTypedNil(), args, origin(2));
    }
    var (n, _) = write(printf(format, args));
    return (int32)n;
}

public static partial int32 /*r*/ Xsnprintf(ж<TLS> t, uintptr str, typesꓸSize_t size, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v str=%v size=%v args=%v, (%v:)", t.OrTypedNil(), str, size, args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (format == 0) {
            r = 0; goto ᒐdone;
        }

        var b = printf(format, args);
        r = (int32)len(b);
        if (size == 0) {
            goto ᒐdone;
        }

        if (len(b) + 1 > (nint)size) {
            b = b.slice(0, (nint)(size - 1));
        }
        nint n = len(b);
        copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)str, 0, n, n, 1125899906842623), b);
        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(str + (uintptr)n))).Value = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial int32 /*r*/ X__builtin___snprintf_chk(ж<TLS> t, uintptr str, typesꓸSize_t maxlen, int32 flag, typesꓸSize_t os, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v str=%v maxlen=%v flag=%v os=%v args=%v, (%v:)", t.OrTypedNil(), str, maxlen, flag, os, args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (os != ~((typesꓸSize_t)0) && maxlen > os) {
            Xabort(t);
        }

        r = Xsnprintf(t, str, maxlen, format, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 /*r*/ X__builtin___vsnprintf_chk(ж<TLS> t, uintptr str, typesꓸSize_t maxlen, int32 flag, typesꓸSize_t os, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v str=%v maxlen=%v flag=%v os=%v args=%v, (%v:)", t.OrTypedNil(), str, maxlen, flag, os, args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (os != ~((typesꓸSize_t)0) && maxlen > os) {
            Xabort(t);
        }

        r = Xsnprintf(t, str, maxlen, format, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 /*r*/ X__builtin___vsprintf_chk(ж<TLS> t, uintptr s, int32 flag, typesꓸSize_t os, uintptr format, uintptr args) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v s=%v flag=%v os=%v args=%v, (%v:)", t.OrTypedNil(), s, flag, os, args, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xvsprintf(t, s, format, args);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 Xabs(ж<TLS> t, int32 j) {
    if (__ccgo_strace) {
        trc("t=%v j=%v, (%v:)", t.OrTypedNil(), j, origin(2));
    }
    if (j >= 0) {
        return j;
    }

    return -j;
}

public static partial @long Xlabs(ж<TLS> t, @long j) {
    if (__ccgo_strace) {
        trc("t=%v j=%v, (%v:)", t.OrTypedNil(), j, origin(2));
    }
    if (j >= 0) {
        return j;
    }

    return -j;
}

public static partial int64 Xllabs(ж<TLS> tls, int64 a) {
    if (__ccgo_strace) {
        trc("tls=%v a=%v, (%v:)", tls.OrTypedNil(), a, origin(2));
    }
    if (a >= (int64)0) {
        return a;
    }

    return -a;
}

public static partial int32 X__builtin_isnan(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return Bool32(math.IsNaN(x));
}

public static partial int64 X__builtin_llabs(ж<TLS> tls, int64 a) {
    if (__ccgo_strace) {
        trc("tls=%v a=%v, (%v:)", tls.OrTypedNil(), a, origin(2));
    }
    return Xllabs(tls, a);
}

public static partial float64 Xacos(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Acos(x);
}

public static partial float32 Xacosf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Acos((float64)x);
}

public static partial float64 Xacosh(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Acosh(x);
}

public static partial float32 Xacoshf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Acosh((float64)x);
}

public static partial float64 Xasin(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Asin(x);
}

public static partial float32 Xasinf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Asin((float64)x);
}

public static partial float64 Xasinh(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Asinh(x);
}

public static partial float32 Xasinhf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Asinh((float64)x);
}

public static partial float64 Xatan(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Atan(x);
}

public static partial float32 Xatanf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Atan((float64)x);
}

public static partial float64 Xatan2(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return math.Atan2(x, y);
}

public static partial float32 Xatan2f(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return (float32)math.Atan2((float64)x, (float64)y);
}

public static partial float64 Xatanh(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Atanh(x);
}

public static partial float32 Xatanhf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Atanh((float64)x);
}

public static partial float64 Xceil(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Ceil(x);
}

public static partial float32 Xceilf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Ceil((float64)x);
}

public static partial float64 Xcopysign(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return math.Copysign(x, y);
}

public static partial float32 Xcopysignf(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return (float32)math.Copysign((float64)x, (float64)y);
}

public static partial float64 Xcos(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Cos(x);
}

public static partial float32 Xcosf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Cos((float64)x);
}

public static partial float64 Xcosh(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Cosh(x);
}

public static partial float32 Xcoshf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Cosh((float64)x);
}

public static partial float64 Xexp(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Exp(x);
}

public static partial float32 Xexpf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Exp((float64)x);
}

public static partial float64 Xfabs(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Abs(x);
}

public static partial float32 Xfabsf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Abs((float64)x);
}

public static partial float64 Xfloor(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Floor(x);
}

public static partial float32 Xfloorf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Floor((float64)x);
}

public static partial float64 Xfmod(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return math.Mod(x, y);
}

public static partial float32 Xfmodf(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return (float32)math.Mod((float64)x, (float64)y);
}

public static partial float64 /*r*/ X__builtin_hypot(ж<TLS> t, float64 x, float64 y) {
    return Xhypot(t, x, y);
}

public static partial float64 Xhypot(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return math.Hypot(x, y);
}

public static partial float32 Xhypotf(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return (float32)math.Hypot((float64)x, (float64)y);
}

public static partial int32 Xisnan(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return X__builtin_isnan(t, x);
}

public static partial int32 Xisnanf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return Bool32(math.IsNaN((float64)x));
}

public static partial int32 Xisnanl(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return Bool32(math.IsNaN(x));
}

public static partial float64 Xldexp(ж<TLS> t, float64 x, int32 exp) {
    if (__ccgo_strace) {
        trc("t=%v x=%v exp=%v, (%v:)", t.OrTypedNil(), x, exp, origin(2));
    }
    return math.Ldexp(x, (nint)exp);
}

public static partial float64 Xlog(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Log(x);
}

public static partial float32 Xlogf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Log((float64)x);
}

public static partial float64 Xlog10(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Log10(x);
}

public static partial float32 Xlog10f(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Log10((float64)x);
}

public static partial float64 X__builtin_log2(ж<TLS> t, float64 x) {
    return Xlog2(t, x);
}

public static partial float64 Xlog2(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Log2(x);
}

public static partial float32 Xlog2f(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Log2((float64)x);
}

public static partial float64 Xround(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Round(x);
}

public static partial float32 Xroundf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Round((float64)x);
}

public static partial float64 X__builtin_round(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Round(x);
}

public static partial float64 Xerf(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Erf(x);
}

public static partial float64 Xerfc(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Erfc(x);
}

public static partial float64 Xexp2(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Exp2(x);
}

public static partial float64 Xfdim(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v, (%v:)", t.OrTypedNil(), x, y, origin(2));
    }
    if (math.IsNaN(x) || math.IsNaN(y)) {
        return math.NaN();
    }

    if (x > y) {
        return x - y;
    }

    return 0D;
}

public static partial float64 Xfma(ж<TLS> t, float64 x, float64 y, float64 z) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v z=%v, (%v:)", t.OrTypedNil(), x, y, z, origin(2));
    }
    return math.FMA(x, y, z);
}

public static partial float64 X__builtin_fma(ж<TLS> t, float64 x, float64 y, float64 z) {
    return Xfma(t, x, y, z);
}

public static partial int32 Xilogb(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    switch (ᐧ) {
    case {} when (math.IsNaN(x)) || (x == 0D): {
        return math.MinInt32;
    }
    case {} when math.IsInf(x, 0): {
        return math.MaxInt32;
    }}

    return (int32)math.Ilogb(x);
}

public static partial float64 Xlgamma(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    var (r, _) = math.Lgamma(x);
    return r;
}

public static partial float64 Xlogb(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Logb(x);
}

public static partial float64 Xnextafter(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v, (%v:)", t.OrTypedNil(), x, y, origin(2));
    }
    return math.Nextafter(x, y);
}

public static partial float32 Xnextafterf(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v, (%v:)", t.OrTypedNil(), x, y, origin(2));
    }
    return math.Nextafter32(x, y);
}

public static partial float64 Xnexttoward(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v, (%v:)", t.OrTypedNil(), x, y, origin(2));
    }
    return math.Nextafter(x, y);
}

public static partial float64 Xremainder(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v, (%v:)", t.OrTypedNil(), x, y, origin(2));
    }
    return math.Remainder(x, y);
}

public static partial float64 Xremquo(ж<TLS> t, float64 x, float64 y, uintptr quo) {
    if (__ccgo_strace) {
        trc("t=%v x=%v y=%v quo=%v, (%v:)", t.OrTypedNil(), x, y, quo, origin(2));
    }
    var ux = math.Float64bits(x);
    var uy = math.Float64bits(y);
    nint ex = (nint)(ux >> 52 & 0x7ff);
    nint ey = (nint)(uy >> 52 & 0x7ff);
    var sx = ux >> 63;
    var sy = uy >> 63;
    uint32 q = default;
    uint64 i = default;
    var uxi = ux;

    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)quo)).Value = 0;
    if (uy << 1 == 0 || math.IsNaN(y) || ex == 0x7ff) {
        return (x * y) / (x * y);
    }

    if (ux << 1 == 0) {
        return x;
    }

    if (ex == 0) {
        for (i = uxi << 12; i >> 63 == 0; i <<= 1) {
            ex--;
        }
        uxi.LshAssign((nuint)(-ex + 1));
    } else {
        uxi &= (uint64)((~(uint64)0 >> 12));
        uxi |= (uint64)(((uint64)1 << 52));
    }
    if (ey == 0) {
        for (i = uy << 12; i >> 63 == 0; i <<= 1) {
            ey--;
        }
        uy.LshAssign((nuint)(-ey + 1));
    } else {
        uy &= (uint64)((~(uint64)0 >> 12));
        uy |= (uint64)(((uint64)1 << 52));
    }

    if (ex < ey) {
        if (ex + 1 != ey) {
            return x;
        }
    } else {
        for (; ex > ey; ex--) {
            i = uxi - uy;
            if (i >> 63 == 0) {
                uxi = i;
                q++;
            }
            uxi <<= 1;
            q <<= 1;
        }
        i = uxi - uy;
        if (i >> 63 == 0) {
            uxi = i;
            q++;
        }
        if (uxi == 0) {
            ex = -60;
        } else {
            for (; uxi >> 52 == 0; uxi <<= 1) {
                ex--;
            }
        }
    }

    if (ex > 0) {
        uxi -= ((uint64)1 << 52);
        uxi |= (uint64)(((uint64)ex << 52));
    } else {
        uxi.RshAssign((nuint)(-ex + 1));
    }
    x = math.Float64frombits(uxi);
    if (sy != 0) {
        y = -y;
    }
    if (ex == ey || (ex + 1 == ey && (2D * x > y || (2D * x == y && q % 2 != 0)))) {
        x -= y;
        q++;
    }
    q &= (uint32)(0x7fffffff);
    if ((sx ^ sy) != 0) {
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)quo)).Value = -(int32)q;
    } else {
        (@unsafe.Pointer.To<int32>((@unsafe.Pointer)quo)).Value = (int32)q;
    }
    if (sx != 0) {
        return -x;
    }

    return x;
}

public static partial float64 Xtgamma(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Gamma(x);
}

public static partial float32 X__builtin_roundf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Round((float64)x);
}

public static partial float64 Xsin(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Sin(x);
}

public static partial float32 Xsinf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Sin((float64)x);
}

public static partial float64 Xsinh(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Sinh(x);
}

public static partial float32 Xsinhf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Sinh((float64)x);
}

public static partial float64 Xsqrt(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Sqrt(x);
}

public static partial float32 Xsqrtf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Sqrt((float64)x);
}

public static partial float64 Xtan(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Tan(x);
}

public static partial float32 Xtanf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Tan((float64)x);
}

public static partial float64 Xtanh(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Tanh(x);
}

public static partial float32 Xtanhf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Tanh((float64)x);
}

public static partial float64 Xtrunc(ж<TLS> t, float64 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return math.Trunc(x);
}

public static partial float32 Xtruncf(ж<TLS> t, float32 x) {
    if (__ccgo_strace) {
        trc("t=%v x=%v, (%v:)", t.OrTypedNil(), x, origin(2));
    }
    return (float32)math.Trunc((float64)x);
}

internal static uint64 nextRand = (uint64)1;

public static partial int32 Xrand(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    nextRand = nextRand * 1103515245 + 12345;
    return (int32)(uint32)(nextRand / (uint64)(math.MaxUint32 + 1) % (uint64)math.MaxInt32);
}

public static partial float64 Xpow(ж<TLS> t, float64 x, float64 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    var r = math.Pow(x, y);
    if (x > 0D && r == 1D && y >= -1.0000000000000000715e-18D && y < -1e-30D) {
        r = 0.9999999999999999D;
    }
    return r;
}

public static partial float32 Xpowf(ж<TLS> t, float32 x, float32 y) {
    if (__ccgo_strace) {
        trc("t=%v y=%v, (%v:)", t.OrTypedNil(), y, origin(2));
    }
    return (float32)math.Pow((float64)x, (float64)y);
}

public static partial float64 Xfrexp(ж<TLS> t, float64 x, uintptr exp) {
    if (__ccgo_strace) {
        trc("t=%v x=%v exp=%v, (%v:)", t.OrTypedNil(), x, exp, origin(2));
    }
    var (f, e) = math.Frexp(x);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)exp)).Value = (int32)e;
    return f;
}

public static partial float32 Xfrexpf(ж<TLS> t, float32 x, uintptr exp) {
    if (__ccgo_strace) {
        trc("t=%v x=%v exp=%v, (%v:)", t.OrTypedNil(), x, exp, origin(2));
    }
    var (f, e) = math.Frexp((float64)x);
    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)exp)).Value = (int32)e;
    return (float32)f;
}

public static partial float64 Xmodf(ж<TLS> t, float64 x, uintptr iptr) {
    if (__ccgo_strace) {
        trc("t=%v x=%v iptr=%v, (%v:)", t.OrTypedNil(), x, iptr, origin(2));
    }
    var (i, f) = math.Modf(x);
    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)iptr)).Value = i;
    return f;
}

public static partial float32 Xmodff(ж<TLS> t, float32 x, uintptr iptr) {
    if (__ccgo_strace) {
        trc("t=%v x=%v iptr=%v, (%v:)", t.OrTypedNil(), x, iptr, origin(2));
    }
    var (i, f) = math.Modf((float64)x);
    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)iptr)).Value = (float32)i;
    return (float32)f;
}

public static partial uintptr /*r*/ Xstrncpy(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v n=%v, (%v:)", t.OrTypedNil(), src, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var k = strnlen(src, n);
        var d = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)dest), n);
        copy(d, @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)src), k));
        clear(d.slice((nint)(k)));
        r = dest;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial uintptr /*r*/ X__builtin___strncpy_chk(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n, typesꓸSize_t os) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v os=%v, (%v:)", t.OrTypedNil(), src, os, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (n != ~((typesꓸSize_t)0) && os < n) {
            Xabort(t);
        }

        r = Xstrncpy(t, dest, src, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 Xstrcmp(ж<TLS> t, uintptr s1, uintptr s2) {
    if (__ccgo_strace) {
        trc("t=%v s2=%v, (%v:)", t.OrTypedNil(), s2, origin(2));
    }
    for (;;) {
        var ch1 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s1);
        s1++;
        var ch2 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s2);
        s2++;
        if (ch1 != ch2 || ch1 == 0 || ch2 == 0) {
            return (int32)ch1 - (int32)ch2;
        }
    }
}

public static partial uintptr /*r*/ Xstrcat(ж<TLS> t, uintptr dest, uintptr src) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v, (%v:)", t.OrTypedNil(), src, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = dest;
        while (~@unsafe.Pointer.To<int8>((@unsafe.Pointer)dest) != 0) {
            dest++;
        }
        for (;;) {
            var c = ~@unsafe.Pointer.To<int8>((@unsafe.Pointer)src);
            src++;
            (@unsafe.Pointer.To<int8>((@unsafe.Pointer)dest)).Value = c;
            dest++;
            if (c == 0) {
                goto ᒐdone;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial uintptr /*r*/ X__builtin___strcat_chk(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t os) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v os=%v, (%v:)", t.OrTypedNil(), src, os, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xstrcat(t, dest, src);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 Xstrncmp(ж<TLS> t, uintptr s1, uintptr s2, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s2=%v n=%v, (%v:)", t.OrTypedNil(), s2, n, origin(2));
    }
    byte ch1 = default;
    byte ch2 = default;
    for (; n != 0; n--) {
        ch1 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s1);
        s1++;
        ch2 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s2);
        s2++;
        if (ch1 != ch2) {
            return (int32)ch1 - (int32)ch2;
        }

        if (ch1 == 0) {
            return 0;
        }
    }
    return 0;
}

public static partial uintptr /*r*/ Xstrcpy(ж<TLS> t, uintptr dest, uintptr src) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v, (%v:)", t.OrTypedNil(), src, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = dest;
        for (nint i = 0; i < 16; i++) {
            var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(src + (uintptr)i));
            (@unsafe.Pointer.To<byte>((@unsafe.Pointer)(dest + (uintptr)i))).Value = c;
            if (c == 0) {
                goto ᒐdone;
            }
        }
        dest += 16;
        src += 16;
        var n = strlen(src) + 1;
        copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)dest), n), @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)src), n));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial uintptr Xstrchr(ж<TLS> t, uintptr s, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v, (%v:)", t.OrTypedNil(), s, c, origin(2));
    }
    for (nint i = 0; i < 16; i++) {
        var ch = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        if (ch == (byte)c) {
            return s;
        }

        if (ch == 0) {
            return 0;
        }

        s++;
    }
    {
        nint i = bytes.IndexByte(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s), strlen(s) + 1), (byte)c); if (i >= 0) {
            return s + (uintptr)i;
        }
    }

    return 0;
}

public static partial uintptr /*r*/ Xstrrchr(ж<TLS> t, uintptr s, int32 c) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v s=%v c=%v, (%v:)", t.OrTypedNil(), s, c, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var n = Xstrlen(t, s);
        nint i = bytes.LastIndexByte(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s), n + 1), (byte)c); if (i >= 0) {
            r = s + (uintptr)i; goto ᒐdone;
        }
        r = 0;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial uintptr Xmemset(ж<TLS> t, uintptr dest, int32 c, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v n=%v, (%v:)", t.OrTypedNil(), dest, c, n, origin(2));
    }
    uint8 c8 = default;
    uint32 c32 = default;
    uint64 c64 = default;
    typesꓸSize_t k = default;
    uintptr s = default;

    s = dest;
    if (n == 0) {
        return dest;
    }
    c8 = (uint8)c;
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)s)).Value = c8;
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + (uintptr)(n - 1)))).Value = c8;
    if (n <= ((typesꓸSize_t)2)) {
        return dest;
    }
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + 1))).Value = c8;
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + 2))).Value = c8;
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + (uintptr)(n - 2)))).Value = c8;
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + (uintptr)(n - 3)))).Value = c8;
    if (n <= ((typesꓸSize_t)6)) {
        return dest;
    }
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + 3))).Value = c8;
    (@unsafe.Pointer.To<uint8>((@unsafe.Pointer)(s + (uintptr)(n - 4)))).Value = c8;
    if (n <= ((typesꓸSize_t)8)) {
        return dest;
    }
    k = (typesꓸSize_t)(((typesꓸSize_t)0 - ((typesꓸSize_t)s)) & ((typesꓸSize_t)3));
    s += (uintptr)k;
    n -= k;
    n &= (typesꓸSize_t)(((typesꓸSize_t)(-Int32FromInt32(4))));
    c32 = (uint32)0x01010101 * (uint32)c8;
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(s + (uintptr)0))).Value = c32;
    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)(s + (uintptr)(n - 4)))).Value = c32;
    if (n <= ((typesꓸSize_t)8)) {
        return dest;
    }
    c64 = (uint64)c32 | ((uint64)c32 << 32);
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)4))).Value = c64;
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)(n - 12)))).Value = c64;
    if (n <= ((typesꓸSize_t)24)) {
        return dest;
    }
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)12))).Value = c64;
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)20))).Value = c64;
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)(n - 28)))).Value = c64;
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)(n - 20)))).Value = c64;
    k = ((typesꓸSize_t)24) + (typesꓸSize_t)(((typesꓸSize_t)s) & ((typesꓸSize_t)4));
    s += (uintptr)k;
    n -= k;
    for (;;) {
        if (!(n >= ((typesꓸSize_t)32))) {
            break;
        }
        (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)0))).Value = c64;
        (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)8))).Value = c64;
        (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)16))).Value = c64;
        (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(s + (uintptr)24))).Value = c64;
        n -= ((typesꓸSize_t)32);
        s += (uintptr)32;
    }
    return dest;
}

public static partial uintptr /*r*/ Xmemcpy(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n) {
    uintptr r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("t=%v src=%v n=%v, (%v:)", t.OrTypedNil(), src, n, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (n != 0) {
            copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)dest, 0, (nint)(n), (nint)(n), 1125899906842623), @unsafe.Pointer.View<byte>((@unsafe.Pointer)src, 0, (nint)(n), (nint)(n), 1125899906842623));
        }
        r = dest;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 Xmemcmp(ж<TLS> t, uintptr s1, uintptr s2, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s2=%v n=%v, (%v:)", t.OrTypedNil(), s2, n, origin(2));
    }
    return memcmp(s1, s2, (Tsize_t)n);
}

public static partial uintptr Xmemchr(ж<TLS> t, uintptr s, int32 c, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v n=%v, (%v:)", t.OrTypedNil(), s, c, n, origin(2));
    }
    while (n != 0) {
        var chunk = min(4096 - s % 4096, (uintptr)n);
        nint i = bytes.IndexByte(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)s), chunk), (byte)c); if (i >= 0) {
            return s + (uintptr)i;
        }

        s += chunk;
        n -= ((typesꓸSize_t)chunk);
    }
    return 0;
}

public static partial uintptr Xmemmove(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v src=%v n=%v, (%v:)", t.OrTypedNil(), src, n, origin(2));
    }
    if (n == 0) {
        return dest;
    }

    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)(uintptr)dest, 0, (nint)(n), (nint)(n), 1125899906842623), @unsafe.Pointer.View<byte>((@unsafe.Pointer)(uintptr)src, 0, (nint)(n), (nint)(n), 1125899906842623));
    return dest;
}

public static partial uintptr X__builtin___memmove_chk(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n, typesꓸSize_t os) {
    if (__ccgo_strace) {
        trc("t=%v src=%v os=%v, (%v:)", t.OrTypedNil(), src, os, origin(2));
    }
    if (os != ~((typesꓸSize_t)0) && os < n) {
        Xabort(t);
    }

    return Xmemmove(t, dest, src, n);
}

public static partial uintptr Xgetenv(ж<TLS> t, uintptr name) {
    if (__ccgo_strace) {
        trc("t=%v name=%v, (%v:)", t.OrTypedNil(), name, origin(2));
    }
    return getenv(Environ(), GoString(name));
}

internal static uintptr getenv(uintptr p, @string nm) {
    for (; ; p += uintptrSize) {
        var q = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p);
        if (q == 0) {
            return 0;
        }

        nint n = cstrPrefix(q, nm); if (n == len(nm) && ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(q + (uintptr)n)) == '=') {
            return q + (uintptr)n + 1;
        }
    }
}

public static partial uintptr Xstrstr(ж<TLS> t, uintptr haystack, uintptr needle) {
    if (__ccgo_strace) {
        trc("t=%v needle=%v, (%v:)", t.OrTypedNil(), needle, origin(2));
    }
    var hs = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)haystack), strlen(haystack));
    var nd = @unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)needle), strlen(needle));
    nint i = bytes.Index(hs, nd); if (i >= 0) {
        return haystack + (uintptr)i;
    }

    return 0;
}

public static partial int32 Xputc(ж<TLS> t, int32 c, uintptr fp) {
    if (__ccgo_strace) {
        trc("t=%v c=%v fp=%v, (%v:)", t.OrTypedNil(), c, fp, origin(2));
    }
    return Xfputc(t, c, fp);
}

public static partial int32 Xatoi(ж<TLS> t, uintptr nptr) {
    if (__ccgo_strace) {
        trc("t=%v nptr=%v, (%v:)", t.OrTypedNil(), nptr, origin(2));
    }

    var (_, neg, _, n, _) = strToUint64(ref (t).DerefOrNull(), nptr, 10);
    switch (ᐧ) {
    case {} when neg: {
        return (int32)(((uint64)0 - n));
    }
    default: {
        return (int32)n;
    }}

}

public static partial float64 Xatof(ж<TLS> t, uintptr nptr) {
    if (__ccgo_strace) {
        trc("t=%v nptr=%v, (%v:)", t.OrTypedNil(), nptr, origin(2));
    }
    var (n, _) = strToFloatt64(ref (t).DerefOrNull(), nptr, 64);
    return n;
}

public static partial int32 Xtolower(ж<TLS> t, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    if (c >= 'A' && c <= 'Z') {
        return c + ((rune)'a' - (rune)'A');
    }

    return c;
}

public static partial int32 Xtoupper(ж<TLS> t, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    if (c >= 'a' && c <= 'z') {
        return c - ((rune)'a' - (rune)'A');
    }

    return c;
}

public static partial int32 Xisatty(ж<TLS> t, int32 fd) {
    if (__ccgo_strace) {
        trc("t=%v fd=%v, (%v:)", t.OrTypedNil(), fd, origin(2));
    }
    return Bool32(isatty.IsTerminal((uintptr)fd));
}

public static partial @long Xatol(ж<TLS> t, uintptr nptr) {
    if (__ccgo_strace) {
        trc("t=%v nptr=%v, (%v:)", t.OrTypedNil(), nptr, origin(2));
    }
    var (_, neg, _, n, _) = strToUint64(ref (t).DerefOrNull(), nptr, 10);
    switch (ᐧ) {
    case {} when neg: {
        return (@long)(((uint64)0 - n));
    }
    default: {
        return (@long)n;
    }}

}

internal static nint cstrPrefix(uintptr p, @string s) {
    nint i = default;
    while (i < len(s) && ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(p + (uintptr)i)) == s[i]) {
        i++;
    }
    return i;
}

partial struct tzLocation {
    internal @string tz;
    internal ж<gotime.Location> loc;
}

internal static readonly ж<atomic.Pointer<tzLocation>> ᏑlocalLocation = new StaticBox<atomic.Pointer<tzLocation>>(default(atomic.Pointer<tzLocation>));
internal static ref atomic.Pointer<tzLocation> localLocation => ref ((StaticBox<atomic.Pointer<tzLocation>>)ᏑlocalLocation).ValueSlot;

internal static ж<gotime.Location> /*loc*/ getLocalLocation() {
    ж<gotime.Location> loc = default;

    var r = getenv(Environ(), "TZ");
    if (r == 0) {
        return gotime.Local;
    }

    var c = ᏑlocalLocation.Load(); if (c != nil && cstrPrefix(r, c.tz) == len(c.tz) && ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)(r + (uintptr)len(c.tz))) == 0) {
        return c.loc;
    }

    ref var zname = ref heap<@string>(GoString(r), out var Ꮡzname);
    var (loc2, err) = gotime.LoadLocation(strings.TrimPrefix(zname, ":"));
    switch (ᐧ) {
    case {} when err == nil: {
        loc = loc2;
        break;
    }
    case {} when strings.HasPrefix(zname, ":"): {
        loc = gotime.UTC;
        break;
    }
    default: {
        {
            var (l, ok) = posixTZLocation(zname); if (ok) {
                loc = l;
                break;
            }
        }

        {
            var (zone, off, ok) = parseZone(zname); if (ok) {
                loc = gotime.FixedZone(zone, -off);
            } else {
                loc = gotime.UTC;
            }
        }
        break;
    }}

    ᏑlocalLocation.Store(Ꮡ(new tzLocation(tz: zname, loc: loc)));
    return loc;
}

[GoLazy] partial class lazyᴛposixTZBlock { internal static slice<byte> posixTZBlock = [
    (byte)'T', (byte)'Z', (byte)'i', (byte)'f', (byte)'2', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 0,
    0, 0, 0, 1,
    0, 0, 0, 1,
    0, 0, 0, 1, 0, 0,
    0
]; }

internal static (ж<gotime.Location>, bool) posixTZLocation(@string s) {
    var dataᴷ = new stackAppend<byte>();

    if (s == ""u8 || strings.ContainsRune(s, '\n')) {
        return (default, false);
    }

    var data = appendꓸꓸꓸ(appendꓸꓸꓸ(slice<byte>(default), posixTZBlock), posixTZBlock);
    data = append(append(append(dataᴷ, data, (byte)'\n'), s.ꓸꓸꓸ), (byte)'\n');
    var (loc, err) = gotime.LoadLocationFromTZData(s, data);
    if (err != nil) {
        return (default, false);
    }

    var (name, off) = gotime.Unix(0, 0).In(loc).Zone(); if (name == ""u8 && off == 1) {
        return (default, false);
    }

    return (loc, true);
}

public static partial timeꓸTime_t /*r*/ Xmktime(ж<TLS> t, uintptr ptm) {
    timeꓸTime_t r = default;

    if (__ccgo_strace) {
        trc("t=%v ptm=%v, (%v:)", t.OrTypedNil(), ptm, origin(2));
    }
    var loc = getLocalLocation();
    var tt = gotime.Date(
        (nint)((@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_year + 1900),
        (gotime.Month)(nint)((@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_mon + 1),
        (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_mday,
        (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_hour,
        (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_min,
        (nint)(@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_sec,
        0,
        loc);
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_wday = (int32)tt.Weekday();
    (@unsafe.Pointer.To<timeꓸTm>((@unsafe.Pointer)ptm)).Value.Ftm_yday = (int32)(tt.YearDay() - 1);
    r = (timeꓸTime_t)tt.Unix();
    return r;
}

public static partial uintptr Xstrpbrk(ж<TLS> t, uintptr s, uintptr accept) {
    if (__ccgo_strace) {
        trc("t=%v accept=%v, (%v:)", t.OrTypedNil(), accept, origin(2));
    }
    var bits = newBits(256);
    for (;;) {
        var b = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)accept);
        if (b == 0) {
            break;
        }

        bits.set((nint)b);
        accept++;
    }
    for (;;) {
        var b = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        if (b == 0) {
            return 0;
        }

        if (bits.has((nint)b)) {
            return s;
        }

        s++;
    }
}

public static partial int32 Xstrcasecmp(ж<TLS> t, uintptr s1, uintptr s2) {
    if (__ccgo_strace) {
        trc("t=%v s2=%v, (%v:)", t.OrTypedNil(), s2, origin(2));
    }
    for (;;) {
        var ch1 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s1);
        if (ch1 >= 'a' && ch1 <= 'z') {
            ch1 = (byte)(ch1 - ((rune)'a' - (rune)'A'));
        }
        s1++;
        var ch2 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s2);
        if (ch2 >= 'a' && ch2 <= 'z') {
            ch2 = (byte)(ch2 - ((rune)'a' - (rune)'A'));
        }
        s2++;
        if (ch1 != ch2 || ch1 == 0 || ch2 == 0) {
            var r = (int32)ch1 - (int32)ch2;
            return r;
        }
    }
}

public static partial uint16 Xntohs(ж<TLS> t, uint16 netshortʗp) {
    ref var netshort = ref heap(netshortʗp, out var Ꮡnetshort);

    if (__ccgo_strace) {
        trc("t=%v netshort=%v, (%v:)", t.OrTypedNil(), netshort, origin(2));
    }
    return (uint16)((uint16)((uint16)@unsafe.Pointer.ElementRef<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡnetshort), 0, 2) << 8) | (uint16)@unsafe.Pointer.ElementRef<byte>(@unsafe.Pointer.FromPinnedBox(Ꮡnetshort), 1, 2));
}

public static partial uint16 Xhtons(ж<TLS> t, uint16 hostshort) {
    if (__ccgo_strace) {
        trc("t=%v hostshort=%v, (%v:)", t.OrTypedNil(), hostshort, origin(2));
    }
    ref var a = ref heap(new array<byte>(2), out var Ꮡa);
    a[0] = (byte)(hostshort >> 8);
    a[1] = (byte)hostshort;
    return ~Ꮡa.Reinterpret<array<byte>, uint16>();
}

public static partial uint32 Xhtonl(ж<TLS> t, uint32 hostlong) {
    if (__ccgo_strace) {
        trc("t=%v hostlong=%v, (%v:)", t.OrTypedNil(), hostlong, origin(2));
    }
    ref var a = ref heap(new array<byte>(4), out var Ꮡa);
    a[0] = (byte)(hostlong >> 24);
    a[1] = (byte)(hostlong >> 16);
    a[2] = (byte)(hostlong >> 8);
    a[3] = (byte)hostlong;
    return ~Ꮡa.Reinterpret<array<byte>, uint32>();
}

public static partial uintptr Xfopen(ж<TLS> t, uintptr pathname, uintptr mode) {
    if (__ccgo_strace) {
        trc("t=%v mode=%v, (%v:)", t.OrTypedNil(), mode, origin(2));
    }
    return Xfopen64(t, pathname, mode);
}

public static void Dmesg(@string s, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    if (dmesgs) {
        dmesg(s, args.ꓸꓸꓸ);
    }
}

public static partial void X__ccgo_sqlite3_log(ж<TLS> t, int32 iErrCode, uintptr zFormat, uintptr args) {
    if (__ccgo_strace) {
        trc("t=%v iErrCode=%v zFormat=%v args=%v, (%v:)", t.OrTypedNil(), iErrCode, zFormat, args, origin(2));
    }
}

public static partial int32 X_IO_putc(ж<TLS> t, int32 c, uintptr fp) {
    if (__ccgo_strace) {
        trc("t=%v c=%v fp=%v, (%v:)", t.OrTypedNil(), c, fp, origin(2));
    }
    return Xputc(t, c, fp);
}

public static partial int32 Xatexit(ж<TLS> t, uintptr function) {
    if (__ccgo_strace) {
        trc("t=%v function=%v, (%v:)", t.OrTypedNil(), function, origin(2));
    }
    AtExit(() => {
        GoFuncCookie.Func<Action<ж<TLS>>>(function)(t);
    });
    return 0;
}

public static partial int32 Xvasprintf(ж<TLS> t, uintptr strp, uintptr fmt, uintptr ap) {
    if (__ccgo_strace) {
        trc("t=%v ap=%v, (%v:)", t.OrTypedNil(), ap, origin(2));
    }
    throw panic(todo(""));
}

public static int32 /*val*/ AtomicLoadInt32(ж<int32> addr) { return atomic.LoadInt32(addr); }
public static int64 /*val*/ AtomicLoadInt64(ж<int64> addr) { return atomic.LoadInt64(addr); }
public static uint32 /*val*/ AtomicLoadUint32(ж<uint32> addr) { return atomic.LoadUint32(addr); }
public static uint64 /*val*/ AtomicLoadUint64(ж<uint64> addr) { return atomic.LoadUint64(addr); }
public static uintptr /*val*/ AtomicLoadUintptr(ж<uintptr> addr) { return atomic.LoadUintptr(addr); }

public static float32 /*val*/ AtomicLoadFloat32(ж<float32> addr) {
    return math.Float32frombits(atomic.LoadUint32(addr.Reinterpret<float32, uint32>()));
}

public static float64 /*val*/ AtomicLoadFloat64(ж<float64> addr) {
    return math.Float64frombits(atomic.LoadUint64(addr.Reinterpret<float64, uint64>()));
}

public static int32 /*val*/ AtomicLoadPInt32(uintptr addr) {
    return atomic.LoadInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)addr));
}

public static int64 /*val*/ AtomicLoadPInt64(uintptr addr) {
    return atomic.LoadInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)addr));
}

public static uint32 /*val*/ AtomicLoadPUint32(uintptr addr) {
    return atomic.LoadUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr));
}

public static uint64 /*val*/ AtomicLoadPUint64(uintptr addr) {
    return atomic.LoadUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr));
}

public static uintptr /*val*/ AtomicLoadPUintptr(uintptr addr) {
    return atomic.LoadUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)addr));
}

public static float32 /*val*/ AtomicLoadPFloat32(uintptr addr) {
    return math.Float32frombits(atomic.LoadUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr)));
}

public static float64 /*val*/ AtomicLoadPFloat64(uintptr addr) {
    return math.Float64frombits(atomic.LoadUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr)));
}

public static void AtomicStoreInt32(ж<int32> addr, int32 val) { atomic.StoreInt32(addr, val); }
public static void AtomicStoreInt64(ж<int64> addr, int64 val) { atomic.StoreInt64(addr, val); }
public static void AtomicStoreUint32(ж<uint32> addr, uint32 val) { atomic.StoreUint32(addr, val); }
public static void AtomicStoreUint64(ж<uint64> addr, uint64 val) { atomic.StoreUint64(addr, val); }
public static void AtomicStoreUintptr(ж<uintptr> addr, uintptr val) { atomic.StoreUintptr(addr, val); }

public static void AtomicStoreFloat32(ж<float32> addr, float32 val) {
    atomic.StoreUint32(addr.Reinterpret<float32, uint32>(), math.Float32bits(val));
}

public static void AtomicStoreFloat64(ж<float64> addr, float64 val) {
    atomic.StoreUint64(addr.Reinterpret<float64, uint64>(), math.Float64bits(val));
}

public static void AtomicStorePInt32(uintptr addr, int32 val) {
    atomic.StoreInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)addr), val);
}

public static void AtomicStorePInt64(uintptr addr, int64 val) {
    atomic.StoreInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)addr), val);
}

public static void AtomicStorePUint32(uintptr addr, uint32 val) {
    atomic.StoreUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr), val);
}

public static void AtomicStorePUint64(uintptr addr, uint64 val) {
    atomic.StoreUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr), val);
}

public static void AtomicStorePUintptr(uintptr addr, uintptr val) {
    atomic.StoreUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)addr), val);
}

public static void AtomicStorePFloat32(uintptr addr, float32 val) {
    atomic.StoreUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)addr), math.Float32bits(val));
}

public static void AtomicStorePFloat64(uintptr addr, float64 val) {
    atomic.StoreUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)addr), math.Float64bits(val));
}

public static void AtomicStorePInt8(uintptr addr, int8 val) {
    a_store_8(addr, (byte)val);
}

public static void AtomicStorePInt16(uintptr addr, int16 val) {
    a_store_16(addr, (uint16)val);
}

public static void AtomicStorePUint8(uintptr addr, uint8 val) {
    a_store_8(addr, val);
}

public static void AtomicStorePUint16(uintptr addr, uint16 val) {
    a_store_16(addr, val);
}

public static int32 /*new*/ AtomicAddInt32(ж<int32> addr, int32 delta) { return atomic.AddInt32(addr, delta); }
public static int64 /*new*/ AtomicAddInt64(ж<int64> addr, int64 delta) { return atomic.AddInt64(addr, delta); }
public static uint32 /*new*/ AtomicAddUint32(ж<uint32> addr, uint32 delta) { return atomic.AddUint32(addr, delta); }
public static uint64 /*new*/ AtomicAddUint64(ж<uint64> addr, uint64 delta) { return atomic.AddUint64(addr, delta); }

public static uintptr /*new*/ AtomicAddUintptr(ж<uintptr> addr, uintptr delta) {
    return atomic.AddUintptr(addr, delta);
}

public static float32 /*new*/ AtomicAddFloat32(ж<float32> addr, float32 delta) {
    var v = AtomicLoadFloat32(addr) + delta;
    AtomicStoreFloat32(addr, v);
    return v;
}

public static float64 /*new*/ AtomicAddFloat64(ж<float64> addr, float64 delta) {
    var v = AtomicLoadFloat64(addr) + delta;
    AtomicStoreFloat64(addr, v);
    return v;
}

public static partial typesꓸSize_t Xmbstowcs(ж<TLS> t, uintptr dest, uintptr src, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v src=%v n=%v, (%v:)", t.OrTypedNil(), src, n, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmbtowc(ж<TLS> t, uintptr pwc, uintptr s, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v n=%v, (%v:)", t.OrTypedNil(), s, n, origin(2));
    }
    throw panic(todo(""));
}

public static partial typesꓸSize_t X__ctype_get_mb_cur_max(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xwctomb(ж<TLS> t, uintptr s, wchar_t wc) {
    if (__ccgo_strace) {
        trc("t=%v s=%v wc=%v, (%v:)", t.OrTypedNil(), s, wc, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xmblen(ж<TLS> t, uintptr s, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v n=%v, (%v:)", t.OrTypedNil(), s, n, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xpselect(ж<TLS> t, int32 nfds, uintptr readfds, uintptr writefds, uintptr exceptfds, uintptr timeout, uintptr sigmask) {
    if (__ccgo_strace) {
        trc("t=%v nfds=%v sigmask=%v, (%v:)", t.OrTypedNil(), nfds, sigmask, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xtcsendbreak(ж<TLS> t, int32 fd, int32 duration) {
    if (__ccgo_strace) {
        trc("t=%v duration=%v, (%v:)", t.OrTypedNil(), duration, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 Xwcwidth(ж<TLS> t, wchar_t c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    throw panic(todo(""));
}

public static void AtExit(Action f) {
    ᏑatExitMu.Lock();
    atExit = append(atExit, f);
    ᏑatExitMu.Unlock();
}

public static partial void X__ccgo_dmesg(ж<TLS> t, uintptr fmt, uintptr va) {
    if (__ccgo_strace) {
        trc("t=%v fmt=%v va=%v, (%v:)", t.OrTypedNil(), fmt, va, origin(2));
    }
    if (dmesgs) {
        dmesg("%s", printf(fmt, va));
    }
}

public static partial int32 Xgetentropy(ж<TLS> t, uintptr buffer, size_t length) {
    if (__ccgo_strace) {
        trc("t=%v buffer=%v length=%v, (%v:)", t.OrTypedNil(), buffer, length, origin(2));
    }
    UntypedInt max = 256;
    switch (ᐧ) {
    case {} when length is 0: {
        return 0;
    }
    case {} when buffer == 0: {
        t.setErrno((nint)(errno.EFAULT));
        return -1;
    }
    case {} when length > max: {
        t.setErrno((nint)(errno.EIO));
        return -1;
    }}

    var (_, err) = crand.Read(@unsafe.Pointer.View<byte>((@unsafe.Pointer)buffer, 0, (nint)(length), 1125899906842623)); if (err != nil) {
        t.setErrno((nint)(errno.EIO));
        return -1;
    }

    return 0;
}

public static partial uintptr Xreallocarray(ж<TLS> t, uintptr ptr, size_t nmemb, size_t size) {
    if (__ccgo_strace) {
        trc("t=%v ptr=%v size=%v, (%v:)", t.OrTypedNil(), ptr, size, origin(2));
    }
    var (hi, lo) = mathutil.MulUint128_64((uint64)nmemb, (uint64)size);
    if (hi != 0 || lo > (uint64)/* unsafe.Sizeof(RawMem{}) */ (uintptr)1125899906842623) {
        t.setErrno((nint)(errno.ENOMEM));
        return 0;
    }

    return Xrealloc(t, ptr, (size_t)lo);
}

public static partial int32 Xsetjmp(ж<TLS> t, uintptr env) {
    if (__ccgo_strace) {
        trc("t=%v env=%v, (%v:)", t.OrTypedNil(), env, origin(2));
    }
    return 0;
}

public static partial void Xlongjmp(ж<TLS> t, uintptr env, int32 val) {
    if (__ccgo_strace) {
        trc("t=%v env=%v val=%v, (%v:)", t.OrTypedNil(), env, val, origin(2));
    }
    throw panic(todo(""));
}

public static partial int32 X_setjmp(ж<TLS> t, uintptr env) {
    if (__ccgo_strace) {
        trc("t=%v env=%v, (%v:)", t.OrTypedNil(), env, origin(2));
    }
    return 0;
}

public static partial void X_longjmp(ж<TLS> t, uintptr env, int32 val) {
    if (__ccgo_strace) {
        trc("t=%v env=%v val=%v, (%v:)", t.OrTypedNil(), env, val, origin(2));
    }
    throw panic(todo(""));
}

public static partial uint32 X__sync_add_and_fetch_uint32(ж<TLS> t, uintptr p, uint32 v) {
    if (__ccgo_strace) {
        trc("t=%v p=%v v=%v, (%v:)", t.OrTypedNil(), p, v, origin(2));
    }
    return atomic.AddUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p), v);
}

public static partial uint32 X__sync_sub_and_fetch_uint32(ж<TLS> t, uintptr p, uint32 v) {
    if (__ccgo_strace) {
        trc("t=%v p=%v v=%v, (%v:)", t.OrTypedNil(), p, v, origin(2));
    }
    return atomic.AddUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p), ((uint32)0 - v));
}

public static partial int32 Xsched_yield(ж<TLS> t) {
    if (__ccgo_strace) {
        trc("t=%v, (%v:)", t.OrTypedNil(), origin(2));
    }
    runtime.Gosched();
    return 0;
}

public static partial int32 Xgetc(ж<TLS> t, uintptr stream) {
    if (__ccgo_strace) {
        trc("t=%v stream=%v, (%v:)", t.OrTypedNil(), stream, origin(2));
    }
    return Xfgetc(t, stream);
}

public static partial void Xbzero(ж<TLS> t, uintptr s, typesꓸSize_t n) {
    if (__ccgo_strace) {
        trc("t=%v s=%v n=%v, (%v:)", t.OrTypedNil(), s, n, origin(2));
    }
    var b = @unsafe.Pointer.View<byte>((@unsafe.Pointer)s, 0, (nint)(n), 1125899906842623);
    foreach (var i in range(len(b))) {
        b[i] = 0;
    }
}

public static partial uintptr Xrindex(ж<TLS> t, uintptr s, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v s=%v c=%v, (%v:)", t.OrTypedNil(), s, c, origin(2));
    }
    if (s == 0) {
        return 0;
    }

    uintptr r = default;
    for (;;) {
        var c2 = (int32)(~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s));
        if (c2 == c) {
            r = s;
        }

        if (c2 == 0) {
            return r;
        }

        s++;
    }
}

public static partial int32 Xisascii(ж<TLS> t, int32 c) {
    if (__ccgo_strace) {
        trc("t=%v c=%v, (%v:)", t.OrTypedNil(), c, origin(2));
    }
    return Bool32(c >= 0 && c <= 0x7f);
}

public static partial int32 X__builtin_isunordered(ж<TLS> t, float64 a, float64 b) {
    if (__ccgo_strace) {
        trc("t=%v b=%v, (%v:)", t.OrTypedNil(), b, origin(2));
    }
    return Bool32(math.IsNaN(a) || math.IsNaN(b));
}

public static uint16 AtomicLoadNUint16(uintptr ptr, int32 memorder) {
    return (uint16)a_load_16(ptr);
}

public static int32 PreIncAtomicInt32P(uintptr p, int32 d) {
    return atomic.AddInt32(@unsafe.Pointer.To<int32>((@unsafe.Pointer)p), d);
}

public static int64 PreIncAtomicInt64P(uintptr p, int64 d) {
    return atomic.AddInt64(@unsafe.Pointer.To<int64>((@unsafe.Pointer)p), d);
}

public static uint32 PreIncAtomicUint32P(uintptr p, uint32 d) {
    return atomic.AddUint32(@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p), d);
}

public static uint64 PreIncAtomicUint64P(uintptr p, uint64 d) {
    return atomic.AddUint64(@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p), d);
}

public static uintptr PreInrAtomicUintptrP(uintptr p, uintptr d) {
    return atomic.AddUintptr(@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)p), d);
}

public static partial int32 /*r*/ X__builtin_ffs(ж<TLS> tls, int32 i) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v i=%v, (%v:)", tls.OrTypedNil(), i, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xffs(tls, i);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial int32 /*r*/ Xffs(ж<TLS> tls, int32 i) {
    int32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v i=%v, (%v:)", tls.OrTypedNil(), i, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        if (i == 0) {
            r = 0; goto ᒐdone;
        }

        r = (int32)mbits.TrailingZeros32((uint32)i) + 1;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static float32 _toint5 = Float32FromInt32(1) / Float32FromFloat32(1.1920928955078125e-07F);

public static partial float32 /*r*/ X__builtin_rintf(ж<TLS> tls, float32 x) {
    return Xrintf(tls, x);
}

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] internal partial struct Xrintf__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<uint32>))] public readonly int Fi;
    [FieldOffset(0)] public float32 Ff;
}

public static partial float32 /*r*/ Xrintf(ж<TLS> tls, float32 x) {
    float32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        int32 e = default;
        int32 s = default;
        float32 y = default;
        float32 v1 = default;
        Xrintf__ᴛ1 _ᴛ1 = new();
        _ = e;
        _ = s;
        _ = y;
        _ = v1;
        (@unsafe.Pointer.To<Xrintf__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xrintf__ᴛ1();
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
        e = (int32)(~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp) >> (int)((int32)23) & (uint32)0xff);
        s = (int32)(~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp) >> (int)((int32)31));
        if (e >= Int32FromInt32(0x7f) + Int32FromInt32(23)) {
            r = x; goto ᒐdone;
        }
        if (s != 0) {
            y = x - _toint5 + _toint5;
        } else {
            y = x + _toint5 - _toint5;
        }
        if (y == Float32FromInt32(0)) {
            if (s != 0) {
                v1 = -Float32FromFloat32(0F);
            } else {
                v1 = Float32FromFloat32(0F);
            }
            r = v1; goto ᒐdone;
        }
        r = y;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial @long /*r*/ X__builtin_lrintf(ж<TLS> tls, float32 x) {
    return Xlrintf(tls, x);
}

public static partial @long /*r*/ Xlrintf(ж<TLS> tls, float32 x) {
    @long r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = (@long)Xrintf(tls, x);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial @long /*r*/ X__builtin_lrint(ж<TLS> tls, float64 x) {
    return Xlrint(tls, x);
}

public static partial @long /*r*/ Xlrint(ж<TLS> tls, float64 x) {
    @long r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = (@long)Xrint(tls, x);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial float64 /*r*/ X__builtin_trunc(ж<TLS> tls, float64 x) {
    return Xtrunc(tls, x);
}

public static partial float64 /*r*/ X__builtin_fmin(ж<TLS> tls, float64 x, float64 y) {
    return Xfmin(tls, x, y);
}

internal partial struct Xfmin__ᴛ1 /*dyn*/ {
    public array<uint64> F__i = new(0);
    public float64 F__f;
}

public static partial float64 /*r*/ Xfmin(ж<TLS> tls, float64 x, float64 y) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v y=%v, (%v:)", tls.OrTypedNil(), x, y, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uint64 v1 = default;
        uint64 v10 = default;
        uint64 v3 = default;
        uint64 v5 = default;
        uint64 v7 = default;
        float64 v12 = default;
        float64 v9 = default;
        Xfmin__ᴛ1 _ᴛ1 = new();
        _ = v1;
        _ = v10;
        _ = v12;
        _ = v3;
        _ = v5;
        _ = v7;
        _ = v9;
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
        v1 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _2;
_2:
        if (BoolInt32((v1 & ((((uint64)0 - Uint64FromUint64(1))).Rsh((int64)(Int32FromInt32(1))))) > Uint64FromUint64(0x7ff).Lsh((int64)(Int32FromInt32(52)))) != 0) {
            r = y; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = y;
        v3 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _4;
_4:
        if (BoolInt32((v3 & ((((uint64)0 - Uint64FromUint64(1))).Rsh((int64)(Int32FromInt32(1))))) > Uint64FromUint64(0x7ff).Lsh((int64)(Int32FromInt32(52)))) != 0) {
            r = x; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
        v5 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _6;
_6:
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = y;
        v7 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _8;
_8:
        if (Int32FromUint64(v5.Rsh((int64)(Int32FromInt32(63)))) != Int32FromUint64(v7.Rsh((int64)(Int32FromInt32(63))))) {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
            v10 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
            goto _11;
_11:
            if (Int32FromUint64(v10.Rsh((int64)(Int32FromInt32(63)))) != 0) {
                v9 = x;
            } else {
                v9 = y;
            }
            r = v9; goto ᒐdone;
        }
        if (x < y) {
            v12 = x;
        } else {
            v12 = y;
        }
        r = v12;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal partial struct Xfminf__ᴛ1 /*dyn*/ {
    public array<uint32> F__i = new(0);
    public float32 F__f;
}

public static partial float32 /*r*/ Xfminf(ж<TLS> tls, float32 x, float32 y) {
    float32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v y=%v, (%v:)", tls.OrTypedNil(), x, y, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uint32 v1 = default;
        uint32 v10 = default;
        uint32 v3 = default;
        uint32 v5 = default;
        uint32 v7 = default;
        float32 v12 = default;
        float32 v9 = default;
        Xfminf__ᴛ1 _ᴛ1 = new();
        _ = v1;
        _ = v10;
        _ = v12;
        _ = v3;
        _ = v5;
        _ = v7;
        _ = v9;
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
        v1 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _2;
_2:
        if (BoolInt32((v1 & (uint32)0x7fffffff) > (uint32)0x7f800000) != 0) {
            r = y; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = y;
        v3 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _4;
_4:
        if (BoolInt32((v3 & (uint32)0x7fffffff) > (uint32)0x7f800000) != 0) {
            r = x; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
        v5 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _6;
_6:
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = y;
        v7 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _8;
_8:
        if (Int32FromUint32(v5.Rsh((int64)(Int32FromInt32(31)))) != Int32FromUint32(v7.Rsh((int64)(Int32FromInt32(31))))) {
            (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
            v10 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
            goto _11;
_11:
            if (Int32FromUint32(v10.Rsh((int64)(Int32FromInt32(31)))) != 0) {
                v9 = x;
            } else {
                v9 = y;
            }
            r = v9; goto ᒐdone;
        }
        if (x < y) {
            v12 = x;
        } else {
            v12 = y;
        }
        r = v12;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial float64 /*r*/ Xfminl(ж<TLS> tls, float64 x, float64 y) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v y=%v, (%v:)", tls.OrTypedNil(), x, y, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xfmin(tls, x, y);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal partial struct Xfmax__ᴛ1 /*dyn*/ {
    public array<uint64> F__i = new(0);
    public float64 F__f;
}

public static partial float64 /*r*/ Xfmax(ж<TLS> tls, float64 x, float64 y) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v y=%v, (%v:)", tls.OrTypedNil(), x, y, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uint64 v1 = default;
        uint64 v10 = default;
        uint64 v3 = default;
        uint64 v5 = default;
        uint64 v7 = default;
        float64 v12 = default;
        float64 v9 = default;
        Xfmax__ᴛ1 _ᴛ1 = new();
        _ = v1;
        _ = v10;
        _ = v12;
        _ = v3;
        _ = v5;
        _ = v7;
        _ = v9;
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
        v1 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _2;
_2:
        if (BoolInt32((v1 & ((((uint64)0 - Uint64FromUint64(1))).Rsh((int64)(Int32FromInt32(1))))) > Uint64FromUint64(0x7ff).Lsh((int64)(Int32FromInt32(52)))) != 0) {
            r = y; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = y;
        v3 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _4;
_4:
        if (BoolInt32((v3 & ((((uint64)0 - Uint64FromUint64(1))).Rsh((int64)(Int32FromInt32(1))))) > Uint64FromUint64(0x7ff).Lsh((int64)(Int32FromInt32(52)))) != 0) {
            r = x; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
        v5 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _6;
_6:
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = y;
        v7 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
        goto _8;
_8:
        if (Int32FromUint64(v5.Rsh((int64)(Int32FromInt32(63)))) != Int32FromUint64(v7.Rsh((int64)(Int32FromInt32(63))))) {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
            v10 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
            goto _11;
_11:
            if (Int32FromUint64(v10.Rsh((int64)(Int32FromInt32(63)))) != 0) {
                v9 = y;
            } else {
                v9 = x;
            }
            r = v9; goto ᒐdone;
        }
        if (x < y) {
            v12 = y;
        } else {
            v12 = x;
        }
        r = v12;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal partial struct Xfmaxf__ᴛ1 /*dyn*/ {
    public array<uint32> F__i = new(0);
    public float32 F__f;
}

public static partial float32 /*r*/ Xfmaxf(ж<TLS> tls, float32 x, float32 y) {
    float32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v y=%v, (%v:)", tls.OrTypedNil(), x, y, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        uint32 v1 = default;
        uint32 v10 = default;
        uint32 v3 = default;
        uint32 v5 = default;
        uint32 v7 = default;
        float32 v12 = default;
        float32 v9 = default;
        Xfmaxf__ᴛ1 _ᴛ1 = new();
        _ = v1;
        _ = v10;
        _ = v12;
        _ = v3;
        _ = v5;
        _ = v7;
        _ = v9;
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
        v1 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _2;
_2:
        if (BoolInt32((v1 & (uint32)0x7fffffff) > (uint32)0x7f800000) != 0) {
            r = y; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = y;
        v3 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _4;
_4:
        if (BoolInt32((v3 & (uint32)0x7fffffff) > (uint32)0x7f800000) != 0) {
            r = x; goto ᒐdone;
        }
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
        v5 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _6;
_6:
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = y;
        v7 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
        goto _8;
_8:
        if (Int32FromUint32(v5.Rsh((int64)(Int32FromInt32(31)))) != Int32FromUint32(v7.Rsh((int64)(Int32FromInt32(31))))) {
            (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
            v10 = ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)bp);
            goto _11;
_11:
            if (Int32FromUint32(v10.Rsh((int64)(Int32FromInt32(31)))) != 0) {
                v9 = y;
            } else {
                v9 = x;
            }
            r = v9; goto ᒐdone;
        }
        if (x < y) {
            v12 = y;
        } else {
            v12 = x;
        }
        r = v12;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial float64 /*r*/ Xfmaxl(ж<TLS> tls, float64 x, float64 y) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v y=%v, (%v:)", tls.OrTypedNil(), x, y, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xfmax(tls, x, y);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

public static partial float64 /*r*/ X__builtin_fmax(ж<TLS> tls, float64 x, float64 y) {
    return Xfmax(tls, x, y);
}

internal partial struct Xexpm1__ᴛ1 /*dyn*/ {
    public array<uint64> F__i = new(0);
    public float64 F__f;
}

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct Xexpm1__ᴛ2 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint64_t>))] public readonly long Fi;
    [FieldOffset(0)] public float64 Ff;
}

public static partial float64 /*r*/ Xexpm1(ж<TLS> tls, float64 x3) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x3=%v, (%v:)", tls.OrTypedNil(), x3, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tdouble_t c = default;
        Tdouble_t e = default;
        Tdouble_t hfx = default;
        Tdouble_t hi = default;
        Tdouble_t hxs = default;
        Tdouble_t lo = default;
        Tdouble_t r1 = default;
        Tdouble_t t = default;
        Tdouble_t twopk = default;
        Tdouble_t y3 = default;
        Tuint32_t hx = default;
        int32 k = default;
        int32 sign = default;
        float32 y = default;
        float64 y1 = default;
        float64 y2 = default;
        float64 v3 = default;
        uint64 v1 = default;
        Xexpm1__ᴛ1 _ᴛ1 = new();
        Xexpm1__ᴛ2 _ᴛ2 = new();
        _ = c;
        _ = e;
        _ = hfx;
        _ = hi;
        _ = hx;
        _ = hxs;
        _ = k;
        _ = lo;
        _ = r1;
        _ = sign;
        _ = t;
        _ = twopk;
        _ = y;
        _ = y1;
        _ = y2;
        _ = y3;
        _ = v1;
        _ = v3;
        (@unsafe.Pointer.To<Xexpm1__ᴛ2>((@unsafe.Pointer)(bp + 8))).Value = new Xexpm1__ᴛ2();
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)(bp + 8))).Value = x3;
        hx = (uint32)((Tuint64_t)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)(bp + 8)) >> (int)((int32)32) & (uint64)0x7fffffff));
        sign = Int32FromUint64(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)(bp + 8)) >> (int)((int32)63));
        if (hx >= (uint32)0x4043687A) {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x3;
            v1 = ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)bp);
            goto _2;
_2:
            if (BoolInt32((v1 & ((((uint64)0 - Uint64FromUint64(1))).Rsh((int64)(Int32FromInt32(1))))) > Uint64FromUint64(0x7ff).Lsh((int64)(Int32FromInt32(52)))) != 0) {
                r = x3; goto ᒐdone;
            }
            if (sign != 0) {
                r = (float64)(-Int32FromInt32(1)); goto ᒐdone;
            }
            if (x3 > _o_threshold) {
                x3 *= (float64)8.98846567431158e+307D;
                r = x3; goto ᒐdone;
            }
        }
        if (hx > (uint32)0x3fd62e42) {
            if (hx < (uint32)0x3FF0A2B2) {
                if (!(sign != 0)) {
                    hi = x3 - _ln2_hi;
                    lo = _ln2_lo;
                    k = (int32)1;
                } else {
                    hi = x3 + _ln2_hi;
                    lo = -_ln2_lo;
                    k = -(int32)1;
                }
            } else {
                if (sign != 0) {
                    v3 = -Float64FromFloat64(0.5D);
                } else {
                    v3 = (float64)0.5D;
                }
                k = (int32)((float64)(_invln2 * x3) + v3);
                t = (float64)k;
                hi = x3 - (float64)(t * _ln2_hi);
                lo = (Tdouble_t)(t * _ln2_lo);
            }
            x3 = hi - lo;
            c = hi - x3 - lo;
        } else {
            if (hx < (uint32)0x3c900000) {
                if (hx < (uint32)0x00100000) {
                    if ((uint64)4 == (uint64)4) {
                        y = (float32)x3;
                    } else {
                        if ((uint64)4 == (uint64)8) {
                            y1 = (float64)(float32)x3;
                        } else {
                            y2 = (float64)(float32)x3;
                        }
                    }
                }
                r = x3; goto ᒐdone;
            } else {
                k = 0;
            }
        }
        hfx = (Tdouble_t)((float64)0.5D * x3);
        hxs = (Tdouble_t)(x3 * hfx);
        r1 = (float64)1D + (float64)(hxs * (_Q1 + (float64)(hxs * (_Q2 + (float64)(hxs * (_Q3 + (float64)(hxs * (_Q4 + (float64)(hxs * _Q5)))))))));
        t = (float64)3D - (float64)(r1 * hfx);
        e = (Tdouble_t)(hxs * ((r1 - t) / (Float64FromFloat64(6D) - (float64)(x3 * t))));
        if (k == 0) {
            r = x3 - ((float64)(x3 * e) - hxs); goto ᒐdone;
        }
        e = (float64)(x3 * (e - c)) - c;
        e -= hxs;
        if (k == -(int32)1) {
            r = (float64)((float64)0.5D * (x3 - e)) - (float64)0.5D; goto ᒐdone;
        }
        if (k == (int32)1) {
            if (x3 < -Float64FromFloat64(0.25D)) {
                r = (float64)(-Float64FromFloat64(2D) * (e - (x3 + Float64FromFloat64(0.5D)))); goto ᒐdone;
            }
            r = (float64)1D + (float64)((float64)2D * (x3 - e)); goto ᒐdone;
        }
        (@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)(bp + 8))).Value = Uint64FromInt32(Int32FromInt32(0x3ff) + k) << (int)((int32)52);
        twopk = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)(bp + 8));
        if (k < 0 || k > (int32)56) {
            y3 = x3 - e + (float64)1D;
            if (k == (int32)1024) {
                y3 = (Tdouble_t)(((Tdouble_t)(y3 * (float64)2D)) * (float64)8.98846567431158e+307D);
            } else {
                y3 = (Tdouble_t)(y3 * twopk);
            }
            r = y3 - (float64)1D; goto ᒐdone;
        }
        (@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)(bp + 8))).Value = Uint64FromInt32(Int32FromInt32(0x3ff) - k) << (int)((int32)52);
        if (k < (int32)20) {
            y3 = (Tdouble_t)((x3 - e + (Float64FromInt32(1) - ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)(bp + 8)))) * twopk);
        } else {
            y3 = (Tdouble_t)((x3 - (e + ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)(bp + 8))) + Float64FromInt32(1)) * twopk);
        }
        r = y3;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static float32 _ln2_hi1 = (float32)0.69313812256F;
internal static float32 _ln2_lo1 = (float32)9.0580006145e-06F;
internal static float32 _invln21 = (float32)1.4426950216F;
internal static float32 _Q11 = (float32)(-Float64FromFloat64(0.033333212137D));
internal static float32 _Q21 = (float32)0.0015807170421F;

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] internal partial struct Xexpm1f__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint32_t>))] public readonly int Fi;
    [FieldOffset(0)] public float32 Ff;
}

public static partial float32 /*r*/ Xexpm1f(ж<TLS> tls, float32 x3) {
    float32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x3=%v, (%v:)", tls.OrTypedNil(), x3, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tfloat_t c = default;
        Tfloat_t e = default;
        Tfloat_t hfx = default;
        Tfloat_t hi = default;
        Tfloat_t hxs = default;
        Tfloat_t lo = default;
        Tfloat_t r1 = default;
        Tfloat_t t = default;
        Tfloat_t twopk = default;
        Tfloat_t y3 = default;
        Tuint32_t hx = default;
        int32 k = default;
        int32 sign = default;
        float32 y = default;
        float32 v1 = default;
        float64 y1 = default;
        float64 y2 = default;
        Xexpm1f__ᴛ1 _ᴛ1 = new();
        _ = c;
        _ = e;
        _ = hfx;
        _ = hi;
        _ = hx;
        _ = hxs;
        _ = k;
        _ = lo;
        _ = r1;
        _ = sign;
        _ = t;
        _ = twopk;
        _ = y;
        _ = y1;
        _ = y2;
        _ = y3;
        _ = v1;
        (@unsafe.Pointer.To<Xexpm1f__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xexpm1f__ᴛ1();
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x3;
        hx = (Tuint32_t)(~@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp) & (uint32)0x7fffffff);
        sign = Int32FromUint32(~@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp) >> (int)((int32)31));
        if (hx >= (uint32)0x4195b844) {
            if (hx > (uint32)0x7f800000) {
                r = x3; goto ᒐdone;
            }
            if (sign != 0) {
                r = (float32)(-Int32FromInt32(1)); goto ᒐdone;
            }
            if (hx > (uint32)0x42b17217) {
                x3 *= Float32FromFloat32(1.7014118346046923e+38F);
                r = x3; goto ᒐdone;
            }
        }
        if (hx > (uint32)0x3eb17218) {
            if (hx < (uint32)0x3F851592) {
                if (!(sign != 0)) {
                    hi = x3 - _ln2_hi1;
                    lo = _ln2_lo1;
                    k = (int32)1;
                } else {
                    hi = x3 + _ln2_hi1;
                    lo = -_ln2_lo1;
                    k = -(int32)1;
                }
            } else {
                if (sign != 0) {
                    v1 = -Float32FromFloat32(0.5F);
                } else {
                    v1 = Float32FromFloat32(0.5F);
                }
                k = (int32)((float32)(_invln21 * x3) + v1);
                t = (float32)k;
                hi = x3 - (float32)(t * _ln2_hi1);
                lo = (Tfloat_t)(t * _ln2_lo1);
            }
            x3 = hi - lo;
            c = hi - x3 - lo;
        } else {
            if (hx < (uint32)0x33000000) {
                if (hx < (uint32)0x00800000) {
                    if ((uint64)4 == (uint64)4) {
                        y = (float32)(x3 * x3);
                    } else {
                        if ((uint64)4 == (uint64)8) {
                            y1 = (float64)(x3 * x3);
                        } else {
                            y2 = (float64)(x3 * x3);
                        }
                    }
                }
                r = x3; goto ᒐdone;
            } else {
                k = 0;
            }
        }
        hfx = (Tfloat_t)(Float32FromFloat32(0.5F) * x3);
        hxs = (Tfloat_t)(x3 * hfx);
        r1 = Float32FromFloat32(1F) + (float32)(hxs * (_Q11 + (float32)(hxs * _Q21)));
        t = Float32FromFloat32(3F) - (float32)(r1 * hfx);
        e = (Tfloat_t)(hxs * ((r1 - t) / (Float32FromFloat32(6F) - (float32)(x3 * t))));
        if (k == 0) {
            r = x3 - ((float32)(x3 * e) - hxs); goto ᒐdone;
        }
        e = (float32)(x3 * (e - c)) - c;
        e -= hxs;
        if (k == -(int32)1) {
            r = (float32)(Float32FromFloat32(0.5F) * (x3 - e)) - Float32FromFloat32(0.5F); goto ᒐdone;
        }
        if (k == (int32)1) {
            if (x3 < -Float32FromFloat32(0.25F)) {
                r = (float32)(-Float32FromFloat32(2F) * (e - (x3 + Float32FromFloat32(0.5F)))); goto ᒐdone;
            }
            r = Float32FromFloat32(1F) + (float32)(Float32FromFloat32(2F) * (x3 - e)); goto ᒐdone;
        }
        (@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp)).Value = Uint32FromInt32(((int32)0x7f + k) << (int)((int32)23));
        twopk = ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp);
        if (k < 0 || k > (int32)56) {
            y3 = x3 - e + Float32FromFloat32(1F);
            if (k == (int32)128) {
                y3 = (Tfloat_t)(((Tfloat_t)(y3 * Float32FromFloat32(2F))) * Float32FromFloat32(1.7014118346046923e+38F));
            } else {
                y3 = (Tfloat_t)(y3 * twopk);
            }
            r = y3 - Float32FromFloat32(1F); goto ᒐdone;
        }
        (@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp)).Value = Uint32FromInt32(((int32)0x7f - k) << (int)((int32)23));
        if (k < (int32)23) {
            y3 = (Tfloat_t)((x3 - e + (Float32FromInt32(1) - ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp))) * twopk);
        } else {
            y3 = (Tfloat_t)((x3 - (e + ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)) + Float32FromInt32(1)) * twopk);
        }
        r = y3;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial float64 /*r*/ Xexpm1l(ж<TLS> tls, float64 x) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xexpm1(tls, x);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static float64 _o_threshold = (float64)709.782712893384D;
internal static float64 _ln2_hi = (float64)0.6931471803691238D;
internal static float64 _ln2_lo = (float64)1.9082149292705877e-10D;
internal static float64 _invln2 = (float64)1.4426950408889634D;
internal static float64 _Q1 = -Float64FromFloat64(0.03333333333333313D);
internal static float64 _Q2 = (float64)0.0015873015872548146D;
internal static float64 _Q3 = -Float64FromFloat64(7.93650757867488e-05D);
internal static float64 _Q4 = (float64)4.008217827329362e-06D;
internal static float64 _Q5 = -Float64FromFloat64(2.0109921818362437e-07D);

internal static float64 _ln2_hi2 = (float64)0.6931471803691238D;
internal static float64 _ln2_lo2 = (float64)1.9082149292705877e-10D;
internal static float64 _Lg12 = (float64)0.6666666666666735D;
internal static float64 _Lg22 = (float64)0.3999999999940942D;
internal static float64 _Lg32 = (float64)0.2857142874366239D;
internal static float64 _Lg42 = (float64)0.22222198432149784D;
internal static float64 _Lg51 = (float64)0.1818357216161805D;
internal static float64 _Lg61 = (float64)0.15313837699209373D;
internal static float64 _Lg71 = (float64)0.14798198605116586D;

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct Xlog1p__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint64_t>))] public readonly long Fi;
    [FieldOffset(0)] public float64 Ff;
}

public static partial float64 /*r*/ Xlog1p(ж<TLS> tls, float64 x3) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x3=%v, (%v:)", tls.OrTypedNil(), x3, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tdouble_t R = default;
        Tdouble_t c = default;
        Tdouble_t dk = default;
        Tdouble_t f = default;
        Tdouble_t hfsq = default;
        Tdouble_t s = default;
        Tdouble_t t1 = default;
        Tdouble_t t2 = default;
        Tdouble_t w = default;
        Tdouble_t z = default;
        Tuint32_t hu = default;
        Tuint32_t hx = default;
        int32 k = default;
        float32 y = default;
        float64 y1 = default;
        float64 y2 = default;
        float64 v1 = default;
        Xlog1p__ᴛ1 _ᴛ1 = new();
        _ = R;
        _ = c;
        _ = dk;
        _ = f;
        _ = hfsq;
        _ = hu;
        _ = hx;
        _ = k;
        _ = s;
        _ = t1;
        _ = t2;
        _ = w;
        _ = y;
        _ = y1;
        _ = y2;
        _ = z;
        _ = v1;
        (@unsafe.Pointer.To<Xlog1p__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xlog1p__ᴛ1();
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x3;
        hx = (uint32)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) >> (int)((int32)32));
        k = (int32)1;
        if (hx < (uint32)0x3fda827a || hx >> (int)((int32)31) != 0) {
            if (hx >= (uint32)0xbff00000U) {
                if (x3 == (float64)(-Int32FromInt32(1))) {
                    r = x3 / (float64)0D; goto ᒐdone;
                }
                r = (x3 - x3) / (float64)0D; goto ᒐdone;
            }
            if (hx << (int)((int32)1) < Uint32FromInt32(Int32FromInt32(0x3ca00000).Lsh((int64)(Int32FromInt32(1))))) {
                if ((Tuint32_t)(hx & (uint32)0x7ff00000) == (uint32)0) {
                    if ((uint64)4 == (uint64)4) {
                        y = (float32)x3;
                    } else {
                        if ((uint64)4 == (uint64)8) {
                            y1 = (float64)(float32)x3;
                        } else {
                            y2 = (float64)(float32)x3;
                        }
                    }
                }
                r = x3; goto ᒐdone;
            }
            if (hx <= (uint32)0xbfd2bec4U) {
                k = 0;
                c = Float64FromInt32(0);
                f = x3;
            }
        } else {
            if (hx >= (uint32)0x7ff00000) {
                r = x3; goto ᒐdone;
            }
        }
        if (k != 0) {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = Float64FromInt32(1) + x3;
            hu = (uint32)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) >> (int)((int32)32));
            hu += Uint32FromInt32(Int32FromInt32(0x3ff00000) - Int32FromInt32(0x3fe6a09e));
            k = Int32FromUint32(hu >> (int)(Int32FromInt32(20))) - (int32)0x3ff;
            if (k < (int32)54) {
                if (k >= (int32)2) {
                    v1 = Float64FromInt32(1) - (~@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp) - x3);
                } else {
                    v1 = x3 - (~@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp) - Float64FromInt32(1));
                }
                c = v1;
                c /= ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp);
            } else {
                c = Float64FromInt32(0);
            }
            hu = (Tuint32_t)(hu & (uint32)0x000fffff) + (uint32)0x3fe6a09e;
            (@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp)).Value = (uint64)hu << (int)((int32)32) | (Tuint64_t)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) & (uint64)0xffffffffU);
            f = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp) - Float64FromInt32(1);
        }
        hfsq = (Tdouble_t)((float64)((float64)0.5D * f) * f);
        s = f / (Float64FromFloat64(2D) + f);
        z = (Tdouble_t)(s * s);
        w = (Tdouble_t)(z * z);
        t1 = (Tdouble_t)(w * (_Lg22 + (float64)(w * (_Lg42 + (float64)(w * _Lg61)))));
        t2 = (Tdouble_t)(z * (_Lg12 + (float64)(w * (_Lg32 + (float64)(w * (_Lg51 + (float64)(w * _Lg71)))))));
        R = t2 + t1;
        dk = (float64)k;
        r = ((Tdouble_t)(s * (hfsq + R))) + (((Tdouble_t)(dk * _ln2_lo2)) + c) - hfsq + f + ((Tdouble_t)(dk * _ln2_hi2));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

internal static float32 _ln2_hi3 = (float32)0.69313812256F;
internal static float32 _ln2_lo3 = (float32)9.0580006145e-06F;
internal static float32 _Lg13 = (float32)0.6666666269302368F;
internal static float32 _Lg23 = (float32)0.40000972151756287F;
internal static float32 _Lg33 = (float32)0.2849878668785095F;
internal static float32 _Lg43 = (float32)0.24279078841209412F;

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] internal partial struct Xlog1pf__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint32_t>))] public readonly int Fi;
    [FieldOffset(0)] public float32 Ff;
}

public static partial float32 /*r*/ Xlog1pf(ж<TLS> tls, float32 x3) {
    float32 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x3=%v, (%v:)", tls.OrTypedNil(), x3, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tfloat_t R = default;
        Tfloat_t c = default;
        Tfloat_t dk = default;
        Tfloat_t f = default;
        Tfloat_t hfsq = default;
        Tfloat_t s = default;
        Tfloat_t t1 = default;
        Tfloat_t t2 = default;
        Tfloat_t w = default;
        Tfloat_t z = default;
        Tuint32_t iu = default;
        Tuint32_t ix = default;
        int32 k = default;
        float32 y = default;
        float32 v1 = default;
        float64 y1 = default;
        float64 y2 = default;
        Xlog1pf__ᴛ1 _ᴛ1 = new();
        _ = R;
        _ = c;
        _ = dk;
        _ = f;
        _ = hfsq;
        _ = iu;
        _ = ix;
        _ = k;
        _ = s;
        _ = t1;
        _ = t2;
        _ = w;
        _ = y;
        _ = y1;
        _ = y2;
        _ = z;
        _ = v1;
        (@unsafe.Pointer.To<Xlog1pf__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xlog1pf__ᴛ1();
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x3;
        ix = ~@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp);
        k = (int32)1;
        if (ix < (uint32)0x3ed413d0 || ix >> (int)((int32)31) != 0) {
            if (ix >= (uint32)0xbf800000U) {
                if (x3 == (float32)(-Int32FromInt32(1))) {
                    r = x3 / Float32FromFloat32(0F); goto ᒐdone;
                }
                r = (x3 - x3) / Float32FromFloat32(0F); goto ᒐdone;
            }
            if (ix << (int)((int32)1) < Uint32FromInt32(Int32FromInt32(0x33800000).Lsh((int64)(Int32FromInt32(1))))) {
                if ((Tuint32_t)(ix & (uint32)0x7f800000) == (uint32)0) {
                    if ((uint64)4 == (uint64)4) {
                        y = (float32)(x3 * x3);
                    } else {
                        if ((uint64)4 == (uint64)8) {
                            y1 = (float64)(x3 * x3);
                        } else {
                            y2 = (float64)(x3 * x3);
                        }
                    }
                }
                r = x3; goto ᒐdone;
            }
            if (ix <= (uint32)0xbe95f619U) {
                k = 0;
                c = Float32FromInt32(0);
                f = x3;
            }
        } else {
            if (ix >= (uint32)0x7f800000) {
                r = x3; goto ᒐdone;
            }
        }
        if (k != 0) {
            (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = Float32FromInt32(1) + x3;
            iu = ~@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp);
            iu += Uint32FromInt32(Int32FromInt32(0x3f800000) - Int32FromInt32(0x3f3504f3));
            k = Int32FromUint32(iu >> (int)(Int32FromInt32(23))) - (int32)0x7f;
            if (k < (int32)25) {
                if (k >= (int32)2) {
                    v1 = Float32FromInt32(1) - (~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp) - x3);
                } else {
                    v1 = x3 - (~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp) - Float32FromInt32(1));
                }
                c = v1;
                c /= ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp);
            } else {
                c = Float32FromInt32(0);
            }
            iu = (Tuint32_t)(iu & (uint32)0x007fffff) + (uint32)0x3f3504f3;
            (@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp)).Value = iu;
            f = ~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp) - Float32FromInt32(1);
        }
        s = f / (Float32FromFloat32(2F) + f);
        z = (Tfloat_t)(s * s);
        w = (Tfloat_t)(z * z);
        t1 = (Tfloat_t)(w * (_Lg23 + (float32)(w * _Lg43)));
        t2 = (Tfloat_t)(z * (_Lg13 + (float32)(w * _Lg33)));
        R = t2 + t1;
        hfsq = (Tfloat_t)((float32)(Float32FromFloat32(0.5F) * f) * f);
        dk = (float32)k;
        r = ((Tfloat_t)(s * (hfsq + R))) + (((Tfloat_t)(dk * _ln2_lo3)) + c) - hfsq + f + ((Tfloat_t)(dk * _ln2_hi3));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r;
}

public static partial float64 /*r*/ Xlog1pl(ж<TLS> tls, float64 x) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xlog1p(tls, x);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static uint32 _B1 = (uint32)715094163;
internal static uint32 _B2 = (uint32)696219795;

internal static float64 _P0 = (float64)1.87595182427177D;
internal static float64 _P1 = -Float64FromFloat64(1.8849797954337717D);
internal static float64 _P2 = (float64)1.6214297201053545D;
internal static float64 _P3 = -Float64FromFloat64(0.758397934778766D);
internal static float64 _P4 = (float64)0.14599619288661245D;

[global::go.golib.GoInlineLayout(Size = 8, Align = 8), StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct Xcbrt__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint64_t>))] public readonly long Fi;
    [FieldOffset(0)] public float64 Ff;
}

public static partial float64 /*r1*/ Xcbrt(ж<TLS> tls, float64 x) {
    float64 r1 = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r1);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tuint32_t hx = default;
        Tdouble_t r = default;
        Tdouble_t s = default;
        Tdouble_t t = default;
        Tdouble_t w = default;
        uintptr p1 = default;
        Xcbrt__ᴛ1 _ᴛ1 = new();
        _ = hx;
        _ = r;
        _ = s;
        _ = t;
        _ = w;
        _ = p1;
        (@unsafe.Pointer.To<Xcbrt__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xcbrt__ᴛ1();
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = x;
        hx = (uint32)((Tuint64_t)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) >> (int)((int32)32) & (uint64)0x7fffffff));
        if (hx >= (uint32)0x7ff00000) {
            r1 = x + x; goto ᒐdone;
        }
        if (hx < (uint32)0x00100000) {
            (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = (float64)(x * (float64)1.8014398509481984e+16D);
            hx = (uint32)((Tuint64_t)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) >> (int)((int32)32) & (uint64)0x7fffffff));
            if (hx == (uint32)0) {
                r1 = x; goto ᒐdone;
            }
            hx = hx / (uint32)3 + _B2;
        } else {
            hx = hx / (uint32)3 + _B1;
        }
        p1 = bp;
        (@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)p1)).Value = (Tuint64_t)((Tuint64_t)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)p1) & (Uint64FromUint64(1).Lsh((int64)(Int32FromInt32(63))))));
        (@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp)).Value |= ((uint64)hx << (int)((int32)32));
        t = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp);
        r = (Tdouble_t)(((Tdouble_t)(t * t)) * (t / x));
        t = (Tdouble_t)(t * (_P0 + (float64)(r * (_P1 + (float64)(r * _P2))) + (float64)(((Tdouble_t)(((Tdouble_t)(r * r)) * r)) * (_P3 + (float64)(r * _P4)))));
        (@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp)).Value = t;
        (@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp)).Value = (uint64)(~@unsafe.Pointer.To<Tuint64_t>((@unsafe.Pointer)bp) + Uint64FromUint32(0x80000000U)) & (uint64)0xffffffffc0000000UL;
        t = ~@unsafe.Pointer.To<float64>((@unsafe.Pointer)bp);
        s = (Tdouble_t)(t * t);
        r = x / s;
        w = t + t;
        r = (r - t) / (w + r);
        t = t + ((Tdouble_t)(t * r));
        r1 = t;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r1;
}

internal static uint32 _B11 = (uint32)709958130;
internal static uint32 _B21 = (uint32)642849266;

[global::go.golib.GoInlineLayout(Size = 4, Align = 4), StructLayout(LayoutKind.Explicit, Size = 4)] internal partial struct Xcbrtf__ᴛ1 /*dyn*/ {
    [FieldOffset(0)] [GoAlignmentCarrier(typeof(array<Tuint32_t>))] public readonly int Fi;
    [FieldOffset(0)] public float32 Ff;
}

public static partial float32 /*r1*/ Xcbrtf(ж<TLS> tls, float32 x) {
    float32 r1 = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r1);
            }, ref ᒐ);
        }
        var bp = tls.Alloc(16);
        defer(tls.Free, (nint)16, ref ᒐ);
        Tdouble_t T = default;
        Tdouble_t r = default;
        Tuint32_t hx = default;
        Xcbrtf__ᴛ1 _ᴛ1 = new();
        _ = T;
        _ = hx;
        _ = r;
        (@unsafe.Pointer.To<Xcbrtf__ᴛ1>((@unsafe.Pointer)bp)).Value = new Xcbrtf__ᴛ1();
        (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = x;
        hx = (Tuint32_t)(~@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp) & (uint32)0x7fffffff);
        if (hx >= (uint32)0x7f800000) {
            r1 = x + x; goto ᒐdone;
        }
        if (hx < (uint32)0x00800000) {
            if (hx == (uint32)0) {
                r1 = x; goto ᒐdone;
            }
            (@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp)).Value = (float32)(x * Float32FromFloat32(1.6777216e+07F));
            hx = (Tuint32_t)(~@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp) & (uint32)0x7fffffff);
            hx = hx / (uint32)3 + _B21;
        } else {
            hx = hx / (uint32)3 + _B11;
        }
        (@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp)).Value &= (uint32)0x80000000U;
        (@unsafe.Pointer.To<Tuint32_t>((@unsafe.Pointer)bp)).Value |= hx;
        T = (float64)(~@unsafe.Pointer.To<float32>((@unsafe.Pointer)bp));
        r = (Tdouble_t)(((Tdouble_t)(T * T)) * T);
        T = ((Tdouble_t)(T * ((float64)x + (float64)x + r))) / ((float64)x + r + r);
        r = (Tdouble_t)(((Tdouble_t)(T * T)) * T);
        T = ((Tdouble_t)(T * ((float64)x + (float64)x + r))) / ((float64)x + r + r);
        r1 = (float32)T;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return r1;
}

public static partial float64 /*r*/ Xcbrtl(ж<TLS> tls, float64 x) {
    float64 r = default;
    GoFrame ᒐ = default;
    try {
        if (__ccgo_strace) {
            trc("tls=%v x=%v, (%v:)", tls.OrTypedNil(), x, origin(2));
            defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                trc("-> %v", r);
            }, ref ᒐ);
        }
        r = Xcbrt(tls, x);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return r;
}

internal static void putenvNarrow(ж<TLS> t, @string kv) {
    nint x = strings.IndexByte(kv, (byte)'=');
    if (x <= 0) {
        return;
    }

    var p = Environ();
    nint n = 0;
    for (; ; n++) {
        var q = ~@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + (uintptr)n * uintptrSize));
        if (q == 0) {
            break;
        }

        @string s = GoString(q);
        if (len(s) > x && s[x] == '=' && s.slice(0, x) == kv.slice(0, x)) {
            var nsΔ1 = Xcalloc(t, 1, (typesꓸSize_t)(len(kv) + 1));
            copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)nsΔ1), len(kv)), kv);
            Xfree(t, q);
            (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(p + (uintptr)n * uintptrSize))).Value = nsΔ1;
            return;
        }
    }

    var np = Xrealloc(t, p, (typesꓸSize_t)((n + 2) * (nint)uintptrSize));
    if (np == 0) {
        throw panic("OOM");
    }

    var ns = Xcalloc(t, 1, (typesꓸSize_t)(len(kv) + 1));
    copy(@unsafe.Slice(@unsafe.Pointer.To<byte>((@unsafe.Pointer)ns), len(kv)), kv);
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(np + (uintptr)n * uintptrSize))).Value = ns;
    (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)(np + (uintptr)(n + 1) * uintptrSize))).Value = 0;
    Xenviron = np;
}

} // end libc_package
