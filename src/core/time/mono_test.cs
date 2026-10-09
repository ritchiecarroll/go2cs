// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;
using Δtesting = testing_package;
using static time_package;
using static go.time_internal_test_package;
using Δtime = time_package;

partial class time_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string after1ˢ = "<-After(1)"u8;
internal static readonly @string tick1ˢ = "<-Tick(1)"u8;
internal static readonly @string date200911230000Utcˢ = "Date(2009, 11, 23, 0, 0, 0, 0, UTC)"u8;
internal static readonly @string parseUnixDateSatMar71106ˢ = @"Parse(UnixDate, ""Sat Mar  7 11:06:39 PST 2015"")"u8;
internal static readonly @string unix14860573710ˢ = "Unix(1486057371, 0)"u8;
internal static readonly @string nowˢ = "Now()"u8;
internal static readonly @string tuAdd1ˢ = "tu.Add(1)"u8;
internal static readonly @string tuInUtcˢ = "tu.In(UTC)"u8;
internal static readonly @string tuAddDate111ˢ = "tu.AddDate(1, 1, 1)"u8;
internal static readonly @string tuAddDate000ˢ = "tu.AddDate(0, 0, 0)"u8;
internal static readonly @string tuLocalˢ = "tu.Local()"u8;
internal static readonly @string tuUtcˢ = "tu.UTC()"u8;
internal static readonly @string tuRound2ˢ = "tu.Round(2)"u8;
internal static readonly @string tuTruncate2ˢ = "tu.Truncate(2)"u8;
internal static readonly @string tmAdd1ˢ = "tm.Add(1)"u8;
internal static readonly @string tmAddDate111ˢ = "tm.AddDate(1, 1, 1)"u8;
internal static readonly @string tmAddDate000ˢ = "tm.AddDate(0, 0, 0)"u8;
internal static readonly @string tmInUtcˢ = "tm.In(UTC)"u8;
internal static readonly @string tmLocalˢ = "tm.Local()"u8;
internal static readonly @string tmUtcˢ = "tm.UTC()"u8;
internal static readonly @string tmRound2ˢ = "tm.Round(2)"u8;
internal static readonly @string tmTruncate2ˢ = "tm.Truncate(2)"u8;

public static void TestHasMonotonicClock(ж<Δtesting.T> Ꮡt) {
    void yes(@string expr, Δtime.Time ttʗp) {
        ref var tt = ref heap(ttʗp, out var Ꮡtt);
        if (time_internal_test_package.GetMono(Ꮡtt) == 0) {
            Ꮡt.Errorf("%s: missing monotonic clock reading"u8, expr);
        }
    }
    void no(@string expr, Δtime.Time ttʗp) {
        ref var tt = ref heap(ttʗp, out var Ꮡtt);
        if (time_internal_test_package.GetMono(Ꮡtt) != 0) {
            Ꮡt.Errorf("%s: unexpected monotonic clock reading"u8, expr);
        }
    }
    yes(after1ˢ, ᐸꟷ(After(1)));
    var ticker = NewTicker(1);
    yes(tick1ˢ, ᐸꟷ((~ticker).C));
    ticker.Stop();
    no(date200911230000Utcˢ, Date(2009, 11, 23, 0, 0, 0, 0, ΔUTC));
    var (tp, _) = Parse(UnixDate, satMar7110639Pst2015ˢ2);
    no(parseUnixDateSatMar71106ˢ, tp);
    no(unix14860573710ˢ, Unix(1486057371, 0));
    yes(nowˢ, Now());
    var tu = Unix(1486057371, 0);
    ref var tm = ref heap<Δtime.Time>(out var Ꮡtm);
    tm = tu;
    time_internal_test_package.SetMono(Ꮡtm, 123456);
    no("tu"u8, tu);
    yes("tm"u8, tm);
    no(tuAdd1ˢ, tu.Add(1));
    no(tuInUtcˢ, tu.In(ΔUTC));
    no(tuAddDate111ˢ, tu.AddDate(1, 1, 1));
    no(tuAddDate000ˢ, tu.AddDate(0, 0, 0));
    no(tuLocalˢ, tu.Local());
    no(tuUtcˢ, tu.UTC());
    no(tuRound2ˢ, tu.Round(2));
    no(tuTruncate2ˢ, tu.Truncate(2));
    yes(tmAdd1ˢ, tm.Add(1));
    no(tmAddDate111ˢ, tm.AddDate(1, 1, 1));
    no(tmAddDate000ˢ, tm.AddDate(0, 0, 0));
    no(tmInUtcˢ, tm.In(ΔUTC));
    no(tmLocalˢ, tm.Local());
    no(tmUtcˢ, tm.UTC());
    no(tmRound2ˢ, tm.Round(2));
    no(tmTruncate2ˢ, tm.Truncate(2));
}

