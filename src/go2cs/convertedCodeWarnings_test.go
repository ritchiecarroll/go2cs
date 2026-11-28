// convertedCodeWarnings_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the converter emissions behind two C# warnings the converted behavioral corpus raised in a
// go2cs.slnx build, each fixed at the emitter rather than suppressed:
//
//   - CS8778, a constant cast to nint/nuint that does not fit 32 bits. go2cs targets are 64-bit, so
//     the value is exact, but C# checks the conversion as if the host could be 32-bit. Each emitter
//     that writes such a cast wraps it in unchecked(...) when, and only when, the constant is beyond
//     the 32-bit range (uncheckedIfBeyond32Bit) -- the form the signed literal branch already used.
//     The in-range casts are the controls and must not move.
//   - CS0108, an interface member that hides an inherited one. An interface method that one of the
//     emitted C# bases also supplies (a Go embed, or a structural base the converter inherits from)
//     is re-declared with `new`; a method no base supplies is not.
//
// The CS0649 half (a blank tuple component's never-assigned field) is guarded by
// TestPackageTupleVarSpecInitOrderRelocation.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertConvertedCodeWarningsFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module wide\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type Sealed interface {
	Label() string
	Seal() string
}

type Rated interface {
	Label() string
	Rating() int
}
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"wide/lib"
)

type word uint

type Path string

type PrinterHost interface{ Ref(p Path) string }

type SpecifierHost interface {
	Ref(p Path) string
	Dir() string
}

// CS0108 shape 1: declared method also supplied by the embeds.
type OwnHost interface {
	PrinterHost
	SpecifierHost
	Ref(p Path) string
}

// CS0108 shape 2: re-declared over two structural bases.
type certificate interface {
	Label() string
	Seal() string
	Rating() int
	Serial() int
}

// CS0108 control: a declared method no base supplies.
type lone interface {
	PrinterHost
	Extra() int
}

type host struct{}

func (host) Ref(p Path) string { return string(p) }
func (host) Dir() string       { return "/" }
func (host) Label() string     { return "L" }
func (host) Seal() string      { return "S" }
func (host) Rating() int       { return 1 }
func (host) Serial() int       { return 2 }
func (host) Extra() int        { return 3 }

func maskAddr(i uintptr) uintptr { return i & 0x00ffffffffff }

func main() {
	var w word = 0x8000000000000001
	var p uintptr = 0x0102030405060708
	var u uint
	u = 1<<57 + 5
	far := []uint{1 << 32, 1<<32 + 1}
	nmin := -9223372036854775808
	nctl := -9223372036854775807
	var n int
	if n > 1<<31-1 {
		n = 0
	}
	small := []uint{1<<31 + 1}
	fmt.Println(w, p, u, far, nmin, nctl, n, small, maskAddr(5))
	var o OwnHost = host{}
	var c certificate = host{}
	var l lone = host{}
	var s lib.Sealed = c
	var r lib.Rated = c
	fmt.Println(o.Ref("a"), c.Label(), l.Extra(), s.Seal(), r.Rating())
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "wide", "main.cs"))
}

func TestConvertedCodeWarnings(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: loads the fixture module via go/packages")
	}

	mainCs := convertConvertedCodeWarningsFixture(t)

	// CS8778: one line per emitter, each beyond the 32-bit range.
	for _, want := range []string{
		// convBasicLit: an unsigned literal above MaxUint32 in a native-width context.
		"word w = unchecked((nuint)0x8000000000000001UL);",
		"uintptr p = unchecked((nuint)0x0102030405060708UL);",
		"(uintptr)unchecked((nuint)0x00ffffffffffUL)",
		// nativeIntConstCastType: a computed constant assigned to a native-width variable.
		"u = unchecked((nuint)(144115188075855872L + 5));",
		// widenedConstExprCastType: a computed constant as a []uint element.
		"unchecked((nuint)(4294967296L + 1))",
		// The exact int64 minimum in a Go-int context.
		"nint nmin = unchecked((nint)(-9223372036854775808L));",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("CS8778: a native-int constant cast beyond 32 bits is not unchecked, want %q:\n%s", want, mainCs)
		}
	}

	// Controls: casts within the 32-bit range, and the form the signed branch already wrote.
	for _, want := range []string{
		"nint nctl = -unchecked((nint)9223372036854775807L);",
		"if (n > (nint)(2147483648L - 1)) {",
		"var small = slice<nuint>([(nuint)(2147483648L + 1)]);",
		"[(nuint)1 << 32, ",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("CS8778 control moved, want %q:\n%s", want, mainCs)
		}
	}

	if strings.Contains(mainCs, "unchecked(unchecked(") {
		t.Errorf("CS8778: a cast is wrapped twice:\n%s", mainCs)
	}

	// CS0108: a declared method a base also supplies takes `new`; one no base supplies does not.
	for _, want := range []string{
		"partial interface OwnHost :\r\n    PrinterHost,\r\n    SpecifierHost\r\n{\r\n    new @string Ref(Path p);\r\n}",
		"partial interface certificate :\r\n    lib.Rated,\r\n    lib.Sealed\r\n{\r\n    new @string Label();\r\n    nint Serial();\r\n}",
		"partial interface lone :\r\n    PrinterHost\r\n{\r\n    nint Extra();\r\n}",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("CS0108: interface member modifiers, want %q:\n%s", want, mainCs)
		}
	}
}
