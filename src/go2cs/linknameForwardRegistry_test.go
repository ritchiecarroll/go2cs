// linknameForwardRegistry_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// TestLinknameForwardTargetsMatchGoSource checks every linknameForwardTargets row against the REAL Go
// source in GOROOT — the PULL direction's counterpart to TestLinknamePushRegistryMatchesGoSource, and
// for the same reason: while converting the PULLING package the converter sees only a bodyless
// declaration carrying a two-arg directive, and cannot see whether the named target exists, has an
// implementation, or authorizes the pull. The whitelist records that missing half as a human judgment,
// and an unverified judgment is exactly the thing that rots.
//
// The rows fall into two classes and the guard treats them differently, because the two get their C#
// implementation from different places:
//
//   - the NATIVE class (syscall's loadlibrary/loadsystemlibrary/getprocaddress) is bodyless assembly
//     in Go everywhere it is declared; its C# body is hand-written in the converted standard library,
//     so no handle is involved and none is required;
//   - the ORDINARY-CONVERTED-GO class (time.registerLoadFromEmbeddedTZData, runtime.fcntl,
//     runtime.blockUntilEmptyFinalizerQueue, net/textproto.readMIMEHeader) is real Go the converter
//     emits itself — and that emission is `internal` for an unexported name unless packageFuncAccess
//     widens it, which it does ONLY when the defining package carries the one-arg `//go:linkname
//     <name>` handle. Without the handle the forwarder compiles into a different assembly and calls
//     an inaccessible symbol: CS0122, at corpus-build time, for a row that looked perfectly fine here.
//
// A row with a linknameForwardDefinitions entry is a third shape and is verified at its DEFINITION:
// Go gives the symbol its body in a func of another name (time's legacyAbsClock for time.absClock),
// under a two-arg directive naming the symbol, and that directive -- not a one-arg handle -- is the
// authorization packageFuncAccess widens on. The symbol itself is not a func name there (and
// time.Time.abs is not even a method), so the lookup below would find nothing and report a stale row.
//
// So the discriminator is "does Go give this symbol a body anywhere", which is also why the body scan
// looks at EVERY declaration rather than the first: runtime.fcntl is declared once per GOOS, bodyless
// on the BSDs and with a body on linux/darwin/solaris/aix, and which one comes first is an artifact of
// filename order rather than a fact about the row.
//
// Build constraints are ignored throughout (parseGoPackageDir scans every .go file in the directory),
// so a windows-only or unix-only target is verifiable from any lane's host.
func TestLinknameForwardTargetsMatchGoSource(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("GOROOT not resolvable; nothing to verify the registry against")
	}

	if len(linknameForwardTargets) == 0 {
		t.Fatal("linknameForwardTargets is empty: the registry guard is vacuous")
	}

	for target, definition := range linknameForwardDefinitions {
		if !linknameForwardTargets[target] {
			t.Errorf("definition entry %q -> %q has no linknameForwardTargets row, so funcLinknameForward never reaches it and it forwards nothing", target, definition)
		}
	}

	for target := range linknameForwardTargets {
		if definition, isDefined := linknameForwardDefinitions[target]; isDefined {
			verifyLinknameForwardDefinition(t, goRoot, target, definition)
			continue
		}

		pkgPath, symbol, ok := splitLastDot(target)

		if !ok {
			t.Errorf("whitelist entry %q is not <pkgPath>.<symbol>", target)
			continue
		}

		decls := findGoFuncDecls(t, goRoot, pkgPath, symbol)

		if len(decls) == 0 {
			t.Errorf("whitelist entry %q: no func %s declared in %s — the row names a symbol Go's source does not have (renamed? deleted?), so it forwards nothing and the pull silently falls back to a throwing stub", target, symbol, pkgPath)
			continue
		}

		// An unexported target with a real Go body is emitted by the converter itself, so it needs
		// the defining package's one-arg handle to be widened to `public` (packageFuncAccess). An
		// EXPORTED name is already public and needs no handle whatever its body.
		if !anyDeclHasBody(decls) || token.IsExported(symbol) {
			continue
		}

		if !pkgHasLinknameHandle(t, goRoot, pkgPath, symbol) {
			t.Errorf("whitelist entry %q: %s has a Go body but %s carries no one-arg `//go:linkname %s` handle — packageFuncAccess widens a forward target to `public` only on that handle, so the emitted forwarder would call an `internal` symbol across an assembly boundary (CS0122)", target, symbol, pkgPath, symbol)
		}
	}
}

