// suppressionCompanion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The L3 invariant this file guards: EVERY GOOS an entry in manualConversionFuncs is scoped to must
// have a hand-own companion in the corpus providing what that GOOS's emission suppresses.
//
// The defect is the mirror image of platformHandOwn_test.go's ("a question never asked"): there, a
// hand-own existed but kept a placement the emission had moved out from under it. HERE, the
// suppression fires for a GOOS that has no companion at all — the converter dutifully drops the auto
// body, writes its placeholder comment, and nothing provides the declaration. The package then fails
// to compile on that target and ONLY on that target, so every gate on every other flavor stays green
// while the corpus is broken for a platform nobody builds locally.
//
// That is not hypothetical: it is exactly what the FIRST darwin CI census found (2026-08-23, run
// 32611912106). `os`'s File.readdir was scoped to windows AND darwin, `windows/dir_windows_impl.cs`
// existed, and darwin had only the placeholder — 19 errors on both mac legs, all three dir.cs call
// sites, invisible to every Windows and Linux gate the fleet runs. The companion landed the same day
// (os/darwin/dir_darwin_impl.cs); this guard is what makes the NEXT one impossible to add silently,
// because it fails in the plain `go test ./...` every converter change already runs.
//
// The check is deliberately corpus-reading rather than synthetic, for platformHandOwn_test.go's own
// stated reason: the next gap will be a scope somebody widens or a GOOS somebody adds, and only a
// walk of src/core can see it.

package main

