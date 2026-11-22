// oracleTestArgs_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// oracleTestArgs_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"encoding/json"
	"path/filepath"
	"slices"
	"strings"
	"testing"
	"time"
)

// TestOracleTestArgsCarryResolvedBuildTags guards the ORACLE HALF of the corpus flavour.
//
// The defect it locks out, measured on crypto/internal/fips140/nistec at 1.24.13: the conversion
// side loads packages under resolveBuildTags' answer (the corpus defaults purego,math_big_pure_go
// for every -stdlib/-tests run, whether or not -tags was passed), while this oracle ran `go test`
// with NO tags. The two sides therefore selected DIFFERENT FILE SETS -- p256_table_test.go is
// `//go:build ... || purego` and is selected only when tagged -- so the converted side ran and
// passed a test the oracle never compiled, and the comparison reported 44 entries of
// Go="" C#="pass". That reads as a corpus divergence and is nothing of the kind.
//
// The asymmetry was UNCONDITIONAL, not opt-in: commandLineOptions.go's resolveBuildTags applies the
// corpus defaults when -tags is absent, so omitting the flag did not avoid it.
func TestOracleTestArgsCarryResolvedBuildTags(t *testing.T) {
	// resolveBuildTags' OWN answer for a -tests run with no explicit -tags -- not a copy of the
	// expected tag list. A test that spelled the tags itself would go green against a converter
	// whose defaults had drifted, which is the failure mode the corpus cannot afford.
	resolved := resolveBuildTags(false, true, false, false, false, nil)
	if len(resolved) == 0 {
		t.Fatalf("precondition: a -tests run resolves to no build tags; this arm would be vacuous")
	}

	options := Options{testTimeout: 2 * time.Minute, buildTags: resolved}
	args := oracleTestArgs(options, "", "")

	want := "-tags=" + strings.Join(resolved, ",")
	if !slices.Contains(args, want) {
		t.Fatalf("the go test oracle does not carry the resolved build tags.\n"+
			"  want argument: %s\n  oracle args  : %v\n"+
			"  Every tag-gated test in every row reads as a false divergence without it.", want, args)
	}

	// The SAME renderer the conversion side uses, not a second spelling of it. conversionDriver,
	// stdLibConverter and this file's own packages.Load calls all take loaderBuildFlags(); if the
	// oracle rendered its own, the two could drift apart silently and the comparison would again be
	// measuring two different questions.
	loader := options.loaderBuildFlags()
	if len(loader) != 1 || loader[0] != want {
		t.Fatalf("oracle and loader renderings disagree: loader %v, oracle wants %s", loader, want)
	}

	// go test takes flags BEFORE the package argument; a -tags after the trailing "." is passed to
	// the compiled test binary instead and silently selects nothing.
	tagAt := slices.Index(args, want)
	dotAt := slices.Index(args, ".")
	if dotAt < 0 {
		t.Fatalf("oracle args carry no package argument: %v", args)
	}
	if tagAt > dotAt {
		t.Fatalf("-tags appears AFTER the package argument (%d > %d): %v", tagAt, dotAt, args)
	}
}

// TestOracleTestArgsUntaggedRunIsUnchanged is the zero arm. A conversion that resolves to no tags
// (a -recurse or single-file run, which stay tag-neutral by design) must produce the command line
// it produced before this fix existed -- no empty `-tags=` flag, which go test reads as a request
// for the tag set containing the empty string.
func TestOracleTestArgsUntaggedRunIsUnchanged(t *testing.T) {
	options := Options{testTimeout: 2 * time.Minute}
	args := oracleTestArgs(options, "", "")

	for _, a := range args {
		if strings.HasPrefix(a, "-tags") {
			t.Fatalf("an untagged run emitted a tag flag %q: %v", a, args)
		}
	}
}