// verifyLinknameForwardDefinition checks a forward row whose symbol Go defines under another name: the
// definition exists in the symbol's own package with a real body, and carries the exact two-arg
// `//go:linkname <definition> <symbol>` directive that gives the symbol that body. Remove or respell the
// directive at a new pin and the row is forwarding to a func Go no longer links to the symbol.
func verifyLinknameForwardDefinition(t *testing.T, goRoot string, target string, definition string) {
	t.Helper()

	pkgPath, definitionFunc, ok := splitLastDot(definition)

	if !ok {
		t.Errorf("definition entry %q -> %q is not <pkgPath>.<func>", target, definition)
		return
	}

	// The directive names the definition's OWN package; a symbol elsewhere is a push, not this shape.
	if !strings.HasPrefix(target, pkgPath+".") {
		t.Errorf("definition entry %q -> %q: the symbol is not in the definition's package %s, so this is not a definition under another name", target, definition, pkgPath)
		return
	}

	decls := findGoFuncDecls(t, goRoot, pkgPath, definitionFunc)

	if len(decls) == 0 {
		t.Errorf("definition entry %q -> %q: no func %s declared in %s (renamed? deleted?), so the forwarder calls nothing", target, definition, definitionFunc, pkgPath)
		return
	}

	if !anyDeclHasBody(decls) {
		t.Errorf("definition entry %q -> %q: %s has no Go body anywhere, so the converter emits no implementation to forward to", target, definition, definitionFunc)
	}

	if !pkgHasLinknamePush(t, goRoot, pkgPath, definitionFunc, target) {
		t.Errorf("definition entry %q -> %q: %s carries no `//go:linkname %s %s` -- Go does not give the symbol this body, so the row forwards to the wrong func", target, definition, pkgPath, definitionFunc, target)
	}
}

// findGoFuncDecls returns EVERY package-level func declaration named symbol in pkgPath. Unlike
// findGoFuncDecl (which answers the push guard's "the one consumer declaration" question) this
// collects all of them, because a runtime symbol is routinely declared once per GOOS with different
// shapes and the guard's question is about the set, not about whichever file sorts first.
func findGoFuncDecls(t *testing.T, goRoot string, pkgPath string, symbol string) []*ast.FuncDecl {
	t.Helper()

	var decls []*ast.FuncDecl

	for _, file := range parseGoPackageDir(t, goRoot, pkgPath) {
		for _, decl := range file.Decls {
			funcDecl, isFunc := decl.(*ast.FuncDecl)

			// Recv != nil is a method, which a linkname forward target never is.
			if !isFunc || funcDecl.Recv != nil || funcDecl.Name == nil || funcDecl.Name.Name != symbol {
				continue
			}

			decls = append(decls, funcDecl)
		}
	}

	return decls
}

// anyDeclHasBody reports whether Go supplies a real body for the symbol anywhere — the discriminator
// between the native class (bodyless assembly on every platform, C# body hand-written) and the
// ordinary-converted-Go class (the converter emits the body, so accessibility is its problem).
func anyDeclHasBody(decls []*ast.FuncDecl) bool {
	for _, decl := range decls {
		if decl.Body != nil {
			return true
		}
	}

	return false
}

// pkgHasLinknameHandle reports whether pkgPath carries the one-argument `//go:linkname <symbol>`
// handle — Go 1.23's opt-in authorizing other packages to linkname-PULL the symbol, and the exact
// condition collectLinknameHandles records and packageFuncAccess gates the `public` widening on. The
// whole comment set is scanned rather than each func's doc, because Go places these directives freely
// (net/textproto keeps readMIMEHeader's on its own line above the doc comment, and runtime collects
// many of them in linkname.go away from the definitions they open).
func pkgHasLinknameHandle(t *testing.T, goRoot string, pkgPath string, symbol string) bool {
	t.Helper()

	for _, file := range parseGoPackageDir(t, goRoot, pkgPath) {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				fields := strings.Fields(comment.Text)

				if len(fields) == 2 && fields[0] == "//go:linkname" && fields[1] == symbol {
					return true
				}
			}
		}
	}

	return false
}

