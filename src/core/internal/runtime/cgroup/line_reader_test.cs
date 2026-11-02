// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using cgroup = go.@internal.runtime.cgroup_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal.runtime;
using iter = iter_package;
using static go.@internal.runtime.cgroup_internal_test_package;

partial class cgroup_test_package {

partial struct nextLine {
    internal @string line;
    internal bool incomplete; // next call before this line should return incomplete
}

internal static nextLine complete(@string s) {
    return new nextLine(line: s);
}

internal static nextLine incomplete(@string s) {
    return new nextLine(line: s, incomplete: true);
}

internal static UntypedInt scratchSize => 8;

partial struct readerTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string contents;
    internal slice<nextLine> want;
}
internal static slice<readerTestsᴛ1> readerTests = slice<readerTestsᴛ1>([
    new(
        name: "empty",
        contents: ""
    ),
    new(
        name: "single",
        contents: "1234\n",
        want: slice<nextLine>([
            complete("1234")
        ])
    ),
    new(
        name: "single-incomplete",
        contents: "1234",
        want: slice<nextLine>([
            incomplete("1234")
        ])
    ),
    new(
        name: "single-exact",
        contents: "1234567\n",
        want: slice<nextLine>([
            complete("1234567")
        ])
    ),
    new(
        name: "single-exact-incomplete",
        contents: "12345678",
        want: slice<nextLine>([
            incomplete("12345678")
        ])
    ),
    new(
        name: "multi",
        contents: """
1234
5678

"""u8.Raw(),
        want: slice<nextLine>([
            complete("1234"),
            complete("5678")
        ])
    ),
    new(
        name: "multi-short",
        contents: """
12
34
56
78

"""u8.Raw(),
        want: slice<nextLine>([
            complete("12"),
            complete("34"),
            complete("56"),
            complete("78")
        ])
    ),
    new(
        name: "multi-notrailingnewline",
        contents: """
1234
5678
"""u8.Raw(),
        want: slice<nextLine>([
            complete("1234"),
            incomplete("5678")
        ])
    ),
    new(
        name: "middle-too-long",
        contents: """
1234
1234567890
5678

"""u8.Raw(),
        want: slice<nextLine>([
            complete("1234"),
            incomplete("12345678"),
            complete("5678")
        ])
    ),
    new(
        // Multiple reads required to find newline.
        name: "middle-way-too-long",
        contents: """
1234
12345678900000000000000000000000000000000000000000000000000
5678

"""u8.Raw(),
        want: slice<nextLine>([
            complete("1234"),
            incomplete("12345678"),
            complete("5678")
        ])
    )
]);

public static void TestLineReader(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in readerTests) {
        ref var tc = ref heap(new readerTestsᴛ1(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            array<byte> scratch = new(8); /* scratchSize */
            var l = cgroup_internal_test_package.NewLineReader(0, scratch[..], readString(tcʗ1.contents));

            slice<nextLine> got = default;
            for (;;) {
                var err = l.Next();
                if (AreEqual(err, cgroup_internal_test_package.ErrEOF)) {
                    break;
                } else if (AreEqual(err, cgroup_internal_test_package.ErrIncompleteLine)) {
                    got = append(got, incomplete((@string)l.Line()));
                } else if (err != nil) {
                    tΔ1.Fatalf("next got err %v", err);
                } else {
                    got = append(got, complete((@string)l.Line()));
                }
            }

            if (len(got) != len(tcʗ1.want)) {
                tΔ1.Logf("got lines %+v", got);
                tΔ1.Logf("want lines %+v", tcʗ1.want);
                tΔ1.Fatalf("lineReader got %d lines, want %d", len(got), len(tcʗ1.want));
            }

            foreach (var i in range(got)) {
                if (got[i].line != tcʗ1.want[i].line) {
                    tΔ1.Errorf("line %d got %q want %q", i, got[i].line, tcʗ1.want[i].line);
                }
                if (got[i].incomplete != tcʗ1.want[i].incomplete) {
                    tΔ1.Errorf("line %d got incomplete %v want %v", i, got[i].incomplete, tcʗ1.want[i].incomplete);
                }
            }
        });
    }
}

public static void FuzzLineReader(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    foreach (var (_, tc) in readerTests) {
        f.Add(tc.contents);
    }
    Ꮡf.Fuzz((ж<testing.T> t, @string input) => {
        var scratch = new slice<byte>(scratchSize);
        var reader = cgroup_internal_test_package.NewLineReader(0, scratch, readString(input));
        foreach (var iᴛ1 in range<@string>(strings.Lines(input).Invoke)) {
            var expected = iᴛ1;
            var errΔ1 = reader.Next();
            var line = reader.Line();

            error expectedErr = default;
            if (len(expected) > scratchSize) {
                expected = expected[..(int)(scratchSize)];
                expectedErr = cgroup_internal_test_package.ErrIncompleteLine;
            } else if (expected[len(expected) - 1] == '\n') {
                expected = expected.slice(0, len(expected) - 1);
            } else {
                expectedErr = cgroup_internal_test_package.ErrIncompleteLine;
            }

            if (!AreEqual(errΔ1, expectedErr)) {
                t.Fatalf("got err %v, want %v", errΔ1, expectedErr);
            }

            if (((sstring)line) != expected) {
                t.Fatalf("got %q, want %q", (@string)line, expected);
            }
        }
        var err = reader.Next();
        if (!AreEqual(err, cgroup_internal_test_package.ErrEOF)) {
            t.Fatalf("got %v, want EOF", err);
        }
    });
}

} // end cgroup_test_package
