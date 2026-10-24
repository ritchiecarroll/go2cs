// rootFrameLines_test.go - Gbtc
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
	"strconv"
	"strings"
	"testing"
)

// A ROOT FRAME'S LINE IS THE PINNED GO RELEASE'S (COORD ruling of 2026-10-24 on R's crash-traceback seat).
//
// Some frames golib reports have no live method behind them: the goroutine root (runtime.goexit), the test-host
// root (testing.tRunner), the panic machinery splicePanic splices (runtime.gopanic and the fault frames under
// it), and the allocator frames a heap-profile record starts with. Their file:line positions are Go's, written
// into hand-owned code as literals, so they are a transcription of one Go release and a hop strands them
// silently: the gopanic line stayed go1.24.13's panic.go:792 after the tree moved to go1.26.10, where Go prints
// :860, in runtime.Callers and in every crash traceback.
//
// A converted program has no GOROOT to read at run time, so the lines stay literals and this guard derives each
// one from the GOROOT src/version.props pins (pinnedGoRoot): a declaration line, or a statement found by
// walking forward from it, and fails naming the derived line when a literal differs. There is no allow-list: every
// root line derives, so a hop that moves any of them fails here in full.
//
// POSITIVE CONTROL: TestRootFrameLineScannerFires drives the same scanner over planted sources, one per refusal,
// plus a clean control that must read empty.

// rootFramePin is one Go position written as a literal in hand-owned code.
type rootFramePin struct {
	name   string         // what the line is, as findings name it
	csFile string         // repo-relative file holding the literal
	cs     *regexp.Regexp // the literal: one capture group, and every match must capture the same number
	goFile string         // GOROOT/src-relative Go file
	steps  []string       // a declaration regexp, then each statement found after the previous step's line
}

const (
	managedImpl   = "src/core/runtime/managed_impl.cs"
	mprofImpl     = "src/core/runtime/mprof_impl.cs"
	testExecution = "src/core/testing/TestExecution.cs"
)

func rootPin(name, csFile, cs, goFile string, steps ...string) rootFramePin {
	return rootFramePin{name, csFile, regexp.MustCompile(cs), goFile, steps}
}

// mallocgcDispatch walks to an allocator call in mallocgc's DEFAULT dispatch: the else of
// `if sizeSpecializedMallocEnabled`, a GOEXPERIMENT that is off by default and whose branch calls the same
// allocators first, on other lines.
func mallocgcDispatch(call string) []string {
	return []string{`^func mallocgc\(`, `^\tif sizeSpecializedMallocEnabled \{`, `^\t\} else \{`, call}
}

