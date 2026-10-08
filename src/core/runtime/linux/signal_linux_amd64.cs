// Copyright 2013 The Go Authors. All rights reserved.
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
internal static ж<sigcontext> regs(this ref sigctxt c) {
    return Ꮡ(((ж<ucontext>)(uintptr)(c.ctxt)).Value.uc_mcontext).Reinterpret<mcontext, sigcontext>();
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
    return (~c.regs()).eflags;
}

internal static uint64 cs(this ref sigctxt c) {
    return (uint64)(~c.regs()).cs;
}

internal static uint64 fs(this ref sigctxt c) {
    return (uint64)(~c.regs()).fs;
}

internal static uint64 gs(this ref sigctxt c) {
    return (uint64)(~c.regs()).gs;
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
    ((ж<uintptr>)(uintptr)((uintptr)add(@unsafe.Pointer.FromPinnedBox(c.info), 2 * goarch.PtrSize))).Value = (uintptr)x;
}

} // end runtime_package
