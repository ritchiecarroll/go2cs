// declaredNotImplemented_test.go - Gbtc
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

// q82 (COORD f0815504ac, promoted from C1's SUGGEST the moment RED 7 made it a live class): the
// DECLARED-NOT-IMPLEMENTED census.
//
// go2cs-gen's PartialStubGenerator mints a THROWING stub for every partial method DEFINITION in a
// converted package that has no implementing part:
//
//	throw new NotImplementedException("<name>: no implementation reached this compilation
//	                                   (assembly, cgo, or a linkname whose push did not arrive)")
//
// Such a package COMPILES CLEAN and dies at the first call. RED 7 is that failure reaching a user:
// crypto/internal/fips140.setIndicator killed the GolibTests host on gcm.Seal's first call
// (i9, c0eecf8850), and internal/sync's mutex linknames were the same class found earlier and
// unreached. This guard makes the population visible and keeps it from growing silently.
//
// ⚠ IT IS A CENSUS, NOT A BAN. The population is legitimately large: Go implements much of runtime
// and syscall in ASSEMBLY, and a stub is the honest answer for those until the corpus grows a body.
// What the guard refuses is VACUITY — a walk that finds nothing, or a known member that stops being
// found — because a census that cannot see its own subjects reads exactly like a clean tree.
//
// ⚠ AND ONE THING IT DOES BAN, which is the shape COORD ruled at 7fad751867 on C2's sizing
// (5ffca1e37b, candidate (f)): a push target that becomes a throwing stub WITHOUT A ROW SAYING SO.
// declaredPushStubs below is the declared set — every (package, symbol) for which a //go:linkname
// push exists in this corpus and the corpus still emits a stub — and the check is measured ==
// declared, in both directions. A release hop that strands a new member then arrives as a FAILING
// GATE naming it, instead of as a dead test host forty minutes into a validation row. The 1.23 → 1.24
// hop would have listed 24 such members; nothing asked, and two of them reached users three weeks
// apart.
//
// The predicate is the GENERATOR'S, ported rather than restated: a partial definition with no
// implementing part, minus the -tests init hook (a classic partial, designed to erase), minus a
// declaration another generator is obliged to implement ([LibraryImport]).
func TestDeclaredNotImplementedCensus(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	// ⚠ NOT A SKIP. This was written as t.Skip and the skip ate this guard's own vacuity control:
	// the control pointed the walk at a directory that does not exist, the test took the skip door,
	// and `go test` printed `ok` — a regression that was supposed to prove the guard can fail
	// instead proved the guard can VANISH, in the one costume that reads as a pass. src/core is THE
	// stdlib of the one tree and is always beside the converter, so its absence is a broken
	// checkout, not a configuration this guard should tolerate quietly.
	if _, err := os.Stat(coreDir); err != nil {
		t.Fatalf("%s is not beside the converter (%v); this census cannot run, and a SKIP here would "+
			"read as a clean tree", coreDir, err)
	}

	census, err := declaredNotImplementedIn(coreDir)

	if err != nil {
		t.Fatalf("walking %s: %v", coreDir, err)
	}

	// ⚠ ANTI-VACUITY, arm 1: the walk must SEE declarations at all. A regex that stops matching, a
	// layout change, a walk rooted at the wrong directory — each reads as "nothing is stubbed".
	if census.declarations == 0 {
		t.Fatalf("VACUOUS: 0 partial declarations found under %s; the walk is measuring nothing", coreDir)
	}

	// ⚠ ANTI-VACUITY, arm 2: the members whose absence has already cost the fleet a dead test host
	// must be IN the population. Arm 3 below also requires every control to be a PUSHED member (its
	// //go:linkname push exists in this corpus), so an assembly stub cannot serve here, however many
	// hosts it has killed. Today the controls are two pushes that do not arrive and have callers in
	// their own package: internal/runtime/maps.mapKeyError (pushed from runtime/map_swiss.cs) and
	// runtime/pprof.mach_vm_region (pushed from runtime/darwin/sys_darwin.cs), which the Phase 5
	// exported-reach census confirmed a user API reaches (pprof.StartCPUProfile on darwin, arm64 and
	// x64; docs/phase4/CENSUS-phase5-exported-reach.md). It replaced
	// internal/coverage/cfile.getCovCounterList in the commit that bodied that one (emit_impl.cs,
	// 2026-10-10), which moved to the negative controls below.
	//
	// It named RED 7's three (fips140's getIndicator, setIndicator and fatal) until RED 7 (a) gave
	// them bodies; they left the population and their rows left this list IN THAT SAME COMMIT, which
	// is the mechanism working rather than an exception to it. The four internal/sync Mutex hooks
	// (runtime_SemacquireMutex, runtime_Semrelease, runtime_canSpin, runtime_doSpin) followed the
	// same route on 2026-09-22 when internal/sync/runtime_impl.cs bodied them: they are NEGATIVE
	// controls below now. The list is never left EMPTY: an empty list passes this arm while
	// checking nothing, which is the vacuity the arm exists to refuse. So when a control gains a
	// body, a member that is still stubbed replaces it in the same commit.
	controls := []string{
		"internal/runtime/maps/runtime_swiss.cs:mapKeyError",
		"runtime/pprof/darwin/vminfo_darwin.cs:mach_vm_region",
	}

	if len(controls) == 0 {
		t.Fatalf("VACUOUS: the positive control list is empty; a member that is still stubbed must replace the last one to gain a body")
	}

	// ⚠ ANTI-VACUITY, arm 5 — the NEGATIVE control, and the only one this census has against a
	// BUILD. i9 measured the pair directly (c5f7b4b90d): time.cs declares runtimeNow and runtimeNano
	// identically, runtime/time.cs pushes both, and the built package mints a throwing stub for
	// runtimeNow and NONE for runtimeNano — because time_impl.cs implements runtimeNano and nothing
	// implements runtimeNow. A text census must reproduce that split or it is not reading the same
	// population the generator does; reading runtimeNano as stubbed is how this census found its own
	// brace-style blind spot, which had it overstating the population by 122 members.
	implemented := []string{
		"time/time.cs:runtimeNano",

		// ⚠ runtimeNow MOVED HERE FROM THE CONTROLS ABOVE, and the guard is what moved it. It was
		// RED 7's second reached member — time.Now() threw at 4586b299a0 because this stub was what
		// it called — and it is the member whose TUPLE return type caught this census's own first
		// blind spot. Its body landed at f9baa2a778 (time_impl.cs, `runtimeNow() => now()`), and
		// on the first run against the tip carrying that merge this file's own CONTROL MISSING arm
		// fired and said, in as many words, to drop it from the control list in the commit that
		// supplied the body. That is the arm doing its job on the author, and it is worth more as a
		// NEGATIVE control now than it was as a positive one: it is a second member, beside
		// runtimeNano, whose implementing part this predicate must SEE.
		"time/time.cs:runtimeNow",

		// The four internal/sync Mutex hooks, moved here from the controls above in the commit that
		// bodied them (internal/sync/runtime_impl.cs, 2026-09-22), by the same rule that moved
		// runtimeNow. They sit in a package whose companion ALREADY implemented two of the file's
		// seven partials (throw, fatal), so the predicate must see a split inside one file, which
		// is the shape this arm exists to check.
		"internal/sync/runtime.cs:runtime_SemacquireMutex",
		"internal/sync/runtime.cs:runtime_Semrelease",
		"internal/sync/runtime.cs:runtime_canSpin",
		"internal/sync/runtime.cs:runtime_doSpin",

		// Moved here from the controls above in the commit that bodied it (internal/coverage/cfile/
		// emit_impl.cs, 2026-10-10): the empty counter list Go's runtime walk returns for a binary built
		// without -cover. Its implementing part sits in a companion beside a converted file that keeps
		// the declaration, the same split runtimeNano checks.
		"internal/coverage/cfile/emit.cs:getCovCounterList",
	}

	found := map[string]bool{}

	for _, entry := range census.stubs {
		found[entry.file+":"+entry.name] = true
	}

	for _, control := range controls {
		if !found[control] {
			t.Errorf("CONTROL MISSING: %s is not in the stubbed population; either it gained a body "+
				"(then drop it from this list, in the same commit that supplies the body) or the "+
				"census stopped seeing its shape", control)
		}
	}

	for _, member := range implemented {
		if found[member] {
			t.Errorf("FALSE MEMBER: %s is reported as having no implementation, but a build of its "+
				"package mints no stub for it — an implementing part exists that this census cannot "+
				"see, so the whole population is overstated by however many share its shape", member)
		}
	}

	// ⚠ ANTI-VACUITY, arm 3: the PUSH column must be populated for the members whose push is known
	// to exist, and arm 4: no member may be reported as pushed by ITS OWN FILE. Both guard the
	// push/pull discrimination, which is the reading's load-bearing half — RED 7 is exactly the
	// claim "a push exists in this corpus and did not arrive", and a column that cannot tell a
	// push from a declaration's own self-naming permission marker answers that question wrongly
	// while looking populated. Measured before the discriminator was added: fips140's three — the
	// class's own controls — were each attributed to the very file the census reports them from.
	pushedBy := map[string]string{}

	for _, entry := range census.stubs {
		pushedBy[entry.file+":"+entry.name] = entry.pushedBy
	}

	for _, control := range controls {
		if by, present := pushedBy[control]; present && by == "" {
			t.Errorf("PUSH COLUMN EMPTY for %s: every control is a member whose //go:linkname push "+
				"exists in this corpus, so an empty column means the discriminator stopped seeing "+
				"pushes, not that the push went away", control)
		}
	}

	for _, entry := range census.stubs {
		if entry.pushedBy != "" && entry.pushedBy == entry.file {
			t.Errorf("SELF-ATTRIBUTED PUSH: %s:%s is reported as pushed by its own file. A "+
				"two-argument //go:linkname in the declaring file is the declaration's own "+
				"permission marker, not a push; the body is the discriminator", entry.file, entry.name)
		}
	}

	// The reading, logged rather than asserted: a number that moves is a fact for whoever moved it,
	// and a per-class split says WHY each member has no body. PUSHED names the members RED 7 is
	// about — a //go:linkname push exists in this very corpus and did not arrive.
	t.Logf("declarations %d · stubbed %d · packages %d", census.declarations, len(census.stubs), census.packages)
	t.Logf("by reason (the consumer's OWN marker): linkname %d · cgo %d · assembly-or-other %d",
		census.linkname, census.cgo, census.other)
	t.Logf("of the %d, a //go:linkname PUSH exists in this corpus for %d — RED 7's population, "+
		"counted over ALL stubs and not only the linkname-marked, because the push is the "+
		"producer's directive and the consumer need carry none", len(census.stubs), census.pushed)

	var pushed []string

	measured := map[string]string{}

	for _, entry := range census.stubs {
		if entry.pushedBy != "" {
			pushed = append(pushed, fmt.Sprintf("%s:%s  <- %s  (first-order callers in its own package: %d)",
				entry.file, entry.name, entry.pushedBy, entry.callers))
			measured[memberKey(entry.file, entry.name)] = entry.pushedBy
		}
	}

	sort.Strings(pushed)

	for _, line := range pushed {
		t.Logf("  PUSH DID NOT ARRIVE  %s", line)
	}

	// ⚠ THE GATE: the MEASURED set equals the DECLARED set. This is what turns a census into
	// something that stops the next release hop from arriving as a dead test host forty minutes into
	// a row. RED 7 reached the fleet as two separate surprises — fips140's setIndicator killing a
	// GolibTests host on gcm.Seal, and time.runtimeNow making every converted time.Now() throw —
	// three weeks apart, and both were in this population the whole time with nothing asking.
	//
	// A member is keyed by (package, symbol) with per-GOOS flavours of ONE member folded together,
	// because one package's flavours compile as one assembly and a stub in each is one member, not
	// three. Every row carries a DISPOSITION: what is owed, not merely that something is.
	var appeared, vanished []string

	for member := range measured {
		if _, declared := declaredPushStubs[member]; !declared {
			appeared = append(appeared, member+"  <- "+measured[member])
		}
	}

	for member, disposition := range declaredPushStubs {
		_ = disposition

		if _, present := measured[member]; !present {
			vanished = append(vanished, member+"  ("+disposition+")")
		}
	}

	sort.Strings(appeared)
	sort.Strings(vanished)

	for _, member := range appeared {
		t.Errorf("UNDECLARED STRANDED TARGET: %s\n"+
			"\ta //go:linkname push exists in this corpus for this member and the corpus emits it as a "+
			"throwing stub, and no row here says so. Either the release hop added it — the case this "+
			"gate exists for — or a body was removed. It compiles either way and dies at the first "+
			"call. THE TREE MOVED, NOT THE TABLE: add the row WITH its disposition in the same commit "+
			"that moved the tree.", member)
	}

	for _, member := range vanished {
		t.Errorf("DECLARED BUT NOT MEASURED: %s\n"+
			"\tthis member is no longer a throwing stub, which is good news that has to be recorded: "+
			"delete its row IN THE SAME COMMIT that supplied the body. A declared set that outlives its "+
			"members stops being a description of the tree.", member)
	}

	var unreached int

	for _, entry := range census.stubs {
		if entry.pushedBy != "" && entry.callers == 0 {
			unreached++
		}
	}

	t.Logf("declared %d · measured %d · appeared %d · vanished %d — the unit on BOTH sides is the "+
		"TARGET (package, symbol) with per-GOOS flavours of one member folded; the %d lines above are "+
		"per FILE, which is the same population counted in a different unit and is never the one "+
		"compared", len(declaredPushStubs), len(measured), len(appeared), len(vanished), len(pushed))
	t.Logf("first-order reach: %d of the pushed stubs have NO caller in their own package. That is not "+
		"a safety margin — it is the condition under which a member reaches a user as a dead host "+
		"instead of as a failing gate", unreached)
}

