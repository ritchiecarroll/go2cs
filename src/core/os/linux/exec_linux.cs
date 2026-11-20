// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using syscall = syscall_package;
using static os_package;

partial class os_package {

} partial class os_package {

internal static void closeHandleᴍ(ref processHandle ph) {
    syscall.Close((nint)ph.handle);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class osᴛprocessHandle {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static void closeHandle(this ref processHandle ph) => os_package.closeHandleᴍ(ref ph);
}
// </GlyphForwarders>
