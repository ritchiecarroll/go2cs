// nativeCallGateDarwin_test.go - Gbtc
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

// DARWIN'S NATIVE CALL GATE, AND THE FOURTEEN SITES BEHIND IT.
//
// THE CLASS, AND WHICH FORK OF IT. A converted wrapper hands the kernel a managed struct BY ADDRESS.
// When the pointee is reference-bearing -- it holds a ж<T>, array<T>, slice<T>, @string or map<> --
// the CLR gives the managed record AUTO layout, so the address does not describe the native record
// and libc reads the wrong offsets. That is fork (i) of the native-boundary class, and it is what
// this file measures. It is NOT the fork q86 measured: nativeBoundaryBoxDeref_test.go reads fork
// (ii), a kernel-RETURNED address reinterpreted as a managed record, which is why its predicate is a
// cast `(ж<T>)(uintptr)` and this one is a parameter type. One class, two directions across the
// same boundary, and they share the layout rule -- referenceBearing -- and nothing else.
//
// THE GATE, AND WHY THIS IS A CENSUS AND NOT A WATCH. Windows has ONE gate (asmstdcall) and it is
// DEAD, so nativeCallGateWindows_test.go watches the GATE -- it asserts the declaration is bodyless
// and unrealized in both buckets, and PRINTS its 102 stdcallN call sites as a reading it does not
// assert, precisely because a dead gate makes the site count not worth gating on.
// Darwin's gate is libcCall, and it is REALIZED -- a bodied hand-own at
// src/core/runtime/darwin/libccall_impl.cs, on golib's platform-neutral dispatcher. Every site behind
// it is LIVE, so the darwin reading has to be per-site. That distinction is asserted, not assumed:
// TestDarwinLibcCallGateIsRealized fails if the gate ever goes bodyless, because a census of live
// sites taken over a dead gate is measuring a different flavour and would read as good news.
//
// WHY THE POINTEE IS READ FROM THE MEMBER SIGNATURE AND NOT THE ARGS STRUCT. Go's //go:cgo_unsafe_args
// block lift flattens every parameter into a machine word, and the converter reproduces it:
//
//	[GoType("dyn")] internal partial struct pthread_attr_init_args {
//	    internal uintptr attr;                                        <- the type is GONE here
//	}
//	internal static int32 pthread_attr_init(ж<pthreadattr> Ꮡattr) {   <- and it is HERE
//	    ref var args = ref heap(new pthread_attr_init_args((uintptr)Ꮡattr.OrTypedNil()), out var Ꮡargs);
//	    var ret = libcCall((@unsafe.Pointer)abi.FuncPCABI0(pthread_attr_init_trampoline), …);
//
// C1's first darwin census looked in the args struct and read ZERO pointer-to-reference-bearing sites
// across the whole population -- a zero that was refuted by libccall_impl.cs's own remedy list naming
// five members of the class. The lesson is recorded because the route matters: a census whose finding
// is a zero must be checked against a KNOWN POSITIVE from another population.
//
// ⚠ THE ARITHMETIC IS CORRECTED HERE, AND THE CORRECTION IS THE FIRST THING THIS FILE OWES A READER.
// q90 (2) (mailbox 2ae99188f) reported "46 libcCall sites = 7 + 0 + 7 + 32", and the 32-clear bucket
// was written as including "libcCall's own body". libcCall's own body is the DECLARATION, not a call
// site: it is what the 45 callers call. Measured at this tree by a call-shaped predicate, the
// population is 45 CALL SITES = 14 reference-bearing + 31 clear. The declared set of fourteen -- the
// number that carries the verdict -- is UNAFFECTED; what moves is the denominator, by one, in the
// direction that makes the guard's own log honest. The prediction on record (mailbox, q90 (b)) said
// 46/14/32 and is falsified on the denominator alone, by this cause, stated before the count is read
// anywhere else.
//
// THE TWO KINDS. Every row here is a HAZARD -- a defect awaiting its companion, a population that
// SHRINKS to zero and NEVER grows. An undeclared site in the class is a defect to fix, not a row to
// add; a row whose remedy lands is deleted IN THE SAME COMMIT that cures it. Each row also carries
// its REMEDY, which is what makes it HAZARD-DEFERRED rather than merely known: the darwin axis's
// increment 6 native mirror, per libccall_impl.cs's own pending list. No row here is
// DISCLOSED-INERT -- that kind (q86's CertContext pair) needs a reach reading proving nothing reads
// through the box, and no such reading can be taken on darwin while its run layer is unbuilt.
//
// ⚠ WHERE THE SEVEN UNNAMED MEMBERS ARE NAMED, and why it is here rather than in the file whose
// header names the other seven. libccall_impl.cs's header names FIVE pointee TYPES as pending
// (itimerval, keventt, stackt, pthreadattr; usigactiont done), which reach SEVEN members. C1's
// q90 (2) reading found SEVEN MORE members in the same class that the header does not name -- the
// pthread mutex/cond family -- and COORD ruled them SAME CLASS BY SHAPE at mailbox fcc021277 §4. They
// are named in THIS file's declared set, and libccall_impl.cs is not edited, for three reasons:
// (1) a comment-only edit to a hand-owned C# file buys a compile another lane must run for no
// behavioural change; (2) the guard is the thing that goes red, so the reader who needs the seven is
// already reading it; (3) that header is the DARWIN AXIS's record of its own remedy work, and the
// seven are not remedies it has planned -- writing them there would put one lane's reading inside
// another owner's plan. This file CITES the header rather than extending it.
//
// WHAT IS NOT CLAIMED. That libc reads through any of the fourteen. That is a run-time reading nobody
// can take while darwin's run layer is unbuilt. The rows are HAZARD-DEFERRED and DISCLOSED, remedied
// by the darwin axis's increment, and this guard says exactly that in its own red.
//
// CONTROLS, both of them. TestDarwinLibcCallScannerFires drives the SAME scanner over a synthetic
// tracked tree and requires it to REPORT (a) a member whose pointee is reference-bearing through a
// plain field, (b) a member whose pointee is reference-bearing ONLY through an INITIALIZED array
// field -- pthreadattr's real shape, and the arm the q86 field-pattern fix bought -- (c) a member
// whose pointee is reference-bearing only through a NESTED named type (itimerval through timeval),
// and (d) a pointee that resolves nowhere, as unresolved rather than as benign; and requires it NOT
// to report (e) an all-scalar pointee, nor (f) a call in a line comment, a block comment or a
// TRAILING comment on a line of code. It further requires the three bucketed counts to be exact --
// hand-own 1 and declaration 1, each COUNTED and not gated, so two populations are never folded into
// one number. TestRealizationBucketReadsBothBraceStyles is the control on the GATE test's own
// predicate, over K&R, Allman, a signature spanning lines, a bodyless declaration and both spellings
// of the interop attribute. The real corpus's 31 clear sites are the KNOWN NEGATIVE that proves the
// predicate is a rule and not a plant-detector.
//
// RED-FIRST, TAKEN AT THE REAL CORPUS AND RESTORED BYTE-IDENTICAL. A gate that has never been made
// to fail proves nothing, so three deliberate regressions were run against src/core itself before
// this file was committed (sys_darwin.cs sha256 f37bfa80f30ed40a5…, libccall_impl.cs
// 940646ce4eaffaa80…, both restored and re-read identical):
//
//	open(ж<byte> Ꮡname, …)              -> ж<keventt>   guard: "UNDECLARED reference-bearing libcCall
//	                                                     site: runtime/darwin/sys_darwin.cs:549 open
//	                                                     -> ж<keventt>"
//	pthread_mutex_lock(ж<pthreadmutex>)  -> ж<timespec>  guard: "DECLARED BUT NOT MEASURED:
//	                                                     pthread_mutex_lock -> ж<pthreadmutex>", with
//	                                                     the remedy printed beside it
//	libcCall's `) {`                     -> `);`         gate:  "realizes as NONE, want BODY"
//
// ⚠ AND THE FIRST TWO TOGETHER ARE THE ARGUMENT FOR THE DECLARED SET. Run at the same time they move
// the census by NOTHING -- one clear site became reference-bearing and one reference-bearing site
// became clear, so the log still read "45 = 14 + 31" while TWO members were wrong. A guard that
// asserted the COUNT would have passed. Only the by-name arms caught it, which is why every row here
// is a member and not a tally.
//
// ⚠ HOW MUCH OF THIS CENSUS THE q86 FIELD-PATTERN FIX BUYS, MEASURED RATHER THAN ASSERTED. The
// file-pattern fix at 292756d64 -- admitting a field that carries an initializer -- is reverted to its
// pre-fix spelling in nativeBoundaryBoxDeref_test.go and this guard re-run against the same corpus:
//
//	with the fix     call sites 45 = reference-bearing 14 + clear 31 · parameters 21   (green)
//	without it       call sites 45 = reference-bearing  2 + clear 43 · parameters  4   (TWELVE rows
//	                 DECLARED BUT NOT MEASURED)
//
// The two that survive the revert are stackt and keventt, whose reference is a plain UNINITIALIZED
// `ж<byte>` field. Every other pointee in the set -- pthreadattr, pthreadmutex, pthreadmutexattr,
// pthreadcond, pthreadcondattr and (through timeval) itimerval -- is reference-bearing ONLY through an
// `array<T> x = new(N);`, so twelve of fourteen hazards are invisible to the pre-fix pattern. Restored
// byte-identical afterwards (sha256 695fbf907cfdfd11c…). This is also the arm the prediction on record
// named as a falsifier, taken as a measurement rather than left as a claim.
//
// THE PREDICATE IS CONSULTED, NOT COPIED. buildStructTable, referenceBearing, blittablePointees,
// memberHeaderPattern, simpleName and readTrackedText are nativeBoundaryBoxDeref_test.go's, used
// here directly. A second copy of a layout rule drifts, and a drifted instrument files its own target
// under the SOUND bucket -- the worst direction for an instrument whose finding is a zero.

