namespace go;

using fmt = fmt_package;
using time = time_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static nint x = 1;

internal static int32 getNext() {
    x++;
    return (int32)x;
}

internal static @string getStr(@string test) {
    return "string"u8 + test;
}

internal static @string getStr2(any test1, @string test2) {
    return test1._<@string>() + test2;
}

internal static @string getStr3(@string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return fmt.Sprintf(format, a.ꓸꓸꓸ);
}

public static nint Foo(nint n) {
    fmt.Println(n);
    return n;
}

internal static void Main() {
    fmt.Println(getStr("test"));
    fmt.Println(getStr2("hello, ", "world"));
    fmt.Println(getStr3("hello, %s", "world"));

    nint i = 2;
    fmt.Print("Write ", i, " as ");
    switch (i) {
    case 1: {
        fmt.Println("one");
        break;
    }
    case 2: {
        fmt.Println("two");
        break;
    }
    case 3: {
        {
            fmt.Println("three");
        }
        break;
    }
    case 4 or 5 or 6: {
        fmt.Println("four, five or siz");
        break;
    }
    default: {
        fmt.Println("unknown");
        break;
    }}

    nint x = 5;
    fmt.Println(x);

    {
        nint xΔ1 = 6;
        fmt.Println(xΔ1);
    }

    fmt.Println(x);

    switch (time.Now().Weekday()) {
    case var ᴛ when ᴛ == time.Saturday || ᴛ == time.Sunday: {
        fmt.Println("It's the weekend");
        break;
    }
    case var ᴛ when ᴛ == time.Monday: {
        fmt.Println("Ugh, it's Monday");
        break;
    }
    default: {
        fmt.Println("It's a weekday");
        break;
    }}

    var t = time.Now();
    switch (ᐧ) {
    case {} when t.Hour() is < 12: {
        fmt.Println("It's before noon");
        break;
    }
    default: {
        fmt.Println("It's after noon");
        break;
    }}

    nint hour = 1;
    nint hour1 = time.Now().Hour();

    {
        nint hourΔ1 = time.Now().Hour();
        switch (ᐧ) {
        case {} when hourΔ1 is 1 or < 12 or 2: {
            fmt.Println("Good morning!");
            break;
        }
        case {} when (hourΔ1 == 1) || (hourΔ1 < 12) || (hourΔ1 == 2 || hour1 == 4): {
            fmt.Println("Good morning (opt 2)!");
            break;
        }
        case {} when hourΔ1 is < 17: {
            fmt.Println("Good afternoon!");
            break;
        }
        case {} when hourΔ1 is 0: {
            fmt.Println("Midnight!");
            break;
        }
        case {} when hourΔ1 == 0 && hour1 == 1: {
            fmt.Println("Midnight (opt 2)!");
            break;
        }
        default: {
            fmt.Println("Good evening!");
            break;
        }}
    }

    fmt.Println(hour);

    var c = (rune)'\r';

    switch (c) {
    case (rune)' ' or (rune)'\t' or (rune)'\n' or (rune)'\f' or (rune)'\r': {
        fmt.Println("whitespace");
        break;
    }}

    fmt.Printf("i before = %d\n"u8, i);

    {
        nint iΔ1 = 1;
        var exprᴛ2 = getNext();
        var matchᴛ1 = false;
        if (exprᴛ2 == -1) { matchᴛ1 = true;
            fmt.Println("negative");
        }
        else if (exprᴛ2 is 0) { matchᴛ1 = true;
            fmt.Println("zero");
        }
        else if (exprᴛ2 is 1 or 2) { matchᴛ1 = true;
            fmt.Println("one or two");
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ2 is 3) { matchᴛ1 = true;
            fmt.Printf("three, but x=%d "u8, x);
            fmt.Printf("and i now = %d\n"u8, iΔ1);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

    fmt.Printf("i after = %d\n"u8, i);

    {
        var next = getNext();
        var matchᴛ2 = false;
        if (next <= -1) { matchᴛ2 = true;
            fmt.Println("negative");

            var exprᴛ4 = getNext();
            var matchᴛ3 = false;
            if (exprᴛ4 is 1 or 2) { matchᴛ3 = true;
                fmt.Println("sub0 one or two");
            }
            else if (exprᴛ4 is 3) { matchᴛ3 = true;
                fmt.Println("sub0 three");
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ3) { /* default: */
                fmt.Println("sub0 default");
            }

        }
        else if (next is 0) { matchᴛ2 = true;
            fmt.Println("zero");

            {
                var nextΔ2 = getNext();
                var matchᴛ4 = false;
                if (nextΔ2 is 1 or <= 2) { matchᴛ4 = true;
                    fmt.Println("sub1 one or two");
                }
                else if (nextΔ2 is 3) { matchᴛ4 = true;
                    fmt.Println("sub1 three");
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ4) { /* default: */
                    fmt.Println("sub1 default");
                }
            }

        }
        else if (next is 1 or 2) { matchᴛ2 = true;
            fmt.Println("one or two");

            switch (next) {
            case 1 or 2: {
                fmt.Println("sub2 one or two");
                break;
            }
            case 3: {
                fmt.Println("sub2 three");
                break;
            }
            default: {
                fmt.Println("sub2 default");
                break;
            }}

            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && (next >= 3 && next < 100)) { matchᴛ2 = true;
            fmt.Printf("three or greater < 100: %d\n"u8, x);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2) { /* default: */
            fmt.Println("plus, always a default here because of fallthrough");
        }
    }

    var exprᴛ6 = Foo(2);
    var matchᴛ5 = false;
    if (exprᴛ6 == Foo(1) || exprᴛ6 == Foo(2) || exprᴛ6 == Foo(3)) { matchᴛ5 = true;
        fmt.Println("First case");
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ5 && exprᴛ6 == Foo(4)) {
        fmt.Println("Second case");
    }
    else if (!matchᴛ5) { /* default: */
        fmt.Println("Default case");
    }

    nint v = 3;
    switch (ᐧ) {
    case {} when ᐧᐧ: {
        fmt.Println("strict checks disabled");
        break;
    }
    case {} when v is > 2: {
        fmt.Println("unreachable but compiled");
        break;
    }}

    pace dur = 5;
    switch (ᐧ) {
    case {} when dur >= 6: {
        fmt.Println("fast");
        break;
    }
    case {} when dur == 5: {
        fmt.Println("steady");
        break;
    }
    default: {
        fmt.Println("slow");
        break;
    }}

}

partial struct pace /*num:int64*/;

} // end main_package
