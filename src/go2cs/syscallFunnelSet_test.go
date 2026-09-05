// syscallFunnelSet_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Pins the SET syscallKeepAliveAnalysis.go intercepts — the membership question, not the emission
// shape (deferredSyscallFunnel_test.go's CONTROL B already pins the shape for syscall.Syscall).
//
// The defect this guards (measured 2026-09-02, the pin-LIFETIME census): the funnel set carried
// Syscall/Syscall6/9/12/15/18/N and omitted RawSyscall/RawSyscall6, which Go marks with the SAME
// //go:uintptrkeepalive directive (GOROOT go1.23.12, syscall/syscall_linux.go:50,58,69,91 — those
// four declarations are the whole of the directive outside cmd/). The omission is invisible to
// every standing gate: an unprotected `(uintptr)Ꮡx` argument is valid C# and compiles clean. It
// left eleven generated Linux wrappers handing the kernel a managed address with nothing holding
// the box that pins it — syscall.pipe2, EpollCtl, Getrusage, prlimit1, Settimeofday, Times,
// Getrlimit/setrlimit, getgroups, socketpair, getpeername/getsockname, Capget — reachable from
// os.Pipe, os/exec's ProcessState.SysUsage, os/user's getgroups and net's socketpair paths.
//
// Why the fixture converts for linux/amd64 REGARDLESS of host: RawSyscall/RawSyscall6 are declared
// on the unix platforms only (syscall/dll_windows.go has no such member), so a host-platform
// fixture would not type-check on Windows and the guard would silently degrade to a skip on the
// machine class that runs the corpus gates. The converter's own loader sets GOOS/GOARCH from
// targetPlatform (moduleConverter.go:168), so pinning the target makes this guard's verdict
// host-INDEPENDENT — the opposite trade from deferredSyscallFunnel_test.go, whose shape genuinely
// differs per platform (syscall.Syscall's arity) and so must be derived per host.
//
// The other half of the widening — internal/runtime/syscall.Syscall6, the Linux boundary's own
// bottom — cannot be reached by a fixture at all (an `internal/` path is not importable from a
// test module), so it is pinned by the corpus footprint instead: the two-seeded A/B for the linux
// target must move internal/runtime/syscall/linux/syscall_linux.cs's EpollCtl.

package main

import (
	"fmt"
	"go/ast"
	"go/build"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"io"
	"log"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// rawSyscallFunnelFixture is the Go source the guard converts. Each call carries its OWN trap
// variable so the assertions can name a single emitted statement without depending on argument
// order, arity or the converter's spelling of the pointer expression.
const rawSyscallFunnelFixture = `package main

import (
	"fmt"
	"syscall"
	"unsafe"
)

var (
	trapRaw6   uintptr
	trapRaw    uintptr
	trapPlain  uintptr
	trapTwoStep uintptr
	_zero      uintptr
)

// twoStepPointerArg is mksyscall's shape for a []byte argument — the pointer reaches the funnel
// through an unsafe.Pointer VARIABLE, which is what zsyscall_linux_amd64.go's read/write/pread/
// pwrite/recvfrom/sendto do (16 calls there, 13 in the darwin twin).
func twoStepPointerArg(fd uintptr, buf []byte) uintptr {
	var _p0 unsafe.Pointer
	if len(buf) > 0 {
		_p0 = unsafe.Pointer(&buf[0])
	} else {
		_p0 = unsafe.Pointer(&_zero)
	}
	r1, _, _ := syscall.Syscall(trapTwoStep, fd, uintptr(_p0), uintptr(len(buf)))
	return r1
}

// POSITIVE - RawSyscall6 with a pointer-derived argument. Go marks RawSyscall6
// //go:uintptrkeepalive; the CLR heap moves, so the box that pins the buffer must be held across
// the call or the kernel may write through storage the GC has since relocated.
func raw6PointerArg(buf []byte) uintptr {
	r1, _, _ := syscall.RawSyscall6(trapRaw6, 0, uintptr(unsafe.Pointer(&buf[0])), uintptr(len(buf)), 0, 0, 0)
	return r1
}

// POSITIVE - RawSyscall, the 3-argument member of the same directive set.
func rawPointerArg(p *byte) uintptr {
	r1, _, _ := syscall.RawSyscall(trapRaw, 0, uintptr(unsafe.Pointer(p)), 0)
	return r1
}

// CONTROL - a RawSyscall call whose arguments are all integers takes NO temp. The widening is
// about pointer-derived arguments, not about the callee's name: capturing here would be pure
// noise in the emission and would mean pointerDerivedArgSource had widened too.
func rawNoPointer(fd uintptr) uintptr {
	r1, _, _ := syscall.RawSyscall(trapPlain, fd, 1, 0)
	return r1
}

func main() {
	buf := make([]byte, 8)
	fmt.Println(raw6PointerArg(buf), rawPointerArg(&buf[0]), rawNoPointer(0), twoStepPointerArg(0, buf))
}
`

// convertRawSyscallFunnelFixture converts the fixture for linux/amd64 and returns its emitted C#.
func convertRawSyscallFunnelFixture(t *testing.T) string {
	t.Helper()

	return convertFunnelFixture(t, "example.com/rawfunnel", "rawfunnel", rawSyscallFunnelFixture)
}

// convertFunnelFixture converts a one-file module for linux/amd64 and returns its emitted main.cs;
// modulePath is the go.mod module path and dir its last element (where the emission lands).
func convertFunnelFixture(t *testing.T, modulePath, dir, source string, opts ...func(*Options)) string {
	t.Helper()

	root, err := convertFunnelModule(t, modulePath, source, opts...)

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(root, "out", "src", "example.com", dir, "main.cs"))
}

