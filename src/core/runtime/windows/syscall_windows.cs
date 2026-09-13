// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class runtime_package {

// cbs stores all registered Go callbacks.

[GoType("dyn")] partial struct cbsᴛ1 {
    internal mutex @lock; // use cbsLock / cbsUnlock for race instrumentation.
    internal array<winCallback> ctxt = new(cb_max);
    internal map<winCallbackKey, nint> index;
    internal nint n;
}
internal static ж<cbsᴛ1> Ꮡcbs = new StandardBox<cbsᴛ1>(new cbsᴛ1());
internal static ref cbsᴛ1 cbs => ref Ꮡcbs.Value;

internal static void cbsLock() {
    @lock(Ꮡcbs.of(cbsᴛ1.Ꮡlock));
    // compileCallback is used by goenvs prior to completion of schedinit.
    // raceacquire involves a racecallback to get the proc, which is not
    // safe prior to scheduler initialization. Thus avoid instrumentation
    // until then.
    if (raceenabled && mainStarted) {
        raceacquire(new @unsafe.Pointer(Ꮡcbs.of(cbsᴛ1.Ꮡlock)));
    }
}

internal static void cbsUnlock() {
    if (raceenabled && mainStarted) {
        racerelease(new @unsafe.Pointer(Ꮡcbs.of(cbsᴛ1.Ꮡlock)));
    }
    unlock(Ꮡcbs.of(cbsᴛ1.Ꮡlock));
}

// winCallback records information about a registered Go callback.
[GoType] partial struct winCallback {
    internal ж<funcval> fn; // Go function
    internal uintptr retPop;  // For 386 cdecl, how many bytes to pop on return
    internal abiDesc abiMap;
}

[GoType("num:nint")] partial struct abiPartKind;

internal static abiPartKind abiPartBad => /* iota */ 0;
internal static abiPartKind abiPartStack => 1; // Move a value from memory to the stack.
internal static abiPartKind abiPartReg => 2; // Move a value from memory to a register.

// abiPart encodes a step in translating between calling ABIs.
[GoType] partial struct abiPart {
    internal abiPartKind kind;
    internal uintptr srcStackOffset;
    internal uintptr dstStackOffset; // used if kind == abiPartStack
    internal nint dstRegister;    // used if kind == abiPartReg
    internal uintptr len;
}

[GoRecv] internal static bool tryMerge(this ref abiPart a, abiPart b) {
    if (a.kind != abiPartStack || b.kind != abiPartStack) {
        return false;
    }
    if (a.srcStackOffset + a.len == b.srcStackOffset && a.dstStackOffset + a.len == b.dstStackOffset) {
        a.len += b.len;
        return true;
    }
    return false;
}

// abiDesc specifies how to translate from a C frame to a Go
// frame. This does not specify how to translate back because
// the result is always a uintptr. If the C ABI is fastcall,
// this assumes the four fastcall registers were first spilled
// to the shadow space.
[GoType] partial struct abiDesc {
    internal slice<abiPart> parts;
    internal uintptr srcStackSize; // stdcall/fastcall stack space tracking
    internal uintptr dstStackSize; // Go stack space used
    internal uintptr dstSpill; // Extra stack space for argument spill slots
    internal nint dstRegisters;    // Go ABI int argument registers used
    // retOffset is the offset of the uintptr-sized result in the Go
    // frame.
    internal uintptr retOffset;
}

