// internalPackageMarking.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The marking of Go internal and vendored packages (OQ1, docs/phase4/DESIGN-internal-package-marking.md). Go
// lets only code at or below a package's ROOT import an internal or vendored package; .NET has no such rule, so the
// converted package stays public and is marked instead:
//
//	[assembly: GoInternalRoot("crypto", Standard = true)]
//	...
//	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
//	[GoPackage("aes")]
//	public static partial class aes_package
//
// go2cs-gen and go.lib's consumer analyzer read the assembly stamp and warn (GO2CS0014) on hand-written C# that names
// one of the package's types from outside its root. Both lines are regenerated on every write of package_info.cs, so a
// package that leaves internal/ loses them. Test info files are not marked: a test assembly is never referenced.
package main

import (
	"fmt"
	"path/filepath"
	"slices"
	"strings"
)

// internalPackageBrowsableLine hides the package class from IntelliSense. It is fully qualified, so no Go identifier
// named ComponentModel or EditorBrowsable can capture it and no using line is needed. It goes ABOVE [GoPackage]:
// go.lib.pack.targets reads the package class with a pattern that wants [GoPackage("…")] followed only by whitespace
// and the class declaration, so a line between them fails the pack of every marked package.
const internalPackageBrowsableLine = "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]"

// goInternalRoot returns the root of the import restriction Go places on importPath, and whether there is one, as
// cmd/go/internal/load enforces it (findInternal, disallowVendor):
//
//   - internal: the root is everything before the FINAL `internal` element ("" when the path starts with it);
//   - vendor: the root is the parent of the `vendor` element ("" for GOROOT/src/vendor, the standard library's);
//   - both: the tighter root, which is the longer one.
func goInternalRoot(importPath string) (root string, restricted bool) {
	if importPath == "" {
		return "", false
	}

	elements := strings.Split(importPath, "/")
	internalAt, vendorAt := -1, -1

	for i, element := range elements {
		switch element {
		case "internal":
			internalAt = i
		case "vendor":
			vendorAt = i
		}
	}

	if internalAt < 0 && vendorAt < 0 {
		return "", false
	}

	at := max(internalAt, vendorAt)

	return strings.Join(elements[:at], "/"), true
}

// internalRootStampProseLines returns the comment above the [GoInternalRoot] stamp. Like every emitted-artifact
// comment it states what the stamp says and why.
func internalRootStampProseLines() []string {
	return []string{
		"// This is a Go internal or vendored package: Go lets only code at or below the root named",
		"// below import it. It stays public to .NET, so go2cs-gen and go.lib's analyzer warn",
		"// (GO2CS0014) on hand-written C# that names its types from outside that root.",
	}
}

// isInternalRootStampLine reports whether a line is the [GoInternalRoot] stamp, in any of the spellings
// rootAttributeName gives it.
func isInternalRootStampLine(line string) bool {
	return strings.HasPrefix(respellRootAttribute(line, "GoInternalRoot", true), "[assembly: GoInternalRoot(")
}

// internalRootStamp returns the stamp for the package being converted, or "" when Go places no import restriction on
// it. Standard is set for a package of the standard library, whose root Go tests by directory (below GOROOT/src)
// rather than by import path.
func internalRootStamp(bare bool) string {
	root, restricted := goInternalRoot(goReflectPackagePath(currentPackagePath))

	if !restricted {
		return ""
	}

	arguments := fmt.Sprintf("%q", root)

	if !currentPackageOutsideGoroot {
		arguments += ", Standard = true"
	}

	return "[assembly: " + rootAttributeName("GoInternalRoot", bare) + "(" + arguments + ")]"
}

// applyInternalRootStamp writes the [GoInternalRoot] stamp into package_info.cs of an internal or vendored package,
// immediately above the <GoSourcePositionMaps> prose, and removes it, with its prose and the blank line after it,
// from any other. It is regenerated on every write, so a persisted file never keeps a stale stamp.
func applyInternalRootStamp(packageInfoLines []string, packageInfoFileName string) []string {
	prose := internalRootStampProseLines()
	kept := make([]string, 0, len(packageInfoLines))

	for i := 0; i < len(packageInfoLines); i++ {
		if !isInternalRootStampLine(packageInfoLines[i]) {
			kept = append(kept, packageInfoLines[i])
			continue
		}

		if len(kept) >= len(prose) && slices.Equal(kept[len(kept)-len(prose):], prose) {
			kept = kept[:len(kept)-len(prose)]
		}

		if i+1 < len(packageInfoLines) && strings.TrimSpace(packageInfoLines[i+1]) == "" {
			i++
		}
	}

	if filepath.Base(packageInfoFileName) != PackageInfoFileName {
		return kept
	}

	stamp := internalRootStamp(importsRootNamespace(kept))

	if stamp == "" {
		return kept
	}

	anchor := slices.Index(kept, goSourcePositionMapsProseLines()[0])

	if anchor < 0 {
		anchor = slices.IndexFunc(kept, func(line string) bool { return strings.Contains(line, "<GoSourcePositionMaps>") })
	}

	if anchor < 0 {
		showWarning("Package info file \"%s\" has no <GoSourcePositionMaps> section; its [GoInternalRoot] stamp was not emitted", packageInfoFileName)
		return kept
	}

	block := append(append([]string(nil), prose...), stamp, "")

	return slices.Concat(kept[:anchor], block, kept[anchor:])
}

// applyInternalPackageBrowsable puts the EditorBrowsable line directly above each [GoPackage] stamp of an internal or
// vendored package's package_info.cs, keeping one already there, and removes it from any other. A persisted file is
// patched in place, so this is what reaches an existing file; the package template is used for new files only.
func applyInternalPackageBrowsable(packageInfoLines []string, packageInfoFileName string) []string {
	_, restricted := goInternalRoot(goReflectPackagePath(currentPackagePath))
	mark := restricted && filepath.Base(packageInfoFileName) == PackageInfoFileName
	updated := make([]string, 0, len(packageInfoLines)+1)

	for _, line := range packageInfoLines {
		trimmed := strings.TrimSpace(line)

		if trimmed == internalPackageBrowsableLine {
			continue
		}

		if mark && strings.HasPrefix(trimmed, goPackageStampPrefix) {
			indent := line[:len(line)-len(strings.TrimLeft(line, " \t"))]
			updated = append(updated, indent+internalPackageBrowsableLine)
		}

		updated = append(updated, line)
	}

	return updated
}
