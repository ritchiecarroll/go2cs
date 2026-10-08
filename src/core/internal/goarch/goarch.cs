// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goarch contains GOARCH-specific constants.
namespace go.@internal;

partial class goarch_package {

partial struct ArchFamilyType /*num:nint*/;

// The next line makes 'go generate' write the zgoarch*.go files with
// per-arch information, including constants named $GOARCH for every
// GOARCH. The constant is 1 on the current system, 0 otherwise; multiplying
// by them is useful for defining GOARCH-specific constants.
//
//go:generate go run gengoarch.go
public static ArchFamilyType AMD64 => /* iota */ 0;
public static ArchFamilyType ARM => 1;
public static ArchFamilyType ARM64 => 2;
public static ArchFamilyType I386 => 3;
public static ArchFamilyType LOONG64 => 4;
public static ArchFamilyType MIPS => 5;
public static ArchFamilyType MIPS64 => 6;
public static ArchFamilyType PPC64 => 7;
public static ArchFamilyType RISCV64 => 8;
public static ArchFamilyType S390X => 9;
public static ArchFamilyType WASM => 10;

// PtrSize is the size of a pointer in bytes - unsafe.Sizeof(uintptr(0)) but as an ideal constant.
// It is also the size of the machine's native word size (that is, 4 on 32-bit systems, 8 on 64-bit).
public static UntypedInt PtrSize => /* 4 << (^uintptr(0) >> 63) */ 8;

// ArchFamily is the architecture family (AMD64, ARM, ...)
public static ArchFamilyType ArchFamily => /* _ArchFamily */ 0;

// BigEndian reports whether the architecture is big-endian.
public const bool BigEndian = /* IsArmbe|IsArm64be|IsMips|IsMips64|IsPpc|IsPpc64|IsS390|IsS390x|IsSparc|IsSparc64 == 1 */ false;

// DefaultPhysPageSize is the default physical page size.
public static UntypedInt DefaultPhysPageSize => /* _DefaultPhysPageSize */ 4096;

// PCQuantum is the minimal unit for a program counter (1 on x86, 4 on most other systems).
// The various PC tables record PC deltas pre-divided by PCQuantum.
public static UntypedInt PCQuantum => /* _PCQuantum */ 1;

// Int64Align is the required alignment for a 64-bit integer (4 on 32-bit systems, 8 on 64-bit).
public static UntypedInt Int64Align => /* PtrSize */ 8;

// MinFrameSize is the size of the system-reserved words at the bottom
// of a frame (just above the architectural stack pointer).
// It is zero on x86 and PtrSize on most non-x86 (LR-based) systems.
// On PowerPC it is larger, to cover three more reserved words:
// the compiler word, the link editor word, and the TOC save word.
public static UntypedInt MinFrameSize => /* _MinFrameSize */ 0;

// StackAlign is the required alignment of the SP register.
// The stack must be at least word aligned, but some architectures require more.
public static UntypedInt StackAlign => /* _StackAlign */ 8;

} // end goarch_package
