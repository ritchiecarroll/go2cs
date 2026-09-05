// cgoUnsafeArgsLift_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// The //go:cgo_unsafe_args block lift's fixture guard (DESIGN-cgo-unsafe-args-block-lift.md §7): a
// synthetic module declaring the darwin runtime's shape — `libcCall(fn, arg unsafe.Pointer) int32`
// reached with the address of a function's first parameter under the directive — one function per
// shape the design names, and two that must be LEFT ALONE. Converted for linux/amd64 like its
// funnel-set siblings: the rule reads the directive and the call shape, never the target.
const cgoUnsafeArgsLiftFixture = `package main

import (
	"fmt"
	"runtime"
	"unsafe"
)

type timespec struct {
	tv_sec  int64
	tv_nsec int64
}

type pthread uintptr

func libcCall(fn, arg unsafe.Pointer) int32 { return 0 }

func funcPC(f func()) unsafe.Pointer { return nil }

func kevent_trampoline()       {}
func open_trampoline()         {}
func pthread_kill_trampoline() {}
func walltime_trampoline()     {}
func raise_trampoline()        {}

// shape (a): three integers, the trampoline reads all three through &kq
//
//go:cgo_unsafe_args
func kevent(kq int32, nch int32, nev int32) int32 {
	ret := libcCall(funcPC(kevent_trampoline), unsafe.Pointer(&kq))
	return ret
}

// shape (c): a *byte first parameter, an integer and a *timespec behind it
//
//go:cgo_unsafe_args
func open(name *byte, mode int32, ts *timespec) int32 {
	ret := libcCall(funcPC(open_trampoline), unsafe.Pointer(&name))
	runtime.KeepAlive(name)
	runtime.KeepAlive(ts)
	return ret
}

// a NAMED integer parameter (type pthread uintptr): the block field is the underlying width
//
//go:cgo_unsafe_args
func pthread_kill(t pthread, sig uint32) {
	libcCall(funcPC(pthread_kill_trampoline), unsafe.Pointer(&t))
}

// shape (b): the address of a LOCAL -- untouched; the dispatcher's per-symbol form table owns it
//
//go:cgo_unsafe_args
func walltime() (int64, int32) {
	var t timespec
	libcCall(funcPC(walltime_trampoline), unsafe.Pointer(&t))
	return t.tv_sec, int32(t.tv_nsec)
}

// the negative arm: NO directive, the same call shape -- left alone
func raise(sig uint32) {
	libcCall(funcPC(raise_trampoline), unsafe.Pointer(&sig))
}

func main() {
	var b byte
	var ts timespec
	fmt.Println(kevent(1, 2, 3), open(&b, 4, &ts))
	pthread_kill(pthread(7), 9)
	s, ns := walltime()
	raise(2)
	fmt.Println(s, ns)
}
`

// convertCgoUnsafeArgsLiftFixture converts the fixture for linux/amd64 and returns its emitted C#.
func convertCgoUnsafeArgsLiftFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/cgolift\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), cgoUnsafeArgsLiftFixture)

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

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "cgolift", "main.cs"))
}

// functionBody returns the emitted text of the named function: from its declaration line to the
// next top-level declaration, so an assertion about one function cannot be satisfied by another.
func liftFunctionBody(t *testing.T, mainCs, declaration string) string {
	t.Helper()

	start := strings.Index(mainCs, declaration)

	if start < 0 {
		t.Fatalf("no emitted declaration %q:\n%s", declaration, mainCs)
	}

	rest := mainCs[start+len(declaration):]
	end := strings.Index(rest, "\ninternal static ")

	if end < 0 {
		end = strings.Index(rest, "\n[GoType")
	}

	if end < 0 {
		return declaration + rest
	}

	return declaration + rest[:end]
}

// liftRequireBlock requires the emitted `[GoType("dyn")]` block declaration named `name` with exactly
// `fields` in order — compared LINE BY LINE with the indentation stripped, since a module conversion
// nests the package class one level deeper than the corpus's file-scoped form.
func liftRequireBlock(t *testing.T, mainCs, name string, fields []string) {
	t.Helper()

	header := "[GoType(\"dyn\")] internal partial struct " + name + " {"
	lines := strings.Split(mainCs, "\n")

	for i, line := range lines {
		if strings.TrimSpace(line) != header {
			continue
		}

		got := []string{}

		for j := i + 1; j < len(lines) && strings.TrimSpace(lines[j]) != "}"; j++ {
			got = append(got, strings.TrimSpace(lines[j]))
		}

		if strings.Join(got, "|") != strings.Join(fields, "|") {
			t.Errorf("%s: fields %q, expected %q — one field per parameter in Go's ABI0 order", name, got, fields)
		}

		return
	}

	t.Errorf("no emitted block declaration %q:\n%s", header, mainCs)
}

