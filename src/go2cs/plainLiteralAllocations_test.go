// plainLiteralAllocations_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// plainAllocationFixture holds one site per class that plain mode left rendering `"…"u8` in a value slot,
// which builds a fresh @string on every evaluation (seat #51, owner order 2026-10-30). Every literal is
// distinct, so each assertion names exactly one site.
const plainAllocationFixture = "package plit\n\n" +
	"import \"fmt\"\n\n" +
	"type Name string\n\n" +
	"type logger struct{}\n\n" +
	"func (l logger) logf(format string, args ...any) {}\n\n" +
	"func myf(format string, args ...any) string { return format }\n\n" +
	"func takeName(n Name) Name { return n }\n\n" +
	"func Echo[T any](v T) T { return v }\n\n" +
	"func shortName() Name { return \"OK\" }\n\n" +
	"var errNamed = Name(\"package level conversion\")\n\n" +
	"func sites(i int) int {\n" +
	"\t_ = Name(\"xy\")\n" +
	"\tvar l logger\n" +
	"\tl.logf(\"method format position %d\", i)\n" +
	"\t_ = myf(\"function format position %v\", i)\n" +
	"\t_ = fmt.Sprintf(\"twin format position %v\", i)\n" +
	"\tparts := []string{}\n" +
	"\tparts = append(parts, \"appended element\")\n" +
	"\tparts = append(parts, \"first of two\", \"second of two\")\n" +
	"\tnames := []Name{\"named composite element\"}\n" +
	"\tm := map[Name]string{\"named map key\": \"plain map value\"}\n" +
	"\t_ = takeName(\"ab\")\n" +
	"\t_ = Echo(\"cd\")\n" +
	"\t_ = takeName(\"\")\n" +
	"\tboxed := []any{\"boxed composite element\", \"%v\", i}\n" +
	"\tboxed = append(boxed, \"appended any element\", \"ef\")\n" +
	"\tbag := map[string]any{\"plain bag key\": \"boxed map value\"}\n" +
	"\tcell := holder{v: \"boxed field value\"}\n" +
	"\tf := fmt.Println\n" +
	"\tf(\"OK\")\n" +
	"\tbuf := []byte{}\n" +
	"\tbuf = append(buf, \"spread literal\"...)\n" +
	"\tbuf = append(buf, \"\\t\"...)\n" +
	"\tprint(\"print literal \", i)\n" +
	"\tprintln(\"println literal\")\n" +
	"\tfor _, c := range \"range literal\" {\n\t\t_ = c\n\t}\n" +
	"\treturn len(parts) + len(names) + len(m) + len(boxed) + len(bag) + len(cell.s) + len(buf)\n" +
	"}\n\n" +
	"type holder struct {\n\tv any\n\ts string\n}\n"

func convertPlainAllocationFixture(t *testing.T, hoist bool) string {
	t.Helper()
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/plit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "plit.go"), plainAllocationFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		hoistLiterals:       hoist,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "plit", "plit.cs")), "\r\n", "\n")
}

// TestFormatPositionLiteralsGoPlain: a format string bound to an exact `string` parameter takes the bare
// literal, like any other argument there. The format exclusion only keeps a bad slug off a hoisted field, and
// a plain literal has no name. A format position of an sstring twin keeps `"…"u8`, the zero-copy view.
func TestFormatPositionLiteralsGoPlain(t *testing.T) {
	emitted := convertPlainAllocationFixture(t, false)

	for _, want := range []string{
		`l.logf("method format position %d", `,
		`myf("function format position %v", `,
		`fmt.Sprintf("twin format position %v"u8, `,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want %q:\n%s", want, emitted)
		}
	}
}

