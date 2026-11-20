// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using bytes = bytes_package;
using fmt = fmt_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using @unsafe = unsafe_package;
using io = io_package;

partial class libc_package {

internal static UntypedInt modNone => iota;
internal static UntypedInt modHH => 1;
internal static UntypedInt modH => 2;
internal static UntypedInt modL => 3;
internal static UntypedInt modLL => 4;
internal static UntypedInt modLD => 5;
internal static UntypedInt modQ => 6;
internal static UntypedInt modCapitalL => 7;
internal static UntypedInt modJ => 8;
internal static UntypedInt modZ => 9;
internal static UntypedInt modCapitalZ => 10;
internal static UntypedInt modT => 11;
internal static UntypedInt mod32 => 12;
internal static UntypedInt mod64 => 13;

internal static slice<byte> printf(uintptr format, uintptr argsʗp) {
    ref var args = ref heap(argsʗp, out var Ꮡargs);

    var buf = bytes.NewBuffer(default);
    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (c) {
        case (byte)'%': {
            format = printfConversion(buf, format, Ꮡargs);
            break;
        }
        case 0: {
            return buf.Bytes();
        }
        default: {
            format++;
            buf.WriteByte(c);
            break;
        }}

    }
}

internal static uintptr printfConversion(ж<bytes.Buffer> Ꮡbuf, uintptr format, ж<uintptr> args) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    format++;
    @string spec = "%";

flags:
    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (c) {
        case (byte)'#': {
            format++;
            spec += "#"u8;
            break;
        }
        case (byte)'0': {
            format++;
            spec += "0"u8;
            break;
        }
        case (byte)'-': {
            format++;
            spec += "-"u8;
            break;
        }
        case (byte)' ': {
            format++;
            spec += " "u8;
            break;
        }
        case (byte)'+': {
            format++;
            spec += "+"u8;
            break;
        }
        default: {
            goto break_flags;
            break;
        }}

continue_flags:;
    }
break_flags:;
    (format, var width, var hasWidth) = parseFieldWidth(format, args);
    if (hasWidth) {
        spec += strconv.Itoa(width);
    }
    (format, var prec, var hasPrecision) = parsePrecision(format, args);
    (format, var mod) = parseLengthModifier(format);

    @string str = default;

