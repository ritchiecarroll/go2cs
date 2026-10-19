// diagnosticCodeRegistry_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// THE GO2CS DIAGNOSTIC CODE REGISTRY GUARD.
//
// go2cs reports GO2CSnnnn codes from two sides, each with ONE registry:
//
//   - the converter: the const block in src/go2cs/diagnosticOutput.go, codes 1000-2999 (1xxx a package that did
//     not load cleanly, 2xxx an input the converter refuses);
//   - go2cs-gen: the DiagnosticDescriptor fields of src/gen/go2cs-gen/GeneratorDiagnostics.cs, codes 0001-0999
//     (a diagnostic in the C# build). No other go2cs-gen file constructs a descriptor.
//
// docs/DiagnosticCodes.md holds the one table that documents every code. Before this guard the generator codes
// were declared in three files (two descriptors inline beside their single use) and only the converter's codes
// had a table, so a new code could be minted anywhere, reuse a number, or ship undocumented, and nothing said so.
//
// The test fails BY NAME on each class: a code used in tracked src/ but registered on neither side; a
// DiagnosticDescriptor constructed outside GeneratorDiagnostics.cs; a code registered twice (within a side or
// across both); a code registered outside its side's range; a registered code with no table row; a table row
// for a code nobody registered; and a row whose side or severity disagrees with the registry.
//
// POSITIVE CONTROLS, verified when this guard landed (each planted in the real tree, named by
// TestGo2csDiagnosticCodeRegistry, then restored byte-identical): an unregistered code in a converter comment, a
// second generator descriptor reusing a converter code, a converter const at 0999, a generator descriptor in
// another file, a registered code with its row deleted, and a row for an unregistered code.
// TestGo2csDiagnosticCodeRegistryNamesEachClass controls every class without the tree.
//
// This file never spells a code literally (see diagnosticCode), so its own fixtures cannot reach the source scan.

const (
	converterRegistryPath = "src/go2cs/diagnosticOutput.go"
	generatorRegistryPath = "src/gen/go2cs-gen/GeneratorDiagnostics.cs"
	diagnosticCodesDoc    = "docs/DiagnosticCodes.md"

	converterSide = "converter"
	generatorSide = "go2cs-gen"
)

// diagnosticCodeUse is any GO2CSnnnn token: exactly four digits, so GO2CS_GOROUTINE_STACK and longer numbers
// are not codes.
var diagnosticCodeUse = regexp.MustCompile(`\bGO2CS[0-9]{4}\b`)

var generatorDescriptorEntry = regexp.MustCompile(`(?s)id:\s*"(GO2CS[0-9]{4})".*?defaultSeverity:\s*DiagnosticSeverity\.(\w+)`)

var descriptorConstruction = regexp.MustCompile(`new\s+DiagnosticDescriptor\s*\(|DiagnosticDescriptor\s+\w+\s*=\s*new\s*\(`)

var diagnosticCodeRow = regexp.MustCompile("^\\|\\s*`(GO2CS[0-9]{4})`\\s*\\|([^|]*)\\|([^|]*)\\|")

// diagnosticCode spells code n, so no literal code appears in this file.
func diagnosticCode(n int) string {
	return fmt.Sprintf("GO2CS%04d", n)
}

type registeredCode struct {
	code     string
	side     string
	severity string
	where    string
}

type codeOccurrence struct {
	code  string
	where string
}

type diagnosticCodeRowEntry struct {
	code     string
	side     string
	severity string
	where    string
}

type diagnosticCodeSnapshot struct {
	registered        []registeredCode
	descriptorOutside []string
	uses              []codeOccurrence
	rows              []diagnosticCodeRowEntry
}

// codeNumber is the four-digit number of a well-formed code.
func codeNumber(code string) int {
	n, _ := strconv.Atoi(strings.TrimPrefix(code, "GO2CS"))
	return n
}

// sideOfCode names the side whose range holds code, or "" for a number neither side may use.
func sideOfCode(code string) string {
	switch n := codeNumber(code); {
	case n >= 1 && n <= 999:
		return generatorSide
	case n >= 1000 && n <= 2999:
		return converterSide
	default:
		return ""
	}
}

