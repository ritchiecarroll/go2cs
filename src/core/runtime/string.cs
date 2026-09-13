// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using bytealg = @internal.bytealg_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// The constant is known to the compiler.
// There is no fundamental theory behind this number.
internal static UntypedInt tmpStringBufSize => 32;

[GoType("[32]byte")] /* [tmpStringBufSize]byte */
partial struct tmpBuf;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringConcatenationTooˢ = "string concatenation too long"u8;

// concatstrings implements a Go string concatenation x+y+z+...
// The operands are passed in the slice a.
// If buf != nil, the compiler has determined that the result does not
// escape the calling function, so the string data can be stored in buf
// if small enough.
internal static @string concatstrings(ж<tmpBuf> Ꮡbuf, slice<@string> a) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    nint idx = 0;
    nint l = 0;
    nint count = 0;
    foreach (var (i, x) in a) {
        nint n = len(x);
        if (n == 0) {
            continue;
        }
        if (l + n < l) {
            @throw(stringConcatenationTooˢ);
        }
        l += n;
        count++;
        idx = i;
    }
    if (count == 0) {
        return ""u8;
    }
    // If there is just one string and either it is not on the stack
    // or our result does not escape the calling frame (buf != nil),
    // then we can return that string directly.
    if (count == 1 && (Ꮡbuf != nil || !stringDataOnStack(a[idx]))) {
        return a[idx];
    }
    var (s, b) = rawstringtmp(Ꮡbuf, l);
    foreach (var (_, x) in a) {
        copy(b, x);
        b = b[(int)(len(x))..];
    }
    return s;
}

internal static @string concatstring2(ж<tmpBuf> Ꮡbuf, @string a0, @string a1) {
    return concatstrings(Ꮡbuf, new @string[]{a0, a1}.slice());
}

internal static @string concatstring3(ж<tmpBuf> Ꮡbuf, @string a0, @string a1, @string a2) {
    return concatstrings(Ꮡbuf, new @string[]{a0, a1, a2}.slice());
}

internal static @string concatstring4(ж<tmpBuf> Ꮡbuf, @string a0, @string a1, @string a2, @string a3) {
    return concatstrings(Ꮡbuf, new @string[]{a0, a1, a2, a3}.slice());
}

internal static @string concatstring5(ж<tmpBuf> Ꮡbuf, @string a0, @string a1, @string a2, @string a3, @string a4) {
    return concatstrings(Ꮡbuf, new @string[]{a0, a1, a2, a3, a4}.slice());
}

// slicebytetostring converts a byte slice to a string.
// It is inserted by the compiler into generated code.
// ptr is a pointer to the first element of the slice;
// n is the length of the slice.
// Buf is a fixed-size buffer for the result,
// it is not nil if the result does not escape.
//
// slicebytetostring should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/cloudwego/frugal
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname slicebytetostring
internal static @string slicebytetostring(ж<tmpBuf> Ꮡbuf, ж<byte> Ꮡptr, nint n) {
    ref var ptr = ref Ꮡptr.DerefOrNull();

    if (n == 0) {
        // Turns out to be a relatively common case.
        // Consider that you want to parse out data between parens in "foo()bar",
        // you find the indices and convert the subslice to string.
        return ""u8;
    }
    if (raceenabled) {
        racereadrangepc(new @unsafe.Pointer(Ꮡptr),
            (uintptr)n,
            getcallerpc(),
            abi.FuncPCABIInternal(slicebytetostring));
    }
    if (msanenabled) {
        msanread(new @unsafe.Pointer(Ꮡptr), (uintptr)n);
    }
    if (asanenabled) {
        asanread(new @unsafe.Pointer(Ꮡptr), (uintptr)n);
    }
    if (n == 1) {
        @unsafe.Pointer pΔ1 = new @unsafe.Pointer(Ꮡstaticuint64s.at<uint64>((nint)(ptr)));
        if (goarch.BigEndian) {
            pΔ1 = (uintptr)add(pΔ1, 7);
        }
        return @unsafe.String((ж<byte>)(uintptr)(pΔ1), 1);
    }
    @unsafe.Pointer Δp = default!;
    if (Ꮡbuf != nil && n <= 32){
        Δp = new @unsafe.Pointer(Ꮡbuf);
    } else {
        Δp = (uintptr)mallocgc((uintptr)n, nil, false);
    }
    memmove(Δp, new @unsafe.Pointer(Ꮡptr), (uintptr)n);
    return @unsafe.String((ж<byte>)(uintptr)(Δp), n);
}

