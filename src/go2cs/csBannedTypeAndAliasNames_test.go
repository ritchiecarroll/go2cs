// csBannedTypeAndAliasNames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// csBannedTypeAndAliasNames are the C# contextual keywords the compiler REFUSES as a type or alias name, with the error
// each draws. Measured on the .NET 10 SDK at LangVersion 14 by compiling `using <word> = System.String;` for every
// contextual keyword: these four are errors; every other one only warns (CS8981 lower-case type name, CS8860 `record`,
// CS0440 `global`). A Go package so named reaches C# as an import alias, and a Go type so named as a type name.
var csBannedTypeAndAliasNames = map[string]string{
	"file":      "CS9056",
	"required":  "CS9029",
	"scoped":    "CS9062",
	"extension": "CS9306",
}

// csBannedOnlyAsTypeOrAlias are the banned names that are escaped ONLY where C# bans them. Escaped everywhere,
// `extension` put '@' on std locals that never change meaning, so it is handled like the C# 15 names in
// csReservedTypeNames: a declared type is Δ-renamed package-wide and an import alias is '@'-escaped; a local stays
// bare. The others predate that set and are full keywords, escaped in every position.
var csBannedOnlyAsTypeOrAlias = map[string]bool{"extension": true}

// Every banned name is '@'-escaped as an import alias (getSanitizedImport), and is either a full keyword (escaped as
// any identifier) or a reserved type name (Δ-renamed where declared, bare as a local). Guarded end to end by the
// behavioral test KeywordPackageAliases, which imports a package named after each one and compiles the result.
func TestCSBannedTypeAndAliasNamesAreEscaped(t *testing.T) {
	for name, code := range csBannedTypeAndAliasNames {
		if got := getSanitizedImport(name); got != "@"+name {
			t.Errorf("import alias for a package named %q is %q; C# refuses the bare name (%s)", name, got, code)
		}

		if csBannedOnlyAsTypeOrAlias[name] {
			if !csReservedTypeNames.Contains(name) {
				t.Errorf("%q is refused by C# as a type name (%s) but a declaring package is not renamed", name, code)
			}

			if got := getSanitizedIdentifier(name); got != name {
				t.Errorf("local %q is emitted as %q; C# accepts it bare, so the escape is noise", name, got)
			}

			continue
		}

		if !keywords.Contains(name) {
			t.Errorf("%q is refused by C# as a type or alias name (%s) but is not in the keyword set", name, code)
		}

		if got := getSanitizedIdentifier(name); got != "@"+name {
			t.Errorf("identifier %q is emitted as %q; C# refuses the bare name as a type (%s)", name, got, code)
		}
	}
}
