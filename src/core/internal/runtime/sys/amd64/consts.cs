// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using goarch = go.@internal.goarch_package;
using goos = go.@internal.goos_package;
using go.@internal;

partial class sys_package {

// AIX and OpenBSD require a larger stack for syscalls.
// The race build also needs more stack. See issue 54291.
// This arithmetic must match that in cmd/internal/objabi/stack.go:stackGuardMultiplier.
public static UntypedInt StackGuardMultiplier => /* 1 + goos.IsAix + goos.IsOpenbsd + isRace */ 1;

// DefaultPhysPageSize is the default physical page size.
public static UntypedInt DefaultPhysPageSize => /* goarch.DefaultPhysPageSize */ 4096;

// PCQuantum is the minimal unit for a program counter (1 on x86, 4 on most other systems).
// The various PC tables record PC deltas pre-divided by PCQuantum.
public static UntypedInt PCQuantum => /* goarch.PCQuantum */ 1;

// Int64Align is the required alignment for a 64-bit integer (4 on 32-bit systems, 8 on 64-bit).
public static UntypedInt Int64Align => /* goarch.PtrSize */ 8;

// MinFrameSize is the size of the system-reserved words at the bottom
// of a frame (just above the architectural stack pointer).
// It is zero on x86 and PtrSize on most non-x86 (LR-based) systems.
// On PowerPC it is larger, to cover three more reserved words:
// the compiler word, the link editor word, and the TOC save word.
public static UntypedInt MinFrameSize => /* goarch.MinFrameSize */ 0;

// StackAlign is the required alignment of the SP register.
// The stack must be at least word aligned, but some architectures require more.
public static UntypedInt StackAlign => /* goarch.StackAlign */ 8;

} // end sys_package
