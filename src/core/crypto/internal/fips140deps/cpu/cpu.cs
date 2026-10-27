// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140deps;

using cpu = go.@internal.cpu_package;
using goarch = go.@internal.goarch_package;
using go.@internal;

partial class cpu_package {

public const bool BigEndian = /* goarch.BigEndian */ false;
public const bool AMD64 = /* goarch.IsAmd64 == 1 */ true;
public const bool ARM64 = /* goarch.IsArm64 == 1 */ false;
public const bool PPC64 = /* goarch.IsPpc64 == 1 */ false;
public const bool PPC64le = /* goarch.IsPpc64le == 1 */ false;

public static readonly ж<bool> ᏑARM64HasAES = new StaticBox<bool>(cpu.ARM64.HasAES);
public static ref bool ARM64HasAES => ref ((StaticBox<bool>)ᏑARM64HasAES).ValueSlot;
public static readonly ж<bool> ᏑARM64HasPMULL = new StaticBox<bool>(cpu.ARM64.HasPMULL);
public static ref bool ARM64HasPMULL => ref ((StaticBox<bool>)ᏑARM64HasPMULL).ValueSlot;
public static readonly ж<bool> ᏑARM64HasSHA2 = new StaticBox<bool>(cpu.ARM64.HasSHA2);
public static ref bool ARM64HasSHA2 => ref ((StaticBox<bool>)ᏑARM64HasSHA2).ValueSlot;
public static readonly ж<bool> ᏑARM64HasSHA512 = new StaticBox<bool>(cpu.ARM64.HasSHA512);
public static ref bool ARM64HasSHA512 => ref ((StaticBox<bool>)ᏑARM64HasSHA512).ValueSlot;
public static readonly ж<bool> ᏑARM64HasSHA3 = new StaticBox<bool>(cpu.ARM64.HasSHA3);
public static ref bool ARM64HasSHA3 => ref ((StaticBox<bool>)ᏑARM64HasSHA3).ValueSlot;

public static readonly ж<bool> ᏑLOONG64HasLSX = new StaticBox<bool>(cpu.Loong64.HasLSX);
public static ref bool LOONG64HasLSX => ref ((StaticBox<bool>)ᏑLOONG64HasLSX).ValueSlot;
public static readonly ж<bool> ᏑLOONG64HasLASX = new StaticBox<bool>(cpu.Loong64.HasLASX);
public static ref bool LOONG64HasLASX => ref ((StaticBox<bool>)ᏑLOONG64HasLASX).ValueSlot;

public static readonly ж<bool> ᏑS390XHasAES = new StaticBox<bool>(cpu.S390X.HasAES);
public static ref bool S390XHasAES => ref ((StaticBox<bool>)ᏑS390XHasAES).ValueSlot;
public static readonly ж<bool> ᏑS390XHasAESCBC = new StaticBox<bool>(cpu.S390X.HasAESCBC);
public static ref bool S390XHasAESCBC => ref ((StaticBox<bool>)ᏑS390XHasAESCBC).ValueSlot;
public static readonly ж<bool> ᏑS390XHasAESCTR = new StaticBox<bool>(cpu.S390X.HasAESCTR);
public static ref bool S390XHasAESCTR => ref ((StaticBox<bool>)ᏑS390XHasAESCTR).ValueSlot;
public static readonly ж<bool> ᏑS390XHasAESGCM = new StaticBox<bool>(cpu.S390X.HasAESGCM);
public static ref bool S390XHasAESGCM => ref ((StaticBox<bool>)ᏑS390XHasAESGCM).ValueSlot;
public static readonly ж<bool> ᏑS390XHasECDSA = new StaticBox<bool>(cpu.S390X.HasECDSA);
public static ref bool S390XHasECDSA => ref ((StaticBox<bool>)ᏑS390XHasECDSA).ValueSlot;
public static readonly ж<bool> ᏑS390XHasGHASH = new StaticBox<bool>(cpu.S390X.HasGHASH);
public static ref bool S390XHasGHASH => ref ((StaticBox<bool>)ᏑS390XHasGHASH).ValueSlot;
public static readonly ж<bool> ᏑS390XHasSHA256 = new StaticBox<bool>(cpu.S390X.HasSHA256);
public static ref bool S390XHasSHA256 => ref ((StaticBox<bool>)ᏑS390XHasSHA256).ValueSlot;
public static readonly ж<bool> ᏑS390XHasSHA3 = new StaticBox<bool>(cpu.S390X.HasSHA3);
public static ref bool S390XHasSHA3 => ref ((StaticBox<bool>)ᏑS390XHasSHA3).ValueSlot;
public static readonly ж<bool> ᏑS390XHasSHA512 = new StaticBox<bool>(cpu.S390X.HasSHA512);
public static ref bool S390XHasSHA512 => ref ((StaticBox<bool>)ᏑS390XHasSHA512).ValueSlot;

public static readonly ж<bool> ᏑX86HasAES = new StaticBox<bool>(cpu.X86.HasAES);
public static ref bool X86HasAES => ref ((StaticBox<bool>)ᏑX86HasAES).ValueSlot;
public static readonly ж<bool> ᏑX86HasADX = new StaticBox<bool>(cpu.X86.HasADX);
public static ref bool X86HasADX => ref ((StaticBox<bool>)ᏑX86HasADX).ValueSlot;
public static readonly ж<bool> ᏑX86HasAVX = new StaticBox<bool>(cpu.X86.HasAVX);
public static ref bool X86HasAVX => ref ((StaticBox<bool>)ᏑX86HasAVX).ValueSlot;
public static readonly ж<bool> ᏑX86HasAVX2 = new StaticBox<bool>(cpu.X86.HasAVX2);
public static ref bool X86HasAVX2 => ref ((StaticBox<bool>)ᏑX86HasAVX2).ValueSlot;
public static readonly ж<bool> ᏑX86HasBMI2 = new StaticBox<bool>(cpu.X86.HasBMI2);
public static ref bool X86HasBMI2 => ref ((StaticBox<bool>)ᏑX86HasBMI2).ValueSlot;
public static readonly ж<bool> ᏑX86HasPCLMULQDQ = new StaticBox<bool>(cpu.X86.HasPCLMULQDQ);
public static ref bool X86HasPCLMULQDQ => ref ((StaticBox<bool>)ᏑX86HasPCLMULQDQ).ValueSlot;
public static readonly ж<bool> ᏑX86HasSHA = new StaticBox<bool>(cpu.X86.HasSHA);
public static ref bool X86HasSHA => ref ((StaticBox<bool>)ᏑX86HasSHA).ValueSlot;
public static readonly ж<bool> ᏑX86HasSSE41 = new StaticBox<bool>(cpu.X86.HasSSE41);
public static ref bool X86HasSSE41 => ref ((StaticBox<bool>)ᏑX86HasSSE41).ValueSlot;
public static readonly ж<bool> ᏑX86HasSSSE3 = new StaticBox<bool>(cpu.X86.HasSSSE3);
public static ref bool X86HasSSSE3 => ref ((StaticBox<bool>)ᏑX86HasSSSE3).ValueSlot;

} // end cpu_package
