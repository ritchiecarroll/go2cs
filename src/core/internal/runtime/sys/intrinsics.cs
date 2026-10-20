// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

partial class sys_package {

// Copied from math/bits to avoid dependence.
internal static array<byte> deBruijn32tab = new byte[]{
    0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
    31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
}.array();

internal static UntypedInt deBruijn32 => 0x077CB531;

internal static array<byte> deBruijn64tab = new byte[]{
    0, 1, 56, 2, 57, 49, 28, 3, 61, 58, 42, 50, 38, 29, 17, 4,
    62, 47, 59, 36, 45, 43, 51, 22, 53, 39, 33, 30, 24, 18, 12, 5,
    63, 55, 48, 27, 60, 41, 37, 16, 46, 35, 44, 21, 52, 32, 23, 11,
    54, 26, 40, 15, 34, 20, 31, 10, 25, 14, 19, 9, 13, 8, 7, 6
}.array();

internal static UntypedInt deBruijn64 => 0x03f79d71b4ca8b09;

internal static readonly @string ntz8tab = "\b\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x05\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x06\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x05\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\a\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x05\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x06\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x05\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00\x04\x00\x01\x00\x02\x00\x01\x00\x03\x00\x01\x00\x02\x00\x01\x00";

// TrailingZeros32 returns the number of trailing zero bits in x; the result is 32 for x == 0.
public static nint TrailingZeros32(uint32 x) {
    if (x == 0) {
        return 32;
    }
    // see comment in TrailingZeros64
    return (nint)deBruijn32tab[((x & ((uint32)0 - x)) * (uint32)deBruijn32 >> (int)(32 - 5))];
}

// TrailingZeros64 returns the number of trailing zero bits in x; the result is 64 for x == 0.
public static nint TrailingZeros64(uint64 x) {
    if (x == 0) {
        return 64;
    }
    // If popcount is fast, replace code below with return popcount(^x & (x - 1)).
    //
    // x & -x leaves only the right-most bit set in the word. Let k be the
    // index of that bit. Since only a single bit is set, the value is two
    // to the power of k. Multiplying by a power of two is equivalent to
    // left shifting, in this case by k bits. The de Bruijn (64 bit) constant
    // is such that all six bit, consecutive substrings are distinct.
    // Therefore, if we have a left shifted version of this constant we can
    // find by how many bits it was shifted by looking at which six bit
    // substring ended up at the top of the word.
    // (Knuth, volume 4, section 7.3.1)
    return (nint)deBruijn64tab[((x & ((uint64)0 - x)) * (uint64)deBruijn64 >> (int)(64 - 6))];
}

// TrailingZeros8 returns the number of trailing zero bits in x; the result is 8 for x == 0.
public static nint TrailingZeros8(uint8 x) {
    return (nint)ntz8tab[x];
}

internal static readonly @string len8tab = "\x00\x01\x02\x02\x03\x03\x03\x03\x04\x04\x04\x04\x04\x04\x04\x04\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x05\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\x06\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\a\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";

// Len64 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
//
// nosplit because this is used in src/runtime/histogram.go, which make run in sensitive contexts.
//
//go:nosplit
public static nint /*n*/ Len64(uint64 x) {
    nint n = default!;

    if (x >= (uint64)1 << 32) {
        x >>= (int)(32);
        n = 32;
    }
    if (x >= (uint64)1 << 16) {
        x >>= (int)(16);
        n += 16;
    }
    if (x >= (uint64)1 << 8) {
        x >>= (int)(8);
        n += 8;
    }
    return n + (nint)len8tab[(uint8)x];
}

// --- OnesCount ---
internal static UntypedInt m0 => 0x5555555555555555; // 01010101 ...

internal static UntypedInt m1 => 0x3333333333333333; // 00110011 ...

internal static UntypedInt m2 => 0x0f0f0f0f0f0f0f0f; // 00001111 ...

// OnesCount64 returns the number of one bits ("population count") in x.
public static nint OnesCount64(uint64 x) {
    // Implementation: Parallel summing of adjacent bits.
    // See "Hacker's Delight", Chap. 5: Counting Bits.
    // The following pattern shows the general approach:
    //
    //   x = x>>1&(m0&m) + x&(m0&m)
    //   x = x>>2&(m1&m) + x&(m1&m)
    //   x = x>>4&(m2&m) + x&(m2&m)
    //   x = x>>8&(m3&m) + x&(m3&m)
    //   x = x>>16&(m4&m) + x&(m4&m)
    //   x = x>>32&(m5&m) + x&(m5&m)
    //   return int(x)
    //
    // Masking (& operations) can be left away when there's no
    // danger that a field's sum will carry over into the next
    // field: Since the result cannot be > 64, 8 bits is enough
    // and we can ignore the masks for the shifts by 8 and up.
    // Per "Hacker's Delight", the first line can be simplified
    // more, but it saves at best one instruction, so we leave
    // it alone for clarity.
    UntypedInt m = /* 1<<64 - 1 */ 18446744073709551615;
    x = (uint64)(x >> 1 & ((uint64)((uint64)m0 & (uint64)m))) + (uint64)(x & ((uint64)((uint64)m0 & (uint64)m)));
    x = (uint64)(x >> 2 & ((uint64)((uint64)m1 & (uint64)m))) + (uint64)(x & ((uint64)((uint64)m1 & (uint64)m)));
    x = (uint64)(((x >> 4) + x) & ((uint64)((uint64)m2 & (uint64)m)));
    x += (x >> 8);
    x += (x >> 16);
    x += (x >> 32);
    return (nint)x & (nint)((1 << 7) - 1);
}

// LeadingZeros64 returns the number of leading zero bits in x; the result is 64 for x == 0.
public static nint LeadingZeros64(uint64 x) {
    return 64 - Len64(x);
}

// LeadingZeros8 returns the number of leading zero bits in x; the result is 8 for x == 0.
public static nint LeadingZeros8(uint8 x) {
    return 8 - Len8(x);
}

// Len8 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len8(uint8 x) {
    return (nint)len8tab[x];
}

