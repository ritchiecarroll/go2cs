// validationProofPages.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file turns the compare oracle's per-test differential — the record that today lives only in
// the gitignored go2cs_test_comparison.json — into a committed, human-readable PROOF PAGE per
// validated package (docs/validation/current/<dot-id>.md) plus a generated roster index.
// Design: docs/phase4/DESIGN-validation-proof-pages.md.
//
// Two properties are load-bearing:
//
//  1. DETERMINISM — identical inputs render byte-identical output. Every map is walked through a
//     sorted key slice; Go's map iteration order must never reach the page. The converter is
//     byte-deterministic and this feature is not allowed to be the exception.
//
//  2. CONTENT STABILITY — a routine re-validation sweep must leave docs/validation untouched. The
//     page's only volatile text is its provenance line (validation date + converter commit); the
//     writer compares everything ELSE against the file already on disk and skips the write when it
//     is unchanged. Provenance therefore moves only when a VERDICT moves — i.e. at banking — which
//     keeps the proof pages out of every sweep's drift report by construction rather than by
//     membership in some restore-unless-rebanked class.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"time"

	"go2cs/internal/releasestamp"

	"github.com/ritchiecarroll/hashset"
)

const (
	// The layout constants are ALIASES: internal/releasestamp owns where a published release is
	// recorded, so the emitter here and the H2 counter guard (which cannot import main) read one
	// definition. Spelling them again here keeps every existing use site unchanged.
	validationDocsDirName    = releasestamp.DocsDirName
	validationCurrentDirName = releasestamp.CurrentDirName
	validationIndexFileName  = "index.md"

	// proofProvenancePrefix identifies the ONE volatile line of a proof page. Everything else is
	// the content-stable portion the writer compares before deciding to rewrite.
	proofProvenancePrefix = "*Validated "

	// proofTotalsFormat is the single authority for the shape of the page's totals line. The
	// renderer writes it and the README badge emitter reads it back — proofTotalsPattern below is
	// DERIVED from this very string, so a badge can never claim counts in a shape the page no
	// longer renders.
	proofTotalsFormat = "**%d matched · %d disclosed**"

	go2csRepositoryURL = "https://github.com/ritchiecarroll/go2cs"
)

// proofTotalsPattern matches a rendered totals line, built from proofTotalsFormat by escaping its
// literal text and turning each %d into a capture group. It is deliberately not anchored at the end:
// the rendered line continues with the Go version and platform.
var proofTotalsPattern = regexp.MustCompile(`^` + strings.ReplaceAll(regexp.QuoteMeta(proofTotalsFormat), `%d`, `(\d+)`))

// parseProofTotals recovers the matched and disclosed counts from a rendered proof page. CRs are
// ignored so a page smudged to CRLF by autocrlf reads the same as freshly rendered text.
func parseProofTotals(page string) (int, int, bool) {
	for _, line := range strings.Split(strings.ReplaceAll(page, "\r", ""), "\n") {
		match := proofTotalsPattern.FindStringSubmatch(line)

		if match == nil {
			continue
		}

		matched, matchedErr := strconv.Atoi(match[1])
		disclosed, disclosedErr := strconv.Atoi(match[2])

		if matchedErr != nil || disclosedErr != nil {
			continue
		}

		return matched, disclosed, true
	}

	return 0, 0, false
}

// proofPageProvenance carries the page header's non-verdict facts. Date and commit are the volatile
// pair (stripped before the stability compare); import path, Go version and platform are CONTENT —
// a Go-version or platform change is a different claim and must rewrite the page.
type proofPageProvenance struct {
	importPath string
	goVersion  string
	platform   string
	date       string
	commit     string

	// convertedPath is set for a THIRD-PARTY page only: the converted package's directory relative to the
	// conversion's output root. It replaces the standard-library roster sentence and the src/core link, so
	// a stdlib page (convertedPath "") renders exactly as it always has.
	convertedPath string

	// buildTags are the tags both sides of a THIRD-PARTY reading were built with (the conversion and its
	// `go test` baseline share one resolved set). A module page states them because a module's default
	// adds `safe` (defaultModuleBuildTags), so its verdicts are Go's under that tag, not under a bare
	// `go test`. Unused on a stdlib page, whose purego default is documented once for the whole library.
	buildTags []string

	// inputDigest is a THIRD-PARTY page's GoInputDigest, as the package's production project records it
	// (packageInputDigest.go): what the proven conversion was made from, which nugetgo-pack.ps1 compares with the
	// packed project's. "" renders nothing, so a stdlib page is unchanged.
	inputDigest string
}

// proofInputDigestPattern reads a third-party proof page's input digest back, for its MODULE.md row.
var proofInputDigestPattern = regexp.MustCompile("(?m)^Input digest `(sha256-[0-9a-f]+)`")

// proofBuildTagsPattern reads a third-party proof page's build-tags sentence back, so MODULE.md can state
// the tags its pages were read with.
var proofBuildTagsPattern = regexp.MustCompile("(?m)^Both sides were built with (.+)\\.$")

// proofBuildTagsText renders a tag set the way a proof page and MODULE.md state it.
func proofBuildTagsText(tags []string) string {
	if len(tags) == 0 {
		return "no build tags"
	}

	return "`-tags " + strings.Join(tags, ",") + "`"
}

// validationProofDotID maps a package import path to its page name — "/" becomes "." so the pages
// live flat under current/ (mirroring the NuGet ids: io, path.filepath, math.rand.v2).
func validationProofDotID(importPath string) string {
	return strings.ReplaceAll(importPath, "/", ".")
}

// validationProofImportPath is the inverse used by the index generator, which only has filenames to
// work with. Exact for every Go standard-library import path (no element of one contains a dot).
func validationProofImportPath(dotID string) string {
	return strings.ReplaceAll(dotID, ".", "/")
}

// escapeProofCell makes an arbitrary manifest string safe inside a Markdown table cell: a literal
// pipe would end the column and an embedded newline would end the row.
func escapeProofCell(value string) string {
	value = strings.ReplaceAll(value, "\r\n", " ")
	value = strings.ReplaceAll(value, "\n", " ")
	value = strings.ReplaceAll(value, "|", `\|`)

	return strings.TrimSpace(value)
}

