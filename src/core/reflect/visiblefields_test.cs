// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static reflect_package;
using strings = strings_package;
using Δtesting = testing_package;
using static global::go.reflect_internal_test_package;
using Δreflect = reflect_package;

partial class reflect_test_package {

partial struct structField {
    internal @string name;
    internal slice<nint> index;
}

partial struct fieldsTestsᴛ1 /*dyn*/ {
    internal @string testName;
    internal any val;
    internal slice<structField> expect;
}
    partial struct Δtypeᴛ37 /*dyn*/ {
        public nint A;
        public @string B;
        public bool C;
    }
    partial struct typeᴛ38_A /*dyn*/ {
        public nint X;
    }

    partial struct Δtypeᴛ38 /*dyn*/ {
        public typeᴛ38_A A;
    }
    partial struct Δtypeᴛ39 /*dyn*/ {
        /*embed*/ public SFG SFG;
    }
    partial struct Δtypeᴛ40 /*dyn*/ {
        /*embed*/ internal sFG sFG;
    }
    partial struct Δtypeᴛ41 /*dyn*/ {
        /*embed*/ public SFG SFG;
        /*embed*/ public SF SF;
    }
    partial struct Δtypeᴛ42 /*dyn*/ {
        /*embed*/ public SFGH3 SFGH3;
        /*embed*/ public SG1 SG1;
        /*embed*/ public SFG2 SFG2;
        /*embed*/ public SF2 SF2;
        public nint L;
    }
    partial struct Δtypeᴛ43 /*dyn*/ {
        /*embed*/ public ж<SF> SF;
    }
    partial struct Δtypeᴛ44 /*dyn*/ {
        /*embed*/ public ΔM M;
    }
internal static slice<fieldsTestsᴛ1> fieldsTests = new fieldsTestsᴛ1[]{new(
    testName: "SimpleStruct"u8,
    val: new Δtypeᴛ37(),
    expect: new structField[]{new(
        name: "A"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "B"u8,
        index: new nint[]{1}.slice()
    ), new(
        name: "C"u8,
        index: new nint[]{2}.slice()
    )
    }.slice()
), new(
    testName: "NonEmbeddedStructMember"u8,
    val: new Δtypeᴛ38(),
    expect: new structField[]{new(
        name: "A"u8,
        index: new nint[]{0}.slice()
    )
    }.slice()
), new(
    testName: "EmbeddedExportedStruct"u8,
    val: new Δtypeᴛ39(),
    expect: new structField[]{new(
        name: "SFG"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "F"u8,
        index: new nint[]{0, 0}.slice()
    ), new(
        name: "G"u8,
        index: new nint[]{0, 1}.slice()
    )
    }.slice()
), new(
    testName: "EmbeddedUnexportedStruct"u8,
    val: new Δtypeᴛ40(),
    expect: new structField[]{new(
        name: "sFG"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "F"u8,
        index: new nint[]{0, 0}.slice()
    ), new(
        name: "G"u8,
        index: new nint[]{0, 1}.slice()
    )
    }.slice()
), new(
    testName: "TwoEmbeddedStructsWithCancelingMembers"u8,
    val: new Δtypeᴛ41(),
    expect: new structField[]{new(
        name: "SFG"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "G"u8,
        index: new nint[]{0, 1}.slice()
    ), new(
        name: "SF"u8,
        index: new nint[]{1}.slice()
    )
    }.slice()
), new(
    testName: "EmbeddedStructsWithSameFieldsAtDifferentDepths"u8,
    val: new Δtypeᴛ42(),
    expect: new structField[]{new(
        name: "SFGH3"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "SFGH2"u8,
        index: new nint[]{0, 0}.slice()
    ), new(
        name: "SFGH1"u8,
        index: new nint[]{0, 0, 0}.slice()
    ), new(
        name: "SFGH"u8,
        index: new nint[]{0, 0, 0, 0}.slice()
    ), new(
        name: "H"u8,
        index: new nint[]{0, 0, 0, 0, 2}.slice()
    ), new(
        name: "SG1"u8,
        index: new nint[]{1}.slice()
    ), new(
        name: "SG"u8,
        index: new nint[]{1, 0}.slice()
    ), new(
        name: "G"u8,
        index: new nint[]{1, 0, 0}.slice()
    ), new(
        name: "SFG2"u8,
        index: new nint[]{2}.slice()
    ), new(
        name: "SFG1"u8,
        index: new nint[]{2, 0}.slice()
    ), new(
        name: "SFG"u8,
        index: new nint[]{2, 0, 0}.slice()
    ), new(
        name: "SF2"u8,
        index: new nint[]{3}.slice()
    ), new(
        name: "SF1"u8,
        index: new nint[]{3, 0}.slice()
    ), new(
        name: "SF"u8,
        index: new nint[]{3, 0, 0}.slice()
    ), new(
        name: "L"u8,
        index: new nint[]{4}.slice()
    )
    }.slice()
), new(
    testName: "EmbeddedPointerStruct"u8,
    val: new Δtypeᴛ43(),
    expect: new structField[]{new(
        name: "SF"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "F"u8,
        index: new nint[]{0, 0}.slice()
    )
    }.slice()
), new(
    testName: "EmbeddedNotAPointer"u8,
    val: new Δtypeᴛ44(),
    expect: new structField[]{new(
        name: "M"u8,
        index: new nint[]{0}.slice()
    )
    }.slice()
), new(
    testName: "RecursiveEmbedding"u8,
    val: new Rec1(nil),
    expect: new structField[]{new(
        name: "Rec2"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "F"u8,
        index: new nint[]{0, 0}.slice()
    ), new(
        name: "Rec1"u8,
        index: new nint[]{0, 1}.slice()
    )
    }.slice()
), new(
    testName: "RecursiveEmbedding2"u8,
    val: new Rec2(nil),
    expect: new structField[]{new(
        name: "F"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "Rec1"u8,
        index: new nint[]{1}.slice()
    ), new(
        name: "Rec2"u8,
        index: new nint[]{1, 0}.slice()
    )
    }.slice()
), new(
    testName: "RecursiveEmbedding3"u8,
    val: new RS3(nil),
    expect: new structField[]{new(
        name: "RS2"u8,
        index: new nint[]{0}.slice()
    ), new(
        name: "RS1"u8,
        index: new nint[]{1}.slice()
    ), new(
        name: "i"u8,
        index: new nint[]{1, 0}.slice()
    )
    }.slice()
)
}.slice();

partial struct SFG {
    public nint F;
    public nint G;
}

partial struct SFG1 {
    /*embed*/ public SFG SFG;
}

partial struct SFG2 {
    /*embed*/ public SFG1 SFG1;
}

partial struct SFGH {
    public nint F;
    public nint G;
    public nint H;
}

partial struct SFGH1 {
    /*embed*/ public SFGH SFGH;
}

partial struct SFGH2 {
    /*embed*/ public SFGH1 SFGH1;
}

partial struct SFGH3 {
    /*embed*/ public SFGH2 SFGH2;
}

partial struct SF {
    public nint F;
}

partial struct SF1 {
    /*embed*/ public SF SF;
}

partial struct SF2 {
    /*embed*/ public SF1 SF1;
}

partial struct SG {
    public nint G;
}

partial struct SG1 {
    /*embed*/ public SG SG;
}

partial struct sFG {
    public nint F;
    public nint G;
}

partial struct RS1 {
    internal nint i;
}

partial struct RS2 {
    /*embed*/ public RS1 RS1;
}

partial struct RS3 {
    /*embed*/ public RS2 RS2;
    /*embed*/ public RS1 RS1;
}

partial struct ΔM /*map[@string, any]*/;

partial struct Rec1 {
    /*embed*/ public ж<Rec2> Rec2;
}

partial struct Rec2 {
    public @string F;
    /*embed*/ public ж<Rec1> Rec1;
}

public static void TestFields(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, vᴛ1) in fieldsTests) {
        ref var test = ref heap(new fieldsTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.testName, (ж<Δtesting.T> tΔ1) => {
            var typ = TypeOf(testʗ1.val);
            var fields = VisibleFields(typ);
            {
                nint got = len(fields);
                nint want = len(testʗ1.expect); if (got != want) {
                    tΔ1.Fatalf("unexpected field count; got %d want %d"u8, got, want);
                }
            }

            foreach (var (j, field) in fields) {
                var expect = testʗ1.expect[j];
                tΔ1.Logf("field %d: %s"u8, j, expect.name);
                var gotField = typ.FieldByIndex(field.Index);
                // Unfortunately, FieldByIndex does not return
                // a field with the same index that we passed in,
                // so we set it to the expected value so that
                // it can be compared later with the result of FieldByName.
                gotField.Index = field.Index;
                var expectField = typ.FieldByIndex(expect.index);
                // ditto.
                expectField.Index = expect.index;
                if (!DeepEqual(gotField, expectField)) {
                    tΔ1.Fatalf("unexpected field result\ngot %#v\nwant %#v"u8, gotField, expectField);
                }

                // Sanity check that we can actually access the field by the
                // expected name.
                var (gotField1, ok) = typ.FieldByName(expect.name);
                if (!ok) {
                    tΔ1.Fatalf("field %q not accessible by name"u8, expect.name);
                }
                if (!DeepEqual(gotField1, expectField)) {
                    tΔ1.Fatalf("unexpected FieldByName result; got %#v want %#v"u8, gotField1, expectField);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;
internal static readonly @string embeddedStructFieldAˢ = "embedded struct field A"u8;

internal partial struct TestFieldByIndexErr_A /*dyn*/ {
    public @string S;
}

internal partial struct TestFieldByIndexErr_B /*dyn*/ {
    /*embed*/ public ж<TestFieldByIndexErr_A> A;
}

// Must not panic with nil embedded pointer.
public static void TestFieldByIndexErr(ж<Δtesting.T> Ꮡt) {
    var v = ValueOf(new TestFieldByIndexErr_B(nil));
    var (_, err) = v.FieldByIndexErr(new nint[]{0, 0}.slice());
    if (err == default!) {
        Ꮡt.Fatal(expectedErrorˢ);
    }
    if (!strings.Contains(err.Error(), embeddedStructFieldAˢ)) {
        Ꮡt.Fatal(err);
    }
}

} // end reflect_test_package
