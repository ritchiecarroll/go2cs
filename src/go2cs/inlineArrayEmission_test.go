// inlineArrayEmission_test.go - Gbtc
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

// convertInlineArrayFixture converts a one-file package and returns its emitted C#, line endings normalized.
func convertInlineArrayFixture(t *testing.T, pkg string, source string) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/"+pkg+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, pkg+".go"), source)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		hoistLiterals:       true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", pkg, pkg+".cs")), "\r\n", "\n")
}

// C1's review, M3: a KEYED array literal that initializes an inline field. Go's keys are constants, so each element
// lands at its position (a positional element after a key takes the next one), the gaps are the element's zero value,
// and the constructor zero-fills past the last element. libc carries 37 such literals in types it admits
// (Foffsets: [8]uint8{1: uint8(4)} and kin).
func TestInlineArrayKeyedLiteralTakesItsPositions(t *testing.T) {
	cs := convertInlineArrayFixture(t, "keyed", `package keyed

import "unsafe"

type T struct {
	Off [8]uint8
	N   [3]int32
	E   [2]int16
}

func view(p unsafe.Pointer) *T { return (*T)(p) }

var V = T{Off: [8]uint8{1: uint8(4), 5, 6}, N: [3]int32{2: 9}, E: [2]int16{}}
`)

	for _, want := range []string{
		`Off: new array8<uint8>(default, (uint8)4, 5, 6)`,
		`N: new array3<int32>(default, default, 9)`,
		`E: new array2<int16>()`,
	} {
		if !strings.Contains(cs, want) {
			t.Errorf("want %q:\n%s", want, cs)
		}
	}
}