// proofVerdictNames returns the sorted union of the two verdict maps — every test the comparison
// saw, from either side. On a validated comparison the two key sets are identical (a one-sided row
// is a mismatch and never validates), but the union is what makes the renderer honest if that ever
// changes.
func proofVerdictNames(comparison testComparison) []string {
	seen := hashset.HashSet[string]{}
	names := make([]string, 0, len(comparison.Go)+len(comparison.CSharp))

	for name := range comparison.Go {
		if seen.Add(name) {
			names = append(names, name)
		}
	}

	for name := range comparison.CSharp {
		if seen.Add(name) {
			names = append(names, name)
		}
	}

	sort.Strings(names)

	return names
}

// proofDisclosedNames returns the sorted tests whose two verdicts DISAGREE. On a validated
// comparison that set is exactly the disclosed-divergent set: every non-agreeing pair the oracle
// did not reclassify against a pinned signature is a mismatch, and a mismatch never validates. It
// is derived from the verdicts rather than parsed back out of the comparison's formatted
// `name (class): reason` strings, so a disclosed ancestor rolled up from disclosed subtests (which
// has no manifest entry of its own, and therefore an empty class and reason) is still counted.
//
// A HOST-CONDITIONAL row is the one disclosed shape whose two verdicts can AGREE (Go fail / C#
// fail — see testDisclosure.HostConditional), so verdict disagreement alone would drop it from the
// page on exactly the hosts the annotation exists for, and the totals line would read 401 + 1 where
// the roster banks 400 + 2. It is added back from the manifest, which is safe because the oracle
// reached this renderer at all: a fail/fail annotated row whose C# output missed the pinned
// signature is a mismatch, and a mismatch never validates, so no page is written.
func proofDisclosedNames(comparison testComparison, names []string, disclosures map[string]testDisclosure) []string {
	disclosed := make([]string, 0, len(comparison.Disclosed))

	for _, name := range names {
		if comparison.Go[name] != comparison.CSharp[name] {
			disclosed = append(disclosed, name)
			continue
		}

		if disclosure, pinned := disclosures[name]; pinned && disclosure.HostConditional != "" &&
			comparison.Go[name] == "fail" && comparison.CSharp[name] == "fail" {
			disclosed = append(disclosed, name)
		}
	}

	return disclosed
}

// proofVerdict renders one side's terminal status, or an em dash when that side never reported the
// test at all (only reachable on a comparison that did not validate).
func proofVerdict(results map[string]string, name string) string {
	if status, ok := results[name]; ok {
		return status
	}

	return "&mdash;"
}

