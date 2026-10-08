// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

partial struct InterfaceSwitch {
    public ж<InterfaceSwitchCache> Cache;
    public nint NCases;
    // Array of NCases elements.
    // Each case must be a non-empty interface type.
    public array<ж<ΔInterfaceType>> Cases = new(1);
}

partial struct InterfaceSwitchCache {
    public uintptr Mask;                      // mask for index. Must be a power of 2 minus 1
    public array<InterfaceSwitchCacheEntry> Entries = new(1); // Mask+1 entries total
}

partial struct InterfaceSwitchCacheEntry {
    // type of source value (a *Type)
    public uintptr Typ;
    // case # to dispatch to
    public nint Case;
    // itab to use for resulting case variable (a *runtime.itab)
    public uintptr Itab;
}

internal const bool go122InterfaceSwitchCache = true;

public static bool UseInterfaceSwitchCache(@string goarch) {
    if (!go122InterfaceSwitchCache) {
        return false;
    }
    // We need an atomic load instruction to make the cache multithreaded-safe.
    // (AtomicLoadPtr needs to be implemented in cmd/compile/internal/ssa/_gen/ARCH.rules.)
    var exprᴛ1 = goarch;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "riscv64"u8 || exprᴛ1 == "s390x"u8) {
        return true;
    }
    { /* default: */
        return false;
    }

}

partial struct TypeAssert {
    public ж<TypeAssertCache> Cache;
    public ж<ΔInterfaceType> Inter;
    public bool CanFail;
}

partial struct TypeAssertCache {
    public uintptr Mask;
    public array<TypeAssertCacheEntry> Entries = new(1);
}

partial struct TypeAssertCacheEntry {
    // type of source value (a *runtime._type)
    public uintptr Typ;
    // itab to use for result (a *runtime.itab)
    // nil if CanFail is set and conversion would fail.
    public uintptr Itab;
}

} // end abi_package
