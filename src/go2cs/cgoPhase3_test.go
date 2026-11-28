// cgoPhase3_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"slices"
	"strings"
	"testing"
	"time"
)

// cgo Phase 3 increment 3.1 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16): cmd/cgo's own helpers, C.malloc and
// C.free, a void pointer both ways, and the deferred call through the check closure.

// cgoPhase3FixtureDir holds increment 3.1's fixture and the two files cmd/cgo generated from it: the trees the route is
// tested on where no C compiler is.
var cgoPhase3FixtureDir = filepath.Join("testdata", "cgo", "phase3")

// Phase 3's fixture is made and held as Phase 2's is, without cgo_import.go: its probe functions are not linked when
// go list compiles it, so go list hands none back.
func TestTheCgoPhase3FixtureIsCmdCgosOutputForFixtureGo(t *testing.T) {
	requireCmdCgoOutput(t, cgoPhase3FixtureDir, "example.com/cgophase3", "fixture.cgo1.go", "cgo_gotypes.go")
}

// cgoPhase3Offline is the Phase 3 fixture as checked-in files, with the dynamic imports a real host's go list hands
// back once the package links: one for each C symbol the offline capture's table names.
func cgoPhase3Offline(t *testing.T) map[string]string {
	t.Helper()

	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)
	files["cgo_import.go"] = cgoOfflineImportFile(cgoPhase2Libraries)

	return files
}

// cgoBindingsMember is the text of one member of a bindings file: from the line that declares it to the line that
// closes it.
func cgoBindingsMember(t *testing.T, bindings, declaration string) string {
	t.Helper()

	start := strings.Index(bindings, declaration)

	if start < 0 {
		t.Fatalf("the bindings file declares no %q", declaration)
	}

	member := bindings[start:]

	if end := strings.Index(member, "\n    }\n"); end >= 0 {
		member = member[:end]
	}

	return member
}

// §16.1: the Phase 3 fixture converts, where every one of its C uses was refused. What it wrote is read here as text;
// TestCgoPhase3FixturesOutputBuildsAsAProject compiles it and the end-to-end program runs it.
func TestCgoPhase3FixtureConvertsItsHelpersVoidPointersAndDeferredCalls(t *testing.T) {
	stderr, output, err := convertCgoOffline(t, cgoPhase3Offline(t))

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	bindings := strings.ReplaceAll(readConverted(t, output, filepath.Join("linux-amd64", "cgophase3.cgo.cs")), "\r\n", "\n")
	converted := strings.ReplaceAll(readConverted(t, output, "fixture.cs"), "\r\n", "\n")

	// A. Each of cmd/cgo's helpers is a member of the pseudo-package, over golib's copies.
	for declaration, calls := range map[string][]string{
		"internal static ж<@char> CString(@string p0)":                     {"ᴍallocate(", "GoCgoArgs.StringSize(p0)", "GoCgoArgs.CopyString(p0, address)"},
		"internal static @unsafe.Pointer CBytes(slice<byte> p0)":           {"ᴍallocate(", "GoCgoArgs.BytesSize(p0)", "GoCgoArgs.CopyBytes(p0, address)"},
		"internal static @string GoString(ж<@char> p0)":                    {`GoCgoArgs.GoStringOf(p0, "GoString")`},
		"internal static @string GoStringN(ж<@char> p0, @int p1)":          {`GoCgoArgs.GoStringNOf(p0, (int)(int32)p1, "GoStringN")`},
		"internal static slice<byte> GoBytes(@unsafe.Pointer p0, @int p1)": {`GoCgoArgs.GoBytesOf(p0, (int)(int32)p1, "GoBytes")`},
		"internal static @unsafe.Pointer malloc(@ulong p0)":                {"ᴍallocate((nuint)(uint64)p0)"},
	} {
		member := cgoBindingsMember(t, bindings, declaration)

		for _, call := range calls {
			if !strings.Contains(member, call) {
				t.Errorf("%s does not hold %q:\n%s", declaration, call, member)
			}
		}
	}

	// The allocator is the package's own malloc, bound once, in the library the capture named for it.
	if count := strings.Count(bindings, `LibraryImport("libc.so.6", EntryPoint = "malloc")`); count != 1 {
		t.Errorf("the bindings file binds malloc %d times in libc.so.6, want 1", count)
	}

	// No helper is bound as a C symbol: they are Go functions, and no library holds one.
	for name := range cgoBuiltinNames {
		if strings.Contains(bindings, `EntryPoint = "`+name+`"`) {
			t.Errorf("the bindings file binds cmd/cgo's helper %s as a C symbol", name)
		}
	}

	// B. A void pointer is classified, then pinned by a `fixed` whose block holds the transition and the call (18.12
	// item 2), and a void pointer result is its number.
	for _, function := range []string{"free", "go2cs_probe_release", "go2cs_probe_same", "go2cs_probe_sum"} {
		member := cgoBindingsMember(t, bindings, " "+function+"(@unsafe.Pointer p0")
		classified := strings.Index(member, `GoCgoArgs.PinVoidPointer(p0, "`+function+`", out global::go.golib.GoCgoWordMirror? w0)`)
		pinned := strings.Index(member, "fixed (byte* f0 = g0)")
		addressed := strings.Index(member, "nint a0 = g0.Address(f0);")
		entered := strings.Index(member, "syscallEntersyscall()")

		if classified < 0 || pinned < 0 || addressed < 0 || entered < 0 || classified > pinned || pinned > addressed || addressed > entered {
			t.Errorf("C.%s does not classify, then pin, its void pointer before the transition:\n%s", function, member)
		}
	}

	if member := cgoBindingsMember(t, bindings, "internal static @unsafe.Pointer go2cs_probe_same("); !strings.Contains(member, "return new @unsafe.Pointer((uintptr)(nuint)r);") {
		t.Errorf("C.go2cs_probe_same does not return its result as a number:\n%s", member)
	}

	// P7: every native call sits inside the transition bracket, so externs equal brackets.
	externs, brackets := strings.Count(bindings, "LibraryImport("), strings.Count(bindings, "syscallEntersyscall();")
	t.Logf("READING the Phase 3 fixture's bindings: %d externs, %d transition brackets", externs, brackets)

	if externs == 0 || externs != brackets {
		t.Errorf("the bindings file holds %d externs and %d transition brackets", externs, brackets)
	}

	// C. Each deferred call through the check closure is a deferred call of the bound function, its argument evaluated
	// at the defer, and none of cmd/cgo's own variables reaches the C#.
	if count := strings.Count(converted, "defer(C.free, "); count != 6 {
		t.Errorf("fixture.cs defers C.free %d times, want 6", count)
	}

	if count := strings.Count(converted, "defer(C.go2cs_probe_release, "); count != 2 {
		t.Errorf("fixture.cs defers C.go2cs_probe_release %d times, want 2", count)
	}

	if !regexp.MustCompile(`defer\(C\.go2cs_probe_log_put, [^,]+, value, ref ᒐ\);\n\s*value = 9;`).MatchString(converted) {
		t.Errorf("the deferred call's argument is not handed over at the defer, ahead of its reassignment:\n%s", lastLines(converted, 40))
	}

	if strings.Contains(converted, "_cgo") {
		t.Error("one of cmd/cgo's own variables reached fixture.cs")
	}
}

