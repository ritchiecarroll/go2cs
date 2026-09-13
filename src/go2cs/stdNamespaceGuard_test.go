// stdNamespaceGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// The `namespace go.std` divergence, and the three guards that make it loud.
//
// SYMPTOM. A conversion emits every standard-library package into `namespace go.std.<pkg>` instead of
// `namespace go.<pkg>`, and names its projects `std.<pkg>.csproj` instead of `<pkg>.csproj`. The run
// reports success and exits 0. The damage surfaces later and elsewhere — in the CONSUMER packages, as
// `error CS0117: 'utf8_package' does not contain a definition for …` — which points away from the
// cause and reads exactly like a converter regression that dropped public members.
//
// MECHANISM. Both halves of the name derive from the package's IMPORT PATH, and for a standard-library
// package that path is recovered by subtracting GOROOT/src from the package's directory
// (getProjectName, importOperations.go). When the under-GOROOT test there says NO for a package that
// is in fact under GOROOT/src, the function falls through to its general module-resolution branch,
// walks up from the package directory looking for a go.mod — and finds $GOROOT/src/go.mod, which
// declares `module std`. Every standard-library package is then legitimately, consistently and
// wrongly named `std/<pkg>`.
//
// WHY IT IS INVOCATION-CONDITIONAL. Nothing in the converter changes between a healthy run and a
// poisoned one. What changes is the SPELLING of GOROOT that the invoking environment supplies. The
// same directory can arrive as `C:\Users\u\sdk\go1.23.12`, as `C:/Users/u/sdk/go1.23.12`, with a
// trailing separator, case-folded, or — from an MSYS/Cygwin shell — as `/c/Users/u/sdk/go1.23.12`,
// which is not a path Windows resolves at all. A byte-prefix test treats each as a different
// directory, so the conversion is decided by which shell launched it.
//
// THE GUARDS, in the order the poison would meet them:
//
//	TestGoRootSpellingRejectsUnresolvablePath      — a GOROOT this host cannot resolve is refused at
//	                                                 startup rather than half-recognized.
//	TestGoRootSpellingVariantsAgreeOnProjectName   — every resolvable spelling of one GOROOT yields
//	                                                 the same import path, so the emission cannot
//	                                                 depend on which shell launched the run.
//	TestStdLibLoaderReturnsBareImportPaths         — the standing environment canary: the loader the
//	                                                 driver itself uses must return bare import paths.
//
// The third is the one that does not depend on the converter being right. Normalization can only fold
// the spellings it knows how to fold; the canary asks the loader what it actually returned, so a
// poisoned environment is loud BEFORE a regen trusts its emission, whatever produced the poison.

// testGoRoot resolves the GOROOT these guards measure against — the toolchain running the test, which
// is by construction the one a conversion launched from this environment would load through.
func testGoRoot(t *testing.T) string {
	t.Helper()

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("no GOROOT is available to this test environment")
	}

	return filepath.Clean(goRoot)
}

