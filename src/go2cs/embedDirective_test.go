// embedDirective_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The `//go:embed` rules, one arm per rule. The whole point of resolving patterns in the CONVERTER
// rather than in the C# helper is that these are the parts that can be gotten subtly wrong, and
// here they can be armed red-first with no .NET on the critical path.
//
// The fixture below is embed/internal/embedtest's testdata shape, because that suite is what will
// judge this on a hardware lane and its TestHidden asserts the two directory listings verbatim.
package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// embedFixture builds embedtest's testdata shape in a temp directory and returns the package dir.
//
//	testdata/-not-hidden/fortune.txt   ← a LEADING DASH is not hidden; the arms need it to prove the
//	testdata/.hidden/fortune.txt         rule is about '.' and '_' and not about punctuation
//	testdata/.hidden/more/fortune.txt
//	testdata/.hidden/.more/fortune.txt
//	testdata/.hidden/_more/fortune.txt
//	testdata/_hidden/fortune.txt
//	testdata/ascii.txt  glass.txt  hello.txt  ken.txt
//	testdata/i/i18n.txt  testdata/i/j/k/k8s.txt
//	testdata/empty/                    ← no files: the empty-directory drop
func embedFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()

	for _, file := range []string{
		"testdata/-not-hidden/fortune.txt",
		"testdata/.hidden/fortune.txt",
		"testdata/.hidden/more/fortune.txt",
		"testdata/.hidden/.more/fortune.txt",
		"testdata/.hidden/_more/fortune.txt",
		"testdata/_hidden/fortune.txt",
		"testdata/ascii.txt",
		"testdata/glass.txt",
		"testdata/hello.txt",
		"testdata/ken.txt",
		"testdata/i/i18n.txt",
		"testdata/i/j/k/k8s.txt",
		"acvp_capabilities.json",
	} {
		path := filepath.Join(root, filepath.FromSlash(file))

		if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
			t.Fatalf("MkdirAll: %v", err)
		}

		if err := os.WriteFile(path, []byte("x"), 0644); err != nil {
			t.Fatalf("WriteFile: %v", err)
		}
	}

	if err := os.MkdirAll(filepath.Join(root, "testdata", "empty"), 0755); err != nil {
		t.Fatalf("MkdirAll: %v", err)
	}

	return root
}

// resolvedNames resolves the patterns and returns the entry names, in order.
func resolvedNames(t *testing.T, root string, patterns ...string) []string {
	t.Helper()

	entries, err := resolveEmbedPatterns(root, patterns)

	if err != nil {
		t.Fatalf("resolveEmbedPatterns(%v): %v", patterns, err)
	}

	names := make([]string, 0, len(entries))

	for _, entry := range entries {
		names = append(names, entry.Name)
	}

	return names
}

