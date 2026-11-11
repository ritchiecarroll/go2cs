// declaredNotImplementedFlavour_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"sort"
	"strings"
	"sync"
	"testing"
)

// The PER-FLAVOUR view of the declared-not-implemented census (COORD, 2026-11-10; the instrument is
// i9's scratch from the linux/arm64 stub triage, DATA-la64-s3-census.md "Amendment, 2026-11-10").
//
// TestDeclaredNotImplementedCensus POOLS a package's layout key folders: a member counts as
// implemented when an implementing part sits in ANY of them. That is the right question for "does
// the corpus hold a body at all" and the wrong one for "does THIS target's build reach it", because
// one target compiles the flat files plus only the key folders that serve it. Two things follow that
// the pooled view cannot show:
//
//   - a member stubbed at ONE target and bodied or absent at the others reads, pooled, exactly like
//     the hundreds of assembly stubs every target shares. linux/arm64 arrived with nine of those.
//   - a member whose only body sits in another target's key folder is not in the pooled census AT
//     ALL, and still throws at every target that folder does not serve. This is the MASKED class.
//     Measured on the day this guard was written: two members, neither known before it. Both were
//     classic partials the generator should never have stubbed, and left the class the same day
//     (declaredErasedByDesign below); the class and its gate stay for the next one.
//
// So this test reads the SAME walk one target at a time and keeps a ledger of every member the
// targets DISAGREE about. Like declaredPushStubs it is measured == declared, in both directions: a
// new flavour-only stub arrives as a failing gate naming the member and its targets, not as a throw
// on one leg of a validation sweep.
//
// ⚠ A ROW IS NOT A LICENCE. It records that somebody looked and what they found. A member that is
// reached at its target must be bodied, not rowed.
func TestFlavourOnlyStubCensus(t *testing.T) {
	census, err := corpusStubCensus()

	if err != nil {
		t.Fatalf("walking ../core: %v", err)
	}

	// ⚠ ANTI-VACUITY, arm 1: the walk must hand this view its records. The pooled test checks its own
	// counters; these two are separate fields and an edit to the walk can empty them alone.
	if len(census.parts) == 0 || len(census.bodies) == 0 {
		t.Fatalf("VACUOUS: the walk recorded %d declarations and %d implemented names with their key "+
			"folders; the per-flavour view is measuring nothing", len(census.parts), len(census.bodies))
	}

	// ⚠ THE CONTROL: with every key folder served, this view IS the pooled census, member for member.
	// It ties the folder records to the path the pooled test has always read, so a per-target number
	// below cannot drift from the pooled one through a fault in the records themselves.
	pooled := map[string]bool{}

	for _, entry := range census.stubs {
		pooled[entry.file+":"+entry.name] = true
	}

	everyFolder := map[string]bool{}

	for _, part := range census.stubbedAt("") {
		everyFolder[part.file+":"+part.name] = true
	}

	for _, key := range sortedMembers(everyFolder) {
		if !pooled[key] {
			t.Errorf("CONTROL: %s is a stub with every key folder served and is NOT in the pooled census", key)
		}
	}

	for _, key := range sortedMembers(pooled) {
		if !everyFolder[key] {
			t.Errorf("CONTROL: %s is in the pooled census and is NOT a stub with every key folder served", key)
		}
	}

	// The per-target reading. A member is keyed as declaredPushStubs keys it, (package, symbol) with
	// the key folder folded away, and carries the targets whose build mints a stub for it.
	type reading struct {
		targets []string
		masked  bool
	}

	measured := map[string]*reading{}
	reached := map[string]bool{}
	declared := map[string]bool{}
	var counts []string

	for _, part := range census.parts {
		declared[memberKey(part.file, part.name)] = true
	}

	for _, target := range platformUniverse {
		stubs := census.stubbedAt(target)

		// ⚠ ANTI-VACUITY, arm 2: every target has assembly-backed stubs today. A target that reads
		// zero is a folder predicate that stopped matching, not a clean flavour.
		if len(stubs) == 0 {
			t.Errorf("VACUOUS: 0 stubs at %s; the folder predicate serves this target nothing", target)
		}

		seen := map[string]bool{}

		for _, part := range stubs {
			reached[part.file+":"+part.name] = true
			member := memberKey(part.file, part.name)

			if seen[member] {
				continue
			}

			seen[member] = true
			entry := measured[member]

			if entry == nil {
				entry = &reading{}
				measured[member] = entry
			}

			entry.targets = append(entry.targets, target)

			// MASKED: a body exists somewhere in the package, so the pooled census does not list the
			// member, and this target's build still does not reach that body.
			if len(census.bodies[part.pkg+"."+part.name]) > 0 {
				entry.masked = true
			}
		}

		counts = append(counts, target+" "+itoa(len(stubs)))
	}

	// Every pooled stub must be a stub at SOME target. One that is not sits in a key folder no target
	// of platformUniverse compiles, and the per-target counts then no longer account for the pool.
	for _, key := range sortedMembers(pooled) {
		if !reached[key] {
			t.Errorf("POOLED STUB AT NO TARGET: %s\n\tits key folder serves no target in "+
				"platformUniverse, so no per-target view can see it", key)
		}
	}

	// ⚠ ANTI-VACUITY, arm 3: one positive and two negative controls, by name.
	//   + runtime.load_g is declared in runtime/linux-arm64 alone ("called from assembly only") and
	//     has no body anywhere: stubbed at linux/arm64 and at no other target.
	//   - math.archExp2 and hash/crc32.castagnoliUpdate are declared in linux-arm64 and BODIED there by
	//     hand-owned companions (site 9 and i9's triage): declared, and a stub at no target. They are
	//     the two members whose arm64 stubs reached a sweep before anything per-flavour stood.
	if entry := measured["runtime.load_g"]; entry == nil || strings.Join(entry.targets, ",") != atLinuxArm64 {
		t.Errorf("POSITIVE CONTROL: runtime.load_g must read as stubbed at %s alone; read %v", atLinuxArm64, entry)
	}

	for _, member := range []string{"math.archExp2", "hash/crc32.castagnoliUpdate"} {
		if !declared[member] {
			t.Errorf("NEGATIVE CONTROL: %s is no longer a partial declaration; pick another bodied "+
				"flavour-only member, do not delete the control", member)
		} else if entry := measured[member]; entry != nil {
			t.Errorf("NEGATIVE CONTROL: %s is bodied in its own key folder and reads as a stub at %s",
				member, strings.Join(entry.targets, ","))
		}
	}

	// ⚠ THE GATE. In scope: every member the targets disagree about (stubbed at some and not all),
	// and every masked member whatever its targets. Out of scope, by rule and counted:
	//   - a member stubbed at EVERY target and not masked: the pooled census's subject.
	//   - a member with a declaredPushStubs row: it already carries a disposition there.
	//   - darwin's libc trampolines (`*_trampoline`, stubbed at darwin targets only): one class of a
	//     few hundred members that are passed by address into the syscall shim and never called as
	//     functions. A trampoline stubbed anywhere else is in scope again.
	inScope := map[string]*reading{}
	var shared, pushed, trampolines int

	for member, entry := range measured {
		switch {
		case entry.masked:
			inScope[member] = entry
		case len(entry.targets) == len(platformUniverse):
			shared++
		case len(declaredPushStubs[member]) > 0:
			pushed++
		case strings.HasSuffix(member, "_trampoline") && darwinOnly(entry.targets):
			trampolines++
		default:
			inScope[member] = entry
		}
	}

	for _, member := range sortedMembers(inScope) {
		entry := inScope[member]
		targets := strings.Join(entry.targets, ",")
		row, rowed := declaredFlavourOnlyStubs[member]

		switch {
		case !rowed && entry.masked:
			t.Errorf("UNDECLARED MASKED STUB: %s at %s\n"+
				"\tan implementing part exists (key folders %q, \"\" is flat) and the build of each "+
				"target named does not compile it, so that target mints a THROWING stub while the "+
				"pooled census reads the member as implemented. Move or add the body so every target "+
				"that serves the declaration reaches one; a row with flavourMasked is for a member "+
				"that has been read and left.", member, targets, census.bodies[member])
		case !rowed:
			t.Errorf("UNDECLARED FLAVOUR-ONLY STUB: %s at %s\n"+
				"\tthis member is a throwing stub in the build of the targets named and not in the "+
				"others, and no row in declaredFlavourOnlyStubs says so. Either a flavour arrived "+
				"with a bodiless declaration of its own, or one target lost its implementing part. "+
				"It compiles either way and dies at the first call on that target only. Body it, or "+
				"add the row WITH its target set and disposition in the commit that moved the tree.",
				member, targets)
		case row.targets != targets:
			t.Errorf("TARGET SET MOVED: %s is declared stubbed at %s and measured at %s\n"+
				"\tread why before editing the row: a target gaining a stub is the case this guard "+
				"exists for.", member, row.targets, targets)
		case entry.masked != (row.disposition == flavourMasked):
			t.Errorf("MASKED DISAGREES: %s measures masked=%v and its row says %q\n"+
				"\tflavourMasked is a measured property (a body exists that the target does not "+
				"compile), not a label.", member, entry.masked, row.disposition)
		}
	}

	for _, member := range sortedMembers(declaredFlavourOnlyStubs) {
		if inScope[member] != nil {
			continue
		}

		state := "it is a stub at no target now"

		if entry := measured[member]; entry != nil {
			state = "it is a stub at " + strings.Join(entry.targets, ",") + " now, which the scope rule leaves out"
		}

		t.Errorf("DECLARED BUT NOT MEASURED: %s (%s)\n"+
			"\t%s. If it was bodied, delete the row in the same commit; a row for a member the "+
			"targets no longer disagree about hides the next one.",
			member, declaredFlavourOnlyStubs[member].disposition, state)
	}

	t.Logf("per-target stubs: %s · pooled %d", strings.Join(counts, " · "), len(census.stubs))
	t.Logf("members: %d stubbed at every target · %d the targets disagree about or masked, of which "+
		"%d rowed here (declared %d) · %d rowed in declaredPushStubs · %d darwin trampolines",
		shared, len(inScope)+pushed+trampolines, len(inScope), len(declaredFlavourOnlyStubs), pushed, trampolines)
}

