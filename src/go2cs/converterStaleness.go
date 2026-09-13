// converterStaleness.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"sync"
	"time"
)

// ---------------------------------------------------------------------------------------------
// THE STALE-BINARY SELF-CHECK — CLAUDE.md's false-green route #1, closed from INSIDE the binary.
//
// WHY IT IS HERE AND NOT IN A CALLER
//   Three harnesses already refuse to run a stale converter: BehavioralRunner, BehavioralTestBase
//   and PerformanceRunner all consult ConverterBuildInputs.IsConverterStale, and CNR plus the
//   validated sweep's default arm rebuild unconditionally. A 2026-08-31 census found the paths that
//   consult NOTHING, and they share one property — there is no caller to instrument:
//
//     1. `go2cs -tests -test-action convert|all` run by hand from a lane shell. THIS IS THE
//        INCIDENT PATH: it produced a `std.reflect.csproj` pair on 2026-08-30 from a binary
//        predating 433e9e4e0's GOROOT normalization, wrote them beside the committed project, and
//        exited 0. The artifacts survived ten hours and then aborted an unrelated CNR's
//        solution-integrity preflight on a duplicate `reflect` graph node — which is the only
//        reason anyone noticed.
//     2. `go2cs <pkg>` / `go2cs -stdlib` run by hand.
//     3. `run-validated-sweep.ps1 -SkipBuild`, whose `Test-Path` asks existence, never freshness.
//
//   The caller in cases 1 and 2 is a human at a shell, and IsConverterStale is C# this binary
//   cannot call. So the check moves inside: one probe at startup covers every invocation path that
//   exists or ever will, including the two no external predicate can reach.
//
// WHY IT ENUMERATES INSTEAD OF NAMING ONE FILE
//   The first version named the single NEWEST input and left the extent unstated. A 2026-09-03
//   hand-invoked `-tests` run met exactly that: the advisory named `manualTypeOperations.go` where
//   six inputs were newer, four of them (importOperations.go, visitImportSpec.go, visitTypeSpec.go,
//   writeOperations.go) affecting every package's emission — and the lane reasoned from the one
//   name it was given ("that entry only touches syscall") to the conclusion that the measurement
//   stood. A diagnostic that reports one member of a set invites exactly that inference, so the
//   report now carries the COUNT, the newest ten paths (all of them when there are ten or fewer),
//   and which of them are emission-affecting. Same shape as every census rule in this repo: an
//   instrument that under-reports is worse than one that says nothing, because its answer gets
//   reasoned from.
//
// WHY IT REFUSES FOR -stdlib AND -tests, AND ONLY ADVISES OTHERWISE
//   The original text here argued the check must never be fatal, on the -go2cspath unresolved-root
//   precedent, because a deliberately pinned binary and a deployed binary are both legitimate. Both
//   of those remain true and are the reason this file still has an advisory at all — but the
//   incident above is the counter-case the argument did not cover: the two whole-corpus drivers
//   produce output that is BANKED or MEASURED, so a stale run there does not merely risk being
//   wrong, it manufactures a right-looking number from the previous converter and exits 0. A
//   warning is the correct instrument for a scratch probe and the wrong one for a measurement.
//
//   So the shape is split by what the run's output is FOR:
//
//     -stdlib / -tests        REFUSE. Corpus emission and the Phase-4 pipeline; their artifacts are
//                             committed, swept and compared against goldens.
//     single file / package   ADVISE. This is the scratch-probe shape — `go2cs example.go`,
//                             `go2cs <pkg-dir>` while iterating on a converter change — where the
//                             operator is looking at the output directly, a deliberately pinned
//                             binary is ordinary, and a fatal would break the fastest loop in the
//                             project to catch a mistake the warning names just as well.
//     -recurse                ADVISE (unchanged). Deliberately NOT widened here: it is the one mode
//                             an outside user runs, usually from a deployed binary where this probe
//                             is silent anyway, and nothing in the incident touched it. Widening it
//                             is a separate decision with its own evidence.
//
//   The refusal is escapable by ONE named flag, -allow-stale-converter, and never by anything
//   ambient. That flag exists for the documented deliberate case: an A/B against a PRESERVED
//   binary, where the pinned binary IS the measurement (CLAUDE.md's three-run flake standard —
//   "swap the PRESERVED pre-change go2cs.exe into the sweep path instead"). Because it must be
//   typed, a run that proceeds stale says so in its own command line.
//
// HOW THE BINARY KNOWS WHEN IT WAS BUILT — and the holes, recorded rather than papered
//   The reference instant is the EXECUTABLE'S OWN mtime (os.Stat on os.Executable), compared
//   against each build input's mtime. Nothing is embedded and nothing is hashed; this is the same
//   comparison ConverterBuildInputs.IsConverterStale makes from the C# side.
//
//     - `go build -o <path>` WRITES the output file every time, cache hit or not (the cached link
//       result is copied out to <path>), so a cached rebuild still stamps a current mtime. The
//       comparison is therefore honest after a no-op rebuild.
//     - A copy that PRESERVES mtimes (PowerShell Copy-Item, `cp -p`) carries the original build
//       instant with it, so a preserved binary is still measured against the moment it was built.
//     - HOLE: a copy that does NOT preserve mtimes — plain `cp` from a POSIX shell, which stamps
//       the copy with the current time — makes a stale binary look FRESH. It only bites when the
//       copy is then run from inside a converter source tree, since the probe is silent otherwise;
//       -allow-stale-converter is unaffected, because that path is about proceeding, not detecting.
//     - HOLE: mtime measures MODIFICATION, not content. A `git checkout` that restores a file to
//       byte-identical content still bumps its mtime, so a branch switch can make this fire on a
//       tree whose sources did not really change. That false positive is the second reason the
//       escape flag is named rather than removed.
//     - HOLE (deliberate, route #4's half): no toolchain comparison. runtime.Version() is free
//       here, but the only cheap in-process thing to compare it against is go.mod's `go` directive,
//       which is a MINIMUM-version declaration and not a build pin, so any legitimately newer
//       toolchain would false-refuse. Route #4 is answered exactly — and authoritatively — by the
//       harness predicate comparing the embedded stamp against the live `go env GOVERSION`. A
//       weaker, false-positive-prone duplicate inside the binary would be worse than not having
//       one, so this covers the mtime half only and says so. A toolchain hop therefore leaves a
//       binary this refusal cannot see as stale.
//
// THE INPUT SET mirrors ConverterBuildInputs.cs, which is the ONE definition of what the binary is
// built from: every .go under the converter source root (internal/ included), the //go:embed assets
// those files name, and go.mod/go.sum. Route #5 is why it is not a top-level *.go scan — editing a
// csproj template or stdlib-metadata.txt changes every project the converter emits while touching
// no .go file at all.
// ---------------------------------------------------------------------------------------------

