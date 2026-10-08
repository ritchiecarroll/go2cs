// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// Should be a built-in for unsafe.Pointer?
//
// add should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - fortio.org/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname add
//go:nosplit
internal static @unsafe.Pointer add(@unsafe.Pointer Δp, uintptr x) {
    return (@unsafe.Pointer)((uintptr)Δp + x);
}

// getg returns the pointer to the current g.
// The compiler rewrites calls to this function into instructions
// that fetch the g directly (from TLS or from the dedicated register).
internal static partial ж<g> getg();

// mcall switches from the g to the g0 stack and invokes fn(g),
// where g is the goroutine that made the call.
// mcall saves g's current PC/SP in g->sched so that it can be restored later.
// It is up to fn to arrange for that later execution, typically by recording
// g in a data structure, causing something to call ready(g) later.
// mcall returns to the original goroutine g later, when g has been rescheduled.
// fn must not return at all; typically it ends by calling schedule, to let the m
// run other goroutines.
//
// mcall can only be called from g stacks (not g0, not gsignal).
//
// This must NOT be go:noescape: if fn is a stack-allocated closure,
// fn puts g on a run queue, and g executes before fn returns, the
// closure will be invalidated while it is still executing.
internal static partial void mcall(Action<ж<g>> fn);

// systemstack runs fn on a system stack.
// If systemstack is called from the per-OS-thread (g0) stack, or
// if systemstack is called from the signal handling (gsignal) stack,
// systemstack calls fn directly and returns.
// Otherwise, systemstack is being called from the limited stack
// of an ordinary goroutine. In this case, systemstack switches
// to the per-OS-thread stack, calls fn, and switches back.
// It is common to use a func literal as the argument, in order
// to share inputs and outputs with the code around the call
// to system stack:
//
//	... set up y ...
//	systemstack(func() {
//		x = bigcall(y)
//	})
//	... use x ...
//
//go:noescape
internal static partial void systemstack(Action fn);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fatalSystemstackCalledˢ = "fatal: systemstack called from unexpected goroutine"u8;

//go:nosplit
//go:nowritebarrierrec
internal static void badsystemstack() {
    writeErrStr(fatalSystemstackCalledˢ);
}

// memclrNoHeapPointers clears n bytes starting at ptr.
//
// Usually you should use typedmemclr. memclrNoHeapPointers should be
// used only when the caller knows that *ptr contains no heap pointers
// because either:
//
// *ptr is initialized memory and its type is pointer-free, or
//
// *ptr is uninitialized memory (e.g., memory that's being reused
// for a new allocation) and hence contains only "junk".
//
// memclrNoHeapPointers ensures that if ptr is pointer-aligned, and n
// is a multiple of the pointer size, then any pointer-aligned,
// pointer-sized portion is cleared atomically. Despite the function
// name, this is necessary because this function is the underlying
// implementation of typedmemclr and memclrHasPointers. See the doc of
// memmove for more details.
//
// The (CPU-specific) implementations of this function are in memclr_*.s.
//
// memclrNoHeapPointers should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/chenzhuoyu/iasm
//   - github.com/dgraph-io/ristretto
//   - github.com/outcaste-io/ristretto
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname memclrNoHeapPointers
//go:noescape
internal static partial void memclrNoHeapPointers(@unsafe.Pointer ptr, uintptr n);

//go:linkname reflect_memclrNoHeapPointers reflect.memclrNoHeapPointers
internal static void reflect_memclrNoHeapPointers(@unsafe.Pointer ptr, uintptr n) {
    memclrNoHeapPointers(ptr, n);
}

// memmove copies n bytes from "from" to "to".
//
// memmove ensures that any pointer in "from" is written to "to" with
// an indivisible write, so that racy reads cannot observe a
// half-written pointer. This is necessary to prevent the garbage
// collector from observing invalid pointers, and differs from memmove
// in unmanaged languages. However, memmove is only required to do
// this if "from" and "to" may contain pointers, which can only be the
// case if "from", "to", and "n" are all be word-aligned.
//
// Implementations are in memmove_*.s.
//
// Outside assembly calls memmove.
//
// memmove should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//   - github.com/cloudwego/dynamicgo
//   - github.com/ebitengine/purego
//   - github.com/tetratelabs/wazero
//   - github.com/ugorji/go/codec
//   - gvisor.dev/gvisor
//   - github.com/sagernet/gvisor
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname memmove
//go:noescape
internal static partial void memmove(@unsafe.Pointer to, @unsafe.Pointer from, uintptr n);

//go:linkname reflect_memmove reflect.memmove
internal static void reflect_memmove(@unsafe.Pointer to, @unsafe.Pointer from, uintptr n) {
    memmove(to, from, n);
}

// exported value for testing
internal const float32 hashLoad = /* float32(loadFactorNum) / float32(loadFactorDen) */ 0.875f;