// TestLinknameForwardTargetsExposeNoUnexportedTypes refuses a row whose target's signature names an
// UNEXPORTED named type of the target package. packageFuncAccess emits a forward target `public`, and a
// public C# method whose parameter or result type is `internal` is CS0051 -- in the TARGET package's
// own compilation, for a row TestLinknameForwardTargetsMatchGoSource passes (body and handle both
// present). Go's linker allows such a pull because it links symbols, not declarations; C# accessibility
// does not. Measured 2026-10-02 (S7): a syscall.sysctl row, whose `mib []_C_int` names syscall's
// unexported `type _C_int int32`, broke the darwin syscall build with exactly that error. Such a pull
// needs a hand companion in the PULLING package instead (S7b: vendor/golang.org/x/net/route).
//
// Every declaration WITH A BODY in a file one of the corpus's targets builds (windows, linux, darwin;
// amd64; cgo off) is checked, because each is one the converter emits; a bodyless one (assembly,
// another GOOS's stub) emits no signature to widen, and a file no corpus target builds (wasip1's
// `openat(..., pathLen size, ...)`) is never converted at all. A definition row is checked at its
// definition, which is the func the converter widens. Each offending type is reported once per row.
func TestLinknameForwardTargetsExposeNoUnexportedTypes(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("GOROOT not resolvable; nothing to verify the registry against")
	}

	walked := 0

	for target := range linknameForwardTargets {
		widened := target

		if definition, isDefined := linknameForwardDefinitions[target]; isDefined {
			widened = definition
		}

		pkgPath, symbol, ok := splitLastDot(widened)

		if !ok {
			continue
		}

		files := corpusTargetFiles(t, goRoot, pkgPath)
		typeNames := packageTypeNames(files)
		reported := map[signatureTypeUse]bool{}

		for _, file := range files {
			for _, d := range file.Decls {
				decl, isFunc := d.(*ast.FuncDecl)

				if !isFunc || decl.Recv != nil || decl.Name.Name != symbol || decl.Body == nil {
					continue
				}

				for _, use := range unexportedSignatureTypes(decl, typeNames) {
					if reported[use] {
						continue
					}

					reported[use] = true
					t.Errorf("forward row %q: %s's signature names %s's UNEXPORTED type %s in %s, so the widened `public` target is CS0051 "+
						"(inconsistent accessibility) in %s's own build. Remove the row and give the pull a hand companion in the "+
						"pulling package (the S7b shape), which can stay `internal` on both sides of the seam",
						target, symbol, pkgPath, use.typeName, use.where, pkgPath)
				}
			}
		}

		walked++
	}

	if walked == 0 {
		t.Fatal("no forward row walked: the arm is vacuous")
	}

	t.Logf("walked %d forward rows", walked)
}

// corpusTargetFiles parses the non-test files of pkgPath that at least one corpus target builds: the
// -stdlib platforms windows/amd64, linux/amd64 and darwin/amd64, with cgo off (the corpus's pinned state).
func corpusTargetFiles(t *testing.T, goRoot string, pkgPath string) []*ast.File {
	t.Helper()

	dir := filepath.Join(goRoot, "src", filepath.FromSlash(pkgPath))
	entries, err := os.ReadDir(dir)

	if err != nil {
		t.Errorf("reading %s: %v", dir, err)
		return nil
	}

	fileSet := token.NewFileSet()
	var files []*ast.File

	for _, entry := range entries {
		name := entry.Name()

		if !strings.HasSuffix(name, ".go") || strings.HasSuffix(name, "_test.go") {
			continue
		}

		built := false

		for _, goos := range []string{"windows", "linux", "darwin"} {
			ctx := build.Default
			ctx.GOROOT, ctx.GOOS, ctx.GOARCH, ctx.CgoEnabled = goRoot, goos, "amd64", false

			if match, err := ctx.MatchFile(dir, name); err == nil && match {
				built = true
				break
			}
		}

		if !built {
			continue
		}

		if file, err := parser.ParseFile(fileSet, filepath.Join(dir, name), nil, 0); err == nil {
			files = append(files, file)
		}
	}

	return files
}