// renderValidationProofPage renders the whole page. It is a pure function of its arguments — no
// clock, no filesystem, no git — so the golden test compares text, and so re-rendering the same
// comparison twice is byte-identical by construction.
func renderValidationProofPage(provenance proofPageProvenance, comparison testComparison, disclosures map[string]testDisclosure, notes []string) string {
	names := proofVerdictNames(comparison)
	disclosed := proofDisclosedNames(comparison, names, disclosures)

	skipped := 0
	for _, name := range names {
		if comparison.Go[name] == "skip" && comparison.CSharp[name] == "skip" {
			skipped++
		}
	}

	var page strings.Builder

	fmt.Fprintf(&page, "# `%s` — validation proof\n\n", provenance.importPath)
	fmt.Fprintf(&page, "Go's own `%s` test suite, converted to C# by [go2cs](%s), built against the converted standard\n", provenance.importPath, go2csRepositoryURL)
	page.WriteString("library, run under the Go-semantics test host, and compared verdict for verdict against a clean\n")
	page.WriteString("`go test -json` baseline of the same sources. This page is generated by the converter from that\n")
	if provenance.convertedPath != "" {
		page.WriteString("comparison, and written beside the conversion: it is not a row of go2cs's standard-library roster.\n\n")
	} else {
		fmt.Fprintf(&page, "comparison — it is the evidence behind the `%s` row in\n", provenance.importPath)
		page.WriteString("[Validated Test Packages](../../ValidatedTestPackages.md).\n\n")
	}

	// The provenance line — the page's only volatile text. Everything below it is compared before
	// a rewrite, so a re-validation that reproduces the same verdicts never touches this file.
	if provenance.commit != "" {
		fmt.Fprintf(&page, "%s%s · converter `%s`*\n\n", proofProvenancePrefix, provenance.date, provenance.commit)
	} else {
		fmt.Fprintf(&page, "%s%s*\n\n", proofProvenancePrefix, provenance.date)
	}

	fmt.Fprintf(&page, proofTotalsFormat+" — Go %s, `%s`, converted package\n",
		len(names)-len(disclosed), len(disclosed), provenance.goVersion, provenance.platform)
	if provenance.convertedPath != "" {
		fmt.Fprintf(&page, "`%s`.\n", provenance.convertedPath)
	} else {
		fmt.Fprintf(&page, "[`src/core/%s`](%s/tree/master/src/core/%s).\n", provenance.importPath, go2csRepositoryURL, provenance.importPath)
	}

	// Every verdict carries the level it was measured at (coordinator ruling, 2026-09-02): a
	// timing-bounded row's pass/fail can depend on build configuration and JIT tiering, so a reader
	// comparing two proof pages — or the same page across a regeneration — must never have to assume
	// which one this was. Rendered unconditionally, not just for the non-default case. An unset
	// Configuration (a testComparison built before this field existed, or by hand in a test fixture)
	// reads as Debug, which is what every such record was actually produced under.
	configuration := comparison.Environment.Configuration
	if configuration == "" {
		configuration = "Debug"
	}

	// The oracle clause is appended to the SAME sentence, not a new one — beside the configuration
	// is what was asked for, and a reader should never have to reconcile two separate provenance
	// lines that could in principle disagree. Omitted entirely when the probe could not run (a
	// testComparison built before this field existed, or by hand in a test fixture, or a genuine
	// best-effort miss); the sentence still reads correctly without it.
	oracleClause := ""
	if comparison.Environment.OracleGoVersion != "" {
		oracleClause = fmt.Sprintf(", oracle `%s`", comparison.Environment.OracleGoVersion)
	}

	// The driver's terminal context, in the same sentence for the same reason: a row whose tests
	// gate on /dev/tty (syscall's foreground pair) skips them on both sides without a controlling
	// terminal and runs them on both with one, so two pages with equal counts can have measured
	// different things, and a reader must be able to tell which. Omitted when the record carries
	// no observation (a Windows run, or a record written before the field existed).
	terminalClause := ""
	switch comparison.Environment.Terminal {
	case driverTerminalPresent:
		terminalClause = ", under a controlling terminal"
	case driverTerminalAbsent:
		terminalClause = ", with no controlling terminal"
	}

	if configuration == "Release" {
		tiering := "off"
		if comparison.Environment.Tiered {
			tiering = "on"
		}
		fmt.Fprintf(&page, "\nMeasured at `Release` (tiered JIT %s)%s%s.\n", tiering, oracleClause, terminalClause)
	} else {
		fmt.Fprintf(&page, "\nMeasured at `%s`%s%s.\n", configuration, oracleClause, terminalClause)
	}

	if provenance.convertedPath != "" {
		fmt.Fprintf(&page, "\nBoth sides were built with %s.\n", proofBuildTagsText(provenance.buildTags))
	}

	if provenance.inputDigest != "" {
		fmt.Fprintf(&page, "\nInput digest `%s`: the package's Go sources, conversion options, target and converter, as its\n"+
			"project records them (`%s`).\n", provenance.inputDigest, inputDigestProperty)
	}

	if skipped > 0 {
		fmt.Fprintf(&page, "\nBoth runtimes skip %d of the matched tests identically.\n", skipped)
	}

	// Package-level notes from the hand-owned disclosure manifest: caveats about the
	// comparison's MEANING no verdict row can express (crypto/tls's expired-fixture ceiling),
	// carried verbatim so a regeneration never loses them.
	for _, note := range notes {
		fmt.Fprintf(&page, "\n> %s\n", strings.TrimSpace(note))
	}

	page.WriteString("\n## Verdicts\n\n")
	page.WriteString("| Test | `go test` | go2cs |\n")
	page.WriteString("|:--|:--:|:--:|\n")

	disclosedNames := hashset.HashSet[string]{}
	for _, name := range disclosed {
		disclosedNames.Add(name)
	}

	for _, name := range names {
		csharp := proofVerdict(comparison.CSharp, name)

		if disclosedNames.Contains(name) {
			csharp += " ([disclosed](#disclosed-divergences))"
		}

		fmt.Fprintf(&page, "| `%s` | %s | %s |\n", escapeProofCell(name), proofVerdict(comparison.Go, name), csharp)
	}

	if len(disclosed) > 0 {
		// The blanket "not a skipped test" wording is TRUE for every class except platform-skip,
		// whose whole shape is a Go=pass/C#=skip pair. Rather than rewrite the sentence corpus-wide
		// (which would churn the text of every existing proof page on its next regeneration), the
		// clause is selected by what this package's manifest actually contains — so pages without a
		// platform-skip row keep their exact wording, and the page that has one cannot state
		// something false about itself.
		hasPlatformSkip, hasDeferred := false, false

		for _, name := range disclosed {
			if disclosure, pinned := disclosures[name]; pinned {
				// compiler-property shares platform-skip's pass/skip shape, so the "not a skipped test" clause
				// is false of it too. (cgo-configuration has the same shape and is NOT added here: that would
				// re-word a banked page at its next regeneration, so it is named for its own ruling instead.)
				hasPlatformSkip = hasPlatformSkip || disclosure.hasClass(platformSkipClass) || disclosure.hasClass(compilerPropertyClass)
				hasDeferred = hasDeferred || disclosure.hasClass(deferredClass)

				// runtime-capability admits the pass/skip shape too (ruling 2026-09-28 09:47, item 3a). Only a
				// row that actually skipped makes the clause false, so every existing runtime-capability
				// failure row keeps its page's wording byte for byte.
				hasPlatformSkip = hasPlatformSkip || (disclosure.hasClass(runtimeCapabilityClass) && comparison.CSharp[name] == "skip")
			}
		}

		// "Provably cannot" is FALSE of a deferred entry: that class is an assertion the CLR CAN meet,
		// pinned against the named plan that will (see deferredClass). So a page that discloses one
		// states the weaker claim every class shares and lets the Class column say which kind each
		// entry is; a page with no deferred entry keeps its wording byte for byte.
		claim := "the managed CLR *provably cannot* satisfy"

		if hasDeferred {
			claim = "this conversion does not satisfy"
		}

		page.WriteString("\n## Disclosed divergences\n\n")

		if hasPlatformSkip {
			fmt.Fprintf(&page, "A disclosed divergence is a specific Go assertion %s — never\n", claim)
			page.WriteString("a tolerance, and never a test skipped to make a row pass. Each one is pinned by exact signature in the package's\n")
		} else {
			fmt.Fprintf(&page, "A disclosed divergence is a specific Go assertion %s — not\n", claim)
			page.WriteString("a skipped test and not a tolerance. Each one is pinned by exact failure signature in the package's\n")
		}
		fmt.Fprintf(&page, "hand-owned [`go2cs_test_disclosures.json`](%s/blob/master/src/core/%s/go2cs_test_disclosures.json);\n",
			go2csRepositoryURL, provenance.importPath)
		page.WriteString("a disclosed test that fails any *other* way is still a hard mismatch.\n\n")

		if hasDeferred {
			page.WriteString("The **Class** column says which kind each one is: a `deferred` entry is an assertion the managed\n")
			page.WriteString("CLR *can* meet, pinned against the named plan that will retire it; every other class is one it\n")
			page.WriteString("*provably cannot* satisfy.\n\n")
		}
		page.WriteString("| Test | Class | Pinned reason |\n")
		page.WriteString("|:--|:--|:--|\n")

		for _, name := range disclosed {
			disclosure, pinned := disclosures[name]

			if !pinned {
				// A parent whose Go=pass/C#=fail divergence is purely the roll-up of disclosed
				// subtests carries no manifest entry of its own (matchTerminalStatuses' aggregation
				// rule), so say what it actually is rather than rendering two empty cells.
				fmt.Fprintf(&page, "| `%s` | `%s` | %s |\n", escapeProofCell(name), "aggregate",
					escapeProofCell("no failure text of its own — the roll-up of this test's disclosed subtests"))
				continue
			}

			// One row per reason: a plain entry's one, or each half of a two-half entry (ruling
			// 2026-09-28 09:47, item 3b: the page reports each half).
			for _, part := range disclosure.parts() {
				fmt.Fprintf(&page, "| `%s` | `%s` | %s |\n", escapeProofCell(name), escapeProofCell(part.Class), escapeProofCell(part.Reason))
			}
		}

		// A platform-skip row's own note. The ruling's anti-laundering clause requires the verdict
		// pair to be recorded OPENLY rather than folded into a count, so the page states it in
		// words next to the table that already shows it — a reader must not have to know the class
		// vocabulary to see that Go ran the test and the converted side did not.
		for _, name := range disclosed {
			disclosure, pinned := disclosures[name]

			if !pinned || !disclosure.hasClass(platformSkipClass) {
				continue
			}

			fmt.Fprintf(&page, "\n`%s` is a **source-defined platform skip**: `go test` reports **pass** and the converted\n"+
				"suite reports **skip**, because the test's own upstream source skips on a platform property the\n"+
				"converted corpus genuinely and permanently holds. The skip is Go's, not the harness's — it is pinned\n"+
				"to that upstream message, so the row moves to a hard mismatch if the converted side ever skips for a\n"+
				"different reason, or stops skipping.\n", escapeProofCell(name))
		}

		// A compiler-property row's own note, for the same anti-laundering reason as platform-skip's: Go
		// ran the test and the converted side skipped, and a reader must see that in words.
		for _, name := range disclosed {
			disclosure, pinned := disclosures[name]

			if !pinned || disclosure.Class != compilerPropertyClass {
				continue
			}

			fmt.Fprintf(&page, "\n`%s` is a **compiler-property skip**: `go test` reports **pass** and the converted\n"+
				"suite reports **skip**, at the test's own upstream check for a decision of the Go compiler (inlining)\n"+
				"that the converted program does not carry; the owner ruled the family structural. It is pinned to that\n"+
				"upstream message, and any subtest Go ran beneath it is listed as withdrawn, never counted as matched.\n", escapeProofCell(name))
		}

		// A runtime-capability SKIP's own note, for the same anti-laundering reason: that class admits the
		// pass/skip shape as well as pass/fail (ruling 2026-09-28 09:47, item 3a), and where the converted
		// side skipped, a reader must see in words that Go ran the test.
		for _, name := range disclosed {
			disclosure, pinned := disclosures[name]

			if !pinned || !disclosure.hasClass(runtimeCapabilityClass) || comparison.CSharp[name] != "skip" {
				continue
			}

			fmt.Fprintf(&page, "\n`%s` is a **runtime-capability skip**: `go test` reports **pass** and the converted\n"+
				"suite reports **skip**, at a check for a runtime capability the managed host does not have. It is\n"+
				"pinned to that skip message, so the row moves to a hard mismatch if the converted side ever skips\n"+
				"for a different reason, or stops skipping.\n", escapeProofCell(name))
		}

		// A host-conditional row's own note, modeled on the roster's internal/zstd row: name the
		// environmental dependency, then name BOTH accepted shapes. It is rendered from the
		// manifest rather than hand-written into the page, so it survives every regeneration and
		// cannot drift from the annotation the compare oracle actually applies.
		for _, name := range disclosed {
			disclosure, pinned := disclosures[name]

			if !pinned || disclosure.HostConditional == "" {
				continue
			}

			fmt.Fprintf(&page, "\n`%s` is **host-conditional**: %s\n",
				escapeProofCell(name), strings.TrimSpace(disclosure.HostConditional))
			page.WriteString("The row is therefore accepted — and counted as disclosed — in two shapes: `go test` **pass**\n")
			page.WriteString("with go2cs **fail**, the pinned divergence above; or **both sides failing**, agreement on a host\n")
			page.WriteString("where the Go premise itself fails. Any movement on the go2cs side still fails the comparison, in\n")
			page.WriteString("either direction — the pin stays strict on the half that is deterministic.\n")
		}

		if len(comparison.Withdrawn) > 0 {
			// Count-by-root rather than 3,000 names: the withdrawn set under one root is the
			// root's own case fan-out, so the root plus a count says everything the list would.
			counts := make(map[string]int)
			for _, name := range comparison.Withdrawn {
				root, _, _ := strings.Cut(name, "/")
				counts[root]++
			}

			roots := make([]string, 0, len(counts))
			for root := range counts {
				roots = append(roots, root)
			}
			sort.Strings(roots)

			page.WriteString("\nA disclosed test that fails at its root never reaches its own case fan-out, so the\n")
			page.WriteString("subtest verdict rows `go test` reports underneath it have no converted counterpart to\n")
			page.WriteString("compare. Those rows are withdrawn with their disclosed root — none is claimed by the\n")
			page.WriteString("matched count above:\n\n")

			for _, root := range roots {
				suffix := "s"

				if counts[root] == 1 {
					suffix = ""
				}

				fmt.Fprintf(&page, "- `%s` — %d verdict row%s withdrawn\n", escapeProofCell(root), counts[root], suffix)
			}
		}
	}

	if len(comparison.Excluded) > 0 {
		excluded := make([]string, len(comparison.Excluded))
		copy(excluded, comparison.Excluded)
		sort.Strings(excluded)

		page.WriteString("\n## Excluded declarations\n\n")
		page.WriteString("Declarations filtered from **both** sides of the comparison, and therefore not claimed above:\n")
		page.WriteString("`Benchmark`, `Fuzz` and `Example` declarations the converted host does not execute, plus any\n")
		page.WriteString("test requiring a capability the managed runtime does not provide — a `testing` member the host\n")
		page.WriteString("has not implemented, or a platform behavior it provably cannot reproduce. Each is named with\n")
		page.WriteString("the capability it needs.\n\n")

		for _, entry := range excluded {
			fmt.Fprintf(&page, "- %s\n", strings.TrimSpace(entry))
		}
	}

	if len(comparison.Gated) > 0 {
		page.WriteString("\n## Gated by a host capability\n\n")
		page.WriteString("A gate names a declaration the converted test host *provably cannot run at all* — not an\n")
		page.WriteString("assertion it fails. It is filtered from **both** sides of the comparison, and because the gate\n")
		page.WriteString("keys on the DECLARATION, every verdict row `go test` reports underneath it is withdrawn with\n")
		page.WriteString("it. None of the rows below is claimed by the matched count above; they are named here so this\n")
		page.WriteString("page states exactly what the row leaves out.\n")

		for _, declaration := range comparison.Gated {
			fmt.Fprintf(&page, "\n### `%s` — %s\n\n", declaration.Name, declaration.Capabilities)

			if len(declaration.Rows) == 0 {
				page.WriteString("`go test` reported no verdict row for this declaration on this platform.\n")

				continue
			}

			quoted := make([]string, len(declaration.Rows))

			for i, row := range declaration.Rows {
				quoted[i] = "`" + row + "`"
			}

			suffix := "s"

			if len(quoted) == 1 {
				suffix = ""
			}

			fmt.Fprintf(&page, "%d verdict row%s `go test` reports and this comparison does not claim:\n\n%s\n",
				len(quoted), suffix, strings.Join(quoted, ", "))
		}
	}

	return page.String()
}