more:
    {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        var cᴛ1 = c;
        var matchᴛ1 = false;
        if (cᴛ1 is (byte)'d' or (byte)'i') { matchᴛ1 = true;
            do {
                format++;
                int64 arg = default;
                if (isWindows && mod == modL) {
                    mod = modNone;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modL || ᴛ == modLL || ᴛ == mod64 || ᴛ == modJ: {
                    arg = VaInt64(args);
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    arg = (int64)(int16)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    arg = (int64)(int8)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == mod32 || ᴛ == modNone: {
                    arg = (int64)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modT: {
                    arg = (int64)VaInt64(args);
                    break;
                }
                default: {
                    throw panic(todo("", mod));
                    break;
                }}

                if (arg == 0 && hasPrecision && prec == 0) {
                    break;
                }

                if (hasPrecision) {
                    throw panic(todo("", prec));
                }

                @string f = spec + "d"u8;
                str = fmt.Sprintf(f, arg);
            } while (false);
        }
        else if (cᴛ1 is (byte)'u') { matchᴛ1 = true;
            do {
                format++;
                uint64 arg = default;
                if (isWindows && mod == modL) {
                    mod = modNone;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    arg = (uint64)VaUint32(args);
                    break;
                }
                case var ᴛ when ᴛ == modL || ᴛ == modLL || ᴛ == mod64: {
                    arg = VaUint64(args);
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    arg = (uint64)(uint16)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    arg = (uint64)(uint8)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == mod32: {
                    arg = (uint64)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modZ: {
                    arg = (uint64)VaInt64(args);
                    break;
                }
                default: {
                    throw panic(todo("", mod));
                    break;
                }}

                if (arg == 0 && hasPrecision && prec == 0) {
                    break;
                }

                if (hasPrecision) {
                    throw panic(todo("", prec));
                }

                @string f = spec + "d"u8;
                str = fmt.Sprintf(f, arg);
            } while (false);
        }
        else if (cᴛ1 is (byte)'o') { matchᴛ1 = true;
            do {
                format++;
                uint64 arg = default;
                if (isWindows && mod == modL) {
                    mod = modNone;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    arg = (uint64)VaUint32(args);
                    break;
                }
                case var ᴛ when ᴛ == modL || ᴛ == modLL || ᴛ == mod64: {
                    arg = VaUint64(args);
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    arg = (uint64)(uint16)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    arg = (uint64)(uint8)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == mod32: {
                    arg = (uint64)VaInt32(args);
                    break;
                }
                default: {
                    throw panic(todo("", mod));
                    break;
                }}

                if (arg == 0 && hasPrecision && prec == 0) {
                    break;
                }

                if (hasPrecision) {
                    throw panic(todo("", prec));
                }

                @string f = spec + "o"u8;
                str = fmt.Sprintf(f, arg);
            } while (false);
        }
        else if (cᴛ1 is (byte)'b') { matchᴛ1 = true;
            do {
                format++;
                uint64 arg = default;
                if (isWindows && mod == modL) {
                    mod = modNone;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    arg = (uint64)VaUint32(args);
                    break;
                }
                case var ᴛ when ᴛ == modL || ᴛ == modLL || ᴛ == mod64: {
                    arg = VaUint64(args);
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    arg = (uint64)(uint16)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    arg = (uint64)(uint8)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == mod32: {
                    arg = (uint64)VaInt32(args);
                    break;
                }
                default: {
                    throw panic(todo("", mod));
                    break;
                }}

                if (arg == 0 && hasPrecision && prec == 0) {
                    break;
                }

                if (hasPrecision) {
                    throw panic(todo("", prec));
                }

                @string f = spec + "b"u8;
                str = fmt.Sprintf(f, arg);
            } while (false);
        }
        else if (cᴛ1 is (byte)'I') { matchᴛ1 = true;
            if (!isWindows) {
                throw panic(todo("%#U", c));
            }

            format++;
            c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
            switch (c) {
            case (byte)'x' or (byte)'X': {
                if (/* unsafe.Sizeof(int(0)) */ (uintptr)8 == 4) {
                    mod = mod32;
                }
                break;
            }
            case (byte)'3': {
                format++;
                c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
                switch (c) {
                case (byte)'2': {
                    format++;
                    mod = mod32;
                    goto more;
                    break;
                }
                default: {
                    throw panic(todo("%#U", c));
                    break;
                }}

                break;
            }
            case (byte)'6': {
                format++;
                c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
                switch (c) {
                case (byte)'4': {
                    format++;
                    mod = mod64;
                    goto more;
                    break;
                }
                default: {
                    throw panic(todo("%#U", c));
                    break;
                }}

                break;
            }
            default: {
                throw panic(todo("%#U", c));
                break;
            }}

            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && cᴛ1 is (byte)'X') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && cᴛ1 is (byte)'x') { matchᴛ1 = true;
            do {
                format++;
                uint64 arg = default;
                if (isWindows && mod == modL) {
                    mod = modNone;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    arg = (uint64)VaUint32(args);
                    break;
                }
                case var ᴛ when ᴛ == modL || ᴛ == modLL || ᴛ == mod64: {
                    arg = VaUint64(args);
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    arg = (uint64)(uint16)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    arg = (uint64)(uint8)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == mod32: {
                    arg = (uint64)VaInt32(args);
                    break;
                }
                case var ᴛ when ᴛ == modZ: {
                    arg = (uint64)VaInt64(args);
                    break;
                }
                default: {
                    throw panic(todo("", mod));
                    break;
                }}

                if (arg == 0 && hasPrecision && prec == 0) {
                    break;
                }

                if (strings.Contains(spec, "#") && arg == 0) {
                    spec = strings.ReplaceAll(spec, "#", "");
                }
                @string f = default;
                switch (ᐧ) {
                case {} when hasPrecision: {
                    f = fmt.Sprintf("%s.%d%c"u8, spec, prec, c);
                    break;
                }
                default: {
                    f = spec + ((@string)c);
                    break;
                }}

                str = fmt.Sprintf(f, arg);
            } while (false);
        }
        else if (cᴛ1 is (byte)'e' or (byte)'E') { matchᴛ1 = true;
            format++;
            var arg = VaFloat64(args);
            if (!hasPrecision) {
                prec = 6;
            }
            @string f = fmt.Sprintf("%s.%d%c"u8, spec, prec, c);
            str = fmt.Sprintf(f, arg);
        }
        else if (cᴛ1 is (byte)'f' or (byte)'F') { matchᴛ1 = true;
            format++;
            var arg = VaFloat64(args);
            if (!hasPrecision) {
                prec = 6;
            }
            @string f = fmt.Sprintf("%s.%d%c"u8, spec, prec, c);
            str = fixNanInf(fmt.Sprintf(f, arg));
        }
        else if (cᴛ1 is (byte)'G') { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && cᴛ1 is (byte)'g') { matchᴛ1 = true;
            format++;
            var arg = VaFloat64(args);
            if (!hasPrecision) {
                prec = 6;
            }
            if (prec == 0) {
                prec = 1;
            }

            @string f = fmt.Sprintf("%s.%d%c"u8, spec, prec, c);
            str = fixNanInf(fmt.Sprintf(f, arg));
        }
        else if (cᴛ1 is (byte)'s') { matchᴛ1 = true;
            format++;
            var arg = VaUintptr(args);
            switch (mod) {
            case var ᴛ when ᴛ == modNone: {
                @string f = default;
                switch (ᐧ) {
                case {} when hasPrecision: {
                    f = fmt.Sprintf("%s.%ds"u8, spec, prec);
                    str = fmt.Sprintf(f, GoString(arg));
                    break;
                }
                default: {
                    f = spec + "s"u8;
                    str = fmt.Sprintf(f, GoString(arg));
                    break;
                }}

                break;
            }
            default: {
                throw panic(todo(""));
                break;
            }}

        }
        else if (cᴛ1 is (byte)'p') { matchᴛ1 = true;
            format++;
            var exprᴛ8 = runtime.GOOS;
            switch (ᐧ) {
            case {} when exprᴛ8 == "windows"u8: {
                var exprᴛ9 = runtime.GOARCH;
                switch (ᐧ) {
                case {} when exprᴛ9 == "386"u8 || exprᴛ9 == "arm"u8: {
                    fmt.Fprintf(new bytes_BufferжWriter(Ꮡbuf), "%08X"u8, VaUintptr(args));
                    break;
                }
                default: {
                    fmt.Fprintf(new bytes_BufferжWriter(Ꮡbuf), "%016X"u8, VaUintptr(args));
                    break;
                }}

                break;
            }
            case {} when exprᴛ8 == "illumos"u8: {
                fmt.Fprintf(new bytes_BufferжWriter(Ꮡbuf), "%x"u8, VaUintptr(args));
                break;
            }
            default: {
                fmt.Fprintf(new bytes_BufferжWriter(Ꮡbuf), "%#0x"u8, VaUintptr(args));
                break;
            }}

        }
        else if (cᴛ1 is (byte)'c') { matchᴛ1 = true;
            format++;
            switch (mod) {
            case var ᴛ when ᴛ == modNone: {
                var arg = VaInt32(args);
                buf.WriteByte((byte)arg);
                break;
            }
            default: {
                throw panic(todo(""));
                break;
            }}

        }
        else if (cᴛ1 is (byte)'%') {
            format++;
            buf.WriteByte((byte)'%');
        }
        else if (!matchᴛ1) { /* default: */
            throw panic(todo("%#U", c));
        }
    }

    break_more:;

    buf.WriteString(str);
    return format;
}

internal static (uintptr, nint n, bool ok) parseFieldWidth(uintptr format, ж<uintptr> args) {
    nint n = default;
    bool ok = default;

    var first = true;
    for (;;) {
        nint digit = default;
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (ᐧ) {
        case {} when first && c == (byte)'0': {
            return (format, n, ok);
        }
        case {} when first && c == (byte)'*': {
            format++;
            var cΔ2 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
            switch (ᐧ) {
            case {} when cΔ2 >= (byte)'0' && cΔ2 <= (byte)'9': {
                throw panic(todo(""));
                break;
            }
            default: {
                return (format, (nint)VaInt32(args), true);
            }}

            break;
        }
        case {} when c >= (byte)'0' && c <= (byte)'9': {
            format++;
            ok = true;
            first = false;
            digit = (nint)c - '0';
            break;
        }
        default: {
            return (format, n, ok);
        }}

        nint n0 = n;
        n = 10 * n + digit;
        if (n < n0) {
            throw panic(todo(""));
        }
    }
}

internal static (uintptr, nint n, bool ok) parsePrecision(uintptr format, ж<uintptr> args) {
    nint n = default;

    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (c) {
        case (byte)'.': {
            format++;
            var first = true;
            for (;;) {
                var cΔ2 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
                switch (ᐧ) {
                case {} when first && cΔ2 == (byte)'*': {
                    format++;
                    n = (nint)VaInt32(args);
                    return (format, n, true);
                }
                case {} when cΔ2 >= (byte)'0' && cΔ2 <= (byte)'9': {
                    format++;
                    first = false;
                    nint n0 = n;
                    n = 10 * n + ((nint)cΔ2 - '0');
                    if (n < n0) {
                        throw panic(todo(""));
                    }
                    break;
                }
                default: {
                    return (format, n, true);
                }}

            }
            break;
        }
        default: {
            return (format, 0, false);
        }}

    }
}

