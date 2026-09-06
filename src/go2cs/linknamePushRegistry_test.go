// linknamePushRegistry_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestLinknamePushRegistryMatchesGoSource checks every linknamePushTargets row against the REAL Go
// source in GOROOT. This is the one verification the converter structurally cannot do for itself:
// while converting the CONSUMER it sees only that package's syntax, so a bodyless declaration is
// indistinguishable from an ordinary assembly stub and the pushing package's two-arg directive is
// invisible. The registry exists precisely to record that missing half as a human judgment — and an
// unverified judgment is exactly the thing that rots.
//
// It is also the check that would have caught the defect this test was written with. The matcher
// required a one-arg `//go:linkname` handle on every consumer, which is only ONE of the two push
// shapes Go's own standard library uses; `syscall.runtime_envs` has none (the older shape, where the
// only directive sits on the pushing side), so it fell through to a throwing PartialStubGenerator
// stub. Because `syscall.envs` is a package-level var initialized from that call, the throw came out
// of syscall's type initializer and took os.init() — and every Linux program that touches fmt — with
// it. Nothing on Windows could see it: env_unix.go is `//go:build unix || ...`.
//
// Three things are asserted per row, and each maps to a way the row can be wrong:
//
//   - the consumer's declaration EXISTS and is BODYLESS (a row naming a symbol Go has since given a
//     body, renamed, or deleted would otherwise sit in the registry doing nothing, silently);
//   - its SHAPE matches the one the row records (the defect above, in every direction) — one arm
//     per arm of linknamePushDeclMatches, because a row whose shape the matcher rejects forwards
//     nothing while looking entirely healthy in the registry;
//   - the pushing side really does carry `//go:linkname <pusherFunc> <target>` (the authorization
//     the converter is taking the row's word for). For the handle and bare shapes the target IS the
//     row's key; for the SELF-SYMBOL shape it is the symbol the consumer's own two-arg directive
//     names, which is a different name in the same package — so that symbol is read out of Go's
//     source here and both sides are required to agree on it, rather than either being assumed.
//
// Build constraints are deliberately ignored — every .go file in the package directory is scanned,
// so a unix-only or windows-only declaration is found whatever host the test runs on. That is the
// point: the registry is platform-agnostic and must be verifiable from any lane.
func TestLinknamePushRegistryMatchesGoSource(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("GOROOT not resolvable; nothing to verify the registry against")
	}

	if len(linknamePushTargets) == 0 {
		t.Fatal("linknamePushTargets is empty: the registry guard is vacuous")
	}

	for key, push := range linknamePushTargets {
		consumerPkg, symbol, ok := splitLastDot(key)

		if !ok {
			t.Errorf("registry key %q is not <pkgPath>.<symbol>", key)
			continue
		}

		pusherPkg, pusherFunc, ok := splitLastDot(push.source)

		if !ok {
			t.Errorf("registry row %q has source %q, which is not <pkgPath>.<funcName>", key, push.source)
			continue
		}

		// The consumer's declaration: present, bodyless, and the recorded shape.
		decl := findGoFuncDecl(t, goRoot, consumerPkg, symbol)

		if decl == nil {
			t.Errorf("registry row %q: no func %s declared in %s — the row names a symbol Go's source does not have (renamed? deleted?)", key, symbol, consumerPkg)
			continue
		}

		if decl.Body != nil {
			t.Errorf("registry row %q: %s.%s HAS a body in Go's source, so it is not a linkname push consumer at all", key, consumerPkg, symbol)
			continue
		}

		// The shape, asked of Go's source one arm per arm of linknamePushDeclMatches — and, for the
		// self-symbol shape, the symbol the pushing side has to name. It is NOT the key there: the
		// consumer's directive binds its declaration to a DIFFERENT name in its own package, and that
		// name is what runtime pushes into, so taking the key would assert a directive Go never wrote.
		pushTarget := key

		switch {
		case push.bareDecl && push.selfSymbolPull:
			// Nothing in Go's source can make both true, and the matcher answers selfSymbolPull first,
			// so a row recording both is a judgment that has silently lost half of itself.
			t.Errorf("registry row %q records BOTH bareDecl and selfSymbolPull; they are mutually exclusive consumer shapes and the matcher would honor only selfSymbolPull", key)
			continue

		case push.selfSymbolPull:
			linkSymbol, hasSelfSymbol := declLinknameSelfSymbol(decl, consumerPkg)

			if !hasSelfSymbol {
				t.Errorf("registry row %q records selfSymbolPull, but %s.%s carries no two-arg //go:linkname naming its OWN package — the matcher fails closed on the shape, so this row silently forwards nothing (a two-arg directive naming ANOTHER package is a cross-package PULL, a different mechanism, and belongs in linknameForwardTargets)",
					key, consumerPkg, symbol)
				continue
			}

			// The premise of the shape: the pulled name is one the consumer's own package never
			// defines. If it ever gains a definition the directive becomes an ordinary local alias,
			// the forward is wrong rather than merely unnecessary, and this arm says so first.
			if _, localName, ok := splitLastDot(linkSymbol); ok {
				if own := findGoFuncDecl(t, goRoot, consumerPkg, localName); own != nil {
					t.Errorf("registry row %q: %s now declares %s itself, so %s.%s's directive is a local alias and not a pull of another package's push — the forward this row emits would shadow a real declaration",
						key, consumerPkg, localName, consumerPkg, symbol)
				}
			}

			pushTarget = linkSymbol

		case push.bareDecl:
			if declHasLinknameDirective(decl) {
				t.Errorf("registry row %q records bareDecl=true but %s.%s DOES carry a //go:linkname directive of its own — the shape is half the judgment and the matcher fails closed on a mismatch, so this row silently forwards nothing",
					key, consumerPkg, symbol)
			}

		default:
			if !declHasLinknameHandle(decl) {
				t.Errorf("registry row %q records the HANDLE shape but %s.%s carries no one-arg `//go:linkname %s` of its own — the matcher requires the handle exactly, so this row silently forwards nothing (a bodyless declaration with no directive is bareDecl; one with a two-arg directive naming its own package is selfSymbolPull)",
					key, consumerPkg, symbol, symbol)
			}
		}

		// The pushing side's two-arg directive — the authorization the row is vouching for.
		if !pkgHasLinknamePush(t, goRoot, pusherPkg, pusherFunc, pushTarget) {
			t.Errorf("registry row %q: %s does not carry `//go:linkname %s %s` — the push this row claims authorizes the forward does not exist in Go's source", key, pusherPkg, pusherFunc, pushTarget)
		}
	}
}