var rootFramePins = []rootFramePin{
	// The goroutine root: goexit's declaration, and the NOP after it that a goroutine's root frame returns to.
	rootPin("runtime.goexit start", managedImpl, `"runtime\.goexit" => (\d+),`, "runtime/asm_amd64.s", `^TEXT runtime·goexit\(SB\)`),
	rootPin("runtime.goexit frame", managedImpl, `internRootFrame\("runtime\.goexit", "runtime/asm_amd64\.s", (\d+)\)`, "runtime/asm_amd64.s",
		`^TEXT runtime·goexit\(SB\)`, `^\s+BYTE\s+\$0x90`),

	// The test-host root: tRunner calls the test function.
	rootPin("testing.tRunner start", managedImpl, `"testing\.tRunner" => (\d+),`, "testing/testing.go", `^func tRunner\(`),
	rootPin("testing.tRunner frame", testExecution, `\[GoStackRoot\("testing\.tRunner", "testing/testing\.go", (\d+)\)\]`, "testing/testing.go",
		`^func tRunner\(`, `^\s*fn\(t\)\s*$`),

	// The panic machinery splicePanic splices, and the crash traceback's panic(...) frame.
	rootPin("runtime.gopanic start", managedImpl, `"runtime\.gopanic" => (\d+),`, "runtime/panic.go", `^func gopanic\(`),
	rootPin("runtime.gopanic frame", managedImpl, `private const int GopanicCallsDeferredLine = (\d+);`, "runtime/panic.go",
		`^func gopanic\(`, `^\s*fn\(\)\s*$`),
	rootPin("runtime.panicmem start", managedImpl, `"runtime\.panicmem" => (\d+),`, "runtime/panic.go", `^func panicmem\(\)`),
	rootPin("runtime.panicmem frame", managedImpl, `internRootFrame\("runtime\.panicmem", "runtime/panic\.go", (\d+)\)`, "runtime/panic.go",
		`^func panicmem\(\)`, `^\s*panic\(memoryError\)`),
	rootPin("runtime.panicdivide start", managedImpl, `"runtime\.panicdivide" => (\d+),`, "runtime/panic.go", `^func panicdivide\(\)`),
	rootPin("runtime.panicdivide frame", managedImpl, `internRootFrame\("runtime\.panicdivide", "runtime/panic\.go", (\d+)\)`, "runtime/panic.go",
		`^func panicdivide\(\)`, `^\s*panic\(divideError\)`),
	rootPin("runtime.panicwrap start", managedImpl, `"runtime\.panicwrap" => (\d+),`, "runtime/error.go", `^func panicwrap\(\)`),
	rootPin("runtime.panicwrap frame", managedImpl, `internRootFrame\("runtime\.panicwrap", "runtime/error\.go", (\d+)\)`, "runtime/error.go",
		`^func panicwrap\(\)`, `^\s*panic\(plainError\("value method `),
	rootPin("runtime.sigpanic start (windows)", managedImpl, `"runtime\.sigpanic" => GOOS == "windows"u8 \? (\d+) : \d+,`, "runtime/signal_windows.go",
		`^func sigpanic\(\)`),
	rootPin("runtime.sigpanic frame (windows)", managedImpl, `internRootFrame\("runtime\.sigpanic", "runtime/signal_windows\.go", (\d+)\)`, "runtime/signal_windows.go",
		`^func sigpanic\(\)`, `^\s*panicmem\(\)`),
	rootPin("runtime.sigpanic start (unix)", managedImpl, `"runtime\.sigpanic" => GOOS == "windows"u8 \? \d+ : (\d+),`, "runtime/signal_unix.go",
		`^func sigpanic\(\)`),
	// A nil dereference is a SIGSEGV: the panicmem under that case, not the SIGBUS one above it.
	rootPin("runtime.sigpanic frame (unix)", managedImpl, `internRootFrame\("runtime\.sigpanic", "runtime/signal_unix\.go", (\d+)\)`, "runtime/signal_unix.go",
		`^func sigpanic\(\)`, `^\s*case _SIGSEGV:`, `^\s*panicmem\(\)`),

	// The allocator frames a heap-profile record starts with (memProfileAlloc and mallocgcLine).
	rootPin("runtime.mallocgc start", managedImpl, `"runtime\.mallocgc" => (\d+),`, "runtime/malloc.go", `^func mallocgc\(`),
	rootPin("runtime.mallocgc frame (tiny)", mprofImpl, `return size < 16 \? (\d+) : \d+;`, "runtime/malloc.go",
		mallocgcDispatch(`mallocgcTiny\(`)...),
	rootPin("runtime.mallocgc frame (small noscan)", mprofImpl, `return size < 16 \? \d+ : (\d+);`, "runtime/malloc.go",
		mallocgcDispatch(`mallocgcSmallNoscan\(`)...),
	rootPin("runtime.mallocgc frame (small scan, no header)", mprofImpl, `return size <= 512 \? (\d+) : \d+;`, "runtime/malloc.go",
		mallocgcDispatch(`mallocgcSmallScanNoHeader\(`)...),
	rootPin("runtime.mallocgc frame (small scan, header)", mprofImpl, `return size <= 512 \? \d+ : (\d+);`, "runtime/malloc.go",
		mallocgcDispatch(`mallocgcSmallScanHeader\(`)...),
	rootPin("runtime.mallocgc frame (large)", mprofImpl, `if \(size > 32768 - 8\) \{\s*return (\d+);`, "runtime/malloc.go",
		mallocgcDispatch(`mallocgcLarge\(`)...),
	rootPin("runtime.newobject start", managedImpl, `"runtime\.newobject" => (\d+),`, "runtime/malloc.go", `^func newobject\(`),
	rootPin("runtime.newobject frame", mprofImpl, `internRootFrame\("runtime\.newobject", "runtime/malloc\.go", (\d+)\)`, "runtime/malloc.go",
		`^func newobject\(`, `mallocgc\(`),
	rootPin("runtime.newarray start", managedImpl, `"runtime\.newarray" => (\d+),`, "runtime/malloc.go", `^func newarray\(`),
	rootPin("runtime.newarray frame", mprofImpl, `internRootFrame\("runtime\.newarray", "runtime/malloc\.go", (\d+)\)`, "runtime/malloc.go",
		`^func newarray\(`, `mallocgc\(mem, typ, true\)`),
	rootPin("runtime.makeslice start", managedImpl, `"runtime\.makeslice" => (\d+),`, "runtime/slice.go", `^func makeslice\(`),
	rootPin("runtime.makeslice frame", mprofImpl, `internRootFrame\("runtime\.makeslice", "runtime/slice\.go", (\d+)\)`, "runtime/slice.go",
		`^func makeslice\(`, `mallocgc\(`),
	rootPin("runtime.growslice start", managedImpl, `"runtime\.growslice" => (\d+),`, "runtime/slice.go", `^func growslice\(`),
	rootPin("runtime.growslice frame (noscan)", mprofImpl, `"runtime\.growslice", "runtime/slice\.go", noscan \? (\d+) : \d+\)`, "runtime/slice.go",
		`^func growslice\(`, `mallocgc\(capmem, nil, false\)`),
	rootPin("runtime.growslice frame (scan)", mprofImpl, `"runtime\.growslice", "runtime/slice\.go", noscan \? \d+ : (\d+)\)`, "runtime/slice.go",
		`^func growslice\(`, `mallocgc\(capmem, et, true\)`),
	rootPin("runtime.moveSlice frame", mprofImpl, `internRootFrame\("runtime\.moveSlice", "runtime/slice\.go", (\d+)\)`, "runtime/slice.go",
		`^func moveSlice\(`, `mallocgc\(`),
	rootPin("runtime.moveSliceNoScan frame", mprofImpl, `internRootFrame\("runtime\.moveSliceNoScan", "runtime/slice\.go", (\d+)\)`, "runtime/slice.go",
		`^func moveSliceNoScan\(`, `mallocgc\(`),
	rootPin("runtime.moveSliceNoCap frame", mprofImpl, `internRootFrame\("runtime\.moveSliceNoCap", "runtime/slice\.go", (\d+)\)`, "runtime/slice.go",
		`^func moveSliceNoCap\(`, `mallocgc\(`),
	rootPin("runtime.moveSliceNoCapNoScan frame", mprofImpl, `internRootFrame\("runtime\.moveSliceNoCapNoScan", "runtime/slice\.go", (\d+)\)`, "runtime/slice.go",
		`^func moveSliceNoCapNoScan\(`, `mallocgc\(`),
}

