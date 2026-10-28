namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct bucketType /*num:nint*/;

internal static bucketType memProfile => /* iota */ 0;
internal static bucketType blockProfile => 1;
internal static bucketType mutexProfile => 2;

internal static @string size(bucketType typ) {
    @string s = default;
    switch (typ) {
    default: {
        s = "invalid";
        break;
    }
    case var ᴛ when ᴛ == memProfile: {
        s = "mem";
        break;
    }
    case var ᴛ when ᴛ == blockProfile || ᴛ == mutexProfile: {
        s = "block-or-mutex";
        break;
    }}

    return s;
}

internal static nint mid(bucketType typ) {
    switch (typ) {
    case var ᴛ when ᴛ == memProfile: {
        return 1;
    }
    default: {
        return -1;
    }
    case var ᴛ when ᴛ == blockProfile: {
        return 2;
    }}

}

internal static nint withFallthrough(bucketType typ) {
    nint n = 0;
    var exprᴛ1 = typ;
    var matchᴛ1 = false;
    if (exprᴛ1 == memProfile) { matchᴛ1 = true;
        n = 1;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == blockProfile) { matchᴛ1 = true;
        n += 10;
    }
    else if (exprᴛ1 == mutexProfile) {
        n = 100;
    }
    else if (!matchᴛ1) { /* default: */
        return -1;
    }

    return n;
}

internal static void Main() {
    fmt.Println(size(memProfile));
    fmt.Println(size(blockProfile));
    fmt.Println(size(mutexProfile));
    fmt.Println(size(99));
    fmt.Println(mid(memProfile), mid(blockProfile), mid(99));
    fmt.Println(withFallthrough(memProfile), withFallthrough(blockProfile), withFallthrough(mutexProfile), withFallthrough(99));
}

} // end main_package
