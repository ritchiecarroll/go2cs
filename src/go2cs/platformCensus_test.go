// platformCensus_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Unit gates for the multi-platform emission census (increment 1 of
// docs/phase4/DESIGN-multiplatform-corpus.md). Two things are proven here, and neither needs a
// standard-library conversion to prove:
//
//	the -platforms LIST parses and validates (a typo must not silently become the host platform)
//	the classification partitions an emission set the way layout L3 prices it
//
// The census's own numbers are proven separately, against the design's measured tables, by running
// the instrument over the real corpus — that gate lives in the increment's report, not here.

package main

import (
	"encoding/json"
	"os"
	"path/filepath"
	"reflect"
	"sort"
	"testing"
	"time"
)

func TestParsePlatformListAcceptsOneTarget(t *testing.T) {
	targets, err := parsePlatformList("windows/amd64")

	if err != nil {
		t.Fatalf("parsePlatformList: unexpected error: %v", err)
	}

	if !reflect.DeepEqual(targets, []string{"windows/amd64"}) {
		t.Errorf("parsePlatformList(single) = %v, want [windows/amd64]", targets)
	}
}

func TestParsePlatformListSplitsCommasAndWhitespace(t *testing.T) {
	want := []string{"windows/amd64", "linux/amd64", "darwin/arm64"}

	for _, value := range []string{
		"windows/amd64,linux/amd64,darwin/arm64",
		"windows/amd64, linux/amd64, darwin/arm64",
		"windows/amd64 linux/amd64 darwin/arm64",
		" windows/amd64 ,,linux/amd64,\tdarwin/arm64 ",
	} {
		targets, err := parsePlatformList(value)

		if err != nil {
			t.Fatalf("parsePlatformList(%q): unexpected error: %v", value, err)
		}

		if !reflect.DeepEqual(targets, want) {
			t.Errorf("parsePlatformList(%q) = %v, want %v", value, targets, want)
		}
	}
}

// A duplicate must collapse to its FIRST occurrence: the first entry is the one every
// single-platform consumer reads as targetPlatform, and a census must not convert the same target
// into two staging roots and then compare it against itself.
func TestParsePlatformListDeduplicatesPreservingFirstOrder(t *testing.T) {
	targets, err := parsePlatformList("linux/amd64,windows/amd64,linux/amd64")

	if err != nil {
		t.Fatalf("parsePlatformList: unexpected error: %v", err)
	}

	if !reflect.DeepEqual(targets, []string{"linux/amd64", "windows/amd64"}) {
		t.Errorf("parsePlatformList(dupes) = %v, want [linux/amd64 windows/amd64]", targets)
	}
}

// A malformed or unknown target must be REJECTED, not quietly ignored. The failure this guards is
// the expensive one: a census spends minutes per target, and a value the loader silently resolves to
// the host platform produces a manifest whose numbers look plausible and mean nothing.
func TestParsePlatformListRejectsMalformedTargets(t *testing.T) {
	for _, value := range []string{
		"",
		",",
		"linux",
		"linux-amd64",
		"linux/",
		"/amd64",
		"linux/amd64/v3",
		"linix/amd64",  // GOOS typo
		"linux/x86_64", // GOARCH typo
		"windows/amd64,linux/x86_64",
	} {
		if targets, err := parsePlatformList(value); err == nil {
			t.Errorf("parsePlatformList(%q) = %v, want an error", value, targets)
		}
	}
}

func TestPlatformTargetTagIsDirectorySafe(t *testing.T) {
	if got := platformTargetTag("darwin/arm64"); got != "darwin-arm64" {
		t.Errorf("platformTargetTag = %q, want darwin-arm64", got)
	}
}