// stringDataOnStack reports whether the string's data is
// stored on the current goroutine's stack.
internal static bool stringDataOnStack(@string s) {
    var ptr = (uintptr)@unsafe.StringData(s);
    var stk = getg().Value.stack;
    return stk.lo <= ptr && ptr < stk.hi;
}

internal static (@string s, slice<byte> b) rawstringtmp(ж<tmpBuf> Ꮡbuf, nint l) {
    @string s = default!;
    slice<byte> b = default!;

    ref var buf = ref Ꮡbuf.DerefOrNull();
    if (Ꮡbuf != nil && l <= 32){
        b = buf.Value[..(int)(l)];
        s = slicebytetostringtmp(Ꮡ(b, 0), len(b));
    } else {
        (s, b) = rawstring(l);
    }
    return (s, b);
}

// slicebytetostringtmp returns a "string" referring to the actual []byte bytes.
//
// Callers need to ensure that the returned string will not be used after
// the calling goroutine modifies the original slice or synchronizes with
// another goroutine.
//
// The function is only called when instrumenting
// and otherwise intrinsified by the compiler.
//
// Some internal compiler optimizations use this function.
//   - Used for m[T1{... Tn{..., string(k), ...} ...}] and m[string(k)]
//     where k is []byte, T1 to Tn is a nesting of struct and array literals.
//   - Used for "<"+string(b)+">" concatenation where b is []byte.
//   - Used for string(b)=="foo" comparison where b is []byte.
internal static @string slicebytetostringtmp(ж<byte> Ꮡptr, nint n) {
    if (raceenabled && n > 0) {
        racereadrangepc(new @unsafe.Pointer(Ꮡptr),
            (uintptr)n,
            getcallerpc(),
            abi.FuncPCABIInternal(slicebytetostringtmp));
    }
    if (msanenabled && n > 0) {
        msanread(new @unsafe.Pointer(Ꮡptr), (uintptr)n);
    }
    if (asanenabled && n > 0) {
        asanread(new @unsafe.Pointer(Ꮡptr), (uintptr)n);
    }
    return @unsafe.String(Ꮡptr, n);
}

internal static slice<byte> stringtoslicebyte(ж<tmpBuf> Ꮡbuf, @string s) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    slice<byte> b = default!;
    if (Ꮡbuf != nil && len(s) <= 32){
        buf = new tmpBuf(new byte[32].array());
        b = buf.Value[..(int)(len(s))];
    } else {
        b = rawbyteslice(len(s));
    }
    copy(b, s);
    return b;
}

internal static slice<rune> stringtoslicerune([GoArrayDims(32)] ж<array<rune>> Ꮡbuf, @string s) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    // two passes.
    // unlike slicerunetostring, no race because strings are immutable.
    nint n = 0;
    foreach ((_, _) in s) {
        n++;
    }
    slice<rune> a = default!;
    if (Ꮡbuf != nil && n <= 32){
        buf = new rune[]{}.array(32);
        a = buf[..(int)(n)];
    } else {
        a = rawruneslice(n);
    }
    n = 0;
    foreach (var (_, r) in s) {
        a[n] = r;
        n++;
    }
    return a;
}

internal static @string slicerunetostring(ж<tmpBuf> Ꮡbuf, slice<rune> a) {
    if (raceenabled && len(a) > 0) {
        racereadrangepc(new @unsafe.Pointer(Ꮡ(a, 0)),
            (uintptr)len(a) * /* unsafe.Sizeof(a[0]) */ (uintptr)4,
            getcallerpc(),
            abi.FuncPCABIInternal(slicerunetostring));
    }
    if (msanenabled && len(a) > 0) {
        msanread(new @unsafe.Pointer(Ꮡ(a, 0)), (uintptr)len(a) * /* unsafe.Sizeof(a[0]) */ (uintptr)4);
    }
    if (asanenabled && len(a) > 0) {
        asanread(new @unsafe.Pointer(Ꮡ(a, 0)), (uintptr)len(a) * /* unsafe.Sizeof(a[0]) */ (uintptr)4);
    }
    array<byte> dum = new(4);
    nint size1 = 0;
    foreach (var (_, r) in a) {
        size1 += encoderune(dum[..], r);
    }
    var (s, b) = rawstringtmp(Ꮡbuf, size1 + 3);
    nint size2 = 0;
    foreach (var (_, r) in a) {
        // check for race
        if (size2 >= size1) {
            break;
        }
        size2 += encoderune(b[(int)(size2)..], r);
    }
    return s[..(int)(size2)];
}

