namespace go;

using json = encoding.json_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using Δio = io_package;
using os = os_package;
using Δregexp = regexp_package;
using sort = sort_package;
using strings = strings_package;
using jwt = github.com.golang_jwt.jwt.jwt_package;
using ecdsa = crypto.ecdsa_package;
using encoding;
using github.com.golang_jwt.jwt;
using rsa = crypto.rsa_package;
using Δcrypto = crypto_package;

partial class main_package {

internal static ж<@string> flagAlg;
internal static void initᴛflagAlg() { flagAlg = flag.String("alg"u8, ""u8, algHelp()); }
internal static ж<@string> flagKey = flag.String("key"u8, ""u8, "path to key file or '-' to read from stdin"u8);
internal static ж<bool> flagCompact = flag.Bool("compact"u8, false, "output compact JSON"u8);
internal static ж<bool> flagDebug = flag.Bool("debug"u8, false, "print out all kinds of debug data"u8);
internal static ArgList flagClaims = new ArgList(0);
internal static ArgList flagHead = new ArgList(0);
internal static ж<@string> flagSign = flag.String("sign"u8, ""u8, "path to claims file to sign, '-' to read from stdin, or '+' to use only -claim args"u8);
internal static ж<@string> flagVerify = flag.String("verify"u8, ""u8, "path to JWT token file to verify or '-' to read from stdin"u8);
internal static ж<@string> flagShow = flag.String("show"u8, ""u8, "path to JWT token file to show without verification or '-' to read from stdin"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string claimˢ = "claim"u8;
private static readonly @string addAdditionalClaimsMayBeˢ = "add additional claims. may be used more than once"u8;
private static readonly @string headerˢ = "header"u8;
private static readonly @string addAdditionalHeaderˢ = "add additional header params. may be used more than once"u8;

internal static void Main() {
    flag.Var(flagClaims, claimˢ, addAdditionalClaimsMayBeˢ);
    flag.Var(flagHead, headerˢ, addAdditionalHeaderˢ);
    flag.Usage = () => {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Usage of %s:\n"u8, os.Args[0]);
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "  One of the following flags is required: sign, verify or show\n"u8);
        flag.PrintDefaults();
    };
    flag.Parse();
    {
        var err = start(); if (err != default!) {
            fmt.Fprintf(new os.FileжWriter(os.Stderr), "Error: %v\n"u8, err);
            os.Exit(1);
        }
    }
}

internal static error start() {
    switch (ᐧ) {
    case {} when flagSign.Value != ""u8: {
        return signToken();
    }
    case {} when flagVerify.Value != ""u8: {
        return verifyToken();
    }
    case {} when flagShow.Value != ""u8: {
        return showToken();
    }
    default: {
        flag.Usage();
        return fmt.Errorf("none of the required flags are present. What do you want me to do?"u8);
    }}

}

internal static (slice<byte>, error retErr) loadData(@string p) {
    slice<byte> _ᴛ1 = default!;
    error retErr = default!;
    GoFrame ᒐ = default;
    try {
        if (p == ""u8) {
            (_ᴛ1, retErr) = (default!, fmt.Errorf("no path specified"u8)); goto ᒐdone;
        }
        Δio.Reader rdr = default!;
        var exprᴛ1 = p;
        if (exprᴛ1 == "-"u8) {
            rdr = new os_FileжReader(os.Stdin);
        }
        else if (exprᴛ1 == "+"u8) {
            (_ᴛ1, retErr) = (slice<byte>("{}"u8), default!); goto ᒐdone;
        }
        else { /* default: */
            var (f, err) = os.Open(p);
            if (err != default!) {
                (_ᴛ1, retErr) = (default!, err); goto ᒐdone;
            }
            rdr = new os_FileжReader(f);
            var fʗ1 = f;
            defer(() => {
                retErr = errors.Join(retErr, fʗ1.Close());
            }, ref ᒐ);
        }

        (_ᴛ1, retErr) = Δio.ReadAll(rdr);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (_ᴛ1, retErr);
}

internal static error printJSON(any j) {
    slice<byte> @out = default!;
    error err = default!;
    if (!flagCompact.Value){
        (@out, err) = json.MarshalIndent(j, ""u8, "    "u8);
    } else {
        (@out, err) = json.Marshal(j);
    }
    if (err == default!) {
        fmt.Println(((@string)@out));
    }
    return err;
}

internal static error verifyToken() {
    var (tokData, err) = loadData(flagVerify.Value);
    if (err != default!) {
        return fmt.Errorf("couldn't read token: %w"u8, err);
    }
    tokData = Δregexp.MustCompile(@"\s*$"u8).ReplaceAll(tokData, new byte[]{}.slice());
    if (flagDebug.Value) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Token len: %v bytes\n"u8, len(tokData));
    }
    (var token, err) = jwt.Parse(((@string)tokData), (ж<jwt.Token> t) => {
        if (isNone()) {
            return (jwt.UnsafeAllowNoneSignatureType, default!);
        }
        var (data, errΔ1) = loadData(flagKey.Value);
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        switch (ᐧ) {
        case {} when isEs(): {
            var (ᴛ1, ᴛ2) = jwt.ParseECPublicKeyFromPEM(data);
            return (ᴛ1.OrTypedNil(), ᴛ2);
        }
        case {} when isRs(): {
            var (ᴛ1, ᴛ2) = jwt.ParseRSAPublicKeyFromPEM(data);
            return (ᴛ1.OrTypedNil(), ᴛ2);
        }
        case {} when isEd(): {
            return jwt.ParseEdPublicKeyFromPEM(data);
        }
        default: {
            return (data, default!);
        }}

    });
    if (err != default!) {
        return fmt.Errorf("couldn't parse token: %w"u8, err);
    }
    if (flagDebug.Value) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Header:\n%v\n"u8, (~token).Header);
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Claims:\n%v\n"u8, (~token).Claims);
    }
    {
        var errΔ2 = printJSON((~token).Claims); if (errΔ2 != default!) {
            return fmt.Errorf("failed to output claims: %w"u8, errΔ2);
        }
    }
    return default!;
}