// The four buckets partition the union, and the partition is by PRESENCE first, content second: a
// name emitted by only some targets is `partial` (or `exclusive`) whatever its contents, because
// layout L3's cost for it is one copy per emitting GOOS either way.
func TestClassifyPlatformEmissionsPartitionsByPresenceThenContent(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	emissions := []map[string]string{
		{ // windows
			"fmt/print.cs":             "same",
			"path/filepath/path.cs":    "win",
			"internal/goos/nonunix.cs": "nonunix",
			"syscall/dll_windows.cs":   "winonly",
		},
		{ // linux
			"fmt/print.cs":          "same",
			"path/filepath/path.cs": "unix",
			"internal/goos/unix.cs": "unix",
		},
		{ // darwin
			"fmt/print.cs":          "same",
			"path/filepath/path.cs": "unix",
			"internal/goos/unix.cs": "unix",
		},
	}

	classes := classifyPlatformEmissions(targets, emissions)

	want := map[string]string{
		"fmt/print.cs":             artifactIdentical,
		"path/filepath/path.cs":    artifactVariant,
		"internal/goos/unix.cs":    artifactPartial,
		"internal/goos/nonunix.cs": artifactExclusive,
		"syscall/dll_windows.cs":   artifactExclusive,
	}

	if len(classes) != len(want) {
		t.Fatalf("classified %d artifacts, want %d", len(classes), len(want))
	}

	for name, wantClass := range want {
		if got := classes[name].class; got != wantClass {
			t.Errorf("class of %s = %q, want %q", name, got, wantClass)
		}
	}

	if got := classes["internal/goos/unix.cs"].emitters; !reflect.DeepEqual(got, []string{"linux/amd64", "darwin/amd64"}) {
		t.Errorf("emitters of internal/goos/unix.cs = %v, want [linux/amd64 darwin/amd64]", got)
	}
}

// Census C (the GOARCH axis) is the same instrument with two targets, where `partial` is empty by
// construction — a name emitted by two of two targets is emitted by ALL of them.
func TestClassifyPlatformEmissionsTwoTargetsHaveNoPartialBucket(t *testing.T) {
	targets := []string{"darwin/amd64", "darwin/arm64"}

	classes := classifyPlatformEmissions(targets, []map[string]string{
		{"math/exp.cs": "same", "internal/cpu/cpu.cs": "amd", "runtime/asm.cs": "amdonly"},
		{"math/exp.cs": "same", "internal/cpu/cpu.cs": "arm"},
	})

	for name, want := range map[string]string{
		"math/exp.cs":         artifactIdentical,
		"internal/cpu/cpu.cs": artifactVariant,
		"runtime/asm.cs":      artifactExclusive,
	} {
		if got := classes[name].class; got != want {
			t.Errorf("class of %s = %q, want %q", name, got, want)
		}
	}

	for name, class := range classes {
		if class.class == artifactPartial {
			t.Errorf("%s classified partial in a two-target census — impossible by construction", name)
		}
	}
}

func TestComparePlatformPairCountsBothSidesAndTouchedPackages(t *testing.T) {
	row := comparePlatformPair("windows/amd64", "linux/amd64",
		map[string]string{
			"fmt/print.cs":           "same",
			"os/proc.cs":             "win",
			"syscall/dll_windows.cs": "winonly",
		},
		map[string]string{
			"fmt/print.cs":               "same",
			"os/proc.cs":                 "linux",
			"internal/syscall/unix/x.cs": "linuxonly",
			"internal/syscall/unix/y.cs": "linuxonly",
		})

	want := pairwiseComparison{
		A: "windows/amd64", B: "linux/amd64",
		EmittedByBoth: 2, Identical: 1, Differ: 1, AOnly: 1, BOnly: 2,
		PackagesTouched: 3, // os, syscall, internal/syscall/unix
	}

	if row != want {
		t.Errorf("comparePlatformPair = %+v, want %+v", row, want)
	}
}

func TestArtifactPackageAndVariantKind(t *testing.T) {
	if got := artifactPackage("net/http/httputil/dump.cs"); got != "net/http/httputil" {
		t.Errorf("artifactPackage = %q, want net/http/httputil", got)
	}

	for artifactPath, want := range map[string]string{
		"os/proc.cs":                     "source",
		"os/" + PackageInfoFileName:      PackageInfoFileName,
		"net/" + PackageInitFileName:     PackageInitFileName,
		"internal/goos/zgoos_windows.cs": "source",
	} {
		if got := variantKind(artifactPath); got != want {
			t.Errorf("variantKind(%q) = %q, want %q", artifactPath, got, want)
		}
	}
}

