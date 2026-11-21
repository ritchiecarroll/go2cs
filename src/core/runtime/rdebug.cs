// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

/* import _ "unsafe" (nothing to initialize) */ // for go:linkname

partial class runtime_package {

//go:linkname setMaxStack runtime/debug.setMaxStack
internal static nint /*out*/ setMaxStack(nint @in) {
    nint @out = default;

    @out = (nint)maxstacksize;
    maxstacksize = (uintptr)@in;
    return @out;
}

//go:linkname setPanicOnFault runtime/debug.setPanicOnFault
internal static bool /*old*/ setPanicOnFault(bool @new) {
    bool old = default;

    var gp = getg();
    old = gp.Value.paniconfault;
    gp.Value.paniconfault = @new;
    return old;
}

} // end runtime_package
