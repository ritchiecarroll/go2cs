// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buildcfg provides access to the build configuration
// described by the current environment. It is for use by build tools
// such as cmd/go or cmd/compile and for setting up go/build's Default context.
//
// Note that it does NOT provide access to the build configuration used to
// build the currently-running binary. For that, use runtime.GOOS etc
// as well as internal/goexperiment.
namespace go.@internal;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using strings = strings_package;
using io = io_package;
using path;

partial class buildcfg_package {

public static @string GOROOT = os.Getenv("GOROOT"u8); // cached for efficiency
public static @string GOARCH;
internal static void initᴛGOARCH() { GOARCH = envOr("GOARCH"u8, defaultGOARCH); }
public static @string GOOS;
internal static void initᴛGOOS() { GOOS = envOr("GOOS"u8, defaultGOOS); }
public static @string GO386;
internal static void initᴛGO386() { GO386 = envOr("GO386"u8, DefaultGO386); }
public static nint GOAMD64;
internal static void initᴛGOAMD64() { GOAMD64 = goamd64(); }
public static GoarmFeatures GOARM;
internal static void initᴛGOARM() { GOARM = goarm(); }
public static Goarm64Features GOARM64;
internal static void initᴛGOARM64() { GOARM64 = goarm64(); }
public static @string GOMIPS;
internal static void initᴛGOMIPS() { GOMIPS = gomips(); }
public static @string GOMIPS64;
internal static void initᴛGOMIPS64() { GOMIPS64 = gomips64(); }
public static nint GOPPC64;
internal static void initᴛGOPPC64() { GOPPC64 = goppc64(); }
public static nint GORISCV64;
internal static void initᴛGORISCV64() { GORISCV64 = goriscv64(); }
public static gowasmFeatures GOWASM;
internal static void initᴛGOWASM() { GOWASM = gowasm(); }
public static slice<@string> ToolTags;
internal static void initᴛToolTags() { ToolTags = toolTags(); }
public static @string GO_LDSO;
internal static void initᴛGO_LDSO() { GO_LDSO = defaultGO_LDSO; }
public static @string GOFIPS140;
internal static void initᴛGOFIPS140() { GOFIPS140 = gofips140(); }
public static @string Version;
internal static void initᴛVersion() { Version = version; }

// Error is one of the errors found (if any) in the build configuration.
public static error Error;

// Check exits the program with a fatal error if Error is non-nil.
public static void Check() {
    if (Error != default!) {
        fmt.Fprintf(new os.FileжWriter(os.Stderr), "%s: %v\n"u8, filepath.Base(os.Args[0]), Error);
        os.Exit(2);
    }
}

internal static @string envOr(@string key, @string value) {
    {
        @string x = os.Getenv(key); if (x != ""u8) {
            return x;
        }
    }
    return value;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goamd64ˢ = "GOAMD64"u8;

internal static nint goamd64() {
    {
        @string v = envOr(goamd64ˢ, DefaultGOAMD64);
        var exprᴛ1 = v;
        if (exprᴛ1 == "v1"u8) {
            return 1;
        }
        if (exprᴛ1 == "v2"u8) {
            return 2;
        }
        if (exprᴛ1 == "v3"u8) {
            return 3;
        }
        if (exprᴛ1 == "v4"u8) {
            return 4;
        }
    }

    Error = fmt.Errorf("invalid GOAMD64: must be v1, v2, v3, v4"u8);
    return (nint)((byte)(DefaultGOAMD64[len("v")] - (rune)'0'));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gofips140ˢ = "GOFIPS140"u8;

internal static @string gofips140() {
    @string v = envOr(gofips140ˢ, DefaultGOFIPS140);
    var exprᴛ1 = v;
    if (exprᴛ1 == "off"u8 || exprᴛ1 == "latest"u8 || exprᴛ1 == "inprocess"u8 || exprᴛ1 == "certified"u8) {
        return v;
    }

    if (isFIPSVersion(v)) {
        return v;
    }
    Error = fmt.Errorf("invalid GOFIPS140: must be off, latest, inprocess, certified, or vX.Y.Z"u8);
    return DefaultGOFIPS140;
}

// isFIPSVersion reports whether v is a valid FIPS version,
// of the form vX.Y.Z or vX.Y.Z-hash.
internal static bool isFIPSVersion(@string v) {
    if (!strings.HasPrefix(v, "v"u8)) {
        return false;
    }
    (v, var ok) = skipNum(v[(int)(len("v"))..]);
    if (!ok || !strings.HasPrefix(v, "."u8)) {
        return false;
    }
    (v, ok) = skipNum(v[(int)(len("."))..]);
    if (!ok || !strings.HasPrefix(v, "."u8)) {
        return false;
    }
    (v, ok) = skipNum(v[(int)(len("."))..]);
    var hasHash = strings.HasPrefix(v, "-"u8) && len(v) == len("-") + 8;
    return ok && (v == ""u8 || hasHash);
}

// skipNum skips the leading text matching [0-9]+
// in s, returning the rest and whether such text was found.
internal static (@string rest, bool ok) skipNum(@string s) {
    nint i = 0;
    while (i < len(s) && (rune)'0' <= s[i] && s[i] <= (rune)'9') {
        i++;
    }
    return (s.slice(i), i > 0);
}

[GoType] partial struct GoarmFeatures {
    public nint Version;
    public bool SoftFloat;
}

public static @string String(this GoarmFeatures g) {
    @string armStr = strconv.Itoa(g.Version);
    if (g.SoftFloat){
        armStr += ",softfloat"u8;
    } else {
        armStr += ",hardfloat"u8;
    }
    return armStr;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goarmˢ = "GOARM"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string softFloatOptᶜ = ",softfloat"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string hardFloatOptᶜ = ",hardfloat"u8;

internal static GoarmFeatures /*g*/ goarm() {
    GoarmFeatures g = default!;

    @string softFloatOpt = softFloatOptᶜ;
    @string hardFloatOpt = hardFloatOptᶜ;
    @string def = DefaultGOARM;
    if (GOOS == "android"u8 && GOARCH == "arm"u8) {
        // Android arm devices always support GOARM=7.
        def = "7"u8;
    }
    @string v = envOr(goarmˢ, def);
    var floatSpecified = false;
    if (strings.HasSuffix(v, softFloatOpt)) {
        g.SoftFloat = true;
        floatSpecified = true;
        v = v.slice(0, len(v) - len(softFloatOpt));
    }
    if (strings.HasSuffix(v, hardFloatOpt)) {
        floatSpecified = true;
        v = v.slice(0, len(v) - len(hardFloatOpt));
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == "5"u8) {
        g.Version = 5;
    }
    else if (exprᴛ1 == "6"u8) {
        g.Version = 6;
    }
    else if (exprᴛ1 == "7"u8) {
        g.Version = 7;
    }
    else { /* default: */
        Error = fmt.Errorf("invalid GOARM: must start with 5, 6, or 7, and may optionally end in either %q or %q"u8, hardFloatOpt, softFloatOpt);
        g.Version = (nint)((byte)(def[0] - (rune)'0'));
    }

    // 5 defaults to softfloat. 6 and 7 default to hardfloat.
    if (!floatSpecified && g.Version == 5) {
        g.SoftFloat = true;
    }
    return g;
}

[GoType] partial struct Goarm64Features {
    public @string Version;
    // Large Systems Extension
    public bool LSE;
    // ARM v8.0 Cryptographic Extension. It includes the following features:
    // * FEAT_AES, which includes the AESD and AESE instructions.
    // * FEAT_PMULL, which includes the PMULL, PMULL2 instructions.
    // * FEAT_SHA1, which includes the SHA1* instructions.
    // * FEAT_SHA256, which includes the SHA256* instructions.
    public bool Crypto;
}

public static @string String(this Goarm64Features g) {
    @string arm64Str = g.Version;
    if (g.LSE) {
        arm64Str += ",lse"u8;
    }
    if (g.Crypto) {
        arm64Str += ",crypto"u8;
    }
    return arm64Str;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string lseOptᶜ = ",lse"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string cryptoOptᶜ = ",crypto"u8;

public static (Goarm64Features g, error e) ParseGoarm64(@string v) {
    Goarm64Features g = default!;
    error e = default!;

    @string lseOpt = lseOptᶜ;
    @string cryptoOpt = cryptoOptᶜ;
    g.LSE = false;
    g.Crypto = false;
    // We allow any combination of suffixes, in any order
    while (ᐧ) {
        if (strings.HasSuffix(v, lseOpt)) {
            g.LSE = true;
            v = v.slice(0, len(v) - len(lseOpt));
            continue;
        }
        if (strings.HasSuffix(v, cryptoOpt)) {
            g.Crypto = true;
            v = v.slice(0, len(v) - len(cryptoOpt));
            continue;
        }
        break;
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == "v8.0"u8) {
        g.Version = v;
    }
    else if (exprᴛ1 == "v8.1"u8 || exprᴛ1 == "v8.2"u8 || exprᴛ1 == "v8.3"u8 || exprᴛ1 == "v8.4"u8 || exprᴛ1 == "v8.5"u8 || exprᴛ1 == "v8.6"u8 || exprᴛ1 == "v8.7"u8 || exprᴛ1 == "v8.8"u8 || exprᴛ1 == "v8.9"u8 || exprᴛ1 == "v9.0"u8 || exprᴛ1 == "v9.1"u8 || exprᴛ1 == "v9.2"u8 || exprᴛ1 == "v9.3"u8 || exprᴛ1 == "v9.4"u8 || exprᴛ1 == "v9.5"u8) {
        g.Version = v;
        g.LSE = true;
    }
    else { /* default: */
        e = fmt.Errorf("invalid GOARM64: must start with v8.{0-9} or v9.{0-5} and may optionally end in %q and/or %q"u8, // LSE extension is mandatory starting from 8.1

            lseOpt, cryptoOpt);
        g.Version = DefaultGOARM64;
    }

    return (g, e);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goarm64ˢ = "GOARM64"u8;

internal static Goarm64Features /*g*/ goarm64() {
    Goarm64Features g = default!;

    (g, Error) = ParseGoarm64(envOr(goarm64ˢ, DefaultGOARM64));
    return g;
}

// Returns true if g supports giving ARM64 ISA
// Note that this function doesn't accept / test suffixes (like ",lse" or ",crypto")
public static bool Supports(this Goarm64Features g, @string s) {
    // We only accept "v{8-9}.{0-9}. Everything else is malformed.
    if (len(s) != 4) {
        return false;
    }
    var major = s[1];
    var minor = s[3];
    // We only accept "v{8-9}.{0-9}. Everything else is malformed.
    if (major < (rune)'8' || major > (rune)'9' || minor < (rune)'0' || minor > (rune)'9' || s[0] != (rune)'v' || s[2] != (rune)'.') {
        return false;
    }
    var g_major = g.Version[1];
    var g_minor = g.Version[3];
    if (major == g_major){
        return minor <= g_minor;
    } else 
    if (g_major == (rune)'9'){
        // v9.0 diverged from v8.5. This means we should compare with g_minor increased by five.
        return minor <= (byte)(g_minor + 5);
    } else {
        return false;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gomipsˢ = "GOMIPS"u8;

internal static @string gomips() {
    {
        @string v = envOr(gomipsˢ, DefaultGOMIPS);
        var exprᴛ1 = v;
        if (exprᴛ1 == "hardfloat"u8 || exprᴛ1 == "softfloat"u8) {
            return v;
        }
    }

    Error = fmt.Errorf("invalid GOMIPS: must be hardfloat, softfloat"u8);
    return DefaultGOMIPS;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gomips64ˢ = "GOMIPS64"u8;

internal static @string gomips64() {
    {
        @string v = envOr(gomips64ˢ, DefaultGOMIPS64);
        var exprᴛ1 = v;
        if (exprᴛ1 == "hardfloat"u8 || exprᴛ1 == "softfloat"u8) {
            return v;
        }
    }

    Error = fmt.Errorf("invalid GOMIPS64: must be hardfloat, softfloat"u8);
    return DefaultGOMIPS64;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goppc64ˢ = "GOPPC64"u8;

internal static nint goppc64() {
    {
        @string v = envOr(goppc64ˢ, DefaultGOPPC64);
        var exprᴛ1 = v;
        if (exprᴛ1 == "power8"u8) {
            return 8;
        }
        if (exprᴛ1 == "power9"u8) {
            return 9;
        }
        if (exprᴛ1 == "power10"u8) {
            return 10;
        }
    }

    Error = fmt.Errorf("invalid GOPPC64: must be power8, power9, power10"u8);
    return (nint)((byte)(DefaultGOPPC64[len("power")] - (rune)'0'));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goriscv64ˢ = "GORISCV64"u8;

internal static nint goriscv64() {
    {
        @string vΔ1 = envOr(goriscv64ˢ, DefaultGORISCV64);
        var exprᴛ1 = vΔ1;
        if (exprᴛ1 == "rva20u64"u8) {
            return 20;
        }
        if (exprᴛ1 == "rva22u64"u8) {
            return 22;
        }
    }

    Error = fmt.Errorf("invalid GORISCV64: must be rva20u64, rva22u64"u8);
    @string v = DefaultGORISCV64[(int)(len("rva"))..];
    nint i = strings.IndexFunc(v, (rune r) => r < (rune)'0' || r > (rune)'9');
    var (year, _) = strconv.Atoi(v.slice(0, i));
    return year;
}

[GoType] public partial struct gowasmFeatures {
    public bool SatConv;
    public bool SignExt;
}

public static @string String(this gowasmFeatures f) {
    slice<@string> flags = default!;
    if (f.SatConv) {
        flags = append(flags, "satconv"u8);
    }
    if (f.SignExt) {
        flags = append(flags, "signext"u8);
    }
    return strings.Join(flags, ","u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gowasmˢ = "GOWASM"u8;

internal static gowasmFeatures /*f*/ gowasm() {
    gowasmFeatures f = default!;

    foreach (var (_, opt) in strings.Split(envOr(gowasmˢ, ""u8), ","u8)) {
        var exprᴛ1 = opt;
        if (exprᴛ1 == "satconv"u8) {
            f.SatConv = true;
        }
        else if (exprᴛ1 == "signext"u8) {
            f.SignExt = true;
        }
        else if (exprᴛ1 == ""u8) {
        }
        else { /* default: */
            Error = fmt.Errorf("invalid GOWASM: no such feature %q"u8, // ignore
 opt);
        }

    }
    return f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goExtlinkEnabledˢ = "GO_EXTLINK_ENABLED"u8;

public static @string Getgoextlinkenabled() {
    return envOr(goExtlinkEnabledˢ, defaultGO_EXTLINK_ENABLED);
}

internal static slice<@string> toolTags() {
    var tags = experimentTags();
    tags = appendꓸꓸꓸ(tags, gogoarchTags());
    return tags;
}

internal static slice<@string> experimentTags() {
    slice<@string> list = default!;
    // For each experiment that has been enabled in the toolchain, define a
    // build tag with the same name but prefixed by "goexperiment." which can be
    // used for compiling alternative files for the experiment. This allows
    // changes for the experiment, like extra struct fields in the runtime,
    // without affecting the base non-experiment code at all.
    foreach (var (_, exp) in ᏑExperiment.Enabled()) {
        list = append(list, "goexperiment."u8 + exp);
    }
    return list;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string go386ˢ = "GO386"u8;

// GOGOARCH returns the name and value of the GO$GOARCH setting.
// For example, if GOARCH is "amd64" it might return "GOAMD64", "v2".
public static (@string name, @string value) GOGOARCH() {
    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "386"u8) {
        return (go386ˢ, GO386);
    }
    if (exprᴛ1 == "amd64"u8) {
        return (goamd64ˢ, fmt.Sprintf("v%d"u8, GOAMD64));
    }
    if (exprᴛ1 == "arm"u8) {
        return (goarmˢ, GOARM.String());
    }
    if (exprᴛ1 == "arm64"u8) {
        return (goarm64ˢ, GOARM64.String());
    }
    if (exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8) {
        return (gomipsˢ, GOMIPS);
    }
    if (exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8) {
        return (gomips64ˢ, GOMIPS64);
    }
    if (exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8) {
        return (goppc64ˢ, fmt.Sprintf("power%d"u8, GOPPC64));
    }
    if (exprᴛ1 == "wasm"u8) {
        return (gowasmˢ, GOWASM.String());
    }

    return ("", "");
}

internal static slice<@string> gogoarchTags() {
    var exprᴛ1 = GOARCH;
    if (exprᴛ1 == "386"u8) {
        return new @string[]{GOARCH + "."u8 + GO386}.slice();
    }
    if (exprᴛ1 == "amd64"u8) {
        slice<@string> list = default!;
        for (nint i = 1; i <= GOAMD64; i++) {
            list = append(list, fmt.Sprintf("%s.v%d"u8, GOARCH, i));
        }
        return list;
    }
    if (exprᴛ1 == "arm"u8) {
        slice<@string> list = default!;
        for (nint i = 5; i <= GOARM.Version; i++) {
            list = append(list, fmt.Sprintf("%s.%d"u8, GOARCH, i));
        }
        return list;
    }
    if (exprᴛ1 == "arm64"u8) {
        slice<@string> list = default!;
        nint major = (nint)((byte)(GOARM64.Version[1] - (rune)'0'));
        nint minor = (nint)((byte)(GOARM64.Version[3] - (rune)'0'));
        for (nint i = 0; i <= minor; i++) {
            list = append(list, fmt.Sprintf("%s.v%d.%d"u8, GOARCH, major, i));
        }
        if (major == 9) {
            // ARM64 v9.x also includes support of v8.x+5 (i.e. v9.1 includes v8.(1+5) = v8.6).
            for (nint i = 0; i <= minor + 5 && i <= 9; i++) {
                list = append(list, fmt.Sprintf("%s.v%d.%d"u8, GOARCH, (nint)(8), i));
            }
        }
        return list;
    }
    if (exprᴛ1 == "mips"u8 || exprᴛ1 == "mipsle"u8) {
        return new @string[]{GOARCH + "."u8 + GOMIPS}.slice();
    }
    if (exprᴛ1 == "mips64"u8 || exprᴛ1 == "mips64le"u8) {
        return new @string[]{GOARCH + "."u8 + GOMIPS64}.slice();
    }
    if (exprᴛ1 == "ppc64"u8 || exprᴛ1 == "ppc64le"u8) {
        slice<@string> list = default!;
        for (nint i = 8; i <= GOPPC64; i++) {
            list = append(list, fmt.Sprintf("%s.power%d"u8, GOARCH, i));
        }
        return list;
    }
    if (exprᴛ1 == "riscv64"u8) {
        var list = new @string[]{GOARCH + "."u8 + "rva20u64"u8}.slice();
        if (GORISCV64 >= 22) {
            list = append(list, GOARCH + "."u8 + "rva22u64"u8);
        }
        return list;
    }
    if (exprᴛ1 == "wasm"u8) {
        slice<@string> list = default!;
        if (GOWASM.SatConv) {
            list = append(list, GOARCH + ".satconv"u8);
        }
        if (GOWASM.SignExt) {
            list = append(list, GOARCH + ".signext"u8);
        }
        return list;
    }

    return default!;
}

} // end buildcfg_package
