// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.io_package;

partial class io_internal_test_package {

// exported for test
public static error ErrInvalidWrite;
internal static void initᴛErrInvalidWrite() { ErrInvalidWrite = errInvalidWrite; }
public static error ErrWhence;
internal static void initᴛErrWhence() { ErrWhence = errWhence; }
public static error ErrOffset;
internal static void initᴛErrOffset() { ErrOffset = errOffset; }

} // end io_internal_test_package