// TestOracleTestArgsPreservesFilterAndSkip holds the two arguments that were already handed to both
// sides verbatim, so the extraction that made this list testable cannot have dropped one.
func TestOracleTestArgsPreservesFilterAndSkip(t *testing.T) {
	options := Options{testTimeout: 90 * time.Second, testFilter: "TestFoo", buildTags: []string{"purego"}}
	args := oracleTestArgs(options, "", "TestBar/sub")

	for _, want := range [][2]string{{"-run", "TestFoo"}, {"-skip", "TestBar/sub"}, {"-timeout", "1m30s"}} {
		i := slices.Index(args, want[0])
		if i < 0 || i+1 >= len(args) || args[i+1] != want[1] {
			t.Fatalf("oracle args lost %s %s: %v", want[0], want[1], args)
		}
	}
}

// TestOracleTestArgsDropMathBigPureGoExceptOnMathBig holds the one tag the oracle does not share with
// the conversion (oracleBuildFlags). On math/big's own row the oracle carries the loader's tags byte for
// byte, because there math_big_pure_go changes the test set. On any other row it carries the loader's
// tags less exactly that one, in the same order. An unknown package keeps the loader's tags.
func TestOracleTestArgsDropMathBigPureGoExceptOnMathBig(t *testing.T) {
	for _, recurse := range []bool{false, true} {
		resolved := resolveBuildTags(false, true, recurse, true, false, nil)
		if !slices.Contains(resolved, mathBigPureGoTag) {
			t.Fatalf("precondition: recurse=%v resolves to %v, without %s; this arm would be vacuous", recurse, resolved, mathBigPureGoTag)
		}

		options := Options{testTimeout: 2 * time.Minute, buildTags: resolved}
		loader := "-tags=" + strings.Join(resolved, ",")

		var others []string
		for _, tag := range resolved {
			if tag != mathBigPureGoTag {
				others = append(others, tag)
			}
		}
		dropped := "-tags=" + strings.Join(others, ",")

		for _, arm := range []struct{ packagePath, want string }{
			{"math/big", loader},
			{"math/big_test", loader},
			{"", loader},
			{"crypto/rsa", dropped},
			{"example.test/bigpull", dropped},
		} {
			args := oracleTestArgs(options, arm.packagePath, "")
			if !slices.Contains(args, arm.want) {
				t.Errorf("recurse=%v package %q: want %s\n  oracle args: %v", recurse, arm.packagePath, arm.want, args)
			}
		}

		// The rule leaves the options it was handed untouched: the conversion side reads the same slice.
		if strings.Join(options.buildTags, ",") != strings.Join(resolved, ",") {
			t.Fatalf("oracleTestArgs mutated the caller's build tags: %v", options.buildTags)
		}
	}
}

// TestOracleBuildTagsAreRecordedAndAnUnknownPathIsNamed: the comparison record says what the oracle ran with
// whenever that differs from the conversion (oracleBuildTags in the record's environment), and a run that cannot
// name its package keeps the tag with a note rather than silently.
func TestOracleBuildTagsAreRecordedAndAnUnknownPathIsNamed(t *testing.T) {
	resolved := resolveBuildTags(false, true, true, true, false, nil)
	options := Options{buildTags: resolved}

	if got := oracleBuildTagsRecord(options, "example.test/bigpull"); got == nil || slices.Contains(*got, mathBigPureGoTag) || len(*got) != len(resolved)-1 {
		t.Fatalf("a row other than math/big records the oracle's tags without %s: got %v from %v", mathBigPureGoTag, got, resolved)
	}

	for _, same := range []string{"math/big", "math/big_test", ""} {
		if got := oracleBuildTagsRecord(options, same); got != nil {
			t.Errorf("package %q: the oracle runs the conversion's own tags, so the record must omit them; got %v", same, got)
		}
	}

	if got := oracleBuildTagsRecord(Options{buildTags: []string{"purego"}}, "crypto/rsa"); got != nil {
		t.Errorf("without %s the oracle's tags are the conversion's; got %v", mathBigPureGoTag, got)
	}

	if _, note := oracleBuildTags(options, ""); !strings.Contains(note, "unknown") {
		t.Errorf("an unknown package path must say why the tag is kept; note %q", note)
	}

	for _, known := range []string{"math/big", "math/big_test", "crypto/rsa"} {
		if _, note := oracleBuildTags(options, known); note != "" {
			t.Errorf("package %q is known; no note expected, got %q", known, note)
		}
	}

	// The record field's JSON name, as a reader of a banked row finds it.
	data, err := json.Marshal(testEnvironmentRecord{OracleBuildTags: &[]string{"purego"}})
	if err != nil || !strings.Contains(string(data), `"oracleBuildTags":["purego"]`) {
		t.Fatalf("record JSON %s (%v) does not carry oracleBuildTags", data, err)
	}

	if data, _ := json.Marshal(testEnvironmentRecord{}); strings.Contains(string(data), "oracleBuildTags") {
		t.Fatalf("an unmoved oracle must not add the key: %s", data)
	}
}

