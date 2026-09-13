// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class main_package {

[GoType("num:nint")] partial struct RF_State;

public static RF_State RF_DONE => /* RF_State(iota) */ 0;
public static RF_State RF_READY => 1;
public static RF_State RF_PANIC => 2;
public static RF_State RF_EXHAUSTED => 3;
public static UntypedInt RF_MISSING_PANIC => 4;

} // end main_package
