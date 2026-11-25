// loaderEnvironment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// unsetCgoEnabled leaves CGO_ENABLED unset for the test, as a host that never mentions it does, and restores it after.
func unsetCgoEnabled(t *testing.T) {
	t.Helper()
	t.Setenv("CGO_ENABLED", "")

	if err := os.Unsetenv("CGO_ENABLED"); err != nil {
		t.Fatalf("unset CGO_ENABLED: %v", err)
	}
}

// os/user's closure as the converter's loader sees it, compiled (as LoadAllSyntax loads: only then does a cgo
// selection add runtime/cgo).
func osUserClosure(t *testing.T, env []string) map[string]bool {
	t.Helper()

	pkgs, err := packages.Load(&packages.Config{
		Mode: packages.NeedName | packages.NeedImports | packages.NeedDeps | packages.NeedCompiledGoFiles,
		Env:  env,
	}, "os/user")

	if err != nil || len(pkgs) != 1 {
		t.Fatalf("load os/user: %d packages, %v", len(pkgs), err)
	}

	seen := map[string]bool{}
	var walk func(p *packages.Package)

	walk = func(p *packages.Package) {
		for path, imp := range p.Imports {
			if !seen[path] {
				seen[path] = true
				walk(imp)
			}
		}
	}

	walk(pkgs[0])

	if len(seen) == 0 {
		t.Fatal("os/user's closure is empty: the load did not resolve its imports")
	}

	return seen
}

// A conversion must not depend on the host's IMPLICIT cgo default. Go turns cgo ON for a native build on a host with a
// C compiler (a Linux CI runner) and OFF for a cross build or a host without one (the Windows i7), and the corpus is
// emitted at CGO_ENABLED=0 on every platform (.claude/rules/corpus.md). With cgo on, os/user selects its cgo files and
// imports runtime/cgo, whose namespace go.runtime.cgo then renames a program's `runtime` alias to `Δruntime`:
// SyscallKeystonePulls read `using Δruntime = runtime_package;` on the linux behavioral leg and `runtime` everywhere
// else, a [Target] red no Windows host could reproduce. Measured only where the default IS on: a native load on a host
// with a C compiler. Elsewhere the default is already off and the arm proves nothing, so it skips by name.
func TestLoaderEnvPinsCgoOffWhenTheHostLeavesItUnset(t *testing.T) {
	_, gcc := exec.LookPath("gcc")
	_, cc := exec.LookPath("cc")

	if gcc != nil && cc != nil {
		t.Skip("no C compiler: Go's own cgo default is already off here, so there is no host default to override")
	}

	unsetCgoEnabled(t)

	goos, goarch := hostTarget(t)

	if seen := osUserClosure(t, converterLoaderEnv("GOOS="+goos, "GOARCH="+goarch, "GOTOOLCHAIN=local")); seen["runtime/cgo"] {
		t.Fatal("os/user's closure holds runtime/cgo with CGO_ENABLED unset on a host with a C compiler: the loader took " +
			"Go's implicit cgo default, so the emission depends on the host (a `Δruntime` alias on a native Linux host)")
	}
}

// With CGO_ENABLED unset, the loader environment ends with the pin, so it wins over any setting before it.
func TestLoaderEnvEndsWithTheCgoPinWhenUnset(t *testing.T) {
	unsetCgoEnabled(t)

	env := converterLoaderEnv("GOOS=darwin", "GOARCH=arm64")

	if got := env[len(env)-1]; got != "CGO_ENABLED=0" {
		t.Fatalf("the loader environment ends with %q, want CGO_ENABLED=0", got)
	}
}

// An EXPLICIT CGO_ENABLED is the caller's decision: CGO_ENABLED=1 on a package that selects a cgo source must reach
// the refusal (TestSelectedCgoRefusalPrintsCanonicalDiagnostic), never be converted as its cgo-off selection.
func TestLoaderEnvKeepsAnExplicitCgoSetting(t *testing.T) {
	t.Setenv("CGO_ENABLED", "1")

	env := converterLoaderEnv("GOOS=linux", "GOARCH=amd64")

	for _, entry := range env {
		if strings.HasPrefix(entry, "CGO_ENABLED=") && entry != "CGO_ENABLED=1" {
			t.Fatalf("an explicit CGO_ENABLED=1 was overridden by %q", entry)
		}
	}
}

