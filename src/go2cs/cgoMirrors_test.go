// cgoMirrors_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// cgoMirrorShapesSource carries a C struct of every member shape a mirror handles, under the names and with the
// padding fields cmd/cgo writes, plus the two shapes a mirror refuses.
const cgoMirrorShapesSource = `package cgoshapes

import "unsafe"

type _Ctype_char int8
type _Ctype_int int32
type _Ctype_long int64
type _Ctype_float float32
type _Ctype__Bool bool

type _Ctype_struct_inner struct {
	a _Ctype_char
	_ [3]byte
	b _Ctype_int
}

type _Ctype_union_u [8]byte

type _Ctype_struct_shapes struct {
	flag  _Ctype__Bool
	_     [7]byte
	count _Ctype_long
	inner _Ctype_struct_inner
	pair  [2]_Ctype_struct_inner
	u     _Ctype_union_u
	ratio _Ctype_float
	_     [4]byte
	next  *_Ctype_struct_shapes
	raw   unsafe.Pointer
	grid  [2][3]_Ctype_int
}

type _Ctype_struct_ints struct {
	a _Ctype_int
	_ [4]byte
	b _Ctype_long
	p *_Ctype_int
	q _Ctype__Bool
	_ [7]byte
}

type _Ctype_struct_clash struct {
	CheckLayout _Ctype_int
	Hold        _Ctype_int
}

type _Ctype_struct_flex struct {
	n    _Ctype_int
	data [0]_Ctype_int
}

type _Ctype_struct_cplx struct {
	z complex64
}
`

// cgoMirrorAmd64 is the sizes the converter uses for an amd64 target.
var cgoMirrorAmd64 = types.SizesFor("gc", "amd64")

func checkCgoMirrorShapes(t *testing.T) *types.Package {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgoshapes.go", cgoMirrorShapesSource, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default(), Sizes: cgoMirrorAmd64}).Check("cgoshapes", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	return pkg
}

func lookupCgoStruct(t *testing.T, pkg *types.Package, name string) *types.Named {
	t.Helper()

	named, ok := pkg.Scope().Lookup(name).Type().(*types.Named)

	if !ok {
		t.Fatalf("%s is not a named type", name)
	}

	return named
}

// cgoFixtureStructXMirror is the mirror of the Phase 1 fixture's struct x on amd64, as the generator writes it.
const cgoFixtureStructXMirror = `[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = 40)]
internal struct _Ctype_struct_xᴍ
{
    [global::System.Runtime.InteropServices.FieldOffset(0)] public int a;
    [global::System.Runtime.InteropServices.FieldOffset(8)] public double b;
    [global::System.Runtime.InteropServices.FieldOffset(16)] public ᴍarray8_sbyte name;
    [global::System.Runtime.InteropServices.FieldOffset(24)] public byte flag;
    [global::System.Runtime.InteropServices.FieldOffset(32)] public nint next;

    internal static void CheckLayoutᴍ()
    {
        Holdᴍ("a", "int", global::System.Runtime.CompilerServices.Unsafe.SizeOf<int>(), 4);
        Holdᴍ("b", "double", global::System.Runtime.CompilerServices.Unsafe.SizeOf<double>(), 8);
        Holdᴍ("name", "ᴍarray8_sbyte", global::System.Runtime.CompilerServices.Unsafe.SizeOf<ᴍarray8_sbyte>(), 8);
        Holdᴍ("name element", "sbyte", global::System.Runtime.CompilerServices.Unsafe.SizeOf<sbyte>(), 1);
        Holdᴍ("flag", "byte", global::System.Runtime.CompilerServices.Unsafe.SizeOf<byte>(), 1);
        Holdᴍ("next", "nint", global::System.Runtime.CompilerServices.Unsafe.SizeOf<nint>(), 8);
    }

    private static void Holdᴍ(string member, string clrType, int clrSize, int cSize)
    {
        if (clrSize != cSize)
            throw new global::System.InvalidOperationException($"cgo mirror _Ctype_struct_x: member {member} is {clrSize} bytes as {clrType} and {cSize} bytes in C, so the mirror cannot cross");
    }
}

[global::System.Runtime.CompilerServices.InlineArray(8)]
internal struct ᴍarray8_sbyte
{
    private sbyte m_element;
}

`

