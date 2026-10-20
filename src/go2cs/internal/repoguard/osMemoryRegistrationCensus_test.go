// osMemoryRegistrationCensus_test.go - Gbtc
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

// WHAT THIS WATCHES (G's ruling on the native pointer-array reader, 2026-10-19).
//
// golib's builtin.NativeArrayPointer tells two kinds of native memory apart by OWNERSHIP: a range the Go
// runtime mapped from the OS, recorded in golib's GoOsMemory, holds Go pointers (kept in a managed shadow), and
// every other range holds foreign data (pointer words read as foreign addresses; any other reference-bearing
// element refused by name). That is sound only while every OS mapping the runtime makes is REGISTERED and every
// release UNREGISTERS. A mapping that does not register would have its own Go pointers refused as foreign; a
// release that does not unregister would leave a recycled address reading as runtime-owned.
//
// The rule is "every OS mapping primitive registers": linux's sysMmap / sysMunmap and darwin's mmap / munmap
// are the only runtime methods that call libc's mmap / munmap, and windows' sys*OS hand-owns the only ones that
// call VirtualAlloc / VirtualFree. So this census names EVERY runtime method, per GOOS, that calls the OS map or
// unmap call, and holds each to a declared class:
//
//	REGISTERED  maps a new range and records it: GoOsMemory.Register in its body
//	INSIDE      commits within a range a REGISTERED method already recorded (windows MEM_COMMIT)
//	RELEASES    unmaps a range and forgets it: GoOsMemory.Unregister in its body
//	DECOMMIT    decommits within a reservation that stays (windows MEM_DECOMMIT): NO Unregister in its body
//
// A caller the table does not name is a NEW ROUTE and fails here, so it is classified before it lands. A table
// row no call site answers fails too, so the table cannot drift into describing a tree that is gone.

type osMemoryClass string

const (
	osRegistered osMemoryClass = "REGISTERED"
	osInside     osMemoryClass = "INSIDE"
	osReleases   osMemoryClass = "RELEASES"
	osDecommit   osMemoryClass = "DECOMMIT"
)

type osMemoryRoute struct {
	class osMemoryClass
	why   string
}

// osMemoryCallers is the declared census, per GOOS: method name -> class.
var osMemoryCallers = map[string]map[string]osMemoryRoute{
	"windows": {
		"sysAllocOS":   {osRegistered, "VirtualAlloc MEM_COMMIT|MEM_RESERVE: the runtime's own pages"},
		"sysReserveOS": {osRegistered, "VirtualAlloc MEM_RESERVE: the arena reservations"},
		"sysUsedOS":    {osInside, "VirtualAlloc MEM_COMMIT within a reservation sysReserveOS recorded"},
		"sysFreeOS":    {osReleases, "VirtualFree MEM_RELEASE"},
		"sysUnusedOS":  {osDecommit, "VirtualFree MEM_DECOMMIT: the reservation stays the runtime's"},
	},
	"linux": {
		"sysMmap":   {osRegistered, "every linux mapping funnels through it (cgo_mmap.go's mmap, every sys*OS)"},
		"sysMunmap": {osReleases, "every linux unmapping funnels through it"},
	},
	"darwin": {
		"mmap":   {osRegistered, "sys_darwin.go's mmap, hand-owned: every darwin sys*OS maps through it"},
		"munmap": {osReleases, "sys_darwin.go's munmap, hand-owned"},
	},
}

// osMemoryPrimitive matches a call to a flavour's OS map or unmap call: libc's, by the hand-owns' DllImport names,
// and windows' kernel calls, directly or through a converted stdcall.
var osMemoryPrimitive = map[string]*regexp.Regexp{
	"windows": regexp.MustCompile(`\bVirtual(?:Alloc|Free)\s*\(|\bstdcall\d{0,2}\s*\(\s*_Virtual(?:Alloc|Free)\b`),
	"linux":   regexp.MustCompile(`\blibc_(?:mmap|munmap)\s*\(`),
	"darwin":  regexp.MustCompile(`\blibc_(?:mmap|munmap)\s*\(`),
}

