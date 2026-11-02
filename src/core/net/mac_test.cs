// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

partial struct parseMACTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.net_package.HardwareAddr @out;
    internal @string err;
}
internal static slice<parseMACTestsᴛ1> parseMACTests = slice<parseMACTestsᴛ1>([
    // See RFC 7042, Section 2.1.1.
    new("00:00:5e:00:53:01", new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""),
    new("00-00-5e-00-53-01", new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""),
    new("0000.5e00.5301", new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""),
    new("00005e005301", new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""),

    // See RFC 7042, Section 2.2.2.
    new("02:00:5e:10:00:00:00:01", new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""),
    new("02-00-5e-10-00-00-00-01", new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""),
    new("0200.5e10.0000.0001", new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""),
    new("02005e1000000001", new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""),

    // See RFC 4391, Section 9.1.1.
    new(
        "00:00:00:00:fe:80:00:00:00:00:00:00:02:00:5e:10:00:00:00:01",
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""
    ),
    new(
        "00-00-00-00-fe-80-00-00-00-00-00-00-02-00-5e-10-00-00-00-01",
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""
    ),
    new(
        "0000.0000.fe80.0000.0000.0000.0200.5e10.0000.0001",
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""
    ),
    new(
        "00000000fe8000000000000002005e1000000001",
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""
    ),

    new("ab:cd:ef:AB:CD:EF", new HardwareAddr(new byte[]{0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef}.slice()), ""),
    new("ab:cd:ef:AB:CD:EF:ab:cd", new HardwareAddr(new byte[]{0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef, 0xab, 0xcd}.slice()), ""),
    new(
        "ab:cd:ef:AB:CD:EF:ab:cd:ef:AB:CD:EF:ab:cd:ef:AB:CD:EF:ab:cd",
        new HardwareAddr(new byte[]{
            0xab, 0xcd, 0xef, 0xab,
            0xcd, 0xef, 0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef,
            0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef, 0xab, 0xcd
        }.slice()),
        ""
    ),

    new("01.02.03.04.05.06", default, "invalid MAC address"),
    new("01:02:03:04:05:06:", default, "invalid MAC address"),
    new("x1:02:03:04:05:06", default, "invalid MAC address"),
    new("01002:03:04:05:06", default, "invalid MAC address"),
    new("01:02003:04:05:06", default, "invalid MAC address"),
    new("01:02:03004:05:06", default, "invalid MAC address"),
    new("01:02:03:04005:06", default, "invalid MAC address"),
    new("01:02:03:04:05006", default, "invalid MAC address"),
    new("01-02:03:04:05:06", default, "invalid MAC address"),
    new("01:02-03-04-05-06", default, "invalid MAC address"),
    new("0123:4567:89AF", default, "invalid MAC address"),
    new("0123-4567-89AF", default, "invalid MAC address"),
    new("0123456789AF0", default, "invalid MAC address")
]);

public static void TestParseMAC(ж<testing.T> t) {
    bool match(error err, @string s) {
        if (s == ""u8) {
            return err == nil;
        }
        return err != nil && strings.Contains(err.Error(), s);
    }

    foreach (var (i, tt) in parseMACTests) {
        var (@out, err) = ParseMAC(tt.@in);
        if (!reflect.DeepEqual(@out, tt.@out) || !match(err, tt.err)) {
            t.Errorf("ParseMAC(%q) = %v, %v, want %v, %v", tt.@in, @out, err, tt.@out, tt.err);
        }
        if (tt.err == ""u8) {
            // Verify that serialization works too, and that it round-trips.
            @string s = @out.String();
            var (out2, errΔ1) = ParseMAC(s);
            if (errΔ1 != nil) {
                t.Errorf("%d. ParseMAC(%q) = %v", i, s, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(out2, @out)) {
                t.Errorf("%d. ParseMAC(%q) = %v, want %v", i, s, out2, @out);
            }
        }
    }
}

} // end net_internal_test_package
