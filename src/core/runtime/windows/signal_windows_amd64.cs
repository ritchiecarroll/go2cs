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
    print((@string)"rax     "u8, (Δhex)r.Rax, (@string)"\n"u8);
    print((@string)"rbx     "u8, (Δhex)r.Rbx, (@string)"\n"u8);
    print((@string)"rcx     "u8, (Δhex)r.Rcx, (@string)"\n"u8);
    print((@string)"rdx     "u8, (Δhex)r.Rdx, (@string)"\n"u8);
    print((@string)"rdi     "u8, (Δhex)r.Rdi, (@string)"\n"u8);
    print((@string)"rsi     "u8, (Δhex)r.Rsi, (@string)"\n"u8);
    print((@string)"rbp     "u8, (Δhex)r.Rbp, (@string)"\n"u8);
    print((@string)"rsp     "u8, (Δhex)r.Rsp, (@string)"\n"u8);
    print((@string)"r8      "u8, (Δhex)r.R8, (@string)"\n"u8);
    print((@string)"r9      "u8, (Δhex)r.R9, (@string)"\n"u8);
    print((@string)"r10     "u8, (Δhex)r.R10, (@string)"\n"u8);
    print((@string)"r11     "u8, (Δhex)r.R11, (@string)"\n"u8);
    print((@string)"r12     "u8, (Δhex)r.R12, (@string)"\n"u8);
    print((@string)"r13     "u8, (Δhex)r.R13, (@string)"\n"u8);
    print((@string)"r14     "u8, (Δhex)r.R14, (@string)"\n"u8);
    print((@string)"r15     "u8, (Δhex)r.R15, (@string)"\n"u8);
    print((@string)"rip     "u8, (Δhex)r.Rip, (@string)"\n"u8);
    print((@string)"rflags  "u8, (Δhex)(uint64)r.EFlags, (@string)"\n"u8);
    print((@string)"cs      "u8, (Δhex)(uint64)r.SegCs, (@string)"\n"u8);
    print((@string)"fs      "u8, (Δhex)(uint64)r.SegFs, (@string)"\n"u8);
    print((@string)"gs      "u8, (Δhex)(uint64)r.SegGs, (@string)"\n"u8);
}

} // end runtime_package
