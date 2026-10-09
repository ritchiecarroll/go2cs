// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using Δio = io_package;
using static reflect_package;
using strings = strings_package;
using Δtesting = testing_package;
using @unsafe = unsafe_package;
using global::go.go;
using static global::go.reflect_internal_test_package;
using Δreflect = reflect_package;

partial class reflect_test_package {

internal partial struct TestImplicitMapConversion_MyBuffer /*global::go.bytes_package.Buffer*/;

public static void TestImplicitMapConversion(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test implicit conversions in MapIndex and SetMapIndex.
    {
        // direct
        var m = new map<nint, nint>();
        var mv = ValueOf(m);
        mv.SetMapIndex(ValueOf((nint)(1)), ValueOf((nint)(2)));
        var (x, ok) = m[1, ꟷ];
        if (x != 2) {
            Ꮡt.Errorf("#1 after SetMapIndex(1,2): %d, %t (map=%v)"u8, x, ok, m);
        }
        {
            nint n = mv.MapIndex(ValueOf((nint)(1))).Interface()._<nint>(); if (n != 2) {
                Ꮡt.Errorf("#1 MapIndex(1) = %d"u8, n);
            }
        }
    }
    {
        // convert interface key
        var m = new map<any, nint>();
        var mv = ValueOf(m);
        mv.SetMapIndex(ValueOf((nint)(1)), ValueOf((nint)(2)));
        var (x, ok) = m[(nint)(1), ꟷ];
        if (x != 2) {
            Ꮡt.Errorf("#2 after SetMapIndex(1,2): %d, %t (map=%v)"u8, x, ok, m);
        }
        {
            nint n = mv.MapIndex(ValueOf((nint)(1))).Interface()._<nint>(); if (n != 2) {
                Ꮡt.Errorf("#2 MapIndex(1) = %d"u8, n);
            }
        }
    }
    {
        // convert interface value
        var m = new map<nint, any>();
        var mv = ValueOf(m);
        mv.SetMapIndex(ValueOf((nint)(1)), ValueOf((nint)(2)));
        var (x, ok) = m[1, ꟷ];
        if (!AreEqual(x, (nint)(2))) {
            Ꮡt.Errorf("#3 after SetMapIndex(1,2): %d, %t (map=%v)"u8, x, ok, m);
        }
        {
            nint n = mv.MapIndex(ValueOf((nint)(1))).Interface()._<nint>(); if (n != 2) {
                Ꮡt.Errorf("#3 MapIndex(1) = %d"u8, n);
            }
        }
    }
    {
        // convert both interface key and interface value
        var m = new map<any, any>();
        var mv = ValueOf(m);
        mv.SetMapIndex(ValueOf((nint)(1)), ValueOf((nint)(2)));
        var (x, ok) = m[(nint)(1), ꟷ];
        if (!AreEqual(x, (nint)(2))) {
            Ꮡt.Errorf("#4 after SetMapIndex(1,2): %d, %t (map=%v)"u8, x, ok, m);
        }
        {
            nint n = mv.MapIndex(ValueOf((nint)(1))).Interface()._<nint>(); if (n != 2) {
                Ꮡt.Errorf("#4 MapIndex(1) = %d"u8, n);
            }
        }
    }
    {
        // convert both, with non-empty interfaces
        var m = new map<Δio.Reader, Δio.Writer>();
        var mv = ValueOf(m);
        var b1 = @new<bytes.Buffer>();
        var b2 = @new<bytes.Buffer>();
        mv.SetMapIndex(ValueOf(b1.OrTypedNil()), ValueOf(b2.OrTypedNil()));
        var (x, ok) = m[new reflect_test_package.bytes_BufferжReader(b1), ꟷ];
        if (!AreEqual(x, b2)) {
            Ꮡt.Errorf("#5 after SetMapIndex(b1, b2): %p (!= %p), %t (map=%v)"u8, x, b2.OrTypedNil(), ok, m);
        }
        {
            @unsafe.Pointer p = (uintptr)mv.MapIndex(ValueOf(b1.OrTypedNil())).Elem().UnsafePointer(); if (p != @unsafe.Pointer.FromPinnedBox(b2)) {
                Ꮡt.Errorf("#5 MapIndex(b1) = %#x want %p"u8, @unsafe.Pointer.OrTypedNil(p), b2.OrTypedNil());
            }
        }
    }
    {
        // convert channel direction
        var m = new map</*<-*/channel<nint>, channel<nint>>();
        var mv = ValueOf(m);
        var c1 = new channel<nint>(0);
        var c2 = new channel<nint>(0);
        mv.SetMapIndex(ValueOf(c1), ValueOf(c2));
        var (x, ok) = m[c1, ꟷ];
        if (x != c2) {
            Ꮡt.Errorf("#6 after SetMapIndex(c1, c2): %p (!= %p), %t (map=%v)"u8, x, c2, ok, m);
        }
        {
            @unsafe.Pointer p = (uintptr)mv.MapIndex(ValueOf(c1)).UnsafePointer(); if (p != (uintptr)ValueOf(c2).UnsafePointer()) {
                Ꮡt.Errorf("#6 MapIndex(c1) = %#x want %p"u8, @unsafe.Pointer.OrTypedNil(p), c2);
            }
        }
    }
    {
        var m = new map<ж<TestImplicitMapConversion_MyBuffer>, ж<bytes.Buffer>>();
        var mv = ValueOf(m);
        var b1 = @new<TestImplicitMapConversion_MyBuffer>();
        var b2 = @new<bytes.Buffer>();
        mv.SetMapIndex(ValueOf(b1.OrTypedNil()), ValueOf(b2.OrTypedNil()));
        var (x, ok) = m[b1, ꟷ];
        if (x != b2) {
            Ꮡt.Errorf("#7 after SetMapIndex(b1, b2): %p (!= %p), %t (map=%v)"u8, x.OrTypedNil(), b2.OrTypedNil(), ok, m);
        }
        {
            @unsafe.Pointer p = (uintptr)mv.MapIndex(ValueOf(b1.OrTypedNil())).UnsafePointer(); if (p != @unsafe.Pointer.FromPinnedBox(b2)) {
                Ꮡt.Errorf("#7 MapIndex(b1) = %#x want %p"u8, @unsafe.Pointer.OrTypedNil(p), b2.OrTypedNil());
            }
        }
    }
}

public static void TestImplicitSetConversion(ж<Δtesting.T> Ꮡt) {
    // Assume TestImplicitMapConversion covered the basics.
    // Just make sure conversions are being applied at all.
    ref var r = ref heap<Δio.Reader>(out var Ꮡr);
    var b = @new<bytes.Buffer>();
    var rv = ValueOf(Ꮡr).Elem();
    rv.Set(ValueOf(b.OrTypedNil()));
    if (!AreEqual(r, b)) {
        Ꮡt.Errorf("after Set: r=%T(%v)"u8, r, r);
    }
}

public static void TestImplicitSendConversion(ж<Δtesting.T> Ꮡt) {
    var c = new channel<Δio.Reader>(10);
    var b = @new<bytes.Buffer>();
    ValueOf(c).Send(ValueOf(b.OrTypedNil()));
    {
        var bb = ᐸꟷ(c); if (!AreEqual(bb, b)) {
            Ꮡt.Errorf("Received %p != %p"u8, bb, b.OrTypedNil());
        }
    }
}

public static void TestImplicitCallConversion(ж<Δtesting.T> Ꮡt) {
    // Arguments must be assignable to parameter types.
    var fv = ValueOf((Func<Δio.Writer, @string, (nint, error)>)(Δio.WriteString));
    var b = @new<strings.Builder>();
    fv.Call(new reflectꓸValue[]{ValueOf(b.OrTypedNil()), ValueOf(helloWorldˢ)}.slice());
    if (b.String() != "hello world"u8) {
        Ꮡt.Errorf("After call: string=%q want %q"u8, b.String(), helloWorldˢ);
    }
}

public static void TestImplicitAppendConversion(ж<Δtesting.T> Ꮡt) {
    // Arguments must be assignable to the slice's element type.
    ref var s = ref heap<slice<Δio.Reader>>(out var Ꮡs);
    s = new Δio.Reader[]{}.slice();
    var sv = ValueOf(Ꮡs).Elem();
    var b = @new<bytes.Buffer>();
    sv.Set(Append(sv, ValueOf(b.OrTypedNil())));
    if (len(s) != 1 || !AreEqual(s[0], b)) {
        Ꮡt.Errorf("after append: s=%v want [%p]"u8, s, b.OrTypedNil());
    }
}


partial struct implementsTestsᴛ1 /*dyn*/ {
    internal any x;
    internal any t;
    internal bool b;
}
internal static slice<implementsTestsᴛ1> implementsTests = new implementsTestsᴛ1[]{
    new(@new<ж<bytes.Buffer>>(), @new<Δio.Reader>(), true),
    new(@new<bytes.Buffer>(), @new<Δio.Reader>(), false),
    new(@new<ж<bytes.Buffer>>(), @new<Δio.ReaderAt>(), false),
    new(@new<ж<ast.Ident>>(), @new<ast.Expr>(), true),
    new(@new<ж<notAnExpr>>(), @new<ast.Expr>(), false),
    new(@new<ж<ast.Ident>>(), @new<notASTExpr>(), false),
    new(@new<notASTExpr>(), @new<ast.Expr>(), false),
    new(@new<ast.Expr>(), @new<notASTExpr>(), false),
    new(@new<ж<notAnExpr>>(), @new<notASTExpr>(), true)
}.slice();

partial struct notAnExpr {
}

internal static tokenꓸPos Pos(this notAnExpr _) {
    return token.NoPos;
}

internal static tokenꓸPos End(this notAnExpr _) {
    return token.NoPos;
}

internal static void exprNode(this notAnExpr _) {
}

partial interface notASTExpr :
    ast.Node
{
    void exprNode();
}

public static void TestImplements(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in implementsTests) {
        var xv = TypeOf(tt.x).Elem();
        var xt = TypeOf(tt.t).Elem();
        {
            var b = xv.Implements(xt); if (b != tt.b) {
                Ꮡt.Errorf("(%s).Implements(%s) = %v, want %v"u8, xv.String(), xt.String(), b, tt.b);
            }
        }
    }
}

