// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cpu = @internal.cpu_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// Offsets into internal/cpu records for use in assembly.
internal static uintptr offsetX86HasAVX => /* unsafe.Offsetof(cpu.X86.HasAVX) */ 66;
internal static uintptr offsetX86HasAVX2 => /* unsafe.Offsetof(cpu.X86.HasAVX2) */ 68;
internal static uintptr offsetX86HasAVX512 => /* unsafe.Offsetof(cpu.X86.HasAVX512) */ 69; // F+CD+BW+DQ+VL
internal static uintptr offsetX86HasERMS => /* unsafe.Offsetof(cpu.X86.HasERMS) */ 85;
internal static uintptr offsetX86HasRDTSCP => /* unsafe.Offsetof(cpu.X86.HasRDTSCP) */ 92;

internal static uintptr offsetARMHasIDIVA => /* unsafe.Offsetof(cpu.ARM.HasIDIVA) */ 65;

internal static uintptr offsetMIPS64XHasMSA => /* unsafe.Offsetof(cpu.MIPS64X.HasMSA) */ 64;

internal static uintptr offsetLOONG64HasLSX => /* unsafe.Offsetof(cpu.Loong64.HasLSX) */ 64;
internal static uintptr offsetLOONG64HasLASX => /* unsafe.Offsetof(cpu.Loong64.HasLASX) */ 65;

// Set in runtime.cpuinit.
// TODO: deprecate these; use internal/cpu directly.
internal static readonly ж<bool> Ꮡx86HasAVX = new StaticBox<bool>(default(bool));
internal static ref bool x86HasAVX => ref ((StaticBox<bool>)Ꮡx86HasAVX).ValueSlot;
internal static bool x86HasFMA;
internal static bool x86HasPOPCNT;
internal static bool x86HasSSE41;

internal static bool armHasVFPv4;

internal static bool arm64HasATOMICS;

internal static bool loong64HasLAMCAS;
internal static bool loong64HasLAM_BH;
internal static bool loong64HasLSX;

internal static bool riscv64HasZbb;

} // end runtime_package
