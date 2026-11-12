// M:N PROTOTYPE (A1): the hand-owned native side of the never-twinned red. This file has no `.go` counterpart.
//
// The wrappers below are written the way a cgo-generated wrapper is: no `unsafe` modifier on the method, an inner
// `unsafe` block, the Go memory pinned with `fixed` for the length of the native call, and the call bracketed by
// GoNativeCall.Enter and Exit. `fill` also calls something that has a twin (a channel send) inside the pinned scope,
// which is what makes the twin generator want to write a twin for it.

using System;
using System.Runtime.InteropServices;
using go.golib;

[module: go.GoRequiresUnsafe]

namespace go;

partial class main_package {

// A cgo-style wrapper whose pinned scope holds a call that can park.
internal static nint fill(slice<byte> buf, nint seed) {
    nint sum;

    unsafe {
        fixed (byte* p = buf.ꓸꓸꓸ) {
            side.ᐸꟷ(seed);
            GoNativeCall.Enter();

            try {
                sum = ((delegate* unmanaged<byte*, int, int, int>)&nativeFill)(p, (int)len(buf), (int)seed);
            } finally {
                GoNativeCall.Exit();
            }
        }
    }

    return sum;
}

// A cgo-style wrapper that pins and calls nothing that can park.
internal static nint count(slice<byte> buf) {
    nint n;

    unsafe {
        fixed (byte* p = buf.ꓸꓸꓸ) {
            GoNativeCall.Enter();

            try {
                n = ((delegate* unmanaged<byte*, int, int>)&nativeCount)(p, (int)len(buf));
            } finally {
                GoNativeCall.Exit();
            }
        }
    }

    return n;
}

// M:N PROTOTYPE (A2): an `unsafe` block with no `fixed` and no `unsafe` modifier on the method, holding a call that
// can park. A twin of it awaits inside an unsafe context, which C# refuses (CS4004), so it is never twinned either.
internal static nint peek(nint seed) {
    nint echo;

    unsafe {
        nint local = seed;
        nint* q = &local;

        side.ᐸꟷ(seed + 1);
        echo = *q;
    }

    return echo;
}

// A wrapper over a LibraryImport extern. The extern is Windows only; elsewhere the wrapper answers without it.
internal static nint processId() {
    if (OperatingSystem.IsWindows()) {
        return (nint)GetCurrentProcessId();
    }

    return 1;
}

[LibraryImport("kernel32.dll", EntryPoint = "GetCurrentProcessId")]
private static partial uint GetCurrentProcessId();

// The native side, reached through an unmanaged function pointer so the red runs on every platform.
[UnmanagedCallersOnly]
private static unsafe int nativeFill(byte* p, int n, int seed) {
    int sum = 0;

    for (int i = 0; i < n; i++) {
        p[i] = (byte)(seed + i);
        sum += p[i];
    }

    return sum;
}

[UnmanagedCallersOnly]
private static unsafe int nativeCount(byte* p, int n) {
    int nonzero = 0;

    for (int i = 0; i < n; i++) {
        if (p[i] != 0) {
            nonzero++;
        }
    }

    return nonzero;
}

} // end main_package
