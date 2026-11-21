// promotedOneHop_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// TS PERF ITEM 1b (COORD 2026-11-21 06:58Z): a pointer-receiver method promoted through N value embeds was called through
// N views, one per level (`x.of(A.ᏑB).of(B.ᏑC).of(C.ᏑD).M()`), each minted and kept per box. Go reaches the method's
// receiver in one step, and go2cs-gen already declares the outer type's accessor for the promoted embed (`A.ᏑD`, a ref
// to `x.B.C.D`), whose Go offset golib resolves by name. So the call takes ONE view, `x.of(A.ᏑD).M()`, which is the same
// Go pointer as the chain's (managed-exact identity: one root, one Go offset). TypeScript's checkdemo minted six views
// per node this way in LeftHandSideExpressionBase.AsNode.
const promotedOneHopMain = `package main

import "fmt"

type D struct{ n int }

func (d *D) Bump() *D { d.n++; return d }

type C struct{ D }
type B struct{ C }
type A struct{ B }

func main() {
	x := &A{}
	p := x.Bump()
	q := x.Bump()
	fmt.Println(x.n, p == q, p == &x.D, p == &x.B.C.D)
}
`

// The emission: one view through the promoted embed's accessor, and no intermediate level.
func TestAPromotedCallTakesOneViewThroughTheDeepAccessor(t *testing.T) {
	out := convertInlineModule(t, "app", map[string]string{"main.go": promotedOneHopMain})
	main := readGenerated(t, filepath.Join(out, "main.cs"))

	if !strings.Contains(main, ".of(A.ᏑD).Bump()") {
		t.Errorf("the promoted call does not take one view through A.ᏑD:\n%s", main)
	}

	if strings.Contains(main, ".of(B.ᏑC)") || strings.Contains(main, ".of(C.ᏑD).Bump()") {
		t.Errorf("the promoted call still walks the intermediate levels:\n%s", main)
	}
}

// End to end: the one view is the same Go pointer as &x.D and as the spelled chain &x.B.C.D, and the method's writes
// land in x. GO2CS_INLINE_BUILD=1 runs it.
func TestAPromotedCallThroughOneViewReadsAsGo(t *testing.T) {
	dotnet := requireInlineBuild(t)
	files := map[string]string{"main.go": promotedOneHopMain}
	want := goRunInlineModule(t, "app", files)
	out := convertInlineModule(t, "app", files)

	if built, err := buildConvertedInlineModule(t, dotnet, out); err != nil {
		t.Fatalf("the converted module does not build: %v\n%s", err, built)
	}

	got := runConvertedInlineModule(t, dotnet, out)
	t.Logf("READING go prints: %s; go2cs prints: %s", want, got)

	if got != want || want == "" {
		t.Fatalf("go2cs differs from go:\n   go: %s\ngo2cs: %s", want, got)
	}
}
