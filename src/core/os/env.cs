// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// General environment variables.
namespace go;

using testlog = @internal.testlog_package;
using syscall = syscall_package;
using @internal;

partial class os_package {

// Expand replaces ${var} or $var in the string based on the mapping function.
// For example, [os.ExpandEnv](s) is equivalent to [os.Expand](s, [os.Getenv]).
public static @string Expand(@string s, Func<@string, @string> mapping) {
    var bufᴷ = new stackAppend<byte>();

    slice<byte> buf = default;
    // ${} is all ASCII, so bytes are fine for this operation.
    nint i = 0;
    for (nint j = 0; j < len(s); j++) {
        if (s[j] == '$' && j + 1 < len(s)) {
            if (buf == nil) {
                buf = new slice<byte>(0, 2 * len(s));
            }
            buf = append(buf, s.slice(i, j).ꓸꓸꓸ);
            var (name, w) = getShellName(s.slice(j + 1));
            if (name == ""u8 && w > 0) {
                // Encountered invalid syntax; eat the
                // characters.
            } else if (name == ""u8) {
                // Valid syntax, but $ was not followed by a
                // name. Leave the dollar character untouched.
                buf = append(bufᴷ, buf, s[j]);
            } else {
                buf = append(buf, mapping(name).ꓸꓸꓸ);
            }
            j += w;
            i = j + 1;
        }
    }
    if (buf == nil) {
        return s;
    }
    return ((sstring)buf) + s.slice(i);
}

// ExpandEnv replaces ${var} or $var in the string according to the values
// of the current environment variables. References to undefined
// variables are replaced by the empty string.
public static @string ExpandEnv(@string s) {
    return Expand(s, Getenv);
}

// isShellSpecialVar reports whether the character identifies a special
// shell variable such as $*.
internal static bool isShellSpecialVar(uint8 c) {
    switch (c) {
    case (uint8)'*' or (uint8)'#' or (uint8)'$' or (uint8)'@' or (uint8)'!' or (uint8)'?' or (uint8)'-' or (uint8)'0' or (uint8)'1' or (uint8)'2' or (uint8)'3' or (uint8)'4' or (uint8)'5' or (uint8)'6' or (uint8)'7' or (uint8)'8' or (uint8)'9': {
        return true;
    }}
    return false;
}

// isAlphaNum reports whether the byte is an ASCII letter, number, or underscore.
internal static bool isAlphaNum(uint8 c) {
    return c == '_' || '0' <= c && c <= '9' || 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z';
}

// getShellName returns the name that begins the string and the number of bytes
// consumed to extract it. If the name is enclosed in {}, it's part of a ${}
// expansion and two more bytes are needed than the length of the name.
internal static (@string, nint) getShellName(@string s) {
    switch (ᐧ) {
    case {} when s[0] is (byte)'{': {
        if (len(s) > 2 && isShellSpecialVar(s[1]) && s[2] == '}') {
            return (s[1..2], 3);
        }
        // Scan to closing brace
        for (nint iΔ2 = 1; iΔ2 < len(s); iΔ2++) {
            if (s[iΔ2] == '}') {
                if (iΔ2 == 1) {
                    return ("", 2); // Bad syntax; eat "${}"
                }
                return (s.slice(1, iΔ2), iΔ2 + 1);
            }
        }
        return ("", 1); // Bad syntax; eat "${"
    }
    case {} when isShellSpecialVar(s[0]): {
        return (s[0..1], 1);
    }}
    // Scan alphanumerics.
    nint i = default;
    for (i = 0; i < len(s) && isAlphaNum(s[i]); i++) {
    }
    return (s.slice(0, i), i);
}

// Getenv retrieves the value of the environment variable named by the key.
// It returns the value, which will be empty if the variable is not present.
// To distinguish between an empty value and an unset value, use [LookupEnv].
public static @string Getenv(@string key) {
    testlog.Getenv(key);
    var (v, _) = syscall.Getenv(key);
    return v;
}

// LookupEnv retrieves the value of the environment variable named
// by the key. If the variable is present in the environment the
// value (which may be empty) is returned and the boolean is true.
// Otherwise the returned value will be empty and the boolean will
// be false.
public static (@string, bool) LookupEnv(@string key) {
    testlog.Getenv(key);
    return syscall.Getenv(key);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setenvˢ = "setenv"u8;

// Setenv sets the value of the environment variable named by the key.
// It returns an error, if any.
public static error Setenv(@string key, @string value) {
    var err = syscall.Setenv(key, value);
    if (err != nil) {
        return NewSyscallError(setenvˢ, err);
    }
    return default;
}

// Unsetenv unsets a single environment variable.
public static error Unsetenv(@string key) {
    return syscall.Unsetenv(key);
}

// Clearenv deletes all environment variables.
public static void Clearenv() {
    syscall.Clearenv();
}

// Environ returns a copy of strings representing the environment,
// in the form "key=value".
public static slice<@string> Environ() {
    return syscall.Environ();
}

} // end os_package
