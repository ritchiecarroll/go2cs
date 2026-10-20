namespace go;

using fmt = fmt_package;
using os = os_package;
using sort = sort_package;
using strings = strings_package;
using syscall = syscall_package;

partial class main_package {


partial struct sentinelsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string value;
}
internal static slice<sentinelsᴛ1> sentinels = new sentinelsᴛ1[]{
    new("GO2CS_ENVPROBE_A_PLAIN", "plain-value"),
    new("GO2CS_ENVPROBE_B_EQUALS", "k1=v1=v2"),
    new("GO2CS_ENVPROBE_C_EMPTY", ""),
    new("GO2CS_ENVPROBE_D_LONG", strings.Repeat("abcdefghij", 40)),
    new("GO2CS_ENVPROBE_E_UNICODE", "über-日本語-\U0001f600")
}.slice();

internal static readonly @string prefix = "GO2CS_ENVPROBE_"u8;

internal static readonly @string hiddenName = "=GO2CSENVPROBEHIDDEN"u8;

internal static void Main() {
    var hiddenSet = os.Setenv(hiddenName, "hid") == default!;

    var before = syscall.Environ();
    if (len(before) == 0) {
        fmt.Println("FATAL empty environment");
        os.Exit(1);
    }

    foreach (var (_, s) in sentinels) {
        {
            var err = os.Setenv(s.name, s.value); if (err != default!) {
                fmt.Println("FATAL Setenv", s.name, err);
                os.Exit(1);
            }
        }
    }

    var after = syscall.Environ();

    fmt.Printf("delta %d\n"u8, len(after) - len(before));

    fmt.Println("-- sentinels via syscall.Environ --");
    foreach (var (_, e) in collect(after)) {
        fmt.Println(ascii(e));
    }

    fmt.Println("-- sentinels via os.Environ --");
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
            fmt.Println("FATAL Unsetenv", err);
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
        nint i = strings.Index(e[1..], "=");
        if (i < 0) {
            return false;
        }
        @string name = e.slice(0, i + 1);
        if (strings.HasPrefix(name, "=")) {
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
        if (strings.HasPrefix(e, "=")) {
            return true;
        }
    }
    return false;
}

internal static (@string, @string) malformed(slice<@string> env) {
    foreach (var (_, e) in env) {
        if (e == ""u8) {
            return (e, "empty");
        }
        if (strings.ContainsRune(e, 0)) {
            return (e, "embedded-nul");
        }
        if (strings.Index(e[1..], "=") < 0) {
            return (e, "no-separator");
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

internal static @string ascii(@string s) {
    @string hexDigits = "0123456789abcdef";

    var @out = new slice<byte>(0, len(s));
    foreach (var (_, r) in s) {
        if (r >= 0x20 && r < 0x7f) {
            @out = append(@out, (byte)r);
            continue;
        }
        @out = append(@out, (byte)((rune)'\\'), (byte)((rune)'u'));
        for (nint shift = 20; shift >= 0; shift -= 4) {
            @out = append(@out, hexDigits[((r.Rsh((nuint)shift)) & 0xf)]);
        }
    }
    return (@string)@out;
}

} // end main_package