// in internal/bytealg/equal_*.s
//
// memequal should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname memequal
//go:noescape
internal static partial bool memequal(@unsafe.Pointer a, @unsafe.Pointer b, uintptr size);

// noescape hides a pointer from escape analysis.  noescape is
// the identity function but escape analysis doesn't think the
// output depends on the input.  noescape is inlined and currently
// compiles down to zero instructions.
// USE CAREFULLY!
//
// noescape should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/ebitengine/purego
//   - github.com/hamba/avro/v2
//   - github.com/puzpuzpuz/xsync/v3
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname noescape
//go:nosplit
internal static @unsafe.Pointer noescape(@unsafe.Pointer Δp) {
    var x = (uintptr)Δp;
    return (@unsafe.Pointer)((uintptr)(x ^ 0));
}

// noEscapePtr hides a pointer from escape analysis. See noescape.
// USE CAREFULLY!
//
//go:nosplit
internal static ж<T> noEscapePtr<T>(ж<T> Ꮡp) {
    var x = (uintptr)Ꮡp;
    return (ж<T>)(uintptr)((@unsafe.Pointer)((uintptr)(x ^ 0)));
}

// Not all cgocallback frames are actually cgocallback,
// so not all have these arguments. Mark them uintptr so that the GC
// does not misinterpret memory when the arguments are not present.
// cgocallback is not called from Go, only from crosscall2.
// This in turn calls cgocallbackg, which is where we'll find
// pointer-declared arguments.
//
// When fn is nil (frame is saved g), call dropm instead,
// this is used when the C thread is exiting.
internal static partial void cgocallback(uintptr fn, uintptr frame, uintptr ctxt);

internal static partial void gogo(ж<gobuf> buf);

internal static partial void asminit();

internal static partial void setg(ж<g> gg);

internal static partial void breakpoint();

