// embeddedArrayCopy_test.go - Gbtc
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

// A struct that carries an array only THROUGH an embedded struct is copied by value in Go like any other: a write
// to the copy's promoted array does not reach the source. go2cs-gen holds an embed as an inline field, so the plain
// C# struct copy copies the embed but shares the array's backing store, unless the struct is stamped
// `[GoValueClone(…)]` with the embed's member and every copy site clones it (typeNeedsValueClone). ONE SITE PER
// PACKAGE.
//
// Each arm names what the emitted file and its package_info.cs must read, and what they must not.
func TestAStructCarryingAnArrayThroughAnEmbedCopiesIt(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	const lib, other = "\"example.com/ea/lib\"", "\"example.com/ea/other\""

	type arm struct {
		imports []string
		body    string
		reads   []string
		absent  []string
	}

	const emb = "type emb struct {\n\tlib.Inner\n\tN int\n}\n\n"

	arms := map[string]arm{
		// The local struct is stamped with its embed's member, and a value copy clones it.
		"stamp": {[]string{lib}, emb + "func F(e emb) int { c := e; c.V[0] = 9; return e.V[0] }",
			[]string{`[GoValueClone("Inner")] public partial struct emb`, "var c = e.ΔClone();"}, nil},
		// A foreign struct of the same shape is stamped in its own package, so copying one clones it here.
		"assign": {[]string{lib}, "func F(le lib.Emb) int { c := le; c.V[0] = 9; return le.V[0] }",
			[]string{"var c = le.ΔClone();"}, nil},
		// The four conversion routes the copy probe read.
		"fromforeign": {[]string{lib}, emb + "func F(le lib.Emb) emb { return emb(le) }",
			[]string{"(emb)le.ΔClone()"}, nil},
		"toforeign": {[]string{lib}, emb + "func F(e emb) lib.Emb { return lib.Emb(e) }",
			[]string{"(lib.Emb)e.ΔClone()"}, nil},
		"local": {[]string{lib}, emb + "type emb2 struct {\n\tlib.Inner\n\tN int\n}\n\nfunc F(e emb) emb2 { return emb2(e) }",
			[]string{"(emb2)e.ΔClone()"}, nil},
		"foreignpair": {[]string{lib, other}, "func F(le lib.Emb) other.Emb { return other.Emb(le) }",
			[]string{"otherꓸEmb(le.ΔClone())"}, nil},
		// An embedded NAMED ARRAY is a field like any other array.
		"namedarray": {nil, "type A [2]int\n\ntype h struct {\n\tA\n\tN int\n}\n\nfunc F(x h) int { c := x; c.A[0] = 9; return x.A[0] }",
			[]string{`[GoValueClone("A")] public partial struct h`, "var c = x.ΔClone();"}, nil},
		// The embed is reached two levels down.
		"deep": {[]string{lib}, "type outer struct {\n\tlib.Emb\n}\n\nfunc F(o outer) int { c := o; c.V[0] = 9; return o.V[0] }",
			[]string{`[GoValueClone("Emb")] public partial struct outer`, "var c = o.ΔClone();"}, nil},

		// An embedded POINTER is shared in Go too: no stamp, no clone.
		"pointer": {[]string{lib}, "type p struct {\n\t*lib.Inner\n\tN int\n}\n\nfunc F(x p) int { c := x; c.N = 9; return x.N }",
			nil, []string{"GoValueClone", "ΔClone"}},
		// An embed that carries no array needs nothing.
		"plain": {[]string{lib}, "type q struct {\n\tlib.Plain\n\tN int\n}\n\nfunc F(x q) int { c := x; c.N = 9; return x.N }",
			nil, []string{"GoValueClone", "ΔClone"}},
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ea\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type Inner struct{ V [2]int }

type Emb struct {
	Inner
	N int
}

type Plain struct{ X int }
`)
	writeModuleFile(t, filepath.Join(appDir, "other", "other.go"), `package other

import "example.com/ea/lib"

type Emb struct {
	lib.Inner
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
		imports = append(imports, "\t_ \"example.com/ea/"+name+"\"\n")
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

	// The foreign struct carries its own stamp, written in its own package.
	libInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "ea", "lib", "package_info.cs"))

	if want := `[GoValueClone("Inner")] public partial struct Emb`; !strings.Contains(libInfo, want) {
		t.Errorf("lib: package_info.cs does not read %q, in:\n%s", want, libInfo)
	}

	for _, name := range names {
		a := arms[name]
		dir := filepath.Join(options.go2csPath, "src", "example.com", "ea", name)
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
