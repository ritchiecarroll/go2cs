// compoundShiftGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the COMPOUND shift-assign's Go-semantics guard (visitAssignStmt's SHL_ASSIGN case,
// compoundShiftGuarded): `x >>= n` with a count not provably below the width renders golib's
// `x.RshAssign(n)`, because C#'s native `x >>= (int)n` masks the count -- runtime's softfloat fadd64
// shifted by 24 where Go shifts by 664 and yields 0. The controls carry as much weight: a bounded count, a
// named-type target and a map-index target must keep the native form.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"testing"
)

func TestCompoundShiftAssignTakesTheGoSemanticsGuard(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/shiftassign\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type holder struct{ bits uint64 }

type named uint64

// POSITIVES - a count that is not provably below the width.
func onLocal(x uint64, n uint) uint64     { x >>= n; return x }
func onField(h *holder, n uint) uint64    { h.bits <<= n; return h.bits }
func onElement(a []uint32, n uint) uint32 { a[1] >>= n; return a[1] }
func onSigned(x int64, n uint) int64      { x >>= n; return x }

// CONTROLS - a provably bounded count, a named target, a map-index target.
func boundedConst(x uint64) uint64          { x >>= 3; return x }
func boundedMask(x uint64, n uint) uint64   { x >>= n & 63; return x }
func onNamed(x named, n uint) named         { x >>= n; return x }
func onMapIndex(m map[int]uint64, n uint) uint64 { m[1] >>= n; return m[1] }

func main() {
	fmt.Println(onLocal(1<<40, 70), onField(&holder{1}, 70), onElement([]uint32{0, 8}, 40), onSigned(-8, 70))
	fmt.Println(boundedConst(64), boundedMask(64, 3), onNamed(64, 3), onMapIndex(map[int]uint64{1: 64}, 3))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "shiftassign", "main.cs"))

	bodyOf := func(name string) string {
		match := regexp.MustCompile(`(?m)^( *)[^\n]*\b` + name + `\([^\n]*\n(?s:.*?)^( *)\}\s*$`).FindString(mainCs)

		if match == "" {
			match = regexp.MustCompile(`(?m)^[^\n]*\b` + name + `\([^\n]*$`).FindString(mainCs)
		}

		if match == "" {
			t.Fatalf("%s: not found in the emission:\n%s", name, mainCs)
		}

		return match
	}

	guarded := regexp.MustCompile(`\.(Rsh|Lsh)Assign\(`)

	for _, positive := range []string{"onLocal", "onField", "onElement", "onSigned"} {
		if !guarded.MatchString(bodyOf(positive)) {
			t.Errorf("%s: expected the compound shift to take golib's guard (`.RshAssign(`/`.LshAssign(`); emission:\n%s", positive, bodyOf(positive))
		}
	}

	for _, control := range []string{"boundedConst", "boundedMask", "onNamed", "onMapIndex"} {
		if guarded.MatchString(bodyOf(control)) {
			t.Errorf("%s: must keep the native compound shift; emission:\n%s", control, bodyOf(control))
		}
	}
}