// diagnosticCodeRegistryProblems is the whole rule, over a snapshot of the tree.
func diagnosticCodeRegistryProblems(s diagnosticCodeSnapshot) []string {
	var problems []string

	bySide := map[string]map[string]registeredCode{converterSide: {}, generatorSide: {}}
	firstAt := map[string]registeredCode{}

	for _, r := range s.registered {
		if prior, ok := firstAt[r.code]; ok {
			problems = append(problems, fmt.Sprintf("%s: %s is registered twice: first by the %s registry at %s", r.where, r.code, prior.side, prior.where))
		} else {
			firstAt[r.code] = r
		}

		if inRange := sideOfCode(r.code); inRange != r.side {
			rangeText := map[string]string{converterSide: "1000-2999", generatorSide: "0001-0999"}[r.side]
			problems = append(problems, fmt.Sprintf("%s: %s is outside the %s range %s", r.where, r.code, r.side, rangeText))
		}

		bySide[r.side][r.code] = r
	}

	for _, where := range s.descriptorOutside {
		problems = append(problems, fmt.Sprintf("%s: constructs a DiagnosticDescriptor outside %s; declare it there", where, generatorRegistryPath))
	}

	for _, use := range s.uses {
		side := sideOfCode(use.code)

		if side == "" {
			problems = append(problems, fmt.Sprintf("%s: %s is in neither side's range (0001-0999 go2cs-gen, 1000-2999 converter)", use.where, use.code))
			continue
		}

		if _, ok := bySide[side][use.code]; !ok {
			registry := map[string]string{converterSide: converterRegistryPath, generatorSide: generatorRegistryPath}[side]
			problems = append(problems, fmt.Sprintf("%s: %s is used but not registered in %s", use.where, use.code, registry))
		}
	}

	rowFor := map[string]diagnosticCodeRowEntry{}

	for _, row := range s.rows {
		if prior, ok := rowFor[row.code]; ok {
			problems = append(problems, fmt.Sprintf("%s: %s has a second table row (first at %s)", row.where, row.code, prior.where))
			continue
		}

		rowFor[row.code] = row
		registered, ok := firstAt[row.code]

		if !ok {
			problems = append(problems, fmt.Sprintf("%s: the table row for %s names a code neither registry declares", row.where, row.code))
			continue
		}

		if row.side != registered.side {
			problems = append(problems, fmt.Sprintf("%s: the table row for %s says side %q; it is registered by the %s", row.where, row.code, row.side, registered.side))
		}

		if !strings.EqualFold(row.severity, registered.severity) {
			problems = append(problems, fmt.Sprintf("%s: the table row for %s says severity %q; the registry says %q", row.where, row.code, row.severity, registered.severity))
		}
	}

	for code, r := range firstAt {
		if _, ok := rowFor[code]; !ok {
			problems = append(problems, fmt.Sprintf("%s: %s is registered but has no row in %s", r.where, code, diagnosticCodesDoc))
		}
	}

	sort.Strings(problems)

	return problems
}

// converterRegistry reads the const block's string values that are codes. Every code the converter prints is
// printed at "error" (canonicalDiagnostic), so that is each one's severity.
func converterRegistry(t *testing.T, root string) []registeredCode {
	t.Helper()

	path := filepath.Join(root, filepath.FromSlash(converterRegistryPath))
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, path, nil, 0)

	if err != nil {
		t.Fatalf("parse %s: %v", converterRegistryPath, err)
	}

	var codes []registeredCode

	for _, decl := range file.Decls {
		gen, ok := decl.(*ast.GenDecl)

		if !ok || gen.Tok != token.CONST {
			continue
		}

		for _, spec := range gen.Specs {
			for _, value := range spec.(*ast.ValueSpec).Values {
				lit, ok := value.(*ast.BasicLit)

				if !ok || lit.Kind != token.STRING {
					continue
				}

				text, err := strconv.Unquote(lit.Value)

				if err != nil || !diagnosticCodeUse.MatchString(text) || len(text) != len("GO2CS0000") {
					continue
				}

				codes = append(codes, registeredCode{text, converterSide, "Error", fmt.Sprintf("%s:%d", converterRegistryPath, fset.Position(lit.Pos()).Line)})
			}
		}
	}

	return codes
}

// generatorRegistry reads each descriptor's id and default severity.
func generatorRegistry(t *testing.T, root string) []registeredCode {
	t.Helper()

	data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(generatorRegistryPath)))

	if err != nil {
		t.Fatalf("read %s: %v", generatorRegistryPath, err)
	}

	content := string(data)
	var codes []registeredCode

	for _, at := range generatorDescriptorEntry.FindAllStringSubmatchIndex(content, -1) {
		line := strings.Count(content[:at[2]], "\n") + 1
		codes = append(codes, registeredCode{content[at[2]:at[3]], generatorSide, content[at[4]:at[5]], fmt.Sprintf("%s:%d", generatorRegistryPath, line)})
	}

	if constructions := len(descriptorConstruction.FindAllStringIndex(content, -1)); constructions != len(codes) {
		t.Fatalf("%s constructs %d descriptors but the reader found %d id/severity pairs; the reader no longer matches the file",
			generatorRegistryPath, constructions, len(codes))
	}

	return codes
}

// scannedForCodes are the tracked text files a code can be used in.
var scannedForCodes = map[string]bool{".go": true, ".cs": true, ".ps1": true, ".psm1": true, ".md": true, ".csproj": true, ".props": true, ".targets": true, ".json": true, ".txt": true}

