// os_linux_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The bootstrap constants Go's linux osinit path sets and the managed runtime never reached
// (increment 7 of the runtime row, 2026-09-05). This file has no `<name>.go` counterpart, so a
// -stdlib reconvert never emits over it; the module marker states the ownership explicitly.
//
// WHY. Go sets physPageSize from the auxiliary vector (sysauxv, AT_PAGESZ, with an mincore probe as
// the fallback when the vector is absent) and physHugePageSize from
// /sys/kernel/mm/transparent_hugepage/hpage_pmd_size (getHugePageSize), both on the osinit path
// schedinit runs before any Go code. The managed host runs none of that: schedinit, osinit and
// sysargs are never called, so both fields sat at their zero values — and every page-allocator row
// died on `alignUp(n, physPageSize)` answering 0, then `mmap(0 bytes)` answering EINVAL, then
// `failed to reserve page summary memory` (the increment-6 readings). The converted getHugePageSize
// cannot run either: it reads the sysfs file through the runtime's raw `open`/`read`/`closefd`,
// which are assembly stubs that throw.
//
// WHAT THIS SETS, per flavour (increment 7 is linux; windows and darwin have their own osinit and
// are not touched by this file):
//   physPageSize      — Environment.SystemPageSize, which is sysconf(_SC_PAGESIZE): the same value the
//                       kernel places in AT_PAGESZ, so Go's primary source, never the mincore fallback.
//   physHugePageSize  — the sysfs file, parsed exactly as getHugePageSize parses it (leading decimal
//                       digits; a non-power-of-two answers 0; an unreadable file answers 0). 0 is
//                       Go's own value when the file cannot be read, and mallocinit accepts it.
// WHAT STAYS ZERO / UNTOUCHED (the rest of osinit and sysargs): ncpu is already Environment.ProcessorCount
// in the converted runtime2.cs; startupRand (AT_RANDOM), secureMode (AT_SECURE), the auxv copy,
// archauxv/vdsoauxv (the vDSO tables), osArchInit (a no-op on linux/amd64) and the signal-mask
// state are not set — none of them has a reaching consumer on the rows this increment measures,
// and each is named here so the next reader finds the boundary rather than rediscovering it.
//
// The [ModuleInitializer] runs before Main and forces runtime_package's type initializer, exactly as
// goenvs_impl.cs does for envs; the fields have no initializers of their own to overwrite.

using System;
using System.IO;
using System.Runtime.CompilerServices;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    private const string sysTHPSizeFile = "/sys/kernel/mm/transparent_hugepage/hpage_pmd_size";

    [ModuleInitializer]
    internal static void ᴛInitBootstrapConstants()
    {
        physPageSize = (uintptr)(nuint)Environment.SystemPageSize;
        physHugePageSize = readTransparentHugePageSize();
    }

    // getHugePageSize's parse, over the file the managed host CAN read: the leading decimal digits
    // (Go stops at the first non-digit of a 20-byte read), a negative or non-power-of-two value
    // answers 0, an unreadable file answers 0.
    private static uintptr readTransparentHugePageSize()
    {
        string text;

        try
        {
            text = File.ReadAllText(sysTHPSizeFile);
        }
        catch (Exception)
        {
            return 0;
        }

        return parseHugePageSize(text);
    }

    internal static uintptr parseHugePageSize(string text)
    {
        long v = 0;
        int i = 0;

        for (; i < text.Length && i < 20 && text[i] >= '0' && text[i] <= '9'; i++)
            v = v * 10 + (text[i] - '0');

        if (i == 0 || v < 0)
            return 0;

        if ((v & (v - 1)) != 0)
            return 0;

        return (uintptr)(nuint)v;
    }

    /// <summary>The two bootstrap constants as the runtime holds them, for the standing guard.</summary>
    public static (nuint physPageSize, nuint physHugePageSize) GoBootstrapConstants()
    {
        return ((nuint)physPageSize, (nuint)physHugePageSize);
    }

    /// <summary>getHugePageSize's parse over caller-supplied text, for the guard's negative arms.</summary>
    public static nuint GoParseHugePageSize(string text)
    {
        return (nuint)parseHugePageSize(text);
    }
}
