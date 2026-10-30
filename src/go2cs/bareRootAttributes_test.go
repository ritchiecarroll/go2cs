// bareRootAttributes_test.go - Gbtc
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

	"github.com/ritchiecarroll/hashset"
)

// golib's three [assembly:] records in a package info file -- GoPositionMap, GoHoistedLiterals and
// GoCgoImportDynamic -- are spelled bare when the file imports the root namespace with `using go;`: an
// assembly attribute binds at compilation-unit scope, where that directive applies, so `go.` there only
// repeats it. A file without the directive keeps the rooted spelling, `global::go.` when a `go.go`
// namespace shadows the root.

// TestRootAttributesAreBareInAConvertedPackageInfo: a converted package's package_info.cs carries
// `using go;`, so its stamp and its position-map records name the attributes bare. RED before the bare
// spelling: both read `go.`.
func TestRootAttributesAreBareInAConvertedPackageInfo(t *testing.T) {
	isolateConverterPathGlobals(t)

	info := convertStampFixture(t, t.TempDir(), true)

	if !strings.Contains(info, "\nusing go;\n") {
		t.Fatalf("fixture: package_info.cs carries no `using go;`, so it cannot test the bare spelling:\n%s", info)
	}

	for _, want := range []string{"[assembly: GoHoistedLiterals]", "[assembly: GoPositionMap("} {
		if !strings.Contains(info, want) {
			t.Errorf("want %q in package_info.cs:\n%s", want, info)
		}
	}

	if strings.Contains(info, "go.GoHoistedLiterals") || strings.Contains(info, "go.GoPositionMap") {
		t.Errorf("package_info.cs still roots an attribute that `using go;` already imports:\n%s", info)
	}
}

// bareAttributeFixture is a package info file's shell: the directive under test (or none), the three
// sections the writers fill, and the file-scoped namespace they must stay above.
func bareAttributeFixture(usingGo bool, positionMapRecords ...string) []string {
	lines := []string{"// <ImportedTypeAliases>", "// </ImportedTypeAliases>", ""}

	if usingGo {
		lines = append(lines, "using go;", "")
	}

	lines = append(lines, goSourcePositionMapsProseLines()...)
	lines = append(lines, "// <GoSourcePositionMaps>")
	lines = append(lines, positionMapRecords...)

	return append(lines, "// </GoSourcePositionMaps>", "", "namespace go;", "", "public static partial class p_package {", "}")
}

// writeBareAttributeFixture runs the three section writers over the fixture as package_info.cs, with one
// fresh position-map record, one cgo record and the hoisted stamp to write.
func writeBareAttributeFixture(t *testing.T, lines []string, mergeExisting bool) string {
	t.Helper()

	savedRecords, savedImports, savedHoists, savedAccess := positionMapRecords, cgoDynamicImports, packageHoistsLiterals, fieldAccessRecords
	t.Cleanup(func() {
		positionMapRecords, cgoDynamicImports, packageHoistsLiterals, fieldAccessRecords = savedRecords, savedImports, savedHoists, savedAccess
	})

	positionMapRecords = map[string][]string{
		"package_info.cs": {"[assembly: " + globalQualifyRooted("go.GoPositionMap") + `("p/a.go", "a.cs", "AA==")]`},
	}
	cgoDynamicImports = map[string]cgoDynamicImportRecord{
		"libc_getgroups_trampoline": {trampoline: "libc_getgroups_trampoline", symbol: "getgroups", library: "/usr/lib/libSystem.B.dylib"},
	}
	packageHoistsLiterals = true
	fieldAccessRecords = hashset.NewHashSet([]string{"p_package.T"})

	lines = applyGoSourcePositionMaps(lines, "package_info.cs", mergeExisting)
	lines = applyHoistedLiteralsStamp(lines, "package_info.cs")
	lines = applyFieldAccessRecords(lines, "package_info.cs", mergeExisting, func(name string) string { return name })
	lines = applyCgoDynamicImports(lines, "package_info.cs", mergeExisting)

	return strings.Join(lines, "\n")
}