[GoRecv] internal static void assignArg(this ref abiDesc Δp, ж<_type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Size_ > goarch.PtrSize) {
        // We don't support this right now. In
        // stdcall/cdecl, 64-bit ints and doubles are
        // passed as two words (little endian); and
        // structs are pushed on the stack. In
        // fastcall, arguments larger than the word
        // size are passed by reference. On arm,
        // 8-byte aligned arguments round up to the
        // next even register and can be split across
        // registers and the stack.
        throw panic("compileCallback: argument size is larger than uintptr");
    }
    {
        var k = (abiꓸKind)(t.Kind_ & abi.KindMask); if (GOARCH != "386"u8 && (k == abi.Float32 || k == abi.Float64)) {
            // In fastcall, floating-point arguments in
            // the first four positions are passed in
            // floating-point registers, which we don't
            // currently spill. arm passes floating-point
            // arguments in VFP registers, which we also
            // don't support.
            // So basically we only support 386.
            throw panic("compileCallback: float arguments not supported");
        }
    }
    if (t.Size_ == 0) {
        // The Go ABI aligns for zero-sized types.
        Δp.dstStackSize = alignUp(Δp.dstStackSize, (uintptr)t.Align_);
        return;
    }
    // In the C ABI, we're already on a word boundary.
    // Also, sub-word-sized fastcall register arguments
    // are stored to the least-significant bytes of the
    // argument word and all supported Windows
    // architectures are little endian, so srcStackOffset
    // is already pointing to the right place for smaller
    // arguments. The same is true on arm.
    var oldParts = Δp.parts;
    if (Δp.tryRegAssignArg(Ꮡt, 0)){
        // Account for spill space.
        //
        // TODO(mknyszek): Remove this when we no longer have
        // caller reserved spill space.
        Δp.dstSpill = alignUp(Δp.dstSpill, (uintptr)t.Align_);
        Δp.dstSpill += t.Size_;
    } else {
        // Register assignment failed.
        // Undo the work and stack assign.
        Δp.parts = oldParts;
        // The Go ABI aligns arguments.
        Δp.dstStackSize = alignUp(Δp.dstStackSize, (uintptr)t.Align_);
        // Copy just the size of the argument. Note that this
        // could be a small by-value struct, but C and Go
        // struct layouts are compatible, so we can copy these
        // directly, too.
        var part = new abiPart(
            kind: abiPartStack,
            srcStackOffset: Δp.srcStackSize,
            dstStackOffset: Δp.dstStackSize,
            len: t.Size_
        );
        // Add this step to the adapter.
        if (len(Δp.parts) == 0 || !Δp.parts[len(Δp.parts) - 1].tryMerge(part)) {
            Δp.parts = append(Δp.parts, part);
        }
        // The Go ABI packs arguments.
        Δp.dstStackSize += t.Size_;
    }
    // cdecl, stdcall, fastcall, and arm pad arguments to word size.
    // TODO(rsc): On arm and arm64 do we need to skip the caller's saved LR?
    Δp.srcStackSize += goarch.PtrSize;
}

// tryRegAssignArg tries to register-assign a value of type t.
// If this type is nested in an aggregate type, then offset is the
// offset of this type within its parent type.
// Assumes t.size <= goarch.PtrSize and t.size != 0.
//
// Returns whether the assignment succeeded.
[GoRecv] internal static bool tryRegAssignArg(this ref abiDesc Δp, ж<_type> Ꮡt, uintptr offset) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var k = (abiꓸKind)(t.Kind_ & abi.KindMask);
        var exprᴛ1 = k;
        if (exprᴛ1 == abi.Bool || exprᴛ1 == abi.Int || exprᴛ1 == abi.Int8 || exprᴛ1 == abi.Int16 || exprᴛ1 == abi.Int32 || exprᴛ1 == abi.Uint || exprᴛ1 == abi.Uint8 || exprᴛ1 == abi.Uint16 || exprᴛ1 == abi.Uint32 || exprᴛ1 == abi.Uintptr || exprᴛ1 == abi.Pointer || exprᴛ1 == abi.UnsafePointer) {
            return Δp.assignReg(t.Size_, // Assign a register for all these types.
 offset);
        }
        if (exprᴛ1 == abi.Int64 || exprᴛ1 == abi.Uint64) {
            if (goarch.PtrSize == 8) {
                // Only register-assign if the registers are big enough.
                return Δp.assignReg(t.Size_, offset);
            }
        }
        else if (exprᴛ1 == abi.Array) {
            var at = Ꮡt.Reinterpret<_type, arraytype>();
            if ((~at).Len == 1) {
                return Δp.tryRegAssignArg((~at).Elem, offset); // TODO fix when runtime is fully commoned up w/ abi.Type
            }
        }
        else if (exprᴛ1 == abi.Struct) {
            var st = Ꮡt.Reinterpret<_type, structtype>();
            foreach (var (i, _) in (~st).Fields) {
                var f = Ꮡ((~st).Fields, i);
                if (!Δp.tryRegAssignArg((~f).Typ, offset + (~f).Offset)) {
                    return false;
                }
            }
            return true;
        }
    }

    // Pointer-sized types such as maps and channels are currently
    // not supported.
    throw panic("compileCallback: type " + toRType(Ꮡt).@string() + " is currently not supported for use in system callbacks");
}