// maxListedStaleInputs caps the enumeration. Ten names is enough to see the SHAPE of what changed
// — whether the newer inputs are one package's business or spread across importOperations.go and
// the visit*.go walkers — and the count line carries the rest, so nothing is hidden by the cap.
const maxListedStaleInputs = 10

// staleCheckOnce keeps the probe and its report to once per process, however many times a
// conversion path asks.
var staleCheckOnce sync.Once

// inputStamp is one converter build input and the time it was last modified.
type inputStamp struct {
	path     string
	modified time.Time
}

// staleInput is a build input NEWER than the running binary: what the report enumerates.
type staleInput struct {
	// relPath is the path relative to the converter source root. Relative on purpose — it is the
	// spelling a reader can act on, it is stable across machines, and it keeps an absolute profile
	// path out of a diagnostic that regularly gets pasted onto a shared surface.
	relPath string

	modified time.Time

	// affectsEmission distinguishes an input that changes what go2cs.exe EMITS from one that
	// cannot. The only inputs that cannot are `_test.go` files: `go build` excludes them, so they
	// are compiled into the converter's own test binary and never into the converter. Everything
	// else in the set qualifies — production .go, the //go:embed assets (route #5's whole point:
	// a csproj template or stdlib-metadata.txt edit changes every project emitted while touching
	// no .go file), and go.mod/go.sum, whose dependency changes alter the built binary exactly as
	// a source edit does.
	affectsEmission bool
}

// stalenessReport is the whole answer to "is this binary current, and if not, by how much".
type stalenessReport struct {
	sourceDir string
	builtAt   time.Time

	// inputs is newest first, ties broken by path so the report is deterministic.
	inputs []staleInput
}