// TestStdLibLoaderReturnsBareImportPaths is the STANDING CANARY: it asks the standard-library driver's
// own loader configuration for one small package and fails if what comes back is not that package's
// bare import path.
//
// It loads through stdLibLoadConfig — the function scanStdLib and buildDependencyGraph load through —
// rather than a transcription of it, so the canary cannot drift away from the thing it guards. One
// package, no network, no writes: cheap enough to stand in the plain `go test ./...` run, which is the
// point. A `std/` prefix here means the go command resolved the standard library as an ordinary module
// named `std` instead of as the standard library, and every name a conversion would emit from this
// environment is wrong before the converter sees a single line of Go.
func TestStdLibLoaderReturnsBareImportPaths(t *testing.T) {
	goRoot := testGoRoot(t)
	srcPath := filepath.Join(goRoot, "src")

	if info, err := os.Stat(srcPath); err != nil || !info.IsDir() {
		t.Fatalf("GOROOT %q holds no src directory, so no standard library can be loaded from it", goRoot)
	}

	// unicode/utf8 is the probe because it is small, has no build-tag variance worth reasoning about,
	// and is a package the corpus has validated since the first Phase-4 milestone. Any std package
	// would do — the property under test belongs to the environment, not to the package.
	const want = "unicode/utf8"

	options := Options{
		goRoot:         goRoot,
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	}

	// TWO patterns, because the two shapes fail differently and only one of them was reproducible.
	//
	//   - The IMPORT-PATH pattern is what scanStdLib ("std") and buildDependencyGraph (each pkgPath)
	//     actually pass. Measured: it stays bare even when GOROOT names a different toolchain than the
	//     sources being loaded, because the go command resolves an import path against GOROOT's own
	//     standard library.
	//   - The DIRECTORY pattern is the shape that goes wrong. Measured on Windows with GOROOT naming a
	//     different valid toolchain than the tree at Dir: module mode returns `std/unicode/utf8` and
	//     GOPATH mode returns the synthetic `_/C_/Users/.../src/unicode/utf8`. Both are cmd/go's
	//     modload.makeMainModules refusing the standard library its empty path-prefix, which it grants
	//     only when search.InDir(moduleRoot, cfg.GOROOTsrc) succeeds — i.e. only when the GOROOT
	//     SPELLING resolves to the directory the sources actually live in.
	//
	// Probing both costs one extra `go list` on one small package and makes the canary sensitive to the
	// condition rather than to one pattern's way of expressing it.
	for _, probe := range []string{want, "./" + want} {
		pkgs, err := packages.Load(stdLibLoadConfig(options, packages.NeedName, srcPath), probe)

		if err != nil {
			t.Fatalf("the standard-library loader configuration could not load %q from %q: %v\n"+
				"this is the environment the converter loads through, so no conversion launched from it can be trusted",
				probe, srcPath, err)
		}

		if len(pkgs) == 0 {
			t.Fatalf("the standard-library loader configuration matched no package for %q under %q", probe, srcPath)
		}

		for _, pkg := range pkgs {
			if pkg.PkgPath == want {
				continue
			}

			if strings.HasPrefix(pkg.PkgPath, "std/") {
				t.Fatalf("POISONED ENVIRONMENT: the loader returned PkgPath %q for pattern %q (want %q).\n"+
					"The go command resolved $GOROOT/src as an ordinary module named \"std\" (its go.mod says `module std`)\n"+
					"rather than as the standard library, so EVERY standard-library package is named std/<pkg>.\n"+
					"A conversion run from this environment emits `namespace go.std.*` and `std.<pkg>.csproj`, reports\n"+
					"success, and exits 0 — the damage appears later as CS0117 in the consumer packages.\n"+
					"Usual cause: GOROOT spelled in a form that does not resolve to the directory the sources are in\n"+
					"(an MSYS/Cygwin \"/c/...\" path on Windows, a stale root, a different toolchain, a symlink spelling).\n"+
					"GOROOT seen by this test: %q; `go env GOROOT` is the spelling to use.",
					pkg.PkgPath, probe, want, goRoot)
			}

			t.Fatalf("the loader returned PkgPath %q for pattern %q, want %q.\n"+
				"The import path a conversion would emit from is not the package's own import path, so every name\n"+
				"derived from it (namespace, project, assembly) is wrong. A synthetic `_/`-prefixed path means the go\n"+
				"command did not recognize these sources as the standard library at all — same cause, GOPATH-mode shape.\n"+
				"GOROOT seen by this test: %q; `go env GOROOT` is the spelling to use.",
				pkg.PkgPath, probe, want, goRoot)
		}
	}
}