// ARM 1, THE DIRECTIVE READ. Every shape the corpus actually spells: indented (inside a
// parenthesized var block — 11 of embedtest's 16), attached to the SPEC's doc rather than the
// GenDecl's, spread over TWO directive lines, and quoted. A line-start scan of the GenDecl doc
// alone finds none of these, which is why the census by go/ast found 8 directives that
// `grep ^//go:embed` does not.
func TestTheDirectiveReadFindsEveryShapeTheCorpusSpells(t *testing.T) {
	// ⚠ THE DIRECTIVE KEYWORD IS COMPOSED AT RUNTIME and never spelled contiguously in this file.
	// embeddedAssets_test.go's guard scans converter SOURCE textually for `//go:embed` and requires
	// every hit to name an asset its C# twin (ConverterBuildInputs.cs) can resolve — it cannot tell
	// a fixture string from a real directive, and these patterns name nothing on disk. That is the
	// same false positive this seat's own census hit on go/build/read_test.go:302, which spells a
	// directive inside a raw string literal; there go/ast excluded it for free, here the guard is
	// textual by design and it is the FIXTURE that gives way, not the guard.
	const directive = "//go:" + "embed"

	source := strings.ReplaceAll(`package p

import "embed"

«D» plain.txt
var plain []byte

var (
	«D» "quoted one.txt" `+"`backquoted.txt`"+`
	«D» second/line.txt
	block embed.FS
)

«D»ded not-a-directive.txt
var notADirective []byte
`, "«D»", directive)

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "p.go", source, parser.ParseComments)

	if err != nil {
		t.Fatalf("ParseFile: %v", err)
	}

	got := map[string][]string{}

	for _, decl := range file.Decls {
		genDecl, ok := decl.(*ast.GenDecl)

		if !ok || genDecl.Tok != token.VAR {
			continue
		}

		for _, spec := range genDecl.Specs {
			valueSpec, ok := spec.(*ast.ValueSpec)

			if !ok {
				continue
			}

			got[valueSpec.Names[0].Name] = embedPatternsFromDocs(genDecl.Doc, valueSpec.Doc)
		}
	}

	if want := []string{"plain.txt"}; !embedNamesEqual(got["plain"], want) {
		t.Errorf("GenDecl-doc directive read %v, want %v", got["plain"], want)
	}

	// Indented, on the SPEC's doc, two lines, one quoted and one back-quoted.
	if want := []string{"quoted one.txt", "backquoted.txt", "second/line.txt"}; !embedNamesEqual(got["block"], want) {
		t.Errorf("block directive read %v, want %v", got["block"], want)
	}

	if len(got["notADirective"]) != 0 {
		t.Errorf("//go:embedded is not //go:embed, but read %v", got["notADirective"])
	}
}

// ARM 2, THE HIDDEN RULE FOR A DIRECTORY PATTERN. `//go:embed testdata` excludes every name
// beginning with '.' or '_' at every level below the named root. This is embedtest's TestHidden
// first list, and `-not-hidden/` is in it: the rule is about those two characters, not about
// punctuation generally.
func TestADirectoryPatternExcludesHiddenNames(t *testing.T) {
	names := resolvedNames(t, embedFixture(t), "testdata")

	for _, want := range []string{"testdata/", "testdata/-not-hidden/", "testdata/ascii.txt", "testdata/i/j/k/k8s.txt"} {
		if !embedHasName(names, want) {
			t.Errorf("%q missing from %v", want, names)
		}
	}

	for _, unwanted := range []string{"testdata/.hidden/", "testdata/_hidden/", "testdata/.hidden/fortune.txt", "testdata/_hidden/fortune.txt"} {
		if embedHasName(names, unwanted) {
			t.Errorf("%q must be excluded by a directory pattern; got %v", unwanted, names)
		}
	}
}

// ARM 3, THE ASYMMETRY — the rule most likely to be implemented as one rule and be wrong.
// `//go:embed testdata/*` matches `.hidden` and `_hidden` AT THE GLOB'S OWN LEVEL, but the walk
// BELOW each match is hidden-excluding again, so `.more` and `_more` inside `testdata/.hidden` stay
// out while `more` goes in. embedtest's TestHidden asserts both halves of exactly this.
func TestAGlobMatchesHiddenAtItsOwnLevelButNotBelowIt(t *testing.T) {
	names := resolvedNames(t, embedFixture(t), "testdata/*")

	for _, want := range []string{
		"testdata/.hidden/", "testdata/_hidden/", "testdata/-not-hidden/",
		"testdata/.hidden/fortune.txt", "testdata/.hidden/more/fortune.txt",
	} {
		if !embedHasName(names, want) {
			t.Errorf("%q must be matched AT the glob's level; got %v", want, names)
		}
	}

	for _, unwanted := range []string{"testdata/.hidden/.more/fortune.txt", "testdata/.hidden/_more/fortune.txt"} {
		if embedHasName(names, unwanted) {
			t.Errorf("%q is one level BELOW the match and must stay excluded; got %v", unwanted, names)
		}
	}
}

