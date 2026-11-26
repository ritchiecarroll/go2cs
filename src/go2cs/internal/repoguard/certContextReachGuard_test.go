// certContextReachGuard_test.go - Gbtc
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

// WHY A REACH GUARD AND NOT A COMPANION. q86's census (nativeBoundaryBoxDeref_test.go) declares two
// native-boundary fork (ii) sites, both producing a ж<CertContext> over an address crypt32 returned.
// The remedy for that class is a blittable mirror transcribed on arrival -- and F3 was ordered to
// write it, until the companion it would have extended turned out to rule on the question already:
//
//	"a context this file did not build -- CertCreateCertificateContext's, a plain native box nothing
//	 reads through -- keeps working unchanged and needs no hand-own"
//	     -- syscall/windows/zsyscall_windows_certchain_impl.cs:94, restated at :288
//
// C1 measured the reach rather than taking the header's word for it, and the header is right. The
// defect in fork (ii) is a WRONG FIELD OFFSET; if no field is ever read through the box, it cannot
// fire. Both consumers of the one member that HAS callers are themselves hand-owned and hand the
// pointer straight back to crypt32 through nativeIdentityOf, whose documented fallback answers a
// native box with its own address -- which is the address crypt32 gave. These two sites are that
// fallback's only live case, and the companion cites them as the evidence its pointer model works.
//
// So COORD ruled (b) at the reading (mailbox b21442c1c -> 94b1c223a): DISCLOSE the two sites as
// INERT and cut no remedy, because a remedy for a defect that cannot fire -- bought by reversing a
// documented design decision without its author and deleting the proof that the design works -- is
// the opposite of the durable path. The durable closure of the CLASS is the converter's: the ж-box
// arc's native box kind for a reference-bearing pointee at a native boundary, post-hop, which
// retires this disclosure BY CONSTRUCTION.
//
// A disclosure is only honest while it is still true, and what makes it true is a property of the
// CALLERS, not of the wrapper: Go's own root_windows.go could grow a field read on any hop. So the
// disclosure is re-measured on every run by this guard, which CAN go red:
//
//	A FIELD READ through either box is RED whatever the row's status. A disclosed row can never
//	quietly cover a new hazard at the same site, because the guard classifies every use site and a
//	read is the defect regardless of what the row says.
//
//	A NEW USE SITE of any kind is RED as UNDECLARED. A human reads it: an address-only handoff is
//	re-stamped deliberately, a read moves the row to HAZARD and the census refuses until a companion
//	lands.
//
// WHAT IS DECLARED, AND WHY BY KIND RATHER THAN BY LINE. The reach reading below enumerates each
// member's use sites by KIND -- producing call; address-only handoff to a NAMED hand-owned consumer
// -- so a line move is not a red and a NEW USE is. The named consumers are not a hardcoded list
// either: the guard requires each one to be DECLARED in a file carrying the hand-own module marker,
// derived with q86's own handOwnMarkerPattern, so a consumer that stops being hand-owned stops
// counting as an address-only handoff.
//
// THE SCOPE OF A USE. Uses are resolved within the ENCLOSING MEMBER of the producing call, not
// across the file. root_windows.cs is why: it binds `ctx` from CertCreateCertificateContext at :52
// and re-uses the same name at :284 for a chain context out of an unrelated loop. A file-wide scan
// would attribute that to this member and report a use nobody wrote. The control plants exactly
// that shape.

// certContextProducerPattern binds the variable a producing call writes its context into, in either
// spelling the converter emits: `var (leafCtx, err) = …` and `(var ctx, errΔ1) = …`.
var certContextProducerPattern = regexp.MustCompile(`(?:var\s*\(\s*([A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*)|\(\s*var\s+([A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*))\s*,[^)]*\)\s*=`)

// certContextFieldReadPattern matches the three shapes that MATERIALISE a record through a box and
// are therefore the defect: `~box`, `box.Value` and `box.Field`. `Ꮡx` (taking an address) is not a
// read and is not matched.
var certContextFieldReadPattern = func(name string) *regexp.Regexp {
	return regexp.MustCompile(`(~\s*` + regexp.QuoteMeta(name) + `\b|\b` + regexp.QuoteMeta(name) + `\s*\.)`)
}

// memberDeclarationPattern matches a member declaration by name, for deciding whether a consumer is
// declared inside a hand-owned file.
var memberDeclarationPattern = func(name string) *regexp.Regexp {
	return regexp.MustCompile(`(?m)^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)\s+[^;]*?\b` + regexp.QuoteMeta(name) + `(?:<[^>()]*>)?\(`)
}

