// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// CPU profiling.
//
// The signal handler for the profiling clock tick adds a new stack trace
// to a log of recent traces. The log is read by a user goroutine that
// turns it into formatted profile data. If the reader does not keep up
// with the log, those writes will be recorded as a count of lost records.
// The actual profile buffer is in profbuf.go.
namespace go;

using abi = @internal.abi_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static UntypedInt maxCPUProfStack => 64;
internal static UntypedInt profBufWordCount => /* 1 << 17 */ 131072;
internal static UntypedInt profBufTagCount => /* 1 << 14 */ 16384;

[GoType] partial struct cpuProfile {
    internal mutex @lock;
    internal bool on;     // profiling is on
    internal ж<profBuf> log; // profile events written here
    // extra holds extra stacks accumulated in addNonGo
    // corresponding to profiling signals arriving on
    // non-Go-created threads. Those stacks are written
    // to log the next time a normal Go thread gets the
    // signal handler.
    // Assuming the stacks are 2 words each (we don't get
    // a full traceback from those threads), plus one word
    // size for framing, 100 Hz profiling would generate
    // 300 words per second.
    // Hopefully a normal Go thread will get the profiling
    // signal at least once every few seconds.
    internal array<uintptr> extra = new(1000);
    internal nint numExtra;
    internal uint64 lostExtra; // count of frames lost because extra is full
    internal uint64 lostAtomic; // count of frames lost because of being in atomic64 on mips/arm; updated racily
}

internal static ж<cpuProfile> Ꮡcpuprof = new StandardBox<cpuProfile>(new cpuProfile());
internal static ref cpuProfile cpuprof => ref Ꮡcpuprof.Value;

// SetCPUProfileRate sets the CPU profiling rate to hz samples per second.
// If hz <= 0, SetCPUProfileRate turns off profiling.
// If the profiler is on, the rate cannot be changed without first turning it off.
//
// Most clients should use the [runtime/pprof] package or
// the [testing] package's -test.cpuprofile flag instead of calling
// SetCPUProfileRate directly.
public static void SetCPUProfileRate(nint hz) {
    // Clamp hz to something reasonable.
    if (hz < 0) {
        hz = 0;
    }
    if (hz > 1000000) {
        hz = 1000000;
    }
    @lock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
    if (hz > 0){
        if (cpuprof.on || cpuprof.log != nil) {
            print((@string)"runtime: cannot set cpu profile rate until previous profile has finished.\n"u8);
            unlock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
            return;
        }
        cpuprof.on = true;
        cpuprof.log = newProfBuf(1, profBufWordCount, profBufTagCount);
        var hdr = new uint64[]{(uint64)hz}.array();
        cpuprof.log.write(nil, nanotime(), hdr[..], default!);
        setcpuprofilerate((int32)hz);
    } else 
    if (cpuprof.on) {
        setcpuprofilerate(0);
        cpuprof.on = false;
        cpuprof.addExtra();
        cpuprof.log.close();
    }
    unlock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
}

// add adds the stack trace to the profile.
// It is called from signal handlers and other limited environments
// and cannot allocate memory or acquire locks that might be
// held at the time of the signal, nor can it use substantial amounts
// of stack.
//
//go:nowritebarrierrec
[GoRecv] internal static void add(this ref cpuProfile Δp, ж<@unsafe.Pointer> ᏑtagPtr, slice<uintptr> stk) {
    // Simple cas-lock to coordinate with setcpuprofilerate.
    while (!Ꮡprof.of(profᴛ1.ᏑsignalLock).CompareAndSwap(0, 1)) {
        // TODO: Is it safe to osyield here? https://go.dev/issue/52672
        osyield();
    }
    if (Ꮡprof.of(profᴛ1.Ꮡhz).Load() != 0) {
        // implies cpuprof.log != nil
        if (Δp.numExtra > 0 || Δp.lostExtra > 0 || Δp.lostAtomic > 0) {
            Δp.addExtra();
        }
        var hdr = new uint64[]{1}.array();
        // Note: write "knows" that the argument is &gp.labels,
        // because otherwise its write barrier behavior may not
        // be correct. See the long comment there before
        // changing the argument here.
        cpuprof.log.write(ᏑtagPtr, nanotime(), hdr[..], stk);
    }
    Ꮡprof.of(profᴛ1.ᏑsignalLock).Store(0);
}

