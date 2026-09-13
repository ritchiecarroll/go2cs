// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class main_package {

[GoType] partial struct InterfaceSwitch {
    public ж<InterfaceSwitchCache> Cache;
    public nint NCases;
    public array<ж<ΔInterfaceType>> Cases = new(1);
}

[GoType] partial struct InterfaceSwitchCache {
    public uintptr Mask;
    public array<InterfaceSwitchCacheEntry> Entries = new(1);
}

[GoType] partial struct InterfaceSwitchCacheEntry {
    public uintptr Typ;
    public nint Case;
    public uintptr Itab;
}

internal const bool go122InterfaceSwitchCache = true;

public static bool UseInterfaceSwitchCache(@string goarch) {
    if (!go122InterfaceSwitchCache) {
        return false;
    }
    var exprᴛ1 = goarch;
    if (exprᴛ1 == "amd64"u8 || exprᴛ1 == "arm64"u8 || exprᴛ1 == "loong64"u8 || exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8 || exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8 || exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8 || exprᴛ1 == "riscv64"u8 || exprᴛ1 == "s390x"u8) {
        return true;
    }
    { /* default: */
        return false;
    }

}

[GoType] partial struct TypeAssert {
    public ж<TypeAssertCache> Cache;
    public ж<ΔInterfaceType> Inter;
    public bool CanFail;
}

[GoType] partial struct TypeAssertCache {
    public uintptr Mask;
    public array<TypeAssertCacheEntry> Entries = new(1);
}

[GoType] partial struct TypeAssertCacheEntry {
    public uintptr Typ;
    public uintptr Itab;
}

} // end main_package
