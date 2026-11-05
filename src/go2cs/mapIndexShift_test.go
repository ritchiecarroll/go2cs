// mapIndexShift_test.go - Gbtc
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

// A compound shift on a MAP ELEMENT, and a shift on an integer TYPE PARAMETER, follow Go's shift rule like
// every other target (BOARD 2026-11-05, P1's finding). Both stayed native: C# masks a map element's count to
// its promoted width (`m[1] >>= 40` on a uint32 element read as `>> 8`), and .NET's IShiftOperators masks a
// type argument's count to its width (`x <<= 9` on a uint8 type argument read as `<< 1`). A negative signed
// count must panic, so it is widened to int64 for golib's signed overloads. The behavioral program
// MapIndexShiftCount runs each case; this test pins the emitted forms, which keep the target evaluated once.

func TestMapElementAndTypeParameterShiftsTakeTheGoSemanticsGuard(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/mapindexshift\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

func key() int { return 1 }

func mapConst(m map[int]uint32) uint32            { m[1] >>= 40; return m[1] }
func mapUnbounded(m map[int]uint8, n uint) uint8  { m[1] <<= n; return m[1] }
func mapSigned(m map[int]uint32, n int) uint32    { m[1] >>= n; return m[1] }
func mapKeyCall(m map[int]uint32, n uint) uint32  { m[key()] >>= n; return m[1] }
func mapBounded(m map[int]uint32) uint32          { m[1] >>= 8; return m[1] }

func tpCompound[T ~uint8 | ~uint32](x T, n uint) T                { x <<= n; return x }
func tpBinary[T ~uint8 | ~uint32](x T, n uint) T                  { return x << n }
func tpConst[T ~uint8 | ~uint32](x T) T                           { return x >> 9 }
func tpBounded[T ~uint8 | ~uint32](x T) T                         { return x >> 3 }
func tpSlot[T ~uint8 | ~uint32](s []T, i func() int, n uint) T    { s[i()] <<= n; return s[0] }
func tpMapKeyCall[T ~uint8 | ~uint32](m map[int]T, n uint) T      { m[key()] >>= n; return m[1] }

func main() {
	fmt.Println(mapConst(map[int]uint32{1: 1}), mapUnbounded(map[int]uint8{1: 1}, 9), mapSigned(map[int]uint32{1: 1}, 1))
	fmt.Println(mapKeyCall(map[int]uint32{1: 1}, 40), mapBounded(map[int]uint32{1: 256}))
	fmt.Println(tpCompound(uint8(1), 9), tpBinary(uint8(1), 9), tpConst(uint8(1)), tpBounded(uint8(64)))
	fmt.Println(tpSlot([]uint8{1}, func() int { return 0 }, 9), tpMapKeyCall(map[int]uint8{1: 1}, 9))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "mapindexshift", "main.cs"))

	for _, want := range []string{
		// A map element whose map and key can be read twice: `m[k] = m[k].Rsh(n)`.
		"m[1] = m[1].Rsh((uint64)40);",
		"m[1] = m[1].Lsh(n);",
		"m[1] = m[1].Rsh((int64)(n));",
		// A key that cannot be read twice: the map and its key, evaluated once.
		"m.RshAssign(key(), n);",
		// A type parameter's target: read twice, by ref, or as a map element.
		"x = x.Lsh(n);",
		"return x.Lsh(n);",
		"return x.Rsh((uint64)9);",
		"GoShift.LshAssign(ref s[i()], n);",
		// Controls: a count provably below the narrowest width stays native.
		"m[1] >>= 8;",
		"return x >> 3;",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("expected %q in the emission:\n%s", want, mainCs)
		}
	}

	if strings.Count(mainCs, "m.RshAssign(key(), n);") != 2 {
		t.Errorf("both key-call map targets (basic and type parameter) must take `m.RshAssign(key(), n)`:\n%s", mainCs)
	}
}
