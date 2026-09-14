// valueCloneStampMembers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

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
// ⚠ POPULATION, STATED BECAUSE IT IS THIN AND BECAUSE THIS PARAGRAPH WAS ONCE WRONG. At the 1.23.12
// corpus this scanned ~145 marked files and found ONE stamp carrying ONE member name, in
// internal/concurrent/hashtriemap_whitebox.cs. It was therefore one rename from vacuous, and the H5
// hop was that rename: a ruling deleted that file (its principal was hashtriemap_test.go's
// dumpMap/dumpNode, absent at 1.24) and the population went to ZERO.
//
// ⚠ THIS PARAGRAPH USED TO SAY "on the 1.24 tree the population is 4 stamps / 13 names (the release
// adds stamped types), so the guard THICKENS at the hop rather than thinning." THAT WAS A PREDICTION
// WEARING THE CLOTHES OF A MEASUREMENT, it was repeated downstream as a reason to accept the vacuity
// as temporary, and it is WITHDRAWN. It was refutable without measuring anything: a reconvert writes
// AUTO files and PRESERVES marked ones, so the hand-own population cannot grow by conversion at all --
// only a human adds the marker. Measured after the hop, the intersection reads 1 / 0 / 0 across the
// checkpoint, the relocation and the reconverted tree, while stamps corpus-wide are healthy (129
// files). The guard is in a KNOWN vacuity, which the arm below now PASSES with all counts printed.
//
// Anyone tempted to delete it as low-value should note what it still catches while empty: a broken
// marker and a drifted stamp spelling both take a corpus-wide count to zero and refuse.

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

// countStampedFiles counts every .cs under root carrying a [GoValueClone] stamp, MARKED OR NOT.
//
// It answers a different question from scanValueCloneStamps and that is its whole purpose: that scan
// asks "which hand-owns disagree with their own declarations", this one asks "can the stamp spelling
// still be seen anywhere at all". The second is what tells an EMPTY intersection apart from a DEAD
// regex -- the two states the old vacuity arm could not distinguish, and it refused on both.
func countStampedFiles(root string) (int, error) {
	stamped := 0
	err := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
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
		if valueCloneRe.Match(raw) {
			stamped++
		}
		return nil
	})
	return stamped, err
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

	// ⚠ WHAT VACUITY MEANS HERE, RE-RULED (d2ad84bdb) AFTER THIS ARM FIRED ON A CORRECT TREE.
	//
	// The original arm refused whenever the marked set carried no stamp, on the reasoning that a zero
	// which could not have been anything else is not a measurement. That is right about an INSTRUMENT
	// and wrong about a POPULATION, and the difference cost a red gate:
	//
	//   - the population was ever exactly ONE file, internal/concurrent/hashtriemap_whitebox.cs,
	//     carrying ONE stamp, on Δindirect<K, V>;
	//   - a ruling correctly DELETED it at the H5 hop -- its principal was hashtriemap_test.go's
	//     dumpMap/dumpNode, and the 1.24 test has neither -- so the intersection went legitimately
	//     empty;
	//   - and NO RECONVERT COULD EVER REFILL IT. A reconvert writes AUTO files and PRESERVES marked
	//     ones; only a human adds [module: GoManualConversion]. The hand-own population therefore
	//     cannot grow by conversion, which refutes this header's former "4 stamps / 13 names at 1.24"
	//     without measuring anything. That figure was a PREDICTION that did not survive measurement
	//     (three trees, intersection 0, 1, 0) and it is withdrawn.
	//
	// So the refusal now tests the two things that can actually BREAK -- that there are hand-owns to
	// scan at all, and that the stamp spelling still matches somewhere in the corpus -- and an empty
	// INTERSECTION of the two passes, with every count printed so the state is read rather than
	// inferred. A KNOWN vacuity is not a corpus defect. An unreadable marker or a drifted stamp
	// spelling is, and those two counts are exactly what still catches them.
	corpusStamped, corpusErr := countStampedFiles(core)
	if corpusErr != nil {
		t.Fatalf("counting stamped files under %s: %v", core, corpusErr)
	}
	if files == 0 || corpusStamped == 0 {
		t.Fatalf("VACUOUS: the MACHINERY cannot be shown to work — %d hand-owned file(s) scanned, "+
			"%d stamped file(s) corpus-wide. A zero on either side means the [module: GoManualConversion] "+
			"marker or the [GoValueClone] spelling stopped matching, which is a real defect. An empty "+
			"INTERSECTION of the two is not, and passes.", files, corpusStamped)
	}

	t.Logf("hand-owned files %d, stamped files corpus-wide %d, stamps inside hand-owns %d, member names %d",
		files, corpusStamped, stamps, members)

	if stamps == 0 {
		t.Logf("KNOWN VACUITY: no hand-own currently carries a [GoValueClone] stamp, so this run " +
			"compared nothing. Both counts above are non-zero, so the machinery is live and the " +
			"intersection is simply empty — the last stamped hand-own left the population under a " +
			"ruling, not by drift. The guard re-arms itself the day a re-derive puts a stamp back " +
			"inside a marked file.")
	}

	if len(findings) > 0 {
		var b strings.Builder
		for _, f := range findings {
			b.WriteString("\n  " + f.String())
		}
		t.Fatalf("%d stamped member(s) are not declared by the type they stamp — go2cs-gen emits a "+
			"member access for each, so every one is a CS1061 at build time:%s", len(findings), b.String())
	}
}

