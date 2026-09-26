// linknameForward_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestRecurseLinknameForwarder guards the cross-package //go:linkname forwarder: a bodyless function
// carrying `//go:linkname <local> <pkg>.<func>` must convert to a forwarder BODY that calls the
// target (bridging num:uintptr types through uintptr), not a throwing `partial` stub. This is how
// golang.org/x/sys/windows's LazyDLL/LazyProc reach syscall.loadlibrary/getprocaddress; a stub there
// left DLL loading dead. The fixture mirrors that exact shape (a *uint16 pass-through parameter and
// two num:uintptr results). Transpile-only: the emitted C# references syscall (which the fixture does
// not import), so it is asserted as text, never compiled.
func TestRecurseLinknameForwarder(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real -recurse converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/lnapp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport _ \"unsafe\"\n\ntype Handle uintptr\ntype Errno uintptr\n\n"+
			"//go:linkname fwd syscall.loadlibrary\nfunc fwd(filename *uint16) (handle Handle, err Errno)\n\n"+
			"//go:linkname notfwd runtime.reflectcall\nfunc notfwd(x uintptr)\n\n"+
			"func main() {\n\t_, _ = fwd(nil)\n\tnotfwd(0)\n}\n")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "lnapp", "main.cs"))

	// The forwarder BODY calls the WHITELISTED linkname target with the parameter passed through.
	// Fully qualified: a linkname forwarder's file need not IMPORT the target's package (that is
	// the whole point of //go:linkname), so no using alias exists to bind a short spelling — the
	// r35-time tzdata forwarder made this the emission rule for never-imported target packages.
	if !strings.Contains(mainCs, "go.syscall_package.loadlibrary(filename)") {
		t.Errorf("linkname forwarder body missing its target call (emitted a stub?):\n%s", mainCs)
	}

	// A NON-whitelisted target (an unimplemented runtime intrinsic — there is no C# runtime.reflectcall)
	// must NOT be forwarded; it stays a bodyless stub so the package still compiles.
	if strings.Contains(mainCs, "runtime.reflectcall") {
		t.Errorf("non-whitelisted linkname target runtime.reflectcall was forwarded (should stay a stub):\n%s", mainCs)
	}

	// Both num:uintptr results are bridged back through uintptr to the local result types.
	if !strings.Contains(mainCs, "(Handle)(uintptr)") || !strings.Contains(mainCs, "(Errno)(uintptr)") {
		t.Errorf("linkname forwarder missing the uintptr result bridge:\n%s", mainCs)
	}

	// It must NOT be emitted as a bodyless `partial` stub (the pre-fix behavior).
	if strings.Contains(mainCs, "fwd(ж<uint16> filename);") {
		t.Errorf("linkname func emitted as a bodyless partial stub, not a forwarder:\n%s", mainCs)
	}

	// The forwarder is not a Go frame: Go binds the pull to the target's symbol, so no frame of the
	// puller exists. It is marked [StackTraceHidden], which runtime.Callers skips (managed_impl.cs,
	// isGoSourceFrame) and Exception.StackTrace omits.
	if !strings.Contains(mainCs, "[global::System.Diagnostics.StackTraceHidden] public static (Handle handle, Errno err) fwd(") &&
		!strings.Contains(mainCs, "[global::System.Diagnostics.StackTraceHidden] internal static (Handle handle, Errno err) fwd(") {
		t.Errorf("linkname forwarder is not marked [StackTraceHidden]:\n%s", mainCs)
	}
}