public static void TestMonotonicAdd(ж<Δtesting.T> Ꮡt) {
    ref var tm = ref heap<Δtime.Time>(out var Ꮡtm);
    tm = Unix(1486057371, 123456);
    time_internal_test_package.SetMono(Ꮡtm, 123456789012345L);
    ref var t2 = ref heap<Δtime.Time>(out var Ꮡt2);
    t2 = tm.Add(100000000);
    if (t2.Nanosecond() != 100123456) {
        Ꮡt.Errorf("t2.Nanosecond() = %d, want 100123456"u8, t2.Nanosecond());
    }
    if (time_internal_test_package.GetMono(Ꮡt2) != 123456889012345L) {
        Ꮡt.Errorf("t2.mono = %d, want 123456889012345"u8, time_internal_test_package.GetMono(Ꮡt2));
    }
    ref var t3 = ref heap<Δtime.Time>(out var Ꮡt3);
    t3 = tm.Add(-9000000000000000000); // wall now out of range
    if (t3.Nanosecond() != 123456) {
        Ꮡt.Errorf("t3.Nanosecond() = %d, want 123456"u8, t3.Nanosecond());
    }
    if (time_internal_test_package.GetMono(Ꮡt3) != 0) {
        Ꮡt.Errorf("t3.mono = %d, want 0 (wall time out of range for monotonic reading)"u8, time_internal_test_package.GetMono(Ꮡt3));
    }
    ref var t4 = ref heap<Δtime.Time>(out var Ꮡt4);
    t4 = tm.Add(+9000000000000000000); // wall now out of range
    if (t4.Nanosecond() != 123456) {
        Ꮡt.Errorf("t4.Nanosecond() = %d, want 123456"u8, t4.Nanosecond());
    }
    if (time_internal_test_package.GetMono(Ꮡt4) != 0) {
        Ꮡt.Errorf("t4.mono = %d, want 0 (wall time out of range for monotonic reading)"u8, time_internal_test_package.GetMono(Ꮡt4));
    }
    var tn = Now();
    var tn1 = tn.Add((Δtime.Duration)(3600000000000L));
    Sleep(100 * Millisecond);
    var d = Until(tn1);
    if (d < (Δtime.Duration)(3540000000000L)) {
        Ꮡt.Errorf("Until(Now().Add(1*Hour)) = %v, wanted at least 59m"u8, d);
    }
    var now = Now();
    if (now.After(tn1)) {
        Ꮡt.Errorf("Now().After(Now().Add(1*Hour)) = true, want false"u8);
    }
    if (!tn1.After(now)) {
        Ꮡt.Errorf("Now().Add(1*Hour).After(now) = false, want true"u8);
    }
    if (tn1.Before(now)) {
        Ꮡt.Errorf("Now().Add(1*Hour).Before(Now()) = true, want false"u8);
    }
    if (!now.Before(tn1)) {
        Ꮡt.Errorf("Now().Before(Now().Add(1*Hour)) = false, want true"u8);
    }
    {
        nint got = now.Compare(tn1);
        nint want = -1; if (got != want) {
            Ꮡt.Errorf("Now().Compare(Now().Add(1*Hour)) = %d, want %d"u8, got, want);
        }
    }
    {
        nint got = tn1.Compare(now);
        nint want = 1; if (got != want) {
            Ꮡt.Errorf("Now().Add(1*Hour).Compare(Now()) = %d, want %d"u8, got, want);
        }
    }
}

