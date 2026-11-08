// handOwnSourcePins_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A hand-owned body that DISPLACES a generated one (manualConversionFuncs) was written against one
// upstream text. When a release hop changes that Go function, the converter keeps emitting the
// displacement placeholder and the hand-owned body keeps compiling, so nothing fails: the port silently
// carries the old algorithm. This guard pins the Go source text of each displaced function whose
// hand-owned body re-implements the ALGORITHM rather than a runtime seam, and fails on any change,
// naming the hand-owned file to re-derive.
//
// Scope: re-implementations only. A displacement whose body is a .NET seam (a syscall, a reinterpret,
// an init that cannot run under the CLR) is bound to the Go function's CONTRACT, not its text, and the
// destination guards (manualConversionDestination_test.go) already tie each registration to a body.
// The first entries are fiat's p521Mul and p521Square: fiat-crypto's formally verified output, replaced
// by hand-written arithmetic because the converted bodies declare 1,006 locals, past the JIT's 512-local
// inlining cut-off (docs/phase4/DATA-p521-table.md). Then p384Mul and p384Square, for the same reason at a
// smaller scale: the inliner stops after about 74 callees, leaving 590 calls per field multiply
// (docs/phase4/DATA-p384-field.md). Then p256Mul and p256Square, whose converted bodies inline fully under
// the JIT but keep 136 calls per field multiply under ReadyToRun, across the version bubble
// (docs/phase4/DATA-p256-field.md).
//
// The pin is the SHA-256 of the function's source text, from the `func` keyword to its closing brace,
// read from GOROOT at the version src/version.props pins. Doc comments are outside the span, so a
// comment-only change upstream does not fire. A hop that changes the body fails here until the hand-own
// is re-derived against the new text and its differential test re-run; only then is the pin updated.
//
// POSITIVE CONTROL: TestHandOwnSourcePinScannerFires drives the same scanner over a synthetic GOROOT
// holding one unchanged function, one changed body, one changed doc comment and one missing function.

package repoguard

import (
	"crypto/sha256"
	"encoding/hex"
	"fmt"
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"go2cs/internal/releasestamp"
)

// handOwnSourcePin is one displaced Go function whose hand-owned body re-implements its algorithm.
type handOwnSourcePin struct {
	goFile   string // GOROOT/src-relative, slash-separated
	funcName string
	sha256   string // of the source text from `func` to the closing brace
	handOwn  string // repository-relative hand-owned body
	reverify string // what to re-run after re-deriving
}

var handOwnSourcePins = []handOwnSourcePin{
	{
		goFile:   "crypto/internal/fips140/nistec/fiat/p521_fiat64.go",
		funcName: "p521Mul",
		sha256:   "542c7f425ee4b098933bf2403b1c6c692462b325c8ab60539626d64ff6fc29bf",
		handOwn:  "src/core/crypto/internal/fips140/nistec/fiat/p521_impl.cs",
		reverify: "GolibTests P521FieldDifferentialTests and the P521CurveBytes behavioral test",
	},
	{
		goFile:   "crypto/internal/fips140/nistec/fiat/p521_fiat64.go",
		funcName: "p521Square",
		sha256:   "c72e1a294a05a9d7520c7d6036b892927f9ff73ac3760a712417c25e792a8f7e",
		handOwn:  "src/core/crypto/internal/fips140/nistec/fiat/p521_impl.cs",
		reverify: "GolibTests P521FieldDifferentialTests and the P521CurveBytes behavioral test",
	},
	{
		goFile:   "crypto/internal/fips140/nistec/fiat/p384_fiat64.go",
		funcName: "p384Mul",
		sha256:   "2e8917365b874d68dabcc13e0b3916ec13665585b5240e49fd56b147e15e4010",
		handOwn:  "src/core/crypto/internal/fips140/nistec/fiat/p384_impl.cs",
		reverify: "GolibTests P384FieldDifferentialTests and the P384CurveBytes behavioral test",
	},
	{
		goFile:   "crypto/internal/fips140/nistec/fiat/p384_fiat64.go",
		funcName: "p384Square",
		sha256:   "471ea92f472c38c09d94f734a06a5cdedc8f353280dc3e06dd1ebda6e90ff9d4",
		handOwn:  "src/core/crypto/internal/fips140/nistec/fiat/p384_impl.cs",
		reverify: "GolibTests P384FieldDifferentialTests and the P384CurveBytes behavioral test",
	},
	{
		goFile:   "crypto/internal/fips140/nistec/fiat/p256_fiat64.go",
		funcName: "p256Mul",
		sha256:   "fd86335726e25794fc53796f31a768b723266cd2af8fade4bab27eb453834563",
		handOwn:  "src/core/crypto/internal/fips140/nistec/fiat/p256_field_impl.cs",
		reverify: "GolibTests P256FieldDifferentialTests and the P256CurveBytes behavioral test",
	},
	{
		goFile:   "crypto/internal/fips140/nistec/fiat/p256_fiat64.go",
		funcName: "p256Square",
		sha256:   "9ca197b4a656f8cd6ccc30629c2fe833a61ce1dd6365ed64dcb709051f6a4030",
		handOwn:  "src/core/crypto/internal/fips140/nistec/fiat/p256_field_impl.cs",
		reverify: "GolibTests P256FieldDifferentialTests and the P256CurveBytes behavioral test",
	},
}