const (
	useProducingCall = "producing call"
	useHandoffPrefix = "address-only handoff -> "
	useFieldRead     = "field read"
)

type disclosedInertMember struct {
	// member is the wrapper whose returned box is disclosed.
	member string
	// uses is the reach reading, BY KIND, sorted. An empty list means the member has no caller at
	// all, which is a reading in its own right and is asserted as one.
	uses []string
	// why states the reach reading in a sentence, for the reader of a red.
	why string
	// retirement names what ends the disclosure, so a deferred class carries its own exit.
	retirement string
}

// disclosedInertBoxDerefs is the reach reading COORD ruled on, re-measured on every run. Rows are
// added only by a ruling; a use site this list does not carry is a red, never an edit to silence it.
var disclosedInertBoxDerefs = []disclosedInertMember{
	{
		member: "CertCreateCertificateContext",
		uses: []string{
			useProducingCall,
			useProducingCall,
			useHandoffPrefix + "CertAddCertificateContextToStore",
			useHandoffPrefix + "CertAddCertificateContextToStore",
			useHandoffPrefix + "CertFreeCertificateContext",
			useHandoffPrefix + "CertFreeCertificateContext",
		},
		why: "crypto/x509's createStoreContext produces a leaf context and one per intermediate, and " +
			"hands each straight back to crypt32 through two hand-owned wrappers; no field is read " +
			"through any of them, so the wrong-offset defect cannot fire",
		retirement: "the ж-box arc's native box kind for a reference-bearing pointee at a native " +
			"boundary, post-hop, which retires this disclosure by construction",
	},
	{
		member: "CertEnumCertificatesInStore",
		uses:   nil,
		why: "no caller on either side of the boundary at the corpus pin: not in the converted corpus, " +
			"and in Go's own tree only the //sys directive, the generated wrapper, a vendored x/sys " +
			"copy and a stdlib manifest row",
		retirement: "the same arc change; until then an unused generated wrapper cannot fire a defect " +
			"nobody reaches",
	},
}

type reachUse struct {
	file string // relative to src/core
	line int
	kind string
}

type reachScan struct {
	filesScanned int
	handOwnFiles int
	producers    int
	byMember     map[string][]reachUse
	unclassified []string
}

// enclosingMemberRange returns the [start, end) line range of the member declaration containing the
// given line, so a binding is resolved where it was written rather than across the file.
func enclosingMemberRange(lines []string, at int) (int, int) {
	start := 0

	for i := at; i >= 0; i-- {
		if memberHeaderPattern.MatchString(lines[i]) && !strings.HasPrefix(strings.TrimSpace(lines[i]), "//") {
			start = i
			break
		}
	}

	depth := 0
	opened := false

	for i := start; i < len(lines); i++ {
		depth += strings.Count(lines[i], "{") - strings.Count(lines[i], "}")

		if strings.Contains(lines[i], "{") {
			opened = true
		}

		if opened && depth <= 0 {
			return start, i + 1
		}
	}

	return start, len(lines)
}

// handOwnedMembers reports which of the named members are DECLARED in a file carrying the hand-own
// module marker. Derived rather than listed: a consumer that stops being hand-owned stops counting.
func handOwnedMembers(t *testing.T, root string, tracked []string, names []string) (map[string]string, int) {
	t.Helper()

	found := map[string]string{}
	handOwn := 0

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		text := readTrackedText(t, root, rel)

		if !handOwnMarkerPattern.MatchString(text) {
			continue
		}

		handOwn++

		for _, name := range names {
			if _, already := found[name]; already {
				continue
			}

			if memberDeclarationPattern(name).MatchString(text) {
				found[name] = strings.TrimPrefix(rel, "src/core/")
			}
		}
	}

	return found, handOwn
}

