// blockEventFrames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the SKIP-COUNTED-WALKER seed of computeNoInliningClosure (callerInliningAnalysis.go): a function
// that calls one of the runtime's skip-counting stack walkers directly is emitted
// [MethodImpl(NoInlining)], because its own frame is one of the Go frames the skip it passes counts --
// inlined, every stack the walk records starts one real frame too high. runtime.blockevent over
// saveblockevent is the measured case (runtime/pprof's TestBlockProfileBias lost its blockFrequentShort
// frame; marking blockevent alone restored it). A thin forwarder to such a hop is marked by the existing
// fixed point, which is what covers a mutexevent-style one-line entry.
//
// The walker set is keyed by package path, so a fixture package stands in for runtime through a
// COPY-ON-WRITE extension restored at cleanup (a restored reference to a mutated map restores nothing).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"testing"
)

func convertBlockEventFixture(t *testing.T) string {
	t.Helper()

	previous := skipCountedWalkers
	extended := make(map[string]bool, len(previous)+2)
	for key, value := range previous {
		extended[key] = value
	}
	extended["example.com/fakert.callers"] = true
	extended["example.com/fakert.save"] = true
	skipCountedWalkers = extended
	t.Cleanup(func() { skipCountedWalkers = previous })

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fakert\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "runtime/debug"

var buf [8]uintptr

// POSITIVE 4 -- a thin forwarder into a frame-LISTING walker (runtime/debug TestStack's ptrmethod shape):
// the printed traceback starts at this frame, so inlining it drops the frame from the stack text.
func stackOf() []byte { return debug.Stack() }

// The walker stand-in (runtime.callers).
func callers(skip int, pc []uintptr) int { return skip + len(pc) }

// POSITIVE 1 -- a walker that forwards its skip to callers (runtime.saveblockevent's shape).
func save(cycles int64, skip int) {
	if cycles > 0 {
		callers(skip, buf[:])
	}
}

// POSITIVE 2 -- the hop nearest a walker (runtime.blockevent's shape).
func blockevent(cycles int64, skip int) {
	if cycles > 0 {
		save(cycles, skip+1)
	}
}

// POSITIVE 3 -- a thin forwarder to the hop, reached by the fixed point (a one-line event entry).
func profEvent(cycles int64) { blockevent(cycles, 2) }

// CONTROL 1 -- calls the hop, but is neither thin nor a walker caller.
func twoStep(cycles int64) {
	c := cycles * 2
	blockevent(c, 2)
}

// CONTROL 2 -- walks nothing.
func other(skip int) int { return skip + 1 }

func main() {
	profEvent(1)
	twoStep(1)
	_ = other(1)
	_ = stackOf()
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

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "fakert", "main.cs"))
}

func TestSkipCountedWalkerHopsAreNotInlined(t *testing.T) {
	mainCs := convertBlockEventFixture(t)

	for _, name := range []string{"save", "blockevent", "profEvent", "stackOf"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("%s is a hop the skip counts, but it is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"twoStep", "other"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but no walk counts its frame: %s", name, line)
		}
	}
}