// memberKey folds a stub's file path and name into the (package, symbol) key the declared set is
// written in, collapsing a layout-L3 GOOS folder into its parent package.
func memberKey(file, name string) string {
	parts := strings.Split(file, "/")

	if len(parts) > 0 {
		parts = parts[:len(parts)-1]
	}

	if n := len(parts); n > 0 {
		switch parts[n-1] {
		case "windows", "linux", "darwin":
			parts = parts[:n-1]
		}
	}

	return strings.Join(parts, "/") + "." + name
}

// The dispositions a declared row can carry. They record what is OWED for that member, so the set
// reads as work rather than as a list of names.
const (
	// dispositionForward — a //go:linkname registry row forwards it to a real body upstream. Ruled
	// for crypto/internal/fips140's three and crypto/internal/sysrand's fatal (COORD 1fa7940a0 on
	// G's split 5bb307d57e); the rows are G's seat, not yet cut at this tip.
	dispositionForward = "registry row owed (G)"

	// dispositionFatalReport — a fatal-family member whose package CANNOT take a registry row: the
	// cycle test excludes it (runtime references internal/runtime/maps) or its csproj carries no
	// runtime reference at all (crypto/rand, internal/sync). It takes the golib FatalReport hand-own
	// instead, precedent sync/mutex.cs:51. C1's.
	//
	// ⚠ ZERO members as of this commit, and the same way dispositionCompanion reached zero: the four
	// it named — internal/sync.fatal, internal/sync.throw, crypto/rand.fatal and
	// internal/runtime/maps.fatal — got their bodies in the commit that deletes their rows. This gate
	// named all four by name on the run that first saw those bodies, which is the second time in one
	// day it has fired on its own author and told them what to do.
	dispositionFatalReport = "FatalReport hand-own owed (C1)"

	// dispositionCompanion — a hand-owned *_impl.cs body supplies it. ⚠ ZERO members at this tip, and
	// that is the vocabulary being honest rather than an oversight: time.runtimeNow held it until
	// f9baa2a778 merged at 17a5819956, and the rule this table runs on — a member that gains a body
	// has its row removed in the SAME commit — removed the row. The word stays because the next
	// stranded hand-own will need it.
	dispositionCompanion = "companion body owed (C1)"

	// dispositionSuppliedElsewhere — the corpus does not need the pushed body at all, because golib
	// supplies the semantics the member exists for. Ruled for runtime's seventeen swiss-map
	// intrinsics (COORD 7bc9d58d43): a converted program's maps are golib's map<K,V>, not runtime's
	// swiss tables.
	//
	// ⚠ THE DISPOSITION HOLDS; HALF OF ITS STATED REASON DOES NOT, and this file says so rather than
	// repeating it. The ruling's parenthetical is "nothing in the emission calls them"; measured
	// here, SIX of the seventeen DO have first-order callers and ELEVEN have none. The six are
	// mapaccess1, mapaccess2, mapassign, mapaccess2_faststr, mapassign_faststr and mapdelete_faststr,
	// and every caller is inside runtime's OWN package — runtime's wrappers calling their own stubs,
	// nothing from outside. C2's independent reach arm names THE SAME SIX (d1c5eca96c) and gives
	// different per-member COUNTS, because its predicate and this one differ: this one is textual and
	// counts every mention of `name(` outside a comment. Two instruments agreeing on the SET and not
	// on the counts is the honest state, and the set is what the disposition rests on.
	//
	// So the row is disposed by its MECHANISM and read by its MEASUREMENT, which is the pair that
	// stops a disposition from quietly becoming a claim nobody re-checks.
	dispositionSuppliedElsewhere = "supplied by golib; the pushed body is not needed"

	// dispositionLatent — nothing is owed yet. The push exists, it did not arrive, and nothing has
	// been measured to reach it. That is NOT "safe": it is the precise condition that makes a
	// member a booby trap, because no standing gate reddens it until something calls it.
	dispositionLatent = "latent"
)

