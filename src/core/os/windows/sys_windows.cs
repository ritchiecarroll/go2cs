// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal.syscall;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string computerNameExˢ = "ComputerNameEx"u8;

internal static (@string name, error err) hostname() {
    // Use PhysicalDnsHostname to uniquely identify host in a cluster
    const uint32 format = /* windows.ComputerNamePhysicalDnsHostname */ 5;
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)64;
    while (ᐧ) {
        var b = new slice<uint16>((nint)(n));
        var errΔ1 = windows.GetComputerNameEx(format, Ꮡ(b, 0), Ꮡn);
        if (errΔ1 == default!) {
            return (syscall.UTF16ToString(b.slice(0, (nint)(n))), default!);
        }
        if (!AreEqual(errΔ1, syscall.ERROR_MORE_DATA)) {
            return ("", NewSyscallError(computerNameExˢ, errΔ1));
        }
        // If we received an ERROR_MORE_DATA, but n doesn't get larger,
        // something has gone wrong and we may be in an infinite loop
        if (n <= (uint32)len(b)) {
            return ("", NewSyscallError(computerNameExˢ, errΔ1));
        }
    }
}

} // end os_package
