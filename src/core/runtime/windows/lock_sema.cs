// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || netbsd || openbsd || plan9 || solaris || windows
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static uintptr locked => 1;

// One-time notifications.
internal static void noteclear(ж<note> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    n.key = 0;
}

// go2cs generated this placeholder — func notewakeup is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func notesleep is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func notetsleep_internal is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string notetsleepNotOnG0ˢ = "notetsleep not on g0"u8;

internal static bool notetsleep(ж<note> n, int64 ns) {
    var gp = getg();
    if (gp != (~(~gp).m).g0) {
        @throw(notetsleepNotOnG0ˢ);
    }
    semacreate(ref ((~gp).m).DerefOrNull());
    return notetsleep_internal(n, ns, nil, 0);
}

// go2cs generated this placeholder — func notetsleepg is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static (ж<g>, bool) beforeIdle(int64 _Δp0, int64 _Δp1) {
    return (default, false);
}

internal static void checkTimeouts() {
}

} // end runtime_package
