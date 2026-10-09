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

internal static readonly @string quote = "`"u8;

internal partial struct readTest {
    // Test input contains ℙ where readGoInfo should stop.
    internal @string @in;
    internal @string err;
}

internal static slice<readTest> readGoInfoTests = new readTest[]{
    new(
        @"package p"u8,
        ""u8
    ),
    new(
        @"package p; import ""x"""u8,
        ""u8
    ),
    new(
        @"package p; import . ""x"""u8,
        ""u8
    ),
    new(
        @"package p; import ""x"";ℙvar x = 1"u8,
        ""u8
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
			
""" + quote + @"x" + quote + """

			_ /*comment*/ 
""" + quote + @"x" + quote + """

			a 
""" + quote + @"x" + quote + """

		)
		import (
		)
		import ()
		import()import()import()
		import();import();import()

		ℙvar x = 1
		
""",
        ""u8
    ),
    new(
        "\ufeff𝔻"u8 + @"package p; import ""x"";ℙvar x = 1"u8,
        ""u8
    )
}.slice();

internal static slice<readTest> readCommentsTests = new readTest[]{
    new(
        @"ℙpackage p"u8,
        ""u8
    ),
    new(
        @"ℙpackage p; import ""x"""u8,
        ""u8
    ),
    new(
        @"ℙpackage p; import . ""x"""u8,
        ""u8
    ),
    new(
        "\ufeff𝔻"u8 + @"ℙpackage p; import . ""x"""u8,
        ""u8
    ),
    new(
        """
// foo

		/* bar */

		/* quux */ // baz

		/*/ zot */

		// asdf
		ℙHello, world
"""u8,
        ""u8
    ),
    new(
        "\ufeff𝔻"u8 + """
// foo

		/* bar */

		/* quux */ // baz

		/*/ zot */

		// asdf
		ℙHello, world
"""u8,
        ""u8
    )
}.slice();

internal static void testRead(ж<testing.T> Ꮡt, slice<readTest> tests, Func<io.Reader, (slice<byte>, error)> read) {
    foreach (var (i, tt) in tests) {
        var (beforeP, afterP, _) = strings.Cut(tt.@in, "ℙ"u8);
        @string @in = beforeP + afterP;
        @string testOut = beforeP;
        {
            var (beforeD, afterD, ok) = strings.Cut(beforeP, "𝔻"u8); if (ok) {
                @in = beforeD + afterD + afterP;
                testOut = afterD;
            }
        }
        var r = strings.NewReader(@in);
        var (buf, err) = read(new build_internal_test_package.strings_ReaderжReader(r));
        if (err != default!) {
            if (tt.err == ""u8){
                Ꮡt.Errorf("#%d: err=%q, expected success (%q)"u8, i, err, ((@string)buf));
            } else 
            if (!strings.Contains(err.Error(), tt.err)) {
                Ꮡt.Errorf("#%d: err=%q, expected %q"u8, i, err, tt.err);
            }
            continue;
        }
        if (tt.err != ""u8) {
            Ꮡt.Errorf("#%d: success, expected %q"u8, i, tt.err);
            continue;
        }
        @string @out = ((@string)buf);
        if (@out != testOut) {
            Ꮡt.Errorf("#%d: wrong output:\nhave %q\nwant %q\n"u8, i, @out, testOut);
        }
    }
}

public static void TestReadGoInfo(ж<testing.T> Ꮡt) {
    testRead(Ꮡt, readGoInfoTests, (io.Reader r) => {
        ref var info = ref heap(new global::go.go.build_package.fileInfo(), out var Ꮡinfo);
        var err = readGoInfo(r, ref (Ꮡinfo).DerefOrNull());
        return (info.header, err);
    });
}

public static void TestReadComments(ж<testing.T> Ꮡt) {
    testRead(Ꮡt, readCommentsTests, readComments);
}

