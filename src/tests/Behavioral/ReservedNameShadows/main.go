// Regression test: user identifiers that shadow names go2cs itself must spell in the
// emitted C#.
//
//   - `any`, `rune`, `nint` as USER TYPES: the emitter spells these names on its own
//     (`interface{}` renders as `any`; an untyped rune-constant default emits
//     `rune c = 'x';`; Go `int` maps to C# `nint`), so a same-named user type is
//     package-scoped Δ-renamed via emitterSpelledTypeNames — it must never be globally
//     `reserved` (that corrupts legitimate spellings in every other package) nor
//     '@'-escaped ('@' does not rename).
//   - `builtin`, `sstring`, `GoFrame` as USER TYPES: golib names in the `reserved` set —
//     the qualified `builtin.len(...)` emission (a method named `len` shadows the built-in),
//     the string([]byte) elision's `sstring` views, and the `GoFrame` local every deferring
//     function declares must all keep resolving.
//   - `required`, `scoped` as USER TYPES: C# contextual keywords banned as type names
//     (CS9029/CS9062) — escaped `@required`/`@scoped` like `file`; `record` begins a record
//     declaration at member start, so it is Δ-renamed like `union` (csReservedTypeNames).
//   - A local named `nil` is a shadowing VARIABLE (universe-object check in convIdent) —
//     it must not be emitted as the `default!` nil-literal rendering.
//   - A local named `__arglist` must escape the (undocumented) C# keyword — the keyword
//     set carried a typo (`__argslist`) that covered nothing.
package main

import "fmt"

type any struct{ x int }

type rune struct{ r int }

type nint struct{ d int }

type builtin struct{ z int }

type sstring struct{ n int }

type required struct{ c int }

type scoped struct{ b int }

type record struct{ a int }

// A user type named after the emitter-spelled `GoFrame` — the golib ref struct every deferring
// function declares a local of. Declared inside the package class it would SHADOW that type, so it
// is in the `reserved` set and Δ-renamed, exactly like `builtin` and `sstring` above. The deferring
// function below is what proves the emitted `GoFrame` still resolves in this package.
type GoFrame struct{ k int }

func deferInShadowedPackage() int {
	f := GoFrame{k: 1}
	defer func() { f.k++ }()
	return f.k
}

type box struct{ items []int }

// A method named `len` forces free `len(...)` calls in this package to be emitted
// qualified as `builtin.len(...)` — which the user type `builtin` must not shadow.
func (b *box) len() int { return len(b.items) + 1 }

func freeLen() int {
	s := []int{1, 2, 3}
	return len(s)
}

// A local named `nil` is a shadowing variable, not the nil literal.
func nilShadow() int {
	nil := 5
	return nil + 1
}

// Locals named after C# keywords (contextual or undocumented) and shadowed type names.
func keywordLocals() int {
	__arglist := 5
	record := 1
	scoped := 2
	required := 3
	nuint := 4
	return __arglist + record + scoped + required + nuint
}

// Predeclared-identifier locals that need no rename (controls).
func predeclLocals() (int, string) {
	iota := 6
	error := "boom"
	comparable := 8
	uintptr := 11
	complex64 := 12
	return iota + comparable + uintptr + complex64, error
}

// The string([]byte) elision (golib sstring view) alongside a local named `sstring`.
func viewLen(b []byte) int {
	sstring := 3
	s := string(b)
	if s == "ok" {
		return sstring
	}
	return len(s) + sstring
}

func main() {
	a := any{x: 1}
	vals := []interface{}{1, "two", 3.5}
	c := 'x'
	c++
	r := rune{r: 2}
	n := nint{d: 4}
	bi := builtin{z: 9}
	sv := sstring{n: 7}
	rq := required{c: 3}
	sc := scoped{b: 2}
	rc := record{a: 1}
	bx := box{items: []int{4, 5}}
	fmt.Println(a.x, len(vals), vals[1], int(c), r.r, n.d, bi.z, sv.n, rq.c, sc.b, rc.a)
	pd, es := predeclLocals()
	fmt.Println(freeLen(), bx.len(), nilShadow(), keywordLocals(), pd, es, viewLen([]byte("xyz")), deferInShadowedPackage())
}