// The one address of Go memory that reaches a void pointer as a bare number with no uintptr in the source: inside a
// method with a pointer receiver, unsafe.Pointer of a pointer of the receiver's own type. The receiver converts by
// reference, and the converter mints that conversion from a reference with no box to carry. Read while the pin
// moments were listed for the review of the void pointer's door: the conversion handed C.f `(uintptr)FromRef(ref h)`,
// and for a local copy of the receiver `FromRef(ref p)`, the address of the local itself. The route refuses each form
// by name, before anything is written. The control beside them is the way round the refusal names: a function that
// takes the pointer as a parameter converts, and mints it with its box.
func TestCgoUnsafePointerOfAMethodsOwnReceiverTypeIsRefusedByName(t *testing.T) {
	const closure = "func() _cgo_unsafe.Pointer { _cgo0 := %s; _cgoCheckPointer(_cgo0, nil); return _Cfunc_go2cs_probe_same(_cgo0) }() != nil"

	method := func(name, prelude, argument string) string {
		return "\nfunc (h *holder) " + name + "(other *holder) bool {\n" + prelude + "\treturn " + fmt.Sprintf(closure, argument) + "\n}\n"
	}

	holder := "\ntype holder struct{ n _Ctype_int }\n"

	for _, arm := range []struct {
		name    string
		methods string
		refused int
	}{
		{"the receiver, handed to C as it is", method("Same", "", "unsafe.Pointer(h)"), 1},
		{"the receiver, as a number in a variable", method("Number", "\tn := uintptr(unsafe.Pointer(h))\n", "unsafe.Pointer(n)"), 1},
		{"the receiver, under parentheses", method("Wrapped", "", "unsafe.Pointer((h))"), 1},
		{"a local copy of the receiver", method("Local", "\tp := h\n", "unsafe.Pointer(p)"), 1},
		{"another pointer of the receiver's type", method("Other", "", "unsafe.Pointer(other)"), 1},
		// The review of 3.1, finding 5: the walk read the receiver's type through its NAME, so a receiver with no name,
		// or the blank one, was skipped, and the same conversion crossed.
		{"a pointer of the type of a receiver with no name", "\nfunc (*holder) Unnamed(other *holder) bool {\n\treturn " + fmt.Sprintf(closure, "unsafe.Pointer(other)") + "\n}\n", 1},
		{"a pointer of the type of a blank receiver", "\nfunc (_ *holder) Blank(other *holder) bool {\n\treturn " + fmt.Sprintf(closure, "unsafe.Pointer(other)") + "\n}\n", 1},
		// The control: a function takes the pointer as a parameter, which carries its box.
		{"in a function that takes the pointer as a parameter", "\nfunc same(p *holder) bool {\n\treturn " + fmt.Sprintf(closure, "unsafe.Pointer(p)") + "\n}\n", 0},
	} {
		t.Run(arm.name, func(t *testing.T) {
			files := cgoPhase3Offline(t)
			files["fixture.cgo1.go"] += holder + arm.methods

			stderr, output, err := convertCgoOffline(t, files)
			lines := cgoRefusalLinesIn(stderr, "")

			if arm.refused == 0 {
				if err != nil {
					t.Fatalf("the control did not convert: %v\n%s", err, lastLines(stderr, 12))
				}

				if converted := readConverted(t, output, "fixture.cs"); !regexp.MustCompile(`C\.go2cs_probe_same\((\(uintptr\))?@unsafe\.Pointer\.From(Pinned)?Box\(Ꮡ?p\)\)`).MatchString(converted) {
					t.Errorf("the parameter is not minted with its box:\n%s", lastLines(converted, 14))
				}

				return
			}

			requireNothingWritten(t, output, err, stderr)

			if len(lines) != arm.refused {
				t.Errorf("%d refusal lines, want %d:\n%s", len(lines), arm.refused, strings.Join(lines, "\n"))
			}

			requireCgoUseRefused(t, lines, "unsafe.Pointer of a pointer of the method's own receiver type", "takes it as a parameter")
		})
	}
}

// The compile half of the test above: what the Phase 3 fixture converts to builds as a project.
func TestCgoPhase3FixturesOutputBuildsAsAProject(t *testing.T) {
	buildCgoOfflineFixture(t, "Phase 3", cgoPhase3Offline(t))
}

