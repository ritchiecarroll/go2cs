// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidUseOfGostartcallˢ => ref invalidUseOfGostartcallˢᵇ;
private static readonly @string invalidUseOfGostartcallˢᵇ = "invalid use of gostartcall"u8;

// adjust Gobuf as if it executed a call to fn with context ctxt
// and then did an immediate Gosave.
internal static void gostartcall(ref gobuf buf, @unsafe.Pointer fn, @unsafe.Pointer ctxt) {
    if (buf.lr != 0) {
        @throw(invalidUseOfGostartcallˢ);
    }
    buf.lr = buf.pc;
    buf.pc = (uintptr)fn;
    buf.ctxt = ctxt;
}

} // end runtime_package