[GoType] partial struct stringStruct {
    internal @unsafe.Pointer str;
    internal nint len;
}

// Variant with *byte pointer type for DWARF debugging.
[GoType] partial struct stringStructDWARF {
    internal ж<byte> str;
    internal nint len;
}

internal static ж<stringStruct> stringStructOf(ж<@string> Ꮡsp) {
    return Ꮡsp.Reinterpret<@string, stringStruct>();
}

internal static @string /*s*/ intstring([GoArrayDims(4)] ж<array<byte>> Ꮡbuf, int64 v) {
    @string s = default!;

    ref var buf = ref Ꮡbuf.DerefOrNull();
    slice<byte> b = default!;
    if (Ꮡbuf != nil){
        b = buf[..];
        s = slicebytetostringtmp(Ꮡ(b, 0), len(b));
    } else {
        (s, b) = rawstring(4);
    }
    if ((int64)(rune)v != v) {
        v = runeError;
    }
    nint n = encoderune(b, (rune)v);
    return s[..(int)(n)];
}

// rawstring allocates storage for a new string. The returned
// string and byte slice both refer to the same storage.
// The storage is not zeroed. Callers should use
// b to set the string contents and then drop b.
internal static (@string s, slice<byte> b) rawstring(nint size) {
    @unsafe.Pointer Δp = (uintptr)mallocgc((uintptr)size, nil, false);
    return (@unsafe.String((ж<byte>)(uintptr)(Δp), size), @unsafe.Slice((ж<byte>)(uintptr)(Δp), size));
}

// rawbyteslice allocates a new byte slice. The byte slice is not zeroed.
internal static slice<byte> /*b*/ rawbyteslice(nint size) {
    ref var b = ref heap<slice<byte>>(out var Ꮡb);

    var cap = roundupsize((uintptr)size, true);
    @unsafe.Pointer Δp = (uintptr)mallocgc(cap, nil, false);
    if (cap != (uintptr)size) {
        memclrNoHeapPointers((uintptr)add(Δp, (uintptr)size), cap - (uintptr)size);
    }
    (Ꮡb.Reinterpret<slice<byte>, Δsliceᴛ>()).Value = new Δsliceᴛ(Δp.Value, size, (nint)cap);
    return b;
}