// reflectcall calls fn with arguments described by stackArgs, stackArgsSize,
// frameSize, and regArgs.
//
// Arguments passed on the stack and space for return values passed on the stack
// must be laid out at the space pointed to by stackArgs (with total length
// stackArgsSize) according to the ABI.
//
// stackRetOffset must be some value <= stackArgsSize that indicates the
// offset within stackArgs where the return value space begins.
//
// frameSize is the total size of the argument frame at stackArgs and must
// therefore be >= stackArgsSize. It must include additional space for spilling
// register arguments for stack growth and preemption.
//
// TODO(mknyszek): Once we don't need the additional spill space, remove frameSize,
// since frameSize will be redundant with stackArgsSize.
//
// Arguments passed in registers must be laid out in regArgs according to the ABI.
// regArgs will hold any return values passed in registers after the call.
//
// reflectcall copies stack arguments from stackArgs to the goroutine stack, and
// then copies back stackArgsSize-stackRetOffset bytes back to the return space
// in stackArgs once fn has completed. It also "unspills" argument registers from
// regArgs before calling fn, and spills them back into regArgs immediately
// following the call to fn. If there are results being returned on the stack,
// the caller should pass the argument frame type as stackArgsType so that
// reflectcall can execute appropriate write barriers during the copy.
//
// reflectcall expects regArgs.ReturnIsPtr to be populated indicating which
// registers on the return path will contain Go pointers. It will then store
// these pointers in regArgs.Ptrs such that they are visible to the GC.
//
// Package reflect passes a frame type. In package runtime, there is only
// one call that copies results back, in callbackWrap in syscall_windows.go, and it
// does NOT pass a frame type, meaning there are no write barriers invoked. See that
// call site for justification.
//
// Package reflect accesses this symbol through a linkname.
//
// Arguments passed through to reflectcall do not escape. The type is used
// only in a very limited callee of reflectcall, the stackArgs are copied, and
// regArgs is only used in the reflectcall frame.
//
//go:noescape
internal static partial void reflectcall(ж<_type> stackArgsType, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

// procyield should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing-tun
//   - github.com/slackhq/nebula
//   - golang.zx2c4.com/wireguard
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname procyield
internal static partial void procyield(uint32 cycles);

partial struct neverCallThisFunction {
}

// goexit is the return stub at the top of every goroutine call stack.
// Each goroutine stack is constructed as if goexit called the
// goroutine's entry point function, so that when the entry point
// function returns, it will return to goexit, which will call goexit1
// to perform the actual exit.
//
// This function must never be called directly. Call goexit1 instead.
// gentraceback assumes that goexit terminates the stack. A direct
// call on the stack will cause gentraceback to stop walking the stack
// prematurely and if there is leftover state it may panic.
internal static partial void goexit(neverCallThisFunction _);

// publicationBarrier performs a store/store barrier (a "publication"
// or "export" barrier). Some form of synchronization is required
// between initializing an object and making that object accessible to
// another processor. Without synchronization, the initialization
// writes and the "publication" write may be reordered, allowing the
// other processor to follow the pointer and observe an uninitialized
// object. In general, higher-level synchronization should be used,
// such as locking or an atomic pointer write. publicationBarrier is
// for when those aren't an option, such as in the implementation of
// the memory manager.
//
// There's no corresponding barrier for the read side because the read
// side naturally has a data dependency order. All architectures that
// Go supports or seems likely to ever support automatically enforce
// data dependency ordering.
internal static partial void publicationBarrier();

//go:noescape
internal static partial int32 asmcgocall(@unsafe.Pointer fn, @unsafe.Pointer arg);

internal static partial void morestack();

// morestack_noctxt should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issues/67401.
// See go.dev/issues/71672.
//
//go:linkname morestack_noctxt
internal static partial void morestack_noctxt();

internal static partial void rt0_go();

// return0 is a stub used to return 0 from deferproc.
// It is called at the very end of deferproc to signal
// the calling Go function that it should not jump
// to deferreturn.
// in asm_*.s
internal static partial void return0();

// in asm_*.s
// not called directly; definitions here supply type information for traceback.
// These must have the same signature (arg pointer map) as reflectcall.
internal static partial void call16(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call32(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call64(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call128(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call256(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call512(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call1024(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call2048(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call4096(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call8192(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call16384(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call32768(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call65536(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call131072(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call262144(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call524288(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call1048576(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call2097152(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call4194304(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call8388608(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call16777216(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call33554432(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call67108864(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call134217728(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call268435456(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call536870912(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void call1073741824(@unsafe.Pointer typ, @unsafe.Pointer fn, @unsafe.Pointer stackArgs, uint32 stackArgsSize, uint32 stackRetOffset, uint32 frameSize, ж<abi.RegArgs> regArgs);

internal static partial void systemstack_switch();

// alignUp rounds n up to a multiple of a. a must be a power of 2.
//
//go:nosplit
internal static uintptr alignUp(uintptr n, uintptr a) {
    return (uintptr)((n + a - 1) & ~(a - 1));
}

// alignDown rounds n down to a multiple of a. a must be a power of 2.
//
//go:nosplit
internal static uintptr alignDown(uintptr n, uintptr a) {
    return (uintptr)(n & ~(a - 1));
}

// divRoundUp returns ceil(n / a).
//
//go:nosplit
internal static uintptr divRoundUp(uintptr n, uintptr a) {
    // a is generally a power of two. This will get inlined and
    // the compiler will optimize the division.
    return (n + a - 1) / a;
}

// checkASM reports whether assembly runtime checks have passed.
internal static partial bool checkASM();

internal static partial bool memequal_varlen(@unsafe.Pointer a, @unsafe.Pointer b);

// bool2int returns 0 if x is false or 1 if x is true.
internal static nint bool2int(bool xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    // Avoid branches. In the SSA compiler, this compiles to
    // exactly what you would want it to.
    return (nint)(~Ꮡx.Reinterpret<bool, uint8>());
}

// abort crashes the runtime in situations where even throw might not
// work. In general it should do something a debugger will recognize
// (e.g., an INT3 on x86). A crash in abort is recognized by the
// signal handler, which will attempt to tear down the runtime
// immediately.
internal static partial void abort();

// Called from compiled code; declared for vet; do NOT call from Go.
internal static partial void gcWriteBarrier1();

// gcWriteBarrier2 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname gcWriteBarrier2
internal static partial void gcWriteBarrier2();

internal static partial void gcWriteBarrier3();

internal static partial void gcWriteBarrier4();

internal static partial void gcWriteBarrier5();

internal static partial void gcWriteBarrier6();

internal static partial void gcWriteBarrier7();

internal static partial void gcWriteBarrier8();

internal static partial void duffzero();

internal static partial void duffcopy();

// Called from linker-generated .initarray; declared for go vet; do NOT call from Go.
internal static partial void addmoduledata();

// Injected by the signal handler for panicking signals.
// Initializes any registers that have fixed meaning at calls but
// are scratch in bodies and calls sigpanic.
// On many platforms it just jumps to sigpanic.
internal static partial void sigpanic0();

// intArgRegs is used by the various register assignment
// algorithm implementations in the runtime. These include:.
// - Finalizers (mfinal.go)
// - Windows callbacks (syscall_windows.go)
//
// Both are stripped-down versions of the algorithm since they
// only have to deal with a subset of cases (finalizers only
// take a pointer or interface argument, Go Windows callbacks
// don't support floating point).
//
// It should be modified with care and are generally only
// modified when testing this package.
//
// It should never be set higher than its internal/abi
// constant counterparts, because the system relies on a
// structure that is at least large enough to hold the
// registers the system supports.
//
// Protected by finlock.
internal static nint intArgRegs = abi.IntArgRegs;

} // end runtime_package
