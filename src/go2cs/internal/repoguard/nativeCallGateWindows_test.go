// nativeCallGateWindows_test.go - Gbtc
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
	"path"
	"path/filepath"
	"regexp"
	"slices"
	"sort"
	"strings"
	"testing"
)

// WHAT THIS WATCHES, AND WHY IT IS A GATE AND NOT A CENSUS.
//
// The converted Windows runtime reaches the kernel through one funnel. Every stdcall0..stdcall12
// delegates to stdcall(fn) (runtime/windows/os_windows.cs:959), whose only outward call is
// asmcgocall(asmstdcallAddr, …) at :973, and asmstdcall is Go's assembly -- declared bodyless at
// os_windows.cs:199, realized nowhere in src/core, and filled by PartialStubGenerator
// (src/gen/go2cs-gen/PartialStubGenerator.cs:111) with
//
//	throw new NotImplementedException("<identifier>: no implementation reached this compilation …")
//
// so every one of the 102 stdcallN call sites in runtime/windows is dead behind it. Three hand-own
// headers already state that consequence in words: runtime/windows/os_windows_impl.cs:19 and :95,
// runtime/windows/signal_windows_impl.cs:27, and internal/poll/windows/runtime_netpoll_impl.cs:16.
//
// That is what makes a per-site census the wrong instrument here. C1's q90 step 1 (mailbox 7a181d2a0)
// took the reachability reading COORD's ruling asks for BEFORE any census: runtime/windows
// os_windows.cs profilem() builds a ж<context> over a byte array and hands it to _GetThreadContext --
// textbook native-boundary fork (ii) -- and it is REFERENCED but UNREACHABLE, with five throwing gates
// between runtime/pprof's StartCPUProfile and the box. A declared set of 102 rows would repeat one
// sentence 102 times, every row would go live or stay dead together, and keeping it in step with the
// corpus would be the drifted-instrument-copy fault by construction. So the population is watched
// through its GATE, and the day the gate is realized this guard goes red and says the per-site census
// is owed. COORD ruled that shape at mailbox abf5362ea.
//
// TWO EXCEPTIONS THIS GUARD CARRIES IN ITS OWN TEXT, because the short version of its finding is
// wrong in both directions:
//
//	A REALIZED SYMBOL CAN STILL BE DEAD. write1 (os_windows.cs:549) has a real body and its first act
//	is stdcall1(_GetStdHandle, …). It is dead BY THE GATE. A guard scoring symbols rather than the
//	gate would have called it live.
//
//	THE GATE IS NOT THE ONLY DOOR. signal_windows_impl.cs:75 declares SetConsoleCtrlHandlerNative as
//	[LibraryImport("kernel32.dll")] -- a LIVE native call that never touches asmstdcall. So this guard
//	prints the interop sites as a reading and its red says "every stdcallN site is dead behind this
//	gate", NEVER "nothing in runtime/windows reaches the kernel".
//
// THE THREE BUCKETS, and why there are three. A realization census that asks only "does a body exist"
// is wrong in both directions, and C1's own first census of this was wrong BOTH ways in one run
// (mailbox 83c8d047f §5): a TUPLE RETURN TYPE breaks a pattern that forbids parentheses before the
// name -- `public static partial (uintptr r1, uintptr r2, uintptr errno) Syscall6(…)` -- and a
// [LibraryImport] / [DllImport] partial is BODYLESS BY DESIGN, realized by the interop source
// generator rather than by PartialStubGenerator's throw. Reading "no body" as "throws" inverts the
// verdict. So a declaration resolves into exactly one of:
//
//	BODY     a partial declaration whose own text opens a block
//	INTEROP  a bodyless partial carrying [LibraryImport] or [DllImport] -- realized by the generator
//	NONE     bodyless with neither -- PartialStubGenerator's throw, which is what "dead" means here
//
// The control plants an interop realization precisely because that bucket is the one C1's fault could
// not see; a two-bucket scanner passes this guard's corpus arm and fails that plant.
//
// WHAT IS ASSERTED, AND WHAT IS ONLY PRINTED. Asserted: asmstdcall is declared, bodyless, and
// unrealized in both buckets. Printed as readings and asserted by nothing -- the stdcallN call-site
// count and the interop declarations -- because they move with ordinary corpus work and a guard that
// asserts a population it was asked to observe becomes a chore rather than a watch.
//
// RETIREMENT: the ж-box arc's native box kind for a reference-bearing pointee at a native boundary,
// post-hop, which is the same exit F3's disclosure names. Until then this gate is what keeps the
// runtime's fork (ii) sites honestly classified as dead rather than silently assumed so.
//
// THE OTHER TWO FLAVOURS ARE NOT THIS SHAPE and are deliberately absent: darwin's gate (libcCall,
// runtime/darwin/libccall_impl.cs:99) IS realized over golib's dispatcher, so its 46 sites are LIVE
// and owed a per-site census; linux has no single gate at all -- per-symbol stubs, six realized by
// hand-own bodies and thirty-nine not, plus DISPLACEMENT, where a stub stays bodyless forever because
// its callers were replaced (futex behind runtime/linux/lock_futex_impl.cs). Each gets its own
// instrument; one guard spanning three mechanisms would rest each arm on a different reading and fail
// as one. Measured in q90 step 2, mailbox 83c8d047f.

