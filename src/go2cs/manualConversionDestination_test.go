// manualConversionDestination_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The BOTH-SIDES guard for manualConversionFuncs: a registration displaces a generated body, and the
// displacement must have a DESTINATION.
//
// Why this exists, stated as the defect it locks out rather than as a rule. `manualConversionFuncs`
// only ever says "do not emit this body" — the hand-owned replacement lives in a separate `*_impl.cs`
// that nothing mechanically ties to the registration. So the two halves can part company at a merge,
// silently and with no conflict: on 2026-08-29 master carried the `Uname` registration and the
// generated placeholder pointing at it, while the `_impl.cs` body it named existed nowhere. Clean
// merge, no warning, `-p:GoTargetOS=linux` red at `kernel_version_linux.cs(21,27) CS0117` and the
// whole Linux corpus behind it.
//
// It got past the seam check that was supposed to catch exactly this, and the reason is the point of
// this file. That check asserted every registered name has ZERO generated bodies and EXACTLY ONE
// placeholder — which verifies the wrapper was DISPLACED and never that the displacement ARRIVES
// anywhere. A placeholder aimed at a body that does not exist passes it cleanly. The fold-bound
// lesson: a displacement property must assert its destination; every seam check carries both sides of
// the ledger.
//
// SCOPE, deliberately narrow. This walks the real corpus and asks one question per registration: does
// SOME hand-owned file in the package define this name? It does not type-check, does not resolve
// overloads, and does not know which platform folder a body belongs in (layout L3 routes hand-owns by
// their principal's platform set — platformHandOwn_test.go owns that invariant). A cheap, mechanical
// yes/no is what the merge seam needs; anything richer would duplicate the compiler, which the corpus
// build already runs.
//
// AMENDED 2026-09-05 (Q62): that scope sentence was the guard's own blind spot, and it was measured.
// The witness folded EVERY per-GOOS folder under `src/core/<pkg>/` into ONE name set, so a
// registration widened to a flavour whose folder holds no body passed whenever ANY other flavour's
// folder held one — a C# project compiles exactly one `<goos>/` folder, so one flavour's body cannot
// answer for another's. C2 measured it on the darwin signal bridge: with `runtime/darwin`'s bridge
// file moved aside the guard stayed GREEN, because `runtime/linux/signal_posix_impl.cs` declares the
// same three members. The registry has carried the flavour axis since the goosScope arc; the witness
// had not. Both arms of the ledger are now asked per FLAVOUR (see the arm comments below), and the
// witness reads exactly the folders a build of that flavour compiles: the package's own directory
// (flat, compiled everywhere) plus `<pkg>/<goos>/` (compiled there alone).

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// TestManualConversionRegistrationsHaveBodies is the forward direction: registration => a hand-owned
// definition exists ON EVERY FLAVOUR THE SCOPE NAMES. This is the direction the Uname subtraction
// broke, and the flavour axis is the Q62 amendment in the file header.
//
// Two arms, and they fail apart:
//
//   - FORWARD (scope ⇒ body): for each flavour the entry is scoped to, a hand-own compiled by that
//     flavour's build must declare the member — flat in `<pkg>/`, or in `<pkg>/<goos>/`. A scope
//     widened past its bodies is the standing shape of the two-flavour hand-own families (the signal
//     bridge, the sockaddr twin, nanotime, sigprocmask): the widening merges, the body does not, and
//     every gate on every other flavour stays green.
//   - REVERSE (body ⇒ scope): a NON-partial hand-own in `<pkg>/<goos>/` declaring a member the entry
//     does NOT displace on that flavour. The generated body survives beside it — CS0111 on that
//     flavour alone. The `partial` exemption is not a softening: a bodyless `public static partial`
//     is displaced simply by WRITING a body (PartialStubGenerator's predicate is
//     `IsPartialDefinition && PartialImplementationPart is null`), which is the OTHER displacement
//     mechanism and needs no registry entry at all. It is the whole measured population of this arm
//     at master — runtime's nanotime1, registered darwin-only, whose windows and linux hand-owns are
//     `partial` completions of the bodyless declaration in each flavour's stubs3.cs.
//
// The relationship with TestScopedSuppressionsHaveCompanions (suppressionCompanion_test.go) is
// deliberate, not duplication. That guard asks the same question per (package, GOOS) at FILE
// granularity — is there any marked companion at all — and is therefore blind to a companion that
// exists but declares something else, which is exactly the darwin-bridge false green. This arm is
// member-granular and sees it. In the other direction that guard is blind to nothing this one relies
// on: its witness is the marker, so it cannot be fooled by the declaration regex below, whose
// deliberate over-collection can hand THIS arm a false pass on a member some other declaration line
// happens to name (measured: `runtime.read` is answered flat by `consistentHeapStats.read` in
// managed_impl.cs — a different member, same bare name). Two witnesses, two failure modes; keep both.
func TestManualConversionRegistrationsHaveBodies(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	if _, err := os.Stat(coreDir); err != nil {
		t.Skip("src/core is not beside the converter; nothing to walk")
	}

	type finding struct {
		entry, goos, detail string
	}

	var missing []finding
	var stranded []finding

	for pkg, funcs := range manualConversionFuncs {
		packageDir := filepath.Join(coreDir, filepath.FromSlash(pkg))
		bodies := handOwnedDefinitionsByFlavor(t, packageDir)

		// A package the corpus does not carry at all is not this guard's business (a scope naming
		// something unconverted), matching TestScopedSuppressionsHaveCompanions' own skip.
		if _, err := os.Stat(packageDir); err != nil {
			continue
		}

		for name, scope := range funcs {
			// A method registration ("g.guintptr", "SockaddrInet4.sockaddr") names the RECEIVER
			// type and the member; the member is what a hand-own defines, so match on the tail.
			member := name
			if dot := strings.LastIndex(member, "."); dot >= 0 {
				member = member[dot+1:]
			}

			entry := pkg + "." + name
			checked := 0

			for _, goos := range knownTargetGOOS {
				// A package that this target does not build cannot owe a body for it. Reuses the
				// same reading suppressionCompanion_test.go arrived at: no folder of its own AND no
				// flat sources means the target simply has no such package.
				if !packageBuildsOn(t, packageDir, goos) {
					continue
				}

				inScope := scope.includes(goos)

				if inScope {
					checked++

					if !bodies.compiledOn(member, goos) {
						where := bodies.locations(member)
						detail := "no hand-own in that flavour's build declares it"

						if where != "" {
							detail = "the only hand-own declaring it is in " + where + ", which this flavour's build does not compile"
						}

						missing = append(missing, finding{entry: entry, goos: goos, detail: detail})
					}

					continue
				}

				// REVERSE: a body this flavour compiles that nothing displaces there.
				if bodies.strandedOn(member, goos) {
					stranded = append(stranded, finding{entry: entry, goos: goos})
				}
			}

			// Every scoped flavour was skipped as "not built there" — fall back to the pre-Q62
			// question so no entry goes unasserted.
			if checked == 0 && !bodies.anywhere(member) {
				missing = append(missing, finding{entry: entry, goos: "(any target)",
					detail: "no hand-owned file in that package defines it"})
			}
		}
	}

	sortFindings := func(list []finding) {
		sort.Slice(list, func(i, j int) bool {
			if list[i].entry != list[j].entry {
				return list[i].entry < list[j].entry
			}

			return list[i].goos < list[j].goos
		})
	}

	sortFindings(missing)
	sortFindings(stranded)

	for _, f := range missing {
		t.Errorf("manualConversionFuncs registers %s and displaces it on %s, but %s — the generated body "+
			"is gone and the displacement has no destination there, which is a build failure on that "+
			"platform alone (CS0117 at the first consumer) while every other flavour's gate stays green. "+
			"Either author the body under src/core/<pkg>/%s/, or narrow the entry's goosScope to the "+
			"flavours that carry one", f.entry, f.goos, f.detail, f.goos)
	}

	for _, f := range stranded {
		t.Errorf("a hand-own under src/core/%s/ declares %s, but the registration does NOT displace it on "+
			"%s — the generated body survives beside the hand-owned one and the package fails CS0111 on "+
			"that platform alone. Either widen the entry's goosScope to %s, or retire the stranded body. "+
			"(A `partial` body is exempt: writing into a bodyless partial is the other displacement "+
			"mechanism and needs no registration.)",
			strings.SplitN(f.entry, ".", 2)[0]+"/"+f.goos, f.entry, f.goos, f.goos)
	}
}