public static void TestMonotonicSub(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var t1 = ref heap<Δtime.Time>(out var Ꮡt1);
    t1 = Unix(1483228799, 995000000);
    time_internal_test_package.SetMono(Ꮡt1, 123456789012345L);
    ref var t2 = ref heap<Δtime.Time>(out var Ꮡt2);
    t2 = Unix(1483228799, 5000000);
    time_internal_test_package.SetMono(Ꮡt2, 123456799012345L);
    ref var t3 = ref heap<Δtime.Time>(out var Ꮡt3);
    t3 = Unix(1483228799, 995000000);
    time_internal_test_package.SetMono(Ꮡt3, 123457789012345L);
    ref var t1w = ref heap<Δtime.Time>(out var Ꮡt1w);
    t1w = t1.AddDate(0, 0, 0);
    if (time_internal_test_package.GetMono(Ꮡt1w) != 0) {
        Ꮡt.Fatalf("AddDate didn't strip monotonic clock reading"u8);
    }
    ref var t2w = ref heap<Δtime.Time>(out var Ꮡt2w);
    t2w = t2.AddDate(0, 0, 0);
    if (time_internal_test_package.GetMono(Ꮡt2w) != 0) {
        Ꮡt.Fatalf("AddDate didn't strip monotonic clock reading"u8);
    }
    ref var t3w = ref heap<Δtime.Time>(out var Ꮡt3w);
    t3w = t3.AddDate(0, 0, 0);
    if (time_internal_test_package.GetMono(Ꮡt3w) != 0) {
        Ꮡt.Fatalf("AddDate didn't strip monotonic clock reading"u8);
    }
    void sub(@string txs, @string tys, Δtime.Time tx, Δtime.Time txw, Δtime.Time ty, Δtime.Time tyw, Δtime.Duration d, Δtime.Duration dw) {
        void check(@string expr, Δtime.Duration dΔ1, Δtime.Duration want) {
            if (dΔ1 != want) {
                Ꮡt.Errorf("%s = %v, want %v"u8, expr, dΔ1, want);
            }
        }
        check(txs + ".Sub("u8 + tys + ")"u8, tx.Sub(ty), d);
        check(txs + "w.Sub("u8 + tys + ")"u8, txw.Sub(ty), dw);
        check(txs + ".Sub("u8 + tys + "w)"u8, tx.Sub(tyw), dw);
        check(txs + "w.Sub("u8 + tys + "w)"u8, txw.Sub(tyw), dw);
    }
    sub("t1"u8, "t1"u8, t1, t1w, t1, t1w, 0, 0);
    sub("t1"u8, "t2"u8, t1, t1w, t2, t2w, -10 * Millisecond, 990 * Millisecond);
    sub("t1"u8, "t3"u8, t1, t1w, t3, t3w, -1000 * Millisecond, 0);
    sub("t2"u8, "t1"u8, t2, t2w, t1, t1w, 10 * Millisecond, -990 * Millisecond);
    sub("t2"u8, "t2"u8, t2, t2w, t2, t2w, 0, 0);
    sub("t2"u8, "t3"u8, t2, t2w, t3, t3w, -990 * Millisecond, -990 * Millisecond);
    sub("t3"u8, "t1"u8, t3, t3w, t1, t1w, 1000 * Millisecond, 0);
    sub("t3"u8, "t2"u8, t3, t3w, t2, t2w, 990 * Millisecond, 990 * Millisecond);
    sub("t3"u8, "t3"u8, t3, t3w, t3, t3w, 0, 0);
    void cmp(@string txs, @string tys, Δtime.Time tx, Δtime.Time txw, Δtime.Time ty, Δtime.Time tyw, nint c, nint cw) {
        void check(@string expr, any b, any want) {
            if (!AreEqual(b, want)) {
                Ꮡt.Errorf("%s = %v, want %v"u8, expr, b, want);
            }
        }
        check(txs + ".After("u8 + tys + ")"u8, tx.After(ty), c > 0);
        check(txs + "w.After("u8 + tys + ")"u8, txw.After(ty), cw > 0);
        check(txs + ".After("u8 + tys + "w)"u8, tx.After(tyw), cw > 0);
        check(txs + "w.After("u8 + tys + "w)"u8, txw.After(tyw), cw > 0);
        check(txs + ".Before("u8 + tys + ")"u8, tx.Before(ty), c < 0);
        check(txs + "w.Before("u8 + tys + ")"u8, txw.Before(ty), cw < 0);
        check(txs + ".Before("u8 + tys + "w)"u8, tx.Before(tyw), cw < 0);
        check(txs + "w.Before("u8 + tys + "w)"u8, txw.Before(tyw), cw < 0);
        check(txs + ".Equal("u8 + tys + ")"u8, tx.Equal(ty), c == 0);
        check(txs + "w.Equal("u8 + tys + ")"u8, txw.Equal(ty), cw == 0);
        check(txs + ".Equal("u8 + tys + "w)"u8, tx.Equal(tyw), cw == 0);
        check(txs + "w.Equal("u8 + tys + "w)"u8, txw.Equal(tyw), cw == 0);
        check(txs + ".Compare("u8 + tys + ")"u8, tx.Compare(ty), c);
        check(txs + "w.Compare("u8 + tys + ")"u8, txw.Compare(ty), cw);
        check(txs + ".Compare("u8 + tys + "w)"u8, tx.Compare(tyw), cw);
        check(txs + "w.Compare("u8 + tys + "w)"u8, txw.Compare(tyw), cw);
    }
    cmp("t1"u8, "t1"u8, t1, t1w, t1, t1w, 0, 0);
    cmp("t1"u8, "t2"u8, t1, t1w, t2, t2w, -1, +1);
    cmp("t1"u8, "t3"u8, t1, t1w, t3, t3w, -1, 0);
    cmp("t2"u8, "t1"u8, t2, t2w, t1, t1w, +1, -1);
    cmp("t2"u8, "t2"u8, t2, t2w, t2, t2w, 0, 0);
    cmp("t2"u8, "t3"u8, t2, t2w, t3, t3w, -1, -1);
    cmp("t3"u8, "t1"u8, t3, t3w, t1, t1w, +1, 0);
    cmp("t3"u8, "t2"u8, t3, t3w, t2, t2w, +1, +1);
    cmp("t3"u8, "t3"u8, t3, t3w, t3, t3w, 0, 0);
}

