// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
namespace go.encoding;

using regexp = regexp_package;
using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

public static void TestNumberIsValid(ж<testing.T> t) {
    // From: https://stackoverflow.com/a/13340826
    ж<regexp.Regexp> jsonNumberRegexp = regexp.MustCompile(dDEEDˢ);

    var validTests = slice<@string>([
        "0",
        "-0",
        "1",
        "-1",
        "0.1",
        "-0.1",
        "1234",
        "-1234",
        "12.34",
        "-12.34",
        "12E0",
        "12E1",
        "12e34",
        "12E-0",
        "12e+1",
        "12e-34",
        "-12E0",
        "-12E1",
        "-12e34",
        "-12E-0",
        "-12e+1",
        "-12e-34",
        "1.2E0",
        "1.2E1",
        "1.2e34",
        "1.2E-0",
        "1.2e+1",
        "1.2e-34",
        "-1.2E0",
        "-1.2E1",
        "-1.2e34",
        "-1.2E-0",
        "-1.2e+1",
        "-1.2e-34",
        "0E0",
        "0E1",
        "0e34",
        "0E-0",
        "0e+1",
        "0e-34",
        "-0E0",
        "-0E1",
        "-0e34",
        "-0E-0",
        "-0e+1",
        "-0e-34"
    ]);

    foreach (var (_, test) in range(validTests)) {
        if (!isValidNumber(test)) {
            t.Errorf("%s should be valid", test);
        }

        ref var f = ref heap(new float64(), out var Ꮡf);
        var err = Unmarshal(slice<byte>(test), Ꮡf); if (err != nil) {
            t.Errorf("%s should be valid but Unmarshal failed: %v", test, err);
        }

        if (!jsonNumberRegexp.MatchString(test)) {
            t.Errorf("%s should be valid but regexp does not match", test);
        }
    }

    var invalidTests = slice<@string>([
        "",
        "invalid",
        "1.0.1",
        "1..1",
        "-1-2",
        "012a42",
        "01.2",
        "012",
        "12E12.12",
        "1e2e3",
        "1e+-2",
        "1e--23",
        "1e",
        "e1",
        "1e+",
        "1ea",
        "1a",
        "1.a",
        "1.",
        "01",
        "1.e1"
    ]);

    foreach (var (_, test) in range(invalidTests)) {
        if (isValidNumber(test)) {
            t.Errorf("%s should be invalid", test);
        }

        ref var f = ref heap(new float64(), out var Ꮡf);
        var err = Unmarshal(slice<byte>(test), Ꮡf); if (err == nil) {
            t.Errorf("%s should be invalid but unmarshal wrote %v", test, f);
        }

        if (jsonNumberRegexp.MatchString(test)) {
            t.Errorf("%s should be invalid but matches regexp", test);
        }
    }
}

} // end json_internal_test_package
