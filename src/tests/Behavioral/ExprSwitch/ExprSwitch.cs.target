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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string testˢ = "test"u8;
private static readonly object helloˢ = (@string)"hello, "u8;
private static readonly @string worldˢ = "world"u8;
private static readonly @string helloSˢ = "hello, %s"u8;
private static readonly object writeˢ = (@string)"Write "u8;
private static readonly object oneˢ = (@string)"one"u8;
private static readonly object twoˢ = (@string)"two"u8;
private static readonly object threeˢ = (@string)"three"u8;
private static readonly object fourFiveOrSizˢ = (@string)"four, five or siz"u8;
private static readonly object unknownˢ = (@string)"unknown"u8;
private static readonly object itSTheWeekendˢ = (@string)"It's the weekend"u8;
private static readonly object ughItSMondayˢ = (@string)"Ugh, it's Monday"u8;
private static readonly object itSAWeekdayˢ = (@string)"It's a weekday"u8;
private static readonly object itSBeforeNoonˢ = (@string)"It's before noon"u8;
private static readonly object itSAfterNoonˢ = (@string)"It's after noon"u8;
private static readonly object goodMorningˢ = (@string)"Good morning!"u8;
private static readonly object goodMorningOpt2ˢ = (@string)"Good morning (opt 2)!"u8;
private static readonly object goodAfternoonˢ = (@string)"Good afternoon!"u8;
private static readonly object midnightˢ = (@string)"Midnight!"u8;
private static readonly object midnightOpt2ˢ = (@string)"Midnight (opt 2)!"u8;
private static readonly object goodEveningˢ = (@string)"Good evening!"u8;
private static readonly object whitespaceˢ = (@string)"whitespace"u8;
private static readonly object negativeˢ = (@string)"negative"u8;
private static readonly object zeroˢ = (@string)"zero"u8;
private static readonly object oneOrTwoˢ = (@string)"one or two"u8;
private static readonly object plusAlwaysADefaultHereˢ = (@string)"plus, always a default here because of fallthrough"u8;
private static readonly object sub0OneOrTwoˢ = (@string)"sub0 one or two"u8;
private static readonly object sub0Threeˢ = (@string)"sub0 three"u8;
private static readonly object sub0Defaultˢ = (@string)"sub0 default"u8;
private static readonly object sub1OneOrTwoˢ = (@string)"sub1 one or two"u8;
private static readonly object sub1Threeˢ = (@string)"sub1 three"u8;
private static readonly object sub1Defaultˢ = (@string)"sub1 default"u8;
private static readonly object sub2OneOrTwoˢ = (@string)"sub2 one or two"u8;
private static readonly object sub2Threeˢ = (@string)"sub2 three"u8;
private static readonly object sub2Defaultˢ = (@string)"sub2 default"u8;
private static readonly object firstCaseˢ = (@string)"First case"u8;
private static readonly object secondCaseˢ = (@string)"Second case"u8;
private static readonly object defaultCaseˢ = (@string)"Default case"u8;
private static readonly object strictChecksDisabledˢ = (@string)"strict checks disabled"u8;
private static readonly object unreachableButCompiledˢ = (@string)"unreachable but compiled"u8;
private static readonly object fastˢ = (@string)"fast"u8;
private static readonly object steadyˢ = (@string)"steady"u8;
private static readonly object slowˢ = (@string)"slow"u8;