// TestLinknamePushRoutesNetNewUnixFile pins ONE pair the registry must keep routing: os's
// `net_newUnixFile`, pushed into net's bodyless `newUnixFile`. The guard above verifies the rows the
// registry HAS against Go's source; nothing there notices a row that is DELETED, and this is a pair
// whose absence is expensive to rediscover — the only symptom is os/exec's TestExtraFilesRace
// returning to an infrastructure-error on a Linux sweep, which runs rarely and off this machine.
//
// It is deliberately not a map lookup against itself. Both halves of the pair are re-derived from
// GOROOT — the consumer's bodyless bare declaration and the pusher's two-arg directive — so Go's
// source is the input and the converter's routing is the thing under test; and the last assertion
// exercises packageFuncAccess itself rather than the reverse index it reads, because publicizing the
// pushing definition is what makes the forwarder compile across the assembly boundary at all. Remove
// the row, break the reverse index, or narrow that access rule, and this goes red first.
func TestLinknamePushRoutesNetNewUnixFile(t *testing.T) {
	const (
		consumerKey = "net.newUnixFile"
		pusherPkg   = "os"
		pusherFunc  = "net_newUnixFile"
	)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	if goRoot == "" {
		t.Skip("GOROOT not resolvable; nothing to verify the pair against")
	}

	// Half one, from Go's source: net declares the symbol bodyless, in the BARE shape (no directive
	// of its own). Both properties are what the matcher requires before it will forward.
	decl := findGoFuncDecl(t, goRoot, "net", "newUnixFile")

	if decl == nil {
		t.Fatalf("net.newUnixFile is not declared in %s/src/net — the pair this guard pins no longer exists in Go's source", goRoot)
	}

	if decl.Body != nil {
		t.Fatal("net.newUnixFile HAS a body in Go's source, so it is no longer a linkname push consumer")
	}

	if declHasLinknameDirective(decl) {
		t.Error("net.newUnixFile now carries a //go:linkname directive of its own — the registry row records the BARE shape and the matcher fails closed, so the row would silently forward nothing")
	}

	// Half two, from Go's source: os performs the push.
	if !pkgHasLinknamePush(t, goRoot, pusherPkg, pusherFunc, consumerKey) {
		t.Errorf("%s does not carry `//go:linkname %s %s` — the push this pair depends on does not exist in Go's source", pusherPkg, pusherFunc, consumerKey)
	}

	// The converter's side: the pair is routed, and routed HONORABLY. An unhonorable disposition
	// would announce the wall instead of forwarding, which for this pair would be wrong — os carries
	// the real body and it runs.
	push, routed := linknamePushTargets[consumerKey]

	if !routed {
		t.Fatalf("linknamePushTargets has no row for %q: net's bodyless declaration falls back to a throwing PartialStubGenerator stub, and (*net.TCPListener).File() dies in it", consumerKey)
	}

	if push.source != pusherPkg+"."+pusherFunc {
		t.Errorf("row %q pushes from %q, want %q", consumerKey, push.source, pusherPkg+"."+pusherFunc)
	}

	if !push.bareDecl {
		t.Errorf("row %q records bareDecl=false, but net's declaration carries no directive — the matcher would reject it", consumerKey)
	}

	if push.reason != "" {
		t.Errorf("row %q is recorded UNHONORABLE (%q), so it emits a panicking stub — but os carries the real body (newFile with kindSock) and it runs", consumerKey, push.reason)
	}

	// The access rule, exercised rather than assumed: os's pushing definition is unexported in Go, so
	// only this arm makes it reachable from net's forwarder in another assembly.
	savedPath := currentPackagePath
	currentPackagePath = pusherPkg

	defer func() { currentPackagePath = savedPath }()

	if access := packageFuncAccess(pusherFunc, true); access != "public" {
		t.Errorf("packageFuncAccess(%q) = %q, want \"public\": net's forwarder calls it across an assembly boundary and an unexported Go name is otherwise emitted internal", pusherFunc, access)
	}
}

