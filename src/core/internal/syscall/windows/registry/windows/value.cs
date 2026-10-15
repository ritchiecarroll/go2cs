// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows

// Hand-owned: SetDWordValue/SetQWordValue are the "caller-reinterpreted byte buffer" class from
// CLAUDE.md's syscall-wrapper census -- the CALLER (not a syscall wrapper) reinterprets a
// scalar's address as a byte array via `(*[N]byte)(unsafe.Pointer(&value))[:]`, which the
// auto-conversion renders as
// `(~(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(Ꮡvalue)))[..]` -- casting a raw native address
// straight to `ж<array<byte>>` and dereferencing it. `array<byte>` is a managed wrapper (a backing
// reference + length), not 4/8 raw bytes, so that dereference reads the value's raw bytes AS IF
// they were an object reference and length, then Slice()s through the bogus "reference" --
// `invalid memory address or nil pointer dereference` in slice.cs, measured via TestValues.
// Unlike the read-direction CryptoAPI members of this class (still open, no wrapper at fault),
// this is write-only data handed to regSetValueEx, so no identity needs to survive the call --
// building the byte representation explicitly and handing that over is the whole fix, no mirror
// or native call needed. Rest of this file is otherwise the ordinary conversion.
[module: go.GoManualConversion]

namespace go.@internal.syscall.windows;

using errors = errors_package;
using syscall = syscall_package;
using utf16 = unicode.utf16_package;
using @unsafe = unsafe_package;
using unicode;

partial class registry_package {

public static UntypedInt NONE => 0;
public static UntypedInt SZ => 1;
public static UntypedInt EXPAND_SZ => 2;
public static UntypedInt BINARY => 3;
public static UntypedInt DWORD => 4;
public static UntypedInt DWORD_BIG_ENDIAN => 5;
public static UntypedInt LINK => 6;
public static UntypedInt MULTI_SZ => 7;
public static UntypedInt RESOURCE_LIST => 8;
public static UntypedInt FULL_RESOURCE_DESCRIPTOR => 9;
public static UntypedInt RESOURCE_REQUIREMENTS_LIST => 10;
public static UntypedInt QWORD => 11;

public static syscall.Errno ErrShortBuffer = syscall.ERROR_MORE_DATA;
public static syscall.Errno ErrNotExist = syscall.ERROR_FILE_NOT_FOUND;
public static error ErrUnexpectedType = errors.New("unexpected key value type"u8);

// GetValue retrieves the type and data for the specified value associated
// with an open key k. It fills up buffer buf and returns the retrieved
// byte count n. If buf is too small to fit the stored value it returns
// ErrShortBuffer error along with the required buffer size n.
// If no buffer is provided, it returns true and actual buffer size n.
// If no buffer is provided, GetValue returns the value's type only.
// If the value does not exist, the error returned is ErrNotExist.
//
// GetValue is a low level function. If value's type is known, use the appropriate
// Get*Value function instead.
public static (nint n, uint32 valtype, error err) GetValue(this Key k, @string name, slice<byte> buf) {
    ref var valtype = ref heap(new uint32(), out var Ꮡvaltype);
    error err = default!;

    (var pname, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        return (0, 0, err);
    }
    ж<byte> pbuf = default!;
    if (len(buf) > 0) {
        pbuf = Ꮡ(buf, 0);
    }
    ref var l = ref heap<uint32>(out var Ꮡl);
    l = (uint32)len(buf);
    err = syscall.RegQueryValueEx(((syscallꓸHandle)k), pname, nil, Ꮡvaltype, pbuf, Ꮡl);
    if (err != default!) {
        return ((nint)l, valtype, err);
    }
    return ((nint)l, valtype, default!);
}

internal static (slice<byte> date, uint32 valtype, error err) getValue(this Key k, @string name, slice<byte> buf) {
    error err = default!;

    (var p, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        return (default!, 0, err);
    }
    ref var t = ref heap(new uint32(), out var Ꮡt);
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)len(buf);
    while (ᐧ) {
        err = syscall.RegQueryValueEx(((syscallꓸHandle)k), p, nil, Ꮡt, Ꮡ(buf, 0), Ꮡn);
        if (err == default!) {
            return (buf[..(int)(n)], t, default!);
        }
        if (!AreEqual(err, syscall.ERROR_MORE_DATA)) {
            return (default!, 0, err);
        }
        if (n <= (uint32)len(buf)) {
            return (default!, 0, err);
        }
        buf = new slice<byte>((nint)(n));
    }
}