// gateDeclPattern matches a partial member declaration by name. The parameter list must HUG the name
// (a converted signature can open with a TUPLE return type, so a pattern that stops at the first
// `(` reads the member's name as `static`), and the pattern deliberately does NOT forbid parentheses
// earlier in the line, which is the other half of the same fault.
func gateDeclPattern(name string) *regexp.Regexp {
	return regexp.MustCompile(`^\s*(?:\[[^\]]*\]\s*)*(?:internal|public|private|protected)\s+(?:static\s+)?(?:unsafe\s+)?partial\s+.*?\b` + regexp.QuoteMeta(name) + `(?:<[^>()]*>)?\s*\(`)
}

// The interop rule -- the two attributes that make a bodyless partial REALIZED by a source
// generator rather than stubbed with a throw -- is ONE definition for both gate watches:
// interopRealizationPattern in nativeCallGateDarwin_test.go, same package. This file carried a
// second, NARROWER spelling of the same rule until the fold; the note on that declaration keeps
// what the two disagreed on and why the wider spelling is the one kept.

// stdcallCallPattern matches a call to the stdcall family. The declarations themselves are excluded
// by the reading, so this counts CALL SITES.
var stdcallCallPattern = regexp.MustCompile(`\bstdcall[0-9]{0,2}\s*\(`)

// windowsRuntimeTarget is the flavour this guard watches, and windowsRuntimeTree names the runtime folders its build
// compiles other than the flat one: windows/, amd64/ and windows-amd64/.
const windowsRuntimeTarget = "windows/amd64"

var windowsRuntimeTree = "src/core/runtime/{" + strings.Join(layoutKeyFoldersServing(windowsRuntimeTarget), ",") + "}/"

// windowsRuntimeTreeFile reports whether a tracked path is a .cs file of the windows runtime tree.
func windowsRuntimeTreeFile(rel string) bool {
	name, ok := strings.CutPrefix(path.Dir(rel), "src/core/runtime/")

	return ok && strings.HasSuffix(rel, ".cs") && slices.Contains(lineKeyedFolderTargets(name), windowsRuntimeTarget)
}

// nativeCallGate is the one declaration a flavour's kernel access funnels through.
type nativeCallGate struct {
	name string
	why  string
}

// windowsNativeCallGate is the gate, and the only thing this file asserts about.
var windowsNativeCallGate = nativeCallGate{
	name: "asmstdcall",
	why: "every stdcall0..stdcall12 delegates to stdcall(fn), whose only outward call is " +
		"asmcgocall(asmstdcallAddr, …); asmstdcall is Go's assembly and has no converted form",
}

type gateResolution struct {
	declarations []string // "<file>:<line>", relative to src/core
	bodies       []string
	interop      []string
}

func (resolution gateResolution) realized() bool {
	return len(resolution.bodies) > 0 || len(resolution.interop) > 0
}

type gateScan struct {
	filesScanned    int
	gate            gateResolution
	sibling         gateResolution
	stdcallSites    int
	stdcallFiles    []string
	interopSites    []string
	interopDeclared int
}

// readGateText reads a tracked file and normalises line endings. The corpus is committed CRLF and a
// pattern written with \n silently matches nothing against it.
func readGateText(t *testing.T, root, rel string) string {
	t.Helper()

	raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

	if err != nil {
		t.Fatalf("cannot read tracked file %s: %v", rel, err)
	}

	return strings.ReplaceAll(string(raw), "\r\n", "\n")
}