// libcCallSitePattern matches a CALL of the gate. The word boundary keeps GoLibcCall.* out, and the
// declaration is excluded separately: a `grep libcCall` that counts the declaration as a site is
// exactly the off-by-one this file corrects.
var libcCallSitePattern = regexp.MustCompile(`(^|[^A-Za-z0-9_.])libcCall\s*\(`)

// libcCallDeclPattern matches the gate's own declaration, bodied or bodyless. ⚠ THE (?m) IS
// LOAD-BEARING: this pattern is used both per line and as a whole-FILE prefilter, and without the
// multi-line flag `^` anchors to the start of the whole text alone -- so the prefilter answered "no
// declaration in this file" for every file in the corpus and TestDarwinLibcCallGateIsRealized read
// ZERO gates. Caught by that zero being the wrong shape: the gate is cited by path and line four
// lines above it. The same anchor fault, in the opposite direction, is what made q86's hand-own
// marker read MENTIONS as DECLARATIONS.
var libcCallDeclPattern = regexp.MustCompile(`(?m)^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)\s+(?:static\s+|unsafe\s+|partial\s+|extern\s+)*[A-Za-z_@\p{L}][^\s(]*\s+libcCall\s*\(`)

// boxParamPattern matches one parameter that arrives as a golib heap box: `ж<pthreadattr> Ꮡattr`.
var boxParamPattern = regexp.MustCompile(`^ж\s*<(.+)>\s+[A-Za-z_@\p{L}Ꮡ]\S*$`)

