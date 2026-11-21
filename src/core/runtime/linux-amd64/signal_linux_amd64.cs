// Copyright 2013 The Go Authors. All rights reserved.
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
[GoMethodBody("regs")] internal static ж<sigcontext> regsᴍ(ref sigctxt c) {
    return @unsafe.Pointer.To<ucontext>(c.ctxt).Ꮡuc_mcontext.Reinterpret<mcontext, sigcontext>();
}

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

[GoMethodBody("rflags")] internal static uint64 rflagsᴍ(ref sigctxt c) { return c.regs().eflags; }
[GoMethodBody("cs")] internal static uint64 csᴍ(ref sigctxt c) { return (uint64)c.regs().cs; }
[GoMethodBody("fs")] internal static uint64 fsᴍ(ref sigctxt c) { return (uint64)c.regs().fs; }
[GoMethodBody("gs")] internal static uint64 gsᴍ(ref sigctxt c) { return (uint64)c.regs().gs; }
[GoMethodBody("sigcode")] internal static uint64 sigcodeᴍ(ref sigctxt c) { return (uint64)c.info.si_code; }
[GoMethodBody("sigaddr")] internal static uint64 sigaddrᴍ(ref sigctxt c) { return c.info.si_addr; }

[GoMethodBody("set_rip")] internal static void set_ripᴍ(ref sigctxt c, uint64 x) { c.regs().rip = x; }
[GoMethodBody("set_rsp")] internal static void set_rspᴍ(ref sigctxt c, uint64 x) { c.regs().rsp = x; }
[GoMethodBody("set_sigcode")] internal static void set_sigcodeᴍ(ref sigctxt c, uint64 x) { c.info.si_code = (int32)x; }
[GoMethodBody("set_sigaddr")] internal static void set_sigaddrᴍ(ref sigctxt c, uint64 x) {
    (@unsafe.Pointer.To<uintptr>(add(@unsafe.Pointer.FromPinnedBox(c.info), 2 * goarch.PtrSize))).Value = (uintptr)x;
}

} partial class runtime_package {

// dumpSigStack prints a signal stack with the context, fpstate pointer field within that context and
// the beginning of the fpstate annotated by C/F/S respectively
internal static void dumpSigStack(@string s, uintptr sp, uintptr stackhi, uintptr ctx) {
    println(s);
    println((@string)"SP:\t", (hex)(uint64)sp);
    println((@string)"ctx:\t", (hex)(uint64)ctx);
    var fpfield = ctx + /* unsafe.Offsetof(ucontext{}.uc_mcontext) */ (uintptr)40 + /* unsafe.Offsetof(mcontext{}.fpregs) */ (uintptr)184;
    println((@string)"fpfield:\t", (hex)(uint64)fpfield);
    var fpbegin = (uintptr)(Ꮡ(new sigctxt(nil, (@unsafe.Pointer)ctx))).regs().fpstate;
    println((@string)"fpstate:\t", (hex)(uint64)fpbegin);
    hexdumpWords(sp, stackhi, (uintptr p, hexdumpMarker hm) => {
        switch (p) {
        case var ᴛ when ᴛ == ctx: {
            hm.start();
            print((@string)"C");
            println();
            break;
        }
        case var ᴛ when ᴛ == fpfield: {
            hm.start();
            print((@string)"F");
            println();
            break;
        }
        case var ᴛ when ᴛ == fpbegin: {
            hm.start();
            print((@string)"S");
            println();
            break;
        }}
    });
}

} // end runtime_package
