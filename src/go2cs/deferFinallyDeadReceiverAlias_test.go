// deferFinallyDeadReceiverAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the defer→finally lowering (deferFinallyLowering.go) when the receiver's entry deref alias
// is DEAD. A lowered call renders against the receiver's value alias (`p.release()` for a `ref T`
// method), and the finally re-roots it on the box because the alias is declared inside the try.
// But the alias is emitted only when the BODY reads the receiver as a value, and the lowered call
// is not part of the body text: when every body use goes through the box, the alias is skipped and
// the finally named a `p` declared nowhere (CS0103). Go 1.26's os/pidfd_linux.go is the shape:
//
//	handle, status := p.handleTransientAcquire() // a box (`this ж<Process>`) method
//	…
//	defer p.handleTransientRelease()             // a `this ref Process` method
//
// Control: a body that reads a receiver field keeps its alias, and its finally is re-rooted on the
// box exactly as before.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertDeadReceiverAliasFixture converts the one fixture the test reads and returns its emitted C#.
func convertDeadReceiverAliasFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/deadrecvalias\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "sync/atomic"

type res struct {
	state atomic.Uint32
	n     int
}

// The atomic field's method takes its address, so this receiver is the box.
func (r *res) acquire() bool { return r.state.Load() == 0 }

// A plain field write: this receiver is the value.
func (r *res) release() { r.n-- }

// SUBJECT - the pidfd shape: the body reaches r only through the box, so its alias is dead.
func (r *res) use() int {
	if !r.acquire() {
		return 0
	}
	defer r.release()

	return 1
}

// CONTROL - the body reads r.n as a value, so the alias is live and the finally is re-rooted.
func (r *res) useLive() int {
	if !r.acquire() {
		return 0
	}
	defer r.release()

	return r.n
}

func main() {
	var r res
	println(r.use(), r.useLive())
}
`)

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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "deadrecvalias", "main.cs"))
}

// TestLoweredDeferReRootsADeadReceiverAlias pins the re-root for a dead alias and its control.
func TestLoweredDeferReRootsADeadReceiverAlias(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertDeadReceiverAliasFixture(t)
	reRooted := "if (" + GoFrameVar + "d1) " + AddressPrefix + "r." + NilDeferringDerefAccessor + ".release();"

	// SUBJECT - no alias is declared, so the finally must go through the box.
	use := emittedFuncBody(t, mainCs, "use")

	if strings.Contains(use, "ref var r = ref") {
		t.Fatalf("use: the fixture no longer has a dead alias; it declares one:\n%s", use)
	}

	if !strings.Contains(use, "finally {") {
		t.Fatalf("use: the defer is no longer lowered into the finally:\n%s", use)
	}

	if !strings.Contains(use, reRooted) {
		t.Errorf("use: expected the finally to call %q (a bare `r.` names nothing, CS0103):\n%s", reRooted, use)
	}

	// CONTROL - the live alias is declared inside the try and the finally is re-rooted as before.
	live := emittedFuncBody(t, mainCs, "useLive")

	if !strings.Contains(live, "ref var r = ref "+AddressPrefix+"r."+NilDeferringDerefAccessor+";") || !strings.Contains(live, reRooted) {
		t.Errorf("useLive: expected the live alias and the re-rooted finally call %q:\n%s", reRooted, live)
	}
}
