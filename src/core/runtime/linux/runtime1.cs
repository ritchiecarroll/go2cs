// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytealg = @internal.bytealg_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using abi = @internal.abi_package;

partial class runtime_package {

// Keep a cached value to make gotraceback fast,
// since we call it on every call to gentraceback.
// The cached value is a uint32 in which the low bits
// are the "crash" and "all" settings and the remaining
// bits are the traceback value (0 off, 1 on, 2 include system).
internal static UntypedInt tracebackCrash => /* 1 << iota */ 1;

internal static UntypedInt tracebackAll => 2;

internal static UntypedInt tracebackShift => /* iota */ 2;

internal static ж<uint32> Ꮡtraceback_cache = new StandardBox<uint32>(((uint32)2 << (int)(tracebackShift)));
internal static ref uint32 traceback_cache => ref Ꮡtraceback_cache.Value;

internal static uint32 traceback_env;

// gotraceback returns the current traceback settings.
//
// If level is 0, suppress all tracebacks.
// If level is 1, show tracebacks, but exclude runtime frames.
// If level is 2, show tracebacks including runtime frames.
// If all is set, print all goroutine stacks. Otherwise, print just the current goroutine.
// If crash is set, crash (core dump, etc) after tracebacking.
//
//go:nosplit
internal static (int32 level, bool all, bool crash) gotraceback() {
    int32 level = default!;
    bool all = default!;
    bool crash = default!;

    var gp = getg();
    var t = atomic.Load(Ꮡtraceback_cache);
    crash = (uint32)(t & (uint32)tracebackCrash) != 0;
    all = (~(~gp).m).throwing >= throwTypeUser || (uint32)(t & (uint32)tracebackAll) != 0;
    if ((~(~gp).m).traceback != 0){
        level = (int32)(~(~gp).m).traceback;
    } else 
    if ((~(~gp).m).throwing >= throwTypeRuntime){
        // Always include runtime frames in runtime throws unless
        // otherwise overridden by m.traceback.
        level = 2;
    } else {
        level = (int32)((t >> (int)(tracebackShift)));
    }
    return (level, all, crash);
}

internal static int32 argc;
internal static ж<ж<byte>> argv;

// nosplit for use in linux startup sysargs.
//
//go:nosplit
internal static ж<byte> argv_index(ж<ж<byte>> Ꮡargv, int32 i) {
    return ~(ж<ж<byte>>)(uintptr)(add(@unsafe.Pointer.FromPinnedBox(Ꮡargv), (uintptr)i * (uintptr)goarch.PtrSize));
}

internal static void args(int32 c, ж<ж<byte>> Ꮡv) {
    argc = c;
    argv = Ꮡv;
    sysargs(c, Ꮡv);
}

internal static void goargs() {
    if (GOOS == "windows"u8) {
        return;
    }
    argslice = new slice<@string>(argc);
    for (var i = (int32)0; i < argc; i++) {
        argslice[i] = gostringnocopy(argv_index(argv, i));
    }
}

internal static void goenvs_unix() {
    // TODO(austin): ppc64 in dynamic linking mode doesn't
    // guarantee env[] will immediately follow argv. Might cause
    // problems.
    var n = (int32)0;
    while (argv_index(argv, argc + 1 + n) != nil) {
        n++;
    }
    envs = new slice<@string>(n);
    for (var i = (int32)0; i < n; i++) {
        envs[i] = gostring(argv_index(argv, argc + 1 + i));
    }
}

internal static slice<@string> environ() {
    return envs;
}

// TODO: These should be locals in testAtomic64, but we don't 8-byte
// align stack variables on 386.
internal static ж<uint64> Ꮡtest_z64 = new StandardBox<uint64>(default(uint64));
internal static ref uint64 test_z64 => ref Ꮡtest_z64.Value;
internal static uint64 test_x64;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cas64Failedˢ = "cas64 failed"u8;
internal static readonly @string load64Failedˢ = "load64 failed"u8;
internal static readonly @string store64Failedˢ = "store64 failed"u8;
internal static readonly @string xadd64Failedˢ = "xadd64 failed"u8;
internal static readonly @string xchg64Failedˢ = "xchg64 failed"u8;

internal static void testAtomic64() {
    test_z64 = 42;
    test_x64 = 0;
    if (atomic.Cas64(Ꮡtest_z64, test_x64, 1)) {
        @throw(cas64Failedˢ);
    }
    if (test_x64 != 0) {
        @throw(cas64Failedˢ);
    }
    test_x64 = 42;
    if (!atomic.Cas64(Ꮡtest_z64, test_x64, 1)) {
        @throw(cas64Failedˢ);
    }
    if (test_x64 != 42 || test_z64 != 1) {
        @throw(cas64Failedˢ);
    }
    if (atomic.Load64(Ꮡtest_z64) != 1) {
        @throw(load64Failedˢ);
    }
    atomic.Store64(Ꮡtest_z64, (uint64)((1099511627776L) + 1));
    if (atomic.Load64(Ꮡtest_z64) != (uint64)((1099511627776L) + 1)) {
        @throw(store64Failedˢ);
    }
    if (atomic.Xadd64(Ꮡtest_z64, 1099511627777L) != (uint64)((2199023255552L) + 2)) {
        @throw(xadd64Failedˢ);
    }
    if (atomic.Load64(Ꮡtest_z64) != (uint64)((2199023255552L) + 2)) {
        @throw(xadd64Failedˢ);
    }
    if (atomic.Xchg64(Ꮡtest_z64, (uint64)((3298534883328L) + 3)) != (uint64)((2199023255552L) + 2)) {
        @throw(xchg64Failedˢ);
    }
    if (atomic.Load64(Ꮡtest_z64) != (uint64)((3298534883328L) + 3)) {
        @throw(xchg64Failedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badAˢ = "bad a"u8;
internal static readonly @string badBˢ = "bad b"u8;
internal static readonly @string badCˢ = "bad c"u8;
internal static readonly @string badDˢ = "bad d"u8;
internal static readonly @string badEˢ = "bad e"u8;
internal static readonly @string badFˢ = "bad f"u8;
internal static readonly @string badGˢ = "bad g"u8;
internal static readonly @string badHˢ = "bad h"u8;
internal static readonly @string badIˢ = "bad i"u8;
internal static readonly @string badJˢ = "bad j"u8;
internal static readonly @string badKˢ = "bad k"u8;
internal static readonly @string badLˢ = "bad l"u8;
internal static readonly @string badUnsafeSizeofX1ˢ = "bad unsafe.Sizeof x1"u8;
internal static readonly @string badOffsetofY1Yˢ = "bad offsetof y1.y"u8;
internal static readonly @string badUnsafeSizeofY1ˢ = "bad unsafe.Sizeof y1"u8;
internal static readonly @string badTimedivˢ = "bad timediv"u8;
internal static readonly @string cas1ˢ = "cas1"u8;
internal static readonly @string cas2ˢ = "cas2"u8;
internal static readonly @string cas3ˢ = "cas3"u8;
internal static readonly @string cas4ˢ = "cas4"u8;
internal static readonly @string cas5ˢ = "cas5"u8;
internal static readonly @string cas6ˢ = "cas6"u8;
internal static readonly @string atomicor8ˢ = "atomicor8"u8;
internal static readonly @string atomicand8ˢ = "atomicand8"u8;
internal static readonly @string float64nanˢ = "float64nan"u8;
internal static readonly @string float64nan1ˢ = "float64nan1"u8;
internal static readonly @string float64nan2ˢ = "float64nan2"u8;
internal static readonly @string float64nan3ˢ = "float64nan3"u8;
internal static readonly @string float32nanˢ = "float32nan"u8;
internal static readonly @string float32nan1ˢ = "float32nan1"u8;
internal static readonly @string float32nan2ˢ = "float32nan2"u8;
internal static readonly @string float32nan3ˢ = "float32nan3"u8;
internal static readonly @string fixedStackIsNotPowerOf2ˢ = "FixedStack is not power-of-2"u8;
internal static readonly @string assemblyChecksFailedˢ = "assembly checks failed"u8;

[GoType("dyn")] internal partial struct check_x1t {
    internal uint8 x;
}

[GoType("dyn")] internal partial struct check_y1t {
    internal check_x1t x1;
    internal uint8 y;
}

internal static void check() {
    int8 a = default!;
    uint8 b = default!;
    int16 c = default!;
    uint16 d = default!;
    ref var e = ref heap(new int32(), out var Ꮡe);
    uint32 f = default!;
    int64 g = default!;
    uint64 h = default!;
    ref var i = ref heap(new float32(), out var Ꮡi);
    ref var i1 = ref heap(new float32(), out var Ꮡi1);
    ref var j = ref heap(new float64(), out var Ꮡj);
    ref var j1 = ref heap(new float64(), out var Ꮡj1);
    @unsafe.Pointer k = default!;
    ж<uint16> l = default!;
    ref var m = ref heap(new array<byte>(4), out var Ꮡm);
    check_x1t x1 = default!;
    check_y1t y1 = default!;
    if (/* unsafe.Sizeof(a) */ (uintptr)1 != 1) {
        @throw(badAˢ);
    }
    if (/* unsafe.Sizeof(b) */ (uintptr)1 != 1) {
        @throw(badBˢ);
    }
    if (/* unsafe.Sizeof(c) */ (uintptr)2 != 2) {
        @throw(badCˢ);
    }
    if (/* unsafe.Sizeof(d) */ (uintptr)2 != 2) {
        @throw(badDˢ);
    }
    if (/* unsafe.Sizeof(e) */ (uintptr)4 != 4) {
        @throw(badEˢ);
    }
    if (/* unsafe.Sizeof(f) */ (uintptr)4 != 4) {
        @throw(badFˢ);
    }
    if (/* unsafe.Sizeof(g) */ (uintptr)8 != 8) {
        @throw(badGˢ);
    }
    if (/* unsafe.Sizeof(h) */ (uintptr)8 != 8) {
        @throw(badHˢ);
    }
    if (/* unsafe.Sizeof(i) */ (uintptr)4 != 4) {
        @throw(badIˢ);
    }
    if (/* unsafe.Sizeof(j) */ (uintptr)8 != 8) {
        @throw(badJˢ);
    }
    if (/* unsafe.Sizeof(k) */ (uintptr)8 != goarch.PtrSize) {
        @throw(badKˢ);
    }
    if (/* unsafe.Sizeof(l) */ (uintptr)8 != goarch.PtrSize) {
        @throw(badLˢ);
    }
    if (/* unsafe.Sizeof(x1) */ (uintptr)1 != 1) {
        @throw(badUnsafeSizeofX1ˢ);
    }
    if (/* unsafe.Offsetof(y1.y) */ (uintptr)1 != 1) {
        @throw(badOffsetofY1Yˢ);
    }
    if (/* unsafe.Sizeof(y1) */ (uintptr)2 != 2) {
        @throw(badUnsafeSizeofY1ˢ);
    }
    if (timediv(12345000054321L, 1000000000, Ꮡe) != 12345 || e != 54321) {
        @throw(badTimedivˢ);
    }
    ref var z = ref heap(new uint32(), out var Ꮡz);
    z = 1;
    if (!atomic.Cas(Ꮡz, 1, 2)) {
        @throw(cas1ˢ);
    }
    if (z != 2) {
        @throw(cas2ˢ);
    }
    z = 4;
    if (atomic.Cas(Ꮡz, 5, 6)) {
        @throw(cas3ˢ);
    }
    if (z != 4) {
        @throw(cas4ˢ);
    }
    z = 0xffffffffU;
    if (!atomic.Cas(Ꮡz, 0xffffffffU, 0xfffffffeU)) {
        @throw(cas5ˢ);
    }
    if (z != 0xfffffffeU) {
        @throw(cas6ˢ);
    }
    m = new byte[]{1, 1, 1, 1}.array();
    atomic.Or8(Ꮡm.at<byte>(1), 0xf0);
    if (m[0] != 1 || m[1] != 0xf1 || m[2] != 1 || m[3] != 1) {
        @throw(atomicor8ˢ);
    }
    m = new byte[]{0xff, 0xff, 0xff, 0xff}.array();
    atomic.And8(Ꮡm.at<byte>(1), 0x1);
    if (m[0] != 0xff || m[1] != 0x1 || m[2] != 0xff || m[3] != 0xff) {
        @throw(atomicand8ˢ);
    }
    (Ꮡj.Reinterpret<float64, uint64>()).Value = ~(uint64)0;
    if (j == j) {
        @throw(float64nanˢ);
    }
    if (!(j != j)) {
        @throw(float64nan1ˢ);
    }
    (Ꮡj1.Reinterpret<float64, uint64>()).Value = ~(uint64)1;
    if (j == j1) {
        @throw(float64nan2ˢ);
    }
    if (!(j != j1)) {
        @throw(float64nan3ˢ);
    }
    (Ꮡi.Reinterpret<float32, uint32>()).Value = ~(uint32)0;
    if (i == i) {
        @throw(float32nanˢ);
    }
    if (i == i) {
        @throw(float32nan1ˢ);
    }
    (Ꮡi1.Reinterpret<float32, uint32>()).Value = ~(uint32)1;
    if (i == i1) {
        @throw(float32nan2ˢ);
    }
    if (i == i1) {
        @throw(float32nan3ˢ);
    }
    testAtomic64();
    if (fixedStack != round2(fixedStack)) {
        @throw(fixedStackIsNotPowerOf2ˢ);
    }
    if (!checkASM()) {
        @throw(assemblyChecksFailedˢ);
    }
}

[GoType] partial struct dbgVar {
    internal @string name;
    internal ж<int32> value;     // for variables that can only be set at startup
    internal ж<atomic.Int32> atomic; // for variables that can be changed during execution
    internal int32 def;         // default value (ideally zero)
}

// Holds variables parsed from GODEBUG env var,
// except for "memprofilerate" since there is an
// existing int var for that value, which may
// already have an initial value.

[GoType("dyn")] partial struct debugᴛ1 {
    internal int32 cgocheck;
    internal int32 clobberfree;
    internal int32 disablethp;
    internal int32 dontfreezetheworld;
    internal int32 efence;
    internal int32 gccheckmark;
    internal int32 gcpacertrace;
    internal int32 gcshrinkstackoff;
    internal int32 gcstoptheworld;
    internal int32 gctrace;
    internal int32 invalidptr;
    internal int32 madvdontneed; // for Linux; issue 28466
    internal atomic.Int32 runtimeContentionStacks;
    internal int32 scavtrace;
    internal int32 scheddetail;
    internal int32 schedtrace;
    internal int32 tracebackancestors;
    internal int32 asyncpreemptoff;
    internal int32 harddecommit;
    internal int32 adaptivestackstart;
    internal int32 tracefpunwindoff;
    internal int32 traceadvanceperiod;
    internal int32 traceCheckStackOwnership;
    internal int32 profstackdepth;
    internal int32 dataindependenttiming;
    // debug.malloc is used as a combined debug check
    // in the malloc function and should be set
    // if any of the below debug options is != 0.
    internal bool malloc;
    internal int32 inittrace;
    internal int32 sbrk;
    // traceallocfree controls whether execution traces contain
    // detailed trace data about memory allocation. This value
    // affects debug.malloc only if it is != 0 and the execution
    // tracer is enabled, in which case debug.malloc will be
    // set to "true" if it isn't already while tracing is enabled.
    // It will be set while the world is stopped, so it's safe.
    // The value of traceallocfree can be changed any time in response
    // to os.Setenv("GODEBUG").
    internal atomic.Int32 traceallocfree;
    internal atomic.Int32 panicnil;
    // asynctimerchan controls whether timer channels
    // behave asynchronously (as in Go 1.22 and earlier)
    // instead of their Go 1.23+ synchronous behavior.
    // The value can change at any time (in response to os.Setenv("GODEBUG"))
    // and affects all extant timer channels immediately.
    // Programs wouldn't normally change over an execution,
    // but allowing it is convenient for testing and for programs
    // that do an os.Setenv in main.init or main.main.
    internal atomic.Int32 asynctimerchan;
}
internal static ж<debugᴛ1> Ꮡdebug = new StandardBox<debugᴛ1>(default(debugᴛ1));
internal static ref debugᴛ1 debug => ref Ꮡdebug.Value;

internal static slice<ж<dbgVar>> dbgvars = new ж<dbgVar>[]{
    Ꮡ(new dbgVar(name: "adaptivestackstart"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡadaptivestackstart))),
    Ꮡ(new dbgVar(name: "asyncpreemptoff"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡasyncpreemptoff))),
    Ꮡ(new dbgVar(name: "asynctimerchan"u8, atomic: Ꮡdebug.of(debugᴛ1.Ꮡasynctimerchan))),
    Ꮡ(new dbgVar(name: "cgocheck"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡcgocheck))),
    Ꮡ(new dbgVar(name: "clobberfree"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡclobberfree))),
    Ꮡ(new dbgVar(name: "dataindependenttiming"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡdataindependenttiming))),
    Ꮡ(new dbgVar(name: "disablethp"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡdisablethp))),
    Ꮡ(new dbgVar(name: "dontfreezetheworld"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡdontfreezetheworld))),
    Ꮡ(new dbgVar(name: "efence"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡefence))),
    Ꮡ(new dbgVar(name: "gccheckmark"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡgccheckmark))),
    Ꮡ(new dbgVar(name: "gcpacertrace"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡgcpacertrace))),
    Ꮡ(new dbgVar(name: "gcshrinkstackoff"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡgcshrinkstackoff))),
    Ꮡ(new dbgVar(name: "gcstoptheworld"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡgcstoptheworld))),
    Ꮡ(new dbgVar(name: "gctrace"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡgctrace))),
    Ꮡ(new dbgVar(name: "harddecommit"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡharddecommit))),
    Ꮡ(new dbgVar(name: "inittrace"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡinittrace))),
    Ꮡ(new dbgVar(name: "invalidptr"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡinvalidptr))),
    Ꮡ(new dbgVar(name: "madvdontneed"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡmadvdontneed))),
    Ꮡ(new dbgVar(name: "panicnil"u8, atomic: Ꮡdebug.of(debugᴛ1.Ꮡpanicnil))),
    Ꮡ(new dbgVar(name: "profstackdepth"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡprofstackdepth), def: 128)),
    Ꮡ(new dbgVar(name: "runtimecontentionstacks"u8, atomic: Ꮡdebug.of(debugᴛ1.ᏑruntimeContentionStacks))),
    Ꮡ(new dbgVar(name: "sbrk"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡsbrk))),
    Ꮡ(new dbgVar(name: "scavtrace"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡscavtrace))),
    Ꮡ(new dbgVar(name: "scheddetail"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡscheddetail))),
    Ꮡ(new dbgVar(name: "schedtrace"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡschedtrace))),
    Ꮡ(new dbgVar(name: "traceadvanceperiod"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡtraceadvanceperiod))),
    Ꮡ(new dbgVar(name: "traceallocfree"u8, atomic: Ꮡdebug.of(debugᴛ1.Ꮡtraceallocfree))),
    Ꮡ(new dbgVar(name: "tracecheckstackownership"u8, value: Ꮡdebug.of(debugᴛ1.ᏑtraceCheckStackOwnership))),
    Ꮡ(new dbgVar(name: "tracebackancestors"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡtracebackancestors))),
    Ꮡ(new dbgVar(name: "tracefpunwindoff"u8, value: Ꮡdebug.of(debugᴛ1.Ꮡtracefpunwindoff)))
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string gotracebackˢ = "GOTRACEBACK"u8;

