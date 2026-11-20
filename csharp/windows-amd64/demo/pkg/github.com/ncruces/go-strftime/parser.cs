namespace go.github.com.ncruces.go_strftime;

using utf8 = unicode.utf8_package;
using unicode;
using static strftime_package;

partial class strftime_package {

partial struct parser {
    internal Func<byte, byte, error> format;
    internal Func<byte, error> literal;
}

} partial class strftimeᴛparser {

internal static error parse(this ref parser p, @string fmt) {
    const nint initial = iota;
    const nint percent = 1;
    const nint flagged = 2;
    const nint modified = 3;

    byte flag = default;
    byte modifier = default;
    error err = default;
    nint state = initial;
    nint start = 0;
    foreach (var (i, b) in range(slice<byte>(fmt))) {
        switch (state) {
        default: {
            if (b == '%') {
                state = percent;
                start = i;
                continue;
            }
            err = p.literal(b);
            break;
        }
        case var ᴛ when ᴛ == percent: {
            if (b == '-' || b == ':') {
                state = flagged;
                flag = b;
                continue;
            }
            if (b == 'E' || b == 'O') {
                state = modified;
                modifier = b;
                flag = 0;
                continue;
            }
            err = p.format(b, 0);
            state = initial;
            break;
        }
        case var ᴛ when ᴛ == flagged: {
            if (b == 'E' || b == 'O') {
                state = modified;
                modifier = b;
                continue;
            }
            err = p.format(b, flag);
            state = initial;
            break;
        }
        case var ᴛ when ᴛ == modified: {
            if (okModifier(modifier, b)) {
                err = p.format(b, flag);
            } else {
                err = p.literals(fmt.slice(start, i + 1));
            }
            state = initial;
            break;
        }}

        if (err != nil) {
            var (errΔ1, ok) = err._<formatError>(ᐧ); if (ok) {
                errΔ1.setDirective(fmt, start, i);
                return errΔ1;
            }
            return err;
        }
    }

    if (state != initial) {
        return p.literals(fmt.slice(start));
    }
    return default;
}

internal static error literals(this ref parser p, @string literal) {
    foreach (var (_, b) in range(slice<byte>(literal))) {
        var err = p.literal(b); if (err != nil) {
            return err;
        }
    }
    return default;
}

} partial class strftime_package {

partial struct literalErr /*@string*/;

} partial class strftimeᴛliteralErr {

internal static @string Error(this literalErr e) {
    return "strftime: unsupported literal: "u8 + ((@string)e);
}

} partial class strftime_package {

partial struct formatError {
    internal @string message;
    internal @string directive;
}

} partial class strftimeᴛformatError {

internal static @string Error(this formatError e) {
    return "strftime: unsupported directive: "u8 + e.directive + " "u8 + e.message;
}

internal static void setDirective(this ref formatError e, @string str, nint i, nint j) {
    var (_, n) = utf8.DecodeRuneInString(str.slice(j));
    e.directive = str.slice(i, j + n);
}

} // end strftimeᴛformatError