// convertFunnelModule runs the converter over a one-file module and returns the temp root it
// converted into, with a conversion failure returned as the error rather than failing the test,
// so a guard whose expected outcome is a REFUSAL can assert on its text. A converter panic is
// such a failure: debugMode lets it through the per-file recovery, and the module driver then
// recovers it per PACKAGE into a `panic converting …` WARNING on the log while ConvertModule
// itself returns nil — so the log is captured and that line becomes the error.
func convertFunnelModule(t *testing.T, modulePath, source string, opts ...func(*Options)) (string, error) {
	t.Helper()

	logged := &strings.Builder{}
	originalLog := log.Writer()
	log.SetOutput(io.MultiWriter(originalLog, logged))
	defer log.SetOutput(originalLog)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+modulePath+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), source)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	for _, opt := range opts {
		opt(&options)
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)
	err := converter.ConvertModule(appDir)

	if err == nil {
		for _, line := range strings.Split(logged.String(), "\n") {
			if strings.Contains(line, "panic converting ") {
				return root, fmt.Errorf("%s", strings.TrimSpace(line))
			}
		}
	}

	return root, err
}

// funnelTempAtCallSite is the emitted cast of a captured box at the call site: `(uintptr)ᴋ7`.
var funnelTempAtCallSite = regexp.MustCompile(`\(uintptr\)(ᴋ\d+)`)

// callStatementLine returns the one emitted line carrying `marker`, and the lines around it.
func callStatementLine(t *testing.T, mainCs, marker string) (line string, index int, lines []string) {
	t.Helper()

	lines = strings.Split(mainCs, "\n")

	for i, candidate := range lines {
		if strings.Contains(candidate, marker) {
			return strings.TrimSpace(candidate), i, lines
		}
	}

	t.Fatalf("no emitted statement carrying %q:\n%s", marker, mainCs)

	return "", -1, nil
}

