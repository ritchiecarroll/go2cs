// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

partial struct parsePortTestsᴛ1 /*dyn*/ {
    internal @string service;
    internal nint port;
    internal bool needsLookup;
}
internal static slice<parsePortTestsᴛ1> parsePortTests = slice<parsePortTestsᴛ1>([
    new("", 0, false),

    // Decimal number literals
    new("-1073741825", -1 << 30, false),
    new("-1073741824", -1 << 30, false),
    new("-1073741823", -((1 << 30) - 1), false),
    new("-123456789", -123456789, false),
    new("-1", -1, false),
    new("-0", 0, false),
    new("0", 0, false),
    new("+0", 0, false),
    new("+1", 1, false),
    new("65535", 65535, false),
    new("65536", 65536, false),
    new("123456789", 123456789, false),
    new("1073741822", (1 << 30) - 2, false),
    new("1073741823", (1 << 30) - 1, false),
    new("1073741824", (1 << 30) - 1, false),
    new("1073741825", (1 << 30) - 1, false),

    // Others
    new("abc", 0, true),
    new("9pfs", 0, true),
    new("123badport", 0, true),
    new("bad123port", 0, true),
    new("badport123", 0, true),
    new("123456789badport", 0, true),
    new("-2147483649badport", 0, true),
    new("2147483649badport", 0, true)
]);

public static void TestParsePort(ж<testing.T> t) {
    // The following test cases are cribbed from the strconv
    foreach (var (_, tt) in parsePortTests) {
        {
            var (port, needsLookup) = parsePort(tt.service); if (port != tt.port || needsLookup != tt.needsLookup) {
                t.Errorf("parsePort(%q) = %d, %t; want %d, %t", tt.service, port, needsLookup, tt.port, tt.needsLookup);
            }
        }
    }
}

} // end net_internal_test_package
