// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using bits = go.math.bits_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using static go.math.big_package;

partial class big_internal_test_package {

public static void TestMaxBase(ж<testing.T> Ꮡt) {
    if (MaxBase != len(digits)) {
        Ꮡt.Fatalf("%d != %d"u8, (int32)(MaxBase), len(digits));
    }
}

// log2 computes the integer binary logarithm of x.
// The result is the integer n for which 2^n <= x < 2^(n+1).
// If x == 0, the result is -1.
internal static nint log2(global::go.math.big_package.Word x) {
    return bits.Len((nuint)x) - 1;
}

internal static slice<byte> itoa(global::go.math.big_package.nat x, nint @base) {
    // special cases
    switch (ᐧ) {
    case {} when @base is < 2: {
        throw panic("illegal base");
        break;
    }
    case {} when len(x) is 0: {
        return slice<byte>("0"u8);
    }}

    // allocate buffer for conversion
    nint i = quo(x.bitLen(), log2(((global::go.math.big_package.Word)(nuint)@base))) + 1; // +1: round up
    var s = new slice<byte>(i);
    // don't destroy x
    var q = ((global::go.math.big_package.nat)default!).set(x);
    // convert
    while (len(q) > 0) {
        i--;
        global::go.math.big_package.Word r = default!;
        (q, r) = q.divW(q, ((global::go.math.big_package.Word)(nuint)@base));
        s[i] = digits[r];
    }
    return s.slice(i);
}


[GoType("dyn")] partial struct strTestsᴛ1 {
    internal global::go.math.big_package.nat x;    // nat value to be converted
    internal nint b;   // conversion base
    internal @string s; // expected result
}
internal static slice<strTestsᴛ1> strTests = new strTestsᴛ1[]{
    new(default!, 2, "0"u8),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), 2, "1"u8),
    new(new nat(new global::go.math.big_package.Word[]{0xc5}.slice()), 2, "11000101"u8),
    new(new nat(new global::go.math.big_package.Word[]{1721}.slice()), 8, "3271"u8),
    new(new nat(new global::go.math.big_package.Word[]{10}.slice()), 10, "10"u8),
    new(new nat(new global::go.math.big_package.Word[]{1234567890}.slice()), 10, "1234567890"u8),
    new(new nat(new global::go.math.big_package.Word[]{0xdeadbeefU}.slice()), 16, "deadbeef"u8),
    new(new nat(new global::go.math.big_package.Word[]{0x229be7}.slice()), 17, "1a2b3c"u8),
    new(new nat(new global::go.math.big_package.Word[]{0x309663e6}.slice()), 32, "o9cov6"u8),
    new(new nat(new global::go.math.big_package.Word[]{0x309663e6}.slice()), 62, "TakXI"u8)
}.slice();

public static void TestString(ж<testing.T> Ꮡt) {
    // test invalid base explicitly
    @string panicStr = default!;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                panicStr = recover()._<@string>();
            }, ref ᒐ);
            natOne.utoa(1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    if (panicStr != "invalid base"u8) {
        Ꮡt.Errorf("expected panic for invalid base"u8);
    }
    foreach (var (_, a) in strTests) {
        @string s = ((@string)a.x.utoa(a.b));
        if (s != a.s) {
            Ꮡt.Errorf("string%+v\n\tgot s = %s; want %s"u8, a, s, a.s);
        }
        var (x, b, _, err) = ((global::go.math.big_package.nat)default!).scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(a.s)), a.b, false);
        if (x.cmp(a.x) != 0) {
            Ꮡt.Errorf("scan%+v\n\tgot z = %v; want %v"u8, a, x, a.x);
        }
        if (b != a.b) {
            Ꮡt.Errorf("scan%+v\n\tgot b = %d; want %d"u8, a, b, a.b);
        }
        if (err != default!) {
            Ꮡt.Errorf("scan%+v\n\tgot error = %s"u8, a, err);
        }
    }
}

