// packageInfoSystemUsings.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/types"
	"os"
	"strings"
)

// The System attributes package_info.cs writes on its method holders (methodHolderAttributes) and forwarders
// (methodForwarderAttributes), in the short spelling the file's `using` directives make available. Both sections
// are rendered in the global:: spelling and respelled here, once, for the whole file.
const (
	methodHolderAttributesShort    = "EditorBrowsable(EditorBrowsableState.Never)"
	methodForwarderAttributesShort = "MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough"
)

// The using directives the short spellings read: the holders' attribute, and the forwarders' attributes.
var (
	methodHolderSystemUsings    = []string{"using System.ComponentModel;"}
	methodForwarderSystemUsings = []string{"using System.Diagnostics;", "using System.Runtime.CompilerServices;"}
)

// packageInfoSystemClashes lists the names the package being converted declares that the System usings would make
// ambiguous in its package_info.cs; set by resetPackageState. Empty, the file spells its System attributes through
// usings; otherwise the whole file keeps the global:: spelling.
var packageInfoSystemClashes []string

// systemUsingClashes returns the names of pkg that the System usings of package_info.cs would make ambiguous.
//
// `using static <pkg>_package;` and a namespace using resolve at the same stage, so a package-scope TYPE named like a
// type of System.ComponentModel, System.Diagnostics or System.Runtime.CompilerServices is ambiguous wherever the file
// spells the name bare at the compilation unit's level (CS0104: an interface record's `GoImplement<StackTrace, …>`;
// the forwarders read it through the namespace's own `using static`, insertForwarderAliasDirectives), and a type
// named like an attribute's short form
// counts too, since `[MethodImpl]` looks up MethodImpl as well as MethodImplAttribute. A name that is not a type matters only where the file spells it in an expression: the two enums the attribute
// arguments read, MethodImplOptions and EditorBrowsableState. A method of either name is one too: its forwarder is a
// member of the holder class whose attributes read the enum, and a member is found before any using.
//
// An external test package's package_info.cs also carries `using static` of the package it tests, so that package's
// names count with its own.
func systemUsingClashes(pkg *types.Package) []string {
	if pkg == nil {
		return nil
	}

	scopes := []*types.Scope{pkg.Scope()}

	if tested, isExternalTest := strings.CutSuffix(pkg.Path(), "_test"); isExternalTest {
		for _, imported := range pkg.Imports() {
			if imported.Path() == tested {
				scopes = append(scopes, imported.Scope())
			}
		}
	}

	enumName := func(name string) bool { return name == "MethodImplOptions" || name == "EditorBrowsableState" }
	seen := map[string]bool{}
	var clashes []string

	add := func(name string) {
		if !seen[name] {
			seen[name] = true
			clashes = append(clashes, name)
		}
	}

	for _, scope := range scopes {
		for _, name := range scope.Names() {
			typeName, isType := scope.Lookup(name).(*types.TypeName)

			if isType && glyphSystemTypeNames[name] || enumName(name) {
				add(name)
			}

			if !isType || typeName.IsAlias() {
				continue
			}

			if named, ok := typeName.Type().(*types.Named); ok {
				for i := range named.NumMethods() {
					if method := named.Method(i).Name(); enumName(method) {
						add(name + "." + method)
					}
				}
			}
		}
	}

	return clashes
}

// respellSystemAttributes rewrites a holder declaration or a forwarder line, in either spelling, to the short one
// (short) or the global:: one, and returns any other line unchanged.
func respellSystemAttributes(line string, short bool) string {
	pairs := [][2]string{
		{methodHolderAttributes, methodHolderAttributesShort},
		{methodForwarderAttributes, methodForwarderAttributesShort},
	}

	for _, pair := range pairs {
		from, to := pair[0], pair[1]

		if !short {
			from, to = to, from
		}

		line = strings.Replace(line, from, to, 1)
	}

	return line
}

// systemAttributeLineKind reports whether a trimmed package_info.cs line is a method-holder declaration or a
// forwarder, in either spelling.
func systemAttributeLineKind(trimmed string) (holder bool, forwarder bool) {
	if methodHolderLineClass(trimmed) != "" {
		return true, false
	}

	return false, strings.HasPrefix(trimmed, "["+methodForwarderAttributes+"]") || strings.HasPrefix(trimmed, "["+methodForwarderAttributesShort+"]")
}

// applyPackageInfoSystemUsings spells the System attributes of package_info.cs's holders and forwarders. Without a
// clash (packageInfoSystemClashes), each is written short and the file's compilation-unit using directives -- the
// template's `using go;` and `using static <ns>.<pkg>_package;` -- gain the namespaces they read, after the last of
// those lines; with one, every line keeps the global:: spelling and the file carries none of them, and stderr names
// the clashing names. A merging write finds lines of an earlier write in either spelling, and the pass converts them
// in either direction, so the file is never left mixed.
func applyPackageInfoSystemUsings(packageInfoLines []string) []string {
	namespaceIndex := -1

	for i, line := range packageInfoLines {
		if strings.HasPrefix(strings.TrimSpace(line), "namespace ") {
			namespaceIndex = i
			break
		}
	}

	systemUsings := map[string]bool{}

	for _, using := range append(append([]string(nil), methodHolderSystemUsings...), methodForwarderSystemUsings...) {
		systemUsings[using] = true
	}

	// Drop the directives an earlier write added: they are re-derived below.
	if namespaceIndex >= 0 {
		kept := make([]string, 0, len(packageInfoLines))

		for i, line := range packageInfoLines {
			if i < namespaceIndex && systemUsings[strings.TrimSpace(line)] {
				continue
			}

			kept = append(kept, line)
		}

		packageInfoLines = kept
	}

	short := len(packageInfoSystemClashes) == 0 && namespaceIndex >= 0
	hasHolders, hasForwarders := false, false

	for i, line := range packageInfoLines {
		trimmed := strings.TrimSpace(line)
		holder, forwarder := systemAttributeLineKind(trimmed)

		if !holder && !forwarder {
			continue
		}

		hasHolders = hasHolders || holder
		hasForwarders = hasForwarders || forwarder
		packageInfoLines[i] = line[:len(line)-len(strings.TrimLeft(line, " \t"))] + respellSystemAttributes(trimmed, short)
	}

	if !hasHolders && !hasForwarders {
		return packageInfoLines
	}

	if !short {
		if len(packageInfoSystemClashes) > 0 {
			fmt.Fprintf(os.Stderr, "package_info.cs of %s keeps the global:: spelling of its System attributes: the package declares %s\n", currentPackagePath, strings.Join(packageInfoSystemClashes, ", "))
		}

		return packageInfoLines
	}

	var usings []string

	if hasHolders {
		usings = append(usings, methodHolderSystemUsings...)
	}

	if hasForwarders {
		usings = append(usings, methodForwarderSystemUsings...)
	}

	// After the template's compilation-unit directives; a file without them takes the usings right above its
	// namespace declaration.
	anchor := -1

	for i, line := range packageInfoLines {
		trimmed := strings.TrimSpace(line)

		if strings.HasPrefix(trimmed, "namespace ") {
			if anchor < 0 {
				anchor = i - 1
			}

			break
		}

		if trimmed == "using "+RootNamespace+";" || strings.HasPrefix(trimmed, "using static ") {
			anchor = i
		}
	}

	return append(packageInfoLines[:anchor+1:anchor+1], append(usings, packageInfoLines[anchor+1:]...)...)
}