// goTopLevelDecl ends the function a derivation walks: no step may match past the next top-level declaration.
var goTopLevelDecl = regexp.MustCompile(`^(func |TEXT |type |var |const )`)

// deriveGoLine walks a Go source's lines: the first step finds the declaration, and each later step the first
// line after the previous one that matches it, inside the same declaration and outside a // comment.
func deriveGoLine(lines []string, steps []string) (int, error) {
	at := -1

	for s, step := range steps {
		re := regexp.MustCompile(step)
		found := -1

		for i := at + 1; i < len(lines); i++ {
			if s > 0 && goTopLevelDecl.MatchString(lines[i]) {
				break
			}

			if strings.HasPrefix(strings.TrimSpace(lines[i]), "//") {
				continue
			}

			if re.MatchString(lines[i]) {
				found = i
				break
			}
		}

		if found < 0 {
			if s == 0 {
				return 0, fmt.Errorf("no line matches the declaration %q", step)
			}

			return 0, fmt.Errorf("no line after line %d matches %q before the declaration ends", at+1, step)
		}

		at = found
	}

	return at + 1, nil
}

// pinnedLiteral reads a pin's literal: the regexp must match, and every match must capture the same number.
func pinnedLiteral(content string, cs *regexp.Regexp) (int, error) {
	matches := cs.FindAllStringSubmatch(content, -1)

	if len(matches) == 0 {
		return 0, fmt.Errorf("no literal matches %q", cs)
	}

	value := matches[0][1]

	for _, m := range matches[1:] {
		if m[1] != value {
			return 0, fmt.Errorf("the literal %q captures both %s and %s", cs, value, m[1])
		}
	}

	return strconv.Atoi(value)
}