// osMemoryMethodDecl matches a member declaration and captures its name: modifiers at the line's start, the
// parameter list hugging the name (a tuple return type opens with a parenthesis, see gateDeclPattern).
var osMemoryMethodDecl = regexp.MustCompile(`^\s*(?:\[[^\]]*\]\s*)*(?:(?:internal|public|private|protected|static|unsafe|extern|partial)\s+)+.*?\b(\w+)\s*(?:<[^>()]*>)?\s*\(`)

// osMemoryStringLiteral matches a C# string literal, so a message naming "mmap(" is not a call.
var osMemoryStringLiteral = regexp.MustCompile(`"(?:[^"\\]|\\.)*"`)

type osMemoryMethod struct {
	sites []string // "<file>:<line>"
	body  strings.Builder
}

type osMemoryScan struct {
	files   int
	methods map[string]*osMemoryMethod // method name -> sites and text
}

// scanOsMemoryFile attributes every map and unmap call in one file to its enclosing method.
func scanOsMemoryFile(scan *osMemoryScan, goos, rel, text string) {
	primitive := osMemoryPrimitive[goos]
	var current *osMemoryMethod

	for number, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		code := line

		if comment := strings.Index(code, "//"); comment >= 0 {
			code = code[:comment]
		}

		code = osMemoryStringLiteral.ReplaceAllString(code, `""`)

		if match := osMemoryMethodDecl.FindStringSubmatch(code); match != nil && !strings.Contains(strings.SplitN(code, "(", 2)[0], "=") {
			current = scan.methods[match[1]]

			if current == nil {
				current = &osMemoryMethod{}
				scan.methods[match[1]] = current
			}

			// The declaration's own name is not a call (a DllImport declares libc_mmap).
			code = code[strings.Index(code, match[1])+len(match[1]):]
			code = code[strings.Index(code, "(")+1:]
		}

		if current != nil {
			current.body.WriteString(code)
			current.body.WriteByte('\n')
		}

		if current != nil && primitive.MatchString(code) {
			current.sites = append(current.sites, fmt.Sprintf("%s:%d", rel, number+1))
		}
	}
}

// osMemoryFindings holds a GOOS's scan to the declared table.
func osMemoryFindings(goos string, scan osMemoryScan) []string {
	var findings []string
	table := osMemoryCallers[goos]

	var names []string
	for name := range scan.methods {
		names = append(names, name)
	}
	sort.Strings(names)

	for _, name := range names {
		method := scan.methods[name]

		if len(method.sites) == 0 {
			continue
		}

		route, declared := table[name]

		if !declared {
			findings = append(findings, fmt.Sprintf("%s: NEW ROUTE %s calls the OS map or unmap call at %v and is not classified: "+
				"a mapping must register (GoOsMemory.Register) and a release unregister (GoOsMemory.Unregister), or its "+
				"memory reads as the wrong kind", goos, name, method.sites))
			continue
		}

		body := method.body.String()

		switch route.class {
		case osRegistered:
			if !strings.Contains(body, "GoOsMemory.Register(") {
				findings = append(findings, fmt.Sprintf("%s: %s is REGISTERED but its body at %v never calls GoOsMemory.Register: "+
					"its Go pointers would read as foreign", goos, name, method.sites))
			}
		case osReleases:
			if !strings.Contains(body, "GoOsMemory.Unregister(") {
				findings = append(findings, fmt.Sprintf("%s: %s RELEASES but its body at %v never calls GoOsMemory.Unregister: "+
					"a recycled address would read as runtime-owned", goos, name, method.sites))
			}
		case osDecommit, osInside:
			if strings.Contains(body, "GoOsMemory.Unregister(") {
				findings = append(findings, fmt.Sprintf("%s: %s is %s but its body at %v calls GoOsMemory.Unregister: the "+
					"reservation stays the runtime's", goos, name, route.class, method.sites))
			}
		}
	}

	var declared []string
	for name := range table {
		declared = append(declared, name)
	}
	sort.Strings(declared)

	for _, name := range declared {
		if method := scan.methods[name]; method == nil || len(method.sites) == 0 {
			findings = append(findings, fmt.Sprintf("%s: the table names %s (%s) but no call site answers it: the table "+
				"describes a tree that is gone", goos, name, table[name].class))
		}
	}

	return findings
}

