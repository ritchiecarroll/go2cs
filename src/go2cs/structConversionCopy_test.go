// structConversionCopy_test.go - Gbtc
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

// A Go conversion copies its operand. Where the operand is existing storage whose type carries an array,
// directly or in a nested struct, the emitted C# shares the array's backing unless the operand is cloned,
// as every other value transfer clones it (exprReadsValueNeedingClone). ONE SITE PER PACKAGE.
//
// Each arm names what the emitted file must read and what it must not.
func TestAStructConversionOfExistingStorageCopiesItsArrays(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	const lib = "\"example.com/cc/lib\""

	type arm struct {
		imports string
		body    string
		reads   []string
		absent  []string
	}

	const chord = "type chord struct {\n\tName  string\n\tNotes [2]int\n\tIn    lib.Inner\n}\n\n"

	arms := map[string]arm{
		// Every direction of a named conversion: the operand is cloned.
		"fromforeign": {lib, chord + "func F(lc lib.Chord) chord { return chord(lc) }",
			[]string{"(chord)lc.ΔClone()"}, nil},
		"toforeign": {lib, chord + "func F(c chord) lib.Chord { return lib.Chord(c) }",
			[]string{"(lib.Chord)c.ΔClone()"}, nil},
		"local": {lib, chord + "type chord2 struct {\n\tName  string\n\tNotes [2]int\n\tIn    lib.Inner\n}\n\nfunc F(c chord) chord2 { return chord2(c) }",
			[]string{"(chord2)c.ΔClone()"}, nil},
		// A conversion bound to a variable is the case the probe found: the visitor took the call for a
		// fresh value and the assignment did not clone.
		"bound": {lib, chord + "func F(lc lib.Chord) int { c := chord(lc); c.Notes[0] = 9; return lc.Notes[0] }",
			[]string{"(chord)lc.ΔClone()"}, nil},
		// Existing storage of every shape: a field, an element, a pointer's target.
		"field": {lib, chord + "type holder struct{ L lib.Chord }\n\nfunc F(h holder) chord { return chord(h.L) }",
			[]string{"(chord)h.L.ΔClone()"}, nil},
		"element": {lib, chord + "func F(s []lib.Chord) chord { return chord(s[0]) }",
			[]string{"(chord)s[0].ΔClone()"}, nil},
		"deref": {lib, chord + "func F(p *lib.Chord) chord { return chord(*p) }",
			[]string{"ΔClone()"}, nil},
		// The unnamed target, written as a struct type literal. (A parameter's own entry copy reads
		// `c = c.ΔClone();`, so each want names the cast.)
		"unnamed": {lib, chord + "func F(c chord) int { x := struct {\n\tName  string\n\tNotes [2]int\n\tIn    lib.Inner\n}(c); x.Notes[0] = 1; return c.Notes[0] }",
			[]string{"F_x)c.ΔClone()"}, nil},
		// A named array converts its array the same way.
		"array": {"", "type notes [2]int\n\nfunc F(b [2]int) notes { return notes(b) }",
			[]string{"(notes)b.Clone()"}, nil},

		// A fresh operand is no one else's storage: no clone.
		"call": {lib, chord + "func src() lib.Chord { return lib.Chord{} }\n\nfunc F() chord { return chord(src()) }",
			nil, []string{"ΔClone"}},
		"literal": {lib, chord + "func F() chord { return chord(lib.Chord{Name: \"a\"}) }",
			nil, []string{"ΔClone"}},
		// A defined type written over a named struct is an inherited wrapper whose operators copy: the
		// site adds no second copy.
		"wrapper": {"", "type counts struct{ vals [4]int }\n\ntype Counts counts\n\nfunc F(c Counts) counts { return counts(c) }",
			[]string{"return (counts)c;"}, []string{"(counts)c.ΔClone()"}},
		// A struct WRITTEN with a literal is no wrapper: its unnamed twin still takes the site clone.
		"literalwrite": {lib, chord + "func F(c chord) int { x := struct {\n\tName  string\n\tNotes [2]int\n\tIn    lib.Inner\n}(c); x.Notes[0] = 1; return c.Notes[0] }\n\nfunc G(c chord) chord { return chord(c) }",
			[]string{"F_x)c.ΔClone()", "(chord)c.ΔClone()"}, nil},
		// A struct without an array copies whole as a C# struct: no clone.
		"plain": {"", "type a struct{ X int }\n\ntype b struct{ X int }\n\nfunc F(v a) b { return b(v) }",
			nil, []string{"ΔClone", ".Clone()"}},
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/cc\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

type Inner struct{ V [2]int }

type Chord struct {
	Name  string
	Notes [2]int
	In    Inner
}
`)

	var names, imports []string

	for name, a := range arms {
		header := "package " + name + "\n\n"

		if a.imports != "" {
			header += "import " + a.imports + "\n\n"
		}

		writeModuleFile(t, filepath.Join(appDir, name, name+".go"), header+a.body+"\n")
		names = append(names, name)
		imports = append(imports, "\t_ \"example.com/cc/"+name+"\"\n")
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

	for _, name := range names {
		a := arms[name]
		emitted := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "cc", name, name+".cs"))

		for _, want := range a.reads {
			if !strings.Contains(emitted, want) {
				t.Errorf("%s: the emitted file does not read %q, in:\n%s", name, want, emitted)
			}
		}

		// The value is copied once: no clone of a clone, at any site.
		for _, twice := range []string{".Clone().Clone()", "ΔClone().ΔClone()", ".Clone().ΔClone()", "ΔClone().Clone()"} {
			if strings.Contains(emitted, twice) {
				t.Errorf("%s: the emitted file copies twice, %q, in:\n%s", name, twice, emitted)
			}
		}

		for _, absent := range a.absent {
			if strings.Contains(emitted, absent) {
				t.Errorf("%s: the emitted file reads %q, in:\n%s", name, absent, emitted)
			}
		}
	}
}
