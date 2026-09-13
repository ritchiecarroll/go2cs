// darwinTrampolineMap_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// THE DARWIN TRAMPOLINE -> libSystem SYMBOL MAP, derived TWO INDEPENDENT WAYS from the committed
// corpus and asserted to agree. Step 1 of docs/phase4/SIZING-darwin-option2.md §5, approved as C2's
// item by the coordinator at mailbox `e4b84be5b` §2: it needs no Apple hardware, and it converts the
// darwin run layer's largest unknown -- "does the map exist, and is it self-checking?" -- into a
// committed artifact with a standing guard.
//
// WHY THE MAP MATTERS. A converted darwin program dies in a module initializer because darwin's
// syscall entry points are libc assembly trampolines in Go, emitted as bodyless partials and filled
// with throwing stubs (docs/phase4/FINDING-darwin-run-layer.md). Option 2 implements ten keystones
// plus a real abi.FuncPCABI0 -- eleven bodies under 207 distinct trampolines -- and FuncPCABI0's whole
// job is trampoline -> symbol -> NativeLibrary.GetExport. That middle arrow is this map.
//
// THE TWO DERIVATIONS, and why they are independent:
//
//	(a) THE PRAGMA. The converter preserves Go's `//go:cgo_import_dynamic <local> <symbol> "<library>"`
//	    into the emitted C# as a comment, so the map is readable off the committed corpus with no
//	    converter change. This is the authority.
//	(b) THE NAME. A trampoline is named `<local>_trampoline` and `<local>` is conventionally a prefix
//	    plus the symbol itself (`libc_write` -> `write`), so the name alone predicts the symbol.
//
// (b) is what an implementation would be tempted to use, because it needs no table. The guard exists
// to keep (b) honest against (a) -- and to FAIL when the corpus grows a case (b) cannot express.
//
// ⚠ THREE THINGS THIS GUARD ENCODES BECAUSE THE CORPUS MEASURED THEM, each of which a naive
// "the name derives the symbol" map would have got silently wrong (C2, 2026-09-13, at a02ac3df3):
//
//  1. THE MAP MUST BE KEYED PER PACKAGE, NOT GLOBALLY BY LOCAL NAME. `libc_exit` names TWO DIFFERENT
//     symbols: `_exit` in runtime/darwin/sys_darwin.cs and `exit` in
//     syscall/darwin/zsyscall_darwin_amd64.cs. A global map keyed on the local name silently keeps
//     whichever it read last, and resolving runtime's `libc_exit` to `exit` calls process-exit where
//     thread-exit was meant. This is the one finding here with teeth.
//  2. DERIVATION (b) CANNOT PRODUCE TWO OF THE PAIRS, and they fail in opposite directions.
//     `libc_error` -> `__error` (errno, already annotated in runtime/darwin/libccall_impl.cs) does not
//     contain its symbol at all, and (b) yields `error`, which is not a libSystem export -- a LOUD
//     failure. Runtime's `libc_exit` -> `_exit` does contain its symbol, so a containment test calls it
//     derivable, but the transform (b) actually runs yields `exit` -- a real export meaning process
//     exit instead of thread exit, a SILENT failure. So (b) is right 217 of 219 pragma LINES / 201 of
//     203 distinct local names, and both exceptions are named rather than averaged away.
//     ⚠ docs/phase4/DESIGN-darwin-run-layer.md §1.2 records "zero mismatches" for this comparison;
//     that held over the 123 pragmas it could see. C2 amended §1.2 with a dated block on 2026-09-13.
//  3. A TRAMPOLINE NAME CAN COME FROM PROSE. The scan that produced these counts first reported 208
//     distinct trampolines; one was `libc_x`, matched inside a COMMENT in
//     syscall/darwin/sockaddr_darwin_impl.cs that uses `abi.FuncPCABI0(libc_x_trampoline)` as an
//     illustration. Comment lines are excluded below, and the count is 207. An assertion about code
//     that reads prose is a class this repository hit four times on 2026-09-13 alone.
const (
	// The one local name whose symbol its own name does not CONTAIN at all, and the symbol it names.
	// Spelled as data rather than tolerated by a loosened predicate: a guard that accepted any mismatch
	// to accommodate this one would accept the next one too.
	errnoLocalName = "libc_error"
	errnoSymbol    = "__error"

	// The SECOND pair derivation (b) cannot produce, and the dangerous one of the two -- see the
	// derivationExceptions table for why the two fail differently.
	threadExitLocalName = "libc_exit"
	threadExitSymbol    = "_exit"
)

