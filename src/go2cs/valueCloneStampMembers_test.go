// valueCloneStampMembers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// THE CLASS THIS GUARDS, and it cost a build on 2026-09-08.
//
// go2cs-gen's TypeGenerator reads a [GoValueClone(...)] member list and emits a member ACCESS for
// each name. A name in the stamp with no matching declaration in the stamped type is therefore
// CS1061 at build time — "does not contain a definition for 'X'" — reported against GENERATED code,
// so the error names a .g.cs file and the defect is in a hand-written one.
//
// It reached the H5 ladder as `[GoValueClone("tls","createstack","Δtrace",…)] partial struct m`
// whose field is declared `trace`: the hand-own had been re-derived from an emission produced
// BEFORE a60eb2274 (the converter cut that spells a stamped field as the DECLARATION does), so the
// stamp carried the pre-fix Δ-prefixed spelling while the field carried the post-fix one.
//
// WHY A GUARD AND NOT A CENSUS. A frozen [module: GoManualConversion] file stops receiving converter
// improvements, so this can only drift further; and the mismatch is invisible to every
// release-agnostic instrument (G's H6 alias census asks whether the Go PACKAGE moved — the package
// here never moved, and nothing about GOROOT can see a C# stamp disagreeing with a C# field).
//
// ⚠ IT IS AN INTERNAL-CONSISTENCY CHECK ON PURPOSE. It compares the stamp against the declaration
// BESIDE IT and never against an emission, which is what makes it exact and gives it no dependency
// on a built tree, a converted corpus, or a toolchain.
//
// ⚠ IT DOES NOT DUPLICATE valueCloneFieldSpelling_test.go, and the difference is the whole reason it
// exists. That guard unit-tests the CONVERTER's spelling functions against types.Var, so it
// guarantees correct EMISSION from a60eb2274 forward. This one scans files the converter NEVER
// REWRITES — a [module: GoManualConversion] hand-own is frozen, so a stamp that was correct when the
// file was minted stays wrong forever once the rule beneath it changes. Neither can see the other's
// class.
//
// ⚠ POPULATION, STATED BECAUSE IT IS THIN: at the 1.23.12 corpus this scans ~145 marked files and
// finds ONE stamp carrying ONE member name. It is therefore one rename away from vacuous TODAY, and
// the vacuity arm below fires only at zero. On the 1.24 tree the population is 4 stamps / 13 names
// (the release adds stamped types), so the guard THICKENS at the hop rather than thinning. Anyone
// tempted to delete it as low-value should read that second number first.

var (
	handOwnMarkerRe = regexp.MustCompile(`(?m)^[\t ]*\[module:[\t ]*(?:go\.)?GoManualConversion\]`)
	valueCloneRe    = regexp.MustCompile(`\[GoValueClone\(([^)]*)\)\][^\n]*?partial\s+(?:struct|class)\s+([\p{L}_][\p{L}\p{N}_]*)`)
	quotedNameRe    = regexp.MustCompile(`"([^"]+)"`)
)

// stampFinding is one stamped member with no declaration in the type it stamps.
type stampFinding struct {
	File   string
	Type   string
	Member string
}

func (f stampFinding) String() string {
	return fmt.Sprintf("%s: [GoValueClone] on %q names %q, which that type does not declare", f.File, f.Type, f.Member)
}

// bracedBodyAt returns the brace-balanced body beginning at the first '{' at or after `from`.
// Returning "" means no body was found, and a caller must SKIP rather than treat that as a finding:
// an unparsed declaration is the scanner failing to read, not the file failing to declare.
func bracedBodyAt(text string, from int) string {
	i := strings.Index(text[from:], "{")
	if i < 0 {
		return ""
	}
	i += from
	depth := 0
	for j := i; j < len(text); j++ {
		switch text[j] {
		case '{':
			depth++
		case '}':
			depth--
			if depth == 0 {
				return text[i : j+1]
			}
		}
	}
	return ""
}

// declaresMember reports whether `body` declares a field or property named `name`. The name is
// matched only where a declaration can begin — after whitespace or a type-closing character — and
// only where a declaration can continue, which keeps it from matching the name inside a longer
// identifier or inside a call.
func declaresMember(body, name string) bool {
	re := regexp.MustCompile(`(^|[\s<>\]\)])` + regexp.QuoteMeta(name) + `\s*(;|=[^=]|=>|\{)`)
	return re.MatchString(body)
}

// scanValueCloneStamps returns every stamped member that its own type does not declare, over the
// hand-owned files under root. Files without the marker are skipped: a CONVERTED file's stamp is
// the converter's own output and is the converter's to get right, which a60eb2274 already guards.
func scanValueCloneStamps(root string) (findings []stampFinding, files, stamps, members int, err error) {
	walkErr := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			switch info.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}
			return nil
		}
		if !strings.HasSuffix(path, ".cs") {
			return nil
		}
		raw, readErr := os.ReadFile(path)
		if readErr != nil {
			return readErr
		}
		text := string(raw)
		if !handOwnMarkerRe.MatchString(text) {
			return nil
		}
		files++
		rel, relErr := filepath.Rel(root, path)
		if relErr != nil {
			rel = path
		}
		for _, m := range valueCloneRe.FindAllStringSubmatchIndex(text, -1) {
			stamps++
			args := text[m[2]:m[3]]
			typeName := text[m[4]:m[5]]
			body := bracedBodyAt(text, m[1])
			if body == "" {
				continue
			}
			for _, nm := range quotedNameRe.FindAllStringSubmatch(args, -1) {
				members++
				if !declaresMember(body, nm[1]) {
					findings = append(findings, stampFinding{File: filepath.ToSlash(rel), Type: typeName, Member: nm[1]})
				}
			}
		}
		return nil
	})
	sort.Slice(findings, func(i, j int) bool { return findings[i].String() < findings[j].String() })
	return findings, files, stamps, members, walkErr
}

