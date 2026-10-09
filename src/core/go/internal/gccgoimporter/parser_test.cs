// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using bytes = bytes_package;
using types = global::go.go.types_package;
using strings = strings_package;
using testing = testing_package;
using scanner = text.scanner_package;
using global::go.go;
using io = io_package;
using static global::go.go.@internal.gccgoimporter_package;
using text;

partial class gccgoimporter_internal_test_package {


partial struct typeParserTestsᴛ1 /*dyn*/ {
    internal @string id, typ, want, underlying, methods;
}
internal static slice<typeParserTestsᴛ1> typeParserTests = new typeParserTestsᴛ1[]{
    new(id: "foo"u8, typ: "<type -1>"u8, want: "int8"u8),
    new(id: "foo"u8, typ: "<type 1 *<type -19>>"u8, want: "*error"u8),
    new(id: "foo"u8, typ: "<type 1 *any>"u8, want: "unsafe.Pointer"u8),
    new(id: "foo"u8, typ: "<type 1 \"Bar\" <type 2 *<type 1>>>"u8, want: "foo.Bar"u8, underlying: "*foo.Bar"u8),
    new(id: "foo"u8, typ: "<type 1 \"bar.Foo\" \"bar\" <type -1>\nfunc (? <type 1>) M ();\n>"u8, want: "bar.Foo"u8, underlying: "int8"u8, methods: "func (bar.Foo).M()"u8),
    new(id: "foo"u8, typ: "<type 1 \".bar.foo\" \"bar\" <type -1>>"u8, want: "bar.foo"u8, underlying: "int8"u8),
    new(id: "foo"u8, typ: "<type 1 []<type -1>>"u8, want: "[]int8"u8),
    new(id: "foo"u8, typ: "<type 1 [42]<type -1>>"u8, want: "[42]int8"u8),
    new(id: "foo"u8, typ: "<type 1 map [<type -1>] <type -2>>"u8, want: "map[int8]int16"u8),
    new(id: "foo"u8, typ: "<type 1 chan <type -1>>"u8, want: "chan int8"u8),
    new(id: "foo"u8, typ: "<type 1 chan <- <type -1>>"u8, want: "<-chan int8"u8),
    new(id: "foo"u8, typ: "<type 1 chan -< <type -1>>"u8, want: "chan<- int8"u8),
    new(id: "foo"u8, typ: "<type 1 struct { I8 <type -1>; I16 <type -2> \"i16\"; }>"u8, want: "struct{I8 int8; I16 int16 \"i16\"}"u8),
    new(id: "foo"u8, typ: "<type 1 interface { Foo (a <type -1>, b <type -2>) <type -1>; Bar (? <type -2>, ? ...<type -1>) (? <type -2>, ? <type -1>); Baz (); }>"u8, want: "interface{Bar(int16, ...int8) (int16, int8); Baz(); Foo(a int8, b int16) int8}"u8),
    new(id: "foo"u8, typ: "<type 1 (? <type -1>) <type -2>>"u8, want: "func(int8) int16"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testGoxˢ = "test.gox"u8;

public static void TestTypeParser(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in typeParserTests) {
        ref var p = ref heap(new global::go.go.@internal.gccgoimporter_package.parser(), out var Ꮡp);
        Ꮡp.init(testGoxˢ, new gccgoimporter_internal_test_package.strings_ReaderжReader(strings.NewReader(test.typ)), new map<@string, ж<types.Package>>());
        p.version = "v2"u8;
        p.pkgname = test.id;
        p.pkgpath = test.id;
        p.maybeCreatePackage();
        var typ = Ꮡp.parseType(p.pkg);
        if (p.tok != scanner.EOF) {
            Ꮡt.Errorf("expected full parse, stopped at %q"u8, p.lit);
        }
        // interfaces must be explicitly completed
        {
            var (ityp, _) = typ._<ж<types.Interface>>(ᐧ); if (ityp != nil) {
                ityp.Complete();
            }
        }
        @string got = typ.String();
        if (got != test.want) {
            Ꮡt.Errorf("got type %q, expected %q"u8, got, test.want);
        }
        if (test.underlying != ""u8) {
            @string underlying = typ.Underlying().String();
            if (underlying != test.underlying) {
                Ꮡt.Errorf("got underlying type %q, expected %q"u8, underlying, test.underlying);
            }
        }
        if (test.methods != ""u8) {
            var nt = typ._<ж<types.Named>>();
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            for (nint i = 0; i != nt.NumMethods(); i++) {
                buf.WriteString(nt.Method(i).String());
            }
            @string methods = Ꮡbuf.String();
            if (methods != test.methods) {
                Ꮡt.Errorf("got methods %q, expected %q"u8, methods, test.methods);
            }
        }
    }
}

} // end gccgoimporter_internal_test_package
