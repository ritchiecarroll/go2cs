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
internal static ж<sigcontext> regsᴍ(ref sigctxt c) {
    return @unsafe.Pointer.To<ucontext>(c.ctxt).Ꮡuc_mcontext.Reinterpret<mcontext, sigcontext>();
}

internal static uint64 raxᴍ(ref sigctxt c) { return c.regs().rax; }
internal static uint64 rbxᴍ(ref sigctxt c) { return c.regs().rbx; }
internal static uint64 rcxᴍ(ref sigctxt c) { return c.regs().rcx; }
internal static uint64 rdxᴍ(ref sigctxt c) { return c.regs().rdx; }
internal static uint64 rdiᴍ(ref sigctxt c) { return c.regs().rdi; }
internal static uint64 rsiᴍ(ref sigctxt c) { return c.regs().rsi; }
internal static uint64 rbpᴍ(ref sigctxt c) { return c.regs().rbp; }
internal static uint64 rspᴍ(ref sigctxt c) { return c.regs().rsp; }
internal static uint64 r8ᴍ(ref sigctxt c) { return c.regs().r8; }
internal static uint64 r9ᴍ(ref sigctxt c) { return c.regs().r9; }
internal static uint64 r10ᴍ(ref sigctxt c) { return c.regs().r10; }
internal static uint64 r11ᴍ(ref sigctxt c) { return c.regs().r11; }
internal static uint64 r12ᴍ(ref sigctxt c) { return c.regs().r12; }
internal static uint64 r13ᴍ(ref sigctxt c) { return c.regs().r13; }
internal static uint64 r14ᴍ(ref sigctxt c) { return c.regs().r14; }
internal static uint64 r15ᴍ(ref sigctxt c) { return c.regs().r15; }

//go:nosplit
//go:nowritebarrierrec
internal static uint64 ripᴍ(ref sigctxt c) { return c.regs().rip; }

internal static uint64 rflagsᴍ(ref sigctxt c) { return c.regs().eflags; }
internal static uint64 csᴍ(ref sigctxt c) { return (uint64)c.regs().cs; }
internal static uint64 fsᴍ(ref sigctxt c) { return (uint64)c.regs().fs; }
internal static uint64 gsᴍ(ref sigctxt c) { return (uint64)c.regs().gs; }
internal static uint64 sigcodeᴍ(ref sigctxt c) { return (uint64)c.info.si_code; }
internal static uint64 sigaddrᴍ(ref sigctxt c) { return c.info.si_addr; }

internal static void set_ripᴍ(ref sigctxt c, uint64 x) { c.regs().rip = x; }
internal static void set_rspᴍ(ref sigctxt c, uint64 x) { c.regs().rsp = x; }
internal static void set_sigcodeᴍ(ref sigctxt c, uint64 x) { c.info.si_code = (int32)x; }
internal static void set_sigaddrᴍ(ref sigctxt c, uint64 x) {
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

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class runtimeᴛsigctxt {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static ж<sigcontext> regs(this ref sigctxt c) => runtime_package.regsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rax(this ref sigctxt c) => runtime_package.raxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rbx(this ref sigctxt c) => runtime_package.rbxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rcx(this ref sigctxt c) => runtime_package.rcxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rdx(this ref sigctxt c) => runtime_package.rdxᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rdi(this ref sigctxt c) => runtime_package.rdiᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rsi(this ref sigctxt c) => runtime_package.rsiᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rbp(this ref sigctxt c) => runtime_package.rbpᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rsp(this ref sigctxt c) => runtime_package.rspᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r8(this ref sigctxt c) => runtime_package.r8ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r9(this ref sigctxt c) => runtime_package.r9ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r10(this ref sigctxt c) => runtime_package.r10ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r11(this ref sigctxt c) => runtime_package.r11ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r12(this ref sigctxt c) => runtime_package.r12ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r13(this ref sigctxt c) => runtime_package.r13ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r14(this ref sigctxt c) => runtime_package.r14ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 r15(this ref sigctxt c) => runtime_package.r15ᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rip(this ref sigctxt c) => runtime_package.ripᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 rflags(this ref sigctxt c) => runtime_package.rflagsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 cs(this ref sigctxt c) => runtime_package.csᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 fs(this ref sigctxt c) => runtime_package.fsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 gs(this ref sigctxt c) => runtime_package.gsᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 sigcode(this ref sigctxt c) => runtime_package.sigcodeᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static uint64 sigaddr(this ref sigctxt c) => runtime_package.sigaddrᴍ(ref c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_rip(this ref sigctxt c, uint64 x) => runtime_package.set_ripᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_rsp(this ref sigctxt c, uint64 x) => runtime_package.set_rspᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_sigcode(this ref sigctxt c, uint64 x) => runtime_package.set_sigcodeᴍ(ref c, x);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void set_sigaddr(this ref sigctxt c, uint64 x) => runtime_package.set_sigaddrᴍ(ref c, x);
}
// </GlyphForwarders>
