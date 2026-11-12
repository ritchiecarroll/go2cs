// cgoLibraryRegistration_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"slices"
	"strings"
	"testing"
)

// Each captured library is named for the -l it came from, by each target's file-name rule, or, with exactly one -l,
// that one (an API set for -lm); a library no -l names is one the toolchain links by default.
func TestCgoLibraryFlagsNameTheLibraryTheyCameFrom(t *testing.T) {
	two := cgoLinkFlags{libraries: []string{"m", "go2csa"}}

	for _, c := range []struct {
		flags      cgoLinkFlags
		name, want string
	}{
		{two, "libm.so.6", "-lm"},
		{two, "libgo2csa.so.1", "-lgo2csa"},
		{cgoLinkFlags{libraries: []string{"System"}}, "/usr/lib/libSystem.B.dylib", "-lSystem"},
		{cgoLinkFlags{libraries: []string{"user32", "kernel32"}}, "USER32.dll", "-luser32"},
		{cgoLinkFlags{libraries: []string{"m"}}, "api-ms-win-crt-math-l1-1-0.dll", "-lm"},
		{two, "api-ms-win-crt-math-l1-1-0.dll", cgoDefaultLibrariesFlag},
		{cgoLinkFlags{}, "KERNEL32.dll", cgoDefaultLibrariesFlag},
	} {
		if got := cgoLibraryFlag(c.flags, c.name); got != c.want {
			t.Errorf("%s with %v: %q, want %q", c.name, c.flags.libraries, got, c.want)
		}
	}
}

func TestCgoLibraryTableHasOneRowPerLibrary(t *testing.T) {
	table := cgoLibraryTable(cgoLinkFlags{libraries: []string{"m", "go2csa"}}, []cgoSymbolLibrary{
		{"sin", "libm.so.6"}, {"go2cs_cgo_a", "libgo2csa.so.1"}, {"cos", "libm.so.6"},
	})

	want := []cgoLibraryEntry{{"-lgo2csa", "libgo2csa.so.1"}, {"-lm", "libm.so.6"}}

	if !slices.Equal(table, want) {
		t.Errorf("table %+v, want %+v", table, want)
	}
}

// The registration is a module initializer handing golib's resolver the package's own assembly, its bindings' one target
// (§5.3, §11.20) and its table.
func TestCgoLibrariesRegistrationIsAModuleInitializer(t *testing.T) {
	text := cgoLibrariesRegistration("cgotest", "linux/amd64", []cgoLibraryEntry{{"-lm", "libm.so.6"}})

	for _, want := range []string{
		"[global::System.Runtime.CompilerServices.ModuleInitializer]",
		"internal static void registerᴍlibraries() => global::go.golib.GoCgoLibraries.Register(typeof(cgotestꓸC_package).Assembly, \"linux/amd64\",",
		`new global::go.golib.GoCgoLibrary("-lm", "libm.so.6"));`,
	} {
		if !strings.Contains(text, want) {
			t.Errorf("the registration does not contain %q:\n%s", want, text)
		}
	}

	if attribute := cgoLibrariesSearchPathsAttribute(); !strings.Contains(attribute, "DllImportSearchPath.SafeDirectories | global::System.Runtime.InteropServices.DllImportSearchPath.AssemblyDirectory)]") {
		t.Errorf("the assembly attribute pins no search path: %s", attribute)
	}
}
