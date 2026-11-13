namespace go;

using fmt = fmt_package;

partial class main_package {

internal static @string bomKind(@string s) {
    array<byte> bom = new(2);
    if (len(s) >= 2) {
        bom = array<byte>([s[0], s[1]]);
        switch (bom) {
        case var ᴛ when ᴛ == array<byte>([0xFF, 0xFE]): {
            return "le";
        }
        case var ᴛ when ᴛ == array<byte>([0xFE, 0xFF]): {
            return "be";
        }}

    }
    return "none";
}

internal static @string pair(/*[2]*/ array<byte> b) {
    b = b.Clone();

    switch (b) {
    case var ᴛ when ᴛ == array<byte>([1, 2]) || ᴛ == array<byte>([3, 4]): {
        return "low";
    }
    case var ᴛ when ᴛ == array<byte>([9, 9]): {
        return "nines";
    }
    default: {
        return "other";
    }}

}

partial struct point {
    public nint X, Y;
}

internal static @string where(point p) {
    switch (p) {
    case var ᴛ when ᴛ == new point(0, 0): {
        return "origin";
    }
    case var ᴛ when ᴛ == new point(1, 0) || ᴛ == new point(0, 1): {
        return "unit";
    }}

    return "elsewhere";
}

internal static @string sum(nint n, nint a, nint b) {
    switch (n) {
    case var ᴛ when ᴛ == a + b: {
        return "sum";
    }
    case var ᴛ when ᴛ == a * b: {
        return "product";
    }}

    return "neither";
}

internal static slice<@string> through(/*[2]*/ array<byte> b) {
    b = b.Clone();

    slice<@string> @out = default;
    var exprᴛ1 = b;
    var matchᴛ1 = false;
    if (exprᴛ1 == array<byte>([1, 1])) { matchᴛ1 = true;
        @out = append(@out, (@string)"one");
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == array<byte>([2, 2])) {
        @out = append(@out, (@string)"two");
    }
    else if (exprᴛ1 == array<byte>([3, 3])) { matchᴛ1 = true;
        @out = append(@out, (@string)"three");
    }

    return @out;
}

internal static slice<nint> trace;

internal static array<byte> at(nint i, /*[2]*/ array<byte> v) {
    v = v.Clone();

    trace = append(trace, i);
    return v.Clone();
}

internal static @string order(/*[2]*/ array<byte> b) {
    b = b.Clone();

    trace = default;
    switch (b) {
    case var ᴛ when ᴛ == at(1, array<byte>([7, 7])) || ᴛ == at(2, array<byte>([8, 8])): {
        return fmt.Sprint("first ", trace);
    }
    case var ᴛ when ᴛ == at(3, array<byte>([9, 9])): {
        return fmt.Sprint("second ", trace);
    }}

    return fmt.Sprint("none ", trace);
}

internal static @string digit(nint n) {
    switch (n) {
    case 1: {
        return "one";
    }
    case 2 or 3: {
        return "few";
    }}

    return "many";
}

internal static void main() {
    fmt.Println(bomKind(((@string)(new byte[]{0xff, 0xfe, 0x61, 0x62}))), bomKind(((@string)(new byte[]{0xfe, 0xff, 0x61, 0x62}))), bomKind("ab"), bomKind("a"));
    fmt.Println(pair(array<byte>([1, 2])), pair(array<byte>([3, 4])), pair(array<byte>([9, 9])), pair(array<byte>([5, 5])));
    fmt.Println(where(new point()), where(new point(0, 1)), where(new point(2, 2)));
    fmt.Println(sum(5, 2, 3), sum(6, 2, 3), sum(7, 2, 3));
    fmt.Println(through(array<byte>([1, 1])), through(array<byte>([2, 2])), through(array<byte>([3, 3])), len(through(new byte[]{}.array(2))));
    fmt.Println(order(array<byte>([8, 8])), order(array<byte>([9, 9])), order(array<byte>([0, 0])));
    fmt.Println(digit(1), digit(3), digit(9));
}

} // end main_package