// resolveGate finds every declaration of a name across the tracked tree and sorts each into exactly
// one of the three buckets.
func resolveGate(t *testing.T, root string, tracked []string, name string) gateResolution {
	t.Helper()

	pattern := gateDeclPattern(name)
	resolution := gateResolution{}

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		lines := strings.Split(readGateText(t, root, rel), "\n")

		for n, line := range lines {
			trimmed := strings.TrimSpace(line)

			// A declaration quoted in a comment is not a declaration. The census that found this
			// gate's own siblings had to learn the same lesson about a module marker quoted in a
			// generated file's placeholder prose.
			if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*") {
				continue
			}

			if !pattern.MatchString(line) {
				continue
			}

			where := fmt.Sprintf("%s:%d", strings.TrimPrefix(rel, "src/core/"), n+1)
			resolution.declarations = append(resolution.declarations, where)

			// The declaration's own text decides BODY vs the other two: a block opened before any
			// semicolon is a body.
			semicolon := strings.Index(line, ";")
			brace := strings.Index(line, "{")

			if brace >= 0 && (semicolon < 0 || brace < semicolon) {
				resolution.bodies = append(resolution.bodies, where)
				continue
			}

			window := strings.Join(lines[max(0, n-4):n+1], "\n")

			if interopRealizationPattern.MatchString(window) {
				resolution.interop = append(resolution.interop, where)
			}
		}
	}

	sort.Strings(resolution.declarations)
	sort.Strings(resolution.bodies)
	sort.Strings(resolution.interop)

	return resolution
}

// scanWindowsGate resolves the gate and takes the two readings beside it.
func scanWindowsGate(t *testing.T, root string, tracked []string) gateScan {
	t.Helper()

	scan := gateScan{
		gate:    resolveGate(t, root, tracked, windowsNativeCallGate.name),
		sibling: resolveGate(t, root, tracked, "asmstdcall_trampoline"),
	}

	files := map[string]bool{}

	for _, rel := range tracked {
		if !windowsRuntimeTreeFile(rel) {
			continue
		}

		scan.filesScanned++
		lines := strings.Split(readGateText(t, root, rel), "\n")
		short := strings.TrimPrefix(rel, "src/core/")

		for n, line := range lines {
			trimmed := strings.TrimSpace(line)

			if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*") {
				continue
			}

			if interopRealizationPattern.MatchString(line) {
				scan.interopDeclared++
				scan.interopSites = append(scan.interopSites, fmt.Sprintf("%s:%d", short, n+1))
			}

			if !stdcallCallPattern.MatchString(line) {
				continue
			}

			// The stdcall family's own declarations are not call sites.
			if regexp.MustCompile(`^\s*(?:internal|public|private|protected)\s+static\s+\S+\s+stdcall[0-9]{0,2}\s*\(`).MatchString(line) {
				continue
			}

			scan.stdcallSites++
			files[short] = true
		}
	}

	for file := range files {
		scan.stdcallFiles = append(scan.stdcallFiles, file)
	}

	sort.Strings(scan.stdcallFiles)
	sort.Strings(scan.interopSites)

	return scan
}

