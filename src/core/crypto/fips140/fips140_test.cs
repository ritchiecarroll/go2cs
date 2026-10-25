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
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string fips140ˢ = "fips140"u8;
internal static readonly @string fips140Offˢ = "fips140=off"u8;
internal static readonly @string fips140Onˢ = "fips140=on"u8;
internal static readonly @string fips140ˢ2 = "fips140="u8;

public static void TestImmutableGODEBUG(ж<testing.T> Ꮡt) {
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
            Ꮡt.Errorf("Enabled() changed after setting GODEBUG=fips140=off"u8);
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            Ꮡt.Errorf("fips140Setting.Value() changed after setting GODEBUG=fips140=off"u8);
        }

        os.Setenv(godebugˢ, fips140Onˢ);
        if (Enabled() != fips140Enabled) {
            Ꮡt.Errorf("Enabled() changed after setting GODEBUG=fips140=on"u8);
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            Ꮡt.Errorf("fips140Setting.Value() changed after setting GODEBUG=fips140=on"u8);
        }

        os.Setenv(godebugˢ, fips140ˢ2);
        if (Enabled() != fips140Enabled) {
            Ꮡt.Errorf("Enabled() changed after setting GODEBUG=fips140="u8);
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            Ꮡt.Errorf("fips140Setting.Value() changed after setting GODEBUG=fips140="u8);
        }

        os.Setenv(godebugˢ, ""u8);
        if (Enabled() != fips140Enabled) {
            Ꮡt.Errorf("Enabled() changed after setting GODEBUG="u8);
        }
        if (fips140Setting.Value() != fips140SettingValue) {
            Ꮡt.Errorf("fips140Setting.Value() changed after setting GODEBUG="u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end fips140_internal_test_package
