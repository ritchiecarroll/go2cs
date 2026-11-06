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

    print((@string)"rax    ", (Δhex)c.rax(), (@string)"\n");
    print((@string)"rbx    ", (Δhex)c.rbx(), (@string)"\n");
    print((@string)"rcx    ", (Δhex)c.rcx(), (@string)"\n");
    print((@string)"rdx    ", (Δhex)c.rdx(), (@string)"\n");
    print((@string)"rdi    ", (Δhex)c.rdi(), (@string)"\n");
    print((@string)"rsi    ", (Δhex)c.rsi(), (@string)"\n");
    print((@string)"rbp    ", (Δhex)c.rbp(), (@string)"\n");
    print((@string)"rsp    ", (Δhex)c.rsp(), (@string)"\n");
    print((@string)"r8     ", (Δhex)c.r8(), (@string)"\n");
    print((@string)"r9     ", (Δhex)c.r9(), (@string)"\n");
    print((@string)"r10    ", (Δhex)c.r10(), (@string)"\n");
    print((@string)"r11    ", (Δhex)c.r11(), (@string)"\n");
    print((@string)"r12    ", (Δhex)c.r12(), (@string)"\n");
    print((@string)"r13    ", (Δhex)c.r13(), (@string)"\n");
    print((@string)"r14    ", (Δhex)c.r14(), (@string)"\n");
    print((@string)"r15    ", (Δhex)c.r15(), (@string)"\n");
    print((@string)"rip    ", (Δhex)c.rip(), (@string)"\n");
    print((@string)"rflags ", (Δhex)c.rflags(), (@string)"\n");
    print((@string)"cs     ", (Δhex)c.cs(), (@string)"\n");
    print((@string)"fs     ", (Δhex)c.fs(), (@string)"\n");
    print((@string)"gs     ", (Δhex)c.gs(), (@string)"\n");
}

//go:nosplit
//go:nowritebarrierrec
internal static uintptr sigpc(this ref sigctxt c) { return (uintptr)c.rip(); }

internal static void setsigpc(this ref sigctxt c, uint64 x) { c.set_rip(x); }
internal static uintptr sigsp(this ref sigctxt c) { return (uintptr)c.rsp(); }
internal static uintptr siglr(this ref sigctxt c) { return 0; }
internal static uintptr fault(this ref sigctxt c) { return (uintptr)c.sigaddr(); }

// preparePanic sets up the stack to look like a call to sigpanic.
internal static void preparePanic(this ref sigctxt c, uint32 sig, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Work around Leopard bug that doesn't set FPE_INTDIV.
    // Look at instruction to see if it is a divide.
    // Not necessary in Snow Leopard (si_code will be != 0).
    if (GOOS == "darwin"u8 && sig == _SIGFPE && gp.sigcode0 == 0) {
        var pcΔ1 = (ж<array<byte>>)(uintptr)((@unsafe.Pointer)gp.sigpc);
        nint i = 0;
        if ((byte)(pcΔ1.Value[i] & 0xF0) == 0x40) { // 64-bit REX prefix
            i++;
        } else if (pcΔ1.Value[i] == 0x66) { // 16-bit instruction prefix
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
    if (shouldPushSigpanic(ref (Ꮡgp).DerefOrNull(), pc, ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)sp))) {
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
