// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using bytealg = @internal.bytealg_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using go.unicode;

partial class strings_package {

// A Builder is used to efficiently build a string using [Builder.Write] methods.
// It minimizes memory copying. The zero value is ready to use.
// Do not copy a non-zero Builder.
partial struct Builder {
    internal ж<Builder> addr; // of receiver, to detect copies by value
    // External users should never get direct access to this buffer, since
    // the slice at some point will be converted to a string using unsafe, also
    // data between len(buf) and cap(buf) might be uninitialized.
    internal slice<byte> buf;
}

internal static void copyCheck(this ж<Builder> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (b.addr == nil){
        // This hack works around a failing of Go's escape analysis
        // that was causing b to escape and be heap allocated.
        // See issue 23382.
        // TODO: once issue 7921 is fixed, this should be reverted to
        // just "b.addr = b".
        b.addr = Ꮡb;
    } else 
    if (b.addr != Ꮡb) {
        throw panic("strings: illegal use of non-zero Builder copied by value");
    }
}

// String returns the accumulated string.
public static @string String(this ref Builder b) {
    return @unsafe.String(@unsafe.SliceData(b.buf), len(b.buf));
}

// Len returns the number of accumulated bytes; b.Len() == len(b.String()).
public static nint Len(this ref Builder b) {
    return len(b.buf);
}

// Cap returns the capacity of the builder's underlying byte slice. It is the
// total space allocated for the string being built and includes any bytes
// already written.
public static nint Cap(this ref Builder b) {
    return cap(b.buf);
}

// Reset resets the [Builder] to be empty.
public static void Reset(this ref Builder b) {
    b.addr = default!;
    b.buf = default!;
}

// grow copies the buffer to a new, larger buffer so that there are at least n
// bytes of capacity beyond len(b.buf).
internal static void grow(this ref Builder b, nint n) {
    var buf = bytealg.MakeNoZero(2 * cap(b.buf) + n).slice(0, len(b.buf));
    copy(buf, b.buf);
    b.buf = buf;
}

// Grow grows b's capacity, if necessary, to guarantee space for
// another n bytes. After Grow(n), at least n bytes can be written to b
// without another allocation. If n is negative, Grow panics.
public static void Grow(this ж<Builder> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    if (n < 0) {
        throw panic("strings.Builder.Grow: negative count");
    }
    if (cap(b.buf) - len(b.buf) < n) {
        b.grow(n);
    }
}

// Write appends the contents of p to b's buffer.
// Write always returns len(p), nil.
public static (nint, error) Write(this ж<Builder> Ꮡb, slice<byte> p) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    b.buf = appendꓸꓸꓸ(b.buf, p);
    return (len(p), default!);
}

// WriteByte appends the byte c to b's buffer.
// The returned error is always nil.
public static error WriteByte(this ж<Builder> Ꮡb, byte c) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    b.buf = append(b.buf, c);
    return default!;
}

// WriteRune appends the UTF-8 encoding of Unicode code point r to b's buffer.
// It returns the length of r and a nil error.
public static (nint, error) WriteRune(this ж<Builder> Ꮡb, rune r) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    nint n = len(b.buf);
    b.buf = utf8.AppendRune(b.buf, r);
    return (len(b.buf) - n, default!);
}

// WriteString appends the contents of s to b's buffer.
// It returns the length of s and a nil error.
public static (nint, error) WriteString(this ж<Builder> Ꮡb, @string s) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡb.copyCheck();
    b.buf = append(b.buf, s.ꓸꓸꓸ);
    return (len(s), default!);
}

} // end strings_package