// ARM 4, `all:`. It ships as Go specifies it although NO corpus row reaches it (zero `all:`
// directives exist anywhere in GOROOT/src at 1.24.13) — refusing a valid directive would be a
// manufactured failure. This synthetic fixture is the only thing that will ever prove it.
func TestTheAllPrefixIncludesHiddenNamesAtEveryLevel(t *testing.T) {
	names := resolvedNames(t, embedFixture(t), "all:testdata")

	for _, want := range []string{
		"testdata/.hidden/fortune.txt", "testdata/_hidden/fortune.txt",
		"testdata/.hidden/.more/fortune.txt", "testdata/.hidden/_more/fortune.txt",
	} {
		if !embedHasName(names, want) {
			t.Errorf("all: must include %q at every level; got %v", want, names)
		}
	}

	plain := resolvedNames(t, embedFixture(t), "testdata")

	if len(names) <= len(plain) {
		t.Errorf("all:testdata (%d entries) must be a strict superset of testdata (%d)", len(names), len(plain))
	}
}

// ARM 5, THE EMPTY-DIRECTORY DROP. "Matches for empty directories are ignored" — `testdata/empty`
// exists on disk and contributes no entry, because no file's path passes through it.
func TestAnEmptyDirectoryContributesNoEntry(t *testing.T) {
	for _, pattern := range []string{"testdata", "testdata/*", "all:testdata"} {
		names := resolvedNames(t, embedFixture(t), pattern)

		if embedHasName(names, "testdata/empty/") {
			t.Errorf("pattern %q kept the empty directory: %v", pattern, names)
		}
	}
}

// ARM 6, THE ORDER, on embed.cs's OWN documented example. It is a contract, not tidiness: readDir
// binary-searches this list and needs a directory's contents to be one contiguous run. A plain
// lexical sort over the full names puts "q/s/t" between "q/s/" and "q/v" and the search then reads
// a short directory — which is why the arm compares against the exact documented sequence.
func TestEntriesAreOrderedByDirThenElem(t *testing.T) {
	files := map[string]string{"p": "p", "q/r": "r", "q/s/t": "t", "q/s/u": "u", "q/v": "v", "w": "w"}

	want := []string{"p", "q/", "w", "q/r", "q/s/", "q/v", "q/s/t", "q/s/u"}
	var got []string

	for _, entry := range embedEntriesInFSOrder(files) {
		got = append(got, entry.Name)
	}

	if !embedNamesEqual(got, want) {
		t.Errorf("order was\n  %v\nwant\n  %v", got, want)
	}

	// The lexical sort this must NOT be, stated so a later reader cannot "simplify" it back.
	if embedNamesEqual(got, []string{"p", "q/", "q/r", "q/s/", "q/s/t", "q/s/u", "q/v", "w"}) {
		t.Errorf("entries came back in plain lexical order, which breaks readDir's binary search")
	}
}

// ARM 7, THE SCALAR CASE AND THE REFUSALS. fips140test's shape — one pattern, one file at the
// package root — plus the patterns that must FAIL rather than silently yield an empty FS.
func TestASingleFilePatternResolvesAndBadPatternsFail(t *testing.T) {
	root := embedFixture(t)

	if names := resolvedNames(t, root, "acvp_capabilities.json"); !embedNamesEqual(names, []string{"acvp_capabilities.json"}) {
		t.Errorf("single-file pattern resolved %v", names)
	}

	// A SCALAR PATTERN IN A SUBDIRECTORY. Eight of embedtest's variants spell exactly this, and
	// every one of them was refused on the first emission read: the resolve also synthesizes the
	// `testdata/` marker, so the entry list is not a singleton even though the FILE set is. The
	// marker is an embed.FS concept and a string or []byte target must keep only its file.
	entries, err := resolveEmbedPatterns(root, []string{"testdata/hello.txt"})

	if err != nil {
		t.Fatalf("resolveEmbedPatterns: %v", err)
	}

	files := 0

	for _, entry := range entries {
		if !entry.IsDir {
			files++
		}
	}

	if files != 1 {
		t.Errorf("a single-file pattern must resolve ONE file, got %d in %v", files, entries)
	}

	if len(entries) == 1 {
		t.Fatalf("this arm only measures the scalar reduction while the resolve DOES synthesize the marker; it no longer does")
	}

	// THE REDUCTION ITSELF, which is what a scalar target keeps.
	scalar := embedScalarEntries(entries)

	if len(scalar) != 1 || scalar[0].IsDir || scalar[0].Name != "testdata/hello.txt" {
		t.Errorf("the scalar reduction kept %v, want just the file", scalar)
	}

	if got := embedScalarEntries([]embedEntry{{Name: "d/", IsDir: true}}); got != nil {
		t.Errorf("a marker-only list reduces to nothing, got %v", got)
	}

	for _, bad := range []string{"nothing-here.txt", "../escape.txt", "/absolute.txt", ".."} {
		if _, err := resolveEmbedPatterns(root, []string{bad}); err == nil {
			t.Errorf("pattern %q must fail, not yield an empty FS", bad)
		}
	}
}