// renderValidationIndex renders the roster page from the dot-ids present under current/, sorted.
func renderValidationIndex(dotIDs []string) string {
	sorted := make([]string, len(dotIDs))
	copy(sorted, dotIDs)
	sort.Strings(sorted)

	return renderValidationIndexPage(sorted)
}

// renderValidationIndexPage renders the whole page with its rows in the order given.
func renderValidationIndexPage(dotIDs []string) string {
	var page strings.Builder

	page.WriteString("# Validation proofs\n\n")
	page.WriteString("One page per validated package — the full per-test differential behind its row in\n")
	page.WriteString("[Validated Test Packages](../ValidatedTestPackages.md): every eligible `Test` function, `go test`'s\n")
	page.WriteString("verdict and go2cs's, side by side. The converter writes these pages itself, from the same\n")
	page.WriteString("comparison record that decides whether a package validates at all.\n\n")
	page.WriteString("Pages under `current/` are living proof — regenerated only when a package's verdicts change.\n")
	page.WriteString("Versioned sibling directories are frozen publication snapshots: written once at release and never\n")
	page.WriteString("rewritten, so the proof link for a published package stays the proof as of that binary.\n\n")

	if len(dotIDs) == 0 {
		page.WriteString("*No proof pages have been generated yet.*\n")

		return page.String()
	}

	page.WriteString(validationIndexTableHead + "\n")
	page.WriteString("|:--|:--|:--|\n")

	for _, row := range renderValidationIndexRows(dotIDs) {
		page.WriteString(row + "\n")
	}

	return page.String()
}

