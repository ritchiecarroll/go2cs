// typeParamUntypedConst_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Go gives an untyped constant the type of the integer type parameter it meets (COORD's seat after the
// map-index shift, 2026-11-05): `return 1 << n` in a func returning T emitted `1 << (int)(n)`, an int that
// C# will not convert to T (CS0029), and so did `return 1`, `var x T = 7`, `x = 7`, a T argument (`(T)2`,
// CS0030) and a []T element. Only a non-shift binary operator and a T(x) conversion already took golib's
// ConvertToType<T>. The behavioral program TypeParamUntypedConst runs each case; this test pins the forms.

func TestAnUntypedConstantTypedAsATypeParameterIsMaterialized(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/tpconst\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type integer interface{ ~uint8 | ~uint32 | ~int64 }

const five = 5

func one[T integer]() T                { return 1 }
func named[T integer]() T              { return five }
func constShift[T integer]() T         { return 1 << 3 }
func bit[T integer](n uint) T          { return 1 << n }
func bitSigned[T integer](n int) T     { return 1 << n }
func mask[T integer](n uint) T         { return 1<<n - 1 }
func varBit[T integer](n uint) T       { var x T = 1 << n; return x }
func orBit[T integer](x T, n uint) T   { x |= 1 << n; return x }
func assign[T integer]() T             { var x T; x = 7; return x }
func pair[T integer](a, b T) T         { return a + b }
func passConst[T integer](x T) T       { return pair(x, 2) }
func elems[T integer]() []T            { return []T{1, 2} }
func isZero[T integer](x T) bool       { return x == 0 }
func convert[T integer]() T            { return T(4) }

func main() {
	fmt.Println(one[uint8](), named[uint8](), constShift[uint8](), bit[uint8](9), bitSigned[uint8](3), mask[uint8](3))
	fmt.Println(varBit[uint8](3), orBit[uint8](1, 3), assign[uint8](), passConst[uint8](1), elems[uint8](), isZero[uint8](0), convert[uint8]())
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "tpconst", "main.cs"))

	for _, want := range []string{
		"return ConvertToType<T>(1);",
		"return ConvertToType<T>(five);",
		"return ConvertToType<T>(1 << 3);",
		// The shift takes the type parameter's guard, with the materialized constant as its receiver.
		"return ConvertToType<T>(1).Lsh(n);",
		"return ConvertToType<T>(1).Lsh((int64)(n));",
		"return ConvertToType<T>(1).Lsh(n) - ConvertToType<T>(1);",
		"T x = ConvertToType<T>(1).Lsh(n);",
		"x |= ConvertToType<T>(1).Lsh(n);",
		"x = ConvertToType<T>(7);",
		"return pair(x, ConvertToType<T>(2));",
		"slice<T>([ConvertToType<T>(1), ConvertToType<T>(2)])",
		// Controls: the two places that already materialized the constant do so once.
		"return AreEqual(x, ConvertToType<T>(0));",
		"return ConvertToType<T>(4);",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("expected %q in the emission:\n%s", want, mainCs)
		}
	}

	if strings.Contains(mainCs, "ConvertToType<T>(ConvertToType<T>(") {
		t.Errorf("a constant was materialized twice:\n%s", mainCs)
	}
}