// assertKeepAlivePair requires the statement carrying `marker` to cast a captured temp at the call
// site, to have that temp DECLARED above it, and to be followed by the temp's GC.KeepAlive — the
// three halves of the uintptrkeepalive emission, asserted independently so a partial regression
// (a temp with no KeepAlive, or a KeepAlive whose temp was never hoisted) names which half moved.
func assertKeepAlivePair(t *testing.T, mainCs, marker string) {
	t.Helper()

	line, index, lines := callStatementLine(t, mainCs, marker)
	matches := funnelTempAtCallSite.FindAllStringSubmatch(line, -1)

	if len(matches) == 0 {
		t.Errorf("the pointer-derived argument of %s is not routed through a captured box temp — the uintptrkeepalive contract is not applied to this callee:\n    %s", marker, line)
		return
	}

	for _, match := range matches {
		temp := match[1]
		declaration := fmt.Sprintf("var %s = ", temp)
		keepAlive := fmt.Sprintf("System.GC.KeepAlive(%s);", temp)

		if !strings.Contains(strings.Join(lines[:index], "\n"), declaration) {
			t.Errorf("temp %s is cast at the %s call site but never declared above it:\n    %s", temp, marker, line)
		}

		if !strings.Contains(strings.Join(lines[index+1:], "\n"), keepAlive) {
			t.Errorf("temp %s is cast at the %s call site but never kept alive after the statement — the box is unreachable the instant the argument is evaluated:\n    %s", temp, marker, line)
		}
	}
}

// TestRawSyscallFunnelKeepsItsPointerArgumentAlive is the positive: Go's //go:uintptrkeepalive set
// is RawSyscall, RawSyscall6, Syscall and Syscall6, and the converter's interception must cover all
// four. Red before the widening — RawSyscall/RawSyscall6 fell through to the general call path,
// which renders the argument as a bare `(uintptr)Ꮡ(buf, 0)` with nothing left referencing the box.
func TestRawSyscallFunnelKeepsItsPointerArgumentAlive(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRawSyscallFunnelFixture(t)

	assertKeepAlivePair(t, mainCs, "RawSyscall6(trapRaw6")
	assertKeepAlivePair(t, mainCs, "RawSyscall(trapRaw,")
}

// TestRawSyscallFunnelLeavesIntegerArgumentsAlone is the SCOPE half. The interception is keyed on
// the ARGUMENT shape (pointerDerivedArgSource) as much as on the callee, so an all-integer call
// into a now-intercepted callee must emit exactly what it emitted before: no temp, no KeepAlive.
// Without this, a widening of pointerDerivedArgSource would ride in unnoticed behind the set
// change — the two are separate predicates and this guard measures only one of them.
func TestRawSyscallFunnelLeavesIntegerArgumentsAlone(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRawSyscallFunnelFixture(t)
	line, _, _ := callStatementLine(t, mainCs, "RawSyscall(trapPlain")

	if strings.Contains(line, "ᴋ") {
		t.Errorf("an all-integer RawSyscall call captured a temp — the interception widened past pointer-derived arguments:\n    %s", line)
	}
}

// The TWO-STEP shape, added 2026-09-04 with the pin-lifetime cut. Go's own generated wrappers hand a
// []byte to the funnel through an `unsafe.Pointer` VARIABLE, never inline — and this file's
// predicate matched only the inline form, on a reading of unsafe.Pointer's rule (4) that took "the
// conversion must appear in the argument list" to exclude an intermediate variable. The GO COMPILER
// disagrees, and it is the authority: escape.rewriteArgument (cmd/compile/internal/escape/call.go)
// keeps any argument alive that is an OCONVNOP whose OPERAND TYPE is unsafe.Pointer and whose own
// type is uintptr — a test on the type, not on the syntax below it.
//
// What the gap cost, measured before the fix: every converted read and write handed the kernel a
// managed buffer address with nothing holding the box that pins it, and sixteen concurrent TLS
// connections over the converted stack died SIGSEGV in five seconds, 3/3, recovering when the box
// was held. Nothing in the standing ladder could see it — the emission compiles, is byte-identical
// in shape to a correct one, and only a collection landing INSIDE the kernel's window is wrong.
func TestTwoStepPointerArgumentIsKeptAliveAcrossTheFunnel(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRawSyscallFunnelFixture(t)
	assertKeepAlivePair(t, mainCs, "Syscall(trapTwoStep")
}

