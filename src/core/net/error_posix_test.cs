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

[GoType("dyn")] internal partial struct TestSpuriousENOTAVAIL_type {
    [GoEmbedded] internal error error;
    internal bool ok;
}

public static void TestSpuriousENOTAVAIL(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in new TestSpuriousENOTAVAIL_type[]{
        new(syscall.EADDRNOTAVAIL, true),
        new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall"u8, Err: syscall.EADDRNOTAVAIL))), true),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op"u8, Err: syscall.EADDRNOTAVAIL))), true),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op"u8, Err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall"u8, Err: syscall.EADDRNOTAVAIL)))))), true),
        new(syscall.EINVAL, false),
        new(new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall"u8, Err: syscall.EINVAL))), false),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op"u8, Err: syscall.EINVAL))), false),
        new(new global::go.net_package.OpErrorжerror(Ꮡ(new OpError(Op: "op"u8, Err: new Δos.SyscallErrorжerror(Ꮡ(new Δos.SyscallError(Syscall: "syscall"u8, Err: syscall.EINVAL)))))), false)
    }.slice()) {
        {
            var ok = spuriousENOTAVAIL(tt.error); if (ok != tt.ok) {
                Ꮡt.Errorf("spuriousENOTAVAIL(%v) = %v; want %v"u8, tt.error, ok, tt.ok);
            }
        }
    }
}

} // end net_internal_test_package