public static void TestMonotonicOverflow(ж<Δtesting.T> Ꮡt) {
    ref var t1 = ref heap<Δtime.Time>(out var Ꮡt1);
    t1 = Now().Add((Δtime.Duration)(-30000000000L));
    var d = Until(t1);
    if (d < (Δtime.Duration)(-35000000000L) || (Δtime.Duration)(-30000000000L) < d) {
        Ꮡt.Errorf("Until(Now().Add(-30s)) = %v, want roughly -30s (-35s to -30s)"u8, d);
    }
    t1 = Now().Add((Δtime.Duration)(30000000000L));
    d = Until(t1);
    if (d < (Δtime.Duration)(25000000000L) || (Δtime.Duration)(30000000000L) < d) {
        Ꮡt.Errorf("Until(Now().Add(-30s)) = %v, want roughly 30s (25s to 30s)"u8, d);
    }
    var t0 = Now();
    t1 = t0.Add((Δtime.Duration)(9223372036854775807L));
    if (time_internal_test_package.GetMono(Ꮡt1) != 0) {
        Ꮡt.Errorf("Now().Add(maxDuration) has monotonic clock reading (%v => %v %d %d)"u8, t0.String(), t1.String(), t0.Unix(), t1.Unix());
    }
    var t2 = t1.Add(-(Δtime.Duration)(9223372036854775807L));
    d = Since(t2);
    if (d < (Δtime.Duration)(-10000000000L) || (Δtime.Duration)(10000000000L) < d) {
        Ꮡt.Errorf("Since(Now().Add(max).Add(-max)) = %v, want [-10s, 10s]"u8, d);
    }
    t0 = Now();
    t1 = t0.Add((Δtime.Duration)(3600000000000L));
    Sleep(100 * Millisecond);
    t2 = Now().Add((Δtime.Duration)(-5000000000L));
    if (!t1.After(t2)) {
        Ꮡt.Errorf("Now().Add(1*Hour).After(Now().Add(-5*Second)) = false, want true\nt1=%v\nt2=%v"u8, t1, t2);
    }
    if (t2.After(t1)) {
        Ꮡt.Errorf("Now().Add(-5*Second).After(Now().Add(1*Hour)) = true, want false\nt1=%v\nt2=%v"u8, t1, t2);
    }
    if (t1.Before(t2)) {
        Ꮡt.Errorf("Now().Add(1*Hour).Before(Now().Add(-5*Second)) = true, want false\nt1=%v\nt2=%v"u8, t1, t2);
    }
    if (!t2.Before(t1)) {
        Ꮡt.Errorf("Now().Add(-5*Second).Before(Now().Add(1*Hour)) = false, want true\nt1=%v\nt2=%v"u8, t1, t2);
    }
    {
        nint got = t1.Compare(t2);
        nint want = 1; if (got != want) {
            Ꮡt.Errorf("Now().Add(1*Hour).Compare(Now().Add(-5*Second)) = %d, want %d\nt1=%v\nt2=%v"u8, got, want, t1, t2);
        }
    }
    {
        nint got = t2.Compare(t1);
        nint want = -1; if (got != want) {
            Ꮡt.Errorf("Now().Add(-5*Second).Before(Now().Add(1*Hour)) = %d, want %d\nt1=%v\nt2=%v"u8, got, want, t1, t2);
        }
    }
}


