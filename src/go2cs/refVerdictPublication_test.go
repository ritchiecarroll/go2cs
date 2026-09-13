// refVerdictPublication_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards for the cross-package lowering contract's C0 increment (refVerdictPublication.go). The
// property under test is the one the design places on the DECLARING side because it is the side
// where a failure can be made loud: the set of `GoRefPrimary` records a package publishes equals
// the set of exported primaries its own selection decided on, in both directions, read from the
// selection map and go/types — never from emitted text, which cannot tell a value receiver's
// `[GoRecv] this ref T` from a pointer-receiver primary's.
//
// Every arm converts a real fixture package in-process through processConversion (the production
// driver, the same path -stdlib takes), so the guard measures the writer, the driver call and the
// reader together rather than any one of them in isolation.

package main

import (
	"go/build"
	"go/types"
	"os"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"testing"

	"go2cs/internal/stdlibmeta"
)

// refVerdictFixtureSource is a package with exactly one publishable primary: Inc is exported, on an
// exported struct, and fluent (`return c` — the S0 arm-(a) shape). bump is selected alike but
// unexported; hidden.Inc is exported on an UNEXPORTED type; Value is a value receiver, which the
// value path emits as `this ref` and which is never a primary. Use keeps every method live so the
// capture-mode pre-pass sees the calls.
const refVerdictFixtureSource = `package refpub

// Counter is the exported receiver type.
type Counter struct{ n int }

// Inc is exported, pointer-receiver and fluent: the one publishable primary.
func (c *Counter) Inc() *Counter {
	c.n++
	return c
}

// bump is selected like Inc but unexported: no foreign caller can bind it, so it is not published.
func (c *Counter) bump() *Counter {
	c.n += 2
	return c
}

// Value is a value receiver: the value path emits it as ` + "`this ref`" + `, which is not a primary.
func (c Counter) Value() int { return c.n }

type hidden struct{ n int }

// Inc on an unexported type is not publishable however it is spelled.
func (h *hidden) Inc() *hidden {
	h.n++
	return h
}

// Use keeps every method reachable.
func Use() int {
	c := &Counter{}
	c.Inc().bump()
	h := &hidden{}
	h.Inc()
	return c.Value()
}
`

// refVerdictFixture writes the fixture module and returns its package directory and the Options a
// production conversion of it takes (the trailingComments_test shape).
func refVerdictFixture(t *testing.T) (string, Options) {
	t.Helper()

	root := t.TempDir()
	pkgDir := filepath.Join(root, "refpub")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example/refpub\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "counter.go"), refVerdictFixtureSource)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	return pkgDir, options
}

// convertRefVerdictFixture converts the fixture with the dual-recv flags as given and returns the
// emitted package_info.cs lines (CR-stripped) and the conversion error, if any.
func convertRefVerdictFixture(t *testing.T, pkgDir string, outDir string, options Options, dualRecv bool) ([]string, error) {
	t.Helper()

	previousRecv, previousParams := dualRecvEnabled, dualRecvParamsEnabled
	dualRecvEnabled, dualRecvParamsEnabled = dualRecv, false

	defer func() { dualRecvEnabled, dualRecvParamsEnabled = previousRecv, previousParams }()

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		return nil, err
	}

	contents, err := os.ReadFile(filepath.Join(outDir, PackageInfoFileName))

	if err != nil {
		t.Fatalf("reading the emitted package_info.cs: %v", err)
	}

	return strings.Split(strings.ReplaceAll(string(contents), "\r", ""), "\n"), nil
}

// refVerdictRecordDiff compares two record sets and names what each side lacks — the checker the
// guard's equality arm rests on, positive-controlled below.
func refVerdictRecordDiff(published [][2]string, expected [][2]string) (missing []string, extra []string) {
	key := func(pair [2]string) string { return pair[0] + "." + pair[1] }

	publishedSet := map[string]bool{}
	expectedSet := map[string]bool{}

	for _, pair := range published {
		publishedSet[key(pair)] = true
	}

	for _, pair := range expected {
		expectedSet[key(pair)] = true
	}

	for k := range expectedSet {
		if !publishedSet[k] {
			missing = append(missing, k)
		}
	}

	for k := range publishedSet {
		if !expectedSet[k] {
			extra = append(extra, k)
		}
	}

	sort.Strings(missing)
	sort.Strings(extra)

	return missing, extra
}