// The other half of the same cut: keeping the unsafe.Pointer VALUE alive is worth nothing unless the
// value carries the box that holds the pin. `new @unsafe.Pointer(box)` binds the implicit
// ж→uintptr conversion into Pointer(uintptr), which pins and retains nothing; the mint must be the
// retaining door instead. Asserted on the emission rather than on golib so the two halves cannot
// drift apart silently — a fixed golib with an unchanged emission is still the defect.
func TestPointerMintFromABoxTakesTheRetainingDoor(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRawSyscallFunnelFixture(t)

	if !strings.Contains(mainCs, "@unsafe.Pointer.FromPinnedBox(") {
		t.Errorf("no `unsafe.Pointer(&x)` mint took the retaining door — the pointer carries no pin holder:\n%s", mainCs)
	}

	if bare := regexp.MustCompile(`new @unsafe\.Pointer\(Ꮡ`).FindAllString(mainCs, -1); len(bare) > 0 {
		t.Errorf("%d mint(s) still construct a Pointer directly from a box (%v) — the box carrying the pin is unreachable the instant the mint returns", len(bare), bare)
	}
}

// ---- Q49: the bridged-wrapper argument and darwin's funnel names (2026-09-05) ---------------------
//
// The pin-lifetime class's MANAGED-callee member: `f(g(unsafe.Pointer(&x)))` where g returns
// unsafe.Pointer. The converter bridges g's Pointer result to a number (`(uintptr)g(…)`), so the
// retained box is stripped before f's `@unsafe.Pointer` parameter re-wraps it; with nothing else
// referencing Ꮡx past the mint, a collection under the call retires the provenance entry and the
// callee's resolve refuses by name (runtime's int32Hash inside TestSmhasherWindowed). The remedy
// names the frame-minted box for a KeepAlive after the statement — the funnel's contract one
// callee kind over — and these guards pin its three statement shapes and its two scope controls.
const bridgedWrapperFixture = `package main

import "unsafe"

//go:noinline
func noescapeLike(p unsafe.Pointer) unsafe.Pointer {
	x := uintptr(p)
	return unsafe.Pointer(x ^ 0)
}

//go:noinline
func consume(p unsafe.Pointer, seed uintptr) uintptr {
	return uintptr(p) + seed
}

func wrappedReturn(i uint32, seed uintptr) uintptr {
	return consume(noescapeLike(unsafe.Pointer(&i)), seed)
}

func wrappedAssign(i uint32) uintptr {
	r := consume(noescapeLike(unsafe.Pointer(&i)), 1)
	return r
}

func wrappedExpr(i uint32) {
	consume(noescapeLike(unsafe.Pointer(&i)), 2)
}

func bareArg(i uint32) uintptr {
	return consume(unsafe.Pointer(&i), 3)
}

var global uint32

func wrappedGlobal() uintptr {
	return consume(noescapeLike(unsafe.Pointer(&global)), 4)
}

//go:noinline
func consumeWith(p unsafe.Pointer, f func()) uintptr {
	f()
	return uintptr(p)
}

func wrappedConversion(i uint32) uintptr {
	n := uintptr(noescapeLike(unsafe.Pointer(&i)))
	return n
}

func wrappedCondition(i uint32) uintptr {
	if consume(noescapeLike(unsafe.Pointer(&i)), 5) == 0 {
		return 1
	}
	return 0
}

func wrappedAroundLiteral(i uint32) uintptr {
	return consumeWith(noescapeLike(unsafe.Pointer(&i)), func() {
		println("inner")
	})
}

func wrappedInLiteral(i uint32) uintptr {
	f := func() uintptr {
		j := i
		return consume(noescapeLike(unsafe.Pointer(&j)), 6)
	}
	return f()
}

func main() {
	println(wrappedReturn(1, 2), wrappedAssign(3), bareArg(4), wrappedGlobal())
	println(wrappedConversion(6), wrappedCondition(7), wrappedAroundLiteral(8), wrappedInLiteral(9))
	wrappedExpr(5)
}
`

// forClauseFixture is the one placement the drain cannot serve: a `for` INIT (or POST) clause is
// emitted inside the C# for-header, so a box it names has no statement to be kept alive after.
const forClauseFixture = `package main

import "unsafe"

//go:noinline
func noescapeLike(p unsafe.Pointer) unsafe.Pointer {
	x := uintptr(p)
	return unsafe.Pointer(x ^ 0)
}

//go:noinline
func consume(p unsafe.Pointer, seed uintptr) uintptr {
	return uintptr(p) + seed
}

func inForInit(i uint32) uintptr {
	var total uintptr
	for n := consume(noescapeLike(unsafe.Pointer(&i)), 1); n < 3; n++ {
		total += n
	}
	return total
}

func main() {
	println(inForInit(1))
}
`