// TestValueCloneVacuityArmsCanReachZero controls the RELAXED refusal, which is the part of this file
// most likely to be quietly dead.
//
// Loosening a vacuity arm trades one failure mode for another: the old arm refused too eagerly, and
// the obvious fix — refuse on two counts instead of one — is worthless if neither count can ever be
// zero. So this plants the two states the refusal exists for and asserts each count actually reaches
// its trigger, independently. Without this, "the guard passes on an empty intersection" and "the
// guard cannot fail at all" look identical from the outside.
func TestValueCloneVacuityArmsCanReachZero(t *testing.T) {
	// A tree with a MARKED hand-own but no stamp anywhere: the corpus-wide count must read 0, which
	// is the arm that catches a drifted [GoValueClone] spelling.
	noStamps := t.TempDir()
	if err := os.WriteFile(filepath.Join(noStamps, "handown.cs"),
		[]byte("[module: go.GoManualConversion]\nnamespace go;\npartial class x_package {\n}\n"), 0o600); err != nil {
		t.Fatalf("planting: %v", err)
	}
	if got, err := countStampedFiles(noStamps); err != nil || got != 0 {
		t.Errorf("countStampedFiles over a stamp-free tree = %d, %v; want 0 — the corpus-wide arm "+
			"cannot reach its trigger, so a drifted stamp spelling would pass", got, err)
	}
	if _, files, _, _, err := scanValueCloneStamps(noStamps); err != nil || files != 1 {
		t.Errorf("scanValueCloneStamps over that tree saw %d marked file(s), %v; want 1", files, err)
	}

	// A tree with a STAMP but no marker: the hand-own count must read 0, which is the arm that
	// catches a broken [module: GoManualConversion] spelling.
	noMarker := t.TempDir()
	if err := os.WriteFile(filepath.Join(noMarker, "auto.cs"),
		[]byte("namespace go;\npartial class x_package {\n[GoType] [GoValueClone(\"f\")] partial struct s {\n    internal int f;\n}\n}\n"), 0o600); err != nil {
		t.Fatalf("planting: %v", err)
	}
	if _, files, _, _, err := scanValueCloneStamps(noMarker); err != nil || files != 0 {
		t.Errorf("scanValueCloneStamps over an unmarked tree saw %d marked file(s), %v; want 0 — the "+
			"hand-own arm cannot reach its trigger, so a broken marker would pass", files, err)
	}
	if got, err := countStampedFiles(noMarker); err != nil || got != 1 {
		t.Errorf("countStampedFiles over that tree = %d, %v; want 1 — the corpus-wide count must see "+
			"an UNMARKED stamp, or it is just the intersection under another name", got, err)
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
