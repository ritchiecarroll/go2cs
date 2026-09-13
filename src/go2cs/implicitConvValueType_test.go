// implicitConvValueType_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"go/token"
	"go/types"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// A GoImplicitConv record's ValueType is a CAST, not a type argument: ImplicitConvGenerator emits
//
//	public static implicit operator WaitStatus(ΔSignal src) => new WaitStatus((ValueType)src.Value);
//
// so ValueType must name the constructed type's BACKING PRIMITIVE — what that constructor takes.
// Recording the constructed NAMED type there instead round-trips through its own conversion
// operators, which compiles only while a standard EXPLICIT conversion exists between the two
// primitives and fails when it does not.
//
// This is the syscall shape exactly: `WaitStatus` is backed by `uint32` and `Signal` by `int`
// (`nint`), and `uint32`→`nint` is not a standard implicit conversion, so its reverse is not a
// standard explicit one, the user-defined operator is not applicable, and the generated cast is
// CS0030 on every unix flavor. Windows spells `WaitStatus` as a struct, so the pair is never
// registered and the corpus never saw it.
func TestNumericConversionRecordsBackingPrimitiveAsValueType(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\n"+
			// Deliberately DIFFERENT underlyings, the case the old form could not express.
			"type WaitLike uint32\n\n"+
			"type SignalLike int\n\n"+
			"func widen(s SignalLike) WaitLike { return WaitLike(s) }\n\n"+
			"func narrow(w WaitLike) SignalLike { return SignalLike(w) }\n\n"+
			"func main() { _ = widen(0); _ = narrow(0) }\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	packageInfo := readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", PackageInfoFileName))

	// The record whose LH is `WaitLike` must cast to uint32 — WaitLike's backing primitive — so the
	// value feeds `new WaitLike(uint32)`. This is the `WaitStatus`/`uint32` row.
	if !strings.Contains(packageInfo, `GoImplicitConv<WaitLike, SignalLike>(Inverted = true, ValueType = "uint32")`) {
		t.Errorf("the record constructing WaitLike must cast to its backing uint32:\n%s", packageInfo)
	}

	// …and the record whose LH is `SignalLike` must cast to nint, Go `int`'s C# spelling.
	if !strings.Contains(packageInfo, `GoImplicitConv<SignalLike, WaitLike>(Inverted = true, ValueType = "nint")`) {
		t.Errorf("the record constructing SignalLike must cast to its backing nint:\n%s", packageInfo)
	}

	// NEGATIVE control — the pre-fix emission named the constructed type itself. If either of these
	// reappears the generator is back to `new WaitLike((WaitLike)src.Value)`, which is CS0030 the
	// moment the two primitives have no standard explicit conversion between them.
	for _, regressed := range []string{`ValueType = "WaitLike"`, `ValueType = "SignalLike"`} {
		if strings.Contains(packageInfo, regressed) {
			t.Errorf("ValueType must name the backing primitive, not the constructed named type (%s):\n%s", regressed, packageInfo)
		}
	}
}

// Unit half of the same rule, over the type shapes the corpus's 49 records actually cover: every Go
// basic kind that can back a named numeric type must render as the C# name the generated cast will
// use. `int`/`uint` are the ones that matter most — they are native-sized in C# (`nint`/`nuint`) and
// are exactly where the round-trip form broke.
func TestNumericConversionValueTypeNameRendersBackingPrimitive(t *testing.T) {
	cases := []struct {
		kind types.BasicKind
		want string
	}{
		{types.Int, "nint"},
		{types.Uint, "nuint"},
		{types.Int8, "int8"},
		{types.Int16, "int16"},
		{types.Int32, "int32"},
		{types.Int64, "int64"},
		{types.Uint8, "uint8"},
		{types.Uint16, "uint16"},
		{types.Uint32, "uint32"},
		{types.Uint64, "uint64"},
		{types.Uintptr, "uintptr"},
		{types.Float32, "float32"},
		{types.Float64, "float64"},
	}

	pkg := types.NewPackage("example.com/app", "app")

	for _, testCase := range cases {
		basic := types.Typ[testCase.kind]
		name := types.NewTypeName(token.NoPos, pkg, "Named"+basic.Name(), nil)
		named := types.NewNamed(name, basic, nil)

		if got := numericConversionValueTypeName(named); got != testCase.want {
			t.Errorf("numericConversionValueTypeName(type Named%s %s) = %q, want %q", basic.Name(), basic.Name(), got, testCase.want)
		}

		// The named type's own rendering must NOT be the answer — that is the defect's shape.
		if got := numericConversionValueTypeName(named); got == name.Name() {
			t.Errorf("numericConversionValueTypeName must not return the named type %q", name.Name())
		}
	}
}