// invalid: no digits
// invalid: incorrect use of decimal point
// invalid: incorrect use of separators
// valid: separators are not accepted for base != 0
// valid, no decimal point
// octal 0
// valid, with decimal point
// valid, with separators

[GoType("dyn")] partial struct natScanTestsᴛ1 {
    internal @string s; // string to be scanned
    internal nint @base;   // input base
    internal bool frac;   // fraction ok
    internal global::go.math.big_package.nat x;    // expected nat
    internal nint b;   // expected base
    internal nint count;   // expected digit count
    internal error err;  // expected error
    internal rune next;   // next character (or 0, if at EOF)
}
internal static slice<natScanTestsᴛ1> natScanTests;
internal static void initᴛnatScanTests() { natScanTests = new natScanTestsᴛ1[]{
    new(""u8, 0, false, default!, 10, 0, errNoDigits, 0),
    new("_"u8, 0, false, default!, 10, 0, errNoDigits, 0),
    new("?"u8, 0, false, default!, 10, 0, errNoDigits, (rune)'?'),
    new("?"u8, 10, false, default!, 10, 0, errNoDigits, (rune)'?'),
    new(""u8, 10, false, default!, 10, 0, errNoDigits, 0),
    new(""u8, 36, false, default!, 36, 0, errNoDigits, 0),
    new(""u8, 62, false, default!, 62, 0, errNoDigits, 0),
    new("0b"u8, 0, false, default!, 2, 0, errNoDigits, 0),
    new("0o"u8, 0, false, default!, 8, 0, errNoDigits, 0),
    new("0x"u8, 0, false, default!, 16, 0, errNoDigits, 0),
    new("0x_"u8, 0, false, default!, 16, 0, errNoDigits, 0),
    new("0b2"u8, 0, false, default!, 2, 0, errNoDigits, (rune)'2'),
    new("0B2"u8, 0, false, default!, 2, 0, errNoDigits, (rune)'2'),
    new("0o8"u8, 0, false, default!, 8, 0, errNoDigits, (rune)'8'),
    new("0O8"u8, 0, false, default!, 8, 0, errNoDigits, (rune)'8'),
    new("0xg"u8, 0, false, default!, 16, 0, errNoDigits, (rune)'g'),
    new("0Xg"u8, 0, false, default!, 16, 0, errNoDigits, (rune)'g'),
    new("345"u8, 2, false, default!, 2, 0, errNoDigits, (rune)'3'),
    new("._"u8, 0, true, default!, 10, 0, errNoDigits, 0),
    new(".0"u8, 0, false, default!, 10, 0, errNoDigits, (rune)'.'),
    new(".0"u8, 10, false, default!, 10, 0, errNoDigits, (rune)'.'),
    new("."u8, 0, true, default!, 10, 0, errNoDigits, 0),
    new("0x."u8, 0, true, default!, 16, 0, errNoDigits, 0),
    new("0x.g"u8, 0, true, default!, 16, 0, errNoDigits, (rune)'g'),
    new("0x.0"u8, 0, false, default!, 16, 0, errNoDigits, (rune)'.'),
    new("_0"u8, 0, false, default!, 10, 1, errInvalSep, 0),
    new("0_"u8, 0, false, default!, 10, 1, errInvalSep, 0),
    new("0__0"u8, 0, false, default!, 8, 1, errInvalSep, 0),
    new("0x___0"u8, 0, false, default!, 16, 1, errInvalSep, 0),
    new("0_x"u8, 0, false, default!, 10, 1, errInvalSep, (rune)'x'),
    new("0_8"u8, 0, false, default!, 10, 1, errInvalSep, (rune)'8'),
    new("123_."u8, 0, true, new nat(new global::go.math.big_package.Word[]{123}.slice()), 10, 0, errInvalSep, 0),
    new("._123"u8, 0, true, new nat(new global::go.math.big_package.Word[]{123}.slice()), 10, -3, errInvalSep, 0),
    new("0b__1000"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0x8}.slice()), 2, 4, errInvalSep, 0),
    new("0o60___0"u8, 0, false, new nat(new global::go.math.big_package.Word[]{384}.slice()), 8, 3, errInvalSep, 0),
    new("0466_"u8, 0, false, new nat(new global::go.math.big_package.Word[]{310}.slice()), 8, 3, errInvalSep, 0),
    new("01234567_8"u8, 0, false, new nat(new global::go.math.big_package.Word[]{342391}.slice()), 8, 7, errInvalSep, (rune)'8'),
    new("1_."u8, 0, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, 0, errInvalSep, 0),
    new("0._1"u8, 0, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, -1, errInvalSep, 0),
    new("2.7_"u8, 0, true, new nat(new global::go.math.big_package.Word[]{27}.slice()), 10, -1, errInvalSep, 0),
    new("0x1.0_"u8, 0, true, new nat(new global::go.math.big_package.Word[]{0x10}.slice()), 16, -1, errInvalSep, 0),
    new("0_"u8, 10, false, default!, 10, 1, default!, (rune)'_'),
    new("1__0"u8, 10, false, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, 1, default!, (rune)'_'),
    new("0__8"u8, 10, false, default!, 10, 1, default!, (rune)'_'),
    new("xy_z_"u8, 36, false, new nat(new global::go.math.big_package.Word[]{33 * 36 + 34}.slice()), 36, 2, default!, (rune)'_'),
    new("0"u8, 0, false, default!, 10, 1, default!, 0),
    new("0"u8, 36, false, default!, 36, 1, default!, 0),
    new("0"u8, 62, false, default!, 62, 1, default!, 0),
    new("1"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, 1, default!, 0),
    new("1"u8, 10, false, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, 1, default!, 0),
    new("0 "u8, 0, false, default!, 10, 1, default!, (rune)' '),
    new("00 "u8, 0, false, default!, 8, 1, default!, (rune)' '),
    new("0b1"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1}.slice()), 2, 1, default!, 0),
    new("0B11000101"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0xc5}.slice()), 2, 8, default!, 0),
    new("0B110001012"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0xc5}.slice()), 2, 8, default!, (rune)'2'),
    new("07"u8, 0, false, new nat(new global::go.math.big_package.Word[]{7}.slice()), 8, 1, default!, 0),
    new("08"u8, 0, false, default!, 10, 1, default!, (rune)'8'),
    new("08"u8, 10, false, new nat(new global::go.math.big_package.Word[]{8}.slice()), 10, 2, default!, 0),
    new("018"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1}.slice()), 8, 1, default!, (rune)'8'),
    new("0o7"u8, 0, false, new nat(new global::go.math.big_package.Word[]{7}.slice()), 8, 1, default!, 0),
    new("0o18"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1}.slice()), 8, 1, default!, (rune)'8'),
    new("0O17"u8, 0, false, new nat(new global::go.math.big_package.Word[]{15}.slice()), 8, 2, default!, 0),
    new("03271"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1721}.slice()), 8, 4, default!, 0),
    new("10ab"u8, 0, false, new nat(new global::go.math.big_package.Word[]{10}.slice()), 10, 2, default!, (rune)'a'),
    new("1234567890"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1234567890}.slice()), 10, 10, default!, 0),
    new("A"u8, 36, false, new nat(new global::go.math.big_package.Word[]{10}.slice()), 36, 1, default!, 0),
    new("A"u8, 37, false, new nat(new global::go.math.big_package.Word[]{36}.slice()), 37, 1, default!, 0),
    new("xyz"u8, 36, false, new nat(new global::go.math.big_package.Word[]{(33 * 36 + 34) * 36 + 35}.slice()), 36, 3, default!, 0),
    new("XYZ?"u8, 36, false, new nat(new global::go.math.big_package.Word[]{(33 * 36 + 34) * 36 + 35}.slice()), 36, 3, default!, (rune)'?'),
    new("XYZ?"u8, 62, false, new nat(new global::go.math.big_package.Word[]{(59 * 62 + 60) * 62 + 61}.slice()), 62, 3, default!, (rune)'?'),
    new("0x"u8, 16, false, default!, 16, 1, default!, (rune)'x'),
    new("0xdeadbeef"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0xdeadbeefU}.slice()), 16, 8, default!, 0),
    new("0XDEADBEEF"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0xdeadbeefU}.slice()), 16, 8, default!, 0),
    new("0."u8, 0, false, default!, 10, 1, default!, (rune)'.'),
    new("0."u8, 10, true, default!, 10, 0, default!, 0),
    new("0.1.2"u8, 10, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, -1, default!, (rune)'.'),
    new(".000"u8, 10, true, default!, 10, -3, default!, 0),
    new("12.3"u8, 10, true, new nat(new global::go.math.big_package.Word[]{123}.slice()), 10, -1, default!, 0),
    new("012.345"u8, 10, true, new nat(new global::go.math.big_package.Word[]{12345}.slice()), 10, -3, default!, 0),
    new("0.1"u8, 0, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 10, -1, default!, 0),
    new("0.1"u8, 2, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 2, -1, default!, 0),
    new("0.12"u8, 2, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 2, -1, default!, (rune)'2'),
    new("0b0.1"u8, 0, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 2, -1, default!, 0),
    new("0B0.12"u8, 0, true, new nat(new global::go.math.big_package.Word[]{1}.slice()), 2, -1, default!, (rune)'2'),
    new("0o0.7"u8, 0, true, new nat(new global::go.math.big_package.Word[]{7}.slice()), 8, -1, default!, 0),
    new("0O0.78"u8, 0, true, new nat(new global::go.math.big_package.Word[]{7}.slice()), 8, -1, default!, (rune)'8'),
    new("0xdead.beef"u8, 0, true, new nat(new global::go.math.big_package.Word[]{0xdeadbeefU}.slice()), 16, -4, default!, 0),
    new("1_000"u8, 0, false, new nat(new global::go.math.big_package.Word[]{1000}.slice()), 10, 4, default!, 0),
    new("0_466"u8, 0, false, new nat(new global::go.math.big_package.Word[]{310}.slice()), 8, 3, default!, 0),
    new("0o_600"u8, 0, false, new nat(new global::go.math.big_package.Word[]{384}.slice()), 8, 3, default!, 0),
    new("0x_f0_0d"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0xf00d}.slice()), 16, 4, default!, 0),
    new("0b1000_0001"u8, 0, false, new nat(new global::go.math.big_package.Word[]{0x81}.slice()), 2, 8, default!, 0),
    new("1_000.000_1"u8, 0, true, new nat(new global::go.math.big_package.Word[]{10000001}.slice()), 10, -4, default!, 0),
    new("0x_f00d.1e"u8, 0, true, new nat(new global::go.math.big_package.Word[]{0xf00d1e}.slice()), 16, -2, default!, 0),
    new("0x_f00d.1E2"u8, 0, true, new nat(new global::go.math.big_package.Word[]{0xf00d1e2}.slice()), 16, -3, default!, 0),
    new("0x_f00d.1eg"u8, 0, true, new nat(new global::go.math.big_package.Word[]{0xf00d1e}.slice()), 16, -2, default!, (rune)'g')
}.slice(); }

