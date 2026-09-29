// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static UntypedInt _MEM_COMMIT => 0x1000;
internal static UntypedInt _MEM_RESERVE => 0x2000;
internal static UntypedInt _MEM_DECOMMIT => 0x4000;
internal static UntypedInt _MEM_RELEASE => 0x8000;
internal static UntypedInt _PAGE_READWRITE => 0x0004;
internal static UntypedInt _PAGE_NOACCESS => 0x0001;
internal static UntypedInt _ERROR_NOT_ENOUGH_MEMORY => 8;
internal static UntypedInt _ERROR_COMMITMENT_LIMIT => 1455;

// go2cs generated this placeholder — func sysAllocOS is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func sysUnusedOS is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func sysUsedOS is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void sysHugePageOS(@unsafe.Pointer v, uintptr n) {
}

internal static void sysNoHugePageOS(@unsafe.Pointer v, uintptr n) {
}

internal static void sysHugePageCollapseOS(@unsafe.Pointer v, uintptr n) {
}

// go2cs generated this placeholder — func sysFreeOS is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void sysFaultOS(@unsafe.Pointer v, uintptr n) {
    // SysUnused makes the memory inaccessible and prevents its reuse
    sysUnusedOS(v, n);
}

// go2cs generated this placeholder — func sysReserveOS is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void sysMapOS(@unsafe.Pointer v, uintptr n) {
}

} // end runtime_package
