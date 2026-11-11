// crc32_arm64_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// hash/crc32's two arm64 assembly kernels, castagnoliUpdate and ieeeUpdate (crc32_arm64.go declares them,
// crc32_arm64.s implements them with the ARMv8 CRC32 extension).
//
// WHY THEY WERE OWED (S3 site 9, the arm64-only stubbed census). They were bodyless, so PartialStubGenerator threw.
// That throw is reachable: crc32_arm64.go routes every update through them whenever cpu.ARM64.HasCRC32, and at
// linux/arm64 internal/cpu fills that from the kernel's AT_HWCAP (S3 site 0). So on any arm64 CPU with the CRC32
// extension, ChecksumIEEE, the Castagnoli table and everything above them (gzip, zip, png) would have thrown.
//
// THE CONTRACT IS THE INSTRUCTIONS'. archUpdateIEEE and archUpdateCastagnoli invert on the way in and out
// (`return ^ieeeUpdate(^crc, p)`), so these two are the raw instruction loops: CRC32X/CRC32B for IEEE and
// CRC32CX/CRC32CB for Castagnoli, eight bytes at a time as a little-endian doubleword, then byte by byte, with no
// inversion. System.Runtime.Intrinsics.Arm.Crc32 is those same instructions. The software loop below is the same
// reflected-polynomial recurrence one bit at a time. It is reached only if the CLR does not report the extension,
// which Go's own routing already rules out (it calls these only when HasCRC32), and it keeps the body total
// rather than a throw.
//
// This file's principal is crc32_arm64.cs, which only linux/arm64 emits in this universe, so the hand-own router
// keeps it in linux-arm64/.
//
// Hand-owned (no crc32_arm64_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go.hash;

using System;
using System.Buffers.Binary;
using ArmCrc32 = System.Runtime.Intrinsics.Arm.Crc32;

partial class crc32_package
{
    // The reflected polynomials the two instructions implement: IEEE 0xEDB88320, Castagnoli 0x82F63B78.
    private const uint ieeeReflected = 0xEDB88320u;
    private const uint castagnoliReflected = 0x82F63B78u;

    internal static partial uint32 castagnoliUpdate(uint32 crc, slice<byte> p) => armCrc32Update(crc, p, castagnoli: true);

    internal static partial uint32 ieeeUpdate(uint32 crc, slice<byte> p) => armCrc32Update(crc, p, castagnoli: false);

    private static uint32 armCrc32Update(uint32 crc, slice<byte> p, bool castagnoli)
    {
        ReadOnlySpan<byte> s = p.ToSpan();
        int i = 0;

        if (ArmCrc32.Arm64.IsSupported)
        {
            for (; i + 8 <= s.Length; i += 8)
            {
                ulong word = BinaryPrimitives.ReadUInt64LittleEndian(s[i..]);
                crc = castagnoli ? ArmCrc32.Arm64.ComputeCrc32C(crc, word) : ArmCrc32.Arm64.ComputeCrc32(crc, word);
            }
        }

        if (ArmCrc32.IsSupported)
        {
            for (; i < s.Length; i++)
                crc = castagnoli ? ArmCrc32.ComputeCrc32C(crc, s[i]) : ArmCrc32.ComputeCrc32(crc, s[i]);

            return crc;
        }

        uint poly = castagnoli ? castagnoliReflected : ieeeReflected;

        for (; i < s.Length; i++)
        {
            crc ^= s[i];

            for (int bit = 0; bit < 8; bit++)
                crc = (crc >> 1) ^ (poly & (0u - (crc & 1u)));
        }

        return crc;
    }
}
