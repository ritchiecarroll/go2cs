namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct IntSlice /*[]nint*/;

internal static void g1(channel<nint> ch) {
    ch.ᐸꟷ(12);
}

internal static void g2(channel<nint> ch) {
    ch.ᐸꟷ(32);
}

internal static void sum(slice<nint> s, channel<nint> c) {
    nint sum = 0;
    foreach (var (_, v) in s) {
        sum += v;
    }
    c.ᐸꟷ(sum);
}

internal static void fibonacci(channel<nint> f, channel<nint> quit) {
    nint x = 0;
    nint y = 1;
    while (ᐧ) {
        var selᴛ1 = f.ᐸꟷ(x, ꓸꓸꓸ);
        var selᴛ2 = quit;
        switch (select(selᴛ1, ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0: {
            (x, y) = (y, x + y);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out _): {
            fmt.Println("quit");
            return;
        }}
    }
}

internal static void sendOnly(channel/*<-*/<@string> s) {
    s.ᐸꟷ("output");
}

public static Action<Func<nint, bool>> All(this IntSlice s) {
    var sʗ1 = s;
    return (Func<nint, bool> yield) => {
        foreach (var (_, v) in sʗ1) {
            if (!yield(v)) {
                return;
            }
        }
    };
}

internal static void generate(channel/*<-*/<nint> ch) {
    for (nint i = 2; ᐧ ; i++) {
        ch.ᐸꟷ(i);
    }
}

internal static void filter(/*<-*/channel<nint> src, channel/*<-*/<nint> dst, nint prime) {
    foreach (var i in src) {
        if (rem(i, prime) != 0) {
            dst.ᐸꟷ(i);
        }
    }
}

internal static partial void sieve() {
    var ch = new channel<nint>(0);
    goǃ(generate, ch.WithDirection(GoChanDir.Send));
    while (ᐧ) {
        nint prime = ᐸꟷ(ch);
        fmt.Print(prime, "\n");
        var ch1 = new channel<nint>(0);
        goǃ(filter, ch.WithDirection(GoChanDir.Recv), ch1.WithDirection(GoChanDir.Send), prime);
        ch = ch1;

        if (prime > 40) {
            break;
        }
    }
}

internal static nint f() {
    return 0;
}

internal static partial void Main() {
    var ch = new channel<nint>(2);

    ch.ᐸꟷ(1);
    ch.ᐸꟷ(2);

    fmt.Println(ᐸꟷ(ch));
    fmt.Println(ᐸꟷ(ch));

    var a = new slice<nint>(2);
    var ch1 = new channel<nint>(0);
    var ch2 = new channel<nint>(0);
    var ch3 = new channel<nint>(0);
    var ch4 = new channel<nint>(0);

    goǃ(g1, ch1);
    goǃ(g2, ch2);
    goǃ(g1, ch3);
    goǃ(g2, ch4);

    for (nint i = 0; i < 4; i++) {
        var selᴛ3 = ch1;
        var selᴛ4 = ch2;
        var selᴛ5 = ch3;
        var selᴛ6 = ch4;
        switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ), ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
        case 0 when selᴛ3.ꟷᐳ(out var v1): {
            fmt.Println("Got: ", v1);
            break;
        }
        case 1 when selᴛ4.ꟷᐳ(out var v1): {
            fmt.Println("Got: ", v1);
            break;
        }
        case 2 when selᴛ5.ꟷᐳ(out var v1, out var okΔ1): {
            fmt.Println("OK: ", okΔ1, " -- got: ", v1);
            break;
        }
        case 3 when selᴛ6.ꟷᐳ(out a[f()]): {
            fmt.Println("Got: ", a[f()]);
            break;
        }}
    }

    ch1 = default!;
    close(ch2);

    var selᴛ7 = ch1.ᐸꟷ(1, ꓸꓸꓸ);
    var selᴛ8 = ch1;
    var selᴛ9 = ch2;
    var selᴛ10 = ch3;
    var selᴛ11 = ch4;
    switch (select(selᴛ7, ᐸꟷ(selᴛ8, ꓸꓸꓸ), ᐸꟷ(selᴛ9, ꓸꓸꓸ), ᐸꟷ(selᴛ10, ꓸꓸꓸ), ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("unexpected send to nil channel");
        break;
    }
    case 1 when selᴛ8.ꟷᐳ(out var v1): {
        fmt.Println("unexpected received from nil channel: ", v1);
        break;
    }
    case 2 when selᴛ9.ꟷᐳ(out var v1): {
        fmt.Println("closed channel 2 selected immediately: ", v1);
        break;
    }
    case 3 when selᴛ10.ꟷᐳ(out var v1, out var okΔ2): {
        fmt.Println("unexpected: OK: ", okΔ2, " -- got: ", v1);
        break;
    }
    case 4 when selᴛ11.ꟷᐳ(out a[f()]): {
        fmt.Println("unexpected: ", a[f()]);
        break;
    }}

    var s = new nint[]{7, 2, 8, -9, 4, 0}.slice();

    var c = new channel<nint>(0);
    goǃ(sum, s.slice(0, len(s) / 2), c);
    goǃ(sum, s.slice(len(s) / 2), c);
    goǃ(sum, s[2..5], c);
    nint x = ᐸꟷ(c);
    nint y = ᐸꟷ(c);
    nint z = ᐸꟷ(c);
    fmt.Println(x, y, x + y, z);
    var fΔ1 = new channel<nint>(0);
    var quit = new channel<nint>(0);
    var fʗ1 = fΔ1;
    var quitʗ1 = quit;
    goǃ(() => {
        for (nint i = 0; i < 10; i++) {
            fmt.Println(ᐸꟷ(fʗ1));
        }
        quitʗ1.ᐸꟷ(0);
    });
    fibonacci(fΔ1, quit);

    var mychanl = new channel<@string>(0);

    goǃ(sendOnly, mychanl.WithDirection(GoChanDir.Send));

    var (result, ok) = ᐸꟷ(mychanl, ꟷ);
    fmt.Println(result, ok);
    foreach (var v in range(((IntSlice)s).All())) {
        fmt.Println(v);
    }
    sieve();

    var ca = new channel<@string>(1);
    var cb = new channel<@string>(1);
    ca.ᐸꟷ("hello");
    fmt.Println(firstMsg(ca, cb));
    var done = new channel<EmptyStruct>(0);
    fmt.Println(poll(done));
    close(done);
    fmt.Println(poll(done));

    var (r, outerPrimary) = raceSend();
    fmt.Println("raced:", r.value, r.primary, outerPrimary);
}

