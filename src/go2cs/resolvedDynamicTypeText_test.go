// resolvedDynamicTypeText_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"reflect"
	"strings"
	"testing"
)

// withDynamicTypeRegistries runs body with the three registries the marker resolver consults replaced by the given
// maps, then puts the originals back. Each global is REPLACED, never mutated, so restoring the saved reference is a
// real restore.
func withDynamicTypeRegistries(t *testing.T, own, production, constraint map[string]string, body func()) {
	t.Helper()

	packageLock.Lock()
	savedOwn, savedProduction, savedConstraint := packageDynamicTypeNames, productionDynamicTypeNames, packageConstraintLiftNames
	packageDynamicTypeNames, productionDynamicTypeNames, packageConstraintLiftNames = own, production, constraint
	packageLock.Unlock()

	defer func() {
		packageLock.Lock()
		packageDynamicTypeNames, productionDynamicTypeNames, packageConstraintLiftNames = savedOwn, savedProduction, savedConstraint
		packageLock.Unlock()
	}()

	body()
}

// TestResolvedDynamicTypeText pins every shape a record's type text can take (sqlite-cs W7, P3's review): a marker
// alone, after a namespace prefix, two in one value, one answered by each registry, an unresolved one (stays AND is
// returned for the caller to report), an undecodable payload, and a prefix with no suffix (not a marker).
func TestResolvedDynamicTypeText(t *testing.T) {
	const (
		ownSig        = "struct{A int32}"
		productionSig = "struct{B int64}"
		constraintKey = "constraint example.com/p.G#0"
		unknownSig    = "struct{C uint8}"
	)

	own := map[string]string{ownSig: "Tᴛ1"}
	production := map[string]string{productionSig: "Pᴛ1"}
	constraint := map[string]string{constraintKey: "G_T_constraint"}
	undecodable := dynamicTypeMarkerPrefix + "not-hex" + dynamicTypeMarkerSuffix

	cases := []struct {
		name, text, want string
		unresolved       []string
	}{
		{"marker alone", dynamicTypeMarker(ownSig), "Tᴛ1", nil},
		{"after a namespace prefix", "go.main_package." + dynamicTypeMarker(ownSig), "go.main_package.Tᴛ1", nil},
		{"two in one value", "map<" + dynamicTypeMarker(ownSig) + ", " + dynamicTypeMarker(productionSig) + ">",
			"map<Tᴛ1, Pᴛ1>", nil},
		{"production registry", dynamicTypeMarker(productionSig), "Pᴛ1", nil},
		{"constraint registry", dynamicTypeMarker(constraintKey), "G_T_constraint", nil},
		{"unresolved stays and is returned", "go.p." + dynamicTypeMarker(unknownSig),
			"go.p." + dynamicTypeMarker(unknownSig), []string{unknownSig}},
		{"one of two unresolved", dynamicTypeMarker(ownSig) + dynamicTypeMarker(unknownSig),
			"Tᴛ1" + dynamicTypeMarker(unknownSig), []string{unknownSig}},
		{"undecodable payload", undecodable, undecodable, []string{"«undecodable marker payload: not-hex»"}},
		{"prefix with no suffix", "go.p." + dynamicTypeMarkerPrefix + "abc", "go.p." + dynamicTypeMarkerPrefix + "abc", nil},
		{"no marker", "go.main_package.TAggInfoᴛ1", "go.main_package.TAggInfoᴛ1", nil},
	}

	withDynamicTypeRegistries(t, own, production, constraint, func() {
		for _, c := range cases {
			got, unresolved := resolvedDynamicTypeText(c.text)

			if got != c.want {
				t.Errorf("%s: text = %q, want %q", c.name, got, c.want)
			}

			if !reflect.DeepEqual(unresolved, c.unresolved) {
				t.Errorf("%s: unresolved = %q, want %q", c.name, unresolved, c.unresolved)
			}
		}
	})
}

// TestExportedTypeAliasRecordReportsAnUnresolvedMarker pins P3's item 1: a GoTypeAlias record whose marker no registry
// answers is written as is (the importer's build names it) AND recorded for unresolvedDynamicTypeError, as the .cs
// resolver records its own. Before, the record path kept the marker silently: the package built green and only an
// importer failed (CS1730), the way W7 first presented.
func TestExportedTypeAliasRecordReportsAnUnresolvedMarker(t *testing.T) {
	const unknownSig = "struct{C uint8}"

	takeUnresolvedDynamicTypes()
	defer takeUnresolvedDynamicTypes()

	withDynamicTypeRegistries(t, map[string]string{"struct{A int32}": "Tᴛ1"}, nil, map[string]string{}, func() {
		resolved := exportedTypeAliasRecord("A", "go.p."+dynamicTypeMarker("struct{A int32}"), "package_info.cs")

		if want := `[assembly: GoTypeAlias("A", "go.p.Tᴛ1")]`; resolved != want {
			t.Fatalf("resolved record = %q, want %q", resolved, want)
		}

		if sites := takeUnresolvedDynamicTypes(); len(sites) != 0 {
			t.Fatalf("a resolved record recorded %d unresolved site(s): %+v", len(sites), sites)
		}

		record := exportedTypeAliasRecord("C", "go.p."+dynamicTypeMarker(unknownSig), "package_info.cs")

		if !strings.Contains(record, dynamicTypeMarkerPrefix) {
			t.Fatalf("an unresolved record lost its marker: %q", record)
		}

		sites := takeUnresolvedDynamicTypes()

		if len(sites) != 1 || sites[0].signature != unknownSig || sites[0].fileName != "package_info.cs" {
			t.Fatalf("unresolved record sites = %+v, want one for %q in package_info.cs", sites, unknownSig)
		}
	})
}
