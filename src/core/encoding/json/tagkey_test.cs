// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using testing = testing_package;
using static go.encoding.json_package;

partial class json_internal_test_package {

internal partial struct basicLatin2xTag {
    public @string V; /*`json:"$%-/"`*/
}

internal partial struct basicLatin3xTag {
    public @string V; /*`json:"0123456789"`*/
}

internal partial struct basicLatin4xTag {
    public @string V; /*`json:"ABCDEFGHIJKLMO"`*/
}

internal partial struct basicLatin5xTag {
    public @string V; /*`json:"PQRSTUVWXYZ_"`*/
}

internal partial struct basicLatin6xTag {
    public @string V; /*`json:"abcdefghijklmno"`*/
}

internal partial struct basicLatin7xTag {
    public @string V; /*`json:"pqrstuvwxyz"`*/
}

internal partial struct miscPlaneTag {
    public @string V; /*`json:"色は匂へど"`*/
}

internal partial struct percentSlashTag {
    public @string V; /*`json:"text/html%"`*/ // https://golang.org/issue/2718
}

internal partial struct punctuationTag {
    public @string V; /*`json:"!#$%&()*+-./:;<=>?@[]^_{|}~ "`*/ // https://golang.org/issue/3546
}

internal partial struct dashTag {
    public @string V; /*`json:"-,"`*/
}

internal partial struct emptyTag {
    public @string W;
}

internal partial struct misnamedTag {
    public @string X; /*`jsom:"Misnamed"`*/
}

internal partial struct badFormatTag {
    public @string Y; /*`:"BadFormat"`*/
}

internal partial struct badCodeTag {
    public @string Z; /*`json:" !\"#&'()*+,."`*/
}

internal partial struct spaceTag {
    public @string Q; /*`json:"With space"`*/
}

internal partial struct unicodeTag {
    public @string W; /*`json:"Ελλάδα"`*/
}

internal partial struct TestStructTagObjectKey_tests /*dyn*/ {
    public partial ref CaseName CaseName { get; }
    internal any raw;
    internal @string value;
    internal @string key;
}

public static void TestStructTagObjectKey(ж<testing.T> Ꮡt) {
    var tests = new TestStructTagObjectKey_tests[]{
        new(Name(""u8), new basicLatin2xTag("2x"u8), "2x"u8, "$%-/"u8),
        new(Name(""u8), new basicLatin3xTag("3x"u8), "3x"u8, "0123456789"u8),
        new(Name(""u8), new basicLatin4xTag("4x"u8), "4x"u8, "ABCDEFGHIJKLMO"u8),
        new(Name(""u8), new basicLatin5xTag("5x"u8), "5x"u8, "PQRSTUVWXYZ_"u8),
        new(Name(""u8), new basicLatin6xTag("6x"u8), "6x"u8, "abcdefghijklmno"u8),
        new(Name(""u8), new basicLatin7xTag("7x"u8), "7x"u8, "pqrstuvwxyz"u8),
        new(Name(""u8), new miscPlaneTag("いろはにほへと"u8), "いろはにほへと"u8, "色は匂へど"u8),
        new(Name(""u8), new dashTag("foo"u8), "foo"u8, "-"u8),
        new(Name(""u8), new emptyTag("Pour Moi"u8), "Pour Moi"u8, "W"u8),
        new(Name(""u8), new misnamedTag("Animal Kingdom"u8), "Animal Kingdom"u8, "X"u8),
        new(Name(""u8), new badFormatTag("Orfevre"u8), "Orfevre"u8, "Y"u8),
        new(Name(""u8), new badCodeTag("Reliable Man"u8), "Reliable Man"u8, "Z"u8),
        new(Name(""u8), new percentSlashTag("brut"u8), "brut"u8, "text/html%"u8),
        new(Name(""u8), new punctuationTag("Union Rags"u8), "Union Rags"u8, "!#$%&()*+-./:;<=>?@[]^_{|}~ "u8),
        new(Name(""u8), new spaceTag("Perreddu"u8), "Perreddu"u8, "With space"u8),
        new(Name(""u8), new unicodeTag("Loukanikos"u8), "Loukanikos"u8, "Ελλάδα"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestStructTagObjectKey_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.raw);
            if (err != default!) {
                tΔ1.Fatalf("%s: Marshal error: %v"u8, ttʗ1.Where, err);
            }
            ref var f = ref heap<any>(out var Ꮡf);
            err = Unmarshal(b, Ꮡf);
            if (err != default!) {
                tΔ1.Fatalf("%s: Unmarshal error: %v"u8, ttʗ1.Where, err);
            }
            foreach (var (k, v) in f._<map<@string, any>>()) {
                if (k == ttʗ1.key){
                    {
                        var (s, ok) = v._<@string>(ᐧ); if (!ok || s != ttʗ1.value) {
                            tΔ1.Fatalf("%s: Unmarshal(%#q) value:\n\tgot:  %q\n\twant: %q"u8, ttʗ1.Where, b, s, ttʗ1.value);
                        }
                    }
                } else {
                    tΔ1.Fatalf("%s: Unmarshal(%#q): unexpected key: %q"u8, ttʗ1.Where, b, k);
                }
            }
        });
    }
}

} // end json_internal_test_package