// handOwnBodies is one package's hand-own witness, split by which builds compile it: `flat` is the
// package's own directory (every target), `perGOOS` is one set per platform folder (that target
// alone). `perGOOSPartial` narrows a per-GOOS set to declarations carrying `partial` — the bodyless
// partial completion, which displaces without a registration. `perGOOSReplaced` narrows it to
// declarations sitting in a WHOLE-FILE `[module: GoManualConversion]` replacement — the third
// displacement mechanism, which also needs no registration (see strandedOn).
type handOwnBodies struct {
	flat            map[string]bool
	perGOOS         map[string]map[string]bool
	perGOOSPartial  map[string]map[string]bool
	perGOOSReplaced map[string]map[string]bool
}

// compiledOn is the FORWARD arm's whole rule: is a hand-own declaring this member in the set of
// files a build of `goos` compiles? A flat hand-own is in every flavour's build; a per-GOOS one is
// in its own alone, and in no other's — which is the fact the pre-Q62 union witness lost.
func (b handOwnBodies) compiledOn(member string, goos string) bool {
	return b.flat[member] || b.perGOOS[goos][member]
}

// strandedOn is the REVERSE arm's whole rule: a body this flavour compiles that no registration
// displaces there. THERE ARE THREE DISPLACEMENT MECHANISMS and only one of them needs a registry
// entry, so two are exempt here:
//
//   - `partial` — writing a body into a bodyless partial IS the displacement; PartialStubGenerator's
//     predicate is `IsPartialDefinition && PartialImplementationPart is null`, so a written body
//     steps the throwing stub aside by construction.
//   - a WHOLE-FILE `[module: GoManualConversion]` replacement — `containsManualConversionMarker`
//     drops the marked file from the convert set, so the converter NEVER EMITS a body for the
//     members it declares and there is nothing to collide with. An `_impl.cs` COMPANION is the
//     opposite case and is NOT exempt: the converter still emits the file beside it, which is
//     exactly what a registration displaces.
//
// Measured 2026-09-06: without the replacement exemption this arm reported syscall's hand-owned
// `Exec`/`forkExec` under linux/ and windows/ as stranded the moment a darwin-scoped entry existed,
// three findings, zero real — the windows corpus builds at 0 errors across 307 projects and
// syscall.csproj builds at 0 errors under -p:GoTargetOS=linux.
func (b handOwnBodies) strandedOn(member string, goos string) bool {
	return b.perGOOS[goos][member] &&
		!b.perGOOSPartial[goos][member] &&
		!b.perGOOSReplaced[goos][member]
}