// The Phase 1 fixture's struct x, through the generator: the C layout cmd/cgo recorded (a mixed struct of an int, a
// double, a char array, an unsigned char and a self-pointer) becomes an explicit-layout mirror, written exactly.
func TestCgoMirrorOfTheFixturesStructXIsItsCLayout(t *testing.T) {
	pkg := loadCgoFixture(t, true)
	builder := newCgoMirrorBuilder(cgoMirrorAmd64)

	if _, err := builder.mirror(lookupCgoStruct(t, pkg.Types, "_Ctype_struct_x")); err != nil {
		t.Fatal(err)
	}

	if got := builder.csharp(); got != cgoFixtureStructXMirror {
		t.Errorf("mirror of _Ctype_struct_x\n got:\n%s\nwant:\n%s", got, cgoFixtureStructXMirror)
	}
}

// Every member shape, against go/types itself: each member's offset is Offsetsof's, each mirror's size Sizeof's, and
// cmd/cgo's padding fields are left out. Nested structs nest their mirrors, an array of structs and an array of arrays
// get helpers of helpers, and a union is its byte array.
func TestCgoMirrorShapesFollowGoTypesLayout(t *testing.T) {
	pkg := checkCgoMirrorShapes(t)
	builder := newCgoMirrorBuilder(cgoMirrorAmd64)
	shapes := lookupCgoStruct(t, pkg, "_Ctype_struct_shapes")
	built, err := builder.mirror(shapes)

	if err != nil {
		t.Fatal(err)
	}

	want := []struct {
		name, clrType string
		offset        int64
	}{
		{"flag", "byte", 0},
		{"count", "long", 8},
		{"inner", "_Ctype_struct_innerᴍ", 16},
		{"pair", "ᴍarray2__Ctype_struct_innerᴍ", 24},
		{"u", "ᴍarray8_byte", 40},
		{"ratio", "float", 48},
		{"next", "nint", 56},
		{"raw", "nint", 64},
		{"grid", "ᴍarray2_ᴍarray3_int", 72},
	}

	if len(built.fields) != len(want) {
		t.Fatalf("mirror has %d members, want %d: %+v", len(built.fields), len(want), built.fields)
	}

	structType := shapes.Underlying().(*types.Struct)
	vars := make([]*types.Var, structType.NumFields())

	for i := range vars {
		vars[i] = structType.Field(i)
	}

	offsets := cgoMirrorAmd64.Offsetsof(vars)
	goOffset := map[string]int64{}

	for i, field := range vars {
		goOffset[field.Name()] = offsets[i]
	}

	for i, member := range built.fields {
		if member.name != want[i].name || member.clrType != want[i].clrType || member.offset != want[i].offset {
			t.Errorf("member %d is %s %s at %d, want %s %s at %d", i, member.clrType, member.name, member.offset, want[i].clrType, want[i].name, want[i].offset)
		}

		if member.offset != goOffset[member.name] {
			t.Errorf("member %s is at %d, go/types puts it at %d", member.name, member.offset, goOffset[member.name])
		}
	}

	if built.size != 96 || built.size != cgoMirrorAmd64.Sizeof(structType) {
		t.Errorf("mirror size %d, want 96 (go/types: %d)", built.size, cgoMirrorAmd64.Sizeof(structType))
	}

	wantHelpers := []string{"ᴍarray2__Ctype_struct_innerᴍ", "ᴍarray8_byte", "ᴍarray3_int", "ᴍarray2_ᴍarray3_int"}

	if strings.Join(builder.arrayAt, " ") != strings.Join(wantHelpers, " ") {
		t.Errorf("inline-array helpers %v, want %v", builder.arrayAt, wantHelpers)
	}
}

// A flexible array member and a complex number are refused by name, the struct and the member named.
func TestCgoMirrorRefusesWhatABlittableMirrorCannotHold(t *testing.T) {
	pkg := checkCgoMirrorShapes(t)

	for name, want := range map[string]string{
		"_Ctype_struct_flex": "_Ctype_struct_flex.data is a flexible array member",
		"_Ctype_struct_cplx": "_Ctype_struct_cplx.z is a complex number",
	} {
		_, err := newCgoMirrorBuilder(cgoMirrorAmd64).mirror(lookupCgoStruct(t, pkg, name))

		if err == nil || !strings.Contains(err.Error(), want) {
			t.Errorf("mirror of %s: err %v, want it to say %q", name, err, want)
		}
	}
}

