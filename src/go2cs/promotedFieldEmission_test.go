// promotedFieldEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards #31b (fieldAccess.go, fieldPropertySelector): a PROMOTED field selected through a pointer reads and writes as
// Go spells it, `p.f`, through the property go2cs-gen walks over the embeds (FieldAccessGenerator.PromotedFields), where
// it was `(~p).f` and `p.Value.f`. Through value embeds and pointer embeds alike, at any depth, for a struct of this
// package and of another. The controls keep today's spelling: a promoted field named like a member of ж<T>, and one
// reached through an embed this package defines over another named struct (its fields are generated, so go2cs-gen
// cannot walk it).

package main

import (
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

func TestPromotedFieldReadsAsGoSpellsIt(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "inner", "inner.go"), "package inner\n\ntype Base struct{ ID int }\n\ntype Outer struct {\n\tBase\n\tName string\n}\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"

	"example.com/fieldaccess/inner"
)

type leaf struct{ n int }

type mid struct {
	leaf
	m int
}

type top struct {
	mid
	t int
}

type viaPtr struct {
	*leaf
	k int
}

type boxed struct{ Value int }

type holdsBoxed struct {
	boxed
	h int
}

type wrap leaf

type viaWrap struct {
	wrap
	w int
}

func positives(tops []*top, ptrs []*viaPtr, outs []*inner.Outer) {
	tops[0].n = 1
	tops[0].m += tops[0].n
	ptrs[0].n = 3
	outs[0].ID = 4
	for _, x := range tops {
		x.n++
	}
	fmt.Println(tops[0].n, tops[0].m, tops[0].t, ptrs[0].n, outs[0].ID)
}

func controls(bs []*holdsBoxed, vs []*viaWrap) {
	bs[0].Value = 5
	vs[0].n = 6
	fmt.Println(bs[0].Value, bs[0].h, vs[0].n, vs[0].w)
}

func main() {
	positives([]*top{{}}, []*viaPtr{{leaf: &leaf{}}}, []*inner.Outer{{}})
	controls([]*holdsBoxed{{}}, []*viaWrap{{}})
}
`)

	out := convertFieldAccessModule(t, root)
	cs := readGenerated(t, filepath.Join(out, "main.cs"))
	info := readGenerated(t, filepath.Join(out, PackageInfoFileName))

	positives := funcBody(t, cs, "void positives(")

	for _, want := range []string{
		"tops[0].n = 1;",
		"tops[0].m += tops[0].n;",
		"ptrs[0].n = 3;",
		"outs[0].ID = 4;",
		"x.n++;",
		"fmt.Println(tops[0].n, tops[0].m, tops[0].t, ptrs[0].n, outs[0].ID);",
	} {
		if !strings.Contains(positives, want) {
			t.Errorf("a promoted field through a pointer does not read as Go spells it (want %q):\n%s", want, positives)
		}
	}

	if strings.Contains(positives, ".Value.") || strings.Contains(positives, "(~") {
		t.Errorf("a promoted field through a pointer still reads through the whole value:\n%s", positives)
	}

	records := fieldAccessRecordsOf(info)
	sort.Strings(records)

	if got, want := strings.Join(records, " "), "Outer holdsBoxed top viaPtr viaWrap"; got != want {
		t.Errorf("package_info.cs records %q, want %q:\n%s", got, want, info)
	}

	controls := funcBody(t, cs, "void controls(")

	for _, want := range []string{
		"bs[0].Value.Value = 5;", // promoted, and named like a member of ж<T>
		"vs[0].Value.n = 6;",     // promoted through a wrapper this package defines over another named struct
		"fmt.Println((~bs[0]).Value, bs[0].h, (~vs[0]).n, vs[0].w);",
	} {
		if !strings.Contains(controls, want) {
			t.Errorf("a control does not keep today's spelling (want %q):\n%s", want, controls)
		}
	}
}
