// bytealg_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using go.golib;

namespace go.@internal;

partial class bytealg_package
{
    public static partial slice<byte> MakeNoZero(nint n)
    {
        // Go's runtime implementation panics (uintptr(len) > maxAlloc) with a recoverable
        // "runtime error: makeslice: len out of range"; .NET's heap ceiling for a byte[] is
        // Array.MaxLength, and exceeding it raised OverflowException — which recover() cannot
        // catch, so strings/bytes TestRepeatCatchesOverflow died instead of observing the panic.
        if (n < 0 || n > Array.MaxLength)
            throw RuntimeErrorPanic.MakeSliceLenOutOfRange();

        return new slice<byte>(new byte[n]);
    }

    // internal/bytealg's Index/Compare/Count family are assembly-optimized on amd64, so the
    // converter emitted the platform (asm-linked) variants as bodyless partials — throwing stubs.
    // Go ships pure-Go fallbacks for non-asm platforms; these supply the equivalent managed bodies
    // so the family RUNS (e.g. syscall.UTF16FromString scans for a NUL via IndexByteString on the
    // way into any Windows FFI call, and strings/bytes route Index/Compare/Count through here).
    // Each returns Go's contract: a 0-based index or -1, a count, or a -1/0/1 comparison.

    public static partial nint IndexByte(slice<byte> b, byte c)
    {
        return b.ToSpan().IndexOf(c);
    }

    public static partial nint IndexByteString(@string s, byte c)
    {
        for (int i = 0, n = s.Length; i < n; i++)
        {
            if (s[i] == c)
                return i;
        }

        return -1;
    }

    public static partial nint Index(slice<byte> a, slice<byte> b)
    {
        return b.Length == 0 ? 0 : a.ToSpan().IndexOf(b.ToSpan());
    }

    public static partial nint IndexString(@string a, @string b)
    {
        int n = a.Length;
        int m = b.Length;

        if (m == 0)
            return 0;

        for (int i = 0; i + m <= n; i++)
        {
            int j = 0;

            while (j < m && a[i + j] == b[j])
                j++;

            if (j == m)
                return i;
        }

        return -1;
    }

    public static partial nint Count(slice<byte> b, byte c)
    {
        nint count = 0;

        foreach (byte x in b.ToSpan())
        {
            if (x == c)
                count++;
        }

        return count;
    }

    public static partial nint CountString(@string s, byte c)
    {
        nint count = 0;

        for (int i = 0, n = s.Length; i < n; i++)
        {
            if (s[i] == c)
                count++;
        }

        return count;
    }

    public static partial nint Compare(slice<byte> a, slice<byte> b)
    {
        return Math.Sign(a.ToSpan().SequenceCompareTo(b.ToSpan()));
    }

    internal static partial nint abigen_runtime_cmpstring(@string a, @string b)
    {
        int n = Math.Min(a.Length, b.Length);

        for (int i = 0; i < n; i++)
        {
            if (a[i] != b[i])
                return a[i] < b[i] ? -1 : 1;
        }

        return Math.Sign(a.Length - b.Length);
    }
}
