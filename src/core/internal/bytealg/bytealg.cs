// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using cpu = go.@internal.cpu_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class bytealg_package {

// Offsets into internal/cpu records for use in assembly.
internal static uintptr offsetX86HasSSE42 => /* unsafe.Offsetof(cpu.X86.HasSSE42) */ 84;

internal static uintptr offsetX86HasAVX2 => /* unsafe.Offsetof(cpu.X86.HasAVX2) */ 67;

internal static uintptr offsetX86HasPOPCNT => /* unsafe.Offsetof(cpu.X86.HasPOPCNT) */ 78;

internal static uintptr offsetS390xHasVX => /* unsafe.Offsetof(cpu.S390X.HasVX) */ 80;

internal static uintptr offsetPPC64HasPOWER9 => /* unsafe.Offsetof(cpu.PPC64.IsPOWER9) */ 67;

// MaxLen is the maximum length of the string to be searched for (argument b) in Index.
// If MaxLen is not 0, make sure MaxLen >= 4.
public static nint MaxLen;

// PrimeRK is the prime base used in Rabin-Karp algorithm.
public static UntypedInt PrimeRK => 16777619;

// HashStr returns the hash and the appropriate multiplicative
// factor for use in Rabin-Karp algorithm.
public static (uint32, uint32) HashStr<T>(T sep)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    var hash = (uint32)0;
    for (nint i = 0; i < len(sep); i++) {
        hash = hash * (uint32)PrimeRK + (uint32)sep[i];
    }
    uint32 pow = 1;
    uint32 sq = PrimeRK;
    for (nint i = len(sep); i > 0; i >>= (int)(1)) {
        if ((nint)(i & 1) != 0) {
            pow *= sq;
        }
        sq *= sq;
    }
    return (hash, pow);
}

// HashStrRev returns the hash of the reverse of sep and the
// appropriate multiplicative factor for use in Rabin-Karp algorithm.
public static (uint32, uint32) HashStrRev<T>(T sep)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    var hash = (uint32)0;
    for (nint i = len(sep) - 1; i >= 0; i--) {
        hash = hash * (uint32)PrimeRK + (uint32)sep[i];
    }
    uint32 pow = 1;
    uint32 sq = PrimeRK;
    for (nint i = len(sep); i > 0; i >>= (int)(1)) {
        if ((nint)(i & 1) != 0) {
            pow *= sq;
        }
        sq *= sq;
    }
    return (hash, pow);
}

// IndexRabinKarp uses the Rabin-Karp search algorithm to return the index of the
// first occurrence of sep in s, or -1 if not present.
public static nint IndexRabinKarp<T>(T s, T sep)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    // Rabin-Karp search
    var (hashss, pow) = HashStr(sep);
    nint n = len(sep);
    uint32 h = default!;
    for (nint i = 0; i < n; i++) {
        h = h * (uint32)PrimeRK + (uint32)s[i];
    }
    if (h == hashss && s.slice(0, n).ToGoString() == sep.ToGoString()) {
        return 0;
    }
    for (nint i = n; i < len(s); ) {
        h *= PrimeRK;
        h += (uint32)s[i];
        h -= pow * (uint32)s[i - n];
        i++;
        if (h == hashss && s.slice(i - n, i).ToGoString() == sep.ToGoString()) {
            return i - n;
        }
    }
    return -1;
}

// LastIndexRabinKarp uses the Rabin-Karp search algorithm to return the last index of the
// occurrence of sep in s, or -1 if not present.
public static nint LastIndexRabinKarp<T>(T s, T sep)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{
    // Rabin-Karp search from the end of the string
    var (hashss, pow) = HashStrRev(sep);
    nint n = len(sep);
    nint last = len(s) - n;
    uint32 h = default!;
    for (nint i = len(s) - 1; i >= last; i--) {
        h = h * (uint32)PrimeRK + (uint32)s[i];
    }
    if (h == hashss && s.slice(last).ToGoString() == sep.ToGoString()) {
        return last;
    }
    for (nint i = last - 1; i >= 0; i--) {
        h *= PrimeRK;
        h += (uint32)s[i];
        h -= pow * (uint32)s[i + n];
        if (h == hashss && s.slice(i, i + n).ToGoString() == sep.ToGoString()) {
            return i;
        }
    }
    return -1;
}

// MakeNoZero makes a slice of length n and capacity of at least n Bytes
// without zeroing the bytes (including the bytes between len and cap).
// It is the caller's responsibility to ensure uninitialized bytes
// do not leak to the end user.
public static partial slice<byte> MakeNoZero(nint n);

} // end bytealg_package
