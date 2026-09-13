// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class main_package {

[GoType("num:uint8")] partial struct FuncFlag;

public static FuncFlag FuncFlagTopFrame => /* 1 << iota */ 1;
public static FuncFlag FuncFlagSPWrite => 2;
public static FuncFlag FuncFlagAsm => 4;

[GoType("num:uint8")] partial struct FuncID;

public static FuncID FuncIDNormal => /* iota */ 0;
public static FuncID FuncID_abort => 1;
public static FuncID FuncID_asmcgocall => 2;
public static FuncID FuncID_asyncPreempt => 3;
public static FuncID FuncID_cgocallback => 4;
public static FuncID FuncID_corostart => 5;
public static FuncID FuncID_debugCallV2 => 6;
public static FuncID FuncID_gcBgMarkWorker => 7;
public static FuncID FuncID_goexit => 8;
public static FuncID FuncID_gogo => 9;
public static FuncID FuncID_gopanic => 10;
public static FuncID FuncID_handleAsyncEvent => 11;
public static FuncID FuncID_mcall => 12;
public static FuncID FuncID_morestack => 13;
public static FuncID FuncID_mstart => 14;
public static FuncID FuncID_panicwrap => 15;
public static FuncID FuncID_rt0_go => 16;
public static FuncID FuncID_runfinq => 17;
public static FuncID FuncID_runtime_main => 18;
public static FuncID FuncID_sigpanic => 19;
public static FuncID FuncID_systemstack => 20;
public static FuncID FuncID_systemstack_switch => 21;
public static FuncID FuncIDWrapper => 22;

public static UntypedInt ArgsSizeUnknown => /* -0x80000000 */ -2147483648;

public static UntypedInt PCDATA_UnsafePoint => 0;
public static UntypedInt PCDATA_StackMapIndex => 1;
public static UntypedInt PCDATA_InlTreeIndex => 2;
public static UntypedInt PCDATA_ArgLiveIndex => 3;
public static UntypedInt FUNCDATA_ArgsPointerMaps => 0;
public static UntypedInt FUNCDATA_LocalsPointerMaps => 1;
public static UntypedInt FUNCDATA_StackObjects => 2;
public static UntypedInt FUNCDATA_InlTree => 3;
public static UntypedInt FUNCDATA_OpenCodedDeferInfo => 4;
public static UntypedInt FUNCDATA_ArgInfo => 5;
public static UntypedInt FUNCDATA_ArgLiveInfo => 6;
public static UntypedInt FUNCDATA_WrapInfo => 7;

public static UntypedInt UnsafePointSafe => -1;
public static UntypedInt UnsafePointUnsafe => -2;
public static UntypedInt UnsafePointRestart1 => -3;
public static UntypedInt UnsafePointRestart2 => -4;
public static UntypedInt UnsafePointRestartAtEntry => -5;

public static UntypedInt MINFUNC => 16;

public static UntypedInt FuncTabBucketSize => /* 256 * MINFUNC */ 4096;

} // end main_package
