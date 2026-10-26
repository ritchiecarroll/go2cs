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

public static bool ARM64HasAES = cpu.ARM64.HasAES;
public static bool ARM64HasPMULL = cpu.ARM64.HasPMULL;
public static bool ARM64HasSHA2 = cpu.ARM64.HasSHA2;
public static bool ARM64HasSHA512 = cpu.ARM64.HasSHA512;
public static bool ARM64HasSHA3 = cpu.ARM64.HasSHA3;

public static bool LOONG64HasLSX = cpu.Loong64.HasLSX;
public static bool LOONG64HasLASX = cpu.Loong64.HasLASX;

public static bool S390XHasAES = cpu.S390X.HasAES;
public static bool S390XHasAESCBC = cpu.S390X.HasAESCBC;
public static bool S390XHasAESCTR = cpu.S390X.HasAESCTR;
public static bool S390XHasAESGCM = cpu.S390X.HasAESGCM;
public static bool S390XHasECDSA = cpu.S390X.HasECDSA;
public static bool S390XHasGHASH = cpu.S390X.HasGHASH;
public static bool S390XHasSHA256 = cpu.S390X.HasSHA256;
public static bool S390XHasSHA3 = cpu.S390X.HasSHA3;
public static bool S390XHasSHA512 = cpu.S390X.HasSHA512;

public static bool X86HasAES = cpu.X86.HasAES;
public static bool X86HasADX = cpu.X86.HasADX;
public static bool X86HasAVX = cpu.X86.HasAVX;
public static bool X86HasAVX2 = cpu.X86.HasAVX2;
public static bool X86HasBMI2 = cpu.X86.HasBMI2;
public static bool X86HasPCLMULQDQ = cpu.X86.HasPCLMULQDQ;
public static bool X86HasSHA = cpu.X86.HasSHA;
public static bool X86HasSSE41 = cpu.X86.HasSSE41;
public static bool X86HasSSSE3 = cpu.X86.HasSSSE3;

} // end cpu_package