// hostTarget is the host's own GOOS/GOARCH, so the load above is native: a cross load defaults cgo off by itself.
func hostTarget(t *testing.T) (string, string) {
	t.Helper()

	out, err := exec.Command("go", "env", "GOHOSTOS", "GOHOSTARCH").Output()

	if err != nil {
		t.Fatalf("go env GOHOSTOS GOHOSTARCH: %v", err)
	}

	fields := strings.Fields(string(out))

	if len(fields) != 2 {
		t.Fatalf("go env GOHOSTOS GOHOSTARCH: %q", out)
	}

	return fields[0], fields[1]
}

// -cgo makes the conversion's loads cgo-on, so it is refused wherever it would be silently ignored (a driver whose loads
// it does not reach) and wherever the environment asks for the opposite load.
//
// RE-CITED for the cgo-against-go.* item (INC 1, C2 ca1fd3a4e): -cgo beside -recurse was refused here ("the module
// driver's loads stay at CGO_ENABLED=0"). The module driver now loads cgo-on under -cgo (moduleConverter.go), so the row
// reads accepted; TestARecurseNugetConversionTakesACgoPackageThroughTheCgoRoute holds what it then converts.
//
// RE-CITED for cgo Phase 3 increment 3.4 seat 4a (DESIGN-cgo-phase3-marshaling section 19): -cgo beside -tests was
// refused here ("the test drivers' loads stay at CGO_ENABLED=0"). The package's own test loads and the oracle now run
// cgo-on under -cgo (packageTestLoaderEnv, cgoOracleEnv), so the row reads accepted; the corpus is still protected, by
// checkCgoTestsOutput (TestACgoTestsConversionNeverWritesTheRepositorysCorpus).
func TestCheckCgoFlagRefusesWhatItCannotHonour(t *testing.T) {
	cases := []struct {
		name               string
		cgo, stdlib, tests bool
		env                string // "-" leaves CGO_ENABLED unset
		refused            string
	}{
		{name: "off", env: "0"},
		{name: "on, unset", cgo: true, env: "-"},
		{name: "on, empty", cgo: true, env: ""},
		{name: "on, 1", cgo: true, env: "1"},
		{name: "on, 0", cgo: true, env: "0", refused: "CGO_ENABLED=0"},
		{name: "stdlib", cgo: true, stdlib: true, env: "-", refused: "-stdlib"},
		{name: "tests is accepted", cgo: true, tests: true, env: "-"},
		{name: "recurse is accepted", cgo: true, env: "-"},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			if c.env == "-" {
				unsetCgoEnabled(t)
			} else {
				t.Setenv("CGO_ENABLED", c.env)
			}

			err := checkCgoFlag(c.cgo, c.stdlib, c.tests)

			switch {
			case c.refused == "" && err != nil:
				t.Errorf("refused: %v", err)
			case c.refused != "" && (err == nil || !strings.Contains(err.Error(), c.refused)):
				t.Errorf("want a refusal naming %q, got %v", c.refused, err)
			}
		})
	}
}

// -cgo -tests loads the CONVERTED package's own test variants cgo-on and runs the oracle cgo-on, as its production load
// is; every other load keeps the loader pin (stop line S-G). Without -cgo nothing changes (seat 4a, section 19).
func TestOnlyTheConvertedPackagesTestLoadsAndTheOracleTurnCgoOn(t *testing.T) {
	unsetCgoEnabled(t)

	last := func(env []string) string {
		value := ""

		for _, entry := range env {
			if v, found := strings.CutPrefix(entry, "CGO_ENABLED="); found {
				value = v
			}
		}

		return value
	}

	if got := last(packageTestLoaderEnv(Options{parseCgoTargets: true}, "GOOS=linux")); got != "1" {
		t.Errorf("-cgo: the package's test load runs at CGO_ENABLED=%q, want 1", got)
	}

	if got := last(packageTestLoaderEnv(Options{}, "GOOS=linux")); got != "0" {
		t.Errorf("no -cgo: the package's test load runs at CGO_ENABLED=%q, want the loader pin 0", got)
	}

	if got := last(converterLoaderEnv("GOOS=linux")); got != "0" {
		t.Errorf("another package's load runs at CGO_ENABLED=%q, want the loader pin 0 (S-G)", got)
	}

	if got := cgoOracleEnv(Options{parseCgoTargets: true}); len(got) != 1 || got[0] != "CGO_ENABLED=1" {
		t.Errorf("-cgo: the oracle's extra environment is %q, want [CGO_ENABLED=1]", got)
	}

	if got := cgoOracleEnv(Options{}); got != nil {
		t.Errorf("no -cgo: the oracle's extra environment is %q, want none", got)
	}
}