// TestOracleBuildTagsRecordKeepsAnEmptyOracleSet: when math_big_pure_go is the conversion's ONLY tag, the oracle runs
// with no tags at all. The record must keep that apart from "the oracle ran the conversion's tags" through a JSON
// round trip, or a page re-rendered from the banked record claims Go ran with math_big_pure_go.
func TestOracleBuildTagsRecordKeepsAnEmptyOracleSet(t *testing.T) {
	options := Options{buildTags: []string{mathBigPureGoTag}}

	data, err := json.Marshal(testEnvironmentRecord{OracleBuildTags: oracleBuildTagsRecord(options, "crypto/rsa")})
	if err != nil {
		t.Fatal(err)
	}

	var banked testEnvironmentRecord
	if err := json.Unmarshal(data, &banked); err != nil {
		t.Fatal(err)
	}

	page := renderValidationProofPage(
		proofPageProvenance{importPath: "example.com/mod/sub", goVersion: "1.26.11", platform: "linux/amd64",
			buildTags: options.buildTags, convertedPath: "src/example.com/mod/sub"},
		testComparison{Package: "example.com/mod/sub", Status: "validated", Matched: true, Go: map[string]string{"TestA": "pass"},
			CSharp: map[string]string{"TestA": "pass"}, Environment: banked},
		nil, nil)

	want := "Both sides were built with no build tags; the conversion also takes `" + mathBigPureGoTag + "`"
	if !strings.Contains(page, want) {
		t.Fatalf("the record %s lost the empty oracle tag set; the page must read %q:\n%s", data, want, page)
	}
}

// -test-oracle-vet-off (COORD's ruling on go-difflib v1.0.0, 2026-11-22): when Go's OWN `go test` fails at vet, not
// at a test, the oracle may run with -vet=off. Never a default; the comparison is still test by test, against the
// -vet=off run; and the vet findings are disclosed mechanically, by analyzer, on the proof page and in MODULE.md.
func TestOracleTestArgsTurnVetOffOnlyWhenAsked(t *testing.T) {
	options := Options{testTimeout: 2 * time.Minute}

	if args := oracleTestArgs(options, "", ""); slices.ContainsFunc(args, func(arg string) bool { return strings.HasPrefix(arg, "-vet") }) {
		t.Fatalf("the default oracle sets vet: %v", args)
	}

	options.testOracleVetOff = true

	if args := oracleTestArgs(options, "", ""); !slices.Contains(args, "-vet=off") {
		t.Fatalf("-test-oracle-vet-off does not reach the oracle: %v", args)
	}
}