// assignReg attempts to assign a single register for an
// argument with the given size, at the given offset into the
// value in the C ABI space.
//
// Returns whether the assignment was successful.
[GoRecv] internal static bool assignReg(this ref abiDesc Δp, uintptr size, uintptr offset) {
    if (Δp.dstRegisters >= intArgRegs) {
        return false;
    }
    Δp.parts = append(Δp.parts, new abiPart(
        kind: abiPartReg,
        srcStackOffset: Δp.srcStackSize + offset,
        dstRegister: Δp.dstRegisters,
        len: size
    ));
    Δp.dstRegisters++;
    return true;
}

[GoType] partial struct winCallbackKey {
    internal ж<funcval> fn;
    internal bool cdecl;
}

internal static partial void callbackasm();

// callbackasmAddr returns address of runtime.callbackasm
// function adjusted by i.
// On x86 and amd64, runtime.callbackasm is a series of CALL instructions,
// and we want callback to arrive at
// correspondent call instruction instead of start of
// runtime.callbackasm.
// On ARM, runtime.callbackasm is a series of mov and branch instructions.
// R12 is loaded with the callback index. Each entry is two instructions,
// hence 8 bytes.
internal static uintptr callbackasmAddr(nint i) {
    nint entrySize = default!;
    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "386"u8 || exprᴛ1 == "amd64"u8) {
        entrySize = 5;
    }
    else if (exprᴛ1 == "arm"u8 || exprᴛ1 == "arm64"u8) {
        entrySize = 8;
    }
    else { /* default: */
        throw panic("unsupported architecture");
    }

    // On ARM and ARM64, each entry is a MOV instruction
    // followed by a branch instruction
    return abi.FuncPCABI0(callbackasm) + (uintptr)(i * entrySize);
}

internal static UntypedInt callbackMaxFrame => /* 64 * goarch.PtrSize */ 512;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tooManyCallbackFunctionsˢ = "too many callback functions"u8;

// compileCallback converts a Go function fn into a C function pointer
// that can be passed to Windows APIs.
//
// On 386, if cdecl is true, the returned C function will use the
// cdecl calling convention; otherwise, it will use stdcall. On amd64,
// it always uses fastcall. On arm, it always uses the ARM convention.
//
//go:linkname compileCallback syscall.compileCallback
internal static uintptr /*code*/ compileCallback(eface fn, bool cdecl) {
    if (GOARCH != "386"u8) {
        // cdecl is only meaningful on 386.
        cdecl = false;
    }
    if (fn._type == nil || ((abiꓸKind)((~fn._type).Kind_ & abi.KindMask)) != abi.Func) {
        throw panic("compileCallback: expected function with one uintptr-sized result");
    }
    var ft = fn._type.FuncType();
    // Check arguments and construct ABI translation.
    abiDesc abiMap = default!;
    foreach (var (_, t) in ft.InSlice()) {
        abiMap.assignArg(t);
    }
    // The Go ABI aligns the result to the word size. src is
    // already aligned.
    abiMap.dstStackSize = alignUp(abiMap.dstStackSize, goarch.PtrSize);
    abiMap.retOffset = abiMap.dstStackSize;
    if (len(ft.OutSlice()) != 1) {
        throw panic("compileCallback: expected function with one uintptr-sized result");
    }
    if ((~ft.OutSlice()[0]).Size_ != goarch.PtrSize) {
        throw panic("compileCallback: expected function with one uintptr-sized result");
    }
    {
        var k = (abiꓸKind)((~ft.OutSlice()[0]).Kind_ & abi.KindMask); if (k == abi.Float32 || k == abi.Float64) {
            // In cdecl and stdcall, float results are returned in
            // ST(0). In fastcall, they're returned in XMM0.
            // Either way, it's not AX.
            throw panic("compileCallback: float results not supported");
        }
    }
    if (intArgRegs == 0) {
        // Make room for the uintptr-sized result.
        // If there are argument registers, the return value will
        // be passed in the first register.
        abiMap.dstStackSize += goarch.PtrSize;
    }
    // TODO(mknyszek): Remove dstSpill from this calculation when we no longer have
    // caller reserved spill space.
    var frameSize = alignUp(abiMap.dstStackSize, goarch.PtrSize);
    frameSize += abiMap.dstSpill;
    if (frameSize > callbackMaxFrame) {
        throw panic("compileCallback: function argument frame too large");
    }
    // For cdecl, the callee is responsible for popping its
    // arguments from the C stack.
    uintptr retPop = default!;
    if (cdecl) {
        retPop = abiMap.srcStackSize;
    }
    var key = new winCallbackKey((ж<funcval>)(uintptr)(fn.data), cdecl);
    cbsLock();
    // Check if this callback is already registered.
    {
        var (nΔ1, ok) = cbs.index[key, ꟷ]; if (ok) {
            cbsUnlock();
            return callbackasmAddr(nΔ1);
        }
    }
    // Register the callback.
    if (cbs.index == default!) {
        cbs.index = new map<winCallbackKey, nint>();
    }
    nint n = cbs.n;
    if (n >= len(cbs.ctxt)) {
        cbsUnlock();
        @throw(tooManyCallbackFunctionsˢ);
    }
    var c = new winCallback(key.fn, retPop, abiMap);
    cbs.ctxt[n] = c;
    cbs.index[key] = n;
    cbs.n++;
    cbsUnlock();
    return callbackasmAddr(n);
}

