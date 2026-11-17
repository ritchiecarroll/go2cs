// inlineArrayImporterSeed_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// W18 RULING C, ITEM (3) (COORD 2026-11-17 18:57Z, for .5): in a -recurse run the converter has the whole closure, so
// an import can be seeded from what its importers admit, with no flag. When an importer admits an anonymous struct
// identical to an alias a direct import declares, and the import does not admit it, the import is seeded with that
// alias exactly as -inline-arrays=<import>#<alias> would seed it. The two packages then agree on one lift and a value
// crosses between them by value, which is what ruling C's flag does for one named type (modernc.org/libc#largeInteger)
// and this does for every such twin in the closure.

// The emission: with no flag, lib is seeded with largeInteger (main admits its identical TLARGE_INTEGER), lib records
// the seed, and main reuses lib's lift.
func TestRecurseSeedsTheImportsTwinOfAnAdmittedAnonymousStruct(t *testing.T) {
	out := convertInlineModuleWith(t, "app", map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionCrossingMain}, "")

	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if want := "global using TLARGE_INTEGER = go.example.com.app.lib_package.largeIntegerᴛ1;"; !strings.Contains(main, want) {
		t.Errorf("main's admitted union does not reuse lib's lift; lib was not seeded from its importer; want %q:\n%s", want, main)
	}

	info := readGenerated(t, filepath.Join(out, "lib", "package_info.cs"))

	if !strings.Contains(info, `GoInlineArraySeed("largeInteger")`) {
		t.Errorf("lib's stamp does not record the importer's seed:\n%s", info)
	}
}

// The composition with ruling C's flag: the same type named by the flag and by the importer is seeded once.
func TestImporterSeedAndFlagSeedOfOneTypeRecordOnce(t *testing.T) {
	out := convertInlineModuleWith(t, "app", map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionCrossingMain}, largeIntegerSeed)

	info := readGenerated(t, filepath.Join(out, "lib", "package_info.cs"))

	if got := strings.Count(info, `GoInlineArraySeed("largeInteger")`); got != 1 {
		t.Errorf("the seed is recorded %d times, want once:\n%s", got, info)
	}

	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if !strings.Contains(main, "global using TLARGE_INTEGER = go.example.com.app.lib_package.largeIntegerᴛ1;") {
		t.Errorf("main does not reuse lib's lift:\n%s", main)
	}
}

// The composition's other side: "-<pattern>" turns lib's admission off, and off wins over the importer's demand. lib
// is not seeded and main keeps its own lift (ruling A's known mismatch, kept on purpose).
func TestAdmissionOffBeatsTheImporterSeed(t *testing.T) {
	out := convertInlineModuleWith(t, "app", map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionMain}, "-example.com/app/lib")

	info := readGenerated(t, filepath.Join(out, "lib", "package_info.cs"))

	if strings.Contains(info, "GoInlineArraySeed(") {
		t.Errorf("lib is seeded although -inline-arrays turns its admission off:\n%s", info)
	}

	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if strings.Contains(main, "lib_package.largeIntegerᴛ1") {
		t.Errorf("main reuses lib's unadmitted lift:\n%s", main)
	}
}

// End to end: the by-value crossing builds and reads as go with NO flag. GO2CS_INLINE_BUILD=1 runs it.
func TestRecurseImporterSeedCrossesByValueAndReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"lib/lib.go": aliasSpelledLib, "main.go": admittedUnionCrossingMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModuleWith(t, "app", files, "")

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}

// The seed rule's negative control (COORD 2026-11-17 19:18Z): an array-free struct converted whole through
// unsafe.Pointer is still NOT admitted when nothing seeds it (its layout is Go's already); seeded, it is admitted.
const arrayFreeSeedLib = `package lib

import "unsafe"

type Pair = struct {
	A int32
	B int32
}

func Read(p unsafe.Pointer) int32 { return (*Pair)(p).A }
`

func TestAnArrayFreeStructIsAdmittedOnlyWhenSeeded(t *testing.T) {
	plain, _ := admitFixture(t, "", Options{}, [2]string{"lib.go", arrayFreeSeedLib})

	if plain != nil && len(plain.keyOrder) > 0 {
		t.Fatalf("an UNSEEDED array-free struct is admitted (%q); only a seed may admit one", plain.keyOrder)
	}

	override, err := parseInlineArrayOverride("example.com/lib#Pair")

	if err != nil {
		t.Fatal(err)
	}

	seeded, _ := admitFixture(t, "", Options{inlineArrays: override}, [2]string{"lib.go", arrayFreeSeedLib})

	if seeded == nil || len(seeded.keyOrder) != 1 || !strings.HasPrefix(seeded.keyOrder[0], "anon:") {
		var keys []string

		if seeded != nil {
			keys = seeded.keyOrder
		}

		t.Fatalf("the SEEDED array-free struct is not admitted alone (keys %q)", keys)
	}
}

// The pre-pass reads a std stamp as the real conversion does (COORD 2026-11-17 19:34Z): only the convert-set's stamps
// are in memory; any other import's is read where the conversion reads it. lib reinterprets syscall.RawSockaddrInet4
// (array-holding, std's decision) as its own Sa; a std stamp that does not admit it keeps lib's Sa out (the pair rule),
// so main, which admits the identical struct, demands lib#Sa. A pre-pass blind to std stamps saw the pair as foreign,
// admitted Sa, and demanded nothing.
const stdPairLib = `package lib

import (
	"syscall"
	"unsafe"
)

type Sa = struct {
	Family uint16
	Port   uint16
	Addr   [4]byte
	Zero   [8]uint8
}

func Port(sa *syscall.RawSockaddrInet4) uint16 { return (*Sa)(unsafe.Pointer(sa)).Port }
`

const stdPairMain = `package main

import (
	"fmt"
	"unsafe"

	"example.com/app/lib"
)

type S = struct {
	Family uint16
	Port   uint16
	Addr   [4]byte
	Zero   [8]uint8
}

func main() {
	var raw [2]uint64
	s := (*S)(unsafe.Pointer(&raw))
	s.Port = 9
	fmt.Println(s.Port, lib.Port(nil) == 0)
}
`

func TestImporterSeedPrePassReadsAStdStamp(t *testing.T) {
	restore := lookupInlineArrayStamp
	t.Cleanup(func() { lookupInlineArrayStamp = restore })

	lookupInlineArrayStamp = func(importPath string, options Options) (inlineArrayStamp, bool) {
		if importPath == "syscall" {
			return inlineArrayStamp{mode: inlineArraysModeAuto, anonymous: []string{}}, true
		}

		return restore(importPath, options)
	}

	out := convertInlineModuleWith(t, "app", map[string]string{"lib/lib.go": stdPairLib, "main.go": stdPairMain}, "")
	info := readGenerated(t, filepath.Join(out, "lib", "package_info.cs"))

	if !strings.Contains(info, `GoInlineArraySeed("Sa")`) {
		t.Errorf("the pre-pass did not read syscall's stamp: lib's Sa is kept out by the pair rule in the conversion, "+
			"but the pre-pass admitted it and main demanded no seed:\n%s", info)
	}
}
