// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using registry = @internal.syscall.windows.registry_package;
using Δtesting = testing_package;
using static time_package;
using @internal.syscall.windows;
using static go.time_internal_test_package;
using Δtime = time_package;

partial class time_test_package {

internal static void testZoneAbbr(ж<Δtesting.T> t) {
    var t1 = Now();
    // discard nsec
    t1 = Date(t1.Year(), t1.Month(), t1.Day(), t1.Hour(), t1.Minute(), t1.Second(), 0, t1.Location());

    var (t2, err) = Parse(RFC1123, t1.Format(RFC1123));
    if (err != nil) {
        t.Fatalf("Parse failed: %v", err);
    }
    if (t1 != t2) {
        t.Fatalf("t1 (%v) is not equal to t2 (%v)", t1, t2);
    }
}

public static void TestUSPacificZoneAbbr(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        time_internal_test_package.ForceUSPacificFromTZIForTesting(); // reset the Once to trigger the race
        defer(time_internal_test_package.ForceUSPacificForTesting, ref ᒐ);
        testZoneAbbr(t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAusZoneAbbr(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        time_internal_test_package.ForceAusFromTZIForTesting();
        defer(time_internal_test_package.ForceUSPacificForTesting, ref ᒐ);
        testZoneAbbr(t);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string muiStdˢ => ref muiStdˢᵇ;
private static readonly @string muiStdˢᵇ = "MUI_Std"u8;
internal static ref readonly @string muiDltˢ => ref muiDltˢᵇ;
private static readonly @string muiDltˢᵇ = "MUI_Dlt"u8;
internal static ref readonly @string stdˢ => ref stdˢᵇ;
private static readonly @string stdˢᵇ = "Std"u8;
internal static ref readonly @string dltˢ => ref dltˢᵇ;
private static readonly @string dltˢᵇ = "Dlt"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "Central Europe Standard Time"u8;

public static void TestToEnglishName(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        @string want = wantᶜ;
        var (k, err) = registry.OpenKey(registry.LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones\" + want, registry.READ);
        if (err != nil) {
            t.Fatalf("cannot open CEST time zone information from registry: %s", err);
        }
        defer(() => k.Close(), ref ᒐ);

        @string std = default;
        @string dlt = default;
        // Try MUI_Std and MUI_Dlt first, fallback to Std and Dlt if *any* error occurs
        (std, err) = k.GetMUIStringValue(muiStdˢ);
        if (err == nil) {
            (dlt, err) = k.GetMUIStringValue(muiDltˢ);
        }
        if (err != nil) { // Fallback to Std and Dlt
            {
                (std, _, err) = k.GetStringValue(stdˢ); if (err != nil) {
                    t.Fatalf("cannot read CEST Std registry key: %s", err);
                }
            }
            {
                (dlt, _, err) = k.GetStringValue(dltˢ); if (err != nil) {
                    t.Fatalf("cannot read CEST Dlt registry key: %s", err);
                }
            }
        }

        (var name, err) = time_internal_test_package.ToEnglishName(std, dlt);
        if (err != nil) {
            t.Fatalf("toEnglishName failed: %s", err);
        }
        if (name != want) {
            t.Fatalf("english name: %q, want: %q", name, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end time_test_package