// TestRootAttributesFollowTheFilesUsingGo: with `using go;` all three records are bare; without it they
// keep `go.`, and `global::go.` when a `go.go` namespace shadows the root -- in which case the bare name
// still binds through `using go;`, because the directive itself resolves at the global namespace.
func TestRootAttributesFollowTheFilesUsingGo(t *testing.T) {
	savedNamespace := packageNamespace
	t.Cleanup(func() { packageNamespace = savedNamespace })

	for _, test := range []struct {
		namespace string
		usingGo   bool
		prefix    string
	}{
		{"go", true, ""},
		{"go", false, "go."},
		{"go.go.format_package", true, ""},
		{"go.go.format_package", false, "global::go."},
	} {
		packageNamespace = test.namespace
		info := writeBareAttributeFixture(t, bareAttributeFixture(test.usingGo), false)

		for _, want := range []string{
			"[assembly: " + test.prefix + `GoPositionMap("p/a.go", "a.cs", "AA==")]`,
			"[assembly: " + test.prefix + "GoHoistedLiterals]",
			"[assembly: " + test.prefix + "GoFieldAccess(typeof(p_package.T))]",
			"[assembly: " + test.prefix + `GoCgoImportDynamic("libc_getgroups_trampoline", "getgroups", "/usr/lib/libSystem.B.dylib")]`,
		} {
			if strings.Count(info, want) != 1 {
				t.Errorf("namespace %s, using go %v: want %q exactly once in:\n%s", test.namespace, test.usingGo, want, info)
			}
		}
	}
}

// TestRootAttributesKeptByAMergeAreRespelled: a merging write keeps the records of files it did not
// re-record, and those were written in the old spelling. They are respelled with the fresh ones, so the
// section never mixes spellings and a re-recorded file never survives as a twin under its old one.
func TestRootAttributesKeptByAMergeAreRespelled(t *testing.T) {
	lines := bareAttributeFixture(true,
		`[assembly: go.GoPositionMap("p/a.go", "a.cs", "stale")]`,
		`[assembly: go.GoPositionMap("p/b.go", "b.cs", "BB==")]`,
	)

	lines = append(lines[:len(lines)-4], append([]string{
		"[assembly: go.GoFieldAccess(typeof(p_package.T))]",
		"",
		"// <" + CgoDynamicImportsSection + ">",
		`[assembly: go.GoCgoImportDynamic("libc_getgroups_trampoline", "getgroups", "/usr/lib/libSystem.B.dylib")]`,
		"// </" + CgoDynamicImportsSection + ">",
		"",
	}, lines[len(lines)-4:]...)...)

	info := writeBareAttributeFixture(t, lines, true)

	for _, want := range []string{
		`[assembly: GoPositionMap("p/a.go", "a.cs", "AA==")]`,
		`[assembly: GoPositionMap("p/b.go", "b.cs", "BB==")]`,
		"[assembly: GoFieldAccess(typeof(p_package.T))]",
		`[assembly: GoCgoImportDynamic("libc_getgroups_trampoline", "getgroups", "/usr/lib/libSystem.B.dylib")]`,
	} {
		if strings.Count(info, want) != 1 {
			t.Errorf("want %q exactly once in:\n%s", want, info)
		}
	}

	if strings.Contains(info, "go.Go") || strings.Contains(info, `"stale"`) {
		t.Errorf("a merge kept an old spelling or a re-recorded file's stale record:\n%s", info)
	}
}

// TestRootAttributesRepeatRunIsByteIdentical: a second conversion into the root the first one wrote reads
// its own bare records back and writes the package info file byte for byte as it was, for a package whose
// root namespace is shadowed (it imports go/format, so `go.go` exists in its compilation) and for one whose
// is not. RED if a reader knows only the rooted spellings: it would keep the bare stamp beside a fresh one.
func TestRootAttributesRepeatRunIsByteIdentical(t *testing.T) {
	for name, source := range map[string]string{
		"unshadowed": "package stamp\n\nimport \"fmt\"\n\nfunc Show() { fmt.Println(\"hello\") }\n",
		"shadowed":   "package stamp\n\nimport (\n\t\"fmt\"\n\t\"go/format\"\n)\n\nfunc Show() { b, _ := format.Source([]byte(\"package p\")); fmt.Println(string(b)) }\n",
	} {
		t.Run(name, func(t *testing.T) {
			isolateConverterPathGlobals(t)
			root := t.TempDir()

			first := convertRootAttributeFixture(t, root, source)

			if !strings.Contains(first, "[assembly: GoHoistedLiterals]") || !strings.Contains(first, "[assembly: GoPositionMap(") {
				t.Fatalf("first conversion: want the bare stamp and records:\n%s", first)
			}

			if shadowed := strings.Contains(first, "global::go."); shadowed != (name == "shadowed") {
				t.Fatalf("fixture: root shadowed = %v, want %v:\n%s", shadowed, name == "shadowed", first)
			}

			if second := convertRootAttributeFixture(t, root, source); second != first {
				t.Errorf("a second conversion into the same root changed package_info.cs:\n--- first ---\n%s\n--- second ---\n%s", first, second)
			}
		})
	}
}

// convertRootAttributeFixture converts a one-file hoisting module with the given source into root's out
// tree and returns its package_info.cs, LF-normalized.
func convertRootAttributeFixture(t *testing.T, root string, source string) string {
	t.Helper()

	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/stamp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "stamp.go"), source)

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

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "stamp", "package_info.cs")), "\r\n", "\n")
}
