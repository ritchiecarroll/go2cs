// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

//go:cgo_export_static main
// Filled in by runtime/cgo when linked into binary.
//go:linkname _cgo_init _cgo_init
//go:linkname _cgo_thread_start _cgo_thread_start
//go:linkname _cgo_sys_thread_create _cgo_sys_thread_create
//go:linkname _cgo_notify_runtime_init_done _cgo_notify_runtime_init_done
//go:linkname _cgo_callers _cgo_callers
//go:linkname _cgo_set_traceback_functions _cgo_set_traceback_functions
//go:linkname _cgo_call_traceback_function _cgo_call_traceback_function
//go:linkname _cgo_call_symbolizer_function _cgo_call_symbolizer_function
//go:linkname _cgo_yield _cgo_yield
//go:linkname _cgo_pthread_key_created _cgo_pthread_key_created
//go:linkname _cgo_bindm _cgo_bindm
//go:linkname _cgo_getstackbound _cgo_getstackbound
internal static @unsafe.Pointer _cgo_init;
internal static @unsafe.Pointer _cgo_thread_start;
internal static @unsafe.Pointer _cgo_sys_thread_create;
internal static @unsafe.Pointer _cgo_notify_runtime_init_done;
internal static @unsafe.Pointer _cgo_callers;
internal static @unsafe.Pointer _cgo_set_traceback_functions;
internal static @unsafe.Pointer _cgo_call_traceback_function;
internal static @unsafe.Pointer _cgo_call_symbolizer_function;
internal static ж<@unsafe.Pointer> Ꮡ_cgo_yield = new StandardBox<@unsafe.Pointer>(default(@unsafe.Pointer));
internal static ref @unsafe.Pointer _cgo_yield => ref Ꮡ_cgo_yield.Value;
internal static @unsafe.Pointer _cgo_pthread_key_created;
internal static @unsafe.Pointer _cgo_bindm;
internal static @unsafe.Pointer _cgo_getstackbound;

// iscgo is set to true by the runtime/cgo package
//
// iscgo should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ebitengine/purego
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname iscgo
public static bool iscgo;

// set_crosscall2 is set by the runtime/cgo package
// set_crosscall2 should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ebitengine/purego
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname set_crosscall2
internal static Action set_crosscall2;

// cgoHasExtraM is set on startup when an extra M is created for cgo.
// The extra M must be created before any C/C++ code calls cgocallback.
internal static bool cgoHasExtraM;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgoUseShouldNotBeCalledˢ = "cgoUse should not be called"u8;

// cgoUse is called by cgo-generated code (using go:linkname to get at
// an unexported name). The calls serve two purposes:
// 1) they are opaque to escape analysis, so the argument is considered to
// escape to the heap.
// 2) they keep the argument alive until the call site; the call is emitted after
// the end of the (presumed) use of the argument by C.
// cgoUse should not actually be called (see cgoAlwaysFalse).
internal static void cgoUse(any _) {
    @throw(cgoUseShouldNotBeCalledˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cgoKeepAliveShouldNotBeˢ = "cgoKeepAlive should not be called"u8;

// cgoKeepAlive is called by cgo-generated code (using go:linkname to get at
// an unexported name). This call keeps its argument alive until the call site;
// cgo emits the call after the last possible use of the argument by C code.
// cgoKeepAlive is marked in the cgo-generated code as //go:noescape, so
// unlike cgoUse it does not force the argument to escape to the heap.
// This is used to implement the #cgo noescape directive.
internal static void cgoKeepAlive(any _) {
    @throw(cgoKeepAliveShouldNotBeˢ);
}

// cgoAlwaysFalse is a boolean value that is always false.
// The cgo-generated code says if cgoAlwaysFalse { cgoUse(p) },
// or if cgoAlwaysFalse { cgoKeepAlive(p) }.
// The compiler cannot see that cgoAlwaysFalse is always false,
// so it emits the test and keeps the call, giving the desired
// escape/alive analysis result. The test is cheaper than the call.
public static bool cgoAlwaysFalse;

internal static ж<@unsafe.Pointer> cgo_yield = Ꮡ_cgo_yield;

internal static void cgoNoCallback(bool v) {
    var g = getg();
    if ((~g).nocgocallback && v) {
        throw panic("runtime: unexpected setting cgoNoCallback");
    }
    g.Value.nocgocallback = v;
}

} // end runtime_package
