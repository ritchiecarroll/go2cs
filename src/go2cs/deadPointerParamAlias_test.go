// deadPointerParamAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// A pointer parameter's deref VALUE alias is dropped when the converted body never names the value
// (bodyReferencesIdentAsValue). The scan is a whole-word text match and deliberately errs toward
// KEEPING the alias — but a kept alias is not inert: it DEREFERENCES the box at function entry, so
// a spurious match turns a legitimately nil argument into a NilPointerDereference the Go never
// performs. These pin the one spurious match that is excluded — a C# NAMED-ARGUMENT LABEL — and,
// just as importantly, every colon form that must NOT be mistaken for one.
func TestBodyReferencesIdentAsValueIgnoresNamedArgumentLabels(t *testing.T) {
	tests := []struct {
		name string
		body string
		want bool
	}{
		// The defect: a Go composite literal's field key emits as a named argument, and Go's own
		// idiom names the field after the value it is initialized from. internal/concurrent's
		// newIndirectNode reads exactly this way, and its ROOT node is built with a nil parent.
		{
			name: "field key alone is not a value use",
			body: "\n    return Ꮡ(new Δindirect<K, V>(node: new node<K, V>(isEntry: false), parent: Ꮡparent));\n",
			want: false,
		},
		{
			name: "first argument's label, after the open paren",
			body: "\n    return Ꮡ(new entry<V>(parent: Ꮡparent));\n",
			want: false,
		},
		{
			name: "label split across lines by the multi-line literal form",
			body: "\n    var ht = Ꮡ(new HashTrieMap<K, V>(\n        parent: Ꮡparent,\n        seed: 0\n    ));\n",
			want: false,
		},
		// A real value use still matches, whether or not a label for the same name is also present.
		{
			name: "a genuine value use matches",
			body: "\n    return parent.id;\n",
			want: true,
		},
		{
			name: "a label does not mask a genuine value use elsewhere",
			body: "\n    var d = parent.depth;\n    return Ꮡ(new node(parent: Ꮡparent, depth: d));\n",
			want: true,
		},
		// Every other colon the converter emits must stay a value use — the exclusion is scoped to
		// the argument-list position, immediately before a single colon.
		{
			name: "namespace qualifier is not a label",
			body: "\n    return f(global::go.parent::T);\n",
			want: true,
		},
		{
			name: "switch case arm is not a label",
			body: "\n    switch (k) {\n    case parent:\n        return 1;\n    }\n",
			want: true,
		},
		{
			name: "conditional operand is not a label",
			body: "\n    return f(x: ok ? parent : other);\n",
			want: true,
		},
		{
			name: "interpolated format specifier is not a label",
			body: "\n    return f($\"{parent:F2}\");\n",
			want: true,
		},
		// The pre-existing contract: the box form Ꮡparent never counts (Ꮡ is a Unicode letter, so
		// the preceding-rune boundary excludes it), and neither does a longer identifier.
		{
			name: "box form alone is not a value use",
			body: "\n    return Ꮡ(new node(next: Ꮡparent));\n",
			want: false,
		},
		{
			name: "a longer identifier is not a match",
			body: "\n    return parentage;\n",
			want: false,
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if got := bodyReferencesIdentAsValue(test.body, "parent"); got != test.want {
				t.Fatalf("bodyReferencesIdentAsValue(%q, \"parent\") = %v, want %v", test.body, got, test.want)
			}
		})
	}
}
