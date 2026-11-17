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
    print((@string)"rax     ", (hex)r.Rax, (@string)"\n");
    print((@string)"rbx     ", (hex)r.Rbx, (@string)"\n");
    print((@string)"rcx     ", (hex)r.Rcx, (@string)"\n");
    print((@string)"rdx     ", (hex)r.Rdx, (@string)"\n");
    print((@string)"rdi     ", (hex)r.Rdi, (@string)"\n");
    print((@string)"rsi     ", (hex)r.Rsi, (@string)"\n");
    print((@string)"rbp     ", (hex)r.Rbp, (@string)"\n");
    print((@string)"rsp     ", (hex)r.Rsp, (@string)"\n");
    print((@string)"r8      ", (hex)r.R8, (@string)"\n");
    print((@string)"r9      ", (hex)r.R9, (@string)"\n");
    print((@string)"r10     ", (hex)r.R10, (@string)"\n");
    print((@string)"r11     ", (hex)r.R11, (@string)"\n");
    print((@string)"r12     ", (hex)r.R12, (@string)"\n");
    print((@string)"r13     ", (hex)r.R13, (@string)"\n");
    print((@string)"r14     ", (hex)r.R14, (@string)"\n");
    print((@string)"r15     ", (hex)r.R15, (@string)"\n");
    print((@string)"rip     ", (hex)r.Rip, (@string)"\n");
    print((@string)"rflags  ", (hex)(uint64)r.EFlags, (@string)"\n");
    print((@string)"cs      ", (hex)(uint64)r.SegCs, (@string)"\n");
    print((@string)"fs      ", (hex)(uint64)r.SegFs, (@string)"\n");
    print((@string)"gs      ", (hex)(uint64)r.SegGs, (@string)"\n");
}

} // end runtime_package
