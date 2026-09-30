// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using global::go.go;
using io = io_package;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class fuzz_package {

// encVersion1 will be the first line of a file with version 1 encoding.
internal static @string encVersion1 = "go test fuzz v1"u8;

// marshalCorpusFile encodes an arbitrary number of arguments into the file format for the
// corpus.
internal static slice<byte> marshalCorpusFile(params ꓸꓸꓸany valsʗp) {
    var vals = valsʗp.sslice();

    if (len(vals) == 0) {
        throw panic("must have at least one value to marshal");
    }
    var b = bytes.NewBuffer(slice<byte>(encVersion1 + "\n"));
    // TODO(katiehockman): keep uint8 and int32 encoding where applicable,
    // instead of changing to byte and rune respectively.
    foreach (var (_, val) in vals) {
        switch (val.type()) {
        case nint _:
        case int8 _:
        case int16 _:
        case int64 _:
        case nuint _:
        case uint16 _:
        case uint32 _:
        case uint64 _:
        case bool _: {
            var t = val;
            fmt.Fprintf(new bytes_BufferжWriter(b), "%T(%v)\n"u8, t, t);
            break;
        }
        case float32 t: {
            if (math.IsNaN((float64)t) && math.Float32bits(t) != math.Float32bits((float32)math.NaN())){
                // We encode unusual NaNs as hex values, because that is how users are
                // likely to encounter them in literature about floating-point encoding.
                // This allows us to reproduce fuzz failures that depend on the specific
                // NaN representation (for float32 there are about 2^24 possibilities!),
                // not just the fact that the value is *a* NaN.
                //
                // Note that the specific value of float32(math.NaN()) can vary based on
                // whether the architecture represents signaling NaNs using a low bit
                // (as is common) or a high bit (as commonly implemented on MIPS
                // hardware before around 2012). We believe that the increase in clarity
                // from identifying "NaN" with math.NaN() is worth the slight ambiguity
                // from a platform-dependent value.
                fmt.Fprintf(new bytes_BufferжWriter(b), "math.Float32frombits(0x%x)\n"u8, math.Float32bits(t));
            } else {
                // We encode all other values — including the NaN value that is
                // bitwise-identical to float32(math.Nan()) — using the default
                // formatting, which is equivalent to strconv.FormatFloat with format
                // 'g' and can be parsed by strconv.ParseFloat.
                //
                // For an ordinary floating-point number this format includes
                // sufficiently many digits to reconstruct the exact value. For positive
                // or negative infinity it is the string "+Inf" or "-Inf". For positive
                // or negative zero it is "0" or "-0". For NaN, it is the string "NaN".
                fmt.Fprintf(new bytes_BufferжWriter(b), "%T(%v)\n"u8, t, t);
            }
            break;
        }
        case float64 t: {
            if (math.IsNaN(t) && math.Float64bits(t) != math.Float64bits(math.NaN())){
                fmt.Fprintf(new bytes_BufferжWriter(b), "math.Float64frombits(0x%x)\n"u8, math.Float64bits(t));
            } else {
                fmt.Fprintf(new bytes_BufferжWriter(b), "%T(%v)\n"u8, t, t);
            }
            break;
        }
        case @string t: {
            fmt.Fprintf(new bytes_BufferжWriter(b), "string(%q)\n"u8, t);
            break;
        }
        case rune t: {
            if (utf8.ValidRune(t)){
                // int32
                // Although rune and int32 are represented by the same type, only a subset
                // of valid int32 values can be expressed as rune literals. Notably,
                // negative numbers, surrogate halves, and values above unicode.MaxRune
                // have no quoted representation.
                //
                // fmt with "%q" (and the corresponding functions in the strconv package)
                // would quote out-of-range values to the Unicode replacement character
                // instead of the original value (see https://go.dev/issue/51526), so
                // they must be treated as int32 instead.
                //
                // We arbitrarily draw the line at UTF-8 validity, which biases toward the
                // "rune" interpretation. (However, we accept either format as input.)
                fmt.Fprintf(new bytes_BufferжWriter(b), "rune(%q)\n"u8, t);
            } else {
                fmt.Fprintf(new bytes_BufferжWriter(b), "int32(%v)\n"u8, t);
            }
            break;
        }
        case byte t: {
            fmt.Fprintf(new bytes_BufferжWriter(b), // uint8
 // For bytes, we arbitrarily prefer the character interpretation.
 // (Every byte has a valid character encoding.)
 "byte(%q)\n"u8, t);
            break;
        }
        case slice<byte> t: {
            fmt.Fprintf(new bytes_BufferжWriter(b), // []uint8
 "[]byte(%q)\n"u8, t);
            break;
        }
        default: {
            var t = val;
            throw panic(fmt.Sprintf("unsupported type: %T"u8, t));
            break;
        }}
    }
    return b.Bytes();
}

// unmarshalCorpusFile decodes corpus bytes into their respective values.
internal static (slice<any>, error) unmarshalCorpusFile(slice<byte> b) {
    if (len(b) == 0) {
        return (default!, fmt.Errorf("cannot unmarshal empty string"u8));
    }
    var lines = bytes.Split(b, slice<byte>("\n"u8));
    if (len(lines) < 2) {
        return (default!, fmt.Errorf("must include version and at least one value"u8));
    }
    @string version = strings.TrimSuffix(((@string)lines[0]), "\r"u8);
    if (version != encVersion1) {
        return (default!, fmt.Errorf("unknown encoding version: %s"u8, version));
    }
    slice<any> vals = default!;
    foreach (var (_, vᴛ1) in lines[1..]) {
        var line = vᴛ1;

        line = bytes.TrimSpace(line);
        if (len(line) == 0) {
            continue;
        }
        var (v, err) = parseCorpusValue(line);
        if (err != default!) {
            return (default!, fmt.Errorf("malformed line %q: %v"u8, line, err));
        }
        vals = append(vals, v);
    }
    return (vals, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testˢ = "(test)"u8;
internal static readonly @string infˢ = "-Inf"u8;
internal static readonly @string infˢ2 = "+Inf"u8;
internal static readonly @string naNˢ = "NaN"u8;
internal static readonly @string uint32ˢ = "uint32"u8;
internal static readonly @string uint64ˢ = "uint64"u8;

internal static (any, error) parseCorpusValue(slice<byte> line) {
    var fs = token.NewFileSet();
    var (expr, err) = parser.ParseExprFrom(fs, testˢ, line, 0);
    if (err != default!) {
        return (default!, err);
    }
    var (call, ok) = expr._<ж<ast.CallExpr>>(ᐧ);
    if (!ok) {
        return (default!, fmt.Errorf("expected call expression"u8));
    }
    if (len((~call).Args) != 1) {
        return (default!, fmt.Errorf("expected call expression with 1 argument; got %d"u8, len((~call).Args)));
    }
    var arg = (~call).Args[0];
    {
        var (arrayType, okΔ1) = (~call).Fun._<ж<ast.ArrayType>>(ᐧ); if (okΔ1) {
            if ((~arrayType).Len != default!) {
                return (default!, fmt.Errorf("expected []byte or primitive type"u8));
            }
            var (elt, okΔ2) = (~arrayType).Elt._<ж<ast.Ident>>(ᐧ);
            if (!okΔ2 || (~elt).Name != "byte"u8) {
                return (default!, fmt.Errorf("expected []byte"u8));
            }
            (var lit, okΔ2) = arg._<ж<ast.BasicLit>>(ᐧ);
            if (!okΔ2 || (~lit).Kind != token.STRING) {
                return (default!, fmt.Errorf("string literal required for type []byte"u8));
            }
            var (s, errΔ1) = strconv.Unquote((~lit).Value);
            if (errΔ1 != default!) {
                return (default!, errΔ1);
            }
            return (slice<byte>(s), default!);
        }
    }
    ж<ast.Ident> idType = default!;
    {
        var (selector, okΔ3) = (~call).Fun._<ж<ast.SelectorExpr>>(ᐧ); if (okΔ3){
            var (xIdent, okΔ4) = (~selector).X._<ж<ast.Ident>>(ᐧ);
            if (!okΔ4 || (~xIdent).Name != "math"u8) {
                return (default!, fmt.Errorf("invalid selector type"u8));
            }
            var exprᴛ1 = (~(~selector).Sel).Name;
            if (exprᴛ1 == "Float64frombits"u8) {
                idType = Ꮡ(new ast.Ident(Name: "float64-bits"u8));
            }
            else if (exprᴛ1 == "Float32frombits"u8) {
                idType = Ꮡ(new ast.Ident(Name: "float32-bits"u8));
            }
            else { /* default: */
                return (default!, fmt.Errorf("invalid selector type"u8));
            }

        } else {
            (idType, okΔ3) = (~call).Fun._<ж<ast.Ident>>(ᐧ);
            if (!okΔ3) {
                return (default!, fmt.Errorf("expected []byte or primitive type"u8));
            }
            if ((~idType).Name == "bool"u8) {
                var (id, okΔ5) = arg._<ж<ast.Ident>>(ᐧ);
                if (!okΔ5) {
                    return (default!, fmt.Errorf("malformed bool"u8));
                }
                if ((~id).Name == "true"u8){
                    return (true, default!);
                } else 
                if ((~id).Name == "false"u8){
                    return (false, default!);
                } else {
                    return (default!, fmt.Errorf("true or false required for type bool"u8));
                }
            }
        }
    }
    @string val = default!;
    token.Token kind = default!;
    {
        var (op, okΔ6) = arg._<ж<ast.UnaryExpr>>(ᐧ); if (okΔ6){
            switch ((~op).X.type()) {
            case ж<ast.BasicLit> lit: {
                if ((~op).Op != token.SUB) {
                    return (default!, fmt.Errorf("unsupported operation on int/float: %v"u8, (~op).Op));
                }
                val = (~op).Op.String() + (~lit).Value; // e.g. "-" + "124"
                kind = lit.Value.Kind;
                break;
            }
            case ж<ast.Ident> lit: {
                if ((~lit).Name != "Inf"u8) {
                    // Special case for negative numbers.
                    return (default!, fmt.Errorf("expected operation on int or float type"u8));
                }
                if ((~op).Op == token.SUB){
                    val = infˢ;
                } else {
                    val = infˢ2;
                }
                kind = token.FLOAT;
                break;
            }
            default: {
                var lit = (~op).X;
                return (default!, fmt.Errorf("expected operation on int or float type"u8));
            }}
        } else {
            switch (arg.type()) {
            case ж<ast.BasicLit> lit: {
                (val, kind) = (lit.Value.Value, lit.Value.Kind);
                break;
            }
            case ж<ast.Ident> lit: {
                if ((~lit).Name != "NaN"u8) {
                    return (default!, fmt.Errorf("literal value required for primitive type"u8));
                }
                (val, kind) = (naNˢ, token.FLOAT);
                break;
            }
            default: {
                var lit = arg;
                return (default!, fmt.Errorf("literal value required for primitive type"u8));
            }}
        }
    }
    {
        @string typ = idType.Value.Name;
        var exprᴛ2 = typ;
        if (exprᴛ2 == "string"u8) {
            if (kind != token.STRING) {
                return (default!, fmt.Errorf("string literal value required for type string"u8));
            }
            var (ᴛ1, ᴛ2) = strconv.Unquote(val);
            return (ᴛ1, ᴛ2);
        }
        if (exprᴛ2 == "byte"u8 || exprᴛ2 == "rune"u8) {
            if (kind == token.INT) {
                var exprᴛ3 = typ;
                if (exprᴛ3 == "rune"u8) {
                    return parseInt(val, typ);
                }
                if (exprᴛ3 == "byte"u8) {
                    return parseUint(val, typ);
                }

            }
            if (kind != token.CHAR) {
                return (default!, fmt.Errorf("character literal required for byte/rune types"u8));
            }
            nint n = len(val);
            if (n < 2) {
                return (default!, fmt.Errorf("malformed character literal, missing single quotes"u8));
            }
            var (code, _, _, errΔ3) = strconv.UnquoteChar(val.slice(1, n - 1), (rune)'\'');
            if (errΔ3 != default!) {
                return (default!, errΔ3);
            }
            if (typ == "rune"u8) {
                return (code, default!);
            }
            if (code >= 256) {
                return (default!, fmt.Errorf("can only encode single byte to a byte type"u8));
            }
            return ((byte)code, default!);
        }
        if (exprᴛ2 == "int"u8 || exprᴛ2 == "int8"u8 || exprᴛ2 == "int16"u8 || exprᴛ2 == "int32"u8 || exprᴛ2 == "int64"u8) {
            if (kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for int types"u8));
            }
            return parseInt(val, typ);
        }
        if (exprᴛ2 == "uint"u8 || exprᴛ2 == "uint8"u8 || exprᴛ2 == "uint16"u8 || exprᴛ2 == "uint32"u8 || exprᴛ2 == "uint64"u8) {
            if (kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for uint types"u8));
            }
            return parseUint(val, typ);
        }
        if (exprᴛ2 == "float32"u8) {
            if (kind != token.FLOAT && kind != token.INT) {
                return (default!, fmt.Errorf("float or integer literal required for float32 type"u8));
            }
            var (v, errΔ4) = strconv.ParseFloat(val, 32);
            return ((float32)v, errΔ4);
        }
        if (exprᴛ2 == "float64"u8) {
            if (kind != token.FLOAT && kind != token.INT) {
                return (default!, fmt.Errorf("float or integer literal required for float64 type"u8));
            }
            var (ᴛ1, ᴛ2) = strconv.ParseFloat(val, 64);
            return (ᴛ1, ᴛ2);
        }
        if (exprᴛ2 == "float32-bits"u8) {
            if (kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for math.Float32frombits type"u8));
            }
            var (bits, errΔ5) = parseUint(val, uint32ˢ);
            if (errΔ5 != default!) {
                return (default!, errΔ5);
            }
            return (math.Float32frombits(bits._<uint32>()), default!);
        }
        if (exprᴛ2 == "float64-bits"u8) {
            if (kind != token.FLOAT && kind != token.INT) {
                return (default!, fmt.Errorf("integer literal required for math.Float64frombits type"u8));
            }
            var (bits, errΔ6) = parseUint(val, uint64ˢ);
            if (errΔ6 != default!) {
                return (default!, errΔ6);
            }
            return (math.Float64frombits(bits._<uint64>()), default!);
        }
        { /* default: */
            return (default!, fmt.Errorf("expected []byte or primitive type"u8));
        }
    }

}

// parseInt returns an integer of value val and type typ.
internal static (any, error) parseInt(@string val, @string typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == "int"u8) {
        var (i, err) = strconv.ParseInt(val, // The int type may be either 32 or 64 bits. If 32, the fuzz tests in the
 // corpus may include 64-bit values produced by fuzzing runs on 64-bit
 // architectures. When running those tests, we implicitly wrap the values to
 // fit in a regular int. (The test case is still “interesting”, even if the
 // specific values of its inputs are platform-dependent.)
 0, 64);
        return ((nint)i, err);
    }
    if (exprᴛ1 == "int8"u8) {
        var (i, err) = strconv.ParseInt(val, 0, 8);
        return ((int8)i, err);
    }
    if (exprᴛ1 == "int16"u8) {
        var (i, err) = strconv.ParseInt(val, 0, 16);
        return ((int16)i, err);
    }
    if (exprᴛ1 == "int32"u8 || exprᴛ1 == "rune"u8) {
        var (i, err) = strconv.ParseInt(val, 0, 32);
        return ((int32)i, err);
    }
    if (exprᴛ1 == "int64"u8) {
        var (ᴛ1, ᴛ2) = strconv.ParseInt(val, 0, 64);
        return (ᴛ1, ᴛ2);
    }
    { /* default: */
        throw panic("unreachable");
    }

}

// parseUint returns an unsigned integer of value val and type typ.
internal static (any, error) parseUint(@string val, @string typ) {
    var exprᴛ1 = typ;
    if (exprᴛ1 == "uint"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 64);
        return ((nuint)i, err);
    }
    if (exprᴛ1 == "uint8"u8 || exprᴛ1 == "byte"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 8);
        return ((uint8)i, err);
    }
    if (exprᴛ1 == "uint16"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 16);
        return ((uint16)i, err);
    }
    if (exprᴛ1 == "uint32"u8) {
        var (i, err) = strconv.ParseUint(val, 0, 32);
        return ((uint32)i, err);
    }
    if (exprᴛ1 == "uint64"u8) {
        var (ᴛ1, ᴛ2) = strconv.ParseUint(val, 0, 64);
        return (ᴛ1, ᴛ2);
    }
    { /* default: */
        throw panic("unreachable");
    }

}

} // end fuzz_package