// The findings come from `go vet -json` over go test's own vet subset, run where the oracle runs, under its tags:
// each as "<analyzer>: <file>:<line>:<col>: <message>", the file named relative to the package, never by a machine
// path. A package go test's vet passes has none, and the caller refuses the option for it.
func TestOracleVetFindingsNameTheAnalyzerAndTheSite(t *testing.T) {
	dir := t.TempDir()
	writeModuleFile(t, filepath.Join(dir, "go.mod"), "module example.test/vetted\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(dir, "one.go"), "package vetted\n\nfunc One() int { return 1 }\n")
	writeModuleFile(t, filepath.Join(dir, "one_test.go"),
		"package vetted\n\nimport \"fmt\"\n\nfunc ExampleTwo() {\n\tfmt.Println(One())\n\t// Output: 1\n}\n")

	t.Setenv("GOWORK", "off")
	t.Setenv("GOFLAGS", "")

	findings, err := oracleVetFindings(dir, Options{}, "example.test/vetted")

	if err != nil {
		t.Fatal(err)
	}

	want := []string{"tests: one_test.go:5:1: ExampleTwo refers to unknown identifier: Two"}

	if !slices.Equal(findings, want) {
		t.Fatalf("findings:\n  want %q\n  got  %q", want, findings)
	}

	// The same package with the example fixed passes go test's vet: no findings.
	writeModuleFile(t, filepath.Join(dir, "one_test.go"),
		"package vetted\n\nimport \"fmt\"\n\nfunc ExampleOne() {\n\tfmt.Println(One())\n\t// Output: 1\n}\n")

	if findings, err := oracleVetFindings(dir, Options{}, "example.test/vetted"); err != nil || len(findings) != 0 {
		t.Fatalf("a package vet passes: want no findings, got %q (%v)", findings, err)
	}
}

// The disclosure round-trips: the proof page states each finding, and MODULE.md, which a module pack ships, names
// them by package.
func TestOracleVetOffIsDisclosedOnThePageAndInModuleMD(t *testing.T) {
	finding := "tests: difflib_test.go:105:1: ExampleGetUnifiedDiffCode refers to unknown identifier: GetUnifiedDiffCode"
	comparison := testComparison{Package: "example.test/m/p", Status: "matched", Matched: true,
		Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"}, OracleVetOff: []string{finding}}
	page := renderValidationProofPage(proofPageProvenance{importPath: "example.test/m/p", goVersion: "1.26.11", platform: "linux/amd64",
		date: "2026-11-22", convertedPath: "src/example.test/m/p"}, comparison, nil, nil)

	if !strings.Contains(page, "\n## Go's tests ran without vet\n") || !strings.Contains(page, "- "+finding+"\n") {
		t.Fatalf("the proof page does not disclose the oracle's -vet=off with its finding:\n%s", page)
	}

	if got := proofOracleVetOff(page); !slices.Equal(got, []string{finding}) {
		t.Fatalf("read back: want [%s], got %q", finding, got)
	}

	moduleDir := filepath.Join(t.TempDir(), "example.test", "m")
	writeModuleFile(t, filepath.Join(moduleDir, "p", "p.md"), page)

	if err := writeThirdPartyModuleSummary(moduleDir, "example.test/m"); err != nil {
		t.Fatal(err)
	}

	// Written with CRLF line ends (writeStableDocFile), read here as lines.
	summary := strings.ReplaceAll(readGenerated(t, filepath.Join(moduleDir, "MODULE.md")), "\r", "")

	if !strings.Contains(summary, "\n## Go's tests ran without vet\n") || !strings.Contains(summary, "- `example.test/m/p`: "+finding+"\n") {
		t.Fatalf("MODULE.md does not name the package's -vet=off oracle and its finding:\n%s", summary)
	}

	// A page without the option renders no such section, so every existing page is unchanged.
	comparison.OracleVetOff = nil

	if page := renderValidationProofPage(proofPageProvenance{importPath: "example.test/m/p", goVersion: "1.26.11", platform: "linux/amd64",
		date: "2026-11-22", convertedPath: "src/example.test/m/p"}, comparison, nil, nil); strings.Contains(page, "without vet") {
		t.Fatalf("a page without the option mentions vet:\n%s", page)
	}
}
