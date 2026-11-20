// Copyright 2020 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using @unsafe = unsafe_package;

partial class libc_package {

internal static int32 /*nvalues*/ scanf(io.ByteScanner r, uintptr format, uintptr argsʗp) {
    int32 nvalues = default;
    ref var args = ref heap(argsʗp, out var Ꮡargs);

    bool ok = default;
@out:
    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (c) {
        case (byte)'%': {
            nint n = default;
            bool match = default;
            (format, n, match) = scanfConversion(r, format, Ꮡargs);
            if (!match) {
                goto break_out;
            }

            nvalues += (int32)n;
            ok = true;
            break;
        }
        case 0: {
            goto break_out;
            break;
        }
        case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
            format = skipWhiteSpace(format);
            ok = true;
next:
            for (;;) {
                var (cΔ2, err) = r.ReadByte();
                if (err != nil) {
                    goto break_out;
                }

                switch (cΔ2) {
                case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
                    break;
                }
                default: {
                    r.UnreadByte();
                    goto break_next;
                    break;
                }}

continue_next:;
            }
break_next:;
            break;
        }
        default: {
            var (c2, err) = r.ReadByte();
            if (err != nil) {
                goto break_out;
            }

            if (c2 != c) {
                r.UnreadByte();
                goto break_out;
            }

            format++;
            ok = true;
            break;
        }}

continue_out:;
    }
break_out:;
    if (ok) {
        return nvalues;
    }

    return -1;
}

internal static (uintptr, nint nvalues, bool match) scanfConversion(io.ByteScanner r, uintptr format, ж<uintptr> args) {
    nint nvalues = default;
    bool match = default;
    var bᴷ = new stackAppend<byte>();
    var re0ᴷ = new stackAppend<byte>();

    format++;

    nint mod = 0;
    nint width = -1;
    var discard = false;
flags:
    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (c) {
        case (byte)'*': {
            format++;
            discard = true;
            break;
        }
        case (byte)'\'': {
            format++;
            throw panic(todo(""));
            break;
        }
        case (byte)'0' or (byte)'1' or (byte)'2' or (byte)'3' or (byte)'4' or (byte)'5' or (byte)'6' or (byte)'7' or (byte)'8' or (byte)'9': {
            width = 0;
num:
            for (;;) {
                nint digit = default;
                var cΔ2 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
                switch (ᐧ) {
                default: {
                    goto break_num;
                    break;
                }
                case {} when cΔ2 >= (byte)'0' && cΔ2 <= (byte)'9': {
                    format++;
                    digit = (nint)cΔ2 - '0';
                    break;
                }}

                nint width0 = width;
                width = 10 * width + digit;
                if (width < width0) {
                    throw panic(todo(""));
                }
continue_num:;
            }
break_num:;
            break;
        }
        case (byte)'h' or (byte)'j' or (byte)'l' or (byte)'L' or (byte)'q' or (byte)'t' or (byte)'z': {
            (format, mod) = parseLengthModifier(format);
            break;
        }
        default: {
            goto break_flags;
            break;
        }}

continue_flags:;
    }