internal static error signToken() {
    var (tokData, err) = loadData(flagSign.Value);
    if (err != default!){
        return fmt.Errorf("couldn't read token: %w"u8, err);
    } else 
    if (flagDebug.Value) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Token: %v bytes"u8, len(tokData));
    }
    ref var claims = ref heap<jwt.MapClaims>(out var Ꮡclaims);
    {
        var errΔ1 = json.Unmarshal(tokData, Ꮡclaims); if (errΔ1 != default!) {
            return fmt.Errorf("couldn't parse claims JSON: %w"u8, errΔ1);
        }
    }
    if (len(flagClaims) > 0) {
        foreach (var (k, v) in flagClaims) {
            claims[k] = v;
        }
    }
    any key = default!;
    if (isNone()){
        key = jwt.UnsafeAllowNoneSignatureType;
    } else {
        (key, err) = loadData(flagKey.Value);
        if (err != default!) {
            return fmt.Errorf("couldn't read key: %w"u8, err);
        }
    }
    var alg = jwt.GetSigningMethod(flagAlg.Value);
    if (alg == default!) {
        return fmt.Errorf("couldn't find signing method: %v"u8, flagAlg.Value);
    }
    var token = jwt.NewWithClaims(alg, claims);
    if (len(flagHead) > 0) {
        foreach (var (k, v) in flagHead) {
            token.Value.Header[k] = v;
        }
    }
    switch (ᐧ) {
    case {} when isEs(): {
        var (k, ok) = key._<slice<byte>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("couldn't convert key data to key"u8);
        }
        (key, err) = jwt.ParseECPrivateKeyFromPEM(k);
        if (err != default!) {
            return err;
        }
        break;
    }
    case {} when isRs(): {
        var (k, ok) = key._<slice<byte>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("couldn't convert key data to key"u8);
        }
        (key, err) = jwt.ParseRSAPrivateKeyFromPEM(k);
        if (err != default!) {
            return err;
        }
        break;
    }
    case {} when isEd(): {
        var (k, ok) = key._<slice<byte>>(ᐧ);
        if (!ok) {
            return fmt.Errorf("couldn't convert key data to key"u8);
        }
        (key, err) = jwt.ParseEdPrivateKeyFromPEM(k);
        if (err != default!) {
            return err;
        }
        break;
    }}

    (var @out, err) = token.SignedString(key);
    if (err != default!) {
        return fmt.Errorf("error signing token: %w"u8, err);
    }
    fmt.Println(@out);
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object headerˢ2 = (@string)"Header:"u8;
private static readonly object claimsˢ = (@string)"Claims:"u8;

internal static error showToken() {
    var (tokData, err) = loadData(flagShow.Value);
    if (err != default!) {
        return fmt.Errorf("couldn't read token: %w"u8, err);
    }
    tokData = Δregexp.MustCompile(@"\s*$"u8).ReplaceAll(tokData, new byte[]{}.slice());
    if (flagDebug.Value) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "Token len: %v bytes\n"u8, len(tokData));
    }
    (var token, _, err) = jwt.NewParser().ParseUnverified(((@string)tokData), new jwt.MapClaims(0));
    if (err != default!) {
        return fmt.Errorf("malformed token: %w"u8, err);
    }
    fmt.Println(headerˢ2);
    {
        var errΔ1 = printJSON((~token).Header); if (errΔ1 != default!) {
            return fmt.Errorf("failed to output header: %w"u8, errΔ1);
        }
    }
    fmt.Println(claimsˢ);
    {
        var errΔ2 = printJSON((~token).Claims); if (errΔ2 != default!) {
            return fmt.Errorf("failed to output claims: %w"u8, errΔ2);
        }
    }
    return default!;
}

internal static bool isEs() {
    return strings.HasPrefix(flagAlg.Value, "ES"u8);
}

internal static bool isRs() {
    return strings.HasPrefix(flagAlg.Value, "RS"u8) || strings.HasPrefix(flagAlg.Value, "PS"u8);
}

internal static bool isEd() {
    return flagAlg.Value == "EdDSA"u8;
}

internal static bool isNone() {
    return flagAlg.Value == "none"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string signingAlgorithmˢ = "signing algorithm identifier, one of\n"u8;

internal static @string algHelp() {
    var algs = jwt.GetAlgorithms();
    sort.Strings(algs);
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    Ꮡb.WriteString(signingAlgorithmˢ);
    foreach (var (i, alg) in algs) {
        if (i > 0) {
            if (i % 7 == 0){
                Ꮡb.WriteString(",\n"u8);
            } else {
                Ꮡb.WriteString(", "u8);
            }
        }
        Ꮡb.WriteString(alg);
    }
    return b.String();
}

partial struct ArgList /*map[@string, @string]*/;

public static @string String(this ArgList l) {
    var (data, _) = json.Marshal(l);
    return ((@string)data);
}

public static error Set(this ArgList l, @string arg) {
    var parts = strings.SplitN(arg, "="u8, 2);
    if (len(parts) != 2) {
        return fmt.Errorf("invalid argument '%v'.  Must use format 'key=value'. %v"u8, arg, parts);
    }
    l[parts[0]] = parts[1];
    return default!;
}

} // end main_package