// The manifest's headline numbers and the L3 pricing must fall out of the classification rather than
// out of hard-coded multipliers: per-GOOS cost is Σ(emitting targets) over every non-identical
// artifact, which is what makes the design's "variants ×3, pair-shared ×2, exclusives ×1" arithmetic
// generalize to any target count.
func TestBuildPlatformManifestSummarizesAndPricesL3(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	contents := []map[string]string{
		{
			"fmt/print.cs":               "same",
			"fmt/" + PackageInfoFileName: "same",
			"path/filepath/path.cs":      "win",
			"os/" + PackageInfoFileName:  "win",
			"os/" + PackageInitFileName:  "win",
			"internal/goos/nonunix.cs":   "nonunix",
		},
		{
			"fmt/print.cs":               "same",
			"fmt/" + PackageInfoFileName: "same",
			"path/filepath/path.cs":      "unix",
			"os/" + PackageInfoFileName:  "unix",
			"os/" + PackageInitFileName:  "unix",
			"internal/goos/unix.cs":      "unix",
		},
		{
			"fmt/print.cs":               "same",
			"fmt/" + PackageInfoFileName: "same",
			"path/filepath/path.cs":      "unix",
			"os/" + PackageInfoFileName:  "unix",
			"os/" + PackageInitFileName:  "unix",
			"internal/goos/unix.cs":      "unix",
		},
	}

	emissions := make([]*platformEmission, 0, len(targets))

	for i, target := range targets {
		artifacts := map[string]artifactState{}

		for name, hash := range contents[i] {
			artifacts[name] = artifactState{hash: hash, emitted: true}
		}

		// One seeded, never-emitted hand-owned file per root: it must not reach any bucket.
		artifacts["sync/mutex.cs"] = artifactState{hash: "handowned", emitted: false}

		emissions = append(emissions, &platformEmission{
			target:    target,
			goos:      target[:len(target)-len("/amd64")],
			goarch:    "amd64",
			artifacts: artifacts,
		})
	}

	manifest := buildPlatformManifest(Options{}, "seed", "census", emissions)
	summary := manifest.Summary

	if summary.UnionCs != 7 {
		t.Errorf("unionCs = %d, want 7", summary.UnionCs)
	}

	if summary.IdenticalOnAllTargets != 2 {
		t.Errorf("identicalOnAllTargets = %d, want 2 (fmt/print.cs, fmt/package_info.cs)", summary.IdenticalOnAllTargets)
	}

	if summary.VariantSameName != 3 {
		t.Errorf("sameNameDifferentContent = %d, want 3", summary.VariantSameName)
	}

	if summary.PartialEmitted != 1 {
		t.Errorf("emittedBySomeButNotAll = %d, want 1", summary.PartialEmitted)
	}

	if summary.ExclusiveEmitted != 1 {
		t.Errorf("platformExclusive = %d, want 1", summary.ExclusiveEmitted)
	}

	if summary.PackagesWithAnyDelta != 3 {
		t.Errorf("packagesWithAnyDelta = %d, want 3 (path/filepath, os, internal/goos)", summary.PackagesWithAnyDelta)
	}

	wantKinds := map[string]int{"source": 1, PackageInfoFileName: 1, PackageInitFileName: 1}

	if !reflect.DeepEqual(summary.VariantByKind, wantKinds) {
		t.Errorf("variantByKind = %v, want %v", summary.VariantByKind, wantKinds)
	}

	// 3 variants ×3 + 1 pair ×2 + 1 exclusive ×1 = 12
	if summary.PerGoosFiles != 12 {
		t.Errorf("l3PerGoosFiles = %d, want 12", summary.PerGoosFiles)
	}

	if summary.UnionTreeTotal != 14 {
		t.Errorf("l3UnionTreeTotal = %d, want 14", summary.UnionTreeTotal)
	}

	if summary.SinglePlatformTree != 6 {
		t.Errorf("singlePlatformTreeFiles = %d, want 6", summary.SinglePlatformTree)
	}

	if len(manifest.Pairwise) != 3 {
		t.Fatalf("pairwise rows = %d, want 3", len(manifest.Pairwise))
	}

	if got := manifest.VariantSourceFiles; !reflect.DeepEqual(got, []string{"path/filepath/path.cs"}) {
		t.Errorf("variantSourceFiles = %v, want [path/filepath/path.cs]", got)
	}

	if got := manifest.VariantPackageInitFiles; !reflect.DeepEqual(got, []string{"os/" + PackageInitFileName}) {
		t.Errorf("variantPackageInitFiles = %v, want [os/%s]", got, PackageInitFileName)
	}

	// The seeded hand-owned file was never emitted, so it must appear in no bucket at all.
	for _, list := range [][]string{manifest.VariantFiles, manifest.PartialFiles, manifest.ExclusiveFiles} {
		for _, name := range list {
			if name == "sync/mutex.cs" {
				t.Errorf("seeded, never-emitted file reached a census bucket: %s", name)
			}
		}
	}
}