func convertBridgedWrapperFixture(t *testing.T) string {
	t.Helper()

	return convertFunnelFixture(t, "example.com/bridged", "bridged", bridgedWrapperFixture)
}

// functionBody returns the emitted lines of the C# method named name (from its declaration line to
// the closing brace at the same indentation), so an assertion about one Go function cannot be
// satisfied by a KeepAlive emitted for its neighbour.
func functionBody(t *testing.T, mainCs, name string) []string {
	t.Helper()

	lines := strings.Split(mainCs, "\n")

	for i, line := range lines {
		if !strings.Contains(line, " "+name+"(") || !strings.Contains(line, "static") {
			continue
		}

		indent := line[:len(line)-len(strings.TrimLeft(line, " \t"))]

		for j := i + 1; j < len(lines); j++ {
			if strings.HasPrefix(lines[j], indent+"}") {
				return lines[i : j+1]
			}
		}
	}

	t.Fatalf("no emitted method named %s:\n%s", name, mainCs)

	return nil
}

func keepAliveCount(lines []string, box string) int {
	count := 0

	for _, line := range lines {
		if strings.Contains(line, "System.GC.KeepAlive("+box+");") {
			count++
		}
	}

	return count
}

func TestBridgedWrapperArgumentKeepsItsFrameMintedBoxAlive(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertBridgedWrapperFixture(t)

	// The RETURN shape: the call is hoisted into a temp, the KeepAlive lands between the call and
	// the return, and the return carries the temp — the alg.go hash helpers' exact shape.
	body := functionBody(t, mainCs, "wrappedReturn")
	joined := strings.Join(body, "\n")

	if keepAliveCount(body, "Ꮡi") != 1 {
		t.Errorf("wrappedReturn: expected exactly one System.GC.KeepAlive(Ꮡi) in the body:\n%s", joined)
	}

	callIndex, keepIndex, returnIndex := -1, -1, -1

	for i, line := range body {
		switch {
		case strings.Contains(line, "consume((uintptr)noescapeLike(@unsafe.Pointer.FromPinnedBox(Ꮡi))"):
			callIndex = i
		case strings.Contains(line, "System.GC.KeepAlive(Ꮡi);"):
			keepIndex = i
		case strings.HasPrefix(strings.TrimSpace(line), "return "):
			returnIndex = i
		}
	}

	if callIndex == -1 || keepIndex == -1 || returnIndex == -1 || !(callIndex < keepIndex && keepIndex < returnIndex) {
		t.Errorf("wrappedReturn: expected call (%d) < KeepAlive (%d) < return (%d):\n%s", callIndex, keepIndex, returnIndex, joined)
	}

	if callIndex != -1 && !strings.Contains(body[callIndex], "var ") {
		t.Errorf("wrappedReturn: the bridged call must be hoisted into a temp so the KeepAlive can precede the return:\n%s", body[callIndex])
	}

	// The ASSIGNMENT and EXPRESSION shapes drain after the statement.
	for _, name := range []string{"wrappedAssign", "wrappedExpr"} {
		body := functionBody(t, mainCs, name)

		if keepAliveCount(body, "Ꮡi") != 1 {
			t.Errorf("%s: expected exactly one System.GC.KeepAlive(Ꮡi) after the statement:\n%s", name, strings.Join(body, "\n"))
		}
	}
}

