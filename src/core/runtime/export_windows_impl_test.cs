// export_windows_impl_test.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-owned body for runtime's export_windows_test.go, the WINDOWS test-file companion: the platform
// in the name makes testConversion join this file to the windows test project alone
// (testImplCompanionAppliesTo), as Go's `_windows` suffix scopes the file it stands beside. The
// converter drops the auto form (manualConversionFuncs["runtime"], goosWindows).
//
// NewContextStub left this file at the Go 1.26 hop's H5 (2026-10-15): go1.26.9 dropped it and type
// ContextStub from export_windows_test.go, so its refusal had nothing to replace.
//
// NumberOfProcessors answers GetSystemInfo's dwNumberOfProcessors, as Go's body does. The converted body
// asked through stdcall1 -> asmcgocall, whose stub throws, so TestNumCPU read infrastructure-error. The
// hand-own asks kernel32 directly, over a blittable mirror of SYSTEM_INFO (runtime's converted
// systeminfo carries pointer fields, which are managed boxes here).
//
// Hand-owned (no export_windows_impl_test.go exists, so a reconvert never regenerates this file).

using System.Runtime.InteropServices;

namespace go;

partial class runtime_internal_test_package
{
    public static int32 NumberOfProcessors()
    {
        GetSystemInfo(out SystemInfo info);
        return (int32)info.NumberOfProcessors;
    }

    // SYSTEM_INFO (sysinfoapi.h), x64 layout.
    [StructLayout(LayoutKind.Sequential)]
    private struct SystemInfo
    {
        public ushort ProcessorArchitecture;
        public ushort Reserved;
        public uint PageSize;
        public nint MinimumApplicationAddress;
        public nint MaximumApplicationAddress;
        public nuint ActiveProcessorMask;
        public uint NumberOfProcessors;
        public uint ProcessorType;
        public uint AllocationGranularity;
        public ushort ProcessorLevel;
        public ushort ProcessorRevision;
    }

    [DllImport("kernel32.dll")]
    private static extern void GetSystemInfo(out SystemInfo info);
}