// ARM 8, THE RESOURCE IDENTITY. The two halves must not share one, because under the recompile and
// white-box models they compile into ONE assembly and embedtest declares `global` in both.
func TestTheTwoHalvesGetDistinctResourceIdentities(t *testing.T) {
	production := embedResourcePrefix("embed/internal/embedtest", false)
	test := embedResourcePrefix("embed/internal/embedtest", true)

	if production != "go.embed/embed/internal/embedtest/" {
		t.Errorf("production prefix %q", production)
	}

	if test != "go.embed/embed/internal/embedtest_test/" {
		t.Errorf("test prefix %q", test)
	}

	if embedResourceName(production, "testdata/hello.txt") == embedResourceName(test, "testdata/hello.txt") {
		t.Errorf("one path in both halves must give two logical names, or the two `global` vars fight over one")
	}
}

// ARM 9, THE EMITTED INITIALIZER. Read as text, because the C# half is uncompiled on this box: what
// is gated here is that the converter hands the helper the ORDERED entry list, directory markers
// included, and the right prefix.
func TestTheInitializerCarriesTheOrderedEntryList(t *testing.T) {
	target := embedTarget{
		ImportPath: "crypto/internal/fips140test",
		TestHalf:   true,
		Entries:    []embedEntry{{Name: "acvp_capabilities.json", SourceOS: "/x/acvp_capabilities.json"}},
	}

	// The helper is named FULLY QUALIFIED, never through the bare `embed` alias — a blank import
	// (`_ "embed"`, which the spec requires for a string or []byte var) mints no alias, and
	// crypto/internal/fips140test is exactly that file. See the import-form arm below.
	got := embedInitializerExpr(embedKindString, target, "fips140test_package", "")
	want := RootNamespace + "." + getSanitizedImport("embed"+PackageSuffix) +
		`.ΔEmbedString(typeof(fips140test_package).Assembly, "go.embed/crypto/internal/fips140test_test/", "acvp_capabilities.json")`

	if got != want {
		t.Errorf("string initializer\n  %s\nwant\n  %s", got, want)
	}

	fs := embedTarget{ImportPath: "p", Entries: []embedEntry{{Name: "d/", IsDir: true}, {Name: "d/a.txt", SourceOS: "/x/d/a.txt"}}}

	if got := embedInitializerExpr(embedKindFS, fs, "p_package", ""); !strings.Contains(got, `["d/", "d/a.txt"]`) {
		t.Errorf("FS initializer must carry every entry IN ORDER, markers included: %s", got)
	}

	if got := embedInitializerExpr(embedKindBytes, fs, "p_package", "T"); !strings.Contains(got, "ΔEmbedBytes<T>(") {
		t.Errorf("a named byte element must reach the helper's type argument: %s", got)
	}
}

// ARM 10, THE ITEM LINES. A directory marker carries no resource, and a file two targets both name
// is ONE item — MSBuild refuses a duplicate EmbeddedResource, and the LogicalName is a function of
// the half and the path alone, so one item serves every variable naming it. embedtest reaches this:
// its `global` and `glass` both embed `testdata/g*.txt`.
func TestResourceItemsSkipDirectoriesAndDeduplicate(t *testing.T) {
	entry := embedEntry{Name: "testdata/glass.txt", SourceOS: filepath.Join("proj", "testdata", "glass.txt")}
	dir := embedEntry{Name: "testdata/", IsDir: true}

	items := embedResourceItemLines([]embedTarget{
		{ImportPath: "p", TestHalf: true, Entries: []embedEntry{dir, entry}},
		{ImportPath: "p", TestHalf: true, Entries: []embedEntry{entry}},
	})

	if got := strings.Count(items, "<EmbeddedResource"); got != 1 {
		t.Errorf("want ONE item for one file named by two targets, got %d:\n%s", got, items)
	}

	if strings.Contains(items, `LogicalName="go.embed/p_test/testdata/"`) {
		t.Errorf("a directory marker carries no bytes and must mint no resource:\n%s", items)
	}

	if !strings.Contains(items, `Include="testdata/glass.txt"`) {
		t.Errorf("the Include must be relative to the project file, forward-slashed:\n%s", items)
	}
}

