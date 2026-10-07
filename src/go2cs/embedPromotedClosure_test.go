// embedPromotedClosure_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"testing"

	"golang.org/x/tools/go/packages"
)

// A struct a TEST file declares with an embedded foreign struct (go-cmp's internal/function suite:
// `type myType struct{ bytes.Buffer }`) is compiled into the test assembly, and go2cs-gen emits a
// forwarding method there for every method the embed promotes. bytes.Buffer's ReadFrom and WriteTo
// take an io.Reader and an io.Writer, so binding those forwarders needs the io assembly. The suite
// imports bytes but not io, and the test project disables transitive references, so the compile
// failed CS0234 `go.io_package` x4 in the generated myType.g.cs.
func TestDeclarationClosureImportsSurfacesPromotedMethodSignatures(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod":   "module example/promo\n\ngo 1.23\n",
		"promo.go": "package promo\n\nfunc Answer() int { return 42 }\n",
		"promo_test.go": "package promo\n" +
			"import (\n\t\"bytes\"\n\t\"testing\"\n)\n" +
			"type myType struct{ bytes.Buffer }\n" +
			"func TestPromoted(t *testing.T) {\n" +
			"\tvar m myType\n" +
			"\tm.WriteString(\"hi\")\n" +
			"\tif m.Len() != 2 || Answer() != 42 {\n\t\tt.Fatal(\"bad\")\n\t}\n" +
			"}\n",
	})

	production := loadProductionForDir(t, dir)
	internal, external := loadTestVariantsForDir(t, dir)
	roots := []*packages.Package{production, internal, external}
	referenced := []string{production.PkgPath, "bytes", "testing"}

	if got := declarationClosureImports(roots, nil, referenced, nil, nil); len(got) != 1 || got[0] != "io" {
		t.Fatalf("myType embeds bytes.Buffer, whose promoted ReadFrom/WriteTo name io.Reader/io.Writer; got %v", got)
	}
}

// The PROMOTED-FIELD edge: go2cs-gen also writes a ref accessor per promoted FIELD of the embed, which
// spells the field's type. testify's suite tests embed suite.Suite, whose unexported `mu sync.RWMutex` is
// a legal promoted field in the white-box variant; sync is imported only by the production file, so the
// generated accessors were CS0234 `go.sync_package` in all 13 suite files. The test above is this edge's
// control: bytes.Buffer's fields are all unexported and FOREIGN, so they add nothing beside io.
func TestDeclarationClosureImportsSurfacesPromotedFieldTypes(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/suite\n\ngo 1.23\n",
		"suite.go": "package suite\n" +
			"import \"sync\"\n" +
			"type Suite struct {\n\tmu sync.RWMutex\n\tName string\n}\n" +
			"func (s *Suite) Lock() { s.mu.Lock(); s.mu.Unlock() }\n",
		"suite_test.go": "package suite\n" +
			"import \"testing\"\n" +
			"type mySuite struct{ Suite }\n" +
			"func TestSuite(t *testing.T) {\n\tvar s mySuite\n\ts.Lock()\n\tif s.Name != \"\" {\n\t\tt.Fatal(\"bad\")\n\t}\n}\n",
	})

	production := loadProductionForDir(t, dir)
	internal, external := loadTestVariantsForDir(t, dir)
	roots := []*packages.Package{production, internal, external}
	referenced := []string{production.PkgPath, "testing"}

	if got := declarationClosureImports(roots, nil, referenced, nil, nil); len(got) != 1 || got[0] != "sync" {
		t.Fatalf("mySuite embeds Suite, whose promoted field mu is a sync.RWMutex; got %v", got)
	}
}

// The boundaries: a NAMED field promotes nothing, and a PRODUCTION file's embed is generated in the
// production assembly (with its own references), never in the test assembly.
func TestDeclarationClosureImportsPromotedEdgeIsEmbedAndTestScoped(t *testing.T) {
	for _, fixture := range []struct {
		name, production, test string
	}{
		{
			name:       "named field",
			production: "package promo\n\nfunc Answer() int { return 42 }\n",
			test: "package promo\n" +
				"import (\n\t\"bytes\"\n\t\"testing\"\n)\n" +
				"type holder struct{ b bytes.Buffer }\n" +
				"func TestNamed(t *testing.T) {\n\tvar h holder\n\th.b.WriteString(\"hi\")\n\tif h.b.Len() != 2 || Answer() != 42 {\n\t\tt.Fatal(\"bad\")\n\t}\n}\n",
		},
		{
			name: "production embed",
			production: "package promo\n" +
				"import \"bytes\"\n" +
				"type Holder struct{ bytes.Buffer }\n" +
				"func Answer() int { return 42 }\n",
			test: "package promo\n" +
				"import \"testing\"\n" +
				"func TestProduction(t *testing.T) {\n\tif Answer() != 42 {\n\t\tt.Fatal(\"bad\")\n\t}\n}\n",
		},
	} {
		t.Run(fixture.name, func(t *testing.T) {
			dir := t.TempDir()
			writeModuleFiles(t, dir, map[string]string{
				"go.mod":        "module example/promo\n\ngo 1.23\n",
				"promo.go":      fixture.production,
				"promo_test.go": fixture.test,
			})

			production := loadProductionForDir(t, dir)
			internal, external := loadTestVariantsForDir(t, dir)
			roots := []*packages.Package{production, internal, external}
			referenced := []string{production.PkgPath, "bytes", "testing"}

			if got := declarationClosureImports(roots, nil, referenced, nil, nil); len(got) != 0 {
				t.Fatalf("no test-declared struct embeds a foreign struct, so nothing must be surfaced; got %v", got)
			}
		})
	}
}
