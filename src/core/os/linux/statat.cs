// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go;

using testlog = @internal.testlog_package;
using @internal;
using static os_package;

partial class os_package {

} partial class os_package {

internal static (FileInfo, error) lstatatᴍ(ж<File> Ꮡf, @string name) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (stathook != nil) {
        var (fi, err) = stathook(Ꮡf, name);
        if (fi != nil || err != nil) {
            return (fi, err);
        }
    }
    var log = testlog.Logger(); if (log != nil) {
        log.Stat(joinPath(f.Name(), name));
    }
    return Ꮡf.lstatatNolog(name);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class osᴛFile {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (FileInfo, error) lstatat(this ж<File> Ꮡf, @string name) => os_package.lstatatᴍ(Ꮡf, name);
}
// </GlyphForwarders>