// cgoListedImports builds the one-file cgo package source makes, as go list does, and returns the names cmd/cgo's own
// binary imports: the local name of each //go:cgo_import_dynamic line of its _cgo_import.go, which is what the
// verified-names rule reads (cgoImportedByCmdCgo).
func cgoListedImports(t *testing.T, source string) map[string]bool {
	t.Helper()

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module example.com/cgoimports\n\ngo 1.26\n")
	writeCgoFixture(t, dir, "source.go", source)

	list := exec.Command("go", "list", "-compiled", "-json", ".")
	list.Dir = dir
	list.Env = append(os.Environ(), "CGO_ENABLED=1", "GOWORK=off", "GOFLAGS=")
	output, err := list.Output()

	if err != nil {
		t.Fatalf("go list -compiled: %v", err)
	}

	var listed struct {
		Dir             string
		CompiledGoFiles []string
	}

	if err := json.Unmarshal(output, &listed); err != nil {
		t.Fatal(err)
	}

	var imports map[string]bool

	for _, path := range listed.CompiledGoFiles {
		if !filepath.IsAbs(path) {
			path = filepath.Join(listed.Dir, path)
		}

		content, err := os.ReadFile(path)

		if err != nil {
			t.Fatal(err)
		}

		for _, line := range strings.Split(strings.ReplaceAll(string(content), "\r\n", "\n"), "\n") {
			if fields := strings.Fields(line); len(fields) >= 2 && fields[0] == "//go:cgo_import_dynamic" && fields[1] != "_" {
				if imports == nil {
					imports = map[string]bool{}
				}

				imports[fields[1]] = true
			}
		}
	}

	if imports == nil {
		t.Fatalf("go list -compiled handed back no file of dynamic imports: %v", listed.CompiledGoFiles)
	}

	return imports
}

// Two READINGS at the verified-names rule's own input (§16.1 D, §16.4). The rule binds a C name only when cmd/cgo's own
// binary imports a symbol of that name.
//
// strtol, owed from increment 3.0: its char ** parameter is the out-parameter, which is increment 3.2's, so the
// conversion refuses the call before the rule reads it. What the rule WOULD read is printed here: whether the binary
// imports strtol, or the name a C23 glibc header gives it. The test asserts only that the rule's verdict follows that.
//
// malloc and free: the helpers allocate with the package's own malloc, and the caller frees with its own free, so both
// must be names the rule binds. That half is asserted on every host (stop line S3.1-C).
func TestCgoNamesCmdCgosOwnBinaryImportsAreRead(t *testing.T) {
	cgoRealProbeDir(t)

	target := runtime.GOOS + "/" + runtime.GOARCH
	toolchain, _ := cgoRealToolchainLine(t)

	matching := func(imports map[string]bool, pattern string) []string {
		var names []string

		for name := range imports {
			if regexp.MustCompile(pattern).MatchString(name) {
				names = append(names, name)
			}
		}

		slices.Sort(names)

		return names
	}

	t.Run("strtol", func(t *testing.T) {
		imports := cgoListedImports(t, "package cgoimports\n\n// #include <stdlib.h>\nimport \"C\"\n\n// Parse makes the one C call this package reads.\nfunc Parse(text *C.char) int { return int(C.strtol(text, nil, 10)) }\n")
		refused := len(cgoImportedByCmdCgo([]string{"strtol"}, imports, nil)) > 0

		t.Logf("READING %s under %q: cmd/cgo's binary imports %v of the names that hold strtol; the verified-names rule would refuse C.strtol: %t", target, toolchain, matching(imports, `strtol`), refused)

		if refused == imports["strtol"] {
			t.Errorf("the rule's verdict does not follow the imports: strtol imported %t, refused %t", imports["strtol"], refused)
		}
	})

	t.Run("malloc and free", func(t *testing.T) {
		imports := cgoListedImports(t, "package cgoimports\n\n// #include <stdlib.h>\nimport \"C\"\n\nimport \"unsafe\"\n\n// Copy allocates through a helper and frees with the package's own free.\nfunc Copy(s string) string {\n\tcs := C.CString(s)\n\tdefer C.free(unsafe.Pointer(cs))\n\n\treturn C.GoString(cs)\n}\n")

		t.Logf("READING %s under %q: cmd/cgo's binary imports %v of the names that hold malloc or free", target, toolchain, matching(imports, `malloc|free`))

		for _, name := range []string{"malloc", "free"} {
			if !imports[name] {
				t.Errorf("cmd/cgo's binary does not import %s under its plain name, so the helpers cannot bind it (S3.1-C)", name)
			}
		}
	})
}

// cgoP3FixtureDir holds the end-to-end program's sources.
const cgoP3FixtureDir = "testdata/cgo/p3"

// cgoP3GoMemory is in every refusal increment 3.1 makes of a pointer into Go memory: the increment that admits it.
const cgoP3GoMemory = "not supported yet, so go2cs refuses the call"

// cgoP3WordToken is in the refusal of a pointer-sized integer that is go2cs's token for a Go value with no address
// (GoCgoArgs.Word; design 20.4 S4).
const cgoP3WordToken = "token for a Go value that has no address"

// cgoP3WriteThrough is in the refusal of a typed pointer C would write through (GoCgoArgs.StructPointer, Phase 2's door
// for every typed pointer parameter): a native box minted over managed storage. Its text names no increment.
const cgoP3WriteThrough = "which C must not write through"

// cgoP3LayoutEntryPointer is go2cs's refusal of a read of native memory as a Go type whose CLR layout is not its Go
// layout (golib RuntimeErrorPanic.cs: "cannot read native memory as {0}: its CLR layout is not its Go layout ({1})"),
// pinned to the TYPE s6next reads; the parenthesised reason is left unpinned. Its text names no increment. It is
// s6next's end state in 3.2 until seat 1c (18.11) routed **C.T loads and stores through GoCgoArgs.LoadPointer and
// StorePointer, which retired that pin; it now names the floor's refusal of the two shapes the door does not reach, a
// ref-lowered out-parameter (s6nextset) and a tuple target (s6nexttuple), each a named deviation. Increment 3.2 seat 1a (18.9) retired its sibling, cgoP3LayoutEntry ("cannot read native memory
// as main._Ctype_struct_go2cs_probe_entry: its CLR layout is not its Go layout"), the pin of s6nextnil, s6nextdirect
// and s6name: the field accessor reads each as Go does. Hosted reading before 1a, os-matrix run 44883224786, the same
// on all five targets: s6nextnil exited 2 with "panic: runtime error: cannot read native memory as
// main._Ctype_struct_go2cs_probe_entry: its CLR layout is not its Go layout (field next is a managed reference where Go
// stores the value inline)"; s6next exits 2 with "panic: runtime error: cannot read native memory as
// *main._Ctype_struct_go2cs_probe_entry: its CLR layout is not its Go layout (a managed reference where Go stores
// words)".
const cgoP3LayoutEntryPointer = "cannot read native memory as *main._Ctype_struct_go2cs_probe_entry: its CLR layout is not its Go layout"

