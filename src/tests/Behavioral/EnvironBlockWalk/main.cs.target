namespace go;

using fmt = fmt_package;
using os = os_package;
using sort = sort_package;
using strings = strings_package;
using syscall = syscall_package;

partial class main_package {


[GoType("dyn")] partial struct sentinelsᴛ1 {
    internal @string name;
    internal @string value;
}
internal static slice<sentinelsᴛ1> sentinels = new sentinelsᴛ1[]{
    new("GO2CS_ENVPROBE_A_PLAIN"u8, "plain-value"u8),
    new("GO2CS_ENVPROBE_B_EQUALS"u8, "k1=v1=v2"u8),
    new("GO2CS_ENVPROBE_C_EMPTY"u8, ""u8),
    new("GO2CS_ENVPROBE_D_LONG"u8, strings.Repeat("abcdefghij"u8, 40)),
    new("GO2CS_ENVPROBE_E_UNICODE"u8, "über-日本語-\U0001f600"u8)
}.slice();

internal static readonly @string prefix = "GO2CS_ENVPROBE_"u8;

internal static readonly @string hiddenName = "=GO2CSENVPROBEHIDDEN"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string hidˢ = "hid"u8;
private static readonly object fatalEmptyEnvironmentˢ = (@string)"FATAL empty environment"u8;
private static readonly object fatalSetenvˢ = (@string)"FATAL Setenv"u8;
private static readonly object sentinelsViaSyscallˢ = (@string)"-- sentinels via syscall.Environ --"u8;
private static readonly object sentinelsViaOsEnvironˢ = (@string)"-- sentinels via os.Environ --"u8;
private static readonly object fatalUnsetenvˢ = (@string)"FATAL Unsetenv"u8;

internal static void Main() {
    var hiddenSet = os.Setenv(hiddenName, hidˢ) == default!;
    var before = syscall.Environ();
    if (len(before) == 0) {
        fmt.Println(fatalEmptyEnvironmentˢ);
        os.Exit(1);
    }
    foreach (var (_, s) in sentinels) {
        {
            var err = os.Setenv(s.name, s.value); if (err != default!) {
                fmt.Println(fatalSetenvˢ, s.name, err);
                os.Exit(1);
            }
        }
    }
    var after = syscall.Environ();
    fmt.Printf("delta %d\n"u8, len(after) - len(before));
    fmt.Println(sentinelsViaSyscallˢ);
    foreach (var (_, e) in collect(after)) {
        fmt.Println(ascii(e));
    }
    fmt.Println(sentinelsViaOsEnvironˢ);
    foreach (var (_, e) in collect(os.Environ())) {
        fmt.Println(ascii(e));
    }
    fmt.Printf("getenv-agrees %v\n"u8, getenvAgrees(after));
    fmt.Printf("hidden %v %v %v\n"u8, hiddenSet, contains(after, hiddenName + "="), hasHidden(after));
    var (bad, why) = malformed(after);
    fmt.Printf("well-formed %v %s\n"u8, bad == ""u8, why);
    var stable = true;
    for (nint i = 0; i < 50; i++) {
        if (len(syscall.Environ()) != len(after)) {
            stable = false;
            break;
        }
    }
    fmt.Printf("stable %v\n"u8, stable);
    {
        var err = os.Unsetenv(sentinels[0].name); if (err != default!) {
            fmt.Println(fatalUnsetenvˢ, err);
            os.Exit(1);
        }
    }
    var unset = syscall.Environ();
    fmt.Printf("unset %d %v\n"u8, len(after) - len(unset), contains(unset, sentinels[0].name + "="u8));
}

internal static slice<@string> collect(slice<@string> env) {
    slice<@string> @out = default!;
    foreach (var (_, e) in env) {
        if (strings.HasPrefix(e, prefix)) {
            @out = append(@out, e);
        }
    }
    sort.Strings(@out);
    return @out;
}

internal static bool getenvAgrees(slice<@string> env) {
    foreach (var (_, e) in env) {
        nint i = strings.Index(e[1..], "="u8);
        if (i < 0) {
            return false;
        }
        @string name = e.slice(0, i + 1);
        if (strings.HasPrefix(name, "="u8)) {
            continue;
        }
        {
            var (got, ok) = syscall.Getenv(name); if (!ok || got != e.slice(i + 2)) {
                return false;
            }
        }
    }
    return true;
}

internal static bool hasHidden(slice<@string> env) {
    foreach (var (_, e) in env) {
        if (strings.HasPrefix(e, "="u8)) {
            return true;
        }
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string emptyˢ = "empty"u8;
private static readonly @string embeddedNulˢ = "embedded-nul"u8;
private static readonly @string noSeparatorˢ = "no-separator"u8;

internal static (@string, @string) malformed(slice<@string> env) {
    foreach (var (_, e) in env) {
        if (e == ""u8) {
            return (e, emptyˢ);
        }
        if (strings.ContainsRune(e, 0)) {
            return (e, embeddedNulˢ);
        }
        if (strings.Index(e[1..], "="u8) < 0) {
            return (e, noSeparatorˢ);
        }
    }
    return ("", "ok");
}

internal static bool contains(slice<@string> env, @string p) {
    foreach (var (_, e) in env) {
        if (strings.HasPrefix(e, p)) {
            return true;
        }
    }
    return false;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string hexDigitsᶜ = "0123456789abcdef"u8;

internal static @string ascii(@string s) {
    @string hexDigits = hexDigitsᶜ;
    var @out = new slice<byte>(0, len(s));
    foreach (var (_, r) in s) {
        if (r >= 0x20 && r < 0x7f) {
            @out = append(@out, (byte)r);
            continue;
        }
        @out = append(@out, (byte)((rune)'\\'), (byte)((rune)'u'));
        for (nint shift = 20; shift >= 0; shift -= 4) {
            @out = append(@out, hexDigits[(rune)((r.Rsh((nuint)shift)) & 0xf)]);
        }
    }
    return ((@string)@out);
}

} // end main_package
