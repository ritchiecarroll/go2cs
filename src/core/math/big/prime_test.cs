// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using strings = strings_package;
using testing = testing_package;
using unicode = unicode_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal static slice<@string> primes = slice<@string>([
    "2",
    "3",
    "5",
    "7",
    "11",

    "13756265695458089029",
    "13496181268022124907",
    "10953742525620032441",
    "17908251027575790097",

    // https://golang.org/issue/638
    "18699199384836356663",

    "98920366548084643601728869055592650835572950932266967461790948584315647051443",
    "94560208308847015747498523884063394671606671904944666360068158221458669711639",

    // https://primes.utm.edu/lists/small/small3.html
    "449417999055441493994709297093108513015373787049558499205492347871729927573118262811508386655998299074566974373711472560655026288668094291699357843464363003144674940345912431129144354948751003607115263071543163",
    "230975859993204150666423538988557839555560243929065415434980904258310530753006723857139742334640122533598517597674807096648905501653461687601339782814316124971547968912893214002992086353183070342498989426570593",
    "5521712099665906221540423207019333379125265462121169655563495403888449493493629943498064604536961775110765377745550377067893607246020694972959780839151452457728855382113555867743022746090187341871655890805971735385789993",
    "203956878356401977405765866929034577280193993314348263094772646453283062722701277632936616063144088173312372882677123879538709400158306567338328279154499698366071906766440037074217117805690872792848149112022286332144876183376326512083574821647933992961249917319836219304274280243803104015000563790123",

    // ECC primes: https://tools.ietf.org/html/draft-ladd-safecurves-02
    "3618502788666131106986593281521497120414687020801267626233049500247285301239",                                                                                 // Curve1174: 2^251-9
    "57896044618658097711785492504343953926634992332820282019728792003956564819949",                                                                                // Curve25519: 2^255-19
    "9850501549098619803069760025035903451269934817616361666987073351061430442874302652853566563721228910201656997576599",                                          // E-382: 2^382-105
    "42307582002575910332922579714097346549017899709713998034217522897561970639123926132812109468141778230245837569601494931472367",                                // Curve41417: 2^414-17
    "6864797660130609714981900799081393217269435300143305409394463459185543183397656052122559640661454554977296311391480858037121987999716643812574028291115057151" // E-521: 2^521-1
]);

internal static slice<@string> composites = slice<@string>([
    "0",
    "1",
    "21284175091214687912771199898307297748211672914763848041968395774954376176754",
    "6084766654921918907427900243509372380954290099172559290432744450051395395951",
    "84594350493221918389213352992032324280367711247940675652888030554255915464401",
    "82793403787388584738507275144194252681",

    // Arnault, "Rabin-Miller Primality Test: Composite Numbers Which Pass It",
    // Mathematics of Computation, 64(209) (January 1995), pp. 335-361.
    "1195068768795265792518361315725116351898245581", // strong pseudoprime to prime bases 2 through 29
    // strong pseudoprime to all prime bases up to 200
    """

     80383745745363949125707961434194210813883768828755814583748891752229
      74273765333652186502336163960045457915042023603208766569966760987284
       0439654082329287387918508691668573282677617710293896977394701670823
        0428687109997439976544144845341155872450633409279022275296229414984
         2306881685404326457534018329786111298960644845216191652872597534901
"""u8.Raw(),

    // Extra-strong Lucas pseudoprimes. https://oeis.org/A217719
    "989",
    "3239",
    "5777",
    "10877",
    "27971",
    "29681",
    "30739",
    "31631",
    "39059",
    "72389",
    "73919",
    "75077",
    "100127",
    "113573",
    "125249",
    "137549",
    "137801",
    "153931",
    "155819",
    "161027",
    "162133",
    "189419",
    "218321",
    "231703",
    "249331",
    "370229",
    "429479",
    "430127",
    "459191",
    "473891",
    "480689",
    "600059",
    "621781",
    "632249",
    "635627",

    "3673744903",
    "3281593591",
    "2385076987",
    "2738053141",
    "2009621503",
    "1502682721",
    "255866131",
    "117987841",
    "587861",

    "6368689",
    "8725753",
    "80579735209",
    "105919633"
]);

internal static rune cutSpace(rune r) {
    if (unicode.IsSpace(r)) {
        return -1;
    }
    return r;
}