// cgoP3StoreGoIntoC is in go2cs's refusal of a Go pointer stored into a pointer field of a C struct in C memory (golib
// ж.CgoFields.cs, CgoFieldStoreIntoCText). Go stores it; C memory must not hold one, so go2cs refuses it by name.
const cgoP3StoreGoIntoC = "storing a Go pointer into C memory is refused"

// cgoP3ReturnedIntoGoMemory is in the refusal of a word C returned inside a pinned argument's Go storage
// (GoCgoArgs.RequireWordOutsidePin; P3's review of 1b, item 4).
const cgoP3ReturnedIntoGoMemory = "returned an address inside the Go memory it was handed"

// cgoP3InsideStackCopy is in the refusal of a word C returned inside the stack copy of a Go argument that is not the copy
// itself (GoCgoArgs.PointerMemberBack; .5 cgo item 1(b)).
const cgoP3InsideStackCopy = "inside the stack copy of a Go argument"

// cgoP3WideReinterpret is in the refusal of a reinterpret view no door can pin over a value wider than its pointee
// (GoCgoArgs.ScalarPointer; P3's review of 1b, item 3).
const cgoP3WideReinterpret = "C may write past one"

// cgoP3ImageTwoDoors is in the refusal of one Go value handed to C as an image and through another door
// (GoCgoImages; the struct image door's B5).
const cgoP3ImageTwoDoors = "name the same Go value"

// cgoP3Arms are the end-to-end program's arms. An arm with neither goFails nor csFails prints the same lines and exits
// 0 both ways. Since increment 3.2 seat 1b (18.12, P-2) an arm that hands C or a helper a pointer into Go memory whose
// box names its storage prints as Go does: the wrapper pins the storage for the call. govoidnumber and govoidexpr hand
// over a NUMBER, which stays refused by name (M3), and the arms with other csFails are refused as their own text says. innerdefer and innergo are the controls of goinnerdefer
// and goinnergo: the same deferred or started Go function over C memory runs to its end both ways.
func cgoP3Arms() []cgoG1Arm {
	return []cgoG1Arm{
		{name: "text"},
		{name: "roundtrip"},
		{name: "invalid"},
		{name: "nul"},
		{name: "nilstring"},
		{name: "counted"},
		{name: "zero"},
		{name: "negstring"},
		{name: "negbytes"},
		{name: "cbytes"},
		{name: "emptybytes"},
		{name: "copy"},
		{name: "fill"},
		{name: "same"},
		{name: "deferred"},
		{name: "innerdefer"},
		{name: "innergo"},
		{name: "goinnerdefer"},
		{name: "goinnergo"},
		{name: "govoid"},
		// S4 (design 20.4): a number of Go memory crosses through the record of its mint, in a variable or in one
		// expression, and at an offset inside its storage; a uintptr_t handed a token is refused by name.
		{name: "govoidnumber"},
		{name: "govoidexpr"},
		{name: "govoidnumberwrite"},
		// The go-sqlite3 marshalling slice: a void * member written by C, from Go memory and from C memory; a Go pointer
		// in that member, which cgo's own check refuses on both sides; and a typedef'd const char * result.
		{name: "slicecolsgo"},
		{name: "slicecolsc"},
		{name: "slicecolgoptr", goFails: "Go pointer to unpinned Go pointer", csFails: "Go pointer to unpinned Go pointer"},
		{name: "slicefname"},
		// The S4 cure's near misses (P3, F1): a typedef of a typedef (uint64_t) and a typedef'd enum, each a constant
		// cmd/cgo's check closure declares at the typedef.
		{name: "typedefwide"},
		{name: "typedefenum"},
		{name: "wordtoken", csFails: cgoP3WordToken},
		{name: "wordint"},
		// S6 (design 21): a pointer C keeps past the call, pinned with runtime.Pinner, read back after a collection.
		{name: "pinnerkeep"},
		// S6, P3's MUST 2: a pinned C struct handed to a typed parameter C keeps.
		{name: "pinnerkeepstruct"},
		{name: "gobytes"},
		{name: "gostring"},
		// Pointers DERIVED from a view of Go memory (the review of increment 3.1, §16.11).
		{name: "goviewfield"},
		{name: "goviewfieldarray"},
		{name: "goviewagain"},
		{name: "goviewelem"},
		{name: "goviewelemstring"},
		{name: "goslice"},
		{name: "goslicedata"},
		{name: "goadd"},
		{name: "goaddlocal"},
		// A pointer that reaches C as the result of a call, through a variable and through a user function.
		{name: "goaddvar"},
		{name: "goaddfunc"},
		{name: "goviewfieldfunc"},
		{name: "goviewfieldvar"},
		// The second review (§16.12): a typed pointer from an interior unsafe.Pointer value, and an array field of a view.
		{name: "goaddtyped"},
		{name: "goaddfill"},
		{name: "gofieldtyped"},
		{name: "goviewnamestring"},
		{name: "goviewnameadd"},
		{name: "goviewnameslice"},
		{name: "goviewnamedata"},
		// Seat 1b's arms (18.12 item 5): cmd/cgo's element and slice check closures, made by cmd/cgo itself from the Go
		// code each arm writes, through the void door and the scalar door.
		{name: "goindex"},
		{name: "goindexfill"},
		{name: "goslicefill"},
		// Seat 1b's out-parameter (18.12 item 4, shape (i)): strtod's char ** endptr, &end over a Go variable.
		{name: "outstrtod"},
		// P3's review of 1b (COORD inbox 20261124T173809Z): an element of a native window over a view, typed (item 2), and
		// the two out-parameter shapes (items 1 and 4). The element of a Go array of C pointers was refused by name until
		// the .5 cgo item 2 word mirror, and now runs as Go does; the pointer into Go memory stays refused.
		{name: "goviewcelemstring"},
		{name: "goviewcelemfill"},
		{name: "outstrtodelem"},
		{name: "outstrtodgo", csFails: cgoP3ReturnedIntoGoMemory},
		// P3's read of the fold (COORD inbox 20261124T204626Z): the pointer result inside a pin and its identity
		// near-miss, and the wide reinterpret view and its same-size near-miss.
		{name: "resultstrtokgo", csFails: cgoP3ReturnedIntoGoMemory},
		{name: "resultstrcpyself"},
		{name: "goviewwide", csFails: cgoP3WideReinterpret},
		{name: "goviewsame"},
		// The struct image door (19.9): one image per root (B5), against Go, and the mixed case refused by name.
		{name: "imgaddinto"},
		{name: "imgmixed", csFails: cgoP3ImageTwoDoors},
		{name: "inlinemixed"},
		// Increment 3.2's first reds (section 6). s6field, s6reinterp and s6slice print as Go does, and since seat 1a
		// (18.9) s6nextnil, s6nextdirect and s6name do too: the field accessor retired their cgoP3LayoutEntry pins (the
		// readings before it are at cgoP3LayoutEntryPointer). Since seat 1c (18.11) s6next does too: the **C.T door
		// retired its pin.
		{name: "s6field"},
		{name: "s6next"},
		{name: "s6nextnil"},
		{name: "s6nextdirect"},
		{name: "s6name"},
		{name: "s6reinterp"},
		{name: "s6slice"},
		// Increment 3.2's stores into a pointer field of C memory (D-4): a C pointer reads back in C, and a Go pointer
		// is refused by name.
		{name: "s6store"},
		{name: "s6storego", csFails: cgoP3StoreGoIntoC},
		// Seat 1c's arms (18.11, E-3): the load and the store through a pointer to a C pointer, the resolver's walk, a
		// Go slot that holds nil, and the two shapes the door does not reach, refused by the floor as named deviations.
		{name: "s6nextstore"},
		{name: "s6nextstorego", csFails: cgoP3StoreGoIntoC},
		{name: "s6nextwalk"},
		{name: "s6nilslot"},
		{name: "s6nextset", csFails: cgoP3LayoutEntryPointer},
		{name: "s6nexttuple", csFails: cgoP3LayoutEntryPointer},
		// Their twins over C memory, which run to their end both ways.
		{name: "caddtyped"},
		{name: "cfieldtyped"},
		{name: "cviewnamestring"},
		{name: "cviewnameadd"},
		{name: "cviewnameslice"},
		{name: "cviewnamedata"},
		// The .5 cgo items 1(a) and 1(b) (18.12.3): a pointer member C writes into Go memory it was handed, refused by
		// name, and its C-memory near-miss; the same for a struct result; an out-word that is the argument's stack copy
		// (out == &a) and its C-memory near-miss; the copy's address through another pointee, and an out-word inside a
		// copy, each refused by name; a member that is its struct's own copy (node->next == node).
		{name: "membergo", csFails: cgoP3ReturnedIntoGoMemory},
		{name: "memberc"},
		{name: "resultmembergo", csFails: cgoP3ReturnedIntoGoMemory},
		{name: "resultmemberc"},
		{name: "outself"},
		{name: "outc"},
		{name: "outothertype", csFails: cgoP3InsideStackCopy},
		{name: "outinterior", csFails: cgoP3InsideStackCopy},
		{name: "ringself"},
		// The .5 cgo item 2, the argv shape (18.12.3, H's verdict (A)): a whole Go array of C pointers through the typed
		// door, filled by C, as a struct field and through the void door; a Go pointer among the words (cgo's panic on
		// both sides); a word C stores into the array itself, refused by name (the mirror is one of the pins); and the
		// near-miss over C memory.
		{name: "argvgo"},
		{name: "argvfill"},
		// Both sides panic, so the arm carries no expectation: the shared branch compares the exit and the first line.
		{name: "argvgoptr"},
		{name: "argvself", csFails: cgoP3ReturnedIntoGoMemory},
		{name: "argvc"},
		{name: "argvfield"},
		{name: "argvvoid"},
		// C global variables (cgo item 3): an int, a struct, an errno-style int a call sets, and a global's address,
		// each read and written in place both ways.
		{name: "cvar"},
		{name: "cvarstruct"},
		{name: "cvarerrno"},
		{name: "cvaraddr"},
	}
}