public static void TestScanBase(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in natScanTests) {
        var r = strings.NewReader(a.s);
        var (x, b, count, err) = ((global::go.math.big_package.nat)default!).scan(new big_test_package.strings_ReaderжByteScanner(r), a.@base, a.frac);
        if (!AreEqual(err, a.err)) {
            Ꮡt.Errorf("scan%+v\n\tgot error = %v; want %v"u8, a, err, a.err);
        }
        if (x.cmp(a.x) != 0) {
            Ꮡt.Errorf("scan%+v\n\tgot z = %v; want %v"u8, a, x, a.x);
        }
        if (b != a.b) {
            Ꮡt.Errorf("scan%+v\n\tgot b = %d; want %d"u8, a, b, a.@base);
        }
        if (count != a.count) {
            Ꮡt.Errorf("scan%+v\n\tgot count = %d; want %d"u8, a, count, a.count);
        }
        (var next, _, err) = r.ReadRune();
        if (AreEqual(err, io.EOF)) {
            next = 0;
            err = default!;
        }
        if (err == default! && next != a.next) {
            Ꮡt.Errorf("scan%+v\n\tgot next = %q; want %q"u8, a, next, a.next);
        }
    }
}

internal static @string pi = "3"u8 + "14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651"u8 + "32823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461"u8 + "28475648233786783165271201909145648566923460348610454326648213393607260249141273724587006606315588174881520920"u8 + "96282925409171536436789259036001133053054882046652138414695194151160943305727036575959195309218611738193261179"u8 + "31051185480744623799627495673518857527248912279381830119491298336733624406566430860213949463952247371907021798"u8 + "60943702770539217176293176752384674818467669405132000568127145263560827785771342757789609173637178721468440901"u8 + "22495343014654958537105079227968925892354201995611212902196086403441815981362977477130996051870721134999999837"u8 + "29780499510597317328160963185950244594553469083026425223082533446850352619311881710100031378387528865875332083"u8 + "81420617177669147303598253490428755468731159562863882353787593751957781857780532171226806613001927876611195909"u8 + "21642019893809525720106548586327886593615338182796823030195203530185296899577362259941389124972177528347913151"u8 + "55748572424541506959508295331168617278558890750983817546374649393192550604009277016711390098488240128583616035"u8 + "63707660104710181942955596198946767837449448255379774726847104047534646208046684259069491293313677028989152104"u8 + "75216205696602405803815019351125338243003558764024749647326391419927260426992279678235478163600934172164121992"u8 + "45863150302861829745557067498385054945885869269956909272107975093029553211653449872027559602364806654991198818"u8 + "34797753566369807426542527862551818417574672890977772793800081647060016145249192173217214772350141441973568548"u8 + "16136115735255213347574184946843852332390739414333454776241686251898356948556209921922218427255025425688767179"u8 + "04946016534668049886272327917860857843838279679766814541009538837863609506800642251252051173929848960841284886"u8 + "26945604241965285022210661186306744278622039194945047123713786960956364371917287467764657573962413890865832645"u8 + "99581339047802759009946576407895126946839835259570982582262052248940772671947826848260147699090264013639443745"u8 + "53050682034962524517493996514314298091906592509372216964615157098583874105978859597729754989301617539284681382"u8 + "68683868942774155991855925245953959431049972524680845987273644695848653836736222626099124608051243884390451244"u8 + "13654976278079771569143599770012961608944169486855584840635342207222582848864815845602850601684273945226746767"u8 + "88952521385225499546667278239864565961163548862305774564980355936345681743241125150760694794510965960940252288"u8 + "79710893145669136867228748940560101503308617928680920874760917824938589009714909675985261365549781893129784821"u8 + "68299894872265880485756401427047755513237964145152374623436454285844479526586782105114135473573952311342716610"u8 + "21359695362314429524849371871101457654035902799344037420073105785390621983874478084784896833214457138687519435"u8 + "06430218453191048481005370614680674919278191197939952061419663428754440643745123718192179998391015919561814675"u8 + "14269123974894090718649423196156794520809514655022523160388193014209376213785595663893778708303906979207734672"u8 + "21825625996615014215030680384477345492026054146659252014974428507325186660021324340881907104863317346496514539"u8 + "05796268561005508106658796998163574736384052571459102897064140110971206280439039759515677157700420337869936007"u8 + "23055876317635942187312514712053292819182618612586732157919841484882916447060957527069572209175671167229109816"u8 + "90915280173506712748583222871835209353965725121083579151369882091444210067510334671103141267111369908658516398"u8 + "31501970165151168517143765761835155650884909989859982387345528331635507647918535893226185489632132933089857064"u8 + "20467525907091548141654985946163718027098199430992448895757128289059232332609729971208443357326548938239119325"u8 + "97463667305836041428138830320382490375898524374417029132765618093773444030707469211201913020330380197621101100"u8 + "44929321516084244485963766983895228684783123552658213144957685726243344189303968642624341077322697802807318915"u8 + "44110104468232527162010526522721116603966655730925471105578537634668206531098965269186205647693125705863566201"u8 + "85581007293606598764861179104533488503461136576867532494416680396265797877185560845529654126654085306143444318"u8 + "58676975145661406800700237877659134401712749470420562230538994561314071127000407854733269939081454664645880797"u8 + "27082668306343285878569830523580893306575740679545716377525420211495576158140025012622859413021647155097925923"u8 + "09907965473761255176567513575178296664547791745011299614890304639947132962107340437518957359614589019389713111"u8 + "79042978285647503203198691514028708085990480109412147221317947647772622414254854540332157185306142288137585043"u8 + "06332175182979866223717215916077166925474873898665494945011465406284336639379003976926567214638530673609657120"u8 + "91807638327166416274888800786925602902284721040317211860820419000422966171196377921337575114959501566049631862"u8 + "94726547364252308177036751590673502350728354056704038674351362222477158915049530984448933309634087807693259939"u8 + "78054193414473774418426312986080998886874132604721569516239658645730216315981931951673538129741677294786724229"u8 + "24654366800980676928238280689964004824354037014163149658979409243237896907069779422362508221688957383798623001"u8 + "59377647165122893578601588161755782973523344604281512627203734314653197777416031990665541876397929334419521541"u8 + "34189948544473456738316249934191318148092777710386387734317720754565453220777092120190516609628049092636019759"u8 + "88281613323166636528619326686336062735676303544776280350450777235547105859548702790814356240145171806246436267"u8 + "94561275318134078330336254232783944975382437205835311477119926063813346776879695970309833913077109870408591337"u8;

