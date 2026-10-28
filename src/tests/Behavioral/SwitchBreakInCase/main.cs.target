namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedInt stateA => iota;
internal static UntypedInt stateB => 1;
internal static UntypedInt stateC => 2;

internal static @string classify(nint n, bool flag) {
    @string result = "none";
    switch (n) {
    case var ᴛ when ᴛ == stateA: {
        if (flag) {
            break;
        }
        result = "a";
        break;
    }
    case var ᴛ when ᴛ == stateB: {
        result = "b";
        if (flag) {
            break;
        }
        result = "b-noflag";
        break;
    }
    default: {
        result = "other";
        break;
    }}

    return result;
}

internal static @string pick(nint x, nint y) {
    @string @out = "none";
Big:
    switch (x) {
    case 1: {
        switch (y) {
        case 2: {
            @out = "one-two";
            goto break_Big;
            break;
        }}

        @out = "one-other";
        break;
    }
    case 3: {
        @out = "three";
        break;
    }}

    break_Big:;
    return @out;
}

internal static void Main() {
    fmt.Println(pick(1, 2), pick(1, 5), pick(3, 0));
    fmt.Println(classify(stateA, true));
    fmt.Println(classify(stateA, false));
    fmt.Println(classify(stateB, true));
    fmt.Println(classify(stateB, false));
    fmt.Println(classify(stateC, false));
}

} // end main_package