public static void TestProbablyPrime(ж<testing.T> t) {
    nint nreps = 20;
    if (testing.Short()) {
        nreps = 1;
    }
    foreach (var (i, s) in primes) {
        var (p, _) = @new<global::go.math.big_package.ΔInt>().SetString(s, 10);
        if (!p.ProbablyPrime(nreps) || nreps != 1 && !p.ProbablyPrime(1) || !p.ProbablyPrime(0)) {
            t.Errorf("#%d prime found to be non-prime (%s)", i, s);
        }
    }

    foreach (var (i, vᴛ1) in composites) {
        var s = vᴛ1;

        s = strings.Map(cutSpace, s);
        var (cΔ1, _) = @new<global::go.math.big_package.ΔInt>().SetString(s, 10);
        if (cΔ1.ProbablyPrime(nreps) || nreps != 1 && cΔ1.ProbablyPrime(1) || cΔ1.ProbablyPrime(0)) {
            t.Errorf("#%d composite found to be prime (%s)", i, s);
        }
    }

    // check that ProbablyPrime panics if n <= 0
    var c = NewInt(11); // a prime
    foreach (var (_, n) in slice<nint>([-1, 0, 1])) {
        var cʗ1 = c;
        func(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    if (n < 0 && recover() == nil) {
                        t.Fatalf("expected panic from ProbablyPrime(%d)", n);
                    }
                }, ref ᒐ);
                if (!cʗ1.ProbablyPrime(n)) {
                    t.Fatalf("%v should be a prime", cʗ1.OrTypedNil());
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lucasˢ => ref lucasˢᵇ;
private static readonly @string lucasˢᵇ = "Lucas"u8;
internal static ref readonly @string millerRabinBase2ˢ => ref millerRabinBase2ˢᵇ;
private static readonly @string millerRabinBase2ˢᵇ = "MillerRabinBase2"u8;

public static void BenchmarkProbablyPrime(ж<testing.B> b) {
    GoFrame ᒐ = default;
    try {
        var stk = getStack();
        var stkʗ1 = stk;
        defer(stkʗ1.free, ref ᒐ);

        var (p, _) = @new<global::go.math.big_package.ΔInt>().SetString("203956878356401977405765866929034577280193993314348263094772646453283062722701277632936616063144088173312372882677123879538709400158306567338328279154499698366071906766440037074217117805690872792848149112022286332144876183376326512083574821647933992961249917319836219304274280243803104015000563790123", 10);
        foreach (var (_, n) in slice<nint>([0, 1, 5, 10, 20])) {
            var pʗ1 = p;
            b.Run(fmt.Sprintf("n=%d"u8, n), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    pʗ1.ProbablyPrime(n);
                }
            });
        }

        var pʗ2 = p;
        var stkʗ2 = stk;
        b.Run(lucasˢ, (ж<testing.B> bΔ2) => {
            for (nint i = 0; i < (~bΔ2).N; i++) {
                pʗ2.abs.probablyPrimeLucas(stkʗ2);
            }
        });
        var pʗ3 = p;
        var stkʗ3 = stk;
        b.Run(millerRabinBase2ˢ, (ж<testing.B> bΔ3) => {
            for (nint i = 0; i < (~bΔ3).N; i++) {
                pʗ3.abs.probablyPrimeMillerRabin(stkʗ3, 1, true);
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string probablyPrimeMillerRabinˢ => ref probablyPrimeMillerRabinˢᵇ;
private static readonly @string probablyPrimeMillerRabinˢᵇ = "probablyPrimeMillerRabin"u8;

public static void TestMillerRabinPseudoprimes(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var stk = getStack();
        var stkʗ1 = stk;
        defer(stkʗ1.free, ref ᒐ);

            var stkʗ2 = stk;
        testPseudoprimes(t, probablyPrimeMillerRabinˢ,
            (global::go.math.big_package.Δnat n) => n.probablyPrimeMillerRabin(stkʗ2, 1, true) && !n.probablyPrimeLucas(stkʗ2),
            // https://oeis.org/A001262
            slice<nint>([2047, 3277, 4033, 4681, 8321, 15841, 29341, 42799, 49141, 52633, 65281, 74665, 80581, 85489, 88357, 90751]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string probablyPrimeLucasˢ => ref probablyPrimeLucasˢᵇ;
private static readonly @string probablyPrimeLucasˢᵇ = "probablyPrimeLucas"u8;

public static void TestLucasPseudoprimes(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var stk = getStack();
        var stkʗ1 = stk;
        defer(stkʗ1.free, ref ᒐ);

            var stkʗ2 = stk;
        testPseudoprimes(t, probablyPrimeLucasˢ,
            (global::go.math.big_package.Δnat n) => n.probablyPrimeLucas(stkʗ2) && !n.probablyPrimeMillerRabin(stkʗ2, 1, true),
            // https://oeis.org/A217719
            slice<nint>([989, 3239, 5777, 10877, 27971, 29681, 30739, 31631, 39059, 72389, 73919, 75077]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testPseudoprimes(ж<testing.T> t, @string name, Func<global::go.math.big_package.Δnat, bool> cond, slice<nint> want) {
    var n = new Δnat(new global::go.math.big_package.Word[]{1}.slice());
    for (nint i = 3; i < 100000; i += 2) {
        if (testing.Short()) {
            if (len(want) == 0) {
                break;
            }
            if (i < want[0] - 2) {
                i = want[0] - 2;
            }
        }
        n[0] = (global::go.math.big_package.Word)(nuint)i;
        var pseudo = cond(n);
        if (pseudo && (len(want) == 0 || i != want[0])) {
            t.Errorf("%s(%v, base=2) = true, want false", name, i);
        } else if (!pseudo && len(want) >= 1 && i == want[0]) {
            t.Errorf("%s(%v, base=2) = false, want true", name, i);
        }
        if (len(want) > 0 && i == want[0]) {
            want = want[1..];
        }
    }
    if (len(want) > 0) {
        t.Fatalf("forgot to test %v", want);
    }
}

} // end big_internal_test_package
