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

internal static slice<@string> primes = new @string[]{
    "2"u8,
    "3"u8,
    "5"u8,
    "7"u8,
    "11"u8,
    "13756265695458089029"u8,
    "13496181268022124907"u8,
    "10953742525620032441"u8,
    "17908251027575790097"u8,

    // https://golang.org/issue/638
    "18699199384836356663"u8,
    "98920366548084643601728869055592650835572950932266967461790948584315647051443"u8,
    "94560208308847015747498523884063394671606671904944666360068158221458669711639"u8,

    // https://primes.utm.edu/lists/small/small3.html
    "449417999055441493994709297093108513015373787049558499205492347871729927573118262811508386655998299074566974373711472560655026288668094291699357843464363003144674940345912431129144354948751003607115263071543163"u8,
    "230975859993204150666423538988557839555560243929065415434980904258310530753006723857139742334640122533598517597674807096648905501653461687601339782814316124971547968912893214002992086353183070342498989426570593"u8,
    "5521712099665906221540423207019333379125265462121169655563495403888449493493629943498064604536961775110765377745550377067893607246020694972959780839151452457728855382113555867743022746090187341871655890805971735385789993"u8,
    "203956878356401977405765866929034577280193993314348263094772646453283062722701277632936616063144088173312372882677123879538709400158306567338328279154499698366071906766440037074217117805690872792848149112022286332144876183376326512083574821647933992961249917319836219304274280243803104015000563790123"u8,

    // ECC primes: https://tools.ietf.org/html/draft-ladd-safecurves-02
    "3618502788666131106986593281521497120414687020801267626233049500247285301239"u8, // Curve1174: 2^251-9
    "57896044618658097711785492504343953926634992332820282019728792003956564819949"u8, // Curve25519: 2^255-19
    "9850501549098619803069760025035903451269934817616361666987073351061430442874302652853566563721228910201656997576599"u8, // E-382: 2^382-105
    "42307582002575910332922579714097346549017899709713998034217522897561970639123926132812109468141778230245837569601494931472367"u8, // Curve41417: 2^414-17
    "6864797660130609714981900799081393217269435300143305409394463459185543183397656052122559640661454554977296311391480858037121987999716643812574028291115057151"u8 // E-521: 2^521-1
}.slice();

internal static slice<@string> composites = new @string[]{
    "0"u8,
    "1"u8,
    "21284175091214687912771199898307297748211672914763848041968395774954376176754"u8,
    "6084766654921918907427900243509372380954290099172559290432744450051395395951"u8,
    "84594350493221918389213352992032324280367711247940675652888030554255915464401"u8,
    "82793403787388584738507275144194252681"u8,

    // Arnault, "Rabin-Miller Primality Test: Composite Numbers Which Pass It",
    // Mathematics of Computation, 64(209) (January 1995), pp. 335-361.
    "1195068768795265792518361315725116351898245581"u8, // strong pseudoprime to prime bases 2 through 29
    // strong pseudoprime to all prime bases up to 200
    """

     80383745745363949125707961434194210813883768828755814583748891752229
      74273765333652186502336163960045457915042023603208766569966760987284
       0439654082329287387918508691668573282677617710293896977394701670823
        0428687109997439976544144845341155872450633409279022275296229414984
         2306881685404326457534018329786111298960644845216191652872597534901
"""u8.Raw(),

    // Extra-strong Lucas pseudoprimes. https://oeis.org/A217719
    "989"u8,
    "3239"u8,
    "5777"u8,
    "10877"u8,
    "27971"u8,
    "29681"u8,
    "30739"u8,
    "31631"u8,
    "39059"u8,
    "72389"u8,
    "73919"u8,
    "75077"u8,
    "100127"u8,
    "113573"u8,
    "125249"u8,
    "137549"u8,
    "137801"u8,
    "153931"u8,
    "155819"u8,
    "161027"u8,
    "162133"u8,
    "189419"u8,
    "218321"u8,
    "231703"u8,
    "249331"u8,
    "370229"u8,
    "429479"u8,
    "430127"u8,
    "459191"u8,
    "473891"u8,
    "480689"u8,
    "600059"u8,
    "621781"u8,
    "632249"u8,
    "635627"u8,
    "3673744903"u8,
    "3281593591"u8,
    "2385076987"u8,
    "2738053141"u8,
    "2009621503"u8,
    "1502682721"u8,
    "255866131"u8,
    "117987841"u8,
    "587861"u8,
    "6368689"u8,
    "8725753"u8,
    "80579735209"u8,
    "105919633"u8
}.slice();

internal static rune cutSpace(rune r) {
    if (unicode.IsSpace(r)) {
        return -1;
    }
    return r;
}

