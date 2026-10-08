// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Windows architecture-independent definitions.
namespace go;

partial class runtime_package {

internal static UntypedInt _PROT_NONE => 0;
internal static UntypedInt _PROT_READ => 1;
internal static UntypedInt _PROT_WRITE => 2;
internal static UntypedInt _PROT_EXEC => 4;
internal static UntypedInt _MAP_ANON => 1;
internal static UntypedInt _MAP_PRIVATE => 2;
internal static UntypedInt _DUPLICATE_SAME_ACCESS => 0x2;
internal static UntypedInt _THREAD_PRIORITY_HIGHEST => 0x2;
internal static UntypedInt _SIGINT => 0x2;
internal static UntypedInt _SIGTERM => 0xF;
internal static UntypedInt _CTRL_C_EVENT => 0x0;
internal static UntypedInt _CTRL_BREAK_EVENT => 0x1;
internal static UntypedInt _CTRL_CLOSE_EVENT => 0x2;
internal static UntypedInt _CTRL_LOGOFF_EVENT => 0x5;
internal static UntypedInt _CTRL_SHUTDOWN_EVENT => 0x6;
internal static UntypedInt _EXCEPTION_ACCESS_VIOLATION => 0xc0000005;
internal static UntypedInt _EXCEPTION_IN_PAGE_ERROR => 0xc0000006;
internal static UntypedInt _EXCEPTION_BREAKPOINT => 0x80000003;
internal static UntypedInt _EXCEPTION_ILLEGAL_INSTRUCTION => 0xc000001d;
internal static UntypedInt _EXCEPTION_FLT_DENORMAL_OPERAND => 0xc000008d;
internal static UntypedInt _EXCEPTION_FLT_DIVIDE_BY_ZERO => 0xc000008e;
internal static UntypedInt _EXCEPTION_FLT_INEXACT_RESULT => 0xc000008f;
internal static UntypedInt _EXCEPTION_FLT_OVERFLOW => 0xc0000091;
internal static UntypedInt _EXCEPTION_FLT_UNDERFLOW => 0xc0000093;
internal static UntypedInt _EXCEPTION_INT_DIVIDE_BY_ZERO => 0xc0000094;
internal static UntypedInt _EXCEPTION_INT_OVERFLOW => 0xc0000095;
internal static UntypedInt _INFINITE => 0xffffffff;
internal static UntypedInt _WAIT_TIMEOUT => 0x102;
internal static UntypedInt _EXCEPTION_CONTINUE_EXECUTION => /* -0x1 */ -1;
internal static UntypedInt _EXCEPTION_CONTINUE_SEARCH => 0x0;
internal static UntypedInt _EXCEPTION_CONTINUE_SEARCH_SEH => 0x1;

partial struct systeminfo {
    internal array<byte> anon0 = new(4);
    internal uint32 dwpagesize;
    internal ж<byte> lpminimumapplicationaddress;
    internal ж<byte> lpmaximumapplicationaddress;
    internal uintptr dwactiveprocessormask;
    internal uint32 dwnumberofprocessors;
    internal uint32 dwprocessortype;
    internal uint32 dwallocationgranularity;
    internal uint16 wprocessorlevel;
    internal uint16 wprocessorrevision;
}

partial struct exceptionpointers {
    internal ж<exceptionrecord> record;
    internal ж<context> context;
}

partial struct exceptionrecord {
    internal uint32 exceptioncode;
    internal uint32 exceptionflags;
    internal ж<exceptionrecord> Δexceptionrecord;
    internal uintptr exceptionaddress;
    internal uint32 numberparameters;
    internal array<uintptr> exceptioninformation = new(15);
}

partial struct overlapped {
    internal uintptr @internal;
    internal uintptr internalhigh;
    internal array<byte> anon0 = new(8);
    internal ж<byte> hevent;
}

partial struct memoryBasicInformation {
    internal uintptr baseAddress;
    internal uintptr allocationBase;
    internal uint32 allocationProtect;
    internal uintptr regionSize;
    internal uint32 state;
    internal uint32 protect;
    internal uint32 type_;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfow
partial struct _OSVERSIONINFOW {
    internal uint32 osVersionInfoSize;
    internal uint32 majorVersion;
    internal uint32 minorVersion;
    internal uint32 buildNumber;
    internal uint32 platformId;
    internal array<uint16> csdVersion = new(128);
}

} // end runtime_package
