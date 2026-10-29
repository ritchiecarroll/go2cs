// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using windows = @internal.runtime.syscall.windows_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime.syscall;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class runtime_package {

// cbs stores all registered Go callbacks.
partial struct cbsᴛ1 /*dyn*/ {
    internal mutex @lock; // use cbsLock / cbsUnlock for race instrumentation.
    internal array<winCallback> ctxt = new(2000);
    internal map<winCallbackKey, nint> index;
    internal nint n;
}
internal static readonly ж<cbsᴛ1> Ꮡcbs = new StaticBox<cbsᴛ1>(new cbsᴛ1());
internal static ref cbsᴛ1 cbs => ref ((StaticBox<cbsᴛ1>)Ꮡcbs).ValueSlot;

internal static void cbsLock() {
    @lock(Ꮡcbs.of(cbsᴛ1.Ꮡlock));
    // compileCallback is used by goenvs prior to completion of schedinit.
    // raceacquire involves a racecallback to get the proc, which is not
    // safe prior to scheduler initialization. Thus avoid instrumentation
    // until then.
    if (raceenabled && mainStarted) {
        raceacquire(@unsafe.Pointer.FromPinnedBox(Ꮡcbs.of(cbsᴛ1.Ꮡlock)));
    }
}

internal static void cbsUnlock() {
    if (raceenabled && mainStarted) {
        racerelease(@unsafe.Pointer.FromPinnedBox(Ꮡcbs.of(cbsᴛ1.Ꮡlock)));
    }
    unlock(Ꮡcbs.of(cbsᴛ1.Ꮡlock));
}

// winCallback records information about a registered Go callback.
partial struct winCallback {
    internal ж<funcval> fn;  // Go function
    internal uintptr retPop; // For 386 cdecl, how many bytes to pop on return
    internal abiDesc abiMap;
}

// abiPartKind is the action an abiPart should take.
partial struct abiPartKind /*num:nint*/;

internal static abiPartKind abiPartBad => /* iota */ 0;
internal static abiPartKind abiPartStack => 1; // Move a value from memory to the stack.
internal static abiPartKind abiPartReg => 2;   // Move a value from memory to a register.

// abiPart encodes a step in translating between calling ABIs.
partial struct abiPart {
    internal abiPartKind kind;
    internal uintptr srcStackOffset;
    internal uintptr dstStackOffset; // used if kind == abiPartStack
    internal nint dstRegister;       // used if kind == abiPartReg
    internal uintptr len;
}