// TestValueSitesWithNoPlainFormTakeTheCachedCast: a value slot a bare C# string cannot take, and that has no
// field, renders `(@string)"…"`. StringLiteralCache converts the C# literal once, and the cast fixes the C# type
// at @string. That covers an append argument (a bare string would bind append's ISlice overload with
// T = System.String), a named string type's composite element and map key, a degenerate named slot and a
// degenerate generic slot, a literal spread into append (`append(b, "x"...)`, whose @string the spread
// projects to its bytes), a print or println builtin's argument (golib's overload set takes the @string) and
// a range over a literal. The empty literal keeps `""u8`, which allocates nothing.
func TestValueSitesWithNoPlainFormTakeTheCachedCast(t *testing.T) {
	emitted := convertPlainAllocationFixture(t, false)

	for _, want := range []string{
		`parts, (@string)"appended element")`,
		`append(parts, (@string)"first of two", `,
		`[(@string)"named composite element"]`,
		`[(@string)"named map key"] = "plain map value"`,
		`takeName((@string)"ab")`,
		`Echo((@string)"cd")`,
		`return (@string)"OK";`,
		`takeName(""u8)`,
		`append(buf, ((@string)"spread literal").ꓸꓸꓸ)`,
		`append(buf, ((@string)"\t").ꓸꓸꓸ)`,
		`(Name)(@string)"package level conversion"`,
		`(Name)(@string)"xy"`,
		`print((@string)"print literal ", `,
		`println((@string)"println literal")`,
		` in (@string)"range literal")`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want %q:\n%s", want, emitted)
		}
	}

	for _, value := range []string{"appended element", "first of two", "second of two", "named composite element", "named map key", "ab", "cd", "OK"} {
		if unwanted := `"` + value + `"u8`; strings.Contains(emitted, unwanted) {
			t.Errorf("%s still builds an @string per evaluation:\n%s", unwanted, emitted)
		}
	}
}

// TestAnyShapedSitesTakeThePreBoxedField (COORD ruling B, 2026-10-30): an `any` slot no interceptor reaches
// keeps a pre-boxed `static readonly object` field, as an un-interceptable `any` argument already does in plain
// mode. That extends to a composite's `any` element, map value and struct field value, and to an argument
// appended onto `[]any`; a composite gains a field only in its `any` slots. A degenerate slug claims no field
// (the §4.10 floor stands) and renders `(@string)"…"`, the cached @string boxed per evaluation: 32 B where
// `(@string)"…"u8` cost 72.
func TestAnyShapedSitesTakeThePreBoxedField(t *testing.T) {
	emitted := convertPlainAllocationFixture(t, false)

	for _, want := range []string{
		`private static readonly object boxedCompositeElementˢ = (@string)"boxed composite element"u8;`,
		`slice<any>([boxedCompositeElementˢ, (@string)"%v", `,
		`appendedAnyElementˢ`, `(@string)"ef")`,
		`["plain bag key"] = boxedMapValueˢ`,
		`v: boxedFieldValueˢ`,
		`f((@string)"OK");`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want %q:\n%s", want, emitted)
		}
	}

	for _, value := range []string{"%v", "ef", "OK"} {
		if unwanted := `(@string)"` + value + `"u8`; strings.Contains(emitted, unwanted) {
			t.Errorf("%s still converts and boxes per evaluation:\n%s", unwanted, emitted)
		}
	}
}

// TestHoistModeKeepsTheAllocationClassesAsTheyWere:-hoist-literals is the user opt-in seat #52 decides on,
// and #51 leaves it byte-for-byte as it was. Its format positions, append arguments and composite elements
// stay `"…"u8`.
func TestHoistModeKeepsTheAllocationClassesAsTheyWere(t *testing.T) {
	emitted := convertPlainAllocationFixture(t, true)

	for _, want := range []string{
		`l.logf("method format position %d"u8, `,
		`myf("function format position %v"u8, `,
		`parts, "appended element"u8)`,
		`["named composite element"u8]`,
		`["named map key"u8] = "plain map value"u8`,
		`slice<any>([(@string)"boxed composite element"u8, `,
		`f((@string)"OK"u8);`,
		`append(buf, ((@string)"spread literal"u8).ꓸꓸꓸ)`,
		`print((@string)"print literal "u8, `,
		`println((@string)"println literal"u8)`,
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("-hoist-literals: want %q:\n%s", want, emitted)
		}
	}
}