// TestSelfSymbolPullDiscriminationOnSyntheticSource pins the SELF-SYMBOL arm's three questions on
// source this test writes, because Go's own source cannot be made to answer them the other way and a
// gate that has never been made to fail proves nothing.
//
// The arm above asks: does the consumer carry a two-arg //go:linkname naming its OWN package (and
// only its own), and is the name it pulls one that package does not itself declare? The first half
// is exercised against Go's source by flipping the registry row's shape flags; the second cannot be,
// since making it fire would mean editing GOROOT — the oracle every measurement on this tree is read
// against. So it is exercised here, through the same helpers in the same order the arm calls them.
//
// It is deliberately not a set of assertions that could all pass on a helper stuck at one answer:
// arm 1 requires TRUE, arms 3 through 5 require FALSE for three different reasons, and arm 2 is the
// composition that must find a local declaration where arm 1 must not.
func TestSelfSymbolPullDiscriminationOnSyntheticSource(t *testing.T) {
	root := t.TempDir()

	write := func(pkg string, body string) {
		t.Helper()

		dir := filepath.Join(root, "src", pkg)

		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatalf("MkdirAll(%s): %v", dir, err)
		}

		if err := os.WriteFile(filepath.Join(dir, "a.go"), []byte("package "+pkg+"\n\n"+body), 0o644); err != nil {
			t.Fatalf("WriteFile(%s): %v", dir, err)
		}
	}

	// 1. The healthy self-symbol shape: the directive names this package, and the name it pulls is
	//    declared nowhere in it. This is runtime/pprof's shape, and the arm must stay silent on it.
	write("pulled", "//go:linkname localPull pulled.pushedIn\nfunc localPull() int64\n")

	decl := findGoFuncDecl(t, root, "pulled", "localPull")

	if decl == nil {
		t.Fatal("synthetic package pulled does not parse: the rest of this test would be vacuous")
	}

	target, ok := declLinknameSelfSymbol(decl, "pulled")

	if !ok || target != "pulled.pushedIn" {
		t.Fatalf("declLinknameSelfSymbol = (%q, %v), want (\"pulled.pushedIn\", true)", target, ok)
	}

	if _, localName, split := splitLastDot(target); !split || findGoFuncDecl(t, root, "pulled", localName) != nil {
		t.Errorf("pulled declares %q itself; the healthy shape is one where it does not, so the arm would fire on Go's shape too", target)
	}

	// 2. The rot the arm exists to catch: the pulled name gains a local declaration, which turns the
	//    directive into an ordinary alias and makes the forward a shadow rather than a completion.
	write("shadowed", "//go:linkname localPull shadowed.pushedIn\nfunc localPull() int64\n\nfunc pushedIn() int64 { return 0 }\n")

	decl = findGoFuncDecl(t, root, "shadowed", "localPull")
	target, ok = declLinknameSelfSymbol(decl, "shadowed")

	if !ok {
		t.Fatal("declLinknameSelfSymbol rejected the shadowed package's directive; the composition below cannot be reached")
	}

	if _, localName, split := splitLastDot(target); !split || findGoFuncDecl(t, root, "shadowed", localName) == nil {
		t.Error("the arm cannot see a local declaration of the pulled name, so it would never fire on the rot it exists to catch")
	}

	// 3. A two-arg directive naming ANOTHER package is a cross-package PULL — the mechanism the
	//    self-symbol arm is distinguished from, and the one it must never admit.
	write("crosspkg", "//go:linkname localPull elsewhere.pushedIn\nfunc localPull() int64\n")

	if target, ok = declLinknameSelfSymbol(findGoFuncDecl(t, root, "crosspkg", "localPull"), "crosspkg"); ok {
		t.Errorf("declLinknameSelfSymbol admitted %q, a directive naming another package: that is a PULL and belongs in linknameForwardTargets, not a push row", target)
	}

	// 4. The one-arg handle is a different shape and must answer the other helper, not this one.
	write("handled", "//go:linkname localPull\nfunc localPull() int64\n")

	decl = findGoFuncDecl(t, root, "handled", "localPull")

	if _, ok = declLinknameSelfSymbol(decl, "handled"); ok {
		t.Error("declLinknameSelfSymbol admitted a one-arg handle: the two shapes would be interchangeable and the registry's judgment meaningless")
	}

	if !declHasLinknameHandle(decl) {
		t.Error("declHasLinknameHandle rejected a one-arg handle naming its own declaration")
	}

	// 5. The bare shape answers no directive question at all.
	write("bare", "func localPull() int64\n")

	decl = findGoFuncDecl(t, root, "bare", "localPull")

	if _, ok = declLinknameSelfSymbol(decl, "bare"); ok {
		t.Error("declLinknameSelfSymbol admitted a declaration carrying no directive")
	}

	if declHasLinknameHandle(decl) || declHasLinknameDirective(decl) {
		t.Error("a bodyless declaration with no directive is reported as carrying one; the bare arm would fail closed on every row")
	}
}

