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
}
