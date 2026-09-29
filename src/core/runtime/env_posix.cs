// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getenvBeforeEnvInitˢ = "getenv before env init"u8;

internal static @string gogetenv(@string key) {
    var env = environ();
    if (env == default!) {
        @throw(getenvBeforeEnvInitˢ);
    }
    foreach (var (_, s) in env) {
        if (len(s) > len(key) && s[len(key)] == (rune)'=' && envKeyEqual(s.slice(0, len(key)), key)) {
            return s.slice(len(key) + 1);
        }
    }
    return ""u8;
}

// envKeyEqual reports whether a == b, with ASCII-only case insensitivity
// on Windows. The two strings must have the same length.
internal static bool envKeyEqual(@string a, @string b) {
    if (GOOS == "windows"u8) {
        // case insensitive
        for (nint i = 0; i < len(a); i++) {
            var (ca, cb) = (a[i], b[i]);
            if (ca == cb || lowerASCII(ca) == lowerASCII(cb)) {
                continue;
            }
            return false;
        }
        return true;
    }
    return a == b;
}

internal static byte lowerASCII(byte c) {
    if ((rune)'A' <= c && c <= (rune)'Z') {
        return (byte)(c + ((rune)'a' - (rune)'A'));
    }
    return c;
}

// _cgo_setenv should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ebitengine/purego
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname _cgo_setenv
public static @unsafe.Pointer _cgo_setenv;       // pointer to C function

// _cgo_unsetenv should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/ebitengine/purego
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname _cgo_unsetenv
public static @unsafe.Pointer _cgo_unsetenv;     // pointer to C function

// Update the C environment if cgo is loaded.
internal static void setenv_c(@string k, @string v) {
    if (_cgo_setenv == nil) {
        return;
    }
    ref var arg = ref heap<array<@unsafe.Pointer>>(out var Ꮡarg);
    arg = new @unsafe.Pointer[]{(uintptr)cstring(k), (uintptr)cstring(v)}.array();
    asmcgocall(_cgo_setenv, @unsafe.Pointer.FromPinnedBox(Ꮡarg));
}

// Update the C environment if cgo is loaded.
internal static void unsetenv_c(@string k) {
    if (_cgo_unsetenv == nil) {
        return;
    }
    ref var arg = ref heap<array<@unsafe.Pointer>>(out var Ꮡarg);
    arg = new @unsafe.Pointer[]{(uintptr)cstring(k)}.array();
    asmcgocall(_cgo_unsetenv, @unsafe.Pointer.FromPinnedBox(Ꮡarg));
}

internal static @unsafe.Pointer cstring(@string s) {
    var Δp = new slice<byte>(len(s) + 1);
    copy(Δp, s);
    return @unsafe.Pointer.FromPinnedBox(Ꮡ(Δp, 0));
}

} // end runtime_package