// derivationExceptions are every (local name, symbol) pair that derivation (b) -- strip the prefix
// ending in `_`, the transform an implementation reaches for because it needs no table -- CANNOT
// produce. Both are spelled as DATA and both are asserted still present below, so neither the
// tolerance nor the guard can quietly rot into dead code.
//
// ⚠ THEY FAIL IN OPPOSITE DIRECTIONS, which is the whole reason the strict predicate is worth having:
// derivation (b) applied to `libc_error` yields `error`, which is not a libSystem export, so that one
// fails LOUDLY at lookup time. Applied to runtime's `libc_exit` it yields `exit`, which IS a real
// libSystem export with different semantics -- process exit where thread exit was meant. A predicate
// loose enough to call the second one derivable reports the silent failure as a success.
// derivesByName IS derivation (b): strip the prefix ending in `_` and what remains is the symbol. It
// has ONE home so the scoring loop and the assertion that the exceptions really ARE exceptions cannot
// drift apart -- the same reason G's BOM predicate has one home. Note the `_`: without it this is the
// weaker containment test, which calls runtime's `libc_exit` -> `_exit` derivable when the transform
// yields `exit`.
func derivesByName(local, symbol string) bool {
	return local == symbol || strings.HasSuffix(local, "_"+symbol)
}

var derivationExceptions = []struct{ Local, Symbol, Why string }{
	{errnoLocalName, errnoSymbol, "errno; annotated in runtime/darwin/libccall_impl.cs. (b) yields `error`, which does not exist -- fails loudly"},
	{threadExitLocalName, threadExitSymbol, "thread exit in runtime/darwin/sys_darwin.cs. (b) yields `exit`, which exists and means something else -- fails SILENTLY"},
}

// darwinPragma is one `//go:cgo_import_dynamic` reading, kept with the file it came from because
// finding 1 above makes the file's package part of the key.
type darwinPragma struct {
	Local   string
	Symbol  string
	Library string
	File    string
}

var (
	// The pragma, as the converter emits it into a comment line.
	darwinPragmaPattern = regexp.MustCompile(`//go:cgo_import_dynamic\s+(\S+)\s+(\S+)\s+"([^"]*)"`)

	// An address-taken trampoline. Anchored on abi.FuncPCABI0 because that is the coupling the run
	// layer actually has: a trampoline nobody takes the address of needs no symbol.
	darwinTrampolinePattern = regexp.MustCompile(`\babi\.FuncPCABI0\(\s*([A-Za-z_0-9]+)_trampoline`)
)