// checkConverterStaleness is the ONE entry point main() calls. `converting` says whether this run
// is one of the two drivers whose output is banked or measured (-stdlib, -tests) — those refuse;
// every other shape gets the advisory. allowStale is -allow-stale-converter, the single named
// escape.
func checkConverterStaleness(converting bool, allowStale bool) {
	staleCheckOnce.Do(func() {
		report, stale := probeConverterStaleness()

		if !stale {
			return
		}

		if stalenessRefuses(stale, converting, allowStale) {
			log.Fatalf("%s\n", report.refusal())
		}

		showWarning("%s", report.advisory(converting && allowStale))
	})
}

// stalenessRefuses is the decision itself, split out from the fatal so it can be asserted: a stale
// binary stops a -stdlib or -tests run unless the operator named -allow-stale-converter.
func stalenessRefuses(stale bool, converting bool, allowStale bool) bool {
	return stale && converting && !allowStale
}

// probeConverterStaleness answers whether this executable is older than any of the converter
// sources it was built from, and enumerates every input that is. Reports NOT stale when no
// converter source tree sits adjacent to the executable (a deployed or relocated binary), and when
// anything cannot be answered — an unanswerable probe must not refuse a run, so it says nothing.
func probeConverterStaleness() (*stalenessReport, bool) {
	executable, err := os.Executable()

	if err != nil {
		return nil, false
	}

	sourceDir, ok := adjacentConverterSource(executable)

	if !ok {
		return nil, false
	}

	info, err := os.Stat(executable)

	if err != nil {
		return nil, false
	}

	builtAt := info.ModTime()
	inputs := staleConverterInputs(sourceDir, builtAt)

	if len(inputs) == 0 {
		return nil, false
	}

	return &stalenessReport{sourceDir: sourceDir, builtAt: builtAt, inputs: inputs}, true
}

// staleConverterInputs returns every build input under root modified after builtAt, newest first.
func staleConverterInputs(root string, builtAt time.Time) []staleInput {
	var stale []staleInput

	for _, stamp := range converterBuildInputs(root) {
		if !stamp.modified.After(builtAt) {
			continue
		}

		relPath, err := filepath.Rel(root, stamp.path)

		if err != nil {
			relPath = filepath.Base(stamp.path)
		}

		stale = append(stale, staleInput{
			relPath:         relPath,
			modified:        stamp.modified,
			affectsEmission: !strings.HasSuffix(strings.ToLower(filepath.Base(stamp.path)), "_test.go"),
		})
	}

	sort.SliceStable(stale, func(i, j int) bool {
		if stale[i].modified.Equal(stale[j].modified) {
			// Filesystem timestamp granularity ties several files touched in one operation; order
			// them by path so the enumeration is reproducible rather than walk-order dependent.
			return stale[i].relPath < stale[j].relPath
		}

		return stale[i].modified.After(stale[j].modified)
	})

	return stale
}

// converterBuildInputs enumerates every file the binary is built from under root, deduplicated —
// an asset can be named by //go:embed directives in two different sources, and a count that
// double-reported it would be reporting the directives, not the inputs.
func converterBuildInputs(root string) []inputStamp {
	var (
		inputs []inputStamp
		seen   = map[string]struct{}{}
	)

	consider := func(path string) {
		info, err := os.Stat(path)

		if err != nil || info.IsDir() {
			return
		}

		key := inputKey(path)

		if _, duplicate := seen[key]; duplicate {
			return
		}

		seen[key] = struct{}{}
		inputs = append(inputs, inputStamp{path: path, modified: info.ModTime()})
	}

	_ = filepath.WalkDir(root, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return nil
		}

		if entry.IsDir() {
			if path != root && isSkippedInputDirectory(entry.Name()) {
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(entry.Name(), ".go") {
			return nil
		}

		consider(path)

		for _, asset := range embeddedAssetPaths(path) {
			consider(asset)
		}

		return nil
	})

	for _, moduleFile := range []string{"go.mod", "go.sum"} {
		consider(filepath.Join(root, moduleFile))
	}

	return inputs
}

// inputKey is the identity two enumerations of the same file must agree on. ConverterBuildInputs.cs
// dedupes case-insensitively; here the fold is applied only where the filesystem itself is
// case-insensitive, since elsewhere `Foo.go` and `foo.go` are two files and merging them would
// under-count.
func inputKey(path string) string {
	cleaned := filepath.Clean(path)

	if runtime.GOOS == "windows" {
		return strings.ToLower(cleaned)
	}

	return cleaned
}

