// foreignPairStructConversion_test.go - Gbtc
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
	"regexp"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// A conversion between two struct types that BOTH belong to other packages has no C# type of this
// package to declare its operator, so the site calls the function go2cs-gen writes for the recorded
// pair, named as Go names the target (`libꓸPitch(o)`). ONE SITE PER PACKAGE, as in the named and
// unnamed matrix: the records are a set per package.
//
// Each `call` arm must record exactly its pair and read as the call. Each `cast` arm is a pair the
// function is not written for, and must keep the emission it has today: no function call, and no
// record of two foreign types.
func TestAStructConversionBetweenTwoForeignTypesCallsItsFunction(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	const lib, other = "\"example.com/fp/lib\"", "\"example.com/fp/other\""

	type arm struct {
		imports string
		body    string
		record  string   // "source -> target" for a call arm, empty for a cast arm
		reads   []string // what the emitted file holds
		absent  []string // what it must not hold
	}

	arms := map[string]arm{
		// The function is called wherever the conversion is written: bound to a variable, and as the base
		// of a selector.
		"cross": {lib + "\n" + other, "func F(o other.Pitch) int { p := lib.Pitch(o); return p.Hz + lib.Pitch(o).Hz }",
			"other.Pitch -> lib.Pitch", []string{"var p = libꓸPitch(o);", "libꓸPitch(o).Hz"}, []string{"(lib.Pitch)o"}},
		"back": {lib + "\n" + other, "func F(p lib.Pitch) other.Pitch { return other.Pitch(p) }",
			"lib.Pitch -> other.Pitch", []string{"return otherꓸPitch(p);"}, nil},
		// Two types of ONE foreign package are as closed to this package as two of two.
		"same": {lib, "func F(p lib.Pitch) lib.Twin { return lib.Twin(p) }",
			"lib.Pitch -> lib.Twin", []string{"return libꓸTwin(p);"}, nil},
		"fields": {lib + "\n" + other, "func F(n other.Note) lib.Note { return lib.Note(n) }",
			"other.Note -> lib.Note", []string{"return libꓸNote(n);"}, nil},
		// The operand is whatever expression Go converts, written once.
		"operand": {lib + "\n" + other, "func source() other.Pitch { return other.Pitch{Hz: 3} }\n\nfunc F() lib.Pitch { return lib.Pitch(source()) }",
			"other.Pitch -> lib.Pitch", []string{"return libꓸPitch(source());"}, nil},
		// A local alias of the target names the same foreign type, so the same function.
		"alias": {lib + "\n" + other, "type P = lib.Pitch\n\nfunc F(o other.Pitch) P { return P(o) }",
			"other.Pitch -> lib.Pitch", []string{"libꓸPitch(o)"}, nil},
		// A file's own name for the import does not rename the function: it is one per package.
		"renamed": {"l " + lib + "\n" + other, "func F(o other.Pitch) l.Pitch { return l.Pitch(o) }",
			"other.Pitch -> l.Pitch", []string{"return libꓸPitch(o);"}, nil},

		// A type written over another named type converts from that base through its own operator.
		"wrapper": {lib + "\n" + other, "func F(o other.Pitch) lib.Tag { return lib.Tag(o) }",
			"", nil, []string{"libꓸTag("}},
		// An unexported field is out of this package's reach.
		"hidden": {lib, "func F(h lib.Hidden) lib.Veiled { return lib.Veiled(h) }",
			"", nil, []string{"libꓸVeiled("}},
		// Each package lifts its own C# type for an unnamed struct field.
		"deep": {lib + "\n" + other, "func F(d other.Deep) lib.Deep { return lib.Deep(d) }",
			"", nil, []string{"libꓸDeep("}},
		"generic": {lib + "\n" + other, "func F(b other.Box[int]) lib.Box[int] { return lib.Box[int](b) }",
			"", nil, []string{"libꓸBox"}},
		// A local side hosts the operator, as before.
		"local": {other, "type mine struct{ Hz int }\n\nfunc F(o other.Pitch) int { return mine(o).Hz }",
			"other.Pitch -> mine", []string{"(mine)o"}, []string{"ꓸmine("}},
		// A conversion to the operand's own type converts nothing.
		"self": {lib, "func F(p lib.Pitch) lib.Pitch { return lib.Pitch(p) }",
			"", nil, []string{"libꓸPitch("}},
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "other", "other.go"), `package other

type Pitch struct{ Hz int }

type Note struct {
	Hz   int
	Name string
}

type Deep struct{ In struct{ X int } }

type Box[T any] struct{ V T }
`)
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), `package lib

import "example.com/fp/other"

type Pitch struct{ Hz int }

type Twin struct{ Hz int }

type Note struct {
	Hz   int
	Name string
}

type Tag other.Pitch

type Hidden struct{ hz int }

type Veiled struct{ hz int }

type Deep struct{ In struct{ X int } }

type Box[T any] struct{ V T }
`)

	var names, imports []string

	for name, a := range arms {
		header := "package " + name + "\n\nimport (\n\t" + strings.ReplaceAll(a.imports, "\n", "\n\t") + "\n)\n\n"

		writeModuleFile(t, filepath.Join(appDir, name, name+".go"), header+a.body+"\n")
		names = append(names, name)
		imports = append(imports, "\t_ \"example.com/fp/"+name+"\"\n")
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

	record := regexp.MustCompile(`^\[assembly: GoImplicitConv<([^,>]+), ([^,>]+)>\]$`)
	foreignPair := regexp.MustCompile(`^\w+\.\w+ -> \w+\.\w+$`)

	for _, name := range names {
		a := arms[name]
		dir := filepath.Join(options.go2csPath, "src", "example.com", "fp", name)
		packageInfo := readGenerated(t, filepath.Join(dir, "package_info.cs"))
		emitted := readGenerated(t, filepath.Join(dir, name+".cs"))

		var records []string

		for _, line := range strings.Split(packageInfo, "\n") {
			if m := record.FindStringSubmatch(strings.TrimSpace(line)); m != nil {
				records = append(records, m[1]+" -> "+m[2])
			}
		}

		if a.record == "" {
			for _, r := range records {
				if foreignPair.MatchString(r) {
					t.Errorf("%s: a pair the function is not written for is recorded: %v", name, records)
				}
			}
		} else if len(records) != 1 || records[0] != a.record {
			t.Errorf("%s: want the one record %q, got %v", name, a.record, records)
		}

		for _, want := range a.reads {
			if !strings.Contains(emitted, want) {
				t.Errorf("%s: the emitted file does not read %q, in:\n%s", name, want, emitted)
			}
		}

		for _, absent := range a.absent {
			if strings.Contains(emitted, absent) {
				t.Errorf("%s: the emitted file reads %q, in:\n%s", name, absent, emitted)
			}
		}
	}
}