// TestValueCloneStampMembersAreDeclared is the guard.
//
// ⚠ IT READS FILES OUTSIDE THIS MODULE (src\core), and cmd/go DROPS out-of-module files from the
// test input hash — so a cached PASS would survive a reintroduction. Run the converter suite with
// -count=1, which is what every gate in this repo already does, and which the neighbouring fleet
// identifier census carries the same warning about.
func TestValueCloneStampMembersAreDeclared(t *testing.T) {
	root := repoRootFromPackageDir(t)
	core := filepath.Join(root, "src", "core")
	if _, err := os.Stat(core); err != nil {
		t.Fatalf("converted corpus not found at %s: %v", core, err)
	}

	findings, files, stamps, members, err := scanValueCloneStamps(core)
	if err != nil {
		t.Fatalf("scanning %s: %v", core, err)
	}

	// A zero that could not have been anything else is not a measurement. The population is small
	// (a handful of stamps across the marked set), so assert it is NON-EMPTY: if a rename or a
	// marker change empties it, this guard goes quietly vacuous exactly when it stops protecting
	// anything, and that is the failure mode worth catching here.
	if stamps == 0 || members == 0 {
		t.Fatalf("VACUOUS: scanned %d hand-owned files and found %d stamps / %d member names; "+
			"the guard cannot fail in this state — check the marker and [GoValueClone] spellings", files, stamps, members)
	}
	t.Logf("hand-owned files %d, [GoValueClone] stamps %d, member names %d", files, stamps, members)

	if len(findings) > 0 {
		var b strings.Builder
		for _, f := range findings {
			b.WriteString("\n  " + f.String())
		}
		t.Fatalf("%d stamped member(s) are not declared by the type they stamp — go2cs-gen emits a "+
			"member access for each, so every one is a CS1061 at build time:%s", len(findings), b.String())
	}
}

// TestValueCloneStampScannerFiresAndAdmits is the positive control, in BOTH directions: a planted
// mismatch must be reported, and a planted CORRECT stamp must not be. An admit-only control cannot
// fail on a dead scanner, and a fires-only control cannot fail on one that refuses everything.
//
// The mismatch planted is the REAL historical one — a Δ-prefixed stamp over an unprefixed field —
// rather than an invented shape, so the control also pins the glyph handling the finding depends on.
func TestValueCloneStampScannerFiresAndAdmits(t *testing.T) {
	dir := t.TempDir()

	write := func(name, body string) {
		t.Helper()
		if err := os.WriteFile(filepath.Join(dir, name), []byte(body), 0o600); err != nil {
			t.Fatalf("planting %s: %v", name, err)
		}
	}

	// BAD: the stamp names Δtrace, the field is trace.
	write("bad.cs", "[module: go.GoManualConversion]\n"+
		"namespace go;\n"+
		"partial class runtime_package {\n"+
		"[GoType] [GoValueClone(\"tls\", \"Δtrace\")] partial struct m {\n"+
		"    internal ж<tls> tls;\n"+
		"    internal mTraceState trace;\n"+
		"}\n"+
		"}\n")

	// GOOD: same shape, spelled as the declaration is.
	write("good.cs", "[module: go.GoManualConversion]\n"+
		"namespace go;\n"+
		"partial class runtime_package {\n"+
		"[GoType] [GoValueClone(\"tls\", \"trace\")] partial struct m {\n"+
		"    internal ж<tls> tls;\n"+
		"    internal mTraceState trace;\n"+
		"}\n"+
		"}\n")

	// UNMARKED: a converted file with the same mismatch must be IGNORED — the converter owns its
	// own emission and a60eb2274 guards it; this guard is about frozen hand-owns only.
	write("unmarked.cs", "namespace go;\n"+
		"partial class runtime_package {\n"+
		"[GoType] [GoValueClone(\"Δtrace\")] partial struct m2 {\n"+
		"    internal mTraceState trace;\n"+
		"}\n"+
		"}\n")

	findings, files, stamps, members, err := scanValueCloneStamps(dir)
	if err != nil {
		t.Fatalf("scanning the planted tree: %v", err)
	}
	if files != 2 {
		t.Fatalf("expected 2 MARKED files scanned (the unmarked plant must be skipped), got %d", files)
	}
	if stamps != 2 || members != 4 {
		t.Fatalf("expected 2 stamps / 4 member names across the marked plants, got %d / %d", stamps, members)
	}
	if len(findings) != 1 {
		t.Fatalf("expected exactly 1 finding (bad.cs only), got %d: %v", len(findings), findings)
	}
	got := findings[0]
	if got.File != "bad.cs" || got.Type != "m" || got.Member != "Δtrace" {
		t.Fatalf("the finding does not name the planted mismatch: %+v", got)
	}
}