// TestOsMemoryRegistrationCensus is the guard.
func TestOsMemoryRegistrationCensus(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	for _, goos := range []string{"windows", "linux", "darwin"} {
		scan := osMemoryScan{methods: map[string]*osMemoryMethod{}}

		for _, rel := range tracked {
			dir, file := filepath.Split(filepath.FromSlash(rel))
			dir = filepath.ToSlash(dir)

			if dir != "src/core/runtime/" && dir != "src/core/runtime/"+goos+"/" {
				continue
			}

			if !strings.HasSuffix(file, ".cs") || strings.HasSuffix(file, "_test.cs") {
				continue
			}

			raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

			if err != nil {
				t.Fatal(err)
			}

			scan.files++
			scanOsMemoryFile(&scan, goos, rel, string(raw))
		}

		if scan.files < 20 {
			t.Fatalf("VACUOUS: %d runtime files for %s; the census is measuring nothing", scan.files, goos)
		}

		var readings []string
		for name, method := range scan.methods {
			if len(method.sites) > 0 {
				readings = append(readings, fmt.Sprintf("%s %v", name, method.sites))
			}
		}
		sort.Strings(readings)
		t.Logf("READING · %s: %d files, callers %v", goos, scan.files, readings)

		for _, finding := range osMemoryFindings(goos, scan) {
			t.Error(finding)
		}
	}
}

// TestOsMemoryRegistrationCensusFires is the control: the same scanner over planted text.
func TestOsMemoryRegistrationCensusFires(t *testing.T) {
	plant := func(goos, text string) []string {
		scan := osMemoryScan{methods: map[string]*osMemoryMethod{}}
		scanOsMemoryFile(&scan, goos, "plant.cs", strings.ReplaceAll(text, "\n", "\r\n"))

		return osMemoryFindings(goos, scan)
	}

	// The darwin flavour, complete and registered: no finding. A message and a comment naming libc_mmap( are not
	// calls, and neither is the DllImport declaration.
	clean := `
private static extern nint libc_mmap(nint addr, nuint length, int prot, int flags, int fd, long offset);
internal static (@unsafe.Pointer, nint) mmap(@unsafe.Pointer addr, uintptr n, int32 prot, int32 flags, int32 fd, uint32 off) {
    nint r = libc_mmap((nint)addr, n, prot, flags, fd, off); // libc_mmap( in a comment
    GoOsMemory.Register((nuint)r, (nuint)n);
    print((@string)"runtime: libc_mmap("u8);
    return (new @unsafe.Pointer((nuint)r), 0);
}
internal static void munmap(@unsafe.Pointer addr, uintptr n) {
    GoOsMemory.Unregister((nuint)addr, (nuint)n);
    libc_munmap((nint)addr, n);
}
`
	if findings := plant("darwin", clean); len(findings) != 0 {
		t.Fatalf("the clean plant reported %d finding(s): %v", len(findings), findings)
	}

	expect := func(label, text, want string) {
		t.Helper()

		findings := plant("darwin", text)

		for _, finding := range findings {
			if strings.Contains(finding, want) {
				return
			}
		}

		t.Errorf("%s: no finding containing %q; got %v", label, want, findings)
	}

	expect("a new route", clean+`
internal static void newArena(uintptr n) {
    libc_mmap(0, n, 3, 0x1002, -1, 0);
}
`, "NEW ROUTE newArena")

	expect("a missed registration", strings.Replace(clean, "    GoOsMemory.Register((nuint)r, (nuint)n);\n", "", 1),
		"mmap is REGISTERED but")

	expect("a missed release", strings.Replace(clean, "    GoOsMemory.Unregister((nuint)addr, (nuint)n);\n", "", 1),
		"munmap RELEASES but")

	expect("a stale table row", strings.Replace(clean, "    libc_munmap((nint)addr, n);\n", "", 1),
		"the table names munmap")

	// windows: a decommit that unregisters would make a reservation it keeps read as foreign.
	windows := `
internal static void sysUnusedOS(@unsafe.Pointer v, uintptr n) {
    GoOsMemory.Unregister((nuint)v, (nuint)n);
    VirtualFree(v, n, _MEM_DECOMMIT);
}
`
	found := false

	for _, finding := range plant("windows", windows) {
		if strings.Contains(finding, "sysUnusedOS is DECOMMIT but") {
			found = true
		}
	}

	if !found {
		t.Errorf("a decommit that unregisters: no DECOMMIT finding")
	}
}
