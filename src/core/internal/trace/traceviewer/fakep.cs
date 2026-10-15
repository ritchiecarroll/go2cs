// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

partial class traceviewer_package {

// Special P identifiers:
public static UntypedInt FakeP => /* 1000000 + iota */ 1000000;
public static UntypedInt TimerP => 1000001; // depicts timer unblocks
public static UntypedInt NetpollP => 1000002; // depicts network unblocks
public static UntypedInt SyscallP => 1000003; // depicts returns from syscalls
public static UntypedInt GCP => 1000004; // depicts GC state
public static UntypedInt ProfileP => 1000005; // depicts recording of CPU profile samples

} // end traceviewer_package