// Increment 3.1's end-to-end test (§16.3): the program is built by go and converted by go2cs on the same host, with the
// same C compiler and the same libraries, and each arm is run both ways. go's side is built and read FIRST, so that how
// Go itself behaves on an arm is on record before go2cs is compared with it, and where the conversion is refused.
func TestCgoPhase3ProgramRunsAsGoDoes(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build and run the program (Inconclusive, not a pass)", cgoBuildEnv)
	}

	probeDir := cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	files := cgoFixtureFiles(t, cgoP3FixtureDir)

	// go's own build, with this host's compiler and the probe on the link line.
	module := t.TempDir()
	writeCgoFixture(t, module, "go.mod", "module example.com/cgop3\n\ngo 1.26\n")

	for name, contents := range files {
		writeCgoFixture(t, module, name, contents)
	}

	binaries := t.TempDir()
	goProgram := filepath.Join(binaries, "p3go")

	if runtime.GOOS == "windows" {
		goProgram += ".exe"
	}

	// On darwin go's linker runs Apple's strip unless DWARF is off, and strip refuses what the pinned zig links
	// (cgoG1_test.go).
	goBuild := []string{"build", "-o", goProgram}

	if runtime.GOOS == "darwin" {
		goBuild = append(goBuild, "-ldflags=-w")
	}

	build := exec.Command("go", append(goBuild, ".")...)
	build.Dir = module
	build.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1", "CGO_LDFLAGS=-L"+probeDir+" -lgo2csprobe")

	if out, err := build.CombinedOutput(); err != nil {
		t.Fatalf("go did not build the program: %v\n%s", err, lastLines(string(out), 30))
	}

	libraryEnv := cgoG1LibraryEnv(probeDir)

	if runtime.GOOS == "windows" {
		copyCgoG1File(t, filepath.Join(probeDir, cgoG1ProbeFile()), filepath.Join(binaries, cgoG1ProbeFile()))
	}

	// A directory with nothing in it is where every arm runs.
	empty := t.TempDir()
	arms := cgoP3Arms()
	goRuns := make([]cgoG1Run, len(arms))

	for i, arm := range arms {
		goRuns[i] = runCgoG1(t, []string{goProgram}, arm, empty, libraryEnv)
		t.Logf("READING %s %s: go exit %d %q %s", target, arm.label(), goRuns[i].exit, strings.TrimSpace(goRuns[i].stdout), cgoP3FirstLine(goRuns[i].stderr))
	}

	// The conversion, on this host's real route.
	combined, _, output, err := convertCgoReal(t, probeDir, files)

	if err != nil {
		for _, line := range cgoRefusalLinesIn(combined, "") {
			t.Logf("READING %s: refused: %s", target, line[strings.Index(line, "error "):])
		}

		t.Fatalf("the program did not convert on %s: %v\n%s", target, err, lastLines(combined, 30))
	}

	output = cgoBuildDir(t, output)
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	started := time.Now()
	dotnetBuild := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	dotnetBuild.Env = cgoBuildEnvironment()
	built, err := dotnetBuild.CombinedOutput()
	buildErrors := cgoCompilerErrors(string(built))

	t.Logf("READING %s: the converted program's build: %d distinct error(s) in %s", target, len(buildErrors), time.Since(started).Round(time.Second))

	for _, line := range buildErrors {
		t.Logf("READING %s: build error %s", target, line)
	}

	if err != nil {
		t.Fatalf("the converted program does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}

	assembly := cgoBuiltAssembly(t, output, projects[0])
	csProgram := []string{dotnet, assembly}

	if runtime.GOOS == "windows" {
		copyCgoG1File(t, filepath.Join(probeDir, cgoG1ProbeFile()), filepath.Join(filepath.Dir(assembly), cgoG1ProbeFile()))
	}

	for i, arm := range arms {
		t.Run(arm.label(), func(t *testing.T) {
			goRun := goRuns[i]
			csRun := runCgoG1(t, csProgram, arm, empty, libraryEnv)

			t.Logf("READING %s %s: go exit %d %q; go2cs exit %d %q %s", target, arm.label(), goRun.exit, strings.TrimSpace(goRun.stdout), csRun.exit, strings.TrimSpace(csRun.stdout), cgoP3FirstLine(csRun.stderr))

			if arm.csFails != "" {
				// Go allows the arm, which is the control: the program is valid Go and does what it says.
				if goRun.exit != 0 || goRun.stdout == "" {
					t.Errorf("go was to run this arm to its end, and exited %d:\n%s%s", goRun.exit, goRun.stdout, lastLines(goRun.stderr, 8))
				}

				if csRun.exit == 0 || !strings.Contains(csRun.stderr+csRun.stdout, arm.csFails) {
					t.Errorf("go2cs was to refuse with %q, and exited %d:\n%s%s", arm.csFails, csRun.exit, csRun.stdout, lastLines(csRun.stderr, 8))
				}

				return
			}

			if goRun.exit != csRun.exit || goRun.stdout != csRun.stdout || (goRun.exit == 0 && goRun.stdout == "") {
				t.Errorf("the two runs differ, or printed nothing:\n   go: exit %d %q\n%s\ngo2cs: exit %d %q\n%s", goRun.exit, goRun.stdout, lastLines(goRun.stderr, 6), csRun.exit, csRun.stdout, lastLines(csRun.stderr, 12))
			}

			// An arm Go itself ends (a negative count) ends the same way here: the same exit status, and the same first
			// line on standard error, which says whether it was a panic or a fatal error and with what text.
			if goRun.exit != 0 && cgoP3FirstLine(goRun.stderr) != cgoP3FirstLine(csRun.stderr) {
				t.Errorf("the two runs end differently:\n   go: %s\ngo2cs: %s", cgoP3FirstLine(goRun.stderr), cgoP3FirstLine(csRun.stderr))
			}
		})
	}
}

// cgoP3FirstLine is the first line a failing run wrote to its standard error, in brackets, or "".
func cgoP3FirstLine(stderr string) string {
	for _, line := range strings.Split(stderr, "\n") {
		if line = strings.TrimSpace(line); line != "" {
			return "[" + line + "]"
		}
	}

	return ""
}

// A pointer argument cmd/cgo declares at its own type, `var _cgo0 *_Ctype_char = cs` (seat 1b's hosted reading, CS0119 on
// strtod's call): the argument renders the box itself, never a Go-shaped `(*C.@char)(cs)`, which C# reads as a type in
// an expression. The closure is cmd/cgo's for `C.strtol(cs, &end, 10)` with `var end *C.char`.
func TestCgoAPointerArgumentDeclaredAtItsOwnTypeRendersItsBox(t *testing.T) {
	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)

	// The capture names strtol's library, for this test alone (the package's tests run serially).
	cgoPhase2Libraries["strtol"] = "libc.so.6"
	defer delete(cgoPhase2Libraries, "strtol")
	files["cgo_import.go"] = strings.Replace(cgoOfflineImportFile(cgoPhase2Libraries), "package cgophase2", "package cgophase3", 1)
	files["fixture.cgo1.go"] += `
func strtolEnd(cs *_Ctype_char) (int, string) {
	var end *_Ctype_char
	n := func() _Ctype_long{ var _cgo0 *_Ctype_char = cs; _cgoBase1 := &end; _cgo1 := _cgoBase1; var _cgo2 _Ctype_int = 10; _cgoCheckPointer(_cgoBase1, 0 == 0); return _Cfunc_strtol(_cgo0, _cgo1, _cgo2); }()
	return int(n), ( _Cfunc_GoString)(end)
}
`
	files["cgo_gotypes.go"] += `
//go:cgo_import_static _cgo_55dd9c1e1280_Cfunc_strtol
//go:linkname __cgofn__cgo_55dd9c1e1280_Cfunc_strtol _cgo_55dd9c1e1280_Cfunc_strtol
var __cgofn__cgo_55dd9c1e1280_Cfunc_strtol byte
var _cgo_55dd9c1e1280_Cfunc_strtol = unsafe.Pointer(&__cgofn__cgo_55dd9c1e1280_Cfunc_strtol)

//go:cgo_unsafe_args
func _Cfunc_strtol(p0 *_Ctype_char, p1 **_Ctype_char, p2 _Ctype_int) (r1 _Ctype_long) {
	_cgo_runtime_cgocall(_cgo_55dd9c1e1280_Cfunc_strtol, uintptr(unsafe.Pointer(&p0)))
	if _Cgo_always_false {
		_Cgo_use(p0)
		_Cgo_use(p1)
		_Cgo_use(p2)
	}
	return
}
`
	stderr, output, err := convertCgoOffline(t, files)
	if err != nil {
		t.Fatalf("%v\n%s", err, stderr)
	}
	converted := readConverted(t, output, "fixture.cs")

	if want := "var n = C.strtol(cs, Ꮡend, (C.@int)(10));"; !strings.Contains(converted, want) {
		t.Errorf("the call does not read %q", want)
	}

	if strings.Contains(converted, "(*C.@char)(") {
		t.Errorf("a Go-shaped pointer conversion reached the C#")
	}
}

