// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package profilerecord holds internal types used to represent profiling
// records with deep stack traces.
//
// TODO: Consider moving this to internal/runtime, see golang.org/issue/65355.
namespace go.@internal;

partial class profilerecord_package {

partial struct StackRecord {
    public slice<uintptr> Stack;
}

partial struct MemProfileRecord {
    public int64 AllocBytes, FreeBytes;
    public int64 AllocObjects, FreeObjects;
    public slice<uintptr> Stack;
}

public static int64 InUseBytes(this ref MemProfileRecord r) {
    return r.AllocBytes - r.FreeBytes;
}

public static int64 InUseObjects(this ref MemProfileRecord r) {
    return r.AllocObjects - r.FreeObjects;
}

partial struct BlockProfileRecord {
    public int64 Count;
    public int64 Cycles;
    public slice<uintptr> Stack;
}

} // end profilerecord_package