break_flags:;

    {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
        switch (c) {
        case (byte)'%': {
            format++;
            skipReaderWhiteSpace(r);
            var (cΔ11, errΔ4) = r.ReadByte();
            if (errΔ4 != nil) {
                return (format, -1, false);
            }

            if (cΔ11 == '%') {
                return (format, 1, true);
            }

            r.UnreadByte();
            return (format, 0, false);
        }
        case (byte)'d': {
            format++;
            skipReaderWhiteSpace(r);
            uint64 digit = default;
            uint64 n = default;
            var allowSign = true;
            var neg = false;
dec:
            for (; width != 0; width--) {
                var (cΔ12, errΔ5) = r.ReadByte();
                if (errΔ5 != nil) {
                    if (match) {
                        goto break_dec;
                    }

                    return (0, 0, false);
                }

                if (allowSign) {
                    switch (cΔ12) {
                    case (byte)'-': {
                        allowSign = false;
                        neg = true;
                        continue;
                        break;
                    }
                    case (byte)'+': {
                        allowSign = false;
                        continue;
                        break;
                    }}

                }

                switch (ᐧ) {
                case {} when cΔ12 >= (byte)'0' && cΔ12 <= (byte)'9': {
                    digit = (uint64)cΔ12 - '0';
                    break;
                }
                default: {
                    r.UnreadByte();
                    goto break_dec;
                    break;
                }}

                match = true;
                var n0 = n;
                n = n * 10 + digit;
                if (n < n0) {
                    throw panic(todo(""));
                }
continue_dec:;
            }
break_dec:;
            if (!match) {
                break;
            }

            if (!discard) {
                var arg = VaUintptr(args);
                var v = (int64)n;
                if (neg) {
                    v = -v;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    (@unsafe.Pointer.To<int32>((@unsafe.Pointer)arg)).Value = (int32)v;
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    (@unsafe.Pointer.To<int16>((@unsafe.Pointer)arg)).Value = (int16)v;
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    (@unsafe.Pointer.To<int8>((@unsafe.Pointer)arg)).Value = (int8)v;
                    break;
                }
                case var ᴛ when ᴛ == modL: {
                    (@unsafe.Pointer.To<@long>((@unsafe.Pointer)arg)).Value = (@long)v;
                    break;
                }
                case var ᴛ when ᴛ == modLL: {
                    (@unsafe.Pointer.To<int64>((@unsafe.Pointer)arg)).Value = (int64)v;
                    break;
                }
                default: {
                    throw panic(todo("", mod));
                    break;
                }}

            }
            nvalues = 1;
            break;
        }
        case (byte)'D': {
            format++;
            throw panic(todo(""));
            break;
        }
        case (byte)'i': {
            format++;
            throw panic(todo(""));
            break;
        }
        case (byte)'o': {
            format++;
            throw panic(todo(""));
            break;
        }
        case (byte)'u': {
            format++;
            throw panic(todo(""));
            break;
        }
        case (byte)'x' or (byte)'X': {
            format++;
            skipReaderWhiteSpace(r);
            uint64 digit = default;
            uint64 n = default;
            var allowPrefix = true;
            slice<byte> b = default;
hex:
            for (; width != 0; width--) {
                var (cΔ13, errΔ6) = r.ReadByte();
                if (errΔ6 != nil) {
                    if (match || AreEqual(errΔ6, io.EOF)) {
                        goto break_hex;
                    }

                    throw panic(todo("", errΔ6));
                }

                if (allowPrefix) {
                    if (len(b) == 1 && b[0] == '0' && (cΔ13 == 'x' || cΔ13 == 'X')) {
                        allowPrefix = false;
                        match = false;
                        b = default;
                        continue;
                    }

                    b = append(bᴷ, b, cΔ13);
                }

                switch (ᐧ) {
                case {} when cΔ13 >= (byte)'0' && cΔ13 <= (byte)'9': {
                    digit = (uint64)cΔ13 - '0';
                    break;
                }
                case {} when cΔ13 >= (byte)'a' && cΔ13 <= (byte)'f': {
                    digit = (uint64)cΔ13 - 'a' + 10;
                    break;
                }
                case {} when cΔ13 >= (byte)'A' && cΔ13 <= (byte)'F': {
                    digit = (uint64)cΔ13 - 'A' + 10;
                    break;
                }
                default: {
                    r.UnreadByte();
                    goto break_hex;
                    break;
                }}

                match = true;
                var n0 = n;
                n = (n << 4) + digit;
                if (n < n0) {
                    throw panic(todo(""));
                }
continue_hex:;
            }
break_hex:;
            if (!match) {
                break;
            }

            if (!discard) {
                var arg = VaUintptr(args);
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    (@unsafe.Pointer.To<uint32>((@unsafe.Pointer)arg)).Value = (uint32)n;
                    break;
                }
                case var ᴛ when ᴛ == modH: {
                    (@unsafe.Pointer.To<uint16>((@unsafe.Pointer)arg)).Value = (uint16)n;
                    break;
                }
                case var ᴛ when ᴛ == modHH: {
                    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)arg)).Value = (byte)n;
                    break;
                }
                case var ᴛ when ᴛ == modL: {
                    (@unsafe.Pointer.To<@ulong>((@unsafe.Pointer)arg)).Value = (@ulong)n;
                    break;
                }
                default: {
                    throw panic(todo(""));
                    break;
                }}

            }
            nvalues = 1;
            break;
        }
        case (byte)'f' or (byte)'e' or (byte)'g' or (byte)'E' or (byte)'a': {
            format++;
            skipReaderWhiteSpace(r);
            var seq = fpLiteral(r);
            if (len(seq) == 0) {
                return (0, 0, false);
            }

            bool neg = default;
            switch (seq[0]) {
            case (byte)'+': {
                seq = seq[1..];
                break;
            }
            case (byte)'-': {
                neg = true;
                seq = seq[1..];
                break;
            }}

            var (n, errΔ7) = strconv.ParseFloat((@string)seq, 64);
            if (errΔ7 != nil) {
                throw panic(todo("", errΔ7));
            }

            if (!discard) {
                var arg = VaUintptr(args);
                if (neg) {
                    n = -n;
                }
                switch (mod) {
                case var ᴛ when ᴛ == modNone: {
                    (@unsafe.Pointer.To<float32>((@unsafe.Pointer)arg)).Value = (float32)n;
                    break;
                }
                case var ᴛ when ᴛ == modL: {
                    (@unsafe.Pointer.To<float64>((@unsafe.Pointer)arg)).Value = n;
                    break;
                }
                default: {
                    throw panic(todo("", mod, neg, n));
                    break;
                }}

            }
            return (format, 1, true);
        }
        case (byte)'s': {
            byte cΔ14 = default;
            error errΔ8 = default;
            uintptr arg = default;
            if (!discard) {
                arg = VaUintptr(args);
            }
scans:
            for (; width != 0; width--) {
                (cΔ14, errΔ8) = r.ReadByte(); if (errΔ8 != nil) {
                    if (!AreEqual(errΔ8, io.EOF)) {
                        nvalues = -1;
                    }
                    goto break_scans;
                }

                switch (cΔ14) {
                case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
                    goto break_scans;
                    break;
                }}

                nvalues = 1;
                match = true;
                if (!discard) {
                    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)arg)).Value = cΔ14;
                    arg++;
                }