// Test case for BenchmarkScanPi.
public static void TestScanPi(ж<testing.T> Ꮡt) {
    global::go.math.big_package.nat x = default!;
    var (z, _, _, err) = x.scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(pi)), 10, false);
    if (err != default!) {
        Ꮡt.Errorf("scanning pi: %s"u8, err);
    }
    {
        @string s = ((@string)z.utoa(10)); if (s != pi) {
            Ꮡt.Errorf("scanning pi: got %s"u8, s);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestScanPiParallel(ж<testing.T> Ꮡt) {
    const nint n = 2;
    var c = new channel<nint>(0);
    for (nint i = 0; i < n; i++) {
        var cʗ1 = c;
        goǃ(() => {
            TestScanPi(Ꮡt);
            cʗ1.ᐸꟷ(0);
        });
    }
    for (nint i = 0; i < n; i++) {
        ᐸꟷ(c);
    }
}

public static void BenchmarkScanPi(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        global::go.math.big_package.nat x = default!;
        x.scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(pi)), 10, false);
    }
}

public static void BenchmarkStringPiParallel(ж<testing.B> Ꮡb) {
    global::go.math.big_package.nat x = default!;
    (x, _, _, _) = x.scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(pi)), 0, false);
    if (((sstring)x.utoa(10)) != pi) {
        throw panic("benchmark incorrect: conversion failed");
    }
    var xʗ1 = x;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            xʗ1.utoa(10);
        }
    });
}