internal static void Main() {
    fmt.Println(getStr(testˢ));
    fmt.Println(getStr2(helloˢ, worldˢ));
    fmt.Println(getStr3(helloSˢ, worldˢ));
    nint i = 2;
    fmt.Print(writeˢ, i, (@string)" as "u8);
    switch (i) {
    case 1: {
        fmt.Println(oneˢ);
        break;
    }
    case 2: {
        fmt.Println(twoˢ);
        break;
    }
    case 3: {
        {
            fmt.Println(threeˢ);
        }
        break;
    }
    case 4 or 5 or 6: {
        fmt.Println(fourFiveOrSizˢ);
        break;
    }
    default: {
        fmt.Println(unknownˢ);
        break;
    }}

    nint x = 5;
    fmt.Println(x);
    {
        nint xΔ1 = 6;
        fmt.Println(xΔ1);
    }
    fmt.Println(x);
    var exprᴛ1 = time.Now().Weekday();
    if (exprᴛ1 == time.Saturday || exprᴛ1 == time.Sunday) {
        fmt.Println(itSTheWeekendˢ);
    }
    else if (exprᴛ1 == time.Monday) {
        fmt.Println(ughItSMondayˢ);
    }
    else { /* default: */
        fmt.Println(itSAWeekdayˢ);
    }

    var t = time.Now();
    switch (ᐧ) {
    case {} when t.Hour() is < 12: {
        fmt.Println(itSBeforeNoonˢ);
        break;
    }
    default: {
        fmt.Println(itSAfterNoonˢ);
        break;
    }}

    nint hour = 1;
    nint hour1 = time.Now().Hour();
    {
        nint hourΔ1 = time.Now().Hour();
        switch (ᐧ) {
        case {} when hourΔ1 is 1 or < 12 or 2: {
            fmt.Println(goodMorningˢ);
            break;
        }
        case {} when (hourΔ1 == 1) || (hourΔ1 < 12) || (hourΔ1 == 2 || hour1 == 4): {
            fmt.Println(goodMorningOpt2ˢ);
            break;
        }
        case {} when hourΔ1 is < 17: {
            fmt.Println(goodAfternoonˢ);
            break;
        }
        case {} when hourΔ1 is 0: {
            fmt.Println(midnightˢ);
            break;
        }
        case {} when hourΔ1 == 0 && hour1 == 1: {
            fmt.Println(midnightOpt2ˢ);
            break;
        }
        default: {
            fmt.Println(goodEveningˢ);
            break;
        }}
    }

    fmt.Println(hour);
    var c = (rune)'\r';
    switch (c) {
    case (rune)' ' or (rune)'\t' or (rune)'\n' or (rune)'\f' or (rune)'\r': {
        fmt.Println(whitespaceˢ);
        break;
    }}

    fmt.Printf("i before = %d\n"u8, i);
    {
        nint iΔ1 = 1;
        var exprᴛ2 = getNext();
        var matchᴛ1 = false;
        if (exprᴛ2 == -1) { matchᴛ1 = true;
            fmt.Println(negativeˢ);
        }
        else if (exprᴛ2 is 0) { matchᴛ1 = true;
            fmt.Println(zeroˢ);
        }
        else if (exprᴛ2 is 1 or 2) { matchᴛ1 = true;
            fmt.Println(oneOrTwoˢ);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ2 is 3) { matchᴛ1 = true;
            fmt.Printf("three, but x=%d "u8, x);
            fmt.Printf("and i now = %d\n"u8, iΔ1);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            fmt.Println(plusAlwaysADefaultHereˢ);
        }
    }

    fmt.Printf("i after = %d\n"u8, i);
    {
        var next = getNext();
        var matchᴛ2 = false;
        if (next <= -1) { matchᴛ2 = true;
            fmt.Println(negativeˢ);
            var exprᴛ4 = getNext();
            var matchᴛ3 = false;
            if (exprᴛ4 is 1 or 2) { matchᴛ3 = true;
                fmt.Println(sub0OneOrTwoˢ);
            }
            else if (exprᴛ4 is 3) { matchᴛ3 = true;
                fmt.Println(sub0Threeˢ);
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ3) { /* default: */
                fmt.Println(sub0Defaultˢ);
            }

        }
        else if (next is 0) { matchᴛ2 = true;
            fmt.Println(zeroˢ);
            {
                var nextΔ2 = getNext();
                var matchᴛ4 = false;
                if (nextΔ2 is 1 or <= 2) { matchᴛ4 = true;
                    fmt.Println(sub1OneOrTwoˢ);
                }
                else if (nextΔ2 is 3) { matchᴛ4 = true;
                    fmt.Println(sub1Threeˢ);
                    fallthrough = true;
                }
                if (fallthrough || !matchᴛ4) { /* default: */
                    fmt.Println(sub1Defaultˢ);
                }
            }

        }
        else if (next is 1 or 2) { matchᴛ2 = true;
            fmt.Println(oneOrTwoˢ);
            switch (next) {
            case 1 or 2: {
                fmt.Println(sub2OneOrTwoˢ);
                break;
            }
            case 3: {
                fmt.Println(sub2Threeˢ);
                break;
            }
            default: {
                fmt.Println(sub2Defaultˢ);
                break;
            }}

            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2 && (next >= 3 && next < 100)) { matchᴛ2 = true;
            fmt.Printf("three or greater < 100: %d\n"u8, x);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ2) { /* default: */
            fmt.Println(plusAlwaysADefaultHereˢ);
        }
    }

    var exprᴛ6 = Foo(2);
    var matchᴛ5 = false;
    if (exprᴛ6 == Foo(1) || exprᴛ6 == Foo(2) || exprᴛ6 == Foo(3)) { matchᴛ5 = true;
        fmt.Println(firstCaseˢ);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ5 && exprᴛ6 == Foo(4)) {
        fmt.Println(secondCaseˢ);
    }
    else if (!matchᴛ5) { /* default: */
        fmt.Println(defaultCaseˢ);
    }

    nint v = 3;
    switch (ᐧ) {
    case {} when ᐧᐧ: {
        fmt.Println(strictChecksDisabledˢ);
        break;
    }
    case {} when v is > 2: {
        fmt.Println(unreachableButCompiledˢ);
        break;
    }}

    pace dur = 5;
    switch (ᐧ) {
    case {} when dur >= 6: {
        fmt.Println(fastˢ);
        break;
    }
    case {} when dur == 5: {
        fmt.Println(steadyˢ);
        break;
    }
    default: {
        fmt.Println(slowˢ);
        break;
    }}

}

partial struct pace /*num:int64*/;

} // end main_package
