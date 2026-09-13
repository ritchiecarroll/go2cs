// archExclusive_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// The GOARCH exclusivity ledger, both sides.
//
// A behavioral package whose Go source is architecture-conditional cannot be measured on a foreign
// architecture -- not by go2cs, and not by GO. Go's own filename rule makes `name_GOARCH.go` an
// implicit build constraint, so StdLibInternalAbi (which copies internal/abi and internal/goarch
// into a package main carrying abi_amd64.go, goarch_amd64.go and zgoarch_amd64.go) fails
// `GOARCH=arm64 go build` outright with `undefined: IntArgRegs` and `undefined: _ArchFamily`, where
// GOARCH=amd64 builds clean. On such a host the converter emits a best-effort conversion and the
// valueless const reaches the C# compiler as `goarch.cs(23,22): error CS0145` -- which is what both
// darwin censuses read on osx-arm64 while osx-x64 passed every phase.
//
// [GoArchExclusive("amd64")] in package_info.cs is the remedy, and this file is its ledger. A seam
// check that verifies one direction passes the exact failure it was written for in mirror form, so
// both are here:
//
//   1. Every arch-CONDITIONAL behavioral package carries the marker.  Without this, the next guard
//      written against an arch-specific construct repeats the whole class silently -- and the way it
//      surfaces is a red arm64 census days later, attributed to whatever else moved.
//   2. Every MARKED package really is arch-conditional.  A marker on a portable package would skip a
//      guard nothing is wrong with, which is the vacuous-green direction.
//
// And the third leg, without which 1 and 2 are a ledger nobody reads: the three instruments that
// enumerate behavioral packages must actually CONSULT the marker. That is asserted on the LIVE
// pattern each one matches with, never on the file's text -- every one of them explains this marker
// in prose beside the code, so a `strings.Contains` over the file stays green when the live pattern
// is reworded. (The same trap the platform marker's own guard was caught by.)
//
// CACHE CAVEAT: the C# and PowerShell files read below live under src/tests and src/core, OUTSIDE
// this module root, and cmd/go drops such files from a test's input fingerprint. This test therefore
// reports `ok (cached)` after a change to one of them and only re-runs under `-count=1`. Any change
// touching the harness side owes `go test -count=1 ./...`, exactly as embeddedAssets_test.go's
// predicate guard does.