// renderValidationIndexRows renders one CURRENT-table row per dot-id, in the order given.
func renderValidationIndexRows(dotIDs []string) []string {
	rows := make([]string, 0, len(dotIDs))

	for _, dotID := range dotIDs {
		importPath := validationProofImportPath(dotID)
		rows = append(rows, fmt.Sprintf("| `%s` | [`%s.md`](%s/%s.md) | [`src/core/%s`](%s/tree/master/src/core/%s) |",
			importPath, dotID, validationCurrentDirName, dotID, importPath, go2csRepositoryURL, importPath))
	}

	return rows
}

// proofStableContent strips a page down to the text the stability rule compares: the provenance
// line is removed, and CRs are dropped so a file smudged to CRLF by autocrlf still compares equal
// to freshly rendered text.
func proofStableContent(page string) string {
	lines := strings.Split(strings.ReplaceAll(page, "\r", ""), "\n")
	stable := make([]string, 0, len(lines))

	for _, line := range lines {
		if strings.HasPrefix(line, proofProvenancePrefix) {
			continue
		}

		stable = append(stable, line)
	}

	return strings.Join(stable, "\n")
}

// writeStableDocFile writes page to fileName unless the file already holds the same content-stable
// text — the rule that keeps routine sweeps out of docs/validation. Returns whether it wrote.
// Output is CRLF, matching the converter's other generated text (README.md) and the docs tree on
// disk, so autocrlf has nothing to smudge.
func writeStableDocFile(fileName string, page string) (bool, error) {
	if existing, err := os.ReadFile(fileName); err == nil {
		if proofStableContent(string(existing)) == proofStableContent(page) {
			return false, nil
		}
	}

	contents := []byte(strings.ReplaceAll(page, "\n", "\r\n"))

	if err := os.WriteFile(fileName, contents, 0644); err != nil {
		return false, fmt.Errorf("failed to write validation proof file \"%s\": %s", fileName, err)
	}

	return true, nil
}

// writeValidationProofPage renders and writes one package's proof page under docsPath, then
// regenerates the roster index from whatever pages now exist. Both writes are stability-gated, so
// a re-validation reproducing the same verdicts leaves the whole directory untouched.
func writeValidationProofPage(docsPath string, provenance proofPageProvenance, comparison testComparison, disclosures map[string]testDisclosure, notes []string) error {
	currentPath := filepath.Join(docsPath, validationDocsDirName, validationCurrentDirName)

	if err := os.MkdirAll(currentPath, 0755); err != nil {
		return err
	}

	dotID := validationProofDotID(provenance.importPath)
	page := renderValidationProofPage(provenance, comparison, disclosures, notes)

	if _, err := writeStableDocFile(filepath.Join(currentPath, dotID+".md"), page); err != nil {
		return err
	}

	return writeValidationIndex(docsPath)
}

// rosterRowPattern matches one banked row of docs/ValidatedTestPackages.md and captures its import
// path. It is a verbatim port of $RosterRowPattern in src/_roster.ps1, and must stay in step with it
// and with ROSTER_ROW in docs/phase4/hopA-inputs/regen-validation-index.py: three readers of one
// table. TestRosterRowPatternsAgree holds them together.
var rosterRowPattern = regexp.MustCompile("^\\|\\s*\\[`([^`]+)`\\]\\([^)]*\\)\\s*\\|\\s*(\\d+)\\s*\\|\\s*(\\d*)\\s*\\|")

// validationRosterFileName is the roster of record, beside docs/validation.
const validationRosterFileName = "ValidatedTestPackages.md"

// validationIndexTableHead is the CURRENT table's header row. Everything above it in the committed
// index, and everything after its rows, is hand-maintained text the writer keeps.
const validationIndexTableHead = "| Package | Proof | Converted package |"

// readValidationRoster returns the import paths the roster banks, and whether the roster exists.
func readValidationRoster(docsPath string) ([]string, bool, error) {
	data, err := os.ReadFile(filepath.Join(docsPath, validationRosterFileName))

	if os.IsNotExist(err) {
		return nil, false, nil
	}

	if err != nil {
		return nil, false, err
	}

	var packages []string

	for _, line := range strings.Split(strings.ReplaceAll(string(data), "\r", ""), "\n") {
		if match := rosterRowPattern.FindStringSubmatch(line); match != nil {
			packages = append(packages, match[1])
		}
	}

	return packages, true, nil
}

