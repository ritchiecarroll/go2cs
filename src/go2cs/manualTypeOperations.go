// manualTypeOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
	"go/types"
)

// Some Go declarations cannot be faithfully auto-converted because their semantics depend on
// hiding a managed pointer inside an integer (e.g. runtime's guintptr family): the CLR cannot
// hold a managed reference as a number across a GC move, so the managed conversion must store
// the ж<T> box DIRECTLY (model precedent: core/sync/atomic Pointer<T>). Those declarations are
// hand-converted in the package's *_impl.cs (marked [module: GoManualConversion], kept in
// src/core/<pkg>/ and restored over auto output by the overlay). The converter SKIPS emitting:
//   - the type declaration itself (a marker comment is left in its place),
//   - every method declared on the type,
//   - adjacent free functions / methods on other types listed in manualConversionFuncs,
//   - GoImplicitConv assembly attributes referencing the type (the manual file declares any
//     conversion operators its call sites need).
//
// Call-site emission is unchanged except conversions handled in convCallExpr: a manual-type
// conversion from unsafe.Pointer(x) emits the referent-preserving ctor form `new T(x)` instead
// of the numeric cast chain `(T)(uintptr)new Pointer(x)` (which would lose the referent).
//
// Keys are RAW Go identifiers (rename analyses apply downstream at emission).
var manualConversionTypes = map[string]map[string]bool{
	"runtime": {
		"guintptr": true,
		"puintptr": true,
		"muintptr": true,
	},
}

// goosScope names the target operating systems a manual-conversion entry applies to. The EMPTY
// scope (goosAny) means every target, which is what all but a handful of entries want.
//
// The scope exists because the registry is keyed by NAME, and a Go name is not unique across
// platforms: Go selects one of several files declaring the same function by build constraint, and
// those declarations are not interchangeable. Name-keyed alone, one entry silently spoke for all of
// them — turning every flavor's declaration into a placeholder while an implementation existed for
// only the one somebody hand-wrote. That is load-bearing in both directions:
//
//   - A declaration only Go's Windows sources carry (syscall's generated wrappers,
//     os.readReparseLink) is inert on other targets today, so scoping it moves no emission — but it
//     records WHY, instead of leaving the next reader to re-derive it from Go's file set.
//   - A declaration EVERY target carries, where only some flavors need hand-owning, cannot be
//     expressed at all without a scope. os.(*File).readdir is that case: the windows and darwin
//     flavors hand OS memory to a Go struct and must be hand-owned, while dir_unix.go's is pure Go
//     over internal/poll and converts faithfully — yet the unscoped entry deleted its body too, and
//     left every Linux os build with a placeholder and nothing to link against.
//
// What a scope deliberately does NOT express is a per-platform SIGNATURE. runtime's
// notetsleep_internal is four parameters in lock_sema.go and two in lock_futex.go; the answer is one
// goosAny entry — both flavors need hand-owning — with each flavor's own *_impl.cs declaring its own
// signature, which layout L3 already routes per GOOS. The registry decides WHETHER a declaration is
// hand-owned; the hand-owned file decides what it looks like.
type goosScope []string

// goosAny scopes an entry to every target — the common case, and the zero value, so an entry that
// says nothing keeps the registry's original name-keyed behavior.
var goosAny goosScope

// goosWindows scopes an entry to Windows alone.
var goosWindows = goosScope{"windows"}

// goosLinux scopes an entry to the Linux flavor alone. Its first members are syscall's Fstat and
// fstatat: darwin declares BOTH names too (syscall_darwin.go, via libc) with a body that is not the
// defective one, so an unscoped entry would turn darwin's working wrappers into placeholders with
// nothing to link against — the exact os.(*File).readdir lesson, one package down.
var goosLinux = goosScope{"linux"}

// goosDarwin scopes an entry to the darwin flavor alone. Its first member is runtime's nanotime1:
// linux and windows declare it BODYLESS (stubs3.cs) and are displaced simply by writing a body, so
// they need no registry entry at all, while darwin's sys_darwin.cs carries a real converted body over
// its own libc trampoline — a bodied function is displaced only through this registry. Scoping it
// darwin-only is therefore not a preference but the shape of the difference: an unscoped entry would
// turn the other two flavors' bodyless partials into placeholders that their own *_impl.cs already
// implements, which is CS0111 rather than a displacement.
var goosDarwin = goosScope{"darwin"}

// goosWindowsLinux scopes an entry to the two flavors that each hand-own the SAME declaration in
// their own per-GOOS file — the sockaddr family: Windows in syscall/windows/syscall_windows_impl.cs
// (L10), Linux in syscall/linux/sockaddr_linux_impl.cs (the 2026-08-22 mirror). darwin declares the
// same names and keeps its auto bodies until a darwin lane measures them (the lock_sema/lock_futex
// precedent: one entry, each flavor's file the authority on its own body).
var goosWindowsLinux = goosScope{"windows", "linux"}

// goosWindowsDarwin scopes an entry to the two targets whose Go flavor reinterprets or hands OS
// memory to a Go struct — the raw-metal-on-non-native-types fork — where the third does not.
var goosWindowsDarwin = goosScope{"windows", "darwin"}

// goosLinuxDarwin scopes an entry to the two unix flavors that each hand-own the SAME declaration in
// their own per-GOOS file -- the free-function half of the sockaddr family (anyToSockaddr, Recvfrom,
// Sendto, recvmsgRaw, SendmsgN): Linux in syscall/linux/sockaddr_linux_impl.cs, darwin in
// syscall/darwin/sockaddr_darwin_impl.cs (increment 8, 2026-09-05). Windows declares none of them
// under these names (its decode is RawSockaddrAny.Sockaddr, its senders WSASendto), so the scope is
// the two that do.
var goosLinuxDarwin = goosScope{"linux", "darwin"}

// includes reports whether the scope covers the named target operating system.
func (scope goosScope) includes(goos string) bool {
	if len(scope) == 0 {
		return true
	}

	for _, name := range scope {
		if name == goos {
			return true
		}
	}

	return false
}

