// forClauseReAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a pointer PARAMETER repointed inside a for CLAUSE (visitAssignStmt, visitForStmt). The parameter is a box
// plus a ref-local alias of its target, so a repoint is two steps: `Ꮡp = E` and the re-alias `p = ref
// Ꮡp.DerefOrNull()`. A clause holds no `;`, so the pair is written as one C# expression list, in the INIT and in the
// POST alike. Two failures stood here: the init wrote the two steps with nothing between them (CS1003, the
// TypeScript experiment's C28), and the post's re-alias was injected at the top of the loop BODY, where the condition
// had already read the previous node through the stale alias (a wrong result that built). A post that is lowered out
// of the header keeps the pair as two statements, at the end of the body. The control is a repoint in the body.

package main

import (
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

func convertForClauseReAliasFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type node struct {
	val  int
	next *node
}

func walkPost(p *node) (sum int) {
	for ; p != nil; p = p.next {
		sum += p.val
	}
	return
}

func walkCond(p *node, limit int) (steps int) {
	for ; p != nil && p.val < limit; p = p.next {
		steps++
	}
	return
}

func walkInit(p *node) (sum int) {
	for p = p.next; p != nil; p = p.next {
		sum += p.val
	}
	return
}

func initFrom(a, b *node) (sum int) {
	for a = b; a != nil; a = a.next {
		sum += a.val
	}
	return
}

func swapWalk(a, b *node) (sum int) {
	for a, b = b, a; a != nil && b != nil; a, b = a.next, b.next {
		sum += a.val + b.val
	}
	return
}

func pair(p *node) (*node, int) { return p.next, p.val }

func pick(n *node, k int) *node { return n }

func spillWalk(p *node, limit int) (steps int) {
	for ; p != nil && p.val < limit; p = pick(pair(p)) {
		steps++
	}
	return
}

func bodyWalk(p *node, n int) (sum int) {
	for i := 0; i < n; i++ {
		sum += p.val
		p = p.next
	}
	return
}

func main() {
	k := &node{val: 1, next: &node{val: 2}}
	fmt.Println(walkPost(k), walkCond(k, 2), walkInit(k), initFrom(nil, k), swapWalk(k, k), spillWalk(k, 2), bodyWalk(k, 2))
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs"))
}

// TestAPointerParameterRepointedInAForClauseReAliasesInThatClause requires the box repoint and the re-alias to share
// the clause that repoints, so the alias is current before the condition runs, and requires that no loop opens its
// body with a re-alias.
func TestAPointerParameterRepointedInAForClauseReAliasesInThatClause(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertForClauseReAliasFixture(t)

	const (
		postP = "Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()"
		postA = "Ꮡa = a.next, a = ref Ꮡa.DerefOrNull()"
	)

	bodyOpensWithReAlias := regexp.MustCompile(`\{\r?\n\s*\w+ = ref Ꮡ\w+\.DerefOrNull\(\);`)

	for _, row := range []struct{ fn, want string }{
		{"walkPost(", "for (; Ꮡp != nil; " + postP + ") {"},
		// The condition reads through the alias: it must follow the re-alias, not precede it.
		{"walkCond(", "for (; Ꮡp != nil && p.val < limit; " + postP + ") {"},
		{"walkInit(", "for (" + postP + "; Ꮡp != nil; " + postP + ") {"},
		{"initFrom(", "for (Ꮡa = b, a = ref Ꮡa.DerefOrNull(); Ꮡa != nil; " + postA + ") {"},
		{"swapWalk(", "for ((Ꮡa, Ꮡb) = (Ꮡb, Ꮡa), a = ref Ꮡa.DerefOrNull(), b = ref Ꮡb.DerefOrNull(); Ꮡa != nil && Ꮡb != nil; " +
			"(Ꮡa, Ꮡb) = (a.next, b.next), a = ref Ꮡa.DerefOrNull(), b = ref Ꮡb.DerefOrNull()) {"},
		// The post left the header: the pair closes the body as two statements.
		{"spillWalk(", "Ꮡp = pick(ᴛ1, ᴛ2); p = ref Ꮡp.DerefOrNull();"},
	} {
		body := funcBody(t, cs, row.fn)

		if !strings.Contains(body, row.want) {
			t.Errorf("%s does not read %q:\n%s", row.fn, row.want, body)
		}

		if at := bodyOpensWithReAlias.FindString(body); at != "" {
			t.Errorf("%s opens a block with the re-alias %q, after the condition has run:\n%s", row.fn, at, body)
		}
	}

	// The control: a repoint in the loop body is two statements, as it is anywhere outside a clause.
	if body := funcBody(t, cs, "bodyWalk("); !strings.Contains(body, "Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();") {
		t.Errorf("bodyWalk( no longer repoints with two statements:\n%s", body)
	}
}