// flavourDecl is one partial declaration as the census walk saw it: its package (key folder folded
// away), name, file and the layout key folder the file sits in ("" is flat).
type flavourDecl struct {
	pkg, name, file, folder string
}

// corpusStubCensus is the one walk of src/core both census tests read.
var corpusStubCensus = sync.OnceValues(func() (stubCensus, error) {
	return declaredNotImplementedIn("../core")
})

// stubbedAt reports the declarations that are throwing stubs in ONE target's build: those the target
// compiles (flat, or in a key folder that serves it) for which no implementing part sits in a file the
// same target compiles. An empty target serves every folder, which is the pooled census.
func (census stubCensus) stubbedAt(target string) []flavourDecl {
	return census.unimplementedAt(census.parts, target)
}

// erasedAt reports the CLASSIC partial declarations that one target's build compiles and no part it
// compiles implements. C# erases each of those there, together with every call to it.
func (census stubCensus) erasedAt(target string) []flavourDecl {
	return census.unimplementedAt(census.erased, target)
}

// unimplementedAt is the one reading stubbedAt and erasedAt share: of the declarations given, those
// the target compiles for which no implementing part sits in a file the same target compiles.
func (census stubCensus) unimplementedAt(parts []flavourDecl, target string) []flavourDecl {
	serves := func(folder string) bool {
		return len(folder) == 0 || len(target) == 0 || layoutFolderServes(folder, target)
	}

	var stubs []flavourDecl

	for _, part := range parts {
		if !serves(part.folder) {
			continue
		}

		bodied := false

		for _, folder := range census.bodies[part.pkg+"."+part.name] {
			if serves(folder) {
				bodied = true
				break
			}
		}

		if !bodied {
			stubs = append(stubs, part)
		}
	}

	sort.Slice(stubs, func(i, j int) bool {
		if stubs[i].file != stubs[j].file {
			return stubs[i].file < stubs[j].file
		}

		return stubs[i].name < stubs[j].name
	})

	return stubs
}

