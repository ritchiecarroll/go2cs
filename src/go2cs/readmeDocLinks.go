// readmeDocLinks.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A converted package's README is rendered from the package's Go doc comment, and a Go doc comment
// can link. Left to itself, go/doc/comment renders those links RELATIVE — `[io.Reader]` becomes
// `[io.Reader](/io#Reader)` — because the printer's DocLinkBaseURL defaults to empty and
// DocLink.DefaultURL then composes a site-root path. That is exactly right for pkg.go.dev, which
// serves the README's ancestor at its own site root, and exactly wrong for all three surfaces this
// README actually renders on: GitHub resolves `/io#Reader` against github.com, Pages/Jekyll against
// the site root, and nuget.org against nuget.org. The link is dead in every one of them.
//
// So the emitter installs its own resolver. The rules are the two the reader would apply by hand:
// a standard-library package or symbol resolves to pkg.go.dev PINNED to the Go release that produced
// the conversion, and a GOROOT-vendored third-party package resolves to the module version GOROOT's
// own src/vendor/modules.txt records — the same two rules, and the same honesty doctrine, the Docs
// badge beside it already follows (readmeValidationBadge.go).
//
// COMPLETENESS is not a judgement call here, because the grammar is closed. go/doc/comment's Text
// interface has exactly four implementations — Plain, Italic, *Link, *DocLink — and only two of them
// carry a URL:
//
//   - *Link URLs are ABSOLUTE BY CONSTRUCTION. Both of the parser's two link sources require a
//     scheme: parseLink rejects a `[text]: url` definition whose url has no `isScheme(...)://`, and
//     autoURL rejects inline text on the same test. A *Link therefore cannot reach this file with a
//     relative URL, and needs no resolution — it passes through untouched, which is also what the
//     "already-absolute URLs pass through" rule asks for.
//   - *DocLink is the sole relative-URL producer, and its own documentation enumerates the exhaustive
//     set of five field combinations. resolveDocLinkURL answers all five, including the two
//     same-package forms the converter's parser configuration cannot currently produce (it leaves
//     Parser.LookupSym nil, so `[Name]` stays literal text rather than becoming a link). Answering
//     them anyway is what makes the resolver total against the grammar rather than against today's
//     corpus census.
package main

import (
	"fmt"
	"go/doc/comment"
	"strings"
)

// resolveDocLinkURL resolves one doc link to a fully-qualified, version-pinned URL.
//
// currentImportPath is the import path of the package being converted, which is what the two
// same-package forms (Name, or Recv+Name, with no ImportPath) are relative TO. version is the Go
// release that produced this conversion, and goRoot is the distribution the vendored module pins are
// read from.
//
// The five combinations DocLink documents, and what each yields:
//
//	ImportPath              https://pkg.go.dev/io@go1.23.1
//	ImportPath, Name        https://pkg.go.dev/io@go1.23.1#Reader
//	ImportPath, Recv, Name  https://pkg.go.dev/io@go1.23.1#Writer.Write
//	Name                    https://pkg.go.dev/<current>@go1.23.1#Name
//	Recv, Name              https://pkg.go.dev/<current>@go1.23.1#Recv.Name
//
// The one case with nowhere honest to point — a same-package link from a conversion whose import
// path could not be recovered — falls back to DefaultURL, i.e. to precisely what would have been
// emitted before this resolver existed. That keeps an unrecoverable case no worse than the status
// quo instead of inventing an absolute URL for a package this conversion cannot name.
func resolveDocLinkURL(link *comment.DocLink, currentImportPath string, version string, goRoot string) string {
	importPath := link.ImportPath

	if importPath == "" {
		importPath = currentImportPath
	}

	if importPath == "" {
		return link.DefaultURL("")
	}

	return pkgGoDevPackageURL(importPath, version, goRoot) + docLinkFragment(link)
}

// docLinkFragment is the `#Symbol` / `#Recv.Symbol` suffix a doc link carries when it names a symbol
// rather than a whole package, spelled the way pkg.go.dev anchors it — which is the same spelling
// DocLink.DefaultURL uses, since both are naming the same anchor on the same site.
func docLinkFragment(link *comment.DocLink) string {
	if link.Name == "" {
		return ""
	}

	if link.Recv != "" {
		return "#" + link.Recv + "." + link.Name
	}

	return "#" + link.Name
}

// pkgGoDevPackageURL is the fully-qualified pkg.go.dev URL for one Go package, pinned as precisely as
// the conversion can honestly pin it.
//
// A standard-library path pins the Go release, exactly as the Docs badge does: the sources this
// conversion read are that release's sources, so `io@go1.23.1` names the documentation for the very
// code the C# beside the link was converted from.
//
// A path naming an EXTERNAL MODULE (first element carries a dot, so it is a domain rather than a
// std package) is pinned only when GOROOT actually vendors that exact package and modules.txt
// therefore records the snapshot the conversion read. When it does not — `golang.org/x/sys/windows`
// is referenced by std doc comments but is not among the x/sys packages GOROOT vendors — the URL is
// emitted UNVERSIONED rather than borrowed from the module's other vendored packages. That would
// claim a pin the distribution never made for this package, and a fabricated pin is worse than an
// unpinned link: the unpinned one is still fully qualified and still resolves on all three surfaces,
// which is the whole defect being fixed here. Same reasoning as the Source·Go badge's degradation —
// an unresolvable pin costs precision, never correctness.
//
// A `vendor/`-prefixed path is accepted for completeness. No std doc comment writes GOROOT's
// internal spelling, but lookupPkg accepts any slash-bearing import path as written, so the grammar
// permits one and the resolver answers it the same way the Docs badge does.
func pkgGoDevPackageURL(importPath string, version string, goRoot string) string {
	importPath = strings.TrimPrefix(importPath, vendorImportPrefix)

	if isExternalModulePath(importPath) {
		if modulePath, pin, ok := vendoredModulePin(goRoot, importPath); ok {
			target := fmt.Sprintf("%s/%s@%s", goPackageDocsURL, modulePath, pin)

			if subPath := strings.TrimPrefix(strings.TrimPrefix(importPath, modulePath), "/"); subPath != "" {
				target += "/" + subPath
			}

			return target
		}

		return fmt.Sprintf("%s/%s", goPackageDocsURL, importPath)
	}

	if version == "" {
		return fmt.Sprintf("%s/%s", goPackageDocsURL, importPath)
	}

	return fmt.Sprintf("%s/%s@go%s", goPackageDocsURL, importPath, version)
}

// isExternalModulePath reports whether an import path names a module outside the standard library,
// by the same test the go command uses to tell the two apart: a std import path's first element
// never contains a dot, because it is a directory under GOROOT/src rather than a domain name.
func isExternalModulePath(importPath string) bool {
	first, _, _ := strings.Cut(importPath, "/")

	return strings.Contains(first, ".")
}
