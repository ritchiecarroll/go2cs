// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using regexp = regexp_package;
using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

public static void TestNumberIsValid(ж<testing.T> Ꮡt) {
    // From: https://stackoverflow.com/a/13340826
    ж<regexp.Regexp> jsonNumberRegexp = regexp.MustCompile(dDEEDˢ);

    var validTests = new @string[]{
        "0"u8,
        "-0"u8,
        "1"u8,
        "-1"u8,
        "0.1"u8,
        "-0.1"u8,
        "1234"u8,
        "-1234"u8,
        "12.34"u8,
        "-12.34"u8,
        "12E0"u8,
        "12E1"u8,
        "12e34"u8,
        "12E-0"u8,
        "12e+1"u8,
        "12e-34"u8,
        "-12E0"u8,
        "-12E1"u8,
        "-12e34"u8,
        "-12E-0"u8,
        "-12e+1"u8,
        "-12e-34"u8,
        "1.2E0"u8,
        "1.2E1"u8,
        "1.2e34"u8,
        "1.2E-0"u8,
        "1.2e+1"u8,
        "1.2e-34"u8,
        "-1.2E0"u8,
        "-1.2E1"u8,
        "-1.2e34"u8,
        "-1.2E-0"u8,
        "-1.2e+1"u8,
        "-1.2e-34"u8,
        "0E0"u8,
        "0E1"u8,
        "0e34"u8,
        "0E-0"u8,
        "0e+1"u8,
        "0e-34"u8,
        "-0E0"u8,
        "-0E1"u8,
        "-0e34"u8,
        "-0E-0"u8,
        "-0e+1"u8,
        "-0e-34"u8
    }.slice();

    foreach (var (_, test) in validTests) {
        if (!isValidNumber(test)) {
            Ꮡt.Errorf("%s should be valid"u8, test);
        }

        ref var f = ref heap(new float64(), out var Ꮡf);
        {
            var err = Unmarshal(slice<byte>(test), Ꮡf); if (err != default!) {
                Ꮡt.Errorf("%s should be valid but Unmarshal failed: %v"u8, test, err);
            }
        }

        if (!jsonNumberRegexp.MatchString(test)) {
            Ꮡt.Errorf("%s should be valid but regexp does not match"u8, test);
        }
    }

    var invalidTests = new @string[]{
        ""u8,
        "invalid"u8,
        "1.0.1"u8,
        "1..1"u8,
        "-1-2"u8,
        "012a42"u8,
        "01.2"u8,
        "012"u8,
        "12E12.12"u8,
        "1e2e3"u8,
        "1e+-2"u8,
        "1e--23"u8,
        "1e"u8,
        "e1"u8,
        "1e+"u8,
        "1ea"u8,
        "1a"u8,
        "1.a"u8,
        "1."u8,
        "01"u8,
        "1.e1"u8
    }.slice();

    foreach (var (_, test) in invalidTests) {
        if (isValidNumber(test)) {
            Ꮡt.Errorf("%s should be invalid"u8, test);
        }

        ref var f = ref heap(new float64(), out var Ꮡf);
        {
            var err = Unmarshal(slice<byte>(test), Ꮡf); if (err == default!) {
                Ꮡt.Errorf("%s should be invalid but unmarshal wrote %v"u8, test, f);
            }
        }

        if (jsonNumberRegexp.MatchString(test)) {
            Ꮡt.Errorf("%s should be invalid but matches regexp"u8, test);
        }
    }
}

} // end json_internal_test_package