// TestBridgedWrapperArgumentScopeControls is the SCOPE half: a BARE unsafe.Pointer(&x) argument
// carries no bridge (the Pointer retains its box through the call) and a package-level variable's
// box is a static field alive for the process — neither may record a KeepAlive, or the arm has
// widened past the class the census measured.
func TestBridgedWrapperArgumentScopeControls(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertBridgedWrapperFixture(t)

	if body := functionBody(t, mainCs, "bareArg"); keepAliveCount(body, "Ꮡi") != 0 {
		t.Errorf("bareArg: a bare unsafe.Pointer(&x) argument is not bridged and must not record a KeepAlive:\n%s", strings.Join(body, "\n"))
	}

	if body := functionBody(t, mainCs, "wrappedGlobal"); strings.Contains(strings.Join(body, "\n"), "System.GC.KeepAlive(") {
		t.Errorf("wrappedGlobal: a package-level variable is not frame-minted and must not record a KeepAlive:\n%s", strings.Join(body, "\n"))
	}

	// A CONVERSION over the wrapper — `n := uintptr(noescape(unsafe.Pointer(&i)))` — STORES the
	// number; a KeepAlive after the store holds the box exactly as far as the store, so the
	// stored-number shape is outside the class and the arm must not claim it (nine such sites in
	// runtime/os_windows.go and runtime/proc.go were reached before the exclusion, 2026-09-05).
	if body := functionBody(t, mainCs, "wrappedConversion"); strings.Contains(strings.Join(body, "\n"), "System.GC.KeepAlive(") {
		t.Errorf("wrappedConversion: a conversion is not a call — the stored-number shape must not record a KeepAlive:\n%s", strings.Join(body, "\n"))
	}
}

// TestBridgedWrapperArgumentPlacement pins WHERE the KeepAlive lands for the shapes the Windows
// leg of the cut's own two-seeded diff surfaced (2026-09-05): a box named inside an `if`
// condition, a literal converted while the enclosing statement has already named a box, and a
// box named inside a literal's own body. Each is a frame question — the KeepAlive must sit in the
// frame that minted the box, after the call, on a path the call reaches.
func TestBridgedWrapperArgumentPlacement(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertBridgedWrapperFixture(t)

	// CONDITION: the box named in the `if` condition is kept alive after the call and by exactly
	// one KeepAlive — the drain lands it at the first statement after the call (inside the body,
	// before its `return`), which keeps the local live across the call on every path.
	body := functionBody(t, mainCs, "wrappedCondition")

	if keepAliveCount(body, "Ꮡi") != 1 {
		t.Errorf("wrappedCondition: expected exactly one System.GC.KeepAlive(Ꮡi):\n%s", strings.Join(body, "\n"))
	}

	callIndex, keepIndex := -1, -1

	for i, line := range body {
		switch {
		case strings.Contains(line, "consume((uintptr)noescapeLike(@unsafe.Pointer.FromPinnedBox(Ꮡi))"):
			callIndex = i
		case strings.Contains(line, "System.GC.KeepAlive(Ꮡi);"):
			keepIndex = i
		}
	}

	if callIndex == -1 || keepIndex == -1 || keepIndex <= callIndex {
		t.Errorf("wrappedCondition: expected the KeepAlive (%d) after the condition's call (%d):\n%s", keepIndex, callIndex, strings.Join(body, "\n"))
	}

	// AROUND a literal: the enclosing return names Ꮡi BEFORE its literal argument is converted,
	// so without the frame isolation the literal's first statement drains it INSIDE the lambda.
	// The KeepAlive must sit in the OUTER frame — at the hoisted call's own indentation, after the
	// literal's body — and nowhere inside it.
	body = functionBody(t, mainCs, "wrappedAroundLiteral")

	if keepAliveCount(body, "Ꮡi") != 1 {
		t.Errorf("wrappedAroundLiteral: expected exactly one System.GC.KeepAlive(Ꮡi):\n%s", strings.Join(body, "\n"))
	}

	callIndent, keepIndent, innerIndex, keepIndex := "", "", -1, -1

	for i, line := range body {
		switch {
		case strings.Contains(line, "consumeWith("):
			callIndent = leadingSpace(line)
		case strings.Contains(line, "System.GC.KeepAlive(Ꮡi);"):
			keepIndent = leadingSpace(line)
			keepIndex = i
		case strings.Contains(line, "\"inner\""):
			innerIndex = i
		}
	}

	if keepIndex == -1 || innerIndex == -1 || keepIndex < innerIndex || keepIndent != callIndent {
		t.Errorf("wrappedAroundLiteral: the KeepAlive must land in the OUTER frame after the literal (keep %d, inner %d, keep indent %q vs call indent %q):\n%s", keepIndex, innerIndex, keepIndent, callIndent, strings.Join(body, "\n"))
	}

	// INSIDE a literal: the box is minted and named in the literal's own frame and drained there,
	// between the hoisted call and the literal's `return`.
	body = functionBody(t, mainCs, "wrappedInLiteral")

	if keepAliveCount(body, "Ꮡj") != 1 {
		t.Errorf("wrappedInLiteral: expected exactly one System.GC.KeepAlive(Ꮡj):\n%s", strings.Join(body, "\n"))
	}

	callIndex, keepIndex, returnIndex := -1, -1, -1

	for i, line := range body {
		switch {
		case strings.Contains(line, "consume((uintptr)noescapeLike(@unsafe.Pointer.FromPinnedBox(Ꮡj))"):
			callIndex = i
		case strings.Contains(line, "System.GC.KeepAlive(Ꮡj);"):
			keepIndex = i
		case keepIndex != -1 && returnIndex == -1 && strings.HasPrefix(strings.TrimSpace(line), "return "):
			returnIndex = i
		}
	}

	if callIndex == -1 || keepIndex == -1 || returnIndex == -1 || !(callIndex < keepIndex && keepIndex < returnIndex) {
		t.Errorf("wrappedInLiteral: expected call (%d) < KeepAlive (%d) < return (%d) inside the literal:\n%s", callIndex, keepIndex, returnIndex, strings.Join(body, "\n"))
	}
}

