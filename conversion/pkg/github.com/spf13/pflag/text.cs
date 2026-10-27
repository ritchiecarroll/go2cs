namespace go.github.com.spf13;

using encoding = encoding_package;
using fmt = fmt_package;
using reflect = reflect_package;

partial class pflag_package {

partial struct textValue {
    internal encoding.TextUnmarshaler p;
}

internal static textValue newTextValue(encoding.TextMarshaler val, encoding.TextUnmarshaler p) {
    var ptrVal = reflect.ValueOf(p);
    if (ptrVal.Kind() != reflect.Ptr) {
        throw panic("variable value type must be a pointer");
    }
    var defVal = reflect.ValueOf(val);
    if (defVal.Kind() == reflect.Ptr) {
        defVal = defVal.Elem();
    }
    if (!AreEqual(defVal.Type(), ptrVal.Type().Elem())) {
        throw panic(fmt.Sprintf("default type does not match variable type: %v != %v"u8, defVal.Type(), ptrVal.Type().Elem()));
    }
    ptrVal.Elem().Set(defVal);
    return new textValue(p);
}

internal static error Set(this textValue v, @string s) {
    return v.p.UnmarshalText(slice<byte>(s));
}

internal static any Get(this textValue v) {
    return v.p;
}

internal static @string String(this textValue v) {
    {
        var (m, ok) = v.p._<encoding.TextMarshaler>(ᐧ); if (ok) {
            {
                var (b, err) = m.MarshalText(); if (err == default!) {
                    return (@string)b;
                }
            }
        }
    }
    return "";
}

internal static @string Type(this textValue v) {
    return reflect.ValueOf(v.p).Type().Name();
}

public static error GetText(this ж<FlagSet> Ꮡf, @string name, encoding.TextUnmarshaler @out) {
    var flag = Ꮡf.Lookup(name);
    if (flag == nil) {
        return fmt.Errorf("flag accessed but not defined: %s"u8, name);
    }
    if ((~flag).Value.Type() != reflect.TypeOf(@out).Name()) {
        return fmt.Errorf("trying to get %s value of flag of type %s"u8, reflect.TypeOf(@out).Name(), (~flag).Value.Type());
    }
    return @out.UnmarshalText(slice<byte>((~flag).Value.String()));
}

public static void TextVar(this ж<FlagSet> Ꮡf, encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) {
    Ꮡf.VarP(newTextValue(value, p), name, "", usage);
}

public static void TextVarP(this ж<FlagSet> Ꮡf, encoding.TextUnmarshaler p, @string name, @string shorthand, encoding.TextMarshaler value, @string usage) {
    Ꮡf.VarP(newTextValue(value, p), name, shorthand, usage);
}

public static void TextVar(encoding.TextUnmarshaler p, @string name, encoding.TextMarshaler value, @string usage) {
    CommandLine.VarP(newTextValue(value, p), name, "", usage);
}

public static void TextVarP(encoding.TextUnmarshaler p, @string name, @string shorthand, encoding.TextMarshaler value, @string usage) {
    CommandLine.VarP(newTextValue(value, p), name, shorthand, usage);
}

} // end pflag_package
