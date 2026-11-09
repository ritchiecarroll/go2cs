namespace go.github.com.itchyny.gojq;

using fmt = fmt_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;

partial class cli_package {

internal static (slice<@string>, error) parseFlags(slice<@string> args, any opts) {
    var rest = new slice<@string>(0, len(args));
    var val = reflect.ValueOf(opts).Elem();
    var typ = val.Type();
    var longToValue = new map<@string, reflectꓸValue>{};
    var longToPositional = new map<@string, EmptyStruct>{};
    var shortToValue = new map<@string, reflectꓸValue>{};
    foreach (var i in range(val.NumField())) {
        {
            var (flag, ok) = typ.Field(i).Tag.Lookup("long"); if (ok) {
                longToValue[flag] = val.Field(i);
                var (_, okΔ1) = typ.Field(i).Tag.Lookup("positional"); if (okΔ1) {
                    longToPositional[flag] = new EmptyStruct();
                }
            }
        }
        {
            var (flag, ok) = typ.Field(i).Tag.Lookup("short"); if (ok) {
                shortToValue[flag] = val.Field(i);
            }
        }
    }
    var mapKeys = new map<@string, EmptyStruct>{};
    reflectꓸValue positionalVal = new(nil);
    bool optsDone = default;
    for (nint i = 0; i < len(args); i++) {
        @string arg = args[i];
        reflectꓸValue valΔ1 = new(nil);
        bool ok = default;
        @string shortopts = default;
        switch (ᐧ) {
        case {} when optsDone: {
            break;
        }
        case {} when arg == "--"u8: {
            optsDone = true;
            continue;
            break;
        }
        case {} when strings.HasPrefix(arg, "--"): {
            (valΔ1, ok) = longToValue[arg[2..], ꟷ]; if (!ok) {
                nint j = strings.IndexByte(arg, (byte)'='); if (j >= 0) {
                    (valΔ1, ok) = longToValue[arg.slice(2, j), ꟷ]; if (ok) {
                        if (valΔ1.Kind() == reflect.ΔBool) {
                            return (default, fmt.Errorf("boolean flag `%s' cannot have an argument"u8, arg.slice(0, j)));
                        }
                        args[i] = arg.slice(j + 1);
                        arg = arg.slice(0, j);
                        i--;
                    }
                }
                if (!ok) {
                    return (default, fmt.Errorf("unknown flag `%s'"u8, arg));
                }
            }
            break;
        }
        case {} when len(arg) > 1 && arg[0] == (byte)'-': {
            bool skip = default;
            for (nint iΔ2 = 1; iΔ2 < len(arg); iΔ2++) {
                @string opt = arg.slice(iΔ2, iΔ2 + 1);
                (valΔ1, ok) = shortToValue[opt, ꟷ]; if (ok) {
                    if (valΔ1.Kind() != reflect.ΔBool) {
                        break;
                    }
                } else if (!("A"u8 <= opt && opt <= "Z"u8 || "a"u8 <= opt && opt <= "z"u8)) {
                    skip = true;
                    break;
                }
            }
            if (!skip && (len(arg) > 2 || !ok)) {
                shortopts = arg[1..];
                goto L;
            }
            break;
        }}

        if (!ok) {
            if (positionalVal.IsValid() && len(rest) > 0) {
                positionalVal.Set(reflect.Append(positionalVal, reflect.ValueOf(arg)));
            } else {
                rest = append(rest, arg);
            }
            continue;
        }
S:
        switch (valΔ1.Kind()) {
        case var ᴛ when ᴛ == reflect.ΔBool: {
            valΔ1.SetBool(true);
            break;
        }
        case var ᴛ when ᴛ == reflect.ΔString: {
            i++; if (i >= len(args)) {
                return (default, fmt.Errorf("expected argument for flag `%s'"u8, arg));
            }
            valΔ1.SetString(args[i]);
            break;
        }
        case var ᴛ when ᴛ == reflect.ΔPointer: {
            if (valΔ1.Type().Elem().Kind() == reflect.ΔInt) {
                i++; if (i >= len(args)) {
                    return (default, fmt.Errorf("expected argument for flag `%s'"u8, arg));
                }
                var (v, err) = strconv.Atoi(args[i]);
                if (err != nil) {
                    return (default, fmt.Errorf("invalid argument for flag `%s': %w"u8, arg, err));
                }
                valΔ1.Set(reflect.New(valΔ1.Type().Elem()));
                valΔ1.Elem().SetInt((int64)v);
            }
            break;
        }
        case var ᴛ when ᴛ == reflect.ΔSlice: {
            var (_, okΔ3) = longToPositional[arg[2..], ꟷ]; if (okΔ3) {
                if (positionalVal.IsValid()) {
                    while (positionalVal.Len() > valΔ1.Len()) {
                        valΔ1.Set(reflect.Append(valΔ1, reflect.Zero(valΔ1.Type().Elem())));
                    }
                }
                positionalVal = valΔ1;
            } else {
                i++; if (i >= len(args)) {
                    return (default, fmt.Errorf("expected argument for flag `%s'"u8, arg));
                }
                valΔ1.Set(reflect.Append(valΔ1, reflect.ValueOf(args[i])));
            }
            break;
        }
        case var ᴛ when ᴛ == reflect.Map: {
            i += 2; if (i >= len(args)) {
                return (default, fmt.Errorf("expected 2 arguments for flag `%s'"u8, arg));
            }
            if (valΔ1.IsNil()) {
                valΔ1.Set(reflect.MakeMap(valΔ1.Type()));
            }
            @string name = args[i - 1];
            var (_, okΔ4) = mapKeys[name, ꟷ]; if (!okΔ4) {
                mapKeys[name] = new EmptyStruct();
                valΔ1.SetMapIndex(reflect.ValueOf(name), reflect.ValueOf(args[i]));
            }
            break;
        }}

        break_S:;
L:
        if (shortopts != ""u8) {
            @string opt = shortopts[..1];
            (valΔ1, ok) = shortToValue[opt, ꟷ]; if (!ok) {
                return (default, fmt.Errorf("unknown flag `%s'"u8, opt));
            }
            if (valΔ1.Kind() != reflect.ΔBool && len(shortopts) > 1) {
                if (shortopts[1] == '=') {
                    args[i] = shortopts[2..];
                } else {
                    args[i] = shortopts[1..];
                }
                i--;
                shortopts = "";
            } else {
                shortopts = shortopts[1..];
            }
            arg = "-"u8 + opt;
            goto S;
        }
    }
    return (rest, default);
}

internal static @string formatFlags(any opts) {
    var val = reflect.ValueOf(opts).Elem();
    var typ = val.Type();
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    Ꮡsb.WriteString("Command Options:\n");
    foreach (var i in range(typ.NumField())) {
        reflect.StructTag tag = typ.Field(i).Tag;
        if (i == typ.NumField() - 1) {
            Ꮡsb.WriteString("\nHelp Option:\n");
        }
        Ꮡsb.WriteString("  ");
        bool @short = default;
        {
            var (flag, ok) = tag.Lookup("short"); if (ok) {
                Ꮡsb.WriteString("-");
                Ꮡsb.WriteString(flag);
                @short = true;
            } else {
                Ꮡsb.WriteString("  ");
            }
        }
        nint m = sb.Len();
        {
            var (flag, ok) = tag.Lookup("long"); if (ok) {
                if (@short) {
                    Ꮡsb.WriteString(", ");
                } else {
                    Ꮡsb.WriteString("  ");
                }
                Ꮡsb.WriteString("--");
                Ꮡsb.WriteString(flag);
            }
        }
        {
            var (args, ok) = tag.Lookup("args"); if (ok) {
                Ꮡsb.WriteString(" ");
                Ꮡsb.WriteString(args);
            }
        }
        Ꮡsb.WriteString(strings.Repeat(" ", 24 - (sb.Len() - m)));
        Ꮡsb.WriteString(tag.Get("description"));
        Ꮡsb.WriteString("\n");
    }
    return sb.String();
}

} // end cli_package