// leadingSpace returns a line's indentation.
func leadingSpace(line string) string {
	return line[:len(line)-len(strings.TrimLeft(line, " \t"))]
}

// TestBridgedWrapperInForClauseIsRefused pins the one placement the statement drain cannot
// serve: a box named by a `for` INIT (or POST) clause has no statement to be kept alive after —
// the clause is emitted inside the for-header — so the converter refuses by name rather than
// splicing a KeepAlive into the header. debugMode lets the refusal through the per-file recovery;
// the module driver reports it as the conversion error.
func TestBridgedWrapperInForClauseIsRefused(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	_, err := convertFunnelModule(t, "example.com/forclause", forClauseFixture, func(o *Options) { o.debugMode = true })

	if err == nil {
		t.Fatal("a bridged-wrapper argument in a for INIT clause converted without refusal: its KeepAlive has no statement to follow")
	}

	if !strings.Contains(err.Error(), "@rejectForClauseKeepAlive") {
		t.Fatalf("the for-clause shape was refused for another reason: %v", err)
	}
}

// TestUndrainedKeepAliveAtAFrameBoundaryIsLoud is the unit arm of the frame-boundary contract:
// no statement shape can reach assertNoPendingKeepAlive with a non-empty list (visitStmt drains
// every statement), so this is what proves the assertion fires when a future producer does.
func TestUndrainedKeepAliveAtAFrameBoundaryIsLoud(t *testing.T) {
	v := &Visitor{pendingSyscallKeepAlive: []string{"Ꮡx"}}

	recovered := func() (r any) {
		defer func() { r = recover() }()

		v.assertNoPendingKeepAlive("func probe")

		return nil
	}()

	if recovered == nil || !strings.Contains(fmt.Sprint(recovered), "@assertNoPendingKeepAlive") {
		t.Fatalf("an undrained KeepAlive box at a frame boundary was not refused: %v", recovered)
	}

	v.pendingSyscallKeepAlive = nil
	v.assertNoPendingKeepAlive("func probe") // an empty list is the normal case and must be silent
}

