// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go;

partial class constraints_package {

partial struct Frog {
    public @string Name;
    public @string Color;
}

partial interface ConstraintTest1<ΔT> {
    //  Type constraints: string | []int | map[string]int | chan string | *int | [2]int | Frog
    // Derived operators: none
    @string Upper();
}

partial interface ConstraintTest2<ΔT> {
    //  Type constraints: string | chan string | *int | [2]int | Frog
    // Derived operators: none
    @string Lower();
}

partial interface Signed<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~int | ~int8 | ~int16 | ~int32 | ~int64
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface Unsigned<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: ~uint | ~uint8 | ~uint16 | ~uint32 | ~uint64 | ~uintptr
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface Integer<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: Signed | Unsigned
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface PromotedTest1<ΔT> /*operators = Sum, Arithmetic, Integer, Comparable, Ordered*/ {
    //  Type constraints: Signed
    // Derived operators: +, -, *, /, %, &, |, ^, <<, >>, ==, !=, <, <=, >, >=
}

partial interface PromotedTest2<ΔT> :
    ConstraintTest1<ΔT>
{
    //  Type constraints: ConstraintTest1
    // Derived operators: none
}

partial interface PromotedTest3<ΔT> :
    ConstraintTest2<ΔT>
{
    //  Type constraints: ConstraintTest2
    // Derived operators: none
}

partial interface Float<ΔT> /*operators = Sum, Arithmetic, Comparable, Ordered*/ {
    //  Type constraints: ~float32 | ~float64
    // Derived operators: +, -, *, /, ==, !=, <, <=, >, >=
}

partial interface Complex<ΔT> /*operators = Sum, Arithmetic, Comparable*/ {
    //  Type constraints: ~complex64 | ~complex128
    // Derived operators: +, -, *, /, ==, !=
}

partial interface Ordered<ΔT> /*operators = Sum, Comparable, Ordered*/ {
    //  Type constraints: Integer | Float | ~string
    // Derived operators: +, ==, !=, <, <=, >, >=
}

partial struct recordA {
    internal nint n;
}

partial struct recordB {
    internal nint n;
}

partial struct recordC {
    internal nint n;
}

partial interface RecordUnion<ΔT> {
    //  Type constraints: recordA | recordB | recordC
    // Derived operators: none
}

internal static T firstOf<T>(slice<T> p)
    where T : /* RecordUnion */ new()
{
    T zero = GoZero<T>();
    if (len(p) == 0) {
        return zero;
    }
    return p[0];
}

public static nint UseRecordUnion() {
    var a = firstOf(new recordA[]{new(1)}.slice());
    var b = firstOf(new recordB[]{new(2)}.slice());
    var c = firstOf(new recordC[]{new(4)}.slice());
    return a.n + b.n + c.n;
}

} // end constraints_package
