// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δos = os_package;
using Δtesting = testing_package;
using static go.os_internal_test_package;

partial class os_test_package {

[GoInit] internal static void initΔ2() {
    pipeDeadlinesTestCases = new pipeDeadlineTest[]{
        new(
            "named overlapped pipe"u8,
            (ж<Δtesting.T> t) => {
                ж<Δos.File> r = default!;
                ж<Δos.File> w = default!;
                @string name = pipeName();
                w = newBytePipe(new os_test_package.testing_TжTB(t), name, true);
                r = newFileOverlapped(new os_test_package.testing_TжTB(t), name, true);
                return (r, w);
            }
        )
    }.slice();
}

} // end os_test_package
