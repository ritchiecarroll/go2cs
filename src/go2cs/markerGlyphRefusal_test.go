// markerGlyphRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/scanner"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strconv"
	"strings"
	"testing"
	"unicode"

	"golang.org/x/tools/go/packages"
)

// lettersInGoStringTokens answers every non-ASCII letter inside a STRING or CHAR token of the converter's non-test
// sources, read by go/scanner so no comment is ever read, mapped to one site.
func lettersInGoStringTokens(t *testing.T) (map[rune]string, []string) {
	t.Helper()

	files, err := filepath.Glob("*.go")

	if err != nil || len(files) == 0 {
		t.Fatalf("no converter sources beside the test: %v", err)
	}

	found := map[rune]string{}
	var literals []string

	for _, file := range files {
		// The marker list itself is not a writer of names: its literals would make every listed letter "written".
		if strings.HasSuffix(file, "_test.go") || file == "markerGlyphRefusal.go" {
			continue
		}

		source, err := os.ReadFile(file)

		if err != nil {
			t.Fatal(err)
		}

		fset := token.NewFileSet()
		var scan scanner.Scanner
		scan.Init(fset.AddFile(file, -1, len(source)), source, nil, 0)

		for {
			pos, tok, lit := scan.Scan()

			if tok == token.EOF {
				break
			}

			if tok != token.STRING && tok != token.CHAR {
				continue
			}

			value, err := strconv.Unquote(lit)

			if err != nil {
				value = lit
			}

			literals = append(literals, value)

			for _, r := range value {
				if r > unicode.MaxASCII && unicode.IsLetter(r) {
					if _, seen := found[r]; !seen {
						found[r] = fset.Position(pos).String()
					}
				}
			}
		}
	}

	return found, literals
}

var (
	csCommentPattern = regexp.MustCompile(`(?s)//[^\n]*|/\*.*?\*/`)
	csStringPattern  = regexp.MustCompile(`@"(?:[^"]|"")*"|"(?:[^"\\\n]|\\.)*"|'(?:[^'\\\n]|\\.)+'`)
)

