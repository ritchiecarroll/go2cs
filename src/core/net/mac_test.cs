// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using static go.net_package;

partial class net_internal_test_package {

// See RFC 7042, Section 2.1.1.
// See RFC 7042, Section 2.2.2.
// See RFC 4391, Section 9.1.1.

partial struct parseMACTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.net_package.HardwareAddr @out;
    internal @string err;
}
internal static slice<parseMACTestsᴛ1> parseMACTests = new parseMACTestsᴛ1[]{
    new("00:00:5e:00:53:01"u8, new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""u8),
    new("00-00-5e-00-53-01"u8, new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""u8),
    new("0000.5e00.5301"u8, new HardwareAddr(new byte[]{0x00, 0x00, 0x5e, 0x00, 0x53, 0x01}.slice()), ""u8),
    new("02:00:5e:10:00:00:00:01"u8, new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""u8),
    new("02-00-5e-10-00-00-00-01"u8, new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""u8),
    new("0200.5e10.0000.0001"u8, new HardwareAddr(new byte[]{0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01}.slice()), ""u8),
    new(
        "00:00:00:00:fe:80:00:00:00:00:00:00:02:00:5e:10:00:00:00:01"u8,
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""u8
    ),
    new(
        "00-00-00-00-fe-80-00-00-00-00-00-00-02-00-5e-10-00-00-00-01"u8,
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""u8
    ),
    new(
        "0000.0000.fe80.0000.0000.0000.0200.5e10.0000.0001"u8,
        new HardwareAddr(new byte[]{
            0x00, 0x00, 0x00, 0x00,
            0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x5e, 0x10, 0x00, 0x00, 0x00, 0x01
        }.slice()),
        ""u8
    ),
    new("ab:cd:ef:AB:CD:EF"u8, new HardwareAddr(new byte[]{0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef}.slice()), ""u8),
    new("ab:cd:ef:AB:CD:EF:ab:cd"u8, new HardwareAddr(new byte[]{0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef, 0xab, 0xcd}.slice()), ""u8),
    new(
        "ab:cd:ef:AB:CD:EF:ab:cd:ef:AB:CD:EF:ab:cd:ef:AB:CD:EF:ab:cd"u8,
        new HardwareAddr(new byte[]{
            0xab, 0xcd, 0xef, 0xab,
            0xcd, 0xef, 0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef,
            0xab, 0xcd, 0xef, 0xab, 0xcd, 0xef, 0xab, 0xcd
        }.slice()),
        ""u8
    ),
    new("01.02.03.04.05.06"u8, default!, "invalid MAC address"u8),
    new("01:02:03:04:05:06:"u8, default!, "invalid MAC address"u8),
    new("x1:02:03:04:05:06"u8, default!, "invalid MAC address"u8),
    new("01002:03:04:05:06"u8, default!, "invalid MAC address"u8),
    new("01:02003:04:05:06"u8, default!, "invalid MAC address"u8),
    new("01:02:03004:05:06"u8, default!, "invalid MAC address"u8),
    new("01:02:03:04005:06"u8, default!, "invalid MAC address"u8),
    new("01:02:03:04:05006"u8, default!, "invalid MAC address"u8),
    new("01-02:03:04:05:06"u8, default!, "invalid MAC address"u8),
    new("01:02-03-04-05-06"u8, default!, "invalid MAC address"u8),
    new("0123:4567:89AF"u8, default!, "invalid MAC address"u8),
    new("0123-4567-89AF"u8, default!, "invalid MAC address"u8)
}.slice();

public static void TestParseMAC(ж<testing.T> Ꮡt) {
    bool match(error err, @string s) {
        if (s == ""u8) {
            return err == default!;
        }
        return err != default! && strings.Contains(err.Error(), s);
    }
    foreach (var (i, tt) in parseMACTests) {
        var (@out, err) = ParseMAC(tt.@in);
        if (!reflect.DeepEqual(@out, tt.@out) || !match(err, tt.err)) {
            Ꮡt.Errorf("ParseMAC(%q) = %v, %v, want %v, %v"u8, tt.@in, @out, err, tt.@out, tt.err);
        }
        if (tt.err == ""u8) {
            // Verify that serialization works too, and that it round-trips.
            @string s = @out.String();
            var (out2, errΔ1) = ParseMAC(s);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("%d. ParseMAC(%q) = %v"u8, i, s, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(out2, @out)) {
                Ꮡt.Errorf("%d. ParseMAC(%q) = %v, want %v"u8, i, s, out2, @out);
            }
        }
    }
}

} // end net_internal_test_package
