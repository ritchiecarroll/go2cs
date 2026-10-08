// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

partial struct sigctxt {
    internal ж<siginfo> info;
    internal @unsafe.Pointer ctxt;
}

//go:nosplit
//go:nowritebarrierrec
internal static ж<regs64> regs(this ref sigctxt c) {
    return ((ж<ucontext>)(uintptr)(c.ctxt)).Value.uc_mcontext.of(mcontext64.Ꮡss);
}

internal static uint64 rax(this ref sigctxt c) {
    return (~c.regs()).rax;
}

internal static uint64 rbx(this ref sigctxt c) {
    return (~c.regs()).rbx;
}

internal static uint64 rcx(this ref sigctxt c) {
    return (~c.regs()).rcx;
}

internal static uint64 rdx(this ref sigctxt c) {
    return (~c.regs()).rdx;
}

internal static uint64 rdi(this ref sigctxt c) {
    return (~c.regs()).rdi;
}

internal static uint64 rsi(this ref sigctxt c) {
    return (~c.regs()).rsi;
}

internal static uint64 rbp(this ref sigctxt c) {
    return (~c.regs()).rbp;
}

internal static uint64 rsp(this ref sigctxt c) {
    return (~c.regs()).rsp;
}

internal static uint64 r8(this ref sigctxt c) {
    return (~c.regs()).r8;
}

internal static uint64 r9(this ref sigctxt c) {
    return (~c.regs()).r9;
}

internal static uint64 r10(this ref sigctxt c) {
    return (~c.regs()).r10;
}

internal static uint64 r11(this ref sigctxt c) {
    return (~c.regs()).r11;
}

internal static uint64 r12(this ref sigctxt c) {
    return (~c.regs()).r12;
}

internal static uint64 r13(this ref sigctxt c) {
    return (~c.regs()).r13;
}

internal static uint64 r14(this ref sigctxt c) {
    return (~c.regs()).r14;
}

internal static uint64 r15(this ref sigctxt c) {
    return (~c.regs()).r15;
}

//go:nosplit
//go:nowritebarrierrec
internal static uint64 rip(this ref sigctxt c) {
    return (~c.regs()).rip;
}

internal static uint64 rflags(this ref sigctxt c) {
    return (~c.regs()).rflags;
}

internal static uint64 cs(this ref sigctxt c) {
    return (~c.regs()).cs;
}

internal static uint64 fs(this ref sigctxt c) {
    return (~c.regs()).fs;
}

internal static uint64 gs(this ref sigctxt c) {
    return (~c.regs()).gs;
}

internal static uint64 sigcode(this ref sigctxt c) {
    return (uint64)(~c.info).si_code;
}

internal static uint64 sigaddr(this ref sigctxt c) {
    return (~c.info).si_addr;
}

internal static void set_rip(this ref sigctxt c, uint64 x) {
    c.regs().Value.rip = x;
}

internal static void set_rsp(this ref sigctxt c, uint64 x) {
    c.regs().Value.rsp = x;
}

internal static void set_sigcode(this ref sigctxt c, uint64 x) {
    c.info.Value.si_code = (int32)x;
}

internal static void set_sigaddr(this ref sigctxt c, uint64 x) {
    c.info.Value.si_addr = x;
}

//go:nosplit
internal static void fixsigcode(this ref sigctxt c, uint32 sig) {
    var exprᴛ1 = sig;
    if (exprᴛ1 == _SIGTRAP) {
        var pc = (uintptr)c.rip();
        var code = (ж<array<byte>>)(uintptr)((@unsafe.Pointer)(pc - 2));
        if (code.Value[1] != 0xCC && (code.Value[0] != 0xCD || code.Value[1] != 3)) {
            // OS X sets c.sigcode() == TRAP_BRKPT unconditionally for all SIGTRAPs,
            // leaving no way to distinguish a breakpoint-induced SIGTRAP
            // from an asynchronous signal SIGTRAP.
            // They all look breakpoint-induced by default.
            // Try looking at the code to see if it's a breakpoint.
            // The assumption is that we're very unlikely to get an
            // asynchronous SIGTRAP at just the moment that the
            // PC started to point at unmapped memory.
            // OS X will leave the pc just after the INT 3 instruction.
            // INT 3 is usually 1 byte, but there is a 2-byte form.
            // SIGTRAP on something other than INT 3.
            c.set_sigcode(_SI_USER);
        }
    }
    else if (exprᴛ1 == _SIGSEGV) {
        if (c.sigcode() == _SI_USER) {
            // x86-64 has 48-bit virtual addresses. The top 16 bits must echo bit 47.
            // The hardware delivers a different kind of fault for a malformed address
            // than it does for an attempt to access a valid but unmapped address.
            // OS X 10.9.2 mishandles the malformed address case, making it look like
            // a user-generated signal (like someone ran kill -SEGV ourpid).
            // We pass user-generated signals to os/signal, or else ignore them.
            // Doing that here - and returning to the faulting code - results in an
            // infinite loop. It appears the best we can do is rewrite what the kernel
            // delivers into something more like the truth. The address used below
            // has very little chance of being the one that caused the fault, but it is
            // malformed, it is clearly not a real pointer, and if it does get printed
            // in real life, people will probably search for it and find this code.
            // There are no Google hits for b01dfacedebac1e or 0xb01dfacedebac1e
            // as I type this comment.
            //
            // Note: if this code is removed, please consider
            // enabling TestSignalForwardingGo for darwin-amd64 in
            // misc/cgo/testcarchive/carchive_test.go.
            c.set_sigcode(_SI_USER + 1);
            c.set_sigaddr(0xb01dfacedebac1eUL);
        }
    }

}

} // end runtime_package
