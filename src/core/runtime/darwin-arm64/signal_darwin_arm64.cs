// Copyright 2015 The Go Authors. All rights reserved.
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
internal static ж<regs64> regs⁔(/*this*/ ref sigctxt c) { return (@unsafe.Pointer.To<ucontext>(c.ctxt)).Value.uc_mcontext.Ꮡss; }

internal static uint64 r0⁔(/*this*/ ref sigctxt c) { return c.regs().x[0]; }
internal static uint64 r1⁔(/*this*/ ref sigctxt c) { return c.regs().x[1]; }
internal static uint64 r2⁔(/*this*/ ref sigctxt c) { return c.regs().x[2]; }
internal static uint64 r3⁔(/*this*/ ref sigctxt c) { return c.regs().x[3]; }
internal static uint64 r4⁔(/*this*/ ref sigctxt c) { return c.regs().x[4]; }
internal static uint64 r5⁔(/*this*/ ref sigctxt c) { return c.regs().x[5]; }
internal static uint64 r6⁔(/*this*/ ref sigctxt c) { return c.regs().x[6]; }
internal static uint64 r7⁔(/*this*/ ref sigctxt c) { return c.regs().x[7]; }
internal static uint64 r8⁔(/*this*/ ref sigctxt c) { return c.regs().x[8]; }
internal static uint64 r9⁔(/*this*/ ref sigctxt c) { return c.regs().x[9]; }
internal static uint64 r10⁔(/*this*/ ref sigctxt c) { return c.regs().x[10]; }
internal static uint64 r11⁔(/*this*/ ref sigctxt c) { return c.regs().x[11]; }
internal static uint64 r12⁔(/*this*/ ref sigctxt c) { return c.regs().x[12]; }
internal static uint64 r13⁔(/*this*/ ref sigctxt c) { return c.regs().x[13]; }
internal static uint64 r14⁔(/*this*/ ref sigctxt c) { return c.regs().x[14]; }
internal static uint64 r15⁔(/*this*/ ref sigctxt c) { return c.regs().x[15]; }
internal static uint64 r16⁔(/*this*/ ref sigctxt c) { return c.regs().x[16]; }
internal static uint64 r17⁔(/*this*/ ref sigctxt c) { return c.regs().x[17]; }
internal static uint64 r18⁔(/*this*/ ref sigctxt c) { return c.regs().x[18]; }
internal static uint64 r19⁔(/*this*/ ref sigctxt c) { return c.regs().x[19]; }
internal static uint64 r20⁔(/*this*/ ref sigctxt c) { return c.regs().x[20]; }
internal static uint64 r21⁔(/*this*/ ref sigctxt c) { return c.regs().x[21]; }
internal static uint64 r22⁔(/*this*/ ref sigctxt c) { return c.regs().x[22]; }
internal static uint64 r23⁔(/*this*/ ref sigctxt c) { return c.regs().x[23]; }
internal static uint64 r24⁔(/*this*/ ref sigctxt c) { return c.regs().x[24]; }
internal static uint64 r25⁔(/*this*/ ref sigctxt c) { return c.regs().x[25]; }
internal static uint64 r26⁔(/*this*/ ref sigctxt c) { return c.regs().x[26]; }
internal static uint64 r27⁔(/*this*/ ref sigctxt c) { return c.regs().x[27]; }
internal static uint64 r28⁔(/*this*/ ref sigctxt c) { return c.regs().x[28]; }
internal static uint64 r29⁔(/*this*/ ref sigctxt c) { return c.regs().fp; }
internal static uint64 lr⁔(/*this*/ ref sigctxt c) { return c.regs().lr; }
internal static uint64 sp⁔(/*this*/ ref sigctxt c) { return c.regs().sp; }

//go:nosplit
//go:nowritebarrierrec
internal static uint64 pc⁔(/*this*/ ref sigctxt c) { return c.regs().pc; }

internal static uintptr fault⁔(/*this*/ ref sigctxt c) { return (uintptr)c.info.si_addr; }

internal static uint64 sigcode⁔(/*this*/ ref sigctxt c) { return (uint64)c.info.si_code; }
internal static uint64 sigaddr⁔(/*this*/ ref sigctxt c) { return (uint64)(uintptr)c.info.si_addr; }

internal static void set_pc⁔(/*this*/ ref sigctxt c, uint64 x) { c.regs().pc = x; }
internal static void set_sp⁔(/*this*/ ref sigctxt c, uint64 x) { c.regs().sp = x; }
internal static void set_lr⁔(/*this*/ ref sigctxt c, uint64 x) { c.regs().lr = x; }
internal static void set_r28⁔(/*this*/ ref sigctxt c, uint64 x) { c.regs().x[28] = x; }

internal static void set_sigcode⁔(/*this*/ ref sigctxt c, uint64 x) { c.info.si_code = (int32)x; }
internal static void set_sigaddr⁔(/*this*/ ref sigctxt c, uint64 x) {
    c.info.si_addr = @unsafe.Pointer.To<byte>((@unsafe.Pointer)(uintptr)x);
}

//go:nosplit
internal static void fixsigcode⁔(/*this*/ ref sigctxt c, uint32 sig) {
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
        var pc = (uintptr)c.pc();
        // OS X will leave the pc just after the instruction.
        var code = @unsafe.Pointer.To<uint32>((@unsafe.Pointer)(pc - 4));
        if (code.Value != 0xd4200000U) {
            // SIGTRAP on something other than breakpoint.
            c.set_sigcode(_SI_USER);
        }
        break;
    }}
}

} // end runtime_package