// declaredPushStubs is the DECLARED set: every (package, symbol) for which a //go:linkname push
// exists in this corpus and the corpus nonetheless emits a throwing stub.
//
// Measured at claude/version-go1.24.13 17a5819956 — 88 members, and FROZEN there at MEMBER level by
// two instruments built from different inputs:
//
//	C1 (this file)  walks the CORPUS: every bodyless partial with no implementing part, cross-
//	                referenced against every //go:linkname push in the corpus whose local name has a body
//	C2 (census7)    walks the GO SOURCE at the pin for push targets, then classifies what the corpus
//	                emits for each
//
// They first read 89 and 70. The 19-member difference was settled not by either instrument but at Go's
// own source (C1 a11683a40d, C2 accepting and re-running at 98ccc78e6e): runtime's 17 swiss-map
// intrinsics, where Go's own comment at runtime/map_swiss.go:85 says "mapaccess1 is pushed from
// internal/runtime/maps"; internal/sync.throw, pushed at runtime/panic.go:1061; and
// crypto/internal/fips140hash.sha3Unwrap, the one member whose pusher is NOT runtime
// (crypto/sha3/sha3.go:104). C2's own causes were a census SCOPED to one pusher package — which could
// not see the first two classes by construction — and a lookbehind that excluded C#'s verbatim `@throw`.
// C2 then posted all 88 rows by name (d1c5eca96c) and C1 diffed them against this table: ZERO members
// on either side. That is the freeze condition, and a per-package agreement would not have been one.
//
// The 89th was time.runtimeNow. Its row is not here because its body is: C1's f9baa2a778 merged at
// 17a5819956, and the rule below removed the row in the commit that reads the merged tree. It is the
// rule's first member, and C2's instrument refused its own first run against that tree for the same
// reason — both controls firing on the same fact from opposite sides.
//
// The five internal/synctest members (Run, Wait, acquire, inBubble, release) left with R's synctest
// re-entry train 0b6bf15ab0, whose S4 commit bodied them in internal/synctest/synctest_impl.cs without
// deleting their rows; the train's reported gates did not include this suite. The union assembly's converter leg
// read "vanished 5" (2026-09-25), and the rows go in a repair based on that train's tip.
//
// The four runtime/trace user-annotation members (userLog, userRegion, userTaskCreate, userTaskEnd)
// left with their bodies in runtime/trace/annotation_impl.cs (2026-09-27, beside the managed execution
// tracer): no-ops, which is Go's behaviour with tracing off and a named model limit with it on. Before
// that, a converted program calling trace.NewTask died on the first call.
//
// internal/coverage/cfile.getCovCounterList left with its body in internal/coverage/cfile/emit_impl.cs
// (2026-10-10, the Phase 5 windows/linux seat): the empty list, Go's own answer for a binary built
// without -cover. Before that, runtime/coverage.ClearCounters died on its first call on every OS.
//
// ⚠ EDIT THIS TABLE IN THE COMMIT THAT MOVES THE TREE, never afterwards. That is the whole mechanism.
var declaredPushStubs = map[string]string{
	"crypto/x509/internal/macos.syscall":                 dispositionLatent,
	"internal/runtime/maps.mapKeyError":                  dispositionLatent,
	"internal/syscall/windows.QueryPerformanceCounter":   dispositionLatent,
	"internal/syscall/windows.QueryPerformanceFrequency": dispositionLatent,
	"os.ignoreSIGSYS":                   dispositionLatent,
	"os.restoreSIGSYS":                  dispositionLatent,
	"reflect.chancap":                   dispositionLatent,
	"reflect.chanclose":                 dispositionLatent,
	"reflect.chanlen":                   dispositionLatent,
	"reflect.chanrecv":                  dispositionLatent,
	"reflect.chansend0":                 dispositionLatent,
	"reflect.growslice":                 dispositionLatent,
	"reflect.ifaceE2I":                  dispositionLatent,
	"reflect.makechan":                  dispositionLatent,
	"reflect.makemap":                   dispositionLatent,
	"reflect.mapaccess":                 dispositionLatent,
	"reflect.mapaccess_faststr":         dispositionLatent,
	"reflect.mapassign0":                dispositionLatent,
	"reflect.mapassign_faststr0":        dispositionLatent,
	"reflect.mapclear":                  dispositionLatent,
	"reflect.mapdelete":                 dispositionLatent,
	"reflect.mapdelete_faststr":         dispositionLatent,
	"reflect.maplen":                    dispositionLatent,
	"reflect.memmove":                   dispositionLatent,
	"reflect.rselect":                   dispositionLatent,
	"reflect.typedarrayclear":           dispositionLatent,
	"reflect.typedmemclr":               dispositionLatent,
	"reflect.typedmemclrpartial":        dispositionLatent,
	"reflect.typedmemmove":              dispositionLatent,
	"reflect.typedslicecopy":            dispositionLatent,
	"reflect.typehash":                  dispositionLatent,
	"reflect.unsafe_New":                dispositionLatent,
	"reflect.unsafe_NewArray":           dispositionLatent,
	"reflect.unsafeslice":               dispositionLatent,
	"reflect.verifyNotInHeapPtr":        dispositionLatent,
	"runtime.mapaccess1":                dispositionSuppliedElsewhere,
	"runtime.mapaccess1_fast32":         dispositionSuppliedElsewhere,
	"runtime.mapaccess1_fast64":         dispositionSuppliedElsewhere,
	"runtime.mapaccess1_faststr":        dispositionSuppliedElsewhere,
	"runtime.mapaccess2":                dispositionSuppliedElsewhere,
	"runtime.mapaccess2_fast32":         dispositionSuppliedElsewhere,
	"runtime.mapaccess2_fast64":         dispositionSuppliedElsewhere,
	"runtime.mapaccess2_faststr":        dispositionSuppliedElsewhere,
	"runtime.mapassign":                 dispositionSuppliedElsewhere,
	"runtime.mapassign_fast32":          dispositionSuppliedElsewhere,
	"runtime.mapassign_fast32ptr":       dispositionSuppliedElsewhere,
	"runtime.mapassign_fast64":          dispositionSuppliedElsewhere,
	"runtime.mapassign_fast64ptr":       dispositionSuppliedElsewhere,
	"runtime.mapassign_faststr":         dispositionSuppliedElsewhere,
	"runtime.mapdelete_fast32":          dispositionSuppliedElsewhere,
	"runtime.mapdelete_fast64":          dispositionSuppliedElsewhere,
	"runtime.mapdelete_faststr":         dispositionSuppliedElsewhere,
	"runtime/pprof.mach_vm_region":      dispositionLatent,
	"runtime/pprof.proc_regionfilename": dispositionLatent,
	"syscall.runtime_AfterFork":         dispositionLatent,
	"syscall.runtime_AfterForkInChild":  dispositionLatent,
	"syscall.runtime_BeforeFork":        dispositionLatent,
}

