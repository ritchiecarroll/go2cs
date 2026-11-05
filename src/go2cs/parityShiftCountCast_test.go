// parityShiftCountCast_test.go - Gbtc
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
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// Visual-parity seat A of the next converter train (owner, 2026-10-16): a shift count that is a go/types CONSTANT
// in [0, width) of the shifted operand, rendered as a plain integer literal, is written as Go writes it, `x <<= 8`,
// never `x <<= (int)(8)`. Both are the same IL (measured 2026-10-16, 48 of 48 pairs). The binary form and the
// compound form of an unnamed basic target with a decimal literal already read that way (seats SH and #27); this
// pins the compound shapes they left cast: a NAMED-numeric target (its wrapper's `operator <<(T, int)` binds a bare
// int literal exactly, as the binary form `n << 4` already relies on), a TYPE-PARAMETER target with a count below 8
// (in range for every integer type its set can hold), and a hex or binary literal count.
//
// The cast STAYS for every count the rule does not cover: an identifier or expression count, a named-type count,
// and a type-parameter count of 8 or more. (A basic or named target never reaches the native form with a constant
// count at or past its width: the guard, or the wrapper's wide operator, takes it.)

const shiftCountCastFixture = `package main

type word uint

type count uint8

type integer interface{ ~uint | ~uint32 | ~uint64 | ~int }

const shift = 8

type myInt int

const namedShift myInt = 2

func named(w word, c count, n uint) (word, count) {
	w >>= 8
	w <<= 0x3
	c <<= 1
	c >>= 0b10
	w <<= n
	w >>= shift
	w <<= namedShift
	return w, c
}

func basic(x uint64) uint64 {
	x >>= 0x8
	x <<= 0b11
	x >>= (shift / 4)
	return x
}

func generic[T integer](x T) T {
	x <<= 3
	x >>= 0x7
	x <<= 9
	return x
}
`

func convertShiftCountCastFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/shiftcountcast\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), shiftCountCastFixture)

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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "shiftcountcast", "main.cs"))
}

func TestShiftCountConstantLiteralUncast(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs := convertShiftCountCastFixture(t)

	wants := []string{
		// Dropped: a constant literal count in [0, width).
		`w >>= 8;`, `w <<= 0x3;`, `c <<= 1;`, `c >>= 0b10;`,
		`x >>= 0x8;`, `x <<= 0b11;`,
		`x <<= 3;`, `x >>= 0x7;`,
		// Kept: an identifier, expression or named-type count, and a type-parameter count of 8 or more.
		`w >>= (int)(shift);`, `w <<= (int)(namedShift);`, `x >>= (int)((shift / 4));`, `x <<= (int)(9);`,
	}

	for _, want := range wants {
		if !strings.Contains(mainCs, want) {
			t.Errorf("no emitted line carries %q", want)
		}
	}

	// A non-constant count on a named target keeps its widened wide-operator form, never a bare or int-cast count.
	if !regexp.MustCompile(`w <<= \(uint64\)\(?n\)?;|w <<= n;`).MatchString(mainCs) {
		t.Errorf("the named target's non-constant count lost its widened form")
	}

	if t.Failed() {
		t.Logf("emitted main.cs:\n%s", mainCs)
	}
}