// test runs implementsTests too
internal static slice<implementsTestsᴛ1> assignableTests = new implementsTestsᴛ1[]{
    new(@new<channel<nint>>(), Ꮡ(/*<-*/channel<nint>.RecvOnly), true),
    new(Ꮡ(/*<-*/channel<nint>.RecvOnly), @new<channel<nint>>(), false),
    new(@new<ж<nint>>(), @new<IntPtr>(), true),
    new(@new<IntPtr>(), @new<ж<nint>>(), true),
    new(@new<IntPtr>(), @new<IntPtr1>(), false),
    new(@new<Ch>(), Ꮡ(/*<-*/channel<any>.RecvOnly), true)
}.slice();

partial class IntPtr /*ж<nint>*/;

partial class IntPtr1 /*ж<nint>*/;

partial struct Ch /*<-chan any*/;

public static void TestAssignableTo(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in appendꓸꓸꓸ(assignableTests, implementsTests)) {
        var xv = TypeOf(tt.x).Elem();
        var xt = TypeOf(tt.t).Elem();
        {
            var b = xv.AssignableTo(xt); if (b != tt.b) {
                Ꮡt.Errorf("(%s).AssignableTo(%s) = %v, want %v"u8, xv.String(), xt.String(), b, tt.b);
            }
        }
    }
}

} // end reflect_test_package