// anywhere is the pre-Q62 question, kept for the fallback: does SOME hand-own of this package
// declare the member, wherever it sits?
func (b handOwnBodies) anywhere(member string) bool {
	if b.flat[member] {
		return true
	}

	for _, names := range b.perGOOS {
		if names[member] {
			return true
		}
	}

	return false
}

// locations names the folders that DO declare a member, so a forward failure says where the body
// went rather than only that it is not here. Empty when nothing declares it anywhere.
func (b handOwnBodies) locations(member string) string {
	var where []string

	if b.flat[member] {
		where = append(where, "the package's own directory")
	}

	for _, goos := range knownTargetGOOS {
		if b.perGOOS[goos][member] {
			where = append(where, goos+"/")
		}
	}

	return strings.Join(where, ", ")
}

// handOwnedDefinitionsByFlavor reads one package's hand-owns the way a BUILD reads them: the
// package's own directory plus each `<goos>/` folder, never a sibling's and never a child package's.
//
// Two scope rules, both of them the reason this replaced a recursive walk:
//
//   - Depth. The walk it replaced descended into subdirectories, and a converted package's
//     subdirectories are usually OTHER packages (net/http holds cgi, httptest, …) — the same false
//     PASS generatedFuncPlaceholders below already refuses by stopping at depth 1. Measured at the
//     Q62 census: zero registrations at master were witnessed only by a child package, so closing
//     it costs nothing and removes the hole.
//   - Flavour. One flavour's folder cannot answer for another's, which is the amendment itself.
func handOwnedDefinitionsByFlavor(t *testing.T, packageDir string) handOwnBodies {
	t.Helper()

	bodies := handOwnBodies{
		flat:            map[string]bool{},
		perGOOS:         map[string]map[string]bool{},
		perGOOSPartial:  map[string]map[string]bool{},
		perGOOSReplaced: map[string]map[string]bool{},
	}

	bodies.flat, _, _ = handOwnedDefinitionsIn(packageDir)

	for _, goos := range knownTargetGOOS {
		bodies.perGOOS[goos], bodies.perGOOSPartial[goos], bodies.perGOOSReplaced[goos] =
			handOwnedDefinitionsIn(filepath.Join(packageDir, goos))
	}

	return bodies
}

// handOwnedDefinitionsIn returns the member names declared by the hand-owned files sitting DIRECTLY
// in one directory — every `*_impl.cs` plus every file carrying the whole-file
// `[module: GoManualConversion]` marker, which are the two shapes a hand-own takes
// (platformHandOwn_test.go's own framing) — and, separately, the subset declared on a `partial` line.
func handOwnedDefinitionsIn(dir string) (map[string]bool, map[string]bool, map[string]bool) {
	defined := map[string]bool{}
	partial := map[string]bool{}
	replaced := map[string]bool{}

	entries, err := os.ReadDir(dir)
	if err != nil {
		return defined, partial, replaced
	}

	for _, entry := range entries {
		name := entry.Name()

		// `.cs.auto` review siblings are the converter's own output, never a hand-own.
		if entry.IsDir() || !strings.HasSuffix(name, ".cs") || strings.HasSuffix(name, ".cs.auto") {
			continue
		}

		content, readErr := os.ReadFile(filepath.Join(dir, name))
		if readErr != nil {
			continue
		}

		text := string(content)

		// `_impl` anywhere in the base name, not just as the suffix: the hand-owned TEST surface
		// uses `<name>_impl_test.cs` (internal/reflectlite's export_impl_test.cs defines Field,
		// TField and Zero, all three registered). Calibrated against the corpus rather than
		// assumed — a `_impl.cs`-only suffix check reported those three as missing bodies.
		isImpl := strings.Contains(name, "_impl")
		isMarked := manualConversionMarker.MatchString(text)

		if !isImpl && !isMarked {
			continue
		}

		// The two shapes displace DIFFERENTLY, so the reverse arm must tell them apart. A marked
		// file that is NOT an `_impl` companion is a whole-file REPLACEMENT: the converter drops it
		// from the convert set and emits no body for what it declares, so nothing can collide and no
		// registration is owed. A companion supplements a file the converter still writes, and there
		// a registration is exactly what keeps the two from colliding.
		isWholeFileReplacement := isMarked && !isImpl

		for _, line := range csharpDeclarationLine.FindAllString(text, -1) {
			isPartial := csharpPartialDeclaration.MatchString(line)

			for _, match := range csharpCallableName.FindAllStringSubmatch(line, -1) {
				defined[match[1]] = true

				if isPartial {
					partial[match[1]] = true
				}

				if isWholeFileReplacement {
					replaced[match[1]] = true
				}
			}
		}
	}

	return defined, partial, replaced
}

// csharpPartialDeclaration marks a declaration line as the implementing half of a bodyless partial —
// the displacement mechanism that needs no registry entry, and therefore the reverse arm's exemption.
var csharpPartialDeclaration = regexp.MustCompile(`\bpartial\b`)

// packageBuildsOn reports whether a target's build of this package compiles anything: its own
// `<goos>/` folder, or flat sources every target shares. A package with neither is simply not built
// there (crypto/x509/internal/macos on windows), and cannot owe a body for it. The same reading
// suppressionCompanion_test.go arrived at, reached through its own helpers so there is one
// definition of the fact rather than two that merge without a conflict.
func packageBuildsOn(t *testing.T, packageDir string, goos string) bool {
	t.Helper()

	if info, err := os.Stat(filepath.Join(packageDir, goos)); err == nil && info.IsDir() {
		return true
	}

	return hasFlatSources(t, packageDir)
}

