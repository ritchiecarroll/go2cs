// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !plan9
namespace go;

using Δos = os_package;
using syscall = syscall_package;
using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

internal partial struct TestSpuriousENOTAVAIL_type /*dyn*/ {
    /*embed*/ internal error error;
    internal bool ok;
}

public static void TestSpuriousENOTAVAIL(ж<testing.T> t) {
    foreach (var (_, tt) in range(slice<TestSpuriousENOTAVAIL_type>([
        new(syscall.EADDRNOTAVAIL, true),
        new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall", Err: syscall.EADDRNOTAVAIL))), true),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op", Err: syscall.EADDRNOTAVAIL))), true),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op", Err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall", Err: syscall.EADDRNOTAVAIL)))))), true),

        new(syscall.EINVAL, false),
        new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall", Err: syscall.EINVAL))), false),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op", Err: syscall.EINVAL))), false),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op", Err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall", Err: syscall.EINVAL)))))), false)
    ]))) {
        var ok = spuriousENOTAVAIL(tt.error); if (ok != tt.ok) {
            t.Errorf("spuriousENOTAVAIL(%v) = %v; want %v", tt.error, ok, tt.ok);
        }
    }
}

} // end net_internal_test_package