func liftRequireContains(t *testing.T, where, text, needle, why string) {
	t.Helper()

	if !strings.Contains(text, needle) {
		t.Errorf("%s: expected %q — %s\n%s", where, needle, why, text)
	}
}

func liftRequireAbsent(t *testing.T, where, text, needle, why string) {
	t.Helper()

	if strings.Contains(text, needle) {
		t.Errorf("%s: must not contain %q — %s\n%s", where, needle, why, text)
	}
}

// TestCgoUnsafeArgsLiftSynthesizesTheParameterBlock covers shapes (a) and (c) and the named-integer
// parameter: the block declaration in the corpus's lifted-args form, one field per parameter in
// Go's order with pointers as uintptr, the construction line at entry, the pinned block as the
// libcCall argument, the first parameter no longer heap-boxed, and the KeepAlive lines untouched.
func TestCgoUnsafeArgsLiftSynthesizesTheParameterBlock(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertCgoUnsafeArgsLiftFixture(t)

	// shape (a)
	liftRequireBlock(t, mainCs, "kevent_args", []string{"internal int32 kq;", "internal int32 nch;", "internal int32 nev;"})
	kevent := liftFunctionBody(t, mainCs, "internal static int32 kevent(int32 kq, int32 nch, int32 nev) {")
	liftRequireContains(t, "kevent", kevent, "ref var args = ref heap(new kevent_args(kq, nch, nev), out var Ꮡargs);", "the block is constructed at entry from the plain parameters")
	liftRequireContains(t, "kevent", kevent, "@unsafe.Pointer.FromPinnedBox(Ꮡargs)", "the libcCall receives the block's pinned box")
	liftRequireAbsent(t, "kevent", kevent, "Ꮡkq", "the consumed &kq must not heap-box the first parameter")
	liftRequireAbsent(t, "kevent", kevent, "kqʗp", "nor rename it to the boxed-parameter form")

	// shape (c)
	liftRequireBlock(t, mainCs, "open_args", []string{"internal uintptr name;", "internal int32 mode;", "internal uintptr ts;"})
	open := liftFunctionBody(t, mainCs, "internal static int32 open(ж<byte> Ꮡname, int32 mode, ж<timespec> Ꮡts) {")
	liftRequireContains(t, "open", open, "ref var args = ref heap(new open_args((uintptr)Ꮡname.OrTypedNil(), mode, (uintptr)Ꮡts.OrTypedNil()), out var Ꮡargs);", "each pointer field is minted through golib's address model from the parameter's box")
	liftRequireContains(t, "open", open, "@unsafe.Pointer.FromPinnedBox(Ꮡargs)", "the libcCall receives the block's pinned box")
	liftRequireAbsent(t, "open", open, "FromPinnedBox(Ꮡname)", "the consumed &name is no longer the argument")

	if n := strings.Count(open, "KeepAlive("); n != 2 {
		t.Errorf("open: expected the two KeepAlive lines untouched, found %d:\n%s", n, open)
	}

	// the named integer parameter
	liftRequireBlock(t, mainCs, "pthread_kill_args", []string{"internal uintptr t;", "internal uint32 sig;"})
	kill := liftFunctionBody(t, mainCs, "internal static void pthread_kill(pthread t, uint32 sig) {")
	liftRequireContains(t, "pthread_kill", kill, "new pthread_kill_args((uintptr)t, sig)", "the named wrapper converts to the field's width")
}

// TestCgoUnsafeArgsLiftLeavesTheOtherShapesAlone is the negative arm: a local's address under the
// directive (shape (b)) and the same call shape WITHOUT the directive keep today's emission —
// no synthesized block, the first parameter's own box as the argument.
func TestCgoUnsafeArgsLiftLeavesTheOtherShapesAlone(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertCgoUnsafeArgsLiftFixture(t)

	liftRequireAbsent(t, "walltime", mainCs, "walltime_args", "the address of a LOCAL is shape (b): the dispatcher's form table, never a block")
	walltime := liftFunctionBody(t, mainCs, "internal static (int64, int32) walltime() {")
	liftRequireContains(t, "walltime", walltime, "@unsafe.Pointer.FromPinnedBox(Ꮡt)", "the local's own box stays the argument")

	liftRequireAbsent(t, "raise", mainCs, "raise_args", "without the directive there is no contiguity guarantee and no lift")
	raise := liftFunctionBody(t, mainCs, "internal static void raise(uint32 sigʗp) {")
	liftRequireContains(t, "raise", raise, "ref var sig = ref heap(sigʗp, out var Ꮡsig);", "the first parameter stays heap-boxed for its address-of")
	liftRequireContains(t, "raise", raise, "@unsafe.Pointer.FromPinnedBox(Ꮡsig)", "and its box stays the argument")
}