import (
	"os"
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

// TestScopedSuppressionsHaveCompanions walks manualConversionFuncs and asserts that for every
// (package, GOOS) whose emission is suppressed, the corpus carries a hand-own that can provide the
// suppressed declaration on that GOOS.
//
// "Can provide" is judged the way the corpus itself is organized (CLAUDE.md, corpus mechanics): a
// hand-own for a per-GOOS-varying package lives in that package's `<goos>/` folder, and a hand-own
// for a platform-neutral package lives flat beside the file it supplements. Either shape counts;
// what does not count is nothing at all.
func TestScopedSuppressionsHaveCompanions(t *testing.T) {
	corpus := filepath.Join("..", "core")

	if info, err := os.Stat(filepath.Join(corpus, "golib", "golib.csproj")); err != nil || !info.Mode().IsRegular() {
		t.Skip("src/core is not beside the converter; nothing to walk")
	}

	type gap struct {
		pkg, goos string
		members   []string
	}

	var gaps []gap

	for pkgPath, members := range manualConversionFuncs {
		// Which targets does any scoped member suppress on?
		suppressedOn := map[string][]string{}

		for member, scope := range members {
			for _, goos := range knownTargetGOOS {
				if scope.includes(goos) {
					suppressedOn[goos] = append(suppressedOn[goos], member)
				}
			}
		}

		for goos, suppressed := range suppressedOn {
			pkgDir := filepath.Join(corpus, filepath.FromSlash(pkgPath))

			// A package that is not in this corpus at all (a scope naming something unconverted)
			// is not this guard's business — the roster guards that.
			if _, err := os.Stat(pkgDir); err != nil {
				continue
			}

			// The package may not exist for this GOOS: no per-GOOS folder AND no flat sources is
			// not a gap, it is a package that target does not build.
			perGoos := filepath.Join(pkgDir, goos)
			hasPerGoos := false

			if info, err := os.Stat(perGoos); err == nil && info.IsDir() {
				hasPerGoos = true
			}

			// A companion in the TARGET's own folder is the answer for a per-GOOS-varying package.
			if hasCompanion(t, perGoos) {
				continue
			}

			// A flat companion counts ONLY for a package with no per-GOOS folders at all — a
			// platform-neutral package whose hand-own serves every target. Accepting a flat marker
			// for a package that DOES vary per GOOS is what made the first draft of this guard
			// vacuous: `os` carries flat `tempfile_impl.cs`, so every target looked covered and
			// removing `darwin/dir_darwin_impl.cs` outright still passed. A marker in a sibling's
			// folder (or a neutral file) cannot provide a declaration the target's own emission
			// suppressed.
			if !hasAnyPlatformFolder(t, pkgDir) && hasCompanion(t, pkgDir) {
				continue
			}

			// A FLAT companion that DECLARES every suppressed member answers for every target,
			// because a flat file is compiled on every target — the routing guard's own rule
			// (platformHandOwn_test.go: a shared principal's companion stays flat, one file, not
			// three) seen from this side. Existence alone is NOT enough here, which is what the
			// `os` lesson above is about: `tempfile_impl.cs` is flat and marked and declares no
			// `readdir`, so it must not answer for one. The file has to name the member. Added
			// 2026-09-05 for time's syncTimer (Q44): a flat principal (sleep.cs) in an L3 package,
			// a shape neither arm above could accept and the routing guard forbids duplicating.
			if hasFlatSources(t, pkgDir) && flatCompanionDeclares(t, pkgDir, suppressed) {
				continue
			}

			// A package whose sources are entirely per-GOOS and has no folder for this target
			// simply is not built there.
			if !hasPerGoos && !hasFlatSources(t, pkgDir) {
				continue
			}

			sort.Strings(suppressed)
			gaps = append(gaps, gap{pkg: pkgPath, goos: goos, members: suppressed})
		}
	}

	if len(gaps) == 0 {
		return
	}

	sort.Slice(gaps, func(i, j int) bool {
		if gaps[i].pkg != gaps[j].pkg {
			return gaps[i].pkg < gaps[j].pkg
		}

		return gaps[i].goos < gaps[j].goos
	})

	var b strings.Builder
	b.WriteString("manualConversionFuncs suppresses emission with no hand-own companion to provide it:\n")

	for _, g := range gaps {
		b.WriteString("  " + g.pkg + " on " + g.goos + " — suppressed: " + strings.Join(g.members, ", ") + "\n")
		b.WriteString("      expected a companion under src/core/" + g.pkg + "/" + g.goos + "/ (or flat, for a\n")
		b.WriteString("      platform-neutral package) carrying [module: GoManualConversion]\n")
	}

	b.WriteString("\nEach gap compiles clean on every OTHER target and fails only on the named one — the\n")
	b.WriteString("shape the first darwin census found in os (19 errors, invisible to every local gate).\n")
	b.WriteString("Either author the companion, or narrow the entry's goosScope to the targets it serves.")

	t.Fatal(b.String())
}

// knownTargetGOOS is the set of targets the corpus emits for (the -platforms triple). A new target
// joins here when the corpus starts emitting it, which is deliberately a decision rather than a
// discovery: this guard's whole point is that adding a target must confront its hand-own debts.
var knownTargetGOOS = []string{"windows", "linux", "darwin"}

// hasCompanion reports whether dir directly contains a hand-owned file — one carrying the module
// marker. Only the directory itself is read; per-GOOS routing means a companion for target X lives
// in X's own folder, never in a sibling's.
func hasCompanion(t *testing.T, dir string) bool {
	t.Helper()

	entries, err := os.ReadDir(dir)
	if err != nil {
		return false
	}

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".cs") {
			continue
		}

		marked, err := containsManualConversionMarker(filepath.Join(dir, entry.Name()))
		if err != nil {
			continue
		}

		if marked {
			return true
		}
	}

	return false
}

// flatCompanionDeclares reports whether the package's FLAT hand-own companions — marked files and
// `*_impl.cs` companions directly in the package directory, which every target compiles — declare
// every one of the suppressed members. A method entry ("recvType.method") is looked up by its
// method name. The predicate is deliberately textual (the member name followed by `(`): the
// companion is C#, and this guard runs where no C# compiler is, so "declares" means "spells the
// declaration", the same reading the emitted placeholder's own text invites.
func flatCompanionDeclares(t *testing.T, dir string, members []string) bool {
	t.Helper()

	entries, err := os.ReadDir(dir)

	if err != nil {
		return false
	}

	var companions []string

	for _, entry := range entries {
		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".cs") {
			continue
		}

		path := filepath.Join(dir, entry.Name())
		marked, err := containsManualConversionMarker(path)

		if err != nil {
			continue
		}

		if marked || strings.HasSuffix(entry.Name(), "_impl.cs") {
			content, err := os.ReadFile(path)

			if err != nil {
				continue
			}

			companions = append(companions, string(content))
		}
	}

	if len(companions) == 0 {
		return false
	}

	for _, member := range members {
		name := member

		if dot := strings.LastIndex(member, "."); dot >= 0 {
			name = member[dot+1:]
		}

		declared := false

		for _, content := range companions {
			if declaresStaticMember(content, name) {
				declared = true
				break
			}
		}

		if !declared {
			return false
		}
	}

	return true
}