// ARM 11, THE DIGEST INPUT — the hook the ruling required. An embedded payload need not live under
// `testdata`: fips140test embeds from the package root and traceviewer from `static/`, neither of
// which the fixture walk enumerates. Without them, editing an embedded file leaves a prior
// comparison looking valid while the emission still carries yesterday's bytes.
func TestEmbedPayloadsAreDigestInputs(t *testing.T) {
	root := embedFixture(t)
	entries, err := resolveEmbedPatterns(root, []string{"acvp_capabilities.json"})

	if err != nil {
		t.Fatalf("resolveEmbedPatterns: %v", err)
	}

	payloads := embedPayloadPaths([]embedTarget{{Entries: entries}})

	if len(payloads) != 1 || filepath.Base(payloads[0]) != "acvp_capabilities.json" {
		t.Fatalf("payload paths %v", payloads)
	}

	// The payload is NOT under testdata, so the fixture walk cannot be what covers it.
	fixtures, err := testFixturePaths(root)

	if err != nil {
		t.Fatalf("testFixturePaths: %v", err)
	}

	for _, fixture := range fixtures {
		if filepath.Base(fixture) == "acvp_capabilities.json" {
			t.Fatalf("the fixture walk already covers this payload, so this arm no longer measures the gap")
		}
	}

	// A directory marker has no on-disk path and must contribute none.
	if got := embedPayloadPaths([]embedTarget{{Entries: []embedEntry{{Name: "d/", IsDir: true}}}}); len(got) != 0 {
		t.Errorf("a directory marker is not a digest input: %v", got)
	}
}

func embedHasName(haystack []string, needle string) bool {
	for _, value := range haystack {
		if value == needle {
			return true
		}
	}

	return false
}

func embedNamesEqual(left, right []string) bool {
	if len(left) != len(right) {
		return false
	}

	for i := range left {
		if left[i] != right[i] {
			return false
		}
	}

	return true
}

