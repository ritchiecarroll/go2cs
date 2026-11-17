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