// TestDarwinTrampolineFunnelsAreInTheSet pins the darwin half of Q49 at the predicate: the
// lowercase libc trampoline funnels declared by package syscall on darwin are funnel calls, and a
// same-named function in any other package is not. Type-checked from a synthetic package rather
// than a module fixture because the darwin funnels are unexported — no fixture can call them.
func TestDarwinTrampolineFunnelsAreInTheSet(t *testing.T) {
	src := `package syscall

import "unsafe"

type Errno uintptr

func syscall(fn, a1, a2, a3 uintptr) (r1, r2 uintptr, err Errno)
func syscall6(fn, a1, a2, a3, a4, a5, a6 uintptr) (r1, r2 uintptr, err Errno)
func rawSyscall(fn, a1, a2, a3 uintptr) (r1, r2 uintptr, err Errno)
func other(fn, a1, a2, a3 uintptr) (r1, r2 uintptr, err Errno)

func use(fn uintptr, x uint32) {
	syscall(fn, uintptr(unsafe.Pointer(&x)), 0, 0)
	syscall6(fn, uintptr(unsafe.Pointer(&x)), 0, 0, 0, 0, 0)
	rawSyscall(fn, uintptr(unsafe.Pointer(&x)), 0, 0)
	other(fn, uintptr(unsafe.Pointer(&x)), 0, 0)
}
`
	calls := funnelCallsOf(t, "syscall", src)

	for _, name := range []string{"syscall", "syscall6", "rawSyscall"} {
		if !calls[name] {
			t.Errorf("darwin's %s is not recognised as a funnel: its pointer-derived arguments stay unheld on darwin", name)
		}
	}

	if calls["other"] {
		t.Errorf("a non-funnel in package syscall was recognised as a funnel")
	}

	// The same spellings in ANOTHER package are not funnels — the match is package-qualified.
	foreign := funnelCallsOf(t, "notsyscall", strings.Replace(src, "package syscall", "package notsyscall", 1))

	for name, matched := range foreign {
		if matched {
			t.Errorf("%s in package notsyscall was recognised as a funnel — the set must stay package-qualified", name)
		}
	}

	// crypto/x509/internal/macos declares its OWN `syscall` funnel (corefoundation.go:208, with a
	// float argument no other funnel takes) and calls it with `uintptr(unsafe.Pointer(&value))`
	// in CFNumberGetValue — the one raw pointer-derived funnel argument outside package syscall
	// on darwin (measured 2026-09-05). The path set must admit it, and only at that path.
	macosSrc := `package macos

import "unsafe"

func syscall(fn, a1, a2, a3, a4, a5 uintptr, f1 float64) uintptr

func use(fn uintptr, x int64) {
	syscall(fn, uintptr(unsafe.Pointer(&x)), 0, 0, 0, 0, 0)
}
`

	if calls := funnelCallsOf(t, "crypto/x509/internal/macos", macosSrc); !calls["syscall"] {
		t.Errorf("crypto/x509/internal/macos's own syscall funnel is not recognised: CFNumberGetValue's pointer-derived argument stays unheld")
	}

	if calls := funnelCallsOf(t, "crypto/x509/internal/notmacos", strings.Replace(macosSrc, "package macos", "package notmacos", 1)); calls["syscall"] {
		t.Errorf("a syscall funnel outside the path set was recognised — the set must stay package-qualified")
	}
}

// funnelCallsOf type-checks src as package path pkgPath and reports, per callee name, whether
// syscallFunnelCall recognised its call.
func funnelCallsOf(t *testing.T, pkgPath, src string) map[string]bool {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, pkgPath+".go", src, 0)

	if err != nil {
		t.Fatalf("parse: %v", err)
	}

	info := &types.Info{Uses: map[*ast.Ident]types.Object{}, Types: map[ast.Expr]types.TypeAndValue{}, Defs: map[*ast.Ident]types.Object{}}
	config := types.Config{Importer: importer.Default()}

	if _, err := config.Check(pkgPath, fset, []*ast.File{file}, info); err != nil {
		t.Fatalf("type-check: %v", err)
	}

	calls := map[string]bool{}

	ast.Inspect(file, func(node ast.Node) bool {
		if call, ok := node.(*ast.CallExpr); ok {
			if ident, ok := call.Fun.(*ast.Ident); ok {
				if _, isFunc := info.Uses[ident].(*types.Func); isFunc {
					calls[ident.Name] = syscallFunnelCall(info, call)
				}
			}
		}

		return true
	})

	return calls
}