type stubEntry struct {
	file, name, pushedBy string

	// callers is the number of FIRST-ORDER call sites inside the member's OWN emitted package —
	// the reach column COORD asked the swiss-map members to be stated with (8907b68472).
	//
	// ⚠ ITS LIMITS, because a zero here is the reading most likely to be misused. It is TEXTUAL
	// (`name(` outside comments, in the package's own .cs, excluding the declaration itself) and
	// FIRST-ORDER: a member with zero callers may still be reached from another package, and a
	// member with callers may sit behind code nothing runs. It answers "does anything in this
	// package even mention it", which is the cheap half of reachability and the half that separates
	// `reflect`'s map intrinsics — which golib supplies, so the emitted reflect never calls them —
	// from a member like time.runtimeNow that every program's first time.Now() went through.
	// "Nothing reaches it" is NOT a reason to relax: it is the precise condition that makes a
	// member a booby trap, because no standing gate reddens it until something calls it.
	callers int
}

type stubCensus struct {
	declarations, packages, linkname, cgo, other, pushed int
	stubs                                                []stubEntry
}

var (
	// ⚠ THE RETURN TYPE MAY ITSELF BE PARENTHESISED. A Go function with multiple results converts
	// to a C# tuple return — `internal static partial (int64 sec, int32 nsec, int64 mono)
	// runtimeNow();` — so a pattern that forbids parentheses before the name cannot see it. An
	// earlier version of this census did exactly that and MISSED time.runtimeNow, which is the
	// member whose stub makes every time.Now() throw. The greedy `.*` consumes the return type up
	// to the LAST identifier before the parameter list, which is the method name in both shapes.
	partialDeclRe = regexp.MustCompile(`^\s*(?:internal|public|private|protected)?\s*(?:static\s+)?(?:unsafe\s+)?partial\s+.*\b([A-Za-z_@][A-Za-z0-9_@]*)\s*(?:<[^>]*>)?\s*\([^()]*\)\s*;\s*$`)
	// ⚠ AN IMPLEMENTING PART NEED NOT OPEN A BRACE AT ALL. A hand-owned companion routinely writes
	// one as an EXPRESSION body — `internal static partial void runtime_Semacquire(ж<uint32> s) =>
	// RuntimeSemaphore.Acquire(s, WaitReason.Semacquire);` — which ends in a semicolon like a
	// bodyless declaration and opens no brace, so a pattern testing only for `{` sees neither a
	// declaration nor a definition and the member reads as unimplemented. Measured: all ten of
	// sync's runtime bridges, every one of them implemented in sync/runtime_impl.cs.
	//
	// ⚠ THE BRACE MAY BE ON THE NEXT LINE. The converter emits K&R, but a hand-owned *_impl.cs
	// companion is ordinary C# and puts the opening brace on its own line — so a pattern requiring
	// `{` at the end of the signature cannot see the implementing part that companion supplies, and
	// every member it implements reads as an unimplemented stub. Measured: time.runtimeNano, which
	// time_impl.cs:57 implements, was in the population while i9's BUILD of the same package mints
	// no stub for it (c5f7b4b90d). A partial DEFINITION therefore ends in `)` with or without the
	// brace, and a partial DECLARATION ends in `);` — the two patterns are disjoint on the
	// semicolon. Checked at the tree: every partial line in the corpus that ends in neither is a
	// partial PROPERTY (`partial ref T X { get; }`, 579 of them), never a multi-line signature.
	partialDefnRe = regexp.MustCompile(`^\s*(?:internal|public|private|protected)?\s*(?:static\s+)?(?:unsafe\s+)?partial\s+.*\b([A-Za-z_@][A-Za-z0-9_@]*)\s*(?:<[^>]*>)?\s*\([^()]*\)\s*(?:\{.*|=>.*)?\s*$`)

	// ⚠ ANCHORED TO THE START OF THE COMMENT, and to the end of the line. A directive is a whole
	// comment line by Go's own rule, and an UNANCHORED pattern matches the token inside ordinary
	// PROSE: measured, `//go:linkname` written inside a sentence produced 22 phantom directives
	// whose "destination" was the next English word (`and`, `before`, `in`, `instead.`). Third
	// instance of the unanchored-identifier class in this lane, after `go.` matching inside
	// `global::go.` and `alias.` inside `valias.`.
	linknameRe = regexp.MustCompile(`^\s*//go:linkname\s+(\S+)(?:\s+(\S+))?\s*$`)

	// The two shapes that END a declaration: a body opens, or a bodyless declaration terminates.
	bodyOpensRe  = regexp.MustCompile(`\{\s*$`)
	bodylessRe   = regexp.MustCompile(`;\s*$`)
	linknameMark = regexp.MustCompile(`(?m)^//go:linkname\b`)
	cgoMark      = regexp.MustCompile(`(?m)^//go:cgo_import`)
)

