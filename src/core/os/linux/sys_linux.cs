// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;

partial class os_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localhostˢ = "localhost"u8;
internal static readonly @string procSysKernelHostnameˢ = "/proc/sys/kernel/hostname"u8;

internal static (@string name, error err) hostname() {
    @string name = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        // Try uname first, as it's only one system call and reading
        // from /proc is not allowed on Android.
        ref var un = ref heap(new syscall.Utsname(), out var Ꮡun);
        err = syscall.Uname(Ꮡun);
        array<byte> buf = new(512);                 // Enough for a DNS name.
        foreach (var (i, b) in un.Nodename[..]) {
            buf[i] = (uint8)b;
            if (b == 0) {
                name = ((@string)(buf.slice(0, i)));
                break;
            }
        }
        // If we got a name and it's not potentially truncated
        // (Nodename is 65 bytes), return it.
        if (err == default! && len(name) > 0 && len(name) < 64) {
            (name, err) = (name, default!); goto ᒐdone;
        }
        if (runtime.GOOS == "android"u8) {
            if (name != ""u8) {
                (name, err) = (name, default!); goto ᒐdone;
            }
            (name, err) = (localhostˢ, default!); goto ᒐdone;
        }
        (var f, err) = Open(procSysKernelHostnameˢ);
        if (err != default!) {
            (name, err) = ("", err); goto ᒐdone;
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        (var n, err) = f.Read(buf[..]);
        if (err != default!) {
            (name, err) = ("", err); goto ᒐdone;
        }
        if (n > 0 && buf[n - 1] == (rune)'\n') {
            n--;
        }
        (name, err) = (((@string)(buf.slice(0, n))), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (name, err);
}

} // end os_package