// TestWindowsNativeCallGateIsUnrealized is the guard.
func TestWindowsNativeCallGateIsUnrealized(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	scan := scanWindowsGate(t, root, tracked)

	if scan.filesScanned < 10 || scan.stdcallSites < 50 {
		t.Fatalf("VACUOUS: %d files in %s, %d stdcallN call sites; the scan is measuring nothing",
			scan.filesScanned, windowsRuntimeTree, scan.stdcallSites)
	}

	// READINGS. Printed, never asserted: both move with ordinary corpus work, and a guard that
	// asserts a population it was asked to observe becomes a chore rather than a watch.
	t.Logf("READING · stdcallN call sites in %s: %d over %d files %v",
		windowsRuntimeTree, scan.stdcallSites, len(scan.stdcallFiles), scan.stdcallFiles)
	t.Logf("READING · LibraryImport/DllImport declarations in %s: %d %v — LIVE native calls that do NOT pass the gate",
		windowsRuntimeTree, scan.interopDeclared, scan.interopSites)
	t.Logf("READING · the gate's sibling asmstdcall_trampoline: declarations %v · bodies %v · interop %v",
		scan.sibling.declarations, scan.sibling.bodies, scan.sibling.interop)

	// THE ASSERTION.
	if len(scan.gate.declarations) == 0 {
		t.Fatalf("NO DECLARATION OF %s FOUND ANYWHERE IN src/core.\n"+
			"This guard cannot report the gate dead when it cannot find the gate: the declaration may have been "+
			"renamed, moved or commented out, and a scan that finds nothing is not a scan that found nothing wrong",
			windowsNativeCallGate.name)
	}

	t.Logf("gate %s · declarations %v · bodies %v · interop %v",
		windowsNativeCallGate.name, scan.gate.declarations, scan.gate.bodies, scan.gate.interop)

	if scan.gate.realized() {
		t.Errorf("THE WINDOWS NATIVE-CALL GATE IS REALIZED: %s now has %d body/bodies %v and %d interop declaration(s) %v.\n"+
			"Why this matters: %s. While it was unrealized, EVERY stdcallN call site in %s was dead behind it — %d of them "+
			"today — and the native-boundary fork (ii) sites in the converted runtime were dead with them. They are now "+
			"REACHABLE, so the per-site census is OWED for this flavour: each site classified HAZARD (a ж<T> over a "+
			"reference-bearing T crossing to the kernel, read through) or DISCLOSED-INERT (in the class, never read "+
			"through), with a reach reading per disclosed member, exactly as q86 and F3 do for syscall.\n"+
			"⚠ This red does NOT mean native calls just started working in %s: %d LibraryImport/DllImport declaration(s) "+
			"there were already live by a different mechanism and never passed this gate.",
			windowsNativeCallGate.name, len(scan.gate.bodies), scan.gate.bodies, len(scan.gate.interop), scan.gate.interop,
			windowsNativeCallGate.why, windowsRuntimeTree, scan.stdcallSites, windowsRuntimeTree, scan.interopDeclared)
	}
}