// collectDarwinFlavour walks src/core's darwin folders and returns the pragmas and the address-taken
// trampoline names, with COMMENT lines excluded from the trampoline scan (finding 3).
func collectDarwinFlavour(t *testing.T) (pragmas []darwinPragma, trampolines map[string][]string) {
	t.Helper()

	core := filepath.Join(repoRootFromPackageDir(t), "src", "core")
	trampolines = map[string][]string{}

	err := filepath.Walk(core, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		if info.IsDir() || !strings.HasSuffix(path, ".cs") {
			return nil
		}

		// Only the darwin flavour: layout L3 puts platform-varying files in a `darwin` folder.
		rel, relErr := filepath.Rel(core, path)

		if relErr != nil {
			return relErr
		}

		isDarwin, inBuildOutput := false, false

		for _, segment := range strings.Split(filepath.ToSlash(rel), "/") {
			if segment == "darwin" {
				isDarwin = true
			}

			// ⚠ BUILD OUTPUT IS EXCLUDED, and because this is a FILESYSTEM walk the exclusion is
			// load-bearing rather than decorative. A built tree carries copies of converted sources under
			// bin\ and obj\, and `Generated\` accumulates; a walk that swept them would inflate every
			// count here on exactly the boxes that compile, and read normally on the ones that do not --
			// the worst shape a census can have. Same list, same reason, as
			// src/reconvert-deletions.ps1's $BuildOutputDirs.
			//
			// MEASURED LATENT, not observed: at a02ac3df3 in a never-built worktree there are ZERO such
			// directories under src/core and zero darwin .cs beneath them, so this guard's first
			// published counts were not affected. The hole was real and unfired -- which is G's BOM
			// reading exactly ("the hole is LATENT and what prevents it is a convention nothing
			// asserts"), and it was found by taking G's own filesystem-grep error seriously the same day
			// (1,975 hits swept out of bin/obj where git grep over tracked files reads 52).
			if segment == "bin" || segment == "obj" || segment == "Generated" {
				inBuildOutput = true
				break
			}
		}

		if !isDarwin || inBuildOutput {
			return nil
		}

		raw, readErr := os.ReadFile(path)

		if readErr != nil {
			return readErr
		}

		for _, line := range strings.Split(string(raw), "\n") {
			line = strings.TrimRight(line, "\r")

			if match := darwinPragmaPattern.FindStringSubmatch(line); match != nil {
				pragmas = append(pragmas, darwinPragma{
					Local: match[1], Symbol: match[2], Library: match[3], File: filepath.ToSlash(rel),
				})

				// A pragma line IS a comment, so the trampoline scan must not also read it.
				continue
			}

			// ⚠ Finding 3: a comment can spell a trampoline as an example. `//` after trimming is
			// enough here because the corpus's own convention puts these in line comments, and a
			// stricter block-comment walk would be a second parser to keep in sync.
			if strings.HasPrefix(strings.TrimSpace(line), "//") {
				continue
			}

			for _, match := range darwinTrampolinePattern.FindAllStringSubmatch(line, -1) {
				name := match[1]
				trampolines[name] = append(trampolines[name], filepath.ToSlash(rel))
			}
		}

		return nil
	})

	if err != nil {
		t.Fatalf("walking the darwin flavour failed: %v", err)
	}

	// NON-EMPTY BEFORE ANY RATIO. An empty population would make every assertion below pass, which is
	// the vacuous-green shape this package exists to refuse.
	if len(pragmas) == 0 {
		t.Fatal("no //go:cgo_import_dynamic pragmas found in the darwin flavour -- the pattern, the " +
			"layout assumption or the corpus has moved. A zero here is a broken scan, not a clean tree.")
	}

	if len(trampolines) == 0 {
		t.Fatal("no abi.FuncPCABI0(<x>_trampoline) call sites found in the darwin flavour -- see above; " +
			"a zero is a broken scan")
	}

	return pragmas, trampolines
}

