namespace go.github.com.spf13;

using base64 = go.encoding.base64_package;
using hex = go.encoding.hex_package;
using fmt = fmt_package;
using strings = strings_package;
using go.encoding;

partial class pflag_package {

partial struct bytesHexValue /*[]byte*/;

internal static @string String(this bytesHexValue bytesHex) {
    return fmt.Sprintf("%X"u8, (slice<byte>)bytesHex);
}

internal static error Set(this ref bytesHexValue bytesHex, @string value) {
    var (bin, err) = hex.DecodeString(strings.TrimSpace(value));

    if (err != default!) {
        return err;
    }

    bytesHex = bin;

    return default!;
}

internal static @string Type(this ref bytesHexValue _) {
    return "bytesHex";
}

internal static ж<bytesHexValue> newBytesHexValue(slice<byte> val, ж<slice<byte>> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<slice<byte>, bytesHexValue>();
}

internal static (any, error) bytesHexConv(@string sval) {
    var (bin, err) = hex.DecodeString(sval);

    if (err == default!) {
        return (bin, default!);
    }

    return (default!, fmt.Errorf("invalid string being converted to Bytes: %s %s"u8, sval, err));
}

public static (slice<byte>, error) GetBytesHex(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "bytesHex", bytesHexConv);

    if (err != default!) {
        return (new byte[]{}.slice(), err);
    }

    return (val._<slice<byte>>(), default!);
}

public static void BytesHexVar(this ж<FlagSet> Ꮡf, ж<slice<byte>> Ꮡp, @string name, slice<byte> value, @string usage) {
    Ꮡf.VarP(new bytesHexValueжValue(newBytesHexValue(value, Ꮡp)), name, "", usage);
}

public static void BytesHexVarP(this ж<FlagSet> Ꮡf, ж<slice<byte>> Ꮡp, @string name, @string shorthand, slice<byte> value, @string usage) {
    Ꮡf.VarP(new bytesHexValueжValue(newBytesHexValue(value, Ꮡp)), name, shorthand, usage);
}

public static void BytesHexVar(ж<slice<byte>> Ꮡp, @string name, slice<byte> value, @string usage) {
    CommandLine.VarP(new bytesHexValueжValue(newBytesHexValue(value, Ꮡp)), name, "", usage);
}

public static void BytesHexVarP(ж<slice<byte>> Ꮡp, @string name, @string shorthand, slice<byte> value, @string usage) {
    CommandLine.VarP(new bytesHexValueжValue(newBytesHexValue(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<byte>> BytesHex(this ж<FlagSet> Ꮡf, @string name, slice<byte> value, @string usage) {
    var p = @new<slice<byte>>();
    Ꮡf.BytesHexVarP(p, name, "", value, usage);
    return p;
}

public static ж<slice<byte>> BytesHexP(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<byte> value, @string usage) {
    var p = @new<slice<byte>>();
    Ꮡf.BytesHexVarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<slice<byte>> BytesHex(@string name, slice<byte> value, @string usage) {
    return CommandLine.BytesHexP(name, "", value, usage);
}

public static ж<slice<byte>> BytesHexP(@string name, @string shorthand, slice<byte> value, @string usage) {
    return CommandLine.BytesHexP(name, shorthand, value, usage);
}

partial struct bytesBase64Value /*[]byte*/;

internal static @string String(this bytesBase64Value bytesBase64) {
    return base64.StdEncoding.EncodeToString((slice<byte>)bytesBase64);
}

internal static error Set(this ref bytesBase64Value bytesBase64, @string value) {
    var (bin, err) = base64.StdEncoding.DecodeString(strings.TrimSpace(value));

    if (err != default!) {
        return err;
    }

    bytesBase64 = bin;

    return default!;
}

internal static @string Type(this ref bytesBase64Value _) {
    return "bytesBase64";
}

internal static ж<bytesBase64Value> newBytesBase64Value(slice<byte> val, ж<slice<byte>> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p = val;
    return Ꮡp.Reinterpret<slice<byte>, bytesBase64Value>();
}

internal static (any, error) bytesBase64ValueConv(@string sval) {
    var (bin, err) = base64.StdEncoding.DecodeString(sval);
    if (err == default!) {
        return (bin, default!);
    }

    return (default!, fmt.Errorf("invalid string being converted to Bytes: %s %s"u8, sval, err));
}

public static (slice<byte>, error) GetBytesBase64(this ж<FlagSet> Ꮡf, @string name) {
    var (val, err) = Ꮡf.getFlagType(name, "bytesBase64", bytesBase64ValueConv);

    if (err != default!) {
        return (new byte[]{}.slice(), err);
    }

    return (val._<slice<byte>>(), default!);
}

public static void BytesBase64Var(this ж<FlagSet> Ꮡf, ж<slice<byte>> Ꮡp, @string name, slice<byte> value, @string usage) {
    Ꮡf.VarP(new bytesBase64ValueжValue(newBytesBase64Value(value, Ꮡp)), name, "", usage);
}

public static void BytesBase64VarP(this ж<FlagSet> Ꮡf, ж<slice<byte>> Ꮡp, @string name, @string shorthand, slice<byte> value, @string usage) {
    Ꮡf.VarP(new bytesBase64ValueжValue(newBytesBase64Value(value, Ꮡp)), name, shorthand, usage);
}

public static void BytesBase64Var(ж<slice<byte>> Ꮡp, @string name, slice<byte> value, @string usage) {
    CommandLine.VarP(new bytesBase64ValueжValue(newBytesBase64Value(value, Ꮡp)), name, "", usage);
}

public static void BytesBase64VarP(ж<slice<byte>> Ꮡp, @string name, @string shorthand, slice<byte> value, @string usage) {
    CommandLine.VarP(new bytesBase64ValueжValue(newBytesBase64Value(value, Ꮡp)), name, shorthand, usage);
}

public static ж<slice<byte>> BytesBase64(this ж<FlagSet> Ꮡf, @string name, slice<byte> value, @string usage) {
    var p = @new<slice<byte>>();
    Ꮡf.BytesBase64VarP(p, name, "", value, usage);
    return p;
}

public static ж<slice<byte>> BytesBase64P(this ж<FlagSet> Ꮡf, @string name, @string shorthand, slice<byte> value, @string usage) {
    var p = @new<slice<byte>>();
    Ꮡf.BytesBase64VarP(p, name, shorthand, value, usage);
    return p;
}

public static ж<slice<byte>> BytesBase64(@string name, slice<byte> value, @string usage) {
    return CommandLine.BytesBase64P(name, "", value, usage);
}

public static ж<slice<byte>> BytesBase64P(@string name, @string shorthand, slice<byte> value, @string usage) {
    return CommandLine.BytesBase64P(name, shorthand, value, usage);
}

} // end pflag_package