[GoType] partial struct callbackArgs {
    internal uintptr index;
    // args points to the argument block.
    //
    // For cdecl and stdcall, all arguments are on the stack.
    //
    // For fastcall, the trampoline spills register arguments to
    // the reserved spill slots below the stack arguments,
    // resulting in a layout equivalent to stdcall.
    //
    // For arm, the trampoline stores the register arguments just
    // below the stack arguments, so again we can treat it as one
    // big stack arguments frame.
    internal @unsafe.Pointer args;
    // Below are out-args from callbackWrap
    internal uintptr result;
    internal uintptr retPop; // For 386 cdecl, how many bytes to pop on return
}

// callbackWrap is called by callbackasm to invoke a registered C callback.
internal static void callbackWrap(ref callbackArgs a) {
    var c = cbs.ctxt[(nint)(a.index)];
    a.retPop = c.retPop;
    // Convert from C to Go ABI.
    ref var regs = ref heap(new abi.RegArgs(), out var Ꮡregs);
    ref var frame = ref heap(new array<byte>(512), out var Ꮡframe);
    @unsafe.Pointer goArgs = new @unsafe.Pointer(Ꮡframe);
    foreach (var (_, part) in c.abiMap.parts) {
        var exprᴛ1 = part.kind;
        if (exprᴛ1 == abiPartStack) {
            memmove((uintptr)add(goArgs, part.dstStackOffset), (uintptr)add(a.args, part.srcStackOffset), part.len);
        }
        else if (exprᴛ1 == abiPartReg) {
            @unsafe.Pointer goReg = @unsafe.Pointer.FromBox(Ꮡregs.at(abi.RegArgs.ᏑInts, part.dstRegister));
            memmove(goReg, (uintptr)add(a.args, part.srcStackOffset), part.len);
        }
        else { /* default: */
            throw panic("bad ABI description");
        }

    }
    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    var frameSize = alignUp(c.abiMap.dstStackSize, goarch.PtrSize);
    frameSize += c.abiMap.dstSpill;
    // Even though this is copying back results, we can pass a nil
    // type because those results must not require write barriers.
    reflectcall(nil, new @unsafe.Pointer(c.fn), (uintptr)noescape(goArgs), (uint32)c.abiMap.dstStackSize, (uint32)c.abiMap.retOffset, (uint32)frameSize, Ꮡregs);
    // Extract the result.
    //
    // There's always exactly one return value, one pointer in size.
    // If it's on the stack, then we will have reserved space for it
    // at the end of the frame, otherwise it was passed in a register.
    if (c.abiMap.dstStackSize != c.abiMap.retOffset){
        a.result = ~Ꮡframe.at<byte>((nint)(c.abiMap.retOffset)).Reinterpret<byte, uintptr>();
    } else {
        nint zero = default!;
        // On architectures with no registers, Ints[0] would be a compile error,
        // so we use a dynamic index. These architectures will never take this
        // branch, so this won't cause a runtime panic.
        a.result = regs.Ints[zero];
    }
}

internal static UntypedInt _LOAD_LIBRARY_SEARCH_SYSTEM32 => 0x00000800;

//go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
internal static (uintptr handle, uintptr err) syscall_loadsystemlibrary(ж<uint16> Ꮡfilename) {
    uintptr handle = default!;
    uintptr err = default!;

    (handle, _, err) = syscall_SyscallN((uintptr)(@unsafe.Pointer)_LoadLibraryExW, (uintptr)Ꮡfilename, 0, _LOAD_LIBRARY_SEARCH_SYSTEM32);
    KeepAlive(Ꮡfilename.OrTypedNil());
    if (handle != 0) {
        err = 0;
    }
    return (handle, err);
}

