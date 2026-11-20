// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package profilerecord holds internal types used to represent profiling
// records with deep stack traces.
//
// TODO: Consider moving this to internal/runtime, see golang.org/issue/65355.
namespace go.@internal;

using static profilerecord_package;

partial class profilerecord_package {

partial struct StackRecord {
    public slice<uintptr> Stack;
}

partial struct MemProfileRecord {
    public int64 AllocBytes, FreeBytes;
    public int64 AllocObjects, FreeObjects;
    public slice<uintptr> Stack;
}

} partial class profilerecord_package {

internal static int64 InUseBytesᴍ(ref MemProfileRecord r) { return r.AllocBytes - r.FreeBytes; }
internal static int64 InUseObjectsᴍ(ref MemProfileRecord r) { return r.AllocObjects - r.FreeObjects; }

} partial class profilerecord_package {

partial struct BlockProfileRecord {
    public int64 Count;
    public int64 Cycles;
    public slice<uintptr> Stack;
}

} // end profilerecord_package

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class profilerecordᴛMemProfileRecord {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 InUseBytes(this ref MemProfileRecord r) => profilerecord_package.InUseBytesᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 InUseObjects(this ref MemProfileRecord r) => profilerecord_package.InUseObjectsᴍ(ref r);
}
// </GlyphForwarders>