public static void TestProbablyPrime(ж<testing.T> Ꮡt) {
    nint nreps = 20;
    if (testing.Short()) {
        nreps = 1;
    }
    foreach (var (i, s) in primes) {
        var (p, _) = @new<global::go.math.big_package.ΔInt>().SetString(s, 10);
        if (!p.ProbablyPrime(nreps) || nreps != 1 && !p.ProbablyPrime(1) || !p.ProbablyPrime(0)) {
            Ꮡt.Errorf("#%d prime found to be non-prime (%s)"u8, i, s);
        }
    }

    foreach (var (i, vᴛ1) in composites) {
        var s = vᴛ1;

        s = strings.Map(cutSpace, s);
        var (cΔ1, _) = @new<global::go.math.big_package.ΔInt>().SetString(s, 10);
        if (cΔ1.ProbablyPrime(nreps) || nreps != 1 && cΔ1.ProbablyPrime(1) || cΔ1.ProbablyPrime(0)) {
            Ꮡt.Errorf("#%d composite found to be prime (%s)"u8, i, s);
        }
    }

    // check that ProbablyPrime panics if n <= 0
    var c = NewInt(11); // a prime
    foreach (var (_, n) in new nint[]{-1, 0, 1}.slice()) {
        var cʗ1 = c;
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    if (n < 0 && recover() == default!) {
                        Ꮡt.Fatalf("expected panic from ProbablyPrime(%d)"u8, n);
                    }
                }, ref ᒐ);
                if (!cʗ1.ProbablyPrime(n)) {
                    Ꮡt.Fatalf("%v should be a prime"u8, cʗ1.OrTypedNil());
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lucasˢ = "Lucas"u8;
internal static readonly @string millerRabinBase2ˢ = "MillerRabinBase2"u8;

public static void BenchmarkProbablyPrime(ж<testing.B> Ꮡb) {
    var (p, _) = @new<global::go.math.big_package.ΔInt>().SetString("203956878356401977405765866929034577280193993314348263094772646453283062722701277632936616063144088173312372882677123879538709400158306567338328279154499698366071906766440037074217117805690872792848149112022286332144876183376326512083574821647933992961249917319836219304274280243803104015000563790123"u8, 10);
    foreach (var (_, n) in new nint[]{0, 1, 5, 10, 20}.slice()) {
        var pʗ1 = p;
        Ꮡb.Run(fmt.Sprintf("n=%d"u8, n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                pʗ1.ProbablyPrime(n);
            }
        });
    }

    var pʗ2 = p;
    Ꮡb.Run(lucasˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            (~pʗ2).abs.probablyPrimeLucas();
        }
    });
    var pʗ3 = p;
    Ꮡb.Run(millerRabinBase2ˢ, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            (~pʗ3).abs.probablyPrimeMillerRabin(1, true);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string probablyPrimeMillerRabinˢ = "probablyPrimeMillerRabin"u8;

public static void TestMillerRabinPseudoprimes(ж<testing.T> Ꮡt) {
    testPseudoprimes(Ꮡt, probablyPrimeMillerRabinˢ,
        (global::go.math.big_package.nat n) => n.probablyPrimeMillerRabin(1, true) && !n.probablyPrimeLucas(),
        // https://oeis.org/A001262
        new nint[]{2047, 3277, 4033, 4681, 8321, 15841, 29341, 42799, 49141, 52633, 65281, 74665, 80581, 85489, 88357, 90751}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string probablyPrimeLucasˢ = "probablyPrimeLucas"u8;

public static void TestLucasPseudoprimes(ж<testing.T> Ꮡt) {
    testPseudoprimes(Ꮡt, probablyPrimeLucasˢ,
        (global::go.math.big_package.nat n) => n.probablyPrimeLucas() && !n.probablyPrimeMillerRabin(1, true),
        // https://oeis.org/A217719
        new nint[]{989, 3239, 5777, 10877, 27971, 29681, 30739, 31631, 39059, 72389, 73919, 75077}.slice());
}

internal static void testPseudoprimes(ж<testing.T> Ꮡt, @string name, Func<global::go.math.big_package.nat, bool> cond, slice<nint> want) {
    var n = new nat(new global::go.math.big_package.Word[]{1}.slice());
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
        if (pseudo && (len(want) == 0 || i != want[0])){
            Ꮡt.Errorf("%s(%v, base=2) = true, want false"u8, name, i);
        } else 
        if (!pseudo && len(want) >= 1 && i == want[0]) {
            Ꮡt.Errorf("%s(%v, base=2) = false, want true"u8, name, i);
        }
        if (len(want) > 0 && i == want[0]) {
            want = want[1..];
        }
    }
    if (len(want) > 0) {
        Ꮡt.Fatalf("forgot to test %v"u8, want);
    }
}

} // end big_internal_test_package
