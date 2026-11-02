// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.runtime.syscall.windows_package;
using @internal.runtime.syscall;

partial class runtime_package {

internal static void prepareContextForSigResume(ref windows.Context c) {
    c.R8 = c.Rsp;
    c.R9 = c.Rip;
}

internal static void dumpregs(ref windows.Context r) {
    print((@string)"rax     ", (Δhex)r.Rax, (@string)"\n");
    print((@string)"rbx     ", (Δhex)r.Rbx, (@string)"\n");
    print((@string)"rcx     ", (Δhex)r.Rcx, (@string)"\n");
    print((@string)"rdx     ", (Δhex)r.Rdx, (@string)"\n");
    print((@string)"rdi     ", (Δhex)r.Rdi, (@string)"\n");
    print((@string)"rsi     ", (Δhex)r.Rsi, (@string)"\n");
    print((@string)"rbp     ", (Δhex)r.Rbp, (@string)"\n");
    print((@string)"rsp     ", (Δhex)r.Rsp, (@string)"\n");
    print((@string)"r8      ", (Δhex)r.R8, (@string)"\n");
    print((@string)"r9      ", (Δhex)r.R9, (@string)"\n");
    print((@string)"r10     ", (Δhex)r.R10, (@string)"\n");
    print((@string)"r11     ", (Δhex)r.R11, (@string)"\n");
    print((@string)"r12     ", (Δhex)r.R12, (@string)"\n");
    print((@string)"r13     ", (Δhex)r.R13, (@string)"\n");
    print((@string)"r14     ", (Δhex)r.R14, (@string)"\n");
    print((@string)"r15     ", (Δhex)r.R15, (@string)"\n");
    print((@string)"rip     ", (Δhex)r.Rip, (@string)"\n");
    print((@string)"rflags  ", (Δhex)(uint64)r.EFlags, (@string)"\n");
    print((@string)"cs      ", (Δhex)(uint64)r.SegCs, (@string)"\n");
    print((@string)"fs      ", (Δhex)(uint64)r.SegFs, (@string)"\n");
    print((@string)"gs      ", (Δhex)(uint64)r.SegGs, (@string)"\n");
}

} // end runtime_package