// rawruneslice allocates a new rune slice. The rune slice is not zeroed.
internal static slice<rune> /*b*/ rawruneslice(nint size) {
    ref var b = ref heap<slice<rune>>(out var Ꮡb);

    if ((uintptr)size > (uintptr)(maxAlloc / 4)) {
        @throw(outOfMemoryˢ);
    }
    var mem = roundupsize((uintptr)size * 4, true);
    @unsafe.Pointer Δp = (uintptr)mallocgc(mem, nil, false);
    if (mem != (uintptr)size * 4) {
        memclrNoHeapPointers((uintptr)add(Δp, (uintptr)size * 4), mem - (uintptr)size * 4);
    }
    (Ꮡb.Reinterpret<slice<rune>, Δsliceᴛ>()).Value = new Δsliceᴛ(Δp.Value, size, (nint)(mem / 4));
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gobytesLengthOutOfRangeˢ = "gobytes: length out of range"u8;

// used by cmd/cgo
internal static slice<byte> /*b*/ gobytes(ж<byte> Ꮡp, nint n) {
    ref var b = ref heap<slice<byte>>(out var Ꮡb);

    if (n == 0) {
        return new slice<byte>(0);
    }
    if (n < 0 || (uintptr)n > maxAlloc) {
        throw panic(((errorString)(@string)gobytesLengthOutOfRangeˢ));
    }
    @unsafe.Pointer bp = (uintptr)mallocgc((uintptr)n, nil, false);
    memmove(bp, new @unsafe.Pointer(Ꮡp), (uintptr)n);
    (Ꮡb.Reinterpret<slice<byte>, Δsliceᴛ>()).Value = new Δsliceᴛ(bp.Value, n, n);
    return b;
}

// This is exported via linkname to assembly in syscall (for Plan9) and cgo.
//
//go:linkname gostring
internal static @string gostring(ж<byte> Ꮡp) {
    nint l = findnull(Ꮡp);
    if (l == 0) {
        return ""u8;
    }
    var (s, b) = rawstring(l);
    memmove(new @unsafe.Pointer(Ꮡ(b, 0)), new @unsafe.Pointer(Ꮡp), (uintptr)l);
    return s;
}

// internal_syscall_gostring is a version of gostring for internal/syscall/unix.
//
//go:linkname internal_syscall_gostring internal/syscall/unix.gostring
internal static @string internal_syscall_gostring(ж<byte> Ꮡp) {
    return gostring(Ꮡp);
}

internal static @string gostringn(ж<byte> Ꮡp, nint l) {
    if (l == 0) {
        return ""u8;
    }
    var (s, b) = rawstring(l);
    memmove(new @unsafe.Pointer(Ꮡ(b, 0)), new @unsafe.Pointer(Ꮡp), (uintptr)l);
    return s;
}

internal const uint64 maxUint64 = /* ^uint64(0) */ 18446744073709551615;
internal const int64 maxInt64 = /* int64(maxUint64 >> 1) */ 9223372036854775807;

// atoi64 parses an int64 from a string s.
// The bool result reports whether s is a number
// representable by a value of type int64.
internal static (int64, bool) atoi64(@string s) {
    if (s == ""u8) {
        return (0, false);
    }
    var neg = false;
    if (s[0] == (rune)'-') {
        neg = true;
        s = s[1..];
    }
    var un = (uint64)0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        if (c < (rune)'0' || c > (rune)'9') {
            return (0, false);
        }
        if (un > maxUint64 / 10) {
            // overflow
            return (0, false);
        }
        un *= 10;
        var un1 = un + (uint64)c - (rune)'0';
        if (un1 < un) {
            // overflow
            return (0, false);
        }
        un = un1;
    }
    if (!neg && un > (uint64)maxInt64) {
        return (0, false);
    }
    if (neg && un > (uint64)maxInt64 + 1) {
        return (0, false);
    }
    var n = (int64)un;
    if (neg) {
        n = -n;
    }
    return (n, true);
}

// atoi is like atoi64 but for integers
// that fit into an int.
internal static (nint, bool) atoi(@string s) {
    {
        var (n, ok) = atoi64(s); if (n == (int64)(nint)n) {
            return ((nint)n, ok);
        }
    }
    return (0, false);
}

// atoi32 is like atoi but for integers
// that fit into an int32.
internal static (int32, bool) atoi32(@string s) {
    {
        var (n, ok) = atoi64(s); if (n == (int64)(int32)n) {
            return ((int32)n, ok);
        }
    }
    return (0, false);
}

// parseByteCount parses a string that represents a count of bytes.
//
// s must match the following regular expression:
//
//	^[0-9]+(([KMGT]i)?B)?$
//
// In other words, an integer byte count with an optional unit
// suffix. Acceptable suffixes include one of
// - KiB, MiB, GiB, TiB which represent binary IEC/ISO 80000 units, or
// - B, which just represents bytes.
//
// Returns an int64 because that's what its callers want and receive,
// but the result is always non-negative.
internal static (int64, bool) parseByteCount(@string s) {
    // The empty string is not valid.
    if (s == ""u8) {
        return (0, false);
    }
    // Handle the easy non-suffix case.
    var last = s[len(s) - 1];
    if (last >= (rune)'0' && last <= (rune)'9') {
        var (nΔ1, okΔ1) = atoi64(s);
        if (!okΔ1 || nΔ1 < 0) {
            return (0, false);
        }
        return (nΔ1, okΔ1);
    }
    // Failing a trailing digit, this must always end in 'B'.
    // Also at this point there must be at least one digit before
    // that B.
    if (last != (rune)'B' || len(s) < 2) {
        return (0, false);
    }
    // The one before that must always be a digit or 'i'.
    {
        var c = s[len(s) - 2]; if (c >= (rune)'0' && c <= (rune)'9'){
            // Trivial 'B' suffix.
            var (nΔ2, okΔ2) = atoi64(s[..(int)(len(s) - 1)]);
            if (!okΔ2 || nΔ2 < 0) {
                return (0, false);
            }
            return (nΔ2, okΔ2);
        } else 
        if (c != (rune)'i') {
            return (0, false);
        }
    }
    // Finally, we need at least 4 characters now, for the unit
    // prefix and at least one digit.
    if (len(s) < 4) {
        return (0, false);
    }
    nint power = 0;
    switch (s[len(s) - 3]) {
    case (rune)'K': {
        power = 1;
        break;
    }
    case (rune)'M': {
        power = 2;
        break;
    }
    case (rune)'G': {
        power = 3;
        break;
    }
    case (rune)'T': {
        power = 4;
        break;
    }
    default: {
        return (0, false);
    }}

    // Invalid suffix.
    var m = (uint64)1;
    for (nint i = 0; i < power; i++) {
        m *= 1024;
    }
    var (n, ok) = atoi64(s[..(int)(len(s) - 3)]);
    if (!ok || n < 0) {
        return (0, false);
    }
    var un = (uint64)n;
    if (un > maxUint64 / m) {
        // Overflow.
        return (0, false);
    }
    un *= m;
    if (un > (uint64)maxInt64) {
        // Overflow.
        return (0, false);
    }
    return ((int64)un, true);
}

