// testingDecoratorFrames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the TESTING-DECORATOR seed of computeNoInliningClosure (callerInliningAnalysis.go): a SMALL function
// that calls testing's decorating API directly (Log, Error, Fatal, Skip and their f forms, Helper, Cleanup,
// Run on T, B, F, common or TB) keeps its frame, because the record's file:line, the helper skip, the cleanup's
// pc and a subtest's creator all name that frame. testing's TestTBHelper is the measured case: under Release
// TieredCompilation=0 its one-line notHelperCallingHelper was inlined and the record named the wrong line. A
// thin forwarder to such a helper joins through the fixed point. A large body, M.Run, and a non-decorating
// method are controls.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"testing"
)

func convertTestingDecoratorFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/decor\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "testing"

// POSITIVE 1 -- a small helper (testing's own helperfuncs_test.go helper).
func helper(t *testing.T, msg string) {
	t.Helper()
	t.Error(msg)
}

// POSITIVE 2 -- a thin forwarder to the helper, reached by the fixed point (notHelperCallingHelper).
func notHelperCallingHelper(t *testing.T, msg string) { helper(t, msg) }

// POSITIVE 3 -- a small helper through the TB interface.
func tbLog(tb testing.TB, msg string) { tb.Log(msg) }

// POSITIVE 4 -- a small cleanup registrar: the cleanup's records name this frame.
func addCleanup(t *testing.T, f func()) { t.Cleanup(f) }

// CONTROL 1 -- calls Errorf, but is not small: no JIT inlines it.
func bigCheck(t *testing.T, xs []int) {
	for i, x := range xs {
		if x < 0 {
			t.Errorf("xs[%d] = %d", i, x)
		}
	}
}

// CONTROL 2 -- M.Run records nothing about its caller.
func runMain(m *testing.M) int { return m.Run() }

// CONTROL 3 -- a testing method that does not decorate.
func nameOf(t *testing.T) string { return t.Name() }

func main() {}
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "decor", "main.cs"))
}

func TestTestingDecoratorCallersKeepTheirFrames(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs := convertTestingDecoratorFixture(t)

	for _, name := range []string{"helper", "notHelperCallingHelper", "tbLog", "addCleanup"} {
		if line := declarationLine(t, mainCs, name); !keepsOwnFrame(line) {
			t.Errorf("%s's frame is the one testing's walk names, but it is not marked: %s", name, line)
		}
	}

	for _, name := range []string{"bigCheck", "runMain", "nameOf"} {
		if line := declarationLine(t, mainCs, name); keepsOwnFrame(line) {
			t.Errorf("%s is marked, but no testing walk needs its frame kept: %s", name, line)
		}
	}
}
