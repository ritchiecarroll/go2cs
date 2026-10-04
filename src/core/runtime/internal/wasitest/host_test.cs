// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime.@internal.wasitest;

using flag = flag_package;

partial class wasi_test_package {

internal static ж<@string> Ꮡtarget = new StandardBox<@string>(default(@string));
internal static ref @string target => ref Ꮡtarget.Value;

[GoInit] internal static void init() {
    // The dist test runner passes -target when running this as a host test.
    flag.StringVar(Ꮡtarget, "target"u8, ""u8, ""u8);
}

} // end wasi_test_package