// TestUnexportedSignatureTypesFires is the arm's positive control, on synthetic source: an unexported
// package type in a parameter, nested in a slice, and in a result is reported with its position, while
// a builtin, an exported type, a qualified type and a type parameter are not.
func TestUnexportedSignatureTypesFires(t *testing.T) {
	source := `package p
type _C_int int32
type Exported int
type hidden struct{}
func f[T any](mib []_C_int, n int, e Exported, q other.Thing, v T) (h *hidden, err error) { return nil, nil }
`
	file, err := parser.ParseFile(token.NewFileSet(), "p.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	var decl *ast.FuncDecl

	for _, d := range file.Decls {
		if funcDecl, isFunc := d.(*ast.FuncDecl); isFunc {
			decl = funcDecl
		}
	}

	var got []string

	for _, use := range unexportedSignatureTypes(decl, packageTypeNames([]*ast.File{file})) {
		got = append(got, use.typeName+" in "+use.where)
	}

	if want := "_C_int in parameter mib, hidden in result h"; strings.Join(got, ", ") != want {
		t.Fatalf("unexportedSignatureTypes = %q, want %q", strings.Join(got, ", "), want)
	}
}

// packageTypeNames returns every package-level type name declared in files.
func packageTypeNames(files []*ast.File) map[string]bool {
	names := map[string]bool{}

	for _, file := range files {
		for _, decl := range file.Decls {
			genDecl, isGen := decl.(*ast.GenDecl)

			if !isGen || genDecl.Tok != token.TYPE {
				continue
			}

			for _, spec := range genDecl.Specs {
				names[spec.(*ast.TypeSpec).Name.Name] = true
			}
		}
	}

	return names
}

type signatureTypeUse struct {
	typeName string
	where    string // "parameter <name>" or "result <name>"
}

// unexportedSignatureTypes reports each unexported package type named anywhere in decl's parameter or
// result types. A qualified type (pkg.T) belongs to another package and is skipped whole.
func unexportedSignatureTypes(decl *ast.FuncDecl, typeNames map[string]bool) []signatureTypeUse {
	var uses []signatureTypeUse

	scan := func(fields *ast.FieldList, kind string) {
		if fields == nil {
			return
		}

		for i, field := range fields.List {
			where := fmt.Sprintf("%s #%d", kind, i+1)

			if len(field.Names) > 0 {
				where = kind + " " + field.Names[0].Name
			}

			ast.Inspect(field.Type, func(node ast.Node) bool {
				switch n := node.(type) {
				case *ast.SelectorExpr:
					return false
				case *ast.Ident:
					if !token.IsExported(n.Name) && typeNames[n.Name] {
						uses = append(uses, signatureTypeUse{typeName: n.Name, where: where})
					}
				}

				return true
			})
		}
	}

	scan(decl.Type.Params, "parameter")
	scan(decl.Type.Results, "result")

	return uses
}

// TestLinknameForwardHandlesCoverEveryPullingTarget is the per-GOOS half that
// TestLinknameForwardTargetsMatchGoSource cannot be: that guard ignores build constraints, so a handle
// in ANY file satisfies it. packageFuncAccess widens a forward target only when the handle is in the
// files THIS target's conversion reads, so a row whose handle builds on darwin alone but whose puller
// also builds on linux passes there and is CS0122 in the linux corpus. Measured at the Go 1.26 hop:
// internal/syscall/unix's utimes.go (unix) pulls syscall.utimensat, whose one-arg handle is only in
// syscall_darwin.go.
//
// For every row whose target the converter emits with a body (unexported, not a definition row), each
// corpus target (windows, linux, darwin; amd64; cgo off; purego) on which a file pulling it builds must
// also build a file of the target's package carrying the handle — or the row must be in
// linknameForwardHandleElsewhere, which widens without the local handle, and then must still need it.
func TestLinknameForwardHandlesCoverEveryPullingTarget(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("GOROOT not resolvable; nothing to verify the registry against")
	}

	for target := range linknameForwardHandleElsewhere {
		if !linknameForwardTargets[target] {
			t.Errorf("linknameForwardHandleElsewhere entry %q has no linknameForwardTargets row, so it widens a symbol nothing forwards to", target)
		}
	}

	pullers := linknamePullSites(t, goRoot)
	checked := 0

	for target := range linknameForwardTargets {
		if _, isDefined := linknameForwardDefinitions[target]; isDefined {
			continue
		}

		pkgPath, symbol, ok := splitLastDot(target)

		if !ok || token.IsExported(symbol) || !anyDeclHasBody(findGoFuncDecls(t, goRoot, pkgPath, symbol)) {
			continue
		}

		handled := handleTargets(t, goRoot, pkgPath, symbol)
		var unhandled []string

		for _, site := range pullers[target] {
			for _, goos := range corpusGOOS {
				if !handled[goos] && corpusTargetBuilds(goRoot, site.dir, site.name, goos) {
					unhandled = append(unhandled, goos+" ("+site.pkg+"/"+site.name+")")
				}
			}
		}

		checked++
		_, widenedElsewhere := linknameForwardHandleElsewhere[target]

		switch {
		case len(unhandled) > 0 && !widenedElsewhere:
			t.Errorf("forward row %q: a puller builds where %s carries no one-arg `//go:linkname %s` handle: %s. packageFuncAccess leaves %s `internal` there and the forwarder is CS0122. Add the row to linknameForwardHandleElsewhere with Go's authorization, or give the pull a companion",
				target, pkgPath, symbol, strings.Join(unhandled, ", "), symbol)
		case len(unhandled) == 0 && widenedElsewhere:
			t.Errorf("linknameForwardHandleElsewhere entry %q: every target a puller builds on now carries the handle, so the entry widens nothing the handle does not; remove it", target)
		}
	}

	if checked == 0 {
		t.Fatal("no forward row checked: the guard is vacuous")
	}

	t.Logf("checked %d forward rows; %d rows have a pulling file in GOROOT", checked, len(pullers))
}