// scanReach finds every producing call of each member and classifies every use of the box it binds.
func scanReach(t *testing.T, root string, tracked []string, members []string, handOwned map[string]string) reachScan {
	t.Helper()

	scan := reachScan{byMember: map[string][]reachUse{}}

	for _, member := range members {
		scan.byMember[member] = nil
	}

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		text := readTrackedText(t, root, rel)
		relevant := false

		for _, member := range members {
			if strings.Contains(text, member+"(") {
				relevant = true
				break
			}
		}

		if !relevant {
			continue
		}

		scan.filesScanned++
		lines := strings.Split(text, "\n")
		short := strings.TrimPrefix(rel, "src/core/")

		for n, line := range lines {
			trimmed := strings.TrimSpace(line)

			if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*") {
				continue
			}

			for _, member := range members {
				if !strings.Contains(line, member+"(") {
					continue
				}

				binding := certContextProducerPattern.FindStringSubmatch(line)

				if binding == nil {
					// A mention that binds nothing is one of two things, and they are not the same.
					// The wrapper's OWN DECLARATION is not a use at all. A CALL whose result is
					// discarded is a use -- it cannot fire the wrong-offset defect, since nothing can
					// be read through a value nobody holds, but COORD's ruling is that a new use site
					// OF ANY KIND is read by a human, so it goes to UNCLASSIFIED rather than being
					// skipped into silence. (Found by re-reading this scanner's own diff before the
					// push: the first spelling skipped both, which would have let a new discarding
					// caller land unseen.)
					if memberDeclarationPattern(member).MatchString(line) {
						continue
					}

					scan.unclassified = append(scan.unclassified,
						fmt.Sprintf("%s:%d :: %s -> <result discarded> (%s)", short, n+1, member, strings.TrimSpace(line)))

					continue
				}

				name := binding[1]

				if name == "" {
					name = binding[2]
				}

				scan.producers++
				scan.byMember[member] = append(scan.byMember[member], reachUse{file: short, line: n + 1, kind: useProducingCall})

				candidates := make([]string, 0, len(handOwned))

				for candidate := range handOwned {
					candidates = append(candidates, candidate)
				}

				sort.Strings(candidates)

				start, end := enclosingMemberRange(lines, n)
				read := certContextFieldReadPattern(name)
				word := regexp.MustCompile(`\b` + regexp.QuoteMeta(name) + `\b`)

				for i := start; i < end; i++ {
					if i == n {
						continue
					}

					useLine := lines[i]
					useTrimmed := strings.TrimSpace(useLine)

					if strings.HasPrefix(useTrimmed, "//") || strings.HasPrefix(useTrimmed, "*") {
						continue
					}

					if !word.MatchString(useLine) {
						continue
					}

					if read.MatchString(useLine) {
						scan.byMember[member] = append(scan.byMember[member], reachUse{file: short, line: i + 1, kind: useFieldRead})
						continue
					}

					consumer := ""

					for _, candidate := range candidates {
						// The consumer is matched as a WHOLE WORD, not as `name(`: a deferred handoff
						// passes the wrapper as a method group with no parameter list --
						// `defer(syscall.CertFreeCertificateContext, leafCtx, ref ᒐ)` is the shape
						// root_windows.cs:36 actually uses -- and a `name(` test reads that line as
						// unclassified. The line is already known to carry our variable, and a field
						// read through it was ruled out above, so the box reaching a hand-owned
						// consumer on this line is the handoff.
						if regexp.MustCompile(`\b` + regexp.QuoteMeta(candidate) + `\b`).MatchString(useLine) {
							consumer = candidate
							break
						}
					}

					if consumer != "" {
						scan.byMember[member] = append(scan.byMember[member], reachUse{file: short, line: i + 1, kind: useHandoffPrefix + consumer})
						continue
					}

					scan.unclassified = append(scan.unclassified,
						fmt.Sprintf("%s:%d :: %s -> %s (%s)", short, i+1, member, name, strings.TrimSpace(useLine)))
				}
			}
		}
	}

	for member := range scan.byMember {
		sort.Slice(scan.byMember[member], func(i, j int) bool {
			a, b := scan.byMember[member][i], scan.byMember[member][j]

			if a.file != b.file {
				return a.file < b.file
			}

			return a.line < b.line
		})
	}

	sort.Strings(scan.unclassified)

	return scan
}

func useKinds(uses []reachUse) []string {
	kinds := make([]string, 0, len(uses))

	for _, use := range uses {
		kinds = append(kinds, use.kind)
	}

	sort.Strings(kinds)

	return kinds
}

// consumerNames collects every hand-owned consumer the reach reading names, so the guard can derive
// which members must be hand-owned rather than assuming it.
func consumerNames() []string {
	seen := map[string]bool{}
	var names []string

	for _, disclosed := range disclosedInertBoxDerefs {
		for _, use := range disclosed.uses {
			if consumer, ok := strings.CutPrefix(use, useHandoffPrefix); ok && !seen[consumer] {
				seen[consumer] = true
				names = append(names, consumer)
			}
		}
	}

	sort.Strings(names)

	return names
}

