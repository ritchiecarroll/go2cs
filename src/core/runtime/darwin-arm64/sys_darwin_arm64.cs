// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// libc function wrappers. Must run on system stack.

//go:nosplit
//go:cgo_unsafe_args
internal static int32 g0_pthread_key_create(ж<pthreadkey> Ꮡk, uintptr destructor) {
    ref var k = ref Ꮡk.DerefOrNull();

    var ret = asmcgocall((@unsafe.Pointer)abi.FuncPCABI0(pthread_key_create_trampoline), @unsafe.Pointer.FromBox(Ꮡ(k)));
    KeepAlive(Ꮡk.OrTypedNil());
    return ret;
}
internal static partial void pthread_key_create_trampoline();

//go:nosplit
//go:cgo_unsafe_args
internal static int32 g0_pthread_setspecific(pthreadkey kʗp, uintptr value) {
    ref var k = ref heap(kʗp, out var Ꮡk);

    return asmcgocall((@unsafe.Pointer)abi.FuncPCABI0(pthread_setspecific_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡk));
}
internal static partial void pthread_setspecific_trampoline();

//go:cgo_import_dynamic libc_pthread_key_create pthread_key_create "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_setspecific pthread_setspecific "/usr/lib/libSystem.B.dylib"

// tlsinit allocates a thread-local storage slot for g.
//
// It finds the first available slot using pthread_key_create and uses
// it as the offset value for runtime.tlsg.
//
// This runs at startup on g0 stack, but before g is set, so it must
// not split stack (transitively). g is expected to be nil, so things
// (e.g. asmcgocall) will skip saving or reading g.
//
//go:nosplit
internal static void tlsinit(ref uintptr tlsg, ref array<uintptr> tlsbase) {
    ref var k = ref heap(new pthreadkey(), out var Ꮡk);
    var err = g0_pthread_key_create(Ꮡk, 0);
    if (err != 0) {
        abort();
    }

    uintptr magic = unchecked((uintptr)0xc476c475c47957);
    err = g0_pthread_setspecific(k, magic);
    if (err != 0) {
        abort();
    }

    foreach (var (i, x) in range(tlsbase)) {
        if (x == magic) {
            tlsg = (uintptr)(i * (nint)goarch.PtrSize);
            g0_pthread_setspecific(k, 0);
            return;
        }
    }
    abort();
}

} // end runtime_package
