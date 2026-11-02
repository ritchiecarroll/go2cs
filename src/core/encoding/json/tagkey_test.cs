// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.jsonv2
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
    /*embed*/ public CaseName CaseName;
    internal any raw;
    internal @string value;
    internal @string key;
}

public static void TestStructTagObjectKey(ж<testing.T> t) {
    var tests = slice<TestStructTagObjectKey_tests>([
        new(Name(""u8), new basicLatin2xTag("2x"), "2x", "$%-/"),
        new(Name(""u8), new basicLatin3xTag("3x"), "3x", "0123456789"),
        new(Name(""u8), new basicLatin4xTag("4x"), "4x", "ABCDEFGHIJKLMO"),
        new(Name(""u8), new basicLatin5xTag("5x"), "5x", "PQRSTUVWXYZ_"),
        new(Name(""u8), new basicLatin6xTag("6x"), "6x", "abcdefghijklmno"),
        new(Name(""u8), new basicLatin7xTag("7x"), "7x", "pqrstuvwxyz"),
        new(Name(""u8), new miscPlaneTag("いろはにほへと"), "いろはにほへと", "色は匂へど"),
        new(Name(""u8), new dashTag("foo"), "foo", "-"),
        new(Name(""u8), new emptyTag("Pour Moi"), "Pour Moi", "W"),
        new(Name(""u8), new misnamedTag("Animal Kingdom"), "Animal Kingdom", "X"),
        new(Name(""u8), new badFormatTag("Orfevre"), "Orfevre", "Y"),
        new(Name(""u8), new badCodeTag("Reliable Man"), "Reliable Man", "Z"),
        new(Name(""u8), new percentSlashTag("brut"), "brut", "text/html%"),
        new(Name(""u8), new punctuationTag("Union Rags"), "Union Rags", "!#$%&()*+-./:;<=>?@[]^_{|}~ "),
        new(Name(""u8), new spaceTag("Perreddu"), "Perreddu", "With space"),
        new(Name(""u8), new unicodeTag("Loukanikos"), "Loukanikos", "Ελλάδα")
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestStructTagObjectKey_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.Name, (ж<testing.T> tΔ1) => {
            var (b, err) = Marshal(ttʗ1.raw);
            if (err != nil) {
                tΔ1.Fatalf("%s: Marshal error: %v", ttʗ1.Where, err);
            }
            ref var f = ref heap<any>(out var Ꮡf);
            err = Unmarshal(b, Ꮡf);
            if (err != nil) {
                tΔ1.Fatalf("%s: Unmarshal error: %v", ttʗ1.Where, err);
            }
            foreach (var (k, v) in f._<map<@string, any>>()) {
                if (k == ttʗ1.key) {
                    {
                        var (s, ok) = v._<@string>(ᐧ); if (!ok || s != ttʗ1.value) {
                            tΔ1.Fatalf("%s: Unmarshal(%#q) value:\n\tgot:  %q\n\twant: %q", ttʗ1.Where, b, s, ttʗ1.value);
                        }
                    }
                } else {
                    tΔ1.Fatalf("%s: Unmarshal(%#q): unexpected key: %q", ttʗ1.Where, b, k);
                }
            }
        });
    }
}

} // end json_internal_test_package