func disclosedMembers() []string {
	names := make([]string, 0, len(disclosedInertBoxDerefs))

	for _, disclosed := range disclosedInertBoxDerefs {
		names = append(names, disclosed.member)
	}

	sort.Strings(names)

	return names
}

// TestDisclosedInertBoxDerefsAreStillInert is the guard.
func TestDisclosedInertBoxDerefsAreStillInert(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	// The kinds, over q86's declared set. A disclosure is only meaningful while the census still
	// declares the site, so the two lists are cross-checked rather than kept in parallel by hand.
	hazards, disclosed := 0, map[string]bool{}

	for _, declared := range declaredBoxDerefs {
		switch declared.kind {
		case boxDerefHazard:
			hazards++
		case boxDerefDisclosedInert:
			disclosed[declared.member()] = true
		default:
			t.Fatalf("declaredBoxDerefs row %q carries no kind", declared.row)
		}
	}

	if hazards != 0 {
		t.Errorf("q86 declares %d HAZARD row(s): a native-boundary site is awaiting its companion. "+
			"That is a defect to cure, not a disclosure", hazards)
	}

	if len(disclosed) != len(disclosedInertBoxDerefs) {
		t.Fatalf("q86 declares %d DISCLOSED-INERT member(s) and this guard carries a reach reading for %d; "+
			"a disclosed row without a reading is a claim nobody re-measures", len(disclosed), len(disclosedInertBoxDerefs))
	}

	for _, disclosedMember := range disclosedInertBoxDerefs {
		if !disclosed[disclosedMember.member] {
			t.Errorf("this guard carries a reach reading for %s, which q86 does not declare DISCLOSED-INERT", disclosedMember.member)
		}

		if disclosedMember.why == "" || disclosedMember.retirement == "" {
			t.Errorf("%s is disclosed without a reading or without a retirement plan; a deferred class carries its own exit", disclosedMember.member)
		}
	}

	consumers := consumerNames()
	handOwned, handOwnFiles := handOwnedMembers(t, root, tracked, consumers)

	if handOwnFiles < 50 {
		t.Fatalf("VACUOUS: %d hand-own files found; the marker scan is measuring nothing", handOwnFiles)
	}

	for _, consumer := range consumers {
		declaredIn, ok := handOwned[consumer]

		if !ok {
			t.Errorf("the reach reading calls %s an address-only handoff, but no hand-owned file declares it. "+
				"A handoff is only address-only while its consumer is the one that hands the address back", consumer)
			continue
		}

		t.Logf("hand-owned consumer %s declared in %s", consumer, declaredIn)
	}

	scan := scanReach(t, root, tracked, disclosedMembers(), handOwned)

	t.Logf("files naming a disclosed member %d · producing calls %d · hand-own files %d",
		scan.filesScanned, scan.producers, handOwnFiles)

	for _, disclosedMember := range disclosedInertBoxDerefs {
		measured := useKinds(scan.byMember[disclosedMember.member])
		declaredUses := append([]string(nil), disclosedMember.uses...)
		sort.Strings(declaredUses)

		for _, use := range scan.byMember[disclosedMember.member] {
			if use.kind == useFieldRead {
				t.Errorf("FIELD READ THROUGH A DISCLOSED-INERT BOX: %s:%d :: %s\n"+
					"the disclosure rests on nothing reading through this box -- the fork (ii) defect is a WRONG "+
					"FIELD OFFSET and this line materialises the record. The row moves to HAZARD and the wrapper "+
					"needs its companion; the disclosure does not cover it", use.file, use.line, disclosedMember.member)
			}
		}

		if fmt.Sprint(measured) != fmt.Sprint(declaredUses) {
			t.Errorf("UNDECLARED USE OF %s\nmeasured %v\ndeclared %v\n"+
				"a use site this reading does not carry is read by a human, never edited away: an address-only "+
				"handoff is re-stamped deliberately, a field read moves the row to HAZARD",
				disclosedMember.member, measured, declaredUses)
		}

		t.Logf("%s · uses %d %v", disclosedMember.member, len(measured), measured)
	}

	for _, row := range scan.unclassified {
		t.Errorf("UNCLASSIFIED USE: %s\n"+
			"neither a producing call, nor an address-only handoff to a hand-owned consumer, nor a field read. "+
			"A use this guard cannot classify is not a use it can call inert", row)
	}
}