// declaredNotImplementedIn walks a converted corpus and reports the partial declarations that no
// implementing part supplies, classified by the reason the body is absent.
//
// The package key folds a layout-L3 GOOS folder into its parent, because one package's flavours
// compile as ONE assembly and an implementing part may sit in either.
func declaredNotImplementedIn(coreDir string) (stubCensus, error) {
	type decl struct {
		name, file string
		reason     string
		key        string
	}

	declsByPkg := map[string][]decl{}
	filesByPkg := map[string][][]string{}
	defnsByPkg := map[string]map[string]bool{}
	pushes := map[string]string{}

	err := filepath.Walk(coreDir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		if info.IsDir() {
			switch info.Name() {
			case "bin", "obj", "Generated", ".vs":
				return filepath.SkipDir
			}
			return nil
		}

		if !strings.HasSuffix(path, ".cs") || strings.HasSuffix(path, ".cs.auto") {
			return nil
		}

		content, readErr := os.ReadFile(path)

		if readErr != nil {
			return nil
		}

		rel := filepath.ToSlash(mustRel(coreDir, path))
		dir := filepath.ToSlash(filepath.Dir(rel))
		pkg := dir

		switch filepath.Base(dir) {
		case "windows", "linux", "darwin":
			pkg = filepath.ToSlash(filepath.Dir(dir))
		}

		lines := strings.Split(strings.ReplaceAll(string(content), "\r\n", "\n"), "\n")
		filesByPkg[pkg] = append(filesByPkg[pkg], lines)

		for i, line := range lines {
			// ⚠ A TWO-ARGUMENT DIRECTIVE IS NOT A PUSH BY ITSELF. `//go:linkname local
			// path.name` reads BOTH ways: the producer naming its consumer (a PUSH, and the local
			// name carries the BODY), or the consumer naming its producer, including the
			// declaration's OWN self-naming permission marker (NOT a push, and the local name is
			// bodyless). Keying only on the destination conflates them: measured, 103 of the 477
			// two-argument directives in the corpus are bodyless, and fips140's three stubs each
			// carry a self-naming marker in the very file the census reports them from — so a
			// destination-keyed map would have answered "a push exists" naming the stub's own file.
			// The discriminator is the BODY, and a push must be PROVEN, never assumed.
			if match := linknameRe.FindStringSubmatch(line); match != nil && match[2] != "" {
				if leadsABody(lines, i) {
					if _, seen := pushes[match[2]]; !seen {
						pushes[match[2]] = rel
					}
				}
			}

			// ⚠ A DECLARATION HAS NOTHING BETWEEN `)` AND `;`. An expression-bodied
			// implementing part ends in a semicolon too — `... runtime_SemacquireRWMutex(ж<uint32>
			// s, bool lifo, nint skipframes) => RuntimeSemaphore.Acquire(s, WaitReason.X);` — and
			// the LAST parenthesised group on that line is the CALL's argument list, so the
			// declaration pattern matches it, captures the CALLEE's name (`Acquire`), and consumes
			// the line before the definition pattern is ever asked. That did two things at once:
			// it invented declarations named after whatever each companion happened to call, and
			// it hid every member those companions implement. Measured against C2's independent
			// census (5ffca1e37b), which read `sync` as fully hand-owned where this read ten
			// unimplemented bridges.
			if match := partialDeclRe.FindStringSubmatch(line); match != nil && !strings.Contains(line, "=>") {
				// ⚠ The window is the declaration's OWN contiguous comment block, not a fixed
				// number of lines back. A fixed window is arbitrary in both directions: too short
				// and a long doc comment hides the marker, too long and it reads the PREVIOUS
				// declaration's. Measured: an 8-line window and a 7-line window disagreed by three
				// on the linkname class, which is the finding's headline number.
				comments, attributes := leadingTrivia(lines, i)

				// The generator's own two exclusions. ⚠ The attribute test must read the
				// ATTRIBUTE lines, not the comment block: [LibraryImport] is trivia of a
				// different kind, and a comment-only window silently stops excluding it —
				// measured, the population read 775 instead of 745 the moment the window
				// narrowed to comments.
				if strings.Contains(attributes, "LibraryImport") || match[1] == PackageTestInitHookMethod {
					continue
				}

				reason := "other"

				// Anchored for the same reason the directive scan is: a prose mention of
				// `//go:linkname` in a doc comment is not a directive.
				switch {
				case linknameMark.MatchString(comments):
					reason = "linkname"
				case cgoMark.MatchString(comments):
					reason = "cgo"
				}

				declsByPkg[pkg] = append(declsByPkg[pkg], decl{
					name:   match[1],
					file:   rel,
					reason: reason,
					key:    pkg + "." + match[1],
				})

				continue
			}

			if match := partialDefnRe.FindStringSubmatch(line); match != nil {
				if defnsByPkg[pkg] == nil {
					defnsByPkg[pkg] = map[string]bool{}
				}
				defnsByPkg[pkg][match[1]] = true
			}
		}

		return nil
	})

	callSites := map[string]int{}

	for pkg, files := range filesByPkg {
		for _, lines := range files {
			for _, line := range lines {
				trimmed := strings.TrimSpace(line)

				if strings.HasPrefix(trimmed, "//") {
					continue
				}

				for _, d := range declsByPkg[pkg] {
					if strings.Contains(line, d.name+"(") && !partialDeclRe.MatchString(line) {
						callSites[pkg+"."+d.name]++
					}
				}
			}
		}
	}

	census := stubCensus{packages: len(declsByPkg)}

	if err != nil {
		return census, err
	}

	for pkg, decls := range declsByPkg {
		for _, d := range decls {
			census.declarations++

			if defnsByPkg[pkg][d.name] {
				continue
			}

			entry := stubEntry{file: d.file, name: d.name, callers: callSites[pkg+"."+d.name]}

			// ⚠ THE PUSH COLUMN IS INDEPENDENT OF THE CONSUMER'S OWN MARKER. A //go:linkname push
			// is written by the PRODUCER; Go does not require the consumer to carry a directive at
			// all, and the bare consumer — a plain bodyless func with no marker of its own — is the
			// MAJORITY shape (C2 measured 193 of 284 push sites at 1.24.13, 5ffca1e37b). Asking
			// this only of declarations whose own comment carries a directive is asking the
			// consumer a question only the producer can answer: it hid every one of reflect's,
			// whose declarations (`static partial Pointer makemap(...)`) carry no marker while
			// runtime/*.cs pushes to them by name. The REASON below is the consumer's own account
			// of why its body is absent; the PUSH is a fact about the corpus, and they are read
			// separately.
			if by, ok := pushes[d.key]; ok {
				census.pushed++
				entry.pushedBy = by
			}

			switch d.reason {
			case "linkname":
				census.linkname++
			case "cgo":
				census.cgo++
			default:
				census.other++
			}

			census.stubs = append(census.stubs, entry)
		}
	}

	sort.Slice(census.stubs, func(i, j int) bool {
		if census.stubs[i].file != census.stubs[j].file {
			return census.stubs[i].file < census.stubs[j].file
		}
		return census.stubs[i].name < census.stubs[j].name
	})

	return census, nil
}