func diagnosticCodeTreeSnapshot(t *testing.T, root string) (diagnosticCodeSnapshot, int) {
	t.Helper()

	snapshot := diagnosticCodeSnapshot{registered: append(converterRegistry(t, root), generatorRegistry(t, root)...)}
	scanned := 0

	for _, rel := range gitTrackedFiles(t, root) {
		if !strings.HasPrefix(rel, "src/") || !scannedForCodes[strings.ToLower(filepath.Ext(rel))] {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			continue // a tracked file deleted in the working tree is not this guard's question
		}

		scanned++

		for i, line := range strings.Split(string(data), "\n") {
			for _, code := range diagnosticCodeUse.FindAllString(line, -1) {
				snapshot.uses = append(snapshot.uses, codeOccurrence{code, fmt.Sprintf("%s:%d", rel, i+1)})
			}

			if strings.HasPrefix(rel, "src/gen/") && strings.HasSuffix(rel, ".cs") && rel != generatorRegistryPath &&
				descriptorConstruction.MatchString(line) {
				snapshot.descriptorOutside = append(snapshot.descriptorOutside, fmt.Sprintf("%s:%d", rel, i+1))
			}
		}
	}

	doc, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(diagnosticCodesDoc)))

	if err != nil {
		t.Fatalf("read %s: %v", diagnosticCodesDoc, err)
	}

	for i, line := range strings.Split(string(doc), "\n") {
		if match := diagnosticCodeRow.FindStringSubmatch(strings.TrimRight(line, "\r")); match != nil {
			snapshot.rows = append(snapshot.rows, diagnosticCodeRowEntry{match[1], strings.TrimSpace(match[2]), strings.TrimSpace(match[3]),
				fmt.Sprintf("%s:%d", diagnosticCodesDoc, i+1)})
		}
	}

	return snapshot, scanned
}

func TestGo2csDiagnosticCodeRegistry(t *testing.T) {
	root := repoRootFromPackageDir(t)
	snapshot, scanned := diagnosticCodeTreeSnapshot(t, root)

	sides := map[string]int{}

	for _, r := range snapshot.registered {
		sides[r.side]++
	}

	// A reader that matches nothing would make every check below pass vacuously.
	if sides[converterSide] == 0 || sides[generatorSide] == 0 || len(snapshot.rows) == 0 || len(snapshot.uses) == 0 {
		t.Fatalf("vacuous read: converter codes %d, go2cs-gen codes %d, table rows %d, uses %d",
			sides[converterSide], sides[generatorSide], len(snapshot.rows), len(snapshot.uses))
	}

	t.Logf("registered: converter %d, go2cs-gen %d; table rows %d; %d uses in %d tracked src/ files",
		sides[converterSide], sides[generatorSide], len(snapshot.rows), len(snapshot.uses), scanned)

	for _, problem := range diagnosticCodeRegistryProblems(snapshot) {
		t.Error(problem)
	}
}

// TestGo2csDiagnosticCodeRegistryNamesEachClass controls the rule without the tree: one defect of each class,
// each of which must be named, beside a clean code that must not be.
func TestGo2csDiagnosticCodeRegistryNamesEachClass(t *testing.T) {
	clean, twice, outOfRange, undocumented := diagnosticCode(10), diagnosticCode(11), diagnosticCode(999), diagnosticCode(1010)
	unregistered, rowOnly, neitherRange := diagnosticCode(2010), diagnosticCode(2011), diagnosticCode(3000)

	snapshot := diagnosticCodeSnapshot{
		registered: []registeredCode{
			{clean, generatorSide, "Error", "gen:1"},
			{twice, generatorSide, "Warning", "gen:2"},
			{twice, converterSide, "Error", "conv:1"},
			{outOfRange, converterSide, "Error", "conv:2"},
			{undocumented, converterSide, "Error", "conv:3"},
		},
		descriptorOutside: []string{"src/gen/Other.cs:5"},
		uses: []codeOccurrence{
			{clean, "src/a.go:1"},
			{unregistered, "src/b.go:2"},
			{neitherRange, "src/c.go:3"},
		},
		rows: []diagnosticCodeRowEntry{
			{clean, generatorSide, "Error", "doc:1"},
			{twice, generatorSide, "Error", "doc:2"},
			{outOfRange, converterSide, "Error", "doc:3"},
			{rowOnly, converterSide, "Error", "doc:4"},
		},
	}

	problems := strings.Join(diagnosticCodeRegistryProblems(snapshot), "\n")

	for _, want := range []string{
		twice + " is registered twice",
		outOfRange + " is outside the converter range",
		twice + " is outside the converter range",
		"src/gen/Other.cs:5: constructs a DiagnosticDescriptor outside",
		unregistered + " is used but not registered",
		neitherRange + " is in neither side's range",
		undocumented + " is registered but has no row",
		"the table row for " + rowOnly + " names a code neither registry declares",
		"the table row for " + twice + " says severity \"Error\"; the registry says \"Warning\"",
	} {
		if !strings.Contains(problems, want) {
			t.Errorf("not named: %q\nproblems:\n%s", want, problems)
		}
	}

	if strings.Contains(problems, clean+" ") {
		t.Errorf("the clean code %s was named:\n%s", clean, problems)
	}
}
