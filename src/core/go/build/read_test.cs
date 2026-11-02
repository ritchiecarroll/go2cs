// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using token = global::go.go.token_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.build_package;

partial class build_internal_test_package {

internal static ref readonly @string quote => ref quoteᵇ;
private static readonly @string quoteᵇ = "`"u8;

internal partial struct readTest {
    // Test input contains ℙ where readGoInfo should stop.
    internal @string @in;
    internal @string err;
}

internal static slice<readTest> readGoInfoTests = slice<readTest>([
    new(
        @"package p",
        ""
    ),
    new(
        @"package p; import ""x""",
        ""
    ),
    new(
        @"package p; import . ""x""",
        ""
    ),
    new(
        @"package p; import ""x"";ℙvar x = 1",
        ""
    ),
    new(
        """
package p

		// comment

		import "x"
		import _ "x"
		import a "x"

		/* comment */

		import (
			"x" /* comment */
			_ "x"
			a "x" // comment
			
""".Raw() + quote + @"x" + quote + """

			_ /*comment*/ 
""".Raw() + quote + @"x" + quote + """

			a 
""".Raw() + quote + @"x" + quote + """

		)
		import (
		)
		import ()
		import()import()import()
		import();import();import()

		ℙvar x = 1
		
""".Raw(),
        ""
    ),
    new(
        "\ufeff𝔻"u8 + @"package p; import ""x"";ℙvar x = 1"u8,
        ""
    )
]);

internal static slice<readTest> readCommentsTests = slice<readTest>([
    new(
        @"ℙpackage p",
        ""
    ),
    new(
        @"ℙpackage p; import ""x""",
        ""
    ),
    new(
        @"ℙpackage p; import . ""x""",
        ""
    ),
    new(
        "\ufeff𝔻"u8 + @"ℙpackage p; import . ""x"""u8,
        ""
    ),
    new(
        """
// foo

		/* bar */

		/* quux */ // baz

		/*/ zot */

		// asdf
		ℙHello, world
"""u8.Raw(),
        ""
    ),
    new(
        "\ufeff𝔻"u8 + """
// foo

		/* bar */

		/* quux */ // baz

		/*/ zot */

		// asdf
		ℙHello, world
"""u8.Raw(),
        ""
    )
]);

internal static void testRead(ж<testing.T> t, slice<readTest> tests, Func<io.Reader, (slice<byte>, error)> read) {
    foreach (var (i, tt) in tests) {
        var (beforeP, afterP, _) = strings.Cut(tt.@in, "ℙ");
        @string @in = beforeP + afterP;
        @string testOut = beforeP;

        {
            var (beforeD, afterD, ok) = strings.Cut(beforeP, "𝔻"); if (ok) {
                @in = beforeD + afterD + afterP;
                testOut = afterD;
            }
        }

        var r = strings.NewReader(@in);
        var (buf, err) = read(new build_internal_test_package.strings_ReaderжReader(r));
        if (err != nil) {
            if (tt.err == ""u8) {
                t.Errorf("#%d: err=%q, expected success (%q)", i, err, (@string)buf);
            } else if (!strings.Contains(err.Error(), tt.err)) {
                t.Errorf("#%d: err=%q, expected %q", i, err, tt.err);
            }
            continue;
        }
        if (tt.err != ""u8) {
            t.Errorf("#%d: success, expected %q", i, tt.err);
            continue;
        }

        @string @out = (@string)buf;
        if (@out != testOut) {
            t.Errorf("#%d: wrong output:\nhave %q\nwant %q\n", i, @out, testOut);
        }
    }
}

public static void TestReadGoInfo(ж<testing.T> t) {
    testRead(t, readGoInfoTests, (io.Reader r) => {
        ref var info = ref heap(new global::go.go.build_package.fileInfo(), out var Ꮡinfo);
        var err = readGoInfo(r, ref (Ꮡinfo).DerefOrNull());
        return (info.header, err);
    });
}

public static void TestReadComments(ж<testing.T> t) {
    testRead(t, readCommentsTests, readComments);
}

internal static slice<readTest> readFailuresTests = slice<readTest>([
    new(
        @"package",
        "syntax error"
    ),
    new(
        "package p\n\x00\nimport `math`\n",
        "unexpected NUL in input"
    ),
    new(
        @"package p; import",
        "syntax error"
    ),
    new(
        @"package p; import """,
        "syntax error"
    ),
    new(
        "package p; import ` \n\n",
        "syntax error"
    ),
    new(
        @"package p; import ""x",
        "syntax error"
    ),
    new(
        @"package p; import _",
        "syntax error"
    ),
    new(
        @"package p; import _ """,
        "syntax error"
    ),
    new(
        @"package p; import _ ""x",
        "syntax error"
    ),
    new(
        @"package p; import .",
        "syntax error"
    ),
    new(
        @"package p; import . """,
        "syntax error"
    ),
    new(
        @"package p; import . ""x",
        "syntax error"
    ),
    new(
        @"package p; import (",
        "syntax error"
    ),
    new(
        @"package p; import (""",
        "syntax error"
    ),
    new(
        @"package p; import (""x",
        "syntax error"
    ),
    new(
        @"package p; import (""x""",
        "syntax error"
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nulˢ => ref nulˢᵇ;
private static readonly @string nulˢᵇ = "NUL"u8;

public static void TestReadFailuresIgnored(ж<testing.T> t) {
    // Syntax errors should not be reported (false arg to readImports).
    // Instead, entire file should be the output and no error.
    // Convert tests not to return syntax errors.
    var tests = new slice<readTest>(len(readFailuresTests));
    copy(tests, readFailuresTests);
    foreach (var i in range(tests)) {
        var tt = Ꮡ(tests, i);
        if (!strings.Contains(tt.err, nulˢ)) {
            tt.err = ""u8;
        }
    }
    testRead(t, tests, (io.Reader r) => {
        ref var info = ref heap(new global::go.go.build_package.fileInfo(), out var Ꮡinfo);
        var err = readGoInfo(r, ref (Ꮡinfo).DerefOrNull());
        return (info.header, err);
    });
}

partial struct readEmbedTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<readEmbedTestsᴛ1> readEmbedTests = slice<readEmbedTestsᴛ1>([
    new(
        "package p\n",
        ""
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n//go:embed x y z\nvar files embed.FS",
        """
test:4:12:x
		 test:4:14:y
		 test:4:16:z
"""u8.Raw()
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n//go:embed x \"\\x79\" `z`\nvar files embed.FS",
        """
test:4:12:x
		 test:4:14:y
		 test:4:21:z
"""u8.Raw()
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n//go:embed x y\n//go:embed z\nvar files embed.FS",
        """
test:4:12:x
		 test:4:14:y
		 test:5:12:z
"""u8.Raw()
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n\t //go:embed x y\n\t //go:embed z\n\t var files embed.FS",
        """
test:4:14:x
		 test:4:16:y
		 test:5:14:z
"""u8.Raw()
    ),
    new(
        "package p\nimport \"embed\"\n//go:embed x y z\nvar files embed.FS",
        """
test:3:12:x
		 test:3:14:y
		 test:3:16:z
"""u8.Raw()
    ),
    new(
        "\ufeffpackage p\nimport \"embed\"\n//go:embed x y z\nvar files embed.FS",
        """
test:3:12:x
		 test:3:14:y
		 test:3:16:z
"""u8.Raw()
    ),
    new(
        "package p\nimport \"embed\"\nvar s = \"/*\"\n//go:embed x\nvar files embed.FS",
        @"test:4:12:x"
    ),
    new(
        """
package p
		 import "embed"
		 var s = "\"\\\\"
		 //go:embed x
		 var files embed.FS
"""u8.Raw(),
        @"test:4:15:x"
    ),
    new(
        "package p\nimport \"embed\"\nvar s = `/*`\n//go:embed x\nvar files embed.FS",
        @"test:4:12:x"
    ),
    new(
        "package p\nimport \"embed\"\nvar s = z/ *y\n//go:embed pointer\nvar pointer embed.FS",
        "test:4:12:pointer"
    ),
    new(
        "package p\n//go:embed x y z\n", // no import, no scan
        ""
    ),
    new(
        "package p\n//go:embed x y z\nvar files embed.FS", // no import, no scan
        ""
    ),
    new(
        "\ufeffpackage p\n//go:embed x y z\nvar files embed.FS", // no import, no scan
        ""
    )
]);

public static void TestReadEmbed(ж<testing.T> t) {
    var fset = token.NewFileSet();
    foreach (var (i, tt) in readEmbedTests) {
        ref var info = ref heap<global::go.go.build_package.fileInfo>(new fileInfo(
            name: "test",
            fset: fset
        ), out var Ꮡinfo);
        var err = readGoInfo(new build_internal_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)), ref info);
        if (err != nil) {
            t.Errorf("#%d: %v", i, err);
            continue;
        }
        var b = Ꮡ(new strings.Builder());
        @string sep = ""u8;
        foreach (var (_, emb) in info.embeds) {
            fmt.Fprintf(new build_internal_test_package.strings_BuilderжWriter(b), "%s%v:%s"u8, sep, emb.pos, emb.pattern);
            sep = "\n";
        }
        @string got = b.String();
        @string want = strings.Join(strings.Fields(tt.@out), "\n");
        if (got != want) {
            t.Errorf("#%d: embeds:\n%s\nwant:\n%s", i, got, want);
        }
    }
}

} // end build_internal_test_package