internal static slice<readTest> readFailuresTests = new readTest[]{
    new(
        @"package"u8,
        "syntax error"u8
    ),
    new(
        "package p\n\x00\nimport `math`\n"u8,
        "unexpected NUL in input"u8
    ),
    new(
        @"package p; import"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import """u8,
        "syntax error"u8
    ),
    new(
        "package p; import ` \n\n"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import ""x"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import _"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import _ """u8,
        "syntax error"u8
    ),
    new(
        @"package p; import _ ""x"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import ."u8,
        "syntax error"u8
    ),
    new(
        @"package p; import . """u8,
        "syntax error"u8
    ),
    new(
        @"package p; import . ""x"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import ("u8,
        "syntax error"u8
    ),
    new(
        @"package p; import ("""u8,
        "syntax error"u8
    ),
    new(
        @"package p; import (""x"u8,
        "syntax error"u8
    ),
    new(
        @"package p; import (""x"""u8,
        "syntax error"u8
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nulˢ = "NUL"u8;

public static void TestReadFailuresIgnored(ж<testing.T> Ꮡt) {
    // Syntax errors should not be reported (false arg to readImports).
    // Instead, entire file should be the output and no error.
    // Convert tests not to return syntax errors.
    var tests = new slice<readTest>(len(readFailuresTests));
    copy(tests, readFailuresTests);
    foreach (var (i, _) in tests) {
        var tt = Ꮡ(tests, i);
        if (!strings.Contains((~tt).err, nulˢ)) {
            tt.Value.err = ""u8;
        }
    }
    testRead(Ꮡt, tests, (io.Reader r) => {
        ref var info = ref heap(new global::go.go.build_package.fileInfo(), out var Ꮡinfo);
        var err = readGoInfo(r, ref (Ꮡinfo).DerefOrNull());
        return (info.header, err);
    });
}

// no import, no scan
// no import, no scan
// no import, no scan

partial struct readEmbedTestsᴛ1 /*dyn*/ {
    internal @string @in, @out;
}
internal static slice<readEmbedTestsᴛ1> readEmbedTests = new readEmbedTestsᴛ1[]{
    new(
        "package p\n"u8,
        ""u8
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n//go:embed x y z\nvar files embed.FS"u8,
        """
test:4:12:x
		 test:4:14:y
		 test:4:16:z
"""u8
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n//go:embed x \"\\x79\" `z`\nvar files embed.FS"u8,
        """
test:4:12:x
		 test:4:14:y
		 test:4:21:z
"""u8
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n//go:embed x y\n//go:embed z\nvar files embed.FS"u8,
        """
test:4:12:x
		 test:4:14:y
		 test:5:12:z
"""u8
    ),
    new(
        "package p\nimport \"embed\"\nvar i int\n\t //go:embed x y\n\t //go:embed z\n\t var files embed.FS"u8,
        """
test:4:14:x
		 test:4:16:y
		 test:5:14:z
"""u8
    ),
    new(
        "package p\nimport \"embed\"\n//go:embed x y z\nvar files embed.FS"u8,
        """
test:3:12:x
		 test:3:14:y
		 test:3:16:z
"""u8
    ),
    new(
        "\ufeffpackage p\nimport \"embed\"\n//go:embed x y z\nvar files embed.FS"u8,
        """
test:3:12:x
		 test:3:14:y
		 test:3:16:z
"""u8
    ),
    new(
        "package p\nimport \"embed\"\nvar s = \"/*\"\n//go:embed x\nvar files embed.FS"u8,
        @"test:4:12:x"u8
    ),
    new(
        """
package p
		 import "embed"
		 var s = "\"\\\\"
		 //go:embed x
		 var files embed.FS
"""u8,
        @"test:4:15:x"u8
    ),
    new(
        "package p\nimport \"embed\"\nvar s = `/*`\n//go:embed x\nvar files embed.FS"u8,
        @"test:4:12:x"u8
    ),
    new(
        "package p\nimport \"embed\"\nvar s = z/ *y\n//go:embed pointer\nvar pointer embed.FS"u8,
        "test:4:12:pointer"u8
    ),
    new(
        "package p\n//go:embed x y z\n"u8,
        ""u8
    ),
    new(
        "package p\n//go:embed x y z\nvar files embed.FS"u8,
        ""u8
    ),
    new(
        "\ufeffpackage p\n//go:embed x y z\nvar files embed.FS"u8,
        ""u8
    )
}.slice();

public static void TestReadEmbed(ж<testing.T> Ꮡt) {
    var fset = token.NewFileSet();
    foreach (var (i, tt) in readEmbedTests) {
        ref var info = ref heap<global::go.go.build_package.fileInfo>(out var Ꮡinfo);
        info = new fileInfo(
            name: "test"u8,
            fset: fset
        );
        var err = readGoInfo(new build_internal_test_package.strings_ReaderжReader(strings.NewReader(tt.@in)), ref info);
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, i, err);
            continue;
        }
        var b = Ꮡ(new strings.Builder(nil));
        @string sep = ""u8;
        foreach (var (_, emb) in info.embeds) {
            fmt.Fprintf(new build_internal_test_package.strings_BuilderжWriter(b), "%s%v:%s"u8, sep, emb.pos, emb.pattern);
            sep = "\n"u8;
        }
        @string got = b.String();
        @string want = strings.Join(strings.Fields(tt.@out), "\n"u8);
        if (got != want) {
            Ꮡt.Errorf("#%d: embeds:\n%s\nwant:\n%s"u8, i, got, want);
        }
    }
}

} // end build_internal_test_package