// exclusionLedgerRowPattern and exclusionLedgerHeadingPattern read the roster's EXCLUSION LEDGER, the
// "Excluded packages" table: the packages a ruling took out of the validated population, each with its
// class and why. They are verbatim ports of $ExclusionLedgerRowPattern and $ExclusionLedgerHeadingPattern
// in src/_roster.ps1, and TestExclusionLedgerPatternsAgree holds them together. A ledger row's first cell
// is a PLAIN code span, never the banked table's linked [`pkg`](url) shape.
var (
	exclusionLedgerRowPattern     = regexp.MustCompile("^\\|\\s*`([^`]+)`\\s*\\|\\s*([^|]*?)\\s*\\|\\s*([^|]*?)\\s*\\|")
	exclusionLedgerHeadingPattern = regexp.MustCompile("^(#{1,6})\\s+Excluded packages\\s*$")
)

// readRosterExclusions returns the import paths the roster's exclusion ledger names, read the way
// Get-ExclusionLedgerRows reads them in src/_roster.ps1: only within the ledger's own section, from its
// heading to the next heading of equal or higher level. Other tables in the roster have the same row
// shape, so the heading is what identifies the ledger. Like that function it refuses rather than guesses:
// a missing roster, a missing heading or a duplicated heading is an error, never an empty ledger.
func readRosterExclusions(docsPath string) (map[string]bool, error) {
	rosterPath := filepath.Join(docsPath, validationRosterFileName)
	data, err := os.ReadFile(rosterPath)

	if err != nil {
		return nil, err
	}

	lines := strings.Split(strings.ReplaceAll(string(data), "\r", ""), "\n")
	start := -1

	for i, line := range lines {
		if exclusionLedgerHeadingPattern.MatchString(line) {
			if start >= 0 {
				return nil, fmt.Errorf("the roster \"%s\" has more than one 'Excluded packages' heading", rosterPath)
			}

			start = i
		}
	}

	if start < 0 {
		return nil, fmt.Errorf("the roster \"%s\" has no 'Excluded packages' heading", rosterPath)
	}

	level := len(exclusionLedgerHeadingPattern.FindStringSubmatch(lines[start])[1])
	sectionEnd := regexp.MustCompile(fmt.Sprintf("^#{1,%d}\\s", level))
	excluded := make(map[string]bool)

	for _, line := range lines[start+1:] {
		if sectionEnd.MatchString(line) {
			break
		}

		if match := exclusionLedgerRowPattern.FindStringSubmatch(line); match != nil {
			excluded[match[1]] = true
		}
	}

	return excluded, nil
}

// composeValidationIndex puts freshly rendered rows into the committed page: the text up to and
// including the CURRENT table's header and separator is kept byte for byte (the Frozen snapshots
// section lives there), the old rows are dropped, and whatever follows them is kept. It reports false
// when the committed page has no CURRENT table to compose around.
func composeValidationIndex(committed string, rows []string) (string, bool) {
	lines := strings.Split(strings.ReplaceAll(committed, "\r", ""), "\n")
	head := -1

	for i, line := range lines {
		if line == validationIndexTableHead && i+1 < len(lines) && strings.HasPrefix(lines[i+1], "|:") {
			head = i
			break
		}
	}

	if head < 0 {
		return "", false
	}

	tail := head + 2

	for tail < len(lines) && strings.HasPrefix(lines[tail], "| `") {
		tail++
	}

	composed := append(append(append([]string{}, lines[:head+2]...), rows...), lines[tail:]...)

	return strings.Join(composed, "\n"), true
}

// writeValidationIndex regenerates the index's rows: one per package the roster banks that has a page
// under current/, in the roster's order. A page the roster does not bank (a retired import path kept
// as its successor's relocation anchor, or an excluded package's evidence) is not a row, and a banked
// package with no page yet has none. Without a roster (a bare temporary tree) every page is a row.
// The hand-maintained text around the CURRENT table is kept (composeValidationIndex). It is computed
// on every validated run rather than only when a page changed: identical content means
// needToWriteFile-style gating writes nothing, and a missing or stale index heals itself.
func writeValidationIndex(docsPath string) error {
	validationPath := filepath.Join(docsPath, validationDocsDirName)

	entries, err := os.ReadDir(filepath.Join(validationPath, validationCurrentDirName))

	if err != nil {
		return err
	}

	pages := make(map[string]bool)

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".md") {
			continue
		}

		pages[strings.TrimSuffix(entry.Name(), ".md")] = true
	}

	rosterPackages, hasRoster, err := readValidationRoster(docsPath)

	if err != nil {
		return err
	}

	// A roster that reads as no rows is a broken instrument, not an empty roster: rewriting the index
	// around nothing would erase every row, so refuse by name instead.
	if hasRoster && len(rosterPackages) == 0 {
		return fmt.Errorf("the roster \"%s\" has no banked rows; the validation index was left as it is", filepath.Join(docsPath, validationRosterFileName))
	}

	// The rows follow the roster's own order, as regen-validation-index.py writes them; without a
	// roster they are sorted.
	var dotIDs []string

	if hasRoster {
		listed := make(map[string]bool)

		for _, importPath := range rosterPackages {
			if dotID := validationProofDotID(importPath); pages[dotID] && !listed[dotID] {
				listed[dotID] = true
				dotIDs = append(dotIDs, dotID)
			}
		}
	} else {
		for dotID := range pages {
			dotIDs = append(dotIDs, dotID)
		}

		sort.Strings(dotIDs)
	}

	indexPath := filepath.Join(validationPath, validationIndexFileName)
	page := renderValidationIndexPage(dotIDs)

	if committed, err := os.ReadFile(indexPath); err == nil {
		if composed, ok := composeValidationIndex(string(committed), renderValidationIndexRows(dotIDs)); ok {
			page = composed
		}
	}

	_, err = writeStableDocFile(indexPath, page)

	return err
}

// emitValidationProofPage publishes the proof page for a package that has just validated. It is a
// silent no-op when the conversion is not rooted in a go2cs repository checkout — the tree root is
// found by the same upward walk the test pipeline uses to self-locate $(go2csPath) (the root
// holding core/golib), and docs/ is that root's sibling; a bare temp -go2cspath conversion, or a
// deployed GOPATH runtime root, has no docs tree to publish into and simply skips.
func emitValidationProofPage(outputPath string, comparison testComparison, manifest testManifest, disclosures map[string]testDisclosure, notes []string, options Options) error {
	if manifest.PackageImportPath == "" {
		return nil
	}

	destination := validationProofDestination(outputPath, manifest)

	if destination.pagePath != "" {
		return writeThirdPartyProofPage(outputPath, destination.pagePath, comparison, manifest, disclosures, notes, options)
	}

	if destination.docsPath == "" {
		return nil
	}

	docsPath := destination.docsPath

	goVersion := strings.TrimPrefix(manifest.GoVersion, "go")

	if goVersion == "" {
		goVersion = goVersionFromToolchain()
	}

	provenance := proofPageProvenance{
		importPath: manifest.PackageImportPath,
		goVersion:  goVersion,
		platform:   options.targetPlatform,
		date:       time.Now().UTC().Format("2006-01-02"),
		commit:     shortGitRevision(filepath.Dir(docsPath)),
	}

	return writeValidationProofPage(docsPath, provenance, comparison, disclosures, notes)
}

