// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using reflect = reflect_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using testing = testing_package;
using go.runtime;

partial class debug_test_package {

// strip removes two leading tabs after each newline of s.
internal static @string strip(@string s) {
    @string replaced = strings.ReplaceAll(s, "\n\t\t"u8, "\n"u8);
    if (len(replaced) > 0 && replaced[0] == (rune)'\n') {
        replaced = replaced[1..];
    }
    return replaced;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pathRscIoFortuneModRscIoˢ = """

		path	rsc.io/fortune
		mod	rsc.io/fortune	v1.0.0
		
"""u8.Raw();
private static readonly object pathCmdTest2jsonˢ = (@string)@"path	cmd/test2json"u8;
private static readonly @string go118PathExampleComMModˢ = """

		go	1.18
		path	example.com/m
		mod	example.com/m	(devel)	
		build	-compiler=gc
		
"""u8.Raw();
private static readonly @string go118PathExampleComMˢ = """

		go	1.18
		path	example.com/m
		build	-compiler=gc
		
"""u8.Raw();
private static readonly @string go118PathExampleComMˢ2 = """

		go 1.18
		path example.com/m
		build CRAZY_ENV="requires\nescaping"
		
"""u8.Raw();

public static void FuzzParseBuildInfoRoundTrip(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    // Package built from outside a module, missing some fields..
    f.Add(strip(pathRscIoFortuneModRscIoˢ));

    // Package built from the standard library, missing some fields..
    f.Add(pathCmdTest2jsonˢ);

    // Package built from inside a module.
    f.Add(strip(go118PathExampleComMModˢ));

    // Package built in GOPATH mode.
    f.Add(strip(go118PathExampleComMˢ));

    // Escaped build info.
    f.Add(strip(go118PathExampleComMˢ2));

    Ꮡf.Fuzz((ж<testing.T> t, @string s) => {
        var (bi, err) = debug.ParseBuildInfo(s);
        if (err != default!) {
            // Not a round-trippable BuildInfo string.
            t.Log(err);
            return;
        }

        // s2 could have different escaping from s.
        // However, it should parse to exactly the same contents.
        @string s2 = bi.String();
        (var bi2, err) = debug.ParseBuildInfo(s2);
        if (err != default!) {
            t.Fatalf("%v:\n%s"u8, err, s2);
        }

        if (!reflect.DeepEqual(bi2.OrTypedNil(), bi.OrTypedNil())) {
            t.Fatalf("Parsed representation differs.\ninput:\n%s\noutput:\n%s"u8, s, s2);
        }
    });
}

} // end debug_test_package
