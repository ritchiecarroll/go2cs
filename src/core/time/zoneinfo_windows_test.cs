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

internal static void testZoneAbbr(ж<Δtesting.T> Ꮡt) {
    var t1 = Now();
    // discard nsec
    t1 = Date(t1.Year(), t1.Month(), t1.Day(), t1.Hour(), t1.Minute(), t1.Second(), 0, t1.Location());
    var (t2, err) = Parse(RFC1123, t1.Format(RFC1123));
    if (err != default!) {
        Ꮡt.Fatalf("Parse failed: %v"u8, err);
    }
    if (t1 != t2) {
        Ꮡt.Fatalf("t1 (%v) is not equal to t2 (%v)"u8, t1, t2);
    }
}

public static void TestUSPacificZoneAbbr(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        time_internal_test_package.ForceUSPacificFromTZIForTesting(); // reset the Once to trigger the race
        defer(time_internal_test_package.ForceUSPacificForTesting, ref ᒐ);
        testZoneAbbr(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAusZoneAbbr(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        time_internal_test_package.ForceAusFromTZIForTesting();
        defer(time_internal_test_package.ForceUSPacificForTesting, ref ᒐ);
        testZoneAbbr(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string muiStdˢ = "MUI_Std"u8;
internal static readonly @string muiDltˢ = "MUI_Dlt"u8;
internal static readonly @string stdˢ = "Std"u8;
internal static readonly @string dltˢ = "Dlt"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "Central Europe Standard Time"u8;

public static void TestToEnglishName(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string want = wantᶜ;
        var (k, err) = registry.OpenKey(registry.LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones\" + want, registry.READ);
        if (err != default!) {
            Ꮡt.Fatalf("cannot open CEST time zone information from registry: %s"u8, err);
        }
        defer(() => k.Close(), ref ᒐ);
        @string std = default!;
        @string dlt = default!;
        // Try MUI_Std and MUI_Dlt first, fallback to Std and Dlt if *any* error occurs
        (std, err) = k.GetMUIStringValue(muiStdˢ);
        if (err == default!) {
            (dlt, err) = k.GetMUIStringValue(muiDltˢ);
        }
        if (err != default!) {
            // Fallback to Std and Dlt
            {
                (std, _, err) = k.GetStringValue(stdˢ); if (err != default!) {
                    Ꮡt.Fatalf("cannot read CEST Std registry key: %s"u8, err);
                }
            }
            {
                (dlt, _, err) = k.GetStringValue(dltˢ); if (err != default!) {
                    Ꮡt.Fatalf("cannot read CEST Dlt registry key: %s"u8, err);
                }
            }
        }
        (var name, err) = time_internal_test_package.ToEnglishName(std, dlt);
        if (err != default!) {
            Ꮡt.Fatalf("toEnglishName failed: %s"u8, err);
        }
        if (name != want) {
            Ꮡt.Fatalf("english name: %q, want: %q"u8, name, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end time_test_package