// proofDestination is where a validated package's proof page is published: the go2cs checkout's
// docs/ tree (the standard-library roster, its index and pages), or a page file written beside a
// third-party conversion. Both empty means no page is written.
type proofDestination struct {
	docsPath string
	pagePath string
}

// validationProofDestination decides a validated package's proof-page destination.
//
// A THIRD-PARTY package (one with a module path; the standard library has none) is published beside its
// conversion, at <outRoot>/validation/<modulePath>/<path within the module>.md, the module's root package
// as index.md there -- never into a go2cs checkout's roster, even when the conversion sits inside one
// (the multi-package module design, D7, ruled 2026-09-29). <outRoot> is the output directory with the
// recurse layout's trailing src/<importPath> removed, or the output directory itself for any other layout.
// Paths are keyed by the import path, so the stdlib's dot-id (not invertible once a path element holds a
// dot, as github.com does) is never involved.
//
// A standard-library package keeps the rule it has always had, unchanged: under a go2cs root, into that
// checkout's docs/.
func validationProofDestination(outputPath string, manifest testManifest) proofDestination {
	if manifest.ModulePath != "" {
		return proofDestination{pagePath: thirdPartyProofPagePath(outputPath, manifest)}
	}

	root := findGo2CSRootAbove(outputPath)

	if root == "" {
		return proofDestination{}
	}

	docsPath := filepath.Join(filepath.Dir(root), "docs")

	if info, err := os.Stat(docsPath); err != nil || !info.IsDir() {
		return proofDestination{}
	}

	return proofDestination{docsPath: docsPath}
}

// thirdPartyOutputRoot is the conversion's output root: outputPath without the recurse layout's trailing
// src/<importPath>, or outputPath itself when it is not in that layout.
func thirdPartyOutputRoot(outputPath string, importPath string) string {
	outputPath = filepath.Clean(outputPath)
	suffix := string(filepath.Separator) + filepath.Join("src", filepath.FromSlash(importPath))

	if len(outputPath) > len(suffix) && strings.EqualFold(outputPath[len(outputPath)-len(suffix):], suffix) {
		return outputPath[:len(outputPath)-len(suffix)]
	}

	return outputPath
}

// thirdPartyProofPagePath is a third-party package's page: <outRoot>/validation/<modulePath>/<rel>.md,
// where <rel> is the package's path within its module and the module's root package is index.md.
func thirdPartyProofPagePath(outputPath string, manifest testManifest) string {
	moduleDir := filepath.Join(thirdPartyOutputRoot(outputPath, manifest.PackageImportPath), "validation", filepath.FromSlash(manifest.ModulePath))

	rel, ok := strings.CutPrefix(manifest.PackageImportPath, manifest.ModulePath+"/")
	if manifest.PackageImportPath == manifest.ModulePath || !ok {
		rel = "index"
	}

	return filepath.Join(moduleDir, filepath.FromSlash(rel)+".md")
}

// thirdPartyModuleSummaryName is the per-module summary written beside a module's package pages.
const thirdPartyModuleSummaryName = "MODULE.md"

// writeThirdPartyProofPage writes a third-party package's proof page at pagePath, then regenerates its
// module's MODULE.md from every page now beside it. Both are stability-gated like the stdlib's.
func writeThirdPartyProofPage(outputPath string, pagePath string, comparison testComparison, manifest testManifest, disclosures map[string]testDisclosure, notes []string, options Options) error {
	goVersion := strings.TrimPrefix(manifest.GoVersion, "go")

	if goVersion == "" {
		goVersion = goVersionFromToolchain()
	}

	outRoot := thirdPartyOutputRoot(outputPath, manifest.PackageImportPath)
	converted, err := filepath.Rel(outRoot, filepath.Clean(outputPath))
	if err != nil || converted == "." {
		converted = filepath.Base(outputPath)
	}

	provenance := proofPageProvenance{
		importPath:    manifest.PackageImportPath,
		goVersion:     goVersion,
		platform:      options.targetPlatform,
		date:          time.Now().UTC().Format("2006-01-02"),
		convertedPath: filepath.ToSlash(converted),
		buildTags:     options.buildTags,
		inputDigest:   productionProjectInputDigest(outputPath, manifest),
	}

	if err := os.MkdirAll(filepath.Dir(pagePath), 0o755); err != nil {
		return err
	}

	if _, err := writeStableDocFile(pagePath, renderValidationProofPage(provenance, comparison, disclosures, notes)); err != nil {
		return err
	}

	moduleDir := filepath.Join(outRoot, "validation", filepath.FromSlash(manifest.ModulePath))

	return writeThirdPartyModuleSummary(moduleDir, manifest.ModulePath)
}

// productionProjectInputDigest is the GoInputDigest the package's production project in outputPath records: the value
// this -tests run's own production pass wrote, or "" when the project records none.
func productionProjectInputDigest(outputPath string, manifest testManifest) string {
	if manifest.ProjectName == "" {
		return ""
	}

	data, err := os.ReadFile(filepath.Join(outputPath, projectFileBaseName(manifest.ProjectName)+".csproj"))

	if err != nil {
		return ""
	}

	return projectInputDigest(string(data))
}

