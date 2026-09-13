// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

using fmt = fmt_package;
using rand = math.rand_package;
using math;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static UntypedInt win => 100;
internal static UntypedInt gamesPerSeries => 10;

[GoType] partial struct score {
    internal nint player, opponent, thisTurn;
}

// type action is a methodless func type — rendered inline as its base delegate

internal static (score, bool) roll(score s) {
    nint outcome = rand.Intn(6) + 1;
    if (outcome == 1) {
        return (new score(s.opponent, s.player, 0), true);
    }
    return (new score(s.player, s.opponent, outcome + s.thisTurn), false);
}

internal static (score, bool) stay(score s) {
    return (new score(s.opponent, s.player + s.thisTurn, 0), true);
}

internal delegate Func<score, (score, bool)> strategy(score _);

internal static strategy stayAtK(nint k) {
    return (score s) => {
        if (s.thisTurn >= k) {
            return stay;
        }
        return roll;
    };
}

internal static nint play(strategy strategy0, strategy strategy1) {
    var strategies = new strategy[]{strategy0, strategy1}.slice();
    score s = default!;
    bool turnIsOver = default!;
    nint currentPlayer = rand.Intn(2);
    while (s.player + s.thisTurn < win) {
        var action = strategies[currentPlayer](s);
        (s, turnIsOver) = action(s);
        if (turnIsOver) {
            currentPlayer = (currentPlayer + 1) % 2;
        }
    }
    return currentPlayer;
}

internal static (slice<nint>, nint) roundRobin(slice<strategy> strategies) {
    var wins = new slice<nint>(len(strategies));
    for (nint i = 0; i < len(strategies); i++) {
        for (nint j = i + 1; j < len(strategies); j++) {
            for (nint k = 0; k < gamesPerSeries; k++) {
                nint winner = play(strategies[i], strategies[j]);
                if (winner == 0){
                    wins[i]++;
                } else {
                    wins[j]++;
                }
            }
        }
    }
    nint gamesPerStrategy = (nint)gamesPerSeries * (len(strategies) - 1);
    return (wins, gamesPerStrategy);
}

internal static @string ratioString(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    nint total = 0;
    foreach (var (_, val) in vals) {
        total += val;
    }
    @string s = ""u8;
    foreach (var (_, val) in vals) {
        if (s != ""u8) {
            s += ", "u8;
        }
        var pct = 100D * (float64)val / (float64)total;
        s += fmt.Sprintf("%d/%d (%0.1f%%)"u8, val, total, pct);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object convertedFuncTypeCallˢ = (@string)"converted func type call:"u8;
private static readonly @string pkgFmtˢ = "pkg/fmt"u8;
private static readonly @string fmtˢ = "fmt"u8;
private static readonly @string printfˢ = "Printf"u8;
private static readonly object collapseHoistˢ = (@string)"collapse hoist:"u8;

internal static void Main() {
    var strategies = new slice<strategy>(win);
    foreach (var (kΔ1, _) in strategies) {
        strategies[kΔ1] = stayAtK(kΔ1 + 1);
    }
    var (wins, games) = roundRobin(strategies);
    nint k = default!;
    foreach (var (iᴛ1, _) in strategies) {
        k = iᴛ1;

        fmt.Printf("Wins, losses staying at k =% 4d: %s\n"u8,
            k + 1, ratioString(wins[k], games - wins[k]));
    }
    var converted = (Func<score, (score, bool)>)(stay);
    var (result, turnIsOver) = converted(new score(player: 3, opponent: 5, thisTurn: 7));
    fmt.Println(convertedFuncTypeCallˢ, result.player, result.opponent, result.thisTurn, turnIsOver);
    var m = new machine(split: (@string s) => (len(s), s + "!", default!));
    var (n, @out, serr) = m.split("hi"u8);
    fmt.Println(n, @out, serr == default!);
    var p = new provider(produce: (nint x) => x * 2);
    var r = new registry(h: new handler(p.produce));
    fmt.Println(r.h(21));
    tagged tg = new handlerᴠtagged(r.h);
    fmt.Println(tg.tag(), r.h(1));
    var wk = Ꮡ(new worker(n: 3));
    {
        var err = wk.initWorker(0); if (err == default!) {
            fmt.Println(wk.run(new byte[]{1, 2}.slice()), lastN.Value);
        }
    }
    var res = new resolver(
        lookupPackage: (@string name) => {
            if (name == "fmt"u8) {
                return (pkgFmtˢ, true);
            }
            return ("", false);
        },
        lookupSym: (@string recv, @string name) => {
            return recv == ""u8 && name == "Printf"u8;
        }
    );
    var (ip, found) = res.lookupPackage(fmtˢ);
    fmt.Println(ip, found, res.lookupSym(""u8, printfˢ), res.lookupSym("T"u8, "M"u8));
    (any, error) fetch() {
        var (ᴛ1, ᴛ2) = fetchPair();
        return (ᴛ1, ᴛ2);
    }
    var (got, gerr) = fetch();
    fmt.Println(collapseHoistˢ, got, gerr == default!);
    fmt.Println(passThrough());
}

internal static @string passThrough() {
    var (ᴛ1, ᴛ2) = fetchPair();
    return joinPair(ᴛ1, ᴛ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pairˢ = "pair"u8;

internal static (@string, error) fetchPair() {
    return (pairˢ, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string errˢ = "err"u8;

internal static @string joinPair(@string s, error err) {
    if (err != default!) {
        return errˢ;
    }
    return "got:"u8 + s;
}

[GoType] partial struct resolver {
    internal Func<@string, (@string importPath, bool ok)> lookupPackage;
    internal Func<@string, @string, bool> lookupSym;
}

// type splitter is a methodless func type — rendered inline as its base delegate

[GoType] partial struct machine {
    internal Func<@string, (nint, @string, error)> split;
}

[GoType] partial struct worker {
    internal Func<ж<worker>, slice<byte>, nint> fill;
    internal Action<ж<worker>> step;
    internal nint n;
}

internal static ж<nint> lastN;

internal static nint fillFast(this ж<worker> Ꮡw, slice<byte> b) {
    ref var w = ref Ꮡw.DerefOrNull();

    lastN = Ꮡw.of(worker.Ꮡn);
    return len(b) + w.n;
}

[GoRecv] internal static void bump(this ref worker w) {
    w.n++;
}

[GoRecv] internal static error /*err*/ initWorker(this ref worker w, nint level) {
    switch (ᐧ) {
    case {} when level is 0: {
        w.fill = ((Func<ж<worker>, slice<byte>, nint>)(fillFast));
        w.step = ((Action<ж<worker>>)(bump));
        break;
    }
    default: {
        return fmt.Errorf("bad level %d"u8, level);
    }}

    return default!;
}

internal static nint run(this ж<worker> Ꮡw, slice<byte> b) {
    ref var w = ref Ꮡw.DerefOrNull();

    w.step(Ꮡw);
    return w.fill(Ꮡw, b);
}

internal delegate nint handler(nint _);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string handlerˢ = "handler"u8;

internal static @string tag(this handler h) {
    return handlerˢ;
}

[GoType] partial interface tagged {
    @string tag();
}

[GoType] partial struct provider {
    internal Func<nint, nint> produce;
}

[GoType] partial struct registry {
    internal handler h;
}

} // end main_package