// An argument cmd/cgo declares at a C TYPEDEF, `var _cgo2 _Ctype_size_t = 2` (cgo S4's hosted reading, CS0246 on
// memset's call in the p3 program off linux/amd64): cmd/cgo declares a typedef as an alias, and the cast names it as a
// declaration does, through the pseudo-package (Cꓸsize_t), never as the Go name _Ctype_size_t. The closure is cmd/cgo's
// shape for `C.memset(p, 7, 2)`.
func TestCgoAnArgumentDeclaredAtACTypedefCastsToItsPseudoPackageName(t *testing.T) {
	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)

	// The capture names memset's library, for this test alone (the package's tests run serially).
	cgoPhase2Libraries["memset"] = "libc.so.6"
	defer delete(cgoPhase2Libraries, "memset")
	// P3's near misses (F1): a typedef of a typedef, as glibc declares uint64_t, and a typedef'd enum, each beside a void
	// pointer, which is what makes cmd/cgo write the closure (a typed pointer to a C integer takes none).
	cgoPhase2Libraries["go2cs_probe_wide_into"] = "libgo2csprobe.so"
	defer delete(cgoPhase2Libraries, "go2cs_probe_wide_into")
	cgoPhase2Libraries["go2cs_probe_color_into"] = "libgo2csprobe.so"
	defer delete(cgoPhase2Libraries, "go2cs_probe_color_into")
	files["cgo_import.go"] = strings.Replace(cgoOfflineImportFile(cgoPhase2Libraries), "package cgophase2", "package cgophase3", 1)
	files["fixture.cgo1.go"] += `
func fillSeven(p unsafe.Pointer) {
	func() _cgo_unsafe.Pointer{ _cgo0 := p; var _cgo1 _Ctype_int = 7; var _cgo2 _Ctype_size_t = 2; _cgoCheckPointer(_cgo0, nil); return _Cfunc_memset(_cgo0, _cgo1, _cgo2); }()
}

func wideInto(dst unsafe.Pointer) _Ctype_uint64_t {
	return func() _Ctype_uint64_t{ _cgo0 := dst; var _cgo1 _Ctype_uint64_t = 0x123456789; _cgoCheckPointer(_cgo0, nil); return _Cfunc_go2cs_probe_wide_into(_cgo0, _cgo1); }()
}

func colorInto(dst unsafe.Pointer) _Ctype_int {
	return func() _Ctype_int{ _cgo0 := dst; var _cgo1 _Ctype_go2cs_probe_color = 4; _cgoCheckPointer(_cgo0, nil); return _Cfunc_go2cs_probe_color_into(_cgo0, _cgo1); }()
}
`
	files["cgo_gotypes.go"] += `
//go:cgo_import_static _cgo_55dd9c1e1280_Cfunc_memset
//go:linkname __cgofn__cgo_55dd9c1e1280_Cfunc_memset _cgo_55dd9c1e1280_Cfunc_memset
var __cgofn__cgo_55dd9c1e1280_Cfunc_memset byte
var _cgo_55dd9c1e1280_Cfunc_memset = unsafe.Pointer(&__cgofn__cgo_55dd9c1e1280_Cfunc_memset)

type _Ctype___uint64_t = _Ctype_ulong

type _Ctype_uint64_t = _Ctype___uint64_t

type _Ctype_go2cs_probe_color uint32

//go:cgo_import_static _cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into
//go:linkname __cgofn__cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into _cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into
var __cgofn__cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into byte
var _cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into = unsafe.Pointer(&__cgofn__cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into)

//go:cgo_unsafe_args
func _Cfunc_go2cs_probe_wide_into(p0 unsafe.Pointer, p1 _Ctype_uint64_t) (r1 _Ctype_uint64_t) {
	_cgo_runtime_cgocall(_cgo_55dd9c1e1280_Cfunc_go2cs_probe_wide_into, uintptr(unsafe.Pointer(&p0)))
	if _Cgo_always_false {
		_Cgo_use(p0)
		_Cgo_use(p1)
	}
	return
}

//go:cgo_import_static _cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into
//go:linkname __cgofn__cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into _cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into
var __cgofn__cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into byte
var _cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into = unsafe.Pointer(&__cgofn__cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into)

//go:cgo_unsafe_args
func _Cfunc_go2cs_probe_color_into(p0 unsafe.Pointer, p1 _Ctype_go2cs_probe_color) (r1 _Ctype_int) {
	_cgo_runtime_cgocall(_cgo_55dd9c1e1280_Cfunc_go2cs_probe_color_into, uintptr(unsafe.Pointer(&p0)))
	if _Cgo_always_false {
		_Cgo_use(p0)
		_Cgo_use(p1)
	}
	return
}

//go:cgo_unsafe_args
func _Cfunc_memset(p0 unsafe.Pointer, p1 _Ctype_int, p2 _Ctype_size_t) (r1 unsafe.Pointer) {
	_cgo_runtime_cgocall(_cgo_55dd9c1e1280_Cfunc_memset, uintptr(unsafe.Pointer(&p0)))
	if _Cgo_always_false {
		_Cgo_use(p0)
		_Cgo_use(p1)
		_Cgo_use(p2)
	}
	return
}
`
	stderr, output, err := convertCgoOffline(t, files)
	if err != nil {
		t.Fatalf("%v\n%s", err, stderr)
	}
	converted := readConverted(t, output, "fixture.cs")

	if want := "C.memset(p, (C.@int)(7), (Cꓸsize_t)(2));"; !strings.Contains(converted, want) {
		t.Errorf("the call does not read %q", want)
	}

	// The near misses. The typedef of a typedef stops at the name the parameter was declared at (Cꓸuint64_t); the hosted
	// probe 45908623601 read the pre-cure `(_Ctype_uint64_t)` and CS0246 on linux, darwin and windows. cmd/cgo declares
	// a typedef'd enum as a DEFINED type (`type _Ctype_go2cs_probe_color uint32`), never an alias, so it already read
	// `(C.go2cs_probe_color)` before the cure; its line guards that the cure leaves a defined C type's spelling alone.
	for _, want := range []string{"(Cꓸuint64_t)(0x123456789UL)", "(C.go2cs_probe_color)(4)"} {
		if !strings.Contains(converted, want) {
			t.Errorf("the near miss does not read %q; the calls read:\n%s", want, cgoLinesContaining(converted, "_into("))
		}
	}

	if strings.Contains(converted, "_Ctype_") {
		t.Errorf("a cmd/cgo Go name reached the C#")
	}
}

