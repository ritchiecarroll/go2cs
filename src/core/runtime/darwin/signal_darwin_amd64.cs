// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using static runtime_package;

partial class runtime_package {

partial struct sigctxt {
    internal ж<siginfo> info;
    internal @unsafe.Pointer ctxt;
}

} partial class runtime_package {

//go:nosplit
//go:nowritebarrierrec
[GoMethodBody("regs")] internal static ж<regs64> regsᴍ(ref sigctxt c) { return (@unsafe.Pointer.To<ucontext>(c.ctxt)).Value.uc_mcontext.Ꮡss; }

[GoMethodBody("rax")] internal static uint64 raxᴍ(ref sigctxt c) { return c.regs().rax; }
[GoMethodBody("rbx")] internal static uint64 rbxᴍ(ref sigctxt c) { return c.regs().rbx; }
[GoMethodBody("rcx")] internal static uint64 rcxᴍ(ref sigctxt c) { return c.regs().rcx; }
[GoMethodBody("rdx")] internal static uint64 rdxᴍ(ref sigctxt c) { return c.regs().rdx; }
[GoMethodBody("rdi")] internal static uint64 rdiᴍ(ref sigctxt c) { return c.regs().rdi; }
[GoMethodBody("rsi")] internal static uint64 rsiᴍ(ref sigctxt c) { return c.regs().rsi; }
[GoMethodBody("rbp")] internal static uint64 rbpᴍ(ref sigctxt c) { return c.regs().rbp; }
[GoMethodBody("rsp")] internal static uint64 rspᴍ(ref sigctxt c) { return c.regs().rsp; }
[GoMethodBody("r8")] internal static uint64 r8ᴍ(ref sigctxt c) { return c.regs().r8; }
[GoMethodBody("r9")] internal static uint64 r9ᴍ(ref sigctxt c) { return c.regs().r9; }
[GoMethodBody("r10")] internal static uint64 r10ᴍ(ref sigctxt c) { return c.regs().r10; }
[GoMethodBody("r11")] internal static uint64 r11ᴍ(ref sigctxt c) { return c.regs().r11; }
[GoMethodBody("r12")] internal static uint64 r12ᴍ(ref sigctxt c) { return c.regs().r12; }
[GoMethodBody("r13")] internal static uint64 r13ᴍ(ref sigctxt c) { return c.regs().r13; }
[GoMethodBody("r14")] internal static uint64 r14ᴍ(ref sigctxt c) { return c.regs().r14; }
[GoMethodBody("r15")] internal static uint64 r15ᴍ(ref sigctxt c) { return c.regs().r15; }

//go:nosplit
//go:nowritebarrierrec
[GoMethodBody("rip")] internal static uint64 ripᴍ(ref sigctxt c) { return c.regs().rip; }

[GoMethodBody("rflags")] internal static uint64 rflagsᴍ(ref sigctxt c) { return c.regs().rflags; }
[GoMethodBody("cs")] internal static uint64 csᴍ(ref sigctxt c) { return c.regs().cs; }
[GoMethodBody("fs")] internal static uint64 fsᴍ(ref sigctxt c) { return c.regs().fs; }
[GoMethodBody("gs")] internal static uint64 gsᴍ(ref sigctxt c) { return c.regs().gs; }
[GoMethodBody("sigcode")] internal static uint64 sigcodeᴍ(ref sigctxt c) { return (uint64)c.info.si_code; }
[GoMethodBody("sigaddr")] internal static uint64 sigaddrᴍ(ref sigctxt c) { return c.info.si_addr; }

[GoMethodBody("set_rip")] internal static void set_ripᴍ(ref sigctxt c, uint64 x) { c.regs().rip = x; }
[GoMethodBody("set_rsp")] internal static void set_rspᴍ(ref sigctxt c, uint64 x) { c.regs().rsp = x; }
[GoMethodBody("set_sigcode")] internal static void set_sigcodeᴍ(ref sigctxt c, uint64 x) { c.info.si_code = (int32)x; }
[GoMethodBody("set_sigaddr")] internal static void set_sigaddrᴍ(ref sigctxt c, uint64 x) { c.info.si_addr = x; }

//go:nosplit
[GoMethodBody("fixsigcode")] internal static void fixsigcodeᴍ(ref sigctxt c, uint32 sig) {
    switch (sig) {
    case var ᴛ when ᴛ == _SIGTRAP: {
        // OS X sets c.sigcode() == TRAP_BRKPT unconditionally for all SIGTRAPs,
        // leaving no way to distinguish a breakpoint-induced SIGTRAP
        // from an asynchronous signal SIGTRAP.
        // They all look breakpoint-induced by default.
        // Try looking at the code to see if it's a breakpoint.
        // The assumption is that we're very unlikely to get an
        // asynchronous SIGTRAP at just the moment that the
        // PC started to point at unmapped memory.
        var pc = (uintptr)c.rip();
        // OS X will leave the pc just after the INT 3 instruction.
        // INT 3 is usually 1 byte, but there is a 2-byte form.
        var code = @unsafe.Pointer.ToArray<byte>((@unsafe.Pointer)(pc - 2), 2);
        if (code.ElementRef(1) != 0xCC && (code.ElementRef(0) != 0xCD || code.ElementRef(1) != 3)) {
            // SIGTRAP on something other than INT 3.
            c.set_sigcode(_SI_USER);
        }
        break;
    }
    case var ᴛ when ᴛ == _SIGSEGV: {
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
        if (c.sigcode() == _SI_USER) {
            c.set_sigcode(_SI_USER + 1);
            c.set_sigaddr(0xb01dfacedebac1eUL);
        }
        break;
    }}
}

}
