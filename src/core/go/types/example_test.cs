// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Only run where builders (build.golang.org) have
// access to compiled packages for import.
//
//go:build !android && !ios && !js && !wasip1
namespace go.go;

// This file shows examples of basic usage of the go/types API.
//
// To locate a Go package, use (*go/build.Context).Import.
// To load, parse, and type-check a complete Go program
// from source, use golang.org/x/tools/go/loader.
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using format = global::go.go.format_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using log = log_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using global::go.go;
using io = io_package;
using iter = iter_package;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string temperatureˢ => ref temperatureˢᵇ;
private static readonly @string temperatureˢᵇ = "temperature"u8;
internal static ref readonly @string aFAFDˢ => ref aFAFDˢᵇ;
private static readonly @string aFAFDˢᵇ = @" 0x[a-fA-F\d]*"u8;

// ExampleScope prints the tree of Scopes of a package created from a
// set of parsed files.
public static void ExampleScope() {
    var filesᴷ = new stackAppend<ж<ast.File>>();

    // Parse the source files for a package.
    var fset = token.NewFileSet();
    slice<ж<ast.File>> files = default;
    foreach (var (_, src) in range(slice<@string>([
        """
package main
import "fmt"
func main() {
	freezing := FToC(-18)
	fmt.Println(freezing, Boiling) }

"""u8.Raw(),
        """
package main
import "fmt"
type Celsius float64
func (c Celsius) String() string { return fmt.Sprintf("%g°C", c) }
func FToC(f float64) Celsius { return Celsius(f - 32 / 9 * 5) }
const Boiling Celsius = 100
func Unused() { {}; {{ var x int; _ = x }} } // make sure empty block scopes get printed

"""u8.Raw()
    ]))) {
        files = append(filesᴷ, files, mustParse(fset, src));
    }

    // Type-check a package consisting of these files.
    // Type information for the imported "fmt" package
    // comes from $GOROOT/pkg/$GOOS_$GOOARCH/fmt.a.
    ref var conf = ref heap<types.Config>(new types.Config(Importer: defaultImporter(fset)), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check(temperatureˢ, fset, files, nil);
    if (err != nil) {
        log.Fatal(err);
    }

    // Print the tree of scopes.
    // For determinism, we redact addresses.
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    pkg.Scope().WriteTo(new types_test_package.strings_BuilderжWriter(Ꮡbuf), 0, true);
    var rx = regexp.MustCompile(aFAFDˢ);
    fmt.Println(rx.ReplaceAllString(buf.String(), ""u8));

    // Output:
    // package "temperature" scope {
    // .  const temperature.Boiling temperature.Celsius
    // .  type temperature.Celsius float64
    // .  func temperature.FToC(f float64) temperature.Celsius
    // .  func temperature.Unused()
    // .  func temperature.main()
    // .  main scope {
    // .  .  package fmt
    // .  .  function scope {
    // .  .  .  var freezing temperature.Celsius
    // .  .  }
    // .  }
    // .  main scope {
    // .  .  package fmt
    // .  .  function scope {
    // .  .  .  var c temperature.Celsius
    // .  .  }
    // .  .  function scope {
    // .  .  .  var f float64
    // .  .  }
    // .  .  function scope {
    // .  .  .  block scope {
    // .  .  .  }
    // .  .  .  block scope {
    // .  .  .  .  block scope {
    // .  .  .  .  .  var x int
    // .  .  .  .  }
    // .  .  .  }
    // .  .  }
    // .  }
    // }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string celsiusGoˢ => ref celsiusGoˢᵇ;
private static readonly @string celsiusGoˢᵇ = "celsius.go"u8;
internal static ref readonly @string celsiusˢ => ref celsiusˢᵇ;
private static readonly @string celsiusˢᵇ = "Celsius"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = """

package temperature
import "fmt"
type Celsius float64
func (c Celsius) String() string  { return fmt.Sprintf("%g°C", c) }
func (c *Celsius) SetF(f float64) { *c = Celsius(f - 32 / 9 * 5) }

type S struct { I; m int }
type I interface { m() byte }

"""u8.Raw();

// ExampleMethodSet prints the method sets of various types.
public static void ExampleMethodSet() {
    // Parse a single source file.
    @string input = inputᶜ;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, celsiusGoˢ, input, 0);
    if (err != nil) {
        log.Fatal(err);
    }

    // Type-check a package consisting of this file.
    // Type information for the imported packages
    // comes from $GOROOT/pkg/$GOOS_$GOOARCH/fmt.a.
    ref var conf = ref heap<types.Config>(new types.Config(Importer: defaultImporter(fset)), out var Ꮡconf);
    (var pkg, err) = Ꮡconf.Check(temperatureˢ, fset, [f], nil);
    if (err != nil) {
        log.Fatal(err);
    }

    // Print the method sets of Celsius and *Celsius.
    var celsius = pkg.Scope().Lookup(celsiusˢ).Type();
    foreach (var (_, t) in range(slice<typesꓸType>([celsius, new types.PointerжΔType(types.NewPointer(celsius))]))) {
        fmt.Printf("Method set of %s:\n"u8, t);
        foreach (var m in range<ж<types.Selection>>(types.NewMethodSet(t).Methods().Invoke)) {
            fmt.Println(m.OrTypedNil());
        }
        fmt.Println();
    }

    // Print the method set of S.
    var styp = pkg.Scope().Lookup("S").Type();
    fmt.Printf("Method set of %s:\n"u8, styp);
    fmt.Println(types.NewMethodSet(styp).OrTypedNil());

    // Output:
    // Method set of temperature.Celsius:
    // method (temperature.Celsius) String() string
    //
    // Method set of *temperature.Celsius:
    // method (*temperature.Celsius) SetF(f float64)
    // method (*temperature.Celsius) String() string
    //
    // Method set of temperature.S:
    // MethodSet {}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fibˢ => ref fibˢᵇ;
private static readonly @string fibˢᵇ = "fib"u8;
internal static readonly object defsAndUsesOfEachNamedˢ = (@string)"Defs and Uses of each named object:"u8;
internal static readonly object typesAndValuesOfEachˢ = (@string)"Types and Values of each expression:"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ1 => ref inputᶜ1ᵇ;
private static readonly @string inputᶜ1ᵇ = """

package fib

type S string

var a, b, c = len(b), S(c), "hello"

func fib(x int) int {
	if x < 2 {
		return x
	}
	return fib(x-1) - fib(x-2)
}
"""u8.Raw();

// ExampleInfo prints various facts recorded by the type checker in a
// types.Info struct: definitions of and references to each named object,
// and the type, value, and mode of every expression in the package.
public static void ExampleInfo() {
    var itemsᴷ = new stackAppend<@string>();

    // Parse a single source file.
    @string input = inputᶜ1;
    // We need a specific fileset in this test below for positions.
    // Cannot use typecheck helper.
    var fset = token.NewFileSet();
    var f = mustParse(fset, input);

    // Type-check the package.
    // We create an empty map for each kind of input
    // we're interested in, and Check populates them.
    ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
        Types: new map<ast.Expr, types.TypeAndValue>(),
        Defs: new map<ж<ast.Ident>, types.Object>(),
        Uses: new map<ж<ast.Ident>, types.Object>()
    ), out var Ꮡinfo);
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check(fibˢ, fset, [f], Ꮡinfo);
    if (err != nil) {
        log.Fatal(err);
    }

    // Print package-level variables in initialization order.
    fmt.Printf("InitOrder: %v\n\n"u8, info.InitOrder);

    // For each named object, print the line and
    // column of its definition and each of its uses.
    fmt.Println(defsAndUsesOfEachNamedˢ);
    var usesByObj = new map<types.Object, slice<@string>>();
    foreach (var (id, obj) in info.Uses) {
        var posn = fset.Position(id.Pos());
        @string lineCol = fmt.Sprintf("%d:%d"u8, posn.Line, posn.Column);
        usesByObj[obj] = append(usesByObj[obj], lineCol);
    }
    slice<@string> items = default;
    foreach (var (obj, uses) in usesByObj) {
        slices.Sort<slice<@string>, @string>(uses);
        @string item = fmt.Sprintf("%s:\n  defined at %s\n  used at %s"u8,
            types.ObjectString(obj, types.RelativeTo(pkg)),
            fset.Position(obj.Pos()),
            strings.Join(uses, ", "));
        items = append(itemsᴷ, items, item);
    }
    slices.Sort<slice<@string>, @string>(items); // sort by line:col, in effect
    fmt.Println(strings.Join(items, "\n"));
    fmt.Println();

    fmt.Println(typesAndValuesOfEachˢ);
    items = default;
    foreach (var (expr, tv) in info.Types) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var posn = fset.Position(expr.Pos());
        @string tvstr = tv.Type.String();
        if (tv.Value != nil) {
            tvstr += " = "u8 + tv.Value.String();
        }
        // line:col | expr | mode : type = value
        fmt.Fprintf(new types_test_package.strings_BuilderжWriter(Ꮡbuf), "%2d:%2d | %-19s | %-7s : %s"u8,
            posn.Line, posn.Column, exprString(fset, expr),
            mode(tv), tvstr);
        items = append(items, buf.String());
    }
    slices.Sort<slice<@string>, @string>(items);
    fmt.Println(strings.Join(items, "\n"));

    // Output:
    // InitOrder: [c = "hello" b = S(c) a = len(b)]
    //
    // Defs and Uses of each named object:
    // builtin len:
    //   defined at -
    //   used at 6:15
    // func fib(x int) int:
    //   defined at fib:8:6
    //   used at 12:20, 12:9
    // type S string:
    //   defined at fib:4:6
    //   used at 6:23
    // type int:
    //   defined at -
    //   used at 8:12, 8:17
    // type string:
    //   defined at -
    //   used at 4:8
    // var b S:
    //   defined at fib:6:8
    //   used at 6:19
    // var c string:
    //   defined at fib:6:11
    //   used at 6:25
    // var x int:
    //   defined at fib:8:10
    //   used at 10:10, 12:13, 12:24, 9:5
    //
    // Types and Values of each expression:
    //  4: 8 | string              | type    : string
    //  6:15 | len                 | builtin : func(fib.S) int
    //  6:15 | len(b)              | value   : int
    //  6:19 | b                   | var     : fib.S
    //  6:23 | S                   | type    : fib.S
    //  6:23 | S(c)                | value   : fib.S
    //  6:25 | c                   | var     : string
    //  6:29 | "hello"             | value   : string = "hello"
    //  8:12 | int                 | type    : int
    //  8:17 | int                 | type    : int
    //  9: 5 | x                   | var     : int
    //  9: 5 | x < 2               | value   : untyped bool
    //  9: 9 | 2                   | value   : int = 2
    // 10:10 | x                   | var     : int
    // 12: 9 | fib                 | value   : func(x int) int
    // 12: 9 | fib(x - 1)          | value   : int
    // 12: 9 | fib(x-1) - fib(x-2) | value   : int
    // 12:13 | x                   | var     : int
    // 12:13 | x - 1               | value   : int
    // 12:15 | 1                   | value   : int = 1
    // 12:20 | fib                 | value   : func(x int) int
    // 12:20 | fib(x - 2)          | value   : int
    // 12:24 | x                   | var     : int
    // 12:24 | x - 2               | value   : int
    // 12:26 | 2                   | value   : int = 2
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string varˢ => ref varˢᵇ;
private static readonly @string varˢᵇ = "var"u8;
internal static ref readonly @string mapindexˢ => ref mapindexˢᵇ;
private static readonly @string mapindexˢᵇ = "mapindex"u8;
internal static ref readonly @string unknownˢ => ref unknownˢᵇ;
private static readonly @string unknownˢᵇ = "unknown"u8;

internal static @string mode(types.TypeAndValue tv) {
    switch (ᐧ) {
    case {} when tv.IsVoid(): {
        return voidˢ;
    }
    case {} when tv.IsType(): {
        return typeˢ;
    }
    case {} when tv.IsBuiltin(): {
        return builtinˢ;
    }
    case {} when tv.IsNil(): {
        return nilˢ;
    }
    case {} when tv.Assignable(): {
        if (tv.Addressable()) {
            return varˢ;
        }
        return mapindexˢ;
    }
    case {} when tv.IsValue(): {
        return valueˢ;
    }
    default: {
        return unknownˢ;
    }}
}

internal static @string exprString(ж<token.FileSet> fset, ast.Expr expr) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    format.Node(new types_test_package.strings_BuilderжWriter(Ꮡbuf), fset, expr);
    return buf.String();
}

} // end types_test_package
