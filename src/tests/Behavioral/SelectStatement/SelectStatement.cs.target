namespace go;

using fmt = fmt_package;

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
    foreach (var (_, v) in range(s)) {
        sum += v;
    }
    c.ᐸꟷ(sum);
}

internal static void fibonacci(channel<nint> f, channel<nint> quit) {
    nint x = 0;
    nint y = 1;
    for (;;) {
        var fᴛ1 = f.ᐸꟷ(x, ꓸꓸꓸ);
        var quitᴛ1 = quit;
        switch (select(fᴛ1, ᐸꟷ(quitᴛ1, ꓸꓸꓸ))) {
        case 0: {
            (x, y) = (y, x + y);
            break;
        }
        case 1 when quitᴛ1.ꟷᐳ(out _): {
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
        foreach (var (_, v) in range<nint>(sʗ1)) {
            if (!yield(v)) {
                return;
            }
        }
    };
}

internal static void generate(channel/*<-*/<nint> ch) {
    for (nint i = 2; ; i++) {
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
    goǃ(generate, ch.AsSendOnly());
    for (;;) {
        nint prime = ᐸꟷ(ch);
        fmt.Print(prime, "\n");
        var ch1 = new channel<nint>(0);
        goǃ(filter, ch.AsRecvOnly(), ch1.AsSendOnly(), prime);
        ch = ch1;

        if (prime > 40) {
            break;
        }
    }
}

internal static nint f() {
    return 0;
}

internal static partial void main() {
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
        var ch1ᴛ1 = ch1;
        var ch2ᴛ1 = ch2;
        var ch3ᴛ1 = ch3;
        var ch4ᴛ1 = ch4;
        switch (select(ᐸꟷ(ch1ᴛ1, ꓸꓸꓸ), ᐸꟷ(ch2ᴛ1, ꓸꓸꓸ), ᐸꟷ(ch3ᴛ1, ꓸꓸꓸ), ᐸꟷ(ch4ᴛ1, ꓸꓸꓸ))) {
        case 0 when ch1ᴛ1.ꟷᐳ(out var v1): {
            fmt.Println("Got: ", v1);
            break;
        }
        case 1 when ch2ᴛ1.ꟷᐳ(out var v1): {
            fmt.Println("Got: ", v1);
            break;
        }
        case 2 when ch3ᴛ1.ꟷᐳ(out var v1, out var okΔ1): {
            fmt.Println("OK: ", okΔ1, " -- got: ", v1);
            break;
        }
        case 3 when ch4ᴛ1.ꟷᐳ(out a[f()]): {
            fmt.Println("Got: ", a[f()]);
            break;
        }}
    }

    ch1 = default;
    close(ch2);

    var ch1ᴛ2 = ch1.ᐸꟷ(1, ꓸꓸꓸ);
    var ch1ᴛ3 = ch1;
    var ch2ᴛ2 = ch2;
    var ch3ᴛ2 = ch3;
    var ch4ᴛ2 = ch4;
    switch (select(ch1ᴛ2, ᐸꟷ(ch1ᴛ3, ꓸꓸꓸ), ᐸꟷ(ch2ᴛ2, ꓸꓸꓸ), ᐸꟷ(ch3ᴛ2, ꓸꓸꓸ), ᐸꟷ(ch4ᴛ2, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("unexpected send to nil channel");
        break;
    }
    case 1 when ch1ᴛ3.ꟷᐳ(out var v1): {
        fmt.Println("unexpected received from nil channel: ", v1);
        break;
    }
    case 2 when ch2ᴛ2.ꟷᐳ(out var v1): {
        fmt.Println("closed channel 2 selected immediately: ", v1);
        break;
    }
    case 3 when ch3ᴛ2.ꟷᐳ(out var v1, out var okΔ2): {
        fmt.Println("unexpected: OK: ", okΔ2, " -- got: ", v1);
        break;
    }
    case 4 when ch4ᴛ2.ꟷᐳ(out a[f()]): {
        fmt.Println("unexpected: ", a[f()]);
        break;
    }}

    var s = slice<nint>([7, 2, 8, -9, 4, 0]);

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

    goǃ(sendOnly, mychanl.AsSendOnly());

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
    var aᴛ1 = a;
    var bᴛ1 = b;
    switch (select(ᐸꟷ(aᴛ1, ꓸꓸꓸ), ᐸꟷ(bᴛ1, ꓸꓸꓸ))) {
    case 0 when aᴛ1.ꟷᐳ(out var m): {
        return "a:"u8 + m;
    }
    case 1 when bᴛ1.ꟷᐳ(out var m): {
        return "b:"u8 + m;
    }}
    return default;
}

internal static @string poll(channel<EmptyStruct> done) {
    var doneᴛ1 = done;
    switch (trySelect(ᐸꟷ(doneᴛ1, ꓸꓸꓸ))) {
    case 0 when doneᴛ1.ꟷᐳ(out _): {
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
        var resultsᴛ1 = resultsʗ1.ᐸꟷ(new raceResult(value: 7, primary: primaryΔ1), ꓸꓸꓸ);
        var doneᴛ1 = doneʗ1;
        switch (select(resultsᴛ1, ᐸꟷ(doneᴛ1, ꓸꓸꓸ))) {
        case 0: {
            break;
        }
        case 1 when doneᴛ1.ꟷᐳ(out _): {
            break;
        }}
    }
    racer(false);
    return (ᐸꟷ(results), primary);
}

} // end main_package