// The manifest is a comparison instrument, so two censuses of the same corpus must produce the same
// bytes: every list it carries is sorted and no timestamp is recorded.
func TestWritePlatformManifestIsDeterministic(t *testing.T) {
	// Enough artifacts per bucket, in deliberately non-alphabetical insertion order, that an
	// unsorted list would betray Go's randomized map iteration between the two builds below.
	windows := map[string]artifactState{}
	linux := map[string]artifactState{}

	for _, name := range []string{"z", "m", "a", "q", "d", "k"} {
		windows[name+"/variant.cs"] = artifactState{hash: "win", emitted: true}
		linux[name+"/variant.cs"] = artifactState{hash: "linux", emitted: true}
		windows[name+"/winonly.cs"] = artifactState{hash: "win", emitted: true}
		linux[name+"/linuxonly.cs"] = artifactState{hash: "linux", emitted: true}
		windows[name+"/shared.cs"] = artifactState{hash: "same", emitted: true}
		linux[name+"/shared.cs"] = artifactState{hash: "same", emitted: true}
	}

	emissions := []*platformEmission{
		{target: "windows/amd64", goos: "windows", goarch: "amd64", artifacts: windows},
		{target: "linux/amd64", goos: "linux", goarch: "amd64", artifacts: linux},
	}

	dir := t.TempDir()
	first := filepath.Join(dir, "first.json")
	second := filepath.Join(dir, "second.json")

	if err := writePlatformManifest(first, buildPlatformManifest(Options{}, "seed", "census", emissions)); err != nil {
		t.Fatalf("writePlatformManifest: %v", err)
	}

	if err := writePlatformManifest(second, buildPlatformManifest(Options{}, "seed", "census", emissions)); err != nil {
		t.Fatalf("writePlatformManifest: %v", err)
	}

	firstBytes, err := os.ReadFile(first)

	if err != nil {
		t.Fatalf("read manifest: %v", err)
	}

	secondBytes, err := os.ReadFile(second)

	if err != nil {
		t.Fatalf("read manifest: %v", err)
	}

	if string(firstBytes) != string(secondBytes) {
		t.Error("two manifests of the same emission set differ — the census is not reproducible")
	}

	var decoded platformManifest

	if err := json.Unmarshal(firstBytes, &decoded); err != nil {
		t.Fatalf("manifest is not valid JSON: %v", err)
	}

	if decoded.Schema != platformManifestSchema {
		t.Errorf("schema = %q, want %q", decoded.Schema, platformManifestSchema)
	}

	for label, list := range map[string][]string{
		"variantFiles":      decoded.VariantFiles,
		"exclusiveFiles":    decoded.ExclusiveFiles,
		"packagesWithDelta": decoded.PackagesWithDelta,
	} {
		if len(list) < 2 {
			t.Fatalf("%s holds %d entries — too few to prove ordering", label, len(list))
		}

		if !sort.StringsAreSorted(list) {
			t.Errorf("%s is not sorted: %v", label, list)
		}
	}
}

// The marker gate is CLAUDE.md's hard pre-overlay check, mechanized: a hand-owned path the run
// emitted as a plain `.cs` means the staging root was not seeded, and every number measured against
// it describes a corpus that does not exist.
func TestMarkerGateViolationsFlagsHandOwnedFileEmittedAsPlainCs(t *testing.T) {
	marked := []string{"sync/mutex.cs", "time/time_impl.cs", "syscall/dll_windows.cs"}

	artifacts := map[string]artifactState{
		"sync/mutex.cs":          {hash: "handowned", emitted: false}, // seeded, diverted to .cs.auto
		"time/time_impl.cs":      {hash: "handowned", emitted: false}, // seeded, never re-emitted
		"syscall/dll_windows.cs": {hash: "auto", emitted: true},       // CLOBBERED
	}

	got := markerGateViolations(marked, artifacts)

	if !reflect.DeepEqual(got, []string{"syscall/dll_windows.cs"}) {
		t.Errorf("markerGateViolations = %v, want [syscall/dll_windows.cs]", got)
	}

	if violations := markerGateViolations(marked, map[string]artifactState{
		"sync/mutex.cs":     {hash: "handowned", emitted: false},
		"time/time_impl.cs": {hash: "handowned", emitted: false},
	}); len(violations) != 0 {
		t.Errorf("markerGateViolations on a correctly seeded root = %v, want none", violations)
	}
}