// darwinGateFile is where the gate is realized; darwinGateName is the gate.
const (
	darwinGateFile = "src/core/runtime/darwin/libccall_impl.cs"
	darwinGateName = "libcCall"
)

// declaredLibcCall is one declared row: the member the site sits in, the reference-bearing pointees
// its signature carries, and the remedy that retires it.
type declaredLibcCall struct {
	member string
	// pointees is what the member's signature carries that costs it sequential layout. It is part of
	// the row so a signature that changes its parameter types reads as a CHANGED row rather than
	// silently keeping a stale row's name.
	pointees string
	// namedByHeader records whether libccall_impl.cs's own pending list names this member's pointee
	// type. The seven that are false are C1's q90 (2) finding; see the header above.
	namedByHeader bool
	remedy        string
}

func (declared declaredLibcCall) row() string {
	return fmt.Sprintf("%s -> %s", declared.member, declared.pointees)
}

// declaredLibcCalls is the declared set: FOURTEEN HAZARD-DEFERRED rows, every one of them in
// src/core/runtime/darwin/sys_darwin.cs, each with its remedy named. Ruled at mailbox fcc021277 §4
// after C1's q90 (2) sizing at 2ae99188f.
//
// ⚠ A ROW SPELLS PARAMETERS, NOT TYPES, and that is the second correction this file owes q90 (2).
// That sizing listed each member's reference-bearing pointee TYPE once; three members take the same
// type TWICE -- sigaltstack(new, old), setitimer(mode, new, old), kevent(kq, ch, nch, ev, nev, ts) --
// so the fourteen members carry TWENTY-ONE reference-bearing parameters. The member count, which is
// what the verdict rides on, is unchanged; a row that hid the second parameter would let one of the
// pair be cured while the row still read as fully open. Caught by the guard's own row-text arm on
// its first run against the corpus.
//
// This population SHRINKS. A member whose remedy lands loses its site (usigactiont's already did --
// its member is DISPLACED into sigaction_impl.cs, so no libcCall site takes a ж<usigactiont> at all,
// which is what "done" means here: the remedy retired the SITE, not just the layout) and its row goes
// in the same commit. A NEW reference-bearing site is a defect to fix, never a row to add.
var declaredLibcCalls = []declaredLibcCall{
	// Named by libccall_impl.cs's own pending list (7 members over 4 pointee types).
	{"pthread_attr_init", "ж<pthreadattr>", true, "increment 6's native mirror for pthreadattr"},
	{"pthread_attr_getstacksize", "ж<pthreadattr>", true, "increment 6's native mirror for pthreadattr"},
	{"pthread_attr_setdetachstate", "ж<pthreadattr>", true, "increment 6's native mirror for pthreadattr"},
	{"pthread_create", "ж<pthreadattr>", true, "increment 6's native mirror for pthreadattr"},
	{"sigaltstack", "ж<stackt>, ж<stackt>", true, "increment 6's native mirror for stackt"},
	{"setitimer", "ж<itimerval>, ж<itimerval>", true, "increment 6's native mirror for itimerval (reference-bearing THROUGH timeval)"},
	{"kevent", "ж<keventt>, ж<keventt>", true, "increment 6's native mirror for keventt"},

	// ⚠ NOT named by that header (7) — C1's q90 (2) finding, ruled SAME CLASS BY SHAPE at
	// fcc021277 §4. Every one of these pointees is reference-bearing ONLY through an INITIALIZED
	// `array<int8> X__opaque = new(N);` field, which is why the q86 field-pattern fix
	// (292756d64) is load-bearing for this whole half of the set.
	{"pthread_mutex_init", "ж<pthreadmutex>, ж<pthreadmutexattr>", false, "increment 6's native mirror for the pthread mutex family"},
	{"pthread_mutex_lock", "ж<pthreadmutex>", false, "increment 6's native mirror for the pthread mutex family"},
	{"pthread_mutex_unlock", "ж<pthreadmutex>", false, "increment 6's native mirror for the pthread mutex family"},
	{"pthread_cond_init", "ж<pthreadcond>, ж<pthreadcondattr>", false, "increment 6's native mirror for the pthread cond family"},
	{"pthread_cond_wait", "ж<pthreadcond>, ж<pthreadmutex>", false, "increment 6's native mirror for the pthread cond family"},
	{"pthread_cond_timedwait_relative_np", "ж<pthreadcond>, ж<pthreadmutex>", false, "increment 6's native mirror for the pthread cond family"},
	{"pthread_cond_signal", "ж<pthreadcond>", false, "increment 6's native mirror for the pthread cond family"},
}