// splitLastDot splits a "<pkgPath>.<name>" record at its LAST dot, so an import path containing dots
// or slashes (internal/weak, golang.org/x/...) keeps its shape.
func splitLastDot(qualified string) (pkgPath string, name string, ok bool) {
	dot := strings.LastIndex(qualified, ".")

	if dot <= 0 || dot == len(qualified)-1 {
		return "", "", false
	}

	return qualified[:dot], qualified[dot+1:], true
}

// directivePhrase renders the assertion failure in the direction the reader needs to act on.
func directivePhrase(hasDirective bool) string {
	if hasDirective {
		return "DOES carry"
	}

	return "does NOT carry"
}

// parseGoPackageDir parses every .go file directly in GOROOT/src/<pkgPath>, comments included and
// build constraints ignored, so a platform-gated declaration is visible from any host.
//
// An EXTERNAL TEST package (`<base>_test`) has no directory of its own: its source is the base
// package's `_test.go` files whose package clause carries the `_test` suffix. That is where a
// production package pushing into its own test package keeps the consumer half
// (runtime/metrics_test.runtime_readMetricNames), so the guard resolves the suffix to the base
// directory and inverts the file selection — test files only, and only those in the external test
// package (in-package `_test.go` files belong to `<base>` itself, a different consumer path).
func parseGoPackageDir(t *testing.T, goRoot string, pkgPath string) []*ast.File {
	t.Helper()

	basePath, isExternalTest := strings.CutSuffix(pkgPath, "_test")

	if !isExternalTest {
		basePath = pkgPath
	}

	dir := filepath.Join(goRoot, "src", filepath.FromSlash(basePath))

	matches, err := filepath.Glob(filepath.Join(dir, "*.go"))

	if err != nil || len(matches) == 0 {
		t.Errorf("no Go source found for package %s at %s", pkgPath, dir)
		return nil
	}

	fileSet := token.NewFileSet()
	files := make([]*ast.File, 0, len(matches))

	for _, match := range matches {
		if strings.HasSuffix(match, "_test.go") != isExternalTest {
			continue
		}

		file, err := parser.ParseFile(fileSet, match, nil, parser.ParseComments)

		if err != nil {
			// A file this converter never reads (assembly stubs with odd build tags) must not fail
			// the guard; a genuinely unparseable package will simply yield no match below.
			continue
		}

		// A `_test.go` whose package clause has no `_test` suffix is an IN-PACKAGE test file —
		// part of `<base>`, not of the external test package this pkgPath names.
		if isExternalTest && (file.Name == nil || !strings.HasSuffix(file.Name.Name, "_test")) {
			continue
		}

		files = append(files, file)
	}

	return files
}