// ARM 12, THE BLANK IMPORT — a CORPUS REGRESSION this seat's first cut introduced and the reason
// the initializer is fully qualified.
//
// The spec REQUIRES `import _ "embed"` when the variable is a string or []byte, because nothing in
// the file names the package. The converter mints no alias for a blank import, so an initializer
// spelled `embed.ΔEmbedBytes(…)` names an identifier that does not exist:
// crypto/internal/fips140test's acvp_test.cs went from compiling to CS0103 'embed' at (84,48). It
// is the ONE corpus instance — 22 files blank-import embed at 1.24.13 and 21 are under cmd/, which
// the converted corpus does not contain — and one is enough, because it is a package that compiled
// before this seat touched it.
//
// The fix is unconditional qualification rather than minting an alias the Go file never asked for:
// `go.embed_package` needs no import and cannot be shadowed. Both import forms are armed below, so
// the rule is ONE rule and a later reader cannot re-introduce the alias for the named-import case.
func TestTheInitializerIsQualifiedUnderEitherImportForm(t *testing.T) {
	isolateConverterPathGlobals(t)
	for _, form := range []struct {
		name    string
		imports string
		decl    string
	}{
		{"blank import, the []byte form the spec requires it for", "\t_ \"embed\"\n", "//go:embed data.txt\nvar payload []byte"},
		{"named import, the embed.FS form", "\t\"embed\"\n", "//go:embed data.txt\nvar payload embed.FS"},
	} {
		t.Run(form.name, func(t *testing.T) {
			dir := t.TempDir()

			writeModuleFiles(t, dir, map[string]string{
				"go.mod":        "module example/blankembed\n\ngo 1.23\n",
				"blankembed.go": "package blankembed\n\nfunc Use() {}\n",
				"data.txt":      "payload bytes\n",
				"blankembed_test.go": "package blankembed\n\nimport (\n" + form.imports + "\t\"testing\"\n)\n\n" +
					form.decl + "\n\nfunc TestPayload(t *testing.T) { _ = payload }\n",
			})

			internal, _ := loadTestVariantsForDir(t, dir)

			if internal == nil {
				t.Fatal("the internal test variant did not load")
			}

			outputPath := t.TempDir()
			options := testVariantOptions(Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}, testProjectRecompile, false, "bridge")

			testMethodRenames = make(map[types.Object]bool)
			defer func() { testMethodRenames = nil }()

			if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
				t.Fatalf("convertTestVariant: %v", err)
			}

			emitted := readConvertedAssembly(t, outputPath)

			if !strings.Contains(emitted, RootNamespace+"."+getSanitizedImport("embed"+PackageSuffix)+".ΔEmbed") {
				t.Errorf("the initializer must name %s.%s; emitted:\n%s", RootNamespace, getSanitizedImport("embed"+PackageSuffix), emitted)
			}

			// THE REGRESSION'S OWN SPELLING. A bare `embed.ΔEmbed…` compiles only where an alias
			// exists, which a blank import never provides.
			if strings.Contains(emitted, " embed.ΔEmbed") || strings.Contains(emitted, "=embed.ΔEmbed") || strings.Contains(emitted, "= embed.ΔEmbed") {
				t.Errorf("the initializer used the BARE alias, which a blank import does not mint; emitted:\n%s", emitted)
			}
		})
	}
}

// ARM 13, THE GROUPED SPEC — a regression comment-parity seats B and C introduced: visitGenDecl began
// passing each spec the doc it WRITES (its own doc in a `var ( … )` group, or the group doc merged
// with it) where the directive reads expected the GenDecl's doc. They also read valueSpec.Doc, so a
// grouped variable's `//go:embed` was read twice, two patterns for a []byte variable, and refused.
// The directive reads take the declaration's doc; the writer takes the spec's (visitValueSpec).
func TestAGroupedSpecReadsItsEmbedDirectiveOnce(t *testing.T) {
	isolateConverterPathGlobals(t)
	for _, form := range []struct {
		name string
		decl string
	}{
		{"first spec of a group", "var (\n\t//go:embed data.txt\n\tpayload []byte\n)"},
		{"later spec of a group", "var (\n\tother int\n\n\t//go:embed data.txt\n\tpayload []byte\n)"},
	} {
		t.Run(form.name, func(t *testing.T) {
			dir := t.TempDir()

			writeModuleFiles(t, dir, map[string]string{
				"go.mod":        "module example/groupembed\n\ngo 1.23\n",
				"groupembed.go": "package groupembed\n\nfunc Use() {}\n",
				"data.txt":      "payload bytes\n",
				"groupembed_test.go": "package groupembed\n\nimport (\n\t_ \"embed\"\n\t\"testing\"\n)\n\n" +
					form.decl + "\n\nfunc TestPayload(t *testing.T) { _ = payload }\n",
			})

			internal, _ := loadTestVariantsForDir(t, dir)

			if internal == nil {
				t.Fatal("the internal test variant did not load")
			}

			outputPath := t.TempDir()
			options := testVariantOptions(Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}, testProjectRecompile, false, "bridge")

			testMethodRenames = make(map[types.Object]bool)
			defer func() { testMethodRenames = nil }()

			if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
				t.Fatalf("convertTestVariant: %v", err)
			}

			emitted := readConvertedAssembly(t, outputPath)

			if !strings.Contains(emitted, RootNamespace+"."+getSanitizedImport("embed"+PackageSuffix)+".ΔEmbed") {
				t.Errorf("the grouped variable must take its embed initializer; emitted:\n%s", emitted)
			}
		})
	}
}