// TestPerFlavorWitnessCannotAnswerAcrossFlavors is the standing control for the Q62 amendment, and it
// is synthetic on purpose: the four controls that measured the amendment on the real corpus all
// MUTATE it (a body moved aside, a scope widened, a scope narrowed, a `partial` stripped), which is
// fine for a one-off measurement and impossible for a test that must run in every clone. This one
// builds the four shapes in a temp directory instead, so both arms of the ledger keep a control that
// can go RED with no corpus to disturb.
//
// The four shapes, each pinning one clause the corpus measurement exercised:
//
//	linux/only_impl.cs      a per-GOOS body — visible to linux, INVISIBLE to windows and darwin.
//	                        This is the whole amendment: the union witness it replaced answered
//	                        "defined" for all three, which is how a darwin scope widened past its
//	                        body stayed green while runtime/linux carried the twin.
//	shared_impl.cs          a flat body — visible to EVERY flavour, which is why the forward arm
//	                        accepts it as a witness for a scope naming any of them.
//	darwin/partial_impl.cs  a `partial` body — the other displacement mechanism, exempt from the
//	                        reverse arm; without the exemption every bodyless-partial completion in
//	                        the corpus reads as a stranded body (nanotime1 is the live instance).
//	child/deep_impl.cs      a CHILD package's body — never a witness for its parent, the depth rule
//	                        generatedFuncPlaceholders already states and the replaced walk did not.
func TestPerFlavorWitnessCannotAnswerAcrossFlavors(t *testing.T) {
	packageDir := t.TempDir()

	write := func(rel string, decl string) {
		full := filepath.Join(packageDir, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		source := "namespace go;\n\npartial class fixture_package {\n\n" + decl + " {\n}\n\n}\n"

		if err := os.WriteFile(full, []byte(source), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	// A WHOLE-FILE `[module: GoManualConversion]` replacement — no `_impl` in the name, so the
	// collector admits it on the marker alone, exactly as the corpus's syscall/linux/exec_unix.cs is
	// admitted. The marker goes above the namespace, where the corpus writes it.
	writeMarked := func(rel string, decl string) {
		full := filepath.Join(packageDir, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		source := "[module: go.GoManualConversion]\n\nnamespace go;\n\npartial class fixture_package {\n\n" +
			decl + " {\n}\n\n}\n"

		if err := os.WriteFile(full, []byte(source), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	write("linux/only_impl.cs", "internal static error perFlavorOnly(nint fd)")
	write("shared_impl.cs", "internal static error flatEverywhere(nint fd)")
	write("darwin/partial_impl.cs", "internal static partial int64 partialCompletion()")
	write("child/deep_impl.cs", "internal static error childPackageOnly(nint fd)")
	writeMarked("linux/whole_file.cs", "internal static error wholeFileReplacement(nint fd)")

	if err := os.MkdirAll(filepath.Join(packageDir, "windows"), 0o755); err != nil {
		t.Fatal(err)
	}

	bodies := handOwnedDefinitionsByFlavor(t, packageDir)

	// The witness must collect what it was pointed at, or every assertion below is vacuous.
	if !bodies.perGOOS["linux"]["perFlavorOnly"] || !bodies.flat["flatEverywhere"] || !bodies.perGOOS["darwin"]["partialCompletion"] {
		t.Fatalf("the witness did not collect the fixture bodies (linux=%v flat=%v darwin=%v); the control measures nothing",
			bodies.perGOOS["linux"], bodies.flat, bodies.perGOOS["darwin"])
	}

	// THE POPULATION MUST STAY WHOLE. The whole-file replacement is exempt from the REVERSE arm, and
	// the cheap way to spell that exemption is a `continue` in the collector — which would also empty
	// it out of the FORWARD arm's witness, so a registration whose only body sits in a whole-file
	// hand-own would read as MISSING. That is the silent-subtraction class arriving through the fix
	// for a false positive. This assertion fails on a collector-level exemption and passes on the
	// reasoning-level one, which is the whole difference between the two implementations.
	if !bodies.perGOOS["linux"]["wholeFileReplacement"] {
		t.Fatal("the witness did not collect the whole-file-marked body — the exemption belongs in strandedOn, " +
			"NOT in handOwnedDefinitionsIn: a collector that skips marked files blinds the FORWARD arm too")
	}

	// FORWARD: a per-GOOS body answers for its own flavour and for no other.
	for _, goos := range knownTargetGOOS {
		want := goos == "linux"

		if got := bodies.compiledOn("perFlavorOnly", goos); got != want {
			t.Errorf("compiledOn(perFlavorOnly, %s) = %v, want %v — a body under <pkg>/linux/ is compiled by the linux build alone", goos, got, want)
		}

		// FORWARD: a flat body answers for every flavour.
		if !bodies.compiledOn("flatEverywhere", goos) {
			t.Errorf("compiledOn(flatEverywhere, %s) = false — a flat hand-own is in every flavour's build", goos)
		}

		// A child package is never a witness for its parent, on any flavour.
		if bodies.compiledOn("childPackageOnly", goos) {
			t.Errorf("compiledOn(childPackageOnly, %s) = true — a subdirectory that is not a GOOS folder is another PACKAGE, and its bodies cannot answer for this one", goos)
		}
	}

	// The pre-Q62 union question still answers TRUE for the per-GOOS body, which is precisely why it
	// could not see the defect: the amendment is the difference between these two lines.
	if !bodies.anywhere("perFlavorOnly") {
		t.Error("anywhere(perFlavorOnly) = false; the pre-Q62 witness is mis-modelled and the contrast this control draws is not the real one")
	}

	// REVERSE: a non-partial per-GOOS body is stranded on its own flavour and nowhere else.
	for _, goos := range knownTargetGOOS {
		if got, want := bodies.strandedOn("perFlavorOnly", goos), goos == "linux"; got != want {
			t.Errorf("strandedOn(perFlavorOnly, %s) = %v, want %v", goos, got, want)
		}

		// REVERSE: a `partial` completion is never stranded — it displaces by being written.
		if bodies.strandedOn("partialCompletion", goos) {
			t.Errorf("strandedOn(partialCompletion, %s) = true — writing a body into a bodyless partial is the other displacement mechanism and needs no registration", goos)
		}

		// REVERSE: a flat body is not a per-GOOS stranding; the flat/per-GOOS split is the point.
		if bodies.strandedOn("flatEverywhere", goos) {
			t.Errorf("strandedOn(flatEverywhere, %s) = true — a flat body is not routed to any one flavour", goos)
		}

		// REVERSE: a WHOLE-FILE replacement is never stranded — the converter drops the marked file
		// from the convert set and emits no body for what it declares, so there is nothing beside it
		// to collide with and no registration is owed. Corpus instance: syscall's Exec/forkExec under
		// linux/ and windows/ read as stranded the moment a darwin-scoped entry existed, three
		// findings and zero real, while both platforms built at 0 errors.
		if bodies.strandedOn("wholeFileReplacement", goos) {
			t.Errorf("strandedOn(wholeFileReplacement, %s) = true — a whole-file [module: GoManualConversion] "+
				"replacement is the THIRD displacement mechanism: the converter never emits a body for it, "+
				"so nothing can collide and no registry entry is owed", goos)
		}
	}

	// AND THE ARM MUST STILL BITE. The exemption above is narrow by construction — it keys on a file
	// being marked AND not an `_impl` companion — so the genuinely stranded case must survive it. If
	// this ever goes quiet, the exemption has widened into a blanket and the reverse arm is disarmed.
	if !bodies.strandedOn("perFlavorOnly", "linux") {
		t.Error("strandedOn(perFlavorOnly, linux) = false — the reverse arm no longer fires on a plain " +
			"`_impl` body that no registration covers; the whole-file exemption has widened into a blanket")
	}
}

// manualConversionMarker moved to testConversion.go (2026-09-04), where handOwnHostTestTarget needs
// the same predicate in PRODUCTION code: that function asks "is the C# counterpart at this output
// path hand-owned?" and this guard asks "does a hand-own declare what it displaces?" — one question
// about one marker, so one regexp. A second copy here would be the silent-duplication shape (two
// definitions of one fact, merging without a conflict); it is caught by the compiler today only
// because both copies carry the same name, which is luck rather than a guard.

// A line that DECLARES something: an access modifier at the start, after any attributes. Selecting
// the line first and harvesting names second is what makes TUPLE RETURN TYPES work —
// `internal static unsafe (nint wpid, error err) wait4(…)` puts parentheses in the RETURN, so any
// pattern that walks from the modifier to the first `(` stops in the wrong place. Measured, not
// guessed: the first cut of this guard did exactly that and reported 22 false failures, including
// `wait4`, `Select` and `Adjtimex` — bodies in this very branch.
var csharpDeclarationLine = regexp.MustCompile(`(?m)^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)\b[^\n;]*\([^\n]*$`)

// Every `Name(` on such a line. Deliberately over-collecting: a tuple return contributes its field
// names too, and that is fine. Over-collection can only cost a false PASS on a name nothing defines,
// which the corpus build then catches at the first consumer; under-collection costs a false FAIL,
// which is the failure mode that makes a merge-seam guard worthless — nobody believes a check that
// cries wolf, and this one exists to be believed.
// A C# callable name, with an OPTIONAL type-parameter list between the name and its parameter list:
// a hand-owned body for a GENERIC Go function is spelled `overlaps<E>(…)`, and the first such
// registration (slices.overlaps, the overlap-race remedy, 2026-09-03) was reported as having no
// body by the un-widened form, which required `(` to follow the name directly.
var csharpCallableName = regexp.MustCompile(`\b([A-Za-z_][A-Za-z0-9_]*)\s*(?:<[^()]*>)?\s*\(`)

// TestManualConversionRegistrationsDisplaceSomething is the SOURCE direction of the same ledger: a
// registration must actually DISPLACE a generated body. The guard above asks whether the
// displacement ARRIVES; this one asks whether it ever DEPARTS, and the two fail apart.
//
// The defect, stated as the defect. `manualConversionFuncs` is keyed by NAME, and a Go METHOD's key
// carries its receiver — "Value.extendSlice", never the bare "extendSlice". The bare form matches no
// declaration, so the converter displaces nothing, the generated body survives, the hand-owned
// `_impl.cs` body beside it becomes a duplicate, and the package dies CS0111. The guard above passes
// cleanly on exactly that mistake, because the `_impl.cs` really does define the member — which is
// how the trap has been paid three separate times, most recently by reflect's `extendSlice`
// (2026-09-01). Worse than an ordinary build failure: a `-tests` build that fails leaves the PREVIOUS
// comparison record in place, so the run reports the old verdicts and reads as "the fix does not
// work" rather than as a compile error.
//
// The registry's OTHER field has had this guard for a while, with the reasoning already written
// down. TestEveryManualConversionScopeNamesAKnownGOOS exists because a scope naming "win" "matches no
// target at all, which silently turns the entry off everywhere — the auto body is emitted and
// compiles, and the hand-own it was protecting is simply gone." That sentence is true word for word
// of a mistyped NAME. Only the name field lacked the check.
//
// WITNESS. The converter writes one fixed line wherever it displaces a func body (visitFuncDecl.go's
// placeholder), so this is a filesystem scan of the same cost class as the guard above: no
// type-checking, no overload resolution, no Go toolchain.
//
// IN SYNC BY CONSTRUCTION, not by luck. A hand-own bank must ship its regenerated package with it, or
// the committed corpus carries BOTH the generated body and the new `_impl.cs` body and fails to
// compile. Registration and placeholder therefore land in the same commit, and this guard cannot red
// merely because the corpus lags the converter.
//
// WHAT IT CANNOT SEE, stated because a guard's blind spot is worth more written down than
// rediscovered. The placeholder names funcDecl.Name.Name — the member alone — so this test strips a
// key's receiver before matching and therefore cannot tell "Value.extendSlice" from a bare
// "extendSlice" while a placeholder for that member already sits in the corpus. That is not the
// trap's actual shape: a NEW hand-own keyed bare produces no placeholder at all (the same mechanism
// that makes reflect.methodName visible here — a key matching no declaration displaces nothing), and
// isManualFuncDecl is the single decision behind both the displacement and the placeholder, so
// witness and displacement cannot disagree. What survives is the narrow case of editing an ALREADY
// CORRECT key down to its bare form without regenerating, where the stale placeholder answers for
// the new key until the next regen — and the CS0111 that follows is caught by the corpus build, one
// layer out, exactly as the over-collection in handOwnedDefinitionsIn above is.
//
// NO EXEMPTION LIST, and that was measured rather than assumed. The three `runtime` entries declared
// in runtime2.go look structurally unwitnessable — `runtime/runtime2.cs` is a whole-file hand-own, so
// the converter never emits that file — but it emits their placeholders into `runtime2.cs.auto`, the
// review sibling it writes for exactly that case, and searching the siblings takes the residual set
// to zero. The sibling is the right place to look, not a loophole: it is the converter's own record
// of what it WOULD emit, which is precisely this test's question. Note the asymmetry with
// handOwnedDefinitionsIn above, which SKIPS `.cs.auto` — its question is "does a HAND-OWN define
// this?", and a sibling is not a hand-own. Same files, opposite treatment, both correct; do not
// "unify" them.
//
// manualConversionTypes is deliberately NOT covered. All three of its entries (guintptr, puintptr,
// muintptr) are witnessed in that same sibling, so a types arm would restate this one over three
// names and add no independent signal.
func TestManualConversionRegistrationsDisplaceSomething(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	if _, err := os.Stat(coreDir); err != nil {
		t.Skip("src/core is not beside the converter; nothing to walk")
	}

	// The GOROOT the corpus was converted from — the source of the weaker, test-side witness below.
	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	var undisplaced []string
	witnessed := 0
	testWitnessed := 0

	for pkg, funcs := range manualConversionFuncs {
		placeholders := generatedFuncPlaceholders(t, filepath.Join(coreDir, filepath.FromSlash(pkg)))

		// Lazily parsed on the first entry that misses its production placeholder — a hand-own of a
		// GOROOT `_test.go` declaration (reflect's export_test.go IsExported) has no on-disk
		// placeholder until reflect `-tests` has run in THIS tree, so a clone that has never run it
		// would report the entry undisplaced. Its Go declaration is in the package's own test files,
		// which every clone has, so that is the witness. Nil until needed; empty map means "parsed,
		// none found" (distinct from "not yet parsed").
		var testFuncs map[string]bool

		for name := range funcs {
			// A method registration names the RECEIVER type and the member; the placeholder names the
			// member alone, because it is written from funcDecl.Name.Name.
			member := name
			if dot := strings.LastIndex(member, "."); dot >= 0 {
				member = member[dot+1:]
			}

			if placeholders[member] {
				witnessed++
				continue
			}

			// Weaker witness: the production body was not displaced on disk, but the name IS a
			// declaration in the package's GOROOT test files — a test-only hand-own (export_test.go).
			// Tallied separately; the production arm above stays first and decides the common case.
			if testFuncs == nil {
				testFuncs = testDeclaredFuncs(goRoot, pkg)
			}
			if testFuncs[member] {
				testWitnessed++
				continue
			}

			undisplaced = append(undisplaced, pkg+"."+name)
		}
	}

	sort.Strings(undisplaced)

	// The weaker witness is tallied separately AND surfaced: an entry that relies on it has no
	// on-disk production placeholder, so a reviewer who sees this count non-zero can confirm each
	// such entry is a genuine GOROOT-test-file hand-own (IsExported; GCBits when it lands) rather
	// than a production displacement the strong arm should have caught.
	if testWitnessed > 0 {
		t.Logf("%d registration(s) witnessed only by their GOROOT _test.go declaration (no on-disk "+
			"production placeholder); this is the test-side hand-own case (e.g. reflect.IsExported)", testWitnessed)
	}

	for _, entry := range undisplaced {
		t.Errorf("manualConversionFuncs registers %s, but the converter displaced no body for it — the "+
			"entry matches no Go declaration in that package. A method key needs its receiver "+
			"(\"Value.extendSlice\", not \"extendSlice\"); a renamed or removed upstream declaration needs "+
			"the entry retired. Either way the generated body survives, a hand-owned one beside it is a "+
			"duplicate, and the package fails CS0111 — reported through a -tests run that reuses the "+
			"previous comparison record and so reads as a failed fix", entry)
	}

	// A census that finds nothing reports every registration as undisplaced, which reads as a
	// catastrophic registry rather than as a broken instrument. Anchor it, the way the scope guard
	// anchors its own.
	if witnessed == 0 {
		t.Error("no registration matched a generated placeholder anywhere in the corpus; the placeholder " +
			"census is broken, not the registry")
	}
}

// testDeclaredFuncs returns the set of top-level function and method NAMES declared in the GOROOT
// package's own `_test.go` files (both the in-package and the external `<pkg>_test` test files sit
// in the same directory). It is the weaker, test-side witness for a registration whose production
// body was not displaced on disk — a hand-own of a GOROOT test declaration
// (reflect/export_test.go's IsExported), whose generated placeholder exists only where the package's
// `-tests` conversion has run. Keyed by the member name (funcDecl.Name.Name), matching the
// production arm's receiver-stripped `member`. An unreadable/absent GOROOT package yields an empty
// set, which correctly leaves the entry undisplaced rather than silently witnessing it.
func testDeclaredFuncs(goRoot, pkg string) map[string]bool {
	names := map[string]bool{}

	if goRoot == "" {
		return names
	}

	dir := filepath.Join(goRoot, "src", filepath.FromSlash(pkg))

	entries, err := os.ReadDir(dir)
	if err != nil {
		return names
	}

	fset := token.NewFileSet()

	for _, entry := range entries {
		name := entry.Name()

		if entry.IsDir() || !strings.HasSuffix(name, "_test.go") {
			continue
		}

		file, parseErr := parser.ParseFile(fset, filepath.Join(dir, name), nil, 0)
		if parseErr != nil {
			continue
		}

		for _, decl := range file.Decls {
			if funcDecl, ok := decl.(*ast.FuncDecl); ok && funcDecl.Name != nil {
				names[funcDecl.Name.Name] = true
			}
		}
	}

	return names
}

// generatedFuncPlaceholders returns the member names the converter displaced a func body for in ONE
// package: the generated `.cs` at the package root, the per-GOOS folders layout L3 routes a
// platform-scoped declaration into, and the `.cs.auto` review siblings (see the caller's note on why
// the siblings count here and not in handOwnedDefinitionsIn).
//
// Scope is the package's OWN files — root plus GOOS folders — not a full recursive walk. A converted
// package's subdirectories are usually OTHER packages (net/http holds cgi, httptest, …), and counting
// a child's placeholder for its parent would be a false PASS on exactly the question this test asks.
func generatedFuncPlaceholders(t *testing.T, packageDir string) map[string]bool {
	t.Helper()

	witnessed := map[string]bool{}

	dirs := []string{packageDir}

	// Layout L3's platform folders, and ONLY those: a GOOS-named subdirectory can also be a package
	// (`internal/syscall/windows` is the corpus's only one), but a package's placeholders sit in its
	// OWN platform folder — depth 2 from here — and this walk stops at depth 1, so a child package's
	// placeholders cannot answer for its parent. Measured 2026-09-01 rather than assumed:
	// `internal/syscall/windows/*.cs` carries no placeholder at all; all NINE of them are in
	// `internal/syscall/windows/windows/`, its own platform folder. A csproj test was written here
	// first and then removed: it could not be made to fire, because the depth rule already closes the
	// case, and an unexercisable branch in a guard is exactly what this file's neighbours refuse to
	// carry.
	if entries, err := os.ReadDir(packageDir); err == nil {
		for _, entry := range entries {
			if entry.IsDir() && isKnownGOOS(entry.Name()) {
				dirs = append(dirs, filepath.Join(packageDir, entry.Name()))
			}
		}
	}

	for _, dir := range dirs {
		entries, err := os.ReadDir(dir)
		if err != nil {
			continue
		}

		for _, entry := range entries {
			name := entry.Name()

			if entry.IsDir() || !(strings.HasSuffix(name, ".cs") || strings.HasSuffix(name, ".cs.auto")) {
				continue
			}

			content, readErr := os.ReadFile(filepath.Join(dir, name))
			if readErr != nil {
				continue
			}

			for _, match := range generatedFuncPlaceholder.FindAllStringSubmatch(string(content), -1) {
				witnessed[match[1]] = true
			}
		}
	}

	return witnessed
}

// The displacement witness, anchored on the converter's own prefix rather than on the trailing prose.
// `runtime/runtime2.cs` carries 13 HAND-WRITTEN lines ending in the same words ("func set is
// hand-converted with managed semantics — see the package's *_impl.cs"), a wording the converter emits
// nowhere; a pattern loose enough to match those would let a hand-own satisfy a guard about generated
// output, and this test would report clean while measuring nothing.
var generatedFuncPlaceholder = regexp.MustCompile(`(?m)^// go2cs generated this placeholder — func ([A-Za-z_][A-Za-z0-9_]*) is hand-converted\b`)

// TestCallableNameAdmitsOnlyGenericHandOwnBodies is the positive control for csharpCallableName's
// type-parameter arm (the overlap-race remedy, 2026-09-03). The un-widened form required `(` to follow
// the name directly, so the FIRST registered generic hand-own body — slices.overlaps<E> — was reported
// by TestManualConversionRegistrationsHaveBodies as having no body: a false FAIL on a body that exists.
// Three assertions, so the widening cannot read as a loosening: (1) the old form reproduces the false
// FAIL on that body and the widened form collects it; (2) on a non-generic declaration both forms agree;
// (3) over EVERY hand-own file in the corpus, the widened form collects exactly the old form's names
// plus names whose declaration carries a type-parameter list — nothing else is newly admitted, so a
// dead hand-own (a name nothing displaces) is named by the widened collector exactly as it was before.
func TestCallableNameAdmitsOnlyGenericHandOwnBodies(t *testing.T) {
	old := regexp.MustCompile(`\b([A-Za-z_][A-Za-z0-9_]*)\s*\(`)
	generic := regexp.MustCompile(`\b([A-Za-z_][A-Za-z0-9_]*)\s*<[^()]*>\s*\(`)

	collect := func(re *regexp.Regexp, line string) map[string]bool {
		names := map[string]bool{}
		for _, m := range re.FindAllStringSubmatch(line, -1) {
			names[m[1]] = true
		}
		return names
	}

	// (1) the motivating body, spelled as slices/slices_impl.cs spells it
	genericDecl := "    internal static bool overlaps<E>(slice<E> a, slice<E> b)"

	if collect(old, genericDecl)["overlaps"] {
		t.Fatalf("the un-widened callable-name form collected overlaps<E> — the false FAIL this control reproduces no longer reproduces; re-derive the control")
	}

	if !collect(csharpCallableName, genericDecl)["overlaps"] {
		t.Fatalf("csharpCallableName does not collect the generic hand-own body %q", genericDecl)
	}

	// (2) a non-generic declaration reads the same under both forms
	plainDecl := "    public static bool AnyOverlap(slice<byte> x, slice<byte> y)"

	if !collect(old, plainDecl)["AnyOverlap"] || !collect(csharpCallableName, plainDecl)["AnyOverlap"] {
		t.Fatalf("both forms must collect the non-generic declaration %q", plainDecl)
	}

	// (3) corpus-wide: new − old is exactly the generic declarations, and the motivating body is in it
	coreDir := filepath.Join("..", "core")
	newlyAdmitted := map[string]string{}

	err := filepath.Walk(coreDir, func(path string, info os.FileInfo, err error) error {
		if err != nil || info.IsDir() || !strings.HasSuffix(path, ".cs") || strings.HasSuffix(path, ".cs.auto") {
			return nil
		}

		content, readErr := os.ReadFile(path)

		if readErr != nil {
			return nil
		}

		text := string(content)

		if !strings.Contains(filepath.Base(path), "_impl") && !manualConversionMarker.MatchString(text) {
			return nil
		}

		for _, line := range csharpDeclarationLine.FindAllString(text, -1) {
			before := collect(old, line)

			for name := range collect(csharpCallableName, line) {
				if before[name] {
					continue
				}

				if !collect(generic, line)[name] {
					t.Errorf("%s: the widened collector newly admits %q on a line with no type-parameter list: %q", path, name, strings.TrimSpace(line))
				}

				newlyAdmitted[name] = path
			}
		}

		return nil
	})
	if err != nil {
		t.Fatalf("walking %s: %v", coreDir, err)
	}

	if _, ok := newlyAdmitted["overlaps"]; !ok {
		t.Fatalf("the corpus walk did not newly admit slices.overlaps<E> — the motivating generic hand-own body is missing from %s", coreDir)
	}
}

// A registry key is the package's ON-DISK path, and for a GOROOT-vendored package that carries the
// `vendor/` prefix the type-checker's Package.Path() does not (conversionDriver loads by directory;
// `go list` reports a vendored directory's ImportPath unprefixed). The first vendored registration —
// vendor/golang.org/x/crypto/internal/alias — was never consulted for exactly that reason: its two-seeded
// three-target diff read ZERO paths. The lookup canonicalizes through resolveGorootVendoredPath, and this
// guard holds it there from both spellings. Skipped, never vacuously green, when GOROOT has no vendored
// copy to resolve against.
func TestManualFuncLookupReachesVendoredRegistrationFromTypeCheckerSpelling(t *testing.T) {
	const onDisk = "vendor/golang.org/x/crypto/internal/alias"
	const typeChecker = "golang.org/x/crypto/internal/alias"

	if _, registered := manualConversionFuncs[onDisk]["AnyOverlap"]; !registered {
		t.Fatalf("manualConversionFuncs[%q] no longer registers AnyOverlap; this guard needs a vendored registration to exercise", onDisk)
	}

	if _, err := os.Stat(filepath.Join(build.Default.GOROOT, "src", "vendor", filepath.FromSlash(typeChecker))); err != nil {
		t.Skipf("GOROOT %s carries no vendored %s: nothing to resolve against", build.Default.GOROOT, typeChecker)
	}

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "alias_purego.go", "package alias\nfunc AnyOverlap(x, y []byte) bool { return false }\nfunc InexactOverlap(x, y []byte) bool { return false }\n", 0)

	if err != nil {
		t.Fatal(err)
	}

	decls := map[string]*ast.FuncDecl{}

	for _, decl := range file.Decls {
		if fn, ok := decl.(*ast.FuncDecl); ok {
			decls[fn.Name.Name] = fn
		}
	}

	for _, spelling := range []string{onDisk, typeChecker} {
		if !isManualFuncDeclInPackage(spelling, "linux", decls["AnyOverlap"]) {
			t.Errorf("isManualFuncDeclInPackage(%q, linux, AnyOverlap) = false; the vendored registration is unreachable from that spelling", spelling)
		}

		if isManualFuncDeclInPackage(spelling, "linux", decls["InexactOverlap"]) {
			t.Errorf("isManualFuncDeclInPackage(%q, linux, InexactOverlap) = true; only AnyOverlap is registered", spelling)
		}
	}

	// The canonicalization is a no-op for a plain stdlib path: crypto/internal/alias keys itself.
	if !isManualFuncDeclInPackage("crypto/internal/alias", "linux", decls["AnyOverlap"]) {
		t.Errorf("isManualFuncDeclInPackage(crypto/internal/alias, linux, AnyOverlap) = false; the unvendored twin's registration regressed")
	}
}
