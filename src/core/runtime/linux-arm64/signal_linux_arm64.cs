// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;
using static runtime_package;

partial class runtime_package {

partial struct sigctxt {
    internal ж<siginfo> info;
    internal @unsafe.Pointer ctxt;
}

} partial class runtime_package {

//go:nosplit
//go:nowritebarrierrec
[GoMethodBody("regs")] internal static ж<sigcontext> regsᴍ(ref sigctxt c) { return @unsafe.Pointer.To<ucontext>(c.ctxt).Ꮡuc_mcontext; }

[GoMethodBody("r0")] internal static uint64 r0ᴍ(ref sigctxt c) { return c.regs().regs[0]; }
[GoMethodBody("r1")] internal static uint64 r1ᴍ(ref sigctxt c) { return c.regs().regs[1]; }
[GoMethodBody("r2")] internal static uint64 r2ᴍ(ref sigctxt c) { return c.regs().regs[2]; }
[GoMethodBody("r3")] internal static uint64 r3ᴍ(ref sigctxt c) { return c.regs().regs[3]; }
[GoMethodBody("r4")] internal static uint64 r4ᴍ(ref sigctxt c) { return c.regs().regs[4]; }
[GoMethodBody("r5")] internal static uint64 r5ᴍ(ref sigctxt c) { return c.regs().regs[5]; }
[GoMethodBody("r6")] internal static uint64 r6ᴍ(ref sigctxt c) { return c.regs().regs[6]; }
[GoMethodBody("r7")] internal static uint64 r7ᴍ(ref sigctxt c) { return c.regs().regs[7]; }
[GoMethodBody("r8")] internal static uint64 r8ᴍ(ref sigctxt c) { return c.regs().regs[8]; }
[GoMethodBody("r9")] internal static uint64 r9ᴍ(ref sigctxt c) { return c.regs().regs[9]; }
[GoMethodBody("r10")] internal static uint64 r10ᴍ(ref sigctxt c) { return c.regs().regs[10]; }
[GoMethodBody("r11")] internal static uint64 r11ᴍ(ref sigctxt c) { return c.regs().regs[11]; }
[GoMethodBody("r12")] internal static uint64 r12ᴍ(ref sigctxt c) { return c.regs().regs[12]; }
[GoMethodBody("r13")] internal static uint64 r13ᴍ(ref sigctxt c) { return c.regs().regs[13]; }
[GoMethodBody("r14")] internal static uint64 r14ᴍ(ref sigctxt c) { return c.regs().regs[14]; }
[GoMethodBody("r15")] internal static uint64 r15ᴍ(ref sigctxt c) { return c.regs().regs[15]; }
[GoMethodBody("r16")] internal static uint64 r16ᴍ(ref sigctxt c) { return c.regs().regs[16]; }
[GoMethodBody("r17")] internal static uint64 r17ᴍ(ref sigctxt c) { return c.regs().regs[17]; }
[GoMethodBody("r18")] internal static uint64 r18ᴍ(ref sigctxt c) { return c.regs().regs[18]; }
[GoMethodBody("r19")] internal static uint64 r19ᴍ(ref sigctxt c) { return c.regs().regs[19]; }
[GoMethodBody("r20")] internal static uint64 r20ᴍ(ref sigctxt c) { return c.regs().regs[20]; }
[GoMethodBody("r21")] internal static uint64 r21ᴍ(ref sigctxt c) { return c.regs().regs[21]; }
[GoMethodBody("r22")] internal static uint64 r22ᴍ(ref sigctxt c) { return c.regs().regs[22]; }
[GoMethodBody("r23")] internal static uint64 r23ᴍ(ref sigctxt c) { return c.regs().regs[23]; }
[GoMethodBody("r24")] internal static uint64 r24ᴍ(ref sigctxt c) { return c.regs().regs[24]; }
[GoMethodBody("r25")] internal static uint64 r25ᴍ(ref sigctxt c) { return c.regs().regs[25]; }
[GoMethodBody("r26")] internal static uint64 r26ᴍ(ref sigctxt c) { return c.regs().regs[26]; }
[GoMethodBody("r27")] internal static uint64 r27ᴍ(ref sigctxt c) { return c.regs().regs[27]; }
[GoMethodBody("r28")] internal static uint64 r28ᴍ(ref sigctxt c) { return c.regs().regs[28]; }
[GoMethodBody("r29")] internal static uint64 r29ᴍ(ref sigctxt c) { return c.regs().regs[29]; }
[GoMethodBody("lr")] internal static uint64 lrᴍ(ref sigctxt c) { return c.regs().regs[30]; }
[GoMethodBody("sp")] internal static uint64 spᴍ(ref sigctxt c) { return c.regs().sp; }

//go:nosplit
//go:nowritebarrierrec
[GoMethodBody("pc")] internal static uint64 pcᴍ(ref sigctxt c) { return c.regs().pc; }

[GoMethodBody("pstate")] internal static uint64 pstateᴍ(ref sigctxt c) { return c.regs().pstate; }
[GoMethodBody("fault")] internal static uintptr faultᴍ(ref sigctxt c) { return (uintptr)c.regs().fault_address; }

[GoMethodBody("sigcode")] internal static uint64 sigcodeᴍ(ref sigctxt c) { return (uint64)c.info.si_code; }
[GoMethodBody("sigaddr")] internal static uint64 sigaddrᴍ(ref sigctxt c) { return c.info.si_addr; }

[GoMethodBody("set_pc")] internal static void set_pcᴍ(ref sigctxt c, uint64 x) { c.regs().pc = x; }
[GoMethodBody("set_sp")] internal static void set_spᴍ(ref sigctxt c, uint64 x) { c.regs().sp = x; }
[GoMethodBody("set_lr")] internal static void set_lrᴍ(ref sigctxt c, uint64 x) { c.regs().regs[30] = x; }
[GoMethodBody("set_r28")] internal static void set_r28ᴍ(ref sigctxt c, uint64 x) { c.regs().regs[28] = x; }

[GoMethodBody("set_sigaddr")] internal static void set_sigaddrᴍ(ref sigctxt c, uint64 x) {
    (@unsafe.Pointer.To<uintptr>(add(@unsafe.Pointer.FromPinnedBox(c.info), 2 * goarch.PtrSize))).Value = (uintptr)x;
}

} partial class runtime_package {

internal static void dumpSigStack(@string s, uintptr sp, uintptr stackhi, uintptr ctx) {
    println(s);
    println((@string)"SP:\t", (hex)(uint64)sp);
    println((@string)"ctx:\t", (hex)(uint64)ctx);
    var entriesStart = (uintptr)@unsafe.Pointer.To<ucontext>((@unsafe.Pointer)ctx).Ꮡuc_mcontext.Ꮡ__reserved;
    hexdumpWords(sp, stackhi, (uintptr p, hexdumpMarker hm) => {
        switch (p) {
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