// §11.7 (RULED): only an integer- and pointer-only struct crosses by value. A float, an inline array or a union
// anywhere inside, nested structs included, keeps it to the pointer path.
func TestCgoMirrorPassesByValueOnlyIntegerAndPointerStructs(t *testing.T) {
	pkg := checkCgoMirrorShapes(t)
	builder := newCgoMirrorBuilder(cgoMirrorAmd64)

	for name, want := range map[string]bool{"_Ctype_struct_ints": true, "_Ctype_struct_inner": true, "_Ctype_struct_shapes": false} {
		built, err := builder.mirror(lookupCgoStruct(t, pkg, name))

		if err != nil {
			t.Fatal(err)
		}

		if got := builder.passableByValue(built); got != want {
			t.Errorf("%s passable by value: %t, want %t", name, got, want)
		}
	}
}

// The mirrors compiled and run by .NET itself. A standalone console project holds every mirror this file builds; for
// each it runs CheckLayout and prints the CLR's own reading of every member offset and of the mirror's size, which the
// test holds to go/types. Then the planted UNDERFLOW (§11.2): struct ints' 8-byte C long member b, generated as a C#
// int through the test-only override, must be refused by its CheckLayout, naming the struct and the member.
func TestCgoMirrorsCompileAndHoldTheirLayoutUnderDotnet(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds and runs a project with the installed .NET SDK")
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("cgo mirror layout NOT RUN: no dotnet on PATH (Inconclusive, not a pass)")
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	shapesPkg := checkCgoMirrorShapes(t)
	fixture := loadCgoFixture(t, true)
	builder := newCgoMirrorBuilder(cgoMirrorAmd64)
	var built []*cgoMirror

	for _, named := range []*types.Named{
		lookupCgoStruct(t, fixture.Types, "_Ctype_struct_x"),
		lookupCgoStruct(t, shapesPkg, "_Ctype_struct_shapes"),
		lookupCgoStruct(t, shapesPkg, "_Ctype_struct_ints"),
		lookupCgoStruct(t, shapesPkg, "_Ctype_struct_clash"),
	} {
		mirror, err := builder.mirror(named)

		if err != nil {
			t.Fatal(err)
		}

		built = append(built, mirror)
	}

	planted := newCgoMirrorBuilder(cgoMirrorAmd64)
	planted.fieldTypeOverride = func(structName, fieldName string) (string, bool) {
		return "int", structName == "_Ctype_struct_ints" && fieldName == "b"
	}

	if _, err := planted.mirror(lookupCgoStruct(t, shapesPkg, "_Ctype_struct_ints")); err != nil {
		t.Fatal(err)
	}

	// The same plant one level down (P2's second read): inner.b, a 4-byte C int, as a C# short, inside struct shapes,
	// checked only through shapes' own CheckLayout. A nested mirror never crosses on its own, so the outer check must
	// reach it.
	plantedNested := newCgoMirrorBuilder(cgoMirrorAmd64)
	plantedNested.fieldTypeOverride = func(structName, fieldName string) (string, bool) {
		return "short", structName == "_Ctype_struct_inner" && fieldName == "b"
	}

	if _, err := plantedNested.mirror(lookupCgoStruct(t, shapesPkg, "_Ctype_struct_shapes")); err != nil {
		t.Fatal(err)
	}

	// Program.cs prints one line per reading: SIZE <mirror> <bytes>, OFFSET <mirror>.<member> <bytes>, CHECKED <mirror>,
	// and for each planted mirror PLANTED or PLANTED_NESTED <REFUSED message|ACCEPTED>.
	var program strings.Builder
	program.WriteString("using U = global::System.Runtime.CompilerServices.Unsafe;\n\nnamespace mirrors\n{\n    public static class Program\n    {\n        public static void Main()\n        {\n")

	for _, mirror := range built {
		fmt.Fprintf(&program, "            global::System.Console.WriteLine(\"SIZE %s \" + U.SizeOf<%s>());\n", mirror.name, mirror.name)
		fmt.Fprintf(&program, "            { var v = default(%s);\n", mirror.name)

		for _, member := range mirror.fields {
			fmt.Fprintf(&program, "              global::System.Console.WriteLine(\"OFFSET %s.%s \" + U.ByteOffset(ref U.As<%s, byte>(ref v), ref U.As<%s, byte>(ref v.%s)));\n",
				mirror.name, member.name, mirror.name, member.clrType, member.name)
		}

		fmt.Fprintf(&program, "            }\n            %s.CheckLayoutᴍ();\n            global::System.Console.WriteLine(\"CHECKED %s\");\n", mirror.name, mirror.name)
	}

	program.WriteString("            try { planted._Ctype_struct_intsᴍ.CheckLayoutᴍ(); global::System.Console.WriteLine(\"PLANTED ACCEPTED\"); }\n")
	program.WriteString("            catch (global::System.InvalidOperationException refused) { global::System.Console.WriteLine(\"PLANTED REFUSED \" + refused.Message); }\n")
	program.WriteString("            try { plantedNested._Ctype_struct_shapesᴍ.CheckLayoutᴍ(); global::System.Console.WriteLine(\"PLANTED_NESTED ACCEPTED\"); }\n")
	program.WriteString("            catch (global::System.InvalidOperationException refused) { global::System.Console.WriteLine(\"PLANTED_NESTED REFUSED \" + refused.Message); }\n")
	program.WriteString("        }\n    }\n}\n")

	root := t.TempDir()
	writeModuleFile(t, filepath.Join(root, "mirrors.csproj"), fmt.Sprintf("<Project Sdk=\"Microsoft.NET.Sdk\">\n"+
		"  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>%s</TargetFramework>\n"+
		"    <ImplicitUsings>disable</ImplicitUsings>\n    <Nullable>disable</Nullable>\n  </PropertyGroup>\n</Project>\n", framework[1]))
	writeModuleFile(t, filepath.Join(root, "Mirrors.cs"), "namespace mirrors\n{\n"+builder.csharp()+"}\n")
	writeModuleFile(t, filepath.Join(root, "Planted.cs"), "namespace planted\n{\n"+planted.csharp()+"}\n")
	writeModuleFile(t, filepath.Join(root, "PlantedNested.cs"), "namespace plantedNested\n{\n"+plantedNested.csharp()+"}\n")
	writeModuleFile(t, filepath.Join(root, "Program.cs"), program.String())

	run := exec.Command(dotnet, "run", "--project", filepath.Join(root, "mirrors.csproj"), "--disable-build-servers", "-nodeReuse:false", "-p:UseSharedCompilation=false")
	run.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	output, err := run.CombinedOutput()

	if err != nil {
		t.Fatalf("the mirrors did not build and run: %v\n%s", err, lastLines(string(output), 30))
	}

	readings := map[string]string{}

	for _, line := range strings.Split(strings.ReplaceAll(string(output), "\r\n", "\n"), "\n") {
		if key, value, found := strings.Cut(strings.TrimSpace(line), " "); found && (key == "SIZE" || key == "OFFSET" || key == "PLANTED" || key == "PLANTED_NESTED") {
			if key == "PLANTED" || key == "PLANTED_NESTED" {
				readings[key] = value
			} else {
				name, reading, _ := strings.Cut(value, " ")
				readings[key+" "+name] = reading
			}
		} else if found && key == "CHECKED" {
			readings[key+" "+value] = "ok"
		}
	}

	for _, mirror := range built {
		if got := readings["SIZE "+mirror.name]; got != fmt.Sprint(mirror.size) {
			t.Errorf("the CLR reads %s as %s bytes, go/types %d", mirror.name, got, mirror.size)
		}

		for _, member := range mirror.fields {
			if got := readings["OFFSET "+mirror.name+"."+member.name]; got != fmt.Sprint(member.offset) {
				t.Errorf("the CLR puts %s.%s at %s, go/types at %d", mirror.name, member.name, got, member.offset)
			}
		}

		if readings["CHECKED "+mirror.name] != "ok" {
			t.Errorf("%s's own layout check did not pass", mirror.name)
		}
	}

	t.Logf("READING planted underflow: %s", readings["PLANTED"])

	if want := "REFUSED cgo mirror _Ctype_struct_ints: member b is 4 bytes as int and 8 bytes in C, so the mirror cannot cross"; readings["PLANTED"] != want {
		t.Errorf("the planted underflow (b, an 8-byte C long, as a C# int) reads %q, want %q", readings["PLANTED"], want)
	}

	t.Logf("READING planted nested underflow: %s", readings["PLANTED_NESTED"])

	if want := "REFUSED cgo mirror _Ctype_struct_inner: member b is 2 bytes as short and 4 bytes in C, so the mirror cannot cross"; readings["PLANTED_NESTED"] != want {
		t.Errorf("the planted nested underflow (inner.b, a 4-byte C int, as a C# short, checked through shapes) reads %q, want %q", readings["PLANTED_NESTED"], want)
	}
}
