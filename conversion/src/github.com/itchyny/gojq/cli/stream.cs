namespace go.github.com.itchyny.gojq;

using json = encoding.json_package;
using io = io_package;
using encoding;

partial class cli_package {

partial struct jsonStream {
    internal ж<json.Decoder> dec;
    internal slice<any> path;
    internal slice<nint> states;
}

internal static ж<jsonStream> newJSONStream(ж<json.Decoder> dec) {
    return Ꮡ(new jsonStream(dec: dec, states: [jsonStateTopValue], path: []));
}

internal static UntypedInt jsonStateTopValue => iota;
internal static UntypedInt jsonStateArrayStart => 1;
internal static UntypedInt jsonStateArrayValue => 2;
internal static UntypedInt jsonStateArrayEnd => 3;
internal static UntypedInt jsonStateArrayEmptyEnd => 4;
internal static UntypedInt jsonStateObjectStart => 5;
internal static UntypedInt jsonStateObjectKey => 6;
internal static UntypedInt jsonStateObjectValue => 7;
internal static UntypedInt jsonStateObjectEnd => 8;
internal static UntypedInt jsonStateObjectEmptyEnd => 9;

internal static (any, error) next(this ref jsonStream s) {
    var exprᴛ1 = s.states[len(s.states) - 1];
    var matchᴛ1 = false;
    if (exprᴛ1 == jsonStateArrayEnd || exprᴛ1 == jsonStateObjectEnd) {
        s.path = s.path.slice(0, len(s.path) - 1);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && (exprᴛ1 == jsonStateArrayEmptyEnd || exprᴛ1 == jsonStateObjectEmptyEnd)) { matchᴛ1 = true;
        s.states = s.states.slice(0, len(s.states) - 1);
    }

    if (s.dec.More()) {
        switch (s.states[len(s.states) - 1]) {
        case var ᴛ when ᴛ == jsonStateArrayValue: {
            s.path[len(s.path) - 1] = s.path[len(s.path) - 1]._<nint>() + 1;
            break;
        }
        case var ᴛ when ᴛ == jsonStateObjectValue: {
            s.path = s.path.slice(0, len(s.path) - 1);
            break;
        }}

    }
    for (;;) {
        var (token, err) = s.dec.Token();
        if (err != nil) {
            if (AreEqual(err, io.EOF) && s.states[len(s.states) - 1] != jsonStateTopValue) {
                err = io.ErrUnexpectedEOF;
            }
            return (default, err);
        }
        var (d, ok) = token._<json.Delim>(ᐧ); if (ok) {
            switch (d) {
            case var ᴛ when ᴛ == (json.Delim)((rune)'[') || ᴛ == (json.Delim)((rune)'{'): {
                switch (s.states[len(s.states) - 1]) {
                case var ᴛ2 when ᴛ2 == jsonStateArrayStart: {
                    s.states[len(s.states) - 1] = jsonStateArrayValue;
                    break;
                }
                case var ᴛ2 when ᴛ2 == jsonStateObjectKey: {
                    s.states[len(s.states) - 1] = jsonStateObjectValue;
                    break;
                }}

                if (d == (rune)'[') {
                    s.states = append(s.states, (nint)(jsonStateArrayStart));
                    s.path = append(s.path, (any)((nint)0));
                } else {
                    s.states = append(s.states, (nint)(jsonStateObjectStart));
                }
                break;
            }
            case var ᴛ when ᴛ == (json.Delim)((rune)']'): {
                if (s.states[len(s.states) - 1] == jsonStateArrayStart) {
                    s.states[len(s.states) - 1] = jsonStateArrayEmptyEnd;
                    s.path = s.path.slice(0, len(s.path) - 1);
                    return (slice<any>([s.copyPath(), slice<any>([])]), default);
                }
                s.states[len(s.states) - 1] = jsonStateArrayEnd;
                return (slice<any>([s.copyPath()]), default);
            }
            case var ᴛ when ᴛ == (json.Delim)((rune)'}'): {
                if (s.states[len(s.states) - 1] == jsonStateObjectStart) {
                    s.states[len(s.states) - 1] = jsonStateObjectEmptyEnd;
                    return (slice<any>([s.copyPath(), new map<@string, any>{}]), default);
                }
                s.states[len(s.states) - 1] = jsonStateObjectEnd;
                return (slice<any>([s.copyPath()]), default);
            }}

        } else {
            var exprᴛ5 = s.states[len(s.states) - 1];
            var matchᴛ2 = false;
            if (exprᴛ5 == jsonStateArrayStart) { matchᴛ2 = true;
                s.states[len(s.states) - 1] = jsonStateArrayValue;
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ2 && exprᴛ5 == jsonStateArrayValue) { matchᴛ2 = true;
                return (slice<any>([s.copyPath(), token]), default);
            }
            if (exprᴛ5 == jsonStateObjectStart || exprᴛ5 == jsonStateObjectValue) { matchᴛ2 = true;
                s.states[len(s.states) - 1] = jsonStateObjectKey;
                s.path = append(s.path, (any)(token));
            }
            else if (exprᴛ5 == jsonStateObjectKey) {
                s.states[len(s.states) - 1] = jsonStateObjectValue;
                return (slice<any>([s.copyPath(), token]), default);
            }
            else if (!matchᴛ2) { /* default: */
                s.states[len(s.states) - 1] = jsonStateTopValue;
                return (slice<any>([s.copyPath(), token]), default);
            }

        }
    }
}

internal static slice<any> copyPath(this ref jsonStream s) {
    var path = new slice<any>(len(s.path));
    copy(path, s.path);
    return path;
}

} // end cli_package