public static void BenchmarkScan(ж<testing.B> Ꮡb) {
    UntypedInt x = 10;
    foreach (var (_, @base) in new nint[]{2, 8, 10, 16}.slice()) {
        foreach (var (_, y) in new global::go.math.big_package.Word[]{10, 100, 1000, 10000, 100000}.slice()) {
            if (isRaceBuilder && y > 1000) {
                continue;
            }
            Ꮡb.Run(fmt.Sprintf("%d/Base%d"u8, y, @base), (ж<testing.B> bΔ1) => {
                bΔ1.StopTimer();
                global::go.math.big_package.nat z = default!;
                z = z.expWW(x, y);
                var s = z.utoa(@base);
                {
                    var t = itoa(z, @base); if (!bytes_package.Equal(s, t)) {
                        bΔ1.Fatalf("scanning: got %s; want %s"u8, s, t);
                    }
                }
                bΔ1.StartTimer();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    z.scan(new big_test_package.bytes_ReaderжByteScanner(bytes_package.NewReader(s)), @base, false);
                }
            });
        }
    }
}

public static void BenchmarkString(ж<testing.B> Ꮡb) {
    UntypedInt x = 10;
    foreach (var (_, @base) in new nint[]{2, 8, 10, 16}.slice()) {
        foreach (var (_, y) in new global::go.math.big_package.Word[]{10, 100, 1000, 10000, 100000}.slice()) {
            if (isRaceBuilder && y > 1000) {
                continue;
            }
            Ꮡb.Run(fmt.Sprintf("%d/Base%d"u8, y, @base), (ж<testing.B> bΔ1) => {
                bΔ1.StopTimer();
                global::go.math.big_package.nat z = default!;
                z = z.expWW(x, y);
                z.utoa(@base); // warm divisor cache
                bΔ1.StartTimer();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    _ = z.utoa(@base);
                }
            });
        }
    }
}

