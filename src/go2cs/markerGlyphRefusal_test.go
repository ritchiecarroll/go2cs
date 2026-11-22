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
	"sort"
	"strconv"
	"strings"
	"testing"
	"unicode"
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

// lettersInCSharp answers every non-ASCII letter in the C# sources under dir, comments removed: inside string
// literals when literals is true (what go2cs-gen writes), outside them otherwise (golib's own identifiers).
func lettersInCSharp(t *testing.T, dir string, literals bool) map[rune]string {
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
		var scanned []string

		if literals {
			scanned = csStringPattern.FindAllString(text, -1)
		} else {
			scanned = []string{csStringPattern.ReplaceAllString(text, `""`)}
		}

		for _, part := range scanned {
			for _, r := range part {
				if r > unicode.MaxASCII && unicode.IsLetter(r) {
					if _, seen := found[r]; !seen {
						found[r] = path
					}
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

	written := map[rune]bool{}

	for side, letters := range map[string]map[rune]string{"converter string literal": converter, "go2cs-gen string literal": generator, "golib identifier": golib} {
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

type slice struct{}
type Δslice struct{}

var Ꮡx int

func fᴛ() {}

const maskᶜ = 1

type ReaderжReader struct{}

func appendꓸꓸꓸ() {}

func g() string {
	seedˢ := "x"
	return seedˢ
}

type S struct{ a [2]int }

func (S) ΔClone() S { return S{} }

type T struct{}
type TΔshape struct{}

func Main()  {}
func ΔMain() {}

func Lockꞏtwin() {}

var Δt, Δx float64
var Δ int
var µ, π int

func h(Δv int) int { return Δv }
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
		"Δslice":        "is the name go2cs gives this package's slice when it renames it",
		"Ꮡx":            "contains Ꮡ (U+13D1)",
		"fᴛ":            "contains ᴛ (U+1D1B)",
		"maskᶜ":         "contains ᶜ (U+1D9C)",
		"ReaderжReader": "contains ж (U+0436)",
		"appendꓸꓸꓸ":     "contains ꓸ (U+A4F8)",
		"seedˢ":         "contains ˢ (U+02E2)",
		"ΔClone":        "is the name go2cs generates for the struct value-clone method",
		"TΔshape":       "shape witness go2cs generates for this package's type T",
		"ΔMain":         "is the name go2cs gives this package's Main when it renames it",
		"Lockꞏtwin":     "contains ꞏ (U+A78F)",
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

	// Δ in numeric code, a bare Δ, a Δ parameter and non-marker letters convert exactly as before.
	if refusal := load("package m\n\nvar Δt, Δx float64\nvar Δ int\nvar µ, π int\n\nfunc h(Δv int) int { return Δv }\n"); refusal != nil {
		t.Fatalf("ordinary Δ and Greek names were refused: %v", refusal.sites)
	}
}
