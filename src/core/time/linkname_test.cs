// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δtesting = testing_package;
using Δtime = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname
using static go.time_internal_test_package;

partial class time_test_package {

//go:linkname timeAbs time.Time.abs
[global::System.Diagnostics.StackTraceHidden] internal static uint64 timeAbs(Δtime.Time _) {
    return Δtime.legacyTimeTimeAbs(_);
}

//go:linkname absClock time.absClock
[global::System.Diagnostics.StackTraceHidden] internal static (nint hour, nint min, nint sec) absClock(uint64 _) {
    var (ᴛ1, ᴛ2, ᴛ3) = Δtime.legacyAbsClock(_);
    return (ᴛ1, ᴛ2, ᴛ3);
}

//go:linkname absDate time.absDate
[global::System.Diagnostics.StackTraceHidden] internal static (nint year, timeꓸMonth month, nint day, nint yday) absDate(uint64 _Δp0, bool _Δp1) {
    var (ᴛ1, ᴛ2, ᴛ3, ᴛ4) = Δtime.legacyAbsDate(_Δp0, _Δp1);
    return (ᴛ1, ᴛ2, ᴛ3, ᴛ4);
}

public static void TestLinkname(ж<Δtesting.T> Ꮡt) {
    var tm = Δtime.Date(2006, Δtime.January, 2, 15, 4, 5, 6, Δtime.ΔUTC);
    var abs = timeAbs(tm);
    // wantAbs should be Jan 1 based, not Mar 1 based.
    // See absolute time description in time.go.
    UntypedInt wantAbs = 9223372029851535845; // NOT 9223372029877973939
    if (abs != wantAbs) {
        Ꮡt.Fatalf("timeAbs(2006-01-02 15:04:05 UTC) = %d, want %d"u8, abs, (uint64)wantAbs);
    }
    var (year, month, day, yday) = absDate(abs, true);
    if (year != 2006 || month != Δtime.January || day != 2 || yday != 1) {
        Ꮡt.Errorf("absDate() = %v, %v, %v, %v, want 2006, January, 2, 1"u8, year, month, day, yday);
    }
    var (hour, min, sec) = absClock(abs);
    if (hour != 15 || min != 4 || sec != 5) {
        Ꮡt.Errorf("absClock() = %v, %v, %v, 15, 4, 5"u8, hour, min, sec);
    }
}

} // end time_test_package
