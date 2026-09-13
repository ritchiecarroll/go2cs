// linknameForwardRegistry_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/token"
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

	for target := range linknameForwardTargets {
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