// addNonGo adds the non-Go stack trace to the profile.
// It is called from a non-Go thread, so we cannot use much stack at all,
// nor do anything that needs a g or an m.
// In particular, we can't call cpuprof.log.write.
// Instead, we copy the stack into cpuprof.extra,
// which will be drained the next time a Go thread
// gets the signal handling event.
//
//go:nosplit
//go:nowritebarrierrec
[GoRecv] internal static void addNonGo(this ref cpuProfile Δp, slice<uintptr> stk) {
    // Simple cas-lock to coordinate with SetCPUProfileRate.
    // (Other calls to add or addNonGo should be blocked out
    // by the fact that only one SIGPROF can be handled by the
    // process at a time. If not, this lock will serialize those too.
    // The use of timer_create(2) on Linux to request process-targeted
    // signals may have changed this.)
    while (!Ꮡprof.of(profᴛ1.ᏑsignalLock).CompareAndSwap(0, 1)) {
        // TODO: Is it safe to osyield here? https://go.dev/issue/52672
        osyield();
    }
    if (cpuprof.numExtra + 1 + len(stk) < len(cpuprof.extra)){
        nint i = cpuprof.numExtra;
        cpuprof.extra[i] = (uintptr)(1 + len(stk));
        copy(cpuprof.extra[(int)(i + 1)..], stk);
        cpuprof.numExtra += 1 + len(stk);
    } else {
        cpuprof.lostExtra++;
    }
    Ꮡprof.of(profᴛ1.ᏑsignalLock).Store(0);
}

// addExtra adds the "extra" profiling events,
// queued by addNonGo, to the profile log.
// addExtra is called either from a signal handler on a Go thread
// or from an ordinary goroutine; either way it can use stack
// and has a g. The world may be stopped, though.
[GoRecv] internal static void addExtra(this ref cpuProfile Δp) {
    // Copy accumulated non-Go profile events.
    var hdr = new uint64[]{1}.array();
    for (nint i = 0; i < Δp.numExtra; ) {
        Δp.log.write(nil, 0, hdr[..], Δp.extra[(int)(i + 1)..(int)(i + (nint)Δp.extra[i])]);
        i += (nint)Δp.extra[i];
    }
    Δp.numExtra = 0;
    // Report any lost events.
    if (Δp.lostExtra > 0) {
        var hdrΔ1 = new uint64[]{Δp.lostExtra}.array();
        var lostStk = new uintptr[]{
            abi.FuncPCABIInternal(_LostExternalCode) + (uintptr)sys.PCQuantum,
            abi.FuncPCABIInternal(_ExternalCode) + (uintptr)sys.PCQuantum
        }.array();
        Δp.log.write(nil, 0, hdrΔ1[..], lostStk[..]);
        Δp.lostExtra = 0;
    }
    if (Δp.lostAtomic > 0) {
        var hdrΔ2 = new uint64[]{Δp.lostAtomic}.array();
        var lostStk = new uintptr[]{
            abi.FuncPCABIInternal(_LostSIGPROFDuringAtomic64) + (uintptr)sys.PCQuantum,
            abi.FuncPCABIInternal(_System) + (uintptr)sys.PCQuantum
        }.array();
        Δp.log.write(nil, 0, hdrΔ2[..], lostStk[..]);
        Δp.lostAtomic = 0;
    }
}

// CPUProfile panics.
// It formerly provided raw access to chunks of
// a pprof-format profile generated by the runtime.
// The details of generating that format have changed,
// so this functionality has been removed.
//
// Deprecated: Use the [runtime/pprof] package,
// or the handlers in the [net/http/pprof] package,
// or the [testing] package's -test.cpuprofile flag instead.
public static slice<byte> CPUProfile() {
    throw panic("CPUProfile no longer available");
}

// runtime/pprof.runtime_cyclesPerSecond should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/grafana/pyroscope-go/godeltaprof
//   - github.com/pyroscope-io/godeltaprof
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname pprof_cyclesPerSecond runtime/pprof.runtime_cyclesPerSecond
public static int64 pprof_cyclesPerSecond() {
    return ticksPerSecond();
}

// readProfile, provided to runtime/pprof, returns the next chunk of
// binary CPU profiling stack trace data, blocking until data is available.
// If profiling is turned off and all the profile data accumulated while it was
// on has been returned, readProfile returns eof=true.
// The caller must save the returned data and tags before calling readProfile again.
// The returned data contains a whole number of records, and tags contains
// exactly one entry per record.
//
// runtime_pprof_readProfile should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/pyroscope-io/pyroscope
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname runtime_pprof_readProfile runtime/pprof.readProfile
internal static (slice<uint64>, slice<@unsafe.Pointer>, bool) runtime_pprof_readProfile() {
    @lock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
    var log = cpuprof.log;
    unlock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
    profBufReadMode readMode = profBufBlocking;
    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
        readMode = profBufNonBlocking; // For #61768; on Darwin notes are not async-signal-safe.  See sigNoteSetup in os_darwin.go.
    }
    var (data, tags, eof) = log.read(readMode);
    if (len(data) == 0 && eof) {
        @lock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
        cpuprof.log = default!;
        unlock(Ꮡcpuprof.of(cpuProfile.Ꮡlock));
    }
    return (data, tags, eof);
}

} // end runtime_package