type libcCallSite struct {
	file     string // relative to src/core
	line     int
	member   string
	pointees []string // the reference-bearing ones, in signature order
	row      string
}

type libcCallScan struct {
	filesScanned int
	rawMatches   int
	inComment    int
	inHandOwn    int
	declarations int
	callSites    int
	hazards      []libcCallSite
	clear        int
	// noMember is a call site whose enclosing member could not be read. It is reported, never
	// skipped: a scanner that silently drops what it cannot parse reports a smaller population and
	// calls it good news.
	noMember   []string
	unresolved []string
}

// enclosingMember walks back from a call site to the member declaration that contains it and returns
// the member's name and its parameter list, read across as many lines as the parameters span.
//
// It counts parentheses and does NOT lex string or character literals, so a `)` inside a default
// value's string would end the list early. Measured at this tree: ZERO of the 45 enclosing signatures
// contains a quote of either kind. The limitation is stated rather than engineered around, and the
// arm that would catch it going wrong is scan.noMember plus the by-name declared set -- a truncated
// list loses a parameter, and a member losing its last reference-bearing parameter reads as DECLARED
// BUT NOT MEASURED rather than as silence.
func enclosingMember(lines []string, site int) (name string, params string, ok bool) {
	for i := site; i >= 0; i-- {
		trimmed := strings.TrimSpace(lines[i])

		if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*") {
			continue
		}

		loc := memberHeaderPattern.FindStringSubmatchIndex(lines[i])

		if loc == nil {
			continue
		}

		var builder strings.Builder
		depth := 0

		for j := i; j < len(lines); j++ {
			from := 0

			if j == i {
				from = loc[1] - 1 // the '(' the pattern ends on
			}

			for _, r := range lines[j][from:] {
				switch r {
				case '(':
					depth++

					if depth == 1 {
						continue
					}
				case ')':
					depth--

					if depth == 0 {
						return lines[i][loc[2]:loc[3]], builder.String(), true
					}
				}

				if depth >= 1 {
					builder.WriteRune(r)
				}
			}

			builder.WriteRune(' ')
		}

		return lines[i][loc[2]:loc[3]], "", false
	}

	return "", "", false
}

// splitParams splits a parameter list on top-level commas, respecting parentheses, angle brackets and
// square brackets. A tuple return type or a generic argument carries commas that are not separators.
func splitParams(params string) []string {
	var out []string
	var current strings.Builder
	paren, angle, square := 0, 0, 0

	for _, r := range params {
		switch r {
		case '(':
			paren++
		case ')':
			paren--
		case '<':
			angle++
		case '>':
			if angle > 0 {
				angle--
			}
		case '[':
			square++
		case ']':
			square--
		case ',':
			if paren == 0 && angle == 0 && square == 0 {
				if trimmed := strings.TrimSpace(current.String()); trimmed != "" {
					out = append(out, trimmed)
				}

				current.Reset()

				continue
			}
		}

		current.WriteRune(r)
	}

	if trimmed := strings.TrimSpace(current.String()); trimmed != "" {
		out = append(out, trimmed)
	}

	return out
}

// scanLibcCalls is the one scanner the guard and the control both drive.
func scanLibcCalls(t *testing.T, root string, tracked []string) libcCallScan {
	t.Helper()

	table := buildStructTable(t, root, tracked)
	scan := libcCallScan{}

	var scoped []string

	// SCOPE: every tracked .cs under src/core EXCEPT golib. The scope is not a tree list, so a new
	// file or a new platform directory carrying a call is in the population the day it is written --
	// a hard-coded tree is how a census goes blind to the thing it was widened for. golib is out
	// because it holds the DISPATCHER (GoLibcCall.*), which is what the gate calls, not a caller of
	// the gate; including it would put the mechanism inside its own census.
	for _, rel := range tracked {
		if strings.HasPrefix(rel, "src/core/") && strings.HasSuffix(rel, ".cs") &&
			!strings.HasPrefix(rel, "src/core/golib/") {
			scoped = append(scoped, rel)
		}
	}

	sort.Strings(scoped)

	for _, rel := range scoped {
		text := readTrackedText(t, root, rel)

		if !libcCallSitePattern.MatchString(text) && !libcCallDeclPattern.MatchString(text) {
			continue
		}

		scan.filesScanned++

		handOwn := handOwnMarkerPattern.MatchString(text)
		lines := strings.Split(text, "\n")
		dir := filepath.ToSlash(filepath.Dir(rel))
		inBlockComment := false

		for n, line := range lines {
			trimmed := strings.TrimSpace(line)
			commentHere := inBlockComment || strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*")

			if strings.HasPrefix(trimmed, "/*") && !strings.Contains(trimmed, "*/") {
				inBlockComment = true
			} else if inBlockComment && strings.Contains(trimmed, "*/") {
				inBlockComment = false
			}

			if !libcCallSitePattern.MatchString(line) {
				continue
			}

			scan.rawMatches++

			if commentHere {
				scan.inComment++
				continue
			}

			// ⚠ THE DECLARATION IS NOT A SITE. This is the whole of the 46-vs-45 correction: the
			// gate's own signature matches a bare `libcCall(` search, and counting it folds the
			// callee into the callers.
			if libcCallDeclPattern.MatchString(line) {
				scan.declarations++
				continue
			}

			// A trailing `// … libcCall(…)` on a line of code. The line is code, the match is not.
			if idx := strings.Index(line, "//"); idx >= 0 {
				if loc := libcCallSitePattern.FindStringIndex(line); loc != nil && loc[0] > idx {
					scan.inComment++
					continue
				}
			}

			if handOwn {
				// A hand-own's call is a human's deliberate choice and is reported, not gated --
				// two populations folded into one number is how a census stops discriminating.
				scan.inHandOwn++
				continue
			}

			scan.callSites++

			member, params, ok := enclosingMember(lines, n)

			if !ok || member == "" {
				scan.noMember = append(scan.noMember, fmt.Sprintf("%s:%d", strings.TrimPrefix(rel, "src/core/"), n+1))
				continue
			}

			site := libcCallSite{
				file:   strings.TrimPrefix(rel, "src/core/"),
				line:   n + 1,
				member: member,
			}

			unresolvedHere := false

			for _, param := range splitParams(params) {
				match := boxParamPattern.FindStringSubmatch(strings.TrimSpace(param))

				if match == nil {
					continue
				}

				pointee := strings.TrimSpace(match[1])
				name := simpleName(pointee)

				if blittablePointees[name] {
					continue
				}

				if _, resolved := table.resolve(dir, name); !resolved {
					scan.unresolved = append(scan.unresolved, fmt.Sprintf("%s :: %s -> ж<%s>", site.file, member, pointee))
					unresolvedHere = true

					continue
				}

				if table.referenceBearing(dir, name, true, nil) {
					site.pointees = append(site.pointees, "ж<"+pointee+">")
				}
			}

			if len(site.pointees) > 0 {
				site.row = fmt.Sprintf("%s -> %s", member, strings.Join(site.pointees, ", "))
				scan.hazards = append(scan.hazards, site)

				continue
			}

			if !unresolvedHere {
				scan.clear++
			}
		}
	}

	sort.Slice(scan.hazards, func(i, j int) bool {
		if scan.hazards[i].file != scan.hazards[j].file {
			return scan.hazards[i].file < scan.hazards[j].file
		}

		return scan.hazards[i].line < scan.hazards[j].line
	})

	sort.Strings(scan.noMember)
	sort.Strings(scan.unresolved)

	return scan
}