func TestPublishedRefVerdictsMatchEmitted(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: converts a fixture package in-process")
	}

	pkgDir, options := refVerdictFixture(t)

	var flagsOnInfo []string

	t.Run("flags on: the published set equals the selection's exported set, both directions", func(t *testing.T) {
		outDir := filepath.Join(t.TempDir(), "out")

		lines, err := convertRefVerdictFixture(t, pkgDir, outDir, options, true)

		if err != nil {
			t.Fatalf("conversion failed: %v", err)
		}

		flagsOnInfo = lines

		// The selection map is the DECISION the records derive from; it still holds the fixture's
		// verdicts after the conversion returns (it is reset at the START of the next package).
		var pkgTypes *types.Package

		for fn := range packageRefReturnPrimaryMethods {
			if fn != nil && fn.Pkg() != nil && fn.Pkg().Path() == "example/refpub" {
				pkgTypes = fn.Pkg()
				break
			}
		}

		if pkgTypes == nil {
			t.Fatalf("the selection picked no primary in the fixture, so this arm would be vacuous — adjust the fixture until Counter.Inc is selected (map: %d entries)", len(packageRefReturnPrimaryMethods))
		}

		expected := publishableRefPrimaries(pkgTypes)
		published := parseRefPrimaryLines(lines)

		if len(expected) == 0 || len(published) == 0 {
			t.Fatalf("vacuous arm: expected %v, published %v", expected, published)
		}

		missing, extra := refVerdictRecordDiff(published, expected)

		if len(missing) != 0 || len(extra) != 0 {
			t.Errorf("published records differ from the selection's exported set: missing %v, extra %v\n%s", missing, extra, strings.Join(lines, "\n"))
		}

		// The fixture's own expectations, so the equality above cannot be satisfied by two wrong sets.
		want := map[string]bool{"Counter.Inc": true}
		never := []string{"Counter.bump", "hidden.Inc", "Counter.Value"}

		for _, pair := range published {
			k := pair[0] + "." + pair[1]
			delete(want, k)

			for _, n := range never {
				if k == n {
					t.Errorf("%s must never be published (unexported method, unexported type or value receiver)", n)
				}
			}
		}

		for k := range want {
			t.Errorf("%s is exported, on an exported type and fluent, and must be published", k)
		}

		if !containsLine(lines, refVerdictSectionStart) || !containsLine(lines, refVerdictSectionEnd) {
			t.Errorf("the section markers must surround the records:\n%s", strings.Join(lines, "\n"))
		}
	})

	t.Run("the checker names a missing and an extra record", func(t *testing.T) {
		published := [][2]string{{"Counter", "Inc"}, {"Counter", "Stray"}}
		expected := [][2]string{{"Counter", "Inc"}, {"Counter", "Dec"}}

		missing, extra := refVerdictRecordDiff(published, expected)

		if strings.Join(missing, ",") != "Counter.Dec" || strings.Join(extra, ",") != "Counter.Stray" {
			t.Fatalf("the checker must report exactly the missing and the extra record: missing %v, extra %v", missing, extra)
		}
	})

	t.Run("flags off: no primary is selected and no section is written at all", func(t *testing.T) {
		outDir := filepath.Join(t.TempDir(), "out")

		lines, err := convertRefVerdictFixture(t, pkgDir, outDir, options, false)

		if err != nil {
			t.Fatalf("conversion failed: %v", err)
		}

		if len(packageRefReturnPrimaryMethods) != 0 {
			t.Errorf("with -dual-recv off the selection must be empty (corpus-inert), got %d entries", len(packageRefReturnPrimaryMethods))
		}

		// Match the markers and the record prefix exactly: a substring such as "RefVerdicts" also
		// occurs in this test's own temp path, which the position-map records carry.
		for _, line := range lines {
			trimmed := strings.TrimSpace(line)

			if trimmed == refVerdictSectionStart || trimmed == refVerdictSectionEnd || strings.HasPrefix(trimmed, refPrimaryRecordPrefix) {
				t.Errorf("a package with nothing to publish must carry no trace of the section, found %q", line)
			}
		}

		// And the writer, handed an EMPTY set for the flags-on file, must produce exactly this
		// flags-off file: the section is removed whole (prose, markers, records, separator), so a
		// conversion that stops publishing is byte-identical to one that never did.
		if flagsOnInfo == nil {
			t.Skip("the flags-on arm did not run")
		}

		// The one line the FLAG itself legitimately moves is the position-map record: a `ref`
		// primary lays counter.cs out differently, so its C#-line-to-Go-line table differs between
		// the two conversions. Masked here — measured, not assumed: it was the only differing line.
		stripped := maskPositionMapRecords(applyRefVerdictSection(append([]string(nil), flagsOnInfo...), nil, false))
		flagsOff := maskPositionMapRecords(append([]string(nil), lines...))

		if strings.Join(stripped, "\n") != strings.Join(flagsOff, "\n") {
			t.Errorf("removing the section from the flags-on file must reproduce the flags-off file exactly (position-map records masked):\n--- stripped flags-on ---\n%s\n--- flags-off ---\n%s", strings.Join(stripped, "\n"), strings.Join(flagsOff, "\n"))
		}
	})

	t.Run("a registered hand-own primary publishes only when its declaration is on disk", func(t *testing.T) {
		key := refPrimaryHandOwnKey("example/refpub", "Counter", "Reset")
		refPrimaryHandOwns[key] = true

		defer delete(refPrimaryHandOwns, key)

		outDir := filepath.Join(t.TempDir(), "out")

		if err := os.MkdirAll(outDir, 0755); err != nil {
			t.Fatal(err)
		}

		implPath := filepath.Join(outDir, "counter_impl.cs")
		impl := "namespace go;\r\n\r\npartial class refpub_package {\r\n\r\n" +
			"[GoRecv] public static ref Counter Reset(this ref Counter c) { c.n = 0; return ref c; }\r\n\r\n}\r\n"

		if err := os.WriteFile(implPath, []byte(impl), 0644); err != nil {
			t.Fatal(err)
		}

		// Flags OFF: the selection contributes nothing, so the record can only come from the registry.
		lines, err := convertRefVerdictFixture(t, pkgDir, outDir, options, false)

		if err != nil {
			t.Fatalf("conversion with the declaration on disk failed: %v", err)
		}

		published := parseRefPrimaryLines(lines)

		if len(published) != 1 || published[0] != [2]string{"Counter", "Reset"} {
			t.Errorf("the registered, declared hand-own primary must be the ONLY record, got %v", published)
		}

		// Remove the declaration: the same registration must now be refused BY NAME.
		if err := os.Remove(implPath); err != nil {
			t.Fatal(err)
		}

		_, err = convertRefVerdictFixture(t, pkgDir, filepath.Join(t.TempDir(), "out"), options, false)

		if err == nil {
			t.Fatalf("a registered hand-own primary with no declaration on disk must fail the conversion, not publish a record the assembly does not declare")
		}

		if !strings.Contains(err.Error(), key) || !strings.Contains(err.Error(), "Reset(this ref Counter") {
			t.Errorf("the refusal must name the key and the declaration it looked for, got: %v", err)
		}
	})

	t.Run("the reader keys the record and the metadata extractor keeps it", func(t *testing.T) {
		record := formatRefPrimaryRecord("Counter", "Inc")

		previous := importedRefPrimaries
		importedRefPrimaries = HashSet[string]{}

		defer func() { importedRefPrimaries = previous }()

		loadRefPrimaryLines([]string{"// <RefVerdicts>", record, "// </RefVerdicts>"}, "refpub")

		if !importedRefPrimaries.Contains(refPrimaryRecordKey("refpub", "Counter", "Inc")) {
			t.Errorf("the reader must key the record as %q, set: %v", refPrimaryRecordKey("refpub", "Counter", "Inc"), importedRefPrimaries.Keys())
		}

		infoPath := filepath.Join(t.TempDir(), PackageInfoFileName)
		content := strings.Join([]string{
			"// <ExportedTypeAliases>",
			"// </ExportedTypeAliases>",
			"",
			"[assembly: GoImplement<refpub_package.Counter, fmt_package.Stringer>]",
			"",
			"// <RefVerdicts>",
			record,
			"// </RefVerdicts>",
			"",
			"namespace go;",
		}, "\r\n")

		if err := os.WriteFile(infoPath, []byte(content), 0644); err != nil {
			t.Fatal(err)
		}

		extracted, err := stdlibmeta.ExtractForTest(infoPath)

		if err != nil {
			t.Fatal(err)
		}

		// The two alias tags, the GoImplement record and the GoRefPrimary record: a NuGet-referenced
		// package publishes its primaries through the same embedded lines an on-disk file carries.
		if len(extracted) != 4 || extracted[3] != record {
			t.Errorf("the extractor must keep the GoRefPrimary record beside the two families it already carries, got %v", extracted)
		}
	})
}

// maskPositionMapRecords replaces every GoPositionMap record with a fixed placeholder, so two
// package_info files can be compared on everything but the C#-line tables their emissions produced.
func maskPositionMapRecords(lines []string) []string {
	for i, line := range lines {
		if strings.HasPrefix(strings.TrimSpace(line), "[assembly: go.GoPositionMap(") {
			lines[i] = "[assembly: go.GoPositionMap(<masked>)]"
		}
	}

	return lines
}

// containsLine reports whether any line equals want once trimmed.
func containsLine(lines []string, want string) bool {
	for _, line := range lines {
		if strings.TrimSpace(line) == want {
			return true
		}
	}

	return false
}
