// recurseDefaultTags_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"io"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// The -recurse default tag set, end to end (C43). A third-party library commonly ships an assembly path
// behind `!noasm` (or `!purego`) and a pure-Go fallback behind the opposite constraint. A managed runtime
// cannot run the assembly, so the asm path converts to bodyless declarations that compile and then throw.
// klauspost/cpuid/v2 v2.2.10 is the measured case: detect_x86.go (`!noasm`) declares asmCpuid with its
// body in cpuid_amd64.s, and its package init calls it, so every converted program importing cpuid
// failed before main. The -recurse default therefore selects the fallbacks: safe,purego,noasm.
//
// These tests run the real ModuleConverter over a synthetic app + lib pair whose lib carries exactly that
// shape, and read which of the two files was converted. An explicit -tags still replaces the default
// entirely, so `-tags safe` must select the asm path again.

// recurseTagFixture writes the app + lib modules and returns the app's directory. lib's impl_asm.go is the
// asm path: a bodyless declaration, as a library with a .s file has. impl_pure.go is the fallback.
func recurseTagFixture(t *testing.T, root string) string {
	t.Helper()

	appDir := filepath.Join(root, "app")
	libDir := filepath.Join(root, "lib")

	writeModuleFile(t, filepath.Join(libDir, "go.mod"), "module example.com/lib\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(libDir, "impl_asm.go"),
		"//go:build !noasm\n\npackage lib\n\n//go:noescape\nfunc implAsm() int\n\nfunc Impl() string {\n\t_ = implAsm()\n\n\treturn \"asm\"\n}\n")
	writeModuleFile(t, filepath.Join(libDir, "impl_pure.go"),
		"//go:build noasm\n\npackage lib\n\nfunc Impl() string {\n\treturn \"pure\"\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"),
		"module example.com/app\n\ngo 1.23\n\nrequire example.com/lib v0.0.0\n\nreplace example.com/lib => ../lib\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\n\t\"example.com/lib\"\n)\n\nfunc main() {\n\tfmt.Println(lib.Impl())\n}\n")

	return appDir
}

// convertRecurseTagFixture converts the fixture with the given resolved tags and returns the lib's output
// directory and everything the conversion printed to stdout.
func convertRecurseTagFixture(t *testing.T, buildTags []string, tagsExplicit bool) (libOut, stdout string) {
	t.Helper()

	root := t.TempDir()
	appDir := recurseTagFixture(t, root)

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		go2csPathExplicit:   true,
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		buildTags:           buildTags,
		tagsExplicit:        tagsExplicit,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	var convertErr error

	stdout = captureRecurseTagStdout(t, func() {
		convertErr = NewModuleConverter(options).ConvertModule(appDir)
	})

	if convertErr != nil {
		t.Fatalf("ConvertModule: %v\n%s", convertErr, stdout)
	}

	return filepath.Join(options.recurseOutputRoot, "pkg", "example.com", "lib"), stdout
}

// captureRecurseTagStdout runs fn with os.Stdout redirected to a pipe that is drained while fn runs, so a
// conversion's output can never fill the pipe and block it.
func captureRecurseTagStdout(t *testing.T, fn func()) string {
	t.Helper()

	reader, writer, err := os.Pipe()

	if err != nil {
		t.Fatalf("os.Pipe: %v", err)
	}

	drained := make(chan string)

	go func() {
		data, _ := io.ReadAll(reader)
		drained <- string(data)
	}()

	saved := os.Stdout
	os.Stdout = writer

	func() {
		defer func() { os.Stdout = saved }()
		fn()
	}()

	writer.Close()
	captured := <-drained
	reader.Close()

	return captured
}

// TestRecurseDefaultTagsSelectThePureFallback: with no -tags, a -recurse conversion converts the noasm
// fallback and not the asm path, and says at run start which tags it applied and that they are the default.
func TestRecurseDefaultTagsSelectThePureFallback(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	libOut, stdout := convertRecurseTagFixture(t, resolveBuildTags(false, false, true, true, false, nil), false)

	if !fileExists(filepath.Join(libOut, "impl_pure.cs")) {
		t.Errorf("the default -recurse tags did not convert lib's noasm fallback impl_pure.go (no impl_pure.cs in %s)", libOut)
	}

	if fileExists(filepath.Join(libOut, "impl_asm.cs")) {
		t.Errorf("the default -recurse tags converted lib's asm path impl_asm.go (impl_asm.cs in %s): its bodyless implAsm throws at run time", libOut)
	}

	if want := "Applying build tags: safe,purego,noasm (default; pass -tags to override)\n"; !strings.Contains(stdout, want) {
		t.Errorf("a -recurse run did not announce its default tags %q at run start; it printed:\n%s", strings.TrimSpace(want), stdout)
	}
}

// TestRecurseExplicitTagsReplaceTheDefault: an explicit -tags safe replaces the whole default, so the asm
// path is converted again and the fallback is not, and the announcement names exactly what was passed.
func TestRecurseExplicitTagsReplaceTheDefault(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	libOut, stdout := convertRecurseTagFixture(t, resolveBuildTags(false, false, true, true, true, []string{"safe"}), true)

	if !fileExists(filepath.Join(libOut, "impl_asm.cs")) {
		t.Errorf("an explicit -tags safe did not convert lib's asm path impl_asm.go (no impl_asm.cs in %s)", libOut)
	}

	if fileExists(filepath.Join(libOut, "impl_pure.cs")) {
		t.Errorf("an explicit -tags safe still converted lib's noasm fallback impl_pure.go (impl_pure.cs in %s)", libOut)
	}

	if want := "Applying build tags: safe\n"; !strings.Contains(stdout, want) {
		t.Errorf("a -recurse run with -tags safe did not announce %q at run start; it printed:\n%s", strings.TrimSpace(want), stdout)
	}
}