// The three realization buckets, in q90 (1)'s vocabulary: BODY is a declaration that opens a block;
// INTEROP is a bodyless one carrying [LibraryImport]/[DllImport], realized by the interop source
// generator; NONE is a bodyless one with neither, which PartialStubGenerator realizes as a throw
// (src/gen/go2cs-gen/PartialStubGenerator.cs:111).
const (
	realizationBody    = "BODY"
	realizationInterop = "INTEROP"
	realizationNone    = "NONE"
)

// ⚠ ONE RULE, ONE DEFINITION -- AND THE FOLD IS DONE. nativeCallGateWindows_test.go (C1's ref
// fbd5cbd932, merged onto the version tip at c6c20d4eae) declared interopAttributePattern for this
// same rule, spelled `\[\s*(?:LibraryImport|DllImport)\b` -- narrower, because it did not admit the
// FULLY QUALIFIED attribute. Measured when the two were written and re-measured at the fold: they
// disagree on `[System.Runtime.InteropServices.DllImport(“…”)]`, which the narrow one reads as NOT
// interop and therefore as the DEAD bucket -- a bodyless declaration reported unrealized when a
// generator does realize it, which is the false direction for a gate watch. The corpus carries 17 of
// those fully-qualified attributes (16 in syscall/linux/exec_unix.cs, 1 in
// internal/syscall/windows/registry/registry_test.cs, a windows-flavoured file that simply sits
// outside runtime/windows/), so the divergence was one corpus-shaped edit away from the windows
// watch's own population, which carries 0 of them today and 1 bare-form attribute. The narrow
// spelling is gone and both watches read THIS declaration; the two names existed only so the two
// files could compile together before the merge.
var interopRealizationPattern = regexp.MustCompile(`\[\s*(?:System\.Runtime\.InteropServices\.)?(?:LibraryImport|DllImport)\b`)

// realizationBucket classifies the declaration starting at lines[start].
//
// ⚠ IT READS THE FIRST MEANINGFUL CHARACTER AFTER THE SIGNATURE, NOT THE END OF THE SIGNATURE LINE.
// The first spelling asked whether the line ENDS with `{`, which is right for every K&R declaration
// the corpus carries and reads an ALLMAN one -- brace on the following line -- as bodyless. A
// hand-own file is written by a person and is exactly where that style would appear, so the cheap
// predicate fails false-RED on the one file class this guard exists to read. A bodyless declaration
// is one whose signature ends in `;`, and that is what is asked here.
func realizationBucket(lines []string, start int) string {
	depth := 0
	closed := false

	for i := start; i < len(lines) && i < start+24; i++ {
		line := lines[i]
		from := 0

		if !closed {
			for pos, r := range line {
				switch r {
				case '(':
					depth++
				case ')':
					depth--

					if depth == 0 {
						closed = true
						from = pos + 1
					}
				}

				if closed {
					break
				}
			}

			if !closed {
				continue
			}
		}

		rest := strings.TrimSpace(line[from:])

		if idx := strings.Index(rest, "//"); idx >= 0 {
			rest = strings.TrimSpace(rest[:idx])
		}

		if rest == "" {
			continue
		}

		if strings.HasPrefix(rest, "{") {
			return realizationBody
		}

		if strings.HasPrefix(rest, ";") {
			// A bodyless declaration: the attributes above it decide whether anything realizes it.
			for j := start; j >= 0 && j >= start-8; j-- {
				if interopRealizationPattern.MatchString(lines[j]) {
					return realizationInterop
				}

				if j < start && strings.TrimSpace(lines[j]) == "" {
					break
				}
			}

			return realizationNone
		}

		// A `where` clause or a constraint continues the signature; keep reading.
		if strings.HasPrefix(rest, "where") {
			continue
		}

		return realizationNone
	}

	return realizationNone
}

