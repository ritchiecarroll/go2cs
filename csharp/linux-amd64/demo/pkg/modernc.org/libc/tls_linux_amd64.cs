// Copyright 2025 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

partial class libc_package {

public static partial uintptr TLSAlloc(ж<TLS> p0, nint p1);

public static partial void TLSFree(ж<TLS> p0, nint p1);

public static partial void TLSAllocaEntry(ж<TLS> p0);

public static partial void TLSAllocaExit(ж<TLS> p0);

internal static partial uintptr tlsAlloc(ж<TLS> tls, nint n) {
    return tls.Alloc(n);
}

internal static partial void tlsFree(ж<TLS> tls, nint n) {
    tls.Free(n);
}

internal static void tlsAllocaEntry(ж<TLS> Ꮡtls) {
    ref var tls = ref Ꮡtls.DerefOrNull();

    tls.AllocaEntry();
}

internal static partial void tlsAllocaExit(ж<TLS> tls) {
    tls.AllocaExit();
}

} // end libc_package
