package main

import "fmt"

// fixed is a struct whose name is a C# keyword.
type fixed struct {
	n int
}

// sizer is satisfied by fixed through a value receiver.
type sizer interface {
	size(of string) int
}

// lock is an interface whose name is a C# keyword.
type lock interface {
	held() bool
}

// short is a numeric defined type named after a C# keyword; dword is a numeric
// defined type that is not. Converting between them exercises the go2cs-gen
// implicit-conversion operators for a KEYWORD-named type — the [GoType] struct is
// @short, so the generated `partial struct`/operator/`new` must escape the keyword
// (regression guard for the ImplicitConvGenerator: CS0715/CS0057/CS0030).
type short int16
type dword uint32

func toShort(d dword) short { return short(d) }

func (f fixed) size(of string) int {
	return f.n + len(of)
}

func (f *fixed) grow(by int) {
	f.n += by
}

func (f fixed) held() bool {
	return f.n > 3
}

// ---------------------------------------------------------------------------------------------
// A THIRD keyword mechanism, and the only one where '@' lands ILLEGALLY.
//
// The two above are package-level: a keyword TYPE name escapes to a LEADING '@' (`@fixed`,
// `@lock`, `@short`), which is legal C#. A function-local anonymous type is different — the
// converter LIFTS it to a composed `<enclosing>_<variable>` name, and the variable arrives
// already escaped, so the '@' lands MID-IDENTIFIER where C# does not accept it at all:
//
//	vgetrandomInit_@params   ->  CS1513 / CS1514 / CS1519, uncompilable
//
// Go 1.24's runtime/vgetrandom_linux.go is the shape that surfaced it; NOTHING in the Go
// 1.23.12 corpus contains it, so before this guard the class was reachable only through
// -recurse on end-user Go, where `params`, `ref`, `out` and `fixed` are ordinary identifiers.
// Fixed at the single choke point every lift caller funnels through (getUniqueLiftedTypeName),
// which strips EVERY marker from the composed name and re-sanitizes the whole.
//
// Distinct from `InterfaceKeywordParamNames`, which guards go2cs-gen re-reading interface
// PARAMETER names with the escape stripped (CS0501) — a different subsystem and a different
// failure. Three keyword mechanisms, three guards; only this one emits invalid syntax.

// keywordLocalStruct lifts to `keywordLocalStruct_params`, NOT `keywordLocalStruct_@params`.
func keywordLocalStruct() uintptr {
	var params struct {
		size  uintptr
		flags uint32
	}
	params.size = 8
	params.flags = 3
	return params.size + uintptr(params.flags)
}

// keywordLocalIface is the same defect through the interface arm, which no stdlib release
// contains at all — it fails identically and a census over Go's own sources cannot find it.
func keywordLocalIface() int {
	var params interface{ held() bool }
	if params == nil {
		return -1
	}
	if params.held() {
		return 1
	}
	return 0
}

// A second keyword, so the fix is visibly not special-cased to `params`.
func keywordLocalRef() int {
	var ref struct{ n int }
	ref.n = 7
	return ref.n
}

// CONTROL: identical shape, NON-keyword name. This composed correctly before the fix and must
// still — it is what proves the change strips a keyword ESCAPE rather than every '@'.
func plainLocalStruct() uintptr {
	var sizes struct{ size uintptr }
	sizes.size = 4
	return sizes.size
}

// file is named after C#'s contextual `file` keyword (emitted @file). Its exported value method makes go2cs-gen
// write a method-set part for it, which must escape the name too (CS9056).
type file struct{ size int }

func (f file) Size() int { return f.size * 2 }

// extension is a C# 14 keyword, banned as a type name (CS9306), so the type and a type parameter so named are
// renamed. closed, union and safe take the same rename for C# 15, where they become keywords; at the C# 14 the
// harness builds, only the emitted spelling shows it.
type extension struct{ n int }

func pass[extension any](v extension) extension { return v }

type closed int

type union struct{ a, b int }

func keep[safe any](v safe) safe { return v }

var std sizer = fixed{n: 3}

func main() {
	fmt.Println(std.size("hello"))

	f := fixed{n: 1}
	fmt.Println(f.size("ab"))

	f.grow(4)
	fmt.Println(f.size(""))

	var p sizer = &f
	fmt.Println(p.size("ptr"))

	var l lock = f
	fmt.Println(l.held())

	var lp lock = &f
	fmt.Println(lp.held())

	var d dword = 40000
	s := toShort(d)
	fmt.Println(int(s), int(dword(s)))

	fmt.Println(keywordLocalStruct())
	fmt.Println(keywordLocalIface())
	fmt.Println(keywordLocalRef())
	fmt.Println(plainLocalStruct())

	fmt.Println(file{size: 21}.Size())
	fmt.Println(extension{n: 5}.n, pass(42))
	fmt.Println(closed(6), union{a: 3, b: 4}.a+union{a: 3, b: 4}.b, keep("s"))
}