// leadsABody reports whether the declaration introduced at line i is followed by a BODY rather
// than by a bodyless declaration — the discriminator between a //go:linkname PUSH (the producer
// naming its consumer; the local name holds the implementation) and every other two-argument use
// of the same directive (a consumer naming its producer, or a declaration's own self-naming
// permission marker; the local name is bodyless).
//
// It skips the directive's own remaining trivia, then reads forward to whichever terminator comes
// first: `{` opens a body, `;` ends a bodyless declaration. A multi-line parameter list is why this
// scans rather than testing the first code line alone. Anything else — and anything it cannot
// reach within the window — is NOT a push: the census proves a push or does not claim one.
//
// ⚠ LIMIT, stated because a zero here would otherwise read as a fact: this sees a push whose local
// name is a FUNCTION. A //go:linkname on a VARIABLE (runtime's _cgo_* handles) ends in `;` like a
// bodyless declaration and is counted as no push. That costs this census nothing — its population
// is bodyless partial METHODS, which no data push can supply — but a later census asking about data
// linknames cannot reuse this predicate.
func leadsABody(lines []string, i int) bool {
	for j := i + 1; j < len(lines) && j <= i+24; j++ {
		trimmed := strings.TrimSpace(lines[j])

		if trimmed == "" || strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "[") {
			continue
		}

		if bodyOpensRe.MatchString(trimmed) {
			return true
		}

		if bodylessRe.MatchString(trimmed) {
			return false
		}
	}

	return false
}

