// foreignUnexportedPromotionRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// The defect's own program: go run prints `T main.m lib.m`; the converted program printed `T main.m main.m`, because the
// adapters map lib.I's unexported m to U's own m.
const foreignUnexportedLib = `package lib

type I interface {
	m() string
	Name() string
}

type T struct{}

func (T) m() string    { return "lib.m" }
func (T) Name() string { return "T" }

func CallM(i I) string { return i.m() }

// Sentinel is the crypto/internal/fips140/drbg.DefaultReader shape: an interface whose only method is unexported.
type Sentinel interface{ sentinel() }

type mark struct{}

func (mark) sentinel() {}

var Mark Sentinel = mark{}

func IsMarked(x any) bool { _, ok := x.(Sentinel); return ok }
`

const foreignUnexportedMain = `package main

import (
	"fmt"

	"example/fup/lib"
)

type J interface {
	lib.I
	m() string
}

type U struct{ lib.T }

func (U) m() string { return "main.m" }

func main() {
	var j J = U{}
	fmt.Println(j.Name(), j.m(), lib.CallM(j))
}
`

func writeForeignUnexportedModule(t *testing.T, mainSource string) string {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":     "module example/fup\n\ngo 1.24\n",
		"lib/lib.go": foreignUnexportedLib,
		"main.go":    mainSource,
	})

	return dir
}

// TestForeignUnexportedPromotionRefusedThroughTheCommandLine: the defect's program is refused by name, at the
// conversion, and the run exits non-zero.
func TestForeignUnexportedPromotionRefusedThroughTheCommandLine(t *testing.T) {
	dir := writeForeignUnexportedModule(t, foreignUnexportedMain)

	stderr, err := runConverterOnFixture(t, dir, "CGO_ENABLED=0")

	if err == nil {
		t.Fatalf("U meets lib.I's m only through lib.T's m; the conversion must be refused with a non-zero exit\nstderr:\n%s", stderr)
	}

	if !strings.Contains(stderr, "Refusing to convert: package example/fup") {
		t.Errorf("the human-readable refusal line is missing\nstderr:\n%s", stderr)
	}

	requireStderrLine(t, stderr, dir, "main.go", "(19,12): error GO2CS2016: U is converted to J, whose unexported method m it satisfies only through example/fup/lib.T's m, promoted from package example/fup/lib. Go keeps that method apart from any m of U's own package; go2cs maps an interface method by its bare name, so the converted call would reach the wrong method or one it cannot call. go2cs refuses this conversion")
}

// TestForeignUnexportedPromotionSites: each conversion form the gate reads is refused, and each near miss converts.
func TestForeignUnexportedPromotionSites(t *testing.T) {
	cases := []struct {
		name    string
		body    string
		refused int
	}{
		// Refused: every form of conversion of a U to lib.I, which U meets only through lib.T's m.
		{"argument", `func main() { _ = lib.CallM(U{}) }`, 1},
		{"pointer argument", `func main() { _ = lib.CallM(&U{}) }`, 1},
		{"typed var", `func main() { var i lib.I = U{}; _ = i }`, 1},
		{"assignment", `func main() { var i lib.I; i = U{}; _ = i }`, 1},
		{"return", `func f() lib.I { return U{} }; func main() { _ = f() }`, 1},
		{"composite", `func main() { _ = []lib.I{U{}, U{}} }`, 2},
		{"map value", `func main() { _ = map[string]lib.I{"a": U{}} }`, 1},
		{"struct field", `type H struct{ I lib.I }; func main() { _ = H{I: U{}} }`, 1},
		{"send", `func main() { c := make(chan lib.I, 1); c <- U{} }`, 1},
		{"conversion", `func main() { _ = lib.I(U{}) }`, 1},
		{"variadic", `func g(is ...lib.I) {}; func main() { g(U{}, &U{}) }`, 2},
		{"comparison", `func main() { var i lib.I; _ = i == U{} }`, 1},
		{"closure return", `func main() { f := func() lib.I { return U{} }; _ = f }`, 1},

		// Near misses: no conversion to an interface with lib's unexported method, an interface FIELD promotion,
		// the exported method alone, or a type assertion (asked at run time; KnownIssues names it).
		{"any", `func main() { var a any = U{}; _ = a }`, 0},
		{"exported only", `type N interface{ Name() string }; func main() { var n N = U{}; _ = n }`, 0},
		{"interface field", `type S struct{ lib.Sentinel }; func main() { _ = lib.IsMarked(S{lib.Mark}); var s lib.Sentinel = S{lib.Mark}; _ = s }`, 0},
		{"own package", `type I2 interface{ m() string }; func main() { var i I2 = U{}; _ = i }`, 0},
		{"type assertion", `func main() { var a any = U{}; _, _ = a.(lib.I) }`, 0},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			source := "package main\n\nimport \"example/fup/lib\"\n\ntype U struct{ lib.T }\n\nfunc (U) m() string { return \"main.m\" }\n\n" + c.body + "\n"
			pkg := loadProductionForDir(t, writeForeignUnexportedModule(t, source))
			refusal := refuseForeignUnexportedPromotions(pkg)

			got := 0

			if refusal != nil {
				got = len(refusal.sites)
			}

			if got != c.refused {
				var texts []string

				if refusal != nil {
					for _, site := range refusal.sites {
						texts = append(texts, site.position.String()+": "+site.text)
					}
				}

				t.Fatalf("refused %d site(s), want %d\n%s", got, c.refused, strings.Join(texts, "\n"))
			}
		})
	}
}

// TestForeignUnexportedPromotionCannotBeDowngraded: -warn refuses the code, naming why.
func TestForeignUnexportedPromotionCannotBeDowngraded(t *testing.T) {
	_, err := parseWarnCodes(diagnosticForeignUnexportedPromotionRefused)

	if err == nil || !strings.Contains(err.Error(), "silent") {
		t.Fatalf("parseWarnCodes(%s) = %v, want a refusal naming its silent downgrade", diagnosticForeignUnexportedPromotionRefused, err)
	}
}