// TestDarwinLibcCallGateIsRealized asserts the fact that makes the census below a census rather than a
// watch: darwin's gate is DECLARED EXACTLY ONCE and it is BODIED. A bodyless libcCall would put every
// site behind a throw -- good news for the hazard, and a reading this file must never hand back
// silently as "the sites are fine".
func TestDarwinLibcCallGateIsRealized(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	type decl struct {
		file   string
		line   int
		bucket string
	}

	var decls []decl

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") || strings.HasPrefix(rel, "src/core/golib/") {
			continue
		}

		text := readTrackedText(t, root, rel)

		if !libcCallDeclPattern.MatchString(text) {
			continue
		}

		lines := strings.Split(text, "\n")

		for n, line := range lines {
			if !libcCallDeclPattern.MatchString(line) {
				continue
			}

			decls = append(decls, decl{file: rel, line: n + 1, bucket: realizationBucket(lines, n)})
		}
	}

	if len(decls) != 1 {
		t.Fatalf("expected exactly ONE declaration of %s under src/core (outside golib), found %d: %+v\n"+
			"the census below reads every call site against ONE gate; a second declaration means the "+
			"population is split and the reading does not describe it", darwinGateName, len(decls), decls)
	}

	if decls[0].file != darwinGateFile {
		t.Errorf("%s is declared at %s, expected %s\nthe gate moved; the header's citations and the "+
			"remedy ownership in this file's declared set point at the old path", darwinGateName, decls[0].file, darwinGateFile)
	}

	if decls[0].bucket != realizationBody {
		t.Errorf("%s at %s:%d realizes as %s, want %s\n"+
			"darwin's gate is REALIZED at this tree, and that is why q90 (b) is a per-site census rather "+
			"than a gate watch like windows's asmstdcall. An unrealized gate puts every site behind a throw: "+
			"the fourteen hazards below become unreachable, which is a DIFFERENT verdict and must be "+
			"re-ruled, not inherited", darwinGateName, decls[0].file, decls[0].line, decls[0].bucket, realizationBody)
	}

	t.Logf("gate %s realization %s at %s:%d", darwinGateName, decls[0].bucket, decls[0].file, decls[0].line)
}

// TestDarwinLibcCallSitesAreDeclared is the guard.
func TestDarwinLibcCallSitesAreDeclared(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	scan := scanLibcCalls(t, root, tracked)

	if scan.callSites == 0 {
		t.Fatal("zero libcCall call sites under src/core; darwin's converted dispatch bottom has 45 of " +
			"them at this tree, so a zero is the scanner failing, not the corpus changing")
	}

	declared := map[string]declaredLibcCall{}

	for _, row := range declaredLibcCalls {
		if _, dup := declared[row.member]; dup {
			t.Fatalf("declared set carries %s twice; a duplicated row makes the arithmetic below lie", row.member)
		}

		declared[row.member] = row
	}

	measured := map[string]bool{}

	for _, site := range scan.hazards {
		measured[site.member] = true

		row, ok := declared[site.member]

		if !ok {
			t.Errorf("UNDECLARED reference-bearing libcCall site: %s:%d %s\n"+
				"a wrapper hands libc the ADDRESS of a managed record whose pointee is reference-bearing, "+
				"so the CLR lays it out AUTO and the offsets libc reads are not the ones the record has. "+
				"This population SHRINKS to zero and never grows: fix the site (increment 6's native "+
				"mirror is the shape), do not add a row", site.file, site.line, site.row)

			continue
		}

		if row.pointees != strings.Join(site.pointees, ", ") {
			t.Errorf("declared row for %s reads %q, measured %q at %s:%d\n"+
				"the signature's reference-bearing parameters changed. Either the remedy landed in part "+
				"-- in which case the row moves in the same commit -- or a new pointee arrived, which is "+
				"a defect, not a row", site.member, row.pointees, strings.Join(site.pointees, ", "), site.file, site.line)
		}
	}

	for _, row := range declaredLibcCalls {
		if measured[row.member] {
			continue
		}

		t.Errorf("DECLARED BUT NOT MEASURED: %s\n"+
			"remedy on record: %s.\n"+
			"Either that remedy landed -- delete this row IN THE SAME COMMIT, as usigactiont's was deleted "+
			"when its member was displaced into sigaction_impl.cs -- or the scanner stopped seeing a site "+
			"it used to see, which is the failure mode that makes a census read clean while the corpus "+
			"does not", row.row(), row.remedy)
	}

	for _, orphan := range scan.noMember {
		t.Errorf("libcCall call site at %s whose enclosing member could not be read\n"+
			"the site is in the population and its pointees were never examined; a scanner that drops "+
			"what it cannot parse reports a smaller population and calls it good news", orphan)
	}

	for _, row := range scan.unresolved {
		t.Errorf("UNRESOLVED pointee: %s\n"+
			"the name is neither a machine scalar nor a struct declared under src/core, so its layout "+
			"cannot be judged. An unresolved pointee is reported, never assumed benign", row)
	}

	namedByHeader := 0

	for _, row := range declaredLibcCalls {
		if row.namedByHeader {
			namedByHeader++
		}
	}

	t.Logf("gate %s · files %d · raw %d = comments %d + hand-own %d + declaration %d + call sites %d",
		darwinGateName, scan.filesScanned, scan.rawMatches, scan.inComment, scan.inHandOwn, scan.declarations, scan.callSites)
	boxParams := 0

	for _, site := range scan.hazards {
		boxParams += len(site.pointees)
	}

	t.Logf("call sites %d = reference-bearing %d + clear %d · reference-bearing PARAMETERS %d · declared %d (named by libccall_impl.cs's pending list %d · named only here %d) · unresolved %d",
		scan.callSites, len(scan.hazards), scan.clear, boxParams, len(declaredLibcCalls), namedByHeader, len(declaredLibcCalls)-namedByHeader, len(scan.unresolved))

	for _, site := range scan.hazards {
		t.Logf("  HAZARD-DEFERRED %s:%d %s", site.file, site.line, site.row)
	}
}