// declaresStaticMember reports whether some NON-comment line of a C# companion declares `name` as a
// static member: the line carries `static ` and `name(`. A doc comment quoting the member ("Go's
// syncTimer(c)") is not a declaration — the first form of this predicate matched exactly that, and
// its negative control (the body renamed) stayed green until the comment was excluded.
func declaresStaticMember(content, name string) bool {
	for _, line := range strings.Split(content, "\n") {
		trimmed := strings.TrimSpace(line)

		if strings.HasPrefix(trimmed, "//") {
			continue
		}

		if strings.Contains(trimmed, "static ") && strings.Contains(trimmed, name+"(") {
			return true
		}
	}

	return false
}

// TestFlatCompanionDeclaresNamesTheMember is flatCompanionDeclares's own control: a flat `_impl.cs`
// that spells the member answers, one that does not — or a package with no companion at all — does
// not, and a method entry is matched by its method name.
func TestFlatCompanionDeclaresNamesTheMember(t *testing.T) {
	dir := t.TempDir()

	if err := os.WriteFile(filepath.Join(dir, "sleep_impl.cs"), []byte("partial class x { internal static int syncTimer(int c) { return 0; } }"), 0o644); err != nil {
		t.Fatal(err)
	}

	if !flatCompanionDeclares(t, dir, []string{"syncTimer"}) {
		t.Errorf("a flat _impl.cs spelling syncTimer( must answer for syncTimer")
	}

	if !flatCompanionDeclares(t, dir, []string{"Timer.syncTimer"}) {
		t.Errorf("a method entry must be matched by its method name")
	}

	if flatCompanionDeclares(t, dir, []string{"syncTimer", "readdir"}) {
		t.Errorf("a companion that spells only one of two suppressed members must NOT answer for both")
	}

	if flatCompanionDeclares(t, t.TempDir(), []string{"syncTimer"}) {
		t.Errorf("a package with no flat companion must not answer")
	}

	// A companion that only QUOTES the member in a comment does not declare it.
	if err := os.WriteFile(filepath.Join(dir, "sleep_impl.cs"), []byte("// Go's syncTimer(c) is not read here\npartial class x { }"), 0o644); err != nil {
		t.Fatal(err)
	}

	if flatCompanionDeclares(t, dir, []string{"syncTimer"}) {
		t.Errorf("a member spelled only in a comment must NOT count as declared")
	}

	if err := os.WriteFile(filepath.Join(dir, "sleep_impl.cs"), []byte("partial class x { internal static int syncTimer(int c) { return 0; } }"), 0o644); err != nil {
		t.Fatal(err)
	}

	// A plain converted file is not a companion, however much it spells.
	if err := os.WriteFile(filepath.Join(dir, "sleep.cs"), []byte("internal static int readdir(int c) { return 0; }"), 0o644); err != nil {
		t.Fatal(err)
	}

	if flatCompanionDeclares(t, dir, []string{"readdir"}) {
		t.Errorf("an unmarked, non-_impl file must not count as a companion")
	}
}

// hasFlatSources reports whether the package has .cs files directly in its own directory — i.e. it
// builds on every target rather than existing only through per-GOOS folders.
func hasFlatSources(t *testing.T, dir string) bool {
	t.Helper()

	entries, err := os.ReadDir(dir)
	if err != nil {
		return false
	}

	for _, entry := range entries {
		if !entry.IsDir() && strings.HasSuffix(entry.Name(), ".cs") {
			return true
		}
	}

	return false
}

// hasAnyPlatformFolder reports whether the package carries per-GOOS source folders at all — the
// L3 shape. A package that does is one whose emission VARIES by target, so only a companion in the
// target's own folder can answer for it.
func hasAnyPlatformFolder(t *testing.T, dir string) bool {
	t.Helper()

	for _, goos := range knownTargetGOOS {
		if info, err := os.Stat(filepath.Join(dir, goos)); err == nil && info.IsDir() {
			return true
		}
	}

	return false
}