func darwinOnly(targets []string) bool {
	for _, target := range targets {
		if !strings.HasPrefix(target, "darwin/") {
			return false
		}
	}

	return len(targets) > 0
}

func sortedMembers[V any](set map[string]V) []string {
	keys := make([]string, 0, len(set))

	for key := range set {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	return keys
}

// The target sets a row can name, spelled in platformUniverse order because that is the order the
// measurement joins them in.
const (
	atLinuxArm64        = "linux/arm64"
	atLinux             = "linux/amd64,linux/arm64"
	atAmd64             = "windows/amd64,linux/amd64,darwin/amd64"
	atWindows           = "windows/amd64"
	atDarwin            = "darwin/amd64"
	atUnix              = "linux/amd64,darwin/amd64,linux/arm64"
	atWindowsLinuxAmd64 = "windows/amd64,linux/amd64"
)

// The dispositions a row can carry.
const (
	// i9's class (b): Go itself declares the function for go vet and calls it from assembly only.
	// There is no Go caller, and no assembly is converted.
	// The floor-set triage's class (b) (docs/phase4/DATA-la64-s7-bank-plan.md §11.3) is the same class,
	// and it counts one more case in: Go names the member only to take its address. An address take
	// does not run the stub.
	flavourGoSide = "Go-side: called from assembly only"

	// i9's class (c): read, and deliberately left a throwing stub because linux/arm64 never reaches it
	// (DATA-la64-s3-census.md, triage rows 9 to 15, each with its reach).
	// The floor-set triage's class (c) (the same §11.3, each row with its reach): Go calls the member,
	// and this emission does not reach the call at the row's targets. The reach was read, not run, and
	// the guard does not see it: a change that gives one of those callers a root makes the label wrong
	// with nothing red.
	flavourRefused = "refused: read and left; this target does not reach it"

	// Read one by one and not settled: the reading does not exclude a reach at the row's targets
	// (DATA-la64-s7-bank-plan.md §11.2). The member is owed a body or a proof that nothing reaches it.
	flavourOpen = "open: read; a reach at these targets is not excluded"

	// A body exists in a key folder the row's targets do not compile. The pooled census cannot see
	// the member at all. Measured, never asserted: the gate refuses the label on a member that has no
	// body and refuses any other label on one that has.
	flavourMasked = "MASKED: bodied in another key folder; a throwing stub at these targets"

	// Stubbed at these targets before this guard stood and not yet read one by one. The honest label
	// for the inherited population: it says nobody has looked, where a row with a reason says somebody
	// has. New rows do not take it.
	flavourUntriaged = "not triaged: stubbed before this guard (2026-11-10)"
)

type flavourRow struct {
	targets, disposition string
}

// declaredFlavourOnlyStubs is the DECLARED set: every member the targets disagree about (a throwing
// stub in some targets' builds and not in all), plus every masked member, less the two classes the
// gate exempts by rule. Measured at the batch 284 union (b328dd71e7): pooled 495; per target
// windows/amd64 213, linux/amd64 231, darwin/amd64 427, linux/arm64 208. Each target reads one fewer
// (212, 230, 426, 207) since the two masked members stopped being stubs.
//
// 2026-11-11: runtime.sched_getaffinity, runtime.exit and runtime.osyield were bodied at linux
// (runtime/linux/stubs2_impl.cs) and their three rows left. Measured after, with master 742a3dbb24
// merged in: pooled 492; windows/amd64 212, linux/amd64 227, darwin/amd64 426, linux/arm64 204.
//
// 2026-11-11: the rows in the nine floor-set packages took the dispositions their triage settled
// (docs/phase4/DATA-la64-s7-bank-plan.md §11.3, where each row has its reach): 38 Go-side and 26
// refused. runtime.time_now and runtime.getlasterror were read and are open (§11.2). The 7 rows still
// "not triaged" are the ones outside the floor set, the post-hop reading list. A disposition is a
// reading: the guard measures the member and its targets, and checks no label but MASKED.
//
// No row is MASKED today. The two this guard found on its first day (internal/cpu.osInitArm64 and
// internal/poll.prepayPlatformDeadlinePath) were classic partials, stubbed only because
// PartialStubGenerator excluded one classic partial by name where the rule is the shape. They are
// rows of declaredErasedByDesign now.
var declaredFlavourOnlyStubs = map[string]flavourRow{
	// Stubbed at linux/arm64 alone: i9's triage rows 7 to 15.
	"internal/cpu.getisar0":                {atLinuxArm64, flavourRefused},
	"internal/cpu.getpfr0":                 {atLinuxArm64, flavourRefused},
	"runtime.emptyfunc":                    {atLinuxArm64, flavourRefused},
	"runtime.load_g":                       {atLinuxArm64, flavourGoSide},
	"runtime.save_g":                       {atLinuxArm64, flavourGoSide},
	"vendor/golang.org/x/sys/cpu.getisar0": {atLinuxArm64, flavourRefused},
	"vendor/golang.org/x/sys/cpu.getisar1": {atLinuxArm64, flavourRefused},
	"vendor/golang.org/x/sys/cpu.getpfr0":  {atLinuxArm64, flavourRefused},
	"vendor/golang.org/x/sys/cpu.getzfr0":  {atLinuxArm64, flavourRefused},

	// Stubbed at both linux targets.
	"runtime.access":               {atLinux, flavourRefused},
	"runtime.callCgoMmap":          {atLinux, flavourRefused},
	"runtime.callCgoMunmap":        {atLinux, flavourRefused},
	"runtime.callCgoSigaction":     {atLinux, flavourRefused},
	"runtime.connect":              {atLinux, flavourRefused},
	"runtime.exitThread":           {atLinux, flavourRefused},
	"runtime.futex":                {atLinux, flavourRefused},
	"runtime.gettid":               {atLinux, flavourRefused},
	"runtime.raise":                {atLinux, flavourRefused},
	"runtime.raiseproc":            {atLinux, flavourRefused},
	"runtime.rt_sigaction":         {atLinux, flavourRefused},
	"runtime.sbrk0":                {atLinux, flavourRefused},
	"runtime.setitimer":            {atLinux, flavourRefused},
	"runtime.sigaltstack":          {atLinux, flavourRefused},
	"runtime.sigreturn__sigaction": {atLinux, flavourGoSide},
	"runtime.socket":               {atLinux, flavourRefused},
	"runtime.timer_create":         {atLinux, flavourRefused},
	"runtime.timer_delete":         {atLinux, flavourRefused},
	"runtime.timer_settime":        {atLinux, flavourRefused},
	"runtime.vgetrandom1":          {atLinux, flavourRefused},
	"syscall.rawVforkSyscall":      {atLinux, flavourRefused},

	// Stubbed at every amd64 target and not at linux/arm64.
	"internal/cpu.xgetbv":                           {atAmd64, flavourRefused},
	"internal/runtime/gc/scan.ExpandAVX512":         {atAmd64, flavourUntriaged},
	"internal/runtime/gc/scan.FilterNilAVX512":      {atAmd64, flavourUntriaged},
	"internal/runtime/gc/scan.scanSpanPackedAVX512": {atAmd64, flavourUntriaged},
	"runtime.asmcgocall_landingpad":                 {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierBP":                      {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierBX":                      {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierCX":                      {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierDX":                      {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierR8":                      {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierR9":                      {atAmd64, flavourGoSide},
	"runtime.gcWriteBarrierSI":                      {atAmd64, flavourGoSide},
	"runtime.retpolineAX":                           {atAmd64, flavourGoSide},
	"runtime.retpolineBP":                           {atAmd64, flavourGoSide},
	"runtime.retpolineBX":                           {atAmd64, flavourGoSide},
	"runtime.retpolineCX":                           {atAmd64, flavourGoSide},
	"runtime.retpolineDI":                           {atAmd64, flavourGoSide},
	"runtime.retpolineDX":                           {atAmd64, flavourGoSide},
	"runtime.retpolineR10":                          {atAmd64, flavourGoSide},
	"runtime.retpolineR11":                          {atAmd64, flavourGoSide},
	"runtime.retpolineR12":                          {atAmd64, flavourGoSide},
	"runtime.retpolineR13":                          {atAmd64, flavourGoSide},
	"runtime.retpolineR14":                          {atAmd64, flavourGoSide},
	"runtime.retpolineR15":                          {atAmd64, flavourGoSide},
	"runtime.retpolineR8":                           {atAmd64, flavourGoSide},
	"runtime.retpolineR9":                           {atAmd64, flavourGoSide},
	"runtime.retpolineSI":                           {atAmd64, flavourGoSide},
	"runtime.settls":                                {atAmd64, flavourGoSide},
	"runtime.stackcheck":                            {atAmd64, flavourRefused},
	"vendor/golang.org/x/sys/cpu.cpuid":             {atAmd64, flavourUntriaged},
	"vendor/golang.org/x/sys/cpu.xgetbv":            {atAmd64, flavourUntriaged},

	// Stubbed at windows/amd64 alone.
	"internal/runtime/syscall/windows.StdCall":    {atWindows, flavourUntriaged},
	"internal/runtime/syscall/windows.asmstdcall": {atWindows, flavourUntriaged},
	"runtime.callbackasm":                         {atWindows, flavourGoSide},
	"runtime.callbackasm1":                        {atWindows, flavourGoSide},
	"runtime.exceptiontramp":                      {atWindows, flavourGoSide},
	"runtime.firstcontinuetramp":                  {atWindows, flavourGoSide},
	"runtime.getlasterror":                        {atWindows, flavourOpen},
	"runtime.lastcontinuetramp":                   {atWindows, flavourGoSide},
	"runtime.osSetupTLS":                          {atWindows, flavourRefused},
	"runtime.sehtramp":                            {atWindows, flavourGoSide},
	"runtime.sigFetchGSafe":                       {atWindows, flavourRefused},
	"runtime.sigresume":                           {atWindows, flavourGoSide},
	"runtime.tstart_stdcall":                      {atWindows, flavourGoSide},
	"runtime.wintls":                              {atWindows, flavourGoSide},

	// Stubbed at darwin/amd64 alone (the trampolines are exempt by rule).
	"os.readdir_r":         {atDarwin, flavourRefused},
	"runtime.mstart_stub":  {atDarwin, flavourGoSide},
	"runtime.syscall_x509": {atDarwin, flavourGoSide},

	// Stubbed at every target but windows/amd64.
	"runtime.cgoSigtramp": {atUnix, flavourGoSide},
	"runtime.sigfwd":      {atUnix, flavourRefused},
	"runtime.sigtramp":    {atUnix, flavourGoSide},

	// Stubbed at windows/amd64 and linux/amd64.
	"runtime.time_now": {atWindowsLinuxAmd64, flavourOpen},
}

// TestErasedByDesignCensus is the ledger of calls that compile to NOTHING (COORD's ruling A1,
// 2026-11-10): every classic partial method, at every target whose build compiles the declaration
// and no implementing part.
//
// A classic partial (isClassicPartialDecl) is the one shape C# erases when nothing implements it, and
// PartialStubGenerator leaves it to the compiler. So nothing throws at those targets, and nothing
// runs either: the call site is gone from the assembly. That is what its author asked for, and it is
// still a difference between targets that no build log and no test states. The ledger states it,
// measured == declared in both directions, with the reason somebody gave for accepting the silence.
//
// ⚠ A ROW IS A DECISION THAT SILENCE IS CORRECT AT THOSE TARGETS. A hook whose absence would change
// behaviour there is not a row; it is a missing implementing part.
func TestErasedByDesignCensus(t *testing.T) {
	census, err := corpusStubCensus()

	if err != nil {
		t.Fatalf("walking ../core: %v", err)
	}

	measured := map[string][]string{}

	for _, target := range platformUniverse {
		seen := map[string]bool{}

		for _, part := range census.erasedAt(target) {
			member := memberKey(part.file, part.name)

			if !seen[member] {
				seen[member] = true
				measured[member] = append(measured[member], target)
			}
		}
	}

	// ⚠ THE CONTROLS, by name. The classic rule is three conditions, and the walk applies it to every
	// bodiless partial in the corpus: read too wide, it would take real stubs out of both stub
	// censuses without a sound.
	//   + internal/poll.prepayPlatformDeadlinePath is `static partial void` and must read classic.
	//   - internal/poll.runtime_Semacquire is `internal static partial void` in the same package: void,
	//     no out parameter, and NOT classic, because it carries an accessibility keyword.
	classic := map[string]bool{}
	parts := map[string]bool{}

	for _, part := range census.erased {
		classic[memberKey(part.file, part.name)] = true
	}

	for _, part := range census.parts {
		parts[memberKey(part.file, part.name)] = true
	}

	if !classic["internal/poll.prepayPlatformDeadlinePath"] || parts["internal/poll.prepayPlatformDeadlinePath"] {
		t.Errorf("POSITIVE CONTROL: internal/poll.prepayPlatformDeadlinePath must read as a classic partial " +
			"and as nothing else; pick another classic member if it was removed, do not delete the control")
	}

	if classic["internal/poll.runtime_Semacquire"] || !parts["internal/poll.runtime_Semacquire"] {
		t.Errorf("NEGATIVE CONTROL: internal/poll.runtime_Semacquire carries an accessibility keyword and " +
			"must read as an ordinary partial declaration, never as a classic one")
	}

	for _, member := range sortedMembers(measured) {
		targets := strings.Join(measured[member], ",")
		row, rowed := declaredErasedByDesign[member]

		switch {
		case !rowed:
			t.Errorf("UNDECLARED ERASED CALL: %s at %s\n"+
				"\ta classic partial (no accessibility keyword, void, no out parameter) that the build "+
				"of each target named compiles with no implementing part, so every call to it compiles "+
				"to nothing there. If the hook is optional at those targets, add the row to "+
				"declaredErasedByDesign WITH its target set and the reason silence is correct; if it "+
				"is not, those targets are missing a body.", member, targets)
		case row.targets != targets:
			t.Errorf("TARGET SET MOVED: %s is declared erased at %s and measured at %s\n"+
				"\tread why before editing the row: a target that lost its implementing part now "+
				"skips the call without a word.", member, row.targets, targets)
		case len(strings.TrimSpace(row.reason)) == 0:
			t.Errorf("NO REASON: %s is rowed as erased by design at %s and the row does not say why "+
				"silence is correct there", member, targets)
		}
	}

	for _, member := range sortedMembers(declaredErasedByDesign) {
		if measured[member] == nil {
			t.Errorf("DECLARED BUT NOT MEASURED: %s\n"+
				"\tno target erases this call now: it gained an implementing part everywhere, stopped "+
				"being a classic partial, or was removed. Delete the row in the same commit.", member)
		}
	}

	t.Logf("classic partial declarations %d · erased at some target %d (declared %d)",
		len(census.erased), len(measured), len(declaredErasedByDesign))
}

// TestIsClassicPartialDecl pins the classic rule one condition at a time, on lines of the shapes the
// corpus holds. Each line is first shown to be a declaration the walk sees at all.
func TestIsClassicPartialDecl(t *testing.T) {
	cases := []struct {
		line    string
		classic bool
	}{
		{"    static partial void osInitArm64();", true},
		{"static partial void prepayPlatformDeadlinePath();", true},
		{"    partial void hook(int count, ref int total);", true},
		{"    static unsafe partial void hook(byte* buffer);", true},
		{"    static partial void layout(int about);", true},

		// An accessibility keyword: C# demands an implementing part, so the generator stubs it.
		{"internal static partial void runtime_Semacquire(ж<uint32> sema);", false},
		{"    private static partial void hook();", false},
		{"public static partial void hook();", false},
		{"    protected partial void hook();", false},

		// Not void.
		{"    static partial int64 runtimeNano();", false},
		{"    static unsafe partial void* hook();", false},
		{"    static partial (uintptr, nint) hook(uintptr fd);", false},

		// An out parameter.
		{"    static partial void hook(out int value);", false},
		{"    static partial void hook(int count, out int value);", false},
	}

	for _, tc := range cases {
		if !partialDeclRe.MatchString(tc.line) {
			t.Errorf("%q is not a partial declaration to the walk, so the case tests nothing", tc.line)
			continue
		}

		if got := isClassicPartialDecl(tc.line); got != tc.classic {
			t.Errorf("isClassicPartialDecl(%q) = %v, want %v", tc.line, got, tc.classic)
		}
	}
}

type erasedRow struct {
	targets, reason string
}

// declaredErasedByDesign is the DECLARED set of TestErasedByDesignCensus: each classic partial that
// some target leaves unimplemented, the targets where its calls compile to nothing, and why that is
// correct there. Until 2026-11-10 both members were throwing stubs at these targets, rowed MASKED in
// declaredFlavourOnlyStubs.
var declaredErasedByDesign = map[string]erasedRow{
	// Declared and called in amd64/cpu_arm64_process_impl.cs, from a module initializer and only
	// when the process is arm64; bodied under darwin/ and linux-amd64/.
	"internal/cpu.osInitArm64": {atWindows, "an arm64 windows process skips OS feature detection " +
		"silently and runs the generic paths: correct behaviour, only slower, and latent (no shipped " +
		"RID runs an arm64 windows process)"},

	// Declared flat in deadline_prepay_impl.cs; bodied under windows/.
	"internal/poll.prepayPlatformDeadlinePath": {atUnix, "the part of the deadline path only one " +
		"flavour has: only windows implements it, and elsewhere the shared part is all there is to prepay"},
}