partial struct monotonicStringTestsᴛ1 /*dyn*/ {
    internal int64 mono;
    internal @string want;
}
internal static slice<monotonicStringTestsᴛ1> monotonicStringTests = new monotonicStringTestsᴛ1[]{
    new(0, "m=+0.000000000"u8),
    new(123456789, "m=+0.123456789"u8),
    new(-123456789, "m=-0.123456789"u8),
    new(123456789000L, "m=+123.456789000"u8),
    new(-123456789000L, "m=-123.456789000"u8),
    new(9000000000000000000, "m=+9000000000.000000000"u8),
    new(-9000000000000000000, "m=-9000000000.000000000"u8),
    new(-9223372036854775808L, "m=-9223372036.854775808"u8)
}.slice();

public static void TestMonotonicString(ж<Δtesting.T> Ꮡt) {
    var t1 = Now();
    Ꮡt.Logf("Now() = %v"u8, t1);
    foreach (var (_, tt) in monotonicStringTests) {
        ref var t1Δ1 = ref heap<Δtime.Time>(out var Ꮡt1Δ1);
        t1Δ1 = Now();
        time_internal_test_package.SetMono(Ꮡt1Δ1, tt.mono);
        @string s = t1Δ1.String();
        @string got = s.slice(strings.LastIndex(s, " "u8) + 1);
        if (got != tt.want) {
            Ꮡt.Errorf("with mono=%d: got %q; want %q"u8, tt.mono, got, tt.want);
        }
    }
}

} // end time_test_package