import (
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

const archMarkerName = "GoArchExclusive"

var (
	behavioralRootForArch = filepath.Join("..", "tests", "Behavioral")

	// The shared C#/PowerShell predicates that decide which packages a host may measure.
	archMarkerConsumers = []string{
		filepath.Join("..", "tests", "PlatformExclusive.cs"),
		filepath.Join("..", "tests", "Behavioral", "check-no-regression.ps1"),
	}

	// The attribute the marker binds to must exist for a marked package to compile at all.
	archMarkerAttributeSource = filepath.Join("..", "core", "golib", "GoArchExclusiveAttribute.cs")

	// Line-anchored for the same reason the harness predicates are: this file's own prose names the
	// attribute, and an unanchored scan would read a comment as a marker.
	archMarkerLine = regexp.MustCompile(`(?m)^\s*\[(?:go\.)?GoArchExclusive\s*\(([^)]*)\)\]`)

	archNameLiteral = regexp.MustCompile(`"([^"]+)"`)

	// An explicit build constraint naming a GOARCH. Deliberately narrow: it is the arch names that
	// matter, not every //go:build line.
	//
	// The NEGATED form (`//go:build !amd64`) matches too, and that is deliberate rather than sloppy.
	// Such a package may well build on every architecture -- a different file is simply selected --
	// but then its EMISSION varies by architecture while its golden is one architecture's, which is
	// exactly the second criterion the platform marker's own history settled on (SendtoSeam: marked
	// not because it failed to type-check but because its emitted C# differed by platform). A package
	// in that shape should be looked at, so this guard makes someone look.
	archBuildConstraint = regexp.MustCompile(`(?m)^//go:build\b.*\b(` + strings.Join(knownArchNames, "|") + `)\b`)
)

// knownArchNames is go/build's GOARCH list as of Go 1.23 -- the same set directiveOperations.go
// carries for the filename rule. Kept here rather than reached for across files because this guard
// must keep working if that map is refactored; a divergence shows up as a guard that names a
// package the other side does not, which is the loud direction.
var knownArchNames = []string{
	"386", "amd64", "amd64p32", "arm", "armbe", "arm64", "arm64be", "loong64",
	"mips", "mipsle", "mips64", "mips64le", "mips64p32", "mips64p32le",
	"ppc", "ppc64", "ppc64le", "riscv", "riscv64", "s390", "s390x",
	"sparc", "sparc64", "wasm",
}

// archConditionalGoFiles reports the Go files in dir that only build on some architectures, by
// either of Go's two mechanisms: the `name_GOARCH.go` / `name_GOOS_GOARCH.go` filename rule, and an
// explicit //go:build line naming an arch.
func archConditionalGoFiles(t *testing.T, dir string) []string {
	t.Helper()

	entries, err := os.ReadDir(dir)

	if err != nil {
		t.Fatalf("cannot read behavioral package %s: %v", dir, err)
	}

	var conditional []string

	for _, entry := range entries {
		name := entry.Name()

		if entry.IsDir() || !strings.HasSuffix(name, ".go") || strings.HasSuffix(name, "_test.go") {
			continue
		}

		if archSuffixed(name) {
			conditional = append(conditional, name)
			continue
		}

		content, err := os.ReadFile(filepath.Join(dir, name))

		if err != nil {
			t.Fatalf("cannot read %s: %v", filepath.Join(dir, name), err)
		}

		if archBuildConstraint.Match(content) {
			conditional = append(conditional, name)
		}
	}

	return conditional
}

// archSuffixed applies go/build's filename rule: the LAST underscore-separated component being a
// GOARCH constrains the file, as does a GOOS_GOARCH pair. Only the trailing components count --
// `fe_arm64_noasm.go` ends in `_noasm` and is NOT constrained, which is why the corpus carries its
// emission on an amd64 host at all.
func archSuffixed(fileName string) bool {
	base := strings.TrimSuffix(fileName, ".go")
	parts := strings.Split(base, "_")

	if len(parts) < 2 {
		return false
	}

	last := parts[len(parts)-1]

	for _, arch := range knownArchNames {
		if last == arch {
			return true
		}
	}

	return false
}

// markedArches returns the GOARCH names a behavioral package's package_info.cs declares, and whether
// the marker is present at all.
func markedArches(t *testing.T, dir string) ([]string, bool) {
	t.Helper()

	content, err := os.ReadFile(filepath.Join(dir, "package_info.cs"))

	if err != nil {
		return nil, false
	}

	match := archMarkerLine.FindSubmatch(content)

	if match == nil {
		return nil, false
	}

	var arches []string

	for _, literal := range archNameLiteral.FindAllSubmatch(match[1], -1) {
		arches = append(arches, string(literal[1]))
	}

	return arches, true
}

// TestArchExclusiveLedgerBothSides is the ledger: arch-conditional implies marked, and marked
// implies arch-conditional.
func TestArchExclusiveLedgerBothSides(t *testing.T) {
	entries, err := os.ReadDir(behavioralRootForArch)

	if err != nil {
		t.Fatalf("cannot enumerate the behavioral tree: %v", err)
	}

	// The population is expected to be small -- ONE member when this landed. Counting it makes a
	// silently EMPTY walk (a moved tree, a renamed root) fail instead of passing vacuously, which is
	// the false-green shape this repository has paid for repeatedly.
	scanned := 0

	for _, entry := range entries {
		if !entry.IsDir() {
			continue
		}

		dir := filepath.Join(behavioralRootForArch, entry.Name())

		if _, err := os.Stat(filepath.Join(dir, "package_info.cs")); err != nil {
			continue
		}

		scanned++

		conditional := archConditionalGoFiles(t, dir)
		arches, marked := markedArches(t, dir)

		if len(conditional) > 0 && !marked {
			t.Errorf("behavioral package %q has architecture-conditional Go sources %v but carries no [%s(...)] marker in package_info.cs -- it cannot be built by GO on a foreign architecture, so every harness will transpile a best-effort conversion there and report the resulting C# error as a conversion failure",
				entry.Name(), conditional, archMarkerName)
		}

		if marked && len(conditional) == 0 {
			t.Errorf("behavioral package %q carries [%s(%s)] but has no architecture-conditional Go source -- the marker would skip a package nothing is wrong with, which is the vacuous-green direction of this ledger",
				entry.Name(), archMarkerName, strings.Join(arches, ", "))
		}
	}

	if scanned < 100 {
		t.Fatalf("only %d behavioral packages were scanned, which is far below the tree's real size -- the walk found the wrong root and this guard proved nothing", scanned)
	}
}

// TestArchExclusiveMarkerIsConsulted asserts the instruments still READ the marker -- by EXTRACTING
// each consumer's live pattern and RUNNING it, never by scanning for the attribute's name.
//
// The weaker form was written first and its own control killed it: a name scan is satisfied by
// `GoArchExclusiveXX`, so rewording the live regex left the guard green. That is the substring
// over-match this repository has paid for elsewhere (a `ΔHandle` census matching inside `ΔHandler`),
// and here it would have meant a guard that could not fail for the one reason it exists.
func TestArchExclusiveMarkerIsConsulted(t *testing.T) {
	// What every consumer's pattern must accept, and what none of it may accept. The decoy is the
	// prose form: these files all explain the marker in a comment beside the code that matches it,
	// which is how a guard ends up reading the commentary instead of the classifier.
	const markerLine = `    [GoArchExclusive("amd64")]`
	const goQualifiedLine = `    [go.GoArchExclusive("amd64")]`
	const decoyLine = `    // GoArchExclusive marks a package as native to some architectures only.`

	for _, consumer := range archMarkerConsumers {
		content, err := os.ReadFile(consumer)

		if err != nil {
			t.Fatalf("cannot read %s: %v", consumer, err)
		}

		pattern, ok := livePattern(string(content))

		if !ok {
			t.Errorf("%s carries no live pattern matching on %q -- the instruments would disagree about which behavioral packages a host may measure, and an arch-exclusive package would be transpiled best-effort on a foreign architecture instead of skipped by name",
				consumer, archMarkerName)
			continue
		}

		compiled, err := regexp.Compile("(?m)" + pattern)

		if err != nil {
			t.Errorf("%s's live pattern %q does not compile here: %v -- if it has grown syntax Go's regexp cannot read, this guard needs teaching, not deleting",
				consumer, pattern, err)
			continue
		}

		if !compiled.MatchString(markerLine) {
			t.Errorf("%s's live pattern %q no longer matches a real marker line %q -- a marked package would be enumerated and measured on an architecture its Go source cannot even build on",
				consumer, pattern, strings.TrimSpace(markerLine))
		}

		if compiled.MatchString(decoyLine) {
			t.Errorf("%s's live pattern %q matches PROSE (%q) -- an unanchored pattern gates a package on a comment, the same defect the GoManualConversion census had at 63-against-40",
				consumer, pattern, strings.TrimSpace(decoyLine))
		}
	}

	// The C# predicate additionally has to accept the go.-qualified spelling, because the converter
	// emits some assembly attributes root-escaped and a hand-added marker may follow suit. CNR's
	// pattern deliberately does not, matching its platform twin; asserted only where it is true.
	shared, err := os.ReadFile(archMarkerConsumers[0])

	if err != nil {
		t.Fatalf("cannot read %s: %v", archMarkerConsumers[0], err)
	}

	if pattern, ok := livePattern(string(shared)); ok {
		if compiled, err := regexp.Compile("(?m)" + pattern); err == nil && !compiled.MatchString(goQualifiedLine) {
			t.Errorf("%s's live pattern %q no longer accepts the go.-qualified marker spelling",
				archMarkerConsumers[0], pattern)
		}
	}

	// The attribute a marked package_info.cs binds to. Without it the marked project does not
	// compile at all, which is a loud failure -- but only on a host that reaches Compile, so assert
	// it here where every lane pays for it.
	attribute, err := os.ReadFile(archMarkerAttributeSource)

	if err != nil {
		t.Fatalf("cannot read %s: %v -- a package carrying [%s(...)] cannot compile without it",
			archMarkerAttributeSource, err, archMarkerName)
	}

	if !strings.Contains(string(attribute), "class "+archMarkerName+"Attribute") {
		t.Errorf("%s no longer declares %sAttribute", archMarkerAttributeSource, archMarkerName)
	}
}

// livePattern extracts the regex literal a consumer actually matches with -- the C# verbatim string
// of `new(@"...")` or the PowerShell single-quoted operand of `-match '...'` -- from the first LIVE
// line naming the marker.
//
// "Live" means the line is code, not commentary, and not a NEGATED match. Both distinctions are
// load-bearing and both were learned the expensive way: every one of these files explains the marker
// in prose beside the classifier, and CNR carries such markers twice -- once to classify and once as
// a `-notmatch` exclusion -- so a guard reading either would stay green with the classifier deleted.
func livePattern(source string) (string, bool) {
	for _, line := range strings.Split(source, "\n") {
		trimmed := strings.TrimSpace(strings.TrimSuffix(line, "\r"))

		if !strings.Contains(trimmed, archMarkerName) {
			continue
		}

		if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "#") || strings.HasPrefix(trimmed, "*") {
			continue
		}

		if strings.Contains(trimmed, "-notmatch") {
			continue
		}

		if pattern, ok := csharpVerbatimLiteral(trimmed); ok {
			return pattern, true
		}

		if pattern, ok := powerShellMatchLiteral(trimmed); ok {
			return pattern, true
		}
	}

	return "", false
}

// csharpVerbatimLiteral pulls the body out of a C# `@"..."` verbatim string. `""` is a verbatim
// escape for one quote; none of these patterns use it, and a pattern that grows one should fail
// loudly here rather than be half-read.
func csharpVerbatimLiteral(line string) (string, bool) {
	start := strings.Index(line, `@"`)

	if start < 0 {
		return "", false
	}

	rest := line[start+2:]
	end := strings.Index(rest, `"`)

	if end < 0 {
		return "", false
	}

	return rest[:end], true
}

// powerShellMatchLiteral pulls the single-quoted operand out of a PowerShell `-match '...'`.
func powerShellMatchLiteral(line string) (string, bool) {
	idx := strings.Index(line, "-match")

	if idx < 0 {
		return "", false
	}

	rest := line[idx+len("-match"):]
	start := strings.Index(rest, "'")

	if start < 0 {
		return "", false
	}

	rest = rest[start+1:]
	end := strings.Index(rest, "'")

	if end < 0 {
		return "", false
	}

	return rest[:end], true
}