// Free functions ("funcName") and methods on other types ("recvTypeName.funcName") owned by the
// same manual files — declarations whose bodies are inseparable from the manual types' semantics.
var manualConversionFuncs = map[string]map[string]goosScope{
	"crypto/internal/alias": {
		// AnyOverlap orders element ADDRESSES — four `(uintptr)Ꮡ(…)` takes, each pinning its backing only
		// until the box that took it is finalized, so a collection landing between two takes relocates one
		// operand and the ordering compares two heap layouts. Measured 2026-09-03 (Release, tiering off):
		// the converted predicate answered TRUE for two distinct fresh arrays 9 s into a 16-thread stress,
		// and the converted GCM Open raised `crypto/aes: invalid buffer overlap` 27 s in — the panic that
		// killed the banked net/http row on two host classes. Displaced onto golib slice<T>.Overlaps
		// (canonical backing identity + absolute index range: the managed-referent arm of the S1/CS0030
		// fork). InexactOverlap stays auto — its `Ꮡ(x, 0) == Ꮡ(y, 0)` early-out is already structural.
		// Registered here rather than marked: the package has exactly one non-test Go file, and a whole-file
		// marker would hand-own it BY CONSEQUENCE (the internal/godebug class) and freeze its csproj,
		// package_info and README. crypto/internal/alias/alias_impl.cs holds the body.
		"AnyOverlap": goosAny,
	},
	"vendor/golang.org/x/crypto/internal/alias": {
		// The vendored purego twin of crypto/internal/alias.AnyOverlap: the same four-take address ordering,
		// one reflect call deeper (`reflect.ValueOf(Ꮡ(x, 0)).Pointer()`), reached by chacha20 and
		// chacha20poly1305 through InexactOverlap — the CHACHA20 cipher suites crypto/tls negotiates. Same
		// structural body on golib slice<T>.Overlaps, same reason, same registration shape (one non-test Go
		// file, so a whole-file marker would hand-own the package by consequence). InexactOverlap stays auto.
		// vendor/golang.org/x/crypto/internal/alias/alias_purego_impl.cs holds the body.
		"AnyOverlap": goosAny,
	},
	"vendor/golang.org/x/net/route": {
		// route.init probes the byte order through `(*[4]byte)(unsafe.Pointer(&i))` on a uint32 local —
		// the raw-metal fork golib names at array<T>.AliasPointer (an array<T> cannot be fabricated from a
		// scalar's bytes), measured by the first darwin behavioral census as an index panic inside the
		// module initializer of every net importer (net imports x/net/route on BSDs only). The companion
		// vendor/golang.org/x/net/route/darwin/sys_impl.cs answers the question with
		// BitConverter.IsLittleEndian and keeps the rest of init verbatim. Darwin-scoped: sys.go is BSD-only,
		// so the converted init never existed in the linux or windows emission. A displaced init emits only
		// the placeholder, so the companion carries the [GoInit] module initializer itself.
		"init": goosDarwin,
	},
	"slices": {
		// overlaps has AnyOverlap's four-take shape and its race; its callers Insert/Replace take the
		// hard-case rotation on TRUE, where startIdx panics `needle not found` for a source that does not
		// alias — the same death one panic text over. Same structural body, same reason; slices/slices_impl.cs.
		"overlaps": goosAny,
	},
	"runtime": {
		// persistentalloc1 / inPersistentAlloc (increment 6 of the runtime row, 2026-09-05): Go keeps
		// persistentChunks as a lock-free push through `atomic.Casuintptr((*uintptr)(unsafe.Pointer(&persistentChunks)), …)`
		// and reads it with Loaduintptr over the same view -- a POINTER-typed global reinterpreted as the
		// integer it holds. golib has no arm for a managed reference slot viewed as uintptr (nothing aliases
		// a GC reference as an integer), so the address route mints a nil box and the CAS dereferences it:
		// the nil dereference every persistentalloc row died on once sysMmap answered (the increment-5
		// sizing probe). Displaced onto runtime/mem_persistent_impl.cs, which keeps the emission line for
		// line and turns only those two lines into an Interlocked exchange of boxes on the slot (compared
		// by native address) and a volatile read. The S1/CS0030 fork's native-pointer side: the chunks are
		// mmap'd memory outside the CLR heap. Corpus census 2026-09-05: 21 emitted sites in 12 runtime
		// files (7 Go sites) carry this view; these two are the only ones any row reaches -- the general
		// pointer-slot view is recorded for a second reaching site, not built here.
		"persistentalloc1":  goosAny,
		"inPersistentAlloc": goosAny,
		// addrRanges.init / add / cloneInto (increment 7 of the runtime row, W2a, 2026-09-05): the three
		// writers that build a notInHeapSlice header FIELD BY FIELD over the managed a.ranges --
		// `ranges := (*notInHeapSlice)(unsafe.Pointer(&a.ranges)); ranges.len = …; ranges.cap = …;
		// ranges.array = (*notInHeap)(persistentalloc(…))`. golib's SliceHeaderBox materializes the
		// header from the live slice, but a header handed out as a C# `ref` cannot observe its LAST
		// store (nothing accesses the box after `array` is assigned, and between the `cap` and `array`
		// stores the header is a nil array with a non-zero capacity, which no earlier commit could
		// re-base), so the slice is re-based AT the write instead: runtime/mranges_impl.cs keeps each
		// body line for line and turns the three header stores into one native-backed slice over the
		// persistentalloc'd block (golib's single creation door, with Go's cap as the third word). The
		// whole-header writers of the same view (traceMap, mheap.allspans, allArenas) sit behind
		// sysAlloc/mheap.init and are not displaced.
		"addrRanges.init":      goosAny,
		"addrRanges.add":       goosAny,
		"addrRanges.cloneInto": goosAny,
		// runtime.nanotime1 — darwin's monotonic clock. The other two flavors reach the same golib
		// clock (MonotonicClock.Nanoseconds()) by writing a body into a bodyless partial and need no
		// entry here; darwin's converted body calls libcCall(FuncPCABI0(nanotime_trampoline), …) into a
		// throwing external stub, and a BODIED function is displaced only through this registry. The
		// throw is not a dormant edge: nanotime is read by cpuprof, metrics, mgc, mgcmark, mgcpacer,
		// mprof, netpoll and debuglog, so the first call into any of them dies. See
		// docs/phase4/DESIGN-darwin-run-layer-1.md.
		"nanotime1": goosDarwin,
		// The darwin signal-note primitives (increment 4 of the darwin run layer, 2026-09-03): the
		// converted pipe hands the keystone the address of a managed [2]int32 (refused by name --
		// `field m_array of array`1 is a Int32[]`), and the converted read/write1 hand it the address
		// of `fd` ALONE (Go's ABI0 frame trick: the three args are contiguous stack slots), so the
		// dispatcher's pointee walk places ONE register and libc receives junk for the buffer and
		// the length -- which also mutes every darwin runtime throw, since writeErr writes through
		// write1. runtime/darwin/sys_darwin_signote_impl.cs realizes the three over DllImport("libc")
		// with a native fd pair for pipe and the pinned buffer address for read/write; sized so the
		// keystone's per-symbol layout record can replace them later with the same placeholders.
		"pipe":   goosDarwin,
		"read":   goosDarwin,
		"write1": goosDarwin,
		// runtime.sigprocmask -- the darwin signal-mask primitive (increment 5, 2026-09-04). The
		// train-24 behavioral-stderr stage placed SignalPrimitives on BOTH mac legs at the same
		// statement, signal.Notify, by two independent derivations: x64's stack (FuncPC <-
		// FuncPCABI0 <- runtime.sigprocmask <- runtime.ensureSigM.func1) and arm64's stdout count
		// (2 of 6 lines, zero stderr). The death is in COMPUTING the trampoline address, not in
		// dispatching through it -- sigprocmask_trampoline is a bodyless partial the generator
		// fills with a throw, so abi.FuncPCABI0 has no PC to answer with. Bodied (it calls
		// libcCall), so displaced here rather than by writing into a bodyless partial the way
		// C1's linux rtsigprocmask could be. runtime/darwin/sigprocmask_impl.cs realizes it over
		// libc's PTHREAD_sigmask -- what Go's own trampoline calls on both darwin arches -- with a
		// 32-bit set (darwin's sigset is uint32, not Linux's [2]uint32), darwin's own how numbering
		// (1/2/3, not 0/1/2) and the errno read from the RETURN value, since pthread_sigmask does
		// not set errno. Four clauses, four ways a borrowed linux body would have been wrong.
		"sigprocmask": goosDarwin,
		// runtime.sigaction -- the darwin signal INSTALL/QUERY primitive (increment 6, 2026-09-05, Q41).
		// The train-26 crash report on osx-arm64 placed the SignalPrimitives death inside the CLR's
		// own stack walk, reading a Frame link of 0x0000004200000000 = {sa_mask 0, sa_flags
		// SA_SIGINFO|SA_RESTART}: a sigaction(2) read-back. Read from the code rather than guessed:
		// the converted sigaction dispatches libcCall(fn, FromPinnedBox(Ꮡsig)) -- the box of its
		// FIRST parameter only, Go's cgo_unsafe_args `&sig` being the head of a contiguous
		// (sig, new, old) block that exists only on a Go stack -- so GoLibcCall places ONE register
		// and libc reads `new` from and writes `old` through whatever the second and third registers
		// held. Bodied (it calls libcCall), so displaced here, increment 5's door.
		// runtime/darwin/sigaction_impl.cs realizes it over libc's sigaction(2) with `new` ENCODED
		// into a native 16-byte struct sigaction and `old` DECODED back into the managed usigactiont,
		// whose [8]byte union converts to an array<byte> and so can never be passed by address (the
		// struct-passing class; the mirror arc's writeNativeSockaddr shape).
		"sigaction": goosDarwin,
		// runtime.libcCall — darwin's dispatch bottom: every libc trampoline in sys_darwin.cs is reached
		// through libcCall(FuncPCABI0(x_trampoline), &args). Its converted body opens with getg() and
		// ends in asmcgocall, four bodyless intrinsics with no implementing part anywhere (the
		// generator stubs all four), so a real function pointer handed to it dies one line in. The
		// hand-own (runtime/darwin/libccall_impl.cs) drops the g0 switch and the libcall* profiler
		// bookkeeping the managed host has no counterpart for and dispatches the call itself. Bodied,
		// so displaced here. See docs/phase4/DESIGN-darwin-run-layer-2.md §2.2 and §7.
		"libcCall": goosDarwin,
		// getgcmask answers a TYPE's GC pointer bitmap, and Go answers it by reading the GC's own
		// heap metadata: findObject, the span's typePointersOfUnchecked iterator, activeModules'
		// data/bss bitmaps, the stack's locals map. None of those exist in a managed runtime, so the
		// converted body cannot work here and does not fail cheaply -- reflect's TestGCBits reports
		// infrastructure-error rather than a failure, because the process does not survive the walk.
		//
		// It is hand-owned rather than DISCLOSED because the datum is not lost: the bitmap is a
		// TYPE-level property, and golib's layout walk already enumerates exactly the words that
		// define it -- the same walk PtrBytes comes from, where PtrBytes reports where the last
		// pointer word ENDS and the mask reports WHICH words they are. GoReflect.GoGCMaskOf answers
		// from that walk, so the hand-own reports the same truth at finer resolution rather than
		// substituting a plausible one. runtime/mbitmap_impl.cs holds the body.
		"getgcmask":  goosAny,
		"g.guintptr": goosAny,
		"setGNoWB":   goosAny,
		"setMNoWB":   goosAny,
		// The mutex/note key-slot protocol. Go has TWO flavors of it and selects one per GOOS:
		// lock_sema.go (windows, darwin, plan9, aix …) smuggles an *m address through the uintptr
		// slot and parks waiters on OS semaphores; lock_futex.go (linux, freebsd, dragonfly) uses a
		// {0,1,2} slot and parks on a futex. Neither OS primitive has a managed realization, so BOTH
		// flavors need hand-owning and both converge on the same managed model — a {0, keyLocked}
		// latch with SpinWait escalation — which is why the scope is goosAny and why the managed
		// core is ONE flat file (runtime/lock_managed_impl.cs) rather than a copy per flavor. Thin
		// wrappers (lock/unlock/noteclear/notetsleep) and the consts stay auto on both.
		//
		// The flavors DO differ in one signature: notetsleep_internal is (n, ns, gp, deadline) in
		// lock_sema.go and (n, ns) in lock_futex.go. A name-keyed registry cannot express that and
		// does not try — each flavor's own *_impl.cs declares its own, delegating to the shared core
		// (windows/darwin/lock_sema_impl.cs, linux/lock_futex_impl.cs), and layout L3 routes each to
		// exactly the platforms its principal is built on.
		//
		// notetsleepg is the one thin wrapper that could NOT stay auto. Its Go body opens with getg()
		// — still an unimplemented intrinsic — then semacreate/entersyscallblock over the same dead
		// g/m graph, so every caller threw before reaching the note at all. Its two callers are
		// exactly the ones that must WAIT rather than poll: sigqueue's signal_recv (idle until a
		// signal arrives, possibly for the life of the process) and profbuf's reader. Both run on a
		// goroutine, which golib gives a dedicated thread, so the managed body is a real blocking
		// wait — the only member of this family that blocks rather than spins. Its g0 sibling
		// notetsleep shares the getg() prologue but has no reachable caller, so it stays auto and
		// stays throwing rather than being hand-owned speculatively.
		"mutexContended":      goosAny,
		"lock2":               goosAny,
		"unlock2":             goosAny,
		"notewakeup":          goosAny,
		"notesleep":           goosAny,
		"notetsleep_internal": goosAny,
		"notetsleepg":         goosAny,
		// The os/signal OS-handler-INSTALL layer (linux/signal_posix_impl.cs). sigenable/sigdisable/
		// sigignore are the three functions signal_enable/signal_disable/signal_ignore (sigqueue.go,
		// which stay auto) call to reach the kernel: the converted bodies install Go's own sigtramp
		// via setsig → sysSigaction → rt_sigaction, and sigenable/sigdisable additionally hand off to
		// ensureSigM's goroutine over sigprocmask. Both syscalls are unimplemented external stubs on
		// the CLR (which owns Linux signal handling), so every signal.Notify/Ignore threw. The
		// hand-own routes the install through .NET PosixSignalRegistration whose handler feeds the
		// EXISTING sigqueue (sigsend → signal_recv → the os/signal channel), keeping every line of the
		// wanted/ignored bookkeeping and the delivery path auto; only the kernel-install layer is
		// displaced, and ensureSigM's sigprocmask handshake is elided with it. Scoped goosLinux — the
		// linux flavor of signal_unix.go — since PosixSignalRegistration is the linux target's bridge;
		// darwin's copy stays auto until its own arc. Signals outside .NET's PosixSignal enum
		// (SIGUSR*, SIGPIPE, the real-time signals) have no registration and stay the honest
		// rt_sigaction residual. See docs/phase4/DESIGN-signal-posix-bridge.md.
		"sigenable":  goosLinux,
		"sigdisable": goosLinux,
		"sigignore":  goosLinux,
		// runtime.StartTrace (trace.go, build-tag-free — selected on every platform): the execution
		// tracer is a serialization of the scheduler the managed host does not have — the converted
		// body's first step is semacquire → getg, an unimplemented g-model intrinsic, so every
		// trace.Start THREW as an infrastructure error. Go's StartTrace returns an error by
		// signature, and a capability the host cannot provide is honestly an ERROR, not a crash (the
		// AllThreadsSyscall→ENOTSUP pattern): the hand-own (<goos>/trace_impl.cs, identical content
		// on each flavor since the error text is not platform-specific) returns a named
		// tracing-not-supported error, so runtime/trace.Start propagates it and a test asserting
		// trace output fails cleanly with a disclosable signature (runtime's own TestCrashWhileTracing
		// and os/signal's TestSignalTrace are the measured consumers). Scoped windowsLinux — extended
		// from linux-only 2026-09-02 once TestCrashWhileTracing showed windows hitting the identical
		// gap; darwin's copy stays auto until its own arc (the lock_sema_impl.cs precedent: one
		// hand-own, routed by L3 into a copy per targeted platform).
		"StartTrace": goosWindowsLinux,
		// runtime.StopTrace, StartTrace's companion and registered for the same reason one level
		// down. StartTrace's own hand-own comment claimed "StopTrace and the rest of the tracer stay
		// auto: they are unreachable while StartTrace refuses" — MEASURED FALSE 2026-09-02, and the
		// witness is runtime/trace's own suite: TestTraceDoubleStart's FIRST statement (trace_test.go
		// line 39) is a bare Stop(), before any Start, so runtime/trace.Stop → runtime.StopTrace →
		// traceAdvance → semacquire → semacquire1 → getg() threw and the row read
		// infrastructure-error where its sibling read a clean fail. A no-op is the FAITHFUL body, not
		// a convenience: Go documents StopTrace as stopping tracing "if it was previously enabled",
		// tracing is never enabled on this host because StartTrace always refuses, and the signature
		// has no error channel to report anything through — so doing nothing is precisely what Go's
		// own contract prescribes here. The auto body's traceAdvance(true) would additionally
		// early-return at its own gen==0 check even if semacquire worked, so nothing observable is
		// being skipped. ReadTrace and the rest of the tracer genuinely do stay auto: they are
		// reached only from the goroutine trace.Start spawns AFTER it succeeds, which it never does.
		"StopTrace": goosWindowsLinux,
		// The PROCESS-CONTROL surface (managed_impl.cs). Each of these is a public runtime API
		// whose converted body drives Go's own scheduler / GC pacer — stopTheWorld, gcStart,
		// mcall(gosched_m), the g/m/p stack walk — machinery that has no managed counterpart and
		// dies on the first getg()/mcall() assembly stub. The CLR does, however, answer every one
		// of these API CONTRACTS natively, so they are reimplemented at the API boundary the same
		// way sync's Mutex/notifyList were: honor the observable contract, never emulate the
		// mechanism. Everything BELOW them (the scheduler, the pacer, the mark/sweep engine) stays
		// auto-converted and simply becomes unreachable.
		"GC":         goosAny,
		"GOMAXPROCS": goosAny,
		"Gosched":    goosAny,
		// Goexit belongs to the same surface for the same reason: its converted body drives Go's
		// own _panic record and stack unwinder (p.start(getcallerpc(), getcallersp()) → nextDefer →
		// goexit1), all of it assembly. The managed shape unwinds the calling goroutine with a
		// golib GoexitException, which the defer machinery and the goroutine root already handle —
		// see managed_impl.cs and docs/phase4/DESIGN-goexit.md.
		"Goexit":         goosAny,
		"Stack":          goosAny,
		"ReadMemStats":   goosAny,
		"LockOSThread":   goosAny,
		"UnlockOSThread": goosAny,
		// The lower-case pair is the runtime-internal variant of the same contract (syscall and
		// mime's registry reader reach it through startTemplateThread); it takes the same body.
		"lockOSThread":   goosAny,
		"unlockOSThread": goosAny,
		// Pinner (Q45, docs/phase4/DESIGN-runtime-pinner.md). Address stability is the address-take's
		// own contract for a managed ж<T> box (EnsureStableAddress at the pin moment, held for the
		// box's life), so Pin takes no CLR pin — but the two OBSERVABLES Go's suite measures are real
		// state: the pin BIT (isPinned, read by the cgo argument check) and the lifetime HOLD until
		// Unpin. runtime/pinner_impl.cs keeps a pin COUNT keyed by the pointer's ReferentObject (Go's
		// per-object span index, one level down: pinning &sl[0] pins the whole backing) and holds the
		// referents until Unpin (Go's refs). The auto bodies walk the scheduler (acquirem → getg) and
		// the span table (setPinned → findObject → mheap_.arenas, never allocated here): isPinned
		// nil-derefs in spanOf on its first read, pinnerGetPinCounter walks the same table, and
		// cgoCheckPointer returns at `debug.cgocheck == 0` because parsedebugvars sits on the schedinit
		// path the managed host never runs. All three are BODIED, so the registry displaces them;
		// setPinned, unpin, pinnerGetPtr, cgoCheckArg and cgoCheckUnknownPointer stay converted and
		// dead behind them. Consumers: runtime's own pinner_test.go (21 rows) and internal/fmtsort's
		// test init, whose emitted `(uintptr)` argument Pin tolerates (Q49's bridge class).
		"Pinner.Pin":          goosAny,
		"Pinner.Unpin":        goosAny,
		"isPinned":            goosAny,
		"pinnerGetPinCounter": goosAny,
		"cgoCheckPointer":     goosAny,
		// The traceback surface (managed_impl.cs). Callers' auto body enters the raw-metal
		// unwinder on its first step (callers → getcallersp, an assembly stub), and Frames.Next
		// reads linker funcInfo tables (findfunc) that have no managed form. Both API contracts —
		// "record the calling goroutine's frames as opaque PCs" / "expand PCs to function/file/
		// line" — the CLR answers natively via System.Diagnostics.StackTrace, projected to
		// GO-LOGICAL frames (converted-source frames only; go2cs-gen shells and forwarders are
		// invisible, exactly as Go's interface dispatch adds no frame). getcallersp itself stays
		// an honest stub — a caller's stack pointer has no managed answer; the chain is severed
		// here, at the semantic boundary that does (the reflection bridge's methodName pattern).
		// io's TestMultiReaderFlatten / TestMultiWriterSingleChainFlatten (relative stack-depth
		// asserts over runtime.Callers) are the demonstrated consumers.
		"Callers":     goosAny,
		"Frames.Next": goosAny,

		// FuncForPC/Func.Name join the traceback family for the same reason the rest of it is
		// here: both bodies reach pclntab, which does not exist. managed_impl.cs's header once
		// ruled a *Func unrecoverable, and that premise EXPIRED when ManagedPointerTokens landed
		// -- a reflect Value.Pointer() token resolves to the delegate behind it, and a Callers PC
		// token already carries its Go-spelled name. Name() must come WITH FuncForPC: its own body
		// Reinterprets Func to _func -- the prefix-downcast the managed model cannot alias -- and
		// then walks the symbol table, so a *Func whose Name() stayed auto is a handle to nothing.
		"FuncForPC": goosAny,
		"Func.Name": goosAny,
		// Entry/FileLine complete the *Func the line above started: FuncForPC/Name answered "what is
		// this function called", but a *Func minted by FuncForPC carries no real _func/moduledata --
		// calling Entry() or FileLine() on it still fell through to funcInfo()'s firstmoduledata walk,
		// which is a permanent empty stub (symtab.cs's Ꮡfirstmoduledata, assigned exactly once, to a
		// moduledata with an always-empty pclntable) and can never resolve. That is TestCaller's crash
		// (runtime_test), not a goroutine race: any resolved Func reaching Entry() dies here,
		// unconditionally, on any goroutine. The managed side table (managed_impl.cs) widens to carry
		// the originating PC beside the name; Entry() returns that token directly -- consistent with
		// the header's "PC values are opaque process-lifetime tokens" doctrine, since a token IS this
		// host's answer to "what identifies this function" -- and FileLine(pc) resolves through the
		// SAME callerFrameRecord Go-position data Callers()/Frames.Next() already use, answering Go's
		// own no-position case ("", 0) when no record exists. firstmoduledata and Frame.Func are
		// deliberately untouched by this arc; see docs/phase4/CENSUS-runtime-semantic-bill.md.
		"Func.Entry":    goosAny,
		"Func.FileLine": goosAny,
		// The metrics-table mutex (managed_impl.cs). Go's bodies acquire metricsSema, a runtime
		// sleeping semaphore whose acquire path is getg() → sudog → gopark — the scheduler
		// machinery that has no managed counterpart — so every path into the metrics table
		// (readMetrics behind runtime/metrics.Read, readMetricNames behind the metrics_test push)
		// died on the getg stub. The CONTRACT is mutual exclusion with waiter handoff over the
		// metrics map and agg scratch state; SemaphoreSlim(1, 1) is the CLR's spelling of exactly
		// that. Everything the lock protects (initMetrics' map build, readMetricsLocked's compute
		// closures) stays auto-converted.
		"metricsLock":   goosAny,
		"metricsUnlock": goosAny,
		// NumCgoCall (managed_impl.cs): its body walks the scheduler's `allm` thread list summing
		// per-m cgo-call counters — a list the managed model never populates (the walk nil-derefs
		// where Go always has at least m0). The CONTRACT is "number of cgo calls made by the
		// current process", and the managed model makes no cgo calls at all, so zero is the true
		// count rather than an approximation. Reached by the /cgo/go-to-c-calls:calls metric's
		// compute closure for every metrics.Read.
		"NumCgoCall": goosAny,
		// NumGoroutine (managed_impl.cs): its body is gcount(), which derives the live count by
		// SUBTRACTION over scheduler state the managed model never populates — allglen, minus
		// sched.gFree.n, minus sched.ngsys, minus each P's gFree.n. Every term is zero here, and
		// gcount's `if n < 1 { n = 1 }` floor then turns the nonsense into a plausible-looking
		// constant: runtime.NumGoroutine() answered 1 for every program, forever, which is exactly
		// the shape of wrong that survives unnoticed (a single-goroutine program's answer IS 1).
		// The managed model has the true count and always did — golib's Goroutine registry
		// maintains it as the live set changes — so this is a WIRING, not an approximation, and
		// the one honest divergence is Go's: the count is momentarily stale under concurrent
		// creation, which Go's own comment on gcount concedes in the same words.
		"NumGoroutine": goosAny,
		// gcount (managed_impl.cs): NumGoroutine's own body, and the reason hand-owning NumGoroutine
		// could only fix ONE caller. The same subtraction over never-populated scheduler state serves
		// THREE more consumers that reach gcount DIRECTLY rather than through NumGoroutine, so each
		// still receives the clamped constant 1: metrics.cs:424's /sched/goroutines compute closure,
		// and mprof.cs's goroutine-profile SIZE (1384) and COUNT (1397). Wiring gcount to the same
		// registry NumGoroutine already reads fixes all three at one seam rather than three times
		// over, and removes the oddity of a public API disagreeing with its own implementation.
		//
		// Declared per-GOOS (windows/linux/darwin proc.go) but identical in all three, and identical
		// in the managed model too — so this is the lock_sema shape: goosAny scope, ONE flat body in
		// managed_impl.cs, all three declarations taking the placeholder.
		//
		// The divergence is MEASURED, and recorded in that file's Honest-divergences ledger rather
		// than waved at: golib's count reads early-by-one while goroutines are starting and decays
		// LATE — 9 where Go reports 1, sampled immediately after a WaitGroup releases. Go's own
		// staleness caveat covers the first half; the decay lag is ours, and it is a named board item
		// with a defined trigger, not a silent approximation.
		"gcount": goosAny,
		// totalMutexWaitTimeNanos (managed_impl.cs): the same `allm` walk as NumCgoCall, summing
		// per-m lock-profile wait times that never exist here. The managed body keeps the two REAL
		// counter loads (sched.totalMutexWaitTime, sched.totalRuntimeLockWaitTime) and drops only
		// the walk. Reached by the /sync/mutex/wait/total:seconds metric's compute closure.
		"totalMutexWaitTimeNanos": goosAny,
		// The consistent heap-stats snapshot read (managed_impl.cs), the one call below the metrics
		// computes that still walked the scheduler: its body disables preemption (acquirem → getg)
		// to hold `allp` stable while merging every P's heap-stats delta. The managed model has no
		// Ps and nothing ever writes a heapStatsDelta (the CLR allocator does not populate Go's
		// allocator bookkeeping), so the faithful snapshot is the zero delta — the same class of
		// honest zero ReadMemStats' hand-own documents for the identical fields. Reached from
		// heapStatsAggregate.compute for every heap-dependent metric.
		"consistentHeapStats.read": goosAny,
		// The lower-case `callers` is the FUNNEL every other traceback entry point goes through
		// (Caller, mprof's profile recorders, proc's createstack, tracestack) and the one that
		// actually reaches getcallersp — so severing it here, one level below Callers, is what
		// makes runtime.Caller work rather than hand-owning Caller itself. Caller stays
		// auto-converted and Go-shaped; its `callers(skip+1, rpc)` now lands on the managed walk.
		// Go's own comment on the declaration ("almost identical to Callers", linkname'd by the
		// ecosystem, do not change the signature) says the same thing: it is an API boundary with
		// a managed answer. log's Output → Caller(calldepth) and testing/slogtest's withSource →
		// Caller(1) are the demonstrated consumers.
		"callers": goosAny,
		// netpollGenericInit (netpoll_impl.cs) — the RUNTIME poller's one-time start-up, and a
		// MODULE-INIT killer rather than a test failure, which is why it is here at all.
		//
		// The corpus already ruled where the network poller lives: internal/poll's ten //go:linkname
		// entry points are hand-owned on a managed poller (internal/poll/windows/runtime_netpoll_impl.cs,
		// DESIGN-netpoll-managed-poller.md §9, ruled 2026-08-13), and that design states in terms that
		// runtime/netpoll.cs and runtime/<goos>/netpoll_*.cs "stay converted and DEAD; zero runtime
		// edits". That scope covered internal/poll's consumers. It did not cover runtime's OWN test
		// suite, which reaches this function DIRECTLY: export_test.go re-exports it as
		// runtime.NetpollGenericInit, and netpoll_os_test.go calls it from a package-level init() — so
		// the converted test host threw "getg: external (assembly or cgo) function is not implemented"
		// in its static constructor, before a single test executed, and every verdict came back empty.
		// This entry is that ruling's one narrow amendment, and its whole extent: ONE function, so the
		// design's "zero runtime edits" still holds for everything the design was actually about.
		//
		// WHY A NO-OP IS THE TRUE BODY, not a silencer. netpollGenericInit's entire job in Go is to
		// create the OS poller object — CreateIoCompletionPort on Windows, epoll_create1+eventfd on
		// Linux, kqueue on Darwin — that the SCHEDULER then drains from findRunnable and sysmon. Under
		// go2cs a goroutine is a managed thread the CLR schedules, so nothing would ever drain it: a
		// perfectly-wired conversion would allocate a completion port and abandon it. Every reader of
		// the state it would publish is in that same unreachable scheduler — measured at the windows
		// target, `netpollinited()` has SIX call sites and all six are proc.cs (findRunnable, sysmon,
		// injectglist, stopm, checkdead), and the one read of netpollInited outside netpoll.cs itself
		// is time.cs's runtime timer heap, which the managed model also does not run.
		//
		// And the honest behaviour is delivered by GO'S OWN GUARD rather than by anything invented
		// here: all three flavors open netpoll(delay) with "the poller object was never created →
		// return an empty gList" (iocphandle == _INVALID_HANDLE_VALUE / epfd == -1 / kq == -1). Not
		// creating it therefore leaves each flavor's poll in its own already-correct
		// nothing-to-report branch, which is the true statement under go2cs: no goroutine is ever
		// parked on the runtime's poller, so no goroutine ever becomes runnable through it.
		//
		// netpollInited is deliberately LEFT AT ZERO. Setting it would assert "the runtime poller is
		// up" while iocphandle stays invalid — an incoherent pair a later reader of that dead code
		// would have to untangle — and the truthful answer is that the runtime's poller is not up and
		// never will be. Polling is available; it is just not here.
		//
		// NOT hand-owned, on purpose: netpollBreak. Its auto body reaches stdcall4 → getg and keeps
		// throwing, so proc_test.go's TestNetpollBreak fails as ONE loud, locatable row instead of
		// going quietly green — which is stubs_impl.cs's standing rule for an unported path, and the
		// right outcome for a test whose premise (a poller wait that a break interrupts) the managed
		// model does not have. A no-op there would buy a green that means nothing.
		"netpollGenericInit": goosAny,
	},
	// internal/abi.TypeOf reads an interface's type-word via unsafe.Pointer to reach a Go runtime
	// type descriptor that has no managed form (the reflection bridge — Phase 4). type_impl.cs
	// synthesizes an abi.Type whose Kind_ is classified from the value's managed System.Type. See
	// docs/phase4/DESIGN-reflection-bridge.md.
	// Type.StructType / Type.ArrayType are Go's PREFIX-DOWNCAST idiom —
	// `(*structType)(unsafe.Pointer(t))` reaches a sub-record the linker really allocated behind
	// the Type header. Nothing sits behind a ж<abi.Type>, and golib's Reinterpret rightly refuses
	// to alias managed storage for a reference-bearing pair, so the auto forms read the
	// specialization's fields out of the memory that follows the value slot: `Fields` came back as
	// a fabricated StructField[] of length 8830452760576 — an IndexOutOfRangeException on the
	// FIRST iteration of unique.buildStructCloneSeq, and internal/reflectlite's NumField/Len read
	// the same garbage. type_impl.cs synthesizes both specializations from the descriptor's
	// carried System.Type over the same golib layout machinery that stamps Size_/Align_.
	// Type.Elem / Type.Key are the SAME idiom one level in — Elem downcasts the header to the
	// slice/array/chan/map/ptr record that sits behind it and reads its Elem field, Key does it for
	// a map — so both inherited the defect and answered nil for every non-scalar descriptor. Nil is
	// not a state Go's callers test here: reflect's haveIdenticalType recurses straight into
	// nameFor(t), which nil-dereferences, so every ConvertibleTo/AssignableTo over a slice, map,
	// pointer, chan or array died (database/sql's TestConversions and TestUserDefinedBytes are the
	// measured pair). type_impl.cs synthesizes both from the carried System.Type over the SAME
	// golib element/key resolution reflect's own rtype.Elem/rtype.Key use one layer up.
	"internal/abi": {
		"TypeOf":          goosAny,
		"Type.StructType": goosAny,
		"Type.ArrayType":  goosAny,
		"Type.Elem":       goosAny,
		"Type.Key":        goosAny,
		// Type.FuncType is the THIRD prefix downcast of that same family, and it failed identically:
		// the auto body's tag check is right (`Kind() != Func → nil`, Go's own "or nil if its tag
		// does not match") and its `Reinterpret<Type, ΔFuncType>` is rightly refused, so a perfectly
		// good func type arrives at reflect's funcLayout as nil and is renamed "funcLayout of
		// non-func type" one frame up. Measured consumer: reflect's TestFuncLayout, 10 comparison
		// rows. Synthesized from the carried System.Type exactly as StructType/ArrayType are.
		"Type.FuncType": goosAny,
		// FuncType.InSlice / FuncType.OutSlice are why the accessor alone is not enough, and they
		// are a WORSE instance of the same defect rather than a separate one. Go keeps a func's
		// parameter and result descriptors in the memory immediately AFTER the FuncType record, so
		// the auto bodies walk there arithmetically (`unsafe.Sizeof(*t)`, plus 16 for TFlagUncommon)
		// and build a span of `*Type` over that address. The managed model has no such layout, and
		// the span's element type is `ж<Type>` — a MANAGED REFERENCE — so the auto form fabricates
		// one reference per parameter out of whatever follows the value slot. That is the same
		// type-safety break the descriptor downcast was refused for, arrived at by a different road.
		//
		// Both derive from the descriptor's carried System.Type over GoReflect.TryFuncShape — the
		// SAME projection reflect's hand-owned rtype.NumIn/In/NumOut/Out use one layer up, so the
		// two layers cannot disagree about a func's shape.
		"FuncType.InSlice":  goosAny,
		"FuncType.OutSlice": goosAny,
		// Type.Len is the third accessor of that same recursion, and the one whose failure is
		// nastiest: it reads a length out of the memory following the descriptor's value slot, so
		// two array descriptors read two different pieces of garbage and haveIdenticalUnderlyingType
		// reports [3]byte and [3]byte as different types. The carried dims already answer it (that
		// is what reflect's own rtype.Len reads), so hand-owning it turns a garbage read into the
		// same truthful one, one layer down.
		"Type.Len": goosAny,
		// Type.ChanDir is the FOURTH member of that family and the only one with no synthesis
		// waiting for it: the direction is not unpopulated, it is not in the managed type at all.
		// `<-chan int`, `chan<- int` and `chan int` all emit as golib's `channel<T>`, so the
		// bridge can only ever describe the BIDIRECTIONAL type — and BothDir is that type's real
		// direction, which Type.String() has always agreed with (`chan T`). The downcast instead
		// read a direction out of the memory following the value slot, non-deterministically.
		// A directional Go channel type remains undescribable here; that is a limit of the
		// converter's channel emission one layer up, recorded in ConversionStrategies-Reference.md.
		"Type.ChanDir": goosAny,
	},
	// internal/cpu.getGOAMD64level is declared in cpu_x86.s and its body is a COMPILE-TIME constant:
	// the GOAMD64_vN define the toolchain sets from `go env GOAMD64`, with `#else MOVL $1` as the
	// fall-through. It answers "which amd64 microarchitecture level was this BINARY built for", not
	// "which does this CPU support" — so probing the host would answer a different question. go2cs
	// emits portable C# with no GOAMD64 define and no microarchitecture-gated emission, which makes
	// the faithful answer the same constant Go's own assembly produces for a build without one.
	// cpu_x86_impl.cs returns it. Reached by doinit's option table (whose level < 2/3/4 gates decide
	// which cpu.* GODEBUG knobs stay switchable) and by internal/cpu's own TestDisableSSE3, whose
	// first line is `if GetGOAMD64level() > 1 { t.Skip(…) }` — the unimplemented stub turned that
	// guard into an infrastructure-error where Go reads 1 and walks on to a matching skip.
	"internal/cpu": {
		"getGOAMD64level": goosAny,
	},
	// internal/runtime/atomic's Loadp — *(*unsafe.Pointer)(ptr) over a BARE unsafe.Pointer (the I5
	// ruling, 2026-08-26). Go's body is real (atomic_amd64.go), but its converted form round-trips
	// through the numeric address (`~(ж<Pointer>)(uintptr)(ptr)`), and for a managed referent that
	// number is a transient GC-heap address: the deref keeps accidentally aliasing the slot only
	// until the collector moves it, and resolves to a wild native read after. The mint for
	// `unsafe.Pointer(&x)` now RETAINS the source box (@unsafe.Pointer.FromBox), so atomic_impl.cs
	// hand-owns Loadp over that retained referent (LoadThrough) — the same recovery its sibling
	// StorepNoWB (already hand-owned; its Go body is a `.s` file) takes for the store direction.
	// The *unsafe.Pointer siblings (Casp1/storePointer/casPointer) carry an aliasing
	// ж<unsafe.Pointer> by signature and stay auto-hooked partials.
	"internal/runtime/atomic": {
		"Loadp": goosAny,
	},
	// internal/chacha8rand's two ARRAY-SHAPE reinterpreters. Go opens the `*[32]uint64` output
	// buffer as `(*[16][4]uint32)(unsafe.Pointer(buf))` (block_generic) and that in turn as
	// `(*[16][2]uint64)(unsafe.Pointer(b32))` (setup) — a differently-typed, differently-RANKED
	// view of one allocation. `array<T>` is a window on a real `T[]`, so a nested `uint32` view
	// over a `ulong[]` has no managed spelling; the literal conversion takes the raw-ADDRESS route
	// and dereferences an `array<…>` STRUCT out of the buffer's own DATA. On the zeroed buffer that
	// reads a null backing, i.e. a LENGTH-ZERO array, and the first index panics `index out of
	// range [0] with length 0` — the package's own TestBlockGeneric, which is the whole of its
	// 1-of-4 gap. It is the STRONGEST form of the address-reinterpret seam
	// (`docs/phase4/DESIGN-native-array-view.md`, RATIFIED with its §3 emission work HELD pending
	// the provenance amendment) and does NOT fall with that arc even when it lands: a native-backed
	// `array<T>` still cannot carry `array<uint32>` ELEMENTS in raw bytes. So this is a per-package
	// ROUTE-AROUND, the remedy vendor/…/sha3's xor.cs and crypto/subtle's xor_generic.cs already
	// take for the same class: chacha8_impl.cs takes the view over the array's own SPAN
	// (MemoryMarshal.Cast), a genuine ALIASING view of the same backing storage, so the writes land
	// in the caller's buffer. `block` was ALREADY hand-owned here (Go implements it in assembly),
	// independently and in a scratch-and-pack shape, so TestBlockGeneric still compares two
	// distinct implementations rather than a function with itself.
	"internal/chacha8rand": {
		"setup":         goosAny,
		"block_generic": goosAny,
	},
	// reflect.Value's entry + value-reader methods (the reflection bridge, Phase 2). Go reads the
	// value through v.ptr as flat memory at computed offsets — no managed form. value_impl.cs carries
	// the boxed managed value directly (a companion `partial struct Value { object boxed }` field) and
	// reads it with System.Reflection + the golib container interfaces. Only the value READERS are
	// hand-owned; Kind/Type/IsValid/CanAddr work from the flag/typ_ the entry sets. Increment 1
	// (scalars, slices, arrays, pointers); struct Field/NumField + map MapRange land next.
	"reflect": {
		"ValueOf":         goosAny,
		"unpackEface":     goosAny,
		"valueInterface":  goosAny, // a free function `valueInterface(v Value, safe bool)`, not a method
		"Value.Interface": goosAny,
		"Value.Bool":      goosAny,
		"Value.Int":       goosAny,
		"Value.Uint":      goosAny,
		"Value.Float":     goosAny,
		"Value.Complex":   goosAny,
		"Value.String":    goosAny,
		"Value.IsNil":     goosAny,
		"Value.Len":       goosAny,
		"Value.Index":     goosAny,
		"Value.Elem":      goosAny,
		"Value.Bytes":     goosAny,
		// Two defects in ONE auto body, which is why it is here: the `v.ptr` data-word read below
		// AND the `Reinterpret<abi.Type, sliceType>` descriptor prefix-downcast. A hand-own needs
		// neither — a slice's elements and a map's entries are ordinary managed containers at this
		// layer. See reflect/value_impl.cs.
		"Value.Clear": goosAny,
		// The WRITE half of Bytes, hand-owned for the same reason one layer down: the auto body is
		// `*(*[]byte)(v.ptr) = x`, a store through the Go data word this bridge never populates, so
		// it wrote nowhere for EVERY byte slice — silently. See reflect/value_impl.cs.
		"Value.SetBytes": goosAny,
		// The RUNE twin of SetBytes, carrying the identical defect one line over:
		// `*(*[]rune)(v.ptr) = x`. It is LOUDER than the byte case only by accident of how the
		// null box fails — a nil dereference rather than a silent no-op — so the SHAPE, not the
		// symptom, is what marks it. Reached through makeRunes from the string↔[]rune conversions.
		"Value.setRunes": goosAny,
		// The READ half of the same rune pair, and the same shape again: `~(*[]rune)(v.ptr)`.
		// Bytes/SetBytes were hand-owned as a pair for this reason; runes/setRunes are the pair
		// that was left, and BOTH halves were broken.
		"Value.runes":         goosAny,
		"Value.NumField":      goosAny,
		"Value.Field":         goosAny,
		"Value.UnsafePointer": goosAny,
		"Value.Pointer":       goosAny,
		// The last two type constructors on the typelinks path, joining PointerTo/ArrayOf/SliceOf/
		// StructOf above for the same recorded reason: the auto body looks the constructed type up
		// by NAME through typesByString -> typelinks(), the linker-built type table, which is a
		// NotImplementedException stub here. MakeChan joins them because its auto body calls the
		// `makechan` runtime stub. See reflect/value_impl.cs.
		"ChanOf":   goosAny,
		"MapOf":    goosAny,
		"MakeChan": goosAny,
		// FuncOf is the same family one step harder: it assembles a funcType record behind a
		// prototype func value it reads out of memory, and a Go func here IS a managed delegate.
		// The composed delegate type is GoReflect.TryFuncShape's exact inverse.
		"FuncOf": goosAny,
		// typelinks is the linker's type table; a managed process has none, and EMPTY is the
		// contract-legal answer (Go documents its only consumer, typesByString, as possibly
		// empty, and every caller mints on the miss). The auto form is a throwing stub.
		"typelinks": goosAny,
		// export_test.go's IsExported resolves the descriptor's name offset into the linker's
		// name blob and reads its flag byte -- nil-deref on a synthesized descriptor. The
		// property is answerable from the bridge's own name machinery; body in the
		// export_impl_test.cs companion (the reflectlite pattern).
		"IsExported": goosAny,
		// gcbits answers reflect's TestGCBits, and Go satisfies export_test.go's bodyless
		// `func gcbits(any) []byte // provided by runtime` with
		// `//go:linkname reflect_gcbits reflect.gcbits` -- so in Go this IS runtime.getgcmask under
		// a second name. The corpus cannot cross that seam: the push's DESTINATION is declared in a
		// _test.go, which -tests emits into reflect_internal_test_package rather than into
		// reflect_package where a production-side push would land, and runtime exposes
		// reflect_gcbits as internal with no InternalsVisibleTo for reflect. Without this
		// registration go2cs-gen mints a throwing PartialStubGenerator stub and TestGCBits reports
		// infrastructure-error whatever runtime answers (measured 2026-09-02). The body is in
		// export_impl_test.cs beside IsExported -- the reflectlite pattern.
		"gcbits": goosAny,
		// Swapper reads the slice header through unsafe.Pointer and swaps flat memory by element
		// size -- the mirror of internal/reflectlite's registration, for the same root; the
		// hand-own swaps through golib's non-generic ISlice indexer. See reflect/value_impl.cs.
		"Swapper": goosAny,
		// Select read each case's direction by reinterpreting the descriptor onto the linker's
		// chanType record (the non-deterministic read abi.ChanDir retired) AND ran through the
		// rselect runtime stub. The hand-own uses the direction cargo and bridges golib's own
		// select engine (GoReflect.RunSelect); rselect is left dead. See reflect/value_impl.cs.
		"Select": goosAny,
		// Value.Close reads the channel direction by reinterpreting the descriptor onto the
		// linker's chanType record -- the non-deterministic read abi.ChanDir was hand-owned to
		// retire, still live here -- and then calls the chanclose runtime stub.
		"Value.Close": goosAny,
		// The auto body reads the interface's two words by dereferencing `v.ptr` as an
		// *[2]uintptr, and a bridge Value has no `ptr` — it carries a boxed managed object — so
		// every InterfaceData call nil-panicked in `~`. Go declares BOTH words unspecified and
		// the API deprecated, so the hand-own answers the one property that has observable
		// meaning and that reflect's own tests read it for: direct-iface-ness, off the same
		// GoReflect.GoIsDirectIface authority abi.synthType stamps KindDirectIface from.
		// See reflect/value_impl.cs.
		"Value.InterfaceData": goosAny,
		"Value.MapRange":      goosAny,
		"MapIter.Next":        goosAny,
		"MapIter.Key":         goosAny,
		"MapIter.Value":       goosAny,
		// The rest of the iterator, joining the three above so the hand-own is COMPLETE in both
		// directions. Reset assigns Go's `hiter` (no managed form), so a reset iterator kept every
		// companion field pointing at the PREVIOUS map; SetIterKey/SetIterValue gate on
		// `iter.hiter.initialized()`, never true here, so both panicked "called before Next" on
		// every correct use — and both also carry the mapType prefix-downcast described just below.
		// Leaving these three auto is what made the hand-own ASYMMETRIC: the happy path was managed
		// while Go's zero/exhausted guards were absent and its setters unusable. Measured both ways
		// against go1.23.12 — TestSetIter/TestMapIterSet panicked when they must not, and
		// TestMapIterSafety/TestMapIterReset did not panic when they must.
		"MapIter.Reset":      goosAny,
		"Value.SetIterKey":   goosAny,
		"Value.SetIterValue": goosAny,
		// The map READ pair, same root as MapRange and landed against it: both auto bodies do
		// `v.typ().Reinterpret<abi.Type, mapType>()` to reach the key/elem types off the embedded
		// abi.MapType, and a synthesized descriptor has no abi.MapType behind it — the emitted
		// mapType holds that embed as a promoted REFERENCE box, so the reinterpret reads the
		// descriptor's first word as an object. go/ast's TestPrint (ast.Fprint over a map) died
		// there. Both also index through Go's mapaccess/hiter intrinsics, which MapRange already
		// replaced; MapKeys is now MapRange collected and MapIndex the golib comma-ok lookup, so
		// one key/element typing rule serves the walk and the lookup alike.
		"Value.MapKeys":  goosAny,
		"Value.MapIndex": goosAny,
		// Type side: reflect.rtype's ΔType methods over the abi.Type's System.Type (%T, %+v names).
		"rtype.String": goosAny,
		"rtype.Name":   goosAny,
		// rtype.PkgPath reads the descriptor's TFlagNamed bit and uncommon().PkgPath name-offset —
		// sub-records a synthesized abi.Type never populates, so it answered "" for every type and
		// gob's Register keyed its registry on the bare "N2" instead of "encoding/gob.N2"
		// (TestRegistrationNaming). The managed nesting carries the package identity
		// (GoReflect.GoPackagePath).
		"rtype.PkgPath":  goosAny,
		"rtype.Elem":     goosAny,
		"rtype.Field":    goosAny,
		"rtype.NumField": goosAny,
		// rtype.FieldByIndex reinterprets the rtype as a *structType — and that one is not
		// representable in the managed model, because structType is LARGER than rtype (it carries
		// PkgPath and Fields beyond the embedded Type). ж.Reinterpret takes the aliasing path only
		// when the destination FITS in the source, so this pair falls to the raw-address route and
		// the derived box's embedded abi.Type carries no managed cargo — its sysType is null, and
		// the very first statement, toType(&t.Type), trips canonType's synthType-was-bypassed
		// assertion. That is a Debug.Assert, so the PROCESS DIES (0x80131623): encoding/xml reaches
		// it through getTypeInfo → addFieldInfo on every Unmarshal of a struct with a promoted
		// field, and 15 of its verdicts came back EMPTY rather than failed. Hand-owned to seed from
		// the rtype's OWN descriptor via common() — which is the same abi.Type Go's &t.Type names
		// after the reinterpret — so no structType is ever synthesized. The walk itself is Go's,
		// unchanged, and each hop goes through the already-hand-owned rtype.Field.
		"rtype.FieldByIndex": goosAny,
		// rtype.NumMethod reads uncommon() method tables a synthesized descriptor never
		// populates, so it answered 0 for every concrete type — and encoding/json's indirect()
		// gates its Unmarshaler/TextUnmarshaler discovery on NumMethod() > 0, so no custom
		// UnmarshalJSON was ever dispatched (time's TestTimeJSON / TestUnmarshalInvalidTimes).
		// Hand-owned over the same golib method-set machinery the emitted asserts resolve
		// through (GoReflect.GoMethodCount), so the gate and the assert cannot disagree.
		//
		// The COUNT and the WALK are one increment, not two: a truthful NumMethod is what lets a
		// consumer's `for i := 0; i < n; i++` reach Method(i) at all, and the auto Method(i) reads
		// the same absent uncommon() table — loudly (panic: reflect: Method index out of range,
		// math/rand and math/rand/v2's TestRegress) where the count failed silently. Value.Method
		// binds the receiver into a managed delegate, so the method VALUE is an ordinary Kind-Func
		// Value and Type()/NumIn/In/Out/Call are the existing bridge surface unchanged.
		"rtype.NumMethod":    goosAny,
		"rtype.Method":       goosAny,
		"rtype.MethodByName": goosAny,
		"Value.Method":       goosAny,
		// reflect.Type must be CANONICAL (Go interns type descriptors so `aType == bType` holds for
		// equal types — internal/fmtsort.compare relies on `aType != bType`). The auto Value.Type()
		// and toType() mint a fresh wrapper per call over a fresh abi.Type box, so identity-equality
		// never matched → map-key sorting reversed. The hand-owned forms in value_impl.cs intern the
		// ΔType wrapper by the underlying System.Type (canonType). See docs/phase4/DESIGN-reflection-bridge.md.
		"Value.Type": goosAny,
		"toType":     goosAny,
		// deepValueEqual keys its cycle-detection visited map on the values' internal data words
		// (v.ptr / v.pointer()) — eface addresses the bridge never populates, so the auto form NREs
		// converting the null unsafe.Pointer slot (strings/bytes TestSplit/TestSplitAfter, R5).
		// deepequal_impl.cs recurses over the bridge's boxed values and keys cycle detection on
		// managed reference identity. DeepEqual itself stays auto (it only uses the bridged
		// ValueOf/Type/AreEqual).
		"deepValueEqual": goosAny,
		// Phase-3 write-back (the chip): Set writes through the addressable Value's aliased ж box
		// (Go's assignTo semantics over the golib assert machinery); Zero builds valid zero Values
		// (a pointer kind yields the canonical typed-nil box). The stack-walking member this chip also
		// named was registered here as `methodName`, and `reflect` does not declare that: Go's name is
		// `valueMethodName` (value.go), registered further down with its own note. The dead key
		// displaced nothing, emitted no placeholder and is retired — 2026-09-01, found by
		// TestManualConversionRegistrationsDisplaceSomething, which it is also the first catch of. The
		// reasoning it carried belongs to `valueMethodName`: runtime.Caller has no managed form, and
		// its getcallersp chain NotImplementedException'd every mustBe* panic path, errors TestAs's
		// first operational hit. (value_impl.cs's own superseded `methodName()` body is inert — called
		// by nothing — and stays for the reflect lane to dispose of; its comment carries the
		// receiver-drop reasoning that made the separate valueMethodName body necessary.)
		"Value.Set": goosAny,
		"Zero":      goosAny,
		// Phase-3 increment 2 (the chip): the call & construction half. Value.Call invokes the
		// boxed delegate (DynamicInvoke; results typed by the STATIC out types); the Set* family
		// coerces through GoReflect.TryConvertTo and writes through the aliased box; New/
		// MakeSlice/MakeMap construct golib containers/boxes (named wrappers included, via
		// ISupportMake); Slice windows the shared backing; the rtype func-introspection methods
		// derive from the delegate Invoke signature; Key/Len read GoReflect/descriptor cargo.
		// See docs/phase4/DESIGN-reflection-bridge-phase3-plan.md (INCREMENT 2).
		"Value.Call":      goosAny,
		"Value.CallSlice": goosAny,
		"Value.Slice":     goosAny,
		// Value.Slice3 joined the set on 2026-08-19 (text/template's three-index `slice` builtin):
		// the auto form is the same raw unsafeheader.Slice walk Slice's was, over the ptr slot the
		// bridge never populates, so it nil-dereferenced rather than degrading.
		"Value.Slice3":      goosAny,
		"Value.SetBool":     goosAny,
		"Value.SetInt":      goosAny,
		"Value.SetUint":     goosAny,
		"Value.SetFloat":    goosAny,
		"Value.SetComplex":  goosAny,
		"Value.SetString":   goosAny,
		"Value.SetZero":     goosAny,
		"Value.SetMapIndex": goosAny,
		"New":               goosAny,
		// NewAt built its result type with the internal blob-based ptrTo (PtrToThis/typeOff/String/
		// typesByString), which a synthesized descriptor has none of -- TestGCBits nil-dereferenced
		// there once C2's gcbits reached the body. The hand-own takes *T from abi.synthType(ж<st>),
		// exactly as New and PointerTo already do. See reflect/value_impl.cs.
		"NewAt":     goosAny,
		"MakeSlice": goosAny,
		// SliceAt's auto body called `unsafeslice` (a //go:linkname runtime helper minted as a
		// throwing PartialStub) and then reinterpreted a raw unsafeheader.Slice{Data,Len,Cap} as a
		// slice Value, which the managed model has no representation for. The hand-own validates (Go
		// runtime.unsafeslice's three panics -- len < 0, nil pointer with length, elemSize*len
		// overflow) and builds the ALIASING slice<T> over the pointer's storage via (ж<T>)(uintptr)p
		// + unsafe.Slice<T>. See reflect/value_impl.cs.
		"SliceAt":         goosAny,
		"MakeMap":         goosAny,
		"MakeMapWithSize": goosAny,
		// MakeFunc's auto body is runtime machinery end to end: it reinterprets the descriptor into
		// a funcType sub-record no synthesized abi.Type has behind it (the box comes back zero, Kind
		// 0), asks funcLayout for a stack map over that nothing ("reflect: funcLayout of non-func
		// type <nil>" — net/http/httptrace's compose was the first operational hit), and pairs an
		// assembly stub with a closure context the managed runtime cannot execute. The hand-owned
		// form (reflect/makefunc_impl.cs) is Value.Call's exact inverse: a delegate of the
		// descriptor's carried System.Type (GoReflect.MakeGoFuncDelegate) marshalling its CLR
		// arguments into a slice<Value> for fn and the result Values back out under Call's own
		// assignability rule. makeMethodValue's identical funcLayout read stays auto: only reachable
		// through flagMethod, which the bridge never sets (Value.Method binds a delegate instead).
		"MakeFunc": goosAny,
		// Copy reinterprets BOTH operands' data words as flat `unsafeheader.Slice` headers
		// (`*(*unsafeheader.Slice)(dst.ptr)`) and hands them to typedslicecopy — a raw memory move
		// with no managed form, which on the bridge's never-populated ptr slot dereferences a nil ж
		// outright. encoding/asn1's parseField copies every parsed []byte into its destination
		// through it, which is crypto/x509's ParsePKCS8PrivateKey and so crypto/ecdsa's TestEqual.
		// Bridged element-wise over the same golib container interfaces every other container
		// method uses, so a window slice writes the backing store it shares with its parent.
		"Copy": goosAny,
		// regAssign is displaced for ONE arm. Its Struct arm iterates the descriptor's field list and
		// returns TRUE when that list is empty -- "every field went to a register", zero steps -- so a
		// synthesized struct descriptor (which carries no Fields) is silently reported as fully
		// register-assigned and funcLayout answers size/argsize/retOffset 0 with empty bitmaps. Measured
		// as reflect's TestFuncLayout: `reflect_test.S` arrives Size=32, Fields.Length=0.
		//
		// The hand-own throws on that shape and ONLY that shape. "Empty means cannot see" is FALSE as a
		// general rule -- `struct{}` is legal and ubiquitous, and legitimately has no fields -- so the
		// predicate is `Fields.Length == 0 && Size() > 0`: a 32-byte struct with no fields is
		// definitionally unseeable, while `struct{}` (0 fields, 0 size) passes through untouched. The
		// ARRAY arm deliberately does NOT join it; see the comment at the arm for the measurement.
		// See docs/phase4/DESIGN-descriptor-cargo.md.
		"abiSeq.regAssign": goosAny,
		// addTypeBits is the SAME defect one call over: its Array and Struct arms raw-reinterpret the
		// descriptor as arrayType/structType, which on a synthesized descriptor reads Len 0 / no Fields,
		// so funcLayout's stack and GC bitmaps stay empty (TestFuncLayout/func(reflect_test.S): stack=[] and
		// gc=[] want [0 0 1 1] after R1 fixed the sizes). The companion reads through the abi accessors.
		"addTypeBits": goosAny,
		// valueMethodName is runtime.Callers-based (getcallersp) — managed stack walk instead. The walk
		// is RETIRED: Release inlines an exported Value method into its caller, so the frame the walk
		// climbs for is not there and every mustBe* panic degraded to Go's "unknown method" fallback
		// (measured: reflect's own TestValuePanic, Go="pass" C#="fail" at -test-config Release, the
		// stack showing `mustBe` called straight from TestValuePanic's closure with no Recv frame).
		// It is a name COMPOSER now, fed by [CallerMemberName] — a compile-time constant no JIT can
		// remove — which is why the five mustBe* members below join it: the attribute has to sit on
		// THEIR parameters, and they are emitted into value.cs, which every -tests run regenerates.
		"valueMethodName":           goosAny,
		"flag.mustBe":               goosAny,
		"flag.mustBeExported":       goosAny,
		"flag.mustBeExportedSlow":   goosAny,
		"flag.mustBeAssignable":     goosAny,
		"flag.mustBeAssignableSlow": goosAny,
		// Append/AppendSlice are package-level FUNCTIONS in Go, so Go's own climb sees a `reflect.X`
		// frame, never `reflect.Value.X`, and prints "unknown method" — measured against go1.23.12.
		// The emission gives them a ΔValue first parameter, which is exactly what made the retired
		// walk match them and MANUFACTURE "reflect.Value.Append": a name Go never prints, on two
		// public entry points, tested by nothing. They are hand-owned to thread the sentinel.
		"Append":           goosAny,
		"AppendSlice":      goosAny,
		"rtype.Key":        goosAny,
		"rtype.Len":        goosAny,
		"rtype.NumIn":      goosAny,
		"rtype.In":         goosAny,
		"rtype.NumOut":     goosAny,
		"rtype.Out":        goosAny,
		"rtype.IsVariadic": goosAny,
		// Phase-3 continuation: the type-relation mirrors + conversion. The auto forms walk
		// descriptor sub-records that only exist in Go's runtime layout: implements() does
		// Reinterpret<abi.Type, interfaceType> and reads .Methods off a promoted-embed box that
		// is default behind a synthesized descriptor (gob's init died there); PointerTo builds
		// a ptrType prototype through an eface Reinterpret; Convert dispatches into the cvt*
		// family, which allocates through the nil unsafe_New stub (internal/fmtsort's ct()
		// table, R-13/R-14). All are bridged in value_impl.cs over the shared golib
		// machinery (GoReflect.GoImplements / TryConvertTo) — one method-set/convertibility
		// rule everywhere.
		//
		// `implements` is the FREE function behind rtype.Implements, and it is registered
		// separately because it is what Go's OWN directlyAssignable / AssignableTo / convertOp /
		// Value.assignTo route through. Bridging the method alone left those four on the
		// throwing downcast, and it is also what let rtype.AssignableTo RETIRE from this list:
		// with `implements` and haveIdenticalUnderlyingType answerable and the descriptor
		// carrying TFlagNamed, Go's own `directlyAssignable(uu.t, t.t) || implements(uu.t, t.t)`
		// is correct, and a hand-own that restated it as identity-on-the-managed-type was
		// strictly narrower than the spec — database/sql's TestUserDefinedBytes is the measured
		// consumer that named the gap.
		"rtype.Implements": goosAny,
		"implements":       goosAny,
		// haveIdenticalUnderlyingType is THE seat of Go's type-identity relation (ConvertibleTo
		// through convertOp, AssignableTo through directlyAssignable, assignTo/Convert through
		// both). Five of its eight arms already worked — Array/Map/Pointer/Slice recurse through
		// the Elem()/Key()/Len() internal/abi synthesizes, and the scalar arm needs nothing. The
		// STRUCT, FUNC and INTERFACE arms reached their operands by the prefix-downcast idiom
		// instead, and did not fail loudly: they read ZERO fields / ZERO in-out counts / ZERO
		// methods off a default promoted-embed box and returned TRUE, so any two structs and any
		// two funcs compared IDENTICAL. Measured: `struct{B []byte; M map[string]int}` reported
		// convertible to the same struct with `M map[string]int64`, to one whose field is merely
		// renamed, and to one with a different field COUNT. A false positive in an identity
		// relation is read by every caller as permission, which is why it is fixed in the same
		// change that lets AssignableTo reach it.
		"haveIdenticalUnderlyingType": goosAny,
		// rtype.ChanDir downcasts onto the chanType record and reads a direction out of the
		// memory following the value slot — NON-DETERMINISTICALLY, so MakeChan's
		// `ChanDir() != BothDir` guard and the identity walk's chan arm each answered differently
		// run to run. The bridge answers the direction the descriptor CARRIES (2026-08-20) and
		// BothDir when nothing stamped one, which is the honest answer for a type nothing
		// narrowed. See internal/abi's Type.ChanDir.
		"rtype.ChanDir": goosAny,
		// Value.recv/Value.send open with the SAME downcast one layer down — reinterpreting the
		// descriptor onto the linker's chanType record and reading `.Dir` out of the memory after
		// the value slot — so behind a synthesized descriptor that reads zero, `0 & RecvDir == 0`
		// refused EVERY receive as send-only. Past that test neither could have worked either:
		// they hand a uintptr channel address and an unsafe.Pointer element slot to chanrecv /
		// chansend0, external stubs the PartialStubGenerator fills with NotImplementedException.
		// Bridged over golib's channel<T> through IChannel's type-erased ChanRecv/ChanSend, with
		// the direction asked of the descriptor's cargo — the ONE authority, and the reason these
		// two had to land WITH the direction arc: a working recv behind a direction that always
		// reads bidirectional turns text/template's `range` over a send-only channel from a fast,
		// attributable error into an unbounded hang.
		"Value.recv":    goosAny,
		"Value.send":    goosAny,
		"PointerTo":     goosAny,
		"Value.Convert": goosAny,
		// ArrayOf is PointerTo's sibling — the other run-time TYPE CONSTRUCTOR — and it fails one
		// step earlier than PointerTo did. Before it assembles its arrayType record (Str/Hash/
		// GCData/PtrBytes/Equal, plus a SliceOf for the record's Slice field) it looks the type up
		// by NAME through typesByString → typelinks(), the LINKER-BUILT type table, which has no
		// managed form and is a NotImplementedException stub: so every call threw, whatever it was
		// asked for, and the caller sees an infrastructure error rather than a wrong answer
		// (encoding/gob's TestIgnoreDepthLimit is the measured consumer).
		//
		// None of that record is reconstructible here and none of it is needed: golib's array<T> IS
		// the array type, and the one part of a Go array type the managed emission cannot hold —
		// its LENGTH — is precisely what the reflection bridge's dims cargo already carries for
		// every DECLARED array. So the hand-own composes the same (managed type, dims) pair
		// abi.TypeOf reaches from a live [n]T value, and interning makes the constructed type and
		// the declared one the SAME canonical reflect.Type. See reflect/value_impl.cs.
		"ArrayOf": goosAny,
		// StructOf is ArrayOf's sibling one order of magnitude up. PointerTo and ArrayOf hand
		// MakeGenericType an EXISTING managed type, because ж<T> and array<T> ARE the Go type; a
		// struct has no generic container to instantiate, so StructOf is the one caller that asks
		// for a Go type NOTHING declared and a real CLR value type has to be MINTED for it.
		//
		// The auto body dies where ArrayOf's does — typesByString → typelinks(), the linker-built
		// type table — and everything past that point is Go's runtime reconstructing linker output:
		// structTypeFixedN prototypes, GC-program construction, resolveReflectName into the name
		// blob, unsafe_New. Both of reflect's OWN callers of StructOf are exactly such
		// reconstructions (describing a func's argument frame; getting an rtype followed in memory
		// by a method array), so a hand-own owes them nothing — the census finds one real consumer,
		// encoding/gob's TestIgnoreDepthLimit.
		//
		// The hand-own mints the type with System.Reflection.Emit (golib's GoStructSynthesis) and
		// then does nothing else: abi.synthType describes the minted type exactly as it describes a
		// converted struct, and GoFields / structLayoutOf / FieldAliasBox / ZeroValueOf /
		// GoTypeName / canonType all run UNMODIFIED, because not one of them asks where a
		// System.Type came from. That is the whole argument for the mechanism — a synthetic answer
		// and the converted answer cannot disagree when there is only one path.
		// See docs/phase4/DESIGN-reflect-structof.md and reflect/value_impl.cs.
		"StructOf": goosAny,
		// SliceOf is the third of the family and the cheapest — the PointerTo shape exactly, one
		// MakeGenericType over golib's slice<T>, which IS Go's slice type. It dies in the same
		// typesByString → typelinks() lookup ArrayOf's auto body died in, and was in fact reached
		// FROM there: Go's arrayType record carries a Slice field, so ArrayOf called SliceOf.
		//
		// The one decision it carries is what dims to hand the descriptor, and the answer is NONE.
		// abi.TypeOf measures dims for an ARRAY value and a POINTER's pointee only, so a DECLARED
		// []T descriptor carries null; handing the element's dims through would break the identity
		// that makes the constructed and the declared type ONE reflect.Type, and would not help
		// either, since rtype.Elem's non-pointer, non-map arm consumes the head of the vector. So
		// SliceOf(ArrayOf(3, byte)) describes [][3]byte with its element's length unknown — exactly
		// what TypeOf([][3]byte{}) reads back today. That residual is the cargo model's (a slice
		// type has no dims slot), not this constructor's.
		"SliceOf": goosAny,
		// rtype.FieldByName Reinterprets the descriptor as a structType and reads .Fields off
		// the default promoted-embed box (gob's compileDec matching wire fields to the local
		// struct). Bridged over the shared GoFields projection — the SAME field table
		// NumField/Field/the value side use, single-hop Index included.
		"rtype.FieldByName": goosAny,
		// Value.Cap reads the never-populated v.ptr slice header (gob's decodeSlice probes
		// `value.Cap() < n`); Value.SetLen writes a new header length through it. Bridged over
		// the golib container interfaces; SetLen re-windows the live slice (same backing/cap,
		// Go's s[:n]) and writes it back through the aliased box.
		"Value.Cap":    goosAny,
		"Value.SetLen": goosAny,
		// Value.SetCap is the FIFTH member of that family: the auto form re-caps through
		// `(ж<unsafeheader.Slice>)(uintptr)(v.ptr)` -- the same never-populated header, the same
		// nil dereference inside `~` (TestSetLenCap). Bridged beside SetLen: Go's own bound check
		// (len <= n <= cap) and text, then the three-index window s[:len:n] over the live slice,
		// converted back into the slot's own type and written through the aliased box.
		"Value.SetCap": goosAny,
		// Value.Grow reads a *unsafeheader.Slice off the same never-populated v.ptr, so it
		// nil-deref'd for every caller (gob's decUint8Slice / decodeArrayHelper Grow(1) in a
		// loop past internal/saferio's 10 MiB chunk). Bridged as an ordinary managed
		// reallocation written back through the aliased box, exactly like SetLen.
		"Value.Grow": goosAny,
		// extendSlice is the FOURTH member of that raw-slice-header family and the last one
		// still auto-converted — Slice, Slice3 and Grow all preceded it. Same never-populated
		// v.ptr, same `~(ж<unsafeheader.Slice>)(uintptr)(v.ptr)` nil dereference, and it sits
		// under reflect.Append/AppendSlice, so every append through reflect died inside `~`
		// (TestAppend, TestImplicitAppendConversion). Bridged over the same golib window
		// machinery Slice and Slice3 use; it needs no addressability because Go shallow-copies
		// the header first and never mutates the source.
		"Value.extendSlice": goosAny,
		// Value.IsZero is three descriptor reads a synthesized descriptor never populates —
		// an Equal function pointer against the shared zeroVal buffer, a TFlagRegularMemory
		// all-bits-zero scan, and `v.ptr == nil` for a non-indirect value. The Array and
		// Struct arms both fell to that last one, so EVERY array and EVERY struct reported
		// itself zero whatever it held — silently, `true` being right for the zero value.
		// Bridged as Go's own recursive definition with the memory shortcuts removed.
		"Value.IsZero": goosAny,
		// Value.Addr derives the pointer type through ptrTo → typesByString → the typelinks()
		// runtime stub (the linker-built type table has no managed form), so every Addr threw.
		// The bridge already holds the address: an addressable Value ALIASES the ж<T> box its
		// storage lives in, so Addr surfaces that box (gob's gobEncodeOpFor/gobDecodeOpFor climb
		// one level with Addr for every GobEncoder-implementing field).
		"Value.Addr": goosAny,
	},
	// internal/reflectlite mirrors the reflect bridge for the mini-surface sort.Slice
	// exercises (ValueOf → Len, Swapper — sort's TestSlice was the first operational hit):
	// the auto forms reinterpret the interface's eface words, so the first touch derefs a
	// nil ж<abi.Type>. value_impl.cs carries the boxed managed value on a companion
	// `partial struct Value { object boxed }` field (typ_/flag set from the Phase-1
	// synthetic abi.Type, so Kind()/IsValid() work from value.cs unchanged); swapper_impl.cs
	// swaps through golib's non-generic ISlice indexer. See docs/phase4/DESIGN-reflection-bridge.md.
	//
	// internal/reflectlite MIRRORS reflect's hand-own set for the mini-surface it exposes, so the
	// same three func names (rtype.String, rtype.Implements, haveIdenticalUnderlyingType) appear in
	// BOTH this sub-map and the "reflect" one above — different packages, not duplicate keys (a
	// duplicate key in one Go map literal is a compile error; this file compiles). A file-wide key
	// census reads them as duplicates; the seam says otherwise here so the next reader does not.
	"internal/reflectlite": {
		"ValueOf":     goosAny,
		"unpackEface": goosAny,
		"Value.Len":   goosAny,
		"Swapper":     goosAny,
		// Phase-3 write-back — the errors.As surface. The auto forms read the never-populated
		// v.ptr eface word (IsNil answered TRUE for every pointer; Elem returned the invalid
		// Value) or descriptor sub-records synthType never populates (rtype.Elem panicked;
		// implements() reinterpreted the descriptor). Bridged in value_impl.cs / type_impl.cs
		// over the carried System.Type + the golib method-set machinery.
		"Value.Elem":       goosAny,
		"Value.IsNil":      goosAny,
		"Value.Set":        goosAny,
		"rtype.Elem":       goosAny,
		"rtype.Implements": goosAny,
		"methodName":       goosAny,
		// rtype.String reads a type-descriptor NAME OFFSET into the linker-built name blob
		// (`t.nameOff(t.Str).Name()`) that a synthesized descriptor never populates, so it
		// answered "" for EVERY type — silently, since the empty string is a legal name for an
		// unnamed type. reflect's own rtype.String is already hand-owned over GoReflect.GoTypeName;
		// this is the same answer for the mini-bridge, so the two can never disagree.
		"rtype.String": goosAny,
		// rtype.PkgPath reads the uncommonType's PkgPath name offset — the same linker name
		// blob as rtype.String, so it answered "" for every type. Bridged over
		// GoReflect.GoPackagePath, the exact machinery reflect's side uses, gated on HasGoName
		// so an anonymous lift answers Go's "" (reflectlite's TestImportPath measured
		// encoding/base64 and the test package's own path).
		"rtype.PkgPath": goosAny,
		// rtype.AssignableTo is NO LONGER hand-owned — the identity-on-the-managed-type
		// restatement was strictly narrower than Go's rule, exactly the defect reflect retired
		// (database/sql's TestUserDefinedBytes there; reflectlite's TestAssignableTo `*int` ↔
		// `type IntPtr *int` rows here). Go's own body runs over the two bridged primitives
		// below, mirroring reflect's retirement one layer down.
		//
		// `implements` is the FREE function directlyAssignable/AssignableTo/assignTo route
		// through (the auto form reinterprets the descriptor as an interfaceType and reads
		// .Methods off a default promoted-embed box); haveIdenticalUnderlyingType is THE seat
		// of the identity relation, whose struct/func/interface arms reached their operands by
		// the prefix-downcast idiom and answered TRUE off zero-read records. Both bridged in
		// type_impl.cs over the same golib machinery as reflect's (GoImplements, GoFields,
		// TryFuncShape), so the two layers cannot disagree.
		"implements":                  goosAny,
		"haveIdenticalUnderlyingType": goosAny,
		// The reflection surface export_test.go hands the SUITE — Field/TField/Zero — builds
		// raw Value{typ, ptr, flag} triples over descriptor downcasts, neither of which the
		// managed bridge populates (v.ptr is never a real address; the downcast reads a
		// default record). Hand-owned in export_impl_test.cs — the first TEST-file companion,
		// carried by the `*_impl_test.cs` convention (the `_test.cs` suffix keeps it under the
		// production csproj's existing test-artifact exclusion; testConversion globs it into
		// the tests project) — mirroring reflect's hand-owned Value.Field/Zero over
		// GoFields/FieldAliasBox/ZeroValueOf. StructFieldType stays literal: it walks whatever
		// StructType record it is HANDED, and the hand-owned TField hands it the synthesized
		// one (abi's Type.StructType()).
		"Field":  goosAny,
		"TField": goosAny,
		"Zero":   goosAny,
		// valueInterface is the mini-bridge's packEface seam: the literal packEface
		// reinterprets a heap `any` as an eface and derefs the never-populated words ("bad
		// indir" / nil ж deref — seven of the suite's 23 mini-bridge failures). Mirrors
		// reflect's packInterfaceValue: the live boxed value, with a null read out of a
		// POINTER-kinded Value re-encoded as the canonical typed nil.
		"valueInterface": goosAny,
	},
	// os.(*File).readdir walks the raw buffer GetFileInformationByHandleEx fills by REINTERPRETING
	// it as a Go struct — `(*windows.FILE_ID_BOTH_DIR_INFO)(entry)`. That struct is managed-referent
	// (its trailing `FileName [1]uint16` / `ShortName [12]uint16` are golib `array<uint16>` object
	// references, 8 bytes where the OS wrote 2/24 inline), so the managed layout does NOT match the
	// bytes: `&info.FileName[0]` read a zero-length array (IndexOutOfRangeException at the FIRST
	// directory read — path/filepath.Glob, os.ReadDir, every testdata-reading test), and any copy of
	// the reinterpreted struct hands the GC a fabricated object reference. This is the raw-metal-on-
	// non-native-types fork: dir_windows_impl.cs decodes the entry fields from the byte slice at
	// their documented offsets and never materializes a managed struct over OS memory.
	// The DARWIN flavor is the same fork at a third site and is scoped in for it: dir_darwin.go's
	// readdir hands libc's readdir_r the ADDRESS of a Go `syscall.Dirent` (`readdir_r(d.dir,
	// &dirent, &entptr)`), whose converted form carries a managed `array<uint8>` reference where the
	// C struct has inline storage — the same non-blittable-struct-by-address seam as syscall's
	// wrappers below. Its hand-own LANDED 2026-08-23 (os/darwin/dir_darwin_impl.cs), dispatched by
	// the FIRST darwin CI census, which found the suppression standing with no companion beside it:
	// 19 errors on both mac legs, all three dir.cs call sites. The companion keeps Go's protocol and
	// replaces only the unrepresentable buffer — ONE unmanaged block per call holding libc's entry
	// record and the `dirent *` out-slot, decoded at darwin's documented offsets — because BOTH
	// native arguments are unrepresentable: the entry is the non-blittable-struct-by-address seam,
	// and `**Dirent` is the OUT-parameter class beside it (`ж<T> → uintptr` answers 0 for a nil box,
	// so libc would receive a NULL slot and the EOF test could never observe anything else).
	//
	// LINUX is scoped OUT, and that is what the scope bought: dir_unix.go's readdir is pure Go over
	// internal/poll's ReadDirent and the dirent_linux.go accessors, so it converts faithfully and
	// needs no hand-own at all. Name-keyed, this entry deleted that body too and left every Linux
	// `os` build with a placeholder and nothing to link against (54 CS0103 two layers below, then 1
	// here) — a hand-own gap invented by the registry rather than by the Go source.
	//
	// os.readReparseLink (file_windows.go) is the SAME fork at a second site: it reinterprets the
	// byte buffer DeviceIoControl fills as windows.{SymbolicLink,MountPoint}ReparseBuffer, both of
	// which end in `PathBuffer [1]uint16` — a Go inline array standing in for the variable-length
	// name the OS wrote after it, and an 8-byte MANAGED REFERENCE in the conversion. golib refuses
	// to alias managed storage for a reference-bearing struct (correctly — that is the fabrication
	// case), so the reinterpret takes the raw-address route and `&rb.PathBuffer[0]` resolves an
	// object reference synthesized out of path bytes: ACCESS_VIOLATION inside array<uint16>.get_Item,
	// which KILLED the whole C# test host mid-run at os's TestReadlink and emptied every verdict
	// after it. file_windows_impl.cs decodes the record from the byte slice at its documented
	// offsets. openSymlink and normaliseLinkPath stay auto — they pass scalars, handles and strings.
	"os": {
		"File.readdir":    goosWindowsDarwin,
		"readReparseLink": goosWindows,
	},
	// os/user's two NetUserGetInfo readers are the SAME fork as net.adapterAddresses below, one
	// structure smaller, and reached through the ptrout class rather than through a []byte the
	// caller fills itself. syscall.NetUserGetInfo hands back a netapi32 buffer and Go reads it as a
	// LEVEL RECORD — `(*UserInfo10)(unsafe.Pointer(p))` for the full name, `(*UserInfo4)(...)` for
	// the primary-group RID. Both records carry `ж<uint16>` fields where native USER_INFO_* has raw
	// pointers, so the reinterpret falls to a native-address box and the very next dereference
	// fabricates managed references out of kernel bytes — the identical failure adapterAddresses
	// had, and precisely why the wrapper could not be taken on its own: publishing a real address
	// while these callers stayed as-is would upgrade a contained nil into heap corruption.
	//
	// The remedy is adapterAddresses': lookup_windows_impl.cs mirrors USER_INFO_10 and USER_INFO_4
	// blittably, transcribes the fields these two callers actually read, and releases the netapi32
	// buffer eagerly instead of through the converted defer. Only these TWO functions are
	// hand-owned; every other lookup_windows.go declaration reads managed values and converts
	// faithfully, so hand-owning the file wholesale would freeze correct conversions for no gain.
	//
	// listGroupsForUsernameAndDomain is the THIRD consumer, and its route to the same fabrication is
	// the one worth naming because it is not a Reinterpret at all: it lays a
	// `ReadOnlySpan<LocalGroupUserInfo0>` DIRECTLY over the native buffer, and that record's single
	// field is a `ж<uint16>`. A Span<T> over native memory whose T carries a managed reference
	// fabricates one per element, so an out-parameter's own type says nothing about the safety of
	// the read — the CALLER decides that, which is the whole lesson of this arc. It lands with
	// internal/syscall/windows' NetUserGetLocalGroups in the same change, for the same reason the
	// other two landed with syscall's NetUserGetInfo.
	//
	// Declared only in lookup_windows.go, so the entry is inert elsewhere; scoped anyway so a
	// same-named unix declaration cannot silently inherit a Windows hand-own the way readdir did.
	"os/user": {
		"lookupFullNameServer":           goosWindows,
		"lookupUserPrimaryGroup":         goosWindows,
		"listGroupsForUsernameAndDomain": goosWindows,
	},
	// net.adapterAddresses is the SAME fork as os.readdir/readReparseLink above, at the biggest
	// record in the corpus — and it is the single producer behind every Windows interface and
	// DNS-configuration answer net can give (interfaceTable, interfaceAddrTable,
	// interfaceMulticastAddrTable, and dnsReadConfig, which is getSystemDNSConfig's ONLY source of
	// DNS servers on Windows). Go fills a []byte with GetAdaptersAddresses and then walks it as a
	// linked record: `(*windows.IpAdapterAddresses)(unsafe.Pointer(&b[0]))`. The CALL is legitimate;
	// the WALK is not. IpAdapterAddresses carries nine `ж<T>` fields, an `array<byte>` and an
	// `array<uint32>` where the native record has raw pointers and inline storage, so golib
	// correctly refuses to alias the byte run as that struct and the reinterpret falls to a
	// native-address box — after which the loop's own nil test fabricates a managed reference out of
	// adapter bytes and the PROCESS dies (ACCESS_VIOLATION in ж<IpAdapterAddresses>.op_Equality,
	// measured from crypto/tls's TestVerifyHostname through dnsReadConfig). It is what stood between
	// the corpus and any name resolution at all on Windows, one layer behind the GetAddrInfoW fix.
	//
	// The remedy is ADDRINFOW's, one structure size up: net/windows/interface_windows_impl.cs holds
	// the buffer in NATIVE memory that never escapes the function and transcribes the whole chain —
	// including each record's SIX nested lists and every sockaddr — into managed boxes, freeing the
	// native buffer eagerly. Its sockaddrs need no ManagedPointerTokens (unlike ADDRINFOW's) because
	// Go declares that field as a TYPED `*syscall.RawSockaddrAny`, so there is no unsafe.Pointer
	// round trip to survive — the consumers' `.Sockaddr()` is syscall's own hand-owned decode, and
	// the transcription writes the managed image that decode reads.
	//
	// ONLY adapterAddresses is hand-owned: its three interface_windows.go siblings and
	// dnsconfig_windows.go's dnsReadConfig read the managed records it returns and convert
	// faithfully. The generated windows.GetAdaptersAddresses wrapper is left auto too — it is
	// CORRECT for the native-address box the hand-own hands it, so hand-owning it would have fixed
	// nothing and frozen a faithful conversion for no gain.
	//
	// Declared only in net's interface_windows.go, so the entry is inert elsewhere; scoped anyway so
	// a same-named unix declaration cannot silently inherit a Windows hand-own the way readdir did.
	"net": {
		"adapterAddresses": goosWindows,
	},
	// internal/poll's FOUR raw-sockaddr converters. Go moves the address by pointer arithmetic over
	// flat bytes -- `(*RawSockaddrInet4)(unsafe.Pointer(rsa))` then `(*[2]byte)(unsafe.Pointer(
	// &pp.Port))` -- and neither line survives the managed representation, in two DIFFERENT ways.
	// The reinterpret asks golib to alias one reference-bearing struct as another, and the two
	// managed layouts share no field offsets at all (RawSockaddrAny holds int8[14] and int8[100]
	// object references where sockaddr_in has four inline octets), so `pp.Addr` reads the WRONG
	// FIELD -- measured at Length=14, which is RawSockaddr.Data. The byte view reinterprets the
	// pointed-at bytes as an `array<byte>` STRUCT and fabricates a managed reference out of them.
	// All four route through syscall's mirror, so the sockaddr layout is spelled in exactly ONE
	// place in the corpus. Windows-only: these are fd_windows.go's.
	//
	// The DECODERS (rawToSockaddr*) came first and use RawSockaddrAny.Sockaddr, which flattens the
	// managed struct back to its 116-byte native image. The ENCODERS (sockaddr*ToRaw) run the same
	// two mechanisms in reverse and are the worse half by a distance: WRITING the wrong offsets
	// deposits a uint16 over the low half of a live object reference, so where a bad decode returns
	// a wrong answer, a bad encode corrupts the heap and kills the host at a death site that moves
	// from run to run. Measured as `index out of range [0] with length 0` (v4) and the same panic
	// with a garbage negative length (v6), and it is what capped `net` at a ~308-name empty tail.
	// The v4 twin is GC-safe by layout ACCIDENT and is hand-owned anyway -- an accident about a
	// field offset is not a contract. They use the inverse seam, GoRawSockaddrFromInet4/6, which
	// names the same helper pair the decode does.
	"internal/poll": {
		"rawToSockaddrInet4": goosWindows,
		"rawToSockaddrInet6": goosWindows,
		"sockaddrInet4ToRaw": goosWindows,
		"sockaddrInet6ToRaw": goosWindows,
		// fdMutex's read/write lock pair. Go hands the ADDRESSES of two semaphore WORDS
		// (`&mu.rsema`, `&mu.wsema`) to the runtime primitives, and the converted port keys its
		// waiter table by the address box -- so these two methods minted a ж<uint32> per wait and,
		// because only a box can carry that identity, could never take a `ref fdMutex` receiver.
		// That pinned everything above them (FD.readLock/writeLock, FD.Write, os.File's pfd).
		// The identity was a property of the port, not of Go: the words are dead storage (Go's
		// internal/poll never reads or writes either as a VALUE -- only the two declarations and
		// four address-takes exist in 1.23.12), and the count lives in the side table. So the gate
		// moves INLINE into the struct, as hand-owned sync/mutex.cs already does for sync.Mutex,
		// and the pair becomes genuine `ref fdMutex` primaries. goosAny: fd_mutex.go carries no
		// build constraint, so every target's emission needs the same displacement.
		// internal/poll/fd_mutex_impl.cs holds the bodies and declares the two gate fields.
		// NOT registered, deliberately: FD.csema is the same family and a different access shape
		// (a free-function argument from converted Close/destroy, which never hold the containing
		// struct), so it keeps the side table and is untouched here.
		// increfAndClose joins them because the protocol, not the box census, decides the set: it is
		// the function that WAKES the waiters rwlock parks (Go's TestMutexCloseUnblock pins exactly
		// that -- "IncrefAndClose() // Must unblock the readers"). Left auto-converted it releases
		// through the side table while rwlock waits on the inline gate, so every close-time wakeup is
		// lost; measured as that test failing at its own 10 s deadline against a passing Go oracle.
		"fdMutex.increfAndClose": goosAny,
		"fdMutex.rwlock":         goosAny,
		"fdMutex.rwunlock":       goosAny,
	},
	// debug/pe's COFF symbol reader pair. Go re-VIEWS one 18-byte symbol record as two struct
	// shapes — `(*COFFSymbolAuxFormat5)(unsafe.Pointer(&sym))` — a free re-typing of the same
	// bytes (both structs are exactly 18 bytes, no padding). The managed surrogates are NOT those
	// bytes: COFFSymbol.Name is an `array<uint8>` MANAGED REFERENCE where Go has 8 inline octets,
	// so the two shapes share no Go-compatible managed layout, Reinterpret's alias arm correctly
	// refuses the pair (6 fields vs 7, no recursive field-type match), and the fallback view puns
	// the C# layouts instead — the scalars land bijectively (write and read cross the same view,
	// so the values round-trip), but the aux shape's blank `_ [3]uint8` SLOT overlays the Name
	// reference and answers with the 8-element Name array itself: debug/pe's
	// TestReadCOFFSymbolAuxInfo prints `_:[0 0 0 0 0 0 0 0]` where Go prints `_:[0 0 0]`.
	// symbol_impl.cs transcribes the GO layout explicitly at both seams instead: readCOFFSymbols
	// decodes every 18-byte record — primary and aux alike — through the COFFSymbol shape (for an
	// aux record that is byte-identical to Go's aux-view read, with the blank-field skip
	// reproduced), so File.COFFSymbols holds exactly the field values Go's memory holds; and
	// COFFSymbolReadSectionDefAux decodes that image back into a real COFFSymbolAuxFormat5 box.
	// Same family as the zero-size/layout-emission arc ("the C# struct is not the Go struct's
	// bytes"); the board's debug/pe entry records the one-file hand-own of the symbol reader as
	// this package's sanctioned remedy.
	"debug/pe": {
		"readCOFFSymbols":                  goosAny,
		"File.COFFSymbolReadSectionDefAux": goosAny,
	},
	// sync's copyChecker detects a copied Cond by storing its OWN ADDRESS in itself and comparing:
	// `uintptr(*c) != uintptr(unsafe.Pointer(c))`. Both halves are raw-metal on a managed referent.
	// The stored word cannot be an address at all (the GC moves boxes, so a compaction between two
	// Wait calls would make a perfectly valid Cond look copied — a SPURIOUS panic), and the
	// address-of-self operand converts to `unsafe.Pointer.FromRef(ref c)` while the CAS operand
	// converts to `Ꮡ((uintptr)(c))`, which boxes a COPY of the value — so the auto body never
	// initializes the checker and never panics, however often the Cond is copied (sync's
	// TestCondCopy). cond_impl.cs compares the pointer's ROOT ALLOCATION IDENTITY instead, which is
	// stable across GC moves and is the managed spelling of the same question. Only this one method
	// is hand-owned; the copyChecker type and every Cond method stay auto.
	"sync": {
		"copyChecker.check": goosAny,
	},
	// syscall's generated GetTimeZoneInformation wrapper hands the native call the ADDRESS of the
	// managed Timezoneinformation box — `Syscall(…, uintptr(unsafe.Pointer(tzi)), …)`. Go's struct
	// is 172 bytes with two INLINE [32]uint16 name buffers; the converted one is ~64 bytes with two
	// `array<uint16>` MANAGED REFERENCES in their place, so the kernel writes 172 bytes of native
	// TIME_ZONE_INFORMATION over a smaller managed object and fabricates object references in the
	// name fields. The next `z.StandardName[:]` then faults (ACCESS_VIOLATION inside
	// slice<ushort>..ctor), which takes down every converted program that reaches
	// time.initLocal — i.e. any Weekday()/Location()/Local use on Windows.
	//
	// This is the same struct-passing seam as exec_windows.go's StartProcess and takes the same
	// remedy: a blittable [StructLayout(LayoutKind.Sequential)] mirror with `fixed` name buffers and
	// a direct P/Invoke, copying field-for-field into the converted struct at the boundary
	// (zsyscall_windows_impl.cs). ONLY this one wrapper is hand-owned; every other declaration in
	// the generated file stays auto — they pass scalars and handles, which convert faithfully.
	//
	// All five are declared ONLY in Go's Windows sources (zsyscall_windows.go / syscall_windows.go),
	// so a non-Windows conversion never sees the declaration and the entries were already inert
	// there. The scope changes no emission; it states the fact instead of leaving it to be
	// re-derived from Go's file set, and it is what keeps a future same-named unix declaration from
	// silently inheriting a Windows hand-own the way os.(*File).readdir did.
	"syscall": {
		// The LINUX members of the same struct-passing class, measured by the 2026-08-22 Linux roster
		// re-run (the poll-seam lane's R1): Stat_t on linux/amd64 carries `X__unused [3]int64`,
		// which converts to an `array<int64>` MANAGED REFERENCE, so the converted struct is not
		// blittable — the CLR lays it out itself (~128 bytes) — and the generated wrappers hand the
		// kernel `uintptr(unsafe.Pointer(stat))`, the pinned MANAGED image. fstatat(2)/fstat(2) then
		// write the 144-byte native `struct stat` over a field order that is not the kernel's and 16
		// bytes past the object: `os.Stat(dir)` answered `IsDir() == false, Mode() == p---------`
		// with a nil error (a quiet wrong answer, the class's worst shape), `Stat().Size()` read 0,
		// and every Glob/Walk/ReadDir-with-Info on the Linux flavor followed — 8 roster rows plus
		// partials. ONLY these two wrappers are hand-owned (zsyscall_linux_amd64_impl.cs): Stat and
		// Lstat are pure Go over fstatat (syscall_linux_amd64.go) and convert faithfully, and every
		// other wrapper in the generated file passes scalars or a byte pointer. Scoped to linux
		// because darwin declares both names too, with working libc-backed bodies.
		"Fstat":   goosLinux,
		"fstatat": goosLinux,
		// The class's first BLOCKING member (the os/exec SIGSEGV arc, 2026-08-26): the generated
		// wait4 handed the kernel two golib box addresses across a call that SLEEPS until a child
		// changes state, so GC compaction relocated the boxes mid-wait and the kernel's eventual
		// status/rusage write corrupted the heap — a SIGSEGV at a moving later point, 4-for-4 on
		// os/exec's suite the day the exec wall opened. Hand-owned beside Fstat with stack-local
		// native buffers that live for the whole call. Scoped to linux exactly as Fstat: darwin
		// declares wait4 too, with a working libc-backed body.
		"wait4": goosLinux,
		// The class's third member, reached 2026-08-28 by os/exec's TestFindExecutableVsNoexec:
		// the kernel writes six 65-byte INLINE character arrays (390 bytes of `struct utsname`)
		// where the converted Utsname is six `array<int8>` references and no characters at all,
		// so `unix.KernelVersion()` — whose whole body is Uname plus a parse of Release — read
		// (0, 0) and the test took Go's OWN v5.8 skip on a 5.15 kernel that has faccessat2. A
		// quiet wrong answer of the Stat_t kind, one level removed. Hand-owned beside Fstat with
		// a blittable mirror; scoped to linux because Uname is a Linux-only declaration.
		"Uname":                  goosLinux,
		"GetTimeZoneInformation": goosWindows,
		// The same seam over a bigger record, and the first member of the class an actual suite
		// reached: the kernel writes a 592-byte WIN32_FIND_DATAW, whose cFileName[260] and
		// cAlternateFileName[14] are 520 and 28 bytes of INLINE storage where the converted
		// win32finddata1 has two one-word `array<uint16>` references. path/filepath's EvalSymlinks
		// → toNorm → normBase asks FindFirstFile for the on-disk spelling of every path element,
		// and the clobbered reference took the test host down BOTH ways — IndexOutOfRangeException
		// inside PinnedBuffer where it still resolved to something, ACCESS_VIOLATION in
		// slice<ushort>..ctor where it did not. Only the two *1 wrappers are hand-owned: Go itself
		// puts the native-layout boundary at win32finddata1 (syscall_windows.go's FindFirstFile
		// allocates one, calls the wrapper, then copies out), so the public FindFirstFile /
		// FindNextFile and copyFindData above them are pure Go logic and convert faithfully.
		"findFirstFile1": goosWindows,
		"findNextFile1":  goosWindows,
		// The third member, and the one that fails SILENTLY: PROCESSENTRY32W is 568 bytes ending in
		// szExeFile[260] INLINE, where the converted ProcessEntry32 holds that as one
		// `array<uint16>` reference — the record is ~56 bytes, every field past th32DefaultHeapID
		// reads from the wrong offset, and nothing faults. syscall.Getppid therefore answered 0,
		// which os's TestGetppid is the demonstrated consumer of. dwSize is an INPUT the mirror has
		// to own as well: Go sets it from `unsafe.Sizeof(procEntry)`, which is the MANAGED size here.
		"Process32First": goosWindows,
		"Process32Next":  goosWindows,
		// The SOCKET-ADDRESS family — the member `net` forces, and the first that is two defects
		// rather than one (syscall_windows_impl.cs carries the full write-up).
		//
		// The two encode methods first. Go writes the port in network byte order through a
		// `(*[2]byte)(unsafe.Pointer(&raw.Port))` alias, and an `array<T>` rebuilt from a raw
		// address is a LENGTH-ZERO array, so `p[0]` panics before any socket call happens —
		// net.Listen never reached bind. Hand-owning them replaces the alias with a plain field
		// write; `raw` is left in exactly the state Go leaves it.
		//
		// RawSockaddrAny.Sockaddr — the DECODE — carries the SAME alias, and is here NOW; it was
		// deliberately absent until 2026-08-14 and that reason is worth keeping, because it is the
		// clearest case in the corpus of a hand-own gated on a CONVERTER CAPABILITY rather than on
		// effort. The only casts of the three Sockaddr types to ΔSockaddr in the package used to live
		// in ITS body, so displacing it dropped the `[assembly: GoImplement<…>(Pointer = true)]`
		// records from package_info.cs — and a MEASURED reconvert of `net` against that shortened
		// package_info showed net minting its OWN `syscall_SockaddrInet4жΔSockaddr` adapters instead
		// of using syscall's: the SECOND-IDENTITY regression samePackageImplements.go exists to
		// prevent.
		//
		// What changed is that recordSamePackageImplements now records the POINTER method set as well
		// as the value one, so the three records are sourced from types.Implements(*T, Sockaddr)
		// rather than from this body's casts. They survive its suppression, and the netpoll S2b lane
		// re-measured exactly that on its own build before adding the line below: with the body
		// displaced, syscall's package_info.cs still carries all three records and a reconvert of
		// `net` still references syscall's adapters rather than minting its own.
		//
		// Why it is taken at all: net's ACCEPT path decodes the AcceptEx output through it
		// (net/windows/fd_windows.cs:255-256), the one route to a Sockaddr that the hand-owned
		// Getsockname/Getpeername do not already cover — so no TCP round trip is reachable while it
		// panics. The old note said this path was "walled independently"; that wall is this arc.
		//
		// Since 2026-08-22 the two INET encoders are hand-owned on LINUX as well: the Linux flavor
		// has the identical port alias (syscall_linux.go writes `(*[2]byte)(unsafe.Pointer(&sa.raw.Port))`
		// and anyToSockaddr reads it back the same way), and it was the 2026-08-22 Linux roster
		// re-run's R5 — encoding/json's TestHTTPDecoding and crypto/tls's TestMain both died in
		// SockaddrInet4.sockaddr at `index out of range [0] with length 0` before any socket call.
		// sockaddr_linux_impl.cs carries the Linux bodies; the scope says BOTH flavors, each file its
		// own authority. RawSockaddrAny.Sockaddr is Windows-only in Go; Linux's decode is the free
		// function anyToSockaddr, registered separately below.
		// DARWIN joins 2026-09-05 (increment 8, syscall/darwin/sockaddr_darwin_impl.cs): the five
		// behavioral net rows died on both mac legs in SockaddrInet4.sockaddr() <- Bind before any
		// socket call -- the same port alias and the same struct-passing seam, over the BSD layout
		// (a one-byte sa_len ahead of a one-byte family). All three flavors now hand-own these, each
		// file the authority on its own body, so the scope is goosAny.
		"SockaddrInet4.sockaddr":  goosAny,
		"SockaddrInet6.sockaddr":  goosAny,
		"RawSockaddrAny.Sockaddr": goosWindows,
		// Then the seam itself. RawSockaddrInet4's `Addr [4]byte` / `Zero [8]uint8` are golib
		// `array<byte>` MANAGED REFERENCES, so `unsafe.Pointer(&sa.raw)` names a ~24-byte object
		// with references where Windows expects a 16-byte sockaddr_in with the octets inline —
		// the same class as GetTimeZoneInformation above, and the case golib's own ж.cs note
		// describes as having "no native layout either". Each of these builds a blittable mirror
		// in a stack buffer and passes its ADDRESS to the package's generated wrapper, which was
		// never the broken part; only the layout translation is hand-owned.
		//
		// Getsockname/Getpeername are here for the same reason a level down: their generated
		// wrappers take a typed `ж<RawSockaddrAny>` instead of an address, so the hand-owned
		// forms call the Syscall trampoline directly. The UDP senders (WSASendto and its Inet4/
		// Inet6 variants) are deliberately NOT listed — nothing on the TCP listen/dial/accept
		// path reaches them, and the board's ruling is to fix a censused wrapper when a suite
		// reaches it rather than speculatively.
		// Bind/Connect/Getsockname/Getpeername are hand-owned on Linux too (2026-08-22): the Linux
		// generated `bind`/`connect` take an address (reused with a stack mirror exactly as here),
		// and `getsockname`/`getpeername`/`accept4` take a typed `ж<RawSockaddrAny>` that the kernel
		// would fill by address — so those go through the Syscall trampoline with a stack buffer and
		// ONE native decode (readNativeSockaddr), which anyToSockaddr — Linux's decode, reached from
		// Accept4/Getsockname/Getpeername and the UDP receive path — also becomes. ConnectEx is
		// Windows-only in Go. Accept4 and anyToSockaddr are declared only in Go's Linux sources.
		// Darwin joins Bind/Connect/Getsockname/Getpeername 2026-09-05 (increment 8): its generated
		// `bind`/`connect` take an address (reused with a stack mirror exactly as linux's), and its
		// `getsockname`/`getpeername`/`accept`/`recvfrom` take a typed `ж<RawSockaddrAny>` that the
		// kernel would fill by address -- so those call the libc trampolines directly with a stack
		// buffer and ONE native decode. Accept is Go's darwin body (syscall_bsd.go) and is darwin's
		// alone: linux's Accept is pure Go over Accept4 and stays auto.
		"Bind":      goosAny,
		"Connect":   goosAny,
		"Accept":    goosDarwin,
		"ConnectEx": goosWindows,
		// The same seam from the WRITE side, and the multicast half of net's residual.
		// `ip_mreq` is two INLINE in_addr; converted, IPMreq holds both as golib `array<byte>`
		// MANAGED REFERENCES, and the generated wrapper handed them to setsockopt via
		// `Ꮡmreq.Reinterpret<IPMreq, byte>()`. golib refuses to alias a reference-bearing pointee
		// (so it can never fabricate one), the call falls to the address route, and the kernel gets
		// eight bytes that are two OBJECT REFERENCES -- WSAEINVAL, surfacing as
		// `setsockopt: The requested address is not valid in its context` on IP_ADD_MEMBERSHIP.
		// SetsockoptIPv6Mreq is NOT registered: Go returns EWINDOWS there, so there is nothing to
		// preserve and a hand-own would invent behaviour.
		// LINUX joins the same registration 2026-09-03, and the scope is the whole of what changed:
		// the defect above is the CONVERTED STRUCT's, not the platform's, so leaving this at
		// goosWindows left the Linux flavour on the generated body with the identical fault.
		// Measured: net's TestIPv4MulticastListener fails on Linux with `setsockopt: cannot assign
		// requested address` -- EADDRNOTAVAIL on IP_ADD_MEMBERSHIP, the Linux errno spelling of the
		// WSAEINVAL recorded above for the same call -- and the managed IPMreq measures 32 bytes
		// (array<byte> is 16: a T[] reference plus two ints) against the 8 SizeofIPMreq promises.
		//
		// The census that found it found SEVEN more members of the same family at the same boundary,
		// where the ruling had named three, and they are registered together because this file's own
		// doctrine forbids deferring the write-back half: the Getsockopt* wrappers allocate a HEAP
		// box of the managed struct and hand the kernel its address, so the kernel writes over
		// `array<>` references inside a GC-tracked object -- the corruption sub-class, not the
		// wrong-answer one. ICMPv6Filter is the fourth struct, surfaced by the census rather than by
		// the ruling. Bodies for all eight in syscall/linux/structclass_linux_impl.cs.
		//
		// SetsockoptIPv6Mreq stays UNREGISTERED on windows for the reason given above (Go returns
		// EWINDOWS there, so a hand-own would invent behaviour); on linux it is a real option.
		"SetsockoptIPMreq":       goosWindowsLinux,
		"SetsockoptIPMreqn":      goosLinux,
		"SetsockoptIPv6Mreq":     goosLinux,
		"SetsockoptICMPv6Filter": goosLinux,
		"GetsockoptIPMreq":       goosLinux,
		"GetsockoptIPMreqn":      goosLinux,
		"GetsockoptIPv6Mreq":     goosLinux,
		"GetsockoptICMPv6Filter": goosLinux,
		"Getsockname":            goosAny,
		"Getpeername":            goosAny,
		"Accept4":                goosLinux,
		"anyToSockaddr":          goosLinuxDarwin,
		// Recvfrom hands the kernel a MANAGED RawSockaddrAny by address in its generated form, which
		// the kernel overwrites -- the fifth instance of the kernel-writes-over-managed-array class,
		// and the first that kills the process (net.Interfaces() -> NetlinkRIB -> AccessViolation).
		// syscall/linux/sockaddr_linux_impl.cs answers it with the mirror's native image + typed decode.
		"Recvfrom": goosLinuxDarwin,
		// Sendto is Recvfrom's own direction reversed, and the same seam: `to.sockaddr()` hands back
		// the address of a MANAGED raw struct (`box.Value.raw`, whose `Addr`/`Zero` fields are
		// `array<byte>` object references, not inline bytes) and the generated body passes that
		// address straight to the kernel. The kernel only READS here, so this is not the
		// write-over-managed-memory class Recvfrom belongs to and it cannot smash the heap — it is
		// the LAYOUT half of the same wall, the one zsyscall_windows_impl.cs was hand-owned for:
		// the kernel reads object references where it expects a 4-byte address and a 2-byte port,
		// so the destination is garbage rather than the caller's. Bind/Connect are the template
		// (same direction, same helper), not Recvfrom's stack-buffer-then-decode: build the native
		// image with writeNativeSockaddr and hand ITS address to the package's own generated
		// `sendto`, which was never the broken part — its payload already travels by pinned
		// slice-element address and its errno handling stays where the converter put it.
		//
		// goosLinux like the rest of this file's set: writeNativeSockaddr is the linux flavor's
		// helper, and darwin/windows declare their own Sendto over their own layouts.
		"Sendto": goosLinuxDarwin,
		// The ANCILLARY pair, and the one member of this class whose defect announces itself with a
		// specific errno rather than as a fault or a wrong answer. Both hand the kernel a MANAGED
		// Msghdr whose Name/Iov/Control are `ж<T>` OBJECT REFERENCES, and recvmsgRaw is the
		// corrupting direction -- the kernel WRITES the sender's address and the control data
		// through two of them, which is Recvfrom's defect with two write targets.
		//
		// SendmsgN's tell was measured before either body was written (ScmRightsSeam, control-first):
		// `msg.Name = (ж<byte>)(uintptr)(ptr)` turns Go's NULL into `new NativeBox<byte>(0)`, an
		// OBJECT, so the kernel reads a non-NULL msg_name where Go passed none and a CONNECTED
		// socket answers EISCONN. A nil that stops being nil on the way to the kernel.
		//
		// The receive side displaces the RAW helper, which covers Recvmsg, recvmsgInet4 and
		// recvmsgInet6 with one body. The send side displaces the PUBLIC function, because
		// sendmsgN's pointer parameter is already a managed address with nothing faithful to
		// transcribe -- only SendmsgN still holds the typed Sockaddr that writeNativeSockaddr can
		// re-encode. sendmsgN / sendmsgNInet4 / sendmsgNInet6 stay auto for the sendtoInet4/6
		// reason: internal/poll reaches the //go:linkname copies, not these.
		"recvmsgRaw": goosLinuxDarwin,
		"SendmsgN":   goosLinuxDarwin,
		// The class's remaining LINUX members, closed PROACTIVELY on 2026-08-28 rather than when
		// reached — the one place this table's per-member-when-reached rule was deliberately not
		// followed, and the reason is measured. verifyheap on a crashed os/exec host found the
		// managed heap genuinely corrupt (6 errors in one contiguous ~0x180-byte run: a zeroed
		// method table, members that are text where pointers belong, a syncblock index of
		// 21,840,206), the smashed run held an `array<System.SByte>` enumerator, and the object
		// referencing into it was ManagedPointerTokens.s_table's own node array. That is what this
		// class's write actually is: the kernel does not merely produce a wrong ANSWER in the
		// caller's struct, it writes its bytes over MANAGED REFERENCES in a GC-tracked slot, and
		// the collector then follows them. A wrong answer is local and shows up in its own test; a
		// smashed reference is not, and surfaces as an unattributable crash somewhere else. So for
		// the write-into-struct members "no roster row reached it" is a statement about COVERAGE,
		// not about execution, and the deferral is unsafe on its own terms.
		//
		// Bodies in syscall/linux/structclass_linux_impl.cs, each the established remedy (blittable
		// mirror, size check at the boundary, field-for-field copy). Select and FcntlFlock are
		// two-way — the kernel reads the caller's image AND writes its answer back — and both also
		// BLOCK, which is the lifetime hazard wait4 was hand-owned for. All goosLinux: darwin
		// declares Select/Statfs/Fstatfs with its own layouts and non-defective bodies, and
		// Sysinfo/Adjtimex/FcntlFlock are Linux-only declarations.
		"Select":     goosLinux,
		"FcntlFlock": goosLinux,
		"Statfs":     goosLinux,
		"Fstatfs":    goosLinux,
		"Sysinfo":    goosLinux,
		"Adjtimex":   goosLinux,
		// The cgocaller keystone's one displaced wrapper (DESIGN-cgocaller-keystone.md §2,
		// increment 1). Setgroups is the only one of syscall_linux.go's nine credential setters
		// that passes a POINTER -- &a[0] into a []_Gid_t -- and the generated body hands that to
		// the cgo shim as `(uintptr)Ꮡ(a, 0)`. golib's uintptr operator DOES pin the storage, but
		// for the BOX's lifetime, and the call site passes the ADDRESS rather than the box.
		// MEASURED 2026-09-03 with a four-arm probe whose movement control fired first: keeping
		// only the uintptr, a compacting GC moved the array and the old address read zeroes;
		// keeping the box, the address was stable. So the slice is collectable during the libc
		// call and the argument must live in unmanaged memory for its duration -- the same seam
		// rule exec_unix.cs's Exec applies to argv/envp. cgocaller itself stays pointer-agnostic
		// (it takes uintptrs and cannot tell a pointer from an integer), so the marshalling
		// belongs at this one call site and nowhere else; the other eight setters pass scalars
		// and need no displacement at all. cgocaller_linux_impl.cs holds the body.
		"Setgroups": goosLinux,
		// The OVERLAPPED family — the SUBMIT SEAM of the managed netpoller arc
		// (docs/phase4/DESIGN-netpoll-managed-poller.md §4.3/§4.4/§4.5;
		// syscall/windows/zsyscall_windows_wsa_impl.cs carries the full write-up). Same
		// struct-passing class as the sockaddr members above, plus the dimension async adds: the
		// kernel keeps the OVERLAPPED and the buffer pointers until COMPLETION, and `&o.o` is an
		// interior field address inside a reference-bearing `operation`, which golib's address model
		// explicitly cannot hold still — a transient address with an UNBOUNDED window. The OVERLAPPED
		// is also the operation's kernel-side IDENTITY: execIO names the same one at three call sites
		// (submit, CancelIoEx, WSAGetOverlappedResult) and cancellation matches BY ADDRESS, so a
		// fresh native copy per call would break cancellation outright. The hand-owns key a
		// per-operation record off the ж<Overlapped> and own the native lifetime.
		//
		// GetAcceptExSockaddrs is here for a DIFFERENT reason and is the one member with no identity
		// of its own — no handle, no overlapped, just the caller's buffer, which under go2cs is an
		// unpinned reinterpret over a managed array. It consumes a goroutine-keyed handoff AcceptEx
		// parks; the coupling is documented at both ends (coordinator ruling 2026-08-14).
		"WSARecv":              goosWindows,
		"WSASend":              goosWindows,
		"AcceptEx":             goosWindows,
		"GetAcceptExSockaddrs": goosWindows,
		"CancelIoEx":           goosWindows,
		// WSARecvFrom joined on 2026-08-23, by exactly the rule the note below states: a suite
		// REACHED it. UdpLoopbackRoundTrip's read is the arrival, and the defect it exposed is the
		// same one AcceptEx already solves -- the kernel writing 116 bytes into a MANAGED
		// RawSockaddrAny that is forty bytes of object references. It is the third member of the
		// GetAcceptExSockaddrs/RawSockaddrAny.Sockaddr pair: stage native, transcribe at harvest
		// (netpoll design SS4.8, RATIFIED).
		"WSARecvFrom": goosWindows,
		// TransmitFile joined on 2026-08-28, by the same rule: net's sendfile family REACHED it.
		// TestSendfileParts and TestSendfileSeeked copy through io.CopyN, whose *io.LimitedReader is
		// not an io.WriterTo, so io.Copy takes the ReaderFrom branch into net.sendFile ->
		// internal/poll.SendFile -> here; TestSendfile passes *os.File directly and os.File.WriteTo
		// wins the WriterTo branch first (go.dev/issue/67042), which is why the family split
		// three-to-two and why the stop looked like a sendfile bug TestSendfile disproved. The
		// generated body created NO operation record, so the socket was never associated with the
		// CLR's completion port and the kernel's completion had nowhere to arrive: execIO's
		// pd.wait('w') blocked forever and beheaded net's whole alphabetical tail. It is also the one
		// member that READS the overlapped -- SendFile publishes the file offset in Offset/OffsetHigh
		// -- so the hand-own carries those onto the record's own control block.
		"TransmitFile": goosWindows,
		// WSASendto and its Inet4/Inet6 variants are the same machinery with more staging and remain
		// absent for the reason WSARecvFrom and TransmitFile no longer are: nothing on the TCP
		// listen/dial/accept/read/write path reaches them, and the board's ruling is to fix a
		// censused wrapper when a suite REACHES it. (The Inet4/Inet6 SENDERS are hand-owned in
		// internal/syscall/windows, where their linkname declarations live.)
		//
		// LoadConnectEx is NOT overlapped at all, and is here because the netpoll design recorded the
		// extension-pointer lookup as "synchronous and already working" and the crypto/tls census
		// MEASURED otherwise: nine loopback dials died in ~2 ms with "failed to find ConnectEx: An
		// invalid argument was supplied". Its single WSAIoctl is defective at both ends. IN:
		// `WSAID_CONNECTEX.Reinterpret<GUID, byte>()` — syscall.GUID holds `Data4 [8]byte` as a golib
		// array<byte> MANAGED REFERENCE, so the struct is reference-bearing, the reinterpret falls
		// back to an unpinned raw-address box, and the 16 bytes Windows compares are a CLR auto-layout
		// image with an object reference in them. Windows answers WSAEINVAL, every time, on every
		// host. OUT: the result address is an interior field of a struct holding an `error`, so it is
		// unpinnable and transient — even a successful call could write the function pointer into
		// memory the GC has moved. Both ends are fixed with the package's established stack-mirror
		// pattern; the GUID value still comes from the converted declaration.
		"LoadConnectEx": goosWindows,
		// The INITIALISATION pair, and the last two struct-passing members `net` reaches. Both run
		// inside internal/poll's InitWSA, once per process that imports `net`, one statement apart —
		// which is why they arrive together even though the chip named only the second.
		//
		// WSAEnumProtocols is the WORST OVERWRITE in the census by an order of magnitude.
		// WSAPROTOCOL_INFOW is 628 bytes native and ends in szProtocol[256] INLINE, with a GUID
		// ([8]byte) and a WSAPROTOCOLCHAIN ([7]uint32) nested inside it — three inline arrays the
		// conversion holds as three `array<T>` MANAGED REFERENCES, so the managed record is roughly
		// 120 bytes. checkSetFileCompletionNotificationModes asks for 32 of them and tells the kernel
		// so in BYTES: `len = unsafe.Sizeof(buf)`, which the converter answers with Go's 20,096 —
		// correct as a native size, and four fifths of it past the end of the ~3.8 KB managed array
		// the same call hands over. Compounding it, `Ꮡbuf.at<WSAProtocolInfo>(0)` cannot even be
		// pinned (PinnedBuffer.PinOnly refuses a reference-bearing element type), so the address was
		// transient as well as wrong. SIZE IS AN INPUT here exactly as it is for Process32First's
		// dwSize, with a second edge: the count Windows RETURNS interprets against native strides,
		// and on WSAENOBUFS it rewrites the caller's length with a required size that is a whole
		// number of NATIVE records.
		//
		// WSAStartup is the same class over WSADATA (~408 bytes native, szDescription[257] and
		// szSystemStatus[129] inline) and is here because it is UPSTREAM: the probe that reproduces
		// the enumeration defect could not reach it. `net` never reads the WSAData it passes, so the
		// overwrite has been silent since the corpus first dialled a socket — but a program that
		// reads Description dies with an ACCESS_VIOLATION in slice<byte>..ctor before
		// WSAEnumProtocols is called at all, the same signature GetTimeZoneInformation had. Silent is
		// not benign: this one runs in every converted program that imports `net`.
		//
		// Both answers are load-bearing beyond the corruption. useSetFileCompletionNotificationModes
		// is set only when EVERY enumerated entry carries XP1_IFS_HANDLES, and it decides
		// FD.skipSyncNotif — whether a synchronously-completing overlapped operation returns at once
		// or waits for a completion packet, which is the behaviour the netpoll design's OQ5 ratified
		// keeping. A corrupt enumeration silently picks the other IO path.
		"WSAStartup":       goosWindows,
		"WSAEnumProtocols": goosWindows,
		// The NAME-RESOLUTION pair, and the first member of this class whose OUTPUT is a LINKED
		// structure (zsyscall_windows_addrinfo_impl.cs carries the full write-up). Native ADDRINFOW
		// is 48 bytes of scalars and raw pointers where the converted AddrinfoW holds Canonname, Addr
		// and Next as MANAGED REFERENCES, so both directions are wrong: the hints Windows READS are
		// garbage, and the `*ADDRINFOW` it WRITES lands in a reference slot. Measured as a process
		// kill — `Fatal error. 0xC0000005` inside Syscall6 — with crypto/tls's TestVerifyHostname as
		// the first consumer; every converted program that resolves a name or a service reaches it
		// (net.Dial → resolveAddrList → LookupPort).
		//
		// Copying the top level alone would not be enough, which is what makes this one long: net
		// reads the sockaddr THROUGH the result (`(*RawSockaddrInet4)(unsafe.Pointer(result.Addr))`),
		// and RawSockaddrInet4's `Addr [4]byte` is an array<byte> BACKING REFERENCE — reading it out
		// of a native sockaddr_in is the fabricated-reference deref the sha3 arc named, which has no
		// general fix. So the whole chain is transcribed into managed records and the sockaddr with
		// it, carried across the `unsafe.Pointer` field by golib's ManagedPointerTokens (this is the
		// second minter of those tokens, and exactly the round trip they were written for).
		//
		// FreeAddrInfoW is hand-owned as a NO-OP for the same reason: the native chain is freed
		// eagerly at the copy, so nothing native escapes, and handing a managed object's address to
		// ws2_32's real free would release memory it does not own. GetAddrInfoW is the only producer
		// of a *AddrinfoW in Go's syscall package, so no caller can reach the free holding a chain
		// this file did not build.
		"GetAddrInfoW":  goosWindows,
		"FreeAddrInfoW": goosWindows,
		// The DNS RECORD pair, the same transcription shape one class over — and the member the
		// ptrout census deferred BY NAME ("it belongs to a `net` DNS arc"). Two defects meet here:
		// _DnsQuery's `qrs` is a `**DNSRecord` OUT-parameter, so the generated `(uintptr)Ꮡqrs`
		// handed DnsQuery_W a NULL ppQueryResults and every MX/NS/TXT/SRV/PTR/CNAME lookup answered
		// "no record"; and the pointee is a linked native chain whose payload structs carry MANAGED
		// references, so publishing the address alone — the ptrout remedy — would let net's
		// `Reinterpret<byte, DNSSRVData>` load eight raw bytes AS an object reference. Contained
		// wrong answer traded for a CLR type-safety break, which is why the fix is a PAIR: this
		// wrapper plus the hand-owned consumer at net/windows/lookup_windows.cs.
		//
		// DnsRecordListFree is hand-owned as a NO-OP for exactly FreeAddrInfoW's reason: the native
		// chain is freed eagerly at the copy, so net's `defer syscall.DnsRecordListFree(rec, 1)`
		// has nothing to do, and handing a managed object's address to dnsapi would release memory
		// it does not own. Only _DnsQuery is registered, not the exported DnsQuery — that wrapper
		// only converts the name to UTF-16 and delegates, so its generated body stays correct.
		"_DnsQuery":         goosWindows,
		"DnsRecordListFree": goosWindows,
		// The `**T` OUT-PARAMETER class — a SECOND syscall class, distinct from every member
		// above (zsyscall_windows_ptrout_impl.cs carries the full write-up). The members above
		// fail on a struct's LAYOUT; these fail with no struct in sight, because the argument is
		// one machine word and a golib pointer box has no such word to lend. `&p` for a Go
		// `var p *T` renders as ж<ж<T>>, whose storage is an OBJECT REFERENCE, and BOTH answers
		// golib's ж→uintptr operator can give are wrong: 0 while the held pointer is still null
		// (which is every out-parameter before the call), telling Windows "no output wanted" so
		// the call SUCCEEDS and the caller reads back its own nil; and a live MANAGED address
		// once it is not, which would have the kernel write a raw pointer over a slot the
		// collector reads as an object reference. Neither is fixable in the operator — no single
		// address is both kernel-writable as eight raw bytes and managed-readable as a ж<T> — so
		// the remedy is a native out-cell plus a publish at the one moment that can reconcile
		// them, which only the wrapper knows. The operator is deliberately unchanged.
		//
		// FIVE of the corpus's thirteen wrappers of this shape were taken first, on the standing
		// fix-it-when-a-suite-reaches-it rule plus the requirement that a VALUE-LEVEL guard can
		// prove each one ("it no longer returns nil" is exactly the evidence this class's history
		// says not to trust). The SID pair is a round trip through advapi32 over `**uint16` and
		// `**SID` (Go's empty struct — an opaque handle nothing reads through, so a native box is
		// not merely safe but exactly right); NetGetJoinInformation adds a third DLL and a
		// different free routine, which is what makes the guard evidence for a CLASS; and the two
		// crypt32 members are crypto/x509's measured consumer. All five are guarded by the
		// PointerOutParameter behavioral output test.
		//
		// NetUserGetInfo is the SIXTH, and it is taken on different evidence: os/user is the
		// consumer whose absence had kept it out, and its suite's FullName / PrimaryGroupID are the
		// value-level proof. It is NOT a publish-and-stop member — see the correction below.
		//
		// The seven left are left for stated reasons, not for lack of effort: DnsQuery/_DnsQuery
		// return a LINKED chain whose converted record holds managed references (the OTHER class,
		// wanting the ADDRINFOW treatment, in a `net` DNS arc); getQueuedCompletionStatus and its
		// exported sibling carry an OVERLAPPED whose identity the netpoll arc's per-operation
		// record owns; and GetFullPathName (plus internal/syscall/windows' CreateEnvironmentBlock)
		// are genuinely the same safe shape — a blittable `uint16` pointee read through directly —
		// with no corpus consumer, hence no available proof.
		//
		// CORRECTION, because this comment previously said the opposite: the two netapi32 members
		// NetUserGetInfo and NetUserGetLocalGroups were recorded here as "the same safe shape with
		// no corpus consumer". BOTH halves were wrong. os/user consumes them at three sites
		// (lookupFullNameServer, lookupUserPrimaryGroup, listGroupsForUsernameAndDomain), and the
		// shape is safe only until something reads THROUGH the published pointer — which all three
		// do, into records holding `ж<uint16>` fields. An out-parameter's own type (`**byte`) says
		// nothing about the record the caller then reads out of the buffer. Publishing the address
		// WITHOUT the matching transcription would turn today's contained wrong-read (a nil the
		// wrapper never filled in) into a fabricated managed reference, which is a CLR type-safety
		// break. That is why the wrapper and its call sites land in one change, and why
		// NetUserGetLocalGroups is not registered here yet: its own call site ships with it.
		"ConvertSidToStringSid": goosWindows,
		"ConvertStringSidToSid": goosWindows,
		"NetGetJoinInformation": goosWindows,
		"NetUserGetInfo":        goosWindows,
		// The two crypt32 members live in zsyscall_windows_certchain_impl.cs rather than the ptrout
		// file, together with the two CertFree* routines that pair with them — because for those two
		// the out-cell is only HALF the answer and the halves are NOT separable.
		// CertGetCertificateChain also hands crypt32 a CERT_CHAIN_PARA BY ADDRESS (the struct-passing
		// class: 80 native bytes read off a managed record whose UsageIdentifiers and CacheResync are
		// references, with dwUrlRetrievalTimeout — a blocking network budget — among the fields taken
		// from the wrong offset), and its `additionalStore` argument is `storeCtx.Store`, a field the
		// CALLER reads out of the CERT_CONTEXT the first of these produced. Fixing only the parameter
		// was MEASURED to leave the identical SEHException in place. So both publish a managed VIEW
		// that remembers its native identity in a syscall-local side table, and the two frees resolve
		// through it — crypt32 reference-counts that memory, so a no-op free (the FreeAddrInfoW
		// answer) would leak a chain per verification while a managed address would be handed to
		// crypt32's own allocator.
		//
		// CertCreateCertificateContext and CertEnumCertificatesInStore deliberately stay generated:
		// they produce plain native boxes nothing reads a field through, and the identity lookup falls
		// back to the box's own address, so they keep working unchanged.
		"CertAddCertificateContextToStore": goosWindows,
		"CertGetCertificateChain":          goosWindows,
		"CertFreeCertificateContext":       goosWindows,
		"CertFreeCertificateChain":         goosWindows,
		// The LAST crypt32 member on the system-verifier path, and the certchain file's third
		// concern in one wrapper: CERT_CHAIN_POLICY_PARA and CERT_CHAIN_POLICY_STATUS are the
		// struct-passing class in both directions at once (one read, one written, both
		// reference-bearing under conversion), pChainContext needs the native identity the view
		// remembers, and pvExtraPolicyPara is the OPAQUE-POINTER MINT — an unsafe.Pointer over an
		// SSL_EXTRA_CERT_CHAIN_POLICY_PARA whose ServerName is itself a pointer, carried across
		// the field by golib's ManagedPointerTokens (convCallExpr's opaquePointerMintEmission is
		// the minter; this wrapper is the resolver — the same round trip GetAddrInfoW's sockaddrs
		// take). Reached only when a chain is trusted AND the caller supplied a DNS name; the
		// SystemCertVerify behavioral test's policy rows are the offline guard.
		"CertVerifyCertificateChainPolicy": goosWindows,
	},
	// The SECOND package holding the syscall struct-passing class, and the one member of it whose
	// established remedy is measured UNREACHABLE — so this entry declares a CAPABILITY LIMIT rather
	// than repairing a layout. Coordinator ruling 2026-08-14; the mechanism, the three costed
	// remedies and the six same-shape wrappers behind this one are in
	// docs/phase4/BOARD-next-validation-candidates.md, "RETRACTED — `os`'s REGRESSION is a HOST
	// CAPABILITY, and the killer is SHARE_INFO_2".
	//
	// Why the blittable-mirror remedy cannot reach it: the wrapper never sees the struct. os's
	// TestNetworkSymbolicLink writes `(*byte)(unsafe.Pointer(&p))`, which converts to
	// `Ꮡp.Reinterpret<windows.SHARE_INFO_2, byte>()`, and Reinterpret correctly REFUSES to alias a
	// reference-bearing struct as byte — so it falls to `(ж<byte>)(uintptr)box` and NetShareAdd
	// receives a native-address box with the managed identity already gone. There is nothing left to
	// copy from, and recovering the struct by reading that raw address would fabricate managed
	// references out of it, which ж.PointerExtensions.cs names as a CLR type-safety break.
	//
	// What the hand-own buys: netapi32 dereferences shi2_path — which, under the CLR's
	// reference-first auto-layout of SHARE_INFO_2, holds the integer 1 — and the process DIES with
	// 0xC0000005 partway through os's suite, turning 683 measurable verdicts into an unknown
	// remainder. Failing BY NAME converts a whole-suite process death into ONE loud row.
	"internal/syscall/windows": {
		"NetShareAdd": goosWindows,
		// This package's member of the PTROUT class (`**byte` out-parameter), taken with its call
		// site rather than alone. os/user's listGroupsForUsernameAndDomain reads the returned
		// netapi32 array as a `ReadOnlySpan<LocalGroupUserInfo0>` laid directly over native
		// memory — and LocalGroupUserInfo0's single field is a `ж<uint16>`, a managed reference. A
		// Span<T> over native memory whose T carries a reference is a THIRD route to the same
		// fabrication the two Reinterpret sites take (see the "os/user" entry above), so
		// publishing a real address while that caller stayed as-is would replace today's contained
		// nil with a CLR type-safety break. Body in zsyscall_windows_ptrout_impl.cs, transcription
		// in os/user's lookup_windows_impl.cs, one change.
		"NetUserGetLocalGroups": goosWindows,
		// The privilege-adjustment member of the struct-passing class, and the one whose corruption
		// BLAMES THE HOST. Its generated body passes advapi32 the address of a managed
		// TOKEN_PRIVILEGES: native wants 16 bytes ending in one INLINE LUID_AND_ATTRIBUTES, and the
		// converted record is 24 whose privilege slot holds a golib `array<>` T[] REFERENCE, so the
		// kernel reads the two halves of a GC-heap address as the LUID and answers
		// ERROR_NOT_ALL_ASSIGNED. os's TestDirectorySymbolicLink then SKIPS with a message naming
		// SeCreateSymbolicLinkPrivilege -- on a box where Go's own suite grants it. Measurement
		// separated this from the rival root (a detached `&tp.Privileges[0].Luid`) by reading both
		// sides of the boundary: the MANAGED LUID is correct, only the native image is not. The
		// hand-own is internal/syscall/windows/windows/zsyscall_windows_privilege_impl.cs.
		"adjustTokenPrivileges": goosWindows,
		// The UDP SEND half of the datagram seam. Their generated bodies pass the kernel the address
		// `sockaddr()` returns -- a pointer into a MANAGED box -- which is the struct-passing class;
		// internal/syscall/windows/windows/net_windows_impl.cs writes a native stack image through the
		// mirror's seam instead. Registered when a suite REACHED them (the UdpLoopbackRoundTrip guard),
		// which is the board's own trigger for fixing a censused wrapper.
		"WSASendtoInet4": goosWindows,
		"WSASendtoInet6": goosWindows,
		// The HARVEST half of the netpoll submit seam, and the only member of it outside `syscall`.
		// execIO harvests by naming the SAME `&o.o` it submitted, but the operation's real control
		// block is the native OVERLAPPED syscall's record allocated — so this wrapper must call the
		// real WSAGetOverlappedResult against THAT address. It reads it through golib's GoAsyncIO,
		// which is the whole contract between the two packages: syscall cannot expose the record
		// (a public seam on a published package is a non-Go symbol) and this package cannot reach
		// into it. Deriving the result from the completion callback instead was rejected on a
		// measured fidelity ground: a callback's errorCode is a WIN32 code where execIO and net
		// branch on WSA ones (ERROR_NETNAME_DELETED vs WSAECONNRESET).
		//
		// ⚠ This package's generated csproj emits AllowUnsafeBlocks=false, so its hand-own carries
		// [module: go.GoRequiresUnsafe] and the csproj flipping to true is part of the intended
		// footprint — unlike `syscall`, which already emits true.
		"WSAGetOverlappedResult": goosWindows,
		// The SUBMIT half of the WriteMsg path, and the second defect on it. Its generated body hands
		// the kernel `uintptr(unsafe.Pointer(msg))` -- the address of the MANAGED WSAMsg, whose
		// syscall.Pointer class reference, ж<WSABuf> and inline reference-bearing Control land nowhere
		// near native WSAMSG's 56 bytes of pointers and lengths. With internal/poll's encoders fixed
		// the call stops corrupting the heap and starts REACHING Winsock, which rejects it: measured
		// `wsasendmsg: An invalid argument was supplied` on both families and both entry points. The
		// hand-own (windows/syscall_windows_impl.cs) builds the mirror in the operation's own staged
		// memory -- overlapped, so a stack image would be a use-after-return -- and flattens the
		// address through syscall's seam rather than learning the layout.
		"WSASendMsg": goosWindows,
		// The HARVEST twin -- the same defect in the opposite direction, which is exactly why it does
		// NOT take a mirrored remedy. A submit finishes its work before it returns; a receive cannot,
		// because the kernel fills the record AFTER the wrapper has returned. So the decode is carried
		// by the operation and run at completion through golib's GoAsyncIO.SetOperationCompletion --
		// syscall's WSARecvFrom is the same shape one package over, and this package's own
		// WSAGetOverlappedResult is where every asynchronous execIO exit runs it. Three things come
		// back that nothing else could supply: the sender ADDRESS (transcribed into the RawSockaddrAny
		// box internal/poll then decodes, through syscall's ONE native->managed transcription),
		// Control.Len (internal/poll's `oobn`) and Flags (its third return). Unblocks
		// ReadMsg/ReadMsgInet4/ReadMsgInet6 -- the whole Windows ReadMsgUDP/ReadMsgUDPAddrPort family.
		"WSARecvMsg": goosWindows,
		// The defect BENEATH WSASendMsg, and the one that fires first. WSASendMsg and WSARecvMsg are
		// Winsock EXTENSIONS with no export to link: their addresses are fetched at run time by
		// handing WSAIoctl a GUID, and Go does that with `(*byte)(unsafe.Pointer(&WSAID_WSASENDMSG))`.
		// syscall.GUID's converted form is NOT blittable -- `Data4 [8]byte` is an array<byte> MANAGED
		// REFERENCE where the native record has eight inline octets -- so ws2_32 reads sixteen bytes
		// that are not the GUID and answers WSAEINVAL. That is what the encode fix left behind: the
		// submission was never made, because the once-guarded lookup ahead of it had failed. Both
		// directions need it, so fixing it here is what a future WSARecvMsg hand-own inherits.
		"loadWSASendRecvMsg": goosWindows,
		// The MODULE-ENUMERATION member of the struct-passing class, and the one this package's own
		// hand-own header named and left for the suite that would reach it. runtime/pprof's suite
		// reached it: newProfileBuilder calls readMapping on EVERY profile it builds, so the two
		// tests that feed profileBuilder nothing but SYNTHETIC []uint64 records — proto_test.go's
		// TestConvertCPUProfileNoSamples and TestEmptyStack, which touch no profiler at all — die
		// before their first assertion, and so does every other test that builds a profile.
		//
		// The defect is the class's textbook shape, one record bigger than Process32First's.
		// MODULEENTRY32W is 1080 bytes ending in szModule[256] and szExePath[260] INLINE; the
		// converted ModuleEntry32 holds both as golib `array<uint16>` MANAGED REFERENCES, so the
		// CLR auto-layouts the record at roughly 64 bytes with the two references grouped. The
		// generated wrapper hands kernel32 `(uintptr)ᏑmoduleEntry` and the caller has already set
		// `module.Size = SizeofModuleEntry32` — which the converter folds from Go's `unsafe.Sizeof`
		// to the NATIVE 1080 — so Module32FirstW writes a full 1080-byte native record over that
		// ~64-byte managed object. About a kilobyte of GC heap past the box is overwritten, and the
		// ExePath field now holds module-path characters where an object reference belongs.
		//
		// It announces itself rather than lying: readMapping's very next statement is
		// `syscall.UTF16ToString(module.ExePath[:])`, and the measured death is an
		// ACCESS_VIOLATION inside `slice<ushort>..ctor` reached from `array<ushort>.get_Item(Range)`
		// — the same manifestation findFirstFile1 produced one package over, for the same reason.
		// (When the fabricated reference happens to resolve, the corruption instead surfaces
		// downstream: the census's ungated run died in `internal/testlog`'s class constructor,
		// reached from peBuildID two statements later, which reads as a defect in a package that
		// has nothing to do with it.)
		//
		// Remedy: the ordinary blittable mirror this class has taken four times already
		// (GetTimeZoneInformation, findFirstFile1, Process32First, adjustTokenPrivileges) — a
		// `fixed`-buffer native image on the stack, the SAME LazyProc call, and a field-for-field
		// copy back into the converted struct. It applies here where it could not apply to
		// NetShareAdd because both wrappers receive the record as a TYPED `*ModuleEntry32`, exactly
		// as this package's zsyscall_windows_impl.cs header predicted. Bodies in
		// zsyscall_windows_module_impl.cs.
		"Module32First": goosWindows,
		"Module32Next":  goosWindows,
	},
	// The three WORD-SIZE leaves of math/bits, and only those three. math/big calls Mul and Add from
	// the innermost loop of Montgomery multiplication -- every RSA private-key operation -- and the
	// converter emits each as a TWO-LEVEL body: a `UintSize == 32` branch, a nested call to the
	// 64-bit leaf, and a tuple materialised at each level. Go pays none of it; cmd/compile
	// intrinsifies bits.Mul64 to one MULQ at the call site (ssagen/ssa.go:5022) and aliases
	// math/big's own mulWW onto it (:5113).
	//
	// Two measured mechanisms live in that shape, and collapsing to ONE BCL call removes both:
	//   - `UintSize == 32` is a STRUCT comparison per call (2.72x). bits.cs:21 emits
	//     `public static UntypedInt UintSize => 64;`, whose operator== is Equals over a private
	//     Compare the JIT compiles standalone at IL 141 and never inlines.
	//   - IL size over the JIT's default inlining budget (1.32-1.42x): the two-level shape puts Mul
	//     at IL 83 and Add at IL 87, and DOTNET_JitDisasmSummary shows both compiled standalone.
	//
	// MEASURED, Release + DOTNET_TieredCompilation=0, one variable, records to distinct paths:
	// the addMulVVW loop 22.4 -> 2.76-2.89 ns/word (8.1x), and the RSA-2048 PSS signature
	// 68.5 -> 22.8 ms (3.0x, -67%), taking it from 82x Go to 27x. The loop figure sits on the
	// slice-bound floor, so what remains is golib's slice<T>, not the call.
	//
	// Mul64/Add64/Sub64 are deliberately NOT registered: they are not on this path, and a withdrawn
	// earlier cut registered exactly those (and not these three) for a measured ZERO -- it made the
	// inner body faster behind an outer wrapper the JIT still would not inline. Bodies in
	// core/math/bits/bits_impl.cs, where Add carries the one AggressiveInlining the measurement
	// showed it needs and Mul deliberately does not.
	"math/bits": {
		"Add": goosAny,
		"Mul": goosAny,
		"Sub": goosAny,
	},
}

