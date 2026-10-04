// siblingPackageNamespace_test.go - Gbtc
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

// Two import paths under one parent that declare one package name (go-cmp's
// cmp/internal/teststructs/{foo1,foo2}, both `package foo`) are two packages in Go, whose identity is
// the import path. The namespace dropped the path's last segment and the class was named for the
// package, so both became `<parent>.foo_package` and a compilation referencing both read CS0433. When a
// package's name differs from its last path segment, the namespace keeps the whole import path: on the
// declaration side and in every importer's alias alike, since both derive it from the path and the name.
//
// Three differences keep today's spelling, because they cannot collide: a /vN tail (`math/rand/v2`), a
// name.vN tail (`gopkg.in/yaml.v3`), and a difference in case only (`crypto/x509/internal/macos` is
// `package macOS`), which Go's module rules already forbid within one module.

func convertSiblingPackageFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/sib\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "teststructs", "foo1", "lib.go"),
		"package foo\n\nfunc Name() string { return \"foo1\" }\n\ntype Pair struct{ Left, Right int }\n\ntype Alias = Pair\n")

	writeModuleFile(t, filepath.Join(appDir, "teststructs", "foo2", "lib.go"),
		"package foo\n\nfunc Name() string { return \"foo2\" }\n\ntype Triple struct{ A, B, C string }\n\ntype Alias = Triple\n")

	// A package declared in the SIBLINGS' parent namespace (teststructs/bar, `package bar`, so namespace
	// <parent>) importing a sibling under an alias that spells the sibling's directory: the sibling's
	// namespace is now a child of that namespace, so the alias collides with it (CS0576) and must be
	// renamed exactly as an alias colliding with any other child namespace is.
	writeModuleFile(t, filepath.Join(appDir, "teststructs", "bar", "lib.go"),
		"package bar\n\nimport foo1 \"example.com/sib/teststructs/foo1\"\n\nfunc Use() string { return foo1.Name() }\n")

	// The exempt shapes, as controls: none of them may move.
	writeModuleFile(t, filepath.Join(appDir, "codec", "v2", "lib.go"), "package codec\n\nfunc Version() int { return 2 }\n")
	writeModuleFile(t, filepath.Join(appDir, "yaml.v3", "lib.go"), "package yaml\n\nfunc Version() int { return 3 }\n")
	writeModuleFile(t, filepath.Join(appDir, "macos", "lib.go"), "package macOS\n\nfunc Version() int { return 14 }\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/sib/codec/v2"
	"example.com/sib/macos"
	"example.com/sib/teststructs/bar"
	foo1 "example.com/sib/teststructs/foo1"
	foo2 "example.com/sib/teststructs/foo2"
	"example.com/sib/yaml.v3"
)

func main() {
	var a foo1.Alias = foo1.Pair{Left: 1, Right: 2}
	var b foo2.Alias = foo2.Triple{A: "x"}
	fmt.Println(foo1.Name(), foo2.Name(), a, b, codec.Version(), yaml.Version(), macOS.Version(), bar.Use())
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

	return filepath.Join(options.go2csPath, "src", "example.com", "sib")
}

func TestSiblingPackagesSharingANameAreDistinctTypes(t *testing.T) {
	out := convertSiblingPackageFixture(t)

	for _, check := range []struct{ file, want string }{
		{"teststructs/foo1/lib.cs", "namespace go.example.com.sib.teststructs.foo1;"},
		{"teststructs/foo2/lib.cs", "namespace go.example.com.sib.teststructs.foo2;"},
		{"teststructs/foo1/package_info.cs", `GoTypeAlias("Alias", "go.example.com.sib.teststructs.foo1.foo_package.Pair")`},
		{"teststructs/foo2/package_info.cs", `GoTypeAlias("Alias", "go.example.com.sib.teststructs.foo2.foo_package.Triple")`},
		{"main.cs", "teststructs.foo1.foo_package;"},
		{"main.cs", "teststructs.foo2.foo_package;"},
		{"teststructs/bar/lib.cs", "using Δfoo1 = "},
	} {
		if text := readGenerated(t, filepath.Join(out, filepath.FromSlash(check.file))); !strings.Contains(text, check.want) {
			t.Errorf("%s: missing %q in:\n%s", check.file, check.want, text)
		}
	}
}

func TestExemptPackageNameDifferencesKeepTheirNamespace(t *testing.T) {
	out := convertSiblingPackageFixture(t)

	for _, check := range []struct{ file, want string }{
		{"codec/v2/lib.cs", "namespace go.example.com.sib.codec;"}, // a /vN tail
		{"yaml.v3/lib.cs", "namespace go.example.com.sib;"},        // a name.vN tail
		{"macos/lib.cs", "namespace go.example.com.sib;"},          // case only
		{"main.cs", "namespace go.example.com;"},                   // package main is never referenced
		{"main.cs", "sib.codec.codec_package;"},
		{"main.cs", "sib.yaml_package;"},
		{"main.cs", "sib.macOS_package;"},
	} {
		if text := readGenerated(t, filepath.Join(out, filepath.FromSlash(check.file))); !strings.Contains(text, check.want) {
			t.Errorf("%s: missing %q in:\n%s", check.file, check.want, text)
		}
	}
}

// A persisted package_info.cs is copied through verbatim outside its marker sections, so a package whose
// namespace moved would keep the old declaration beside sources that declare the new one. The template's
// own `using static` line moves with it; another package's `using static` does not.
func TestPersistedPackageInfoConvergesOnMovedNamespace(t *testing.T) {
	lines := []string{
		"using go;",
		"using static go.a.teststructs.foo_package;",
		"using static go.a.teststructs.bar_package;",
		"",
		"namespace go.a.teststructs;",
		"",
		"public static partial class foo_package",
	}

	got := convergePackageNamespace(append([]string(nil), lines...), "go.a.teststructs.foo1", "foo_package")

	want := []string{
		"using go;",
		"using static go.a.teststructs.foo1.foo_package;",
		"using static go.a.teststructs.bar_package;",
		"",
		"namespace go.a.teststructs.foo1;",
		"",
		"public static partial class foo_package",
	}

	if strings.Join(got, "\n") != strings.Join(want, "\n") {
		t.Errorf("converged:\n%s\nwant:\n%s", strings.Join(got, "\n"), strings.Join(want, "\n"))
	}

	if unchanged := convergePackageNamespace(append([]string(nil), want...), "go.a.teststructs.foo1", "foo_package"); strings.Join(unchanged, "\n") != strings.Join(want, "\n") {
		t.Errorf("a file already on its namespace changed:\n%s", strings.Join(unchanged, "\n"))
	}
}
