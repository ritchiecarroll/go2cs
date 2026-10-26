// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Export debuglog guts for testing.
global using Dlogger = global::go.runtime_package.dloggerImpl;

namespace go;

using static global::go.runtime_package;

partial class runtime_internal_test_package {

public const bool DlogEnabled = /* dlogEnabled */ false;

public static UntypedInt DebugLogBytes => /* debugLogBytes */ 16384;

public static UntypedInt DebugLogStringLimit => /* debugLogStringLimit */ 2048;

internal static ж<Dlogger> Dlog() {
    return dlogImpl();
}

internal static void End(this ж<global::go.runtime_package.dloggerImpl> Ꮡl) {
    Ꮡl.end();
}

internal static ж<global::go.runtime_package.dloggerImpl> B(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, bool x) {
    return Ꮡl.b(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> I(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, nint x) {
    return Ꮡl.i(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> I16(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, int16 x) {
    return Ꮡl.i16(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> U64(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, uint64 x) {
    return Ꮡl.u64(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> Hex(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, uint64 x) {
    return Ꮡl.hex(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> P(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, any x) {
    return Ꮡl.p(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> S(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, @string x) {
    return Ꮡl.s(x);
}

internal static ж<global::go.runtime_package.dloggerImpl> PC(this ж<global::go.runtime_package.dloggerImpl> Ꮡl, uintptr x) {
    return Ꮡl.pc(x);
}

public static @string DumpDebugLog() {
    var gp = getg();
    gp.Value.writebuf = new slice<byte>(0, (1 << 20));
    printDebugLogImpl();
    var buf = gp.Value.writebuf;
    gp.Value.writebuf = default!;

    return (@string)buf;
}

public static void ResetDebugLog() {
    var stw = stopTheWorld(stwForTestResetDebugLog);
    for (var l = allDloggers; l != nil; l = l.Value.allLink) {
        l.Value.w.write = 0;
        (l.Value.w.tick, l.Value.w.nano) = (0, 0);
        (l.Value.w.r.begin, l.Value.w.r.end) = (0, 0);
        (l.Value.w.r.tick, l.Value.w.r.nano) = (0, 0);
    }
    startTheWorld(stw);
}

public static nint CountDebugLog() {
    var stw = stopTheWorld(stwForTestResetDebugLog);
    nint i = 0;
    for (var l = allDloggers; l != nil; l = l.Value.allLink) {
        i++;
    }
    startTheWorld(stw);
    return i;
}

} // end runtime_internal_test_package
