// inlineArraySeed_test.go - Gbtc
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

// RULING C (COORD 2026-11-17 12:10Z): (1) a per-type seed, -inline-arrays=<import path>#<TypeName>, admits one type of
// a package as if that package converted it whole through unsafe.Pointer; (2) the stamp records every admitted
// ANONYMOUS key, so an importer knows which of a package's anonymous lifts are admitted. Together they retire ruling
// A's KNOWN LIMIT: the importer and the import agree on one lift again, so a value crosses between them by value.

// lib admits TWO anonymous structs (it converts both whole through unsafe.Pointer), so its stamp counts two anonymous
// keys and, before (2), an importer cannot tell which of them is admitted (source=unknown-several-anonymous).
const twoAnonymousLib = `package lib

import "unsafe"

type A = struct {
	V [2]int32
	N int32
}

type B = struct {
	W [3]int16
}

func Load(p unsafe.Pointer) (int32, int16) {
	a := (*A)(unsafe.Pointer(p))
	b := (*B)(unsafe.Pointer(p))
	return a.V[1], b.W[2]
}

func Sum(a A) int32 { return a.V[0] + a.V[1] + a.N }

const Name = "lib"
`

// main admits its identical TA and passes it to lib BY VALUE. With lib's admission unknown it keeps its own lift, and
// the crossing is CS1503; with (2) it knows lib admits A and reuses lib's lift.
const twoAnonymousMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type TA = struct {
	V [2]int32
	N int32
}

func main() {
	var raw [3]int32
	a := (*TA)(unsafe.Pointer(&raw))
	a.V[0] = 1
	a.V[1] = 2
	a.N = 3
	v, w := lib.Load(unsafe.Pointer(&raw))
	fmt.Println(raw[2], lib.Sum(*a), v, w, unsafe.Sizeof(*a), lib.Name)
}
`

// (2), emission: main reuses lib's admitted lift of the identical struct.
func TestImporterReusesAnAdmittedAnonymousLiftItCanNowIdentify(t *testing.T) {
	isolateConverterPathGlobals(t)

	emitted := convertModuleMainWithLib(t, "lib", twoAnonymousLib, twoAnonymousMain)

	if want := "global using TA = go.example.com.app.lib_package.Aᴛ1;"; !strings.Contains(emitted, want) {
		t.Errorf("main does not reuse lib's admitted lift of the identical struct (lib admits two anonymous structs); want %q:\n%s", want, emitted)
	}
}

// (2), end to end: the by-value crossing builds and reads as go. GO2CS_INLINE_BUILD=1 runs it.
func TestAdmittedAnonymousLiftCrossesByValueAndReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": twoAnonymousLib, "main.go": twoAnonymousMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}

// The seed the windows sqlite closure needs: libc's largeInteger (aliasSpelledLib's), which libc never converts whole.
const largeIntegerSeed = "example.com/app/lib#largeInteger"

// (1)+(2), emission: with lib seeded, lib admits largeInteger and records it, and main's admitted TLARGE_INTEGER reuses
// lib's lift instead of keeping its own (ruling A's known limit).
func TestSeededImportAdmitsTheTypeAndTheImporterReusesIt(t *testing.T) {
	out := convertInlineModuleWith(t, "app", map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionCrossingMain}, largeIntegerSeed)

	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if want := "global using TLARGE_INTEGER = go.example.com.app.lib_package.largeIntegerᴛ1;"; !strings.Contains(main, want) {
		t.Errorf("main's admitted union does not reuse the seeded lib's lift; want %q:\n%s", want, main)
	}

	info := readGenerated(t, filepath.Join(out, "lib", "package_info.cs"))

	if !strings.Contains(info, `GoInlineArraySeed("largeInteger")`) {
		t.Errorf("lib's stamp does not record the seed, so a -tests run of lib could not repeat it:\n%s", info)
	}

	if !strings.Contains(info, "GoInlineArrayAnonymous(") {
		t.Errorf("lib's stamp records no admitted anonymous key:\n%s", info)
	}
}

// -inline-arrays parses the per-type form without forcing the package, and refuses it with "-" (a seed admits one
// type; it cannot turn admission off) and with an empty type name.
func TestInlineArraySeedFlagParses(t *testing.T) {
	override, err := parseInlineArrayOverride("example.com/a#T, example.com/b/...")

	if err != nil {
		t.Fatalf("a per-type seed is refused: %v", err)
	}

	if override.matches(override.force, "example.com/a") {
		t.Errorf("a seed forced the whole package on")
	}

	if !override.matches(override.force, "example.com/b/c") {
		t.Errorf("the package pattern beside the seed no longer forces its packages")
	}

	for _, bad := range []string{"-example.com/a#T", "example.com/a#", "#T"} {
		if _, err := parseInlineArrayOverride(bad); err == nil {
			t.Errorf("-inline-arrays %q is accepted", bad)
		}
	}
}

// convertInlineModuleWith is convertInlineModule with an -inline-arrays value.
func convertInlineModuleWith(t *testing.T, name string, files map[string]string, inlineArrays string) string {
	t.Helper()
	isolateConverterPathGlobals(t)

	override, err := parseInlineArrayOverride(inlineArrays)

	if err != nil {
		t.Fatalf("-inline-arrays %q: %v", inlineArrays, err)
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/"+name+"\n\ngo 1.23\n")

	for file, source := range files {
		writeModuleFile(t, filepath.Join(appDir, file), source)
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{goRoot: goRoot, goPath: build.Default.GOPATH, go2csPath: filepath.Join(root, "out"), recurse: true, targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces: 4, preferVarDecl: true, useChannelOperators: true, hoistLiterals: true, inlineArrays: override}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return filepath.Join(options.go2csPath, "src", "example.com", name)
}