// TestRecurseLinknameForwardDefinition guards the forward rows whose Go symbol is DEFINED UNDER
// ANOTHER NAME (linknameForwardDefinitions) -- time's shape: `//go:linkname legacyAbsClock
// time.absClock` gives the symbol time.absClock its body in a func named legacyAbsClock, and
// time_test pulls the symbol by the name the definition gives it, not by the func's own name. The
// C# class has no member called absClock, so a forwarder spelled from the symbol calls nothing.
//
// Four properties, one per fixture row:
//
//   - a symbol with a FREE-FUNCTION name forwards to the definition's C# name;
//   - a symbol with a METHOD-SHAPED name (time.Time.abs, pkg.T.abs) forwards to the definition too,
//     and resolves the definition's PACKAGE -- splitting the symbol at its last dot names a package
//     "pkg.T" that does not exist;
//   - each definition is emitted `public`, because the forwarder calls it from another assembly and
//     its Go name is unexported;
//   - a forward row with NO definition row still forwards to the symbol's own name, and a func in
//     the defining package that no row names keeps its Go accessibility -- the definitions map is
//     consulted for its own rows only.
//
// The registries are keyed by stdlib import paths, so the fixture's rows are injected for the
// duration of the test rather than parked in production. Transpile-only: the emitted C# is asserted
// as text, never compiled.
//
// RED PROOF: drop the linknameForwardDefinitions lookup from funcLinknameForward, or the
// linknameForwardDefinitionSources arm from packageFuncAccess -- the first two or the third
// assertion group goes red respectively.
func TestRecurseLinknameForwardDefinition(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real -recurse converter over a module fixture")
	}

	const (
		pullerPath = "example.com/lnfwd"
		defPath    = "example.com/lnfwd/def"
	)

	targets := []string{defPath + ".T.abs", defPath + ".clock", defPath + ".plainTarget"}

	definitions := map[string]string{
		defPath + ".T.abs": defPath + ".legacyTAbs",
		defPath + ".clock": defPath + ".legacyClock",
	}

	for _, target := range targets {
		linknameForwardTargets[target] = true
	}

	for target, definition := range definitions {
		linknameForwardDefinitions[target] = definition
		linknameForwardDefinitionSources[definition] = true
	}

	defer func() {
		for _, target := range targets {
			delete(linknameForwardTargets, target)
		}

		for target, definition := range definitions {
			delete(linknameForwardDefinitions, target)
			delete(linknameForwardDefinitionSources, definition)
		}
	}()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+pullerPath+"\n\ngo 1.23\n")

	// The PULLING side -- time_test's shape: bodyless funcs under two-arg directives naming the
	// symbols, one of them method-shaped.
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t_ \"unsafe\"\n\n\t\""+defPath+"\"\n)\n\n"+
			"//go:linkname tAbs "+defPath+".T.abs\nfunc tAbs(def.T) uint64\n\n"+
			"//go:linkname clock "+defPath+".clock\nfunc clock(uint64) (hour, min int)\n\n"+
			"//go:linkname plain "+defPath+".plainTarget\nfunc plain() int\n\n"+
			"func main() {\n\th, m := clock(tAbs(def.New(1)))\n\tprintln(h, m, plain(), def.Use())\n}\n")

	// The DEFINING side -- time's shape: each symbol's body lives in a func of another name under a
	// two-arg directive naming the defining package's own symbol. plainTarget is the control: an
	// ordinary handle-opened target whose func name IS the symbol.
	writeModuleFile(t, filepath.Join(appDir, "def", "def.go"),
		"package def\n\nimport _ \"unsafe\"\n\ntype T struct{ n uint64 }\n\n"+
			"func New(n uint64) T { return T{n} }\n\n"+
			"//go:linkname legacyTAbs "+defPath+".T.abs\nfunc legacyTAbs(t T) uint64 { return t.n + 1 }\n\n"+
			"//go:linkname legacyClock "+defPath+".clock\nfunc legacyClock(abs uint64) (hour, min int) { return int(abs), int(abs) + 1 }\n\n"+
			"//go:linkname plainTarget\nfunc plainTarget() int { return 7 }\n\n"+
			"func untouched() int { return 0 }\n\n"+
			"func Use() int { return untouched() }\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
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

	outRoot := filepath.Join(options.go2csPath, "src", "example.com", "lnfwd")
	mainCs := readGenerated(t, filepath.Join(outRoot, "main.cs"))
	defCs := readGenerated(t, filepath.Join(outRoot, "def", "def.cs"))

	// The free-function symbol forwards to its definition, not to a member named after the symbol.
	if !strings.Contains(mainCs, ".legacyClock(") {
		t.Errorf("clock did not forward to its definition legacyClock:\n%s", mainCs)
	}

	if strings.Contains(mainCs, ".clock(") {
		t.Errorf("clock forwarded to a member named after the SYMBOL, which the defining class does not have (CS0117):\n%s", mainCs)
	}

	// The method-shaped symbol forwards to its definition in the defining PACKAGE.
	if !strings.Contains(mainCs, ".legacyTAbs(") {
		t.Errorf("tAbs did not forward to its definition legacyTAbs:\n%s", mainCs)
	}

	if strings.Contains(mainCs, ".abs(") || strings.Contains(mainCs, "T_package") {
		t.Errorf("tAbs was routed by splitting the method-shaped symbol at its last dot:\n%s", mainCs)
	}

	if strings.Contains(mainCs, "static partial") {
		t.Errorf("a pull is still a bodyless partial stub (PartialStubGenerator throws on the first call):\n%s", mainCs)
	}

	// The control row: no definition row, so the forwarder spells the symbol's own name.
	if !strings.Contains(mainCs, ".plainTarget()") {
		t.Errorf("a forward row with no definition row stopped forwarding to its own symbol:\n%s", mainCs)
	}

	// Each definition is public: the forwarder reaches it across an assembly boundary.
	if !strings.Contains(defCs, "public static uint64 legacyTAbs(") {
		t.Errorf("definition legacyTAbs not publicized for the cross-assembly forwarder:\n%s", defCs)
	}

	if !strings.Contains(defCs, "public static (nint hour, nint min) legacyClock(") {
		t.Errorf("definition legacyClock not publicized for the cross-assembly forwarder:\n%s", defCs)
	}

	// The widening is bounded by the rows: a func no row names keeps its Go accessibility.
	if !strings.Contains(defCs, "internal static nint untouched(") {
		t.Errorf("an unnamed func in the defining package lost its Go accessibility: the definition access rule widens more than its rows:\n%s", defCs)
	}
}