// funcSourceHash is the SHA-256 of funcName's declaration text in file, from `func` to its closing
// brace, or "" when the file declares no such top-level function.
func funcSourceHash(file string, funcName string) (string, error) {
	src, err := os.ReadFile(file)

	if err != nil {
		return "", err
	}

	fset := token.NewFileSet()
	parsed, err := parser.ParseFile(fset, file, src, parser.SkipObjectResolution)

	if err != nil {
		return "", err
	}

	for _, decl := range parsed.Decls {
		fn, ok := decl.(*ast.FuncDecl)

		if !ok || fn.Recv != nil || fn.Name.Name != funcName {
			continue
		}

		start, end := fset.Position(fn.Pos()).Offset, fset.Position(fn.End()).Offset
		sum := sha256.Sum256(src[start:end])
		return hex.EncodeToString(sum[:]), nil
	}

	return "", nil
}

// scanHandOwnSourcePins reports every pin whose function is absent from goRoot or whose text changed.
func scanHandOwnSourcePins(pins []handOwnSourcePin, goRoot string, version string) []string {
	var findings []string

	for _, pin := range pins {
		file := filepath.Join(goRoot, "src", filepath.FromSlash(pin.goFile))
		got, err := funcSourceHash(file, pin.funcName)

		switch {
		case err != nil:
			findings = append(findings, fmt.Sprintf("%s: cannot read %s at %s (%v); %s displaces it, so the pin cannot be checked",
				pin.handOwn, pin.goFile, version, err, pin.handOwn))
		case got == "":
			findings = append(findings, fmt.Sprintf("%s: func %s is gone from %s at %s; the displacement in manualConversionFuncs and the hand-owned body must follow upstream",
				pin.handOwn, pin.funcName, pin.goFile, version))
		case got != pin.sha256:
			findings = append(findings, fmt.Sprintf("%s: func %s in %s changed at %s (sha256 %s, pinned %s). The hand-owned body re-implements the OLD algorithm: re-derive it against the new text, re-run %s, then update the pin",
				pin.handOwn, pin.funcName, pin.goFile, version, got, pin.sha256, pin.reverify))
		}
	}

	return findings
}

// pinnedGoRootForPins is GOROOT at src/version.props' <GoStdLibVersion>, chosen as the fixture guard
// chooses it: the running toolchain when its version is the pin, else the module cache's.
func pinnedGoRootForPins(t *testing.T, root string) (string, string) {
	props, err := os.ReadFile(filepath.Join(root, "src", "version.props"))

	if err != nil {
		t.Fatalf("cannot read src/version.props: %v", err)
	}

	pin := releasestamp.StdLibVersion(string(props))

	if pin == "" {
		t.Fatalf("src/version.props has no <GoStdLibVersion>; the source pins have no version")
	}

	want := "go" + pin
	goRoot := build.Default.GOROOT

	if goRootVersion(goRoot) != want {
		goRoot = cachedToolchains()[want]
	}

	if goRoot == "" {
		t.Fatalf("no GOROOT at the pinned %s: the running toolchain is %s (GOROOT %s) and the module cache holds no %s toolchain for %s/%s. "+
			"Run with GOTOOLCHAIN=%s; this guard fails rather than skips, since a skip measures nothing",
			want, runtime.Version(), build.Default.GOROOT, want, runtime.GOOS, runtime.GOARCH, want)
	}

	return goRoot, want
}

