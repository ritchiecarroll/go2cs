// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Architecture-independent definitions.
namespace go.@internal.runtime.syscall;

partial class windows_package {

// Pseudo handles.
public static uintptr CurrentProcess => /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615); // -1 = current process
public static uintptr CurrentThread => /* ^uintptr(1) */ unchecked((uintptr)18446744073709551614);  // -2 = current thread

public static uintptr INVALID_HANDLE_VALUE => /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);

public static UntypedInt DWORD_MAX => 0xffffffff;

public static UntypedInt PROT_NONE => 0;
public static UntypedInt PROT_READ => 1;
public static UntypedInt PROT_WRITE => 2;
public static UntypedInt PROT_EXEC => 4;

public static UntypedInt MAP_ANON => 1;
public static UntypedInt MAP_PRIVATE => 2;

public static UntypedInt DUPLICATE_SAME_ACCESS => 0x2;

public static UntypedInt THREAD_PRIORITY_HIGHEST => 0x2;

public static UntypedInt SIGINT => 0x2;
public static UntypedInt SIGTERM => 0xF;

public static UntypedInt CTRL_C_EVENT => 0x0;
public static UntypedInt CTRL_BREAK_EVENT => 0x1;
public static UntypedInt CTRL_CLOSE_EVENT => 0x2;
public static UntypedInt CTRL_LOGOFF_EVENT => 0x5;
public static UntypedInt CTRL_SHUTDOWN_EVENT => 0x6;

public static UntypedInt EXCEPTION_ACCESS_VIOLATION => 0xc0000005;
public static UntypedInt EXCEPTION_IN_PAGE_ERROR => 0xc0000006;
public static UntypedInt EXCEPTION_BREAKPOINT => 0x80000003;
public static UntypedInt EXCEPTION_ILLEGAL_INSTRUCTION => 0xc000001d;
public static UntypedInt EXCEPTION_FLT_DENORMAL_OPERAND => 0xc000008d;
public static UntypedInt EXCEPTION_FLT_DIVIDE_BY_ZERO => 0xc000008e;
public static UntypedInt EXCEPTION_FLT_INEXACT_RESULT => 0xc000008f;
public static UntypedInt EXCEPTION_FLT_OVERFLOW => 0xc0000091;
public static UntypedInt EXCEPTION_FLT_UNDERFLOW => 0xc0000093;
public static UntypedInt EXCEPTION_INT_DIVIDE_BY_ZERO => 0xc0000094;
public static UntypedInt EXCEPTION_INT_OVERFLOW => 0xc0000095;

public static UntypedInt SEM_FAILCRITICALERRORS => 0x0001;
public static UntypedInt SEM_NOGPFAULTERRORBOX => 0x0002;
public static UntypedInt SEM_NOOPENFILEERRORBOX => 0x8000;

public static UntypedInt WER_FAULT_REPORTING_NO_UI => 0x0020;

public static UntypedInt INFINITE => 0xffffffff;

public static UntypedInt WAIT_TIMEOUT => 258;

public static UntypedInt FAIL_FAST_GENERATE_EXCEPTION_ADDRESS => 0x1;

public static UntypedInt EXCEPTION_CONTINUE_EXECUTION => /* -0x1 */ -1;
public static UntypedInt EXCEPTION_CONTINUE_SEARCH => 0x0;
public static UntypedInt EXCEPTION_CONTINUE_SEARCH_SEH => 0x1;

public static UntypedInt CREATE_WAITABLE_TIMER_HIGH_RESOLUTION => 0x00000002;

public static UntypedInt SYNCHRONIZE => 0x00100000;
public static UntypedInt TIMER_QUERY_STATE => 0x0001;
public static UntypedInt TIMER_MODIFY_STATE => 0x0002;

// https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55
public static UntypedInt STATUS_SUCCESS => 0x00000000;
public static UntypedInt STATUS_PENDING => 0x00000103;
public static UntypedInt STATUS_CANCELLED => 0xC0000120;

// https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-system_info
partial struct SystemInfo {
    public uint16 ProcessorArchitecture;
    public uint16 Reserved;
    public uint32 PageSize;
    public ж<byte> MinimumApplicationAddress;
    public ж<byte> MaximumApplicationAddress;
    public uintptr ActiveProcessorMask;
    public uint32 NumberOfProcessors;
    public uint32 ProcessorType;
    public uint32 AllocationGranularity;
    public uint16 ProcessorLevel;
    public uint16 ProcessorRevision;
}

// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-exception_pointers
partial struct ExceptionPointers {
    public ж<ExceptionRecord> Record;
    public ж<Context> Context;
}

// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-exception_record
partial struct ExceptionRecord {
    public uint32 ExceptionCode;
    public uint32 ExceptionFlags;
    public ж<ExceptionRecord> ΔExceptionRecord;
    public uintptr ExceptionAddress;
    public uint32 NumberParameters;
    public array<uintptr> ExceptionInformation = new(15);
}

partial struct Handle /*num:uintptr*/;

// https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-overlapped
partial struct Overlapped {
    public uintptr Internal;
    public uintptr InternalHigh;
    public uint32 Offset;
    public uint32 OffsetHigh;
    public Handle HEvent;
}

// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-memory_basic_information
partial struct MemoryBasicInformation {
    public uintptr BaseAddress;
    public uintptr AllocationBase;
    public uint32 AllocationProtect;
    public uint16 PartitionId;
    public uintptr RegionSize;
    public uint32 State;
    public uint32 Protect;
    public uint32 Type;
}

// https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfow
partial struct OSVERSIONINFOW {
    public uint32 OSVersionInfoSize;
    public uint32 MajorVersion;
    public uint32 MinorVersion;
    public uint32 BuildNumber;
    public uint32 PlatformID;
    public array<uint16> CSDVersion = new(128);
}

} // end windows_package