// golang.org/x/sys linknames syscall.loadlibrary
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_loadlibrary syscall.loadlibrary
internal static (uintptr handle, uintptr err) syscall_loadlibrary(ж<uint16> Ꮡfilename) {
    uintptr handle = default!;
    uintptr err = default!;

    (handle, _, err) = syscall_SyscallN((uintptr)(@unsafe.Pointer)_LoadLibraryW, (uintptr)Ꮡfilename);
    KeepAlive(Ꮡfilename.OrTypedNil());
    if (handle != 0) {
        err = 0;
    }
    return (handle, err);
}

// golang.org/x/sys linknames syscall.getprocaddress
// (in addition to standard package syscall).
// Do not remove or change the type signature.
//
//go:linkname syscall_getprocaddress syscall.getprocaddress
internal static (uintptr outhandle, uintptr err) syscall_getprocaddress(uintptr handle, ж<byte> Ꮡprocname) {
    uintptr outhandle = default!;
    uintptr err = default!;

    (outhandle, _, err) = syscall_SyscallN((uintptr)(@unsafe.Pointer)_GetProcAddress, handle, (uintptr)Ꮡprocname);
    KeepAlive(Ꮡprocname.OrTypedNil());
    if (outhandle != 0) {
        err = 0;
    }
    return (outhandle, err);
}

//go:linkname syscall_Syscall syscall.Syscall
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_Syscall(uintptr fn, uintptr nargs, uintptr a1, uintptr a2, uintptr a3) {
    return syscall_syscalln(fn, nargs, a1, a2, a3);
}

//go:linkname syscall_Syscall6 syscall.Syscall6
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_Syscall6(uintptr fn, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6) {
    return syscall_syscalln(fn, nargs, a1, a2, a3, a4, a5, a6);
}

//go:linkname syscall_Syscall9 syscall.Syscall9
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_Syscall9(uintptr fn, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9) {
    return syscall_syscalln(fn, nargs, a1, a2, a3, a4, a5, a6, a7, a8, a9);
}

//go:linkname syscall_Syscall12 syscall.Syscall12
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_Syscall12(uintptr fn, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12) {
    return syscall_syscalln(fn, nargs, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
}

//go:linkname syscall_Syscall15 syscall.Syscall15
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_Syscall15(uintptr fn, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15) {
    return syscall_syscalln(fn, nargs, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
}

//go:linkname syscall_Syscall18 syscall.Syscall18
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_Syscall18(uintptr fn, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15, uintptr a16, uintptr a17, uintptr a18) {
    return syscall_syscalln(fn, nargs, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18);
}

// maxArgs should be divisible by 2, as Windows stack
// must be kept 16-byte aligned on syscall entry.
//
// Although it only permits maximum 42 parameters, it
// is arguably large enough.
internal static UntypedInt maxArgs => 42;

//go:linkname syscall_SyscallN syscall.SyscallN
//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_SyscallN(uintptr fn, params ꓸꓸꓸuintptr argsʗp) {
    var args = argsʗp.slice();

    return syscall_syscalln(fn, (uintptr)len(args), args.ꓸꓸꓸ);
}

//go:nosplit
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscalln(uintptr fn, uintptr n, params ꓸꓸꓸuintptr argsʗp) {
    var args = argsʗp.slice();

    if (n > (uintptr)len(args)) {
        throw panic("syscall: n > len(args)"); // should not be reachable from user code
    }
    if (n > maxArgs) {
        throw panic("runtime: SyscallN has too many arguments");
    }
    // The cgocall parameters are stored in m instead of in
    // the stack because the stack can move during fn if it
    // calls back into Go.
    var c = (~getg()).m.of(m.Ꮡwinsyscall);
    c.Value.fn = fn;
    c.Value.n = n;
    if ((~c).n != 0) {
        c.Value.args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡ(args, 0)));
    }
    cgocall(asmstdcallAddr, new @unsafe.Pointer(c));
    // cgocall may reschedule us on to a different M,
    // but it copies the return values into the new M's
    // so we can read them from there.
    c = (~getg()).m.of(m.Ꮡwinsyscall);
    return ((~c).r1, (~c).r2, (~c).err);
}

} // end runtime_package
