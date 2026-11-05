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