// TestRealizationBucketReadsBothBraceStyles is the control on the gate test's own predicate. The
// first spelling asked whether the declaration LINE ends with `{`; that is true of every K&R
// declaration the corpus carries and FALSE of an Allman one, which is a style a hand-own file --
// the one file class this guard exists to read -- may well be written in. A false RED on the gate
// would be read as "darwin's sites went behind a throw", the opposite of what it means.
func TestRealizationBucketReadsBothBraceStyles(t *testing.T) {
	for _, plant := range []struct {
		name  string
		lines []string
		want  string
		why   string
	}{
		{
			name:  "K&R",
			lines: []string{"internal static int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg) {", "    return 0;", "}"},
			want:  realizationBody,
			why:   "the corpus's own style, and the real gate's shape at libccall_impl.cs:112",
		},
		{
			name:  "Allman",
			lines: []string{"internal static int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg)", "{", "    return 0;", "}"},
			want:  realizationBody,
			why:   "a hand-written file may open the block on the next line; this is the arm the first predicate failed",
		},
		{
			name:  "signature spanning lines, K&R",
			lines: []string{"internal static int32 libcCall(@unsafe.Pointer fn,", "    @unsafe.Pointer arg) {", "    return 0;", "}"},
			want:  realizationBody,
			why:   "the closing paren is not on the declaration's first line",
		},
		{
			name:  "bodyless, nothing realizes it",
			lines: []string{"internal static partial int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg);"},
			want:  realizationNone,
			why:   "PartialStubGenerator realizes this as a throw — the windows asmstdcall shape",
		},
		{
			name:  "bodyless, interop",
			lines: []string{"[LibraryImport(\"libSystem.B.dylib\")]", "internal static partial int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg);"},
			want:  realizationInterop,
			why:   "the interop source generator realizes it; bodyless is not the same as unrealized",
		},
		{
			name:  "bodyless, interop, fully qualified",
			lines: []string{"[System.Runtime.InteropServices.DllImport(\"libSystem.B.dylib\")]", "internal static extern int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg);"},
			want:  realizationInterop,
			why:   "the attribute is not always written short",
		},
	} {
		start := -1

		for i, line := range plant.lines {
			if libcCallDeclPattern.MatchString(line) {
				start = i
				break
			}
		}

		if start < 0 {
			t.Errorf("libcCallDeclPattern matches no line of the %s plant\n%s\n"+
				"the bucket arm would then read from line 0 and pass or fail on the wrong line; a control "+
				"that silently picks a different subject is not a control", plant.name, plant.why)

			continue
		}

		if got := realizationBucket(plant.lines, start); got != plant.want {
			t.Errorf("%s reads %s, want %s\n%s", plant.name, got, plant.want, plant.why)
		}
	}
}