// -cgo -tests never writes into a repository's corpus (stop line S-D), judged at the OUTPUT with symlinks resolved, and
// only into a root its seed step marked (P3's verdict on 4a, inbox H 20261124T232124Z):
//   - a root with no marker is refused (the repository's src, a deployed root);
//   - route (a): an explicit -go2cspath elsewhere, the output inside a repository's src/core, is refused;
//   - route (b): a scratch src that is a symlink to a repository's src is refused;
//   - a marked scratch root, the output in its own core, is accepted;
//   - without -cgo, or without -tests, nothing is checked.
func TestACgoTestsConversionNeverWritesTheRepositorysCorpus(t *testing.T) {
	repository := t.TempDir()
	repoSrc := filepath.Join(repository, "src")
	scratch := filepath.Join(t.TempDir(), "src")

	for _, dir := range []string{filepath.Join(repoSrc, "core", "net"), filepath.Join(scratch, "core", "net")} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	// A worktree's .git is a FILE; the check reads either.
	if err := os.WriteFile(filepath.Join(repository, ".git"), []byte("gitdir: elsewhere"), 0o644); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(scratch, cgoScratchRootMarker), nil, 0o644); err != nil {
		t.Fatal(err)
	}

	cgoTests := Options{parseCgoTargets: true, convertTests: true}

	check := func(t *testing.T, options Options, root, output, refusal string) {
		t.Helper()
		options.go2csPath = root
		err := checkCgoTestsOutput(options, output)

		switch {
		case refusal != "" && (err == nil || !strings.Contains(err.Error(), refusal)):
			t.Errorf("want a refusal naming %q, got %v", refusal, err)
		case refusal == "" && err != nil:
			t.Errorf("refused: %v", err)
		}
	}

	t.Run("a root with no marker", func(t *testing.T) {
		check(t, cgoTests, repoSrc, filepath.Join(t.TempDir(), "net"), cgoScratchRootMarker)
	})

	t.Run("route (a): an explicit root elsewhere, the output in a repository's core", func(t *testing.T) {
		check(t, cgoTests, scratch, filepath.Join(repoSrc, "core", "net"), "CGO_ENABLED=0 by ruling")
	})

	t.Run("route (b): a scratch src that is a symlink to a repository's src", func(t *testing.T) {
		link := filepath.Join(t.TempDir(), "src")

		if err := os.Symlink(repoSrc, link); err != nil {
			t.Skipf("NOT RUN: this host cannot create a symlink (%v); the hosted linux leg runs this arm", err)
		}

		if err := os.WriteFile(filepath.Join(repoSrc, cgoScratchRootMarker), nil, 0o644); err != nil {
			t.Fatal(err)
		}

		defer os.Remove(filepath.Join(repoSrc, cgoScratchRootMarker))

		check(t, cgoTests, link, filepath.Join(link, "core", "net"), "CGO_ENABLED=0 by ruling")
	})

	t.Run("A1: a marked root, the output in a deployed corpus outside its core", func(t *testing.T) {
		deployed := filepath.Join(t.TempDir(), "go2cs", "core", "net")

		if err := os.MkdirAll(deployed, 0o755); err != nil {
			t.Fatal(err)
		}

		check(t, cgoTests, scratch, deployed, "not under it")
	})

	t.Run("A2: a repository's core typed in another case", func(t *testing.T) {
		upper := t.TempDir()

		if err := os.MkdirAll(filepath.Join(upper, "src", "Core", "net"), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(filepath.Join(upper, ".git"), []byte("gitdir: elsewhere"), 0o644); err != nil {
			t.Fatal(err)
		}

		check(t, cgoTests, scratch, filepath.Join(upper, "src", "Core", "net"), "CGO_ENABLED=0 by ruling")
	})

	t.Run("a marked scratch root", func(t *testing.T) {
		check(t, cgoTests, scratch, filepath.Join(scratch, "core", "net"), "")
	})

	t.Run("no -cgo", func(t *testing.T) {
		check(t, Options{convertTests: true}, repoSrc, filepath.Join(repoSrc, "core", "net"), "")
	})

	t.Run("no -tests", func(t *testing.T) {
		check(t, Options{parseCgoTargets: true}, repoSrc, filepath.Join(repoSrc, "core", "net"), "")
	})
}
