// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using godebug = go.@internal.godebug_package;
using os = os_package;
using testing = testing_package;
using go.@internal;
using static go.crypto.fips140_package;

partial class fips140_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string fips140ˢ => ref fips140ˢᵇ;
private static readonly @string fips140ˢᵇ = "fips140"u8;
internal static ref readonly @string fips140Offˢ => ref fips140Offˢᵇ;
private static readonly @string fips140Offˢᵇ = "fips140=off"u8;
internal static ref readonly @string fips140Onˢ => ref fips140Onˢᵇ;
private static readonly @string fips140Onˢᵇ = "fips140=on"u8;
internal static ref readonly @string fips140ˢ2 => ref fips140ˢ2ᵇ;
private static readonly @string fips140ˢ2ᵇ = "fips140="u8;

public static void TestImmutableGODEBUG(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer((@string v) => {
            os.Setenv(godebugˢ, v);
        }, os.Getenv(godebugˢ), ref ᒐ);

        var fips140Enabled = Enabled();
        var fips140Setting = godebug.New(fips140ˢ);
        @string fips140SettingValue = fips140Setting.Value();

        os.Setenv(godebugˢ, fips140Offˢ);
        if (Enabled() != fips140Enabled) {
            t.Errorf("Enabled() changed after setting GODEBUG=fips140=off");
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            t.Errorf("fips140Setting.Value() changed after setting GODEBUG=fips140=off");
        }

        os.Setenv(godebugˢ, fips140Onˢ);
        if (Enabled() != fips140Enabled) {
            t.Errorf("Enabled() changed after setting GODEBUG=fips140=on");
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            t.Errorf("fips140Setting.Value() changed after setting GODEBUG=fips140=on");
        }

        os.Setenv(godebugˢ, fips140ˢ2);
        if (Enabled() != fips140Enabled) {
            t.Errorf("Enabled() changed after setting GODEBUG=fips140=");
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            t.Errorf("fips140Setting.Value() changed after setting GODEBUG=fips140=");
        }

        os.Setenv(godebugˢ, ""u8);
        if (Enabled() != fips140Enabled) {
            t.Errorf("Enabled() changed after setting GODEBUG=");
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            t.Errorf("fips140Setting.Value() changed after setting GODEBUG=");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fips140_internal_test_package