internal static void parsedebugvars() {
    // defaults
    debug.cgocheck = 1;
    debug.invalidptr = 1;
    debug.adaptivestackstart = 1; // set this to 0 to turn larger initial goroutine stacks off
    if (GOOS == "linux"u8) {
        // On Linux, MADV_FREE is faster than MADV_DONTNEED,
        // but doesn't affect many of the statistics that
        // MADV_DONTNEED does until the memory is actually
        // reclaimed. This generally leads to poor user
        // experience, like confusing stats in top and other
        // monitoring tools; and bad integration with
        // management systems that respond to memory usage.
        // Hence, default to MADV_DONTNEED.
        debug.madvdontneed = 1;
    }
    debug.traceadvanceperiod = defaultTraceAdvancePeriod;
    @string godebug = gogetenv(godebugˢ);
    var Δp = @new<@string>();
    Δp.Value = godebug;
    ᏑgodebugEnv.Store(Δp);
    // apply runtime defaults, if any
    foreach (var (_, v) in dbgvars) {
        if ((~v).def != 0) {
            // Every var should have either v.value or v.atomic set.
            if ((~v).value != nil){
                (~v).value.Value = v.Value.def;
            } else 
            if ((~v).atomic != nil) {
                (~v).atomic.Store((~v).def);
            }
        }
    }
    // apply compile-time GODEBUG settings
    parsegodebug(godebugDefault, default!);
    // apply environment settings
    parsegodebug(godebug, default!);
    debug.malloc = ((int32)(debug.inittrace | debug.sbrk)) != 0;
    debug.profstackdepth = min(debug.profstackdepth, (int32)(maxProfStackDepth));
    setTraceback(gogetenv(gotracebackˢ));
    traceback_env = traceback_cache;
}

