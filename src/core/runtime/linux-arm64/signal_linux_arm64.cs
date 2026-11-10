// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

partial struct sigctxt {
    internal ж<siginfo> info;
    internal @unsafe.Pointer ctxt;
}

//go:nosplit
//go:nowritebarrierrec
internal static ж<sigcontext> regs(this ref sigctxt c) { return Ꮡ(((ж<ucontext>)(uintptr)(c.ctxt)).Value.uc_mcontext); }

internal static uint64 r0(this ref sigctxt c) { return c.regs().regs[0]; }
internal static uint64 r1(this ref sigctxt c) { return c.regs().regs[1]; }
internal static uint64 r2(this ref sigctxt c) { return c.regs().regs[2]; }
internal static uint64 r3(this ref sigctxt c) { return c.regs().regs[3]; }
internal static uint64 r4(this ref sigctxt c) { return c.regs().regs[4]; }
internal static uint64 r5(this ref sigctxt c) { return c.regs().regs[5]; }
internal static uint64 r6(this ref sigctxt c) { return c.regs().regs[6]; }
internal static uint64 r7(this ref sigctxt c) { return c.regs().regs[7]; }
internal static uint64 r8(this ref sigctxt c) { return c.regs().regs[8]; }
internal static uint64 r9(this ref sigctxt c) { return c.regs().regs[9]; }
internal static uint64 r10(this ref sigctxt c) { return c.regs().regs[10]; }
internal static uint64 r11(this ref sigctxt c) { return c.regs().regs[11]; }
internal static uint64 r12(this ref sigctxt c) { return c.regs().regs[12]; }
internal static uint64 r13(this ref sigctxt c) { return c.regs().regs[13]; }
internal static uint64 r14(this ref sigctxt c) { return c.regs().regs[14]; }
internal static uint64 r15(this ref sigctxt c) { return c.regs().regs[15]; }
internal static uint64 r16(this ref sigctxt c) { return c.regs().regs[16]; }
internal static uint64 r17(this ref sigctxt c) { return c.regs().regs[17]; }
internal static uint64 r18(this ref sigctxt c) { return c.regs().regs[18]; }
internal static uint64 r19(this ref sigctxt c) { return c.regs().regs[19]; }
internal static uint64 r20(this ref sigctxt c) { return c.regs().regs[20]; }
internal static uint64 r21(this ref sigctxt c) { return c.regs().regs[21]; }
internal static uint64 r22(this ref sigctxt c) { return c.regs().regs[22]; }
internal static uint64 r23(this ref sigctxt c) { return c.regs().regs[23]; }
internal static uint64 r24(this ref sigctxt c) { return c.regs().regs[24]; }
internal static uint64 r25(this ref sigctxt c) { return c.regs().regs[25]; }
internal static uint64 r26(this ref sigctxt c) { return c.regs().regs[26]; }
internal static uint64 r27(this ref sigctxt c) { return c.regs().regs[27]; }
internal static uint64 r28(this ref sigctxt c) { return c.regs().regs[28]; }
internal static uint64 r29(this ref sigctxt c) { return c.regs().regs[29]; }
internal static uint64 lr(this ref sigctxt c) { return c.regs().regs[30]; }
internal static uint64 sp(this ref sigctxt c) { return c.regs().sp; }

//go:nosplit
//go:nowritebarrierrec
internal static uint64 pc(this ref sigctxt c) { return c.regs().pc; }

internal static uint64 pstate(this ref sigctxt c) { return c.regs().pstate; }
internal static uintptr fault(this ref sigctxt c) { return (uintptr)c.regs().fault_address; }

internal static uint64 sigcode(this ref sigctxt c) { return (uint64)c.info.si_code; }
internal static uint64 sigaddr(this ref sigctxt c) { return c.info.si_addr; }

internal static void set_pc(this ref sigctxt c, uint64 x) { c.regs().pc = x; }
internal static void set_sp(this ref sigctxt c, uint64 x) { c.regs().sp = x; }
internal static void set_lr(this ref sigctxt c, uint64 x) { c.regs().regs[30] = x; }
internal static void set_r28(this ref sigctxt c, uint64 x) { c.regs().regs[28] = x; }

internal static void set_sigaddr(this ref sigctxt c, uint64 x) {
    ((ж<uintptr>)(uintptr)((uintptr)add(@unsafe.Pointer.FromPinnedBox(c.info), 2 * goarch.PtrSize))).Value = (uintptr)x;
}

internal static void dumpSigStack(@string s, uintptr sp, uintptr stackhi, uintptr ctx) {
    println(s);
    println((@string)"SP:\t", (Δhex)(uint64)sp);
    println((@string)"ctx:\t", (Δhex)(uint64)ctx);
    var entriesStart = (uintptr)Ꮡ(((ж<ucontext>)(uintptr)((@unsafe.Pointer)ctx)).Value.uc_mcontext).Ꮡ__reserved;
    hexdumpWords(sp, stackhi, (uintptr Δp, hexdumpMarker hm) => {
        switch (Δp) {
        case var ᴛ when ᴛ == ctx: {
            hm.start();
            print((@string)"C");
            println();
            break;
        }
        case var ᴛ when ᴛ == entriesStart: {
            hm.start();
            print((@string)"E");
            println();
            break;
        }}
    });
}

} // end runtime_package