internal static (uintptr, nint n) parseLengthModifier(uintptr format) {
    nint n = default;

    var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
    switch (c) {
    case (byte)'h': {
        format++;
        n = modH;
        var cΔ3 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (cΔ3) {
        case (byte)'h': {
            format++;
            n = modHH;
            break;
        }}

        return (format, n);
    }
    case (byte)'l': {
        format++;
        n = modL;
        var cΔ4 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (cΔ4) {
        case (byte)'l': {
            format++;
            n = modLL;
            break;
        }}

        return (format, n);
    }
    case (byte)'q': {
        throw panic(todo(""));
        break;
    }
    case (byte)'L': {
        format++;
        n = modLD;
        return (format, n);
    }
    case (byte)'j': {
        format++;
        n = modJ;
        return (format, n);
    }
    case (byte)'z': {
        format++;
        return (format, modZ);
    }
    case (byte)'Z': {
        format++;
        return (format, modCapitalZ);
    }
    case (byte)'t': {
        format++;
        return (format, modT);
    }
    default: {
        return (format, 0);
    }}

}

internal static @string fixNanInf(@string s) {
    switch (ᐧ) {
    case {} when s == "NaN"u8: {
        return "nan";
    }
    case {} when s == "+Inf"u8 || s == "-Inf"u8: {
        return "inf";
    }
    default: {
        return s;
    }}

}

} // end libc_package