//go:nosplit
internal static nint findnull(ж<byte> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (Ꮡs == nil) {
        return 0;
    }
    // Avoid IndexByteString on Plan 9 because it uses SSE instructions
    // on x86 machines, and those are classified as floating point instructions,
    // which are illegal in a note handler.
    if (GOOS == "plan9"u8) {
        var Δp = array<byte>.AliasPointer(Ꮡs, unchecked((nint)140737488355327));
        nint l = 0;
        while (Δp.Value[l] != 0) {
            l++;
        }
        return l;
    }
    // pageSize is the unit we scan at a time looking for NULL.
    // It must be the minimum page size for any architecture Go
    // runs on. It's okay (just a minor performance loss) if the
    // actual system page size is larger than this value.
    UntypedInt pageSize = 4096;
    nint offset = 0;
    ref var ptr = ref heap<@unsafe.Pointer>(out var Ꮡptr);
    ptr = new @unsafe.Pointer(Ꮡs);
    // IndexByteString uses wide reads, so we need to be careful
    // with page boundaries. Call IndexByteString on
    // [ptr, endOfPage) interval.
    ref var safeLen = ref heap<nint>(out var ᏑsafeLen);
    safeLen = (nint)((uintptr)pageSize - (uintptr)ptr % (uintptr)pageSize);
    while (ᐧ) {
        @string t = ~Ꮡ(new stringStruct(ptr.Value, safeLen)).Reinterpret<stringStruct, @string>();
        // Check one page at a time.
        {
            nint i = bytealg.IndexByteString(t, 0); if (i != -1) {
                return offset + i;
            }
        }
        // Move to next page
        ptr = (@unsafe.Pointer)((uintptr)ptr + (uintptr)safeLen);
        offset += safeLen;
        safeLen = pageSize;
    }
}

internal static nint findnullw(ж<uint16> Ꮡs) {
    if (Ꮡs == nil) {
        return 0;
    }
    var Δp = array<uint16>.AliasPointer(Ꮡs, unchecked((nint)70368744177663));
    nint l = 0;
    while (Δp.Value[l] != 0) {
        l++;
    }
    return l;
}

//go:nosplit
internal static @string gostringnocopy(ж<byte> Ꮡstr) {
    ref var ss = ref heap<stringStruct>(out var Ꮡss);
    ss = new stringStruct(str: new @unsafe.Pointer(Ꮡstr), len: findnull(Ꮡstr));
    @string s = ~Ꮡss.Reinterpret<stringStruct, @string>();
    return s;
}

internal static @string gostringw(ж<uint16> Ꮡstrw) {
    array<byte> buf = new(8);
    var str = array<uint16>.AliasPointer(Ꮡstrw, unchecked((nint)70368744177663));
    nint n1 = 0;
    for (nint i = 0; str.Value[i] != 0; i++) {
        n1 += encoderune(buf[..], (rune)str.Value[i]);
    }
    var (s, b) = rawstring(n1 + 4);
    nint n2 = 0;
    for (nint i = 0; str.Value[i] != 0; i++) {
        // check for race
        if (n2 >= n1) {
            break;
        }
        n2 += encoderune(b[(int)(n2)..], (rune)str.Value[i]);
    }
    b[n2] = 0; // for luck
    return s[..(int)(n2)];
}

} // end runtime_package