// TestGoRootSpellingVariantsAgreeOnProjectName pins the property that makes the emission independent of
// the shell that launched it: the SAME directory, spelled every way this host can still resolve, must
// yield the same import path.
//
// Each variant below is a spelling a real invocation has produced — a forward-slash GOROOT on Windows
// (the form recorded in CLAUDE.md's -tests trap), a forward-slash input directory, a trailing
// separator, an interior dot segment, and on Windows a case difference, which is not a difference at
// all to the filesystem. Under the byte-prefix test this replaced, every one of them except the control
// resolved to `std.unicode.utf8` / `go.std.unicode`.
func TestGoRootSpellingVariantsAgreeOnProjectName(t *testing.T) {
	goRoot := testGoRoot(t)
	pkgDir := filepath.Join(goRoot, "src", "unicode", "utf8")

	if info, err := os.Stat(pkgDir); err != nil || !info.IsDir() {
		t.Skipf("GOROOT %q does not hold src/unicode/utf8; no standard-library sources to measure against", goRoot)
	}

	const wantProject = "unicode.utf8"
	const wantNamespace = RootNamespace + ".unicode"

	variants := []struct {
		name   string
		goRoot string
		pkgDir string
	}{
		{"native spelling (the control)", goRoot, pkgDir},
		{"forward-slash GOROOT", filepath.ToSlash(goRoot), pkgDir},
		{"forward-slash package directory", goRoot, filepath.ToSlash(pkgDir)},
		{"forward slashes on both sides", filepath.ToSlash(goRoot), filepath.ToSlash(pkgDir)},
		{"trailing separator on GOROOT", goRoot + string(os.PathSeparator), pkgDir},
		{"interior dot segment in GOROOT", goRoot + string(os.PathSeparator) + ".", pkgDir},
	}

	if runtime.GOOS == "windows" {
		// One directory, two spellings the filesystem does not distinguish — so neither may the
		// converter. This variant is Windows-only because elsewhere the two ARE different directories.
		variants = append(variants, struct {
			name   string
			goRoot string
			pkgDir string
		}{"upper-cased GOROOT (Windows)", strings.ToUpper(goRoot), pkgDir})
	}

	for _, variant := range variants {
		t.Run(variant.name, func(t *testing.T) {
			projectName, namespace := getProjectName(variant.pkgDir, Options{goRoot: variant.goRoot})

			if projectName != wantProject || namespace != wantNamespace {
				t.Fatalf("GOROOT %q with package directory %q produced project %q / namespace %q, want %q / %q.\n"+
					"Two spellings of one directory resolved to two different import paths: the under-GOROOT test in\n"+
					"getProjectName said no, so the package fell through to the module walk-up, which finds\n"+
					"$GOROOT/src/go.mod (`module std`) and names every standard-library package std/<pkg>.\n"+
					"A full conversion in this state emits `namespace go.std.*` and exits 0.",
					variant.goRoot, variant.pkgDir, projectName, namespace, wantProject, wantNamespace)
			}
		})
	}
}

// TestGoRootSpellingRejectsUnresolvablePath covers the half that normalization cannot reach.
// filepath.Clean folds the spellings of a path this host can resolve; it cannot turn an MSYS/Cygwin
// `/c/Users/u/sdk/go1.23.12` into `C:\Users\u\sdk\go1.23.12` — on Windows that Cleans to
// `\c\Users\u\sdk\go1.23.12`, still nothing. checkGoRootSpelling refuses such a value at startup,
// enforcing the doctrine the forward-slash-GOROOT finding paid for: a path the converter
// half-recognizes is worse than one it rejects.
func TestGoRootSpellingRejectsUnresolvablePath(t *testing.T) {
	if err := checkGoRootSpelling(testGoRoot(t)); err != nil {
		t.Fatalf("the host's own GOROOT must pass the spelling check: %v", err)
	}

	// A directory that does not exist at all — the MSYS spelling's shape on Windows, and a stale or
	// mistyped root's shape everywhere.
	missing := filepath.Join(t.TempDir(), "no-such-toolchain")

	// A directory that DOES exist but is not a toolchain root. This is the discriminating case: an
	// existence test alone would accept it, and it is what a GOROOT pointed one level too high (or at
	// an unpacked archive's parent) looks like.
	notAToolchain := t.TempDir()

	for _, poisoned := range []string{missing, notAToolchain} {
		err := checkGoRootSpelling(poisoned)

		if err == nil {
			t.Fatalf("checkGoRootSpelling accepted %q, which holds no src directory — a conversion would proceed\n"+
				"to emit the whole standard library into namespace go.std.* and exit 0", poisoned)
		}

		// The message has to carry the rejected spelling and the consequence, because that pairing is
		// the entire diagnosis for whoever meets it.
		if !strings.Contains(err.Error(), poisoned) {
			t.Errorf("the rejection for %q does not name the path it rejected: %v", poisoned, err)
		}

		if !strings.Contains(err.Error(), "go.std") {
			t.Errorf("the rejection for %q does not name the consequence (namespace go.std.*): %v", poisoned, err)
		}
	}
}