internal static @string firstMsg(channel<@string> a, channel<@string> b) {
    var selᴛ12 = a;
    var selᴛ13 = b;
    switch (select(ᐸꟷ(selᴛ12, ꓸꓸꓸ), ᐸꟷ(selᴛ13, ꓸꓸꓸ))) {
    case 0 when selᴛ12.ꟷᐳ(out var m): {
        return "a:"u8 + m;
    }
    case 1 when selᴛ13.ꟷᐳ(out var m): {
        return "b:"u8 + m;
    }}
    return default!;
}

internal static @string poll(channel<EmptyStruct> done) {
    var selᴛ14 = done;
    switch (trySelect(ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
    case 0 when selᴛ14.ꟷᐳ(out _): {
        return "done";
    }
    default: {
        break;
    }}
    return "pending";
}

partial struct raceResult {
    internal nint value;
    internal bool primary;
}

internal static (raceResult, bool) raceSend() {
    var results = new channel<raceResult>(1);
    var done = new channel<EmptyStruct>(0);
    var primary = true;
    var doneʗ1 = done;
    var resultsʗ1 = results;
    void racer(bool primaryΔ1) {
        var selᴛ15 = resultsʗ1.ᐸꟷ(new raceResult(value: 7, primary: primaryΔ1), ꓸꓸꓸ);
        var selᴛ16 = doneʗ1;
        switch (select(selᴛ15, ᐸꟷ(selᴛ16, ꓸꓸꓸ))) {
        case 0: {
            break;
        }
        case 1 when selᴛ16.ꟷᐳ(out _): {
            break;
        }}
    }
    racer(false);
    return (ᐸꟷ(results), primary);
}

} // end main_package