internal static bool tryMerge(this ref abiPart a, abiPart b) {
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
partial struct abiDesc {
    internal slice<abiPart> parts;
    internal uintptr srcStackSize; // stdcall/fastcall stack space tracking
    internal uintptr dstStackSize; // Go stack space used
    internal uintptr dstSpill;     // Extra stack space for argument spill slots
    internal nint dstRegisters;    // Go ABI int argument registers used
    // retOffset is the offset of the uintptr-sized result in the Go
    // frame.
    internal uintptr retOffset;
}

internal static void assignArg(this ref abiDesc Δp, ж<_type> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.Size_ > goarch.PtrSize) {
        // We don't support this right now. In
        // stdcall/cdecl, 64-bit ints and doubles are
        // passed as two words (little endian); and
        // structs are pushed on the stack. In
        // fastcall, arguments larger than the word
        // size are passed by reference.
        throw panic("compileCallback: argument size is larger than uintptr");
    }
    {
        var k = t.Kind(); if (GOARCH != "386"u8 && (k == abi.Float32 || k == abi.Float64)) {
            // In fastcall, floating-point arguments in
            // the first four positions are passed in
            // floating-point registers, which we don't
            // currently spill.
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
    // arguments.

    var oldParts = Δp.parts;
    if (Δp.tryRegAssignArg(Ꮡt, 0)) {
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

    // cdecl, stdcall, and fastcall pad arguments to word size.
    // TODO(rsc): On arm64 do we need to skip the caller's saved LR?
    Δp.srcStackSize += goarch.PtrSize;
}

// tryRegAssignArg tries to register-assign a value of type t.
// If this type is nested in an aggregate type, then offset is the
// offset of this type within its parent type.
// Assumes t.size <= goarch.PtrSize and t.size != 0.
//
// Returns whether the assignment succeeded.
internal static bool tryRegAssignArg(this ref abiDesc Δp, ж<_type> Ꮡt, uintptr offset) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var k = t.Kind();
        switch (k) {
        case var ᴛ when ᴛ == abi.Bool || ᴛ == abi.Int || ᴛ == abi.Int8 || ᴛ == abi.Int16 || ᴛ == abi.Int32 || ᴛ == abi.Uint || ᴛ == abi.Uint8 || ᴛ == abi.Uint16 || ᴛ == abi.Uint32 || ᴛ == abi.Uintptr || ᴛ == abi.Pointer || ᴛ == abi.UnsafePointer: {
            // Assign a register for all these types.
            return Δp.assignReg(t.Size_, offset);
        }
        case var ᴛ when ᴛ == abi.Int64 || ᴛ == abi.Uint64: {
            // Only register-assign if the registers are big enough.
            if (goarch.PtrSize == 8) {
                return Δp.assignReg(t.Size_, offset);
            }
            break;
        }
        case var ᴛ when ᴛ == abi.Array: {
            var at = Ꮡt.Reinterpret<_type, arraytype>();
            if (at.of(arraytype.ᏑLen).Value == 1) {
                return Δp.tryRegAssignArg(at.of(arraytype.ᏑElem).Value, offset); // TODO fix when runtime is fully commoned up w/ abi.Type
            }
            break;
        }
        case var ᴛ when ᴛ == abi.Struct: {
            var st = Ꮡt.Reinterpret<_type, structtype>();
            foreach (var i in range(st.Fields)) {
                var f = Ꮡ(st.Fields, i);
                if (!Δp.tryRegAssignArg(f.Typ, offset + f.Offset)) {
                    return false;
                }
            }
            return true;
        }}
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
internal static bool assignReg(this ref abiDesc Δp, uintptr size, uintptr offset) {
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

partial struct winCallbackKey {
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
// On ARM64, runtime.callbackasm is a series of mov and branch instructions.
// R12 is loaded with the callback index. Each entry is two instructions,
// hence 8 bytes.
internal static uintptr callbackasmAddr(nint i) {
    nint entrySize = default;
    var exprᴛ1 = GOARCH;
    switch (ᐧ) {
    default: {
        throw panic("unsupported architecture");
        break;
    }
    case {} when exprᴛ1 == "386"u8 || exprᴛ1 == "amd64"u8: {
        entrySize = 5;
        break;
    }
    case {} when exprᴛ1 == "arm64"u8: {
        // On ARM64, each entry is a MOV instruction
        // followed by a branch instruction
        entrySize = 8;
        break;
    }}
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
// it always uses fastcall.
//
//go:linkname compileCallback syscall.compileCallback
internal static uintptr /*code*/ compileCallback(eface fn, bool cdecl) {
    if (GOARCH != "386"u8) {
        // cdecl is only meaningful on 386.
        cdecl = false;
    }

    if (fn._type == nil || fn._type.Kind() != abi.Func) {
        throw panic("compileCallback: expected function with one uintptr-sized result");
    }
    var ft = fn._type.FuncType();

    // Check arguments and construct ABI translation.
    abiDesc abiMap = default;
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
    if (ft.OutSlice()[0].Size_ != goarch.PtrSize) {
        throw panic("compileCallback: expected function with one uintptr-sized result");
    }
    {
        var k = ft.OutSlice()[0].Kind(); if (k == abi.Float32 || k == abi.Float64) {
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
    uintptr retPop = default;
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
    if (cbs.index == nil) {
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

partial struct callbackArgs {
    internal uintptr index;
    // args points to the argument block.
    //
    // For cdecl and stdcall, all arguments are on the stack.
    //
    // For fastcall, the trampoline spills register arguments to
    // the reserved spill slots below the stack arguments,
    // resulting in a layout equivalent to stdcall.
    internal @unsafe.Pointer args;
    // Below are out-args from callbackWrap
    internal uintptr result;
    internal uintptr retPop; // For 386 cdecl, how many bytes to pop on return
}

// callbackWrap is called by callbackasm to invoke a registered C callback.
internal static void callbackWrap(ref callbackArgs a) {
    var c = cbs.ctxt[a.index];
    a.retPop = c.retPop;

    // Convert from C to Go ABI.
    ref var regs = ref heap(new abi.RegArgs(), out var Ꮡregs);
    ref var frame = ref heap(new array<byte>(512), out var Ꮡframe);
    @unsafe.Pointer goArgs = @unsafe.Pointer.FromPinnedBox(Ꮡframe);
    foreach (var (_, part) in c.abiMap.parts) {
        switch (part.kind) {
        case var ᴛ when ᴛ == abiPartStack: {
            memmove((uintptr)add(goArgs, part.dstStackOffset), (uintptr)add(a.args, part.srcStackOffset), part.len);
            break;
        }
        case var ᴛ when ᴛ == abiPartReg: {
            @unsafe.Pointer goReg = @unsafe.Pointer.FromBox(Ꮡregs.at(abi.RegArgs.ᏑInts, part.dstRegister));
            memmove(goReg, (uintptr)add(a.args, part.srcStackOffset), part.len);
            break;
        }
        default: {
            throw panic("bad ABI description");
            break;
        }}
    }

    // TODO(mknyszek): Remove this when we no longer have
    // caller reserved spill space.
    var frameSize = alignUp(c.abiMap.dstStackSize, goarch.PtrSize);
    frameSize += c.abiMap.dstSpill;

    // Even though this is copying back results, we can pass a nil
    // type because those results must not require write barriers.
    reflectcall(nil, @unsafe.Pointer.FromPinnedBox(c.fn), (uintptr)noescape(goArgs), (uint32)c.abiMap.dstStackSize, (uint32)c.abiMap.retOffset, (uint32)frameSize, Ꮡregs);

    // Extract the result.
    //
    // There's always exactly one return value, one pointer in size.
    // If it's on the stack, then we will have reserved space for it
    // at the end of the frame, otherwise it was passed in a register.
    if (c.abiMap.dstStackSize != c.abiMap.retOffset) {
        a.result = ~Ꮡframe.at<byte>((ulong)(c.abiMap.retOffset)).Reinterpret<byte, uintptr>();
    } else {
        nint zero = default;
        // On architectures with no registers, Ints[0] would be a compile error,
        // so we use a dynamic index. These architectures will never take this
        // branch, so this won't cause a runtime panic.
        a.result = regs.Ints[zero];
    }
}

// syscall_syscalln calls fn with args[:n].
// It is used to implement [syscall.SyscallN].
// It shouldn't be used in the runtime package,
// use [stdcall] instead.
//
//go:linkname syscall_syscalln syscall.syscalln
//go:nosplit
//go:uintptrkeepalive
internal static (uintptr r1, uintptr r2, uintptr err) syscall_syscalln(uintptr fn, uintptr n, params ꓸꓸꓸuintptr argsʗp) {
    var args = argsʗp.slice();

    if (n > (uintptr)len(args)) {
        throw panic("syscall: n > len(args)"); // should not be reachable from user code
    }
    if (n > windows.MaxArgs) {
        throw panic("runtime: SyscallN has too many arguments");
    }

    // The cgocall parameters are stored in m instead of in
    // the stack because the stack can move during fn if it
    // calls back into Go.
    var c = (~getg()).m.of(m.Ꮡwinsyscall);
    c.Value.Fn = fn;
    c.Value.N = n;
    if ((~c).N != 0) {
        c.Value.Args = (uintptr)(uintptr)noescape(@unsafe.Pointer.FromBox(Ꮡ(args, 0)));
    }
    cgocall(asmstdcallAddr, @unsafe.Pointer.FromPinnedBox(c));
    // cgocall may reschedule us on to a different M,
    // but it copies the return values into the new M's
    // so we can read them from there.
    c = (~getg()).m.of(m.Ꮡwinsyscall);
    return ((~c).R1, (~c).R2, (~c).Err);
}

} // end runtime_package