// scanRootFrameLines checks every pin against the Go sources readGo returns. It returns one finding per
// violation.
func scanRootFrameLines(pins []rootFramePin, readCS func(string) (string, error), readGo func(string) ([]string, error)) (findings []string) {
	for _, pin := range pins {
		content, err := readCS(pin.csFile)

		if err != nil {
			findings = append(findings, fmt.Sprintf("%s: cannot read %s: %v", pin.name, pin.csFile, err))
			continue
		}

		pinned, err := pinnedLiteral(content, pin.cs)

		if err != nil {
			findings = append(findings, fmt.Sprintf("%s: %s: %v", pin.name, pin.csFile, err))
			continue
		}

		lines, err := readGo(pin.goFile)

		if err != nil {
			findings = append(findings, fmt.Sprintf("%s: the Go file %s cannot be read at the pinned GOROOT: %v", pin.name, pin.goFile, err))
			continue
		}

		derived, err := deriveGoLine(lines, pin.steps)

		if err != nil {
			findings = append(findings, fmt.Sprintf("%s: %s at the pinned GOROOT: %v", pin.name, pin.goFile, err))
			continue
		}

		if pinned != derived {
			findings = append(findings, fmt.Sprintf("%s: %s pins %s:%d, but the pinned GOROOT has it at %s:%d; re-pin the literal",
				pin.name, pin.csFile, pin.goFile, pinned, pin.goFile, derived))
		}
	}

	sort.Strings(findings)
	return findings
}

// TestRootFrameLinesMatchThePinnedGoRoot is the guard.
func TestRootFrameLinesMatchThePinnedGoRoot(t *testing.T) {
	root := repoRootFromPackageDir(t)
	goRoot := pinnedGoRoot(t, root)

	readCS := func(file string) (string, error) {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(file)))
		return strings.ReplaceAll(string(data), "\r\n", "\n"), err
	}

	readGo := func(goFile string) ([]string, error) {
		data, err := os.ReadFile(filepath.Join(goRoot, "src", filepath.FromSlash(goFile)))

		if err != nil {
			return nil, err
		}

		return strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n"), nil
	}

	for _, finding := range scanRootFrameLines(rootFramePins, readCS, readGo) {
		t.Error(finding)
	}

	t.Logf("%d root frame lines checked against GOROOT %s", len(rootFramePins), goRoot)
}

// TestRootFrameLineScannerFires plants one violation per rule and a clean control.
func TestRootFrameLineScannerFires(t *testing.T) {
	goSource := []string{
		"package p",              // 1
		"",                       // 2
		"func f() {",             // 3
		"\t// fn() in a comment", // 4
		"\tfn()",                 // 5
		"}",                      // 6
		"",                       // 7
		"func g() {",             // 8
		"\tfn()",                 // 9
		"}",                      // 10
	}

	readGo := func(string) ([]string, error) { return goSource, nil }

	pins := func(cs string) func(string) (string, error) {
		return func(string) (string, error) { return cs, nil }
	}

	pinF := []rootFramePin{
		rootPin("f start", "x.cs", `"f" => (\d+),`, "p/p.go", `^func f\(`),
		rootPin("f frame", "x.cs", `frame\("f", (\d+)\)`, "p/p.go", `^func f\(`, `^\s*fn\(\)`),
	}

	cases := []struct {
		name string
		pins []rootFramePin
		cs   string
		want string
	}{
		{"clean", pinF, `"f" => 3, frame("f", 5) frame("f", 5)`, ""},
		{"a line moved", pinF, `"f" => 3, frame("f", 4)`, "re-pin the literal"},
		{"a start line moved", pinF, `"f" => 2, frame("f", 5)`, "re-pin the literal"},
		{"two different literals", pinF, `"f" => 3, frame("f", 5) frame("f", 6)`, "captures both"},
		{"no literal", pinF, `"f" => 3`, "no literal matches"},
		{"a step that runs past its function", []rootFramePin{rootPin("f frame", "x.cs", `frame\("f", (\d+)\)`, "p/p.go", `^func f\(`, `^\s*gone\(\)`)},
			`frame("f", 5)`, "before the declaration ends"},
		{"no declaration", []rootFramePin{rootPin("h start", "x.cs", `"h" => (\d+),`, "p/p.go", `^func h\(`)}, `"h" => 1,`, "no line matches the declaration"},
	}

	for _, c := range cases {
		findings := scanRootFrameLines(c.pins, pins(c.cs), readGo)

		if c.want == "" {
			if len(findings) != 0 {
				t.Errorf("%s: want no finding, got %q", c.name, findings)
			}

			continue
		}

		if len(findings) == 0 || !strings.Contains(strings.Join(findings, "\n"), c.want) {
			t.Errorf("%s: want a finding containing %q, got %q", c.name, c.want, findings)
		}
	}

	// The step skips the commented fn() on line 4 and stops inside f: the derivation is line 5, not 4 or 9.
	if line, err := deriveGoLine(goSource, []string{`^func f\(`, `fn\(\)`}); err != nil || line != 5 {
		t.Errorf("deriveGoLine: want 5, got %d (%v)", line, err)
	}
}