// writeThirdPartyModuleSummary regenerates <moduleDir>/MODULE.md from the package pages beneath it: one
// row per page (its import path, matched and disclosed counts, a link) and the module's totals.
func writeThirdPartyModuleSummary(moduleDir string, modulePath string) error {
	type pageRow struct {
		importPath  string
		link        string
		matched     int
		disclosed   int
		inputDigest string
	}

	var rows []pageRow

	// The declarations each page excluded from both sides of its comparison, by import path: MODULE.md is what a
	// module pack ships as VALIDATION.md, so it names them rather than leaving them to the pages (the nugetgo
	// rehearsal, 2026-10-09, gap 7).
	excludedByPackage := map[string][]string{}

	// The build-tags sentence of every page: one value when the module was read in one run (the usual
	// case), more when packages were re-read under different tags, none for pages written before pages
	// stated their tags.
	tagTexts := map[string]bool{}

	err := filepath.WalkDir(moduleDir, func(path string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil || entry.IsDir() || !strings.HasSuffix(path, ".md") || filepath.Base(path) == thirdPartyModuleSummaryName {
			return walkErr
		}

		data, err := os.ReadFile(path)
		if err != nil {
			return err
		}

		matched, disclosed, ok := parseProofTotals(string(data))
		if !ok {
			return nil
		}

		importPath := ""
		if match := proofTitlePattern.FindStringSubmatch(strings.ReplaceAll(string(data), "\r", "")); match != nil {
			importPath = match[1]
		}

		if excluded := proofExcludedDeclarations(string(data)); len(excluded) > 0 {
			excludedByPackage[importPath] = excluded
		}

		if match := proofBuildTagsPattern.FindStringSubmatch(strings.ReplaceAll(string(data), "\r", "")); match != nil {
			tagTexts[match[1]] = true
		}

		inputDigest := ""
		if match := proofInputDigestPattern.FindStringSubmatch(strings.ReplaceAll(string(data), "\r", "")); match != nil {
			inputDigest = match[1]
		}

		link, _ := filepath.Rel(moduleDir, path)
		rows = append(rows, pageRow{importPath: importPath, link: filepath.ToSlash(link), matched: matched, disclosed: disclosed, inputDigest: inputDigest})

		return nil
	})
	if err != nil {
		return err
	}

	sort.Slice(rows, func(i, j int) bool { return rows[i].importPath < rows[j].importPath })

	var page strings.Builder
	totalMatched, totalDisclosed := 0, 0

	fmt.Fprintf(&page, "# `%s` — module validation summary\n\n", modulePath)
	page.WriteString("Every package of this module whose Go tests go2cs converted and validated, summed from the proof pages\n")
	page.WriteString("beside this file. Generated by the converter; regenerate rather than edit.\n\n")

	switch len(tagTexts) {
	case 0:
	case 1:
		for text := range tagTexts {
			fmt.Fprintf(&page, "Both sides of every reading were built with %s.\n\n", text)
		}
	default:
		page.WriteString("The packages were read with different build tags; each proof page states its own.\n\n")
	}

	// The input digest is the one each proof page states; nugetgo-pack.ps1 reads it from this table, packed as
	// VALIDATION.md, and refuses a row whose digest is not the packed project's.
	page.WriteString("| Package | Matched | Disclosed | Input digest | Proof |\n|:--|--:|--:|:--|:--|\n")

	for _, row := range rows {
		digest := "none"
		if row.inputDigest != "" {
			digest = "`" + row.inputDigest + "`"
		}

		fmt.Fprintf(&page, "| `%s` | %d | %d | %s | [%s](%s) |\n", row.importPath, row.matched, row.disclosed, digest, row.link, row.link)
		totalMatched += row.matched
		totalDisclosed += row.disclosed
	}

	fmt.Fprintf(&page, "\n**Total: %d matched · %d disclosed** across %d package(s).\n", totalMatched, totalDisclosed, len(rows))

	if len(excludedByPackage) > 0 {
		count := 0
		for _, excluded := range excludedByPackage {
			count += len(excluded)
		}

		page.WriteString("\n## Excluded declarations\n\n")
		fmt.Fprintf(&page, "%d declaration(s) were excluded from both sides of the comparison and are not counted above;\n", count)
		page.WriteString("each package's proof page gives the reason for its own.\n\n")

		for _, row := range rows {
			for _, entry := range excludedByPackage[row.importPath] {
				fmt.Fprintf(&page, "- `%s`: %s\n", row.importPath, entry)
			}
		}
	}

	_, err = writeStableDocFile(filepath.Join(moduleDir, thirdPartyModuleSummaryName), page.String())

	return err
}

// proofExcludedDeclarations reads a proof page's "Excluded declarations" list back: each entry as the page names it,
// in the page's (sorted) order.
func proofExcludedDeclarations(page string) []string {
	_, section, found := strings.Cut(strings.ReplaceAll(page, "\r", ""), "\n## Excluded declarations\n")
	if !found {
		return nil
	}

	var excluded []string

	for _, line := range strings.Split(section, "\n") {
		if strings.HasPrefix(line, "## ") {
			break
		}

		if entry, ok := strings.CutPrefix(line, "- "); ok {
			excluded = append(excluded, entry)
		}
	}

	return excluded
}

// proofTitlePattern reads a proof page's import path back from its title line.
var proofTitlePattern = regexp.MustCompile("(?m)^# `([^`]+)` — validation proof$")

// goVersionFromToolchain is the fallback when the test manifest recorded no Go version.
func goVersionFromToolchain() string {
	if version := goVersion(); version != "" {
		return version
	}

	return "unknown"
}

// shortGitRevision abbreviates gitRevision to the 9 characters the repository's own `git log
// --oneline` shows. An empty result (no checkout, no git) drops the element from the page.
func shortGitRevision(path string) string {
	revision := gitRevision(path)

	if len(revision) > 9 {
		return revision[:9]
	}

	return revision
}

// publishesRosterArtifacts reports whether a completed comparison may write the COMMITTED validation
// artifacts — the proof page under docs/validation, that directory's index row, and the package
// README's Tests badge.
//
// `validated` alone does not earn them. It means "every test the run compared matched", and a
// FILTERED run compares only what its filter admitted, so a one-test -test-filter earns exactly the
// status a full sweep does. Measured on reflect: `-test-filter '^TestCallPanic$'` wrote a
// `1/1 validated` badge, a docs/validation index row and a full proof page for a package with 124
// failing tests — and those three artifacts are precisely the ones a reader trusts.
//
// The -test-filter flag's own help has always said a gated census is DIAGNOSTIC ONLY and must never
// bank a row. That was enforced by the operator remembering it, which is the kind of rule that
// eventually gets skipped — the same reasoning the seeded-reconvert ritual is mechanical for. This
// makes it structural: a filtered run leaves no roster trace, whatever its status.
func publishesRosterArtifacts(status string, testFilter string) bool {
	return status == "validated" && strings.TrimSpace(testFilter) == ""
}