// Bswap64 returns its input with byte order reversed
// 0x0102030405060708 -> 0x0807060504030201
public static uint64 Bswap64(uint64 x) {
    var c8 = (uint64)0x00ff00ff00ff00ffUL;
    var a = x >> 8 & c8;
    var b = (x & c8) << 8;
    x = a | b;
    var c16 = (uint64)0x0000ffff0000ffffUL;
    a = x >> 16 & c16;
    b = (x & c16) << 16;
    x = a | b;
    var c32 = (uint64)0x00000000ffffffffU;
    a = x >> 32 & c32;
    b = (x & c32) << 32;
    x = a | b;
    return x;
}

// Bswap32 returns its input with byte order reversed
// 0x01020304 -> 0x04030201
public static uint32 Bswap32(uint32 x) {
    var c8 = (uint32)0x00ff00ff;
    var a = x >> 8 & c8;
    var b = (x & c8) << 8;
    x = a | b;
    var c16 = (uint32)0x0000ffff;
    a = x >> 16 & c16;
    b = (x & c16) << 16;
    x = a | b;
    return x;
}

// Prefetch prefetches data from memory addr to cache
//
// AMD64: Produce PREFETCHT0 instruction
//
// ARM64: Produce PRFM instruction with PLDL1KEEP option
public static void Prefetch(uintptr addr) {
}

// PrefetchStreamed prefetches data from memory addr, with a hint that this data is being streamed.
// That is, it is likely to be accessed very soon, but only once. If possible, this will avoid polluting the cache.
//
// AMD64: Produce PREFETCHNTA instruction
//
// ARM64: Produce PRFM instruction with PLDL1STRM option
public static void PrefetchStreamed(uintptr addr) {
}

// GetCallerPC returns the program counter (PC) of its caller's caller.
// GetCallerSP returns the stack pointer (SP) of its caller's caller.
// Both are implemented as intrinsics on every platform.
//
// For example:
//
//	func f(arg1, arg2, arg3 int) {
//		pc := GetCallerPC()
//		sp := GetCallerSP()
//	}
//
// These two lines find the PC and SP immediately following
// the call to f (where f will return).
//
// The call to GetCallerPC and GetCallerSP must be done in the
// frame being asked about.
//
// The result of GetCallerSP is correct at the time of the return,
// but it may be invalidated by any subsequent call to a function
// that might relocate the stack in order to grow or shrink it.
// A general rule is that the result of GetCallerSP should be used
// immediately and can only be passed to nosplit functions.
public static partial uintptr GetCallerPC();

public static partial uintptr GetCallerSP();

// GetClosurePtr returns the pointer to the current closure.
// GetClosurePtr can only be used in an assignment statement
// at the entry of a function. Moreover, go:nosplit directive
// must be specified at the declaration of caller function,
// so that the function prolog does not clobber the closure register.
// for example:
//
//	//go:nosplit
//	func f(arg1, arg2, arg3 int) {
//		dx := GetClosurePtr()
//	}
//
// The compiler rewrites calls to this function into instructions that fetch the
// pointer from a well-known register (DX on x86 architecture, etc.) directly.
//
// WARNING: PGO-based devirtualization cannot detect that caller of
// GetClosurePtr requires closure context, and thus must maintain a list of
// these functions, which is in
// cmd/compile/internal/devirtualize/pgo.maybeDevirtualizeFunctionCall.
public static partial uintptr GetClosurePtr();

} // end sys_package
