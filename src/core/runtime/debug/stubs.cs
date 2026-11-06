// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using time = time_package;

partial class debug_package {

// Implemented in package runtime.
internal static partial void readGCStats(ж<slice<time.Duration>> _);
internal static partial void freeOSMemory();
internal static partial nint setMaxStack(nint _);
internal static partial int32 setGCPercent(int32 _);
internal static partial bool setPanicOnFault(bool _);
internal static partial nint setMaxThreads(nint _);
internal static partial int64 setMemoryLimit(int64 _);

} // end debug_package