continue_scans:;
            }
break_scans:;
            if (match) {
                var matchᴛ1 = false;
                if (width is 0) { matchᴛ1 = true;
                    r.UnreadByte();
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ1) { /* default: */
                    if (!discard) {
                        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)arg)).Value = 0;
                    }
                }

            }
            break;
        }
        case (byte)'c': {
            format++;
            if (width < 0) {
                width = 1;
            }
            uintptr arg = default;
            if (!discard) {
                arg = VaUintptr(args);
            }
            for (; width != 0; width--) {
                var (cΔ15, err) = r.ReadByte();
                if (err != nil) {
                    if (!AreEqual(err, io.EOF)) {
                        nvalues = -1;
                    }
                    return (format, nvalues, match);
                }

                match = true;
                nvalues = 1;
                if (!discard) {
                    (@unsafe.Pointer.To<byte>((@unsafe.Pointer)arg)).Value = cΔ15;
                    arg++;
                }
            }
            if (discard) {
                nvalues = 0;
            }
            return (format, nvalues, match);
        }
        case (byte)'[': {
            format++;
            slice<byte> re0 = default;
bracket:
            for (nint i = 0; ; i++) {
                var cΔ16 = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)format);
                format++;
                if (cΔ16 == ']' && i != 0) {
                    goto break_bracket;
                }

                re0 = append(re0ᴷ, re0, cΔ16);