// isManualType reports whether the named type (raw Go name) is hand-converted in this package.
func (v *Visitor) isManualType(goTypeName string) bool {
	if typeNames, ok := manualConversionTypes[v.pkg.Path()]; ok {
		return typeNames[goTypeName]
	}

	return false
}

// isManualBoxReceiverMethod reports whether obj is a listed foreign-receiver manual method
// (a manualConversionFuncs "recvTypeName.funcName" entry). The motivating members capture the
// receiver's IDENTITY (e.g. g.guintptr wraps the *g itself), so their manual implementation takes
// the receiver BOX (`this ж<T>`) and a deref-aliased call site must pass the box rather than the
// value alias (see convSelectorExpr).
//
// It answers MEMBERSHIP only. A registration displaces a BODY; it decides nothing about the
// declaration's receiver FORM, and the registry holds `[GoRecv] this ref T` members too — so the
// call site pairs this with exprHasReceiverBoxInScope, which asks whether a box exists to pass.
// Reading membership alone as "takes the box" put `Ꮡa.regAssign(Ꮡt, 0)` into reflect's addArg,
// a `this ref abiSeq a` body with no such box (CS0103, measured 2026-09-03).
func (v *Visitor) isManualBoxReceiverMethod(obj types.Object) bool {
	fn, ok := obj.(*types.Func)

	if !ok || fn.Pkg() == nil {
		return false
	}

	funcScopes, ok := manualConversionFuncs[resolveGorootVendoredPath(fn.Pkg().Path())]

	if !ok {
		return false
	}

	sig, ok := fn.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return false
	}

	recvType := sig.Recv().Type()

	if ptr, ok := recvType.(*types.Pointer); ok {
		recvType = ptr.Elem()
	}

	named, ok := recvType.(*types.Named)

	if !ok {
		return false
	}

	scope, listed := funcScopes[named.Obj().Name()+"."+fn.Name()]

	return listed && scope.includes(goosOfTarget(v.options.targetPlatform))
}

