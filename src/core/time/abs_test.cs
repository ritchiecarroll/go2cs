// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.time_package;
using ꓸꓸꓸany = Span<any>;

partial class time_internal_test_package {

public partial interface testingT {
    void Error(params ꓸꓸꓸany argsʗp);
    void Errorf(@string format, params ꓸꓸꓸany argsʗp);
    void Fail();
    void FailNow();
    bool Failed();
    void Fatal(params ꓸꓸꓸany argsʗp);
    void Fatalf(@string format, params ꓸꓸꓸany argsʗp);
    void Helper();
    void Log(params ꓸꓸꓸany argsʗp);
    void Logf(@string format, params ꓸꓸꓸany argsʗp);
    void Skip(params ꓸꓸꓸany argsʗp);
    void SkipNow();
    void Skipf(@string format, params ꓸꓸꓸany argsʗp);
}


partial struct InternalTestsᴛ1 /*dyn*/ {
    public @string Name;
    public Action<testingT> Test;
}
public static slice<InternalTestsᴛ1> InternalTests = new InternalTestsᴛ1[]{
    new("AbsDaysSplit"u8, testAbsDaysSplit),
    new("AbsYdaySplit"u8, testAbsYdaySplit),
    new("AbsDate"u8, testAbsDate),
    new("DateToAbsDays"u8, testDateToAbsDays),
    new("DaysIn"u8, testDaysIn),
    new("DaysBefore"u8, testDaysBefore)
}.slice();

internal static void testAbsDaysSplit(testingT t) {
    bool isLeap(uint64 year) => year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    nint bad = 0;
    var wantYear = (uint64)0;
    global::go.time_package.absYday wantYday = ((global::go.time_package.absYday)0);
    foreach (var days in range<global::go.time_package.absDays>(((global::go.time_package.absDays)1000000))) {
        var (century, cyear, yday) = days.split();
        if (century != ((global::go.time_package.absCentury)(wantYear / 100)) || cyear != ((global::go.time_package.absCyear)(nint)(wantYear % 100)) || yday != wantYday) {
            t.Errorf("absDays(%d).split() = %d, %d, %d, want %d, %d, %d"u8,
                days, century, cyear, yday,
                wantYear / 100, wantYear % 100, wantYday);
            {
                bad++; if (bad >= 20) {
                    t.Fatalf("too many errors"u8);
                }
            }
        }
        global::go.time_package.absYday end = ((global::go.time_package.absYday)365);
        if (isLeap(wantYear + 1)) {
            end = 366;
        }
        {
            wantYday++; if (wantYday == end) {
                wantYear++;
                wantYday = 0;
            }
        }
    }
}

internal static void testAbsYdaySplit(testingT t) {
    var ends = new nint[]{31, 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 29}.slice();
    nint bad = 0;
    global::go.time_package.absMonth wantMonth = ((global::go.time_package.absMonth)3);
    nint wantDay = 1;
    foreach (var yday in range(((global::go.time_package.absYday)366))) {
        var (month, day) = yday.split();
        if (month != wantMonth || day != wantDay) {
            t.Errorf("absYday(%d).split() = %d, %d, want %d, %d"u8, yday, month, day, wantMonth, wantDay);
            {
                bad++; if (bad >= 20) {
                    t.Fatalf("too many errors"u8);
                }
            }
        }
        {
            wantDay++; if (wantDay > ends[wantMonth - 3]) {
                wantMonth++;
                wantDay = 1;
            }
        }
    }
}

internal static void testAbsDate(testingT t) {
    var ends = new nint[]{31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31}.slice();
    bool isLeap(nint year) {
        var y = (uint64)year + (uint64)time_package.absoluteYears;
        return y % 4 == 0 && (y % 100 != 0 || y % 400 == 0);
    }
    nint wantYear = 0;
    global::go.time_package.ΔMonth wantMonth = March;
    nint wantMday = 1;
    nint wantYday = 31 + 29 + 1;
    nint bad = 0;
    var absoluteYears = (int64)time_package.absoluteYears;
    foreach (var days in range<global::go.time_package.absDays>(((global::go.time_package.absDays)1000000))) {
        var (year, month, mday) = days.date();
        year += (nint)absoluteYears;
        if (year != wantYear || month != wantMonth || mday != wantMday) {
            t.Errorf("days(%d).date() = %v, %v, %v, want %v, %v, %v"u8, days,
                year, month, mday,
                wantYear, wantMonth, wantMday);
            {
                bad++; if (bad >= 20) {
                    t.Fatalf("too many errors"u8);
                }
            }
        }
        (year, var yday) = days.yearYday();
        year += (nint)absoluteYears;
        if (year != wantYear || yday != wantYday) {
            t.Errorf("days(%d).yearYday() = %v, %v, want %v, %v, "u8, days,
                year, yday,
                wantYear, wantYday);
            {
                bad++; if (bad >= 20) {
                    t.Fatalf("too many errors"u8);
                }
            }
        }
        {
            wantMday++; if (wantMday == ends[wantMonth - 1] + 1 || wantMonth == February && wantMday == 29 && !isLeap(year)) {
                wantMonth++;
                wantMday = 1;
            }
        }
        wantYday++;
        if (wantMonth == December + 1) {
            wantYear++;
            wantMonth = January;
            wantMday = 1;
            wantYday = 1;
        }
    }
}

internal static void testDateToAbsDays(testingT t) {
    bool isLeap(int64 year) => year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    var wantDays = ((global::go.time_package.absDays)marchThruDecember);
    nint bad = 0;
    for (var year = (int64)1; year < 10000; year++) {
        var days = dateToAbsDays(year - (int64)absoluteYears, January, 1);
        if (days != wantDays) {
            t.Errorf("dateToAbsDays(abs %d, Jan, 1) = %d, want %d"u8, year, days, wantDays);
            {
                bad++; if (bad >= 20) {
                    t.Fatalf("too many errors"u8);
                }
            }
        }
        wantDays += 365;
        if (isLeap(year)) {
            wantDays++;
        }
    }
}

internal static void testDaysIn(testingT t) {
    bool isLeap(nint year) => year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
    var want = new nint[]{0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31}.slice();
    nint bad = 0;
    for (nint year = 0; year <= 1600; year++) {
        for (global::go.time_package.ΔMonth m = January; m <= December; m++) {
            nint w = want[m];
            if (m == February && isLeap(year)) {
                w++;
            }
            nint d = daysIn(m, year - 800);
            if (d != w) {
                t.Errorf("daysIn(%v, %d) = %d, want %d"u8, m, year - 800, d, w);
                {
                    bad++; if (bad >= 20) {
                        t.Fatalf("too many errors"u8);
                    }
                }
            }
        }
    }
}

internal static void testDaysBefore(testingT t) {
    foreach (var (m, want) in new nint[]{0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365}.slice()) {
        nint d = daysBefore(((global::go.time_package.ΔMonth)(m + 1)));
        if (d != want) {
            t.Errorf("daysBefore(%d) = %d, want %d"u8, m, d, want);
        }
    }
}

} // end time_internal_test_package