continue_bracket:;
            }
break_bracket:;
            var set = new map<byte, EmptyStruct>{};
            @string re = (@string)re0;
            var neg = strings.HasPrefix(re, "^");
            if (neg) {
                re = re[1..];
            }
            while (len(re) != 0) {
                switch (ᐧ) {
                case {} when len(re) >= 3 && re[1] == (byte)'-': {
                    for (var cΔ18 = re[0]; cΔ18 <= re[2]; cΔ18++) {
                        set[cΔ18] = new EmptyStruct();
                    }
                    re = re[3..];
                    break;
                }
                default: {
                    set[c] = new EmptyStruct();
                    re = re[1..];
                    break;
                }}

            }
            uintptr arg = default;
            if (!discard) {
                arg = VaUintptr(args);
            }
            for (; width != 0; width--) {
                var (cΔ19, err) = r.ReadByte();
                if (err != nil) {
                    if (AreEqual(err, io.EOF)) {
                        return (format, nvalues, match);
                    }

                    return (format, -1, match);
                }

                var (_, ok) = set[cΔ19, ꟷ]; if (ok == !neg) {
                    match = true;
                    nvalues = 1;
                    if (!discard) {
                        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)arg)).Value = cΔ19;
                        arg++;
                    }
                }
            }
            if (match) {
                var matchᴛ2 = false;
                if (width is 0) { matchᴛ2 = true;
                    r.UnreadByte();
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ2) { /* default: */
                    if (!discard) {
                        (@unsafe.Pointer.To<byte>((@unsafe.Pointer)arg)).Value = 0;
                    }
                }

            }
            break;
        }
        case (byte)'p': {
            format++;
            skipReaderWhiteSpace(r);
            var (cΔ20, err) = r.ReadByte();
            if (err != nil) {
                throw panic(todo("", err));
            }

            if (cΔ20 == '0') {
                (cΔ20, err) = r.ReadByte(); if (err != nil) {
                    throw panic(todo("", err));
                }

                if (cΔ20 != 'x' && cΔ20 != 'X') {
                    r.UnreadByte();
                }
            }

            uint64 digit = default;
            uint64 n = default;
ptr:
            for (; width != 0; width--) {
                var (cΔ21, errΔ9) = r.ReadByte();
                if (errΔ9 != nil) {
                    if (match) {
                        goto break_ptr;
                    }

                    throw panic(todo(""));
                }

                switch (ᐧ) {
                case {} when cΔ21 >= (byte)'0' && cΔ21 <= (byte)'9': {
                    digit = (uint64)cΔ21 - '0';
                    break;
                }
                case {} when cΔ21 >= (byte)'a' && cΔ21 <= (byte)'f': {
                    digit = (uint64)cΔ21 - 'a' + 10;
                    break;
                }
                case {} when cΔ21 >= (byte)'A' && cΔ21 <= (byte)'F': {
                    digit = (uint64)cΔ21 - 'A' + 10;
                    break;
                }
                default: {
                    r.UnreadByte();
                    goto break_ptr;
                    break;
                }}

                match = true;
                var n0 = n;
                n = (n << 4) + digit;
                if (n < n0) {
                    throw panic(todo(""));
                }
continue_ptr:;
            }
break_ptr:;
            if (!match) {
                break;
            }

            if (!discard) {
                var arg = VaUintptr(args);
                (@unsafe.Pointer.To<uintptr>((@unsafe.Pointer)arg)).Value = (uintptr)n;
            }
            nvalues = 1;
            break;
        }
        case (byte)'n': {
            format++;
            throw panic(todo(""));
            break;
        }
        default: {
            throw panic(todo("%#U", c));
            break;
        }}
    }

    return (format, nvalues, match);
}