public static void BenchmarkLeafSize(ж<testing.B> Ꮡb) {
    for (nint nᴛ1 = 0; nᴛ1 <= 16; nᴛ1++) {
        var n = nᴛ1;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            LeafSizeHelper(bΔ1, 10, n);
        });
    }
    // Try some large lengths
    foreach (var (_, n) in new nint[]{32, 64}.slice()) {
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ2) => {
            LeafSizeHelper(bΔ2, 10, n);
        });
    }
}

public static void LeafSizeHelper(ж<testing.B> Ꮡb, nint @base, nint size) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    nint originalLeafSize = leafSize;
    resetTable(cacheBase10.table[..]);
    leafSize = size;
    b.StartTimer();
    for (nint d = 1; d <= 10000; d *= 10) {
        b.StopTimer();
        global::go.math.big_package.nat z = default!;
        z = z.expWW(((global::go.math.big_package.Word)(nuint)@base), ((global::go.math.big_package.Word)(nuint)d)); // build target number
        _ = z.utoa(@base); // warm divisor cache
        b.StartTimer();
        for (nint i = 0; i < b.N; i++) {
            _ = z.utoa(@base);
        }
    }
    b.StopTimer();
    resetTable(cacheBase10.table[..]);
    leafSize = originalLeafSize;
    b.StartTimer();
}

internal static void resetTable(slice<global::go.math.big_package.divisor> table) {
    if (table != default! && table[0].bbb != default!) {
        for (nint i = 0; i < len(table); i++) {
            table[i].bbb = default!;
            table[i].nbits = 0;
            table[i].ndigits = 0;
        }
    }
}

public static void TestStringPowers(ж<testing.T> Ꮡt) {
    global::go.math.big_package.Word p = default!;
    for (nint b = 2; b <= 16; b++) {
        for (p = 0; p <= 512; p++) {
            if (testing.Short() && p > 10) {
                break;
            }
            var x = ((global::go.math.big_package.nat)default!).expWW(((global::go.math.big_package.Word)(nuint)b), p);
            var xs = x.utoa(b);
            var xs2 = itoa(x, b);
            if (!bytes_package.Equal(xs, xs2)) {
                Ꮡt.Errorf("failed at %d ** %d in base %d: %s != %s"u8, b, p, b, xs, xs2);
            }
        }
        if (b >= 3 && testing.Short()) {
            break;
        }
    }
}

} // end big_internal_test_package
