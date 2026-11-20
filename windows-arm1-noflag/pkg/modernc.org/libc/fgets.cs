// Copyright 2026 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using @unsafe = unsafe_package;
using stdio = go.modernc.org.libc.stdio_package;
using go.modernc.org.libc;

partial class libc_package {

public static partial uintptr Xfgets(ж<TLS> t, uintptr s, int32 size, uintptr stream) {
    var bᴷ = new stackAppend<byte>();

    if (__ccgo_strace) {
        trc("t=%v s=%v size=%v stream=%v, (%v:)", t.OrTypedNil(), s, size, stream, origin(2));
    }
    if (size < 1) {
        return 0;
    }
    if (size == 1) {
        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)s)).Value = 0;
        return s;
    }
    slice<byte> b = default;
@out:
    for (; size > 1; size--) {
        var c = Xfgetc(t, stream);
        switch (c) {
        case '\n': {
            b = append(bᴷ, b, (byte)c);
            goto break_out;
            break;
        }
        case var ᴛ when ᴛ == stdio.EOF: {
            goto break_out;
            break;
        }
        default: {
            b = append(b, (byte)c);
            break;
        }}

continue_out:;
    }
break_out:;
    if (len(b) == 0) {
        return 0;
    }

    b = append(b, (byte)0);
    copy(@unsafe.Pointer.View<byte>((@unsafe.Pointer)s, 0, len(b), len(b), 1125899906842623), b);
    return s;
}

} // end libc_package