// newestConverterInput returns the most recent modification time across every build input under
// root, and the path that carried it. An empty path means nothing was readable. Kept beside the
// enumeration because "what is the newest input" is still the cheapest question to ask of a tree.
func newestConverterInput(root string) (time.Time, string) {
	var (
		newest     time.Time
		newestPath string
	)

	for _, stamp := range converterBuildInputs(root) {
		if stamp.modified.After(newest) {
			newest, newestPath = stamp.modified, stamp.path
		}
	}

	return newest, newestPath
}

// emissionAffecting counts the stale inputs that change what the converter emits.
func (report *stalenessReport) emissionAffecting() int {
	count := 0

	for _, input := range report.inputs {
		if input.affectsEmission {
			count++
		}
	}

	return count
}

// body renders the enumeration shared by both messages: the extent, the newest paths, and the
// emission-affecting split. indent aligns continuation lines under the caller's own prefix.
func (report *stalenessReport) body(indent string) string {
	var (
		builder  strings.Builder
		total    = len(report.inputs)
		emission = report.emissionAffecting()
		listed   = total
	)

	fmt.Fprintf(&builder, "go2cs is OLDER than its own sources: %s modified after this binary was built,\n",
		pluralInputs(total))

	fmt.Fprintf(&builder, "%sso this run emits the PREVIOUS converter's output while reporting success.\n", indent)
	fmt.Fprintf(&builder, "%sNewer than the binary, newest first (* marks emission-affecting):\n", indent)

	if listed > maxListedStaleInputs {
		listed = maxListedStaleInputs
	}

	for _, input := range report.inputs[:listed] {
		marker := " "

		if input.affectsEmission {
			marker = "*"
		}

		fmt.Fprintf(&builder, "%s  %s %s\n", indent, marker, input.relPath)
	}

	if total > listed {
		fmt.Fprintf(&builder, "%s    ... and %d more\n", indent, total-listed)
	}

	fmt.Fprintf(&builder, "%s%d of %d are emission-affecting: every build input except a _test.go file changes\n", indent, emission, total)
	fmt.Fprintf(&builder, "%swhat go2cs.exe emits (a _test.go file is compiled into the converter's test binary,\n", indent)
	fmt.Fprintf(&builder, "%snot into the converter itself).", indent)

	return builder.String()
}

// refusal is what a -stdlib or -tests run says before exiting. It names the escape flag explicitly:
// the deliberate case (an A/B on a preserved binary) has to remain reachable, and a refusal that
// does not say how to proceed teaches the reader to reach for something worse.
func (report *stalenessReport) refusal() string {
	const indent = "       "

	return fmt.Sprintf("%s\n"+
		"%sREFUSING: a -stdlib or -tests run banks or measures its output, so emitting the previous\n"+
		"%sconverter's C# and exiting 0 would publish a right-looking number from the wrong binary.\n"+
		"%sRebuild with \"go build -o bin/go2cs%s .\" in %s,\n"+
		"%sor pass -allow-stale-converter to proceed deliberately — that flag is for an A/B against a\n"+
		"%sPRESERVED binary, where the pinned binary IS the measurement.",
		report.body(indent), indent, indent, indent, hostExeSuffix(), report.sourceDir, indent, indent)
}

// advisory is what every other shape says while running anyway. allowed distinguishes a run that
// WOULD have been refused and was let through by the flag, so the escape appears in the log of the
// run it applied to rather than only in the command line.
func (report *stalenessReport) advisory(allowed bool) string {
	const indent = "         "

	trailer := fmt.Sprintf("%sRebuild with \"go build -o bin/go2cs%s .\" in %s, or ignore this if the binary is pinned deliberately.",
		indent, hostExeSuffix(), report.sourceDir)

	if allowed {
		trailer = fmt.Sprintf("%sProceeding because -allow-stale-converter was passed: this run's output comes from the\n"+
			"%sbinary as it stands, which is what an A/B on a preserved converter wants and what nothing\n"+
			"%selse does.", indent, indent, indent)
	}

	return fmt.Sprintf("%s\n%s", report.body(indent), trailer)
}

// pluralInputs keeps the count line grammatical at one.
func pluralInputs(count int) string {
	if count == 1 {
		return "1 converter build input was"
	}

	return fmt.Sprintf("%d converter build inputs were", count)
}

