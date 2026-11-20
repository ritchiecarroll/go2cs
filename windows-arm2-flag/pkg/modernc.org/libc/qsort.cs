// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using sort = sort_package;
using @unsafe = unsafe_package;
using static libc_package;

partial class libc_package {

internal static void qsort(ж<TLS> t, uintptr @base, uintptr nmemb, uintptr size, uintptr compar) {
    if (nmemb < 2 || size == 0) {
        return;
    }

    var s = Ꮡ(new sorter(
        len: (nint)nmemb,
        @base: @base,
        sz: size,
        f: GoFuncCookie.Func<Func<ж<TLS>, uintptr, uintptr, int32>>(compar),
        t: t
    ));
    switch (ᐧ) {
    case {} when @base % 8 == 0 && size % 8 == 0: {
        s.word = 8;
        break;
    }
    case {} when @base % 4 == 0 && size % 4 == 0: {
        s.word = 4;
        break;
    }
    default: {
        s.word = 1;
        break;
    }}

    sort.Sort(new sorterжInterface(s));
}

partial struct sorter {
    internal nint len;
    internal uintptr @base;
    internal uintptr sz;
    internal uintptr word;
    internal Func<ж<TLS>, uintptr, uintptr, int32> f;
    internal ж<TLS> t;
}

} partial class libcᴛsorter {

internal static nint Len(this ref sorter s) { return s.len; }

internal static bool Less(this ref sorter s, nint i, nint j) {
    return s.f(s.t, s.@base + (uintptr)i * s.sz, s.@base + (uintptr)j * s.sz) < 0;
}

internal static void Swap(this ref sorter s, nint i, nint j) {
    var p = s.@base + (uintptr)i * s.sz;
    var q = s.@base + (uintptr)j * s.sz;
    var n = s.sz;
    switch (s.word) {
    case var ᴛ when ᴛ == 8: {
        for (; n >= 8; n -= 8) {
            ((@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p)).Value, (@unsafe.Pointer.To<uint64>((@unsafe.Pointer)q)).Value) = (~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)q), ~@unsafe.Pointer.To<uint64>((@unsafe.Pointer)p));
            p += 8;
            q += 8;
        }
        break;
    }
    case var ᴛ when ᴛ == 4: {
        for (; n >= 4; n -= 4) {
            ((@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p)).Value, (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)q)).Value) = (~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)q), ~@unsafe.Pointer.To<uint32>((@unsafe.Pointer)p));
            p += 4;
            q += 4;
        }
        break;
    }}

    for (; n != 0; n--) {
        ((@unsafe.Pointer.To<byte>((@unsafe.Pointer)p)).Value, (@unsafe.Pointer.To<byte>((@unsafe.Pointer)q)).Value) = (~@unsafe.Pointer.To<byte>((@unsafe.Pointer)q), ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)p));
        p++;
        q++;
    }
}

} // end libcᴛsorter
