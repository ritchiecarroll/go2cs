// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static ж<Δsync.WaitGroup> Ꮡwg = new StandardBox<Δsync.WaitGroup>(default(Δsync.WaitGroup));
internal static ref Δsync.WaitGroup wg => ref Ꮡwg.Value;

[GoInit] internal static void initΔ4() {
    runtime_internal_test_package.NetpollGenericInit();
}

public static partial void BenchmarkNetpollBreak(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < 10; j++) {
            Ꮡwg.Add(1);
            goǃ(() => {
                runtime_internal_test_package.NetpollBreak();
                Ꮡwg.Done();
            });
        }
    }
    Ꮡwg.Wait();
    b.StopTimer();
}

} // end runtime_test_package
