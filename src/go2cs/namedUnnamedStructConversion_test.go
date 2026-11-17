// namedUnnamedStructConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"slices"
	"sort"
	"strings"
	"testing"
)

// Every place Go moves a struct value between a NAMED struct type and an UNNAMED one needs a recorded
// GoImplicitConv, because the unnamed type is lifted to its own C# type and nothing else declares an
// operator between the two. ONE SITE PER PACKAGE: the records are a set per package, so two sites of
// one pair in one package hide each other. That is how a `var x struct{ Hz int } = p` with no record
// went unseen: it compiled wherever an argument or a return of the same pair sat beside it.
//
// The named side is local (`hertz`) or another package's (`lib.Pitch`). N2A moves a named value into
// an unnamed slot and A2N the reverse. `conv` is the written conversion and `cmp` the comparison; both
// also need a cast at the site, read from the emitted file.
func TestEachNamedUnnamedStructMoveRecordsItsConversion(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	const anon = "struct{ Hz int }"

	// One template per place Go's assignability rule lets a struct value change type without a conversion
	// being written, plus the written conversion. SRC is the value's type and DST the slot's: N2A reads
	// SRC as the named type and DST as the unnamed one, A2N the reverse.
	moves := map[string]string{
		"conv":     "func F(v SRC) int { x := DST(v); return x.Hz + DST(v).Hz }",
		"var":      "func F(v SRC) int { var x DST = v; return x.Hz }",
		"pkgvar":   "var v0 SRC\n\nvar X DST = v0",
		"assign":   "func F(v SRC) int { var x DST; x = v; return x.Hz }",
		"setfield": "type box struct{ V DST }\n\nfunc F(v SRC) int { var b box; b.V = v; return b.V.Hz }",
		"setelem":  "func F(v SRC) int { s := make([]DST, 1); s[0] = v; return s[0].Hz }",
		"setmap":   "func F(v SRC) int { m := make(map[string]DST); m[\"k\"] = v; return len(m) }",
		"setderef": "func F(v SRC, p *DST) { *p = v }",
		"arg":      "func take(x DST) int { return x.Hz }\n\nfunc F(v SRC) int { return take(v) }",
		"variadic": "func take(xs ...DST) int { return len(xs) }\n\nfunc F(v SRC) int { return take(v) }",
		"method":   "type taker struct{}\n\nfunc (taker) take(x DST) int { return x.Hz }\n\nfunc F(v SRC) int { return taker{}.take(v) }",
		// The function value takes its type from a declared function. An unnamed struct written INSIDE a
		// func type (`take func(struct{ Hz int }) int`) is not lifted: the converter stops and says so.
		"funcval":   "func take(x DST) int { return x.Hz }\n\nfunc F(v SRC) int { f := take; return f(v) }",
		"deferarg":  "func take(x DST) {}\n\nfunc F(v SRC) { defer take(v) }",
		"goarg":     "func take(x DST) {}\n\nfunc F(v SRC) { go take(v) }",
		"ret":       "func F(v SRC) DST { return v }",
		"ret2":      "func F(v SRC) (DST, error) { return v, nil }",
		"funclit":   "func F(v SRC) int { f := func() DST { return v }; return f().Hz }",
		"field":     "type box struct{ V DST }\n\nfunc F(v SRC) int { b := box{V: v}; return b.V.Hz }",
		"fieldpos":  "type box struct{ V DST }\n\nfunc F(v SRC) int { b := box{v}; return b.V.Hz }",
		"elem":      "func F(v SRC) int { s := []DST{v}; return s[0].Hz }",
		"arrelem":   "func F(v SRC) int { s := [1]DST{v}; return s[0].Hz }",
		"mapval":    "func F(v SRC) int { m := map[string]DST{\"k\": v}; return len(m) }",
		"mapkey":    "func F(v SRC) int { m := map[DST]int{v: 1}; return len(m) }",
		"index":     "func F(v SRC, m map[DST]int) int { return m[v] }",
		"delete":    "func F(v SRC, m map[DST]int) { delete(m, v) }",
		"append":    "func F(v SRC, s []DST) int { s = append(s, v); return len(s) }",
		"send":      "func F(v SRC) int { c := make(chan DST, 1); c <- v; return (<-c).Hz }",
		"selsend":   "func F(v SRC, c chan DST) { select { case c <- v: default: } }",
		"recv":      "func F(c chan SRC) int { var x DST; x = <-c; return x.Hz }",
		"recvok":    "func F(c chan SRC) int { var x DST; var ok bool; x, ok = <-c; _ = ok; return x.Hz }",
		"mapok":     "func F(m map[string]SRC) int { var x DST; var ok bool; x, ok = m[\"k\"]; _ = ok; return x.Hz }",
		"tuple":     "func two() (SRC, bool) { var v SRC; return v, true }\n\nfunc F() int { var x DST; var ok bool; x, ok = two(); _ = ok; return x.Hz }",
		"rangevar":  "func F(s []SRC) int { var x DST; for _, x = range s { }; return x.Hz }",
		"assertion": "func F(i any) int { var x DST; x = i.(SRC); return x.Hz }",
	}

	// A comparison has no slot. Its unnamed operand is cast to the named type, so each of these records the
	// A2N pair whichever side the unnamed operand is on.
	comparisons := map[string]string{
		"cmp":     "func F(a ANON, n NAMED) bool { return a == n || n != a }",
		"casetag": "func F(a ANON, n NAMED) int { switch a { case n: return 1 }; return 0 }",
		"caseval": "func F(a ANON, n NAMED) int { switch n { case a: return 1 }; return 0 }",
	}

	// U2U: both sides are the unnamed type, under two lifts. Inside one function the converter makes one C#
	// type of every place the type is written, and a function declared LATER adopts a lift that a signature
	// or a package-level declaration above it has registered. So each site here has its slot written inside
	// F and takes its value from a declaration BELOW F, which leaves two lifts and one move between them.
	// A return, a package variable and a type assertion have no such form: their slot's lift is registered
	// before the value's is made. `alias` moves a value of an alias of the type.
	const src = "\n\nfunc src() (r ANON) { return }"

	lifts := map[string]string{
		"var":      "func F() int { var x ANON = src(); return x.Hz }" + src,
		"assign":   "func F() int { var x ANON; x = src(); return x.Hz }" + src,
		"setfield": "func F() int { var v ANON; var b box; b.V = v; return b.V.Hz }\n\ntype box struct{ V ANON }",
		"setelem":  "func F() int { var s [1]ANON; s[0] = src(); return s[0].Hz }" + src,
		"setmap":   "func F() int { m := map[string]ANON{}; m[\"k\"] = src(); return len(m) }" + src,
		"setderef": "func F() int { var x ANON; p := &x; *p = src(); return x.Hz }" + src,
		"arg":      "func F() int { var v ANON; return take(v) }\n\nfunc take(x ANON) int { return x.Hz }",
		"variadic": "func F() int { var v ANON; return take(v) }\n\nfunc take(xs ...ANON) int { return len(xs) }",
		"method":   "func F() int { var v ANON; return taker{}.take(v) }\n\ntype taker struct{}\n\nfunc (taker) take(x ANON) int { return x.Hz }",
		"funcval":  "func F() int { var v ANON; f := take; return f(v) }\n\nfunc take(x ANON) int { return x.Hz }",
		"deferarg": "func F() { var v ANON; defer take(v) }\n\nfunc take(x ANON) {}",
		"goarg":    "func F() { var v ANON; go take(v) }\n\nfunc take(x ANON) {}",
		"funclit":  "func F() int { f := func() ANON { return src() }; return f().Hz }" + src,
		"funclit2": "func F() int { f := func() (ANON, error) { return src(), nil }; x, _ := f(); return x.Hz }" + src,
		"field":    "func F() int { var v ANON; b := box{V: v}; return b.V.Hz }\n\ntype box struct{ V ANON }",
		"fieldpos": "func F() int { var v ANON; b := box{v}; return b.V.Hz }\n\ntype box struct{ V ANON }",
		"elem":     "func F() int { s := []ANON{src()}; return s[0].Hz }" + src,
		"arrelem":  "func F() int { s := [1]ANON{src()}; return s[0].Hz }" + src,
		"mapval":   "func F() int { m := map[string]ANON{\"k\": src()}; return len(m) }" + src,
		"mapkey":   "func F() int { m := map[ANON]int{src(): 1}; return len(m) }" + src,
		"index":    "func F() int { m := map[ANON]int{}; return m[src()] }" + src,
		"delete":   "func F() { m := map[ANON]int{}; delete(m, src()) }" + src,
		"append":   "func F() int { var s []ANON; s = append(s, src()); return len(s) }" + src,
		"send":     "func F() { var c chan ANON; c <- src() }" + src,
		"selsend":  "func F() { var c chan ANON; select { case c <- src(): default: } }" + src,
		"recv":     "func F() int { var x ANON; x = <-ch(); return x.Hz }\n\nfunc ch() chan ANON { return nil }",
		"recvok":   "func F() int { var x ANON; var ok bool; x, ok = <-ch(); _ = ok; return x.Hz }\n\nfunc ch() chan ANON { return nil }",
		"mapok":    "func F() int { var x ANON; var ok bool; x, ok = all()[\"k\"]; _ = ok; return x.Hz }\n\nfunc all() map[string]ANON { return nil }",
		"tuple":    "func F() int { var x ANON; var ok bool; x, ok = two(); _ = ok; return x.Hz }\n\nfunc two() (r ANON, ok bool) { return r, true }",
		"rangevar": "func F() int { var x ANON; for _, x = range all() { }; return x.Hz }\n\nfunc all() []ANON { return nil }",
		"alias":    "func F() int { var x ANON = mk(); return x.Hz }\n\nfunc mk() (r later) { return }\n\ntype later = ANON",
		// A comparison casts one operand to the other's lift: to the left operand's, or to the one lift
		// that has its name where the comparison is written.
		"cmp":     "func F() bool { var x ANON; return x == src() || src() != x }" + src,
		"casetag": "func F() int { var x ANON; switch x { case src(): return 1 }; return 0 }" + src,
		"caseval": "func F() int { var x ANON; switch src() { case x: return 1 }; return 0 }" + src,
	}

	// ONE LIFT: the controls. Where the converter already gives both places one C# type there is no move
	// between two types, and nothing is recorded: the value's declaration is above F (a result, a
	// parameter, an alias), or the slot is not F's own (a channel element, a conversion, a result).
	oneLift := map[string]string{
		"above":  "func mk() (r ANON) { return }\n\nfunc F() int { var x ANON = mk(); return x.Hz }",
		"alias":  "type S = ANON\n\nfunc mk() (r S) { return }\n\nfunc F() int { var x ANON = mk(); return x.Hz }",
		"arg":    "func take(x ANON) int { return x.Hz }\n\nfunc F() int { var v ANON; return take(v) }",
		"send":   "func F() int { c := make(chan ANON, 1); c <- src(); return (<-c).Hz }" + src,
		"conv":   "func F() int { x := ANON(src()); return x.Hz }" + src,
		"result": "func F() int { return get().Hz }\n\nfunc get() ANON { var x ANON; return x }",
	}

	sites := map[string]map[string]string{"n2a": {}, "a2n": {}, "u2u": {}, "one": {}}

	for kind, body := range lifts {
		sites["u2u"][kind] = strings.ReplaceAll(body, "ANON", anon)
	}

	for kind, body := range oneLift {
		sites["one"][kind] = strings.ReplaceAll(body, "ANON", anon)
	}

	for kind, body := range moves {
		sites["n2a"][kind] = strings.NewReplacer("SRC", "%[1]s", "DST", anon).Replace(body)
		sites["a2n"][kind] = strings.NewReplacer("SRC", anon, "DST", "%[1]s").Replace(body)
	}

	for kind, body := range comparisons {
		sites["a2n"][kind] = strings.NewReplacer("ANON", anon, "NAMED", "%[1]s").Replace(body)
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nu\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "lib", "lib.go"), "package lib\n\ntype Pitch struct{ Hz int }\n")

	type site struct{ pkg, direction, kind, named string }

	var all []site
	var imports []string

	for direction, kinds := range sites {
		for kind, body := range kinds {
			if direction == "u2u" || direction == "one" {
				s := site{pkg: direction + kind, direction: direction, kind: kind}

				writeModuleFile(t, filepath.Join(appDir, s.pkg, s.pkg+".go"), "package "+s.pkg+"\n\n"+body+"\n")
				all = append(all, s)
				imports = append(imports, "\t_ \"example.com/nu/"+s.pkg+"\"\n")

				continue
			}

			for _, side := range []string{"l", "f"} {
				s := site{pkg: direction + kind + side, direction: direction, kind: kind, named: "hertz"}
				header := "package " + s.pkg + "\n\ntype hertz struct{ Hz int }\n\n"

				if side == "f" {
					s.named = "lib.Pitch"
					header = "package " + s.pkg + "\n\nimport \"example.com/nu/lib\"\n\n"
				}

				writeModuleFile(t, filepath.Join(appDir, s.pkg, s.pkg+".go"), header+fmt.Sprintf(body, s.named)+"\n")
				all = append(all, s)
				imports = append(imports, "\t_ \"example.com/nu/"+s.pkg+"\"\n")
			}
		}
	}

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
	liftDeclaration := regexp.MustCompile(`partial struct (\S+) /\*dyn\*/`)
	sort.Slice(all, func(i, j int) bool { return all[i].pkg < all[j].pkg })

	for _, s := range all {
		dir := filepath.Join(options.go2csPath, "src", "example.com", "nu", s.pkg)
		packageInfo := readGenerated(t, filepath.Join(dir, "package_info.cs"))
		emitted := readGenerated(t, filepath.Join(dir, s.pkg+".cs"))

		var records []string
		lifted := ""

		for _, line := range strings.Split(packageInfo, "\n") {
			if m := record.FindStringSubmatch(strings.TrimSpace(line)); m != nil {
				records = append(records, m[1]+" -> "+m[2])

				switch {
				case s.direction == "n2a" && m[1] == s.named:
					lifted = m[2]
				case s.direction == "a2n" && m[2] == s.named:
					lifted = m[1]
				}
			}
		}

		if s.direction == "one" {
			if len(records) != 0 {
				t.Errorf("%s (one lift, %s): want no record, got %v", s.pkg, s.kind, records)
			}

			continue
		}

		if s.direction == "u2u" {
			var declared []string

			for _, m := range liftDeclaration.FindAllStringSubmatch(emitted, -1) {
				declared = append(declared, m[1])
			}

			from, to, _ := strings.Cut(strings.Join(records, ""), " -> ")

			if len(records) != 1 || from == to || !slices.Contains(declared, from) || !slices.Contains(declared, to) {
				t.Errorf("%s (u2u %s): want one record between two of the lifts %v and no other, got %v", s.pkg, s.kind, declared, records)
				continue
			}

			var wants []string

			switch s.kind {
			case "cmp":
				wants = []string{"x == (" + to + ")src()", "(" + to + ")src() != x"}
			case "casetag":
				wants = []string{"when ᴛ == (" + to + ")src()"}
			case "caseval":
				wants = []string{"when (" + to + ")ᴛ == x"}
			}

			for _, want := range wants {
				if !strings.Contains(emitted, want) {
					t.Errorf("%s: the comparison of two lifts does not read %q, in:\n%s", s.pkg, want, emitted)
				}
			}

			continue
		}

		if lifted == "" || lifted == s.named || len(records) != 1 {
			t.Errorf("%s (%s %s, named %s): want one record of the move and no other, got %v", s.pkg, s.direction, s.kind, s.named, records)
			continue
		}

		switch s.kind {
		case "conv":
			if s.direction == "n2a" {
				for _, want := range []string{"var x = (" + lifted + ")v;", "((" + lifted + ")v).Hz"} {
					if !strings.Contains(emitted, want) {
						t.Errorf("%s: the conversion to the unnamed type does not read %q, in:\n%s", s.pkg, want, emitted)
					}
				}
			}
		case "cmp":
			for _, want := range []string{"(" + s.named + ")a == n", "n != (" + s.named + ")a"} {
				if !strings.Contains(emitted, want) {
					t.Errorf("%s: the comparison does not read %q, in:\n%s", s.pkg, want, emitted)
				}
			}
		case "casetag":
			if want := "when (" + s.named + ")ᴛ == n"; !strings.Contains(emitted, want) {
				t.Errorf("%s: the case over an unnamed tag does not read %q, in:\n%s", s.pkg, want, emitted)
			}
		case "caseval":
			if want := "when ᴛ == (" + s.named + ")a"; !strings.Contains(emitted, want) {
				t.Errorf("%s: the unnamed case value does not read %q, in:\n%s", s.pkg, want, emitted)
			}
		}
	}
}
