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
internal static slice<fieldsTestsᴛ1> fieldsTests = slice<fieldsTestsᴛ1>([new(
    testName: "SimpleStruct",
    val: new Δtypeᴛ37(),
    expect: slice<structField>([new(
        name: "A",
        index: slice<nint>([0])
    ), new(
        name: "B",
        index: slice<nint>([1])
    ), new(
        name: "C",
        index: slice<nint>([2])
    )
    ])
), new(
    testName: "NonEmbeddedStructMember",
    val: new Δtypeᴛ38(),
    expect: slice<structField>([new(
        name: "A",
        index: slice<nint>([0])
    )
    ])
), new(
    testName: "EmbeddedExportedStruct",
    val: new Δtypeᴛ39(),
    expect: slice<structField>([new(
        name: "SFG",
        index: slice<nint>([0])
    ), new(
        name: "F",
        index: slice<nint>([0, 0])
    ), new(
        name: "G",
        index: slice<nint>([0, 1])
    )
    ])
), new(
    testName: "EmbeddedUnexportedStruct",
    val: new Δtypeᴛ40(),
    expect: slice<structField>([new(
        name: "sFG",
        index: slice<nint>([0])
    ), new(
        name: "F",
        index: slice<nint>([0, 0])
    ), new(
        name: "G",
        index: slice<nint>([0, 1])
    )
    ])
), new(
    testName: "TwoEmbeddedStructsWithCancelingMembers",
    val: new Δtypeᴛ41(),
    expect: slice<structField>([new(
        name: "SFG",
        index: slice<nint>([0])
    ), new(
        name: "G",
        index: slice<nint>([0, 1])
    ), new(
        name: "SF",
        index: slice<nint>([1])
    )
    ])
), new(
    testName: "EmbeddedStructsWithSameFieldsAtDifferentDepths",
    val: new Δtypeᴛ42(),
    expect: slice<structField>([new(
        name: "SFGH3",
        index: slice<nint>([0])
    ), new(
        name: "SFGH2",
        index: slice<nint>([0, 0])
    ), new(
        name: "SFGH1",
        index: slice<nint>([0, 0, 0])
    ), new(
        name: "SFGH",
        index: slice<nint>([0, 0, 0, 0])
    ), new(
        name: "H",
        index: slice<nint>([0, 0, 0, 0, 2])
    ), new(
        name: "SG1",
        index: slice<nint>([1])
    ), new(
        name: "SG",
        index: slice<nint>([1, 0])
    ), new(
        name: "G",
        index: slice<nint>([1, 0, 0])
    ), new(
        name: "SFG2",
        index: slice<nint>([2])
    ), new(
        name: "SFG1",
        index: slice<nint>([2, 0])
    ), new(
        name: "SFG",
        index: slice<nint>([2, 0, 0])
    ), new(
        name: "SF2",
        index: slice<nint>([3])
    ), new(
        name: "SF1",
        index: slice<nint>([3, 0])
    ), new(
        name: "SF",
        index: slice<nint>([3, 0, 0])
    ), new(
        name: "L",
        index: slice<nint>([4])
    )
    ])
), new(
    testName: "EmbeddedPointerStruct",
    val: new Δtypeᴛ43(),
    expect: slice<structField>([new(
        name: "SF",
        index: slice<nint>([0])
    ), new(
        name: "F",
        index: slice<nint>([0, 0])
    )
    ])
), new(
    testName: "EmbeddedNotAPointer",
    val: new Δtypeᴛ44(),
    expect: slice<structField>([new(
        name: "M",
        index: slice<nint>([0])
    )
    ])
), new(
    testName: "RecursiveEmbedding",
    val: new Rec1(),
    expect: slice<structField>([new(
        name: "Rec2",
        index: slice<nint>([0])
    ), new(
        name: "F",
        index: slice<nint>([0, 0])
    ), new(
        name: "Rec1",
        index: slice<nint>([0, 1])
    )
    ])
), new(
    testName: "RecursiveEmbedding2",
    val: new Rec2(),
    expect: slice<structField>([new(
        name: "F",
        index: slice<nint>([0])
    ), new(
        name: "Rec1",
        index: slice<nint>([1])
    ), new(
        name: "Rec2",
        index: slice<nint>([1, 0])
    )
    ])
), new(
    testName: "RecursiveEmbedding3",
    val: new RS3(),
    expect: slice<structField>([new(
        name: "RS2",
        index: slice<nint>([0])
    ), new(
        name: "RS1",
        index: slice<nint>([1])
    ), new(
        name: "i",
        index: slice<nint>([1, 0])
    )
    ])
)
]);

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

public static void TestFields(ж<Δtesting.T> t) {
    foreach (var (_, vᴛ1) in fieldsTests) {
        ref var test = ref heap(new fieldsTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.testName, (ж<Δtesting.T> tΔ1) => {
            var typ = TypeOf(testʗ1.val);
            var fields = VisibleFields(typ);
            {
                nint got = len(fields);
                nint want = len(testʗ1.expect); if (got != want) {
                    tΔ1.Fatalf("unexpected field count; got %d want %d", got, want);
                }
            }

            foreach (var (j, field) in fields) {
                var expect = testʗ1.expect[j];
                tΔ1.Logf("field %d: %s", j, expect.name);
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
                    tΔ1.Fatalf("unexpected field result\ngot %#v\nwant %#v", gotField, expectField);
                }

                // Sanity check that we can actually access the field by the
                // expected name.
                var (gotField1, ok) = typ.FieldByName(expect.name);
                if (!ok) {
                    tΔ1.Fatalf("field %q not accessible by name", expect.name);
                }
                if (!DeepEqual(gotField1, expectField)) {
                    tΔ1.Fatalf("unexpected FieldByName result; got %#v want %#v", gotField1, expectField);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;
internal static ref readonly @string embeddedStructFieldAˢ => ref embeddedStructFieldAˢᵇ;
private static readonly @string embeddedStructFieldAˢᵇ = "embedded struct field A"u8;

internal partial struct TestFieldByIndexErr_A /*dyn*/ {
    public @string S;
}

internal partial struct TestFieldByIndexErr_B /*dyn*/ {
    /*embed*/ public ж<TestFieldByIndexErr_A> A;
}

// Must not panic with nil embedded pointer.
public static void TestFieldByIndexErr(ж<Δtesting.T> t) {
    var v = ValueOf(new TestFieldByIndexErr_B());
    var (_, err) = v.FieldByIndexErr(slice<nint>([0, 0]));
    if (err == nil) {
        t.Fatal(expectedErrorˢ);
    }
    if (!strings.Contains(err.Error(), embeddedStructFieldAˢ)) {
        t.Fatal(err);
    }
}

} // end reflect_test_package