// cgoLinesContaining returns the lines of text that contain substr, for a failure message that shows what was emitted.
func cgoLinesContaining(text, substr string) string {
	var lines []string

	for _, line := range strings.Split(text, "\n") {
		if strings.Contains(line, substr) {
			lines = append(lines, strings.TrimSpace(line))
		}
	}

	return strings.Join(lines, "\n")
}

// The go-sqlite3 marshalling slice (docs/phase4/DESIGN-cgo-sqlite-marshalling.md), offline over cmd/cgo's shapes:
// A, a struct whose member is `void *` (go-sqlite3's sqlite3_go_col) crosses by pointer, its member copied as cgo checks
// a pointer member (GoCgoArgs.PointerMember); B, a typedef of `char *`, which cmd/cgo declares as a defined type over the
// pointer (`type _Ctype_T *_Ctype_char`: cmd/cgo writes `=` only where the printed type is itself a _Ctype_ name), is a
// pointer result and a pointer parameter, not a refused scalar.
func TestCgoSliceAVoidPointerMemberAndATypedefdPointerCross(t *testing.T) {
	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)

	for _, name := range []string{"go2cs_col_fill", "go2cs_fname", "go2cs_fname_len"} {
		cgoPhase2Libraries[name] = "libgo2csprobe.so"
		defer delete(cgoPhase2Libraries, name)
	}

	files["cgo_import.go"] = strings.Replace(cgoOfflineImportFile(cgoPhase2Libraries), "package cgophase2", "package cgophase3", 1)
	files["fixture.cgo1.go"] += `
func fillCol(c *_Ctype_struct_go2cs_col) _Ctype_int {
	return func() _Ctype_int{ _cgo0 := c; _cgoCheckPointer(_cgo0, nil); return _Cfunc_go2cs_col_fill(_cgo0); }()
}

func fileName() _Ctype_go2cs_fname {
	return _Cfunc_go2cs_fname()
}

func fileNameLen(f _Ctype_go2cs_fname) _Ctype_int {
	return func() _Ctype_int{ _cgo0 := f; _cgoCheckPointer(_cgo0, nil); return _Cfunc_go2cs_fname_len(_cgo0); }()
}
`
	stub := func(name, params, result, uses string) string {
		return "\n//go:cgo_import_static _cgo_55dd9c1e1280_Cfunc_" + name +
			"\n//go:linkname __cgofn__cgo_55dd9c1e1280_Cfunc_" + name + " _cgo_55dd9c1e1280_Cfunc_" + name +
			"\nvar __cgofn__cgo_55dd9c1e1280_Cfunc_" + name + " byte" +
			"\nvar _cgo_55dd9c1e1280_Cfunc_" + name + " = unsafe.Pointer(&__cgofn__cgo_55dd9c1e1280_Cfunc_" + name + ")\n" +
			"\n//go:cgo_unsafe_args\nfunc _Cfunc_" + name + "(" + params + ") (r1 " + result + ") {" +
			"\n\t_cgo_runtime_cgocall(_cgo_55dd9c1e1280_Cfunc_" + name + ", uintptr(unsafe.Pointer(&" + uses + ")))" +
			"\n\treturn\n}\n"
	}

	files["cgo_gotypes.go"] += `
type _Ctype_struct_go2cs_col struct {
	typ	_Ctype_int
	_	[4]byte
	ptr	unsafe.Pointer
	n	_Ctype_int
	_	[4]byte
}

type _Ctype_go2cs_fname *_Ctype_char
` + stub("go2cs_col_fill", "p0 *_Ctype_struct_go2cs_col", "_Ctype_int", "p0") +
		stub("go2cs_fname", "", "_Ctype_go2cs_fname", "r1") +
		stub("go2cs_fname_len", "p0 _Ctype_go2cs_fname", "_Ctype_int", "p0")

	stderr, output, err := convertCgoOffline(t, files)

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, lastLines(stderr, 12))
	}

	bindings := strings.ReplaceAll(readConverted(t, output, filepath.Join("linux-amd64", "cgophase3.cgo.cs")), "\r\n", "\n")

	if !strings.Contains(bindings, "m.ptr = (nint)global::go.golib.GoCgoArgs.VoidPointerMember(v.ptr);") || !strings.Contains(bindings, "GoCgoArgs.VoidPointerMemberBack((nuint)m.ptr, c, function)") {
		t.Errorf("the void * member ptr is not copied in and back as a pointer member:\n%s", cgoLinesContaining(bindings, "ptr"))
	}

	for _, want := range []string{"go2cs_fname(", "go2cs_fname_len("} {
		if !strings.Contains(bindings, want) {
			t.Errorf("the bindings have no wrapper for %s", want)
		}
	}
}