// TestHandOwnedReimplementationsMatchPinnedGoSource is the guard.
func TestHandOwnedReimplementationsMatchPinnedGoSource(t *testing.T) {
	root := repoRootFromPackageDir(t)
	goRoot, version := pinnedGoRootForPins(t, root)

	for _, pin := range handOwnSourcePins {
		if _, err := os.Stat(filepath.Join(root, filepath.FromSlash(pin.handOwn))); err != nil {
			t.Errorf("%s: the pinned hand-owned body is missing (%v); a pin without its body pins nothing", pin.handOwn, err)
		}
	}

	findings := scanHandOwnSourcePins(handOwnSourcePins, goRoot, version)

	for _, finding := range findings {
		t.Error(finding)
	}

	t.Logf("pinned %s (GOROOT %s) · %d hand-owned re-implementations · %d changed", version, goRoot, len(handOwnSourcePins), len(findings))
}

// TestHandOwnSourcePinScannerFires is the positive control.
func TestHandOwnSourcePinScannerFires(t *testing.T) {
	goRoot := t.TempDir()
	dir := filepath.Join(goRoot, "src", "pkg")

	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	write := func(text string) {
		if err := os.WriteFile(filepath.Join(dir, "f.go"), []byte(text), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	original := "package pkg\n\n// Kept is documented.\nfunc Kept(a int) int {\n\treturn a + 1\n}\n\n// Changed is documented.\nfunc Changed(a int) int {\n\treturn a * 2\n}\n\n// Docs is documented.\nfunc Docs(a int) int {\n\treturn a - 1\n}\n\nfunc Gone() {}\n"
	write(original)

	pinOf := func(name string) handOwnSourcePin {
		sum, err := funcSourceHash(filepath.Join(dir, "f.go"), name)

		if err != nil || sum == "" {
			t.Fatalf("control setup: no hash for %s (%v)", name, err)
		}

		return handOwnSourcePin{goFile: "pkg/f.go", funcName: name, sha256: sum, handOwn: "src/core/pkg/" + strings.ToLower(name) + "_impl.cs", reverify: "its test"}
	}

	pins := []handOwnSourcePin{pinOf("Kept"), pinOf("Changed"), pinOf("Docs"), pinOf("Gone")}

	if findings := scanHandOwnSourcePins(pins, goRoot, "go0.0.0"); len(findings) != 0 {
		t.Fatalf("an unchanged GOROOT reported %d findings: %v", len(findings), findings)
	}

	write(strings.NewReplacer(
		"return a * 2", "return a * 3",
		"// Docs is documented.", "// Docs is documented, and now longer.",
		"func Gone() {}\n", "",
	).Replace(original))

	findings := scanHandOwnSourcePins(pins, goRoot, "go0.0.1")
	joined := strings.Join(findings, "\n")

	if len(findings) != 2 {
		t.Fatalf("want exactly the changed body and the missing function reported, got %d: %v", len(findings), findings)
	}

	for _, want := range []string{"src/core/pkg/changed_impl.cs: func Changed in pkg/f.go changed at go0.0.1", "src/core/pkg/gone_impl.cs: func Gone is gone"} {
		if !strings.Contains(joined, want) {
			t.Errorf("findings do not name %q:\n%s", want, joined)
		}
	}

	if strings.Contains(joined, "kept_impl.cs") || strings.Contains(joined, "docs_impl.cs") {
		t.Errorf("an unchanged body or a doc-comment-only change was reported:\n%s", joined)
	}
}
