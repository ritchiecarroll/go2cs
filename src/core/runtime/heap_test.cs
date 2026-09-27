// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

//go:linkname heapObjectsCanMove runtime.heapObjectsCanMove
internal static bool heapObjectsCanMove() {
    return global::go.runtime_package.heapObjectsCanMove();
}

public static void TestHeapObjectsCanMove(ж<testing.T> Ꮡt) {
    if (heapObjectsCanMove()) {
        // If this happens (or this test stops building),
        // it will break go4.org/unsafe/assume-no-moving-gc.
        Ꮡt.Fatalf("heap objects can move!"u8);
    }
}

} // end runtime_test_package
