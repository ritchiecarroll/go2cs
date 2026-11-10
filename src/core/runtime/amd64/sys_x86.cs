// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || 386
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// adjust Gobuf as if it executed a call to fn with context ctxt
// and then stopped before the first instruction in fn.
internal static void gostartcall(ref gobuf buf, @unsafe.Pointer fn, @unsafe.Pointer ctxt) {
    var sp = buf.sp;
    sp -= goarch.PtrSize;
    ((ж<uintptr>)(uintptr)((@unsafe.Pointer)sp)).Value = buf.pc;
    buf.sp = sp;
    buf.pc = (uintptr)fn;
    buf.ctxt = ctxt;
}

} // end runtime_package
