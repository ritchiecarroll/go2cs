// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class runtime_package {

// vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
// See cmd/compile/internal/arm64/galign.go arch.MAXWIDTH initialization.
internal static UntypedInt vdsoArrayMax => /* 1<<50 - 1 */ 1125899906842623;

// key and version at man 7 vdso : aarch64
internal static readonly ж<vdsoVersionKey> ᏑvdsoLinuxVersion = new StaticBox<vdsoVersionKey>(new vdsoVersionKey("LINUX_2.6.39", 0x75fcb89));
internal static ref vdsoVersionKey vdsoLinuxVersion => ref ((StaticBox<vdsoVersionKey>)ᏑvdsoLinuxVersion).ValueSlot;

internal static slice<vdsoSymbolKey> vdsoSymbolKeys;
internal static void initᴛvdsoSymbolKeys() { vdsoSymbolKeys = [
    new("__kernel_clock_gettime", 0xb0cd725, 0xdfa941fdU, ᏑvdsoClockgettimeSym),
    new("__kernel_getrandom", 0x9800c0d, 0x540d4e24, ᏑvdsoGetrandomSym)
]; }

public static readonly ж<uintptr> ᏑvdsoClockgettimeSym = new StaticBox<uintptr>(default(uintptr));
public static ref uintptr vdsoClockgettimeSym => ref ((StaticBox<uintptr>)ᏑvdsoClockgettimeSym).ValueSlot;
internal static readonly ж<uintptr> ᏑvdsoGetrandomSym = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr vdsoGetrandomSym => ref ((StaticBox<uintptr>)ᏑvdsoGetrandomSym).ValueSlot;

} // end runtime_package