// TestWindowsNativeCallGateScannerFires is the control: the same scanner over synthetic tracked trees,
// one per bucket, plus the two negatives.
func TestWindowsNativeCallGateScannerFires(t *testing.T) {
	write := func(root, rel, content string) {
		full := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		// Written CRLF, as the corpus is committed: a scanner that only reads LF reports a clean tree.
		if err := os.WriteFile(full, []byte(strings.ReplaceAll(content, "\n", "\r\n")), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	const rel = "src/core/runtime/windows/os_windows.cs"

	// (a) RED-FIRST: the gate given a body.
	bodyRoot := t.TempDir()
	write(bodyRoot, rel, `internal static partial void asmstdcall(@unsafe.Pointer fn) {
    DoTheCallForReal(fn);
}
`)

	body := resolveGate(t, bodyRoot, []string{rel}, "asmstdcall")

	if len(body.declarations) != 1 || len(body.bodies) != 1 || len(body.interop) != 0 {
		t.Fatalf("control (a): a gate WITH A BODY read declarations %v bodies %v interop %v; want 1/1/0",
			body.declarations, body.bodies, body.interop)
	}

	if !body.realized() {
		t.Fatal("control (a): a gate with a body must read REALIZED")
	}

	// (b) THE KNOWN POSITIVE FOR THE THIRD BUCKET: interop IS a realization. This is the bucket C1's
	// own first census could not see, and a two-bucket scanner passes the corpus arm and fails here.
	interopRoot := t.TempDir()
	write(interopRoot, rel, `[LibraryImport("kernel32.dll", EntryPoint = "AsmStdCall", SetLastError = true)]
internal static partial void asmstdcall(@unsafe.Pointer fn);
`)

	interop := resolveGate(t, interopRoot, []string{rel}, "asmstdcall")

	if len(interop.declarations) != 1 || len(interop.bodies) != 0 || len(interop.interop) != 1 {
		t.Fatalf("control (b): a gate declared [LibraryImport] read declarations %v bodies %v interop %v; want 1/0/1",
			interop.declarations, interop.bodies, interop.interop)
	}

	if !interop.realized() {
		t.Fatal("control (b): a [LibraryImport] partial is REALIZED by the interop generator, not stubbed with a throw")
	}

	// (c) THE KNOWN NEGATIVE, from the real corpus's shape: the gate bodyless with no attribute, and an
	// UNRELATED interop partial beside it. A scanner that reads the neighbour's attribute as the gate's
	// reports the corpus green-then-red for no reason.
	deadRoot := t.TempDir()
	write(deadRoot, rel, `// Call a Windows function with stdcall conventions,
// and switch to os stack during the call.
internal static partial void asmstdcall(@unsafe.Pointer fn);

internal static @unsafe.Pointer asmstdcallAddr;

[LibraryImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler", SetLastError = true)]
private static unsafe partial int SetConsoleCtrlHandlerNative(void* handler, int add);
`)

	dead := resolveGate(t, deadRoot, []string{rel}, "asmstdcall")

	if len(dead.declarations) != 1 || len(dead.bodies) != 0 || len(dead.interop) != 0 {
		t.Fatalf("control (c): the real corpus shape read declarations %v bodies %v interop %v; want 1/0/0",
			dead.declarations, dead.bodies, dead.interop)
	}

	if dead.realized() {
		t.Fatal("control (c): a bodyless partial with no interop attribute is the DEAD bucket")
	}

	// (d) A declaration that exists only inside a comment is NOT a declaration. The guard must fatal on
	// finding none rather than reporting the gate dead, which is the difference between "nothing found"
	// and "nothing wrong".
	commentRoot := t.TempDir()
	write(commentRoot, rel, `// internal static partial void asmstdcall(@unsafe.Pointer fn);
// stdcall bottoms out in asmstdcall, a throwing stub.
`)

	commented := resolveGate(t, commentRoot, []string{rel}, "asmstdcall")

	if len(commented.declarations) != 0 {
		t.Fatalf("control (d): a declaration quoted in a comment read %v; want none", commented.declarations)
	}

	// The readings, over the dead tree: the scanner must count the neighbour's interop declaration and
	// must not count the stdcall family's own declarations as call sites.
	readingRoot := t.TempDir()
	write(readingRoot, rel, `internal static partial void asmstdcall(@unsafe.Pointer fn);

[LibraryImport("kernel32.dll", EntryPoint = "SetConsoleCtrlHandler")]
private static unsafe partial int SetConsoleCtrlHandlerNative(void* handler, int add);

internal static uintptr stdcall1(stdFunction fn, uintptr a0) {
    return stdcall(fn);
}

internal static void caller() {
    stdcall2(_GetThreadContext, thread, (uintptr)c);
    stdcall7(_DuplicateHandle, a, b, c, d, e, f, g);
}
`)

	reading := scanWindowsGate(t, readingRoot, []string{rel})

	// stdcall(fn) inside stdcall1's body, plus stdcall2 and stdcall7 — three calls; stdcall1's own
	// declaration is not one.
	if reading.stdcallSites != 3 {
		t.Fatalf("control: the stdcallN reading counted %d call sites, want 3 (the family's own declaration is not a call)", reading.stdcallSites)
	}

	if reading.interopDeclared != 1 {
		t.Fatalf("control: the interop reading counted %d declarations, want 1", reading.interopDeclared)
	}

	// The windows runtime tree is every runtime folder a windows/amd64 build compiles. A stdcall site planted in
	// runtime/amd64/ and one in runtime/windows-amd64/ are read; one in runtime/linux-amd64/ is not.
	keyedRoot := t.TempDir()
	keyedRels := []string{
		"src/core/runtime/amd64/stubs_amd64.cs",
		"src/core/runtime/linux-amd64/sys_linux_amd64.cs",
		"src/core/runtime/windows-amd64/sys_windows_amd64.cs",
	}

	for _, keyedRel := range keyedRels {
		write(keyedRoot, keyedRel, "internal static void caller() {\n    stdcall1(_GetThreadContext, thread);\n}\n")
	}

	if keyed := scanWindowsGate(t, keyedRoot, keyedRels); keyed.filesScanned != 2 || keyed.stdcallSites != 2 {
		t.Errorf("control: one stdcall site in each of %v read %d files and %d sites, want 2 and 2 (a windows/amd64 "+
			"build compiles runtime/amd64/ and runtime/windows-amd64/, never runtime/linux-amd64/)",
			keyedRels, keyed.filesScanned, keyed.stdcallSites)
	}
}
