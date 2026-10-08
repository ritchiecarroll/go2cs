// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 && (darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris)
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static void dumpregs(ж<sigctxt> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    print((@string)"rax    "u8, ((Δhex)c.rax()), (@string)"\n"u8);
    print((@string)"rbx    "u8, ((Δhex)c.rbx()), (@string)"\n"u8);
    print((@string)"rcx    "u8, ((Δhex)c.rcx()), (@string)"\n"u8);
    print((@string)"rdx    "u8, ((Δhex)c.rdx()), (@string)"\n"u8);
    print((@string)"rdi    "u8, ((Δhex)c.rdi()), (@string)"\n"u8);
    print((@string)"rsi    "u8, ((Δhex)c.rsi()), (@string)"\n"u8);
    print((@string)"rbp    "u8, ((Δhex)c.rbp()), (@string)"\n"u8);
    print((@string)"rsp    "u8, ((Δhex)c.rsp()), (@string)"\n"u8);
    print((@string)"r8     "u8, ((Δhex)c.r8()), (@string)"\n"u8);
    print((@string)"r9     "u8, ((Δhex)c.r9()), (@string)"\n"u8);
    print((@string)"r10    "u8, ((Δhex)c.r10()), (@string)"\n"u8);
    print((@string)"r11    "u8, ((Δhex)c.r11()), (@string)"\n"u8);
    print((@string)"r12    "u8, ((Δhex)c.r12()), (@string)"\n"u8);
    print((@string)"r13    "u8, ((Δhex)c.r13()), (@string)"\n"u8);
    print((@string)"r14    "u8, ((Δhex)c.r14()), (@string)"\n"u8);
    print((@string)"r15    "u8, ((Δhex)c.r15()), (@string)"\n"u8);
    print((@string)"rip    "u8, ((Δhex)c.rip()), (@string)"\n"u8);
    print((@string)"rflags "u8, ((Δhex)c.rflags()), (@string)"\n"u8);
    print((@string)"cs     "u8, ((Δhex)c.cs()), (@string)"\n"u8);
    print((@string)"fs     "u8, ((Δhex)c.fs()), (@string)"\n"u8);
    print((@string)"gs     "u8, ((Δhex)c.gs()), (@string)"\n"u8);
}

//go:nosplit
//go:nowritebarrierrec
internal static uintptr sigpc(this ref sigctxt c) {
    return (uintptr)c.rip();
}

internal static void setsigpc(this ref sigctxt c, uint64 x) {
    c.set_rip(x);
}

internal static uintptr sigsp(this ref sigctxt c) {
    return (uintptr)c.rsp();
}

internal static uintptr siglr(this ref sigctxt c) {
    return 0;
}

internal static uintptr fault(this ref sigctxt c) {
    return (uintptr)c.sigaddr();
}

// preparePanic sets up the stack to look like a call to sigpanic.
internal static void preparePanic(this ref sigctxt c, uint32 sig, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Work around Leopard bug that doesn't set FPE_INTDIV.
    // Look at instruction to see if it is a divide.
    // Not necessary in Snow Leopard (si_code will be != 0).
    if (GOOS == "darwin"u8 && sig == _SIGFPE && gp.sigcode0 == 0) {
        var pcΔ1 = (ж<array<byte>>)(uintptr)((@unsafe.Pointer)gp.sigpc);
        nint i = 0;
        if ((byte)(pcΔ1.Value[i] & 0xF0) == 0x40){
            // 64-bit REX prefix
            i++;
        } else 
        if (pcΔ1.Value[i] == 0x66) {
            // 16-bit instruction prefix
            i++;
        }
        if (pcΔ1.Value[i] == 0xF6 || pcΔ1.Value[i] == 0xF7) {
            gp.sigcode0 = _FPE_INTDIV;
        }
    }
    var pc = (uintptr)c.rip();
    var sp = (uintptr)c.rsp();
    // In case we are panicking from external code, we need to initialize
    // Go special registers. We inject sigpanic0 (instead of sigpanic),
    // which takes care of that.
    if (shouldPushSigpanic(ref (Ꮡgp).DerefOrNull(), pc, ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)sp))){
        c.pushCall(abi.FuncPCABI0(sigpanic0), pc);
    } else {
        // Not safe to push the call. Just clobber the frame.
        c.set_rip((uint64)abi.FuncPCABI0(sigpanic0));
    }
}

internal static void pushCall(this ref sigctxt c, uintptr targetPC, uintptr resumePC) {
    // Make it look like we called target at resumePC.
    var sp = (uintptr)c.rsp();
    sp -= goarch.PtrSize;
    ((ж<uintptr>)(uintptr)((@unsafe.Pointer)sp)).Value = resumePC;
    c.set_rsp((uint64)sp);
    c.set_rip((uint64)targetPC);
}

} // end runtime_package
