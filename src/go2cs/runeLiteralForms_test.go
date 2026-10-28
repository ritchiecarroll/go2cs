// runeLiteralForms_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the forms of a rune literal (markRuneLiteralForms): a bare C# char where the slot fixes a type C#
// reaches from char implicitly, one cast spelled as the target for a byte, int8 or int16 slot, and `(rune)'x'`
// kept for every slot where C# would infer, box or overload on a char. Each keep arm has its own function, so a
// failure names the arm.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func TestRuneLiteralForms(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()

	writeModuleFile(t, filepath.Join(root, "app", "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(root, "app", "main.go"), `package main

import "fmt"

type letter rune

const untyped = 'a' - 'A'

func generic[T rune | byte](x T) T { return x }

func takesRune(r rune) rune     { return r }
func takesAny(a any) any        { return a }
func takesMany(rs ...rune) rune { return rs[0] }

func compare(r rune, b byte) bool   { return r == 'a' || b < 'z' }
func arith(b byte, u uint64) uint64 { return uint64(b-'0') + u*'b' }
func typed() (rune, uint16)         { var r rune = 'a'; var u uint16; u = 'u'; return r, u }
func caseRune(r rune) int           { switch r { case 'a', 'b': return 1 }; return 0 }
func elements() []rune              { return []rune{'x', 'y'} }
func indexKeys() [128]bool          { return [128]bool{'a': true} }
func argument() rune                { return takesRune('q') }
func conversion() (byte, uint16)    { return byte('a'), uint16('c') }

func narrowAssign() byte     { var b byte = 'q'; return b }
func narrowInt8() int8       { var i int8 = 'A'; return i }
func narrowCase(b byte) int  { switch b { case 'p': return 1 }; return 0 }
func narrowAppend() []byte   { return append([]byte(nil), '!') }
func narrowReturn() byte     { return 'z' }

func keepInfer() rune        { r := 'k'; return r }
func keepBoxed() any         { return takesAny('k') }
func keepAnyVar() any        { var a any = 'k'; return a }
func keepBuiltin(r rune) rune { return max('k', r) }
func keepGeneric() rune      { return generic[rune]('k') }
func keepVariadic() rune     { return takesMany('k') }
func keepNamed() letter      { var l letter = 'k'; return l }
func keepAstral() rune       { var r rune = '\U0001F600'; return r }
func keepConstExpr() rune    { var r rune = 'a' + 1; return r }
func keepAnyCompare(a any) bool { return a == 'k' }

func main() {
	fmt.Println(compare('a', 'b'), arith('7', 1), caseRune('a'), elements(), argument(), untyped)
	fmt.Println(typed())
	fmt.Println(indexKeys()[97])
	fmt.Println(conversion())
	fmt.Println(narrowAssign(), narrowInt8(), narrowCase('p'), narrowAppend(), narrowReturn())
	fmt.Println(keepInfer(), keepBoxed(), keepAnyVar(), keepBuiltin('a'), keepGeneric(), keepVariadic(), keepNamed(),
		keepAstral(), keepConstExpr(), keepAnyCompare('k'))
}
`)

	cs := readGenerated(t, filepath.Join(convertFieldAccessModule(t, root), "main.cs"))

	for _, want := range []struct{ fn, form string }{
		{"compare(", "r == 'a' || b < 'z'"},
		{"arith(", "b - '0'"},
		{"arith(", "u * 'b'"},
		{"typed(", "rune r = 'a';"},
		{"typed(", "u = 'u';"},
		{"caseRune(", "case 'a' or 'b':"},
		{"elements(", "{'x', 'y'}"},
		{"indexKeys(", "['a'] = true"},
		{"argument(", "takesRune('q')"},
		{"conversion(", "(byte)'a'"},
		{"conversion(", "(uint16)'c'"},
		{"narrowAssign(", "byte b = (byte)'q';"},
		{"narrowInt8(", "int8 i = (int8)'A';"},
		{"narrowCase(", "case (byte)'p':"},
		{"narrowAppend(", "(byte)'!'"},
		{"narrowReturn(", "return (byte)'z';"},
		{"keepInfer(", "(rune)'k'"},
		{"keepBoxed(", "takesAny((rune)'k')"},
		{"keepAnyVar(", "(rune)'k'"},
		{"keepBuiltin(", "max((rune)'k', r)"},
		{"keepGeneric(", "(rune)'k'"},
		{"keepVariadic(", "(rune)'k'"},
		{"keepNamed(", "(rune)'k'"},
		{"keepAstral(", "(rune)0x1F600"},
		{"keepAnyCompare(", "(rune)'k'"},
	} {
		if body := funcBody(t, cs, want.fn); !strings.Contains(body, want.form) {
			t.Errorf("%s does not render its rune literal as %q:\n%s", want.fn, want.form, body)
		}
	}

	// A conversion and an append carry ONE cast, never the old double.
	for _, fn := range []string{"conversion(", "narrowAppend("} {
		if body := funcBody(t, cs, fn); strings.Contains(body, "(rune)") {
			t.Errorf("%s still carries a (rune) cast beside its conversion:\n%s", fn, body)
		}
	}
}