// hostExeSuffix is this host's executable suffix, so the remedy line names a command that works
// where it is read rather than a Windows one everywhere.
func hostExeSuffix() string {
	if strings.EqualFold(filepath.Ext(os.Args[0]), ".exe") {
		return ".exe"
	}

	return ""
}

// adjacentConverterSource resolves the converter source root from the executable's own location -
// the `bin/go2cs<exe>` to `src/go2cs` walk, the same shape resolveGo2CSPath already uses to
// self-locate a go2cs root. It is confirmed by MARKER FILES rather than by name, so a binary that
// merely happens to sit in some `bin` directory is never measured against an unrelated tree.
func adjacentConverterSource(executable string) (string, bool) {
	if candidate := filepath.Dir(filepath.Dir(executable)); isConverterSourceRoot(candidate) {
		return candidate, true
	}

	// A binary built into the source root itself (`go build` with no -o) has one less level
	// between it and the sources.
	if candidate := filepath.Dir(executable); isConverterSourceRoot(candidate) {
		return candidate, true
	}

	return "", false
}

// isConverterSourceRoot reports whether dir is THIS converter's source, not merely a Go module.
// Both markers are required: go.mod alone would match any module the binary happened to sit inside.
func isConverterSourceRoot(dir string) bool {
	if info, err := os.Stat(filepath.Join(dir, "main.go")); err != nil || info.IsDir() {
		return false
	}

	data, err := os.ReadFile(filepath.Join(dir, "go.mod"))

	if err != nil {
		return false
	}

	for _, line := range strings.Split(string(data), "\n") {
		if strings.TrimSpace(line) == "module go2cs" {
			return true
		}
	}

	return false
}

// isSkippedInputDirectory mirrors ConverterBuildInputs.cs: the three Go itself ignores when loading
// packages, plus the build output. `bin` is where the executable is written, so walking it would
// compare the binary against itself.
func isSkippedInputDirectory(name string) bool {
	return name == "" || name[0] == '.' || name[0] == '_' ||
		strings.EqualFold(name, "testdata") ||
		strings.EqualFold(name, "bin") ||
		strings.EqualFold(name, "obj")
}

// embeddedAssetPaths resolves the files a Go source file names in //go:embed directives, relative
// to that file's own directory. A directory match expands to its contents, which is what go:embed
// itself does. Derived from the directives rather than listed anywhere, so an asset added tomorrow
// is covered the day it is written - the same reasoning ConverterBuildInputs.cs gives, and the
// directive FORMS both rely on are pinned by embeddedAssets_test.go.
func embeddedAssetPaths(goFile string) []string {
	file, err := os.Open(goFile)

	if err != nil {
		return nil
	}

	defer file.Close()

	var (
		assets  []string
		fileDir = filepath.Dir(goFile)
		scanner = bufio.NewScanner(file)
	)

	scanner.Buffer(make([]byte, 0, 64*1024), 1024*1024)

	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())

		if !strings.HasPrefix(line, "//go:embed") {
			continue
		}

		// Go requires whitespace after the directive, and so do the other two derivations of this
		// set (ConverterBuildInputs.cs and embeddedAssets_test.go's own scanner). Without the same
		// guard here, prose beginning `//go:embedded ...` would be read as a directive naming the
		// pattern `ded` — harmless today because it resolves to nothing, but a third derivation
		// that disagrees with the other two about what a directive IS is the seed of a census
		// that under-reports.
		remainder := line[len("//go:embed"):]

		if remainder != "" && !strings.ContainsAny(remainder[:1], " \t") {
			continue
		}

		for _, pattern := range strings.Fields(remainder) {
			pattern = strings.Trim(pattern, "\"")
			pattern = strings.TrimPrefix(pattern, "all:")

			if pattern == "" {
				continue
			}

			matches, globErr := filepath.Glob(filepath.Join(fileDir, filepath.FromSlash(pattern)))

			if globErr != nil {
				continue
			}

			for _, match := range matches {
				info, statErr := os.Stat(match)

				if statErr != nil {
					continue
				}

				if !info.IsDir() {
					assets = append(assets, match)
					continue
				}

				_ = filepath.WalkDir(match, func(path string, entry os.DirEntry, walkErr error) error {
					if walkErr == nil && !entry.IsDir() {
						assets = append(assets, path)
					}

					return nil
				})
			}
		}
	}

	return assets
}
