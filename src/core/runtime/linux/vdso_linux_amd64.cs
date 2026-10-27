// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname

partial class runtime_package {

// vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
// See cmd/compile/internal/amd64/galign.go arch.MAXWIDTH initialization.
internal static UntypedInt vdsoArrayMax => /* 1<<50 - 1 */ 1125899906842623;

internal static ж<vdsoVersionKey> ᏑvdsoLinuxVersion = new StaticBox<vdsoVersionKey>(new vdsoVersionKey("LINUX_2.6"u8, 0x3ae75f6));
internal static ref vdsoVersionKey vdsoLinuxVersion => ref ᏑvdsoLinuxVersion.Value;

internal static slice<vdsoSymbolKey> vdsoSymbolKeys;
internal static void initᴛvdsoSymbolKeys() { vdsoSymbolKeys = new vdsoSymbolKey[]{
    new("__vdso_gettimeofday"u8, 0x315ca59, 0xb01bca00U, ᏑvdsoGettimeofdaySym),
    new("__vdso_clock_gettime"u8, 0xd35ec75, 0x6e43a318, ᏑvdsoClockgettimeSym),
    new("__vdso_getrandom"u8, 0x25425d, 0x84a559bfU, ᏑvdsoGetrandomSym)
}.slice(); }

public static ж<uintptr> ᏑvdsoGettimeofdaySym = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr vdsoGettimeofdaySym => ref ᏑvdsoGettimeofdaySym.Value;
public static ж<uintptr> ᏑvdsoClockgettimeSym = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr vdsoClockgettimeSym => ref ᏑvdsoClockgettimeSym.Value;
internal static ж<uintptr> ᏑvdsoGetrandomSym = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr vdsoGetrandomSym => ref ᏑvdsoGetrandomSym.Value;

// vdsoGettimeofdaySym is accessed from the syscall package.
//go:linkname vdsoGettimeofdaySym

} // end runtime_package
