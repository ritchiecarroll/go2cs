// toolchainGoRootFix_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// Tests for the 2026-09-13 `-goroot` fix: the flag never reached the LOADER in any mode, and a
// -go2cspath without version.props switched the corpus toolchain pin off silently.
//
// Every test here is paired with the pre-fix behaviour where the pre-fix behaviour still exists in the
// tree, because "this asserts the new thing" and "this would have caught the old thing" are different
// claims and only the second one is a control.

import (
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// --- the loader-GOROOT decision ------------------------------------------------------------------

func TestLoaderGoRootDecisionExportsWhenEnvironmentUnset(t *testing.T) {
	export, err := loaderGoRootDecision("", filepath.Join("some", "root"))

	if err != nil {
		t.Fatalf("an unset environment is not a disagreement, got error: %v", err)
	}

	if !export {
		t.Fatal("an unset environment must EXPORT the flag -- without the export the loader reads the " +
			"ambient root and -goroot is inert for source selection, which is the defect this fixes")
	}
}

func TestLoaderGoRootDecisionProceedsWhenEqual(t *testing.T) {
	root := filepath.Join("go", "toolchain", "go1.23.12")

	export, err := loaderGoRootDecision(root, root)

	if err != nil {
		t.Fatalf("equal values are not a disagreement, got error: %v", err)
	}

	if export {
		t.Fatal("an environment already equal to the flag needs no export")
	}
}

// The arm that matters: a disagreement REFUSES, and the message carries both values so the reader can
// fix it without reproducing the run.
func TestLoaderGoRootDecisionRefusesDisagreementByName(t *testing.T) {
	envRoot := filepath.Join("usr", "local", "go1.24.7")
	flagRoot := filepath.Join("toolchain", "go1.23.12")

	export, err := loaderGoRootDecision(envRoot, flagRoot)

	if err == nil {
		t.Fatal("a flag disagreeing with the environment must REFUSE: the loader follows the environment, " +
			"so the run would convert one tree under the other's label")
	}

	if export {
		t.Fatal("a refusal must not also ask the caller to export")
	}

	for _, needle := range []string{envRoot, flagRoot, "GOROOT disagreement", "LOADER follows the ENVIRONMENT"} {
		if !strings.Contains(err.Error(), needle) {
			t.Errorf("the refusal must name %q; message was:\n%s", needle, err.Error())
		}
	}
}

// --- sameGoRoot, whose job is to keep the refusal off spellings ------------------------------------

func TestSameGoRootIgnoresSpellingAndSymlinks(t *testing.T) {
	base := t.TempDir()
	real := filepath.Join(base, "realroot")

	if err := os.MkdirAll(filepath.Join(real, "src"), 0o755); err != nil {
		t.Fatalf("failed to build the fixture: %v", err)
	}

	if !sameGoRoot(real, real+string(filepath.Separator)) {
		t.Error("a trailing separator is a spelling, not a disagreement")
	}

	if !sameGoRoot(real, filepath.Join(real, "..", "realroot")) {
		t.Error("an unclean but equivalent path is a spelling, not a disagreement")
	}

	link := filepath.Join(base, "linkroot")

	if err := os.Symlink(real, link); err != nil {
		t.Skipf("symlinks unavailable on this host (%v); the other arms still ran", err)
	}

	if !sameGoRoot(link, real) {
		t.Error("a symlink-only difference must NOT refuse -- importOperations.go records that exact " +
			"shape as a working setup")
	}

	if sameGoRoot(real, filepath.Join(base, "otherroot")) {
		t.Error("two genuinely different roots must NOT compare equal, or the refusal can never fire")
	}
}

// --- the corpus pin, and the silent no-op it replaces ---------------------------------------------

func TestCorpusPinnedReleaseOrErrorReadsAPresentPin(t *testing.T) {
	root := writeVersionProps(t, "<Project><PropertyGroup><GoStdLibVersion>1.23.12</GoStdLibVersion></PropertyGroup></Project>")

	release, err := corpusPinnedReleaseOrError(root)

	if err != nil {
		t.Fatalf("a readable pin must not error: %v", err)
	}

	if release != "1.23.12" {
		t.Errorf("release = %q, want 1.23.12", release)
	}
}

func TestCorpusPinnedReleaseOrErrorAllowsAnAbsentRoot(t *testing.T) {
	// "No -go2cspath was supplied" is legitimately unpinned and is not this error's business.
	release, err := corpusPinnedReleaseOrError("")

	if err != nil || release != "" {
		t.Fatalf("an empty root must be (\"\", nil), got (%q, %v)", release, err)
	}
}

// THE CONTROL, and the reason this file exists: the same input, through the old function and the new
// one. corpusPinnedRelease is still in the tree for callers that may legitimately be unpinned, so the
// contrast is directly measurable rather than argued -- pre-fix, the corpus-defining call sites took the
// left-hand answer and checkCorpusToolchainPin returns nil on an empty pin, so the toolchain pin was OFF
// and said nothing.
func TestPinRefusalReplacesASilentNoOp(t *testing.T) {
	root := seedFakeGo2CSRoot(t) // a SEEDED tree with no version.props -- floor rule 2's shape, and C1's

	if silent := corpusPinnedRelease(root); silent != "" {
		t.Fatalf("fixture wrong: corpusPinnedRelease should read %q as empty, got %q", root, silent)
	}

	// The pre-fix path, reconstructed exactly: an empty pin reaching the pin check.
	if err := checkCorpusToolchainPin("-stdlib", "go1.24.13", corpusPinnedRelease(root)); err != nil {
		t.Fatalf("fixture wrong: the pre-fix path must PASS silently (that is the defect), got: %v", err)
	}

	// The fixed path on the same input.
	release, err := corpusPinnedReleaseOrError(root)

	if err == nil {
		t.Fatal("a -go2cspath carrying no version.props must REFUSE by name: silently returning an empty " +
			"pin switches the toolchain check off, which is how two census arms passed on the wrong release")
	}

	if release != "" {
		t.Errorf("a refusal must not also return a release, got %q", release)
	}

	for _, needle := range []string{versionPropsFileName, root, "corpus toolchain pin cannot be read"} {
		if !strings.Contains(err.Error(), needle) {
			t.Errorf("the refusal must name %q; message was:\n%s", needle, err.Error())
		}
	}
}

func TestCorpusPinnedReleaseOrErrorRefusesAMalformedPin(t *testing.T) {
	root := seedFakeGo2CSRoot(t)
	if err := os.WriteFile(filepath.Join(root, versionPropsFileName),
		[]byte("<Project><PropertyGroup><SomethingElse>1.23.12</Somethin"), 0o644); err != nil {
		t.Fatalf("failed to write a malformed %s: %v", versionPropsFileName, err)
	}

	_, err := corpusPinnedReleaseOrError(root)

	if err == nil {
		t.Fatal("a version.props with no <GoStdLibVersion> must refuse -- a partial write leaves exactly " +
			"this shape and it must not read as 'unpinned'")
	}

	if !strings.Contains(err.Error(), "GoStdLibVersion") {
		t.Errorf("the malformed-pin refusal must say which element is missing; message was:\n%s", err.Error())
	}
}

// --- the loader's environment carries GOTOOLCHAIN=local -------------------------------------------

// A converter of a PINNED root must not follow a toolchain switch: under the default `auto` the go
// command re-execs whichever toolchain a module asks for and REWRITES GOROOT in the re-exec'd process,
// so the emission can come from a release nobody chose. Asserted against the loader config's own
// construction rather than a conversion, so it runs in milliseconds and cannot be flaky.
func TestLoaderEnvironmentPinsTheToolchain(t *testing.T) {
	_, thisFile, _, ok := runtime.Caller(0)

	if !ok {
		t.Fatal("cannot locate this test's own directory")
	}

	source, err := os.ReadFile(filepath.Join(filepath.Dir(thisFile), "conversionDriver.go"))

	if err != nil {
		t.Fatalf("failed to read conversionDriver.go: %v", err)
	}

	if !strings.Contains(string(source), `"GOTOOLCHAIN=local"`) {
		t.Error("the loader's packages.Config env must carry GOTOOLCHAIN=local: without it, GOTOOLCHAIN=auto " +
			"re-execs a newer toolchain and rewrites GOROOT, and the conversion silently reads a release " +
			"nobody pinned")
	}

	// The env is built by converterLoaderEnv (loaderEnvironment.go), which extends os.Environ() and pins cgo off.
	if !strings.Contains(string(source), "cfg.Env = converterLoaderEnv(") {
		t.Error("the loader env is expected to be converterLoaderEnv(...); if that changed, this test's premise " +
			"no longer holds and the GOTOOLCHAIN assertion above needs re-siting")
	}

	helper, err := os.ReadFile(filepath.Join(filepath.Dir(thisFile), "loaderEnvironment.go"))

	if err != nil {
		t.Fatalf("failed to read loaderEnvironment.go: %v", err)
	}

	if !strings.Contains(string(helper), "env := append(os.Environ(), settings...)") {
		t.Error("converterLoaderEnv is expected to extend os.Environ() with the caller's settings; " +
			"if that changed, this test's premise no longer holds")
	}
}

// seedFakeGo2CSRoot stands up the minimum that makes isGo2CSRoot true, so a test can distinguish "a
// seeded corpus missing its pin" (refuse) from "a bare bootstrap target" (allow).
func seedFakeGo2CSRoot(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	golib := filepath.Join(root, "core", "golib")

	if err := os.MkdirAll(golib, 0o755); err != nil {
		t.Fatalf("failed to seed a fake go2cs root: %v", err)
	}

	if err := os.WriteFile(filepath.Join(golib, "golib.csproj"), []byte("<Project/>"), 0o644); err != nil {
		t.Fatalf("failed to seed golib.csproj: %v", err)
	}

	if !isGo2CSRoot(root) {
		t.Fatalf("fixture wrong: %q is not recognised as a go2cs root", root)
	}

	return root
}

// The other side of the narrowing, and the reason it exists: a genuinely bare target is how a FIRST
// -stdlib conversion starts, and toolchainResolution_test.go's own helper records that as normal rather
// than a fault. Refusing it would break bootstrapping to no purpose.
func TestCorpusPinnedReleaseOrErrorAllowsABareBootstrapRoot(t *testing.T) {
	bare := t.TempDir()

	if isGo2CSRoot(bare) {
		t.Fatalf("fixture wrong: %q should not look like a go2cs tree", bare)
	}

	release, err := corpusPinnedReleaseOrError(bare)

	if err != nil {
		t.Fatalf("a bare bootstrap target must be allowed through unpinned, got: %v", err)
	}

	if release != "" {
		t.Errorf("release = %q, want empty for an unpinned bare root", release)
	}
}
