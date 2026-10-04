// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_package;

partial class runtime_internal_test_package {

[MethodImpl(MethodImplOptions.NoInlining)] public static void XTestSPWrite(TestingT t) {
    // Test that we can traceback from the stack check prologue of a function
    // that writes to SP. See #62326.
    // Start a goroutine to minimize the initial stack and ensure we grow the stack.
    var done = new channel<bool>(0);
    var doneʗ1 = done;
    goǃ(() => {
        testSPWrite(); // Defined in assembly
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
}

} // end runtime_internal_test_package