// isManualFuncDecl reports whether the function declaration is owned by a manual conversion:
// either any method whose receiver base type is a manual type, or an explicitly listed
// free function / foreign-receiver method.
func (v *Visitor) isManualFuncDecl(funcDecl *ast.FuncDecl) bool {
	return isManualFuncDeclInPackage(v.pkg.Path(), goosOfTarget(v.options.targetPlatform), funcDecl)
}

// isManualFuncDeclInPackage is isManualFuncDecl's package-path-keyed core, callable from the
// whole-package ANALYSIS passes that run before any Visitor exists (the hoisted-literal pre-pass
// must know which declarations emit only a placeholder comment — such a function never renders a
// FunctionPrefixMarker, so it can never carry a hoisted field declaration).
//
// `goos` is the conversion's TARGET operating system, which decides whether a scoped entry applies
// here at all — see goosScope. The analysis passes must pass the same value emission will, or a
// declaration would be hoisted-into and then emitted as a placeholder (or the reverse).
// The registry is keyed by the package's ON-DISK path — for a GOROOT-vendored package the `vendor/`-
// prefixed one, the key the dependency graph, the corpus directory and the ledger guards all use. The
// type-checker spells that package WITHOUT the prefix (`golang.org/x/crypto/internal/alias`, because
// conversionDriver loads a package by its directory and `go list` reports a vendored dir's ImportPath
// unprefixed — measured 2026-09-03), so every lookup canonicalizes through resolveGorootVendoredPath
// first; a registration under the on-disk key was otherwise never consulted (a two-seeded three-target
// diff of the first vendored registration read ZERO paths). Idempotent for an already-prefixed key and
// a no-op for every plain stdlib path.
func isManualFuncDeclInPackage(pkgPath string, goos string, funcDecl *ast.FuncDecl) bool {
	if funcDecl == nil || funcDecl.Name == nil {
		return false
	}

	pkgPath = resolveGorootVendoredPath(pkgPath)
	funcName := funcDecl.Name.Name
	recvName := ""

	if funcDecl.Recv != nil && len(funcDecl.Recv.List) > 0 {
		recvType := funcDecl.Recv.List[0].Type

		if starExpr, ok := recvType.(*ast.StarExpr); ok {
			recvType = starExpr.X
		}

		if ident, ok := recvType.(*ast.Ident); ok {
			recvName = ident.Name
		}
	}

	if recvName != "" {
		if typeNames, ok := manualConversionTypes[pkgPath]; ok && typeNames[recvName] {
			return true
		}
	}

	if funcScopes, ok := manualConversionFuncs[pkgPath]; ok {
		key := funcName

		if recvName != "" {
			key = recvName + "." + funcName
		}

		scope, listed := funcScopes[key]

		return listed && scope.includes(goos)
	}

	return false
}
