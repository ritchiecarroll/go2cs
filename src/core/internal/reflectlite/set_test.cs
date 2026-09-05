// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static global::go.@internal.reflectlite_package;
using io = io_package;
using testing = testing_package;
using global::go.@internal;
using global::go.go;
using reflectlite = global::go.@internal.reflectlite_package;
using static global::go.@internal.reflectlite_internal_test_package;

partial class reflectlite_test_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸbytes() {
    builtin.initPackage(typeof(bytes_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸio() {
    builtin.initPackage(typeof(io_package));
}

public static void TestImplicitSetConversion(ж<testing.T> Ꮡt) {
    // Assume TestImplicitMapConversion covered the basics.
    // Just make sure conversions are being applied at all.
    ref var r = ref heap<io.Reader>(out var Ꮡr);
    var b = @new<bytes.Buffer>();
    var rv = ValueOf(Ꮡr).Elem();
    rv.Set(ValueOf(b.OrTypedNil()));
    if (!AreEqual(r, b)) {
        Ꮡt.Errorf("after Set: r=%T(%v)"u8, r, r);
    }
}


[GoType("dyn")] partial struct implementsTestsᴛ1 {
    internal any x;
    internal any t;
    internal bool b;
}
internal static slice<implementsTestsᴛ1> implementsTests = new implementsTestsᴛ1[]{
    new(@new<ж<bytes.Buffer>>(), @new<io.Reader>(), true),
    new(@new<bytes.Buffer>(), @new<io.Reader>(), false),
    new(@new<ж<bytes.Buffer>>(), @new<io.ReaderAt>(), false),
    new(@new<ж<ast.Ident>>(), @new<ast.Expr>(), true),
    new(@new<ж<notAnExpr>>(), @new<ast.Expr>(), false),
    new(@new<ж<ast.Ident>>(), @new<notASTExpr>(), false),
    new(@new<notASTExpr>(), @new<ast.Expr>(), false),
    new(@new<ast.Expr>(), @new<notASTExpr>(), false),
    new(@new<ж<notAnExpr>>(), @new<notASTExpr>(), true),
    new(@new<mapError>(), @new<error>(), true),
    new(@new<ж<mapError>>(), @new<error>(), true)
}.slice();

[GoType] partial struct notAnExpr {
}

internal static tokenꓸPos Pos(this notAnExpr _) {
    return token.NoPos;
}

internal static tokenꓸPos End(this notAnExpr _) {
    return token.NoPos;
}

internal static void exprNode(this notAnExpr _) {
}

[GoType] partial interface notASTExpr :
    ast.Node
{
    void exprNode();
}

[GoType("map[@string, @string]")] partial struct mapError;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapErrorˢ = "mapError"u8;

internal static @string Error(this mapError _) {
    return mapErrorˢ;
}

internal static error _ᴛ1ʗ = new mapError(new map<@string, @string>{});

internal static error _ᴛ2ʗ = new reflectlite_test_package.mapErrorжerror(@new<mapError>());

public static void TestImplements(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in implementsTests) {
        var xv = TypeOf(tt.x).Elem();
        var xt = TypeOf(tt.t).Elem();
        {
            var b = xv.Implements(xt); if (b != tt.b) {
                Ꮡt.Errorf("(%s).Implements(%s) = %v, want %v"u8, TypeString(xv), TypeString(xt), b, tt.b);
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

[GoType("ж<nint>")] partial class IntPtr;

[GoType("ж<nint>")] partial class IntPtr1;

[GoType("chan any")] [GoChanDir(GoChanDir.Recv)] partial struct Ch;

public static void TestAssignableTo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, tt) in append(assignableTests, implementsTests.ꓸꓸꓸ)) {
        var xv = TypeOf(tt.x).Elem();
        var xt = TypeOf(tt.t).Elem();
        {
            var b = xv.AssignableTo(xt); if (b != tt.b) {
                Ꮡt.Errorf("%d:AssignableTo: got %v, want %v"u8, i, b, tt.b);
            }
        }
    }
}

} // end reflectlite_test_package
