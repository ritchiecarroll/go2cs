namespace go.github.com.itchyny.gojq;

using json = encoding.json_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using yaml = go.github.com.itchyny.go_yaml.yaml_package;
using runewidth = go.github.com.mattn.go_runewidth.runewidth_package;
using gojq = go.github.com.itchyny.gojq_package;
using encoding;
using go.github.com.itchyny;
using go.github.com.itchyny.go_yaml;
using go.github.com.mattn.go_runewidth;
using unicode;

partial class cli_package {

partial struct emptyError {
    internal error err;
}

internal static @string Error(this ref emptyError _) {
    return "";
}

internal static void isEmptyError(this ref emptyError _) { }

internal static nint ExitCode(this ref emptyError err) {
    var (errΔ1, ok) = err.err._<run_typeᴛ1>(ᐧ); if (ok) {
        return errΔ1.ExitCode();
    }
    return exitCodeDefaultErr;
}

partial struct exitCodeError {
    internal nint code;
}

internal static @string Error(this ref exitCodeError err) {
    return "exit code: "u8 + strconv.Itoa(err.code);
}

internal static void isEmptyError(this ref exitCodeError _) { }

internal static nint ExitCode(this ref exitCodeError err) {
    return err.code;
}

partial struct flagParseError {
    internal error err;
}

internal static @string Error(this ref flagParseError err) {
    return err.err.Error();
}

internal static nint ExitCode(this ref flagParseError _) {
    return exitCodeFlagParseErr;
}

partial struct compileError {
    internal error err;
}

internal static @string Error(this ref compileError err) {
    return "compile error: "u8 + err.err.Error();
}

internal static nint ExitCode(this ref compileError _) {
    return exitCodeCompileErr;
}

partial struct queryParseError {
    internal @string fname, contents;
    internal error err;
}

internal static @string Error(this ref queryParseError err) {
    nint offset = default;
    ref var e = ref heap<ж<gojq.ParseError>>(out var Ꮡe);
    if (errors.As(err.err, Ꮡe)) {
        offset = e.Offset - len(e.Token) + 1;
    }
    var (linestr, line, column) = getLineByOffset(err.contents, offset);
    if (err.fname != "<arg>"u8 || containsNewline(err.contents)) {
        return fmt.Sprintf("invalid query: %s:%d\n%s  %s"u8,
            err.fname, line, formatLineInfo(linestr, line, column), err.err);
    }
    return fmt.Sprintf("invalid query: %s\n    %s\n    %*c  %s"u8,
        err.contents, linestr, column + 1, (rune)'^', err.err);
}

internal static nint ExitCode(this ref queryParseError _) {
    return exitCodeCompileErr;
}

partial struct jsonParseError {
    internal @string fname, contents;
    internal nint line;
    internal error err;
}

internal static @string Error(this ref jsonParseError err) {
    nint offset = default;
    if (AreEqual(err.err, io.ErrUnexpectedEOF)) {
        offset = len(err.contents) + 1;
    } else if (err.err._<ж<json.SyntaxError>>(out var e)) {
        offset = (nint)e.Offset;
    }
    var (linestr, line, column) = getLineByOffset(err.contents, offset);
    line += err.line; if (line > 1) {
        return fmt.Sprintf("invalid json: %s:%d\n%s  %s"u8,
            err.fname, line, formatLineInfo(linestr, line, column), err.err);
    }
    return fmt.Sprintf("invalid json: %s\n    %s\n    %*c  %s"u8,
        err.fname, linestr, column + 1, (rune)'^', err.err);
}

partial struct yamlParseError {
    internal @string fname, contents;
    internal error err;
}

internal static @string Error(this ref yamlParseError err) {
    nint index = default;
    @string message = default;
    ref var pe = ref heap<ж<yaml.ParserError>>(out var Ꮡpe);
    ref var te = ref heap<ж<yaml.TypeError>>(out var Ꮡte);
    if (errors.As(err.err, Ꮡpe)) {
        (index, message) = (pe.Index, pe.Message);
    } else if (errors.As(err.err, Ꮡte)) {
        ref var ue = ref heap<ж<yaml.UnmarshalError>>(out var Ꮡue);
        foreach (var (_, e) in range(te.Errors)) {
            if (errors.As(new yaml.UnmarshalErrorжerror(e), Ꮡue)) {
                (index, message) = (ue.Index, ue.Err.Error());
                break;
            }
        }
    }
    var (linestr, line, column) = getLineByOffset(err.contents, index + 1);
    return fmt.Sprintf("invalid yaml: %s:%d\n%s  %s"u8,
        err.fname, line, formatLineInfo(linestr, line, column), message);
}

internal static (@string linestr, nint line, nint column) getLineByOffset(@string str, nint offset) {
    @string linestr = default;
    nint line = default;
    nint column = default;

    var ss = Ꮡ(new stringScanner(str, 0));
    for (;;) {
        var (strΔ1, start, ok) = ss.next();
        if (!ok) {
            offset -= start;
            break;
        }
        line++;
        linestr = strΔ1;
        if (ss.offset >= offset) {
            offset -= start;
            break;
        }
    }
    offset = min(max(offset - 1, 0), len(linestr));
    if (offset > 48) {
        nint skip = len(trimLastInvalidRune(linestr.slice(0, offset - 48)));
        linestr = linestr.slice(skip);
        offset -= skip;
    }
    linestr = trimLastInvalidRune(linestr.slice(0, min(64, len(linestr))));
    if (offset < len(linestr)) {
        offset = len(trimLastInvalidRune(linestr.slice(0, offset)));
    } else {
        offset = len(linestr);
    }
    column = runewidth.StringWidth(linestr.slice(0, offset));
    return (linestr, line, column);
}

internal static @string trimLastInvalidRune(@string s) {
    for (nint i = len(s) - 1; i >= 0 && i > len(s) - (nint)utf8.UTFMax; i--) {
        var b = s[i]; if (b < utf8.RuneSelf) {
            return s.slice(0, i + 1);
        } else if (utf8.RuneStart(b)) {
            var (r, _) = utf8.DecodeRuneInString(s.slice(i)); if (r == utf8.RuneError) {
                return s.slice(0, i);
            }
            break;
        }
    }
    return s;
}

internal static @string formatLineInfo(@string linestr, nint line, nint column) {
    @string l = strconv.Itoa(line);
    return fmt.Sprintf("    %s | %s\n    %*c"u8, l, linestr, column + len(l) + 4, (rune)'^');
}

partial struct stringScanner {
    internal @string str;
    internal nint offset;
}

internal static (@string line, nint start, bool ok) next(this ref stringScanner ss) {
    @string line = default;
    nint start = default;
    bool ok = default;

    if (ss.offset == len(ss.str)) {
        return (line, start, ok);
    }
    (start, ok) = (ss.offset, true);
    line = ss.str.slice(start);
    nint i = indexNewline(line);
    if (i < 0) {
        ss.offset = len(ss.str);
        return (line, start, ok);
    }
    line = line.slice(0, i);
    if (strings.HasPrefix(ss.str.slice(start + i), "\r\n")) {
        i++;
    }
    ss.offset += i + 1;
    return (line, start, ok);
}

internal static bool containsNewline(@string str) {
    return strings.IndexByte(str, (byte)'\n') >= 0 || strings.IndexByte(str, (byte)'\r') >= 0;
}

internal static nint /*i*/ indexNewline(@string str) {
    nint i = default;

    i = strings.IndexByte(str, (byte)'\n'); if (i >= 0) {
        str = str.slice(0, i);
    }
    nint j = strings.IndexByte(str, (byte)'\r'); if (j >= 0) {
        i = j;
    }
    return i;
}

} // end cli_package
