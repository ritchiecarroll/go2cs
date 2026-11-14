// cgoLibraryRegistration.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"path/filepath"
	"sort"
	"strings"
)

// Phase 2 of docs/PLAN-cgo-interop.md, increment 4c (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.2, read through §11.11):
// the data a converted cgo package hands golib's one audited resolver (GoCgoLibraries, COORD's Q1 ruling). Each library
// the package's bindings load is a row, its -l flag and the load-time name increment 4b captured for it, and a module
// initializer registers the table for the package's own assembly. The converter emits nothing else of the resolver.

// cgoLibraryEntry is one row of a package's resolver table: the -l flag that names a library, and its captured name.
type cgoLibraryEntry struct {
	flag string
	name string
}

// cgoDefaultLibrariesFlag stands for a library no -l named: one the C toolchain links by default (the C runtime, kernel32
// on windows), which a symbol can resolve to with no #cgo LDFLAGS at all.
const cgoDefaultLibrariesFlag = "(the C toolchain's default libraries)"

// cgoLibraryFlag names the -l a captured library came from, for the resolver's miss message: the -l whose library a
// captured name is (ELF lib<l>.so.N, darwin lib<l>.dylib or lib<l>.B.dylib, windows <l>.dll), or, when the package names
// exactly one -l and no rule matches (an API set for -lm), that one.
func cgoLibraryFlag(flags cgoLinkFlags, name string) string {
	base := strings.ToLower(filepath.Base(strings.ReplaceAll(name, `\`, "/")))

	for _, library := range flags.libraries {
		l := strings.ToLower(library)

		if strings.HasPrefix(base, "lib"+l+".") || base == l+".dll" {
			return "-l" + library
		}
	}

	if len(flags.libraries) == 1 {
		return "-l" + flags.libraries[0]
	}

	return cgoDefaultLibrariesFlag
}

// cgoLibraryTable is a package's resolver table: one row per distinct captured library, in name order.
func cgoLibraryTable(flags cgoLinkFlags, captured []cgoSymbolLibrary) []cgoLibraryEntry {
	seen := map[string]bool{}
	var table []cgoLibraryEntry

	for _, entry := range captured {
		if seen[entry.name] {
			continue
		}

		seen[entry.name] = true
		table = append(table, cgoLibraryEntry{flag: cgoLibraryFlag(flags, entry.name), name: entry.name})
	}

	sort.Slice(table, func(i, j int) bool { return table[i].name < table[j].name })

	return table
}

// cgoLibrariesRegistrationMember is the name of the module initializer, which carries the ᴍ marker so no C name can
// collide with it (C names are ASCII).
const cgoLibrariesRegistrationMember = "register" + cgoMirrorSuffix + "libraries"

// cgoLibrariesRegistration writes the pseudo-package member that registers the package's resolver table when its module
// loads, with the one target its bindings were converted for (GOOS/GOARCH), which golib's resolver holds the running
// process to (§5.3, as amended by §11.20). A module initializer is rooted, so trimming keeps it, and it runs before any
// binding can be called (§6.2).
func cgoLibrariesRegistration(packageName, target string, table []cgoLibraryEntry) string {
	var text strings.Builder

	text.WriteString("    // The package's C libraries, as the converter captured them (§6.1, §11.5), registered with golib's resolver for this\n")
	text.WriteString("    // assembly's own [LibraryImport] bindings when the module loads (§6.2, §11.11).\n")
	text.WriteString("    [global::System.Runtime.CompilerServices.ModuleInitializer]\n")
	fmt.Fprintf(&text, "    internal static void %s() => global::go.golib.GoCgoLibraries.Register(typeof(%s).Assembly, %q", cgoLibrariesRegistrationMember, cgoPseudoPackageClass(packageName), target)

	for _, entry := range table {
		fmt.Fprintf(&text, ",\n        new global::go.golib.GoCgoLibrary(%q, %q)", entry.flag, entry.name)
	}

	text.WriteString(");\n")

	return text.String()
}

// cgoLibrariesSearchPathsAttribute is the assembly attribute a package with bindings carries, so even the runtime's own
// fallback probing is pinned to the safe directories and the assembly's own, never the current directory (§11.11).
// Increment 5 writes it into the bindings file; it applies to every import in the assembly, which is why GolibTests,
// whose other imports it would change, does not compile it.
func cgoLibrariesSearchPathsAttribute() string {
	const interop = "global::System.Runtime.InteropServices."

	return fmt.Sprintf("[assembly: %sDefaultDllImportSearchPaths(%sDllImportSearchPath.SafeDirectories | %sDllImportSearchPath.AssemblyDirectory)]\n", interop, interop, interop)
}