// TestDarwinTrampolineMapDerivesTwoWaysAndAgrees is the guard itself.
func TestDarwinTrampolineMapDerivesTwoWaysAndAgrees(t *testing.T) {
	pragmas, trampolines := collectDarwinFlavour(t)

	// --- finding 1: the key is (package directory, local name), and a global key would collide ------
	//
	// Built as a map from the local name to the distinct symbols it names anywhere. More than one
	// symbol for one local name is not a defect in the corpus -- Go's own sources do it -- it is a
	// REQUIREMENT on any map an implementation builds, so it is asserted as a known set rather than
	// forbidden.
	symbolsByLocal := map[string]map[string]string{} // local -> symbol -> first file seen

	for _, p := range pragmas {
		if symbolsByLocal[p.Local] == nil {
			symbolsByLocal[p.Local] = map[string]string{}
		}

		if _, seen := symbolsByLocal[p.Local][p.Symbol]; !seen {
			symbolsByLocal[p.Local][p.Symbol] = p.File
		}
	}

	ambiguous := []string{}

	for local, symbols := range symbolsByLocal {
		if len(symbols) > 1 {
			parts := []string{}

			for symbol, file := range symbols {
				parts = append(parts, fmt.Sprintf("%s (%s)", symbol, file))
			}

			sort.Strings(parts)
			ambiguous = append(ambiguous, fmt.Sprintf("%s -> %s", local, strings.Join(parts, " / ")))
		}
	}

	sort.Strings(ambiguous)

	// The known set, spelled out. A NEW ambiguity is a finding a reader must see before an
	// implementation keys a map on the local name alone; the existing one is recorded so the guard
	// does not go red every run for a fact already understood.
	const knownAmbiguous = "libc_exit -> _exit (runtime/darwin/sys_darwin.cs) / exit (syscall/darwin/zsyscall_darwin_amd64.cs)"

	switch {
	case len(ambiguous) == 0:
		t.Errorf("the libc_exit ambiguity has DISAPPEARED from the corpus.\n"+
			"That is not automatically good news: this guard's finding 1 -- that a trampoline map must be "+
			"keyed per package, not globally by local name -- rested on it. Re-read the pragmas and either "+
			"update %s's finding 1, or find what stopped being emitted.", "darwinTrampolineMap_test.go")
	case len(ambiguous) == 1 && ambiguous[0] == knownAmbiguous:
		t.Logf("ambiguous local name, as recorded: %s", ambiguous[0])
	default:
		t.Errorf("the set of ambiguous trampoline local names has CHANGED -- a map keyed on the local "+
			"name alone would silently resolve one of these to the wrong libSystem symbol.\n  want exactly:\n    %s\n  got %d:\n    %s",
			knownAmbiguous, len(ambiguous), strings.Join(ambiguous, "\n    "))
	}

	// --- finding 2 + the two derivations agreeing -------------------------------------------------
	//
	// The PREFIX SET IS DERIVED FROM THE DATA, never typed: for each pragma the prefix is whatever
	// precedes the symbol inside the local name. A typed list drifts from the corpus it describes,
	// which is the defect this whole package keeps finding in other instruments.
	// ⚠ THE PREDICATE IS THE TRANSFORM, NOT CONTAINMENT, and the difference is not cosmetic. Derivation
	// (b) as this guard documents it is "strip the prefix" -- so the honest test is that the local name
	// ends in `_` + the symbol. `strings.HasSuffix(p.Local, p.Symbol)` WITHOUT that underscore is the
	// weaker containment test, and it admits `libc_exit` -> `_exit` (which does end in `_exit`) while
	// the transform an implementation actually runs yields `exit`. Scoring (b) with containment reports
	// 218 of 219 and hides the one row whose naive derivation lands on a real, wrong symbol; scoring it
	// with the transform reports 217 and names both exceptions. Same class as counting one predicate
	// under another predicate's label.
	nameDerives, nameDoesNot := 0, []string{}

	isException := func(p darwinPragma) bool {
		for _, e := range derivationExceptions {
			if p.Local == e.Local && p.Symbol == e.Symbol {
				return true
			}
		}

		return false
	}

	for _, p := range pragmas {
		switch {
		// `p.Local == p.Symbol` is a pragma with no prefix at all, which derivation (b) produces
		// trivially. It matched ZERO rows at this reading and is kept as a legitimate shape rather
		// than as a claim that it occurs -- the count below is what says whether it does.
		case derivesByName(p.Local, p.Symbol):
			nameDerives++
		case isException(p):
			// A named exception. Counted separately, deliberately not folded into nameDerives -- the
			// ratio below is a statement about the convention, and burying the exceptions in it would
			// make the convention look universal.
		default:
			nameDoesNot = append(nameDoesNot, fmt.Sprintf("%s -> %s (%s)", p.Local, p.Symbol, p.File))
		}
	}

	sort.Strings(nameDoesNot)

	if len(nameDoesNot) > 0 {
		known := []string{}

		for _, e := range derivationExceptions {
			known = append(known, fmt.Sprintf("%s -> %s", e.Local, e.Symbol))
		}

		t.Errorf("%d pragma(s) whose symbol derivation (b) CANNOT produce from the local name, beyond the "+
			"%d known exception(s) (%s).\nEach is a trampoline an implementation that derives symbols from "+
			"names would resolve wrongly -- check whether it fails loudly (no such export) or silently (a "+
			"real export, wrong semantics) before deciding how much it matters:\n    %s",
			len(nameDoesNot), len(derivationExceptions), strings.Join(known, ", "), strings.Join(nameDoesNot, "\n    "))
	}

	// Every named exception must still BE there. A guard that tolerates an exception has to notice when
	// the exception goes away, or the tolerance silently becomes dead code that would hide a new case.
	//
	// ⚠ KEYED ON THE PAIR, never on the local name alone. `libc_exit` LEGITIMATELY appears with two
	// symbols (finding 1 is that very fact), so a check asking "does libc_exit name _exit?" of every
	// row it matches goes red on the syscall row for naming `exit` -- correct data failing a guard that
	// forgot its own finding one screen earlier.
	for _, e := range derivationExceptions {
		// ⚠ AND THE PREDICATE MUST STILL REJECT IT. Without this arm the strictness of derivesByName is
		// itself unguarded: relax the `_` back out and `libc_exit` -> `_exit` silently moves from the
		// exception list into the derivable count, the ratio climbs from 217 to 218, every other
		// assertion here stays green, and the reading now describes containment while still being
		// labelled the transform. An exception that the predicate accepts is not an exception.
		if derivesByName(e.Local, e.Symbol) {
			t.Errorf("derivation (b) now ACCEPTS %s -> %s, which this guard records as one of the pairs it "+
				"cannot produce (%s).\nEither the predicate was loosened -- check for a missing `_` in "+
				"derivesByName, which turns the transform into containment -- or the corpus renamed the "+
				"trampoline and the table is stale. The derivable count is not trustworthy until this is "+
				"resolved, because it is now scoring a weaker property than its label claims.",
				e.Local, e.Symbol, e.Why)
		}

		found := []string{}

		for _, p := range pragmas {
			if p.Local == e.Local && p.Symbol == e.Symbol {
				found = append(found, p.File)
			}
		}

		if len(found) == 0 {
			others := []string{}

			for _, p := range pragmas {
				if p.Local == e.Local {
					others = append(others, fmt.Sprintf("%s (%s)", p.Symbol, p.File))
				}
			}

			sort.Strings(others)

			if len(others) == 0 {
				t.Errorf("%s is no longer in the corpus at all. The exception this guard encodes (%s -> %s: %s) "+
					"is dead code now: remove it, or find out what happened to that trampoline.",
					e.Local, e.Local, e.Symbol, e.Why)
			} else {
				t.Errorf("%s no longer names %q anywhere; it now names %s. The exception this guard encodes "+
					"(%s) has MOVED, and an implementation deriving this symbol by name is now either right "+
					"for a new reason or wrong for a new one -- read the pragma before updating the table.",
					e.Local, e.Symbol, strings.Join(others, " / "), e.Why)
			}
		}
	}

	// --- coverage: which address-taken trampolines have an authoritative symbol at all -------------
	covered, uncovered := 0, []string{}

	for name := range trampolines {
		if _, ok := symbolsByLocal[name]; ok {
			covered++
		} else {
			uncovered = append(uncovered, name)
		}
	}

	sort.Strings(uncovered)

	// REPORTED, NOT ASSERTED. These are trampolines whose address the corpus takes and for which no
	// pragma survives, so their symbol is a GUESS -- exactly the population the run layer's step 2
	// has to dispose of, and exactly the number a reader of the sizing record needs. Asserting a
	// count here would turn an honest open question into a brittle gate.
	// LINES and DISTINCT LOCAL NAMES are both reported, and labelled, because they differ (a local
	// name can be declared in more than one darwin package) and a reader comparing one against the
	// other's figure would think one of them was wrong.
	t.Logf("darwin trampoline map: %d pragma LINE(s) over %d distinct local name(s); "+
		"%d distinct address-taken trampoline(s), %d with an authoritative symbol, %d with none "+
		"(symbol only guessable)",
		len(pragmas), len(symbolsByLocal), len(trampolines), covered, len(uncovered))
	// LABELLED WITH THE PREDICATE IT SCORES. "name derives symbol" is ambiguous between containment and
	// the transform, the two readings differ by exactly the row that matters, and an unlabelled ratio
	// invites the reader to assume whichever is more flattering.
	exceptions := []string{}

	for _, e := range derivationExceptions {
		exceptions = append(exceptions, fmt.Sprintf("%s -> %s", e.Local, e.Symbol))
	}

	t.Logf("derivation (b), scored as the TRANSFORM (local name ends in `_` + symbol): %d of %d pragma "+
		"LINE(s), plus %d named exception(s) (%s)",
		nameDerives, len(pragmas), len(derivationExceptions), strings.Join(exceptions, ", "))

	if len(uncovered) > 0 {
		t.Logf("trampolines with no surviving pragma (%d): %s", len(uncovered), strings.Join(uncovered, ", "))
	}

	// The one hard floor on coverage: if NOTHING resolves, the map does not exist and option 2's step
	// 2 has no input. Deliberately a floor rather than an equality -- the corpus is expected to grow.
	if covered == 0 {
		t.Fatal("not one address-taken trampoline has a surviving cgo_import_dynamic pragma -- the map " +
			"this guard exists to prove is not derivable from the corpus at all")
	}
}