// leadingTrivia returns the declaration's own leading COMMENT lines and ATTRIBUTE lines as two
// strings, walking back over the contiguous run of both immediately above index i.
//
// They are kept apart because they answer different questions: a //go:linkname or //go:cgo_import
// marker says WHY a body is absent, while a [LibraryImport] attribute says another generator owes
// the body and the stub generator must decline. A single window conflates them, and a window sized
// in LINES is arbitrary in both directions — too short and a long doc comment hides the marker, too
// long and it reads the previous declaration's.
func leadingTrivia(lines []string, i int) (comments, attributes string) {
	var commentLines, attributeLines []string

	for j := i - 1; j >= 0; j-- {
		trimmed := strings.TrimSpace(lines[j])

		switch {
		case strings.HasPrefix(trimmed, "//"):
			commentLines = append(commentLines, trimmed)
		case strings.HasPrefix(trimmed, "["):
			attributeLines = append(attributeLines, trimmed)
		default:
			return strings.Join(commentLines, "\n"), strings.Join(attributeLines, "\n")
		}
	}

	return strings.Join(commentLines, "\n"), strings.Join(attributeLines, "\n")
}

func mustRel(base, path string) string {
	rel, err := filepath.Rel(base, path)

	if err != nil {
		return path
	}

	return rel
}
