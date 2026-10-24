namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string single;
internal static error _ᴛ1ʗ;
internal static void initᴛsingle() { single = makeGreeting().Item1; }

internal static @string cwd;
internal static error cwdErr;
internal static void initᴛcwd() { var tupleᴛ1ʗ = fakeGetwd(); cwd = tupleᴛ1ʗ.Item1; cwdErr = tupleᴛ1ʗ.Item2; }

internal static @string head;
internal static nint _ᴛ2ʗ;
internal static @string tail;
internal static void initᴛhead() { var tupleᴛ2ʗ = makeTrio(); head = tupleᴛ2ʗ.Item1; tail = tupleᴛ2ʗ.Item3; }

internal static @string chained;
internal static void initᴛchained() { chained = single + "?"u8; }

internal static ж<@string> Ꮡboxed = new StaticBox<@string>(default(@string));
internal static ref @string boxed => ref Ꮡboxed.Value;
internal static error _ᴛ3ʗ;
internal static void initᴛboxed() { boxed = makeGreeting().Item1; }

internal static @string prefix = "go";

internal static @string suffix = "2cs";

internal static (nint, nint) tupleᴛ3ʗ = constantPair();
internal static nint safeA = tupleᴛ3ʗ.Item1;
internal static nint safeB = tupleᴛ3ʗ.Item2;

internal static (@string, error) makeGreeting() {
    return (prefix + "-" + suffix, default!);
}

internal static (@string, error) fakeGetwd() {
    return ("/" + prefix + "/" + suffix, default!);
}

internal static (@string, nint, @string) makeTrio() {
    return (prefix + "<", len(prefix), ">" + suffix);
}

internal static (nint, nint) constantPair() {
    return (10, 20);
}

internal static @string readThrough(ref @string p) {
    return p;
}

internal static void Main() {
    fmt.Println(single);
    fmt.Println(cwd, cwdErr);
    fmt.Println(head, tail);
    fmt.Println(chained);
    fmt.Println(readThrough(ref boxed));
    fmt.Println(safeA, safeB);
}

} // end main_package