// TestReachScannerFires is the control: the same scanner over a synthetic tracked tree carrying a
// field read, a use of a same-named variable in ANOTHER member, and a consumer that is not hand-owned.
func TestReachScannerFires(t *testing.T) {
	root := t.TempDir()

	write := func(rel, content string) {
		full := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(full, []byte(strings.ReplaceAll(content, "\n", "\r\n")), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	write("src/core/syscall/windows/certchain_impl.cs", `[module: go.GoManualConversion]

public static error CertFreeCertificateContext(ж<CertContext> Ꮡctx) {
    return default!;
}
`)

	// A consumer that is NOT hand-owned: a handoff to it cannot be called address-only.
	write("src/core/syscall/windows/plain.cs", `public static error CertShipContextSomewhere(ж<CertContext> Ꮡctx) {
    return default!;
}
`)

	write("src/core/crypto/x509/windows/root_windows.cs", `internal static error createStoreContext() {
    var (leafCtx, err) = syscall.CertCreateCertificateContext(1, Ꮡ(raw, 0), 2);
    defer(syscall.CertFreeCertificateContext, leafCtx);
    return err;
}

internal static error readsThroughIt() {
    var (ctx, err) = syscall.CertCreateCertificateContext(1, Ꮡ(raw, 0), 2);
    var length = (~ctx).Length;
    return err;
}

internal static error handsItToSomethingNotHandOwned() {
    var (other, err) = syscall.CertCreateCertificateContext(1, Ꮡ(raw, 0), 2);
    err = CertShipContextSomewhere(other);
    return err;
}

internal static error discardsTheResult() {
    syscall.CertCreateCertificateContext(1, Ꮡ(raw, 0), 2);
    return default!;
}

internal static error anotherMemberReusesTheName() {
    foreach (var (_, ctx) in lqCtxs) {
        var (chain, err) = verifyChain(Ꮡc, ctx, Ꮡopts);
    }
    return default!;
}
`)

	tracked := []string{
		"src/core/syscall/windows/certchain_impl.cs",
		"src/core/syscall/windows/plain.cs",
		"src/core/crypto/x509/windows/root_windows.cs",
	}

	consumers := []string{"CertFreeCertificateContext", "CertShipContextSomewhere"}
	handOwned, handOwnFiles := handOwnedMembers(t, root, tracked, consumers)

	if handOwnFiles != 1 {
		t.Fatalf("control found %d hand-own files, want 1", handOwnFiles)
	}

	if _, ok := handOwned["CertFreeCertificateContext"]; !ok {
		t.Fatal("control: CertFreeCertificateContext is declared in the marked file and must be recognised as hand-owned")
	}

	if where, ok := handOwned["CertShipContextSomewhere"]; ok {
		t.Fatalf("control: CertShipContextSomewhere is declared in an UNMARKED file and must not count as hand-owned (found in %s)", where)
	}

	scan := scanReach(t, root, tracked, []string{"CertCreateCertificateContext"}, handOwned)

	if scan.producers != 3 {
		t.Fatalf("control found %d producing calls, want 3", scan.producers)
	}

	got := useKinds(scan.byMember["CertCreateCertificateContext"])
	want := []string{
		useHandoffPrefix + "CertFreeCertificateContext",
		useFieldRead,
		useProducingCall,
		useProducingCall,
		useProducingCall,
	}
	sort.Strings(want)

	if fmt.Sprint(got) != fmt.Sprint(want) {
		t.Fatalf("control classified %v\nwant %v", got, want)
	}

	// Two things must land in UNCLASSIFIED rather than be waved through: a handoff to a consumer that
	// is NOT hand-owned (the arm keeping "address-only" from meaning "unread by me"), and a call whose
	// result is DISCARDED (a new use site of a kind this reading does not carry).
	if len(scan.unclassified) != 2 {
		t.Fatalf("control reported %d unclassified uses, want 2: %v", len(scan.unclassified), scan.unclassified)
	}

	unclassified := strings.Join(scan.unclassified, "\n")

	if !strings.Contains(unclassified, "CertShipContextSomewhere") {
		t.Fatalf("control did not report the handoff to a consumer that is not hand-owned: %v", scan.unclassified)
	}

	if !strings.Contains(unclassified, "<result discarded>") {
		t.Fatalf("control did not report the call whose result is discarded: %v", scan.unclassified)
	}

	// FUNCTION SCOPING: the `ctx` bound in anotherMemberReusesTheName is a different variable in a
	// different member. A file-wide scan attributes it to this member and reports a use nobody wrote.
	for _, use := range scan.byMember["CertCreateCertificateContext"] {
		if use.line >= 20 {
			t.Fatalf("control attributed a use at line %d to this member; the enclosing-member scoping regressed", use.line)
		}
	}
}
