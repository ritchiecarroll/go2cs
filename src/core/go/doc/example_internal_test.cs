// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using ast = global::go.go.ast_package;
using global::go.go;
using static global::go.go.doc_package;

partial class doc_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testGoˢ2 = "test.go"u8;

internal partial struct TestImportGroupStarts_type /*dyn*/ {
    internal @string name;
    internal @string @in;
    internal slice<@string> want; // paths of group-starting imports
}

public static void TestImportGroupStarts(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestImportGroupStarts_type[]{
        new(
            name: "one group"u8,
            @in: """
package p
import (
	"a"
	"b"
	"c"
	"d"
)

"""u8,
            want: new @string[]{"a"u8}.slice()
        ),
        new(
            name: "several groups"u8,
            @in: """
package p
import (
	"a"

	"b"
	"c"

	"d"
)

"""u8,
            want: new @string[]{"a"u8, "b"u8, "d"u8}.slice()
        ),
        new(
            name: "extra space"u8,
            @in: """
package p
import (
	"a"


	"b"
	"c"


	"d"
)

"""u8,
            want: new @string[]{"a"u8, "b"u8, "d"u8}.slice()
        ),
        new(
            name: "line comment"u8,
            @in: """
package p
import (
	"a" // comment
	"b" // comment

	"c"
)
"""u8,
            want: new @string[]{"a"u8, "c"u8}.slice()
        ),
        new(
            name: "named import"u8,
            @in: """
package p
import (
	"a"
	n "b"

	m "c"
	"d"
)
"""u8,
            want: new @string[]{"a"u8, "c"u8}.slice()
        ),
        new(
            name: "blank import"u8,
            @in: """
package p
import (
	"a"

	_ "b"

	_ "c"
	"d"
)
"""u8,
            want: new @string[]{"a"u8, "b"u8, "c"u8}.slice()
        )
    }.slice()) {
        ref var test = ref heap(new TestImportGroupStarts_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var fset = token.NewFileSet();
            var (@file, err) = parser.ParseFile(fset, testGoˢ2, strings.NewReader(testʗ1.@in).OrTypedNil(), parser.ParseComments);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var imps = findImportGroupStarts1((~@file).Imports);
            var got = new slice<@string>(len(imps));
            foreach (var (i, imp) in imps) {
                (got[i], err) = strconv.Unquote((~(~imp).Path).Value);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
            }
            if (!reflect.DeepEqual(got, testʗ1.want)) {
                tΔ1.Errorf("got %v, want %v"u8, got, testʗ1.want);
            }
        });
    }
}

} // end doc_internal_test_package