// lettersInCSharp answers every non-ASCII letter in the C# sources under dir, comments removed: in the whole text when
// keepStrings is true (go2cs-gen, whose emitted text lives in literals of every form, raw `"""` strings included),
// outside string literals otherwise (golib's own identifiers).
func lettersInCSharp(t *testing.T, dir string, keepStrings bool) map[rune]string {
	t.Helper()

	found := map[rune]string{}
	files := 0

	err := filepath.WalkDir(dir, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			switch entry.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(path, ".cs") {
			return nil
		}

		source, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		files++
		text := csCommentPattern.ReplaceAllString(string(source), " ")

		if !keepStrings {
			text = csStringPattern.ReplaceAllString(text, `""`)
		}

		for _, r := range text {
			if r > unicode.MaxASCII && unicode.IsLetter(r) {
				if _, seen := found[r]; !seen {
					found[r] = path
				}
			}
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	if files == 0 {
		t.Fatalf("no C# sources read under %s", dir)
	}

	return found
}

func TestMarkerGlyphsAreTheUnionOfEverySideThatWritesThem(t *testing.T) {
	converter, _ := lettersInGoStringTokens(t)
	generator := lettersInCSharp(t, filepath.Join("..", "gen"), true)
	golib := lettersInCSharp(t, filepath.Join("..", "core", "golib"), false)

	// The converter's embedded templates are written into every converted project as they are.
	templates := map[rune]string{}

	for _, name := range []string{"csproj-template.xml", "test-csproj-template.xml", "package_info-template.txt"} {
		source, err := os.ReadFile(name)

		if err != nil {
			t.Fatalf("embedded template %s: %v", name, err)
		}

		for _, r := range string(source) {
			if r > unicode.MaxASCII && unicode.IsLetter(r) {
				if _, seen := templates[r]; !seen {
					templates[r] = name
				}
			}
		}
	}

	written := map[rune]bool{}

	for side, letters := range map[string]map[rune]string{"converter string literal": converter, "converter embedded template": templates, "go2cs-gen source": generator, "golib identifier": golib} {
		for r, site := range letters {
			written[r] = true

			if _, listed := markerGlyphRunes[r]; !listed {
				t.Errorf("%c (U+%04X) appears in a %s (%s) and is not in markerGlyphSources: list the constant or site that writes it", r, r, side, site)
			}
		}
	}

	var dead []string

	for r, source := range markerGlyphRunes {
		if !written[r] {
			dead = append(dead, string(r)+" ("+source+")")
		}
	}

	sort.Strings(dead)

	if len(dead) > 0 {
		t.Errorf("markerGlyphSources lists letters no converter literal, go2cs-gen literal or golib identifier writes: %s", strings.Join(dead, ", "))
	}

	if len(markerGlyphRunes) < 20 {
		t.Fatalf("vacuous marker set: %d letters", len(markerGlyphRunes))
	}
}

// deltaLiteralNamePattern matches a converter literal that is a fixed Δ-led name, alone or followed by its call.
var deltaLiteralNamePattern = regexp.MustCompile(`^(Δ[A-Za-z][A-Za-z0-9]*)(\(|$)`)

func TestDeltaMintedMemberNamesAreEveryFixedDeltaLiteral(t *testing.T) {
	_, literals := lettersInGoStringTokens(t)

	// Suffixes, not names: each is matched by its own rule in markerGlyphReason.
	suffixes := map[string]bool{ShapeWitnessSuffix: true}
	seen := 0

	for _, literal := range literals {
		match := deltaLiteralNamePattern.FindStringSubmatch(literal)

		if match == nil || suffixes[match[1]] {
			continue
		}

		seen++

		if _, listed := deltaMintedMemberNames[match[1]]; !listed {
			t.Errorf("the converter writes the fixed name %s (literal %q) and deltaMintedMemberNames does not list it", match[1], literal)
		}
	}

	if seen == 0 {
		t.Fatal("vacuous read: no Δ-led name literal found in the converter")
	}
}

const markerGlyphFixture = `package m

import "strings"

var Ꮡx int

func fᴛ() {}

const maskᶜ = 1

type ReaderжReader struct{}

func appendꓸꓸꓸ() {}

func g() string {
	seedˢ := "x"
	return seedˢ
}

func Lockꞏtwin() {}

// A local named like a reserved word is renamed Δslice, which would capture the package's own Δslice.
var Δslice = "pkg"

func f() string {
	slice := "local"
	_ = slice
	return Δslice
}

// A shadowed local array is renamed ΔarrayΔ1: its base is array's SANITIZED spelling, which the package never declares.
var ΔarrayΔ1 int

func arr() int {
	array := 1
	{
		array := 2
		_ = array
	}
	return array
}

// An import alias, and the package's own name, are renamed Δ<name> when they collide.
var Δstrings, Δm = strings.ToUpper("x"), 0

// A shape-witnessed type parameter S constrained by Script is written SΔScript inside run.
type Script interface{ Text() string }

func run[S Script](v S) string { return v.Text() }

type SΔScript struct{}


// A generic interface's type parameter ΔT shares a scope with the converted interface's own ΔT.
type Box[ΔT any] interface{ Get() ΔT }

// Main is renamed ΔMain; ΔCopy is a fixed generated member; TΔshape is T's shape witness.
func Main()  {}
func ΔMain() {}

var ΔCopy int

type T struct{}
type TΔshape struct{}

// The generated local shapes: a shadowed x is renamed xΔ1, and a type switch binds t through a temporary Δt.
var xΔ1, Δt int

func k(v any) any {
	x := 1
	{
		x := 2
		_ = x
	}
	_ = x

	switch t := v.(type) {
	default:
		return t
	}
}
`

// markerGlyphMustConvert holds the Δ names that keep converting: Δ in numeric code (beside a plain local t), a bare Δ,
// a Δ parameter, Δ-led package variables, constants and functions, and non-marker letters.
const markerGlyphMustConvert = `package m

var Δt, Δx float64
var Δ int
var µ, π int

const ΔK = 1

func Δf() {}

// A bare Δ method is left alone by golib's method projections.
type Q2 struct{}

func (Q2) Δ() {}

// A function's type parameter ΔT is in no converted interface's scope.
func twice[ΔT any](v ΔT) [2]ΔT { return [2]ΔT{v, v} }

func h(Δv int) int { return Δv }

func g() float64 {
	t := 1.0
	return t + Δt
}

// A Δ-led field, method or type, and a bare Δ field: each is stamped [GoName], which golib reads before it drops a
// leading Δ.
type P struct{ Δt float64 }

func (P) ΔFoo() {}

type I interface{ ΔBar() }

type ΔMatrix struct{}

type P2 struct{ Δ int }
`

func TestMarkerGlyphIdentifiersAreRefused(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	load := func(source string) *markerGlyphRefusal {
		dir := t.TempDir()
		writeModuleFiles(t, dir, map[string]string{"go.mod": "module example.com/m\n\ngo 1.23\n", "m.go": source})
		return refuseMarkerGlyphIdentifiers(loadProductionForDir(t, dir))
	}

	refusal := load(markerGlyphFixture)

	if refusal == nil {
		t.Fatal("no identifier was refused")
	}

	got := map[string]string{}

	for _, site := range refusal.sites {
		name := strings.Fields(strings.TrimPrefix(site.text, "example.com/m."))[0]
		got[name] = site.text
	}

	want := map[string]string{
		"Ꮡx":            "contains Ꮡ (U+13D1)",
		"fᴛ":            "contains ᴛ (U+1D1B)",
		"maskᶜ":         "contains ᶜ (U+1D9C)",
		"ReaderжReader": "contains ж (U+0436)",
		"appendꓸꓸꓸ":     "contains ꓸ (U+A4F8)",
		"seedˢ":         "contains ˢ (U+02E2)",
		"Lockꞏtwin":     "contains ꞏ (U+A78F)",
		"Δslice":        "is the name go2cs gives this package's slice when it renames it",
		"ΔMain":         "is the name go2cs gives this package's Main when it renames it",
		"ΔCopy":         "is the name go2cs generates for the inline-array copy method",
		"TΔshape":       "shape witness go2cs generates for this package's type T",
		"Δt":            "is the temporary go2cs generates for the type switch binding t",
		"xΔ1":           "is the name go2cs gives a shadowed or repeated x",
		"ΔarrayΔ1":      "is the name go2cs gives a shadowed or repeated Δarray",
		"Δstrings":      "is the name go2cs gives the import or package alias strings when it renames it",
		"Δm":            "is the name go2cs gives the import or package alias m when it renames it",
		"SΔScript":      "is the witness type parameter go2cs generates for a type parameter S constrained by Script",
		"ΔT":            "is the name go2cs generates for the interface type parameter",
	}

	// The package variable Δt is refused for the type switch's temporary; the Δ-led field Δt converts.
	var deltaTSites []string

	for _, site := range refusal.sites {
		if strings.HasPrefix(site.text, "example.com/m.Δt ") {
			deltaTSites = append(deltaTSites, site.text)
		}
	}

	if len(deltaTSites) != 1 || !strings.Contains(deltaTSites[0], "is the temporary go2cs generates for the type switch binding t") {
		t.Errorf("want Δt refused once, as the type switch's temporary; got %q", deltaTSites)
	}

	for name, text := range want {
		if site, refused := got[name]; !refused {
			t.Errorf("%s was not refused", name)
		} else if !strings.Contains(site, text) {
			t.Errorf("%s was refused with %q, want it to say %q", name, site, text)
		}
	}

	for name, text := range got {
		if _, wanted := want[name]; !wanted {
			t.Errorf("%s was refused and must convert: %s", name, text)
		}
	}

	if refusal := load(markerGlyphMustConvert); refusal != nil {
		t.Fatalf("ordinary Δ and Greek names were refused: %v", refusal.sites)
	}
}

// A marker declared only in a test file refuses the -tests conversion before either half converts: the variant that
// carries the test file is refused, and the production package alone is not.
func TestMarkerGlyphInATestFileRefusesTheTestVariant(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod":        "module example.com/m\n\ngo 1.23\n",
		"m.go":          "package m\n\nfunc Answer() int { return 42 }\n",
		"m_test.go":     "package m\n\nimport \"testing\"\n\nfunc TestAnswer(t *testing.T) {\n\tvalueᴛ := Answer()\n\tif valueᴛ != 42 {\n\t\tt.Fatal(valueᴛ)\n\t}\n}\n",
		"ext_test.go":   "package m_test\n\nimport (\n\t\"testing\"\n\n\t\"example.com/m\"\n)\n\nfunc TestExternal(t *testing.T) {\n\tᏑa := m.Answer()\n\t_ = Ꮡa\n}\n",
		"plain_test.go": "package m\n",
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: true}, ".")

	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, dir)

	if production == nil {
		t.Fatal("no production package")
	}

	if refusal := refuseMarkerGlyphIdentifiers(production); refusal != nil {
		t.Fatalf("the production package declares no marker and was refused: %v", refusal.sites)
	}

	internal, external := findTestVariants(loaded, production)

	for label, variant := range map[string]*packages.Package{"internal": internal, "external": external} {
		if variant == nil {
			t.Fatalf("no %s test variant", label)
		}
	}

	if refusal := refuseMarkerGlyphIdentifiers(internal); refusal == nil || !strings.Contains(refusal.sites[0].text, "valueᴛ contains ᴛ") {
		t.Errorf("the internal test variant's valueᴛ was not refused: %v", refusal)
	}

	if refusal := refuseMarkerGlyphIdentifiers(external); refusal == nil || !strings.Contains(refusal.sites[0].text, "Ꮡa contains Ꮡ") {
		t.Errorf("the external test variant's Ꮡa was not refused: %v", refusal)
	}

	// The pre-check main runs before the production half is written reads the test files' tokens: one site per name.
	options := Options{targetPlatform: runtime.GOOS + "/" + runtime.GOARCH}
	early := refuseMarkerGlyphTestFiles(dir, options)

	if early == nil || len(early.sites) != 2 {
		t.Fatalf("want the pre-check to name valueᴛ and Ꮡa once each, got %v", early)
	}

	texts := early.sites[0].text + "\n" + early.sites[1].text

	for _, want := range []string{"example.com/m.valueᴛ contains ᴛ", "Ꮡa contains Ꮡ"} {
		if !strings.Contains(texts, want) {
			t.Errorf("the pre-check did not say %q: %s", want, texts)
		}
	}

	clean := t.TempDir()
	writeModuleFiles(t, clean, map[string]string{
		"go.mod":    "module example.com/m\n\ngo 1.23\n",
		"m.go":      "package m\n\nvar Δt float64\n\nfunc Answer() int { return 42 }\n",
		"m_test.go": "package m\n\nimport \"testing\"\n\nfunc TestAnswer(t *testing.T) {\n\tif Answer() != 42 || Δt != 0 {\n\t\tt.Fatal()\n\t}\n}\n",
	})

	if refusal := refuseMarkerGlyphTestFiles(clean, options); refusal != nil {
		t.Fatalf("test files with no marker letter (Δt is not one) were refused: %v", refusal.sites)
	}
}