// TestDarwinLibcCallScannerFires is the control: the same scanner over a synthetic tracked tree. A
// plant proves the predicate CAN fire; the all-scalar pointee and the real corpus's 31 clear sites
// are what prove it is a RULE.
func TestDarwinLibcCallScannerFires(t *testing.T) {
	root := t.TempDir()

	write := func(rel, content string) {
		full := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		// Written CRLF, as the corpus is committed: a scanner that only reads LF reports a clean tree.
		if err := os.WriteFile(full, []byte(strings.ReplaceAll(content, "\n", "\r\n")), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	// The pointee declarations, reproduced with the fields their real counterparts carry. Opaque is
	// pthreadattr's shape exactly: reference-bearing ONLY through an INITIALIZED array field.
	write("src/core/runtime/darwin/defs.cs", `[GoType] partial struct Plain {
    internal ж<byte> ss_sp;
    internal uintptr ss_size;
}

[GoType] partial struct Opaque {
    public int64 X__sig;
    public array<int8> X__opaque = new(56);
}

[GoType] partial struct Inner {
    internal int64 tv_sec;
    internal array<byte> pad_cgo_0 = new(4);
}

[GoType] partial struct Nested {
    internal Inner it_interval;
    internal Inner it_value;
}

[GoType] partial struct Scalar {
    internal int64 tv_sec;
    internal int64 tv_nsec;
}
`)

	write("src/core/runtime/darwin/sys.cs", `internal static int32 libcCall(@unsafe.Pointer fn, @unsafe.Pointer arg) {
    return 0;
}

internal static void plantPlain(ж<Plain> Ꮡp) {
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(plain_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static void plantInitialized(ж<Opaque> Ꮡo, nint state) {
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(opaque_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static void plantNested(int32 mode, ж<Nested> Ꮡnew, ж<Nested> Ꮡold) {
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(nested_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static void plantUnresolved(ж<NoSuchStruct> Ꮡu) {
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(unresolved_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static int32 knownNegative(ж<Scalar> Ꮡts, ж<byte> Ꮡname, uintptr len) {
    return libcCall((@unsafe.Pointer)abi.FuncPCABI0(scalar_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}

internal static void lineComment(ж<Plain> Ꮡp) {
    // libcCall((@unsafe.Pointer)abi.FuncPCABI0(commented_trampoline), nil);
    return;
}

/*
internal static void blockComment(ж<Plain> Ꮡp) {
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(blocked_trampoline), nil);
}
*/

internal static void trailingComment(ж<Plain> Ꮡp) {
    var x = 1; // see libcCall(fn, arg) for the dispatch shape
}
`)

	// The hand-own arm: the same hazard text in a file declaring the marker must be COUNTED and not
	// GATED, so the two populations stay separable.
	write("src/core/runtime/darwin/handown_impl.cs", `[module: go.GoManualConversion]

internal static void handOwnPlant(ж<Plain> Ꮡp) {
    libcCall((@unsafe.Pointer)abi.FuncPCABI0(handown_trampoline), @unsafe.Pointer.FromPinnedBox(Ꮡargs));
}
`)

	tracked := []string{
		"src/core/runtime/darwin/defs.cs",
		"src/core/runtime/darwin/sys.cs",
		"src/core/runtime/darwin/handown_impl.cs",
	}

	scan := scanLibcCalls(t, root, tracked)

	want := map[string]string{
		"plantPlain":       "ж<Plain>",
		"plantInitialized": "ж<Opaque>",
		"plantNested":      "ж<Nested>, ж<Nested>",
	}

	got := map[string]string{}

	for _, site := range scan.hazards {
		got[site.member] = strings.Join(site.pointees, ", ")
	}

	for member, pointees := range want {
		reading, found := got[member]

		if !found {
			t.Errorf("the scanner did not report %s\n"+
				"a plant the predicate cannot fire on means every zero this guard reads is unproven", member)

			continue
		}

		if reading != pointees {
			t.Errorf("the scanner read %s's pointees as %q, want %q", member, reading, pointees)
		}
	}

	for member := range got {
		if _, expected := want[member]; !expected {
			t.Errorf("the scanner reported %s -> %q, which is NOT in the class\n"+
				"a predicate that fires on a clear site inflates the real corpus's fourteen", member, got[member])
		}
	}

	if _, fired := got["knownNegative"]; fired {
		t.Error("the scanner reported knownNegative, whose pointees are an all-scalar struct and a byte\n" +
			"this is the KNOWN NEGATIVE: ж<Scalar> and ж<byte> have the same layout managed or native, " +
			"and a predicate that cannot tell them from ж<Opaque> is not measuring layout")
	}

	if len(scan.unresolved) != 1 || !strings.Contains(scan.unresolved[0], "NoSuchStruct") {
		t.Errorf("unresolved reads %v, want exactly the NoSuchStruct site\n"+
			"a pointee resolving nowhere must be REPORTED; assuming it benign is how a census invents a verdict", scan.unresolved)
	}

	// Four plants + the known negative + the trailing-comment member's own body: the commented calls
	// are not sites, and the hand-own's call is counted apart.
	if scan.callSites != 5 {
		t.Errorf("call sites read %d, want 5 (four plants and the known negative)\n"+
			"raw %d = comments %d + hand-own %d + declaration %d + sites %d",
			scan.callSites, scan.rawMatches, scan.inComment, scan.inHandOwn, scan.declarations, scan.callSites)
	}

	if scan.inComment != 3 {
		t.Errorf("comment-suppressed reads %d, want 3 (a line comment, a block comment and a trailing comment)\n"+
			"hazard text inside a comment is not a site, and a scanner that counts it reports a population "+
			"that does not exist", scan.inComment)
	}

	if scan.inHandOwn != 1 {
		t.Errorf("hand-own reads %d, want 1\n"+
			"a hand-own's call is a human's deliberate choice: it is COUNTED so the number is visible and "+
			"NOT GATED so two populations are not folded into one", scan.inHandOwn)
	}

	if scan.declarations != 1 {
		t.Errorf("declarations read %d, want 1\n"+
			"⚠ this is the 46-vs-45 correction in miniature: the gate's own signature matches a bare "+
			"`libcCall(` search, and counting it as a site folds the callee into the callers", scan.declarations)
	}

	if len(scan.noMember) != 0 {
		t.Errorf("call sites with no readable enclosing member: %v; the control's members are all "+
			"single-line declarations, so any hit here is the walk-back itself failing", scan.noMember)
	}

	if scan.clear != 1 {
		t.Errorf("clear reads %d, want 1 (knownNegative alone)\n"+
			"the unresolved plant is NOT clear -- counting it clear is exactly the assumption this "+
			"guard refuses", scan.clear)
	}
}
