// cpu_darwin_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written bodies for internal/cpu's two darwin runtime pushes. cpu_darwin.cs carries Go's bodyless
// `sysctlbynameInt32` and `sysctlbynameBytes` ("Pushed from runtime."); Go 1.26's runtime pushes them from
// os_darwin.go (`//go:linkname internal_cpu_sysctlbynameInt32 internal/cpu.sysctlbynameInt32` and its
// Bytes twin), whose bodies call runtime.sysctlbyname, a libcCall of libSystem's sysctlbyname(3).
//
// The push cannot be honored as a forward: runtime imports internal/cpu, so internal/cpu cannot name the
// runtime's wrapper. This file makes the same libc call itself, through golib's dispatcher. Go's trampoline
// tests nothing and reads no errno (sys_darwin_amd64.s sysctlbyname_trampoline), so neither does this.
// sysctlbyname is not variadic, so Apple silicon's stack-slot rule does not apply.
//
// internal.cpu.csproj compiles with AllowUnsafeBlocks=false, so the name, the output buffer and the
// length word cross in one unmanaged block rather than as pinned managed addresses: the name is copied
// in, the output buffer is copied in and back out whole, and the length word is set to len(out) as Go's
// `nout` is. Go's `&name[0]` and `&out[0]` panic on an empty slice; the index reads below do the same.
[module: go.GoManualConversion]

namespace go.@internal;

using System;
using System.Runtime.InteropServices;

partial class cpu_package {

private const string libSystemPath = "/usr/lib/libSystem.B.dylib";

private static nint s_sysctlbyname;

// sysctlbyname(name, oldp, &oldlen, NULL, 0) with oldp a native copy of `out`, which is written back.
private static int32 sysctlbynameNative(slice<byte> name, Span<byte> @out) {
    if (s_sysctlbyname == 0) {
        s_sysctlbyname = GoCgoDynamicImports.Resolve("sysctlbyname", libSystemPath);
    }

    ReadOnlySpan<byte> nameBytes = name.ToSpan();
    int outOffset = nameBytes.Length;
    int lengthOffset = (outOffset + @out.Length + 7) & ~7;
    nint block = Marshal.AllocHGlobal(lengthOffset + nint.Size);

    try {
        Marshal.Copy(nameBytes.ToArray(), 0, block, nameBytes.Length);
        Marshal.Copy(@out.ToArray(), 0, block + outOffset, @out.Length);
        Marshal.WriteIntPtr(block + lengthOffset, @out.Length);

        nuint ret = GoLibcCall.Call(s_sysctlbyname, [(nuint)block, (nuint)(block + outOffset), (nuint)(block + lengthOffset), 0, 0], GoLibcErrnoRule.None, 0, out _);

        byte[] written = new byte[@out.Length];
        Marshal.Copy(block + outOffset, written, 0, written.Length);
        written.CopyTo(@out);

        return unchecked((int32)(uint)ret);
    }
    finally {
        Marshal.FreeHGlobal(block);
    }
}

internal static partial (int32, int32) sysctlbynameInt32(slice<byte> name) {
    _ = name[0];

    Span<byte> @out = stackalloc byte[sizeof(int)];
    @out.Clear();

    int32 ret = sysctlbynameNative(name, @out);
    return (ret, BitConverter.ToInt32(@out));
}

internal static partial int32 sysctlbynameBytes(slice<byte> name, slice<byte> @out) {
    _ = name[0];
    _ = @out[0];

    return sysctlbynameNative(name, @out.ToSpan());
}

} // end cpu_package