// findGoFuncDecl returns the package-level func declaration named symbol, or nil.
func findGoFuncDecl(t *testing.T, goRoot string, pkgPath string, symbol string) *ast.FuncDecl {
	t.Helper()

	for _, file := range parseGoPackageDir(t, goRoot, pkgPath) {
		for _, decl := range file.Decls {
			funcDecl, isFunc := decl.(*ast.FuncDecl)

			// Recv != nil is a method, which a linkname push consumer never is.
			if !isFunc || funcDecl.Recv != nil || funcDecl.Name == nil || funcDecl.Name.Name != symbol {
				continue
			}

			return funcDecl
		}
	}

	return nil
}

// declHasLinknameDirective reports whether the declaration carries ANY //go:linkname directive of
// its own — the question linknamePushDeclMatches's BARE arm asks (`!hasDirective`), asked here of
// Go's source rather than of the converter's input so the two can never drift apart. The other two
// arms are narrower than "any directive" and have helpers of their own below; asking this one of a
// handle or self-symbol row would pass a declaration the matcher rejects, which is the whole class
// this guard exists to catch.
func declHasLinknameDirective(funcDecl *ast.FuncDecl) bool {
	if funcDecl.Doc == nil {
		return false
	}

	for _, comment := range funcDecl.Doc.List {
		if fields := strings.Fields(comment.Text); len(fields) > 0 && fields[0] == "//go:linkname" {
			return true
		}
	}

	return false
}

// declHasLinknameHandle reports whether the declaration carries the ONE-ARG `//go:linkname <thisFunc>`
// handle — linknamePushDeclMatches's `hasHandle`, and the thing "carries a directive" is not: a
// declaration under a two-arg directive carries one and has no handle at all.
func declHasLinknameHandle(funcDecl *ast.FuncDecl) bool {
	if funcDecl.Doc == nil {
		return false
	}

	for _, comment := range funcDecl.Doc.List {
		fields := strings.Fields(comment.Text)

		if len(fields) == 2 && fields[0] == "//go:linkname" && fields[1] == funcDecl.Name.Name {
			return true
		}
	}

	return false
}

// declLinknameSelfSymbol returns the target of the declaration's two-arg `//go:linkname <thisFunc>
// <ownPkg>.<symbol>` directive — linknamePushDeclMatches's `hasSelfSymbol`, and the symbol the
// pushing side must name. The own-package test is the whole discrimination: the SAME syntax naming
// another package is a cross-package pull, which this must never admit.
func declLinknameSelfSymbol(funcDecl *ast.FuncDecl, consumerPkg string) (target string, ok bool) {
	if funcDecl.Doc == nil {
		return "", false
	}

	for _, comment := range funcDecl.Doc.List {
		fields := strings.Fields(comment.Text)

		if len(fields) != 3 || fields[0] != "//go:linkname" || fields[1] != funcDecl.Name.Name {
			continue
		}

		if pkgPath, _, split := splitLastDot(fields[2]); split && pkgPath == consumerPkg {
			return fields[2], true
		}
	}

	return "", false
}

// pkgHasLinknamePush reports whether pkgPath carries `//go:linkname <pusherFunc> <target>` anywhere
// in its own source — the two-arg directive that actually performs the push. The whole comment set
// is scanned rather than just each func's doc, because Go places these directives freely (mgc.go
// carries unique's beside the definition, mheap.go carries weak's above it).
func pkgHasLinknamePush(t *testing.T, goRoot string, pkgPath string, pusherFunc string, target string) bool {
	t.Helper()

	for _, file := range parseGoPackageDir(t, goRoot, pkgPath) {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				fields := strings.Fields(comment.Text)

				if len(fields) == 3 && fields[0] == "//go:linkname" && fields[1] == pusherFunc && fields[2] == target {
					return true
				}
			}
		}
	}

	return false
}