// Emitted-vs-seeded is decided by the sentinel modification time, NOT by content — the whole point
// of the control target is that its emission reproduces the seed byte for byte, so a content-based
// discriminator would report it as having emitted nothing.
func TestSnapshotConvertedRootSeparatesEmittedFromSeededEvenWhenIdentical(t *testing.T) {
	root := t.TempDir()
	core := filepath.Join(root, "core")

	if err := os.MkdirAll(filepath.Join(core, "fmt"), 0755); err != nil {
		t.Fatalf("mkdir: %v", err)
	}

	seeded := filepath.Join(core, "fmt", "handowned.cs")
	rewritten := filepath.Join(core, "fmt", "print.cs")

	for _, file := range []string{seeded, rewritten} {
		if err := os.WriteFile(file, []byte("identical contents"), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	if err := stampSeedTimes(root); err != nil {
		t.Fatalf("stampSeedTimes: %v", err)
	}

	// Re-write one file with the SAME bytes, exactly as the Windows control run does.
	if err := os.WriteFile(rewritten, []byte("identical contents"), 0644); err != nil {
		t.Fatalf("rewrite: %v", err)
	}

	if err := os.Chtimes(rewritten, time.Now(), time.Now()); err != nil {
		t.Fatalf("chtimes: %v", err)
	}

	artifacts, err := snapshotConvertedRoot(core)

	if err != nil {
		t.Fatalf("snapshotConvertedRoot: %v", err)
	}

	if state, ok := artifacts["fmt/handowned.cs"]; !ok || state.emitted {
		t.Errorf("fmt/handowned.cs state = %+v (present %v), want emitted=false", state, ok)
	}

	if state, ok := artifacts["fmt/print.cs"]; !ok || !state.emitted {
		t.Errorf("fmt/print.cs state = %+v (present %v), want emitted=true", state, ok)
	}

	if artifacts["fmt/print.cs"].hash != artifacts["fmt/handowned.cs"].hash {
		t.Error("identical contents hashed differently")
	}
}

// The seed is a git working tree, so a corpus file holding a multi-line string literal differs from
// its own faithful re-emission by carriage returns alone (autocrlf). That case must land in its own
// bucket, or the CONTROL target of a census seeded with the committed corpus reports a delta it does
// not have — and the reading that certifies the whole instrument stops reading zero.
func TestCompareEmissionToSeedSeparatesLineEndingPhantomFromRealDeltas(t *testing.T) {
	artifacts := map[string]artifactState{
		"fmt/print.cs":      {hash: "unchanged", normalizedHash: "n-unchanged", emitted: true},
		"os/proc.cs":        {hash: "changed", normalizedHash: "n-changed", emitted: true},
		"net/newfile.cs":    {hash: "new", normalizedHash: "n-new", emitted: true},
		"go/build/build.cs": {hash: "lf", normalizedHash: "n-literal", emitted: true}, // CRLF phantom
		"sync/mutex.cs":     {hash: "handowned", normalizedHash: "n-handowned", emitted: false},
		"fmt/fmt.csproj":    {hash: "unchanged", normalizedHash: "n-unchanged", emitted: true},
		"fmt/print.cs.auto": {hash: "auto", normalizedHash: "n-auto", emitted: true},
	}

	seed := map[string]artifactState{
		"fmt/print.cs":      {hash: "unchanged", normalizedHash: "n-unchanged"},
		"os/proc.cs":        {hash: "seeded", normalizedHash: "n-seeded"},
		"go/build/build.cs": {hash: "crlf", normalizedHash: "n-literal"},
		"sync/mutex.cs":     {hash: "handowned", normalizedHash: "n-handowned"},
		"fmt/fmt.csproj":    {hash: "unchanged", normalizedHash: "n-unchanged"},
	}

	matching, lineEndingOnly, differing := compareEmissionToSeed(artifacts, seed)

	if matching != 1 || lineEndingOnly != 1 || differing != 2 {
		t.Errorf("compareEmissionToSeed = (%d matching, %d line-ending-only, %d differing), want (1, 1, 2)",
			matching, lineEndingOnly, differing)
	}
}

// The two digests must answer two different questions: exact bytes for the cross-target comparison
// (both sides fresh emissions of a deterministic converter), carriage-return-insensitive for the
// seed comparison (one side is a working tree autocrlf has smudged).
func TestHashPairDistinguishesExactFromLineEndingInsensitive(t *testing.T) {
	crlf, crlfNormalized := hashPair([]byte("line one\r\nline two\r\n"))
	lf, lfNormalized := hashPair([]byte("line one\nline two\n"))
	other, otherNormalized := hashPair([]byte("line one\nline three\n"))

	if crlf == lf {
		t.Error("exact hashes of CRLF and LF text are equal — the cross-target compare would miss real deltas")
	}

	if crlfNormalized != lfNormalized {
		t.Error("normalized hashes of CRLF and LF text differ — the seed compare would report the autocrlf phantom")
	}

	if other == lf || otherNormalized == lfNormalized {
		t.Error("different content hashed alike")
	}
}

// The hand-owned census must use the converter's OWN predicate, which reads structure rather than
// text: a `[module: GoManualConversion]` mentioned inside a COMMENT (the bodyless-partial
// placeholders in reflect/value.cs) is not a hand-own, and an unanchored grep that counted it would
// turn the marker gate into a false clobber alarm.
func TestReadCensusSeedHashesAndCensusesHandOwnedFiles(t *testing.T) {
	core := t.TempDir()

	mustWrite := func(relPath, contents string) {
		t.Helper()
		file := filepath.Join(core, filepath.FromSlash(relPath))

		if err := os.MkdirAll(filepath.Dir(file), 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}

		if err := os.WriteFile(file, []byte(contents), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	mustWrite("sync/mutex.cs", "using go;\r\n\r\n[module: go.GoManualConversion]\r\n\r\npartial class sync_package {}\r\n")
	mustWrite("time/time_impl.cs", "[module: GoManualConversion]\r\npartial class time_package {}\r\n")
	mustWrite("reflect/value.cs", "// A placeholder mentioning [module: go.GoManualConversion] in prose.\r\npartial class reflect_package {}\r\n")
	mustWrite("fmt/print.cs", "partial class fmt_package {}\r\n")
	mustWrite("fmt/fmt.csproj", "<Project />")

	seed, err := readCensusSeed(core)

	if err != nil {
		t.Fatalf("readCensusSeed: %v", err)
	}

	if want := []string{"sync/mutex.cs", "time/time_impl.cs"}; !reflect.DeepEqual(seed.markedFiles, want) {
		t.Errorf("markedFiles = %v, want %v", seed.markedFiles, want)
	}

	if len(seed.hashes) != 5 {
		t.Errorf("hashed %d files, want 5 (every file, not just .cs)", len(seed.hashes))
	}

	if seed.hashes["fmt/print.cs"].hash == "" || seed.hashes["fmt/print.cs"].normalizedHash == "" {
		t.Error("fmt/print.cs was not hashed both ways")
	}

	if seed.hashes["fmt/print.cs"].hash == seed.hashes["fmt/fmt.csproj"].hash {
		t.Error("different contents hashed identically")
	}
}

func TestReadFailedPackagesReadsTheResumeList(t *testing.T) {
	root := t.TempDir()

	if got := readFailedPackages(root); len(got) != 0 {
		t.Errorf("readFailedPackages(no file) = %v, want none", got)
	}

	if err := os.WriteFile(filepath.Join(root, "failed_packages.txt"), []byte("net/http\n\nruntime\n"), 0644); err != nil {
		t.Fatalf("write: %v", err)
	}

	if got := readFailedPackages(root); !reflect.DeepEqual(got, []string{"net/http", "runtime"}) {
		t.Errorf("readFailedPackages = %v, want [net/http runtime]", got)
	}
}

// The seeding ritual is what makes a census measure the real corpus, so it must reproduce the
// repository's layout exactly: `core` and `version.props` under the go2cs root, and `docs/validation`
// as that root's SIBLING — the two places the converter's upward walks look.
func TestSeedCensusRootMirrorsRepositoryLayout(t *testing.T) {
	repo := t.TempDir()
	seedRoot := filepath.Join(repo, "src")

	mustWrite := func(parts ...string) {
		t.Helper()
		file := filepath.Join(parts...)

		if err := os.MkdirAll(filepath.Dir(file), 0755); err != nil {
			t.Fatalf("mkdir: %v", err)
		}

		if err := os.WriteFile(file, []byte("x"), 0644); err != nil {
			t.Fatalf("write: %v", err)
		}
	}

	mustWrite(seedRoot, "core", "golib", "golib.csproj")
	mustWrite(seedRoot, "core", "fmt", "print.cs")
	mustWrite(seedRoot, versionPropsFileName)
	mustWrite(repo, "docs", validationDocsDirName, validationCurrentDirName, "fmt.md")

	stage := filepath.Join(t.TempDir(), "windows-amd64")

	copied, err := seedCensusRoot(seedRoot, stage)

	if err != nil {
		t.Fatalf("seedCensusRoot: %v", err)
	}

	if copied != 4 {
		t.Errorf("seeded %d files, want 4", copied)
	}

	for _, expected := range []string{
		filepath.Join(stage, "src", "core", "golib", "golib.csproj"),
		filepath.Join(stage, "src", "core", "fmt", "print.cs"),
		filepath.Join(stage, "src", versionPropsFileName),
		filepath.Join(stage, "docs", validationDocsDirName, validationCurrentDirName, "fmt.md"),
	} {
		if _, err := os.Stat(expected); err != nil {
			t.Errorf("seeded root is missing %s: %v", expected, err)
		}
	}

	// The staging root must satisfy the converter's own root predicate, or the README badge and the
	// project references it composes would resolve against nothing.
	if !isGo2CSRoot(filepath.Join(stage, "src")) {
		t.Error("seeded staging root is not recognized as a go2cs root")
	}
}

// A seeded staging root exists so the converter can read the tree it is about to rewrite: the
// hand-own markers it must not clobber, the package_info.cs files it mints <ImportedTypeAliases>
// from, and the bytes needToWriteFile compares against. None of that lives in a build-output
// directory, and copying them was measured at ~57k files per target on a worked-in clone — three
// times that per multi-platform regen, the single largest cost in the run.
func TestCopyDirTreeSkipsBuildOutputs(t *testing.T) {
	source := t.TempDir()
	destination := filepath.Join(t.TempDir(), "seeded")

	writeTestFile(t, filepath.Join(source, "pkg", "thing.cs"), "// a converted source\n")
	writeTestFile(t, filepath.Join(source, "pkg", "package_info.cs"), "// metadata the converter READS\n")
	writeTestFile(t, filepath.Join(source, "pkg", "pkg.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(source, "pkg", "bin", "Debug", "pkg.dll"), "binary")
	writeTestFile(t, filepath.Join(source, "pkg", "obj", "project.assets.json"), "{}")
	writeTestFile(t, filepath.Join(source, "pkg", "Generated", "go2cs-gen", "thing.g.cs"), "// generator output\n")

	copied, err := copyDirTree(source, destination)

	if err != nil {
		t.Fatalf("copyDirTree returned an error: %v", err)
	}

	if copied != 3 {
		t.Errorf("copied = %d, want 3 (the sources and the project file, none of the build outputs)", copied)
	}

	for _, seeded := range []string{
		filepath.Join(destination, "pkg", "thing.cs"),
		filepath.Join(destination, "pkg", "package_info.cs"),
		filepath.Join(destination, "pkg", "pkg.csproj"),
	} {
		if _, err := os.Stat(seeded); err != nil {
			t.Errorf("%s was not seeded, but the converter reads it: %v", seeded, err)
		}
	}

	for _, skipped := range []string{
		filepath.Join(destination, "pkg", "bin"),
		filepath.Join(destination, "pkg", "obj"),
		filepath.Join(destination, "pkg", "Generated"),
	} {
		if _, err := os.Stat(skipped); !os.IsNotExist(err) {
			t.Errorf("%s was seeded; build outputs are not conversion inputs and cost ~57k files per target", skipped)
		}
	}
}