// GetStringValue retrieves the string value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetStringValue returns ErrNotExist.
// If value is not SZ or EXPAND_SZ, it will return the correct value
// type and ErrUnexpectedType.
public static unsafe (@string val, uint32 valtype, error err) GetStringValue(this Key k, @string name) {
    var (data, typ, err2) = k.getValue(name, new slice<byte>(64));
    if (err2 != default!) {
        return ("", typ, err2);
    }
    var exprᴛ1 = typ;
    if (exprᴛ1 == SZ || exprᴛ1 == EXPAND_SZ) {
    }
    else { /* default: */
        return ("", typ, ErrUnexpectedType);
    }

    if (len(data) == 0) {
        return ("", typ, default!);
    }
    var u = new slice<uint16>(new ReadOnlySpan<uint16>((uint16*)(uintptr)(new @unsafe.Pointer(Ꮡ(data, 0))), (int)(len(data) / 2)));
    return (syscall.UTF16ToString(u), typ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string systemRootSystem32ˢ = "%SystemRoot%\\system32\\"u8;

// GetMUIStringValue retrieves the localized string value for
// the specified value name associated with an open key k.
// If the value name doesn't exist or the localized string value
// can't be resolved, GetMUIStringValue returns ErrNotExist.
public static (@string, error) GetMUIStringValue(this Key k, @string name) {
    var (pname, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        return ("", err);
    }
    var buf = new slice<uint16>(1024);
    ref var buflen = ref heap(new uint32(), out var Ꮡbuflen);
    ж<uint16> pdir = default!;
    err = regLoadMUIString(((syscallꓸHandle)k), pname, Ꮡ(buf, 0), (uint32)len(buf), Ꮡbuflen, 0, pdir);
    if (AreEqual(err, syscall.ERROR_FILE_NOT_FOUND)) {
        // Try fallback path
        // Try to resolve the string value using the system directory as
        // a DLL search path; this assumes the string value is of the form
        // @[path]\dllname,-strID but with no path given, e.g. @tzres.dll,-320.
        // This approach works with tzres.dll but may have to be revised
        // in the future to allow callers to provide custom search paths.
        @string s = default!;
        (s, err) = ExpandString(systemRootSystem32ˢ);
        if (err != default!) {
            return ("", err);
        }
        (pdir, err) = syscall.UTF16PtrFromString(s);
        if (err != default!) {
            return ("", err);
        }
        err = regLoadMUIString(((syscallꓸHandle)k), pname, Ꮡ(buf, 0), (uint32)len(buf), Ꮡbuflen, 0, pdir);
    }
    while (AreEqual(err, syscall.ERROR_MORE_DATA)) {
        // Grow buffer if needed
        if (buflen <= (uint32)len(buf)) {
            break; // Buffer not growing, assume race; break
        }
        buf = new slice<uint16>((nint)(buflen));
        err = regLoadMUIString(((syscallꓸHandle)k), pname, Ꮡ(buf, 0), (uint32)len(buf), Ꮡbuflen, 0, pdir);
    }
    if (err != default!) {
        return ("", err);
    }
    return (syscall.UTF16ToString(buf), default!);
}

// ExpandString expands environment-variable strings and replaces
// them with the values defined for the current user.
// Use ExpandString to expand EXPAND_SZ strings.
public static (@string, error) ExpandString(@string value) {
    if (value == ""u8) {
        return ("", default!);
    }
    var (p, err) = syscall.UTF16PtrFromString(value);
    if (err != default!) {
        return ("", err);
    }
    var r = new slice<uint16>(100);
    while (ᐧ) {
        var (n, errΔ1) = expandEnvironmentStrings(p, Ꮡ(r, 0), (uint32)len(r));
        if (errΔ1 != default!) {
            return ("", errΔ1);
        }
        if (n <= (uint32)len(r)) {
            return (syscall.UTF16ToString(r[..(int)(n)]), default!);
        }
        r = new slice<uint16>((nint)(n));
    }
}

// GetStringsValue retrieves the []string value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetStringsValue returns ErrNotExist.
// If value is not MULTI_SZ, it will return the correct value
// type and ErrUnexpectedType.
public static unsafe (slice<@string> val, uint32 valtype, error err) GetStringsValue(this Key k, @string name) {
    slice<@string> val = default!;

    var (data, typ, err2) = k.getValue(name, new slice<byte>(64));
    if (err2 != default!) {
        return (default!, typ, err2);
    }
    if (typ != MULTI_SZ) {
        return (default!, typ, ErrUnexpectedType);
    }
    if (len(data) == 0) {
        return (default!, typ, default!);
    }
    var p = new slice<uint16>(new ReadOnlySpan<uint16>((uint16*)(uintptr)(new @unsafe.Pointer(Ꮡ(data, 0))), (int)(len(data) / 2)));
    if (len(p) == 0) {
        return (default!, typ, default!);
    }
    if (p[len(p) - 1] == 0) {
        p = p[..(int)(len(p) - 1)]; // remove terminating null
    }
    val = new slice<@string>(0, 5);
    nint from = 0;
    foreach (var (i, c) in p) {
        if (c == 0) {
            val = append(val, syscall.UTF16ToString(p[(int)(from)..(int)(i)]));
            from = i + 1;
        }
    }
    return (val, typ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dwordValueIsNot4Bytesˢ = "DWORD value is not 4 bytes long"u8;
internal static readonly @string qwordValueIsNot8Bytesˢ = "QWORD value is not 8 bytes long"u8;

// GetIntegerValue retrieves the integer value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetIntegerValue returns ErrNotExist.
// If value is not DWORD or QWORD, it will return the correct value
// type and ErrUnexpectedType.
public static (uint64 val, uint32 valtype, error err) GetIntegerValue(this Key k, @string name) {
    var (data, typ, err2) = k.getValue(name, new slice<byte>(8));
    if (err2 != default!) {
        return (0, typ, err2);
    }
    var exprᴛ1 = typ;
    if (exprᴛ1 == DWORD) {
        if (len(data) != 4) {
            return (0, typ, errors.New(dwordValueIsNot4Bytesˢ));
        }
        return ((uint64)(~Ꮡ(data, 0).Reinterpret<byte, uint32>()), DWORD, default!);
    }
    if (exprᴛ1 == QWORD) {
        if (len(data) != 8) {
            return (0, typ, errors.New(qwordValueIsNot8Bytesˢ));
        }
        return (~Ꮡ(data, 0).Reinterpret<byte, uint64>(), QWORD, default!);
    }
    { /* default: */
        return (0, typ, ErrUnexpectedType);
    }

}

// GetBinaryValue retrieves the binary value for the specified
// value name associated with an open key k. It also returns the value's type.
// If value does not exist, GetBinaryValue returns ErrNotExist.
// If value is not BINARY, it will return the correct value
// type and ErrUnexpectedType.
public static (slice<byte> val, uint32 valtype, error err) GetBinaryValue(this Key k, @string name) {
    var (data, typ, err2) = k.getValue(name, new slice<byte>(64));
    if (err2 != default!) {
        return (default!, typ, err2);
    }
    if (typ != BINARY) {
        return (default!, typ, ErrUnexpectedType);
    }
    return (data, typ, default!);
}

internal static error setValue(this Key k, @string name, uint32 valtype, slice<byte> data) {
    var (p, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        return err;
    }
    if (len(data) == 0) {
        return regSetValueEx(((syscallꓸHandle)k), p, 0, valtype, nil, 0);
    }
    return regSetValueEx(((syscallꓸHandle)k), p, 0, valtype, Ꮡ(data, 0), (uint32)len(data));
}

// SetDWordValue sets the data and type of a name value
// under key k to value and DWORD.
public static error SetDWordValue(this Key k, @string name, uint32 valueʗp) {
    return k.setValue(name, DWORD, new slice<byte>(System.BitConverter.GetBytes(valueʗp)));
}

// SetQWordValue sets the data and type of a name value
// under key k to value and QWORD.
public static error SetQWordValue(this Key k, @string name, uint64 valueʗp) {
    return k.setValue(name, QWORD, new slice<byte>(System.BitConverter.GetBytes(valueʗp)));
}

internal static unsafe error setStringValue(this Key k, @string name, uint32 valtype, @string value) {
    var (v, err) = syscall.UTF16FromString(value);
    if (err != default!) {
        return err;
    }
    var buf = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(new @unsafe.Pointer(Ꮡ(v, 0))), (int)(len(v) * 2)));
    return k.setValue(name, valtype, buf);
}

// SetStringValue sets the data and type of a name value
// under key k to value and SZ. The value must not contain a zero byte.
public static error SetStringValue(this Key k, @string name, @string value) {
    return k.setStringValue(name, SZ, value);
}

// SetExpandStringValue sets the data and type of a name value
// under key k to value and EXPAND_SZ. The value must not contain a zero byte.
public static error SetExpandStringValue(this Key k, @string name, @string value) {
    return k.setStringValue(name, EXPAND_SZ, value);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringCannotHave0Insideˢ = "string cannot have 0 inside"u8;

// SetStringsValue sets the data and type of a name value
// under key k to value and MULTI_SZ. The value strings
// must not contain a zero byte.
public static unsafe error SetStringsValue(this Key k, @string name, slice<@string> value) {
    @string ss = ""u8;
    foreach (var (_, s) in value) {
        for (nint i = 0; i < len(s); i++) {
            if (s[i] == 0) {
                return errors.New(stringCannotHave0Insideˢ);
            }
        }
        ss += s + "\x00"u8;
    }
    var v = utf16.Encode(slice<rune>(ss + "\x00"));
    var buf = new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(new @unsafe.Pointer(Ꮡ(v, 0))), (int)(len(v) * 2)));
    return k.setValue(name, MULTI_SZ, buf);
}

// SetBinaryValue sets the data and type of a name value
// under key k to value and BINARY.
public static error SetBinaryValue(this Key k, @string name, slice<byte> value) {
    return k.setValue(name, BINARY, value);
}

// DeleteValue removes a named value from the key k.
public static error DeleteValue(this Key k, @string name) {
    var (namep, err) = syscall.UTF16PtrFromString(name);
    if (err != default!) {
        return err;
    }
    return regDeleteValue(((syscallꓸHandle)k), namep);
}

// ReadValueNames returns the value names of key k.
public static (slice<@string>, error) ReadValueNames(this Key k) {
    var (ki, err) = k.Stat();
    if (err != default!) {
        return (default!, err);
    }
    var names = new slice<@string>(0, (nint)((~ki).ValueCount));
    var buf = new slice<uint16>((nint)((~ki).MaxValueNameLen + 1)); // extra room for terminating null character
loopItems:
    for (var i = (uint32)0; ᐧ ; i++) {
        ref var l = ref heap<uint32>(out var Ꮡl);
        l = (uint32)len(buf);
        while (ᐧ) {
            var errΔ1 = regEnumValue(((syscallꓸHandle)k), i, Ꮡ(buf, 0), Ꮡl, nil, nil, nil, nil);
            if (errΔ1 == default!) {
                break;
            }
            if (AreEqual(errΔ1, syscall.ERROR_MORE_DATA)) {
                // Double buffer size and try again.
                l = (uint32)(2 * len(buf));
                buf = new slice<uint16>((nint)(l));
                continue;
            }
            if (AreEqual(errΔ1, _ERROR_NO_MORE_ITEMS)) {
                goto break_loopItems;
            }
            return (names, errΔ1);
        }
        names = append(names, syscall.UTF16ToString(buf[..(int)(l)]));
continue_loopItems:;
    }
break_loopItems:;
    return (names, default!);
}

} // end registry_package
