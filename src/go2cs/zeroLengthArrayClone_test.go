// zeroLengthArrayClone_test.go - Gbtc
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
	"sort"
	"strings"
	"testing"
)

// A ZERO-LENGTH array holds no element, so a copy of a struct that carries one has nothing to clone: the copy and
// the source can share the empty backing store without any write reaching across, because there is no element to
// write. A `[0]T` field therefore takes no `[GoValueClone(…)]` member, whether it is named (the flexible-array
// member idiom, syscall's `InotifyEvent.Name [0]uint8`) or embedded (the uncomparable-marker idiom,
// `type nonComparable [0]func()`), and a struct whose only arrays are zero-length takes no clone at any copy site.
// A real array beside one keeps its member. ONE SITE PER PACKAGE.
//
// Each arm names what the emitted file and its package_info.cs must read, and what they must not.
func TestAZeroLengthArrayFieldTakesNoClone(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	const lib = "\"example.com/zl/lib\""

	type arm struct {
		imports []string
		body    string
		reads   []string
		absent  []string
	}

	arms := map[string]arm{
		// syscall's shape: a fixed header and a zero-length name at its end.
		"flexible": {nil, "type ev struct {\n\tWd   int32\n\tLen  uint32\n\tName [0]uint8\n}\n\nfunc F(e ev) uint32 { c := e; c.Len = 9; return e.Len }",
			nil, []string{"GoValueClone", "ΔClone"}},
		// go-json-experiment's shape: a zero-length array type embedded to make the struct uncomparable.
		"marker": {nil, "type nonComparable [0]func()\n\ntype tok struct {\n\tnonComparable\n\tN int\n}\n\nfunc F(x tok) tok { c := x; c.N = 9; return c }",
			nil, []string{"GoValueClone", "ΔClone"}},
		// The marker reached through another package's struct.
		"foreign": {[]string{lib}, "func F(x lib.Tok) lib.Tok { c := x; c.N = 9; return c }",
			nil, []string{"ΔClone"}},
		// A zero-length array beside a real one: only the real one is a member.
		"mixed": {nil, "type info struct {\n\tLoads [3]uint64\n\tX_f   [0]int8\n\tN     int\n}\n\nfunc F(x info) uint64 { c := x; c.Loads[0] = 9; return x.Loads[0] }",
			[]string{`[GoValueClone("Loads")] public partial struct info`, "var c = x.ΔClone();"}, []string{"X_f\")"}},
		// A struct whose only array-carrying field is a struct of zero-length arrays.
		"nested": {nil, "type hdr struct{ Name [0]byte }\n\ntype rec struct {\n\tH hdr\n\tN int\n}\n\nfunc F(x rec) int { c := x; c.N = 9; return x.N }",
			nil, []string{"GoValueClone", "ΔClone"}},

		// Controls: a real array, named and embedded, still clones.
		"real": {nil, "type r struct {\n\tV [2]int\n}\n\nfunc F(x r) int { c := x; c.V[0] = 9; return x.V[0] }",
			[]string{`[GoValueClone("V")] public partial struct r`, "var c = x.ΔClone();"}, nil},
		"realembed": {nil, "type A [2]int\n\ntype h struct {\n\tA\n\tN int\n}\n\nfunc F(x h) int { c := x; c.A[0] = 9; return x.A[0] }",
			[]string{`[GoValueClone("A")] public partial struct h`, "var c = x.ΔClone();"}, nil},
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/zl\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type nonComparable [0]func()

type Tok struct {
	nonComparable
	N int
}
`)

	var names, imports []string

	for name, a := range arms {
		header := "package " + name + "\n\n"

		if len(a.imports) > 0 {
			header += "import (\n\t" + strings.Join(a.imports, "\n\t") + "\n)\n\n"
		}

		writeModuleFile(t, filepath.Join(appDir, name, name+".go"), header+a.body+"\n")
		names = append(names, name)
		imports = append(imports, "\t_ \"example.com/zl/"+name+"\"\n")
	}

	sort.Strings(names)
	sort.Strings(imports)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), "package main\n\nimport (\n"+strings.Join(imports, "")+")\n\nfunc main() {}\n")

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

	// The foreign struct takes no stamp in its own package.
	libInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "zl", "lib", "package_info.cs"))

	if strings.Contains(libInfo, "GoValueClone") {
		t.Errorf("lib: package_info.cs reads GoValueClone, in:\n%s", libInfo)
	}

	for _, name := range names {
		a := arms[name]
		dir := filepath.Join(options.go2csPath, "src", "example.com", "zl", name)
		emitted := readGenerated(t, filepath.Join(dir, name+".cs")) + readGenerated(t, filepath.Join(dir, "package_info.cs"))

		for _, want := range a.reads {
			if !strings.Contains(emitted, want) {
				t.Errorf("%s: the emitted files do not read %q, in:\n%s", name, want, emitted)
			}
		}

		for _, twice := range []string{".Clone().Clone()", "ΔClone().ΔClone()", ".Clone().ΔClone()", "ΔClone().Clone()"} {
			if strings.Contains(emitted, twice) {
				t.Errorf("%s: the emitted files copy twice, %q, in:\n%s", name, twice, emitted)
			}
		}

		for _, absent := range a.absent {
			if strings.Contains(emitted, absent) {
				t.Errorf("%s: the emitted files read %q, in:\n%s", name, absent, emitted)
			}
		}
	}
}
