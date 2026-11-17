// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build darwin || freebsd || linux || netbsd || openbsd
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static void dumpregs(ж<sigctxt> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    print((@string)"r0      ", (Δhex)c.r0(), (@string)"\n");
    print((@string)"r1      ", (Δhex)c.r1(), (@string)"\n");
    print((@string)"r2      ", (Δhex)c.r2(), (@string)"\n");
    print((@string)"r3      ", (Δhex)c.r3(), (@string)"\n");
    print((@string)"r4      ", (Δhex)c.r4(), (@string)"\n");
    print((@string)"r5      ", (Δhex)c.r5(), (@string)"\n");
    print((@string)"r6      ", (Δhex)c.r6(), (@string)"\n");
    print((@string)"r7      ", (Δhex)c.r7(), (@string)"\n");
    print((@string)"r8      ", (Δhex)c.r8(), (@string)"\n");
    print((@string)"r9      ", (Δhex)c.r9(), (@string)"\n");
    print((@string)"r10     ", (Δhex)c.r10(), (@string)"\n");
    print((@string)"r11     ", (Δhex)c.r11(), (@string)"\n");
    print((@string)"r12     ", (Δhex)c.r12(), (@string)"\n");
    print((@string)"r13     ", (Δhex)c.r13(), (@string)"\n");
    print((@string)"r14     ", (Δhex)c.r14(), (@string)"\n");
    print((@string)"r15     ", (Δhex)c.r15(), (@string)"\n");
    print((@string)"r16     ", (Δhex)c.r16(), (@string)"\n");
    print((@string)"r17     ", (Δhex)c.r17(), (@string)"\n");
    print((@string)"r18     ", (Δhex)c.r18(), (@string)"\n");
    print((@string)"r19     ", (Δhex)c.r19(), (@string)"\n");
    print((@string)"r20     ", (Δhex)c.r20(), (@string)"\n");
    print((@string)"r21     ", (Δhex)c.r21(), (@string)"\n");
    print((@string)"r22     ", (Δhex)c.r22(), (@string)"\n");
    print((@string)"r23     ", (Δhex)c.r23(), (@string)"\n");
    print((@string)"r24     ", (Δhex)c.r24(), (@string)"\n");
    print((@string)"r25     ", (Δhex)c.r25(), (@string)"\n");
    print((@string)"r26     ", (Δhex)c.r26(), (@string)"\n");
    print((@string)"r27     ", (Δhex)c.r27(), (@string)"\n");
    print((@string)"r28     ", (Δhex)c.r28(), (@string)"\n");
    print((@string)"r29     ", (Δhex)c.r29(), (@string)"\n");
    print((@string)"lr      ", (Δhex)c.lr(), (@string)"\n");
    print((@string)"sp      ", (Δhex)c.sp(), (@string)"\n");
    print((@string)"pc      ", (Δhex)c.pc(), (@string)"\n");
    print((@string)"fault   ", (Δhex)(uint64)c.fault(), (@string)"\n");
}

//go:nosplit
//go:nowritebarrierrec
internal static uintptr sigpc(this ref sigctxt c) { return (uintptr)c.pc(); }

internal static void setsigpc(this ref sigctxt c, uint64 x) { c.set_pc(x); }
internal static uintptr sigsp(this ref sigctxt c) { return (uintptr)c.sp(); }
internal static uintptr siglr(this ref sigctxt c) { return (uintptr)c.lr(); }

// preparePanic sets up the stack to look like a call to sigpanic.
internal static void preparePanic(this ref sigctxt c, uint32 sig, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // We arrange lr, and pc to pretend the panicking
    // function calls sigpanic directly.
    // Always save LR to stack so that panics in leaf
    // functions are correctly handled. This smashes
    // the stack frame but we're not going back there
    // anyway.
    var sp = c.sp() - (uint64)sys.StackAlign; // needs only sizeof uint64, but must align the stack
    c.set_sp(sp);
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(uintptr)sp)).Value = c.lr();
    // Make sure a valid frame pointer is saved on the stack so that the
    // frame pointer checks in adjustframe are happy, if they're enabled.
    // Frame pointer unwinding won't visit the sigpanic frame, since
    // sigpanic will save the same frame pointer before calling into a panic
    // function.
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(uintptr)(sp - (uint64)goarch.PtrSize))).Value = c.r29();

    var pc = gp.sigpc;

    if (shouldPushSigpanic(ref (Ꮡgp).DerefOrNull(), pc, (uintptr)c.lr())) {
        // Make it look the like faulting PC called sigpanic.
        c.set_lr((uint64)pc);
    }

    // In case we are panicking from external C code
    c.set_r28((uint64)(uintptr)Ꮡgp);
    c.set_pc((uint64)abi.FuncPCABIInternal(sigpanic));
}

internal static void pushCall(this ref sigctxt c, uintptr targetPC, uintptr resumePC) {
    // Push the LR to stack, as we'll clobber it in order to
    // push the call. The function being pushed is responsible
    // for restoring the LR and setting the SP back.
    // This extra space is known to gentraceback.
    var sp = c.sp() - 16; // SP needs 16-byte alignment
    c.set_sp(sp);
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(uintptr)sp)).Value = c.lr();
    // Make sure a valid frame pointer is saved on the stack so that the
    // frame pointer checks in adjustframe are happy, if they're enabled.
    // This is not actually used for unwinding.
    (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)(uintptr)(sp - (uint64)goarch.PtrSize))).Value = c.r29();
    // Set up PC and LR to pretend the function being signaled
    // calls targetPC at resumePC.
    c.set_lr((uint64)resumePC);
    c.set_pc((uint64)targetPC);
}

} // end runtime_package
