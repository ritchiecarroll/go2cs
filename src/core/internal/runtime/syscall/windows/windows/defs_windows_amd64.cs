// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime.syscall;

using goarch = go.@internal.goarch_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class windows_package {

public static UntypedInt CONTEXT_CONTROL => 0x100001;

partial struct M128 {
    public uint64 Low;
    public int64 High;
}

partial struct Context {
    public uint64 P1Home;
    public uint64 P2Home;
    public uint64 P3Home;
    public uint64 P4Home;
    public uint64 P5Home;
    public uint64 P6Home;
    public uint32 ContextFlags;
    public uint32 MxCsr;
    public uint16 SegCs;
    public uint16 SegDs;
    public uint16 SegEs;
    public uint16 SegFs;
    public uint16 SegGs;
    public uint16 SegSs;
    public uint32 EFlags;
    public uint64 DR0;
    public uint64 DR1;
    public uint64 DR2;
    public uint64 DR3;
    public uint64 DR6;
    public uint64 DR7;
    public uint64 Rax;
    public uint64 Rcx;
    public uint64 Rdx;
    public uint64 Rbx;
    public uint64 Rsp;
    public uint64 Rbp;
    public uint64 Rsi;
    public uint64 Rdi;
    public uint64 R8;
    public uint64 R9;
    public uint64 R10;
    public uint64 R11;
    public uint64 R12;
    public uint64 R13;
    public uint64 R14;
    public uint64 R15;
    public uint64 Rip;
    internal array<byte> _ = new(512);
    public array<M128> VectorRegister = new(26);
    public uint64 VectorControl;
    public uint64 DebugControl;
    public uint64 LastBranchToRip;
    public uint64 LastBranchFromRip;
    public uint64 LastExceptionToRip;
    public uint64 LastExceptionFromRip;
}

public static uintptr PC(this ref Context c) {
    return (uintptr)c.Rip;
}

public static uintptr SP(this ref Context c) {
    return (uintptr)c.Rsp;
}

// AMD64 does not have link register, so this returns 0.
public static uintptr LR(this ref Context c) {
    return 0;
}

public static void SetLR(this ref Context c, uintptr x) {
}

public static void SetPC(this ref Context c, uintptr x) {
    c.Rip = (uint64)x;
}

public static void SetSP(this ref Context c, uintptr x) {
    c.Rsp = (uint64)x;
}

public static void SetFP(this ref Context c, uintptr x) {
    c.Rbp = (uint64)x;
}

public static void PushCall(this ref Context c, uintptr targetPC, uintptr resumePC) {
    var sp = c.SP() - (uintptr)goarch.StackAlign;
    ((ж<uintptr>)(uintptr)((@unsafe.Pointer)sp)).Value = resumePC;
    c.SetSP(sp);
    c.SetPC(targetPC);
}

partial struct DISPATCHER_CONTEXT {
    public uint64 ControlPc;
    public uint64 ImageBase;
    public uintptr FunctionEntry;
    public uint64 EstablisherFrame;
    public uint64 TargetIp;
    public ж<Context> Context;
    public uintptr LanguageHandler;
    public uintptr HandlerData;
}

public static ж<Context> Ctx(this ref DISPATCHER_CONTEXT c) {
    return c.Context;
}

} // end windows_package