var corpusGOOS = []string{"windows", "linux", "darwin"}

// corpusTargetBuilds reports whether file name in dir is compiled by the corpus conversion for goos:
// amd64, cgo off, and the purego tag the -stdlib default passes.
func corpusTargetBuilds(goRoot string, dir string, name string, goos string) bool {
	ctx := build.Default
	ctx.GOROOT, ctx.GOOS, ctx.GOARCH, ctx.CgoEnabled = goRoot, goos, "amd64", false
	ctx.BuildTags = []string{"purego"}

	match, err := ctx.MatchFile(dir, name)

	return err == nil && match
}

// handleTargets returns the corpus GOOS values on which a non-test file of pkgPath carrying the one-arg
// `//go:linkname symbol` handle builds.
func handleTargets(t *testing.T, goRoot string, pkgPath string, symbol string) map[string]bool {
	t.Helper()

	dir := filepath.Join(goRoot, "src", filepath.FromSlash(pkgPath))
	entries, err := os.ReadDir(dir)

	if err != nil {
		t.Errorf("reading %s: %v", dir, err)
		return nil
	}

	handled := map[string]bool{}
	handle := "//go:linkname " + symbol

	for _, entry := range entries {
		name := entry.Name()

		if !strings.HasSuffix(name, ".go") || strings.HasSuffix(name, "_test.go") {
			continue
		}

		content, err := os.ReadFile(filepath.Join(dir, name))

		if err != nil {
			continue
		}

		found := false

		for _, line := range strings.Split(string(content), "\n") {
			if strings.TrimSpace(line) == handle {
				found = true
				break
			}
		}

		if !found {
			continue
		}

		for _, goos := range corpusGOOS {
			if corpusTargetBuilds(goRoot, dir, name, goos) {
				handled[goos] = true
			}
		}
	}

	return handled
}

// linknamePullSite is one Go file carrying a two-arg `//go:linkname <local> <target>` pull.
type linknamePullSite struct {
	pkg, dir, name string
}

var linknamePullDirective = regexp.MustCompile(`^\s*//go:linkname\s+\S+\s+(\S+)\s*$`)

// linknamePullSites maps each linknameForwardTargets row to the GOROOT files that pull it, test files
// included (the -tests conversion forwards their pulls too). cmd/ and testdata/ are never converted.
func linknamePullSites(t *testing.T, goRoot string) map[string][]linknamePullSite {
	t.Helper()

	srcRoot := filepath.Join(goRoot, "src")
	sites := map[string][]linknamePullSite{}

	err := filepath.WalkDir(srcRoot, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			if rel, _ := filepath.Rel(srcRoot, path); rel == "cmd" || entry.Name() == "testdata" {
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(path, ".go") {
			return nil
		}

		content, err := os.ReadFile(path)

		if err != nil || !strings.Contains(string(content), "//go:linkname") {
			return nil
		}

		dir := filepath.Dir(path)
		rel, _ := filepath.Rel(srcRoot, dir)
		seen := map[string]bool{}

		for _, line := range strings.Split(string(content), "\n") {
			match := linknamePullDirective.FindStringSubmatch(line)

			if match == nil || !linknameForwardTargets[match[1]] || seen[match[1]] {
				continue
			}

			seen[match[1]] = true
			sites[match[1]] = append(sites[match[1]], linknamePullSite{pkg: filepath.ToSlash(rel), dir: dir, name: filepath.Base(path)})
		}

		return nil
	})

	if err != nil {
		t.Fatalf("walking %s: %v", srcRoot, err)
	}

	return sites
}