internal static error skipReaderWhiteSpace(io.ByteScanner r) {
    for (;;) {
        var (c, err) = r.ReadByte();
        if (err != nil) {
            return err;
        }

        switch (c) {
        case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
            break;
        }
        default: {
            r.UnreadByte();
            return default;
        }}

    }
}

internal static uintptr skipWhiteSpace(uintptr s) {
    for (;;) {
        var c = ~@unsafe.Pointer.To<byte>((@unsafe.Pointer)s);
        switch (c) {
        case (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r' or (byte)'\v' or (byte)'\f': {
            s++;
            break;
        }
        default: {
            return s;
        }}

    }
}

internal static slice<byte> /*seq*/ fpLiteral(io.ByteScanner rd) {
    slice<byte> seq = default;
    GoFrame ᒐ = default;
    try {
        const rune endOfText = 0x110000;
        nint pos = default;
        nint width = default;
        nint length = default;

        defer(() => {
            if (len(seq) > length) {
                rd.UnreadByte();
                seq = seq.slice(0, len(seq) - 1);
            }
        }, ref ᒐ);

        rune r = default;
        (rune, nint) step(nint posΔ1) {
            var (b, err) = rd.ReadByte();
            if (err != nil) {
                return (endOfText, 0);
            }

            seq = append(seq, b);
            return ((rune)b, 1);
        }
        var stepʗ1 = step;
        void move() {
            pos += width;
            if (r != endOfText) {
                (r, width) = stepʗ1(pos + width);
            }
        }
        var moveʗ1 = move;
        bool accept(rune x) {
            if (r == x) {
                moveʗ1();
                return true;
            }
            return false;
        }
        var moveʗ2 = move;
        bool accept2(rune x) {
            if (r <= x) {
                moveʗ2();
                return true;
            }
            return false;
        }
        r = endOfText;
        width = 0;
        (r, width) = step(pos);
        if (accept('.')) {
            goto l7;
        }
        if (accept('+')) {
            goto l30;
        }
        if (accept('-')) {
            goto l30;
        }
        if (r < '0') {
            goto l4out;
        }
        if (accept2('9')) {
            goto l35;
        }
l4out:
        goto ᒐdone;
l7:
        if (r < '0') {
            goto l7out;
        }
        if (accept2('9')) {
            goto l10;
        }
l7out:
        goto ᒐdone;
l10:
        length = pos;
        if (accept('E')) {
            goto l18;
        }
        if (accept('e')) {
            goto l18;
        }
        if (r < '0') {
            goto l15out;
        }
        if (accept2('9')) {
            goto l10;
        }
l15out:
        goto ᒐdone;
l18:
        if (accept('+')) {
            goto l23;
        }
        if (accept('-')) {
            goto l23;
        }
        if (r < '0') {
            goto l20out;
        }
        if (accept2('9')) {
            goto l26;
        }
l20out:
        goto ᒐdone;
l23:
        if (r < '0') {
            goto l23out;
        }
        if (accept2('9')) {
            goto l26;
        }
l23out:
        goto ᒐdone;
l26:
        length = pos;
        if (r < '0') {
            goto l27out;
        }
        if (accept2('9')) {
            goto l26;
        }
l27out:
        goto ᒐdone;
l30:
        if (accept('.')) {
            goto l7;
        }
        if (r < '0') {
            goto l32out;
        }
        if (accept2('9')) {
            goto l35;
        }
l32out:
        goto ᒐdone;
l35:
        length = pos;
        if (accept('.')) {
            goto l7;
        }
        if (accept('E')) {
            goto l18;
        }
        if (accept('e')) {
            goto l18;
        }
        if (r < '0') {
            goto l42out;
        }
        if (accept2('9')) {
            goto l35;
        }
l42out:
        goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return seq;
}

} // end libc_package