// reparsedebugvars reparses the runtime's debug variables
// because the environment variable has been changed to env.
internal static void reparsedebugvars(@string env) {
    var seen = new map<@string, bool>();
    // apply environment settings
    parsegodebug(env, seen);
    // apply compile-time GODEBUG settings for as-yet-unseen variables
    parsegodebug(godebugDefault, seen);
    // apply defaults for as-yet-unseen variables
    foreach (var (_, v) in dbgvars) {
        if ((~v).atomic != nil && !seen[(~v).name]) {
            (~v).atomic.Store(0);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgocheck1ModeIsNoLongerˢ = "cgocheck > 1 mode is no longer supported at runtime. Use GOEXPERIMENT=cgocheck2 at build time instead."u8;

// parsegodebug parses the godebug string, updating variables listed in dbgvars.
// If seen == nil, this is startup time and we process the string left to right
// overwriting older settings with newer ones.
// If seen != nil, $GODEBUG has changed and we are doing an
// incremental update. To avoid flapping in the case where a value is
// set multiple times (perhaps in the default and the environment,
// or perhaps twice in the environment), we process the string right-to-left
// and only change values not already seen. After doing this for both
// the environment and the default settings, the caller must also call
// cleargodebug(seen) to reset any now-unset values back to their defaults.
internal static void parsegodebug(@string godebug, map<@string, bool> seen) {
    for (@string Δp = godebug; Δp != ""u8; ) {
        @string field = default!;
        if (seen == default!){
            // startup: process left to right, overwriting older settings with newer
            nint iΔ1 = bytealg.IndexByteString(Δp, (rune)',');
            if (iΔ1 < 0){
                (field, Δp) = (Δp, "");
            } else {
                (field, Δp) = (Δp.slice(0, iΔ1), Δp.slice(iΔ1 + 1));
            }
        } else {
            // incremental update: process right to left, updating and skipping seen
            nint iΔ2 = len(Δp) - 1;
            while (iΔ2 >= 0 && Δp[iΔ2] != (rune)',') {
                iΔ2--;
            }
            if (iΔ2 < 0){
                (Δp, field) = ("", Δp);
            } else {
                (Δp, field) = (Δp.slice(0, iΔ2), Δp.slice(iΔ2 + 1));
            }
        }
        nint i = bytealg.IndexByteString(field, (rune)'=');
        if (i < 0) {
            continue;
        }
        @string key = field.slice(0, i);
        @string value = field.slice(i + 1);
        if (seen[key]) {
            continue;
        }
        if (seen != default!) {
            seen[key] = true;
        }
        // Update MemProfileRate directly here since it
        // is int, not int32, and should only be updated
        // if specified in GODEBUG.
        if (seen == default! && key == "memprofilerate"u8){
            {
                var (n, ok) = atoi(value); if (ok) {
                    MemProfileRate = n;
                }
            }
        } else {
            foreach (var (_, v) in dbgvars) {
                if ((~v).name == key) {
                    {
                        var (n, ok) = atoi32(value); if (ok) {
                            if (seen == default! && (~v).value != nil){
                                (~v).value.Value = n;
                            } else 
                            if ((~v).atomic != nil) {
                                (~v).atomic.Store(n);
                            }
                        }
                    }
                }
            }
        }
    }
    if (debug.cgocheck > 1) {
        @throw(cgocheck1ModeIsNoLongerˢ);
    }
}

//go:linkname setTraceback runtime/debug.SetTraceback
internal static void setTraceback(@string level) {
    uint32 t = default!;
    var exprᴛ1 = level;
    var matchᴛ1 = false;
    if (exprᴛ1 == "none"u8) { matchᴛ1 = true;
        t = 0;
    }
    else if (exprᴛ1 == "single"u8 || exprᴛ1 == ""u8) { matchᴛ1 = true;
        t = ((uint32)1 << (int)(tracebackShift));
    }
    else if (exprᴛ1 == "all"u8) { matchᴛ1 = true;
        t = (uint32)((uint32)(1 << (int)(tracebackShift)) | (uint32)tracebackAll);
    }
    else if (exprᴛ1 == "system"u8) { matchᴛ1 = true;
        t = (uint32)((uint32)(2 << (int)(tracebackShift)) | (uint32)tracebackAll);
    }
    else if (exprᴛ1 == "crash"u8) { matchᴛ1 = true;
        t = (uint32)((UntypedInt)((2 << (int)(tracebackShift)) | tracebackAll) | (uint32)tracebackCrash);
    }
    else if (exprᴛ1 == "wer"u8) { matchᴛ1 = true;
        do {
            if (GOOS == "windows"u8) {
                t = (uint32)((UntypedInt)((2 << (int)(tracebackShift)) | tracebackAll) | (uint32)tracebackCrash);
                enableWER();
                break;
            }
            fallthrough = true;
        } while (false);
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        t = tracebackAll;
        {
            var (n, ok) = atoi(level); if (ok && n == (nint)(uint32)n) {
                t |= (uint32)(((uint32)n << (int)(tracebackShift)));
            }
        }
    }

    // when C owns the process, simply exit'ing the process on fatal errors
    // and panics is surprising. Be louder and abort instead.
    if (islibrary || isarchive) {
        t |= (uint32)(tracebackCrash);
    }
    t |= (uint32)(traceback_env);
    atomic.Store(Ꮡtraceback_cache, t);
}

// Poor mans 64-bit division.
// This is a very special function, do not use it if you are not sure what you are doing.
// int64 division is lowered into _divv() call on 386, which does not fit into nosplit functions.
// Handles overflow in a time-specific manner.
// This keeps us within no-split stack limits on 32-bit processors.
//
//go:nosplit
internal static int32 timediv(int64 v, int32 div, ж<int32> Ꮡrem) {
    ref var rem = ref Ꮡrem.DerefOrNull();

    var res = (int32)0;
    for (nint bit = 30; bit >= 0; bit--) {
        if (v >= ((int64)div).Lsh((nuint)bit)) {
            v = v - (((int64)div).Lsh((nuint)bit));
            // Before this for loop, res was 0, thus all these
            // power of 2 increments are now just bitsets.
            res |= (int32)(((int32)1).Lsh((nuint)bit));
        }
    }
    if (v >= (int64)div) {
        if (Ꮡrem != nil) {
            rem = 0;
        }
        return 0x7fffffff;
    }
    if (Ꮡrem != nil) {
        rem = (int32)v;
    }
    return res;
}

// Helpers for Go. Must be NOSPLIT, must only call NOSPLIT functions, and must not block.

//go:nosplit
internal static ж<m> acquirem() {
    var gp = getg();
    gp.Value.m.Value.locks++;
    return (~gp).m;
}

//go:nosplit
internal static void releasem(ref m mp) {
    var gp = getg();
    mp.locks--;
    if (mp.locks == 0 && (~gp).preempt) {
        // restore the preemption request in case we've cleared it in newstack
        gp.Value.stackguard0 = stackPreempt;
    }
}

// reflect_typelinks is meant for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/goccy/json
//   - github.com/modern-go/reflect2
//   - github.com/vmware/govmomi
//   - github.com/pinpoint-apm/pinpoint-go-agent
//   - github.com/timandy/routine
//   - github.com/v2pro/plz
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_typelinks reflect.typelinks
internal static (slice<@unsafe.Pointer>, slice<slice<int32>>) reflect_typelinks() {
    var modules = activeModules();
    var sections = new @unsafe.Pointer[]{(@unsafe.Pointer)(~modules[0]).types}.slice();
    var ret = new slice<int32>[]{(~modules[0]).typelinks}.slice();
    foreach (var (_, md) in modules[1..]) {
        sections = append(sections, (@unsafe.Pointer)(~md).types);
        ret = append(ret, (~md).typelinks);
    }
    return (sections, ret);
}

// reflect_resolveNameOff resolves a name offset from a base pointer.
//
// reflect_resolveNameOff is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/agiledragon/gomonkey/v2
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_resolveNameOff reflect.resolveNameOff
public static @unsafe.Pointer reflect_resolveNameOff(@unsafe.Pointer ptrInModule, int32 off) {
    return @unsafe.Pointer.FromPinnedBox(resolveNameOff(ptrInModule, ((nameOff)off)).Bytes);
}

// reflect_resolveTypeOff resolves an *rtype offset from a base type.
//
// reflect_resolveTypeOff is meant for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/modern-go/reflect2
//   - github.com/v2pro/plz
//   - github.com/timandy/routine
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_resolveTypeOff reflect.resolveTypeOff
public static @unsafe.Pointer reflect_resolveTypeOff(@unsafe.Pointer Δrtype, int32 off) {
    return @unsafe.Pointer.FromPinnedBox(toRType((ж<_type>)(uintptr)(Δrtype)).typeOff(((typeOff)off)));
}

// reflect_resolveTextOff resolves a function pointer offset from a base type.
//
// reflect_resolveTextOff is for package reflect,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/agiledragon/gomonkey/v2
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname reflect_resolveTextOff reflect.resolveTextOff
public static @unsafe.Pointer reflect_resolveTextOff(@unsafe.Pointer Δrtype, int32 off) {
    return (uintptr)toRType((ж<_type>)(uintptr)(Δrtype)).textOff(((textOff)off));
}

// reflectlite_resolveNameOff resolves a name offset from a base pointer.
//
//go:linkname reflectlite_resolveNameOff internal/reflectlite.resolveNameOff
internal static @unsafe.Pointer reflectlite_resolveNameOff(@unsafe.Pointer ptrInModule, int32 off) {
    return @unsafe.Pointer.FromPinnedBox(resolveNameOff(ptrInModule, ((nameOff)off)).Bytes);
}

// reflectlite_resolveTypeOff resolves an *rtype offset from a base type.
//
//go:linkname reflectlite_resolveTypeOff internal/reflectlite.resolveTypeOff
internal static @unsafe.Pointer reflectlite_resolveTypeOff(@unsafe.Pointer Δrtype, int32 off) {
    return @unsafe.Pointer.FromPinnedBox(toRType((ж<_type>)(uintptr)(Δrtype)).typeOff(((typeOff)off)));
}

// reflect_addReflectOff adds a pointer to the reflection offset lookup map.
//
//go:linkname reflect_addReflectOff reflect.addReflectOff
public static int32 reflect_addReflectOff(@unsafe.Pointer ptr) {
    reflectOffsLock();
    if (reflectOffs.m == default!) {
        reflectOffs.m = new map<int32, @unsafe.Pointer>();
        reflectOffs.minv = new map<@unsafe.Pointer, int32>();
        reflectOffs.next = -1;
    }
    var (id, found) = reflectOffs.minv[ptr, ꟷ];
    if (!found) {
        id = reflectOffs.next;
        reflectOffs.next--; // use negative offsets as IDs to aid debugging
        reflectOffs.m[id] = ptr;
        reflectOffs.minv[ptr] = id;
    }
    reflectOffsUnlock();
    return id;
}

//go:linkname fips_getIndicator crypto/internal/fips140.getIndicator
public static uint8 fips_getIndicator() {
    return (~getg()).fipsIndicator;
}

//go:linkname fips_setIndicator crypto/internal/fips140.setIndicator
public static void fips_setIndicator(uint8 indicator) {
    getg().Value.fipsIndicator = indicator;
}

} // end runtime_package
