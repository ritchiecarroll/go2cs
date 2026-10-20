// deadPointerParamAlias_test.go - Gbtc
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

// A comment never decides emitted code. The alias scan read the body's comments as code, so under
// -comments a comment that names the parameter (runtime's casGToPreemptScan: "We never notify
// gp.syncGroup …") added a dereferencing alias the conversion without -comments does not have.
func TestPointerParamAliasIgnoresComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, `package main

import "fmt"

func touch(p *int) bool {
	ok := p != nil
	// p.field is never read here
	return ok /* nor p.other */
}

func main() {
	x := 1
	fmt.Println(touch(&x))
}
`)

	for i, line := range lines {
		if strings.Contains(line, "static bool touch(") && i+1 < len(lines) && strings.Contains(lines[i+1], "ref var p") {
			t.Errorf("touch's p gets a dereferencing alias that only its comments name: %q", strings.TrimSpace(lines[i+1]))
		}
	}
}

// csharpCodeWithoutComments removes comments and nothing else: a `//` or `/*` inside each literal form
// the converter emits is code, so a value use after it is still found.
func TestCSharpCodeWithoutComments(t *testing.T) {
	for _, test := range []struct{ text, want string }{
		{"a(p); // p.x\nb();", "a(p);  \nb();"},
		{"x = /* p */ y;", "x =   y;"},
		{`s = "http://h/" + p; // q`, `s = "http://h/" + p;  `},
		{`s = "a\"//" + p;`, `s = "a\"//" + p;`},
		{`s = @"a\" + p; // q`, `s = @"a\" + p;  `},
		{`s = @"x""//""y" + p;`, `s = @"x""//""y" + p;`},
		{`s = $@"{p}//" + q;`, `s = $@"{p}//" + q;`},
		{"s = \"\"\"\n// raw\n\"\"\" + p;", "s = \"\"\"\n// raw\n\"\"\" + p;"},
		{`c = '/'; // q`, `c = '/';  `},
		{`c = '\''; d = p;`, `c = '\''; d = p;`},
	} {
		if got := csharpCodeWithoutComments(test.text); got != test.want {
			t.Errorf("csharpCodeWithoutComments(%q) = %q, want %q", test.text, got, test.want)
		}
	}
}
