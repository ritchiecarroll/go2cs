// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using Δtesting = testing_package;
using Δtime = time_package;
// blank import: go.time.tzdata_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using static go.time_internal_test_package;

partial class time_test_package {

internal static slice<@string> zones = new @string[]{
    "Asia/Jerusalem"u8,
    "America/Los_Angeles"u8
}.slice();

public static void TestEmbeddedTZData(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var undo = time_internal_test_package.DisablePlatformSources();
        var undoʗ1 = undo;
        defer(undoʗ1, ref ᒐ);

        foreach (var (_, zone) in zones) {
            var (@ref, err) = Δtime.LoadLocation(zone);
            if (err != default!) {
                Ꮡt.Errorf("LoadLocation(%q): %v"u8, zone, err);
                continue;
            }

            (var embedded, err) = time_internal_test_package.LoadFromEmbeddedTZData(zone);
            if (err != default!) {
                Ꮡt.Errorf("LoadFromEmbeddedTZData(%q): %v"u8, zone, err);
                continue;
            }
            (var sample, err) = Δtime.LoadLocationFromTZData(zone, slice<byte>(embedded));
            if (err != default!) {
                Ꮡt.Errorf("LoadLocationFromTZData failed for %q: %v"u8, zone, err);
                continue;
            }

            // Compare the name and zone fields of ref and sample.
            // The tx field changes faster as tzdata is updated.
            // The cache fields are expected to differ.
            var v1 = reflect.ValueOf(@ref.OrTypedNil()).Elem();
            var v2 = reflect.ValueOf(sample.OrTypedNil()).Elem();
            var typ = v1.Type();
            nint nf = typ.NumField();
            nint found = 0;
            for (nint i = 0; i < nf; i++) {
                var ft = typ.Field(i);
                if (ft.Name != "name"u8 && ft.Name != "zone"u8) {
                    continue;
                }
                found++;
                if (!equal(Ꮡt, v1.Field(i), v2.Field(i))) {
                    Ꮡt.Errorf("zone %s: system and embedded tzdata field %s differs"u8, zone, ft.Name);
                }
            }
            if (found != 2) {
                Ꮡt.Errorf("test must be updated for change to time.Location struct"u8);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// equal is a small version of reflect.DeepEqual that we use to
// compare the values of zoneinfo unexported fields.
internal static bool equal(ж<Δtesting.T> Ꮡt, reflectꓸValue f1, reflectꓸValue f2) {
    var exprᴛ1 = f1.Type().Kind();
    if (exprᴛ1 == reflect.ΔSlice) {
        if (f1.Len() != f2.Len()) {
            return false;
        }
        for (nint i = 0; i < f1.Len(); i++) {
            if (!equal(Ꮡt, f1.Index(i), f2.Index(i))) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == reflect.Struct) {
        nint nf = f1.Type().NumField();
        for (nint i = 0; i < nf; i++) {
            if (!equal(Ꮡt, f1.Field(i), f2.Field(i))) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == reflect.ΔString) {
        return f1.String() == f2.String();
    }
    if (exprᴛ1 == reflect.ΔBool) {
        return f1.Bool() == f2.Bool();
    }
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return f1.Int() == f2.Int();
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return f1.Uint() == f2.Uint();
    }
    { /* default: */
        Ꮡt.Errorf("test internal error: unsupported kind %v"u8, f1.Type().Kind());
        return true;
    }
}

} // end time_test_package
