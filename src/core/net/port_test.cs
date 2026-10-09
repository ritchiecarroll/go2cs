// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

// Decimal number literals
// Others

partial struct parsePortTestsᴛ1 /*dyn*/ {
    internal @string service;
    internal nint port;
    internal bool needsLookup;
}
internal static slice<parsePortTestsᴛ1> parsePortTests = new parsePortTestsᴛ1[]{
    new(""u8, 0, false),
    new("-1073741825"u8, (-1 << (int)(30)), false),
    new("-1073741824"u8, (-1 << (int)(30)), false),
    new("-1073741823"u8, -((1 << (int)(30)) - 1), false),
    new("-123456789"u8, -123456789, false),
    new("-1"u8, -1, false),
    new("-0"u8, 0, false),
    new("0"u8, 0, false),
    new("+0"u8, 0, false),
    new("+1"u8, 1, false),
    new("65535"u8, 65535, false),
    new("65536"u8, 65536, false),
    new("123456789"u8, 123456789, false),
    new("1073741822"u8, (1 << (int)(30)) - 2, false),
    new("1073741823"u8, (1 << (int)(30)) - 1, false),
    new("1073741824"u8, (1 << (int)(30)) - 1, false),
    new("1073741825"u8, (1 << (int)(30)) - 1, false),
    new("abc"u8, 0, true),
    new("9pfs"u8, 0, true),
    new("123badport"u8, 0, true),
    new("bad123port"u8, 0, true),
    new("badport123"u8, 0, true),
    new("123456789badport"u8, 0, true),
    new("-2147483649badport"u8, 0, true),
    new("2147483649badport"u8, 0, true)
}.slice();

public static void TestParsePort(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // The following test cases are cribbed from the strconv
    foreach (var (_, tt) in parsePortTests) {
        {
            var (port, needsLookup) = parsePort(tt.service); if (port != tt.port || needsLookup != tt.needsLookup) {
                Ꮡt.Errorf("parsePort(%q) = %d, %t; want %d, %t"u8, tt.service, port, needsLookup, tt.port, tt.needsLookup);
            }
        }
    }
}

} // end net_internal_test_package
