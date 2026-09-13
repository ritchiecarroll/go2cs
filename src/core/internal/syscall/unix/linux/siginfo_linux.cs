// siginfo_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted siginfo_linux.go output).
//
// SiginfoChild is an OUT-parameter the KERNEL writes: waitid(2) fills 128 bytes at the address
// the caller passes, and both wait paths hand it a heap-boxed managed struct through the
// transient-pin `(uintptr)Ꮡinfo` seam (os/linux/wait_waitid.cs and pidfd_linux.cs). The converted
// form cannot be that struct: Go's two padding fields (`_ [is64bit]int32`, `__ [100]byte`) emit as
// golib `array<T>` fields, which are MANAGED CLASS REFERENCES — eight-byte pointers, not inline
// storage — so the C# layout is Signo(0) errno(4) code(8) pointer(16) Pid(24) Uid(28) Status(32)
// against the kernel's Signo(0) errno(4) code(8) pad(12) Pid(16) Uid(20) Status(24). Measured on
// the exec-wall lane (2026-08-22): Code reads correctly (CLD_EXITED, the offsets agree that far),
// Status reads ZERO (offset 32 is past everything the kernel wrote) — flag's and os/exec's
// TestExitCode saw every child exit as 0 — and the kernel's Pid word lands INSIDE the padding
// field's object reference, overwriting a live GC pointer with raw pid bits. That last part makes
// this more than a wrong answer: it is the same memory-corruption shape as the Windows
// Timezoneinformation family (the non-blittable-out-param wall), and the remedy is that wall's
// established one — a BLITTABLE mirror, hand-owned.
//
// The mirror below is linux-amd64's actual layout: sequential scalars with the one explicit pad,
// `Size = 128` covering the tail padding the kernel may touch. The struct stays `partial` and
// keeps the exported field names, so the package_info shell, the callers' field reads
// (`info.Pid`, `info.WaitStatus()`), and TestSiginfoChildLayout's expectations all bind
// unchanged. The MIPS errno/code swap Go's comment mentions is amd64-irrelevant and this corpus
// is amd64-only per flavor.

using System.Runtime.InteropServices;

// Hand-owned native replacement of the converted siginfo_linux.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go.@internal.syscall;

public static partial class unix_package
{
    [StructLayout(LayoutKind.Sequential, Size = 128)]
    public partial struct SiginfoChild
    {
        public int Signo;
        public int Errno;
        public int Code;
        private int _pad0; // 64-bit alignment pad before the union, exactly Go's `_ [is64bit]int32`
        public int Pid;
        public uint Uid;
        public int Status;
        // Size = 128 supplies the tail Go pads with `__ [100]byte`; the kernel may write into it,
        // and inline (not referenced) storage is the entire point of this mirror.
    }

    internal const int _CLD_EXITED = 1;
    internal const int _CLD_KILLED = 2;
    internal const int _CLD_DUMPED = 3;
    internal const int _CLD_TRAPPED = 4;
    internal const int _CLD_STOPPED = 5;
    internal const int _CLD_CONTINUED = 6;

    private const uint core = 0x80;
    private const uint stopped = 0x7f;
    private const uint continued = 0xffff;

    // WaitStatus converts SiginfoChild, as filled in by the waitid syscall, to syscall.WaitStatus
    // — the converted body verbatim over the mirror's fields.
    public static syscall_package.WaitStatus WaitStatus(this ref SiginfoChild s)
    {
        uint ws = 0;

        switch (s.Code)
        {
            case _CLD_EXITED:
                ws = (uint)(s.Status << 8);
                break;
            case _CLD_DUMPED:
                ws = (uint)s.Status | core;
                break;
            case _CLD_KILLED:
                ws = (uint)s.Status;
                break;
            case _CLD_TRAPPED:
            case _CLD_STOPPED:
                ws = ((uint)(s.Status << 8)) | stopped;
                break;
            case _CLD_CONTINUED:
                ws = continued;
                break;
        }

        return (syscall_package.WaitStatus)ws;
    }
}
